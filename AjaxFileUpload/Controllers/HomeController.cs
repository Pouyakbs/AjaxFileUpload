using AjaxFileUpload.Models;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AjaxFileUpload.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment hostingEnvironment;
        public HomeController(ILogger<HomeController> logger , IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            this.hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult UploadFile(List<IFormFile> files)
        {
            foreach (var item in files)
            {
                if (item.ContentType == "video/mp4" && item.Length < 8000000)
                {
                    string videoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Videos");

                    //create folder if not exist
                    if (!Directory.Exists(videoPath))
                        Directory.CreateDirectory(videoPath);

                    //get file extension
                    FileInfo VideoInfo = new FileInfo(item.FileName);
                    string videoName = item.FileName;

                    string videoNameWithPath = Path.Combine(videoPath, videoName);

                    using (var stream = new FileStream(videoNameWithPath, FileMode.Create))
                    {
                        item.CopyTo(stream);
                    }
                }
                else if (item.ContentType == "image/jpeg" && item.Length < 2000000)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    //get file extension
                    FileInfo fileInfo = new FileInfo(item.FileName);
                    string fileName = item.FileName;

                    string fileNameWithPath = Path.Combine(path, fileName);

                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        item.CopyTo(stream);
                    }
                }
                else
                {
                    return Content("Error");
                }

            }
            return RedirectToAction("index");
        }
        public FileResult DownLoadZip()
        {
            var webRoot = hostingEnvironment.WebRootPath;
            var fileName = "MyZip.zip";
            var tempOutput = webRoot + "/img/" + fileName;

            using (ZipOutputStream IzipOutputStream = new ZipOutputStream(System.IO.File.Create(tempOutput)))
            {
                IzipOutputStream.SetLevel(9);
                byte[] buffer = new byte[4096];
                var imageList = new List<string>();

                imageList.Add(webRoot + "/img/img_nature_wide.jpg");

                for (int i = 0; i < imageList.Count; i++)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(imageList[i]));
                    entry.DateTime = DateTime.Now;
                    entry.IsUnicodeText = true;
                    IzipOutputStream.PutNextEntry(entry);

                    using (FileStream oFileStream = System.IO.File.OpenRead(imageList[i]))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = oFileStream.Read(buffer, 0, buffer.Length);
                            IzipOutputStream.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
                IzipOutputStream.Finish();
                IzipOutputStream.Flush();
                IzipOutputStream.Close();
            }

            byte[] finalResult = System.IO.File.ReadAllBytes(tempOutput);
            if (System.IO.File.Exists(tempOutput))
            {
                System.IO.File.Delete(tempOutput);
            }
            if (finalResult == null || !finalResult.Any())
            {
                throw new Exception(String.Format("Nothing found"));

            }

            return File(finalResult, "application/zip", fileName);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
