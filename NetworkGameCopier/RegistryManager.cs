using System;
using Microsoft.Win32;

namespace GameNetworkCopier
{
    class RegistryManager
    {
        public static string FindInstallLocationByName(string name)
        {
            RegistryKey parentKey 
                = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            if (parentKey != null)
            {
                string[] nameList = parentKey.GetSubKeyNames();
                foreach (string t in nameList)
                {
                    RegistryKey regKey = parentKey.OpenSubKey(t);
                    if (regKey != null && regKey.GetValue("DisplayName").ToString() == name)
                    {
                        return regKey.GetValue("InstallLocation").ToString();
                    }
                }
            }
            throw new NoKeyFoundException();
        }
    }

    class NoKeyFoundException : Exception
    {
        public override string ToString()
        {
            return "Key Not Found";
        }
    }
}
