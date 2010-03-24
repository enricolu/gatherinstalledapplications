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
        private _VMware vmware;
        private System.Collections.Hashtable htBuild = null;
        System.IO.StreamReader sr;

        public Form1() {
            InitializeComponent();
            progressBarControl1.Properties.PercentView = true;
            progressBarControl1.Properties.Step = 1;
            progressBarControl1.Properties.Minimum = 0;
            //this.simpleButtonStart.Enabled = false;
            
            sw = new System.IO.StreamWriter("appver.csv");
            sr = new System.IO.StreamReader("build.csv");
            FillBuildHashTable();
        }

        private void FillBuildHashTable() {
            htBuild = new System.Collections.Hashtable();
            String buf;

            while ((buf = sr.ReadLine()) != null) {
                String[] arraySplit = buf.Split(',');
                if (arraySplit.Length == 2) {
                    htBuild.Add(arraySplit[0], arraySplit[1]);
                }
            }
        }

        private void simpleButtonStart_Click(object sender, EventArgs e) {
            this.simpleButtonStart.Enabled = false;
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
        private void WriteToCSV(_VMware.BuildVersion bv) {
            String updateVersion = (String) htBuild[bv.Build];


            switch(bv.hostType) {
                case _VMware.HostType.ESX:
                    sw.WriteLine("{0},{1},{2},{3},{4}", "VMware", "VMware ESX",bv.Version + "-" + bv.Build,updateVersion , bv.Name);
                    sw.Flush();
                    break;
                case _VMware.HostType.VC:
                    sw.WriteLine("{0},{1},{2},{3},{4}", "VMware", "VMware vCenter", bv.Version + "-" + bv.Build, updateVersion,  bv.Name);
                    sw.Flush();
                    break;
            }
        }
        private void WriteToCSV(RegistryQuery.AppNameVer anv, String server, String appVendor, ProductUpdate pu) {
            String servicePack = (String)htBuild[anv.DisplayVersion];
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
                    sw.WriteLine("{0},{1},{2},{3},{4}", appVendor, anv.DisplayName, anv.DisplayVersion, servicePack ,server);
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
        private void ProcessStartButtonVCOnly() {

            vmware = new _VMware(this);
            vmware.OnDataReady += new _VMware.DataEventReady(vmware_OnDataReady);
            int vCenterServerCount = memoEditvCenter.Lines.Length;
            
            foreach (String vcServer in memoEditvCenter.Lines) {
                vmware.QueryVCESXBuildAsync(vcServer);
            }
        }

        void vmware_OnDataReady() {
            foreach (_VMware.BuildVersion bv in vmware.VcEsxBv) {
                WriteToCSV(bv);
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
