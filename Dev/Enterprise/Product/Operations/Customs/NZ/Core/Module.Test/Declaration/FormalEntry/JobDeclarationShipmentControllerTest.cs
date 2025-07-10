using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(JobDeclarationShipmentController))]
	sealed class JobDeclarationShipmentControllerTest : Customs.Module.Testing.JobDeclarationShipmentControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.JobDeclarationPluggedIntoShipment;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			testDec.JE_JS = shipment.PK;
			Factory.Save();
			return testDec;
		}

		protected override string CountryCode
		{
			get
			{
				return "NZ";
			}
		}
	}
}
