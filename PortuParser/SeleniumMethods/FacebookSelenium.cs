using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iText.IO.Font.Constants;

namespace PortuParser.SeleniumMethods
{
    class FacebookSelenium : SeleniumSocialNetwork 
    {
        public PdfFont font = PdfFontFactory.CreateFont("c:/windows/fonts/times.ttf", "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
        public override void Login(ChromeDriver web, Document logger, string login, string password)
        {
            try
            {
                web.Navigate().GoToUrl("https://facebook.com");
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
                web.FindElement(By.XPath("//input[@id='email']")).SendKeys(login);
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
                web.FindElement(By.XPath("//input[@id='pass']")).SendKeys(password + "\n");
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось ввести пароль" + ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }
        }
        public override void Enter(ChromeDriver web, Document logger, string link)
        {
            try
            {
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
            Thread.Sleep(1000);
            Scroll(web, logger);
            Thread.Sleep(1000);
            List<IWebElement> elements = new List<IWebElement>(web.FindElements(By.XPath("//div/div/div[@class='lzcic4wl']")));
            Paragraph header = new Paragraph(web.FindElement(By.XPath("//div[@class='bi6gxh9e aov4n071'][1]")).Text)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20).SetFont(boldFont);
            LineSeparator ls = new LineSeparator(new SolidLine());
            Paragraph newline = new Paragraph(new Text("\n"));
            document.Add(header);
            document.Add(ls);
            document.Add(newline);
            for (int i = 0; i < elements.Count; ++i)
            {
                InsertToFile(web, elements[i], document);
                document.Add(newline);
            }
        }

        public override void Scroll(ChromeDriver web, Document logger)
        {
            try
            {
                Thread.Sleep(1000);
                IJavaScriptExecutor js = web;
                js.ExecuteScript("window.scrollTo(0, 2000);");
                Thread.Sleep(1000);
                js.ExecuteScript("window.scrollTo(0, 4000);");
                Thread.Sleep(400);
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> { new Paragraph(new Text("ОШИБКА. Не удалось прокрутить страницу вниз" + ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }
        }

        public override void InsertToFile(ChromeDriver web, IWebElement element, Document document)
        {
            InsertText(element, document);

            InsertImage(web, element, document);

            InsertLink(element, document);
        }

        public override void InsertText(IWebElement element, Document document)
        {
            var textElement = element.FindElements(By.XPath(".//div[@class='kvgmc6g5 cxmmr5t8 oygrvhab hcukyx3x c1et5uql ii04i59q']"));
            Paragraph text = textElement.Count > 0 ? new Paragraph(new Text(textElement[0].Text)) : null;
            if (text != null)
                document.Add(text.SetFont(font));
        }

        public override void InsertImage(ChromeDriver web, IWebElement element, Document document)
        {
            var imgElement = element.FindElements(By.XPath(".//img[@class='i09qtzwb n7fi1qx3 datstx6m pmk7jnqg j9ispegn kr520xx4 k4urcfbm bixrwtb6']"));
            if (imgElement.Count > 0)
            {
                String base64Source = imgElement[0].GetAttribute("src");
                using (WebClient client = new WebClient())
                {
                    var base64 = client.DownloadData(base64Source);
                    using (MemoryStream imgStream = new MemoryStream(base64))
                    {
                        byte[] imageBytes = imgStream.ToArray();
                        ImageData rawImage = ImageDataFactory.Create(imageBytes);
                        Image image = new Image(rawImage).SetTextAlignment(TextAlignment.CENTER);
                        document.Add(image);
                    }
                }
            }
            else
            {
                var imgsElement = element.FindElements(By.XPath(".//img[@class='i09qtzwb n7fi1qx3 datstx6m pmk7jnqg j9ispegn kr520xx4 k4urcfbm']"));
                for (int i = 0; i < imgsElement.Count; ++i)
                {
                    String base64Source = imgsElement[i].GetAttribute("src");
                    using (WebClient client = new WebClient())
                    {
                        var base64 = client.DownloadData(base64Source);
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

        public override void InsertLink(IWebElement element, Document document)
        {
            var linksElement = element.FindElements(By.XPath(".//a[@class='oajrlxb2 g5ia77u1 qu0x051f esr5mh6w e9989ue4 r7d6kgcz rq0escxv nhd2j8a9 a8c37x1j " +
                "p7hjln8o kvgmc6g5 cxmmr5t8 oygrvhab hcukyx3x jb3vyjys rz4wbd8a qt6c0cv9 a8nywdso i1ao9s8h esuyzwwr f1sip0of lzcic4wl gmql0nx0 p8dawk7l']"));
            Link link = linksElement.Count > 0 ? new Link("link to news", PdfAction.CreateURI(linksElement[0].GetAttribute("href").ToString()))
                : null;
            Paragraph hyperLink = link != null ? new Paragraph().Add(link.SetBold().SetUnderline()
            .SetItalic().SetFontColor(ColorConstants.BLUE)) : null;
            if (hyperLink != null)
                document.Add(hyperLink);
        }
    }
}
