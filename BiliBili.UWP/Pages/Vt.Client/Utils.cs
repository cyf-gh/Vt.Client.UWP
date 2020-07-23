using BiliBili.UWP.Api;
using BiliBili.UWP.Api.User;
using BiliBili.UWP.Helper;
using BiliBili.UWP.Modules.BiliBili.UWP.Modules.UserCenterModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vt.Client.Core;
using Vt.Client.Corel;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace BiliBili.UWP.Pages.Vt.Client {
    public class VideoDesc {
        public string ls;
        public int index;
        public string md5;
        public VideoDesc()
        {
            ls = "";
            index = -1;
        }
        public VideoDesc( List<PlayerModel>ls, int index )
        {
            this.ls = JsonConvert.SerializeObject( ls );
            this.index = index;
            md5 = VtCore.Handle.SetNewVideoMd5( this.ls, index );
        }
        public List<PlayerModel> ToLS()
        {
            return JsonConvert.DeserializeObject<List<PlayerModel>>(ls);
        }
    }
    /// <summary>
    /// Vt针对BiliUWP的增强
    /// </summary>
    public static class VtUtils {
        /// <summary>
        /// 获取vt当前使用的名字
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetVtUserName()
        {
            var isLogin = ApiHelper.IsLogin();
            if ( !isLogin && !VtCore.IsMentionedUsePCName ) {
                Utils.ShowMessageToast( "检测到还未登录，将启用本机名" );
                VtCore.IsMentionedUsePCName = true;
            }
            return isLogin ? await GetUserName() : VtCore.Handle.ReserveName;
        }
        public static async Task<LobbyStatus> IsHost() {
            try {
                return ( await VtCore.Handle.CheckStatus( await GetVtUserName() ) );
            } catch {
                return LobbyStatus.Idle;
            }
        }
        public static void SwitchSyncStatus()
        {
            VtCore.Handle.Syncing = !VtCore.Handle.Syncing;
            Utils.ShowMessageToast( VtCore.Handle.Syncing ? "开始同步" : "停止同步" );
        }
        /// <summary>
        /// 获取bilibli已登陆账户的名字
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetUserName()
        {
            UserCenterAPI userCenterAPI = new UserCenterAPI();
            var api = userCenterAPI.UserCenterDetail( ApiHelper.GetUserId() );
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

        /// <summary>
        /// 该函数应该于VT_CLIENT_REFRESH_VIDEO_INFO region中调用
        /// 这个函数有两个功能
        /// 如果是host，则发送数据
        /// 如果是guest，则读取参数并打开视频
        /// </summary>
        /// <param name="lsJson"></param>
        /// <param name="index"></param>
        public static void OpenNewVideo( string lsJson, int index )
        {
            if ( VtCore.Handle.IsHost ) {
                VtCore.Handle.SendVideoData( lsJson, index );
            } else {
                Utils.ShowMessageToast("已检测到房主播放了新视频，开始播放！");
                VtCore.Handle.SetNewVideoMd5( lsJson, index );
                MessageCenter.SendNavigateTo(
                    NavigateMode.Play, typeof( PlayerPage ),
                    new object[] { JsonConvert.DeserializeObject<List<PlayerModel>>( lsJson ), index } ); // NEED_TEST
            }
        }
        public static async Task StartSync()
        {
            string username = await GetVtUserName();
            var status = await VtCore.Handle.CheckStatus( username );
            if ( status == LobbyStatus.Idle ) {
                Messagebox.Show("你不在任何房间中，无法开始同步","错误");
                return;
            }
            if ( sync != null ) {
                sync.Stop();
            }
            sync = VtCore.Handle.CreateSyncworker( username, status == LobbyStatus.Host );
            sync.Do();
        }

        private static SyncWorker sync;

        public static class Messagebox {
            static public async void Show( string message, string title )
            {
                var messageDialog = new MessageDialog( message, title );
                await messageDialog.ShowAsync();
            }
        }

    }
}
