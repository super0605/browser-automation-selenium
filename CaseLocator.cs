

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace CaseLocator
{
    [Parallelizable]
    public class Locate
    {
        private string DownloadPath = Directory.GetCurrentDirectory() + "\\Attachments\\";
        public string userName = "";
        public string password = "";
        private IWebDriver _d;
        public string UserDefinedPath;
        private string DefaultPath;
        private string crossRefNum;
        private string caseURL;
        private string caseFilesURL;
        private bool empPDF_flag = false;
        private string[] result = {"1",""};

        private void Init()
        {
            int num = 0;
            while (this._d == null && num < 5)
            {
                ++num;
                try
                {
                    ChromeDriverService defaultService = ChromeDriverService.CreateDefaultService();
                    //bool sss = text.key();
                    defaultService.HideCommandPromptWindow = true;
                    this.DefaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\cases\\";
                    this.DefaultPath = string.IsNullOrEmpty(this.UserDefinedPath) ? this.DefaultPath : this.UserDefinedPath + "\\";
                    new DirectoryInfo(this.DownloadPath).Create();
                    ChromeOptions options = new ChromeOptions();
                    options.AddUserProfilePreference("download.default_directory", (object)this.DownloadPath);
                    options.AddUserProfilePreference("download.prompt_for_download", (object)"true");
                    options.AddUserProfilePreference("disable-popup-blocking", (object)"true");
                    options.AddArgument("no-sandbox");
                    options.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", (object)1);
                    options.AddArguments(new string[1]
                    {
            "disable-infobars"
                    });
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    this._d = (IWebDriver)new ChromeDriver(defaultService, options, TimeSpan.FromMinutes(3.0));
                    this._d.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5.0);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(5000);
                }
            }
        }

        [TestMethod]
        public string[] LocateCase(string refNum, DataGridView grd, TextBox txtConsole, bool clearFolder)
        {
            this.crossRefNum = refNum;
            DataGridViewRow dataGridViewRow = grd.Rows.Cast<DataGridViewRow>().Where<DataGridViewRow>((Func<DataGridViewRow, bool>)(r => r.Cells[0].Value.ToString().Equals(refNum))).First<DataGridViewRow>();
            List<Locate.CrossRefNumber> crossRefNumberList = new List<Locate.CrossRefNumber>();
            if (!this.LoginSite(false))
            {
                 result[0] = "Cannot login";
                 return result;
            }
            
            try
            {
                this.SearchRefNum();
                Thread.Sleep(2000);
                ReadOnlyCollection<IWebElement> elements = this._d.FindElements(By.CssSelector("a[href*= 'CaseDetail.aspx?CaseID=']"));
                List<Locate.CourtCase> courtCaseList = new List<Locate.CourtCase>();
                Thread.Sleep(2000);
                foreach (IWebElement webElement in elements)
                {
                    try
                    {
                        string str = webElement.GetAttribute("href").Replace("https://www.clarkcountycourts.us/Secure/", "");
                        if (this.FindElementIfExists(By.CssSelector("a[href= '" + str + "']")) != null)
                        {
                            courtCaseList.Add(new Locate.CourtCase()
                            {
                                URL = webElement.GetAttribute("href"),
                                caseNum = webElement.Text
                            });
                        }
                        else
                        {
                            TextBox textBox = txtConsole;
                            textBox.Text = textBox.Text + refNum + " Case not found on this page: " + webElement.Text + ", " + str;
                        }
                    }
                    catch (Exception ex)
                    {
                        TextBox textBox = txtConsole;
                        string str = textBox.Text + "Case not found on this page: " + webElement.Text;
                        textBox.Text = str;
                    }
                }
                crossRefNumberList.Add(new Locate.CrossRefNumber()
                {
                    refNum = refNum,
                    caseCount = elements.Count,
                    cases = courtCaseList
                });
                try
                {
                    dataGridViewRow.Cells[2].Value = (object)elements.Count.ToString();
                    dataGridViewRow.Cells[1].Value = (object)"0";
                    grd.Refresh();

                }
                catch (Exception ex)
                {
                }
                foreach (Locate.CourtCase courtCase in courtCaseList)
                {
                    List<Locate.CaseDocument> caseDocumentList1 = new List<Locate.CaseDocument>();
                    this.caseURL = courtCase.URL;
                    this._d.Navigate().GoToUrl(courtCase.URL);
                    Thread.Sleep(500);
                    string path = this.DefaultPath + refNum.ToUpper() + "\\" + courtCase.caseNum;
                    if (clearFolder)
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        if (directoryInfo.Exists)
                        {
                            foreach (FileSystemInfo file in directoryInfo.GetFiles())
                                file.Delete();
                            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                                directory.Delete(true);
                        }
                    }
                    new DirectoryInfo(path).Create();
                    Thread.Sleep(1000);
                    new DirectoryInfo(path + "/pleadings").Create();
                    new DirectoryInfo(path + "/transcripts").Create();
                    Thread.Sleep(1000);

                    IWebElement elementIfExists;
                    int repet = 0;
                    for (elementIfExists = this.FindElementIfExists(By.CssSelector("a[href*= 'CPR.aspx?CaseID=']")); elementIfExists == null; elementIfExists = this.FindElementIfExists(By.CssSelector("a[href*= 'CPR.aspx?CaseID=']")))
                    {
                        this.caseFilesURL = this.caseURL;
                        if (!this.LoginSite(true))
                        {
                            this.Cleanup();
                            result[0] = "0";
                            result[1] = "Cannot load this page";
                            return result;
                        }
                        repet++;
                        if (repet == 3)
                        {
                            this.Cleanup();
                            result[0] = "0";
                            result[1] = "Empty case!";
                            return result;
                        }
                    }
                    this.caseFilesURL = elementIfExists.GetAttribute("href");
                    this._d.Navigate().GoToUrl(this.caseFilesURL);
                    Thread.Sleep(500);
                    try
                    {
                        foreach (IWebElement element in this._d.FindElements(By.TagName("table"))[4].FindElements(By.TagName("a")))
                        {
                            if (!string.IsNullOrEmpty(element.Text) && element.GetAttribute("href").ToLower().Contains("viewdocumentfragment.aspx?documentfragmentid="))
                            {
                                string str1 = HttpUtility.ParseQueryString(element.GetAttribute("href"))[0];
                                List<Locate.CaseDocument> caseDocumentList2 = caseDocumentList1;
                                Locate.CaseDocument caseDocument = new Locate.CaseDocument();
                                caseDocument.DocType = "pleadings";
                                string attribute = element.GetAttribute("href");
                                caseDocument.URL = attribute;
                                string text = element.Text;
                                caseDocument.fileName = text;
                                string str2 = str1;
                                caseDocument.FragmentID = str2;
                                caseDocumentList2.Add(caseDocument);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result[0] = ex.StackTrace;
                        return result;
                    }
                    Thread.Sleep(1000);
                    try
                    {
                        foreach (IWebElement element in this._d.FindElements(By.TagName("table"))[5].FindElements(By.TagName("a")))
                        {
                            if (!string.IsNullOrEmpty(element.Text) && element.GetAttribute("href").ToLower().Contains("viewdocumentfragment.aspx?documentfragmentid="))
                            {
                                string str1 = HttpUtility.ParseQueryString(element.GetAttribute("href"))[0];
                                List<Locate.CaseDocument> caseDocumentList2 = caseDocumentList1;
                                Locate.CaseDocument caseDocument = new Locate.CaseDocument();
                                caseDocument.DocType = "transcripts";
                                string attribute = element.GetAttribute("href");
                                caseDocument.URL = attribute;
                                string text = element.Text;
                                caseDocument.fileName = text;
                                string str2 = str1;
                                caseDocument.FragmentID = str2;
                                caseDocumentList2.Add(caseDocument);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    courtCase.Documents = caseDocumentList1;
                    Thread.Sleep(1000);
                    int itemNum = 1;
                    bool flag = false;
                    List<Locate.CaseDocument> caseDocumentList3 = new List<Locate.CaseDocument>();
                    List<int> intList = new List<int>();
                    caseDocumentList3.Clear();
                    intList.Clear();
                    dataGridViewRow.Cells[7].Value = (object)Convert.ToString((int)Convert.ToInt16(dataGridViewRow.Cells[7].Value) + caseDocumentList1.Count);
                    foreach (Locate.CaseDocument caseDocument in caseDocumentList1)
                    {
                        if (!flag && caseDocument.DocType == "transcripts")
                        {
                            flag = true;
                            itemNum = 1;
                        }
                        Thread.Sleep(100);
                        if (!this.downloadFile(caseDocument.URL, path + "\\" + caseDocument.DocType + "\\", caseDocument.fileName, caseDocument.FragmentID, itemNum, false))
                        {
                            caseDocumentList3.Add(caseDocument);
                            intList.Add(itemNum);
                        }
                        else
                        {
                            
                            dataGridViewRow.Cells[6].Value = (object)Convert.ToString((int)Convert.ToInt16(dataGridViewRow.Cells[6].Value) + 1);
                            if (this.empPDF_flag)
                            {
                                dataGridViewRow.Cells[5].Value = (object)Convert.ToString((int)Convert.ToInt16(dataGridViewRow.Cells[5].Value) + 1);
                                this.empPDF_flag = false;
                            }
                            else
                            {
                                dataGridViewRow.Cells[3].Value = (object)Convert.ToString((int)Convert.ToInt16(dataGridViewRow.Cells[3].Value) + 1);
                            }
                        }
                            
                        ++itemNum;
                    }
                    if (caseDocumentList3.Count > 0)
                    {
                        try
                        {
                            this.LoginSite(true);
                        }
                        catch (Exception ex)
                        {
                            result[0] = "Cannot login";
                            return result;
                        }
                        int index = 0;
                        foreach (Locate.CaseDocument caseDocument in caseDocumentList3)
                        {
                            Thread.Sleep(100);
                            if (this.downloadFile(caseDocument.URL, path + "\\" + caseDocument.DocType + "\\", caseDocument.fileName, caseDocument.FragmentID, intList[index], true))
                            {
                                dataGridViewRow.Cells[3].Value = (object)Convert.ToString((int)Convert.ToInt16(dataGridViewRow.Cells[3].Value) + 1);
                                if (this.empPDF_flag)
                                {
                                    dataGridViewRow.Cells[5].Value = (object)Convert.ToString((int)Convert.ToInt16(dataGridViewRow.Cells[5].Value) + 1);
                                    this.empPDF_flag = false;
                                }
                            }
                            else
                            {
                                TextBox textBox = txtConsole;
                                string str = textBox.Text + "Unable to download: " + this.finalPath(path + "\\" + caseDocument.DocType + "\\", intList[index], caseDocument.fileName, ".unknown");
                                textBox.Text = str;
                                dataGridViewRow.Cells[4].Value = (object)Convert.ToString((int)Convert.ToInt16(dataGridViewRow.Cells[4].Value) + 1);
                            }
                            dataGridViewRow.Cells[6].Value = (object)Convert.ToString((int)Convert.ToInt16(dataGridViewRow.Cells[6].Value) + 1);
                            ++index;
                        }
                    }
                    dataGridViewRow.Cells[1].Value = (object)Convert.ToString((int)Convert.ToInt16(dataGridViewRow.Cells[1].Value) + 1);
                    if (caseDocumentList3.Count == 0) result[1] = "Empty case!";
                }


            }
            catch (Exception ex)
            {
                result[0] = "Process Error";
                return result;
            }
            this.Cleanup();
            if(result[1]!="") result[1] = "Completed";
            else result[1] = "Empty case!";
            result[0]="0";
            return result;
        }

        private void creatpdf(string path , string content)
        {
            PdfDocument pdfDocument = new PdfDocument();
            PdfPage page = pdfDocument.AddPage();
            XGraphics xgraphics1 = XGraphics.FromPdfPage(page);
            XFont xfont = new XFont("Verdana", 10.0, XFontStyle.Bold);
            XGraphics xgraphics2 = xgraphics1;
            string text = content; 
            XFont font = xfont;
            XSolidBrush black = XBrushes.Black;
            double x = 0.0;
            double y = 0.0;
            XUnit xunit = page.Width;
            double point1 = xunit.Point;
            xunit = page.Height;
            double point2 = xunit.Point;
            XRect layoutRectangle = new XRect(x, y, point1, point2);
            XStringFormat center = XStringFormats.Center;
            xgraphics2.DrawString(text, font, (XBrush)black, layoutRectangle, center);
            pdfDocument.Save(path);
        }

        private bool downloadFile(string url, string localPath, string fileName, string fragmentID, int itemNum, bool retry)
        {
            WebClient webClient = new WebClient();
          webClient.Headers[HttpRequestHeader.Cookie] = this.cookieString(this._d);
            string ext1 = "";
            int num = 0;
            fileName = this.RemoveIllegalChars(fileName);
            while (ext1 == "" && num < 10)
            {
                ++num;
                this.empPDF_flag = false;
                string str = "";
                if (System.IO.File.Exists(this.finalPath(localPath, itemNum, fileName, ".tif")) || System.IO.File.Exists(this.finalPath(localPath, itemNum, fileName, ".pdf")))
                    return true;
                if (System.IO.File.Exists(localPath + fileName))
                    System.IO.File.Delete(localPath + fileName);
                try
                {
                    webClient.DownloadFile(url, localPath + fileName);
                    str = webClient.ResponseHeaders["Content-Type"];
                }
                catch (Exception ex)
                {
                }
                if (str.ToLower().Contains("tiff"))
                    ext1 = ".tif";
                else if (str.ToLower().Contains("pdf"))
                {
                    ext1 = ".pdf";
                }
                else
                {
                    if (System.IO.File.Exists(localPath + fileName))
                        System.IO.File.Delete(localPath + fileName);
                    if (fileName.ToLower().Contains("sealed") || fileName.ToLower().StartsWith("fus ") || fileName.ToLower().StartsWith("filed under seal") || fileName.ToLower().StartsWith("non-public"))
                    {
                        string ext2 = ".pdf";
                        this.creatpdf(this.finalPath(localPath, itemNum, fileName, ext2), "Unformat Document. You can not download this document!");
                        this.empPDF_flag = true;
                        return true;
                    }
                    if (this._d.Url.ToLower() != "https://www.clarkcountycourts.us/secure/casedocuments.aspx")
                        this.LoginSite(true);
                    try
                    {
                        this._d.FindElement(By.CssSelector("a[href*= '=" + fragmentID + "&']")).Click();
                        if (this._d.FindElement(By.CssSelector("a[href*= '=" + fragmentID + "&']")).FindElement(By.XPath("./parent::*")).FindElement(By.XPath("./parent::*")).GetCssValue("background-color").ToString() == "rgba(255, 192, 203, 1)")
                        {
                            ext1 = ".pdf";
                            this.creatpdf(this.finalPath(localPath, itemNum, fileName, ext1), "YOUR USER DOES NOT HAVE PERMISSION TO VIEW THIS DOCUMENT");
                            this.empPDF_flag = true;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    for (int index = 0; index < 20; ++index)
                    {
                        Thread.Sleep(5000);
                        try
                        {
                            if (System.IO.File.Exists(this.DownloadPath + "DocumentFragment_" + fragmentID + ".pdf"))
                            {
                                System.IO.File.Move(this.DownloadPath + "DocumentFragment_" + fragmentID + ".pdf", this.finalPath(localPath, itemNum, fileName, ".pdf"));
                                return true;
                            }
                            if (System.IO.File.Exists(this.DownloadPath + "DocumentFragment_" + fragmentID + ".tif"))
                            {
                                System.IO.File.Move(this.DownloadPath + "DocumentFragment_" + fragmentID + ".tif", this.finalPath(localPath, itemNum, fileName, ".tif"));
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                        if (!System.IO.File.Exists(this.DownloadPath + "DocumentFragment_" + fragmentID + ".pdf.crdownload") && !System.IO.File.Exists(this.DownloadPath + "DocumentFragment_" + fragmentID + ".tif.crdownload"))
                            break;
                    }
                    if (num == 10 && !retry)
                        return false;
                    if (num == 10 & retry)
                    {
                        this.createURLShortcut(this.finalPath(localPath, itemNum, fileName, ".lnk"), url);
                        return false;
                    }
                }
            }
            try
            {
                System.IO.File.Move(localPath + fileName, this.finalPath(localPath, itemNum, fileName, ext1));
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public string finalPath(string localPath, int fileNum, string fileName, string ext)
        {
            return localPath + fileNum.ToString("D3") + " - " + fileName + ext;
        }

        private void createURLShortcut(string localPath, string linkUrl)
        {
            using (StreamWriter streamWriter = new StreamWriter(localPath))
            {
                streamWriter.WriteLine("[{000214A0-0000-0000-C000-000000000046}]");
                streamWriter.WriteLine("Prop3 = 19, 11");
                streamWriter.WriteLine("[InternetShortcut]");
                streamWriter.WriteLine("IDList=");
                streamWriter.WriteLine("URL=" + linkUrl);
                streamWriter.Flush();
            }
        }

        private string cookieString(IWebDriver driver)
        {
            try
            {
                return string.Join("; ", driver.Manage().Cookies.AllCookies.Select<OpenQA.Selenium.Cookie, string>((Func<OpenQA.Selenium.Cookie, string>)(c => string.Format("{0}={1}", (object)c.Name, (object)c.Value))));
            }
            catch (Exception ex1)
            {
                try
                {
                    Thread.Sleep(4000);
                    return string.Join("; ", driver.Manage().Cookies.AllCookies.Select<OpenQA.Selenium.Cookie, string>((Func<OpenQA.Selenium.Cookie, string>)(c => string.Format("{0}={1}", (object)c.Name, (object)c.Value))));
                }
                catch (Exception ex2)
                {
                    return "";
                }
            }
        }

        private string RemoveIllegalChars(string fileName)
        {
            foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidFileNameChar, '_');
            return fileName;
        }

        private bool LoginSite(bool goToCaseFiles)
        {
            bool flag = false;
            int num = 0;
            while (!flag && num < 3)
            {
                ++num;
                try
                {
                    this.Login();
                    Thread.Sleep(2000);
                    if (goToCaseFiles)
                    {
                        this._d.Navigate().GoToUrl(this.caseURL);
                        this._d.Navigate().GoToUrl(this.caseFilesURL);
                        if (this._d.Url.ToLower().Contains("erroroccured.aspx"))
                        {
                            this.Login();
                            this.SearchRefNum();
                            this._d.Navigate().GoToUrl(this.caseURL);
                            this._d.Navigate().GoToUrl(this.caseFilesURL);
                        }
                    }
                    if (this.FindElementIfExists(By.CssSelector("a[href*= 'logout.aspx']")) != null)
                        return true;
                }
                catch (Exception ex)
                {
                }
            }
            return false;
        }

        private bool Login()
        {
            try
            {
                int num = 0;
                while (this._d != null && num <= 20)
                {
                    ++num;
                    try
                    {
                        this._d.Quit();
                        this._d = (IWebDriver)null;
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(1000);
                    }
                }
                this.Init();
                bool flag = false;
                for (int index = 0; !flag && index < 10; flag = this.FindElementIfExists(By.CssSelector("a[href*= 'logout.aspx']")) != null)
                {
                    Thread.Sleep(2000);
                    ++index;
                    try
                    {
                        this._d.Navigate().GoToUrl("https://www.clarkcountycourts.us/Secure/Login.aspx");
                        Thread.Sleep(2000);
                        if (this.FindElementIfExists(By.Id("UserName")) != null && this.FindElementIfExists(By.Id("Password")) != null & this.FindElementIfExists(By.Name("SignOn")) != null)
                        {
                            this._d.FindElement(By.Id("UserName")).SendKeys(this.userName);
                            Thread.Sleep(1000);
                            this._d.FindElement(By.Id("Password")).SendKeys(this.password);
                            Thread.Sleep(1000);
                            this._d.FindElement(By.Name("SignOn")).SendKeys(OpenQA.Selenium.Keys.Enter);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void SearchRefNum()
        {
            try
            {
                this._d.Navigate().GoToUrl("https://www.clarkcountycourts.us/Secure/Search.aspx?ID=400");
                IWebElement elementIfExists1 = this.FindElementIfExists(By.Id("CrossRefNumberOption"));
                IWebElement elementIfExists2 = this.FindElementIfExists(By.Id("CaseSearchValue"));
                IWebElement elementIfExists3 = this.FindElementIfExists(By.Id("SearchSubmit"));
                if (elementIfExists1 == null || !(elementIfExists2 != null & elementIfExists3 != null))
                    return;
                elementIfExists1.Click();
                Thread.Sleep(2000);
                elementIfExists2.SendKeys(this.crossRefNum);
                Thread.Sleep(2000);
                elementIfExists3.Click();
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
            }
        }

        private IWebElement FindElementIfExists(By by)
        {
            ReadOnlyCollection<IWebElement> elements = this._d.FindElements(by);
            return elements.Count >= 1 ? elements.First<IWebElement>() : (IWebElement)null;
        }

        [TearDown]
        private void Cleanup()
        {
            try
            {
                this._d.Quit();
            }
            catch (Exception ex)
            {
            }
        }

        private class CrossRefNumber
        {
            public string refNum { get; set; }

            public int caseCount { get; set; }

            public List<Locate.CourtCase> cases { get; set; }
        }

        private class CourtCase
        {
            public string caseNum { get; set; }

            public string URL { get; set; }

            public List<Locate.CaseDocument> Documents { get; set; }
        }

        private class CaseDocument
        {
            public string DocType { get; set; }

            public string URL { get; set; }

            public string fileName { get; set; }

            public string FragmentID { get; set; }

            public string description { get; set; }

            public string pages { get; set; }

            public bool downloaded { get; set; }
        }

        private class ParallelizableAttribute : Attribute
        {
        }
    }
}
