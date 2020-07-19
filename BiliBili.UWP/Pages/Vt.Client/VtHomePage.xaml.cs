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

        private void Page_Loading( FrameworkElement sender, Object args )
        {
            VtCore.Handle.PlayerEvents.OpenNewVideo = StaticEventDef.OpenNewVideo;
            VtCore.Handle.ChangeServer( tb_IpOrDomain.Text, tb_TcpPort.Text, tb_UdpPort.Text );
            LoadLobbies();
        }

        private async void LoadLobbies()
        {
            try {
                list_Lobbies.Items.Clear();
                var lobs = await VtCore.Handle.GetLobbies();
                if ( lobs == null || lobs.Count == 0 ) {
                    Utils.ShowMessageToast( "当前服务器中没有房间." );
                } else {
                    foreach ( var l in lobs ) {
                        list_Lobbies.Items.Add( l );
                    }
                }
            } catch ( Exception ex ) {
                Messagebox.Show( ex.Message );
            }
        }

        private async void btn_ChangeServer_Click( Object sender, RoutedEventArgs e )
        {
            VtCore.Handle.ChangeServer( tb_IpOrDomain.Text, tb_TcpPort.Text, tb_UdpPort.Text );
            var ok = await VtCore.Handle.CheckServerAvailable();
            Messagebox.Show( ok );
            if ( ok == "OK" ) {  LoadLobbies(); }
        }

        private async void btn_CreateLobby_Click( Object sender, RoutedEventArgs e )
        {
            string name = "";
            if ( !ApiHelper.IsLogin() ) {
                Utils.ShowMessageToast( "检测到还未登录，将启用随机名" );
                name = GuidHelper.CreateNewGuid().ToString();
            } else {
                name = await getUserName( ApiHelper.GetUserId() );
            }
            // TODO: 创建房间
            string lobbyName = $"{name}'s lobby";
            VtCore.Handle.CreateLobby( lobbyName );
            txt_status.Text = lobbyName;
        }

        private async Task<string> getUserName( string mid )
        {
            UserCenterAPI userCenterAPI = new UserCenterAPI();
            var api = userCenterAPI.UserCenterDetail( mid );
            var results = await api.Request();
            UserCenterDetailModel UserCenterDetail;

            if ( results.status ) {
                var data = await results.GetData<UserCenterDetailModel>();
                if ( data.success ) {
                    UserCenterDetail = data.data;
                    return UserCenterDetail.card.name;
                } else {
                    Utils.ShowMessageToast( data.message );
                }
            } else {
                Utils.ShowMessageToast( results.message );
            }
            return "";
        }

        private void list_Lobbies_ItemClick( Object sender, ItemClickEventArgs e )
        {

        }
    }
}
