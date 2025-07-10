using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Tracking.Business
{
	public class WebShipmentTransport : DynamicBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string JW_LegOrder = "JW_LegOrder";
			public const string JW_TransportMode = "JW_TransportMode";
			public const string JW_TransportType = "JW_TransportType";
			public const string JW_ETD = "JW_ETD";
			public const string JW_ETA = "JW_ETA";
			public const string JW_ATD = "JW_ATD";
			public const string JW_ATA = "JW_ATA";
			public const string JK_UniqueConsignRef = "JK_UniqueConsignRef";
			public const string JK_MasterBillNum = "JK_MasterBillNum";
			public const string JK_AgentType = "JK_AgentType";
			public const string JK_OA_SendingForwarderAddress = "JK_OA_SendingForwarderAddress";                    // 10
			public const string JK_OA_ReceivingForwarderAddress = "JK_OA_ReceivingForwarderAddress";                // 11
			public const string JA_RL_NKPortOfLoading = "JA_RL_NKPortOfLoading";
			public const string JB_RL_NKPortOfDischarge = "JB_RL_NKPortOfDischarge";
			public const string JV_RV_NKVessel = "JV_RV_NKVessel";
			public const string JV_VoyageFlight = "JV_VoyageFlight";
		}

		#endregion

		public WebShipmentTransport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region  Properties

		public ZByte JW_LegOrder
		{
			get { return this[Schema.JW_LegOrder] != null ? (ZByte)this[Schema.JW_LegOrder] : ZByte.Zero; }
		}

		public ZString JW_TransportType
		{
			get { return this[Schema.JW_TransportType] != null ? (ZString)this[Schema.JW_TransportType] : ZString.Empty; }
		}

		public ZString JW_TransportMode
		{
			get { return this[Schema.JW_TransportMode] != null ? (ZString)this[Schema.JW_TransportMode] : ZString.Empty; }
		}

		public ZDateTime JW_ETD
		{
			get { return this[Schema.JW_ETD] != null ? (ZDateTime)this[Schema.JW_ETD] : ZDateTime.Empty; }
		}

		public ZDateTime JW_ETA
		{
			get { return this[Schema.JW_ETA] != null ? (ZDateTime)this[Schema.JW_ETA] : ZDateTime.Empty; }
		}

		public ZDateTime JW_ATD
		{
			get { return this[Schema.JW_ATD] != null ? (ZDateTime)this[Schema.JW_ATD] : ZDateTime.Empty; }
		}

		public ZDateTime JW_ATA
		{
			get { return this[Schema.JW_ATA] != null ? (ZDateTime)this[Schema.JW_ATA] : ZDateTime.Empty; }
		}

		public ZString JK_UniqueConsignRef
		{
			get { return this[Schema.JK_UniqueConsignRef] != null ? (ZString)this[Schema.JK_UniqueConsignRef] : ZString.Empty; }
		}

		public ZString JK_MasterBillNum
		{
			get { return this[Schema.JK_MasterBillNum] != null ? (ZString)this[Schema.JK_MasterBillNum] : ZString.Empty; }
		}

		public ZString JK_AgentType
		{
			get { return this[Schema.JK_AgentType] != null ? (ZString)this[Schema.JK_AgentType] : ZString.Empty; }
		}

		public ZGuid JK_OA_SendingForwarderAddress
		{
			get { return this[Schema.JK_OA_SendingForwarderAddress] != null ? (ZGuid)this[Schema.JK_OA_SendingForwarderAddress] : ZGuid.Empty; }
		}

		public ZGuid JK_OA_ReceivingForwarderAddress
		{
			get { return this[Schema.JK_OA_ReceivingForwarderAddress] != null ? (ZGuid)this[Schema.JK_OA_ReceivingForwarderAddress] : ZGuid.Empty; }
		}

		public ZString JA_RL_NKPortOfLoading
		{
			get { return this[Schema.JA_RL_NKPortOfLoading] != null ? (ZString)this[Schema.JA_RL_NKPortOfLoading] : ZString.Empty; }
		}

		public ZString JB_RL_NKPortOfDischarge
		{
			get { return this[Schema.JB_RL_NKPortOfDischarge] != null ? (ZString)this[Schema.JB_RL_NKPortOfDischarge] : ZString.Empty; }
		}

		public ZString JV_RV_NKVessel
		{
			get { return this[Schema.JV_RV_NKVessel] != null ? (ZString)this[Schema.JV_RV_NKVessel] : ZString.Empty; }
		}

		public ZString JV_VoyageFlight
		{
			get { return this[Schema.JV_VoyageFlight] != null ? (ZString)this[Schema.JV_VoyageFlight] : ZString.Empty; }
		}

		#endregion

		public override void Delete()
		{
			ErrorReporter.ReportOnce("WebShipmentTransportDelete", "Cannot delete WebShipmentTransport DynamicBusinessObject");
		}
	}
}
