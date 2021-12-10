using HtmlAgilityPack;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PortuParser.CrawlerMethods
{
    class IOnlineCrawler : Crawler
    {
        public PdfFont font = PdfFontFactory.CreateFont("c:/windows/fonts/times.ttf", "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
        public override void AddTitles(HtmlNode nod, Document logger, Document document)
        {
            AddFirstTitle(nod, logger, document);
            AddSecondTitle(nod, logger, document);
        }
    public void AddFirstTitle(HtmlNode nod, Document logger, Document document)
    {
        try
        {
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ITALIC);
            Paragraph newline = new Paragraph(new Text("\n"));
            var title = nod.SelectSingleNode(".//h1").InnerText;
            if (title != null)
            {
                Paragraph title2 = new Paragraph(HttpUtility.HtmlDecode(title)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(18);
                document.Add(title2);
                document.Add(newline);
            }

        }
        catch (Exception ex)
        {
            List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось найти второй заголовок статьи" + ex.Message)),
                    new Paragraph(new Text("\n")) };
            LOG(logger, paragraphs);
        }
    }
    public void AddSecondTitle(HtmlNode nod, Document logger, Document document)
    {
        try
        {
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ITALIC);
            Paragraph newline = new Paragraph(new Text("\n"));
            var title = nod.SelectSingleNode(".//span[@class='lead']").InnerText;
            if (title != null)
            {
                Paragraph title2 = new Paragraph(HttpUtility.HtmlDecode(title)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(18);
                document.Add(title2);
                document.Add(newline);
            }

        }
        catch (Exception ex)
        {
            List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось найти второй заголовок статьи" + ex.Message)),
                    new Paragraph(new Text("\n")) };
            LOG(logger, paragraphs);
        }
    }
    public override void Copy(HtmlDocument doc, Document logger, Document document)
        {
            var nod = doc.DocumentNode.SelectSingleNode("//article");
            if (nod.OuterHtml.Contains("infonews"))
                if (!nod.OuterHtml.Contains("fb-post") && !nod.OuterHtml.Contains("audioContainer"))
                {
                    AddTitles(nod, logger, document);
                    InsertImage(nod, logger, document);
                    InsertText(nod, logger, document);
                }
        }

        public override void InsertImage(HtmlNode nod, Document logger, Document document)
        {
            try
            {
                var imgNod = nod.SelectSingleNode(".//img");
                if (imgNod != null)
                {
                    string base64Source = imgNod.GetAttributeValue("src", null);
                    if (base64Source != null)
                    {
                        AddImage(base64Source, nod, document);
                    }
                }
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось добавить изображение в файл" + ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }
        }

        public void AddImage(string base64Source, HtmlNode node, Document document)
        {
            using (WebClient client = new WebClient())
            {
                var base64 = client.DownloadData(base64Source);
                using (MemoryStream imgStream = new MemoryStream(base64))
                {
                    byte[] imageBytes = imgStream.ToArray();
                    ImageData rawImage = ImageDataFactory.Create(imageBytes);
                    Image image = new Image(rawImage).SetTextAlignment(TextAlignment.CENTER);
                    Paragraph text = GetTextUnderImage(node);
                    Paragraph newline = new Paragraph(new Text("\n"));
                    document.Add(image);
                    if (text != null)
                        document.Add(text);
                    document.Add(newline);
                }
            }
        }

        public Paragraph GetTextUnderImage(HtmlNode node)
        {
            var s = node.SelectSingleNode(".//div[@id='infonews']").InnerText;
            return  s != null ? new Paragraph(new Text(s)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(8) : null;
        }

        public override void InsertText(HtmlNode nod, Document logger, Document document)
        {
            try
            {
                var nodContent = nod.SelectNodes(".//section[@id='corpo']/p");
                var list = new List<HtmlNode>();
                if (nodContent != null)
                    list = nodContent.ToList();
                var strBuilder = new StringBuilder();
                if (list.Count > 1)
                    for (int i = 0; i < list.Count; ++i)
                    {
                        strBuilder.Append(HttpUtility.HtmlDecode(list[i].InnerText));
                    }
                Paragraph text = list.Count > 0 ? list.Count > 1 ? new Paragraph(new Text(strBuilder.ToString()))
                    : new Paragraph(new Text(list[0].InnerText)) : null;
                if (text != null)
                    document.Add(text.SetFont(font));
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось добавить текст статьи в файл" + ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }
        }

        public override List<HtmlNode> ZeroLevel(HtmlDocument doc, Document logger, Document document)
        {
            var urlList = doc.DocumentNode.SelectNodes("//a").Where(s => !s.OuterHtml.Contains("http")
                    && !s.OuterHtml.Contains("ionline") && !s.OuterHtml.Contains("javascript")
                    && !s.OuterHtml.Contains("mailto")).ToList();
            AddUniqueUrl(urlList);
            Url = "https://ionline.sapo.pt/";
            AddHeader("IONLINE", document);
            Copy(doc, logger, document);

            return urlList;
        }

        public override List<HtmlNode> OtherLevels(HtmlDocument doc, Document logger, Document document)
        {
            if (doc.DocumentNode.SelectSingleNode("//a") != null)
            {
                var urlList = doc.DocumentNode.SelectNodes("//a").Where(s => !s.OuterHtml.Contains("http")
                    && !s.OuterHtml.Contains("ionline") && !s.OuterHtml.Contains("javascript")
                    && !s.OuterHtml.Contains("mailto")).ToList();
                AddUniqueUrl(urlList);
                Copy(NewDoc, logger, document);

                return urlList;
            }
            else
                return null;
        }
    }
}
