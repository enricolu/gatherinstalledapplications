using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;

using Microsoft.Win32;
//[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum,
//   ViewAndModify = "HKEY_LOCAL_MACHINE")]

namespace GatherInstalledApplications {
   

    class RegistryQuery {

        public struct AppNameVer {
            public Boolean update;
            public String DisplayName, DisplayVersion;
        }
        
        private String _serverName = "";
        private RegistryKey remoteLocalMachines;
        private Microsoft.Win32.RegistryKey uninstallKey = null;
        private String[] subUninstallKeyNames = null;

        public RegistryQuery(String serverName) {
            _serverName = serverName;
            try {
                remoteLocalMachines = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, _serverName);
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                return;
            }

        }
        public System.Collections.ArrayList Get(String appVendor) {
            System.Collections.ArrayList alAppNameVer = new System.Collections.ArrayList();

            try {
                uninstallKey =
                    remoteLocalMachines.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");

                subUninstallKeyNames = uninstallKey.GetSubKeyNames();

                //Console.WriteLine("SubKeyLength {0}", subUninstallKeyNames.Length);
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                throw (e);
            }
            foreach (String subUninstallKey in subUninstallKeyNames) {
                
                try {
                    RegistryKey rk = uninstallKey.OpenSubKey(subUninstallKey);
                    
                    String displayName = (String)rk.GetValue("DisplayName");
                    Object displayVersion = rk.GetValue("DisplayVersion");
                    if (displayName != null) {
                        
                        //Console.WriteLine("{0},{1},{2},{3},{4}", subUninstallKey, displayName, appVendor, displayVersion, _serverName);
                        
                        if (displayName.Contains(appVendor)) {
                            AppNameVer anv = new AppNameVer();
                            anv.DisplayName = displayName;
                            if (displayName.Contains("Update") || displayName.Contains("Hotfix"))
                            {
                                anv.update = true;
                                String[] split = displayName.Split(new char[] { ' ' });
                                String patch = split.Last<String>();
                                anv.DisplayVersion = patch;
                            }
                            else
                            {
                                anv.update = false;
                                if(displayVersion != null )
                                    anv.DisplayVersion = displayVersion.ToString();
                            }
                            alAppNameVer.Add(anv);
                        }
                    }
                    rk.Close();
                }
                catch (Exception ex) {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);
                }
            }

            return alAppNameVer;
        }
    }
}
