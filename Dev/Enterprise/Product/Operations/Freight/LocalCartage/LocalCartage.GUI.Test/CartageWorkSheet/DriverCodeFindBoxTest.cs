using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class DriverCodeFindBoxTest : TestCaseWithFactory
	{
		public void TestSelectFromPopupForm()
		{
			using (DriverCodeFindBoxForTest guidBox = new DriverCodeFindBoxForTest())
			{
				guidBox.PopupFormForTest();
				AssertEquals("A driver group needs to be set in 'Registry->Port Transport->Port Transport Drivers' to be able to select a driver.", UnitTestUserNotification.Instance.LastMessage.Text);
				GlbGroup driversGroup = Factory.New<GlbGroup>();
				var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
				transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());
				Factory.Save();
				guidBox.PopupFormForTest();
				AssertEquals("The driver group defined in 'Registry->Port Transport->Port Transport Drivers' has no staff set. This group needs to have staff added to be able to select a driver.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class DriverCodeFindBoxForTest : DriverCodeFindBox
		{
			public void PopupFormForTest()
			{
				base.SelectFromPopupForm();
			}
		}
	}
}
