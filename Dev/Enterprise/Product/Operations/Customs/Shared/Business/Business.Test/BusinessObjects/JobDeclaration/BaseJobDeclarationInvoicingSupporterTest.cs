using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobDeclaration.BaseJobDeclarationInvoicingSupporter))]
	sealed class BaseJobDeclarationInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestVoyageVesselOrFlightDate()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "NAUTILUS";
			declaration.JE_VoyageFlightNo = "007";

			var supporter = new BaseJobDeclaration.BaseJobDeclarationInvoicingSupporter(declaration);
			AssertEquals("007/NAUTILUS", supporter.VoyageVesselOrFlightDate);
		}

		public void TestServiceDirection()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var supporter = new BaseJobDeclaration.BaseJobDeclarationInvoicingSupporter(declaration);
			AssertEquals(JobMessageTypeList.Codes.Drawback, supporter.ServiceDirection);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject() => Factory.NewWithValidTestData<BaseJobDeclaration>();
	}
}
