using ResourceCounter.helpers;
using ResourceCounter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace ResourceCounter.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        #region Data members

        private string _rootPath;
        private List<XamlFile> _xamlFiles;
        private bool _isBusy;
        private readonly Dispatcher _uiDispatcher;
        private int _analyzingProgressValue;
        private int _analyzingProgressMax;
        private DataResource _selectedResource;

        #endregion Data members

        #region Properties

        public ObservableCollection<DataResource> ApplicationResourcesFlat { get; private set; }
        public ListCollectionView ApplicationResourcesGrouped { get; private set; }

        public DataResource SelectedResource
        {
            get => _selectedResource;
            set
            {
                if (_selectedResource != value)
                {
                    _selectedResource = value;
                    OnPropertyChanged(() => SelectedResource);
                }
            }
        }

        public string RootPath
        {
            get { return _rootPath; }
            set
            {
                if (_rootPath != value)
                {
                    _rootPath = value;
                    OnPropertyChanged(() => RootPath);
                }
            }
        }

        public ICommand AnalyzeCommand { get; set; }

        public int AnalyzingProgressMax
        {
            get => _analyzingProgressMax;
            set
            {
                if (_analyzingProgressMax != value)
                {
                    _analyzingProgressMax = value;
                    OnPropertyChanged(() => AnalyzingProgressMax);
                }
            }
        }

        public int AnalyzingProgressValue
        {
            get => _analyzingProgressValue;
            set
            {
                if (_analyzingProgressValue != value)
                {
                    _analyzingProgressValue = value;
                    OnPropertyChanged(() => AnalyzingProgressValue);
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(() => IsBusy);
                }
            }
        }

        #endregion Properties

        public MainViewModel()
        {
            AnalyzeCommand = new DelegateCommand<object>(o => { Task.Factory.StartNew(Analyze); }, o => !IsBusy);

            ApplicationResourcesFlat = new ObservableCollection<DataResource>();
            ApplicationResourcesGrouped = new ListCollectionView(ApplicationResourcesFlat);
            ApplicationResourcesGrouped.GroupDescriptions.Add(new PropertyGroupDescription("DefinedInXamlFile.FileName"));

            _uiDispatcher = Dispatcher.CurrentDispatcher;

            // if NOT designMode
            if (!(bool) (DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                RootPath = Serializer.LoadSettings<Settings>().RootPath;
                SaveSettings();
            }
        }

     /*   private bool IsUsed(string staticResource)
        {
            foreach (var xamlFile in _xamlFiles)
            {
                if (xamlFile.XAML.IndexOf("StaticResource " + staticResource) > -1)
                {
                    return true;
                }
            }

            return false;
        }*/

        private void DetermineStyles(XamlFile xamlFile)
        {
            int pos = xamlFile.XAML.IndexOf("x:Key=", 0);

            while (pos > -1)
            {
                int end = xamlFile.XAML.IndexOf("\"", pos + 7);
                var key = xamlFile.XAML.Substring(pos + 7, end - pos - 7);

                _uiDispatcher.Invoke(new Action(() =>
                {
                    ApplicationResourcesFlat.Add(new DataResource
                    {
                        DefinedInXamlFile = xamlFile,
                        Key = key,
                        UsedInXamlFiles = new List<XamlFile>()
                    });
                }));

                pos = xamlFile.XAML.IndexOf("x:Key=", end);
            }
        }

        private void SaveSettings()
        {
            var s = new Settings { RootPath = RootPath };
            Serializer.SaveSettings(s);
        }

        private void Analyze()
        {
            try
            {
                AnalyzingProgressValue = 0;
                AnalyzingProgressMax = 1000000;
                IsBusy = true;
                _uiDispatcher.Invoke(new Action(() => { ApplicationResourcesFlat.Clear(); }));

                _xamlFiles = new List<XamlFile>();
                LoadFiles(_rootPath);
                AnalyzingProgressMax = _xamlFiles.Count * 2;

                foreach (var xamlFile in _xamlFiles)
                {
                    AnalyzingProgressValue++;
                    DetermineStyles(xamlFile);
                }

                AnalyzingProgressMax = _xamlFiles.Count + (_xamlFiles.Count * ApplicationResourcesFlat.Count);

                foreach (var rec in ApplicationResourcesFlat)
                {
                    foreach (var xamlFile in _xamlFiles)
                    {
                        AnalyzingProgressValue++;

                        // find StaticResource with OR without specifying "ResourceKey=" (with any number of spaces between)
                        var regexToUse = $@"{{StaticResource[ ]{{1,}}(ResourceKey=){{0,1}}{rec.Key}" +
                                         // find DynamicResource with OR without specifying "ResourceKey=" (with any number of spaces between)
                                         $@"|{{DynamicResource[ ]{{1,}}(ResourceKey=){{0,1}}{rec.Key}";

                        xamlFile.Count = new Regex(regexToUse).Matches(xamlFile.XAML).Count;
                        if (xamlFile.Count > 0)
                        {
                            rec.UsedInXamlFiles.Add(xamlFile);
                            rec.RaisePropertyChangedTotalOccurrences();
                        }
                    }
                }

                OnPropertyChanged(() => UnusedCount);
            }
            catch (Exception e)
            {
            }
            finally
            {
                IsBusy = false;
            }
        }

        public int UnusedCount
        {
            get { return ApplicationResourcesFlat.Count(x => x.UsedInXamlFiles.Count == 0); }
        }

        private void LoadFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Unable to load folder");
                return;
            }

            var di = new DirectoryInfo(path);
            var items = di.GetFiles("*.xaml");
            foreach (var item in items)
            {
                _xamlFiles.Add(new XamlFile
                {
                    Path = item.FullName,
                    XAML = LoadXAML(item.FullName),
                });
            }

            var folders = di.GetDirectories();
            foreach (var folder in folders)
            {
                LoadFiles(folder.FullName);
            }
        }

        private string LoadXAML(string path)
        {
            using (var reader = new StreamReader(path))
            {
                var text = reader.ReadToEnd();
                reader.Close();
                return text;
            }
        }
    }
}