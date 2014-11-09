using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuestJSWeb.Models
{
    public class CompileModel
    {
        [Required]
        [Display(Name = "File to upload")]
        public HttpPostedFileBase File { get; set; }
    }

    public class CompileResult
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public string DownloadUrl { get; set; }
    }
}