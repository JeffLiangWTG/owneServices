using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(ContainersController))]
	sealed class ContainersControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			ContainersController controller = (ContainersController)(ZControllerFactory.Create(ControllerIDs.Containers));
			AssertEquals(ModuleIDs.Containers, controller.ModuleID);
		}

		public void TestLoadedFormWithCusContainer()
		{
			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			var cusContainer = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>());
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			Factory.Save();

			var controller = (ContainersController)(ZControllerFactory.Create(ControllerIDs.Containers));
			AssertNoExceptionThrown
				(delegate
				{
					using (var form = controller.ShowViewForm(cusContainer))
					{
					}
				});
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Containers;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			Factory.Save();
			return container;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var container = base.GetBusinessObjectWithoutValidationErrors() as CommonContainer;
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_TareWeight = 1450.000m;
			container.JC_RC = containerType.PK;
			return container;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public void TestShowEditForm()
		{
			ContainersController controller = new ContainersController();
			BusinessObject container1 = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>());
			BusinessObject container2 = Factory.NewWithValidTestData<CommonContainer>();

			var agencyBooking = Factory.NewWithValidTestData<CommonShipment>();
			agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking.JS_IsShipping = true;
			var container3 = (CommonContainer)Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Agency.IAgencyBookingContainer>());
			container3.JC_JS_FCLBookingOnlyLink = agencyBooking.PK;

			var billOfLading = Factory.NewWithValidTestData<CommonShipment>();
			billOfLading.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			billOfLading.JS_IsShipping = true;
			var container4 = (CommonContainer)Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Agency.IBillOfLadingContainer>());
			container4.JC_JS_FCLBookingOnlyLink = billOfLading.PK;

			Factory.Save();

			using (var form = (ZForm)controller.ShowEditForm(container1))
			{
				AssertNotNull("Form created for CusContainer", form);
				Assert("container loaded in correct type", form.DataSource is CommonContainer);
				AssertEquals("did not create additional bizObj around row", 1, Factory.GetBizOsForPK(container1.PK.ToGuid()).Length);
			}

			using (var form = (ZForm)controller.ShowEditForm(container2))
			{
				AssertNotNull("Form created for Container", form);
				Assert("container loaded in correct type", form.DataSource is CommonContainer);
				AssertEquals("did not create additional bizObj around row", 1, Factory.GetBizOsForPK(container2.PK.ToGuid()).Length);
			}

			using (var form = (ZForm)controller.ShowEditForm(container3))
			{
				AssertNotNull("Form created for Container", form);
				Assert("container loaded in correct type", form.DataSource is Integration.Agency.IAgencyBookingContainer);
				AssertEquals("did not create additional bizObj around row", 1, Factory.GetBizOsForPK(container3.PK.ToGuid()).Length);
			}

			using (var form = (ZForm)controller.ShowEditForm(container4))
			{
				AssertNotNull("Form created for Container", form);
				Assert("container loaded in correct type", form.DataSource is Integration.Agency.IBillOfLadingContainer);
				AssertEquals("did not create additional bizObj around row", 1, Factory.GetBizOsForPK(container4.PK.ToGuid()).Length);
			}
		}
	}
}
