using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class BrokerageConsumerType : JobInvoicingConsumerType
	{
		public BrokerageConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.JobDeclaration; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(); }
		}

		public override bool OverseasAgentApplicable
		{
			get { return true; }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}

		public override bool ShouldDisplayClientContractNumber(IJobInvoicingPlugIn host)
		{
			return true;
		}

		public override bool IsTransportModeSupported
		{
			get { return true; }
		}

		public override bool IsDirectionSupported
		{
			get { return true; }
		}
	}
}
