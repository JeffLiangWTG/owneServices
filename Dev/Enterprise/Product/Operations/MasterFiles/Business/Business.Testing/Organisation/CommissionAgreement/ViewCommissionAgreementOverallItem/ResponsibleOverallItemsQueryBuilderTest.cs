using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ResponsibleOverallItemsQueryBuilderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_Status = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.Cast<OpportunityStatus>().First(x => x.EffectiveAgreement).Code;

			var uneffectiveAgreement_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			uneffectiveAgreement_XXX_XXX_XXX.CA0_Name = "#1";
			uneffectiveAgreement_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			uneffectiveAgreement_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			uneffectiveAgreement_XXX_XXX_XXX.CA0_EffectiveDate = ZDate.Empty;
			uneffectiveAgreement_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(uneffectiveAgreement_XXX_XXX_XXX, "XXX", "XXX", "XXX");
			AssertEquals("Precondition: IsApproved", true, uneffectiveAgreement_XXX_XXX_XXX.IsApproved);

			var expiredAgreement_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			expiredAgreement_XXX_XXX_XXX.CA0_Name = "#2";
			expiredAgreement_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			expiredAgreement_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			expiredAgreement_XXX_XXX_XXX.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			expiredAgreement_XXX_XXX_XXX.CA0_ExpiredDate = new ZDate(1999, 1, 1);
			expiredAgreement_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(expiredAgreement_XXX_XXX_XXX, "XXX", "XXX", "XXX");
			AssertEquals("Precondition: IsApproved", true, expiredAgreement_XXX_XXX_XXX.IsApproved);

			var reversedAgreement_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			reversedAgreement_XXX_XXX_XXX.CA0_Name = "#3";
			reversedAgreement_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			reversedAgreement_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			reversedAgreement_XXX_XXX_XXX.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			reversedAgreement_XXX_XXX_XXX.Reverse();
			reversedAgreement_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(reversedAgreement_XXX_XXX_XXX, "XXX", "XXX", "XXX");
			AssertEquals("Precondition: IsApproved", true, reversedAgreement_XXX_XXX_XXX.IsApproved);
			AssertEquals("Precondition: IsReversed", true, reversedAgreement_XXX_XXX_XXX.IsReversed);

			var unapprovedAgreement_XXX_XXX_ALL = opportunity.CommissionAgreements.AddNew();
			unapprovedAgreement_XXX_XXX_ALL.CA0_Name = "#4";
			unapprovedAgreement_XXX_XXX_ALL.CA0_OH_Customer = customer.PK;
			unapprovedAgreement_XXX_XXX_ALL.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			unapprovedAgreement_XXX_XXX_ALL.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			unapprovedAgreement_XXX_XXX_ALL.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(unapprovedAgreement_XXX_XXX_ALL, "XXX", "XXX", "ALL");
			AssertEquals("Precondition: IsApproved", false, unapprovedAgreement_XXX_XXX_ALL.IsApproved);

			var agreement_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement_XXX_XXX_XXX.CA0_Name = "#5";
			agreement_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			agreement_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement_XXX_XXX_XXX.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreement_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement_XXX_XXX_XXX, "XXX", "XXX", "XXX");
			AssertEquals("Precondition: IsApproved", true, agreement_XXX_XXX_XXX.IsApproved);

			var agreement_XXX_XXX_YYY = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement_XXX_XXX_YYY.CA0_Name = "#7";
			agreement_XXX_XXX_YYY.CA0_OH_Customer = customer.PK;
			agreement_XXX_XXX_YYY.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement_XXX_XXX_YYY.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreement_XXX_XXX_YYY.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement_XXX_XXX_YYY, "XXX", "XXX", "YYY");
			AssertEquals("Precondition: IsApproved", true, agreement_XXX_XXX_YYY.IsApproved);

			var agreement_XXX_ALL_ALL = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement_XXX_ALL_ALL.CA0_Name = "#8";
			agreement_XXX_ALL_ALL.CA0_OH_Customer = customer.PK;
			agreement_XXX_ALL_ALL.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement_XXX_ALL_ALL.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreement_XXX_ALL_ALL.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement_XXX_ALL_ALL, "XXX", "ALL", "ALL");
			AssertEquals("Precondition: IsApproved", true, agreement_XXX_ALL_ALL.IsApproved);

			var agreement_ALL_ALL_ALL = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement_ALL_ALL_ALL.CA0_Name = "#9";
			agreement_ALL_ALL_ALL.CA0_OH_Customer = customer.PK;
			agreement_ALL_ALL_ALL.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement_ALL_ALL_ALL.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreement_ALL_ALL_ALL.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement_ALL_ALL_ALL, "ALL", "ALL", "ALL");
			AssertEquals("Precondition: IsApproved", true, agreement_ALL_ALL_ALL.IsApproved);

			var agreementForWbpStream_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			agreementForWbpStream_XXX_XXX_XXX.CA0_Name = "#10";
			agreementForWbpStream_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			agreementForWbpStream_XXX_XXX_XXX.CA0_CommissionStream = "WBP";
			agreementForWbpStream_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreementForWbpStream_XXX_XXX_XXX.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreementForWbpStream_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreementForWbpStream_XXX_XXX_XXX, "XXX", "XXX", "XXX");
			AssertEquals("Precondition: IsApproved", true, agreementForWbpStream_XXX_XXX_XXX.IsApproved);

			var agreement_XXX_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement_XXX_XXX_XXX_XXX.CA0_Name = "#11";
			agreement_XXX_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			agreement_XXX_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement_XXX_XXX_XXX_XXX.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreement_XXX_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItemWithConditions(agreement_XXX_XXX_XXX_XXX, "XXX", "XXX", "XXX", "XXX", "", "");
			AssertEquals("Precondition: IsApproved", true, agreement_XXX_XXX_XXX_XXX.IsApproved);

			var agreement_XXX_XXX_XXX_XXX_AUSYD = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement_XXX_XXX_XXX_XXX_AUSYD.CA0_Name = "#12";
			agreement_XXX_XXX_XXX_XXX_AUSYD.CA0_OH_Customer = customer.PK;
			agreement_XXX_XXX_XXX_XXX_AUSYD.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement_XXX_XXX_XXX_XXX_AUSYD.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreement_XXX_XXX_XXX_XXX_AUSYD.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItemWithConditions(agreement_XXX_XXX_XXX_XXX_AUSYD, "XXX", "XXX", "XXX", "XXX", "AUSYD", "");
			AssertEquals("Precondition: IsApproved", true, agreement_XXX_XXX_XXX_XXX_AUSYD.IsApproved);

			Factory.Save();
			{
				var itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(2003, 1, 1), "", "", "");
				var overallItemsXXX_XXX_XXX = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_XXX_XXX,
						agreement_XXX_ALL_ALL,
						agreement_ALL_ALL_ALL,
						agreementForWbpStream_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX_AUSYD
					},
					overallItemsXXX_XXX_XXX.Select(x => x.CommissionAgreement));

				var overallItemsForDefaultStreamXXX_XXX_XXX = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs, ZString.Empty));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_XXX_XXX,
						agreement_XXX_ALL_ALL,
						agreement_ALL_ALL_ALL,
						agreement_XXX_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX_AUSYD
					},
					overallItemsForDefaultStreamXXX_XXX_XXX.Select(x => x.CommissionAgreement));

				var overallItemsForWbpStreamXXX_XXX_XXX = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs, "WBP"));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreementForWbpStream_XXX_XXX_XXX
					},
					overallItemsForWbpStreamXXX_XXX_XXX.Select(x => x.CommissionAgreement));

				itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "ALL", new ZDate(2003, 1, 1), "", "", "");
				var overallItemsXXX_XXX_ALL = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_ALL_ALL,
						agreement_ALL_ALL_ALL
					},
					overallItemsXXX_XXX_ALL.Select(x => x.CommissionAgreement));

				itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "", new ZDate(2003, 1, 1), "", "", "");
				var overallItemsXXX_XXX = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_XXX_XXX,
						agreement_XXX_XXX_YYY,
						agreement_XXX_ALL_ALL,
						agreement_ALL_ALL_ALL,
						agreementForWbpStream_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX_AUSYD
					},
					overallItemsXXX_XXX.Select(x => x.CommissionAgreement));
			}

			{
				OrgCommissionAgreementTestHelper.AddExclusionCommissionAgreementOverallItem(agreement_XXX_ALL_ALL, "XXX", "XXX");
				OrgCommissionAgreementTestHelper.AddExclusionCommissionAgreementOverallItem(agreement_ALL_ALL_ALL, "XXX");
				Factory.Save();

				var itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(2003, 1, 1), "", "", "");
				var overallItemsXXX_XXX_XXX = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_XXX_XXX,
						agreementForWbpStream_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX_AUSYD
					},
					overallItemsXXX_XXX_XXX.Select(x => x.CommissionAgreement));

				itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "ALL", new ZDate(2003, 1, 1), "", "", "");
				var overallItemsXXX_XXX_ALL = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					Enumerable.Empty<OrgCommissionAgreement>(),
					overallItemsXXX_XXX_ALL.Select(x => x.CommissionAgreement));

				itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "", new ZDate(2003, 1, 1), "", "", "");
				var overallItemsXXX_XXX = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_XXX_XXX,
						agreement_XXX_XXX_YYY,
						agreementForWbpStream_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX_AUSYD
					},
					overallItemsXXX_XXX.Select(x => x.CommissionAgreement));
			}

			{
				var itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(2003, 1, 1), "XXX", "", "");
				var overallItemsXXX_XXX_XXX_XXX = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX_AUSYD,
						agreementForWbpStream_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX
					},
					overallItemsXXX_XXX_XXX_XXX.Select(x => x.CommissionAgreement));

				itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(2003, 1, 1), "XXX", "AUSYD", "");
				var overallItemsXXX_XXX_XXX_XXX_AUSYD = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_XXX_XXX_XXX_AUSYD,
						agreement_XXX_XXX_XXX_XXX,
						agreementForWbpStream_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX
					},
					overallItemsXXX_XXX_XXX_XXX_AUSYD.Select(x => x.CommissionAgreement));

				itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(2003, 1, 1), "XXX", "USLAX", "");
				var overallItemsXXX_XXX_XXX_XXX_USLAX = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_XXX_XXX_XXX,
						agreementForWbpStream_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX
					},
					overallItemsXXX_XXX_XXX_XXX_USLAX.Select(x => x.CommissionAgreement));
			}

			{
				// should find no agreements with effective date that is less than or equal to 1999-12-31
				var itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(1999, 12, 31), "XXX", "", "");
				var overallItemsXXX_XXX_XXX_XXX_upto_1999_12_31 = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					System.Array.Empty<OrgCommissionAgreement>(),
					overallItemsXXX_XXX_XXX_XXX_upto_1999_12_31.Select(x => x.CommissionAgreement));

				// should find agreements with effective date that is less than or equal to 2000-01-01
				itemArgs = new CommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(2000, 1, 1), "XXX", "", "");
				var overallItemsXXX_XXX_XXX_XXX_upto_2000_01_01 = Factory.Load<ViewCommissionAgreementOverallItem>(ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs));
				AssertContainsExactElementsInAnyOrder(
					BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
					new[]
					{
						agreement_XXX_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX_XXX_AUSYD,
						agreementForWbpStream_XXX_XXX_XXX,
						agreement_XXX_XXX_XXX
					},
					overallItemsXXX_XXX_XXX_XXX_upto_2000_01_01.Select(x => x.CommissionAgreement));
			}
		}

		public void TestEffectiveDateSql()
		{
			var itemArgs = new CommissionItemArgs(ZGuid.NewZGuid(), "XXX", "XXX", "XXX", new ZDate(2017, 9, 12), "", "", "");
			var query = ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs);
			// SQLComparisonOperator.LessThanOrEqualToDatePartOnly should compare date with the next date using strict 'less than' operation
			AssertContains("VCA_EffectiveDate < '2017-09-13 00:00:00.000'", query.LiteralTextSqlFormatted);
		}
	}
}
