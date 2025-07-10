using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Rating.Module.Testing
{
	public abstract class RatingControllerTest<TController, TRatingHeader> : ZControllerBasherTest
		where TController : RatingController<TRatingHeader>, new()
		where TRatingHeader : RatingHeader
	{
		protected abstract override ControllerID GetControllerID();

		protected abstract SecurityCheckpoint DefaultCheckPointForView { get; }
		protected abstract SecurityCheckpoint DefaultCheckPointForEdit { get; }
		protected abstract SecurityCheckpoint DefaultCheckPointForDelete { get; }
		protected abstract SecurityCheckpoint DefaultCheckPointForCopy { get; }
		protected abstract SecurityCheckpoint DefaultCheckPointForNew { get; }
		protected abstract CRMSecurity DefaultCRMSecurity { get; }
		protected abstract RatingHeader GetRatingHeader();
		protected abstract RatingHeader GetGlobalRatingHeader();

		protected override Type GetBusinessObjectType() => typeof(TRatingHeader);

		public virtual void TestSecurityCheckPoints()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);
			var ratingHeader = GetRatingHeader();

			var controller = new TController();
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.CachingEnabled = false;

				DefaultCheckPointForNew.IsAllowed = true;
				DefaultCheckPointForEdit.IsAllowed = false;
				DefaultCheckPointForDelete.IsAllowed = false;
				DefaultCheckPointForView.IsAllowed = false;
				DefaultCheckPointForCopy.IsAllowed = false;
				DefaultCRMSecurity.DisableCRMSecurityForTesting(false);

				ratingHeader.Header.CompanyData.OB_RateSecurityGroup = "ABC";
				Factory.Save();

				ShowFormAndAssert(() => controller.ShowEditForm(ratingHeader), false, setupResult.DeniedSecurity.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowEditForm(rateEntry), false, setupResult.DeniedSecurity.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowDeleteForm(ratingHeader), false, setupResult.DeniedSecurity.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowDeleteForm(rateEntry), false, setupResult.DeniedSecurity.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowViewForm(ratingHeader), false, setupResult.DeniedSecurity.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowViewForm(rateEntry), false, setupResult.DeniedSecurity.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowTemplateCopyForm(ratingHeader), false, setupResult.DeniedSecurity.ErrorMessageForNotAllowed);

				ratingHeader.Header.CompanyData.OB_RateSecurityGroup = "XYZ";
				Factory.Save();

				// Reloading ratingHeader and rateEntry in new Factory because of security settings' caching.
				var ratingHeaderReloaded = Helper.LoadInNewFactory(ratingHeader);
				var rateEntryReloaded = Helper.LoadInNewFactory(rateEntry);

				ShowFormAndAssert(() => controller.ShowEditForm(ratingHeaderReloaded), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowEditForm(rateEntryReloaded), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowDeleteForm(ratingHeaderReloaded), false, DefaultCRMSecurity.EditByStaffNotAssigned.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowDeleteForm(rateEntryReloaded), false, DefaultCheckPointForDelete.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowViewForm(ratingHeaderReloaded), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowViewForm(rateEntryReloaded), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);
				ShowFormAndAssert(() => controller.ShowTemplateCopyForm(ratingHeaderReloaded), false, DefaultCheckPointForCopy.ErrorMessageForNotAllowed);

				DefaultCheckPointForEdit.IsAllowed = true;
				DefaultCRMSecurity.EditByStaffNotAssigned.IsAllowed = true;
				DefaultCheckPointForDelete.IsAllowed = true;
				DefaultCheckPointForView.IsAllowed = true;
				DefaultCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				DefaultCheckPointForCopy.IsAllowed = true;

				ShowFormAndAssert(() => controller.ShowEditForm(ratingHeaderReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowEditForm(rateEntryReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowDeleteForm(ratingHeaderReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowDeleteForm(rateEntryReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowViewForm(ratingHeaderReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowViewForm(rateEntryReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowTemplateCopyForm(ratingHeaderReloaded), true, ZString.Empty);

				ratingHeader.Header.CompanyData.OB_RateSecurityGroup = "";
				Factory.Save();

				// Reloading ratingHeader and rateEntry in new Factory because of security settings' caching.
				ratingHeaderReloaded = Helper.LoadInNewFactory(ratingHeader);
				rateEntryReloaded = Helper.LoadInNewFactory(rateEntry);

				ShowFormAndAssert(() => controller.ShowEditForm(ratingHeaderReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowEditForm(rateEntryReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowDeleteForm(ratingHeaderReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowDeleteForm(rateEntryReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowViewForm(ratingHeaderReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowViewForm(rateEntryReloaded), true, ZString.Empty);
				ShowFormAndAssert(() => controller.ShowTemplateCopyForm(ratingHeaderReloaded), true, ZString.Empty);
			}
		}

		public virtual void TestRatesSecurityCheckPoints()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				DefaultCheckPointForView.IsAllowed = true;
				DefaultCheckPointForEdit.IsAllowed = true;
				DefaultCheckPointForDelete.IsAllowed = true;
				DefaultCRMSecurity?.DisableCRMSecurityForTesting(true);

				var allowedClientRate = Helper.NewClientRate(setupResult.AllowedOrg);
				var deniedClientRate = Helper.NewClientRate(setupResult.DeniedOrg);
				Factory.Save();

				var controller = new TController();

				Assert(controller.GetCheckPointForView(allowedClientRate).IsAllowed);
				Assert(controller.GetCheckPointForEdit(allowedClientRate).IsAllowed);
				Assert(controller.GetCheckPointForDelete(allowedClientRate).IsAllowed);

				Assert(!controller.GetCheckPointForView(deniedClientRate).IsAllowed);
				Assert(!controller.GetCheckPointForEdit(deniedClientRate).IsAllowed);
				Assert(!controller.GetCheckPointForDelete(deniedClientRate).IsAllowed);
			}
		}

		protected void ShowFormAndAssert(Func<IZForm> showForm, bool isAllowed, ZString securityMessageText)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (showForm.Invoke())
			{
				if (isAllowed)
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertMultilineASCIIEquals("UnitTestUserNotification.Instance.LastMessage", securityMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		protected void AssertSecurityCheckPointsForView(SecurityCheckpoint globalCheckPoint)
		{
			var controller = new TController();
			var ratingHeader = GetRatingHeader();
			var actualViewCheckPoint = controller.GetCheckPointForView(ratingHeader);

			AssertEquals(DefaultCheckPointForView, actualViewCheckPoint);

			var globalRatingHeader = GetGlobalRatingHeader();
			actualViewCheckPoint = controller.GetCheckPointForView(globalRatingHeader);

			AssertEquals("Should use Global Rate check point", globalCheckPoint, actualViewCheckPoint);
		}

		protected void AssertSecurityCheckPointsForNew(SecurityCheckpoint globalCheckPoint)
		{
			var controller = new TController();
			var ratingHeader = GetRatingHeader();
			var actualNewCheckPoint = controller.GetCheckPointForNew(ratingHeader);

			AssertEquals(DefaultCheckPointForNew, actualNewCheckPoint);

			var globalRatingHeader = GetGlobalRatingHeader();
			actualNewCheckPoint = controller.GetCheckPointForNew(globalRatingHeader);

			AssertEquals("Should use Global Rate check point", globalCheckPoint, actualNewCheckPoint);
		}

		protected void AssertSecurityCheckPointsForEdit(SecurityCheckpoint globalCheckPoint)
		{
			var controller = new TController();
			var ratingHeader = GetRatingHeader();
			var actualEditCheckPoint = controller.GetCheckPointForEdit(ratingHeader);

			AssertEquals(DefaultCheckPointForEdit, actualEditCheckPoint);

			var globalRatingHeader = GetGlobalRatingHeader();
			actualEditCheckPoint = controller.GetCheckPointForEdit(globalRatingHeader);

			AssertEquals("Should use Global Rate check point", globalCheckPoint, actualEditCheckPoint);
		}

		protected void AssertSecurityCheckPointsForDelete(SecurityCheckpoint globalCheckPoint)
		{
			var controller = new TController();
			var ratingHeader = GetRatingHeader();
			var actualDeleteCheckPoint = controller.GetCheckPointForDelete(ratingHeader);

			AssertEquals(DefaultCheckPointForDelete, actualDeleteCheckPoint);

			var globalRatingHeader = GetGlobalRatingHeader();
			actualDeleteCheckPoint = controller.GetCheckPointForDelete(globalRatingHeader);

			AssertEquals("Should use Global Rate check point", globalCheckPoint, actualDeleteCheckPoint);
		}

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;
	}
}
