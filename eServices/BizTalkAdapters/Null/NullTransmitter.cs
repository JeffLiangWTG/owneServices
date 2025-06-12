using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Common;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;

namespace CargoWise.eHub.BizTalkAdapters.Null
{
	public class NullTransmitter :
			IBTTransport,
			IBTTransportControl,
			IPersistPropertyBag,
			IBTTransmitter
	{
		#region IBTTransportControl

		IBTTransportProxy transportProxy;

		public virtual void Initialize(IBTTransportProxy transportProxy)
		{
			this.transportProxy = transportProxy;
		}

		public virtual void Terminate()
		{
			Marshal.ReleaseComObject(this.transportProxy);
			transportProxy = null;
		}

		#endregion

		#region IBTTransmitter

		public bool TransmitMessage(Microsoft.BizTalk.Message.Interop.IBaseMessage msg)
		{
			string configXml = (string)msg.Context.Read("AdapterConfig", "http://cargowise.com/ehub/biztalkadapters/null-properties");
			var config = XDocument.Load(new StringReader(configXml)).Element("Config");

			string portName = (string)msg.Context.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			string logDir = config.Element("LogDir").Value;
			int logMaxSize = Int32.Parse(config.Element("LogMaxSize").Value);
			int logMaxCount = Int32.Parse(config.Element("LogMaxCount").Value);

			var logger = TransferrerHelpers.CreatePortLogger(portName + ".Null", logDir, "All", logMaxSize, logMaxCount);

			TransferrerHelpers.Log(null, logger, LogLevel.Info, "========================================================================================================================");
			var msgBldr = new StringBuilder("================================================= START NULLED MESSAGE =================================================").AppendLine().AppendLine();
			for (int i = 0; i < msg.Context.CountProperties; i++)
			{
				string strName, strNamespace;
				object value = msg.Context.ReadAt(i, out strName, out strNamespace);
				if (strName != "AdapterConfig")
					msgBldr.AppendFormat("{0}#{1} = {2}", strNamespace, strName, value).AppendLine();
			}
			TransferrerHelpers.Log(null, logger, LogLevel.Info, msgBldr.ToString());
			using (var sr = new StreamReader(msg.BodyPart.GetOriginalDataStream()))
				TransferrerHelpers.Log(null, logger, LogLevel.Info, "================================================== START MESSAGE DATA ==================================================" 
					+ Environment.NewLine + Environment.NewLine + sr.ReadToEnd() + Environment.NewLine);
			TransferrerHelpers.Log(null, logger, LogLevel.Info, "=================================================== END MESSAGE DATA ===================================================");
			TransferrerHelpers.Log(null, logger, LogLevel.Info, "========================================================================================================================");

			return true;
		}

		#endregion

		#region IBTTransport

		public Guid ClassID
		{
			get { return new Guid("340c5954-ad70-4bcd-b527-097f05253b78"); }
		}

		public string Description
		{
			get { return "Null Adapter"; }
		}

		public string Name
		{
			get { return "Null Adapter"; }
		}

		public string TransportType
		{
			get { return "null"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IPersistPropertyBag

		public void GetClassID(out Guid classID)
		{
			classID = this.ClassID;
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
		}

		#endregion
	}
}
