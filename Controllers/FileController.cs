using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Upoload1.Models;

namespace Upoload1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        // private readonly static Dictionary<string, string> _contentTypes = new Dictionary<string, string>
        // { //
        //     { ".xlsx", "application/octet-stream" },
        //     { ".jpg", "application/octet-stream" }
        // };
        private readonly string _folder;

        public FileController(IWebHostEnvironment env)
        {
            // 把上傳目錄設為：wwwroot\Uploads
            _folder = $@"{env.WebRootPath}\Uploads";
        }

        // GET api/file
        [HttpGet("{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            var path = $@"{_folder}\{fileName}";

            if (string.IsNullOrEmpty(fileName) || !System.IO.File.Exists(path))
            {
                return NotFound();
            }

            // var memoryStream = new MemoryStream();
            // using(var stream = new FileStream(path, FileMode.Open))
            // {
            //     await stream.CopyToAsync(memoryStream);
            //     await stream.FlushAsync();
            // }
            // memoryStream.Seek(0, SeekOrigin.Begin);

            // // 回傳檔案到 Client 需要附上 Content Type，否則瀏覽器會解析失敗。
            // // return new FileStreamResult(memoryStream, _contentTypes[Path.GetExtension(path).ToLowerInvariant()]);
            // // 修改成這樣  每種檔案都是直接下載
            // return new FileStreamResult(memoryStream, "application/octet-stream");

            return PhysicalFile(path, "application/octet-stream");
        }

        // POST api/file
        [HttpPost("")]
        public async Task<IActionResult> UploadFile(List<IFormFile> files)
        {
            var size = files.Sum(f => f.Length);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var path = $@"{_folder}\{file.FileName}";
                    var path_checked = CheckFileExistRename(path);
                    using(var stream = new FileStream(path_checked, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        await stream.FlushAsync();
                    };

                }
            }

            return Ok(new { count = files.Count, size });
        }

        private string CheckFileExistRename(string filePath, int seq = 0)
        {
            string checkPath = filePath;
            seq++;
            if (System.IO.File.Exists(checkPath))
            {
                string ext = Path.GetExtension(checkPath);
                string fileName = Path.GetFileNameWithoutExtension(checkPath);
                string saveFolder = Path.GetDirectoryName(checkPath);
                if (seq == 1)
                    checkPath = Path.Combine(saveFolder, fileName + "(" + seq + ")" + ext);
                else
                    checkPath = Path.Combine(saveFolder, fileName.Replace("(" + (seq - 1) + ")", "(" + seq + ")") + ext);
                string backPath = CheckFileExistRename(checkPath, seq);
                return backPath;
            }
            else
                return checkPath;

        }

    }
}