using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vt.Client.Core {
    public class PlayerEvents {
        public delegate string _GetCurrentPlayTimeLocation();
        public delegate bool _IsPause();
        public delegate void _Play();
        public delegate void _Pause();
        public delegate void _NextP();
        public delegate void _PrevP();
        public delegate void _ExitPlayer();
        public delegate void _SelectP( int index );
        public delegate void _JumpToCurrentLocation(string recv);
        /// <summary>
        /// 当有新视频时打开新视频
        /// </summary>
        public delegate void _OpenNewVideo( string lsJson, int index );
        /// <summary>
        /// 返回当前播放时间戳
        /// </summary>
        public _Play Play { get; set; }
        public _Pause Pause { get; set; }
        public _JumpToCurrentLocation JumpToCurrentLocation { get; set; }
        public _OpenNewVideo OpenNewVideo { get; set; }
        public _NextP NextP { get; set; }
        public _PrevP PrevP { get; set; }
        public _ExitPlayer ExitVideo { get; set; }
        public _SelectP SelectP { get; set; }
    }
}
