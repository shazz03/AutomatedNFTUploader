using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Configuration;
using System.IO;
using System.Linq;
using NLog;
using Newtonsoft.Json;

namespace AutomatedNFTUpload
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var chromeOptions = new ChromeOptions();
            //chromeOptions.AddArguments("headless");
            var chromeProfileDir = ConfigurationManager.AppSettings["ChromeProfileDir"];

            chromeOptions.AddArguments($"user-data-dir={chromeProfileDir}");

            IWebDriver driver = new ChromeDriver(chromeOptions);
            logger.Info("Initiating Request, AutomatedOpenSeaUpload");
            try
            {
                var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                var collectionUrl = ConfigurationManager.AppSettings["CollectionPath"];
                var collectionSlug = ConfigurationManager.AppSettings["CollectionSlug"];
                var baseFilePath = ConfigurationManager.AppSettings["BaseFilePath"];
                var walletPassword = ConfigurationManager.AppSettings["WalletPassword"];
                
                var metaData = File.ReadAllText($"{baseFilePath}\\_metadata.json");

                if (string.IsNullOrWhiteSpace(metaData))
                {
                    driver.Quit();
                    driver.Close();
                }

                var data = JsonConvert.DeserializeObject<List<Metadata>>(metaData);
                var count = 0;
                for (var i = 0; i <= data.Count; i++)
                {
                    var d = data[i];
                    var openSeaUrl = string.Format($"{baseUrl}{collectionUrl}", collectionSlug);
                    driver.Navigate().GoToUrl(openSeaUrl);
                    Thread.Sleep(500);
                    driver.Navigate().Refresh();
                    Thread.Sleep(500);

                    try
                    {
                        //connect wallet
                        var signinBtn = driver.FindElement(By.ClassName("wallet--btn-cta-wrapper"))
                            .FindElement(By.TagName("button"));

                        signinBtn.Click();
                        Thread.Sleep(3000);

                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        var pwdElement = driver.FindElement(By.XPath("//*[@id='password']"));
                        pwdElement.SendKeys(walletPassword);

                        //MuiButtonBase-root
                        var lgnElement = driver.FindElement(By.ClassName("MuiButtonBase-root"));
                        //Enter some text in search text box

                        //pwdElement.SendKeys(password);
                        lgnElement.Submit();
                        Thread.Sleep(1000);
                        driver.SwitchTo().Window(driver.WindowHandles.First());
                        Thread.Sleep(1000);
                        driver.Navigate().Refresh();
                    }
                    catch (Exception e)
                    {
                        //no need to connect wallet 
                        //ignore and move on
                    }

                    driver.FindElement(By.XPath("//*[@id='media']")).SendKeys($"{baseFilePath}\\{d.edition}.png");
                    driver.FindElement(By.XPath("//*[@id='name']")).SendKeys(d.name);
                    driver.FindElement(By.XPath("//*[@id='external_link']")).SendKeys(d.image);
                    driver.FindElement(By.XPath("//*[@id='description']")).SendKeys(d.description);
                    Thread.Sleep(1000);
                    var traitSectionElements = driver.FindElements(By.ClassName("AssetFormTraitSection--item"));
                    foreach (var element in traitSectionElements)
                    {
                        var propertyName = element.FindElement(By.ClassName("AssetFormTraitSection-input-label")).Text;

                        if (propertyName != "Properties") continue;
                        Thread.Sleep(1000);
                        var ele = element.FindElement(By.TagName("button"));
                        var jse = (IJavaScriptExecutor)driver;
                        jse.ExecuteScript("arguments[0].click();", ele);
                        break;
                    }
                    Thread.Sleep(1000);
                    var propertyCount = 0;
                    foreach (var at in d.attributes)
                    {
                        //AssetPropertiesForm--column
                        var propertiesCol = driver.FindElements(By.ClassName("AssetPropertiesForm--column"));
                        propertiesCol[propertyCount].FindElement(By.TagName("input")).SendKeys(at.trait_type);
                        propertyCount++;
                        propertiesCol[propertyCount].FindElement(By.TagName("input")).SendKeys(at.value);
                        propertyCount++;

                        if (d.attributes.Count > 1)
                        {
                            driver.FindElement(By.XPath("//*[@role='dialog']")).FindElement(By.TagName("section")).FindElement(By.TagName("button")).Click();
                        }
                    }
                    
                    //Navigate to google page
                    driver.FindElement(By.XPath("//*[@role='dialog']")).FindElement(By.TagName("footer")).FindElement(By.TagName("button")).Click();
                    Thread.Sleep(1000);
                    //AssetForm--submit
                    driver.FindElement(By.ClassName("AssetForm--submit")).FindElement(By.TagName("button")).Click();
                    logger.Info($"Uploaded {i}, Automated NFT Upload");
                    Thread.Sleep(10000);
                    count++;
                }

                logger.Info("Finished uploading NFT's', Automated NFT Upload");

                //Close the browser 
                driver.Close();
                driver.Dispose();
            }
            catch (Exception ex)
            {
                logger.Debug("Automated NFT Upload Error");
                logger.Error(ex);
                driver.Close();
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}