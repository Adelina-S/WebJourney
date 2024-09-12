using Microsoft.AspNetCore.Mvc;
using System.Buffers;


namespace WebJourney.Controllers
{
    public class ImageResult:ActionResult
    {
        public string ContentType { get; set; }
        public byte[] ImageBytes { get; set; }
        public string SourceFileName { get; set; }
        public ImageResult(byte[] bytes, string contentType, string fileName)
        {
            ImageBytes = bytes;
            ContentType = contentType;
            SourceFileName = fileName;
        }
        public override void ExecuteResult(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.Clear();
            response.ContentType = ContentType;
            if (ImageBytes != null)
            {
                var stream = new MemoryStream(ImageBytes);
                response.BodyWriter.Write(ImageBytes);
                stream.Dispose();
            }
        }
    }
}
