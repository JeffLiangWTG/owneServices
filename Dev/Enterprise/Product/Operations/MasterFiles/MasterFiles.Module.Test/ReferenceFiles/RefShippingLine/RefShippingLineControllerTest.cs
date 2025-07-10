using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefShippingLineController))]
	sealed class RefShippingLineControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			AssertExceptionThrown<SecurityAccessDeniedException>(
				"To register a new carrier with CargoWise, please raise a CR8 Compliance, Reference and Master Data request. Once this request has been processed the new carrier will be available in your system. To register a new carrier with CargoWise, please raise a CR8 Compliance, Reference and Master Data request. Once this request has been processed the new carrier will be available in your system.",
				() => Controller.ShowNewForm());
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals("Type of Top Level BizO should be RefShippingLine", typeof(RefShippingLine), new RefShippingLineController().TypeOfTopLevelBusinessObject);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID should be RefPostCode", ModuleIDs.RefShippingLine, new RefShippingLineController().ModuleID);
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new RefShippingLineController();
			CombineAssertions(() =>
			{
				AssertType<RefShippingLineNewSecurityCheckpoint>("For New", controller.CheckPointForNewExposedForTest);
				AssertEquals("For View", Env.Security.RefShippingLineView, controller.CheckPointForViewExposedForTest);
				AssertEquals("For Edit", Env.Security.RefShippingLineEdit, controller.CheckPointForEditExposedForTest);
				AssertEquals("For Delete", Env.Security.RefShippingLineDelete, controller.CheckPointForDeleteExposedForTest);
			});
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefShippingLine;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CarrierName = "TEST CARRIER";
			shippingLine.RSL_CargoWiseOneCode = "CW1C";
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			Factory.Save();
			return shippingLine;
		}

		#endregion
	}
}
