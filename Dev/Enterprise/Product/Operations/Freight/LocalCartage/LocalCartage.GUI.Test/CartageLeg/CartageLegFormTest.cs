using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(CartageLegForm))]
	class CartageLegFormTest : ZFormBasherTest
	{
		public void TestRunSheetSecurityGUIProvider_Register()
		{
			using (var form = new CartageLegForm(CartageLeg))
			{
				var provider = RunSheetSecurityProvider.GetProvider(Factory);
				AssertNotNull(provider);
				AssertEquals(typeof(RunSheetSecurityGUIProvider), provider.GetType());
			}
		}

		public void TestCartageLegShowsAsReadOnlyIfCartageDeactivated()
		{
			CartageLeg.Cartage.JJ_IsCancelled = true;
			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var cartageLegInAnotherFactory = anotherFactory.Load<CommonCartageLeg>(CartageLeg.PK);
			using (var form = new CartageLegForm(cartageLegInAnotherFactory))
			{
				form.Show();
				AssertEquals("Leg should be set read only", true, cartageLegInAnotherFactory.ReadOnly);
			}
		}

		public void TestNewButton()
		{
			using (var form = new CartageLegForm(CartageLeg))
			{
				var type = form.GetType();
				var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
				var propertyInfo = type.GetProperty("AllowNew", bindingFlags);
				AssertEquals(false, propertyInfo.GetValue(form, null));
			}
		}

		public void TestOpenLocalTransportButton_Click()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "ABC";
			staff.StaffPlainTextPassword = "Hello";
			var createViewLocalCartageCheckPoint = Env.Security.LocalCartageJobTypeEdit;
			var securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityRight = createViewLocalCartageCheckPoint.Code;
			securityRecord.GU_ItemGUID = createViewLocalCartageCheckPoint.ItemGuid;
			securityRecord.GU_SecurityItemIsAllowed = false;
			securityRecord.GU_GS = staff.PK;
			staff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);
			Factory.Save();
			var cartage = Factory.New<CommonCartage>();
			var leg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var form = new CartageLegForm(leg))
			{
				form.Show();
				var legUserControl = form.CartageLegUserControl;
				var openLocalTransportButton = (ZButton)(legUserControl.Controls.Find("OpenLocalTransportButton", true).Single());
				AssertNoExceptionThrown(() => openLocalTransportButton.PerformClick());
				AssertEquals("You do not have the appropriate security rights to run this function.\r\n\r\n" + "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\n" + "Operate -> Port Transport -> Transport Jobs", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			CartageLegForm result = new CartageLegForm(CartageLeg);
			result.ControllerID = ControllerIDs.CartageLeg;
			return result;
		}

		protected override void SetUp()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CartageLeg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			base.SetUp();
		}

		CommonCartageLeg CartageLeg;
	}
}
