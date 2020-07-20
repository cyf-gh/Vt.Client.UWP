using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using stLib.Common;
using stLib.Log;
using stLib.Net;
using stLib.Net.Haste;
using Vt.Client.Core;

namespace Vt.Client.Corel {
    /// <summary>
    /// 静态，使用这个
    /// </summary>
    public static class VtCore {
        static public VtClientCore Handle = new VtClientCore();
    }

    public enum LobbyStatus {
        Idle,
        Guest,
        Host
    }

    public enum VideoStatus {
        AllSame,
        DifferentPart,
        AllDifferent
    }

    /// <summary>
    /// 负责收发报文的操作。
    /// </summary>
    public class VtClientCore {
        public PlayerEvents PlayerEvents { get; set; } = new PlayerEvents();
        public bool IsHost = false;
        public IPPort CurrentServer { get; set; }

        private int curP = 0;
        private string curVideoMD5 = "";
        public readonly string ReserveName;
        public VtClientCore()
        {
            ReserveName = Environment.MachineName;
            CurrentServer = new IPPort{ 
                IP = "127.0.0.1",
                TcpPort = "2334",
                UdpPort = "2333"
            };
        }
        public string[] GetServerInfo()
        {
            return new string[] {
                CurrentServer.IP,
                CurrentServer.TcpPort,
                CurrentServer.UdpPort
            };
        }



        /// <summary>
        /// 八位数字
        /// </summary>
        /// <returns></returns>
        private string getRandomNumberString()
        {
            return stLib.Common.Random.Handle.GetInt32( 10000000, 99999999 ).ToString();
        }

        public string GetRandomPasswd()
        {
            return stLib.Common.Random.Handle.GetInt32( 1000, 9999 ).ToString();
        }
        /// <summary>
        /// 发送视频Host视频信息
        /// </summary>
        /// <param name="lsJson"></param>
        /// <param name="index"></param>
        public void SendVideoData( string lsJson, int index )
        {
            /// TODO:通过某种协议发送报文
            curVideoMD5 = stLib.Encrypt.MD5.GetMD5HashFromString( lsJson + index.ToString() );
        }
        public void SetNewVideoMd5( string lsJson, int index )
        {
            curVideoMD5 = stLib.Encrypt.MD5.GetMD5HashFromString( lsJson + index.ToString() );
            curP = index;
        }
        /// <summary>
        /// 只用于Guest检查当前视频是否与Host相同
        /// </summary>
        /// <param name="lsJson"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public VideoStatus VideoStatus( string lsJson, int index )
        {
            if ( curVideoMD5 == stLib.Encrypt.MD5.GetMD5HashFromString( lsJson ) && index == curP ) {
                return Corel.VideoStatus.AllSame;
            } else if ( curVideoMD5 == stLib.Encrypt.MD5.GetMD5HashFromString( lsJson ) && index != curP ) {
                return Corel.VideoStatus.DifferentPart;
            } else {
                return Corel.VideoStatus.AllDifferent;
            }
        }
        /// <summary>
        /// Guest only
        /// 同步视频
        /// 当收到服务器端的需要修改视频包时，进行调用
        /// </summary>
        /// <param name="lsJson"></param>
        /// <param name="index"></param>
        public void SyncSameVideo( string lsJson, int index )
        {
            switch ( VideoStatus( lsJson, index ) ) {
                case Corel.VideoStatus.AllSame:
                    break;
                case Corel.VideoStatus.DifferentPart:
                    PlayerEvents.SelectP( index );
                    SetNewVideoMd5( lsJson, index );
                    break;
                case Corel.VideoStatus.AllDifferent:
                    PlayerEvents.ExitVideo();
                    PlayerEvents.OpenNewVideo( lsJson, index );
                    SetNewVideoMd5( lsJson, index );
                    break;
                default:
                    break;
            }
        }

        public async Task<string> WhereAmI( string username )
        {
            string showText = "";
            var res = await httpClient.GetAsync( $"{Domain()}/user/where?username={username}" );
            if ( res.ResponseString == "IDLE" ) {
                return "你不在任何房间中";
            } else {
                var strs = res.ResponseString.Split(',');
                showText += $"房间名：{strs[0]}\n";
                showText += $"密码：{strs[2]}\n";
                showText += strs[1] == "HOST" ? "你是房主" : "你是观众";
            }
            return showText;
        }

        _HttpClient httpClient = new _HttpClient();
        private bool IsOk( string res ) { return res == "OK"; }

        public async void SendNewVideoInfo(string hostname, object videoDesc)
        {
            var res = await httpClient.PostAsync( $"{Domain()}/lobby/update/videodesc?hostname={hostname}", videoDesc );
            if ( res.IsOk() ) {
                // do something
            }
        }
        public string Domain()
        {
            return $"http://{CurrentServer.IP}:{CurrentServer.TcpPort}";
        }

        public string Domain(IPPort info)
        {
            return $"http://{info.IP}:{info.TcpPort}";
        }
        public async Task<LobbyStatus> CheckStatus( string userName )
        {
            var res = await httpClient.GetAsync( $"{Domain()}/user/status?username={userName}" );
            switch ( res.ResponseString ) {
                case "HOST":
                    return LobbyStatus.Host;
                case "GUEST":
                    return LobbyStatus.Guest;
                case "IDLE":
                    return LobbyStatus.Idle;
                default:
                    return LobbyStatus.Idle;
            }
        }
        public async Task<string> ExitLobby( string userName )
        {
            // TODO:需要关闭UDP同步
            var res = await httpClient.GetAsync( $"{Domain()}/lobby/exit?username={userName}" );
            return res.ResponseString;
        }
        public async Task<string> CreateLobby( string hostName, string lobbyname, string passwd )
        {

            var res = await httpClient.GetAsync( $"{Domain()}/lobby/create?hostname={hostName}&lobbyname={lobbyname}&passwd={passwd}" );
            return res.ResponseString;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="lobbyName"></param>
        /// <param name="passwd"></param>
        /// <return>
        /// "OK"
        /// "PASSWORD_INCORRENT"
        /// "NO_SUCH_LOBBY"
        /// </return>
        /// 
        public async Task<string> EnterLobby( string userName, string lobbyName, string passwd )
        {
            var res = await httpClient.GetAsync( $"{Domain()}/lobby/enter?username={userName}&lobbyname={lobbyName}&passwd={passwd}" );
            return res.ResponseString;
        }
        public async Task<List<string>> QueryLobbies()
        {
            var res = await httpClient.GetAsync( $"{Domain()}/lobbies" );
            return StringHelper.ParseComData( res.ResponseString );
        }
        #region SERVER
        public void ChangeServer( string ipOrDomain, string tcp, string udp )
        {
            string ip = Regex.Match( ipOrDomain, @"^[a-zA-Z0-9]+([a-zA-Z0-9\-\.]+)?\.$" ).Success ? DNSHelper.ParseIPFromDomain( ipOrDomain ) : ipOrDomain;
            CurrentServer = new IPPort { IP = ip, TcpPort = tcp, UdpPort = udp };
        }
        /// <summary>
        /// 如果可用则会自动更改
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="tcpp"></param>
        /// <param name="udpp"></param>
        /// <returns></returns>
        public async Task<string> CheckServerAvailable(string ip, string tcpp, string udpp)
        {
            var info = new IPPort { IP = ip, TcpPort = tcpp, UdpPort = udpp };
            return await Task.Run( async () => {
                try {
                    UdpClient_ udpClient_ = new UdpClient_();
                    var res = await httpClient.GetAsync( $"{Domain(info)}/ping" );
                    string tcpRe = res.ResponseString;
                    string udpRe = udpClient_.SendMessage( "ping@", info );
                    if ( tcpRe != "OK" ) {
                        return "TCP端口不可用";
                    }
                    if ( udpRe != "OK" ) {
                        return "UDP端口不可用";
                    }
                    CurrentServer = info;
                    return "OK";
                } catch ( Exception ex ) {
                    return $"发生错误：{ex.Message}";
                }
            } );
        }
        #endregion
    }
}


