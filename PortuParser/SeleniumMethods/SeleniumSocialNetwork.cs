using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortuParser.SeleniumMethods
{
    abstract class SeleniumSocialNetwork
    {
        public abstract void Login(ChromeDriver web, Document logger, string login, string password);
        public abstract void Enter(ChromeDriver web, Document logger, string link);
        public abstract void Copy(ChromeDriver web, Document logger, Document document);
        public abstract void Scroll(ChromeDriver web, Document logger);
        public abstract void InsertToFile(ChromeDriver web, IWebElement element, Document document);
        public abstract void InsertText(IWebElement element, Document document);
        public abstract void InsertImage(ChromeDriver web, IWebElement element, Document document);
        public abstract void InsertLink(IWebElement element, Document document);

        public void LOG(Document logger, List<Paragraph> paragraphs)
        {
            PdfFont font = PdfFontFactory.CreateFont("c:/windows/fonts/times.ttf", "cp1251", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            for (int i = 0; i < paragraphs.Count; ++i)
                logger.Add(paragraphs[i].SetFont(font));
            logger.Add(new Paragraph(new Text(DateTime.Now.ToString())));
            LineSeparator ls = new LineSeparator(new SolidLine());
            logger.Add(ls);

        }
    }
}
