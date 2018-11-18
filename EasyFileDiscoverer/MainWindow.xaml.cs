using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace EasyFileDiscoverer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window

    {
        //A collection for holding list of all the files from fetched folders in Datagrid
        public static ObservableCollection<DataGridBox> gridFiles = new ObservableCollection<DataGridBox>();
        static string downloadFolderPath = Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), "Downloads");

        public MainWindow()
        {
            InitializeComponent();
        }

        // method for toggle preview panel between fixed and loose states
        private void Btn_pin_Click(object sender, RoutedEventArgs e)
        {
            if (Btn_preview.Visibility == Visibility.Collapsed)
            {
                Grid_layer1.Visibility = Visibility.Collapsed;
                Btn_preview.Visibility = Visibility.Visible;
                Img_panelpin.Source = new BitmapImage(new Uri("Images/pin_panel.jpg", UriKind.Relative));
            }
            else
            {
                Btn_preview.Visibility = Visibility.Collapsed;
                Img_panelpin.Source = new BitmapImage(new Uri("Images/unpin_panel.jpg", UriKind.Relative));
            }
        }

        //change color on mouse hover over preview button
        private void Btn_preview_MouseEnter(object sender, MouseEventArgs e)
        {           
            this.Resources["DynamicColor"] = new SolidColorBrush(Colors.Green);
            Grid_layer1.Visibility = Visibility.Visible;
            //Img_preview.MinWidth = 150;

        }

        // hide undocked pane when the mouse enters Layer 0
        private void Grid_layer0_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Btn_preview.Visibility == Visibility.Visible)
                Grid_layer1.Visibility = Visibility.Collapsed;
            this.Resources["DynamicColor"] = new SolidColorBrush(Colors.Black);
        }

        private void Btn_browse_Click(object sender, RoutedEventArgs e)
        {
            
            var dialog = new CommonOpenFileDialog

            {
                IsFolderPicker = true
            };

            //dialog.InitialDirectory = @"C:\Program Files";
            dialog.InitialDirectory = downloadFolderPath;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // to clear old collection and start new
                gridFiles.Clear();
                Grid_metadata.DataContext = null;
                OpenFolder(dialog.FileName, "*.*"); /*Pass all files extension*/
               // set the filter back to all to fetch all files
                Rb_all.IsChecked = true;
            }
           
        }

        // Open and fetch files in folders and its sub folders
        private void OpenFolder(string fileName, string filters)
        {
 
            Tbx_path.Text = fileName;
            if (Tbx_path.Text == "")
                return;

            string[] files = filters.Split('|').SelectMany(filter => Directory.GetFiles(Tbx_path.Text, filter, SearchOption.AllDirectories)).ToArray();
            foreach (var filePath in files)
            {
                var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();
                DataGridBox file_items = new DataGridBox() { fileitemIcon = bmpSrc, fileitemText = Path.GetFileName(filePath), fileitemPath = Path.GetFullPath(filePath) };
                gridFiles.Add(file_items);
            }

            Dg_files.ItemsSource = gridFiles;
            Dg_files.SelectedIndex = 0;
            Lbl_count.Content = gridFiles.Count;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //string userRoot = System.Environment.GetEnvironmentVariable("USERPROFILE");   
            OpenFolder(downloadFolderPath, "*.*");
        }

        private void Btn_small_Click(object sender, RoutedEventArgs e)
        {
            Dg_files.Columns[0].Width = 1;
        }

        private void Btn_med_Click(object sender, RoutedEventArgs e)
        {
            Dg_files.Columns[0].Width = 30;
        }

        private void Btn_large_Click(object sender, RoutedEventArgs e)
        {
            Dg_files.Columns[0].Width = 70;
        }

        // set metadata by creating a new object and setting grid's datacontext for different file extensions
        private void Dg_files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Img_preview.Source = null;
            Media_element.Source = null;

            try
            {
                DataGridBox dataGridFile = Dg_files.SelectedItem as DataGridBox;      
                string pathfile = dataGridFile.fileitemPath;
                string filterExtension;
                FileInfo fileInfo = new FileInfo(pathfile);
                filterExtension = Path.GetExtension(pathfile).ToLower();
                
                var metaData = new MetaData() { fileCreation = fileInfo.CreationTime, fileModified = fileInfo.LastWriteTime, fileName = Path.GetFileNameWithoutExtension(pathfile), fileLoc = fileInfo.FullName, fileLength = string.Concat((fileInfo.Length/1024).ToString(), " KB"), fileType = filterExtension };
                Grid_metadata.DataContext = metaData;

                if (filterExtension == ".png" || filterExtension == ".jpg" || filterExtension == ".jpeg" || filterExtension == ".ico" || filterExtension == ".bmp" || filterExtension == ".jpe" || filterExtension == ".jfif")
                {
                    if (Img_preview.Visibility == Visibility.Collapsed)
                    {
                        Img_preview.Visibility = Visibility.Visible;
                        Tblk_preview.Visibility = Visibility.Collapsed;
                        Media_preview.Visibility = Visibility.Collapsed;
                    }
                    ImageSource imageSource = new BitmapImage(new Uri(pathfile));
                    Img_preview.Source = imageSource;
                    Media_element.Stop();
                }
                else if (filterExtension == ".wmv" || filterExtension == ".mp4" || filterExtension == ".avi" || filterExtension == ".mp3" || filterExtension == ".3gp" || filterExtension == ".flv" || filterExtension == ".mkv")
                {
                    if (Media_preview.Visibility == Visibility.Collapsed)
                    {
                        Img_preview.Visibility = Visibility.Collapsed;
                        Tblk_preview.Visibility = Visibility.Collapsed;
                        Media_preview.Visibility = Visibility.Visible;
                    }
                    Btn_play.Visibility = Visibility.Visible;
                    Btn_pause.Visibility = Visibility.Collapsed;
                    Media_element.ScrubbingEnabled = true;
                    Media_element.Source = new Uri(pathfile);
                    Media_element.Position = TimeSpan.FromSeconds(0);
                    Media_element.Pause();

                }
                else
                {
                    Media_element.Stop();
                    Img_preview.Visibility = Visibility.Collapsed;
                    Tblk_preview.Visibility = Visibility.Visible;                    
                    Media_preview.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception x)
            {
                Img_preview.Visibility = Visibility.Collapsed;
                Tblk_preview.Visibility = Visibility.Visible;
                Media_preview.Visibility = Visibility.Collapsed;
            }

        }

        //fetch all the files
        private void Rb_all_Checked(object sender, RoutedEventArgs e)
        {
            gridFiles.Clear();// to clear old collection
            OpenFolder(Tbx_path.Text, "*.*");
        }

        //fetch all the image files
        private void Rb_image_Checked(object sender, RoutedEventArgs e)
        {
             var filteredFiles = (from d in gridFiles where (d.fileitemText.ToLower().EndsWith(".png") || d.fileitemText.ToLower().EndsWith(".jpg") || d.fileitemText.ToLower().EndsWith(".jpeg") || d.fileitemText.ToLower().EndsWith(".bmp") || d.fileitemText.ToLower().EndsWith(".ico") || d.fileitemText.ToLower().EndsWith(".jpe") || d.fileitemText.ToLower().EndsWith(".jfif")) select d).ToList(); 
            Dg_files.ItemsSource = filteredFiles;
            Dg_files.SelectedIndex = 0;
            Lbl_count.Content = filteredFiles.Count;
            if (filteredFiles.Count == 0)
            {
                Grid_metadata.DataContext = null;
            }
        }

        //fetch all the pdf files
        private void Rb_pdf_Checked(object sender, RoutedEventArgs e)
        {
            
            var filteredFiles = (from d in gridFiles where (d.fileitemText.ToLower().EndsWith(".pdf")) select d).ToList();
            Dg_files.ItemsSource = filteredFiles;
            Dg_files.SelectedIndex = 0;
            Lbl_count.Content = filteredFiles.Count;
            if (filteredFiles.Count == 0)
            {
                Grid_metadata.DataContext = null;
            }
            MessageBox.Show("Prototype does not support Preview of PDF Files");
        }

        //fetch all the office files
        private void Rb_office_Checked(object sender, RoutedEventArgs e)
        {
            
            var filteredFiles = (from d in gridFiles where (d.fileitemText.ToLower().EndsWith(".xls") || d.fileitemText.ToLower().EndsWith(".xlsx") || d.fileitemText.ToLower().EndsWith(".ppt") || d.fileitemText.ToLower().EndsWith(".pptx") || d.fileitemText.ToLower().EndsWith(".doc") || d.fileitemText.ToLower().EndsWith(".docx")) select d).ToList();
            Dg_files.ItemsSource = filteredFiles;
            Dg_files.SelectedIndex = 0;
            Lbl_count.Content = filteredFiles.Count;
            if (filteredFiles.Count == 0)
            {
                Grid_metadata.DataContext = null;
            }
            MessageBox.Show("Prototype does not support Preview of Office Files");
        }

        private void Btn_play_Click(object sender, RoutedEventArgs e)
        {
            Media_element.Play();
            Btn_play.Visibility = Visibility.Collapsed;
            Btn_pause.Visibility = Visibility.Visible;
        }

        private void Btn_stop_Click(object sender, RoutedEventArgs e)
        {
            Media_element.Stop();
            Btn_play.Visibility = Visibility.Visible;
            Btn_pause.Visibility = Visibility.Collapsed;
        }

        //fetch all the video files
        private void Rb_video_Checked(object sender, RoutedEventArgs e)
        {   
            var filteredFiles = (from d in gridFiles where (d.fileitemText.ToLower().EndsWith(".wmv") || d.fileitemText.ToLower().EndsWith(".mp4") || d.fileitemText.ToLower().EndsWith(".avi") || d.fileitemText.ToLower().EndsWith(".mp3") || d.fileitemText.ToLower().EndsWith(".3gp") || d.fileitemText.ToLower().EndsWith(".flv") || d.fileitemText.ToLower().EndsWith(".mkv")) select d).ToList();
            Dg_files.ItemsSource = filteredFiles;
            Dg_files.SelectedIndex = 0;
            Lbl_count.Content = filteredFiles.Count;
            if (filteredFiles.Count == 0)
            {
                Grid_metadata.DataContext = null;
                
            }
        }

        //fetch all the text files
        private void Rb_txt_Checked(object sender, RoutedEventArgs e)
        {     
            var filteredFiles = (from d in gridFiles where (d.fileitemText.ToLower().EndsWith(".txt")) select d).ToList();
            Dg_files.ItemsSource = filteredFiles;
            Dg_files.SelectedIndex = 0;
            Lbl_count.Content = filteredFiles.Count;
            MessageBox.Show("Prototype does not support Preview of Text Files");
            if (filteredFiles.Count == 0)
            {
                Grid_metadata.DataContext = null;
            }
        }

        private void Btn_pause_Click(object sender, RoutedEventArgs e)
        {
            Media_element.Pause();
            Btn_play.Visibility = Visibility.Visible;
            Btn_pause.Visibility = Visibility.Collapsed;
        }
    }
}
