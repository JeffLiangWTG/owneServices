using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class BRLPCOConsumerType : JobInvoicingConsumerType
	{
		public BRLPCOConsumerType(string code, MultilingualString description) : base(code, description)
		{
		}

		public override ControllerID ControllerID => ControllerIDs.Customs.BR.LPCO;

		public override Type BizoType => ObjectFactory.GetType<Enterprise.Integration.Customs.BR.ICusLPCOHeader>();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;
	}
}
