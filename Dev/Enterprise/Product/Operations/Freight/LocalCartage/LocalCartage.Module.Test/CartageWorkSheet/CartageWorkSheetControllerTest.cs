using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageWorkSheetController))]
	public class CartageWorkSheetControllerTest : ZControllerBasherTest
	{
		public void TestIsRoot()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			Assert(!workSheet.IsRoot);
			Factory.Save();
			var controller = new CartageWorkSheetController();
			var workSheet_FreshFactory = new BusinessObjectFactory().Load<CommonWorkSheet>(workSheet.PK);
			using (var shownForm = (CartageWorkSheetForm)controller.ShowEditForm(workSheet_FreshFactory))
			{
				Assert("Edit", ((CommonWorkSheet)shownForm.BusinessEntity).IsRoot);
			}

			using (var shownForm = (CartageWorkSheetForm)controller.ShowNewForm())
			{
				Assert("New", ((CommonWorkSheet)shownForm.BusinessEntity).IsRoot);
			}
		}

		[TestDate(2018, 11, 1)]
		public void TestNewRunSheetDefaultsCorrectTimes()
		{
			var controller = new CartageWorkSheetController();
			using (var form = controller.ShowNewForm())
			{
				var runSheet = (CommonWorkSheet)form.BusinessEntityForPersistingForm;
				AssertEquals("Should default start time to the beginning of Today.", new ZDateTime(2018, 11, 1), runSheet.EY_StartTime);
				AssertEquals("Should default start time to the beginning of Today.", new ZDateTime(2018, 11, 1, 23, 59, 0), runSheet.EY_EndTime);
			}
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(CommonWorkSheet);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CartageWorkSheet;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}
	}
}
