using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Diagnostics;

namespace Selenium_Script
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool run = true;

            while (run)
            {
                LoginToSite();
            }            
        }
        public static void ControlPlus(IWebElement element, string key)
        {
            element.SendKeys(Keys.LeftControl + key); //passes in the element to be targeted, then pass in the key to use with control
        }  
        public static string GetSiteUrl()
        {
            Console.WriteLine("Enter Site URL to login to:");
            string siteUrl = Console.ReadLine().Trim();
            bool hasLogin = siteUrl.Contains("login");
            bool hasHttps = siteUrl.Contains("http");

            IDictionary<string, string> shorthandSites = new Dictionary<string, string>();
            shorthandSites.Add("kirk", "https://kirk.interactgo.com/login?local=True");
            shorthandSites.Add("picard", "https://picard.interactgo.com/login?local=True");

            bool isShorthand = shorthandSites.ContainsKey(siteUrl); //Checks to see if the user input is a registered shorthand term.


            if (!isShorthand)
            {
                // the key isn't in the dictionary. i.e. The user input is not a registered shorthand.
                if (hasLogin == false)
                {
                    //The word 'Login' was not detected, add the end of the URL.
                    siteUrl += "/login?local=True";
                }
                if (hasHttps == false)
                {
                    //https was not found on the user input, add it.
                    siteUrl = "https://" + siteUrl;
                }

            }
            else
            {
                //key was found in dictionary. i.e. The user input is a registered shorthand.
                shorthandSites.TryGetValue(siteUrl, out siteUrl);
            }

            return siteUrl;
        }
        public static Cookie GetLoginCookie (string siteUrl, bool testSite, string controlReason)
        {
            Console.WriteLine("Launching browser... ");
            var timer = new Stopwatch();
            timer.Start();

            //want to add a dictionary of test sites that you'd be able to edit from a yaml file 
            //Add a way that you can add /support, application settings or something similar to end
            //Open multiple tabs with support and application settings
            //string testSite = "N";

            ChromeOptions headlessOptions = new ChromeOptions();
            headlessOptions.AddArguments("--start-maximized", "--blink-settings=imagesEnabled=false", "--headless");
            //options.AddArguments(@"user-data-dir=C:\Users\James Clark\AppData\Local\Google\Chrome\User Data\Selenium");
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            try
            {
                IWebDriver testDriver = new ChromeDriver(chromeDriverService, headlessOptions);
                testDriver.Quit();
            }
            catch (Exception InvalidOperationException)
            {
                Console.WriteLine("ERROR CANNOT CONTINUE \n");
                Console.WriteLine("This application is out of date compared to your current Chrome Version. ");
                Console.WriteLine("Please update to the latest NuGet Packages for Selenium.Webdriver and Selenium.WebDriver.Chrome.");
                Console.WriteLine("This can be done by launching this application in Visual Studio>Tools>NuGet Package Manager>Updates Tab.");
                Console.WriteLine("Below are links to the NuGet Packages themselves: \n");
                Console.WriteLine("https://www.nuget.org/packages/Selenium.WebDriver.ChromeDriver/");
                Console.WriteLine("https://www.nuget.org/packages/Selenium.WebDriver/ \n");
                return null;
            }
            
            IWebDriver headlessDriver = new ChromeDriver(chromeDriverService, headlessOptions);

            WebDriverWait wait = new WebDriverWait(headlessDriver, TimeSpan.FromSeconds(10));

            headlessDriver.Navigate().GoToUrl(siteUrl);
            //headlessDriver.Navigate().GoToUrl("https://thehub.integralads.com/login?local=True");
            //headlessDriver.Navigate().GoToUrl("https://kirk.interactgo.com/login?local=True");
            TimeSpan elapsedTime = timer.Elapsed;
            Console.WriteLine("Loading " + siteUrl + "... " + elapsedTime.ToString(@"m\:ss\.fff"));

            //Elements used for site main login page

            IWebElement siteUser = headlessDriver.FindElement(By.Id("Username"));
            IWebElement expandFields = null;
            IWebElement sitePass = headlessDriver.FindElement(By.Id("Password"));

            IWebElement siteLoginBtn = headlessDriver.FindElement(By.Id("loginbtn"));
            IWebElement siteRM;

            try
            {
                siteRM = headlessDriver.FindElement(By.Id("RememberMe"));
            }
            catch (Exception NoSuchElementException)
            {
                Console.WriteLine("No 'Remember Me' checkbox found, launching visible browser.");
                headlessDriver.Quit();
                VisibleLoginToSite(siteUrl, testSite, controlReason);
                Cookie nullCookie = null;
                return nullCookie;
            }
            try
            {
                siteUser.SendKeys("admin");
            }
            catch (Exception ElementNotInteractableException)
            {
                Console.WriteLine("Unable to interact with Username field, expanding fields...");
                expandFields = headlessDriver.FindElement(By.CssSelector(".collapsed"));
                expandFields.Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(siteUser));
                siteUser.SendKeys("admin");
            }
            //Interactions with site main login page elements           
            
            sitePass.SendKeys("bantam");
            siteRM.Click();
            siteLoginBtn.Click();
            elapsedTime = timer.Elapsed;
            Console.WriteLine("Grabbing challenge code... " + elapsedTime.ToString(@"m\:ss\.fff"));

            //Challenge page Elements
            IWebElement challengeBox = headlessDriver.FindElement(By.Id("Challenge"));
            IWebElement challengeFinal = headlessDriver.FindElement(By.Id("Code"));
            IWebElement challengeSubmit = headlessDriver.FindElement(By.Id("submitchallenge"));

            if(expandFields != null)
            {
                expandFields = headlessDriver.FindElement(By.CssSelector(".collapsed"));
                expandFields.Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(challengeBox));
                challengeBox.Click();
            }
            else
            {
                challengeBox.Click();
            }
            
            //Select all then copy the challenge code
            ControlPlus(challengeBox, "a");

            ControlPlus(challengeBox, "c");

            IJavaScriptExecutor js = (IJavaScriptExecutor)headlessDriver;

            js.ExecuteScript("new_tab = window.open('https://control.interactgo.com/Account/Login')");
            headlessDriver.SwitchTo().Window(headlessDriver.WindowHandles.Last());

            

            IWebElement controlUser = headlessDriver.FindElement(By.Id("Email"));
            IWebElement controlPassword = headlessDriver.FindElement(By.Id("Password"));
            IWebElement controlLoginBtn = headlessDriver.FindElement(By.CssSelector(".btn-primary"));

            //Control Site Main Interactions
            controlUser.SendKeys("james.clark@interact-intranet.com");
            controlPassword.SendKeys("R3trieval.br!nk.0verst@te");
            controlLoginBtn.Click();
            elapsedTime = timer.Elapsed;
            Console.WriteLine("Logging into Control... " + elapsedTime.ToString(@"m\:ss\.fff"));

            //Control Challenge Page Elements
            IWebElement controlCodeBox = headlessDriver.FindElement(By.CssSelector(".challenge-code"));
            IWebElement controlCodeReason = headlessDriver.FindElement(By.CssSelector(".challenge-reason"));
            IWebElement controlCodeSubmit = headlessDriver.FindElement(By.CssSelector(".challenge-submit"));

            //Control Challenge Page Interactions

            //Paste challenge code
            ControlPlus(controlCodeBox, "v");

            if (testSite == false)
            {
                wait.Until(ExpectedConditions.ElementToBeClickable(controlCodeReason));
                controlCodeReason.SendKeys("ticket: " + controlReason);
                controlCodeSubmit.Click();
                Thread.Sleep(1500);
            }
            else
            {
                Thread.Sleep(1500); //wait 1.5 seconds for site to load code, need to fix this to be more reliable
            }

            //Select all in challenge code box
            ControlPlus(controlCodeBox, "a");

            //Copy the final code and close the Control window
            ControlPlus(controlCodeBox, "c");

            headlessDriver.SwitchTo().Window(headlessDriver.WindowHandles.First());
            js.ExecuteScript("new_tab.close()");

            elapsedTime = timer.Elapsed;
            Console.WriteLine("Copied Control code..." + elapsedTime.ToString(@"m\:ss\.fff"));

            //Move back to Main Site Challenge Page and paste in final code
            ControlPlus(challengeFinal, "v");

            elapsedTime = timer.Elapsed;
            Console.WriteLine("Logging into " + siteUrl + "... " + elapsedTime.ToString(@"m\:ss\.fff"));
            challengeSubmit.Click();

            string loggedInURL = headlessDriver.Url;


            timer.Stop();
            elapsedTime = timer.Elapsed;
            Console.WriteLine("Finished logging into " + loggedInURL);
            Console.WriteLine("Total login time: " + elapsedTime.ToString(@"m\:ss\.fff") + "\n");

            Cookie cookie = headlessDriver.Manage().Cookies.GetCookieNamed(".AspNet.ApplicationCookie");

            return cookie;
        }
        public static bool IsTestSite(string siteUrl)
        {
            string[] testSites = { "kirk", "picard" };

            if (testSites.Any(siteUrl.Contains))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string GetControlReason()
        {
            string controlReason = "";
            Console.WriteLine("Ticket Number/Reason: ");
            controlReason = Console.ReadLine();
            return controlReason;
        }
        public static string GetBaseUrl(string siteUrl)
        {
            string baseUrl = siteUrl.Remove(siteUrl.LastIndexOf("/") + 1);
            return baseUrl;
        }
        public static void LoginToSite()
        {
            string siteUrl = GetSiteUrl();
            string baseUrl = GetBaseUrl(siteUrl);
            bool testSite = IsTestSite(siteUrl);
            string controlReason = "";

            if (testSite == false)
            {
                controlReason = GetControlReason();
            }

            Cookie cookie = GetLoginCookie(siteUrl, testSite, controlReason);

            if (cookie == null)
            {
                return;
            }

            //Create visible web driver
            ChromeOptions siteOptions = new ChromeOptions();
            siteOptions.AddArgument("--start-maximized");
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            IWebDriver siteDriver = new ChromeDriver(chromeDriverService, siteOptions);

            siteDriver.Navigate().GoToUrl(siteUrl);

            siteDriver.Manage().Cookies.AddCookie(cookie);

            siteDriver.Navigate().GoToUrl(baseUrl);

            //Open tabs
            IJavaScriptExecutor jsSite = (IJavaScriptExecutor)siteDriver;
            jsSite.ExecuteScript("window.open('" + baseUrl + "support')");
            jsSite.ExecuteScript("window.open('" + baseUrl + "Interact/Pages/Admin/Default.aspx?section=-1')");
            jsSite.ExecuteScript("window.open('" + baseUrl + "Interact/Pages/Admin/People/Staff/Default.aspx?section=106')");
            jsSite.ExecuteScript("window.open('" + baseUrl + "InteractV7/UMI/List')");
            siteDriver.SwitchTo().Window(siteDriver.WindowHandles.First());


        }
        public static void VisibleLoginToSite(string siteUrl, bool testSite, string controlReason)
        {
            Console.WriteLine("Launching browser... ");
            var timer = new Stopwatch();
            timer.Start();

            //want to add a dictionary of test sites that you'd be able to edit from a yaml file 
            //Add a way that you can add /support, application settings or something similar to end
            //Open multiple tabs with support and application settings
            //string testSite = "N";

            ChromeOptions Options = new ChromeOptions();
            Options.AddArgument("--start-maximized");
            //options.AddArguments(@"user-data-dir=C:\Users\James Clark\AppData\Local\Google\Chrome\User Data\Selenium");
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;


            IWebDriver Driver = new ChromeDriver(chromeDriverService, Options);

            Driver.Navigate().GoToUrl(siteUrl);
            //headlessDriver.Navigate().GoToUrl("https://thehub.integralads.com/login?local=True");
            //headlessDriver.Navigate().GoToUrl("https://kirk.interactgo.com/login?local=True");
            TimeSpan elapsedTime = timer.Elapsed;
            Console.WriteLine("Loading " + siteUrl + "... " + elapsedTime.ToString(@"m\:ss\.fff"));

            //Elements used for site main login page
            IWebElement siteUser = Driver.FindElement(By.Id("Username"));
            IWebElement sitePass = Driver.FindElement(By.Id("Password"));

            IWebElement siteLoginBtn = Driver.FindElement(By.Id("loginbtn"));

            //Interactions with site main login page elements           
            siteUser.SendKeys("admin");
            sitePass.SendKeys("bantam");
            siteLoginBtn.Click();
            elapsedTime = timer.Elapsed;
            Console.WriteLine("Grabbing challenge code... " + elapsedTime.ToString(@"m\:ss\.fff"));

            //Challenge page Elements
            IWebElement challengeBox = Driver.FindElement(By.Id("Challenge"));
            IWebElement challengeFinal = Driver.FindElement(By.Id("Code"));
            IWebElement challengeSubmit = Driver.FindElement(By.Id("submitchallenge"));
            challengeBox.Click();
            //Select all then copy the challenge code
            ControlPlus(challengeBox, "a");

            ControlPlus(challengeBox, "c");

            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;

            js.ExecuteScript("new_tab = window.open('https://control.interactgo.com/Account/Login')");
            Driver.SwitchTo().Window(Driver.WindowHandles.Last());

            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

            IWebElement controlUser = Driver.FindElement(By.Id("Email"));
            IWebElement controlPassword = Driver.FindElement(By.Id("Password"));
            IWebElement controlLoginBtn = Driver.FindElement(By.CssSelector(".btn-primary"));

            //Control Site Main Interactions
            controlUser.SendKeys("james.clark@interact-intranet.com");
            controlPassword.SendKeys("R3trieval.br!nk.0verst@te");
            controlLoginBtn.Click();
            elapsedTime = timer.Elapsed;
            Console.WriteLine("Logging into Control... " + elapsedTime.ToString(@"m\:ss\.fff"));

            //Control Challenge Page Elements
            IWebElement controlCodeBox = Driver.FindElement(By.CssSelector(".challenge-code"));
            IWebElement controlCodeReason = Driver.FindElement(By.CssSelector(".challenge-reason"));
            IWebElement controlCodeSubmit = Driver.FindElement(By.CssSelector(".challenge-submit"));

            //Control Challenge Page Interactions

            //Paste challenge code
            ControlPlus(controlCodeBox, "v");

            if (testSite == false)
            {
                wait.Until(ExpectedConditions.ElementToBeClickable(controlCodeReason));
                controlCodeReason.SendKeys("ticket: " + controlReason);
                controlCodeSubmit.Click();
                Thread.Sleep(1500);
            }
            else
            {
                Thread.Sleep(1500); //wait 1.5 seconds for site to load code, need to fix this to be more reliable
            }

            //Select all in challenge code box
            ControlPlus(controlCodeBox, "a");

            //Copy the final code and close the Control window
            ControlPlus(controlCodeBox, "c");

            Driver.SwitchTo().Window(Driver.WindowHandles.First());
            js.ExecuteScript("new_tab.close()");

            elapsedTime = timer.Elapsed;
            Console.WriteLine("Copied Control code..." + elapsedTime.ToString(@"m\:ss\.fff"));

            //Move back to Main Site Challenge Page and paste in final code
            ControlPlus(challengeFinal, "v");

            elapsedTime = timer.Elapsed;
            Console.WriteLine("Logging into " + siteUrl + "... " + elapsedTime.ToString(@"m\:ss\.fff"));
            challengeSubmit.Click();

            string loggedInURL = Driver.Url;


            timer.Stop();
            elapsedTime = timer.Elapsed;
            Console.WriteLine("Finished logging into " + loggedInURL);
            Console.WriteLine("Total login time: " + elapsedTime.ToString(@"m\:ss\.fff") + "\n");

        }
    }
}
