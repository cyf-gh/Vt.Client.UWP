using BiliBili.UWP.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vt.Client.Corel;

namespace BiliBili.UWP.Pages.Vt.Client {
// 定位
// MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new object[] { ls, (gv_Play.ItemsSource as List<pagesModel>).IndexOf( info) });
    public static class StaticEventDef {
        /// <summary>
        /// 该函数应该于VT_CLIENT_REFRESH_VIDEO_INFO region中调用
        /// 这个函数有两个功能
        /// 如果是host，则发送数据
        /// 如果是guest，则读取参数并打开视频
        /// </summary>
        /// <param name="lsJson"></param>
        /// <param name="index"></param>
        public static void OpenNewVideo( string lsJson, int index ) {
            if ( VtCore.Handle.IsHost ) {
                VtCore.Handle.SendVideoData( lsJson, index );
            } else {
                MessageCenter.SendNavigateTo( 
                    NavigateMode.Play, typeof( PlayerPage ), 
                    new object[] { JsonConvert.DeserializeObject<List<PlayerModel>>( lsJson ), index } ); // NEED_TEST
            }
        }
    }
}
