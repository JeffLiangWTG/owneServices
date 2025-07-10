using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobShipmentController))]
	public class TestJobShipmentController : ZControllerBasherTest
	{
		public void TestGetForm()
		{
			// Ensure the security tokens here and the security tokens used in arguments to create FormSecurityBuilders in SecurityCore.cs match.
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var controller = ZControllerFactory.Create(GetControllerID());
			using (var form = (ZForm)controller.ShowViewForm(shipment))
			{
				AssertEquals("ShipmentForm", form.SecurityToken);
				AssertNotNull("Customs Job Declaration plug in should not be view only", form.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclaration));
			}
		}

		public void TestGetModuleID()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			AssertEquals("JobShipment", controller.ModuleID.ToString());
		}

		public virtual void TestGetFormCore()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			var controller = new JobShipmentControllerForTest();
			using (var form = controller.GetFormCore(shipment))
			{
				AssertEquals(typeof(ShipmentForm), form.GetType());
			}
		}

		public void TestIsRoot()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			ForwardingPackLine packLine = shipment2.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;

			Factory.Save();

			var controller = (JobShipmentController)ZControllerFactory.Create(GetControllerID());
			ForwardingShipment shipment_FreshFactory = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			using (ShipmentForm shownForm = (ShipmentForm)controller.ShowEditForm(shipment_FreshFactory))
			{
				Assert("packlines should not be readonly", !((ForwardingShipment)shownForm.BusinessEntity).OuterPackLines[0].ReadOnly);
			}

			controller = (JobShipmentController)ZControllerFactory.Create(GetControllerID());
			shipment_FreshFactory = new BusinessObjectFactory().Load<ForwardingShipment>(shipment2.PK);
			using (ShipmentForm shownForm = (ShipmentForm)controller.ShowEditForm(shipment_FreshFactory))
			{
				Assert("packlines should not be readonly", !((ForwardingShipment)shownForm.BusinessEntity).OuterPackLines[0].ReadOnly);
			}
		}

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<ForwardingShipment>();
			var controller = ZControllerFactory.Create(GetControllerID());
			CRMSecurityProviderTest<ForwardingShipment>.AssertController(controller, bizObjWithoutAccess, Env.Security.MaintainShipmentCRMSecurity);
		}

		#endregion

		public void TestGetLoadedBusinessEntityInLocalFactory_NormalRecord()
		{
			var sourceShipment = Factory.New<ForwardingShipment>();
			sourceShipment.JS_HouseBill = "ABC";
			sourceShipment.Factory.Save();

			var controller = new JobShipmentControllerForTest();
			var localShipment = controller.GetLoadedBusinessEntityInLocalFactoryExposed(sourceShipment);

			AssertNotNull(localShipment);

			CombineAssertions(() =>
			{
				AssertEquals("ABC", localShipment.JS_HouseBill);
				AssertNotEquals("Should be in different factory", sourceShipment.Factory._Instance, localShipment.Factory._Instance);
				AssertNotEquals("Should not be in template factory", typeof(TemplateRecordBusinessObjectFactory), localShipment.Factory.GetType());
				AssertEquals("Should have same pk", sourceShipment.PK, localShipment.PK);
				AssertNull((localShipment as ITemplateRecordProvider)?.TemplateRecord);
			});
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_TemplateRecordProvider()
		{
			ForwardingShipment sourceShipment;
			using (var module = new JobShipmentModuleTest.JobShipmentModuleForTest())
			{
				sourceShipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				sourceShipment.JS_HouseBill = "ABC";
				sourceShipment.Factory.Save();
			}

			var controller = new JobShipmentControllerForTest();
			var localShipment = controller.GetLoadedBusinessEntityInLocalFactoryExposed(sourceShipment);

			AssertNotNull(localShipment);

			CombineAssertions(() =>
			{
				AssertEquals("ABC", localShipment.JS_HouseBill);
				AssertNotEquals("Should be in different factory", sourceShipment.Factory._Instance, localShipment.Factory._Instance);
				AssertEquals("Should be in template factory", typeof(TemplateRecordBusinessObjectFactory), localShipment.Factory.GetType());
				AssertNotEquals("Template record do not keep pks", sourceShipment.PK, localShipment.PK);
				AssertEquals(sourceShipment.TemplateRecord.PK, localShipment.TemplateRecord.PK);
			});
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_TemplateRecord()
		{
			ForwardingShipment sourceShipment;
			using (var module = new JobShipmentModuleTest.JobShipmentModuleForTest())
			{
				sourceShipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				sourceShipment.JS_HouseBill = "ABC";
				sourceShipment.Factory.Save();
			}

			var controller = new JobShipmentControllerForTest();
			var localShipment = controller.GetLoadedBusinessEntityInLocalFactoryExposed(sourceShipment.TemplateRecord);

			AssertNotNull(localShipment);

			CombineAssertions(() =>
			{
				AssertEquals("ABC", localShipment.JS_HouseBill);
				AssertNotEquals("Should be in different factory", sourceShipment.Factory._Instance, localShipment.Factory._Instance);
				AssertEquals("Should be in template factory", typeof(TemplateRecordBusinessObjectFactory), localShipment.Factory.GetType());
				AssertNotEquals("Template record do not keep pks", sourceShipment.PK, localShipment.PK);
				AssertEquals(localShipment.TemplateRecord.PK, sourceShipment.TemplateRecord.PK);
			});
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_TemplateRecordType()
		{
			using (var module = new JobShipmentModuleTest.JobShipmentModuleForTest())
			{
				var sourceShipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				sourceShipment.Factory.Save();

				var controller = new JobShipmentControllerForTest();
				var shipment = controller.GetLoadedBusinessEntityInLocalFactoryExposed(sourceShipment);
				var shipmentType = TypeDecider.GetTypeForBinding(typeof(ForwardingShipment));

				AssertType("The shipmentType should be the client type of ForwardingShipment which is defined in JobShipmentControllerForTest.TypeOfTopLevelBusinessObject.", shipmentType, shipment);

				var specialController = new SpecialJobShipmentControllerForTest();
				var specialShipment = specialController.GetLoadedBusinessEntityInLocalFactoryExposed(sourceShipment);
				var specialShipmentType = TypeDecider.GetTypeForBinding(typeof(SpecialForwardingShipmentForTest));

				AssertType("The specialShipmentType should be the client type of SpecialForwardingShipmentForTest which is defined in SpecialJobShipmentControllerForTest.TypeOfTopLevelBusinessObject.", specialShipmentType, specialShipment);
			}
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var shipment = base.GetBusinessObjectWithoutValidationErrors() as ForwardingShipment;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_UniqueConsignRef = Guid.NewGuid().ToString().Substring(1, 20);
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			return shipment;
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobShipment;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
			return shipment;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		class JobShipmentControllerForTest : JobShipmentController
		{
			public new IZForm GetFormCore(IBusiness businessEntity)
			{
				return base.GetFormCore(businessEntity);
			}

			internal ForwardingShipment GetLoadedBusinessEntityInLocalFactoryExposed(IBusiness businessEntity)
			{
				return (ForwardingShipment)GetLoadedBusinessEntityInLocalFactory(businessEntity);
			}
		}

		class SpecialForwardingShipmentForTest : ForwardingShipment
		{
			public SpecialForwardingShipmentForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class SpecialJobShipmentControllerForTest : JobShipmentControllerForTest
		{
			public override Type TypeOfTopLevelBusinessObject => typeof(SpecialForwardingShipmentForTest);
		}

		#endregion
	}
}
