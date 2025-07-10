using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgMergeBillingCreatorTest : TestCaseWithFactory
	{
		public void TestCreateBillingTransaction_NullParams()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { OrgMergeBillingCreator.CreateBillingTransaction(null, null, OrgMergeBillingConstants.Source.DeduplicationResultsViewerForm); });
			AssertExceptionThrown(typeof(ArgumentNullException), () => { OrgMergeBillingCreator.CreateBillingTransaction(null, OrgMergeBillingConstants.Action.Include, null); });
		}

		public void TestCreateBillingTransactionWithConfidence_NullParams()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { OrgMergeBillingCreator.CreateBillingTransaction(null, null, OrgMergeBillingConstants.Source.DeduplicationResultsViewerForm, "Medium"); });
			AssertExceptionThrown(typeof(ArgumentNullException), () => { OrgMergeBillingCreator.CreateBillingTransaction(null, OrgMergeBillingConstants.Action.Include, null, "Medium"); });
			AssertExceptionThrown(typeof(ArgumentNullException), () => { OrgMergeBillingCreator.CreateBillingTransaction(null, OrgMergeBillingConstants.Action.Include, OrgMergeBillingConstants.Source.DeduplicationResultsViewerForm, null, Array.Empty<string>()); });
		}

		[TestDate(2021, 08, 20, 15, 44, 57, 009)]
		public void TestCreateBillingTransactionWithEmptyAdditionalReference()
		{
			OrgMergeBillingCreator.CreateBillingTransaction(((IDbConnected)Factory).Connection, OrgMergeBillingConstants.Action.Include, OrgMergeBillingConstants.Source.DeduplicationResultsViewerForm);

			var transactionXml = GetTransactionXml(Factory);
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var emptyAdditionalRefTransactionFormat = @"<BillableCount>1</BillableCount>
  <Branch>{0}</Branch>
  <Category>MDM</Category>
  <ClientID>{1}</ClientID>
  <ClientNumber>{2}.{3}</ClientNumber>
  <ClientStaffCode>{4}</ClientStaffCode>
  <PriceItemCode>OMG</PriceItemCode>
  <Reference1>{5}</Reference1>
  <ReportingSource>CW1</ReportingSource>
  <ServiceOccuredUTC>2021-08-20T15:44:57.009Z</ServiceOccuredUTC>
  <Version>1</Version>";
			var expectedMsg = string.Format(CultureInfo.InvariantCulture,
				emptyAdditionalRefTransactionFormat,
				GlbBranch.CurrentBranch.GB_Code,
				GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				registrationKey.SystemId,
				GlbCompany.CurrentCompany.GC_Code,
				GlbStaff.CurrentUser.GS_Code,
				"ACT=INC|SRC=ORG",
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty
			);
			AssertXMLContains(expectedMsg, transactionXml);
		}

		[TestDate(2021, 08, 20, 15, 44, 57, 009)]
		public void TestCreateBillingTransactionWithFullAdditionalReference()
		{
			OrgMergeBillingCreator.CreateBillingTransaction(((IDbConnected)Factory).Connection, OrgMergeBillingConstants.Action.Include, OrgMergeBillingConstants.Source.DeduplicationResultsViewerForm, new[] { "ref1", "ref2", "ref3", "ref4" });

			var transactionXml = GetTransactionXml(Factory);
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var expectedMsg = string.Format(CultureInfo.InvariantCulture,
				transactionXMLFormat,
				GlbBranch.CurrentBranch.GB_Code,
				GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				registrationKey.SystemId,
				GlbCompany.CurrentCompany.GC_Code,
				GlbStaff.CurrentUser.GS_Code,
				"ACT=INC|SRC=ORG",
				"ref1",
				"ref2",
				"ref3",
				"ref4"
			);
			AssertXMLContains(expectedMsg, transactionXml);
		}

		[TestDate(2021, 08, 20, 15, 44, 57, 009)]
		public void TestCreateBillingTransactionWithConfidence()
		{
			var actionsContainsConfidence = new[] { OrgMergeBillingConstants.Action.IgnoreForEveryone, OrgMergeBillingConstants.Action.MergeSuccess, OrgMergeBillingConstants.Action.MergeFailure, OrgMergeBillingConstants.Action.MergeCancel };
			actionsContainsConfidence.ForEach(o =>
			{
				AssertConfidenceInfo(o, true, true);
			});

			var actionsDoesNotContainConfidence = new[] { OrgMergeBillingConstants.Action.Link, OrgMergeBillingConstants.Action.Exclude, OrgMergeBillingConstants.Action.Include, OrgMergeBillingConstants.Action.RemoveIgnores, OrgMergeBillingConstants.Action.Deactivate };
			actionsDoesNotContainConfidence.ForEach(o =>
			{
				AssertConfidenceInfo(o, false, true);
			});
		}

		[TestDate(2021, 08, 20, 15, 44, 57, 009)]
		public void TestCreateBillingTransactionWithEmptyConfidence()
		{
			var actionsContainsConfidence = new[] { OrgMergeBillingConstants.Action.IgnoreForEveryone, OrgMergeBillingConstants.Action.MergeSuccess, OrgMergeBillingConstants.Action.MergeFailure, OrgMergeBillingConstants.Action.MergeCancel };
			actionsContainsConfidence.ForEach(o =>
			{
				AssertConfidenceInfo(o, true, false);
			});

			var actionsDoesNotContainConfidence = new[] { OrgMergeBillingConstants.Action.Link, OrgMergeBillingConstants.Action.Exclude, OrgMergeBillingConstants.Action.Include, OrgMergeBillingConstants.Action.RemoveIgnores, OrgMergeBillingConstants.Action.Deactivate };
			actionsDoesNotContainConfidence.ForEach(o =>
			{
				AssertConfidenceInfo(o, false, false);
			});
		}

		void AssertConfidenceInfo(string o, bool containsConfidence, bool notNullOrEmptyConfidence)
		{
			OrgMergeBillingCreator.CreateBillingTransaction(((IDbConnected)Factory).Connection, o,
				OrgMergeBillingConstants.Source.DeduplicationResultsViewerForm, notNullOrEmptyConfidence ? "Medium" : string.Empty, new[] { "ref1", "ref2", "ref3", "ref4" });

			var transactionXml = GetTransactionXml(Factory);
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var expectedMsg = string.Format(CultureInfo.InvariantCulture,
				transactionXMLFormat,
				GlbBranch.CurrentBranch.GB_Code,
				GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				registrationKey.SystemId,
				GlbCompany.CurrentCompany.GC_Code,
				GlbStaff.CurrentUser.GS_Code,
				containsConfidence && notNullOrEmptyConfidence ? $"ACT={o}|SRC=ORG|CON=Medium" : $"ACT={o}|SRC=ORG",
				"ref1",
				"ref2",
				"ref3",
				"ref4"
			);
			AssertXMLContains(expectedMsg, transactionXml);
		}

		public static string GetTransactionXml(BusinessObjectFactory businessObjectFactory)
		{
			var stmUsageDataCollection = new BillingManager().GetTransactions(businessObjectFactory, 10).ToList();
			AssertEquals(1, stmUsageDataCollection.Count);
			var result = BillingManager.GetTransactionXml(stmUsageDataCollection[0]);
			stmUsageDataCollection.DeleteAll();
			businessObjectFactory.Save();
			return result;
		}

		const string transactionXMLFormat = @"<BillableCount>1</BillableCount>
  <Branch>{0}</Branch>
  <Category>MDM</Category>
  <ClientID>{1}</ClientID>
  <ClientNumber>{2}.{3}</ClientNumber>
  <ClientStaffCode>{4}</ClientStaffCode>
  <PriceItemCode>OMG</PriceItemCode>
  <Reference1>{5}</Reference1>
  <Reference2>{6}</Reference2>
  <Reference3>{7}</Reference3>
  <Reference4>{8}</Reference4>
  <Reference5>{9}</Reference5>
  <ReportingSource>CW1</ReportingSource>
  <ServiceOccuredUTC>2021-08-20T15:44:57.009Z</ServiceOccuredUTC>
  <Version>1</Version>";
	}
}
