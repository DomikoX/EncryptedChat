using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;

namespace GuiClient
{
    public static class RichTextBoxExtension
    {
        public static void AppendText(this RichTextBox box, string userName, string text, Color color)
        {
            TextRange head = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            head.Text = $"[{DateTime.Now:T}]<{userName}>: ";
            head.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            TextRange msg = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            msg.Text = $"{text}\r";
            msg.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
            box.ScrollToEnd();
        }

        public static void AppendHyperLink(this RichTextBox box, string userName, string text, Color color)
        {
            box.IsDocumentEnabled = true;
            TextRange head = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            head.Text = $"[{DateTime.Now:T}]<{userName}>: ";
            head.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));


            var textLink = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd) {Text = text +"\r"};
            textLink.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Blue));
            
            Hyperlink link = new Hyperlink(textLink.Start, textLink.End)
            {
                IsEnabled = true,
                TargetName = text
            };
           // textLink.Text += Environment.NewLine;
            link.Click += (sender, args) =>
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CryptedChatTemp");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                path = Path.Combine(path, text);
                
                if (File.Exists(path))
                {
                    Process.Start("explorer.exe", $"/select,{path}");
                }
            };
        
        }
    }
}