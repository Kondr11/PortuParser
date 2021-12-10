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

namespace PortuParser.CrawlerMethods
{
    class CMJornalCrawler : Crawler
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
                var title = nod.SelectSingleNode(".//h1");
                if (title != null)
                {
                    Paragraph title1 = new Paragraph(title.InnerText).SetTextAlignment(TextAlignment.CENTER).SetFontSize(18);
                    document.Add(title1);
                    document.Add(newline);
                }
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось найти первый заголовок статьи" + ex.Message)),
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
                var title = nod.SelectSingleNode(".//strong[@class='lead']");
                if (title != null)
                {
                    Paragraph title2 = new Paragraph(title.InnerText).SetTextAlignment(TextAlignment.CENTER).SetFontSize(14).SetFont(boldFont);
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
            var nod = doc.DocumentNode.SelectNodes("//body//div//section");
            HtmlNode nodArticle = null;
            if (nod != null)
            {
                foreach (var node in nod)
                {
                    if (node.GetAttributeValue("class", null).Contains("main_article"))
                    {
                        nodArticle = node;
                        break;
                    }
                }
                if (nodArticle != null)
                {
                    AddTitles(nodArticle, logger, document);
                    if (nodArticle.SelectSingleNode(".//img[@class='img-fluid']") != null)
                        InsertImage(nodArticle, logger, document);
                    if (!nodArticle.OuterHtml.Contains("exclusivos_container"))
                    {
                        InsertText(nodArticle, logger, document);
                    }
                    else
                    {
                        Paragraph text = new Paragraph(new Text("Conteúdo exclusivo para Assinantes"));
                        if (text != null)
                            document.Add(text.SetFont(font));
                    }

                }
            }
        }

        public override void InsertImage(HtmlNode nod, Document logger, Document document)
        {
            try
            {
                string base64Source = nod.SelectSingleNode(".//img[@class='img-fluid']").GetAttributeValue("src", null);
                if (base64Source != null)
                    AddImage(base64Source, nod, document);
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
                    Paragraph aboveText = GetTextAboveImage(node);
                    Paragraph text = GetTextUnderImage(node);
                    Paragraph newline = new Paragraph(new Text("\n"));
                    if (aboveText != null)
                        document.Add(aboveText);
                    document.Add(image);
                    if (text != null)
                        document.Add(text);
                    document.Add(newline);
                }
            }
        }

        public Paragraph GetTextAboveImage(HtmlNode node)
        {
            return node.SelectSingleNode(".//div[@class='autor_data']") != null ?
                        new Paragraph(new Text(node.SelectSingleNode(".//div[@class='autor_data']").InnerText))
                        .SetTextAlignment(TextAlignment.CENTER).SetFontSize(8) :
                        null;
        }

        public Paragraph GetTextUnderImage(HtmlNode node)
        {
            return node.SelectSingleNode(".//div[@class='multimedia_desc']") != null ?
                        new Paragraph(new Text(node.SelectSingleNode(".//div[@class='multimedia_desc']").InnerText))
                        .SetTextAlignment(TextAlignment.CENTER).SetFontSize(8) :
                        null;
        }

        public override void InsertText(HtmlNode nod, Document logger, Document document)
        {
            try
            {
                var nodContent = nod.SelectNodes(".//div[@class='texto_container paywall']/p");
                var list = new List<HtmlNode>();
                if (nodContent != null)
                    list = nodContent.ToList();
                var strBuilder = new StringBuilder();
                if (list.Count > 1)
                    for (int i = 0; i < list.Count; ++i)
                    {
                        strBuilder.Append(list[i].InnerText);
                    }
                Paragraph text = list.Count > 0 ? list.Count > 1 ? new Paragraph(new Text(strBuilder.ToString()))
                    : new Paragraph(new Text(list[0].InnerText)) : null;
                if (text != null && text != new Paragraph(new Text(string.Empty)))
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

        public override List<HtmlNode> OtherLevels(HtmlDocument doc, Document logger, Document document)
        {
            if (doc.DocumentNode.SelectSingleNode("//a") != null)
            {
                var urlList = doc.DocumentNode.SelectNodes("//a").Where(s => !s.OuterHtml.Contains("http")
                && !s.OuterHtml.Contains("cmjornal") && !s.OuterHtml.Contains("javascript")
                && !s.OuterHtml.Contains("@") && !s.OuterHtml.Contains("#comments")).ToList();
                AddUniqueUrl(urlList);
                Copy(NewDoc, logger, document);

                return urlList;
            }
            else
                return null;
        }

        public override List<HtmlNode> ZeroLevel(HtmlDocument doc, Document logger, Document document)
        {
            var urlList = doc.DocumentNode.SelectNodes("//a").Where(s => !s.OuterHtml.Contains("http")
            && !s.OuterHtml.Contains("cmjornal") && !s.OuterHtml.Contains("javascript")
            && !s.OuterHtml.Contains("@") && !s.OuterHtml.Contains("#comments")).ToList();
            AddUniqueUrl(urlList);
            Url = "https://www.cmjornal.pt/";
            AddHeader("CMJornal", document);
            Copy(doc, logger, document);

            return urlList;
        }
    }
}
