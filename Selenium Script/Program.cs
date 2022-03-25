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
            Console.WriteLine("Enter Site URL to login to:");
            string siteUrl = Console.ReadLine().Trim();
            bool hasLogin = siteUrl.Contains("login");
            bool hasHttps = siteUrl.Contains("http");
            bool testSite = false;

            if (hasLogin == false)
            {
                siteUrl += "/login?local=True";
            }
            if (hasHttps == false)
            {
                siteUrl = "https://" + siteUrl;
            }

            Console.WriteLine("Logging into " + siteUrl);

            string[] testSites = { "kirk", "picard" };
            if (testSites.Any(siteUrl.Contains))
            {
                testSite = true;
            }

            //want to add a dictionary of test sites that you'd be able to edit from a yaml file 
            //Add a way that you can add /support, application settings or something similar to end
            //Open multiple tabs with support and application settings
            //string testSite = "N";

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            IWebDriver sitedriver = new ChromeDriver(options);

            sitedriver.Navigate().GoToUrl(siteUrl);
            //sitedriver.Navigate().GoToUrl("https://thehub.integralads.com/login?local=True");
            //sitedriver.Navigate().GoToUrl("https://kirk.interactgo.com/login?local=True");

            //Elements used for site main login page
            IWebElement siteUser = sitedriver.FindElement(By.Id("Username"));
            IWebElement sitePass = sitedriver.FindElement(By.Id("Password"));
            IWebElement siteRM = sitedriver.FindElement(By.Id("RememberMe"));
            IWebElement siteLoginBtn = sitedriver.FindElement(By.Id("loginbtn"));

            //Interactions with site main login page elements           
            siteUser.SendKeys("admin");
            sitePass.SendKeys("bantam");
            siteRM.Click();
            siteLoginBtn.Click();

            //Challenge page Elements
            IWebElement challengeBox = sitedriver.FindElement(By.Id("Challenge"));
            IWebElement challengeFinal = sitedriver.FindElement(By.Id("Code"));
            IWebElement challengeSubmit = sitedriver.FindElement(By.Id("submitchallenge"));
            challengeBox.Click();
            //Select all then copy the challenge code
            ControlPlus(challengeBox, "a");

            ControlPlus(challengeBox, "c");

            IJavaScriptExecutor js = (IJavaScriptExecutor)sitedriver;

            js.ExecuteScript("new_tab = window.open('https://control.interactgo.com/Account/Login')");
            sitedriver.SwitchTo().Window(sitedriver.WindowHandles.Last());

            WebDriverWait wait = new WebDriverWait(sitedriver, TimeSpan.FromSeconds(10));

            IWebElement controlUser = sitedriver.FindElement(By.Id("Email"));
            IWebElement controlPassword = sitedriver.FindElement(By.Id("Password"));
            IWebElement controlLoginBtn = sitedriver.FindElement(By.CssSelector(".btn-primary"));

            //Control Site Main Interactions
            controlUser.SendKeys("james.clark@interact-intranet.com");
            controlPassword.SendKeys("R3trieval.br!nk.0verst@te");
            controlLoginBtn.Click();


            //Control Challenge Page Elements
            IWebElement controlCodeBox = sitedriver.FindElement(By.CssSelector(".challenge-code"));
            IWebElement controlCodeReason = sitedriver.FindElement(By.CssSelector(".challenge-reason"));
            IWebElement controlCodeSubmit = sitedriver.FindElement(By.CssSelector(".challenge-submit"));

            //Control Challenge Page Interactions

            //Paste challenge code
            ControlPlus(controlCodeBox, "v");

            if (testSite == false)
            {
                wait.Until(ExpectedConditions.ElementToBeClickable(controlCodeReason));
                controlCodeReason.SendKeys("ticket");
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

            sitedriver.SwitchTo().Window(sitedriver.WindowHandles.First());
            js.ExecuteScript("new_tab.close()");
            

            //Launch Control and copy the code
            //GetControlCode(testSite, sitedriver);

            //Move back to Main Site Challenge Page and paste in final code
            ControlPlus(challengeFinal, "v");

            challengeSubmit.Click();
            string loggedInURL = sitedriver.Url;
            js.ExecuteScript("window.open('" + loggedInURL + "support')");
            js.ExecuteScript("window.open('" + loggedInURL + "Interact/Pages/Admin/Default.aspx?section=-1')");
            js.ExecuteScript("window.open('" + loggedInURL + "Interact/Pages/Admin/People/Staff/Default.aspx?section=106')");
            js.ExecuteScript("window.open('" + loggedInURL + "InteractV7/UMI/List')");
            sitedriver.SwitchTo().Window(sitedriver.WindowHandles.First());

            while (isBrowserClosed(sitedriver) == false)
            {
                Thread.Sleep(60000);
            }
            sitedriver.Quit();
        }
        public static void ControlPlus(IWebElement element, string key)
        {
            element.SendKeys(Keys.LeftControl + key); //passes in the element to be targeted, then pass in the key to use with control
        }
        public static bool isBrowserClosed(IWebDriver driver)
        {
            bool isClosed = false;
            try
            {
                string currentURLTest = driver.Url;
            }
            catch (Exception e)
            {
                isClosed = true;
            }

            return isClosed;
        }
    }
}
