using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(CompanyTariffsController))]
	public class CompanyTariffsControllerTest : RatingControllerTest<CompanyTariffsController, CompanyTariff>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlobalRates;
		}

		protected override SecurityCheckpoint DefaultCheckPointForView
		{
			get { return Env.Security.CompanyTariffRatesView; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForEdit
		{
			get { return Env.Security.CompanyTariffRatesEdit; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForDelete
		{
			get { return Env.Security.CompanyTariffRatesDelete; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForCopy
		{
			get { return null; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForNew
		{
			get { return Env.Security.CompanyTariffRatesNew; }
		}

		protected override CRMSecurity DefaultCRMSecurity
		{
			get { return null; }
		}

		protected override RatingHeader GetRatingHeader()
		{
			return Factory.New<CompanyTariff>();
		}

		protected override RatingHeader GetGlobalRatingHeader()
		{
			return Factory.New<GlobalTariff>();
		}

		public void TestCheckPointsForView()
		{
			AssertSecurityCheckPointsForView(Env.Security.GlobalTariffRatesView);
		}

		public void TestCheckPointsForNew()
		{
			AssertSecurityCheckPointsForNew(Env.Security.GlobalTariffRatesNew);
		}

		public void TestCheckPointsForEdit()
		{
			AssertSecurityCheckPointsForEdit(Env.Security.GlobalTariffRatesEdit);
		}

		public void TestCheckPointsForDelete()
		{
			AssertSecurityCheckPointsForDelete(Env.Security.GlobalTariffRatesDelete);
		}

		public override void TestSecurityCheckPoints()
		{
			var ratingHeader = GetRatingHeader();
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR);

			Env.Security.CachingEnabled = false;
			DefaultCheckPointForView.IsAllowed = true;
			DefaultCheckPointForEdit.IsAllowed = true;
			DefaultCheckPointForDelete.IsAllowed = true;
			Factory.Save();

			var controller = new CompanyTariffsController();

			ShowFormAndAssert(() => controller.ShowViewForm(ratingHeader), true, "");
			ShowFormAndAssert(() => controller.ShowViewForm(rateEntry), true, "");

			ShowFormAndAssert(() => controller.ShowEditForm(ratingHeader), true, "");
			ShowFormAndAssert(() => controller.ShowEditForm(rateEntry), true, "");

			ShowFormAndAssert(() => controller.ShowDeleteForm(ratingHeader), true, "");
			ShowFormAndAssert(() => controller.ShowDeleteForm(rateEntry), true, "");

			DefaultCheckPointForView.IsAllowed = false;
			Factory.Save();

			ShowFormAndAssert(() => controller.ShowViewForm(ratingHeader), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);
			ShowFormAndAssert(() => controller.ShowViewForm(rateEntry), false, DefaultCheckPointForView.ErrorMessageForNotAllowed);

			DefaultCheckPointForView.IsAllowed = true;
			DefaultCheckPointForDelete.IsAllowed = false;
			Factory.Save();

			ShowFormAndAssert(() => controller.ShowDeleteForm(ratingHeader), false, DefaultCheckPointForDelete.ErrorMessageForNotAllowed);
			ShowFormAndAssert(() => controller.ShowDeleteForm(rateEntry), false, DefaultCheckPointForDelete.ErrorMessageForNotAllowed);
		}
	}
}
