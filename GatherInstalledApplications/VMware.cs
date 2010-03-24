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
//http://www.codeproject.com/KB/threads/AsyncPowerShell.aspx
namespace GatherInstalledApplications {
    public class _VMware
    {

        public enum HostType { VC, ESX };
        public struct BuildVersion {
            public String Name, Build, Version;
            public HostType hostType;
        }

        private int vCenterCount = 0;

        private PSSnapInException warn;
        private Runspace run;

        private ISynchronizeInvoke invoker;

        private PipelineExecutor pipelineExecutor;
        private _VMware.BuildVersion bv;
        private System.Collections.Queue WaitQueue = new System.Collections.Queue();

        System.Collections.ArrayList alEsxHostInfo = new System.Collections.ArrayList();

        public delegate void DataEventReady();
        public event DataEventReady OnDataReady;
        public System.Collections.ArrayList VcEsxBv {
            get { return alEsxHostInfo; }
        }
        public _VMware.BuildVersion VcBv {
            get { return bv; }
        }


        public _VMware() {
            
            run = RunspaceFactory.CreateRunspace();
            run.RunspaceConfiguration.AddPSSnapIn("VMware.VimAutomation.Core", out warn);
            run.Open();
        }
        public _VMware(ISynchronizeInvoke invoker) {
            this.invoker = invoker;
            //CreateRunspace();
        }

        private void pipelineExecutor_OnDataEnd(PipelineExecutor sender) {
            try {
                if (sender.Pipeline.PipelineStateInfo.State == PipelineState.Failed) {
                    Console.Error.WriteLine("Pipeline Failed but is done");
                }
                else {
                    Console.Error.WriteLine("Pipeline is done");
                    OnDataReady();
                    if (WaitQueue.Count > 0) {
                        vCenterCount = 0;
                        
                        QueryVCESXBuildAsync((string)WaitQueue.Dequeue());

                    }
                }
            }
            catch (Exception e) {
                Console.Error.WriteLine("Stack Trace: {0}, Message: {1}", e.StackTrace, e.Message);
            }
        }

        private void CreateRunspace() {
            try {
                
                run = RunspaceFactory.CreateRunspace();
                run.RunspaceConfiguration.AddPSSnapIn("VMware.VimAutomation.Core", out warn);
                run.Open();
            }
            catch (Exception e) {
                Console.Error.WriteLine("Stack Trace: {0}, Message: {1}", e.StackTrace, e.Message);
            }
        }

        //http://www.codeproject.com/KB/threads/AsyncPowerShell.aspx
        private void StopScript() {
            Console.Error.WriteLine("In Stop Script...");
            if (pipelineExecutor != null) {
                pipelineExecutor.OnDataReady -= new PipelineExecutor.DataReadyDelegate(pipelineExecutor_OnDataReady);
                pipelineExecutor.OnDataEnd -= new PipelineExecutor.DataEndDelegate(pipelineExecutor_OnDataEnd);
                pipelineExecutor.Stop();
                pipelineExecutor = null;
                Console.Error.WriteLine("Script Stopped");
                run.Close();
                run = null;
            }
        }

        private void pipelineExecutor_OnDataReady(PipelineExecutor sender, ICollection<PSObject> data) {
            try {
                Console.Error.WriteLine("Data Finished with {0} results", data.Count);
                if (data.Count > 0) {
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
            catch (Exception e) {
                Console.Error.WriteLine("Stack Trace: {0}, Message: {1}", e.StackTrace, e.Message);
            }
        }

        private void pipelineExecutor_OnErrorReady(PipelineExecutor sender, ICollection<object> data) {
            foreach (object e in data) {
                Console.Error.WriteLine(e.ToString());
            }
        }

        private void FillESXInfo(ICollection<PSObject> data) {
            try {
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
            catch (Exception e) {
                Console.Error.WriteLine("Stack Trace: {0}, Message: {1}", e.StackTrace, e.Message);
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

            if (vCenterCount != 0) {
                WaitQueue.Enqueue(vcServer);   
            }
            else {
                try {
                    this.StopScript();
                    this.CreateRunspace();
                    StringBuilder sbConnectVIServer = new StringBuilder();

                    sbConnectVIServer.AppendFormat("Connect-VIServer {0}\n Get-VMHost", vcServer);
                    Console.Error.WriteLine("Starting PowerCLI command");
                    pipelineExecutor = new PipelineExecutor(run, this.invoker, sbConnectVIServer.ToString());
                    pipelineExecutor.OnDataReady += new PipelineExecutor.DataReadyDelegate(pipelineExecutor_OnDataReady);
                    pipelineExecutor.OnDataEnd += new PipelineExecutor.DataEndDelegate(pipelineExecutor_OnDataEnd);
                    pipelineExecutor.OnErrorReady += new PipelineExecutor.ErrorReadyDelegate(pipelineExecutor_OnErrorReady);
                    pipelineExecutor.Start();
                    vCenterCount = 1;
                }
                catch (Exception e) {
                    Console.Error.WriteLine("Stack Trace: {0}, Message: {1}", e.StackTrace, e.Message);
                }
            }
        }
    }
}
