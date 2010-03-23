using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GatherInstalledApplications {
    public partial class Form1 : Form, IDisposable {

        public enum ProductUpdate { PSE, DDC, VC, ESX, NONE };

        private System.IO.StreamWriter sw;
        private String ddcPatches = "";
        private String psePatches = "";
        private System.Collections.ArrayList alAppNameVer = null;

        public Form1() {
            InitializeComponent();
            progressBarControl1.Properties.PercentView = true;
            progressBarControl1.Properties.Step = 1;
            progressBarControl1.Properties.Minimum = 0;
            //this.simpleButtonStart.Enabled = false;
            
            sw = new System.IO.StreamWriter("appver.csv");
        }

        private void simpleButtonStart_Click(object sender, EventArgs e) {
            if (memoEditvCenter.Lines.Length != 0) {
                ProcessStartButtonVCOnly();
            }
            if (!(checkedComboBoxEditVendors.Properties.Items.Count == 0 || memoEditServers.Lines.Length == 0)) {
                processStartButton();
            }
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
        private void WriteToCSV(_VMware.ESXHostInfo ehi, ProductUpdate pu) {
            switch(pu) {
                case ProductUpdate.ESX:
                    sw.WriteLine("{0},{1},{2},{3},{4}", "VMware", "VMware ESX",ehi.Version, ehi.Build, ehi.Name);
                    sw.Flush();
                    break;
                case ProductUpdate.VC:
                    sw.WriteLine("{0},{1},{2},{3},{4}", "VMware", "VMware vCenter", ehi.Version, ehi.Build, ehi.Name);
                    sw.Flush();
                    break;
            }
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
                    WriteToCSV(anv, server, appVendor, ProductUpdate.PSE);
                }
                else {
                    WriteToCSV(anv, server, appVendor, ProductUpdate.NONE);
                }
            }
        }
        #region IDisposable Members

        public new void Dispose() {
            throw new NotSupportedException("");
        }

        #endregion


        /// <summary>
        /// Here for debugging only...
        /// </summary>
        private void ProcessStartButtonVCOnly() {

            _VMware vmware = new _VMware(this);
            int vCenterServerCount = memoEditvCenter.Lines.Length;
            
            foreach (String vcServer in memoEditvCenter.Lines) {
                //vmware.QueryVCBuildAsync(vcServer);
                vmware.QueryESXBuildAsync(vcServer);

                //System.Collections.ArrayList alEsxHostInfo = vmware.QueryESXBuild(vcServer);
                //foreach (_VMware.ESXHostInfo ehi in alEsxHostInfo) {
                //    WriteToCSV(ehi, ProductUpdate.ESX);
                //}
            }
        }

        private void processStartButton() {
            int appVendorCount = checkedComboBoxEditVendors.Properties.Items.Count;
            int serverCount = memoEditServers.Lines.Length;
            int vCenterServerCount = memoEditvCenter.Lines.Length;


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
