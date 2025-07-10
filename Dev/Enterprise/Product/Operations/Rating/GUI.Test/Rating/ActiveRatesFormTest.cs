using System.Windows.Forms;
using CargoWise.Common.Testing;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class ActiveRatesFormTest : RatingTestCase
	{
		public void TestFiltersShouldBeEnabledInViewMode()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			Factory.Save();

			using (var form = new ActiveRatesForm(clientRate))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				var filterControl = (IReadOnlyToggleControl)form.rateEntryFilterStripControl;
				AssertEquals(false, filterControl.ReadOnly);
			}
		}
	}

	#region Form Basher

	[TestedType(typeof(ActiveRatesForm))]
	public class ActiveRatesFormBasherTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (ZForm testForm = (ZForm)GetFormToBash())
			{
				AssertEquals("Client Rate TESTORG1", testForm.FormCaption);
			}
		}

		[ExpectNoExceptions, SnailTest]
		public void TestValidationPerformance()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = false;
			var rate = Helper.NewFullyPopulatedClientRate();
			rate.RunPreSaveValidation();
		}

		[ExpectNoExceptions, SnailTest]
		public void TestSavePerformance()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = false;
			var rate = Helper.NewFullyPopulatedClientRate();
			rate.Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestDeleteDoesNotThrowExceptions_ClientRate()
		{
			var clientRate = Helper.NewFullyPopulatedClientRate();
			Factory.Save();

			using (var form = new ActiveRatesForm(clientRate))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();

				form.AcceptButton.PerformClick();
			}

			Assert("Should have deleted the bizo", clientRate.IsDeleted);
		}

		#region Implementation

		TestHelper Helper => fHelper ??= new TestHelper(Factory);
		TestHelper fHelper;

		protected override Form GetFormToBashCore()
		{
			Helper.IsMarkAsNeedingValidationSuspended = true;
			var rate = Helper.NewFullyPopulatedClientRate();
			using (rate.SuspendMarkingAsNeedingValidation())
			{
				Factory.Save();
			}
			Helper.IsMarkAsNeedingValidationSuspended = false;

			var result = new ActiveRatesForm(rate);
			result.ControllerID = ControllerIDs.ClientRates;

			return result;
		}

		#endregion
	}

	#endregion
}
