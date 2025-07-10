using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCreditorGroup))]
	public class OrgCreditorGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBusinessObjectsWithRelatedEventsCore()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();

			var systemLevelExchangeRateConfigs = new AccExchangeRateConfigurationCollection(Factory);
			systemLevelExchangeRateConfigs.Load();
			systemLevelExchangeRateConfigs.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			company.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.TransportModes.Air, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			creditorGroup.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.Domestic, Core.Constants.TransportModes.Air, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var systemLevelExRateConfig = creditorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);
			AssertNotNull("System level Ex Rate Config", systemLevelExRateConfig);
			var companyLevelExRateConfig = creditorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);
			AssertNotNull("Company level Ex Rate Config", companyLevelExRateConfig);
			var creditorGroupLevelExRateConfig = creditorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.CreditorGroup);
			AssertNotNull("Creditor group level Ex Rate Config", creditorGroupLevelExRateConfig);

			var bizObjsWithRelatedEvents = creditorGroup.BusinessObjectsWithRelatedEvents;

			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain system level Ex Rate config.", systemLevelExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain company level Ex Rate config.", companyLevelExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents must contain creditor group level Ex Rate config.", creditorGroupLevelExRateConfig, bizObjsWithRelatedEvents);
		}

		public void TestAccExchangeRateConfigurations()
		{
			var orgCredGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			AssertNotNull(orgCredGroup.AccExchangeRateConfigurations);
			AssertEquals(AccExRateConfigurationLevelEnum.CreditorGroup, orgCredGroup.AccExchangeRateConfigurations.Level);
		}

		public void TestHumanReadableName()
		{
			var orgCredGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			orgCredGroup.OG_Code = "ABC";
			orgCredGroup.OG_Desc = "Testing";

			AssertEquals("Creditor Group - ABC - Testing", orgCredGroup.HumanReadableName);
		}

		public void TestCascadeDeleteExchangeRateConfigurations()
		{
			var testObjectCreator = new AccountingTestObjectCreator(Factory);

			var systemLevelExchangeRateConfigs = new AccExchangeRateConfigurationCollection(Factory);
			systemLevelExchangeRateConfigs.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var company = Factory.New<GlbCompany>();
			company.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			Factory.Save();
			debtorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			Factory.Save();
			creditorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var anotherCreditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			Factory.Save();
			anotherCreditorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.SellRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var orgHeader = testObjectCreator.CreateOrgHeader("DMO", true, true);
			orgHeader.CompanyData.AccAPExchangeRateConfigurations.SetExRate("AP", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			orgHeader.CompanyData.AccARExchangeRateConfigurations.SetExRate("AR", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			Factory.Save();

			var systemLevelPK = systemLevelExchangeRateConfigs.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.System).PK;
			var companyLevelPK = company.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Company).PK;
			var debtorGroupPK = debtorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.DebtorGroup).PK;
			var creditorGroupPK = creditorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.CreditorGroup).PK;
			var anotherCreditorGroupPK = anotherCreditorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.CreditorGroup).PK;
			var orgHeaderARPK = orgHeader.CompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Debtor).PK;
			var orgHeaderAPPK = orgHeader.CompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Creditor).PK;

			var queryForExRateConfig = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration))
										.AddToFilter(new ZQuery(AccExchangeRateConfigurationViewSchema.PK,
														new List<ZGuid>() { systemLevelPK, companyLevelPK, debtorGroupPK, creditorGroupPK, orgHeaderARPK, orgHeaderAPPK, anotherCreditorGroupPK }));
			var exRateConfigCount = Factory.Load<AccExchangeRateConfiguration>(queryForExRateConfig);
			AssertEquals(7, exRateConfigCount.Length);

			creditorGroup.AccExchangeRateConfigurations.Reload(true);
			creditorGroup.Delete();
			Factory.Save();

			var queryForExRateConfigExceptCreditorLevel = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration))
										.AddToFilter(new ZQuery(AccExchangeRateConfigurationViewSchema.PK,
														new List<ZGuid>() { systemLevelPK, companyLevelPK, debtorGroupPK, orgHeaderARPK, orgHeaderAPPK, anotherCreditorGroupPK }));
			exRateConfigCount = Factory.Load<AccExchangeRateConfiguration>(queryForExRateConfigExceptCreditorLevel);
			AssertEquals(false, Factory.Exists(typeof(AccExchangeRateConfiguration), new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration)).AddToFilter(AccExchangeRateConfigurationViewSchema.PK, creditorGroupPK), false));
			AssertEquals(6, exRateConfigCount.Length);
		}

		public void TestNoAuditLogs()
		{
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, creditorGroup.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				creditorGroup.OG_Desc = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				creditorGroup.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		#region Validation
		public void TestValidateOG_Code()
		{
			CreditorGroup.OG_Code = ZString.Empty;
			Assert("Expecting OG_Code to be empty and have errors.", CreditorGroup.OG_CodeInfo.HasErrors());
			CreditorGroup.OG_Code = new ZString("ETO");
			Assert("OG_Code should be correct, not expecting errors.", !CreditorGroup.OG_CodeInfo.HasNotifications());
		}

		public void TestValidateOG_Desc()
		{
			CreditorGroup.OG_Desc = ZString.Empty;
			Assert("Expecting OG_Desc to be empty and have errors.", CreditorGroup.OG_DescInfo.HasErrors());
			CreditorGroup.OG_Desc = new ZString("asd");
			Assert("Expecting OG_Desc to have too few characters and have errors.", CreditorGroup.OG_DescInfo.HasErrors());
			CreditorGroup.OG_Desc = new ZString("Australia, Dollars");
			Assert("OG_Desc should be correct, not expecting errors.", !CreditorGroup.OG_DescInfo.HasNotifications());
		}

		#endregion

		#region Implementation
		BusinessObjectFactory TestFactory;
		OrgCreditorGroup CreditorGroup;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			CreditorGroup = TestFactory.New(typeof(OrgCreditorGroup)) as OrgCreditorGroup;
		}
		#endregion
	}
}
