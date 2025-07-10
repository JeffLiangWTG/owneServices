using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(IntercompanyTariffController))]
	public class IntercompanyTariffControllerTest : RatingControllerTest<IntercompanyTariffController, IntercompanyTariff>
	{
		protected override ControllerID GetControllerID() =>
			ControllerIDs.IntercompanyTariffs;

		protected override RatingHeader GetGlobalRatingHeader() =>
			Helper.NewFullyPopulatedIntercompanyTariff();

		protected override RatingHeader GetRatingHeader() =>
			Helper.NewFullyPopulatedIntercompanyTariff();

		#region Security

		protected override SecurityCheckpoint DefaultCheckPointForView => Env.Security.IntercompanyTariffsView;

		protected override SecurityCheckpoint DefaultCheckPointForEdit => Env.Security.IntercompanyTariffsEdit;

		protected override SecurityCheckpoint DefaultCheckPointForDelete => Env.Security.IntercompanyTariffsDelete;

		protected override SecurityCheckpoint DefaultCheckPointForNew => Env.Security.IntercompanyTariffsNew;

		protected override SecurityCheckpoint DefaultCheckPointForCopy => Env.Security.IntercompanyTariffsCopy;

		protected override CRMSecurity DefaultCRMSecurity => null;

		#endregion

		public override void TestSecurityCheckPoints()
		{
			var controller = new IntercompanyTariffController();
			var ratingHeader = GetRatingHeader();
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR);
			rateEntry.RateLines.RemoveAndDeleteAll();

			Env.Security.CachingEnabled = false;

			DefaultCheckPointForNew.IsAllowed = true;
			DefaultCheckPointForEdit.IsAllowed = false;
			DefaultCheckPointForCopy.IsAllowed = false;
			DefaultCheckPointForDelete.IsAllowed = false;
			DefaultCheckPointForView.IsAllowed = false;

			Factory.Save();

			// Reloading ratingHeader and rateEntry in new Factory because of security settings' caching.
			var ratingHeaderReloaded = Helper.LoadInNewFactory(ratingHeader);
			var rateEntryReloaded = Helper.LoadInNewFactory(rateEntry);

			ShowFormAndAssert(() => controller.ShowEditForm(ratingHeaderReloaded), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);
			ShowFormAndAssert(() => controller.ShowEditForm(rateEntryReloaded), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);
			ShowFormAndAssert(() => controller.ShowTemplateCopyForm(ratingHeaderReloaded), false, DefaultCheckPointForCopy.ErrorMessageForNotAllowed);
			ShowFormAndAssert(() => controller.ShowDeleteForm(rateEntryReloaded), false, DefaultCheckPointForDelete.ErrorMessageForNotAllowed);
			ShowFormAndAssert(() => controller.ShowViewForm(ratingHeaderReloaded), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);
			ShowFormAndAssert(() => controller.ShowViewForm(rateEntryReloaded), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);

			DefaultCheckPointForEdit.IsAllowed = true;
			DefaultCheckPointForCopy.IsAllowed = true;
			DefaultCheckPointForDelete.IsAllowed = true;
			DefaultCheckPointForView.IsAllowed = true;

			ShowFormAndAssert(() => controller.ShowEditForm(ratingHeaderReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowEditForm(rateEntryReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowTemplateCopyForm(ratingHeaderReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowDeleteForm(ratingHeaderReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowDeleteForm(rateEntryReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowViewForm(ratingHeaderReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowViewForm(rateEntryReloaded), true, ZString.Empty);

			ratingHeader.Header.CompanyData.OB_RateSecurityGroup = "";
			Factory.Save();

			// Reloading ratingHeader and rateEntry in new Factory because of security settings' caching.
			ratingHeaderReloaded = Helper.LoadInNewFactory(ratingHeader);
			rateEntryReloaded = Helper.LoadInNewFactory(rateEntry);

			ShowFormAndAssert(() => controller.ShowEditForm(ratingHeaderReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowEditForm(rateEntryReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowTemplateCopyForm(ratingHeaderReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowDeleteForm(ratingHeaderReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowDeleteForm(rateEntryReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowViewForm(ratingHeaderReloaded), true, ZString.Empty);
			ShowFormAndAssert(() => controller.ShowViewForm(rateEntryReloaded), true, ZString.Empty);
		}

		public void TestCopyForm()
		{
			var testRatingHeader = GetRatingHeader();
			Factory.Save();
			DefaultCheckPointForCopy.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var controller = ZControllerFactory.Create(ControllerIDs.IntercompanyTariffs);
			using (IZForm form = controller.ShowTemplateCopyForm(testRatingHeader))
			{
				AssertEquals("Should have returned the correct form", typeof(IntercompanyTariffForm), form.GetType());

				var copiedRatingHeader = (RatingHeader)((IntercompanyTariffForm)form).BusinessEntity;
				AssertEquals("Service Provider field should be empty after cloning", copiedRatingHeader.TH_OH, ZGuid.Empty);

				var allTestEntries = testRatingHeader.AllEntries.ToArray();
				var allCopiedEntries = copiedRatingHeader.AllEntries.ToArray();
				AssertEquals("Rate Entry Count should be greater than zero to ensure that copy function works", true, allTestEntries.Length > 0);
				AssertEquals("Rate Entry Count Mismatch", allTestEntries.Length, allCopiedEntries.Length);

				for (int i = 0; i < allTestEntries.Length; i++)
				{
					AssertEquals($"Rate Lines Count Mismatch", allTestEntries[i].RateLines.Count, allCopiedEntries[i].RateLines.Count);
					AssertEquals($"Origin Mismatch", allTestEntries[i].TI_OriginLRC, allCopiedEntries[i].TI_OriginLRC);
					AssertEquals($"Destination Mismatch", allTestEntries[i].TI_DestinationLRC, allCopiedEntries[i].TI_DestinationLRC);
				}
				Assert(!UnitTestUserNotification.Instance.LastMessage.Contains(DefaultCheckPointForCopy.ErrorMessageForNotAllowed));
			}

			DefaultCheckPointForCopy.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			controller = ZControllerFactory.Create(ControllerIDs.IntercompanyTariffs);

			using (IZForm form = controller.ShowTemplateCopyForm(testRatingHeader))
			{
				AssertNull("Should not return a form", form);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains(DefaultCheckPointForCopy.ErrorMessageForNotAllowed));
			}
		}
	}
}
