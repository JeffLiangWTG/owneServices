using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompany))]
	class GlbCompanyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeleteWithRelatedStmLink()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var stmLink = Factory.NewWithValidTestData<ZArchitecture.Favorites.StmLink>();
			ErrorReporter.Clear(); //remove a DeveloperNotificationException about not knowing how to fill STL_GC_LogonCompany
			stmLink.STL_LinkType = "FAV";
			stmLink.STL_OC_Contact = ZGuid.Empty;
			stmLink.STL_GS_NKUser = ((GlbStaff)Env.CurrentUser).GS_Code;
			var pk = company.PK;
			stmLink.STL_GC_LogonCompany = pk;
			Factory.Save();
			company.Delete();
			Factory.Save();
			AssertEquals(null, new BusinessObjectFactory().Load<GlbCompany>(pk));
		}

		public void TestDatabaseType()
		{
			var databaseTypes = new[]
			{
				DatabaseTypes.Codes.Test,
				DatabaseTypes.Codes.Demo,
				DatabaseTypes.Codes.Education,
				DatabaseTypes.Codes.Training,
				DatabaseTypes.Codes.WisecloudTrial,
				DatabaseTypes.Codes.Production,
			};

			var company = Factory.NewWithValidTestData<GlbCompany>();

			foreach (var databaseType in databaseTypes)
			{
				ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = databaseType;
				AssertEquals(databaseType, company.DatabaseType);
			}
		}

		public void TestBusinessObjectsWithRelatedEventsCore()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.AccCFXConfigurations.SetUplifts(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.CurrencyCodes.Afghanistan, 5m, 0.1m);

			var companyLevelCFX = company.AccCFXConfigurations.Cast<AccCFXUpliftConfiguration>().FirstOrDefault();
			AssertNotNull("Company level CFX uplift", companyLevelCFX);

			var systemLevelExchangeRateConfigs = new AccExchangeRateConfigurationCollection(Factory);
			systemLevelExchangeRateConfigs.Load();
			systemLevelExchangeRateConfigs.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			company.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var systemLevelExRateConfig = company.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);
			AssertNotNull("System level Ex Rate Config", systemLevelExRateConfig);
			var companyLevelExRateConfig = company.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);
			AssertNotNull("Company level Ex Rate Config", companyLevelExRateConfig);

			var bizObjsWithRelatedEvents = company.BusinessObjectsWithRelatedEvents;

			AssertCollectionContains("BusinessObjectsWithRelatedEvents must contain company level CFX.", companyLevelCFX, bizObjsWithRelatedEvents);
			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain system level Ex Rate config.", systemLevelExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents must contain company level Ex Rate config.", companyLevelExRateConfig, bizObjsWithRelatedEvents);
		}

		public void TestCascadeDeleteExchangeRateConfigurations()
		{
			var testObjectCreator = new AccountingTestObjectCreator(Factory);

			var systemLevelExchangeRateConfigs = new AccExchangeRateConfigurationCollection(Factory);
			systemLevelExchangeRateConfigs.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var company = Factory.New<GlbCompany>();
			company.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var anotherCompany = Factory.New<GlbCompany>();
			anotherCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.SellRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			Factory.Save();
			debtorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			Factory.Save();
			creditorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var orgHeader = testObjectCreator.CreateOrgHeader("DMO", true, true);
			orgHeader.CompanyData.AccAPExchangeRateConfigurations.SetExRate("AP", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			orgHeader.CompanyData.AccARExchangeRateConfigurations.SetExRate("AR", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			Factory.Save();

			var systemLevelPK = systemLevelExchangeRateConfigs.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.System).PK;
			var companyLevelPK = company.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Company).PK;
			var anotherCompanyLevelPK = anotherCompany.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Company).PK;
			var debtorGroupPK = debtorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.DebtorGroup).PK;
			var creditorGroupPK = creditorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.CreditorGroup).PK;
			var orgHeaderARPK = orgHeader.CompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Debtor).PK;
			var orgHeaderAPPK = orgHeader.CompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Creditor).PK;

			var queryForExRateConfig = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration))
										.AddToFilter(new ZQuery(AccExchangeRateConfigurationViewSchema.PK,
														new List<ZGuid>() { systemLevelPK, companyLevelPK, debtorGroupPK, creditorGroupPK, orgHeaderARPK, orgHeaderAPPK, anotherCompanyLevelPK }));
			var exRateConfigCount = Factory.Load<AccExchangeRateConfiguration>(queryForExRateConfig);
			AssertEquals(7, exRateConfigCount.Length);

			company.AccExchangeRateConfigurations.Reload(true);
			company.Delete();
			Factory.Save();

			var queryForExRateConfigExceptCompanyLevel = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration))
										.AddToFilter(new ZQuery(AccExchangeRateConfigurationViewSchema.PK,
														new List<ZGuid>() { systemLevelPK, debtorGroupPK, creditorGroupPK, orgHeaderARPK, orgHeaderAPPK, anotherCompanyLevelPK }));
			exRateConfigCount = Factory.Load<AccExchangeRateConfiguration>(queryForExRateConfigExceptCompanyLevel);
			AssertEquals(false, Factory.Exists(typeof(AccExchangeRateConfiguration), new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration)).AddToFilter(AccExchangeRateConfigurationViewSchema.PK, companyLevelPK), true));
			AssertEquals(6, exRateConfigCount.Length);
		}

		public void TestBranchEditableChildIsValidatedOnCountryChange()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			var config = branch.AccTaxConfigurations.AddNew();
			config.FillWithValidTestData();
			AssertError(company, false);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var companyInNewFactory = newFactory.Load<GlbCompany>(company.PK);
			companyInNewFactory.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Uruguay;
			AssertError(companyInNewFactory, true);

			void AssertError(GlbCompany companyToCheck, bool hasError)
			{
				companyToCheck.RunPreSaveValidation();

				var expectedError = "Error - ETC_RN_NKCountry: Country/Region must be the same as Company country/region.";
				var errors = companyToCheck.NotificationsIncludingChildren.GetUniqueMessageList();
				AssertCollectionContains($"'{expectedError}' error is expected", expectedError, errors, hasError);
			}
		}

		public void TestEditableChild()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			AssertEquals("Should be ChildEditable", true, company.IsRegisteredEditableChildObject(company.AccTaxConfigurations));
		}

		public void TestSupportElectronicInvoicing()
		{
			var eInvoicingMock = new Mock<IGlobalEInvoicingObjectFactory>();
			ObjectFactory.Substitute(eInvoicingMock.Object);

			AssertSupportingElectronicInvoicing(false);
			AssertSupportingElectronicInvoicing(true);

			void AssertSupportingElectronicInvoicing(bool doesCountrySupport)
			{
				eInvoicingMock.Setup(x => x.DoesCountrySupportElectronicInvoicing(It.IsAny<ZString>())).Returns(doesCountrySupport);
				var message = "Country of the company, should" + (doesCountrySupport ? "" : " not") + " support E-Invoicing";

				AssertEquals(message, doesCountrySupport, GlbCompany.CurrentCompany.SupportsElectronicInvoicing);
			}
		}

		public void TestGetNewOrgProxy()
		{
			var factory = new BusinessObjectFactory();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "name";
			company.GC_Address1 = "address1";
			company.GC_Address2 = "address2";
			company.GC_City = "city";
			company.GC_PostCode = "12345";
			company.GC_State = "NSW";
			company.GC_Phone = "123456789";
			company.GC_Fax = "123456";
			company.GC_Email = "gc@email.com";
			company.GC_WebAddress = "gc_webaddress.com";
			company.GC_BusinessRegNo = "123";
			company.GC_BusinessRegNo2 = "456";

			var branch1 = company.Branches.AddNew();
			branch1.Address1 = "otheraddress";
			branch1.GB_RL_NKHomePort = "GBLON";
			var branch2 = company.Branches.AddNew();
			branch2.Address1 = "address1";
			branch2.Address2 = "address2";
			branch2.City = "city";
			branch2.Postcode = "12345";
			branch2.GB_State = "NSW";
			branch2.GB_RL_NKHomePort = "AUSYD";

			var orgProxy = company.GetNewOrgProxy(factory);

			AssertEquals(factory, orgProxy.Factory);
			AssertEquals("name", orgProxy.OH_FullName);
			AssertEquals("address1", orgProxy.MainAddress.OA_Address1);
			AssertEquals("address2", orgProxy.MainAddress.OA_Address2);
			AssertEquals("city", orgProxy.MainAddress.OA_City);
			AssertEquals("12345", orgProxy.MainAddress.OA_PostCode);
			AssertEquals("NSW", orgProxy.MainAddress.OA_State);
			AssertEquals("123456789", orgProxy.MainAddress.OA_Phone);
			AssertEquals("123456", orgProxy.MainAddress.OA_Fax);
			AssertEquals("gc@email.com", orgProxy.MainAddress.OA_Email);
			AssertEquals("gc_webaddress.com", orgProxy.MainWebURL.PU_URL);
			AssertEquals("AUSYD", orgProxy.OH_RL_NKClosestPort);
			AssertEquals(2, orgProxy.CustomsCodes.Count);
			AssertEquals("123", orgProxy.CustomsCodes[0].OK_CustomsRegNo);
			AssertEquals("AU", orgProxy.CustomsCodes[0].OK_RN_NKCodeCountry);
			AssertEquals("ABN", orgProxy.CustomsCodes[0].OK_CodeType);
			AssertEquals("456", orgProxy.CustomsCodes[1].OK_CustomsRegNo);
			AssertEquals("AU", orgProxy.CustomsCodes[1].OK_RN_NKCodeCountry);
			AssertEquals("GCR", orgProxy.CustomsCodes[1].OK_CodeType);
		}

		public void TestActiveBranches()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var activeBranch = company.Branches.AddNew();
			activeBranch.GB_IsActive = true;
			activeBranch.GB_BranchName = "Barrys Branch";

			var inactiveBranch = company.Branches.AddNew();
			inactiveBranch.GB_IsActive = false;

			var icompany = (ICompany)company;
			AssertEquals(2, icompany.Branches.Count());
			AssertEquals(1, icompany.ActiveBranches.Count());
			AssertEquals("Barrys Branch", icompany.ActiveBranches.Single().Name);
		}

		public void TestHasInactiveBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var activeBranch = company.Branches.AddNew();
			activeBranch.GB_IsActive = true;
			AssertEquals(false, company.HasInactiveBranch);

			var inactiveBranch = company.Branches.AddNew();
			inactiveBranch.GB_IsActive = false;
			AssertEquals(true, company.HasInactiveBranch);
		}

		public void TestAccountFeeSettings()
		{
			var company = Factory.New<GlbCompany>();
			company.AccountFeeSettings.OverrideSettings = true;
			company.AccountFeeSettings.AAF_FeeAmount = -25m;
			company.RunPreSaveValidation();
			AssertHasErrorContaining(company.AccountFeeSettings.AAF_FeeAmountInfo, "Account Fee Amount must be a positive number");
		}

		public void TestIsDataVersionsAutoLogged()
		{
			var company = Factory.New<GlbCompany>();
			AssertEquals(true, ((IDataVersionLoggingSupported)company).IsDataVersionsAutoLogged);
		}

		public void TestBusinessRegistrationNumberOneCaption()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Australia should be", "Australian Business Number", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ID"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Indonesia should be", "PPN Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TH"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Thailand should be", "VAT Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IR"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Iran should be", "VAT Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AE"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for UnitedArabEmirates should be", "VAT Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("BH"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Bahrain should be", "VAT Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("KW"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Kuwait should be", "VAT Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("OM"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Oman should be", "VAT Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("QA"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Qatar should be", "VAT Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SA"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for SaudiArabia should be", "VAT Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NC"))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for New Caledonia should be", "TGC Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Malaysia should be", "SER Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Chad))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for Chad should be", "TVA Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.LaoPeoplesDemocraticRepublic))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for LaoPeoplesDemocraticRepublic should be", "VAT Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.BosniaAndHerzegovina))
			{
				AssertEquals("The BusinessRegistrationNumberOneCaption for BosniaAndHerzegovina should be", "PDV Reg No", GlbCompany.CurrentCompany.BusinessRegistrationNumberOneCaption);
			}
		}

		public void TestHasBranchWithThisPK()
		{
			var company1 = Factory.New<GlbCompany>();
			var branch11 = company1.Branches.AddNew();
			var branch12 = company1.Branches.AddNew();
			var company2 = Factory.New<GlbCompany>();
			var branch21 = company2.Branches.AddNew();
			var branch22 = company2.Branches.AddNew();
			Assert(company1.HasBranchWithThisPK(branch11.PK));
			Assert(company1.HasBranchWithThisPK(branch12.PK));
			Assert(!company1.HasBranchWithThisPK(branch21.PK));
			Assert(!company1.HasBranchWithThisPK(branch22.PK));
			Assert(!company2.HasBranchWithThisPK(branch11.PK));
			Assert(!company2.HasBranchWithThisPK(branch12.PK));
			Assert(company2.HasBranchWithThisPK(branch21.PK));
			Assert(company2.HasBranchWithThisPK(branch22.PK));
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestCurrentCompany()
		{
			AssertEquals(Env.CurrentCompany.PK, GlbCompany.CurrentCompany.PK);
		}

		public void TestGetCurrentCompany_InSameFactory()
		{
			AssertEquals(Env.CurrentCompany, GlbCompany.GetCurrentCompany(GlbCompany.CurrentCompany.Factory));
		}

		public void TestGetCurrentCompany_InDifferentFactory()
		{
			var newFactory = Factory.CreateNewFactory();
			var currentCompany = GlbCompany.GetCurrentCompany(newFactory);

			AssertNotEquals(Env.CurrentCompany, currentCompany);
			AssertEquals(Env.CurrentCompany.PK, currentCompany.PK);
		}

		public void TestGetCurrentCompany_WhenCurrentCompanyIsNull()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(GlbCompany.GetCurrentCompany(Factory));
			}
		}

		public void TestGetActiveCompanies()
		{
			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";
			var youngsBranch = youngs.Branches.AddNew();
			youngsBranch.FillWithValidTestData();

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";
			var charlesWellsBranch = charlesWells.Branches.AddNew();
			charlesWellsBranch.FillWithValidTestData();

			var carltonAndUnitedFosters = Factory.New<GlbCompany>();
			carltonAndUnitedFosters.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			carltonAndUnitedFosters.GC_Code = "CUF";
			var carltonAndUnitedFostersBranch = carltonAndUnitedFosters.Branches.AddNew();
			carltonAndUnitedFostersBranch.FillWithValidTestData();

			var allActiveCompanies = Factory.Load<GlbCompany>(
				new ZQuery(GlbCompanySchema.GC_IsActive, true)
				.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode));

			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory.", allActiveCompanies, GlbCompany.GetActiveCompanies(factory: Factory));
			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory.", allActiveCompanies, GlbCompany.GetActiveCompanies(countryCode: "", factory: Factory));
			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory for the UK.", new[] { youngs, charlesWells }, GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom, Factory));
		}

		public void TestGetActiveCompanies_FactoryParameter()
		{
			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";
			var youngsBranch = youngs.Branches.AddNew();
			youngsBranch.FillWithValidTestData();
			AssertEquals("Precondition.", youngs, GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom, Factory).Single());
			AssertEquals("Should use a new factory.", 0, GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom).Length);

			Factory.Save();
			AssertEquals("Should use a new factory.", youngs.PK, GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom).Single().PK);
		}

		public void TestGetActiveCompanies_FiltersInactiveCompanies()
		{
			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";
			var youngsBranch = youngs.Branches.AddNew();
			youngsBranch.FillWithValidTestData();

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";
			var charlesWellsBranch = charlesWells.Branches.AddNew();
			charlesWellsBranch.FillWithValidTestData();

			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory.", new[] { youngs, charlesWells }, GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom, Factory));

			charlesWells.GC_IsActive = false;
			AssertEquals("Should show only active companies.", youngs, GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom, Factory).Single());
		}

		public void TestSaveInActiveCompany()
		{
			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "CC1";
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = globalChargeCode.Lookups.ChargeGroupList[0].Code;
			globalChargeCode.AC_Desc = "global Desc";
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = false;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotNull(Factory.Load<GlbCompany>(company.PK));
			Assert(!Factory.Exists(typeof(AccChargeCode), new ZQuery(AccChargeCodeSchema.AC_GC, company.PK)));

			company.GC_IsActive = true;
			AssertNoExceptionThrown(() => Factory.Save());
			Assert(Factory.Exists(typeof(AccChargeCode), new ZQuery(AccChargeCodeSchema.AC_GC, company.PK)));

			var chargeCodes = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, company.PK));
			chargeCodes.DeleteAll();
			Factory.Save();
			Assert(!Factory.Exists(typeof(AccChargeCode), new ZQuery(AccChargeCodeSchema.AC_GC, company.PK)));

			var oldValue = company.GC_IsGSTCashBasis;
			company.GC_IsGSTCashBasis = !oldValue;
			Factory.Save();
			Assert(Factory.Exists(typeof(AccChargeCode), new ZQuery(AccChargeCodeSchema.AC_GC, company.PK)));
		}

		public void TestGetActiveCompanies_FiltersCompaniesWithoutBranches()
		{
			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";
			var youngsBranch = youngs.Branches.AddNew();
			youngsBranch.FillWithValidTestData();

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";
			var charlesWellsBranch = charlesWells.Branches.AddNew();
			charlesWellsBranch.FillWithValidTestData();

			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory.", new[] { youngs, charlesWells }, GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom, Factory));

			charlesWellsBranch.Delete();
			AssertEquals("Should show only active companies.", youngs, GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom, Factory).Single());
		}

		public void TestGetActiveCompanies_WithPredicates()
		{
			var demCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, GlbCompany.DemoCompanyCode);
			demCompany.GC_City = "Mascot";

			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";
			youngs.GC_City = "Mascot";
			var youngsBranch = youngs.Branches.AddNew();
			youngsBranch.FillWithValidTestData();

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";
			charlesWells.GC_City = "Mascot";
			var charlesWellsBranch = charlesWells.Branches.AddNew();
			charlesWellsBranch.FillWithValidTestData();

			var carltonAndUnitedFosters = Factory.New<GlbCompany>();
			carltonAndUnitedFosters.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			carltonAndUnitedFosters.GC_Code = "CUF";
			carltonAndUnitedFosters.GC_City = "Mascot";
			var carltonAndUnitedFostersBranch = carltonAndUnitedFosters.Branches.AddNew();
			carltonAndUnitedFostersBranch.FillWithValidTestData();

			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory.", new[] { youngs, charlesWells, carltonAndUnitedFosters }, GlbCompany.GetActiveCompanies(c => c.GC_City == "Mascot", factory: Factory));
			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory.", new[] { youngs, charlesWells, carltonAndUnitedFosters }, GlbCompany.GetActiveCompanies(c => c.GC_City == "Mascot", countryCode: "", factory: Factory));
			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory for the UK.", new[] { youngs, charlesWells }, GlbCompany.GetActiveCompanies(c => c.GC_City == "Mascot", Constants.CountryCodes.UnitedKingdom, Factory));
		}

		public void TestGetActiveCompanies_WithPredicates_FactoryParameter()
		{
			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";
			youngs.GC_City = "Alexandria";
			var youngsBranch = youngs.Branches.AddNew();
			youngsBranch.FillWithValidTestData();

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";
			var charlesWellsBranch = charlesWells.Branches.AddNew();
			charlesWellsBranch.FillWithValidTestData();

			AssertEquals("Precondition.", youngs, GlbCompany.GetActiveCompanies(c => c.GC_City == "Alexandria", Constants.CountryCodes.UnitedKingdom, Factory).Single());
			AssertEquals("Should use a new factory.", 0, GlbCompany.GetActiveCompanies(c => c.GC_City == "Alexandria", Constants.CountryCodes.UnitedKingdom).Length);

			Factory.Save();
			AssertEquals("Should use a new factory.", youngs.PK, GlbCompany.GetActiveCompanies(c => c.GC_City == "Alexandria", Constants.CountryCodes.UnitedKingdom).Single().PK);
		}

		public void TestGetActiveCompanies_WithPredicates_FiltersInactiveCompanies()
		{
			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";
			youngs.GC_City = "Alexandria";
			var youngsBranch = youngs.Branches.AddNew();
			youngsBranch.FillWithValidTestData();

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";
			charlesWells.GC_City = "Alexandria";
			var charlesWellsBranch = charlesWells.Branches.AddNew();
			charlesWellsBranch.FillWithValidTestData();

			var carltonAndUnitedFosters = Factory.New<GlbCompany>();
			carltonAndUnitedFosters.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			carltonAndUnitedFosters.GC_Code = "CUF";
			var carltonAndUnitedFostersBranch = carltonAndUnitedFosters.Branches.AddNew();
			carltonAndUnitedFostersBranch.FillWithValidTestData();

			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory.", new[] { youngs, charlesWells }, GlbCompany.GetActiveCompanies(c => c.GC_City == "Alexandria", Constants.CountryCodes.UnitedKingdom, Factory));

			charlesWells.GC_IsActive = false;
			AssertEquals("Should show only active companies.", youngs, GlbCompany.GetActiveCompanies(c => c.GC_City == "Alexandria", Constants.CountryCodes.UnitedKingdom, Factory).Single());
		}

		public void TestGetActiveCompanies_WithPredicates_FiltersCompaniesWithoutBranches()
		{
			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";
			youngs.GC_City = "Alexandria";
			var youngsBranch = youngs.Branches.AddNew();
			youngsBranch.FillWithValidTestData();

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";
			charlesWells.GC_City = "Alexandria";
			var charlesWellsBranch = charlesWells.Branches.AddNew();
			charlesWellsBranch.FillWithValidTestData();

			var carltonAndUnitedFosters = Factory.New<GlbCompany>();
			carltonAndUnitedFosters.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			carltonAndUnitedFosters.GC_Code = "CUF";
			var carltonAndUnitedFostersBranch = carltonAndUnitedFosters.Branches.AddNew();
			carltonAndUnitedFostersBranch.FillWithValidTestData();

			AssertContainsExactElementsInAnyOrder("Should show all active companies in memory.", new[] { youngs, charlesWells }, GlbCompany.GetActiveCompanies(c => c.GC_City == "Alexandria", Constants.CountryCodes.UnitedKingdom, Factory));

			charlesWellsBranch.Delete();
			AssertEquals("Should show only active companies.", youngs, GlbCompany.GetActiveCompanies(c => c.GC_City == "Alexandria", Constants.CountryCodes.UnitedKingdom, Factory).Single());
		}

		[ExpectNoExceptions()]
		public void TestConstructorWithNoEnvironment()
		{
			string originalLoginName = GlbStaff.CurrentUser.GS_LoginName;
			Guid originalBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			Guid originalDepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (Env.SetTemporaryUserContext(null))
			{
				Factory.New<GlbCompany>();
			}
		}

		public void TestAccountingCountry()
		{
			var company = Factory.New<GlbCompany>();

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			AssertEquals(Core.Constants.CountryCodes.NewZealand, company.AccountingCountry);

			company.AccountingCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, company.GC_RN_NKCountryCode);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, company.GC_RX_NKLocalCurrency);

			bool usIsGSTRegistered = company.GC_IsGSTRegistered;
			bool usIsGSTCashBasis = company.GC_IsGSTCashBasis;
			bool usIsReciprocal = company.GC_IsReciprocal;

			company.GC_RN_NKCountryCode = ZString.Empty;
			company.GC_RX_NKLocalCurrency = ZString.Empty;
			company.GC_IsGSTRegistered = !usIsGSTRegistered;
			company.GC_IsGSTCashBasis = !usIsGSTCashBasis;
			company.GC_IsReciprocal = !usIsReciprocal;

			company.AccountingCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, company.GC_RN_NKCountryCode);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, company.GC_RX_NKLocalCurrency);
			AssertEquals(usIsGSTRegistered, company.GC_IsGSTRegistered);
			AssertEquals(usIsGSTCashBasis, company.GC_IsGSTCashBasis);
			AssertEquals(usIsReciprocal, company.GC_IsReciprocal);
		}

		public void TestFirstBranchForUnLoco()
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			RefUNLOCO unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			AssertEquals("Should return current branch if home port matches", "BNE", company.FirstBranchForUnLoco(unloco).GB_Code);

			GlbBranchExtraPorts extraPort1 = GlbBranch.CurrentBranch.ExtraPorts.AddNew();
			extraPort1.GY_RL_NKAdditionalBranchRelatedPort = "USLAX";

			unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			AssertEquals("Should return current branch if related port matches", "BNE", company.FirstBranchForUnLoco(unloco).GB_Code);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			GlbBranch branch = factory2.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "CPT";
			branch.GB_RL_NKHomePort = "ZACPT";
			factory2.Save();

			unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "ZACPT");
			AssertEquals("Should return branch if home port matches", "CPT", company.FirstBranchForUnLoco(unloco).GB_Code);

			GlbBranchExtraPorts extraPort2 = branch.ExtraPorts.AddNew();
			extraPort2.GY_RL_NKAdditionalBranchRelatedPort = "GBLON";
			factory2.Save();

			unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");
			AssertEquals("Should return branch if related port matches", "CPT", company.FirstBranchForUnLoco(unloco).GB_Code);
		}

		public void TestFirstActiveBranchForUnLoco()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Global";
			org.OH_Code = "GLOBAL";

			var currentOrganization1 = Factory.NewWithValidTestData<OrgHeader>();
			currentOrganization1.OH_Code = "GLOCON1";

			var currentOrganization2 = Factory.NewWithValidTestData<OrgHeader>();
			currentOrganization2.OH_Code = "CUSAFO2";

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";
			company.GC_Code = "~";
			company.GC_OH_OrgProxy = org.PK;

			var inactiveBranch = company.Branches.AddNew();
			inactiveBranch.GB_Code = "BR1";
			inactiveBranch.GB_OH_OrgProxy = currentOrganization1.PK;
			inactiveBranch.GB_RL_NKHomePort = "AUBNE";
			inactiveBranch.GB_IsActive = false;

			var activeBranch = company.Branches.AddNew();
			activeBranch.GB_Code = "BR2";
			activeBranch.GB_OH_OrgProxy = currentOrganization2.PK;
			activeBranch.GB_RL_NKHomePort = "AUPAL";
			activeBranch.GB_IsActive = true;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, activeBranch.PK.ToGuid(), Guid.Empty))
			{
				RefUNLOCO unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUPAL");
				AssertEquals("BR2", company.FirstActiveBranchForUnLoco(unloco).GB_Code);

				unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
				AssertNull(company.FirstActiveBranchForUnLoco(unloco));
			}
		}

		public void TestOrgProxy()
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			AssertNull(GlbCompany.CurrentCompany.OrgProxy);

			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			AssertEquals(org.PK, GlbCompany.CurrentCompany.OrgProxy.PK);
		}

		public void TestDeleteCompanyDeletesUnusedStoredDefaults()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var data1ForCompany = Factory.New<StmData>();
			data1ForCompany.SD_DepartmentGuid = company.PK;
			var data2ForCompany = Factory.New<StmData>();
			data2ForCompany.SD_DepartmentGuid = company.PK;
			data2ForCompany.SD_Owner = ZGuid.NewZGuid();

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var data1ForOtherCompany = Factory.New<StmData>();
			data1ForOtherCompany.SD_DepartmentGuid = otherCompany.PK;
			var data2ForOtherCompany = Factory.New<StmData>();
			data2ForOtherCompany.SD_DepartmentGuid = otherCompany.PK;
			data2ForOtherCompany.SD_Owner = ZGuid.NewZGuid();

			Factory.Save();

			company.Delete();

			AssertNull(Factory.Load<StmData>(data1ForCompany.PK));
			AssertNull(Factory.Load<StmData>(data2ForCompany.PK));
			AssertNotNull(Factory.Load<StmData>(data1ForOtherCompany.PK));
			AssertNotNull(Factory.Load<StmData>(data2ForOtherCompany.PK));
		}

		#region Test Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(GlbCompany));
		}

		#endregion

		#region Related BusinessObjects

		public void TestBranches()
		{
			var company = GetNewCompany();
			AssertEquals("Newly added Branches count", 0, company.Branches.Count);
		}

		#endregion

		#region New Properties

		#region GC_IsWHTAccrualBasis

		public void TestGC_IsWHTAccrualBasis()
		{
			var company = GetNewCompany();
			company.GC_IsWHTRegistered = true;
			company.GC_IsWHTCashBasis = true;
			Assert("GC_IsWHTAccrualBasis", !company.GC_IsWHTAccrualBasis);

			company.GC_IsWHTCashBasis = false;
			Assert("GC_IsWHTAccrualBasis", company.GC_IsWHTAccrualBasis);

			company.GC_IsWHTRegistered = false;
			company.GC_IsWHTCashBasis = true;
			Assert("GC_IsWHTAccrualBasis", !company.GC_IsWHTAccrualBasis);

			company.GC_IsWHTCashBasis = false;
			Assert("GC_IsWHTAccrualBasis", !company.GC_IsWHTAccrualBasis);
		}

		#endregion

		#endregion

		#region Overrides

		#region SetDefaultReadonly

		public void TestCountryCodeReadonly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Assert("Country should not be readonly for new created company", !company.GC_RN_NKCountryCodeInfo.ReadOnly);
			}

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Assert("Country should be readonly once saved", company.GC_RN_NKCountryCodeInfo.ReadOnly);
			}

			Assert("Login user is support user", Env.CurrentUser.IsSupportUser);
			Assert("Country should be not readonly if login user is support user", !company.GC_RN_NKCountryCodeInfo.ReadOnly);
		}

		public void TestCountry()
		{
			var company = Factory.New<GlbCompany>();
			SetCountryNaturalKeyAndCombineAssertions(company, Constants.CountryCodes.Angola, "Angola");
			SetCountryNaturalKeyAndCombineAssertions(company, Constants.CountryCodes.Canada, "Canada");
		}

		void SetCountryNaturalKeyAndCombineAssertions(GlbCompany company, string countryCode, string expectedCountryDescription)
		{
			company.GC_RN_NKCountryCode = countryCode;

			CombineAssertions($"When GC_RN_NKCountryCode = {countryCode}", () =>
			{
				AssertEquals("Country.Code", countryCode, company.Country.Code);
				AssertEquals("Country.Description", expectedCountryDescription, company.Country.Description);
			});
		}

		public void TestCountryCodeIsObsoleteAndMacroIgnore()
		{
			CombineAssertions(() =>
			{
				var propertyInfo = typeof(GlbCompany).GetProperty("CountryCode");
				AssertNotNull("MacroIgnoreAttribute", propertyInfo.GetCustomAttribute<MacroIgnoreAttribute>());
				AssertNotNull("ObsoleteAttribute", propertyInfo.GetCustomAttribute<ObsoleteAttribute>());
			});
		}

		public void TestAccountingPropertiesReadonly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			Factory.Save();

			var company = Factory.New<GlbCompany>();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Assert("Local Currency should be always readonly if login user is not support user", company.GC_RX_NKLocalCurrencyInfo.ReadOnly);
				Assert("Is Reciprocal should be always readonly if login user is not support user", company.GC_IsReciprocalInfo.ReadOnly);

				Assert("Is GST Registered should be not readonly if login user is not support user", !company.GC_IsGSTRegisteredInfo.ReadOnly);
				Assert("Is GST Cash Basis should be not readonly if login user is not support user", !company.GC_IsGSTCashBasisInfo.ReadOnly);
				Assert("Is WHT Registered should be not readonly if login user is not support user", !company.GC_IsWHTRegisteredInfo.ReadOnly);
				Assert("Is GST Cash Basis should be not readonly if login user is not support user", !company.GC_IsWHTCashBasisInfo.ReadOnly);
			}

			Assert("Login user is support user", Env.CurrentUser.IsSupportUser);
			Assert("Local Currency should be not readonly if login user is support user", !company.GC_RX_NKLocalCurrencyInfo.ReadOnly);
			Assert("Is Reciprocal should be not readonly if login user is support user", !company.GC_IsReciprocalInfo.ReadOnly);
			Assert("Is GST Registered should be not readonly if login user is support user", !company.GC_IsGSTRegisteredInfo.ReadOnly);
			Assert("Is GST Cash Basis should be not readonly if login user is support user", !company.GC_IsGSTCashBasisInfo.ReadOnly);
			Assert("Is WHT Registered should be not readonly if login user is support user", !company.GC_IsWHTRegisteredInfo.ReadOnly);
			Assert("Is GST Cash Basis should be not readonly if login user is support user", !company.GC_IsWHTCashBasisInfo.ReadOnly);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			GlbCompany.CurrentCompany.Delete();
			AssertEquals("Current Company could not be deleted.", false, GlbCompany.CurrentCompany.IsDeleted);

			var company1 = Factory.New<GlbCompany>();
			company1.GC_Phone_IsManuallyVerified = true;
			company1.GC_Fax_IsManuallyVerified = true;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Phone_IsManuallyVerified = true;

			var acks1 = new GenCustomAddOnRuleAckCollection(company1);
			var acks2 = new GenCustomAddOnRuleAckCollection(company2);

			AssertEquals("Precondition", 2, acks1.Count);
			AssertEquals("Precondition", 1, acks2.Count);

			company1.Delete();

			AssertEquals(0, acks1.Count);
			AssertEquals(1, acks2.Count);
		}

		public void TestDelete_StmNums()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			var stmNum = company.CustomsNumberProvider.CustomsNumbers.AddNew();
			stmNum.SN_MinimumValue = 10L;
			stmNum.SN_Count = 149L;

			company.Delete();

			Assert(stmNum.IsDeleted);
		}

		#endregion

		#endregion

		#region Accounting Related Data

		AccChargeCode GetFRTChargeCodeForCompany(ZGuid companyPK)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_GC, companyPK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
			return Factory.LoadTop1<AccChargeCode>(query);
		}

		public void TestCopyAndDeleteAccountingRelatedData()
		{
			AssertCopyAndDeleteAccountingRelatedData(false);
		}

		public void TestCopyAndDeleteAccountingRelatedData_HasElectronicProcessingChargeCode()
		{
			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "CC1";
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = globalChargeCode.Lookups.ChargeGroupList[0].Code;
			globalChargeCode.AC_Desc = "global Desc";
			Factory.Save();

			ObjectFactory.Get<IAccountingRegistryProvider>().ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();
			Factory.Save();

			AssertCopyAndDeleteAccountingRelatedData(true);
		}
		
		void AssertCopyAndDeleteAccountingRelatedData(bool hasElectronicProcessingChargeCode)
		{
			var demoCompanyWithholdings = Factory.Load<AccWithholding>(new ZQuery(AccWithholdingSchema.AW_GC, DemoCompany.PK));
			foreach (BusinessObject bo in demoCompanyWithholdings)
			{ bo.Delete(); }
			var newAccWithholding = Factory.NewWithValidTestData<AccWithholding>();
			newAccWithholding.AW_GC = DemoCompany.PK;
			newAccWithholding.AW_Code = "WHT1";
			var demFRT = GetFRTChargeCodeForCompany(DemoCompany.PK);
			demFRT.AC_AW_WithholdingTaxRate = newAccWithholding.PK;
			Factory.Save();

			GlbCompany newCompany = SetupNewCompany(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "NEW", false, false);
			Factory.Save();

			int newCompanyWithholdings = Factory.Load<AccWithholding>(new ZQuery(AccWithholdingSchema.AW_GC, newCompany.PK)).Length;
			AssertEquals("New Company should not have wht because not wht registered", 0, newCompanyWithholdings);
			AssertEquals("FRT charge code should not link to wht", null, GetFRTChargeCodeForCompany(newCompany.PK).WithholdingTaxRate);

			newCompany = SetupNewCompany(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "NW2", false, true);
			Factory.Save();

			newCompanyWithholdings = Factory.Load<AccWithholding>(new ZQuery(AccWithholdingSchema.AW_GC, newCompany.PK)).Length;
			AssertEquals("New Company should have wht because wht registered", 1, newCompanyWithholdings);
			AssertEquals("FRT charge code should link to wht", "WHT1", GetFRTChargeCodeForCompany(newCompany.PK).WithholdingTaxRate.AW_Code);

			int demoCompanyChargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, DemoCompany.PK)).Length;
			int newCompanyChargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, newCompany.PK)).Length;
			Assert("precondition there are charge codes in demo company", demoCompanyChargeCode > 0);
			AssertEquals("New Company should have as many ChargeCodes as Demo Company", demoCompanyChargeCode + (hasElectronicProcessingChargeCode ? 1 : 0), newCompanyChargeCode);

			newCompany.Delete();

			AssertEquals("All new Company's Withholdings should be deleted", 0, Factory.Load<AccWithholding>(new ZQuery(AccWithholdingSchema.AW_GC, newCompany.PK)).Length);
			AssertEquals("All new Company's ChargeCodes should be deleted", 0, Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, newCompany.PK)).Length);
		}

		public void TestDeleteCompany_ShouldDeleteTaxConfigurations_WhenTaxConfigurationsExist()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var dummyTaxConfig1 = company.AccTaxConfigurations.AddNew();
			var dummyTaxConfig2 = company.AccTaxConfigurations.AddNew();

			company.Delete();
			Assert(dummyTaxConfig1.IsDeleted);
			Assert(dummyTaxConfig2.IsDeleted);
		}

		public void TestCopyAndDeleteAccountingRelatedDataShouldNotThrowException()
		{
			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "CC1";
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = globalChargeCode.Lookups.ChargeGroupList[0].Code;
			globalChargeCode.AC_Desc = "global Desc";
			Factory.Save();

			AssertNoExceptionThrown(() => SetupNewCompany("AU", "TST", true, true));
		}

		public void TestCopyAndDeleteAccountingRelatedData_WithGlobal_GST()
		{
			TestCopyAndDeleteAccountingRelatedData_WithGlobal(true);
		}

		public void TestCopyAndDeleteAccountingRelatedData_WithGlobal_NoGST()
		{
			TestCopyAndDeleteAccountingRelatedData_WithGlobal(false);
		}

		void TestCopyAndDeleteAccountingRelatedData_WithGlobal(bool gstRegistered)
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode, globalChargeCodeWithNameInDemo;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode, true, false, "", "NOTINDEMO");
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCodeWithNameInDemo, true, false, "", "FRT");

			var newCompany = SetupNewCompany(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "NEW", gstRegistered);
			Factory.Save();

			var filter = new ZQuery(AccChargeCodeSchema.AC_GC, DemoCompany.PK);
			var demoCompanyChargeCodes = Factory.Load<AccChargeCode>(filter).ToList();
			filter = new ZQuery(AccChargeCodeSchema.AC_GC, newCompany.PK);
			var newCompanyChargeCodes = Factory.Load<AccChargeCode>(filter).OrderBy(c => c.AC_Code).ToList();
			Assert("Precondition, demo company has many charge codes", demoCompanyChargeCodes.Count > 2);
			AssertEquals("New Company should get its codes from global and not copy them from demo", 2, newCompanyChargeCodes.Count);

			// Charge code that has AC_CODE within demo
			AssertEquals("Charge code has same code as global", "FRT", newCompanyChargeCodes[0].AC_Code);
			AssertEquals("Charge code is local", false, newCompanyChargeCodes[0].IsGlobal);
			if (gstRegistered)
			{
				AssertEquals("Charge code tax is copied based on demo", "FREEGST", newCompanyChargeCodes[0].GSTRate.AT_Code);
			}
			else
			{
				AssertEquals("Charge code tax is not set", ZGuid.Empty, newCompanyChargeCodes[0].AC_AT_GSTRate);
			}

			// Not in demo charge code
			AssertEquals("Charge code has same code as global", "NOTINDEMO", newCompanyChargeCodes[1].AC_Code);
			AssertEquals("Charge code is local", false, newCompanyChargeCodes[1].IsGlobal);
			if (gstRegistered)
			{
				AssertEquals("Charge code tax is defaulted using global charge code logic", "GST", newCompanyChargeCodes[1].GSTRate.AT_Code);
			}
			else
			{
				AssertEquals("Charge code tax is not set", ZGuid.Empty, newCompanyChargeCodes[1].AC_AT_GSTRate);
			}

			newCompany.Delete();

			filter = new ZQuery(AccChargeCodeSchema.AC_GC, newCompany.PK);
			AssertEquals("New Company 1; All new Company's ChargeCodes should be deleted", 0, Factory.Load<AccChargeCode>(filter).Length);
		}

		protected bool ColumnExists(string tableName, string columnName)
		{
			return ColumnExistsOnSpecificDb(Db.DatabaseName, tableName, columnName);
		}

		bool ColumnExistsOnSpecificDb(string dbName, string tableName, string columnName)
		{
			string sqlText = string.Format(@"
				IF EXISTS (SELECT null FROM sys.databases WHERE name = '{0}')
					SELECT count(*) FROM [{0}].sys.tables tab INNER JOIN [{0}].sys.columns col ON tab.object_id = col.object_id
						WHERE tab.name = '{1}' AND col.name = '{2}'
				ELSE
					SELECT 0",
				dbName, tableName, columnName);
			bool result = ((int)Db.Connection.ExecuteScalar(sqlText) > 0);
			return result;
		}

		public void TestCopyAndDeleteAccountingRelatedDataForEUCompanies()
		{
			ZQuery filter = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Spain);
			RefCountry countrySP = Factory.LoadTop1<RefCountry>(filter);
			filter = new ZQuery(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Spain);
			RefCurrency currencySP = Factory.LoadTop1<RefCurrency>(filter);

			filter = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.VietNam);
			RefCountry countryVN = Factory.LoadTop1<RefCountry>(filter);
			filter = new ZQuery(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.VietNam);
			RefCurrency currencyVN = Factory.LoadTop1<RefCurrency>(filter);

			filter = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			RefCountry countryUK = Factory.LoadTop1<RefCountry>(filter);
			filter = new ZQuery(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedKingdom);
			RefCurrency currencyUK = Factory.LoadTop1<RefCurrency>(filter);

			GlbCompany demoCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, GlbCompany.DemoCompanyCode);
			Assert("Demo Company should not be null", demoCompany != null);

			GlbCompany newCompanySP = Factory.New<GlbCompany>();
			newCompanySP.GC_Code = "SP";
			newCompanySP.GC_RN_NKCountryCode = countrySP.Code;
			newCompanySP.GC_RX_NKLocalCurrency = currencySP.RX_Code;

			GlbCompany newCompanyVN = Factory.New<GlbCompany>();
			newCompanyVN.GC_Code = "VN";
			newCompanyVN.GC_RN_NKCountryCode = countryVN.Code;
			newCompanyVN.GC_RX_NKLocalCurrency = currencyVN.RX_Code;

			GlbCompany newCompanyUK = Factory.New<GlbCompany>();
			newCompanyUK.GC_Code = "UK";
			newCompanyUK.GC_RN_NKCountryCode = countryUK.Code;
			newCompanyUK.GC_RX_NKLocalCurrency = currencyUK.RX_Code;

			Factory.Save();

			newCompanySP.GC_Address1 = "Address";
			newCompanyVN.GC_Address1 = "Address";
			newCompanyUK.GC_Address1 = "Address";
			Factory.Save();

			filter = new ZQuery(AccWithholdingSchema.AW_GC, demoCompany.PK);
			int demoCompanyWithholdings = Factory.Load<AccWithholding>(filter).Length;
			filter = new ZQuery(AccWithholdingSchema.AW_GC, newCompanySP.PK);
			int newCompanyWithholdings = Factory.Load<AccWithholding>(filter).Length;
			AssertEquals("New EU Company should have as many Withholdings as Demo Company", demoCompanyWithholdings, newCompanyWithholdings);

			filter = new ZQuery(AccChargeCodeSchema.AC_GC, demoCompany.PK);
			int demoCompanyChargeCode = Factory.Load<AccChargeCode>(filter).Length;
			filter = new ZQuery(AccChargeCodeSchema.AC_GC, newCompanySP.PK);
			int newCompanyChargeCode = Factory.Load<AccChargeCode>(filter).Length;
			AssertEquals("New EU Company should have as many ChargeCodes as Demo Company", demoCompanyWithholdings, newCompanyWithholdings);

			newCompanySP.Delete();

			filter = new ZQuery(AccWithholdingSchema.AW_GC, newCompanySP.PK);
			AssertEquals("All new Company's Withholdings should be deleted", 0, Factory.Load<AccWithholding>(filter).Length);

			filter = new ZQuery(AccChargeCodeSchema.AC_GC, newCompanySP.PK);
			AssertEquals("All new Company's ChargeCodes should be deleted", 0, Factory.Load<AccChargeCode>(filter).Length);
		}

		public void TestCopyToLocalAccChargeCode_WhenGlobalAccChargeCodeIsInactive()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode, false, false, "", "NOTINDEMO");

			var typeOverride = globalChargeCode.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobDirection = typeOverride.Lookups.DirectionList[0].Code;
			typeOverride.AN_JobType = typeOverride.Lookups.JobTypes[0].Code;
			typeOverride.AN_ChargeType = typeOverride.Lookups.AC_ChargeType_List[0].Code;
			typeOverride.AN_InvoiceType = typeOverride.Lookups.InvoiceTypes[0].Code;
			typeOverride.AN_MarginPercentage = 50;
			typeOverride.AN_MarginPercentage = 50M;

			var revenueRecOverride = globalChargeCode.RevenueRecOverrides.AddNew();
			revenueRecOverride.AE_JobType = "SHP";
			revenueRecOverride.AE_Direction = "IMP";
			revenueRecOverride.AE_Mode = "FAS";
			revenueRecOverride.BrokerCode = "INT";
			revenueRecOverride.AE_RecognitionType = "ARV";

			var glPostingOverride = globalChargeCode.GLPostingOverrides.AddNew();
			glPostingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			glPostingOverride.Y1_AG_ACR = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Cost);
			glPostingOverride.Y1_AG_CST = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Cost);
			glPostingOverride.Y1_AG_REV = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Revenue);
			glPostingOverride.Y1_AG_WIP = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Revenue);

			Factory.Save();

			globalChargeCode.AC_IsActive = false;
			Factory.Save();

			var newCompany = SetupNewCompany(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "NEW", true);

			var filter = new ZQuery(AccChargeCodeSchema.AC_GC, newCompany.PK);
			var newCompanyChargeCodes = new BusinessObjectFactory().Load<AccChargeCode>(filter).Where(x => !x.AC_IsActive).ToList();
			AssertEquals("New company should have an inactive charge code", 1, newCompanyChargeCodes.Count);
			AssertEquals("The inactive charge code is copied from the global one", globalChargeCode.AC_Code, newCompanyChargeCodes[0].AC_Code);
			AssertEquals("The charge type override collection should be copied", 1, newCompanyChargeCodes[0].ChargeTypeOverrides.Count);
			AssertEquals("The revenue recognition override collection should be copied", 1, newCompanyChargeCodes[0].RevenueRecOverrides.Count);
			AssertEquals("The GL posting override collection should be copied", 1, newCompanyChargeCodes[0].GLPostingOverrides.Count);
		}

		public void TestCreateTaxRatesIfGSTIsRegisteredWasChanged()
		{
			AccTaxRateCollection taxRates = new AccTaxRateCollection(Factory, Constants.CountryCodes.NewZealand);
			taxRates.Load();
			taxRates.RemoveAndDeleteAll();
			Factory.Save();
			taxRates.Load();
			AssertEquals("Precondition: there are no tax rates in DB.", 0, taxRates.Count);

			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			PopulateCompanyInfo(newCompany, "TST", "Test Company", "1 Test Way", "", "Testville", "2222", "NSW", Constants.CountryCodes.NewZealand, "123 456 789 01",
				"", Constants.CurrencyCodes.Afghanistan, false, false, true, true, false);
			Factory.Save();

			taxRates.Load();
			AssertEquals("Tax rates are not added in DB if Company is not GST registered.", 0, taxRates.Count);

			newCompany.GC_IsGSTRegistered = true;
			Factory.Save();

			taxRates.Load();
			AssertNotEquals("Tax rates are added in DB if Company is GST registered.", 0, taxRates.Count);

			taxRates.RemoveAndDeleteAll();
			Factory.Save();
			taxRates.Load();
			AssertEquals("Precondition: there are no tax rates in DB.", 0, taxRates.Count);

			newCompany.GC_Address1 = "Another Address";
			Factory.Save();
			taxRates.Load();
			AssertEquals("Tax rates are not been added in DB if some company fields is changed.", 0, taxRates.Count);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompany newCompanyInnewFactory = newFactory.Load<GlbCompany>(newCompany.PK);
			newCompanyInnewFactory.GC_Address1 = "Another Another Address";
			newFactory.Save();
			taxRates.Load();
			AssertEquals("Tax rates are not been added in DB if some company fields is changed.", 0, taxRates.Count);
		}

		public void TestCreateTaxRatesForNewCompany()
		{
			AccTaxRateCollection taxRates = new AccTaxRateCollection(Factory, Constants.CountryCodes.NewZealand);
			taxRates.Load();
			taxRates.RemoveAndDeleteAll();
			Factory.Save();
			taxRates.Load();
			AssertEquals("Precondition: there are no tax rates in DB.", 0, taxRates.Count);

			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			PopulateCompanyInfo(newCompany, "TST", "Test Company", "1 Test Way", "", "Testville", "2222", "NSW", Constants.CountryCodes.NewZealand, "123 456 789 01",
				"", Constants.CurrencyCodes.Afghanistan, false, true, true, true, false);
			Factory.Save();

			taxRates.Load();
			AssertNotEquals("Tax rates are added in DB for brand new Company if it is GST registered.", 0, taxRates.Count);
		}

		AccTaxRate LoadTaxRate(string code, string countryCode)
		{
			ZQuery query = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, countryCode);
			query.AddToFilter(AccTaxRateSchema.AT_Code, code);
			return Factory.LoadTop1<AccTaxRate>(query);
		}

		public void TestItalianCompanyStampDutyConfiguration()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			PopulateCompanyInfo(company, "TST", "Test Company", "1 Test Way", "", "Testville", "2222", "NSW", Constants.CountryCodes.Italy, "123 456 789 01",
				"", Constants.CurrencyCodes.Italy, false, true, true, true, false);
			Assert("company should NOT in DB now", !company.IsInDatabase);
			Factory.Save();

			Guid dICHINTPK = Guid.Empty;
			Guid aRT10PK = Guid.Empty;
			Guid aRT15PK = Guid.Empty;
			Guid aRT7PK = Guid.Empty;
			Guid aRT71PK = Guid.Empty;
			Guid aRT2PK = Guid.Empty;
			Guid aRT9PK = Guid.Empty;
			Guid eSCLUSEPK = Guid.Empty;
			Guid iVAREVBPK = Guid.Empty;
			Guid fREEIVAB = Guid.Empty;

			AccTaxRate taxRate = LoadTaxRate("DICH.INT", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			dICHINTPK = taxRate.PK.ToGuid();
			taxRate = LoadTaxRate("ART10", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			aRT10PK = taxRate.PK.ToGuid();
			taxRate = LoadTaxRate("ART15", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			aRT15PK = taxRate.PK.ToGuid();
			taxRate = LoadTaxRate("ART7", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			aRT7PK = taxRate.PK.ToGuid();
			taxRate = LoadTaxRate("ART71", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			aRT71PK = taxRate.PK.ToGuid();
			taxRate = LoadTaxRate("ART2", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			aRT2PK = taxRate.PK.ToGuid();
			taxRate = LoadTaxRate("ART9", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			aRT9PK = taxRate.PK.ToGuid();
			taxRate = LoadTaxRate("ESCLUSEB", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			eSCLUSEPK = taxRate.PK.ToGuid();
			taxRate = LoadTaxRate("IVAREVB", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			iVAREVBPK = taxRate.PK.ToGuid();
			taxRate = LoadTaxRate("FREEIVAB", Constants.CountryCodes.Italy);
			AssertNotNull("Tax Rate should not be null", taxRate);
			fREEIVAB = taxRate.PK.ToGuid();

			string taxIDs = dICHINTPK.ToString() + "," + aRT10PK.ToString() + "," + aRT15PK.ToString() + "," + aRT7PK.ToString() + "," + aRT71PK.ToString() + "," + aRT2PK.ToString() + "," + aRT9PK.ToString() + "," + eSCLUSEPK.ToString() + "," + iVAREVBPK.ToString() + "," + fREEIVAB.ToString();

			AssertEquals("TaxIDs", taxIDs, ObjectFactory.Get<IAccounting>().TaxIDsAttractingStampDuty(company.PK.ToGuid()));
			AssertEquals("StampDutyFixedAmount", 2.0m, ObjectFactory.Get<IAccounting>().StampDutyFixedAmount(company.PK.ToGuid()));
			AssertEquals("StampDutyThreshold", 77.47m, ObjectFactory.Get<IAccounting>().StampDutyThreshold(company.PK.ToGuid()));

			Assert("company should in DB now", company.IsInDatabase);
			ObjectFactory.Get<IAccounting>().SetStampDutyConfiguration(company.PK.ToGuid(), dICHINTPK.ToString(), 5.0m, 100.0m);

			company.GC_IsGSTRegistered = true;
			Factory.Save();

			AssertEquals("TaxIDs", dICHINTPK.ToString(), ObjectFactory.Get<IAccounting>().TaxIDsAttractingStampDuty(company.PK.ToGuid()));
			AssertEquals("StampDutyFixedAmount", 5.0m, ObjectFactory.Get<IAccounting>().StampDutyFixedAmount(company.PK.ToGuid()));
			AssertEquals("StampDutyThreshold", 100.0m, ObjectFactory.Get<IAccounting>().StampDutyThreshold(company.PK.ToGuid()));
		}

		public void TestSignatureCredentials()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.SetCountry(Constants.CountryCodes.Australia);
				AssertNotNull(nameof(company1.SignatureCredentials), company1.SignatureCredentials);
				AssertEquals(nameof(company1.SignatureCredentials.IsEditAllowed), false, company1.SignatureCredentials.IsEditAllowed);
				AssertEquals(nameof(company1.SignatureCredentials.IsLoaded), false, company1.SignatureCredentials.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company1.IsRegisteredEditableChildObject(company1.SignatureCredentials));

				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.SetCountry(Constants.CountryCodes.Turkey);
				AssertEquals(nameof(company2.SignatureCredentials.IsEditAllowed), false, company2.SignatureCredentials.IsEditAllowed);
				AssertEquals(nameof(company2.SignatureCredentials.IsLoaded), false, company2.SignatureCredentials.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company2.IsRegisteredEditableChildObject(company2.SignatureCredentials));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.SetCountry(Constants.CountryCodes.Australia);
				AssertNotNull(nameof(company1.SignatureCredentials), company1.SignatureCredentials);
				AssertEquals(nameof(company1.SignatureCredentials.IsEditAllowed), false, company1.SignatureCredentials.IsEditAllowed);
				AssertEquals(nameof(company1.SignatureCredentials.IsLoaded), false, company1.SignatureCredentials.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company1.IsRegisteredEditableChildObject(company1.SignatureCredentials));

				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.SetCountry(Constants.CountryCodes.Turkey);
				AssertEquals(nameof(company2.SignatureCredentials.IsEditAllowed), true, company2.SignatureCredentials.IsEditAllowed);
				AssertEquals(nameof(company2.SignatureCredentials.IsLoaded), true, company2.SignatureCredentials.IsLoaded);
				AssertEquals(nameof(company2.SignatureCredentials.AllowNew), true, company2.SignatureCredentials.AllowNew);
				AssertEquals("Should be ChildEditable", true, company2.IsRegisteredEditableChildObject(company2.SignatureCredentials));

				var signatureCredential = company2.SignatureCredentials.AddNew();
				AssertEquals(nameof(signatureCredential.GP_GC), company2.PK, signatureCredential.GP_GC);
				AssertEquals(nameof(company2.SignatureCredentials.AllowNew), false, company2.SignatureCredentials.AllowNew);
			}
		}

		public void TestHungaryEInvoicingCredentials()
		{
			var company = GetNewCompany();
			company.GC_Code = "XAU";
			AssertNotEquals("Precondition: not Hungary country", Constants.CountryCodes.Hungary, company.GC_RN_NKCountryCode);
			AssertNull("HungaryEInvoicingCredentials should be null when not Hungary company", company.HungaryEInvoicingCredentials);

			var huCompany = GetNewCompany();
			huCompany.GC_Code = "XHU";
			huCompany.SetCountry(Constants.CountryCodes.Hungary);
			AssertNotNull("HungaryEInvoicingCredentials should be lazy loaded / created when in Hungary company", huCompany.HungaryEInvoicingCredentials);

			huCompany.Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedHuCompany = newFactory.Load<GlbCompany>(huCompany.PK);
			AssertNotNull("HungaryEInvoicingCredentials should be loaded", loadedHuCompany.HungaryEInvoicingCredentials);
			AssertEquals("HungaryEInvoicingCredentials should be reloaded after save", huCompany.HungaryEInvoicingCredentials.PK, loadedHuCompany.HungaryEInvoicingCredentials.PK);
		}

		public void TestPhilippinesEInvoicingCredentials()
		{
			var company = GetNewCompany();
			company.GC_Code = "XAU";
			AssertNotEquals("Precondition: not Philippines", Constants.CountryCodes.Philippines, company.GC_RN_NKCountryCode);
			AssertNull("PhilippinesEInvoicingCredentials should be null when not Philippines company", company.PhilippinesEInvoicingCredentials);

			var philippinesCompany = GetNewCompany();
			philippinesCompany.GC_Code = "XPH";
			philippinesCompany.SetCountry(Constants.CountryCodes.Philippines);
			AssertNotNull("PhilippinesEInvoicingCredentials should be lazy loaded / created when in Philippines company", philippinesCompany.PhilippinesEInvoicingCredentials);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var reloadedCredentials = newFactory.Load<GlbCompany>(philippinesCompany.PK)?.PhilippinesEInvoicingCredentials;
			AssertNotNull("PhilippinesEInvoicingCredentials should be loaded", reloadedCredentials);
			AssertEquals("PhilippinesEInvoicingCredentials should be reloaded after save", philippinesCompany.PhilippinesEInvoicingCredentials.ClientCredential.PK, reloadedCredentials.ClientCredential.PK);
			AssertEquals("PhilippinesEInvoicingCredentials should be reloaded after save", philippinesCompany.PhilippinesEInvoicingCredentials.UserCredential.PK, reloadedCredentials.UserCredential.PK);
		}

		public void TestTemplateFiles()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.SetCountry(Constants.CountryCodes.Australia);
				AssertNotNull(nameof(company1.TemplateFiles), company1.TemplateFiles);
				AssertEquals(nameof(company1.IsTemplateFileConfigurationsEnabled), false, company1.IsTemplateFileConfigurationsEnabled);
				AssertEquals(nameof(company1.TemplateFiles.IsLoaded), true, company1.TemplateFiles.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company1.IsRegisteredEditableChildObject(company1.TemplateFiles));

				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.SetCountry(Constants.CountryCodes.Turkey);
				AssertEquals(nameof(company2.IsTemplateFileConfigurationsEnabled), false, company2.IsTemplateFileConfigurationsEnabled);
				AssertEquals(nameof(company2.TemplateFiles.IsLoaded), true, company2.TemplateFiles.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company2.IsRegisteredEditableChildObject(company2.TemplateFiles));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.SetCountry(Constants.CountryCodes.Australia);
				AssertNotNull(nameof(company1.TemplateFiles), company1.TemplateFiles);
				AssertEquals(nameof(company1.IsTemplateFileConfigurationsEnabled), false, company1.IsTemplateFileConfigurationsEnabled);
				AssertEquals(nameof(company1.TemplateFiles.IsLoaded), true, company1.TemplateFiles.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company1.IsRegisteredEditableChildObject(company1.TemplateFiles));

				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.SetCountry(Constants.CountryCodes.Turkey);
				AssertEquals(nameof(company2.IsTemplateFileConfigurationsEnabled), true, company2.IsTemplateFileConfigurationsEnabled);
				AssertEquals(nameof(company2.TemplateFiles.IsLoaded), true, company2.TemplateFiles.IsLoaded);
				AssertEquals(nameof(company2.TemplateFiles.AllowNew), false, company2.TemplateFiles.AllowNew);
				AssertEquals("Should be ChildEditable", true, company2.IsRegisteredEditableChildObject(company2.TemplateFiles));
			}
		}

		public void TestEInvoicingTemplateFileConfigurations()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.SetCountry(Constants.CountryCodes.Australia);
				AssertNotNull(nameof(company1.EInvoicingTemplateFileConfigurations), company1.EInvoicingTemplateFileConfigurations);
				AssertEquals(nameof(company1.IsTemplateFileConfigurationsEnabled), false, company1.IsTemplateFileConfigurationsEnabled);
				AssertEquals(nameof(company1.EInvoicingTemplateFileConfigurations.IsLoaded), true, company1.EInvoicingTemplateFileConfigurations.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company1.IsRegisteredEditableChildObject(company1.EInvoicingTemplateFileConfigurations));

				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.SetCountry(Constants.CountryCodes.Turkey);
				AssertEquals(nameof(company2.IsTemplateFileConfigurationsEnabled), false, company2.IsTemplateFileConfigurationsEnabled);
				AssertEquals(nameof(company2.EInvoicingTemplateFileConfigurations.IsLoaded), true, company2.EInvoicingTemplateFileConfigurations.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company2.IsRegisteredEditableChildObject(company2.EInvoicingTemplateFileConfigurations));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.SetCountry(Constants.CountryCodes.Australia);
				AssertNotNull(nameof(company1.EInvoicingTemplateFileConfigurations), company1.EInvoicingTemplateFileConfigurations);
				AssertEquals(nameof(company1.IsTemplateFileConfigurationsEnabled), false, company1.IsTemplateFileConfigurationsEnabled);
				AssertEquals(nameof(company1.EInvoicingTemplateFileConfigurations.IsLoaded), true, company1.EInvoicingTemplateFileConfigurations.IsLoaded);
				AssertEquals("Should be ChildEditable", true, company1.IsRegisteredEditableChildObject(company1.EInvoicingTemplateFileConfigurations));

				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.SetCountry(Constants.CountryCodes.Turkey);
				AssertEquals(nameof(company2.IsTemplateFileConfigurationsEnabled), true, company2.IsTemplateFileConfigurationsEnabled);
				AssertEquals(nameof(company2.EInvoicingTemplateFileConfigurations.IsLoaded), true, company2.EInvoicingTemplateFileConfigurations.IsLoaded);
				AssertEquals(nameof(company2.EInvoicingTemplateFileConfigurations.AllowNew), true, company2.EInvoicingTemplateFileConfigurations.AllowNew);
				AssertEquals("Should be ChildEditable", true, company2.IsRegisteredEditableChildObject(company2.EInvoicingTemplateFileConfigurations));
			}
		}

		public void TestEmptyCountryCodeNotCausingErrorForNonTurkeyCompanies()
		{
			var companyWithNoCountryCode = Factory.NewWithValidTestData<GlbCompany>();
			companyWithNoCountryCode.GC_RN_NKCountryCode = string.Empty;
			AssertEquals(null, companyWithNoCountryCode.Country);
			AssertNullOrEmpty(companyWithNoCountryCode.GC_RN_NKCountryCode);
			AssertNoExceptionThrown(() => _ = companyWithNoCountryCode.IsTemplateFileConfigurationsEnabled);
			AssertEquals(nameof(companyWithNoCountryCode.IsTemplateFileConfigurationsEnabled), false, companyWithNoCountryCode.IsTemplateFileConfigurationsEnabled);
		}

		#endregion

		#region IDocManagerSupport

		public void TestDocManagerCode()
		{
			var company = GetNewCompany();
			AssertEquals("Code should be CPY. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "CPY", ((IDocManagerSupport)company).DocManagerInfo.DocManagerCode);
		}

		#endregion

		public void TestCheckClientDll()
		{
			Env.Registry.ExpectedClientDLL = null;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			AssertEquals("ExpectedClientDll", null, Env.Registry.ExpectedClientDLL);
		}

		public void TestLicenceKeyIdentifier()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			GlbCompany company = Factory.New<GlbCompany>();
			registrationKey.EnterpriseCodeForTest = "ENT";
			company.GC_Code = "CMP";
			registrationKey.ServerCodeForTest = "SVR";
			AssertEquals("9 digit license key", "ENTCMPSVR", company.LicenceKeyIdentifier);
		}

		public void TestIAddressDetails()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = ZGuid.Empty;

			IAddressDetails addressDetails = company;

			company.GC_Name = "CargoWise";
			AssertEquals("Company Name", "CargoWise", addressDetails.CompanyName);

			company.GC_Address1 = "Unit 3a O'Riodan St";
			AssertEquals("Address1", company.GC_Address1, addressDetails.AddressLine1);

			company.GC_Address2 = "(back of Eat Me restuarant)";
			AssertEquals("AddressLine2", company.GC_Address2, addressDetails.AddressLine2);

			company.GC_City = "Alexandria";
			AssertEquals("City", company.GC_City, addressDetails.City);

			company.GC_Email = "support@cargowise.com";
			AssertEquals("Email", company.GC_Email, addressDetails.Email);

			AssertEquals("ContactName", "", addressDetails.ContactName);

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			AssertEquals("Country", "CN", addressDetails.Country);

			company.GC_Fax = "02 88202200";
			AssertEquals("Fax", company.GC_Fax, addressDetails.Fax);

			company.GC_Phone = "02 88202201";
			AssertEquals("Phone", company.GC_Phone, addressDetails.Phone);

			company.GC_State = "NSW";
			AssertEquals("State", "NSW", addressDetails.State);

			company.GC_PostCode = "2015";
			AssertEquals("PostCode", "2015", addressDetails.PostCode);

			OrgHeader proxy = Factory.New<OrgHeader>();
			company.GC_OH_OrgProxy = proxy.PK;

			proxy.OH_FullName = "CargoWise Pty Ltd";
			AssertEquals("Company Name", "CargoWise Pty Ltd", addressDetails.CompanyName);

			OrgAddress address = proxy.MainAddress;
			address.OA_Address1 = "123 Main st";
			AssertEquals("Address1", "123 Main st", addressDetails.AddressLine1);

			address.OA_Address2 = "(Address2)";
			AssertEquals("AddressLine2", "(Address2)", addressDetails.AddressLine2);

			address.OA_City = "Chicago";
			AssertEquals("City", "Chicago", addressDetails.City);

			address.OA_Email = "support2@cargowise.com";
			AssertEquals("Email", "support2@cargowise.com", addressDetails.Email);

			OrgContact contact = proxy.Contacts.AddNew();
			contact.OC_ContactName = "Henry Ye";
			AssertEquals("ContactName", "Henry Ye", addressDetails.ContactName);

			address.OA_Fax = "02 88202202";
			AssertEquals("Fax", "02 88202202", addressDetails.Fax);

			address.OA_Phone = "02 88202203";
			AssertEquals("Phone", "02 88202203", addressDetails.Phone);

			address.OA_State = "IL";
			AssertEquals("State", "IL", addressDetails.State);

			address.OA_PostCode = "2016";
			AssertEquals("PostCode", "2016", addressDetails.PostCode);

			address.OA_RL_NKRelatedPortCode = "USCHI";
			AssertEquals("Country", "US", addressDetails.Country);
		}

		public void TestFirstActiveBranchPK()
		{
			AssertEquals(Guid.Empty, GlbCompany.FirstActiveBranchPK(ZString.Empty));

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "XYZ";
			AssertEquals(Guid.Empty, GlbCompany.FirstActiveBranchPK("XYZ", Factory));
			var branch = company.Branches.AddNew();
			AssertEquals(branch.PK.ToGuid(), GlbCompany.FirstActiveBranchPK("XYZ", Factory));
			branch.GB_IsActive = false;
			AssertEquals(Guid.Empty, GlbCompany.FirstActiveBranchPK("XYZ", Factory));
		}

		public void TestFirstActiveBranch()
		{
			var company = Factory.New<GlbCompany>();
			AssertEquals("First Active Branch should be null", null, company.FirstActiveBranch);

			var branch1 = company.Branches.AddNew();
			AssertEquals("First Active Branch should be branch1", branch1.PK, company.FirstActiveBranch.PK);

			var branch2 = company.Branches.AddNew();
			Assert("First Active Branch should be one of branch1 and branch2", company.Branches.Contains(company.FirstActiveBranch));

			branch1.GB_IsActive = false;
			AssertEquals("First Active Branch should be branch2", branch2.PK, company.FirstActiveBranch.PK);

			branch2.GB_IsActive = false;
			AssertEquals("First Active Branch should be null", null, company.FirstActiveBranch);
		}

		public void TestHasOnlyOneActiveBranch()
		{
			var company = Factory.New<GlbCompany>();
			AssertEquals("company.HasOnlyOneActiveBranch()", false, company.HasOnlyOneActiveBranch());

			var branch1 = company.Branches.AddNew();
			AssertEquals("company.HasOnlyOneActiveBranch()", true, company.HasOnlyOneActiveBranch());

			var branch2 = company.Branches.AddNew();
			AssertEquals("company.HasOnlyOneActiveBranch()", false, company.HasOnlyOneActiveBranch());
		}

		public void TestIsBranchActive()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			AssertEquals("company.IsBranchActive(branch.GB_Code)", true, company.IsBranchActive(branch.GB_Code));

			branch.GB_IsActive = false;
			AssertEquals("company.IsBranchActive(branch.GB_Code)", false, company.IsBranchActive(branch.GB_Code));
		}

		public void TestRegistrationKeyIsLazyLoaded()
		{
			SqlEventTracker.Instance.Clear();
			RawDataRegistry.Instance.EncryptedRegistrationKey.Inner.ClearCache();

			GlbCompany company;

			using (Globals.TemporaryOverrideForIsTest(false)) // To read license key from db instead of test value
			{
				company = Factory.LoadTop1<GlbCompany>(new ZQuery());
			}

			AssertEquals("Should not read license from db yet", 0, SqlEventTracker.Instance.SqlEventList.Count(e => e.Contains(RawDataRegistry.Instance.EncryptedRegistrationKey.Name)));

			using (Globals.TemporaryOverrideForIsTest(false)) // To read license key from db instead of test value
			{
				_ = company.LicenceKeyIdentifier;
			}

			AssertEquals("Should read license from db now", 1, SqlEventTracker.Instance.SqlEventList.Count(e => e.Contains(RawDataRegistry.Instance.EncryptedRegistrationKey.Name)));
		}

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(GlbCompany)));
		}

		#endregion

		public void TestLicenceBusinessRegistrationNumberType()
		{
			AssertEquals("No type because no country", "", GlbCompany.LicenceBusinessRegNoType(null));

			var country = Factory.New<RefCountry>();
			AssertLicenceBusinessRegNoType(country, "GCR", "AU");
			AssertLicenceBusinessRegNoType(country, "VAT", "FJ");
			AssertLicenceBusinessRegNoType(country, "CNO", "NZ");
			AssertLicenceBusinessRegNoType(country, "GCR", "BE");
			AssertLicenceBusinessRegNoType(country, "GCR", "ET");
			AssertLicenceBusinessRegNoType(country, "GCR", "UG");
			AssertLicenceBusinessRegNoType(country, "GCR", "ZM");
			AssertLicenceBusinessRegNoType(country, "CIJ", "CR");
			AssertLicenceBusinessRegNoType(country, "GCR", "CO");
			AssertLicenceBusinessRegNoType(country, "GCR", "GT");
			AssertLicenceBusinessRegNoType(country, "GCR", "NG");
			AssertLicenceBusinessRegNoType(country, "GCR", "VE");
			AssertLicenceBusinessRegNoType(country, "GCR", "LV");
			AssertLicenceBusinessRegNoType(country, "GCR", "LT");
			AssertLicenceBusinessRegNoType(country, "GCR", "SI");
			AssertLicenceBusinessRegNoType(country, "GCR", "EC");
			AssertLicenceBusinessRegNoType(country, "GCR", "SV");
			AssertLicenceBusinessRegNoType(country, "GCR", "PY");
			AssertLicenceBusinessRegNoType(country, "GCR", "UY");
			AssertLicenceBusinessRegNoType(country, "GCR", "AZ");
			AssertLicenceBusinessRegNoType(country, "GCR", "BM");
			AssertLicenceBusinessRegNoType(country, "GCR", "BO");
			AssertLicenceBusinessRegNoType(country, "GCR", "PF");
			AssertLicenceBusinessRegNoType(country, "GCR", "HN");
			AssertLicenceBusinessRegNoType(country, "GCR", "KE");
			AssertLicenceBusinessRegNoType(country, "BRN", "MU");
			AssertLicenceBusinessRegNoType(country, "GCR", "NC");
			AssertLicenceBusinessRegNoType(country, "GCR", "NI");
			AssertLicenceBusinessRegNoType(country, "GCR", "PT");
			AssertLicenceBusinessRegNoType(country, "GCR", "KW");
			AssertLicenceBusinessRegNoType(country, "GCR", "MN");
			AssertLicenceBusinessRegNoType(country, "GCR", "SK");
			AssertLicenceBusinessRegNoType(country, "GCR", "ML");
			AssertLicenceBusinessRegNoType(country, "GCR", "JO");
			AssertLicenceBusinessRegNoType(country, "GCR", "IQ");
			AssertLicenceBusinessRegNoType(country, "GCR", "MO");
			AssertLicenceBusinessRegNoType(country, "GCR", "BN");
			AssertLicenceBusinessRegNoType(country, "GCR", "BH");
			AssertLicenceBusinessRegNoType(country, "BPN", "ZW");
			AssertLicenceBusinessRegNoType(country, "CRN", "LB");
			AssertLicenceBusinessRegNoType(country, "NIN", "SN");
			AssertLicenceBusinessRegNoType(country, "NRC", "CI");
			AssertLicenceBusinessRegNoType(country, "NRC", "CM");
			AssertLicenceBusinessRegNoType(country, "GCR", "MZ");
			AssertLicenceBusinessRegNoType(country, "GCR", "GQ");
			AssertLicenceBusinessRegNoType(country, "RNC", "DO");
			AssertLicenceBusinessRegNoType(country, "GST", "YR");
			AssertLicenceBusinessRegNoType(country, "GCR", "SO");
			AssertLicenceBusinessRegNoType(country, "NAO", "PA");
			AssertLicenceBusinessRegNoType(country, "NRC", "DZ");
			AssertLicenceBusinessRegNoType(country, "BRN", "MW");
			AssertLicenceBusinessRegNoType(country, "RCC", "NE");
			AssertLicenceBusinessRegNoType(country, "EIN", "PW");
			AssertLicenceBusinessRegNoType(country, "GCR", "CU");
			AssertLicenceBusinessRegNoType(country, "GCR", "GH");
			AssertLicenceBusinessRegNoType(country, "GCR", "BY");
			AssertLicenceBusinessRegNoType(country, "GCR", "SL");
			AssertLicenceBusinessRegNoType(country, "GCR", "KI");
		}

		void AssertLicenceBusinessRegNoType(RefCountry country, string codeType, string countryCode)
		{
			country.Code = countryCode;
			AssertEquals(codeType, codeType, GlbCompany.LicenceBusinessRegNoType(country));
		}

		public void TestCountryConsumptionTaxDescription()
		{
			var company = GetNewCompany();
			foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				company.GC_RN_NKCountryCode = countryCode;
				if (company.Country == null)
				{
					continue;
				}

				var taxName = Country.GetConsumptionTaxDescription(countryCode);
				if (string.IsNullOrEmpty(taxName))
				{
					AssertEquals("Default should equal", "VAT", company.ConsumptionTaxDescriptionForCompanyForm);
				}
				else
				{
					AssertEquals("Country Consumption Tax Description should equal", taxName, company.ConsumptionTaxDescriptionForCompanyForm);
				}
			}
		}

		public void TestPhoneNumbers()
		{
			var company = GetNewCompany();
			company.GC_RN_NKCountryCode = "AU";
			company.GC_Phone_Formatted = "0426 829 924";
			company.GC_Fax_Formatted = "+86-156-0113-1981";

			AssertEquals("+61 426 829 924", company.GC_Phone_Wrapper.FormattedForBinding);
			AssertEquals("+86 156 0113 1981", company.GC_Fax_Wrapper.FormattedForBinding);

			AssertEquals("0426 829 924", company.GC_Phone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals(string.Empty, company.GC_Fax_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
		}

		public void TestBusinessNumberChangeLogging()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_BusinessRegNo = "A000001";
			company.GC_BusinessRegNo2 = "B000001";
			company.GC_RN_NKCountryCode = "01";
			company.GC_RX_NKLocalCurrency = Constants.CurrencyCodes.NewZealand;
			Factory.Save();

			var logs = company.Logs.GetAllLogs().Cast<StmALog>();

			AssertEquals(0, logs.Count(log => log.SL_Reference == "Business Registration Number 1:  to A000001"));
			AssertEquals(0, logs.Count(log => log.SL_Reference == "Business Registration Number 2:  to B000001"));

			company.GC_BusinessRegNo = "A000002";
			company.GC_BusinessRegNo2 = "B000002";
			Factory.Save();

			AssertEquals(1, logs.Count(log => log.SL_Reference == "Business Registration Number 1: A000001 to A000002"));
			AssertEquals(1, logs.Count(log => log.SL_Reference == "Business Registration Number 2: B000001 to B000002"));

			company.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			company.GC_RX_NKLocalCurrency = Constants.CurrencyCodes.Australia;
			company.GC_BusinessRegNo = "A000003";
			company.GC_BusinessRegNo2 = "B000003";
			Factory.Save();

			AssertEquals(1, logs.Count(log => log.SL_Reference == "Australian Business Number: A000002 to A000003"));
			AssertEquals(1, logs.Count(log => log.SL_Reference == "Australian Company Number: B000002 to B000003"));
		}

		public void TestLocalCurrencyChangeLog()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DMO";
			Factory.Save();

			company.GC_RX_NKLocalCurrency = "JPY";
			Factory.Save();
			company.GC_RX_NKLocalCurrency = "AUD";
			Factory.Save();
			var logs = company.Logs.GetAllLogs().Cast<StmALog>().Select(x => x.SL_Reference);
			AssertCollectionContains("Company Currency: JPY to AUD", logs);
			AssertCollectionContains("Company Currency: AUD to JPY", logs);
		}

		public void TestAccCFXUpliftConfigurationsNotNullAndCorrectLevel()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();

			AssertNotNull(glbCompany.AccCFXConfigurations);
			AssertEquals(AccCFXConfigurationLevelEnum.Company, glbCompany.AccCFXConfigurations.Level);
		}

		public void TestAccExRateConfigsNotNullAndCorrectLevel()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();

			AssertNotNull(glbCompany.AccExchangeRateConfigurations);
			AssertEquals(AccExRateConfigurationLevelEnum.Company, glbCompany.AccExchangeRateConfigurations.Level);
		}

		public void TestAccPlaceOfSupplyConfigurationsIsNotNullAndCorrectLevel()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();

			AssertNotNull(glbCompany.AccPlaceOfSupplyConfigurations);
			AssertEquals(AccPOSConfigurationLevel.Company, glbCompany.AccPlaceOfSupplyConfigurations.Level);
			var bizo = glbCompany.AccPlaceOfSupplyConfigurations.AddNew();
			AssertEquals(AccPOSConfigurationLevel.Company, bizo.Level);
		}

		public void TestAccSurchargeConfigurationsIsNotNull()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();

			AssertNotNull(glbCompany.AccSurchargeConfigurations);
		}

		public void TestCashAdvanceConfigurationsNotNullAndCorrectLevel()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();

			AssertNotNull(glbCompany.CashAdvanceConfigurations);
			AssertEquals(AccCashAdvanceDefaultingLevel.Company, glbCompany.CashAdvanceConfigurations.Level);
		}

		#region Address Validation

		public void TestValidationStatus_WhenChangingAddressFieldWhileValueIsCna_ShouldKeepItAsCna()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_ValidationStatus = AddressValidationStatus.CountryNotAvailable;

			// Act & Assert.

			company.GC_Address1 = "[_MOCK_ADDRESS_1_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, company.GC_ValidationStatus);

			company.GC_Address2 = "[_MOCK_ADDRESS_2_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, company.GC_ValidationStatus);

			company.GC_City = "[_MOCK_CITY_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, company.GC_ValidationStatus);

			company.GC_PostCode = "0000";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, company.GC_ValidationStatus);

			company.GC_State = "[_MOCK_STATE_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, company.GC_ValidationStatus);

			company.GC_RN_NKCountryCode = "XY";
			AssertEquals(AddressValidationStatus.ToBeVerified, company.GC_ValidationStatus);
		}

		public void TestChangingAddressResetsValidationStatus()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var address = Factory.NewWithValidTestData<GlbCompany>();
			address.GC_RN_NKCountryCode = "AU";
			address.ValidationStatus = AddressValidationStatus.Verified;

			AssertValidationStatusIsReset(address, () => address.Address1 += "A");
			AssertValidationStatusIsReset(address, () => address.Address2 += "A");
			AssertValidationStatusIsReset(address, () => address.City += "A");
			AssertValidationStatusIsReset(address, () => address.Postcode += "A");
			AssertValidationStatusIsReset(address, () => address.State += "A");
		}

		void AssertValidationStatusIsReset(ISupportWebAddressValidation address, Action action)
		{
			address.ValidationStatus = AddressValidationStatus.Verified;
			action.Invoke();
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestRaiseAddressValidationStatusChanged()
		{
			var address = Factory.NewWithValidTestData<GlbCompany>() as ISupportWebAddressValidation;
			address.ValidationStatus = AddressValidationStatus.ToBeVerified;
			address.Address2 = "";

			address.AddressValidationStatusChanged += address_AddressValidationStatusChanged;
			address.ValidationStatus = AddressValidationStatus.Verified;
			AssertEquals("It happened", address.Addressee);
		}

		void address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			((GlbCompany)sender).GC_Name = "It happened";
		}

		public void TestValidationStatus()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<GlbCompany>();
			address.GC_RN_NKCountryCode = country.Code;
			address.GC_ValidationStatus = AddressValidationStatus.ToBeVerified;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestState()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<GlbCompany>();
			address.GC_RN_NKCountryCode = country.Code;
			address.GC_State = "NSW";
			AssertEquals("New South Wales", address.State);

			address.State = "Victoria";
			AssertEquals("VIC", address.GC_State);
		}

		public void TestNeedValidation()
		{
			var factory = new BusinessObjectFactory();

			var australia = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			australia.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var china = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			china.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;

			var address = factory.NewWithValidTestData<GlbCompany>();
			address.Address1 = "A1";
			address.Address2 = "A2";
			address.Postcode = "1234";
			address.City = "Syd";
			address.State = "NSW";
			address.GC_RN_NKCountryCode = "CN";
			Assert(address.NeedValidation);

			address.GC_RN_NKCountryCode = "AU";
			Assert(address.NeedValidation);

			factory.Save();
			Assert(address.IsInDatabase);
			Assert(!address.NeedValidation);

			address.Address1 += "A";
			Assert(address.NeedValidation);

			address.Address2 = "";
			Assert(address.NeedValidation);

			address.Address1 = "";
			Assert(!address.NeedValidation);
		}

		public void TestResetAddressMap()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var address = Factory.NewWithValidTestData<GlbCompany>();
			address.GC_RN_NKCountryCode = "AU";
			AssertAddressMap(address, address.GC_Address1Info);
			AssertAddressMap(address, address.GC_Address2Info);
			AssertAddressMap(address, address.GC_CityInfo);
			AssertAddressMap(address, address.GC_PostCodeInfo);
			AssertAddressMap(address, address.GC_StateInfo);
			AssertAddressMap(address, address.GC_RN_NKCountryCodeInfo);

			address.GC_RN_NKCountryCode = "AU";
			var usPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")).PK.ToGuid();
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(usPK, disabledForCompany: true));
			AssertAddressMap(address, address.GC_Address1Info);
			AssertAddressMap(address, address.GC_Address2Info);
			AssertAddressMap(address, address.GC_CityInfo);
			AssertAddressMap(address, address.GC_PostCodeInfo);
			AssertAddressMap(address, address.GC_StateInfo);
			AssertAddressMap(address, address.GC_RN_NKCountryCodeInfo);
		}

		void AssertAddressMap(GlbCompany address, ZPropertyInfo propertyInfo)
		{
			address.AddressMap = "ABCDE";
			propertyInfo.Value = (ZString)(propertyInfo.Name == nameof(GlbCompany.GC_RN_NKCountryCode) ? "US" : (ZString)propertyInfo.Value + "1");
			Assert(string.IsNullOrEmpty(address.AddressMap));
		}

		public void TestValidationSection()
		{
			AssertEquals(AddressValidationSection.Company, Factory.New<GlbCompany>().ValidationSection);
		}

		#endregion

		#region GC_ValidationStatus

		public void TestValidationStatus_WhenSetToManuallyVerifiedFromOtherValue_ShouldStayAsIsUntilCompanyIsReloaded()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var company = Factory.NewWithValidTestData<GlbCompany>();

			company.GC_Address1 = "42 FOOBAR STREET";
			company.GC_Address2 = "FUNPLACE";
			company.GC_PostCode = "0000";
			company.GC_City = "WHITERUN";
			company.GC_State = "TAMRIEL";
			company.GC_RN_NKCountryCode = "ID";

			// Act.

			company.GC_ValidationStatus = AddressValidationStatus.ManuallyVerified;

			company.GC_Address1 = "72 O'RIORDAN STREET";
			company.GC_Address2 = "WISETECH GLOBAL";
			company.GC_PostCode = "2015";
			company.GC_City = "ALEXANDRIA";
			company.GC_State = "NSW";
			company.GC_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ManuallyVerified, company.GC_ValidationStatus);
		}

		public void TestValidationStatus_WhenLoadedAsManuallyVerifiedFromDatabase_ShouldResetValueAfterChangingAddressField()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var company = Factory.NewWithValidTestData<GlbCompany>();

			company.GC_Address1 = "42 FOOBAR STREET";
			company.GC_Address2 = "FUNPLACE";
			company.GC_PostCode = "0000";
			company.GC_City = "WHITERUN";
			company.GC_State = "TAMRIEL";
			company.GC_RN_NKCountryCode = "ID";

			company.GC_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();

			var reloadedCompany = new BusinessObjectFactory().Load<GlbCompany>(company.PK);

			// Act.

			reloadedCompany.GC_Address1 = "72 O'RIORDAN STREET";
			reloadedCompany.GC_Address2 = "WISETECH GLOBAL";
			reloadedCompany.GC_PostCode = "2015";
			reloadedCompany.GC_City = "ALEXANDRIA";
			reloadedCompany.GC_State = "NSW";
			reloadedCompany.GC_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ToBeVerified, reloadedCompany.GC_ValidationStatus);
		}

		#endregion

		#region GeoLocation

		public void TestConstructor_WhenCreatingWithDataRow_ShouldInitializeGeoLocationWithNonNullValue()
		{
			// Arrange.

			// Act.

			var company = Factory.New<GlbCompany>();

			// Assert.

			var row = ((INeedRow)company).Row;

			AssertEquals(ZGeography.Empty, row[GlbCompanySchema.Constants.GC_GeoLocation]);
		}

		public void TestGeoLocation_WhenGettingEmptyValue_ShouldSetItToPointZero()
		{
			// Arrange.

			var company = Factory.New<GlbCompany>();

			// Act.

			company.GC_GeoLocation = ZGeography.Empty;

			// Assert.

			AssertEquals(ZGeography.Empty, company.GC_GeoLocation);
		}

		#endregion

		#region LocalCurrency

		public void TestLocalCurrency()
		{
			var testFactory = new TestBusinessObjectFactory();
			var company = testFactory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "ABC";
			company.GC_RN_NKCountryCode = "AU";
			company.GC_RX_NKLocalCurrency = "AUD";
			company.GC_Address1 = "Address";
			testFactory.Save();

			testFactory.LoadedFromFactory = false;
			AssertNotNull("Precondition", company.LocalCurrency);
			AssertEquals("Should load LocalCurrency from factory", true, testFactory.LoadedFromFactory);

			testFactory.LoadedFromFactory = false;
			AssertNotNull("Precondition", company.LocalCurrency);
			AssertEquals("Accessing the LocalCurrency again should not load from factory", false, testFactory.LoadedFromFactory);

			testFactory.LoadedFromFactory = false;
			company.GC_RX_NKLocalCurrency = "USD";
			AssertNotNull("Precondition", company.LocalCurrency);
			AssertEquals("Due to change in LocalCurrency code, should reload from factory", true, testFactory.LoadedFromFactory);
		}

		public void TestCustomsCurrency()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			AssertEquals("Precondition", Constants.CurrencyCodes.Australia, company.LocalCurrency.RX_Code);
			AssertEquals("CustomsCurrency equals LocalCurrency", Constants.CurrencyCodes.Australia, company.CustomsCurrency.RX_Code);

			company.GC_RX_NKLocalCurrency = Constants.CurrencyCodes.Germany;
			AssertEquals("Precondition", Constants.CurrencyCodes.Germany, company.LocalCurrency.RX_Code);
			AssertEquals("CustomsCurrency equals LocalCurrency", Constants.CurrencyCodes.Germany, company.CustomsCurrency.RX_Code);
		}

		public void TestExchangeRateDecimalPlaces()
		{
			var company = GetNewCompany();
			AssertEquals("ExchangeReate decimal places should be 6", 6, company.ExchangeRateDecimalPlaces);
		}

		#endregion

		public void TestEnableConfigurationToSender() => CombineAssertions(() =>
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			AssertEquals("EnableConfigurationToSender when CredentialData is null.", false, company.EnableConfigurationToSender);
			company.CredentialDataCreator = () => new GlbCompanyCredentialData("Id", EDIInterchangeTypeList.Codes.Configuration, new[] { company.GC_CustomsRegistrationNoInfo }, (company) => { return new Customs.XmlCredential.Group(); });
			AssertEquals("EnableConfigurationToSender when CredentialData is not null.", true, company.EnableConfigurationToSender);
		});

		public void TestShouldSendCredential()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_CustomsRegistrationNo = "4321";
			company.CredentialDataCreator = () => new GlbCompanyCredentialData("Id", EDIInterchangeTypeList.Codes.Configuration, new[] { company.GC_CustomsRegistrationNoInfo }, (company) => { return new Customs.XmlCredential.Group(); });

			Factory.Save();
			var interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull("Initial update", interchange);

			interchange.Delete();
			Factory.Save();

			company.GC_CustomsRegistrationNo = "4321";
			Factory.Save();
			interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNull("No change have been made", interchange);
		}

		public void TestShouldSendDeleteCredential()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_CustomsRegistrationNo = "4321";
			company.CredentialDataCreator = () => new GlbCompanyCredentialData("Id", EDIInterchangeTypeList.Codes.Configuration, new[] { company.GC_CustomsRegistrationNoInfo }, (company) => { return new Customs.XmlCredential.Group(); });
			Factory.Save();
			var interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull("Initial update", interchange);

			interchange.Delete();
			company.Delete();
			Factory.Save();

			interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull("Delete message sent", interchange);
		}

		#region Implementation

		GlbCompany DemoCompany
		{
			get
			{
				if (fDemoCompany == null)
				{
					fDemoCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, GlbCompany.DemoCompanyCode);
				}
				return fDemoCompany;
			}
		}
		GlbCompany fDemoCompany;

		GlbCompany SetupNewCompany(ZString country, string code, bool isGstRegistered, bool isWhtRegistered = false)
		{
			GlbCompany result = Factory.New<GlbCompany>();
			result.GC_Code = code;
			result.GC_RN_NKCountryCode = country;
			result.GC_RX_NKLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			result.GC_IsGSTRegistered = isGstRegistered;
			result.GC_IsWHTRegistered = isWhtRegistered;
			Factory.Save();

			result.GC_Address1 = "Address";

			return result;
		}

		GlbCompany GetNewCompany()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			company.GC_RX_NKLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			return company;
		}

		void PopulateCompanyInfo(
			GlbCompany company,
			string code,
			string name,
			string address1,
			string address2,
			string city,
			string postCode,
			string state,
			string countryCode,
			string taxationRegNo,
			string businessRegNo,
			string localCurrencyCode,
			bool isReciprocal,
			bool isGSTRegistered,
			bool isGSTCashBasis,
			bool isWHTRegistered,
			bool isWHTCashBasis,
			string phone = "",
			string fax = "",
			string email = "",
			string webAddress = "")
		{
			company.GC_Code = code;
			company.GC_Name = name;
			company.GC_Address1 = address1;
			company.GC_Address2 = address2;
			company.GC_City = city;
			company.GC_PostCode = postCode;
			company.GC_State = state;
			company.GC_RN_NKCountryCode = countryCode;
			company.GC_BusinessRegNo = taxationRegNo;
			company.GC_BusinessRegNo2 = businessRegNo;
			company.GC_RX_NKLocalCurrency = localCurrencyCode;
			company.GC_IsReciprocal = isReciprocal;
			company.GC_IsGSTRegistered = isGSTRegistered;
			company.GC_IsGSTCashBasis = isGSTCashBasis;
			company.GC_IsWHTRegistered = isWHTRegistered;
			company.GC_IsWHTCashBasis = isWHTCashBasis;
			company.GC_Phone = phone;
			company.GC_Fax = fax;
			company.GC_Email = email;
			company.GC_WebAddress = webAddress;
		}

		class TestBusinessObjectFactory : BusinessObjectFactory
		{
			public override BusinessObject LoadFromUniqueKey(Type bizOType, SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue)
			{
				LoadedFromFactory = true;
				return base.LoadFromUniqueKey(bizOType, uniqueKeyColumn, uniqueKeyValue);
			}

			public bool LoadedFromFactory;
		}

		#endregion
	}
}
