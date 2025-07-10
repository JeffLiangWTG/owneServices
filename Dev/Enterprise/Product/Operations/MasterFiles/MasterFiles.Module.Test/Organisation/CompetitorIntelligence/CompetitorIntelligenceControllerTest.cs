using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CompetitorIntelligenceController))]
	sealed class CompetitorIntelligenceControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader org = factory.New<OrgHeader>();
			org.OH_FullName = "Name";
			org.MainAddress.OA_Address1 = "Address";
			org.OH_RL_NKClosestPort = "USLAX";
			org.OH_IsCompetitor = true;
			factory.Save();

			return org;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CompetitorIntelligence;
		}
	}
}
