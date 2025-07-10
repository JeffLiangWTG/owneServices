using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UNDGSubstanceController))]
	sealed class UNDGSubstanceControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			DGSubstanceTestHelper.Create("1234", "a", "IMO", additionalInitialisation: subs =>
			{
				subs.DG_LQMaxAmt = 250;
				subs.DG_LQMaxAmtUQ = "G";
			});
			return UNDGSubstanceLoader.LoadSubstances(Factory, "1234", "a", "IMO").First();
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.UNDGSubstance;
		}

		[RequiresSTA]
		public void TestGetForm()
		{
			var subs = Factory.NewWithValidTestData<UNDGSubstance>();
			subs.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			Factory.Save();

			var controller = ZControllerFactory.Create(GetControllerID());
			using (var form = controller.ShowViewForm(subs))
			{
				AssertType(typeof(UNDGSubstanceIATAForm), form);
			}

			var imoSub = (UNDGSubstance)GetBusinessObjectThatIsInTheDatabase();
			AssertEquals(UNDGSubstanceStandardTypes.IMO, imoSub.DG_Standard);
			using (var form = controller.ShowViewForm(imoSub))
			{
				AssertType(typeof(UNDGSubstanceForm), form);
			}
		}
	}
}
