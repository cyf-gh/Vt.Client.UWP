using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vt.Client.Corel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliBili.UWP.Pages.Vt.Client.Dialogs {
    public sealed partial class ServerSettingsDialog : ContentDialog {
        public ServerSettingsDialog()
        {
            this.InitializeComponent();
            
            var strs = VtCore.Handle.GetServerInfo();
            tb_IpOrDomain.Text = strs[0];
            tb_TcpPort.Text = strs[1];
            tb_UdpPort.Text = strs[2];
        }

        private void ContentDialog_PrimaryButtonClick( ContentDialog sender, ContentDialogButtonClickEventArgs args )
        {
        }

        private void ContentDialog_SecondaryButtonClick( ContentDialog sender, ContentDialogButtonClickEventArgs args )
        {
        }

        private async void btn_ChangeServer_Click( Object sender, RoutedEventArgs e )
        {
            var ok = await VtCore.Handle.CheckServerAvailable( tb_IpOrDomain.Text, tb_TcpPort.Text, tb_UdpPort.Text );
            VtUtils.Messagebox.Show( ok, "服务器测试结果" );
        }
    }
}
