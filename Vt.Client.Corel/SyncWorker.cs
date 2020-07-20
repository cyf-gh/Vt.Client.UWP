using stLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using stLib.Net.Haste;
using stLib.Log;
using stLib.Net;
using YPM.Packager;

namespace Vt.Client.Core {
    public class SyncWorker {
        /// <summary>
        /// 初始化Sync
        /// </summary>
        /// <param name="lobbyName"></param>
        /// <param name="nickName"></param>
        /// <param name="ipport"></param>
        /// <param name="playerEvents">需要重写的函数</param>
        public SyncWorker( string lobbyName, string nickName, IPPort ipport, PlayerEvents playerEvents )
        {
            this.lobbyName = lobbyName;
            this.nickName = nickName;
            this.synccer = new UdpClient_();
            this.ipport = ipport;
            this.playerEvents = playerEvents;
        }

        UdpProtocolMaker protocolMaker = new UdpProtocolMaker();

        private readonly IPPort ipport;
        private bool stopFlag = false;
        private readonly UdpClient_ synccer;
        private readonly String lobbyName;
        private readonly String nickName;
        private readonly PlayerEvents playerEvents;

        /// <summary>
        /// 同时处理房主与客人的事件，因为房主永远会收到OK
        /// Vt.Client主循环
        /// </summary>
        private void sendLocationPermantly()
        {
            while ( !stopFlag ) {
                try {
                    Console.WriteLine( playerEvents.GetCurrentPlayTimeLocation() );
                    Thread.Sleep( 900 ); // 发送间隔
                    var recv = synccer.SendMessage(
                        protocolMaker.MakePackageMsg(
                            lobbyName,
                            nickName,
                            playerEvents.GetCurrentPlayTimeLocation(),
                            playerEvents.IsPause() )
                        , ipport );
                    Console.WriteLine( recv );
                    switch ( recv ) {
                        case "OK":  // 房主永远收到OK
                            continue;
                        case "p":
                            playerEvents.Pause();
                            continue;
                        case "s":
                            playerEvents.Play();
                            continue;
                        default:
                            if ( recv.Contains( ":" ) ) {
                                playerEvents.JumpToCurrentLocation(recv);
                            }
                            continue;
                    }
                } catch ( Exception ex ) {
                    stLogger.Log( "In sendLocationPermantly", ex );
                    continue;
                }
            }
        }
        public Task SyncHandle { get; set; } = null;
        public void Do()
        {
            TaskFactory taskFactory = new TaskFactory();
            stopFlag = false;
            SyncHandle = taskFactory.StartNew( sendLocationPermantly );
        }

        public void Resume()
        {

        }

        public void Pause()
        {

        }

        public void Stop()
        {
            if ( SyncHandle != null ) {
                stopFlag = true;
            }
        }
    }

    public class LobbyBorrower {
        private readonly IPPort ipport;
        public LobbyBorrower( IPPort ipport, string lobbyName, string lobbyPassword )
        {
            this.ipport = ipport;
            LobbyName = lobbyName;
            LobbyPassword = lobbyPassword;
            LobbyName = lobbyName;
            udpClient = new UdpClient_();
        }

        public String LobbyName { get; }

        private UdpClient_ udpClient;

        public String LobbyPassword { get; }

        public string Lend( string msg )
        {
            try {
                return TcpClient_.SendMessage_ShortConnect( msg, ipport );
            } catch ( Exception ex ) {
                stLogger.Log( ex );
                throw;
            }
        }
        public List<string> QueryViewers()
        {
            try {
                return StringHelper.ParseComData( udpClient.SendMessage( "get_lobby_viewers@" + LobbyName, ipport ) );
            } catch ( Exception ex ) {
                stLogger.Log( ex );
                throw;
            }
        }
    }
}
