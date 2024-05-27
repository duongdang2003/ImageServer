using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;


namespace ImageServer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public class AlbumInfo
        {
            public string UserName { get; set; }
            public string AlbumName { get; set; }
        }
        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files, string jsonData)
        {
            try
            {
                // Add CORS headers before any other action
                Response.AppendHeader("Access-Control-Allow-Origin", "*");
                Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Authorization");

                AlbumInfo albumInfo = JsonConvert.DeserializeObject<AlbumInfo>(jsonData);
                if (files != null && files.Any())
                {
                    List<string> savedFilePaths = new List<string>();

                    // Specify the directory where the files will be saved
                    string uploadDirectory = Server.MapPath("~/Content/Uploads/" + albumInfo.UserName + "/" + albumInfo.AlbumName);

                    // Ensure that the directory exists; if not, create it
                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory);
                    }

                    foreach (var file in files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            // Check if the uploaded file is an image
                            if (IsImage(file.ContentType))
                            {
                                // Generate a unique filename
                                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                                // Combine the directory and filename to get the full path
                                string path = Path.Combine(uploadDirectory, fileName);

                                // Save the file to the specified path
                                file.SaveAs(path);

                                // Add the saved file path to the list
                                savedFilePaths.Add(path);
                            }
                            else
                            {
                                return Content("Please upload valid image files only.");
                            }
                        }
                        else
                        {
                            return Content("One or more files are empty.");
                        }
                    }

                    // Optionally, you can store the paths to the images in the database or perform other operations

                    // Return success message or paths of saved files
                    return Json(new { savedFilePaths = savedFilePaths });
                }
                else
                {
                    return Content("No files uploaded.");
                }
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }

        private bool IsImage(string contentType)
        {
            // Add logic to determine if the content type is an image
            string[] validImageTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            return validImageTypes.Contains(contentType.ToLower());
        }
        [HttpGet]
        public JsonResult GetImagePaths()
        {
            try
            {
                Response.AppendHeader("Access-Control-Allow-Origin", "*");
                Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Authorization");

                var imageDirectory = Server.MapPath("~/Content/Uploads");
                var imagePaths = Directory.GetFiles(imageDirectory)
                                          .Select(filePath => Url.Content("~/Content/Uploads/" + Path.GetFileName(filePath)))
                                          .ToList();
                return Json(imagePaths, JsonRequestBehavior.AllowGet);
            } catch (Exception ex)
            {
                return Json("Error: " + ex.Message);

            }
        }
    }
}