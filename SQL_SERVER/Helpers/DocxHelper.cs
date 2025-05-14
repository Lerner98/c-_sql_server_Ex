using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SQL_SERVER.Helpers
{
    public static class DocxHelper
    {
        public static string ExtractTextFromDocx(byte[] buffer)
        {
            using (MemoryStream stream = new MemoryStream(buffer))
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                return body?.InnerText ?? string.Empty;
            }
        }
    }
}
