using HtmlAgilityPack;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortuParser.CrawlerMethods
{
    abstract class Crawler
    {
        public StringBuilder Builder { get; set; }
        public HtmlDocument NewDoc { get; set; }
        public HtmlWeb Web { get; set; }
        public List<HtmlNode> UniqueURL { get; set; }
        public List<string> UniqueUrlList { get; set; }
        public string Url { get; set; }
        public abstract void Copy(HtmlDocument doc, Document logger, Document document);
        public abstract void AddTitles(HtmlNode nod, Document logger, Document document);
        public abstract void InsertImage(HtmlNode nod, Document logger, Document document);
        public abstract void InsertText(HtmlNode nod, Document logger, Document document);
        public abstract List<HtmlNode> ZeroLevel(HtmlDocument doc, Document logger, Document document);
        public abstract List<HtmlNode> OtherLevels(HtmlDocument doc, Document logger, Document document);

        public void Enter(HtmlDocument doc, int level, int maxLevel, Document logger, Document document)
        {
            if (level > maxLevel)
                return;
            else
            {
                if (level != 0)
                {
                    var urlList = OtherLevels(doc, logger, document);
                    if (level < maxLevel && urlList != null)
                    {
                        ++level;
                        RoundUrlList(urlList, level, maxLevel, logger, document);
                    }
                }
                else
                {
                    ++level;
                    Initialization();
                    var urlList = ZeroLevel(doc, logger, document);
                    RoundUrlList(urlList, level, maxLevel, logger, document);
                }
                return;
            }
        }
        public void GetNewHtmlDoc(HtmlNode node)
        {
            Builder.Append(Url);
            Builder.Append(node.GetAttributeValue("href", null));
            try
            {
                NewDoc = Web.Load(Builder.ToString());
            }
            catch
            {
                NewDoc = null;
            }
            Builder.Clear();
        }

        public void AddUniqueUrl(List<HtmlNode> UrlCollection)
        {
            var removeNodes = new List<HtmlNode>();
            foreach (var nod in UrlCollection)
            {
                var href = nod.GetAttributeValue("href", null);
                if (href != null && href != "/")
                {
                    var subHref = href.Substring(href.LastIndexOf("/") + 1);
                    var indexRef = href.LastIndexOf("?ref");
                    if (indexRef > 0)
                    {
                        subHref = subHref.Substring(0, indexRef - (href.LastIndexOf("/") + 1));
                    }
                    if (UniqueUrlList.Where(u => u.Contains(subHref)).ToList().Count == 0)
                    {
                        UniqueURL.Add(nod);
                        UniqueUrlList.Add(href);
                    }
                    else
                    {
                        removeNodes.Add(nod);
                    }
                }
            }
            UrlCollection.RemoveAll(n => removeNodes.Contains(n));
        }

        public void RoundUrlList(List<HtmlNode> urlList, int level, int maxLevel, Document logger, Document document)
        {
            for (int i = 0; i < urlList.Count; ++i)
            {
                GetNewHtmlDoc(urlList[i]);
                if (NewDoc != null)
                    Enter(NewDoc, level, maxLevel, logger, document);
            }
        }
        public void AddHeader(string head, Document document)
        {
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            Paragraph header = new Paragraph(head)
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(20).SetFont(boldFont);
            LineSeparator ls = new LineSeparator(new SolidLine());
            Paragraph newline = new Paragraph(new Text("\n"));
            document.Add(header);
            document.Add(ls);
            document.Add(newline);
        }
        public void Initialization()
        {
            UniqueURL = new List<HtmlNode>();
            UniqueUrlList = new List<string>();
            if (Web == null)
                Web = new HtmlWeb();
            Builder = new StringBuilder();
        }

        public void Clear()
        {
            UniqueURL.Clear();
            UniqueUrlList.Clear();
            Builder.Clear();
        }
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
