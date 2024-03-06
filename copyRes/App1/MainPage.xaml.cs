using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //string copyFolderPath;
        //String targetFolderPath;
        StorageFolder chooseFolder;
        StorageFolder targetFolder;
        bool isCopySucces = false;
        public MainPage()
        {
            this.InitializeComponent();
            Console.WriteLine("test");
        }

        private void btnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            Task openFolderTask = openFolderAsync(0);
            if (openFolderTask.IsCompleted)
            {

            }
        }

        private async Task openFolderAsync(int type) {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderTokenType"+type, folder);
                if (type == 0) {
                    this.chooseFolderText.Text = "copy path:" + folder.Path;
                    //this.copyFolderPath = folder.Path;
                    chooseFolder = folder;
 
                }
                else if (type == 1)
                {
                    this.targetFolderText.Text = "target path:" + folder.Path;
                    //this.targetFolderPath = folder.Path;
                    targetFolder = folder;
                }
                
            }
            else
            {
                if (type == 0)
                {
                    this.chooseFolderText.Text = "Operation cancelled.";
                }
                else {
                    this.targetFolderText.Text = "Operation cancelled.";
                }
            }
        }

        private void btnTargetFolder_Click(object sender, RoutedEventArgs e)
        {
            Task openFolderTask = openFolderAsync(1);
            if (openFolderTask.IsCompleted)
            {

            }
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e) {
            this.isCopySucces = false;
            Task task = getFolderItemsInfoAction();
            if (task.IsCompleted)
            {

            }
        }

        public async Task CopyFileAsync(StorageFile sourceFile, StorageFolder destinationFolder, string newName, bool needParentFolder = true)
        {
            StorageFolder tempDestinationFolder = destinationFolder;
            try
            {
                if (needParentFolder)
                {
                    var appFolder = await destinationFolder.CreateFolderAsync(chooseFolder.Name, options: CreationCollisionOption.OpenIfExists);
                    tempDestinationFolder = appFolder;
                }
                await sourceFile.CopyAsync(tempDestinationFolder, newName, NameCollisionOption.GenerateUniqueName);
                Console.WriteLine("File copied successfully.");
                isCopySucces = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to copy file: {ex.Message}");
            }
        }

        private async void DisplayNoWifiDialog(String title ,String info)
        {
            ContentDialog noWifiDialog = new ContentDialog()
            {
                XamlRoot = this.XamlRoot,
                Title = title,
                Content = info,
                CloseButtonText = "Ok"
            };

            await noWifiDialog.ShowAsync();
        }

        private async Task getFolderItemsInfoAction() {
            if (this.chooseFolder != null && this.targetFolder != null)
            {
                StringBuilder outputText = new StringBuilder();
                IReadOnlyList<IStorageItem> itemsList = await chooseFolder.GetItemsAsync();
                foreach (var item in itemsList)
                {
                    // foler
                    if (item is StorageFolder)
                    {
                        outputText.Append(item.Name + " folder\n");
                    }
                    // file
                    else
                    {
                        showProgressText.Text = "开始复制："+ item.Name;
                        await CopyFileAsync(sourceFile: item as StorageFile, this.targetFolder, item.Name, needParentFolder: false);
                        outputText.Append(item.Name + "file \n");
                    }
                }
                
                DisplayNoWifiDialog(title: "成功", info: isCopySucces ? "复制成功" : "操作完成");
                Debug.WriteLine(outputText.ToString());
            }
            else {
                DisplayNoWifiDialog(title: "失败",info: "未找到有效源文件和目标文件夹");
                Debug.WriteLine("未找到有效文件夹");
            }

        }
    }
}
