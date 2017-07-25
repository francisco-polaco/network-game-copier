using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkGameCopier
{
    class SettingsManager
    {
        private static readonly SettingsManager Instance = new SettingsManager();

        public static SettingsManager GetInstance()
        {
            return Instance;
        }

        private SettingsManager()
        {
        }


    }
}
