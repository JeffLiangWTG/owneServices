using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class JobInvoicingImporterSecurityFilingConsumerType : JobInvoicingConsumerType
	{
		public JobInvoicingImporterSecurityFilingConsumerType(string code, MultilingualString description)
			: base(code, description)
		{ }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ImporterSecurityFiling; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Customs.US.ISF.ICusISFHeader>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}
	}
}
