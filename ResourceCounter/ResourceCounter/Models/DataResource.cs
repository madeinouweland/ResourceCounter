using System.Collections.Generic;
using ResourceCounter.ViewModels;

namespace ResourceCounter.Models {
    public class DataResource : ObservableObject{
        private string key;

        public string Key {
            get { return key; }
            set {
                if (key != value) {
                    key = value;
                    OnPropertyChanged(() => Key);
                }
            }
        }

        public XamlFile DefinedInXamlFile { get; set; }
        public List<XamlFile> UsedInXamlFiles { get; set; }

        public string Occurrences {
            get {
                var o="";
                foreach(var f in UsedInXamlFiles){
                    o += f.FileName+" ";
                }
                return o;
            }
        }
    }
}
