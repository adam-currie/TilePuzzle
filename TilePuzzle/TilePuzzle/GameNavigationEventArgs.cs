using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;

namespace TilePuzzle {
    public class GameNavigationEventArgs{
        public StorageFile file = null;
        public bool continuing = false;

        public GameNavigationEventArgs(StorageFile file = null, bool continuing = false) {
            this.file = file;
            this.continuing = continuing;
        }
    }
}
