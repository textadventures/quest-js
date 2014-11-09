using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ionic.Zip;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using QuestJSWeb.Models;
using TextAdventures.Quest;

namespace QuestJSWeb.Controllers
{
    public class CompileController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(CompileModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tempFile = Path.GetTempFileName() + ".quest";
            model.File.SaveAs(tempFile);

            var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputFolder);

            var compiler = new Compiler(Server.MapPath("~/bin/Resources"));

            var result = compiler.Compile(new CompileOptions
            {
                Filename = tempFile,
                OutputFolder = outputFolder,
                Profile = "Web",
                Minify = false,
                DebugMode = false,
                Gamebook = true
            });

            var resultModel = new CompileResult
            {
                Success = result.Success,
                Errors = result.Errors
            };

            if (result.Success)
            {
                var guid = Guid.NewGuid().ToString();
                UploadBlob("questjs-input", guid + "/input.quest", tempFile, false);

                var zipFilename = Path.GetTempFileName() + ".zip";

                using (var zip = new ZipFile(zipFilename))
                {
                    zip.AddDirectory(outputFolder);
                    zip.Save();
                }

                var uri = UploadBlob("questjs", guid + "/html.zip", zipFilename, true);

                resultModel.DownloadUrl = uri;
            }

            return View("Result", resultModel);
        }

        private static string UploadBlob(string containerName, string filename, string file, bool publicContainer)
        {
            var container = GetAzureBlobContainer(containerName, publicContainer);
            var blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = "application/zip";
            blob.UploadFromFile(file, FileMode.Open);
            return blob.Uri.ToString();
        }

        private static CloudBlobContainer GetAzureBlobContainer(string containerName, bool isPublic)
        {
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            var account = CloudStorageAccount.Parse(connectionString);

            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions
            {
                PublicAccess = isPublic ? BlobContainerPublicAccessType.Blob : BlobContainerPublicAccessType.Off,
            });
            return container;
        }
	}
}