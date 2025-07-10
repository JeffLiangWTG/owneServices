using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccComplianceSequenceController))]
	sealed class AccComplianceSequenceControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccComplianceSequence;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccComplianceSequence complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();
			return complianceSequence;
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			base.SetUp();
		}
	}
}
