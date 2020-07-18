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
        public delegate void _JumpToCurrentLocation(string recv);
        /// <summary>
        /// 返回当前播放时间戳
        /// </summary>
        public _GetCurrentPlayTimeLocation GetCurrentPlayTimeLocation { get; set; }
        public _IsPause IsPause { get; set; }
        public _Play Play { get; set; }
        public _Pause Pause { get; set; }
        public _JumpToCurrentLocation JumpToCurrentLocation { get; set; }
    }
}
