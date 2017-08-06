using System;
using System.IO;
using Microsoft.Win32;

namespace NetworkGameCopier
{
    class RegistryManager
    {
        public static void WriteExecutionLocation()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true);

            key?.CreateSubKey("NetworkGameCopier");
            key = key?.OpenSubKey("NetworkGameCopier", true);

            key?.SetValue("InstalledPath", Path.GetFullPath("."));
        }

  
        public static string FindInstallLocationByName(string name)
        {
            try
            {
                return FindInstallLocationByName64(name);
            }
            catch (NoKeyFoundException)
            {
                return FindInstallLocationByName32(name);
            }
        }
        private static string FindInstallLocationByName32(string name)
        {
            RegistryKey parentKey 
                = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            return FindInstallLocationByKey(name, parentKey);

        }
        private static string FindInstallLocationByName64(string name)
        {
            RegistryKey parentKey 
                = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            return FindInstallLocationByKey(name, parentKey);
        }

        private static string FindInstallLocationByKey(string name, RegistryKey parentKey)
        {
            if (parentKey != null)
            {
                string[] nameList = parentKey.GetSubKeyNames();
                foreach (string t in nameList)
                {
                    RegistryKey regKey = parentKey.OpenSubKey(t);
                    if (regKey != null && t.Equals(name))
                    {
                        return regKey.GetValue("InstallLocation").ToString();
                    }
                }
            }
            throw new NoKeyFoundException();
        }

    }

    internal class NoKeyFoundException : Exception
    {
        public override string ToString()
        {
            return "Key Not Found";
        }
    }
}
