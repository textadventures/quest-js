using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using TextAdventures.Quest;
using util = TextAdventures.Utility;
using System.Threading;

namespace QuestCompiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Compiler compiler = new Compiler();

        public MainWindow()
        {
            InitializeComponent();
            compiler.CompileFinished += compiler_CompileFinished;
            compiler.StatusUpdated += compiler_StatusUpdated;
            compiler.Progress += compiler_Progress;
            MessageBox.Show("QuestJS does not display in-game maps.  If you have one included, it will be ignored."+Environment.NewLine + Environment.NewLine +"QuestJS has most of the same HTML elements that Quest has, but some have a different ID or class.  (Use your developer tools in your browser if you'd like to find the ID or class of an element.)" + Environment.NewLine + Environment.NewLine + "If you have named an object \"key\", it will be renamed \"key \".  (All scripts will still point to it, and the alias will be set to \"key\".)", "A Few Words of Warning Concerning QuestJS 6.4");
        }

        private void cmdBrowseSource_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = txtSource.Text;
            dlg.Filter = "Quest games (*.quest)|*.quest";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                txtSource.Text = dlg.FileName;
            }
        }

        private void cmdBrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Please select destination folder";
            dlg.SelectedPath = txtDestination.Text;
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtDestination.Text = dlg.SelectedPath;
            }
        }

        private void cmdCompile_Click(object sender, RoutedEventArgs e)
        {
            cmdCompile.IsEnabled = false;
            progress.Visibility = Visibility.Visible;
            SaveValues();
            txtOutput.Text = "";
            progress.Value = 0;
            TaskbarItemInfo.ProgressValue = 0;
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            var options = new CompileOptions
            {
                Filename = txtSource.Text,
                OutputFolder = txtDestination.Text,
                DebugMode = (bool)chkDebug.IsChecked,
                Profile = cmbProfile.Text,
                Minify = (bool)chkMinify.IsChecked,
                Gamebook = (bool)chkGamebook.IsChecked,
            };
            Thread newThread = new Thread(() => compiler.StartCompile(options));
            newThread.Start();
        }

        private void compiler_CompileFinished(object sender, Compiler.CompilerResults result)
        {
            BeginInvoke(() =>
            {
                cmdCompile.IsEnabled = true;
                progress.Visibility = Visibility.Collapsed;
                TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                if (result.Success)
                {
                    if (cmbProfile.Text == "Web")
                    {
                        System.Diagnostics.Process.Start(result.IndexHtml);
                    }
                    if (result.Warnings.Any())
                    {
                        txtOutput.Text += string.Join(Environment.NewLine, result.Warnings);
                    }
                }
                else
                {
                    txtOutput.Text += string.Join(Environment.NewLine, result.Errors);
                }
            });
        }

        private void compiler_StatusUpdated(object sender, Compiler.StatusUpdate e)
        {
            BeginInvoke(() => txtOutput.Text += e.Message + Environment.NewLine);
        }

        private void compiler_Progress(object sender, Compiler.ProgressEventArgs e)
        {
            BeginInvoke(() =>
            {
                progress.Value = e.Progress;
                TaskbarItemInfo.ProgressValue = (double)e.Progress / 100;
            });
        }

        private void BeginInvoke(Action method)
        {
            Dispatcher.BeginInvoke(method);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtSource.Text = util.Registry.GetSetting("Quest", "Compiler", "LastSource", "") as string;
            txtDestination.Text = util.Registry.GetSetting("Quest", "Compiler", "LastDestination", "") as string;
            chkDebug.IsChecked = util.Registry.GetSetting("Quest", "Compiler", "LastDebug", false) as int? == 1;
            chkMinify.IsChecked = util.Registry.GetSetting("Quest", "Compiler", "LastMinify", false) as int? == 1;
            chkGamebook.IsChecked = util.Registry.GetSetting("Quest", "Compiler", "LastGamebook", false) as int? == 1;

            string lastProfile = util.Registry.GetSetting("Quest", "Compiler", "LastProfile", "") as string;

            foreach (string profile in compiler.GetValidProfiles())
            {
                int index = cmbProfile.Items.Add(profile);
                if (profile == lastProfile)
                {
                    cmbProfile.SelectedIndex = index;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveValues();
        }

        private void SaveValues()
        {
            util.Registry.SaveSetting("Quest", "Compiler", "LastSource", txtSource.Text);
            util.Registry.SaveSetting("Quest", "Compiler", "LastDestination", txtDestination.Text);
            util.Registry.SaveSetting("Quest", "Compiler", "LastProfile", cmbProfile.SelectedValue);
            util.Registry.SaveSetting("Quest", "Compiler", "LastDebug", chkDebug.IsChecked == true ? 1 : 0);
            util.Registry.SaveSetting("Quest", "Compiler", "LastMinify", chkMinify.IsChecked == true ? 1 : 0);
            util.Registry.SaveSetting("Quest", "Compiler", "LastGamebook", chkGamebook.IsChecked == true ? 1 : 0);
        }

        private void cmdLoadSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Settings (*.settings)|*.settings";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                SettingsManager mgr = new SettingsManager();
                var settings = mgr.LoadSettings(dlg.FileName);
                LoadSettings(settings);
            }
        }

        private void cmdSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Settings (*.settings)|*.settings";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                SettingsManager mgr = new SettingsManager();
                var settings = SaveSettings();
                mgr.SaveSettings(dlg.FileName, settings);
            }
        }

        private void LoadSettings(Settings settings)
        {
            txtSource.Text = settings.Source;
            txtDestination.Text = settings.Destination;
            chkDebug.IsChecked = settings.Debug;
            chkMinify.IsChecked = settings.Minify;
            chkGamebook.IsChecked = settings.Gamebook;

            cmbProfile.Items.Clear();

            foreach (string profile in compiler.GetValidProfiles())
            {
                int index = cmbProfile.Items.Add(profile);
                if (profile == settings.Profile)
                {
                    cmbProfile.SelectedIndex = index;
                }
            }
        }

        private Settings SaveSettings()
        {
            return new Settings
            {
                Source = txtSource.Text,
                Destination = txtDestination.Text,
                Profile = (string)cmbProfile.SelectedValue,
                Debug = chkDebug.IsChecked == true,
                Minify = chkMinify.IsChecked == true,
                Gamebook = chkGamebook.IsChecked == true,
            };
        }
    }
}
