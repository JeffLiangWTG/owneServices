using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	#region ClientRateProcessTasksProviderTest

	[TestedType(typeof(ClientRate))]
	public class ClientRateWorkflowProviderTest : WorkflowProviderTest<ClientRate, ClientRateProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.ClientRateWorkflowDescriptorCode; }
		}

		protected override ClientRate GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObject(factory);

			return result;
		}
	}

	#endregion

	public class ClientRateTest : RatingTestCase
	{
		#region Client Rate Is...

		public void TestClientRateIs()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			AssertEquals(true, clientRate.IsClientRate());
			AssertEquals(false, clientRate.IsClientRateHavingSubsidiaryRelations());

			AssertEquals(false, clientRate.IsTariff());
			AssertEquals(false, clientRate.IsLevelOneTariff());
			AssertEquals(false, clientRate.IsAdditionalTariff());

			AssertEquals(false, clientRate.IsQuote());

			AssertEquals(false, clientRate.IsCosting());
			AssertEquals(false, clientRate.IsWiseCostRate());
			AssertEquals(false, clientRate.IsStandardCostRate());

			client.RelatedManagementSubsidiaryRelations.AddNew();

			AssertEquals(true, clientRate.IsClientRate());
			AssertEquals(true, clientRate.IsClientRateHavingSubsidiaryRelations());
		}

		public void TestClientRateWithoutClient()
		{
			var clientRate = Helper.NewClientRate(null);

			AssertEquals("Should still be considered a client rate even if there hasn't been a client set yet", true, clientRate.IsClientRate());
			AssertEquals(false, clientRate.IsClientRateHavingSubsidiaryRelations());

			AssertEquals(false, clientRate.IsTariff());
			AssertEquals(false, clientRate.IsLevelOneTariff());
			AssertEquals(false, clientRate.IsAdditionalTariff());

			AssertEquals(false, clientRate.IsQuote());

			AssertEquals(false, clientRate.IsCosting());
			AssertEquals(false, clientRate.IsWiseCostRate());
			AssertEquals(false, clientRate.IsStandardCostRate());
		}

		#endregion

		#region CFX Properties
		public void TestCFXProperties()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			AssertEquals(0m, clientRate.TH_AirCFX);
			AssertEquals(0m, clientRate.TH_SeaCFX);
			AssertEquals(0m, clientRate.TH_ExportAirCFX);
			AssertEquals(0m, clientRate.TH_ExportSeaCFX);

			clientRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).AccCFXConfigurations.SetUplifts("ALL", "IMP", "AIR", 6m);
			clientRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).AccCFXConfigurations.SetUplifts("ALL", "IMP", "SEA", 7m);
			clientRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).AccCFXConfigurations.SetUplifts("ALL", "EXP", "AIR", 8m);
			clientRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).AccCFXConfigurations.SetUplifts("ALL", "EXP", "SEA", 9m);

			AssertEquals(6m, clientRate.TH_AirCFX);
			AssertEquals(7m, clientRate.TH_SeaCFX);
			AssertEquals(8m, clientRate.TH_ExportAirCFX);
			AssertEquals(9m, clientRate.TH_ExportSeaCFX);

			clientRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).Delete();

			AssertEquals(0m, clientRate.TH_AirCFX);
			AssertEquals(0m, clientRate.TH_SeaCFX);
			AssertEquals(0m, clientRate.TH_ExportAirCFX);
			AssertEquals(0m, clientRate.TH_ExportSeaCFX);
		}
		#endregion

		#region Copy / Clone

		public void TestCopyRate()
		{
			var originalRate = Helper.NewClientRate(Helper.NewOrgHeader());
			originalRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").AddRateLine("WAR");
			originalRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX").AddRateLine("WAR");

			var originalAirRateEntries = originalRate.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var originalLclRateEntries = originalRate.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection;
			Factory.Save();
			var newRate = (ClientRate)originalRate.CopyIncludingChildren();
			var newAirRateEntries = newRate.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var newLclRateEntries = newRate.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection;

			originalAirRateEntries.Load();
			originalLclRateEntries.Load();
			newAirRateEntries.Load();
			newLclRateEntries.Load();

			AssertEquals("Client PK", ZGuid.Empty, newRate.TH_OH);

			AssertEquals("AIR  Collection Count", originalAirRateEntries.Count, newAirRateEntries.Count);
			AssertEquals("LCL  Collection Count", originalLclRateEntries.Count, newLclRateEntries.Count);

			AssertEquals("AIR Origin", originalAirRateEntries[0].TI_OriginLRC, newAirRateEntries[0].TI_OriginLRC);
			AssertEquals("AIR Destination", originalAirRateEntries[0].TI_DestinationLRC, newAirRateEntries[0].TI_DestinationLRC);

			AssertEquals("LCL Origin", originalLclRateEntries[0].TI_OriginLRC, newLclRateEntries[0].TI_OriginLRC);
			AssertEquals("LCL Destination", originalLclRateEntries[0].TI_DestinationLRC, newLclRateEntries[0].TI_DestinationLRC);

			AssertEquals("AIR Rate Lines Count", originalAirRateEntries[0].RateLines.Count, newAirRateEntries[0].RateLines.Count);
			AssertEquals("LCL Rate Lines Count", originalLclRateEntries[0].RateLines.Count, newLclRateEntries[0].RateLines.Count);
		}

		public void TestCopyPersistentValuesFrom()
		{
			var quote = Factory.New<Quote>();
			var airEntry = quote.AddRateEntry("AIR");

			var clonedRate = Factory.NewWithValidTestData<ClientRate>();
			clonedRate.CopyPersistentValuesFrom(quote);

			Assert("Different Objects", quote.PK != clonedRate.PK);

			var collection = clonedRate.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection;
			AssertEquals("No child Rate Entry", 0, collection.Count);
		}

		public void TestClone()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			var airEntry = rate.AddRateEntry("AIR");

			var clonedRate = (ClientRate)rate.Clone();

			Assert("Different Objects", rate.PK != clonedRate.PK);
			var collection = clonedRate.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection;
			AssertEquals("No child Rate Entry", 0, collection.Count);
		}

		#endregion

		#region Existing Rate Validation

		public void TestExistingRateValidation()
		{
			NewClient.OH_IsConsignor = true;

			var existingRate = Factory.New<ClientRate>();
			existingRate.TH_OH = NewClient.PK;
			Factory.Save();
			existingRate.RunPreSaveValidation();
			AssertEquals("No Error about Existing Rate", false, existingRate.TH_OHInfo.HasErrors());

			var newRate = Factory.New<ClientRate>();
			newRate.TH_OH = NewClient.PK;
			newRate.RunPreSaveValidation();

			AssertEquals("Error about Existing Rate", true, newRate.TH_OHInfo.HasErrors());
		}

		public void TestExistingRateValidationAcrossCompanies()
		{
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			newCompany.GC_RX_NKLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			NewClient.OH_IsConsignor = true;

			var existingRate = Factory.New<ClientRate>();
			existingRate.TH_GC = newCompany.PK; // ie in a different company
			existingRate.TH_OH = NewClient.PK;
			Factory.Save();
			existingRate.RunPreSaveValidation();
			AssertEquals("No Error about Existing Rate", false, existingRate.TH_OHInfo.HasErrors());

			var newRate = Factory.New<ClientRate>();    // in current company
			newRate.TH_OH = NewClient.PK;
			newRate.RunPreSaveValidation();

			AssertEquals("No Error about Existing Rate as 2nd rate is in different company", false, newRate.TH_OHInfo.HasErrors());
		}

		#endregion

		public void TestCodeProperty()
		{
			AssertEquals("Code property should be client code; if the property changes, the RatingHeaderFindBoxListProvider implementation must also be changed.", RatingHeader.Schema.TH_ClientCode, CodePropertyAttribute.CodePropertyNameFromType(typeof(ClientRate)));

			var rate = Factory.New<ClientRate>();
			rate.TH_OH = NewClient.PK;
			AssertEquals("Code property should be returning client code. If the property changes, the RatingHeaderFindBoxListProvider implementation must also be changed.", NewClient.OH_Code, CodePropertyAttribute.CodeFromBusinessObject(rate));
		}

		public void TestUnacceptedQuotes()
		{
			var client = Helper.NewOrgHeader();
			Helper.NewQuote(client);
			Helper.NewQuote(client);
			Factory.Save();

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "NEW";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company1.Branches.Add(branch1);
			Factory.Save();

			var originalCompanyPK = GlbCompany.CurrentCompany.PK;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var rate = Helper.NewClientRate(client);
				AssertEquals(0, rate.UnacceptedQuotesCollection.Count);
				rate.TH_OH = client.PK;
				rate.TH_GC = originalCompanyPK;
				AssertEquals(2, rate.UnacceptedQuotesCollection.Count);
			}
		}

		#region IImportParentRelatedActivityInfoOnNew

		public void TestImportParentRelatedActivityInfoOnNew()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = contact.PK;

			var rate = Factory.New<ClientRate>();
			((IImportParentRelatedActivityInfoOnNew)rate).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());

			AssertEquals(org.PK, rate.TH_OH);
		}

		#endregion
	}

	#region Business Object TestCase

	[TestedType(typeof(ClientRate))]
	public class ClientRateBizObjTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var rate = Factory.New<ClientRate>();
			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			return rate;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rate = factory.New<ClientRate>();
			rate.TH_OH = factory.NewWithValidTestData<OrgHeader>().PK;

			return rate;
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<Costing>(new ZQuery());
		}
	}

	#endregion

	#region ClientRateRelatableActivityTest

	[TestedType(typeof(ClientRate))]
	sealed class ClientRateRelatableActivityTest : RelatableActivityTestCase<ClientRate>
	{
		protected override ClientRate GetNewActivity()
		{
			return Factory.NewWithValidTestData<ClientRate>();
		}
	}

	#endregion
}
