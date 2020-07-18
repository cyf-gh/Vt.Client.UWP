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

    public class VtClientCore {
        static public PlayerEvents PlayerEvents { get; set; }
        public VtClientCore()
        {
            // fake
            curServer = new IPPort();
        }

        public void RefreshVideoInfo( string vvpDataJson, string infoJson )
        {

        }

        private PlayerEvents GetPlayerEvents() {
            if ( PlayerEvents == null ) {
                return null;
            }
            return PlayerEvents as PlayerEvents;
        }
        private IPPort curServer;

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


