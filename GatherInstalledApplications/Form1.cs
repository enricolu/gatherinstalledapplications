using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GatherInstalledApplications {
    public partial class Form1 : Form {

        public enum ProductUpdate { PSE, DDC, NONE };

        private System.IO.StreamWriter sw;
        private String ddcPatches = "";
        private String psePatches = "";
        private System.Collections.ArrayList alAppNameVer = null;

        public Form1() {
            InitializeComponent();
            progressBarControl1.Properties.Step = 1;
            progressBarControl1.Properties.Minimum = 0;
            
            sw = new System.IO.StreamWriter("appver.csv");
        }

        private void simpleButtonStart_Click(object sender, EventArgs e) {
            processStartButton();
        }

        private void simpleButtonQuit_Click(object sender, EventArgs e) {
            Application.ExitThread();
        }

        private void CreateUpdateString() {
            for( int index = alAppNameVer.Count - 1; index >=0; index-- ) {
                RegistryQuery.AppNameVer anv = (RegistryQuery.AppNameVer) alAppNameVer[index];
                if (anv.update) {
                    if (anv.DisplayVersion.Contains("DDC")) {
                        ddcPatches += anv.DisplayVersion + " ";
                        alAppNameVer.RemoveAt(index);
                    }
                    else if (anv.DisplayVersion.Contains("PSE")) {
                        psePatches += anv.DisplayVersion + " ";
                        alAppNameVer.RemoveAt(index);
                    }
                }
            }
        }
        private void StepProgressBar() {
            progressBarControl1.PerformStep();
            progressBarControl1.Update();
        }
        private void WriteToCSV(RegistryQuery.AppNameVer anv, String server, String appVendor, ProductUpdate pu) {

            switch (pu) {
                case ProductUpdate.DDC:
                    sw.WriteLine("{0},{1},{2},{3},{4}", appVendor, anv.DisplayName, anv.DisplayVersion, ddcPatches, server);
                    sw.Flush();
                    break;
                case ProductUpdate.PSE:
                    sw.WriteLine("{0},{1},{2},{3},{4}", appVendor, anv.DisplayName, anv.DisplayVersion, psePatches, server);
                    sw.Flush();
                    break;
                case ProductUpdate.NONE:
                    sw.WriteLine("{0},{1},{2},,{3}", appVendor, anv.DisplayName, anv.DisplayVersion, server);
                    sw.Flush();
                    break;
            }
        }

        private void ProcessArrayList(String appVendor, String server) {
            foreach( RegistryQuery.AppNameVer anv in alAppNameVer ) {
                StepProgressBar();
                if ((anv.DisplayName.CompareTo("Citrix Desktop Delivery Controller")) == 0) {
                    WriteToCSV(anv, server, appVendor, ProductUpdate.DDC);
                }
                else if (anv.DisplayName.Contains("Citrix Presentation Server for Windows")) {
                    sw.WriteLine("{0},{1},{2},{3},{4}", appVendor, anv.DisplayName, anv.DisplayVersion, psePatches, server);
                }
                else {
                    WriteToCSV(anv, server, appVendor, ProductUpdate.NONE);
                }
            }
        }


        private void processStartButton() {
            int appVendorCount = checkedComboBoxEditVendors.Properties.Items.Count;
            int serverCount = memoEditServers.Lines.Length;

            
            progressBarControl1.Properties.Maximum = appVendorCount * serverCount;

            foreach (String server in memoEditServers.Lines) {
                ddcPatches = "";
                psePatches = "";
                RegistryQuery rq = new RegistryQuery(server);

                for (int i = 0; i < appVendorCount; i++) {
                    if (checkedComboBoxEditVendors.Properties.Items[i].CheckState == CheckState.Checked) {
                        
                        String appVendor = (string)checkedComboBoxEditVendors.Properties.Items[i].Value;
                        alAppNameVer = rq.Get(appVendor);
                        
                        CreateUpdateString();
                        ProcessArrayList(appVendor,server);
                    }
                }
            }
            sw.Close();
        }
    }
}
