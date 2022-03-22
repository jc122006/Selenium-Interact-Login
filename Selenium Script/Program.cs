﻿using System;
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
            string siteUrl = Console.ReadLine();
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
            Actions actionsSite = new Actions(sitedriver);
            challengeBox.Click();
            //Select all then copy the challenge code
            ControlPlus(challengeBox, "a");

            ControlPlus(challengeBox, "c");

            //Launch Control and copy the code
            GetControlCode(testSite);

            //Move back to Main Site Challenge Page and paste in final code
            ControlPlus(challengeFinal, "v");

            challengeSubmit.Click();
        }

        public static void GetControlCode(bool testSite)
        {
            //Control Site (start with maximized window)
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            IWebDriver controlDriver = new ChromeDriver(options);
            WebDriverWait wait = new WebDriverWait(controlDriver, TimeSpan.FromSeconds(10));
            controlDriver.Navigate().GoToUrl("https://control.interactgo.com/Account/Login"); //Open in new tab instead of window, it will be quicker

            //Control Site Main Login Elements
            IWebElement controlUser = controlDriver.FindElement(By.Id("Email"));
            IWebElement controlPassword = controlDriver.FindElement(By.Id("Password"));
            IWebElement controlLoginBtn = controlDriver.FindElement(By.CssSelector(".btn-primary"));

            //Control Site Main Interactions
            controlUser.SendKeys("james.clark@interact-intranet.com");
            controlPassword.SendKeys("R3trieval.br!nk.0verst@te");
            controlLoginBtn.Click();


            //Control Challenge Page Elements
            IWebElement controlCodeBox = controlDriver.FindElement(By.CssSelector(".challenge-code"));
            IWebElement controlCodeReason = controlDriver.FindElement(By.CssSelector(".challenge-reason"));
            IWebElement controlCodeSubmit = controlDriver.FindElement(By.CssSelector(".challenge-submit"));

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
            controlDriver.Quit();
        }
        public static void ControlPlus(IWebElement element, string key)
        {
            element.SendKeys(Keys.LeftControl + key); //passes in the element to be targeted, then pass in the key to use with control
        }
    }
}
