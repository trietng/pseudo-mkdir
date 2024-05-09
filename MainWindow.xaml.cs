using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
/*using System.Text;
using System.Threading.Tasks;
*/
using System.Windows;
using System.Windows.Controls;
/*
using System.Windows.Data;
using System.Windows.Documents;
*/
using System.Windows.Input;
using System.Windows.Media;
/*
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
*/
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Reflection;

namespace FolderTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private static bool IsContainsSpecials(string input)
        {
            foreach (char inv in Path.GetInvalidFileNameChars())
            {
                if (input.Contains(inv))
                {
                    return true;
                }
            }
            foreach (char inv in Path.GetInvalidPathChars())
            {
                if (input.Contains(inv))
                {
                    return true;
                }
            }
            string sPattern = @"^(PRN|AUX|NUL|CON|COM[1-9]|LPT[1-9])$";
            return (Regex.IsMatch(input, sPattern, RegexOptions.IgnoreCase));
        }

        private void IsListContainsSpecials()
        {
            for (int i = 0; i < lstName.Items.Count; i++)
            {
                if (IsContainsSpecials(lstName.Items[i].ToString()))
                {
                    StatusMsg(3);
                    lstName.Items.Clear();
                    return;
                }
            }
            tbResult.Text = null;
        }

        private void ListAddText()
        {
            if (!string.IsNullOrWhiteSpace(txtName.Text) && !lstName.Items.Contains(txtName.Text))
            {
                if (IsContainsSpecials(txtName.Text) == true)
                {
                    StatusMsg(3);
                    return;
                }
                tbResult.Text = null;
                lstName.Items.Add(txtName.Text);
                txtName.Clear();
            }
        }

        private void IsTypeValid()
        {
            if (IsContainsSpecials(txtType.Text) == true)
            {
                StatusMsg(7);
                return;
            }
            tbResult.Text = null;
        }

        private void ButtonAddName_Click(object sender, RoutedEventArgs e)
        {
            ListAddText();
        }

        private void StatusMsg(byte k)
        {
            switch (k)
            {
                case 0:
                    tbResult.Foreground = Brushes.Green;
                    tbResult.Text = "Success";
                    btnOutput.Visibility = Visibility.Visible;
                    break;
                case 1:
                    tbResult.Foreground = Brushes.Orange;
                    tbResult.Text = "Error: Directory not found";
                    break;
                case 2:
                    tbResult.Foreground = Brushes.Red;
                    tbResult.Text = "Error: No name specified";
                    break;
                case 3:
                    tbResult.Foreground = Brushes.Purple;
                    tbResult.Text = "Error: Invalid name";
                    break;
                case 4:
                    tbResult.Foreground = Brushes.DeepPink;
                    tbResult.Text = "Error: Unspecified type of output";
                    break;
                case 5:
                    tbResult.Foreground = Brushes.Orange;
                    tbResult.Text = "Error: Filename is local folder";
                    break;
                case 6:
                    tbResult.Foreground = Brushes.Orange;
                    tbResult.Text = "Error: Folder's name is local file";
                    break;
                case 7:
                    tbResult.Foreground = Brushes.Purple;
                    tbResult.Text = "Error: Invalid type";
                    break;
            }
        }

        private void OnKeyDownHandler_txtName(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ListAddText();
            }
            if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
        }

        private bool t1 = false;
        private bool t2 = false;

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.RestoreDirectory = true;
            if (t1 == false) 
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtPath.Clear();
                txtPath.AppendText(dialog.FileName);
                t1 = true;
            }
        }

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (t2 == false)
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                var fileStream = openFileDialog.OpenFile();
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lstName.Items.Add(line);
                    }
                }
                IsListContainsSpecials();
                t2 = true;
            }
        }

        private void RadioFile_Click(object sender, RoutedEventArgs e)
        {
            spType.Visibility = Visibility.Visible;
        }

        private void RadioFolder_Click(object sender, RoutedEventArgs e)
        {
            spType.Visibility = Visibility.Hidden;
        }

        private bool IsAllChecked()
        {
            if ((radFolder.IsChecked == false) && (radFile.IsChecked == false))
            {
                return false;
            }
            return true;
        }

        private bool IsPathValid()
        {
            if (!Directory.Exists(txtPath.Text))
            {
                txtPath.Clear();
                StatusMsg(1);
                return false;
            }
            tbResult.Text = null;
            return true;
        }

        private void ButtonMkDir_Click(object sender, RoutedEventArgs e)
        {
            if (IsPathValid())
            {
                if (lstName.Items.Count == 0)
                {
                    StatusMsg(2);
                }
                else if (IsAllChecked() == false)
                {
                    StatusMsg(4);
                }
                else
                {
                    bool success = true;
                    List<string> subpath = new List<string>();
                    for (int i = 0; i < lstName.Items.Count; i++)
                    {
                        subpath.Add(lstName.Items[i].ToString());
                    }
                    if (radFile.IsChecked == true)
                    {
                        for (int i = 0; i < subpath.Count(); i++)
                        {
                            subpath[i] = Path.Combine(txtPath.Text, subpath[i] + "." + txtType.Text);
                            if (Directory.Exists(subpath[i]))
                            {
                                StatusMsg(5);
                                success = false;
                                break;
                            }
                            if (File.Exists(subpath[i]) && (checkOverwrite.IsChecked == false))
                            {
                                continue;
                            }
                            File.Create(subpath[i]);
                        }
                    }
                    else if (radFolder.IsChecked == true)
                    {
                        for (int i = 0; i < subpath.Count(); i++)
                        {
                            subpath[i] = Path.Combine(txtPath.Text, subpath[i]);
                            if (File.Exists(subpath[i]))
                            {
                                StatusMsg(6);
                                success = false;
                                break;
                            }
                            Directory.CreateDirectory(subpath[i]);
                        }
                    }
                    if (success)
                    {
                        StatusMsg(0);
                    }
                    if (checkKeep.IsChecked == false || !success)
                    {
                        lstName.Items.Clear();
                    }
                }
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            lstName.Items.Clear();
        }

        //Very important function. Do not delete.
        private static bool IsMouseOverTarget(Visual target, Point point)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(target);
            return bounds.Contains(point);
        }

        private void lstName_MouseDown(object sender, MouseEventArgs e)
        {
            lstName.SelectionMode = SelectionMode.Extended;
            //Index from point. Check if the mouse is over a ListBox item.
            int index = -1;
            for (int i = 0; i < lstName.SelectedItems.Count; i++)
            {
                var lbi = lstName.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (lbi == null) continue;
                if (IsMouseOverTarget(lbi, e.GetPosition((IInputElement)lbi)))
                {
                    index = i;
                    break;
                }
            }
            //Unselect selected items if the MouseDown event (mouse click) is not at a ListBox item.
            if (index <= -1)
            {
                lstName.SelectedItems.Clear();
            }
        }

        private void ListRemoveItems()
        {
            foreach (var item in lstName.SelectedItems.Cast<object>().ToList())
            {
                lstName.Items.Remove(item);
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            ListRemoveItems();
        }

        private void OnKeyDownHandler_txtType(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsTypeValid();
                Keyboard.ClearFocus();
            }
            if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
        }

        private void OnKeyDownHandler_lstName(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                lstName.UnselectAll();
            }
            if (e.Key == Key.Delete)
            {
                ListRemoveItems();
            }
        }

        private void OnKeyDownHandler_txtPath(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                IsPathValid();
            }
            if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
        }

        private void ButtonOutput_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(txtPath.Text);
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "pseudo-mkdir\nA WPF C# GUI tool for simultaneously creating new folders.\nv" + Assembly.GetExecutingAssembly().GetName().Version;
            string caption = "About";
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, icon);
        }

        private void ListBox_Copy()
        {
            List<string> clist = new List<string>();
            string cstring = null;
            for (int i = 0; i < lstName.SelectedItems.Count; i++)
            {
                clist.Add(lstName.SelectedItems[i].ToString());
                cstring += lstName.SelectedItems[i].ToString();
            }
            Clipboard.Clear();
            Clipboard.SetText(cstring);
            clist.Clear();
        }

        private void MenuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            ListBox_Copy();
        }
        
        private void MenuItemRename_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
