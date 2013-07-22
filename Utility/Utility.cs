using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace TextAdventures.Utility
{
    public static class Utility
    {
        public static string RemoveFileColonPrefix(string path)
        {
            if (path.StartsWith(@"file:\")) path = path.Substring(6);
            if (path.StartsWith(@"file:")) path = path.Substring(5);
            return path;
        }
    }
}
