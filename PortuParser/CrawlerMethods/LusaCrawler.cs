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
    class LusaCrawler : Crawler
    {
        public PdfFont font = PdfFontFactory.CreateFont("c:/windows/fonts/times.ttf", "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
        public override void AddTitles(HtmlNode nod, Document logger, Document document)
        {
            try
            {
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ITALIC);
                Paragraph newline = new Paragraph(new Text("\n"));
                var title = nod.SelectSingleNode("h2").InnerText;
                if (title != null)
                {
                    Paragraph title1 = new Paragraph(title).SetTextAlignment(TextAlignment.CENTER).SetFontSize(18);
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
            var nod = doc.DocumentNode.SelectSingleNode("//div[@class='article col-md-12 m-bottom20']");
            string t = string.Empty;
            if (nod != null)
            {
                var nodContent = nod.SelectSingleNode("//div[@class='article-content']");
                AddTitles(nod, logger, document);
                if (nod.OuterHtml.Contains("img"))
                    InsertImage(nod, logger, document);
                if (nodContent.Descendants("div").Where(x => x.GetAttributeValue("class", null) == "line-log m-top70").ToList().Count == 0)
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

        public override List<HtmlNode> ZeroLevel(HtmlDocument doc, Document logger, Document document)
        {
            var urlList = doc.DocumentNode.SelectNodes("//a").Where(s => s.OuterHtml.Contains("https://www.lusa.pt")
                   && !s.OuterHtml.Contains("https://www.lusa.pt/temp")).ToList();
            AddUniqueUrl(urlList);
            Url = string.Empty;
            AddHeader("LUSA", document);
            Copy(doc, logger, document);

            return urlList;
        }

        public override List<HtmlNode> OtherLevels(HtmlDocument doc, Document logger, Document document)
        {
            if (doc.DocumentNode.SelectSingleNode("//a") != null)
            {
                var urlList = doc.DocumentNode.SelectNodes("//a").Where(s => s.OuterHtml.Contains("https://www.lusa.pt")
                    && !s.OuterHtml.Contains("https://www.lusa.pt/temp")).ToList();
                AddUniqueUrl(urlList);
                Copy(NewDoc, logger, document);

                return urlList;
            }
            else
                return null;
        }

        public override void InsertImage(HtmlNode nod, Document logger, Document document)
        {
            try
            {
                string base64Source = nod.SelectSingleNode("img").GetAttributeValue("src", null);
                if (base64Source != null)
                    AddImage(base64Source, document);
            }
            catch (Exception ex)
            {
                List<Paragraph> paragraphs = new List<Paragraph> {
                    new Paragraph(new Text("Ошибка. Не удалось добавить изображение в файл" + ex.Message)),
                    new Paragraph(new Text("\n")) };
                LOG(logger, paragraphs);
            }
        }

        public void AddImage(string base64Source, Document document)
        {
            using (WebClient client = new WebClient())
            {
                var base64 = client.DownloadData(base64Source);
                using (MemoryStream imgStream = new MemoryStream(base64))
                {
                    byte[] imageBytes = imgStream.ToArray();
                    ImageData rawImage = ImageDataFactory.Create(imageBytes);
                    Image image = new Image(rawImage).SetTextAlignment(TextAlignment.CENTER);
                    Paragraph newline = new Paragraph(new Text("\n"));
                    document.Add(image);
                    document.Add(newline);
                }
            }
        }

        public override void InsertText(HtmlNode nod, Document logger, Document document)
        {
            try
            {
                var nodContent = nod.SelectNodes(".//div[@class='lt-text']");
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
    }
}
