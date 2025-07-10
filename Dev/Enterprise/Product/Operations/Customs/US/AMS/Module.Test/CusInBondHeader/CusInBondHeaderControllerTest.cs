using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	[TestedType(typeof(CusInBondHeaderController))]
	sealed class CusInBondHeaderControllerTest : ZControllerBasherTest
	{
		public void TestShowNewForm()
		{
			var controller = new CusInBondHeaderController();
			using (var form = (USAMSForm)controller.ShowNewForm())
			{
				AssertEquals("form.BusinessEntity.IsNVOCCHeader", false, form.BusinessEntity.IsNVOCCHeader);
			}

			controller.CreateNVOCCAMS = true;
			using (var form = (USAMSForm)controller.ShowNewForm())
			{
				AssertEquals("form.BusinessEntity.IsNVOCCHeader", true, form.BusinessEntity.IsNVOCCHeader);
			}
		}

		public void TestGetForm()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			var controller = new CusInBondHeaderController();
			using (var form = controller.ShowEditForm(header))
			{
				AssertEquals(typeof(USAMSForm), form.GetType());
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.US.AMS;
		}
	}
}
