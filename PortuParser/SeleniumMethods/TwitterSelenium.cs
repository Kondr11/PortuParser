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
    class TwitterSelenium : SeleniumSocialNetwork
    {
        public PdfFont font = PdfFontFactory.CreateFont("c:/windows/fonts/times.ttf", "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
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
            List<IWebElement> elements = new List<IWebElement>(web.FindElements(By.XPath("//section[@class='css-1dbjc4n']/div[@class='css-1dbjc4n']" +
                "/div//div[@class='css-1dbjc4n r-j5o65s r-qklmqi r-1adg3ll r-1ny4l3l']")));
            Paragraph header = new Paragraph(web.FindElement(By.XPath("//div[@class='css-901oao r-1awozwy r-18jsvk2 r-6koalj r-37j5jr r-adyw6z r-1vr29t4" +
                " r-135wba7 r-bcqeeo r-1udh08x r-qvutc0']/span[@class='css-901oao css-16my406 r-poiln3 r-bcqeeo r-qvutc0']")).Text)
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

            InsertImage(web,element, document);

            InsertLink(element, document);
        }

        public override void InsertText(IWebElement element, Document document)
        {
            var textElement = element.FindElements(By.XPath(".//div[@class='css-1dbjc4n r-18u37iz']/div[@class='css-1dbjc4n r-1iusvr4 r-16y2uox " +
                "r-1777fci r-kzbkwu']/div[@class='css-1dbjc4n'][2]/div[@class='css-1dbjc4n'][1]"));
            Paragraph text = textElement.Count > 0 ? new Paragraph(new Text(textElement[0].Text)) : null;
            if (text != null)
                document.Add(text.SetFont(font));
        }

        public override void InsertImage(ChromeDriver web, IWebElement element, Document document)
        {
            var imgElement = element.FindElements(By.XPath(".//div[@class='r-1p0dtai r-1pi2tsx r-1d2f490 r-u8s1d r-ipm5af r-13qz1uu']/div[@class='css-1dbjc4n " +
                "r-1p0dtai r-1mlwlqe r-1d2f490 r-11wrixw r-61z16t r-1udh08x r-u8s1d r-zchlnj r-ipm5af r-417010']/img[@class='css-9pa8cd']"));
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
        }

        public override void InsertLink(IWebElement element, Document document)
        {
            var linksElement = element.FindElements(By.XPath(".//a[@class='css-4rbku5 css-18t94o4 css-1dbjc4n r-1loqt21 " +
                "r-18u37iz r-16y2uox r-1wtj0ep r-1ny4l3l r-o7ynqc r-6416eg']"));
            Link link = linksElement.Count > 0 ? new Link("link to news", PdfAction.CreateURI(linksElement[0].GetAttribute("href").ToString()))
                : null;
            Paragraph hyperLink = link != null ? new Paragraph().Add(link.SetBold().SetUnderline()
            .SetItalic().SetFontColor(ColorConstants.BLUE)) : null;
            if (hyperLink != null)
                document.Add(hyperLink);
        }

        public override void Login(ChromeDriver web, Document logger, string login, string password)
        {
            try
            {
                web.Navigate().GoToUrl("https://twitter.com/i/flow/login");
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
                web.FindElement(By.XPath("//input[@class='r-30o5oe r-1niwhzg r-17gur6a r-1yadl64 r-deolkf r-homxoj " +
                    "r-poiln3 r-7cikom r-1ny4l3l r-t60dpp r-1dz5y72 r-fdjqy7 r-13qz1uu']")).SendKeys(login + "\n");
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
                Thread.Sleep(40);
                web.FindElement(By.XPath("//input[@class='r-30o5oe r-1niwhzg r-17gur6a r-1yadl64 r-deolkf r-homxoj " +
                    "r-poiln3 r-7cikom r-1ny4l3l r-t60dpp r-1dz5y72 r-fdjqy7 r-13qz1uu']")).SendKeys(password + "\n");
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось ввести пароль" + ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }
        }
    }
}
