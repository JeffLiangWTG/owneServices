using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class CusCAeMHJobInvoicingConsumerType : JobInvoicingConsumerType
	{
		public CusCAeMHJobInvoicingConsumerType(string code, MultilingualString description)
			: base(code, description)
		{ }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.CA.CAHouseBilleManifest; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICusCAeMHMaster>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}
	}
}
