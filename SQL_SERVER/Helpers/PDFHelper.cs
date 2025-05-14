using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;


namespace SQL_SERVER.Helpers
{
    public static class PDFHelper
    {
        public static string ExtractTextFromPdf(byte[] buffer)
        {
            using (var pdf = PdfDocument.Open(buffer))
            {
                var sb = new StringBuilder();
                foreach (Page page in pdf.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
                return sb.ToString();
            }
        }
    }
}
