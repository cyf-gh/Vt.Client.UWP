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
using Vt.Client.Corel;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace BiliBili.UWP.Pages.Vt.Client {
    public class VideoDesc {
        public string ls;
        public int index;
        public VideoDesc()
        {
            ls = "";
            index = -1;
        }
        public VideoDesc( List<PlayerModel>ls, int index )
        {
            this.ls = JsonConvert.SerializeObject( ls );
            this.index = index;
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
            if ( !isLogin ) {
                Utils.ShowMessageToast( "检测到还未登录，将启用本机名" );
            }
            return isLogin ? await GetUserName() : VtCore.Handle.ReserveName;
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

        public static class Messagebox {
            static public async void Show( string message, string title )
            {
                var messageDialog = new MessageDialog( message, title );
                await messageDialog.ShowAsync();
            }
        }
    }
}
