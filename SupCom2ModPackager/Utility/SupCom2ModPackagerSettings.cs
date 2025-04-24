using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupCom2ModPackager.Extensions;

namespace SupCom2ModPackager.Utility
{
    public class SupCom2ModPackagerSettings
    {
        public string ModPath
        {
            get => this.GetSyncValue<string>();
            set => this.SetSyncValue(value);
        }

    }
}
