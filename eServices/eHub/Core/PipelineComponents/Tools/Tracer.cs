using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
    /// <summary>
    /// Allows transparency of the pipeline execution order, runtime properties, and resolved values.
    /// </summary>
    /// <remarks>
    /// Requires an eHubTraceLevel switch added to BTSNTSvc.exe.config file.
    /// <![CDATA[
    /// <system.diagnostics>
    ///   <switches>
    ///     <!-- 0,1,2,3, and 4 correspond to Off, Error, Warning, Info, and Verbose -->
    ///     <add name="eHubTraceLevel" value="3" />
    ///   </switches>
    /// </system.diagnostics>
    /// ]]>
    /// </remarks>
    internal static class Tracer
    {
        private const string SURROUND =       "---------------------------------------------------------------------------------------------------";
        private const string ERROR_SURROUND = "********************************************** ERROR **********************************************";

        private static TraceSwitch _eHubTraceSwitch = null;

        /// <summary>
        /// Writes out the port name, port type, port uri, full pipeline name etc
        /// </summary>
        /// <param name="pipelineContext"></param>
        /// <param name="message"></param>
        public static void TraceStart(IPipelineContext pipelineContext, IBaseMessage message)
        {
            if (!EHubTraceSwitch.TraceInfo) return;


            string portName = message.Context.ReadPropertyString<MessageTracking.PortName>();
            string receivePortName = message.Context.ReadPropertyString<BTS.ReceivePortName>();
			string inboundUri = message.Context.ReadPropertyString<BTS.InboundTransportLocation>();
			string outboundUri = message.Context.ReadPropertyString<BTS.OutboundTransportLocation>();
            string portUri = String.IsNullOrEmpty(outboundUri) ? inboundUri : outboundUri;
            string portType = String.IsNullOrEmpty(outboundUri) ? "Receive" : "Send";
			string messageType = message.Context.ReadPropertyString<BTS.MessageType>();
			string schemaStrongName = message.Context.ReadPropertyString<BTS.SchemaStrongName>();

            Type caller = new StackFrame(1, false).GetMethod().DeclaringType;
            string category = caller.Name;

            Trace.WriteLine(SURROUND, category);
            Trace.WriteLine(String.Format("Port: {0} ({1} - {2})", (String.IsNullOrEmpty(portName) && String.IsNullOrEmpty(outboundUri)) ? receivePortName : portName, portType, portUri), category);
            Trace.WriteLine(String.Format("Pipeline: {0}", pipelineContext.PipelineName), category);
            Trace.WriteLine(String.Format("Component: {0}", new StackFrame(1, false).GetMethod().DeclaringType.AssemblyQualifiedName), category);
            Trace.WriteLine(String.Format("MessageType: {0} ({1})", messageType, schemaStrongName), category);
            Trace.WriteLine(String.Empty, category);
        }

        public static void TraceInfo(string message)
        {
            if (!EHubTraceSwitch.TraceInfo) return;

            string category = new StackFrame(1, false).GetMethod().DeclaringType.Name;
            TraceInfo(message, category);
        }

        public static void TraceInfo(string format, params object[] args)
        {
            if (!EHubTraceSwitch.TraceInfo) return;

            string message = String.Format(format, args);
            string category = new StackFrame(1, false).GetMethod().DeclaringType.Name;
            TraceInfo(message, category);
        }

        private static void TraceInfo(string message, string category)
        {
            Trace.WriteLine(String.Format("-> {0}", message), category);
        }

        /// <summary>
        /// Writes out the exception detail.  There's no need to call <see cref="TraceEnd"/> after this
        /// </summary>
        public static void TraceError(Exception ex)
        {
            if (!EHubTraceSwitch.TraceError) return;

            List<string> exceptionLines = new List<string>(ex.ToString().Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries));
            string category = new StackFrame(1, false).GetMethod().DeclaringType.Name;
            Trace.WriteLine(String.Empty, category);
            Trace.WriteLine(ERROR_SURROUND, category);
            foreach (string line in exceptionLines)
            {
                Trace.WriteLine(line, category);
            }
            Trace.WriteLine(ERROR_SURROUND, category);
            Trace.WriteLine(SURROUND, category);
        }

        /// <summary>
        /// Writes out the component and pipeline name that has completed
        /// </summary>
        public static void TraceEnd()
        {
            if (!EHubTraceSwitch.TraceInfo) return;

            string category = new StackFrame(1, false).GetMethod().DeclaringType.Name;
            Trace.WriteLine(SURROUND, category);
        }

        private static TraceSwitch EHubTraceSwitch
        {
            get
            {
                if (_eHubTraceSwitch == null)
                {
                    CreateEHubTraceSwitch();
                }
                return _eHubTraceSwitch;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void CreateEHubTraceSwitch()
        {
            if (_eHubTraceSwitch == null)
            {
                TraceSwitch eHubTraceSwitch = new TraceSwitch("eHubTraceLevel", string.Empty);
                _eHubTraceSwitch = eHubTraceSwitch;
            }
        }
    }
}
