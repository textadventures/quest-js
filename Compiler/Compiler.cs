using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextAdventures.Quest
{
    public class CompileOptions
    {
        public string Filename { get; set; }
        public string OutputFolder { get; set; }
        public bool DebugMode { get; set; }
        public string Profile { get; set; }
        public bool Minify { get; set; }
        public bool Gamebook { get; set; }
    }

    public class Compiler
    {
        private readonly string _resourcesFolder;

        public Compiler()
        {
            _resourcesFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().CodeBase);
            _resourcesFolder = TextAdventures.Utility.Utility.RemoveFileColonPrefix(_resourcesFolder);
        }

        public Compiler(string resourcesFolder)
        {
            _resourcesFolder = resourcesFolder;
        }

        public class CompilerResults : EventArgs
        {
            public bool Success { get; set; }
            public List<string> Errors { get; set; }
            public List<string> Warnings { get; set; }
            public string IndexHtml { get; set; }
        }

        public class StatusUpdate : EventArgs
        {
            public string Message { get; set; }
        }

        public class ProgressEventArgs : EventArgs
        {
            public int Progress { get; set; }
        }

        public event EventHandler<CompilerResults> CompileFinished;
        public event EventHandler<StatusUpdate> StatusUpdated;
        public event EventHandler<ProgressEventArgs> Progress;

        public List<string> GetValidProfiles()
        {
            return new List<string> { "Web" };
        }

        public void StartCompile(CompileOptions compileOptions)
        {
            try
            {
                CompilerResults results = Compile(compileOptions);
                if (CompileFinished != null)
                {
                    CompileFinished(this, results);
                }
            }
            catch (Exception ex)
            {
                CompilerResults results = new CompilerResults { Success = false, Errors = new List<string> { ex.ToString() } };
                if (CompileFinished != null)
                {
                    CompileFinished(this, results);
                }
            }
        }

        public CompilerResults Compile(CompileOptions compileOptions)
        {
            CompilerResults result = new CompilerResults();
            GameLoader loader = new GameLoader();
            UpdateStatus(string.Format("Compiling {0} to {1}", compileOptions.Filename, compileOptions.OutputFolder));
            if (!loader.Load(compileOptions.Filename))
            {
                result.Errors = loader.Errors;
            }
            else
            {
                UpdateStatus("Loaded successfully");
                result.Warnings = loader.Warnings;
                result.Success = true;
                var substitutionText = GetSubstitutionText(loader, compileOptions.Profile);
                UpdateStatus("Copying dependencies");
                result.IndexHtml = CopyDependenciesToOutputFolder(compileOptions.OutputFolder, substitutionText, compileOptions.DebugMode, compileOptions.Profile, compileOptions.Minify, loader, compileOptions);

                string saveData = string.Empty;

                UpdateStatus("Saving");
                GameSaver saver = new GameSaver(loader.Elements);
                saver.Progress += saver_Progress;
                saveData = saver.Save();

                UpdateStatus("Copying resources");
                CopyResourcesToOutputFolder(loader.ResourcesFolder, compileOptions.OutputFolder);

                saveData += GetEmbeddedHtmlFileData(loader.ResourcesFolder);
                string saveJs = System.IO.Path.Combine(compileOptions.OutputFolder, "game.js");

                saveData = System.IO.File.ReadAllText(saveJs) + saveData;

                if (compileOptions.Minify)
                {
                    var minifier = new Microsoft.Ajax.Utilities.Minifier();
                    saveData = minifier.MinifyJavaScript(saveData, new Microsoft.Ajax.Utilities.CodeSettings
                    {
                        MacSafariQuirks = true,
                        RemoveUnneededCode = true,
                        LocalRenaming = Microsoft.Ajax.Utilities.LocalRenaming.CrunchAll
                    });

                    var encoding = (Encoding)Encoding.ASCII.Clone();
                    encoding.EncoderFallback = new Microsoft.Ajax.Utilities.JSEncoderFallback();
                    using (var writer = new System.IO.StreamWriter(saveJs, false, encoding))
                    {
                        writer.Write(saveData);
                    }
                }
                else
                {
                    System.IO.File.WriteAllText(saveJs, saveData);
                }

                UpdateStatus("Finished");
            }
            return result;
        }

        void saver_Progress(object sender, GameSaver.ProgressEventArgs e)
        {
            if (Progress != null)
            {
                Progress(this, new ProgressEventArgs { Progress = e.PercentComplete });
            }
        }

        private Dictionary<string, string> GetSubstitutionText(GameLoader loader, string profile)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("BUILD", string.Format("{0:HH}{0:mm}{0:dd}{0:MM}{0:yy}", DateTime.Now));
            result.Add("PROFILE", profile);

            foreach (string field in loader.GetSubstitutionFieldNames())
            {
                result.Add(field, loader.GetSubstitutionText(field));
            }

            return result;
        }

        // TO DO: Different profiles have different dependencies, so want to only copy the required files

        private string CopyDependenciesToOutputFolder(string outputFolder, Dictionary<string, string> substitutionText, bool debugMode, string profile, bool minify, GameLoader loader, CompileOptions options)
        {
            string indexHtm = Copy("index.htm", _resourcesFolder, outputFolder, options, loader, substitutionText, debugMode: debugMode, outputFilename: "index.html");
            Copy("style.css", _resourcesFolder, outputFolder, options, loader, substitutionText);
            Copy("jquery-ui-1.8.16.custom.css", _resourcesFolder, outputFolder, options, loader, substitutionText);
            Copy("game.js", _resourcesFolder, outputFolder, options, loader, substitutionText, debugMode);
            string jsFolder = System.IO.Path.Combine(_resourcesFolder, "js");
            string outputJsFolder = System.IO.Path.Combine(outputFolder, "js");
            System.IO.Directory.CreateDirectory(outputJsFolder);
            Copy("jquery.min.js", jsFolder, outputJsFolder, options, loader);
            Copy("jquery-ui*.js", jsFolder, outputJsFolder, options, loader);
            Copy("xregexp*.js", jsFolder, outputJsFolder, options, loader);
            Copy("jjmenu.js", jsFolder, outputJsFolder, options, loader);
            Copy("bootstrap*.js", jsFolder, outputJsFolder, options, loader);
            Copy("*.css", jsFolder, outputJsFolder, options, loader);
            Copy("bootstrap*.css", _resourcesFolder, outputFolder, options, loader, substitutionText);
            string imagesFolder = System.IO.Path.Combine(_resourcesFolder, "images");
            string outputImagesFolder = System.IO.Path.Combine(outputFolder, "images");
            System.IO.Directory.CreateDirectory(outputImagesFolder);
            Copy("*.png", imagesFolder, outputImagesFolder, options, loader, binary: true);
            return indexHtm;
        }

        private static Regex s_debugModeRegex = new Regex(@"//%%DEBUG START\r?\n?.*?//%%DEBUG END\r?\n?", RegexOptions.Singleline);
        private static Regex s_gamebookModeRegex = new Regex(@"//%%GAMEBOOK PROFILE\r?\n?.*?//%%END GAMEBOOK PROFILE\r?\n?", RegexOptions.Singleline);
        private static Regex s_textAdventureModeRegex = new Regex(@"//%%TEXTADVENTURE PROFILE\r?\n?.*?//%%END TEXTADVENTURE PROFILE\r?\n?", RegexOptions.Singleline);
        private static Dictionary<string, Regex> s_profileSpecificTextRegexes = new Dictionary<string, Regex> {
            { "web", new Regex(@"//%%WEB PROFILE\r?\n?.*?//%%END WEB PROFILE\r?\n?", RegexOptions.Singleline) },
        };

        private class MinMaxRegex
        {
            public Regex Min { get; set; }
            public Regex Max { get; set; }
        }

        private static Dictionary<WorldModelVersion, MinMaxRegex> s_minMaxRegexes = null;

        private static Dictionary<WorldModelVersion, MinMaxRegex> MinMaxRegexes
        {
            get
            {
                if (s_minMaxRegexes == null)
                {
                    s_minMaxRegexes = new Dictionary<WorldModelVersion, MinMaxRegex>();
                    foreach (WorldModelVersion v in GameLoader.PossibleVersions)
                    {
                        string versionName = v.ToString().ToUpper();
                        s_minMaxRegexes.Add(v, new MinMaxRegex
                        {
                            Min = new Regex(@"//%%MIN " + versionName + "\r?\n?.*?//%%END MIN " + versionName + "\r?\n?", RegexOptions.Singleline),
                            Max = new Regex(@"//%%MAX " + versionName + "\r?\n?.*?//%%END MAX " + versionName + "\r?\n?", RegexOptions.Singleline),
                        });
                    }
                }
                return s_minMaxRegexes;
            }
        }

        private string Copy(string filename, string sourceFolder, string outputFolder, CompileOptions options, GameLoader loader, Dictionary<string, string> substitutionText = null, bool debugMode = false, string outputFilename = null, bool binary = false)
        {
            if (filename.Contains("*"))
            {
                string[] files = System.IO.Directory.GetFiles(sourceFolder, filename);
                foreach (string file in files)
                {
                    string resultPath = System.IO.Path.Combine(outputFolder, System.IO.Path.GetFileName(file));
                    CopyInternal(file, resultPath, substitutionText, debugMode, binary, options.Profile, loader, options.Gamebook);
                }
                return null;
            }
            else
            {
                if (outputFilename == null) outputFilename = filename;
                string sourcePath = System.IO.Path.Combine(sourceFolder, filename);
                string resultPath = System.IO.Path.Combine(outputFolder, outputFilename);
                return CopyInternal(sourcePath, resultPath, substitutionText, debugMode, binary, options.Profile, loader, options.Gamebook);
            }
        }

        private string CopyInternal(string sourcePath, string resultPath, Dictionary<string, string> substitutionText, bool debugMode, bool binary, string profile, GameLoader loader, bool gamebook)
        {
            if (binary)
            {
                System.IO.File.Copy(sourcePath, resultPath, true);
                return resultPath;
            }

            string text = System.IO.File.ReadAllText(sourcePath);

            if (substitutionText != null)
            {
                foreach (var item in substitutionText)
                {
                    text = text.Replace(string.Format("$${0}$$", item.Key), item.Value);
                }
            }
            if (gamebook)
            {
                text = s_textAdventureModeRegex.Replace(text, "");
            }
            else
            {
                text = s_gamebookModeRegex.Replace(text, "");
            }
            if (!debugMode)
            {
                text = s_debugModeRegex.Replace(text, "");
            }
            foreach (var profileRegex in s_profileSpecificTextRegexes)
            {
                // remove all profile-specific scripts, apart from the script specific to the current profile
                if (!IsProfileRegexValidForProfile(profileRegex.Key, profile))
                {
                    text = profileRegex.Value.Replace(text, "");
                }
            }
            foreach (var minMaxRegex in MinMaxRegexes)
            {
                if (loader.Version > minMaxRegex.Key)
                {
                    text = minMaxRegex.Value.Max.Replace(text, "");
                }
                if (loader.Version < minMaxRegex.Key)
                {
                    text = minMaxRegex.Value.Min.Replace(text, "");
                }
            }
            System.IO.File.WriteAllText(resultPath, text);
            return resultPath;
        }

        private bool IsProfileRegexValidForProfile(string profileRegex, string profile)
        {
            switch (profile)
            {
                case "Web":
                    return (profileRegex == "web");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateStatus(string text)
        {
            if (StatusUpdated != null)
            {
                StatusUpdated(this, new StatusUpdate { Message = text });
            }
        }

        private static List<string> s_resourceExtensionsToCopy = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".wav", ".mp3" };

        private void CopyResourcesToOutputFolder(string resourcesFolder, string outputFolder)
        {
            foreach (string file in System.IO.Directory.GetFiles(resourcesFolder))
            {
                string ext = System.IO.Path.GetExtension(file).ToLower();
                if (s_resourceExtensionsToCopy.Contains(ext))
                {
                    string resultFile = System.IO.Path.Combine(outputFolder, System.IO.Path.GetFileName(file));
                    System.IO.File.Copy(file, resultFile, true);
                }
            }
        }

        private string GetEmbeddedHtmlFileData(string resourcesFolder)
        {
            StringBuilder result = new StringBuilder();

            foreach (string file in System.IO.Directory.GetFiles(resourcesFolder))
            {
                string ext = System.IO.Path.GetExtension(file).ToLower();
                if (ext == ".htm" || ext == ".html")
                {
                    result.Append(string.Format("embeddedHtml[\"{0}\"] = {1};",
                        System.IO.Path.GetFileName(file),
                        Utility.EscapeString(System.IO.File.ReadAllText(file))));
                }
                if (ext == ".js")
                {
                    result.Append(System.IO.File.ReadAllText(file));
                    result.Append(Environment.NewLine);
                }
            }

            return result.ToString();
        }
    }
}
