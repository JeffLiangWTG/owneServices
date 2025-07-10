using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefPostCodeForm))]
	sealed class RefPostCodeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefPostCodeForm(Factory.New<RefPostCode>());
		}

		public void TestFormCaption()
		{
			using (var form = new RefPostCodeForm(Factory.New<RefPostCode>()))
			{
				AssertEquals("Post Code", Res.GetString("RefPostCodeForm|67e6d098-c1c2-6238-87b6-3f68ed950935", "Post Code"));
			}
		}

		public void TestDetachEventHasError()
		{
			var postcode = Factory.NewWithValidTestData<RefPostCode>();
			var cityTown = Factory.NewWithValidTestData<RefCityTown>();

			postcode.RK_CityTownPostCode = "2000";
			cityTown.R9_InternationalName = "SYDNEY";

			postcode.CityTowns.Add(cityTown);
			var pivot = (RefCityPCodePivot)((ManyToManyRelationship)postcode.CityTowns.Relationship).GetPivotObject(cityTown);
			pivot.R0_IsSystem = true;

			Factory.Save();

			var errorString = "This postcode/city relationship is system defined and therefore the selected city cannot be detached from this postcode.";

			using (var form = new RefPostCodeForm(postcode))
			{
				form.Show();
				var moduleButtonGrid = (ZModuleButtonGrid)form.Controls.Find("zModuleButtonGrid1", true)[0];
				moduleButtonGrid.InnerGrid.Select(0);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleButtonGrid.DetachSelectedElement();
				AssertEquals(errorString, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, postcode.CityTowns.Contains(cityTown));
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			pivot.R0_IsSystem = false;
			Factory.Save();
			using (var form = new RefPostCodeForm(postcode))
			{
				form.Show();
				var moduleButtonGrid = (ZModuleButtonGrid)form.Controls.Find("zModuleButtonGrid1", true)[0];
				moduleButtonGrid.InnerGrid.Select(0);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleButtonGrid.DetachSelectedElement();
				AssertNotEquals(errorString, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, postcode.CityTowns.Contains(cityTown));
			}
		}

		[RequiresSTA]
		public void TestRKLattitudeCalcEdit_RKLongitudeCalcEdit_DecimalPlaces()
		{
			using (var form = new RefPostCodeForm())
			{
				form.Show();
				var mainTab = form.Controls.Find("MainTabPage", true).First();
				var latitude = (ZCalcEdit)mainTab.Controls.Find("RK_LattitudeCalcEdit", true).First();
				var longitude = (ZCalcEdit)mainTab.Controls.Find("RK_LongitudeCalcEdit", true).First();

				AssertEquals("Latitude decimal places", 6, latitude.DecimalPlaces);
				AssertEquals("Longitude decimal places", 6, longitude.DecimalPlaces);
			}
		}
	}
}
