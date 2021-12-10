using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PortuParser.SeleniumMethods;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using System.Threading;
using iText.Layout.Element;
using iText.Kernel.Pdf.Canvas.Draw;
using System.Net.Http;
using HtmlAgilityPack;
using System.Net;
using PortuParser.CrawlerMethods;
using System.Text.RegularExpressions;

namespace PortuParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ChromeDriver web;
        List<string> facebookLinks = new List<string>()
            {
                "https://facebook.com/jornalexpresso", "https://facebook.com/AgenciaLusa",
                "https://facebook.com/ionline.jornal", "https://facebook.com/Publico",
                "https://facebook.com/cmjornal",
            };
        List<string> instargamLinks = new List<string>()
        {
            "https://www.instagram.com/jornalexpresso/", "https://www.instagram.com/lusaagenciadenoticias/",
            "https://www.instagram.com/ionline_/", "https://www.instagram.com/publico.pt/",
            "https://www.instagram.com/correiodamanhaoficial/"
        };
        List<string> twitterLinks = new List<string>()
        {
            "https://twitter.com/expresso", "https://twitter.com/Lusa_noticias/",
            "https://twitter.com/itwitting/", "https://twitter.com/publico",
            "https://twitter.com/cmjornal/"
        };
        string facebookLogin = "";
        string facebookPassword = "";
        string instaLogin = "";
        string instaPassword = "";
        string twitterLogin = "";
        string twitterPassword = "";
        string loggerPath = "LOG.PDF";

        private static readonly Regex _regex = new Regex(@"^\d+$");

        public MainWindow()
        {
            InitializeComponent();

        }

        private void CheckBoxs_Checked(object sender, RoutedEventArgs e)
        {
            SocialNetworkExpander.IsEnabled = SocialNetwork.IsChecked == true;
            SocialNetworkExpander.IsExpanded = SocialNetwork.IsChecked == true;
            WebPagesExpander.IsEnabled = WebPages.IsChecked == true;
            WebPagesExpander.IsExpanded = WebPages.IsChecked == true;
            if (Facebook.IsChecked == true || Instagram.IsChecked == true || Twitter.IsChecked == true || Expresso.IsChecked == true ||
                LUSA.IsChecked == true || JournalI.IsChecked == true || Publico.IsChecked == true || CMJournal.IsChecked == true)
            {
                Start.IsEnabled = true;
                Start.Visibility = Visibility.Visible;
            }
            else
            {
                Start.IsEnabled = false;
                Start.Visibility = Visibility.Hidden;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            using (PdfWriter pdfWriter = new PdfWriter(loggerPath))
            using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
            using (Document logger = new Document(pdfDocument))
            {
                ChromeOptions options = new ChromeOptions();
                options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);
                if (SocialNetwork.IsChecked == true)
                {
                    web = new ChromeDriver(options);
                    web.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0.5);
                }
                if (SocialNetwork.IsChecked == true)
                {
                    SocialNetworkParse(logger);
                    web.Quit();
                }
                if (WebPages.IsChecked == true)
                {
                    WebPagesParse(logger);
                }
                LineSeparator ls = new LineSeparator(new SolidLine());
                logger.Add(ls);
            }
        }

        private void SocialNetworkParse(Document logger)
        {
            FacebookParse(logger);
            InstagramParse(logger);
            TwitterParse(logger);
        }

        private void FacebookParse(Document logger)
        {
            SeleniumSocialNetwork facebook = new FacebookSelenium();
            facebook.Login(web, logger, facebookLogin, facebookPassword);
            Thread.Sleep(400);
            using (PdfWriter pdfWriter = new PdfWriter("Facebook_News(Selenium).pdf"))
            using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
            using (Document document = new Document(pdfDocument))
            {
                for (int i = 0; i < facebookLinks.Count; ++i)
                {
                    Thread.Sleep(40);
                    facebook.Enter(web, logger, facebookLinks[i]);
                    facebook.Copy(web, logger, document);
                }
            }
        }

        private void InstagramParse(Document logger)
        {
            SeleniumSocialNetwork instagram = new InstagramSelenium();
            instagram.Login(web, logger, instaLogin, instaPassword);
            using (PdfWriter pdfWriter = new PdfWriter("Instagram_News(Selenium).pdf"))
            using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
            using (Document document = new Document(pdfDocument))
            {
                for (int i = 0; i < instargamLinks.Count; ++i)
                {
                    Thread.Sleep(40);
                    instagram.Enter(web, logger, instargamLinks[i]);
                    instagram.Copy(web, logger, document);
                }
            }
        }

        private void TwitterParse(Document logger)
        {
            SeleniumSocialNetwork twitter = new TwitterSelenium();
            twitter.Login(web, logger, twitterLogin, twitterPassword);
            using (PdfWriter pdfWriter = new PdfWriter("Twitter_News(Selenium).pdf"))
            using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
            using (Document document = new Document(pdfDocument))
                for (int i = 0; i < twitterLinks.Count; ++i)
                {
                    Thread.Sleep(40);
                    twitter.Enter(web, logger, twitterLinks[i]);
                    twitter.Copy(web, logger, document);
                }
        }

        private void WebPlatformsParse(Document logger)
        {
            
        }

        private void WebPagesParse(Document logger)
        {
            using (PdfWriter pdfWriter = new PdfWriter("Crawler_News.pdf"))
            using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
            using (Document document = new Document(pdfDocument))
            {
                var web = new HtmlWeb();
                HtmlDocument doc = new HtmlDocument();
                Crawler crawler = new ExpressoCrawler();
                if (Expresso.IsChecked == true)
                {
                    doc = web.Load("https://expresso.pt");
                    int a = Level.Text != string.Empty ? int.Parse(Level.Text) : 1;
                    crawler.Enter(doc, 0, a, logger, document);
                }
                if (LUSA.IsChecked == true)
                {
                    doc = web.Load("https://www.lusa.pt");
                    int a = Level.Text != string.Empty ? int.Parse(Level.Text) : 1;
                    crawler = new LusaCrawler();
                    crawler.Enter(doc, 0, a, logger, document);
                }
                if (JournalI.IsChecked == true)
                {
                    doc = web.Load("https://ionline.sapo.pt");
                    int a = Level.Text != string.Empty ? int.Parse(Level.Text) : 1;
                    crawler = new IOnlineCrawler();
                    crawler.Enter(doc, 0, a, logger, document);
                }
                if (Publico.IsChecked == true)
                {
                    doc = web.Load("https://www.publico.pt/");
                    int a = Level.Text != string.Empty ? int.Parse(Level.Text) : 1;
                    crawler = new PublicoCrawler();
                    crawler.Enter(doc, 0, a, logger, document);
                }
                if (CMJournal.IsChecked == true)
                {
                    doc = web.Load("https://www.cmjornal.pt/");
                    int a = Level.Text != string.Empty ? int.Parse(Level.Text) : 1;
                    crawler = new CMJornalCrawler();
                    crawler.Enter(doc, 0, a, logger, document);
                }
            }
       }

        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private static bool IsTextAllowed(string text)
        {
            return _regex.IsMatch(text);
        }

    }
}
