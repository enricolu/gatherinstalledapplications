using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

using VMware.VimAutomation.Client20;
using Codeproject.PowerShell;

using System.Threading;

using System.ComponentModel;
//vCenter Server 4.0.0 | 05 May 2009 | Build 162902

//Latest Released Version: 2.5 Update 6 | 01/29/10 | 227666 

//2.5 Update 5 | 07/10/09 | 174791 
//VirtualCenter 2.5 Server Update 4 | 23 Feb 2009 | Build 147633 (English version)
//VirtualCenter 2.5 Server Update 3 | 03 Oct 2008 | Build 119598 (English version)
//VirtualCenter 2.5 Server Update 2 | 25 July 2008 | Build 104217 (English version)
//VirtualCenter 2.5 Server Update 1 | 10 Apr 2008 | Build 84767 (English version)
//VirtualCenter 2.5 Server | 12/10/2007 | Build 64201

//VMware ESX Server 4.0 | 21 May 2009 | Build 164009

//Mware ESX 3.5 Update 5  Build Number	207095
//VMware ESX Server 3.5 Update 4 | 30 Mar 2009 | Build 153875
//VMware ESX Server 3.5 Update 3 | 06 Nov 2008 | Build 123630
//VMware ESX Server 3.5 Update 2 | 13 Aug 2008 | Build 110268
//VMware ESX Server 3.5 Update 1 | 10 Apr 2008 | Build 82663
//VMware ESX Server 3.5 | 02/20/2008 | Build 64607



//http://www.codeproject.com/KB/threads/AsyncPowerShell.aspx




namespace GatherInstalledApplications {
    public class _VMware
    {

        //public delegate void DataReadyDelegate(PipelineExecutor sender, ICollection<PSObject> data);
        //public event DataReadyDelegate OnDataReady;
        //private DataReadyDelegate synchDataReady;

        public enum HostType { VC, ESX };
        public struct BuildVersion {
            public String Name, Build, Version;
            public HostType hostType;
        }

        //public struct VCInfo {
        //    public String Name, Build, Version;
        //}

        private PSSnapInException warn;
        private Runspace run;
        //private Pipeline pipeline;
        //private Boolean _completed = false;
        //private int count = 0;
        private ISynchronizeInvoke invoker;

        private PipelineExecutor pipelineExecutor;
        private _VMware.BuildVersion bv;

        System.Collections.ArrayList alEsxHostInfo = new System.Collections.ArrayList();

        public delegate void DataEventReady();
        public event DataEventReady OnDataReady;
        public System.Collections.ArrayList VcEsxBv {
            get { return alEsxHostInfo; }
        }
        public _VMware.BuildVersion VcBv {
            get { return bv; }
        }

        //public Boolean CompletedInvoke {
        //    get { return _completed; }
        //}

        public _VMware() {
            run = RunspaceFactory.CreateRunspace();
            run.RunspaceConfiguration.AddPSSnapIn("VMware.VimAutomation.Core", out warn);
            run.Open();
        }
        public _VMware(ISynchronizeInvoke invoker) {
            this.invoker = invoker;
            run = RunspaceFactory.CreateRunspace();
            run.RunspaceConfiguration.AddPSSnapIn("VMware.VimAutomation.Core", out warn);
            run.Open();
        }

        private void pipelineExecutor_OnDataEnd(PipelineExecutor sender) {
            if (sender.Pipeline.PipelineStateInfo.State == PipelineState.Failed) {
                Console.Error.WriteLine("Pipeline Failed but is done");
            }
            else {
                Console.Error.WriteLine("Pipeline is done");
                OnDataReady();
            }
        }

        private void pipelineExecutor_OnDataReady(PipelineExecutor sender, ICollection<PSObject> data) {
            Console.Error.WriteLine("Data Finished with {0} results", data.Count);
            if(data.Count > 0 ) {
                Type type = data.ElementAt<PSObject>(0).BaseObject.GetType();
                switch (type.Name) {
                    case "VIServerImpl":
                        FillVCInfo(data);
                        break;
                    case "VMHostImpl":
                        FillESXInfo(data);
                        break;
                }
            }
        }

        private void pipelineExecutor_OnErrorReady(PipelineExecutor sender, ICollection<object> data) {
            foreach (object e in data) {
                Console.Error.WriteLine(e.ToString());
            }
        }

        private void FillESXInfo(ICollection<PSObject> data) {
            foreach (PSObject vmHost in data) {
                BuildVersion _bv = new BuildVersion();
                VMHostImpl vmHostImpl = (VMHostImpl)vmHost.BaseObject;
                _bv.Build = vmHostImpl.Build;
                _bv.Version = vmHostImpl.Version;
                _bv.Name = vmHostImpl.Name;
                _bv.hostType = HostType.ESX;
                alEsxHostInfo.Add(_bv);
            }
        }

        private void FillVCInfo(ICollection<PSObject> data) {
            bv = new BuildVersion();
            foreach (PSObject vc in data) {
                VMware.VimAutomation.Types.VIServer viserver = (VMware.VimAutomation.Types.VIServer)vc.BaseObject;
                bv.Build = viserver.Build;
                bv.Name = viserver.Name;
                bv.Version = viserver.Version;
                bv.hostType = HostType.VC;
                alEsxHostInfo.Add(bv);
            }
        }

        public void QueryVCBuildAsync(String vcServer) {
            StringBuilder sbConnectVIServer = new StringBuilder();
            sbConnectVIServer.AppendFormat("Connect-VIServer {0}", vcServer);

            pipelineExecutor = new PipelineExecutor(run, this.invoker, sbConnectVIServer.ToString());
            pipelineExecutor.OnDataReady += new PipelineExecutor.DataReadyDelegate(pipelineExecutor_OnDataReady);
            pipelineExecutor.OnDataEnd += new PipelineExecutor.DataEndDelegate(pipelineExecutor_OnDataEnd);
            pipelineExecutor.OnErrorReady += new PipelineExecutor.ErrorReadyDelegate(pipelineExecutor_OnErrorReady);
            pipelineExecutor.Start();
        }
        public void QueryVCESXBuildAsync(String vcServer) {

            StringBuilder sbConnectVIServer = new StringBuilder();

            sbConnectVIServer.AppendFormat("Connect-VIServer {0}\n Get-VMHost", vcServer);
            Console.Error.WriteLine("Starting PowerCLI command");
            pipelineExecutor = new PipelineExecutor(run, this.invoker, sbConnectVIServer.ToString());
            pipelineExecutor.OnDataReady += new PipelineExecutor.DataReadyDelegate(pipelineExecutor_OnDataReady);
            pipelineExecutor.OnDataEnd += new PipelineExecutor.DataEndDelegate(pipelineExecutor_OnDataEnd);
            pipelineExecutor.OnErrorReady += new PipelineExecutor.ErrorReadyDelegate(pipelineExecutor_OnErrorReady);
            pipelineExecutor.Start();
        }
    }
}
