using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class ConsolConsumerType : JobInvoicingConsumerType
	{
		public ConsolConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.JobConsol; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(); }
		}

		public override bool OverseasAgentApplicable
		{
			get { return true; }
		}

		public override bool IsTransportModeSupported
		{
			get { return true; }
		}

		public override bool IsDirectionSupported
		{
			get { return true; }
		}

		public override string OverseasAgentText
		{
			get { return Res.GetString("1093132c-fd4d-417a-8b29-4865377f16ea", "Collect Agent"); }
		}

		public override string LocalClientText
		{
			get { return Res.GetString("46f8e48f-bb33-4c28-8ffe-0cc6c85b81db", "Prepaid Agent"); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		public override bool SupportsWiseRates
		{
			get { return true; }
		}
	}
}
