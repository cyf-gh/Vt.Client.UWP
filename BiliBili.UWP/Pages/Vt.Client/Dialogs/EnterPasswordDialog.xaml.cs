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

namespace BiliBili.UWP.Pages.Vt.Client {
    public sealed partial class EnterPasswordDialog : ContentDialog {
        private readonly String userName;
        private readonly String lobbyName;

        public EnterPasswordDialog( string userName, string lobbyName )
        {
            this.InitializeComponent();
            this.userName = userName;
            this.lobbyName = lobbyName;
            this.Title = $"用户名：{userName}\n房间名：{lobbyName}";
        }

        private async void ContentDialog_OkButtonClick( ContentDialog sender, ContentDialogButtonClickEventArgs args )
        {
            string passwd = tb_passwd.Text;
            string response = await VtCore.Handle.EnterLobby( userName, lobbyName, passwd );
            switch ( response ) {
                case "PASSWORD_INCORRECT":
                    tb_passwd.Text = "";
                    VtUtils.Messagebox.Show( "密码错误，请重试","提示" );
                    return;
                case "OK":
                    await VtCore.Handle.ExitLobby( lobbyName );
                    VtUtils.Messagebox.Show( $"已加入房间：{lobbyName}！\n并已经退出了原来的房间！（如果有的话)", "提示" );
                    Hide();
                    break;
                default:
                    VtUtils.Messagebox.Show( $"未知错误", "提示" );
                    break;
            }
        }

        private void ContentDialog_CancelButtonClick( ContentDialog sender, ContentDialogButtonClickEventArgs args )
        {
            this.Hide();
        }
    }
}
