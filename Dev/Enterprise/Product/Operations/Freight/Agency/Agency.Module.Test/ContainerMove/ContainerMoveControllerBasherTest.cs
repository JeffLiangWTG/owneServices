using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerMoveController))]
	internal sealed class ContainerMoveControllerBasherTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyContainerMove);
			AssertEquals(ModuleIDs.AgencyContainerMove, controller.ModuleID);
		}

		public void TestLoadFromPK()
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100011";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			ContainerMovement movement = stock.Movements.AddNew();

			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyContainerMove);
			BusinessObject bo = controller.Factory.Load(controller.TypeOfTopLevelBusinessObject, movement.PK);

			using (ZForm form = (ZForm)controller.ShowEditForm(bo))
			{
				AssertEquals(true, BusinessObjectEqualityComparer<BusinessObject>.IgnoreFactoryComparer.Equals((BusinessObject)form.BusinessEntity, stock));
			}
		}

		public void TestCheckpoints()
		{
			ContainerMoveController moveController = new ContainerMoveController();
			ContainerManagerController containerController = new ContainerManagerController();

			CombineAssertions(delegate
			{
				AssertEquals("Delete", containerController.GetCheckPointForEdit(null), moveController.GetCheckPointForDelete(null));
				AssertEquals("Edit", containerController.GetCheckPointForEdit(null), moveController.GetCheckPointForEdit(null));
				AssertEquals("New", containerController.GetCheckPointForEdit(null), moveController.GetCheckPointForNew(null));
				AssertEquals("View", containerController.GetCheckPointForView(null), moveController.GetCheckPointForView(null));
			});
		}

		public void TestReUseExistingContainerManagerForms()
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			ContainerMovement movement = stock.Movements.AddNew();

			Factory.Save();

			ZController stockController = ZControllerFactory.Create(ControllerIDs.AgencyContainerManager);
			ZController movementController = ZControllerFactory.Create(ControllerIDs.AgencyContainerMove);

			using (IZForm form1 = stockController.ShowEditForm(stock))
			using (IZForm form2 = movementController.ShowEditForm(movement))
			{
				AssertSame("Should be re-using the same form", form1, form2);
			}
		}

		public void TestShowNewForm()
		{
			using (ZForm form = (ZForm)Controller.ShowNewForm())
			{
				Application.DoEvents();
				AssertType("should return a BulkMovementsForm", typeof(BulkMovementsForm), form);
				AssertEquals("form should be shown", true, form.Visible);
			}
		}

		public void TestShowNewFormDenied()
		{
			Env.Security.AgencyContainerManagerEdit.IsAllowed = false;

			using (ZForm form = (ZForm)Controller.ShowNewForm())
			{
				const string expected =
@"Error You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Liner & Agency -> Container Manager -> Edit
";

				AssertType("Should not have shown a form", null, form);
				AssertMultilineASCIIEquals("Should have shown the security dialog", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestShowTemplateCopyFormDenied()
		{
			ShowFormDelegate method = delegate(AgencyShipmentContainer container, ZController controller)
			{
				return (ZForm)controller.ShowTemplateCopyForm(container);
			};

			GenericShowFormDeniedTest(method);
		}

		public void TestShowDeleteFormDenied()
		{
			ShowFormDelegate method = delegate(AgencyShipmentContainer container, ZController controller)
			{
				return (ZForm)controller.ShowDeleteForm(container);
			};

			GenericShowFormDeniedTest(method);
		}

		public void TestReloadForm()
		{
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			var movement1 = stock.Movements.AddNew();
			movement1.E9_MovementDate = DateTime.Now;

			var movement2 = stock.Movements.AddNew();
			movement2.E9_MovementDate = DateTime.Now.AddDays(1);

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.AgencyContainerMove);
			var bo = controller.Factory.Load(controller.TypeOfTopLevelBusinessObject, movement1.PK);

			using (var editForm = controller.ShowEditForm(bo))
			{
				try
				{
					using (var reloadForm = controller.ShowFormOfGivenDisplayType(editForm.BusinessEntityForPersistingForm as BusinessObject, editForm.DisplayMode))
					{
						Assert(reloadForm is ContainerManagerForm);
						AssertEquals(movement1.PK, ((RefContainerStock)reloadForm.BusinessEntityForPersistingForm).CurrentMovement.PK);
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
					Fail("Reload should not cause exception");
				}
			}
		}

		#region Implementation

		delegate ZForm ShowFormDelegate(AgencyShipmentContainer container, ZController controller);
		void GenericShowFormDeniedTest(ShowFormDelegate method)
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipment>().RealContainers.AddNew();
			Factory.Save();

			try
			{
				ZForm form = method(container, Controller);
				if (form != null)
				{
					form.Dispose();
				}

				Fail("Should have thrown a ModuleGuiNotSupportedException");
			}
			catch (ModuleGuiNotSupportedException)
			{
				Assert(true);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyContainerMove;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			ContainerMovement movement = stock.Movements.AddNew();

			Factory.Save();
			return movement;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
