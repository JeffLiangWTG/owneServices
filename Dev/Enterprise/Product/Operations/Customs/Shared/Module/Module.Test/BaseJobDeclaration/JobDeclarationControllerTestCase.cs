using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class JobDeclarationControllerTestCase : ZControllerBasherTest
	{
		[ExpectNoExceptions]
		public void TestShowNewExWarehouseForm()
		{
			if (CountryHasExWarehouse)
			{
				DummyJobDeclarationController controller = new DummyJobDeclarationController();
				try
				{
					controller.ShowNewExWarehouseForm();
					AssertNotNull("Form shown", controller.LastShownForm);
					AssertEquals("ExWarehouse form", Customs.Business.JobMessageTypeList.Codes.ExWarehouse, ((BaseJobDeclaration)((ZForm)controller.LastShownForm).BusinessEntity).JE_MessageType);
				}
				finally
				{
					if (controller.LastShownForm != null)
					{
						controller.LastShownForm.Dispose();
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestShowNewForm()
		{
			if (CountryHasExWarehouse)
			{
				ShowFormAndAssertExWarehouseList(true, null);
			}
		}

		public void ShowFormAndAssertExWarehouseList(bool shouldHaveExWarehouseOption, BaseJobDeclaration decToEdit)
		{
			DummyJobDeclarationController controller = new DummyJobDeclarationController();
			try
			{
				if (decToEdit == null)
				{
					controller.ShowNewForm();
				}
				else
				{
					controller.ShowEditForm(decToEdit);
				}

				AssertNotNull("Form shown", controller.LastShownForm);
				AssertEquals("Has EXW", shouldHaveExWarehouseOption, ((BaseJobDeclaration)((ZForm)controller.LastShownForm).BusinessEntity).Lookups.MessageTypeList.ContainsCode(Customs.Business.JobMessageTypeList.Codes.ExWarehouse));
			}
			finally
			{
				if (controller.LastShownForm != null)
				{
					controller.LastShownForm.Dispose();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestShowLoadedForm()
		{
			if (CountryHasExWarehouse)
			{
				BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
				dec.FillWithValidTestData();
				dec.JE_MessageType = "ABC";
				Factory.Save();
				ShowFormAndAssertExWarehouseList(true, dec);
				dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				Factory.Save();
				ShowFormAndAssertExWarehouseList(true, dec);
			}
		}

		public void TestPassingPullerIntoGetLoadedBusinessEntityInLocalFactoryCreatesADeclaration()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
			TestHelperJobDeclarationShipmentController controller = new TestHelperJobDeclarationShipmentController();
			DeclarationFromShipmentPuller puller = new DeclarationFromShipmentPuller(Factory);
			puller.ShipmentPK = shipment.PK;
			IBusiness result = controller.GetLoadedBusinessEntityInLocalFactoryPublic(puller);
			AssertEquals("Result.GetType()", typeof(ForwardingShipment), result.GetType());
			AssertEquals("Result.GetType()", shipment.PK, ((ForwardingShipment)result).PK);
		}

		[ExpectNoExceptions]
		public void TestPassingDecToImportIntoGetLoadedBusinessEntityCreatesADeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			Factory.Save();
			DummyJobDeclarationController controller = new DummyJobDeclarationController();
			ImportJobDeclaration importedDeclaration = new ImportJobDeclaration(Factory);
			IBusiness result = controller.GetLoadedBusinessEntityInLocalFactoryPublic(importedDeclaration);
		}

		public void TestOpenDeclarationFormForStandAlone()
		{
			BaseJobDeclaration standAloneDec = GetJobDeclaration();
			Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase());
			Assert("Declaration form is shown for stand-alone", Controller.LastShownForm is BaseJobDeclarationForm);
		}

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<BaseJobDeclaration>();
			CRMSecurityProviderTest<BaseJobDeclaration>.AssertController(new JobDeclarationController(), bizObjWithoutAccess, Env.Security.CustomsDeclarationEnquiryCRMSecurity);
		}

		protected virtual bool CountryHasExWarehouse
		{
			get
			{
				return true;
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			Factory.Save();
			return declaration;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.JobDeclaration;
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<BaseJobDeclaration>();
		}

		sealed class TestHelperJobDeclarationShipmentController : JobDeclarationShipmentController
		{
			public IBusiness GetLoadedBusinessEntityInLocalFactoryPublic(IBusiness sourceEntity)
			{
				return GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}
		}

		sealed class DummyJobDeclarationController : JobDeclarationController
		{
			public IBusiness GetLoadedBusinessEntityInLocalFactoryPublic(IBusiness sourceEntity)
			{
				return GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}

			public override Type TypeOfTopLevelBusinessObject
			{
				get
				{
					return typeof(BaseJobDeclaration);
				}
			}

			protected override IZForm GetFormCore(IBusiness businessEntity)
			{
				return new BaseJobDeclarationForm((BaseJobDeclaration)businessEntity);
			}
		}
	}
}
