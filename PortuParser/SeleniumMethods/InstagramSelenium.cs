using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PortuParser.SeleniumMethods
{
    class InstagramSelenium : SeleniumSocialNetwork
    {
        public PdfFont font = PdfFontFactory.CreateFont("c:/windows/fonts/times.ttf", "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
        public override void Enter(ChromeDriver web, Document logger, string link)
        {
            try
            {
                Thread.Sleep(4000);
                web.Navigate().GoToUrl(link);
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> { new Paragraph(new Text(ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }
        }

        public override void Copy(ChromeDriver web, Document logger, Document document)
        {
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            Paragraph header = new Paragraph(web.FindElement(By.XPath("//h1[@class='rhpdm']")).Text)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20);
            document.Add(header).SetFont(boldFont);
            LineSeparator ls = new LineSeparator(new SolidLine());
            Paragraph newline = new Paragraph(new Text("\n"));
            document.Add(header);
            document.Add(ls);
            document.Add(newline);

            Actions actions = new Actions(web);
            actions.MoveToElement(web.FindElement(By.XPath("(//div[@class='Nnq7C weEfm']//img)[1]"))).Click().Perform();

            for (int i = 0; i < 10; ++i)
            {
                InsertToFile(web, web.FindElement(By.XPath("(//div/descendant::article)")), document);
                document.Add(newline);
                Scroll(web, logger);
            }
        }

        public override void InsertToFile(ChromeDriver web, IWebElement element, Document document)
        {
            InsertText(element, document);

            InsertImage(web, element, document);

        }

        public override void Scroll(ChromeDriver web, Document logger)
        {
            Actions actions = new Actions(web);
            Thread.Sleep(400);
            IJavaScriptExecutor js = web;
            actions.MoveToElement(web.FindElement(By.CssSelector("a.coreSpriteRightPaginationArrow"))).MoveByOffset(19,0).Click().Perform();
        }

        public override void InsertText(IWebElement element, Document document)
        {
            var textElement = element.FindElements(By.XPath("//div[@class='C7I1f X7jCj']/div[@class='C4VMK']/span"));
            Paragraph text = textElement.Count > 0 ? new Paragraph(new Text(textElement[0].Text)) : null;
            if (text != null)
                document.Add(text.SetFont(font));
        }

        public override void InsertImage(ChromeDriver web, IWebElement element, Document document)
        {
            var img = element.FindElements(By.XPath("//div[@class='ZyFrc']/div/div/img"));
            var imgURLList = new List<string>();
            string imgURL = img.Count > 0 ? img[0].GetAttribute("src") : string.Empty;
            if (imgURL != string.Empty)
            {
                imgURLList.Add(imgURL);
            }
            if (img.Count > 1)
            {
                var rightButton = web.FindElements(By.CssSelector("div.coreSpriteRightChevron"));
                while (rightButton.Count > 0)
                {

                    rightButton[0].Click();
                    rightButton = web.FindElements(By.CssSelector("div.coreSpriteRightChevron"));
                    Thread.Sleep(400);
                    var imgElements = element.FindElements(By.XPath("//div[@class='ZyFrc']/div/div/img"));
                    for (int i = 0; i < imgElements.Count(); ++i)
                    {
                        if (!imgURLList.Contains(imgElements[i].GetAttribute("src")))
                            imgURLList.Add(imgElements[i].GetAttribute("src"));
                    }
                    Thread.Sleep(100);
                }
            }
            for (int i = 0; i < imgURLList.Count; ++i)
            {
                using (WebClient client = new WebClient())
                {
                    if (imgURLList[i] != null)
                    {
                        var base64 = client.DownloadData(imgURLList[i]);
                        using (MemoryStream imgStream = new MemoryStream(base64))
                        {
                            byte[] imageBytes = imgStream.ToArray();
                            ImageData rawImage = ImageDataFactory.Create(imageBytes);
                            Image image = new Image(rawImage).SetTextAlignment(TextAlignment.CENTER);
                            document.Add(image);
                        }
                    }
                }
            }
        }

        public override void Login(ChromeDriver web, Document logger, string login, string password)
        {
            try
            {
                web.Navigate().GoToUrl("https://instagram.com");
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось перейти по URL" + ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }

            try
            {
                web.FindElement(By.XPath("//div[@class='-MzZI'][1]//input[@class='_2hvTZ pexuQ zyHYP']")).SendKeys(login);
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось ввести логин" + ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }

            try
            {
                Thread.Sleep(400);
                web.FindElement(By.XPath("//div[@class='-MzZI'][2]//input[@class='_2hvTZ pexuQ zyHYP']")).SendKeys(password + "\n");
                Thread.Sleep(400);
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось ввести пароль" + ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }
            Thread.Sleep(400);
        }

        public override void InsertLink(IWebElement element, Document document)
        {
            throw new NotImplementedException();
        }
    }
}
