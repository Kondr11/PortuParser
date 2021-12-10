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
    class PublicoCrawler : Crawler
    {
        public PdfFont font = PdfFontFactory.CreateFont("c:/windows/fonts/times.ttf", "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
        public override void AddTitles(HtmlNode nod, Document logger, Document document)
        {
            try
            {
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ITALIC);
                Paragraph newline = new Paragraph(new Text("\n"));
                var title = nod.SelectSingleNode(".//h1").InnerText;
                if (title != null)
                {
                    Paragraph title1 = new Paragraph(HttpUtility.HtmlDecode(title)).SetTextAlignment(TextAlignment.CENTER).SetFontSize(18);
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

        public override void Copy(HtmlDocument doc, Document logger, Document document)
        {
            var nod = doc.DocumentNode.SelectSingleNode("//article[@id='story']");
            if (nod != null)
            {
                var nodContent = nod.SelectSingleNode("//div[@id='story-content']");
                AddTitles(nod, logger, document);
                if (nodContent != null)
                {
                    if (nod.SelectSingleNode(".//img") != null)
                        InsertImage(nod, logger, document);
                    if (!nodContent.OuterHtml.Contains("kicker kicker--exclusive"))
                    {
                        InsertText(nod, logger, document);
                    }
                    else
                    {
                        Paragraph text = new Paragraph(new Text("O conteúdo completo está disponível apenas para Subscritores."));
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
                string base64Source = nod.SelectSingleNode(".//img").GetAttributeValue("data-src", null);
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
            return node.SelectSingleNode(".//div[@class='byline-dateline']") != null ?
                        new Paragraph(new Text(HttpUtility.HtmlDecode(node.SelectSingleNode(".//div[@class='byline-dateline']").InnerText)))
                        .SetTextAlignment(TextAlignment.CENTER).SetFontSize(8) :
                        null;
        }

        public Paragraph GetTextUnderImage(HtmlNode node)
        {
            return node.SelectSingleNode(".//figcaption") != null ?
                        new Paragraph(new Text(HttpUtility.HtmlDecode(node.SelectSingleNode(".//figcaption").InnerText)))
                        .SetTextAlignment(TextAlignment.CENTER).SetFontSize(8) :
                        null;
        }

        public override void InsertText(HtmlNode nod, Document logger, Document document)
        {
            try
            {
                var nodContent = nod.SelectNodes(".//div[@id='story-body']/p");
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

        public override List<HtmlNode> OtherLevels(HtmlDocument doc, Document logger, Document document)
        {
            if (doc.DocumentNode.SelectSingleNode("//a") != null)
            {
                var urlList = doc.DocumentNode.SelectNodes("//a").Where(s => !s.OuterHtml.Contains("http")
                    && !s.OuterHtml.Contains("publico") && !s.OuterHtml.Contains("javascript")
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
                    && !s.OuterHtml.Contains("publico") && !s.OuterHtml.Contains("javascript")
                    && !s.OuterHtml.Contains("@") && !s.OuterHtml.Contains("#comments")).ToList();
            AddUniqueUrl(urlList);
            Url = "https://www.publico.pt/";
            AddHeader("PUBLICO", document);
            Copy(doc, logger, document);

            return urlList;
        }
    }
}
