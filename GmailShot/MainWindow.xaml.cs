using System.Windows;
using System.Windows.Media;
using CefSharp;
using System.Web;
using System.IO;
using System.Drawing;
using System;
using System.Windows.Media.Imaging;

namespace GmailShot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //saved to consts to improve readability
        private const string GMAIL_USERNAME_ENTRY_PAGE = "accounts.google.com/signin/v2/identifier?service=mail";
        private const string GMAIL_PASSWORD_ENTRY_PAGE = "accounts.google.com/signin/v2/sl/pwd?service=mail";
        private const string GMAIL_INBOX_PAGE = "mail.google.com/mail/u/0/#inbox";
        private const string GMAIL_USERNAME_ENTRY_JS_FORMAT = "document.getElementById('identifierId').value = '{0}'; document.getElementsByClassName('RveJvd snByac')[0].click();";
        private const string GMAIL_PASSWORD_ENTRY_JS_FORMAT = "document.getElementsByName('password')[0].value = '{0}'; document.getElementById('passwordNext').click();";

        //tracks if the inbox has been loaded at least once so we don't go saving multiple screenshots per run of the app
        private bool inboxVisited = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        
        private void ChromiumWebBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading) //wait for the page to be loaded
            {
                var url = e.Browser.MainFrame.Url;
                //choose what to do based on the browser url
                if (url.Contains(GMAIL_USERNAME_ENTRY_PAGE))
                {
                    //enter username and submit using javascript
                    string formatted = string.Format(GMAIL_USERNAME_ENTRY_JS_FORMAT,
                        Utilities.GetAppSetting(Utilities.USERNAME_CONFIG_KEY));
                    browser.ExecuteScriptAsync(formatted);
                }
                else if (url.Contains(GMAIL_PASSWORD_ENTRY_PAGE))
                {
                    //enter password and submit using javascript
                    string formatted = string.Format(GMAIL_PASSWORD_ENTRY_JS_FORMAT,
                        HttpUtility.HtmlDecode(Utilities.GetAppSetting(Utilities.PASSWORD_CONFIG_KEY)));
                    browser.EvaluateScriptAsync(formatted);
                }
                else if (url.Contains(GMAIL_INBOX_PAGE) && !inboxVisited)
                {
                    //2FA may or may not occur prior to this but the following won't execute until we are actually in the inbox
                    TakeScreenshot();
                    inboxVisited = true; //to track whether we've already done this as the inbox may be loaded again
                }
            }
        }

        private void TakeScreenshot()
        {
            //must do this because the ui is on another thread
            Dispatcher.Invoke(() =>
            {
                //generate the bitmap - the wpf way!
                var width = browser.RenderSize.Width;
                var height = browser.RenderSize.Height;
                double dpi = 96; //this is more or less arbitrary but seems to work fine
                RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)width,
                    (int)height, dpi, dpi, PixelFormats.Pbgra32); //the pixel format is a stack overflow recommendation that seems to work fine
                VisualBrush visualBrush = new VisualBrush();
                visualBrush.Visual = browser;
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext context = drawingVisual.RenderOpen())
                {
                    context.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
                }
                renderTarget.Render(drawingVisual);

                //save the bitmap to the file system
                using (MemoryStream stream = new MemoryStream())
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                    encoder.Save(stream);
                    string filename = "inboxCapture";
                    try
                    {
                        string fullPath = Utilities.SaveBitmapToFile(filename, new Bitmap(stream));
                        MessageBox.Show($"File saved to {fullPath}", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error: Unable to Save Image Capture", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });
        }
    }
}
