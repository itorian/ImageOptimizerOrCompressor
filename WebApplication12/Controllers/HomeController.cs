using System.Web.Mvc;
using WebApplication12.ImageCompression;

namespace WebApplication12.Controllers
{
    public class HomeController : Controller
    {
        public string Index()
        {
            ImageCompressor imageCompressor = new ImageCompressor();
            var result = imageCompressor.CompressFile(@"C:\Users\abhimanyu\Desktop\upload\imagename.jpg", true);

            return result.ToString();
        }
    }
}