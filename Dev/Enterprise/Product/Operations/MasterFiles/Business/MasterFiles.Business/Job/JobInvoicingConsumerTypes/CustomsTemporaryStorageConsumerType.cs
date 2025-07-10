using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsTemporaryStorageConsumerType : JobInvoicingConsumerType
	{
		public CustomsTemporaryStorageConsumerType(string code, MultilingualString description) : base(code, description)
		{
		}

		public override ControllerID ControllerID => ControllerIDs.Customs.EU.UCC6TemporaryStorage;

		public override Type BizoType => ObjectFactory.GetType<Enterprise.Integration.Customs.EU.ITemporaryStorageHeader>();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;
	}
}
