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
            System.Diagnostics.Debug.WriteLine("Called");

            try
            {
                // Add CORS headers before any other action
                Response.AppendHeader("Access-Control-Allow-Origin", "*");
                Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Authorization");

                System.Diagnostics.Debug.WriteLine("Number of files: " + (files != null ? files.Count() : 0));
                AlbumInfo albumInfo = JsonConvert.DeserializeObject<AlbumInfo>(jsonData);
                if (files != null && files.Any())
                {
                    List<string> savedFilePaths = new List<string>();

                    // Specify the directory where the files will be saved
                    string uploadDirectory = Server.MapPath("~/Content/Uploads/" + albumInfo.UserName + "/" + albumInfo.AlbumName);

                    // Ensure that the directory exists; if not, create it
                    if (!Directory.Exists(uploadDirectory))
                    {
                        System.Diagnostics.Debug.WriteLine("Creating directory: " + uploadDirectory);
                        Directory.CreateDirectory(uploadDirectory);
                    }

                    foreach (var file in files)
                    {
                        try
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

                                    System.Diagnostics.Debug.WriteLine("Saved file: " + path);
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
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error saving file: " + ex.Message);
                            System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                            return Content("Error saving file: " + ex.Message);
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
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                return Content("Error: " + ex.Message);
            }
        }
        private bool IsImage(string contentType)
        {
            // Add logic to determine if the content type is an image
            string[] validImageTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            return validImageTypes.Contains(contentType.ToLower());
        }

        [HttpPost]
        public JsonResult GetImageWithAlbumName(FormCollection form)
        {
            try
            {
                Response.AppendHeader("Access-Control-Allow-Origin", "*");
                Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Authorization");

                var imageDirectory = Server.MapPath("~/Content/Uploads/" + form["userName"] + "/" + form["albumName"]);
                var imagePaths = Directory.GetFiles(imageDirectory)
                                          .Select(filePath => Url.Content("https://localhost:44363/" + "Content/Uploads/" + form["userName"] + "/" + form["albumName"] + "/" + Path.GetFileName(filePath)))
                                          .ToList();
                return Json(imagePaths, JsonRequestBehavior.AllowGet);
            } catch (Exception ex)
            {
                return Json("Error: " + ex.Message);

            }
        }
        [HttpPost]
        public JsonResult GetAlbumWithUserName(string username)
        {
            try
            {
                Response.AppendHeader("Access-Control-Allow-Origin", "*");
                Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Authorization");
                System.Diagnostics.Debug.WriteLine(username);
                var userDirectory = Server.MapPath("~/Content/Uploads/" + username);
                if (!Directory.Exists(userDirectory))
                {
                    return Json(new { success = false, message = "User directory not found" }, JsonRequestBehavior.AllowGet);
                }

                var albums = new List<object>();

                foreach (var albumDirectory in Directory.GetDirectories(userDirectory))
                {
                    var albumName = Path.GetFileName(albumDirectory);
                    var imagePaths = Directory.GetFiles(albumDirectory)
                                              .Select(filePath => Url.Content("https://localhost:44363/" + "/Content/Uploads/" + username + "/" + albumName + "/" + Path.GetFileName(filePath)))
                                              .ToList();
                    
                    albums.Add(new { AlbumName = albumName, Images = imagePaths });
                }

                return Json(new { success = true, albums = albums }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetAllImages()
        {
            try
            {
                Response.AppendHeader("Access-Control-Allow-Origin", "*");
                Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Authorization");

                var uploadsDirectory = Server.MapPath("~/Content/Uploads");
                if (!Directory.Exists(uploadsDirectory))
                {
                    return Json(new { success = false, message = "Uploads directory not found" }, JsonRequestBehavior.AllowGet);
                }
                    
                var users = new List<object>();

                foreach (var userDirectory in Directory.GetDirectories(uploadsDirectory))
                {
                    System.Diagnostics.Debug.WriteLine(userDirectory);
                    var username = Path.GetFileName(userDirectory);
                    var albums = new List<object>();

                    foreach (var albumDirectory in Directory.GetDirectories(userDirectory))
                    {
                        var albumName = Path.GetFileName(albumDirectory);
                        var imagePaths = Directory.GetFiles(albumDirectory)
                                                  .Select(filePath => Url.Content("https://localhost:44363/" + "/Content/Uploads/" + username + "/" + albumName + "/" + Path.GetFileName(filePath)))
                                                  .ToList();

                        albums.Add(new { AlbumName = albumName, Images = imagePaths });
                    }

                    users.Add(new { Username = username, Albums = albums });
                }

                return Json(new { success = true, users = users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}