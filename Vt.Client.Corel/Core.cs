using System;
using System.Collections.Generic;
using System.Net;
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

    public enum VtStatus {
        Idle,
        InLobbyGuest,
        InLobbyHost
    }

    public class VtClientCore {
        public PlayerEvents PlayerEvents { get; set; }

        public bool IsHost = false;
        private VtStatus Status;
        private IPPort curServer;

        public VtClientCore()
        {
            // fake
            curServer = new IPPort();
            Status = VtStatus.Idle;
        }

        public VtStatus GetStatus()
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
        }
        /// <summary>
        /// 创建房间并发送报文
        /// </summary>
        /// <param name="name"></param>
        public void CreateLobby( string name )
        {
            Status = VtStatus.InLobbyHost;
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="lobbyName"></param>
        /// <param name="passwd"></param>
        public void EnterLobby( string lobbyName, string passwd )
        {

        }
        public void ChangeServer( string ipOrDomain, string tcp, string udp )
        {
            string ip = Regex.Match( ipOrDomain, @"^[a-zA-Z0-9]+([a-zA-Z0-9\-\.]+)?\.$" ).Success ? DNSHelper.ParseIPFromDomain( ipOrDomain ) : ipOrDomain;
            curServer = new IPPort { IP = ip, TcpPort = tcp, UdpPort = udp };
        }

        public async Task<List<string>> GetLobbies()
        {
            try {
                return await Task.Run( () => {
                    try {
                        return StringHelper.ParseComData( TcpClient_.SendMessage_ShortConnect( "query_lobbies@", curServer ) );
                    } catch {
                        return null;
                    }
                } );
            } catch ( Exception ex ) {
                stLogger.Log( ex );
                throw ex;
            }
        }

        public async Task<string> CheckServerAvailable() {
            return await Task.Run( () => {
                try {
                    UdpClient_ udpClient_ = new UdpClient_();
                    string tcpRe = TcpClient_.SendMessage_ShortConnect( "ping@", curServer );
                    string udpRe = udpClient_.SendMessage( "ping@", curServer );
                    if ( tcpRe != "OK") {
                        return "TCP端口不可用";
                    }
                    if ( udpRe != "OK" ) {
                        return "UDP端口不可用";
                    }
                    return "OK";
                } catch ( Exception ex ) {
                    return $"发生错误：{ex.Message}" ;
                }
            } );
        }
    }
}


