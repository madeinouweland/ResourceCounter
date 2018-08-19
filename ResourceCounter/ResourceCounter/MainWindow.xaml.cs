using ResourceCounter.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ResourceCounter.ViewModels;

namespace ResourceCounter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void masterGrid_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var row = sender as DataGridRow;
                if (row != null)
                {
                    var resourceDeclaration = row.DataContext as DataResource;
                    

                    if (resourceDeclaration == null || !File.Exists(resourceDeclaration.DefinedInXamlFile.Path))
                    {
                        return;
                    }

                    // combine the arguments together
                    // it doesn't matter if there is a space after ','
                    string argument = $@"/select, ""{resourceDeclaration.DefinedInXamlFile.Path}""";

                    System.Diagnostics.Process.Start("explorer.exe", argument);

                    Clipboard.SetText(resourceDeclaration.Key);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while opening declaration file:" + ex.Message);
            }
           
        }

        private void detailGrid_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var vm = DataContext as MainViewModel;
                var row = sender as DataGridRow;

                if (row != null && vm != null)
                {
                    var usageXamlFile = row.DataContext as XamlFile;

                    if (usageXamlFile == null || !File.Exists(usageXamlFile.Path))
                    {
                        return;
                    }

                    // combine the arguments together
                    // it doesn't matter if there is a space after ','
                    string argument = $@"/select, ""{usageXamlFile.Path}""";

                    System.Diagnostics.Process.Start("explorer.exe", argument);

                    Clipboard.SetText(vm.SelectedResource.Key);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while opening usage file:" + ex.Message);
            }
        }
    }
}