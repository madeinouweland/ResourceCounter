using System.Collections.ObjectModel;
using ResourceCounter.ViewModels;
using System.Linq;

namespace ResourceCounter.Models {
    public class XamlFile : ObservableObject {
        private string path;

        public string Path {
            get { return path; }
            set {
                if (path != value) {
                    path = value;
                    OnPropertyChanged(() => Path);
                }
            }
        }

        public string FileName {
            get {
                var parts = path.Split('\\');
                return parts.LastOrDefault();
            }
        }

        public string XAML { get; set; }
        public int Count { get; set; }
    }
}
