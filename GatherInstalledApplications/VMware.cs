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

        public struct ESXHostInfo {
            public String Name, Build, Version;
        }

        public struct VCInfo {
            public String Name, Build, Version;
        }

        private PSSnapInException warn;
        private Runspace run;
        private Pipeline pipeline;
        private Boolean _completed = false;
        private int count = 0;
        private ISynchronizeInvoke invoker;

        private PipelineExecutor pipelineExecutor;
        private _VMware.VCInfo vci;

        System.Collections.ArrayList alEsxHostInfo = new System.Collections.ArrayList();


        public Boolean CompletedInvoke {
            get { return _completed; }
        }

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
                //AppendLine(string.Format("Error in script: {0}", sender.Pipeline.PipelineStateInfo.Reason));
                Console.Error.WriteLine("Pipeline Failed but is done");
            }
            else {
                Console.Error.WriteLine("Pipeline is done");
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
                //AppendLine("Error : " + e.ToString());
            }
        }

        private void FillESXInfo(ICollection<PSObject> data) {
            foreach (PSObject vmHost in data) {
                ESXHostInfo esxHostInfo = new ESXHostInfo();
                VMHostImpl vmHostImpl = (VMHostImpl)vmHost.BaseObject;
                esxHostInfo.Build = vmHostImpl.Build;
                esxHostInfo.Version = vmHostImpl.Version;
                esxHostInfo.Name = vmHostImpl.Name;
                alEsxHostInfo.Add(esxHostInfo);
            }
        }

        private void FillVCInfo(ICollection<PSObject> data) {
            vci = new VCInfo();
            foreach (PSObject vc in data) {
                VMware.VimAutomation.Types.VIServer viserver = (VMware.VimAutomation.Types.VIServer)vc.BaseObject;
                vci.Build = viserver.Build;
                vci.Name = viserver.Name;
                vci.Version = viserver.Version;
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
        public void QueryESXBuildAsync(String vcServer) {

            StringBuilder sbConnectVIServer = new StringBuilder();

            sbConnectVIServer.AppendFormat("Connect-VIServer {0}\n Get-VMHost", vcServer);
            Console.Error.WriteLine("Starting PowerCLI command");
            pipelineExecutor = new PipelineExecutor(run, this.invoker, sbConnectVIServer.ToString());
            pipelineExecutor.OnDataReady += new PipelineExecutor.DataReadyDelegate(pipelineExecutor_OnDataReady);
            pipelineExecutor.OnDataEnd += new PipelineExecutor.DataEndDelegate(pipelineExecutor_OnDataEnd);
            pipelineExecutor.OnErrorReady += new PipelineExecutor.ErrorReadyDelegate(pipelineExecutor_OnErrorReady);
            pipelineExecutor.Start();

            //}
        }

        public System.Collections.ArrayList QueryESXBuild(String vcServer) {
            System.Collections.ArrayList alEsxHostInfo = new System.Collections.ArrayList();
            Collection<PSObject> vmHostCollection;
            StringBuilder sbConnectVIServer = new StringBuilder();
            sbConnectVIServer.AppendFormat("Connect-VIServer {0}", vcServer);

            pipeline = run.CreatePipeline();
            //pipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(OnPipeline_StateChanged);
            using (pipeline) {
                pipeline.Commands.AddScript(sbConnectVIServer.ToString());
                pipeline.Commands.AddScript("Get-VMHost");
                //

                vmHostCollection = pipeline.Invoke();
                foreach (PSObject vmHost in vmHostCollection) {

                    ESXHostInfo esxHostInfo = new ESXHostInfo();
                    VMHostImpl vmHostImpl = (VMHostImpl)vmHost.BaseObject;
                    esxHostInfo.Build = vmHostImpl.Build;
                    esxHostInfo.Version = vmHostImpl.Version;
                    esxHostInfo.Name = vmHostImpl.Name;
                    alEsxHostInfo.Add(esxHostInfo);
                }
                //pipeline.Commands.AddScript("Disconnect-VIServer -Confirm:$false");
                //pipeline.Invoke();

            }
            
            // = pipeline.Output.
            
            return alEsxHostInfo;
        }

        

        public System.Collections.ArrayList RetrieveEsxBuild() {
            System.Collections.ArrayList alEsxHostInfo = new System.Collections.ArrayList();
            //Collection<PSObject> vmHostCollection = pipeline.Output.
            PipelineReader<PSObject> results = pipeline.Output;
            while( !results.EndOfPipeline ) {
                PSObject vmHost = results.Read();
                ESXHostInfo esxHostInfo = new ESXHostInfo();
                VMHostImpl vmHostImpl = (VMHostImpl)vmHost.BaseObject;
                esxHostInfo.Build = vmHostImpl.Build;
                esxHostInfo.Version = vmHostImpl.Version;
                esxHostInfo.Name = vmHostImpl.Name;
                alEsxHostInfo.Add(esxHostInfo);
            }
            return alEsxHostInfo;
        }

        void OnPipeline_StateChanged(object sender, PipelineStateEventArgs e) {
            
            //Console.Error.WriteLine("PipelineStateInfo {0}", e.PipelineStateInfo.State);

            if (e.PipelineStateInfo.State == PipelineState.Stopped) {
                Console.Error.WriteLine("PipelineState.Stopped");
            }

            if (e.PipelineStateInfo.State == PipelineState.Completed) {
                if( count++ == 1 ) {
                    Console.Error.WriteLine("_completed == true");
                    _completed = true;
                }
            }
        }
    }
}
