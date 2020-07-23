using BiliBili.UWP.Api.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Vt.Client.Corel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BiliBili.UWP.Modules.BiliBili.UWP.Modules.UserCenterModels;
using BiliBili.UWP.Api;
using BiliBili.UWP.Pages.Vt.Client.Dialogs;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliBili.UWP.Pages.Vt.Client {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VtHomePage : Page {
        public VtHomePage()
        {
            this.InitializeComponent();
        }

        private async void RefreshStatus( string name )
        {
            try {
                txt_status.Text = await VtCore.Handle.WhereAmI( name );
                // 如果在房间中，则允许开始同步
                btn_startsync.IsEnabled = ( txt_status.Text != VtClientCore.NotInLobbyCn );
                // 如果已退出房间，则一定为未开始同步状态
                if ( !btn_startsync.IsEnabled ) {
                    VtCore.Handle.Syncing = false;
                }
                btn_startsync.Content = VtCore.Handle.Syncing ? "停止同步" : "开始同步";
            } catch ( Exception ex ) {
                Utils.ShowMessageToast( ex.Message );
            }
            LoadLobbies();
        }

        private async void LoadLobbies()
        {
            try {
                list_Lobbies.Items.Clear();
                var lobs = await VtCore.Handle.QueryLobbies();
                if ( lobs == null || lobs.Count == 0 ) {
                    Utils.ShowMessageToast( "当前服务器中没有房间." );
                } else {
                    foreach ( var l in lobs ) {
                        list_Lobbies.Items.Add( l );
                    }
                }
            } catch ( Exception ex ) {
                VtUtils.Messagebox.Show( ex.Message, "错误" );
            }
        }


        private async void btn_CreateLobby_Click( Object sender, RoutedEventArgs e )
        {
            var name = await VtUtils.GetVtUserName();
            // TODO: 创建房间
            string lobbyName = $"{name}'s lobby";
            string passwd = VtCore.Handle.GetRandomPasswd();
            await VtCore.Handle.CreateLobby( name, lobbyName, passwd );
            RefreshStatus( name );
        }

        private async void list_Lobbies_ItemClick( Object sender, ItemClickEventArgs e )
        {
            if ( btn_startsync.IsEnabled ) {
                Utils.ShowMessageToast( "你已在某个房间中，请先退出当前房间！" );
                return;
            }
            EnterPasswordDialog enterPasswordDialog = new EnterPasswordDialog( await VtUtils.GetVtUserName(), e.ClickedItem as string );
            await enterPasswordDialog.ShowAsync();
        }

        private void btn_RefreshLobbies_Click( Object sender, RoutedEventArgs e )
        {
            LoadLobbies();
        }

        private async void btn_Settings_Click( Object sender, RoutedEventArgs e )
        {
            ServerSettingsDialog serverSettings = new ServerSettingsDialog();
            switch ( await serverSettings.ShowAsync() ) {
                case ContentDialogResult.None:
                    break;
                case ContentDialogResult.Primary:
                    LoadLobbies();
                    break;
                case ContentDialogResult.Secondary:
                    break;
                default:
                    break;
            }
        }

        private async void btn_ExitLobby_Click( Object sender, RoutedEventArgs e )
        {
            switch ( await VtCore.Handle.ExitLobby( await VtUtils.GetVtUserName() ) ) {
                case "LOBBY_DELETED":
                    VtUtils.Messagebox.Show("您是房主，房间已被解散。","退出房间提示");
                    break;
                case "LOBBY_EXIT":
                    VtUtils.Messagebox.Show( "已退出房间", "退出房间提示" );
                    break;
                case "NO_SUCH_LOBBY":
                    VtUtils.Messagebox.Show( "您不在任何的房间中", "退出房间提示" );
                    break;
            }
        }

        private async void Page_Loading( FrameworkElement sender, Object args )
        {
            list_Lobbies.IsItemClickEnabled = true;
            RefreshStatus( await VtUtils.GetVtUserName() );
        }

        private async void Page_GotFocus( Object sender, RoutedEventArgs e )
        {
            RefreshStatus( await VtUtils.GetVtUserName() );
        }

        private async void btn_startsync_Click( Object sender, RoutedEventArgs e )
        {
            VtUtils.SwitchSyncStatus();
            RefreshStatus( await VtUtils.GetVtUserName() );
            await VtUtils.StartSync();
        }
    }
}
