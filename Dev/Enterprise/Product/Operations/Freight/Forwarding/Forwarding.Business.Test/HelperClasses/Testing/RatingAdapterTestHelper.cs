using System;
using CargoWise.Types;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public static class RatingAdapterTestHelper
	{
		public class DummyContainerPenalty : IContainerPenalty
		{
			public DummyContainerPenalty(ZString refContainerCode)
			{
				RefContainerCode = refContainerCode;
			}
			public ZString RefContainerCode { get; }
			public ZString CPY_ProcessType { get; set; }
			public ZString CPY_PenaltyType { get; set; }
			public ZString CPY_CreditorType { get; set; }
			public ZDateTime CPY_FreeTime { get; set; }
			public ZString CPY_TimeUnit { get; set; }
			public ZDecimal CPY_PerUnitCost { get; set; }
			public ZString CPY_RX_NKCurrency { get; set; }
		}

		public class DummyTransport : ITransport
		{
			public ZString JW_Status { get; set; }
			public ZBool JW_IsLinked { get; set; }
			public Type ParentType { get; set; }
			public ZString JW_RL_NKLoadPort { get; set; }
			public ZString JW_RL_NKDiscPort { get; set; }
			public ZString JW_VoyageFlight { get; set; }
			public ZString JW_Vessel { get; set; }
			public ZDateTime JW_ETA { get; set; }
			public ZDateTime JW_ATA { get; set; }
			public ZDateTime JW_ETD { get; set; }
			public ZGuid JW_ParentGUID { get; set; }
			public ZString JW_TransportMode { get; set; }
			public ZByte JW_LegOrder { get; set; }
			public ZString JW_VesselScreeningStatus { get; set; }
			public ZGuid JW_OA_CarrierAddress { get; set; }
			public ZString CarrierCode { get; }
			public ZString CarrierName { get; }
			public ZString JW_LegNotes { get; set; }
			public ZDateTime JW_DocumentaryCutOff { get; set; }
			public ZDateTime JW_TerminalCutOff { get; set; }
			public ZDateTime JW_VGMCutOff { get; set; }
		}
	}
}
