using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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

        }
    }
}