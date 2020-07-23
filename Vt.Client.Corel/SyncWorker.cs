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
using Vt.Client.Corel;

namespace Vt.Client.Core {
    public class SyncWorker {
        private readonly String username;
        private readonly Boolean ishost;
        private readonly IPPort ipport;
        private readonly String videoMd5;
        TaskFactory task = new TaskFactory();
        public Task SyncTaskHandle { get; set; } = null;
        public SyncWorker( string username, IPPort ipport, string videoMd5,bool ishost = false )
        {
            this.username = username;
            this.ishost = ishost;
            this.ipport = ipport;
            this.videoMd5 = videoMd5;
        }
        private bool stopFlag = false;

        public void Do()
        {
            if ( ishost ) {
                task.StartNew( syncHost );
            } else {
                task.StartNew( syncGuest );
            }
        }

        public void Stop()
        {
            stopFlag = true;
        }

        /// <summary>
        /// 房主同步包发送
        /// </summary>
        private async void syncHost()
        {
            while ( !stopFlag ) {
                try {
                    Thread.Sleep( 900 );
                    if ( VtCore.Handle.Syncing ) {
                        await VtCore.Handle.SendSyncHost(username);
                    }
                } catch ( Exception ex ) {
                    stLogger.Log( "In Sync Host", ex );
                    continue;
                }
            }
        }

        private async void syncGuest()
        {
            while ( !stopFlag ) {
                try {
                    Thread.Sleep( 900 );
                    if ( VtCore.Handle.Syncing ) {
                        await VtCore.Handle.SendSyncGuest( username );
                    }
                } catch ( Exception ex ) {
                    stLogger.Log( "In Sync Host", ex );
                    continue;
                }
            }
        }
    }
}
