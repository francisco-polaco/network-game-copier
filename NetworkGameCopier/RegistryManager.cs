using System;
using Microsoft.Win32;

namespace GameNetworkCopier
{
    class RegistryManager
    {
        public static string FindInstallLocationByName(string name)
        {
            string s = Registry.LocalMachine.Name;
            RegistryKey parentKey 
                = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            if (parentKey != null)
            {
                string[] nameList = parentKey.GetSubKeyNames();
                foreach (string t in nameList)
                {
                    RegistryKey regKey = parentKey.OpenSubKey(t);
                    Console.WriteLine(t);
                    if (regKey != null && t.Equals(name))
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
