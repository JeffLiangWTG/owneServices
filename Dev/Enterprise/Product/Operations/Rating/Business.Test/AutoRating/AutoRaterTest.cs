using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class AutoRaterTest : TestCaseWithFactory
	{
		#region Email Notifications

		public void TestExpiredRatesEmail()
		{
			using (RatingDataRegistry.Instance.ClientRateJustExpiredNotification.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (RatingDataRegistry.Instance.CompanyTariffJustExpiredNotification.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var staff1 = Factory.New<GlbStaff>();
				staff1.GS_Code = "ST1";
				staff1.GS_EmailAddress = "aaa1@bbb.ccc";

				var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
				rate.Header.StaffAssignments.OverallSalesRep = staff1.GS_Code;
				var entry1 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
				entry1.TI_RateStartDate = ZDate.Today.AddMonths(-3);
				entry1.TI_RateEndDate = ZDate.Today.AddDays(-1);

				var tariff1 = Helper.NewCompanyTariff();
				var entry2 = tariff1.AddRateEntry("AIR", "LSE", "AUSYD", "USSFO");
				entry2.TI_RateStartDate = ZDate.Today.AddMonths(-3);
				entry2.TI_RateEndDate = ZDate.Today.AddDays(-1);

				tariff1.Factory.Save();
				Factory.Save();

				var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 300m, 1m, rate.Header);
				var rater = new TestAutoRater();
				using (_Rating.Start(new LoggerDecorator(), isSaveCalledManuallyAfterSession: true))
				{
					rater.AutoRate(Factory, criteria, CostSell.Revenue, new RatingContext());
				}
				AssertEquals(1, rater.Emails.Count);
				AssertNull("emails don't use given Factory when isFactorySaveAllowed is true", rater.Factory);
				AssertEquals("Client Rate Test Client #1 Expired", rater.Emails[0].Subject);

				using (_Rating.Start(new LoggerDecorator(), isSaveCalledManuallyAfterSession: false))
				{
					rater = new TestAutoRater();
					rater.AutoRate(Factory, criteria, CostSell.Revenue, new RatingContext());
				}
				AssertEquals(1, rater.Emails.Count);
				AssertEquals("emails use given Factory when isFactorySaveAllowed is false", Factory, rater.Factory);
				AssertEquals("Client Rate Test Client #1 Expired", rater.Emails[0].Subject);

				criteria = new TestRatingCriteria("AUSYD", "USSFO", FreightMode.LSE, 300m, 1m, rate.Header);
				rater = new TestAutoRater();

				var context = new RatingContext();

				using (_Rating.Start(new LoggerDecorator(), isSaveCalledManuallyAfterSession: true))
				{
					rater.AutoRate(Factory, criteria, CostSell.Revenue, context);
				}
				AssertEquals(1, rater.Emails.Count);
				AssertEquals("Company Tariff Expired", rater.Emails[0].Subject);

				context.SearchForRatesMode = true;
				using (_Rating.Start(new LoggerDecorator(), isSaveCalledManuallyAfterSession: true))
				{
					rater.AutoRate(Factory, criteria, CostSell.Revenue, context);
				}
				AssertEquals("Should be no more expiring emails", 1, rater.Emails.Count);
			}
		}

		public void TestClientRateNotFoundEmail()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.GS_EmailAddress = "aaa1@bbb.ccc";

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 300m, 1m, Helper.NewOrgHeader());
			criteria.LocalClient.StaffAssignments.OverallSalesRep = staff1.GS_Code;

			var rater = new TestAutoRater();
			using (_Rating.Start(new LoggerDecorator(), isSaveCalledManuallyAfterSession: true))
			{
				rater.AutoRate(Factory, criteria, CostSell.Revenue, new RatingContext());
			}
			AssertEquals(1, rater.Emails.Count);
			AssertNull("emails don't use given Factory when isFactorySaveAllowed is true", rater.Factory);
			AssertEquals("Client Rate for Test Client #1 Not Found", rater.Emails[0].Subject);

			rater = new TestAutoRater();
			using (_Rating.Start(new LoggerDecorator(), isSaveCalledManuallyAfterSession: false))
			{
				rater.AutoRate(Factory, criteria, CostSell.Revenue, new RatingContext());
			}
			AssertEquals(1, rater.Emails.Count);
			AssertEquals("emails use given Factory when isFactorySaveAllowed is false", Factory, rater.Factory);
			AssertEquals("Client Rate for Test Client #1 Not Found", rater.Emails[0].Subject);

			criteria.JobDirection = Directions.Import;
			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "CFR"));
			rater = new TestAutoRater();
			using (_Rating.Start(new LoggerDecorator(), isSaveCalledManuallyAfterSession: true))
			{
				rater.AutoRate(Factory, criteria, CostSell.Revenue, new RatingContext());
			}
			AssertEquals("FRT Rate is not applicable so no need to send email", 0, rater.Emails.Count);
		}

		public void TestClientRateNotFoundEmail_NoFreight()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.GS_EmailAddress = "aaa1@bbb.ccc";

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 300m, 1m, Helper.NewOrgHeader());
			criteria.ChargeCodeGroups.Remove(ChargeCodeGroupList.Codes.Freight);
			criteria.LocalClient.StaffAssignments.OverallSalesRep = staff1.GS_Code;

			var rater = new TestAutoRater();
			rater.AutoRate(Factory, criteria, CostSell.Revenue, new RatingContext());
			AssertEquals("Freight charge code group not included so no email about missing freight", 0, rater.Emails.Count);
		}

		public void TestClientRateNotFoundEmail_NotApplicableForCosts()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "HP1";
			staff.GS_EmailAddress = "Harry@Pottermore.com";

			var criteria = new TestRatingCriteria("GBLON", "AUSYD", FreightMode.LSE, 300m, 1m, Helper.NewOrgHeader());
			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Cost, Core.Constants.IncoTerms.ExWorks));
			criteria.LocalClient.StaffAssignments.OverallSalesRep = staff.GS_Code;

			var autoRater = new TestAutoRater();
			autoRater.AutoRate(Factory, criteria, CostSell.Cost, new RatingContext());

			AssertEquals("No point complaining about company tariffs when auto-rating costs", 0, autoRater.Emails.Count);
		}

		#endregion

		#region Implementation

		class TestAutoRater : AutoRater
		{
			public override bool SendEmail(BusinessObjectFactory factory, RatingEmailDef email)
			{
				Emails.Add(email);
				Factory = factory;
				return true;
			}

			public BusinessObjectFactory Factory;

			public IList<RatingEmailDef> Emails
			{
				get { return fEmails ?? (fEmails = new List<RatingEmailDef>()); }
			}

			List<RatingEmailDef> fEmails;
		}

		TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;

		#endregion
	}
}
