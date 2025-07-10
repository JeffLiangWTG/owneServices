using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(DeclarationLockConfigRegistryItem))]
	sealed class DeclarationLockConfigRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<DeclarationLockConfigCollection>
	{
		public void TestCompanyLevelValue()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new DeclarationLockConfigCollection(fallbackLevel, Factory);

			var lockConfig = collection.AddNew();
			lockConfig.DeclarationType = "IMP";

			var eventInfo = lockConfig.EventInfos.AddNew();
			eventInfo.EventType = "ARV";
			eventInfo.EventReference = "REF=HMZ";
			eventInfo.EventSource = "DEC";

			var tabInfo = lockConfig.TabInfos.AddNew();
			tabInfo.TabPage = "DEC";

			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			var companyValue = registryItem.Value;

			AssertEquals("CompanyValue[0].DeclarationType", "IMP", companyValue[0].DeclarationType);

			AssertEquals("CompanyValue[0].EventType", "ARV", companyValue[0].EventInfos[0].EventType);
			AssertEquals("CompanyValue[0].EventReference", "REF=HMZ", companyValue[0].EventInfos[0].EventReference);
			AssertEquals("CompanyValue[0].EventSource", "DEC", companyValue[0].EventInfos[0].EventSource);
			AssertEquals("CompanyValue[0].TabPage", "DEC", companyValue[0].TabInfos[0].TabPage);
		}

		public void TestDeclarationLockForEdit_DefaultValue()
		{
			var spanishCompany = (BusinessObject)Factory.New<IGlbCompany>();
			spanishCompany[GlbCompanySchema.GC_Name] = "Dummy ES Company";
			spanishCompany[GlbCompanySchema.GC_Code] = "ESC";
			spanishCompany[GlbCompanySchema.GC_RN_NKCountryCode] = "ES";

			var latvianCompany = (BusinessObject)Factory.New<IGlbCompany>();
			latvianCompany[GlbCompanySchema.GC_Name] = "Dummy LV Company";
			latvianCompany[GlbCompanySchema.GC_Code] = "LVC";
			latvianCompany[GlbCompanySchema.GC_RN_NKCountryCode] = "LV";

			Factory.Save();

			SqlEventTracker.Instance.Clear();

			CombineAssertions(() =>
			{
				var itemSet = CustomsDataRegistry.Instance;

				var esConfigCollection = itemSet.DeclarationLockForEdit.GetValueWithoutFallback(spanishCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("Default collection for ES company has 2 configs", 2, esConfigCollection.Count);
				AssertContainsExactElementsInAnyOrder("Default collection for ES company has correct declaration types", new List<ZString>() { "IMP", "EXP" }, esConfigCollection.Cast<DeclarationLockConfig>().Select(x => x.DeclarationType).ToList());
				AssertEquals("Default collection for ES company has correct lock mode", false, esConfigCollection.Cast<DeclarationLockConfig>().Any(x => x.LockMode != "ALL"));

				foreach (DeclarationLockConfig esConfig in esConfigCollection)
				{
					AssertEquals("Default collection for ES company has 2 EventInfos " + esConfig.DeclarationType, 2, esConfig.EventInfos.Count);
					AssertEquals("Default collection for ES company has correct Event type, source and entry type for " + esConfig.DeclarationType, false, esConfig.EventInfos.Cast<DeclarationEventLockInfo>().Any(x => x.EventType != "CES" || x.EventSource != "CEN" || x.EntryType != "ALL"));
					AssertContainsExactElementsInAnyOrder("Default collection for ES company has correct Event reference for " + esConfig.DeclarationType, new List<ZString>() { "CLR", "CDA" }, esConfig.EventInfos.Cast<DeclarationEventLockInfo>().Select(x => x.EventReference).ToList());

					AssertEquals("Default collection for ES company has 1 TabInfos " + esConfig.DeclarationType, 1, esConfig.TabInfos.Count);
					AssertContainsExactElementsInAnyOrder("Default collection for ES company has correct TabInfos for " + esConfig.DeclarationType, new List<ZString>() { "ALL" }, esConfig.TabInfos.Cast<DeclarationTabLockInfo>().Select(x => x.TabPage).ToList());
				}

				var lvConfigCollection = itemSet.DeclarationLockForEdit.GetValueWithoutFallback(latvianCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("Default collection for non ES/CH company has no default configs", 0, lvConfigCollection.Count);
			});

			var list = SqlEventTracker.Instance.SqlEventList;
			Assert(@"Should not load any data of ProcessFieldChangeRules when the registry item is trying to load a company data.", list.All(c => !c.Contains(@"FPR_PK") && !c.Contains(@"FPL_PK")));

			SqlEventTracker.Instance.Clear();
		}

		public void TestDeclarationLockForEdit_DefaultValue_CH()
		{
			var swissCompany = (BusinessObject)Factory.New<IGlbCompany>();
			swissCompany[GlbCompanySchema.GC_Name] = "Dummy CH Company";
			swissCompany[GlbCompanySchema.GC_Code] = "CHC";
			swissCompany[GlbCompanySchema.GC_RN_NKCountryCode] = "CH";

			Factory.Save();

			var itemSet = CustomsDataRegistry.Instance;

			var chConfigCollection = itemSet.DeclarationLockForEdit.GetValueWithoutFallback(swissCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			AssertContainsExactElementsInAnyOrder("Default collection for CH company has correct declaration types", new List<ZString>() { "DEP", "ARN", "ULR", "IMP", "EXP", "EDA" }, chConfigCollection.Cast<DeclarationLockConfig>().Select(x => x.DeclarationType).ToList());
			AssertEquals("Default collection for CH company has correct lock mode", false, chConfigCollection.Cast<DeclarationLockConfig>().Any(x => x.LockMode != "ALL"));

			AssertEventInfos("CH", chConfigCollection, "DEP", "NCT", ("MSC", "ACC"), ("CES", "*"));
			AssertEventInfos("CH", chConfigCollection, "ARN", "NCT", ("MSC", "ACC"), ("CES", "*"));
			AssertEventInfos("CH", chConfigCollection, "ULR", "NCT", ("CES", "CL1"), ("CES", "CL3"));
			AssertEventInfos("CH", chConfigCollection, "IMP", "CEN", ("MSC", "*NEW=ACC*"), ("CES", "*"));
			AssertEventInfos("CH", chConfigCollection, "EXP", "CEN", ("MSC", "*NEW=ACC*"), ("CES", "*"));
			AssertEventInfos("CH", chConfigCollection, "EDA", "CEN", ("CES", "*"));

			AssertTabInfos("CH", chConfigCollection, "DEP", "ALL");
			AssertTabInfos("CH", chConfigCollection, "ARN", "ALL");
			AssertTabInfos("CH", chConfigCollection, "ULR", "ULR");
			AssertTabInfos("CH", chConfigCollection, "IMP", "ALL");
			AssertTabInfos("CH", chConfigCollection, "EXP", "ALL");
			AssertTabInfos("CH", chConfigCollection, "EDA", "ALL");
		}

		public void TestDeclarationLockForEdit_DefaultValue_IT()
		{
			var italianCompany = (BusinessObject)Factory.New<IGlbCompany>();
			italianCompany[GlbCompanySchema.GC_Name] = "Dummy IT Company";
			italianCompany[GlbCompanySchema.GC_Code] = "BIT";
			italianCompany[GlbCompanySchema.GC_RN_NKCountryCode] = "IT";

			Factory.Save();

			var itemSet = CustomsDataRegistry.Instance;
			var itConfigCollection = itemSet.DeclarationLockForEdit.GetValueWithoutFallback(italianCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var actualDeclarationTypes = itConfigCollection.Cast<DeclarationLockConfig>().Select(x => x.DeclarationType).ToArray();
			AssertContainsExactElementsInAnyOrder("DeclarationType(s) for IT company", new ZString[] { "DEP", "TST" }, actualDeclarationTypes);

			AssertEventInfos("IT", itConfigCollection, "DEP", "NCT", ("MSC", "SNT"), ("CES", "ACS"), ("CES", "CO3"), ("CES", "MRN"), ("CES", "REL"));
			AssertEventInfos("IT", itConfigCollection, "TST", "TST", ("MSC", "SNT"), ("MSC", "ACK"), ("MSC", "ACS"), ("CES", "TSA"));

			AssertTabInfos("IT", itConfigCollection, "DEP", "ALL");
			AssertTabInfos("IT", itConfigCollection, "TST", "ALL");
		}

		public void TestDeclarationLockForEdit_DefaultValue_SuspendValidation()
		{
			CombineAssertions(() =>
			{
				TestConnection.ExecuteNonQuery(
					"DELETE FROM dbo.StmEvent WHERE SE_Code = @code",
					cmd => cmd.AddParameterBasedOnDbColumn("@code", "CES", StmEventSchema.SE_Code));
				AssertEquals("Precondition", 0, TestConnection.ExecuteScalar(
					"SELECT count(*) FROM dbo.StmEvent WHERE SE_Code = @code",
					cmd => cmd.AddParameterBasedOnDbColumn("@code", "CES", StmEventSchema.SE_Code)));

				var fallbackLevel = new FallbackLevel(Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);
				var collection = (CustomsDataRegistry.Instance.DeclarationLockForEdit.Inner as DeclarationLockConfigRegistryItemImpl).GetDefaultLockConfigForES(fallbackLevel);
				AssertHasNoNotifications("ES", collection);

				collection = (CustomsDataRegistry.Instance.DeclarationLockForEdit.Inner as DeclarationLockConfigRegistryItemImpl).GetDefaultLockConfigForCH(fallbackLevel);
				AssertHasNoNotifications("CH", collection);

				collection = (CustomsDataRegistry.Instance.DeclarationLockForEdit.Inner as DeclarationLockConfigRegistryItemImpl).GetDefaultLockConfigForIT(fallbackLevel);
				AssertHasNoNotifications("IT", collection);
			});
		}

		void AssertHasNoNotifications(string countryCode, DeclarationLockConfigCollection collection)
		{
			foreach (DeclarationLockConfig config in collection)
			{
				Assert($"{countryCode} {config.DeclarationType} DeclarationType Validation suspended and No error", !config.DeclarationTypeInfo.HasNotifications());

				foreach (DeclarationEventLockInfo eventInfo in config.EventInfos)
				{
					Assert($"{countryCode} {config.DeclarationType} {eventInfo.EventType} event type Validation suspended and No error", !eventInfo.EventTypeInfo.HasNotifications());
				}
				foreach (DeclarationTabLockInfo tabInfo in config.TabInfos)
				{
					Assert($"{countryCode} {config.DeclarationType} {tabInfo.TabPage} event type Validation suspended and No error", !tabInfo.TabPageInfo.HasNotifications());
				}
			}
		}

		void AssertEventInfos(ZString country, DeclarationLockConfigCollection configCollection, ZString declarationType, ZString expectedEventSource, params (ZString expectedEventType, ZString expectedEventReference)[] infos)
		{
			const string expectedEntryType = "ALL";
			var config = configCollection.Cast<DeclarationLockConfig>().SingleOrDefault(x => x.DeclarationType == declarationType);
			AssertNotNull($"No default collection exists for declarationType={declarationType}", config);
			foreach (var info in infos)
			{
				AssertEquals($"Default collection for {country} has correct Event type {info.expectedEventType}, reference {info.expectedEventReference}, source {expectedEventSource} and entry type {expectedEntryType} for declarationType {declarationType}", true, config.EventInfos.Cast<DeclarationEventLockInfo>().Any(x => x.EventType == info.expectedEventType && x.EventReference == info.expectedEventReference && x.EventSource == expectedEventSource && x.EntryType == expectedEntryType));
			}
			AssertContainsExactElementsInAnyOrder($"Default collection for {country} company has correct Event reference for " + declarationType, infos.Select(x => x.expectedEventReference).ToArray(), config.EventInfos.Cast<DeclarationEventLockInfo>().Select(x => x.EventReference).ToArray());
		}

		void AssertTabInfos(ZString country, DeclarationLockConfigCollection configCollection, ZString declarationType, params ZString[] tabPages)
		{
			var config = configCollection.Cast<DeclarationLockConfig>().SingleOrDefault(x => x.DeclarationType == declarationType);
			AssertNotNull($"No default collection exists for declarationType={declarationType}", config);
			AssertContainsExactElementsInAnyOrder($"Default collection for {country} company has correct TabInfos for declarationType {declarationType}", tabPages, config.TabInfos.Cast<DeclarationTabLockInfo>().Select(x => x.TabPage));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new DeclarationLockConfigCollection(fallbackLevel, Factory);

			var lockConfig = collection.AddNew();
			lockConfig.DeclarationType = "IMP";

			registryItem = new DeclarationLockConfigRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue);
		}

		protected override StronglyTypedRegistryItem<DeclarationLockConfigCollection, DeclarationLockConfigCollection> GetNewRegistryItem()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new DeclarationLockConfigCollection(fallbackLevel, Factory);

			return new DeclarationLockConfigRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue);
		}

		DeclarationLockConfigRegistryItem registryItem;

		#endregion
	}
}
