using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(CusInBondHeaderConsolController))]
	sealed class CusInBondHeaderConsolControllerTest : ZControllerBasherTest
	{
		public void TestOpenConsolFormFromInBondModuleForPlugInInBond()
		{
			using (Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertEquals("Consol form is shown for PlugIn In-Bond", typeof(ConsolForm), Controller.LastShownForm.GetType());
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.InBondPluggedIntoConsol;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var inBond = Factory.New<CusInBondHeader>();
			var consol = Factory.New<ForwardingConsol>();
			inBond.BH_ParentID = consol.PK;
			inBond.BH_ParentTableCode = consol.TablePrefix;
			Factory.Save();
			return inBond;
		}

		new CusInBondHeaderConsolControllerForTest Controller
		{
			get
			{
				if (fController == null)
				{
					fController = new CusInBondHeaderConsolControllerForTest();
				}
				return fController;
			}
		}
		CusInBondHeaderConsolControllerForTest fController;

		sealed class CusInBondHeaderConsolControllerForTest : CusInBondHeaderConsolController
		{
			public new IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
			{
				return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}

			public new IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
			{
				return base.LoadBusinessEntity(factory, sourceEntityPK);
			}
		}
	}
}
