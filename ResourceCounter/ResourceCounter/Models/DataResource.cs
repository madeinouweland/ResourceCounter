using ResourceCounter.ViewModels;
using System.Collections.Generic;

namespace ResourceCounter.Models
{
    public class DataResource : ObservableObject
    {
        private string _key;
        private List<XamlFile> _usedInXamlFiles;

        public string Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged(() => Key);
                }
            }
        }

        public XamlFile DefinedInXamlFile { get; set; }

        public List<XamlFile> UsedInXamlFiles
        {
            get => _usedInXamlFiles;
            set
            {
                if (_usedInXamlFiles != value)
                {
                    _usedInXamlFiles = value;
                    OnPropertyChanged(() => UsedInXamlFiles);
                    OnPropertyChanged(() => TotalOccurrences);
                }
            }
        }

        public int TotalOccurrences
        {
            get
            {
                return UsedInXamlFiles.Count;
            }
        }

        public void RaisePropertyChangedTotalOccurrences()
        {
            OnPropertyChanged(() => TotalOccurrences);

        }
    }
}