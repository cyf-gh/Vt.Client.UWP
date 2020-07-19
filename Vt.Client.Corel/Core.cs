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
    public static class VtCore {
        static public VtClientCore Handle = new VtClientCore();
    }

    public enum LobbyStatus {
        Idle,
        InLobbyGuest,
        InLobbyHost
    }

    public enum VideoStatus {
        AllSame,
        DifferentPart,
        AllDifferent
    }

    public class VtClientCore {
        public PlayerEvents PlayerEvents { get; set; }

        public bool IsHost = false;
        private LobbyStatus Status;
        private IPPort curServer;

        private int curP = 0;
        private string curVideoMD5 = "";
        public VtClientCore()
        {
            // fake
            curServer = new IPPort();
            Status = LobbyStatus.Idle;
        }

        public LobbyStatus GetStatus()
        {
            return Status;
        }
        /// <summary>
        /// 发送视频Host视频信息
        /// </summary>
        /// <param name="lsJson"></param>
        /// <param name="index"></param>
        public void SendVideoData( string lsJson, int index )
        {
            /// TODO:通过某种协议发送报文
            curVideoMD5 = stLib.Encrypt.MD5.GetMD5HashFromString( lsJson );
        }
        public void SetNewVideoMd5( string lsJson, int index )
        {
            curVideoMD5 = stLib.Encrypt.MD5.GetMD5HashFromString( lsJson );
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
                    break;
                case Corel.VideoStatus.AllDifferent:
                    PlayerEvents.ExitVideo();
                    PlayerEvents.OpenNewVideo( lsJson, index );
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 创建房间并发送报文
        /// </summary>
        /// <param name="name"></param>
        public async Task<string> CreateLobby( string hostName, string lobbyname, string passwd )
        {
            Status = LobbyStatus.InLobbyHost;
            using ( HttpResponseMessage response =
                await client.GetAsync( $"{ Domain()}/lobby/create?hostname={hostName}&lobbyname={lobbyname}&passwd={passwd}" ).ConfigureAwait( false )
            )
            using ( HttpContent content = response.Content ) {
                return await content.ReadAsStringAsync();
            }
        }
        HttpClient client = new HttpClient();
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
        public string Domain()
        {
            return $"http://{curServer.IP}:{curServer.TcpPort}";
        }
        public async Task<string> EnterLobby( string userName, string lobbyName, string passwd )
        {
            using ( HttpResponseMessage response =
                await client.GetAsync( $"{Domain()}/lobby/enter?username={userName}&lobbyname={lobbyName}&passwd={passwd}" ).ConfigureAwait( false )
                )
            using ( HttpContent content = response.Content ) {
                return await content.ReadAsStringAsync();
            }
        }
        public void ChangeServer( string ipOrDomain, string tcp, string udp )
        {
            string ip = Regex.Match( ipOrDomain, @"^[a-zA-Z0-9]+([a-zA-Z0-9\-\.]+)?\.$" ).Success ? DNSHelper.ParseIPFromDomain( ipOrDomain ) : ipOrDomain;
            curServer = new IPPort { IP = ip, TcpPort = tcp, UdpPort = udp };
        }

        public async Task<List<string>> GetLobbies()
        {
            using ( HttpResponseMessage response =
                await client.GetAsync( $"{Domain()}/lobbies" ).ConfigureAwait( false )
            )
            using ( HttpContent content = response.Content ) {
                return StringHelper.ParseComData( await content.ReadAsStringAsync() );
            }
        }

        public async Task<string> CheckServerAvailable()
        {
            return await Task.Run( async () => {
                try {
                    UdpClient_ udpClient_ = new UdpClient_();
                    string tcpRe = "";             
                    using ( HttpResponseMessage response =
                        await client.GetAsync( $"{Domain()}/ping" ).ConfigureAwait( false )
                    )
                    using ( HttpContent content = response.Content ) {
                        tcpRe = await content.ReadAsStringAsync();
                    }
                    string udpRe = udpClient_.SendMessage( "ping@", curServer );
                    if ( tcpRe != "OK" ) {
                        return "TCP端口不可用";
                    }
                    if ( udpRe != "OK" ) {
                        return "UDP端口不可用";
                    }
                    return "OK";
                } catch ( Exception ex ) {
                    return $"发生错误：{ex.Message}";
                }
            } );
        }
    }
}


