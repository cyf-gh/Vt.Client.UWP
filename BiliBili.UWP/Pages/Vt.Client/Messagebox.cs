using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace BiliBili.UWP.Pages.Vt.Client {
    public static class Messagebox {
        static public async void Show( string message )
        {
            var messageDialog = new MessageDialog( message );
            await messageDialog.ShowAsync();
        }
    }
}
