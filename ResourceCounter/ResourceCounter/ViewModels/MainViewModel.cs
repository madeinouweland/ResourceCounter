using System.Collections.ObjectModel;
using System.IO;
using ResourceCounter.Models;
using System.Collections.Generic;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Linq;
using ResourceCounter.helpers;

namespace ResourceCounter.ViewModels {
    public class MainViewModel : ObservableObject {
        private string rootPath;
        private List<XamlFile> xamlFiles;
        public ObservableCollection<DataResource> ApplicationResources { get; set; }

        public string RootPath {
            get { return rootPath; }
            set {
                if (rootPath != value) {
                    rootPath = value;
                    OnPropertyChanged(() => RootPath);
                    var s = new Settings { RootPath = value };
                    Serializer.SaveSettings(s);
                }
            }
        }

        public ICommand AnalyseCommand { get; set; }

        public MainViewModel() {
            AnalyseCommand = new DelegateCommand<object>(o => {
                Analyse();
            }, o => {
                return true;
            });

            ApplicationResources = new ObservableCollection<DataResource>();

            RootPath = Serializer.LoadSettings<Settings>().RootPath;
        }

        bool IsUsed(string staticResource) {
            foreach (var xamlFile in xamlFiles) {
                if (xamlFile.XAML.IndexOf("StaticResource " + staticResource) > -1)
                    return true;
            }
            return false;
        }

        private void DetermineStyles(XamlFile xamlFile) {
            int pos = xamlFile.XAML.IndexOf("x:Key=", 0);

            while (pos > -1) {
                int end = xamlFile.XAML.IndexOf("\"", pos + 7);
                var key = xamlFile.XAML.Substring(pos + 7, end - pos - 7);
                ApplicationResources.Add(new DataResource { DefinedInXamlFile = xamlFile, Key = key, UsedInXamlFiles = new List<XamlFile>() });
                pos = xamlFile.XAML.IndexOf("x:Key=", end);
            }
        }

        private void Analyse() {
            ApplicationResources.Clear();

            xamlFiles = new List<XamlFile>();
            LoadFiles(rootPath);

            foreach (var xamlFile in xamlFiles) {
                DetermineStyles(xamlFile);
            }

            foreach (var rec in ApplicationResources) {
                foreach (var xamlFile in xamlFiles) {
                    xamlFile.Count = new Regex("StaticResource " + rec.Key).Matches(xamlFile.XAML).Count;
                    if (xamlFile.Count > 0) {
                        rec.UsedInXamlFiles.Add(xamlFile);
                    }
                }
            }

            OnPropertyChanged(() => UnusedCount);

        }

        public int UnusedCount {
            get {
                return ApplicationResources.Count(x => x.UsedInXamlFiles.Count == 0);
            }
        }

        private void LoadFiles(string path) {
            var di = new DirectoryInfo(path);
            var items = di.GetFiles("*.xaml");
            foreach (var item in items) {
                xamlFiles.Add(new XamlFile {
                    Path = item.FullName,
                    XAML = LoadXAML(item.FullName),
                });
            }

            var folders = di.GetDirectories();
            foreach (var folder in folders) {
                LoadFiles(folder.FullName);
            }
        }

        string LoadXAML(string path) {
            using (var reader = new StreamReader(path)) {
                var text = reader.ReadToEnd();
                reader.Close();
                return text;
            }
        }
    }
}
