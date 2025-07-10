using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobDeclarationComplianceRiskTest : ComplianceRiskBusinessObjectTestCase
	{
		public void TestBaseJobDeclarationPartiesAndCountriesAndFetchHint()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			AssertEquals("Precondition: Declaration should be ICompliancePartyRiskStatusProvider", true, declaration is ICompliancePartyRiskStatusProvider);
			AssertEquals("Precondition: Declaration should be IComplianceLocationRiskStatusProvider", true, declaration is IComplianceLocationRiskStatusProvider);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			supplier.OH_RL_NKClosestPort = "AUSEA";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			importer.OH_RL_NKClosestPort = "ATVIE";

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			shippingLine.OH_RL_NKClosestPort = "BDCGP";

			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Forwarder = forwarder.PK;
			forwarder.OH_RL_NKClosestPort = "BEBRU";

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_RL_NKClosestPort = "BNBWN";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = declaration.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_JobNum = "ABC123";

			var deliveryOrPickupCartageCo = Factory.NewWithValidTestData<OrgHeader>();
			deliveryOrPickupCartageCo.OH_RL_NKClosestPort = "BRITJ";
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = deliveryOrPickupCartageCo.MainAddress.PK;

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "RUMOW";
			transport1.JW_RL_NKDiscPort = "SARUH";
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Code = "TestVessel1";
			transport1.JW_Vessel = vessel1.RV_Code;
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_RL_NKClosestPort = "CRSJO";
			transport1.CarrierPK = carrier1.PK;

			var transport2 = declaration.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_RL_NKLoadPort = "CNSHA";
			transport2.JW_RL_NKDiscPort = "JPTYO";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Code = "TestVessel2";
			transport2.JW_Vessel = vessel2.RV_Code;
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_RL_NKClosestPort = "ESBCN";
			transport2.CarrierPK = carrier2.PK;

			var transport3 = declaration.Transports.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_RL_NKLoadPort = "RUMOW";
			transport3.JW_RL_NKDiscPort = "SARUH";
			transport3.JW_Vessel = string.Empty;

			declaration.JE_RL_NKPortOfLoading = "HKHKG";
			declaration.JE_RL_NKPortOfArrival = "IDBEJ";
			declaration.JE_RL_NKOrigin = "IN";
			declaration.JE_RL_NKFinalDestination = "IS";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_RN_NKCountryCode = "JP";
			declaration.JE_RW_NKOriginState = state.RW_Code;

			declaration.JE_RN_NKTrailer1Nationality = "JP";
			declaration.JE_RN_NKTrailer2Nationality = "KR";
			declaration.JE_RN_NKTransportNationality = "LK";
			declaration.JE_RN_NKTransportNationalityInland = "MP";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "MYINVOICE";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "GBP";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1902194000";
			invoiceLine1.JI_CustomsQuantity = 3m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_Weight = 2500;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_CountryOfOrigin = "CH";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0712311000";
			invoiceLine2.JI_CustomsQuantity = 3m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 1300m;
			invoiceLine2.JI_Weight = 650;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_CountryOfOrigin = "FJ";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newDeclaration = newFactory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declaration.PK));

			var parties = ((ICompliancePartyRiskStatusProvider)newDeclaration).Parties.ToArray();
			var countries = ((IComplianceLocationRiskStatusProvider)newDeclaration).Locations.ToArray();

			var tableSelects = newFactory.TableSelects;
			AssertEquals(false, tableSelects.Any(u => u.Value > 1));

			CombineAssertions(() =>
			{
				AssertContainsVessel(transport1, parties);
				AssertContainsVessel(transport2, parties, false);
				AssertContainsVessel(transport3, parties, false);

				AssertContainsComplianceParty("Local Client", localClient, parties);
				AssertContainsComplianceParty("Supplier", supplier, parties);
				AssertContainsComplianceParty("Importer", importer, parties);
				AssertContainsComplianceParty("Forwarder", forwarder, parties);
				AssertContainsComplianceParty("Delivery/Pickup Port Transport Company", deliveryOrPickupCartageCo, parties);
				AssertContainsComplianceParty("Routing Carrier", carrier1, parties);

				AssertContainsComplianceCountry("Port of Loading", GetCountryByCode(declaration.JE_RL_NKPortOfLoading), countries);
				AssertContainsComplianceCountry("Port of Discharge", GetCountryByCode(declaration.JE_RL_NKPortOfArrival), countries);
				AssertContainsComplianceCountry("Port of Origin", GetCountryByCode(declaration.JE_RL_NKOrigin), countries);
				AssertContainsComplianceCountry("Final Destination", GetCountryByCode(declaration.JE_RL_NKFinalDestination), countries);
				AssertContainsComplianceCountry("Origin State", state.Country, countries);
				AssertContainsComplianceCountry("Routing Load Country", transport1.LoadPort.Country, countries);
				AssertContainsComplianceCountry("Routing Discharge Country", transport1.DiscPort.Country, countries);
				AssertContainsComplianceCountry("Routing Load Country", transport2.LoadPort.Country, countries);
				AssertContainsComplianceCountry("Routing Discharge Country", transport2.DiscPort.Country, countries);
				AssertContainsComplianceCountry("Nationality", declaration.Trailer1Nationality, countries);
				AssertContainsComplianceCountry("Nationality", declaration.Trailer2Nationality, countries);
				AssertContainsComplianceCountry("Nationality", declaration.TransportNationality, countries);
				AssertContainsComplianceCountry("Nationality", declaration.TransportNationalityInland, countries);
				AssertContainsComplianceCountry("Goods Origin", invoiceLine1.CountryOfOrigin, countries);
				AssertContainsComplianceCountry("Goods Origin", invoiceLine2.CountryOfOrigin, countries);
			});

			void AssertContainsVessel(Transport transport, IScreeningParty[] complianceParties, bool contains = true)
			{
				var parties = complianceParties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson));
				AssertEquals($"Vessel - {transport.JW_Vessel}",
					contains,
					parties.Any(o => o.NotLinkedVessel != null && (o.NotLinkedVessel as Transport).PK == transport.PK)
					|| parties.Any(o => o.Vessel != null && o.Vessel.RV_Code == transport.JW_Vessel));
			}

			void AssertContainsComplianceParty(string description, OrgHeader orgHeader, IScreeningParty[] complianceParties)
			{
				var parties = complianceParties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).Where(p =>
					p.Description.Contains(description)).ToList();
				AssertGreaterThan($"{description} - {orgHeader.OH_Code}", parties.Count, 0);

				var party = parties.FirstOrDefault(p => orgHeader.OH_Code == ((p?.Header?.OH_Code ?? p?.OrgCode) ?? ZString.Empty));
				AssertEquals($"{description} - {orgHeader.OH_Code}", orgHeader.OH_Code, party.OrgCode);
			}

			RefCountry GetCountryByCode(ZString code)
			{
				return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, code.SubstringSafe(0,2));
			}

			void AssertContainsComplianceCountry(string description, RefCountry expectedCountry, IComplianceLocation[] complianceCountries)
			{
				AssertNotNull($"Precondition: {description} - expectedCountry", expectedCountry);

				var countries = complianceCountries.Where(p => p.ParentsDescription.Contains(description)).ToList();
				AssertGreaterThan($"{description} - {expectedCountry}", countries.Count, 0);

				var country = countries.FirstOrDefault(c => expectedCountry.Code == (c?.Code ?? ZString.Empty));
				AssertEquals($"{description} - {expectedCountry}", expectedCountry.Code, country?.Code);
			}
		}

		public void TestBaseJobDeclarationCommodityRisk()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			AssertEquals("Precondition: Declaration should be IComplianceCommodityRiskStatusProvider", true, declaration is IComplianceCommodityRiskStatusProvider);

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "MYINVOICE";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "GBP";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0712311001";
			invoiceLine1.JI_CountryOfOrigin = "US";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0712311000";
			invoiceLine2.JI_CountryOfOrigin = "AU";

			var commodities = ((IComplianceCommodityRiskStatusProvider)declaration).Commodities.ToArray();

			AssertEquals(2, commodities.Length);
			AssertContainsComplianceCommodity(invoiceLine1);
			AssertContainsComplianceCommodity(invoiceLine2);

			void AssertContainsComplianceCommodity(BaseJobComInvoiceLine invoiceLine)
			{
				AssertEquals(true, commodities.Any(u => u.HarmonizedCode == invoiceLine.JI_TariffForComplianceWise));
			}
		}

		[TestDate(2022, 10, 15)]
		public void TestComplianceEffectiveDate()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			AssertNotNull("Precondition: Shipment should be IComplianceCommodityRiskStatusProvider", declaration is IComplianceCommodityRiskStatusProvider);

			AssertEquals("Precondition", 0, declaration.Transports.Count);
			AssertEquals("Precondition", ZDateTime.Empty, declaration.JE_DateAtOrigin);
			AssertEquals("Precondition", ZDateTime.Empty, declaration.JE_SystemCreateTimeUtc);
			AssertEquals("Effective Date should be Now", ZDateTime.Now, ((IComplianceCommodityRiskStatusProvider)declaration).EffectiveDate);

			declaration.JE_SystemCreateTimeUtc = new ZDateTime(2022, 10, 8);
			AssertEquals("Effective Date should be declaration create time.", new ZDateTime(2022, 10, 8).ToLocalBranchTime(), ((IComplianceCommodityRiskStatusProvider)declaration).EffectiveDate);

			declaration.JE_DateAtOrigin = new ZDateTime(2022, 10, 9);
			AssertEquals("Effective Date should be Date at Origin.", new ZDateTime(2022, 10, 9), ((IComplianceCommodityRiskStatusProvider)declaration).EffectiveDate);

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "RUMOW";
			transport1.JW_RL_NKDiscPort = "SARUH";
			transport1.JW_ATD = new ZDateTime(2022, 10, 10);
			AssertEquals("Effective Date should be first routing ATD.", new ZDateTime(2022, 10, 10), ((IComplianceCommodityRiskStatusProvider)declaration).EffectiveDate);
		}

		public void TestComplianceRiskSupport()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, ((IComplianceItemRiskStatusProvider)declaration).ComplianceRiskSupport.IsSupportInitialization());
			AssertEquals(false, ((IComplianceItemRiskStatusProvider)declaration).ComplianceRiskSupport.IsSupportSubCompliances());
		}

		public void TestAssessmentPointPairInfo()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USORD";
			declaration.JE_DateAtOrigin = new ZDateTime(2024, 10, 9);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2024, 10, 12);
			declaration.JE_TransportMode = Constants.TransportModes.Sea;

			var assessmentPointPairInfo = ((IComplianceCommodityRiskStatusProvider)declaration).AssessmentPointPairInfo;
			AssertContainsExactElementsInAnyOrder(new[] { ("AU", "AUSYD", "Origin", "US", "USORD", "Destination", declaration.JE_DateAtFinalDestination, declaration.JE_DateAtOrigin, declaration.JE_TransportMode.ToString()) },
				assessmentPointPairInfo.PointPairs.Select(u => (u.OriginPoint.Country, u.OriginPoint.UNLOCO, u.OriginPoint.MovementDescription, u.DestinationPoint.Country, u.DestinationPoint.UNLOCO, u.DestinationPoint.MovementDescription, u.EstimatedTimeOfArrival, u.EstimatedTimeOfDeparture, u.Mode)));
		}

		public void TestComplianceRiskProviderParentIdAndTableCode()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(declaration.PK, ((IComplianceCommodityRiskStatusProvider)declaration).ParentID);
			AssertEquals(declaration.TablePrefix, ((IComplianceCommodityRiskStatusProvider)declaration).ParentTableCode);
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceJobIsCurrent()
		{
			using (OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 12))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_SystemCreateTimeUtc = new ZDateTime(2022, 10, 8);
				var job = Factory.NewJobForTesting<JobHeader>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_ParentID = declaration.PK;
				job.JH_JobNum = "ABC123";
				job.JH_Status = JobHeaderStatus.Codes.Complete;
				Factory.Save();

				Assert(!((IComplianceItemRiskStatusProvider)declaration).JobTime.IsCurrent);

				job.JH_Status = JobHeaderStatus.Codes.Working;
				Factory.Save();
				Assert(!((IComplianceItemRiskStatusProvider)declaration).JobTime.IsCurrent);

				declaration.JE_SystemCreateTimeUtc = new ZDateTime(2023, 08, 02);
				Assert(((IComplianceItemRiskStatusProvider)declaration).JobTime.IsCurrent);

				declaration.JE_SystemCreateTimeUtc = new ZDateTime(2022, 10, 8);
				declaration.JE_DateAtOrigin = new ZDateTime(2024, 07, 29);
				Assert(((IComplianceItemRiskStatusProvider)declaration).JobTime.IsCurrent);

				declaration.JE_DateAtOrigin = ZDateTime.Empty;
				declaration.JE_DateAtFinalDestination = new ZDateTime(2024, 08, 02);
				Assert(((IComplianceItemRiskStatusProvider)declaration).JobTime.IsCurrent);

				declaration.JE_DateAtFinalDestination = ZDateTime.Empty;
				Assert(!((IComplianceItemRiskStatusProvider)declaration).JobTime.IsCurrent);

				var transport1 = declaration.Transports.AddNew();
				transport1.JW_TransportMode = Constants.TransportModes.Sea;
				transport1.JW_RL_NKLoadPort = "RUMOW";
				transport1.JW_RL_NKDiscPort = "SARUH";
				transport1.JW_ATD = new ZDateTime(2024, 07, 25);
				Assert(((IComplianceItemRiskStatusProvider)declaration).JobTime.IsCurrent);
			}
		}

		public void TestComplianceRiskSecurity()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var commodityProvider = declaration as IComplianceCommodityRiskStatusProvider;
			AssertEquals(true, commodityProvider.IsEditingCommoditySupported);
			AssertEquals(Env.Security.CustomsComplianceEditHarmonizedCode, commodityProvider.EditHarmonizedCodeSecurity);
			AssertEquals(Env.Security.CustomsComplianceEditComplianceAssessment, commodityProvider.EditComplianceAssessmentSecurity);
			AssertEquals(Env.Security.CustomsComplianceAllowComplianceAssessment, commodityProvider.AllowComplianceAssessmentSecurity);
			AssertEquals(Env.Security.CustomsComplianceDeclineComplianceAssessment, commodityProvider.DeclineComplianceAssessmentSecurity);

			var itemProvider = declaration as IComplianceItemRiskStatusProvider;
			AssertEquals(Env.Security.CustomsComplianceAllowOverrideOverallRiskStatus, itemProvider.AllowOverrideOverallRiskStatusSecurity);
			AssertEquals(Env.Security.CustomsComplianceAllowResynchronizeRiskStatus, itemProvider.AllowResynchronizeRiskStatusSecurity);
			AssertEquals(Env.Security.CustomsComplianceAllowOverrideFreightMovementRestrictions, itemProvider.AllowOverrideFreightMovementRestrictionsSecurity);
		}

		public override void TestRiskCalculateFactor()
		{
			AssertRiskCalculateFactor(JobMessageTypeList.Codes.Export, CommodityRiskCalculateFactor.Export);
			AssertRiskCalculateFactor(JobMessageTypeList.Codes.Import, CommodityRiskCalculateFactor.Import);

			void AssertRiskCalculateFactor(string messageType, CommodityRiskCalculateFactor expected)
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = messageType;

				var provider = (IComplianceCommodityRiskStatusProvider)declaration;
				AssertEquals(expected, provider.RiskCalculateFactor);
			}
		}

		public void TestSupportInteractionWithComplianceWiseCommoditiesEnable()
		{
			AssertSupportInteractionWithComplianceWiseCommoditiesEnable(false);
			AssertSupportInteractionWithComplianceWiseCommoditiesEnable(true);

			void AssertSupportInteractionWithComplianceWiseCommoditiesEnable(bool enableFeature)
			{
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(enableFeature))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					AssertEquals(enableFeature, ((ISupportInteractionWithComplianceWiseCommodities)declaration).Enabled);
				}
			}
		}

		#region Implementation

		protected override IComplianceItemRiskStatusProvider GetComplianceItemRiskStatusProvider() => Factory.NewWithValidTestData<BaseJobDeclaration>();

		protected override ComplianceRiskSupport SupporterInfo_DoNotModifyThisBeforeCheckingBaseImplementation => ComplianceRiskSupport.SupportInitialization;

		#endregion

		#region Commodity Cache

		public void TestDeclarationInvoiceLineCache_LocationsLoadedIfNotCached()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<US.IJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.AddNewInvoiceLine();
				invoiceLine1.JI_Tariff = "33333333";
				invoiceLine1.JI_CountryOfOrigin = "AU";
				Factory.Save();

				var factory1 = new BusinessObjectFactory();
				var declaration1 = factory1.Load<US.IJobDeclaration>(declaration.PK);
				var invoiceLines1 = ((BaseJobDeclaration)declaration1).InvoiceLines; // This is loaded by the BusinessObject workflow.

				// Let's change the country of origin and save to make sure that the cache mechanism will get data from already loaded InvoiceLines.
				invoiceLines1[0].JI_CountryOfOrigin = "NZ";
				Factory.Save();

				var locations1 = ((IComplianceLocationRiskStatusProvider)declaration1).Locations.ToArray(); // This is loaded not by the cache workflow because we've already loaded InvoiceLines and we can get data from them.

				CombineAssertions(() =>
				{
					AssertEquals(1, locations1.Length);
					AssertEquals(1, invoiceLines1.Count);
					AssertEquals(invoiceLines1[0].JI_CountryOfOrigin, locations1[0].Code); // Code as provided.
				});

				// Let's change the country of origin and NOT save to make sure that the cache is updated when InvoiceLines change.
				invoiceLines1[0].JI_CountryOfOrigin = "US";

				var locations2 = ((IComplianceLocationRiskStatusProvider)declaration1).Locations.ToArray(); // This is loaded not by the cache workflow because we've already loaded InvoiceLines and we can get data from them.

				CombineAssertions(() =>
				{
					AssertEquals(1, locations2.Length);
					AssertEquals(1, invoiceLines1.Count);
					AssertEquals(invoiceLines1[0].JI_CountryOfOrigin, locations2[0].Code); // Code as provided.
				});
			}
		}

		public void TestDeclarationInvoiceLineCache_LocationsLoadedFromCache()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<US.IJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.AddNewInvoiceLine();
				invoiceLine1.JI_Tariff = "33333333";
				invoiceLine1.JI_CountryOfOrigin = "AU";
				Factory.Save();

				var factory1 = new BusinessObjectFactory();
				var declaration1 = factory1.Load<US.IJobDeclaration>(declaration.PK);
				var locations1 = ((IComplianceLocationRiskStatusProvider)declaration1).Locations.ToArray(); // This is loaded by the cache workflow.

				Assert(!declaration1.IsInvoiceLinesLoaded); // We've got locations but InvoiceLines are not yet loaded.

				var invoiceLines1 = ((BaseJobDeclaration)declaration1).InvoiceLines; // This is loaded by the BusinessObject workflow.

				Assert(declaration1.IsInvoiceLinesLoaded); // Now InvoiceLines should be loaded.

				CombineAssertions(() =>
				{
					AssertEquals(1, locations1.Length);
					AssertEquals(1, invoiceLines1.Count);
					AssertEquals(invoiceLines1[0].JI_CountryOfOrigin, locations1[0].Code); // Code as provided.
				});

				// Let's change the country of origin and NOT save to make sure that the cache is updated when InvoiceLines change.
				invoiceLines1[0].JI_Tariff = "55555555";

				var locations2 = ((IComplianceLocationRiskStatusProvider)declaration1).Locations.ToArray(); // This is loaded not by the cache workflow because we've already loaded InvoiceLines and we can get data from them.

				CombineAssertions(() =>
				{
					AssertEquals(1, locations2.Length);
					AssertEquals(1, invoiceLines1.Count);
					AssertEquals(invoiceLines1[0].JI_CountryOfOrigin, locations2[0].Code); // Code as provided.
				});
			}
		}

		public void TestDeclarationInvoiceLineCache_CommoditiesLoadedIfNotCached()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<US.IJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.AddNewInvoiceLine();
				invoiceLine1.JI_Tariff = "33333333";
				Factory.Save();

				var factory1 = new BusinessObjectFactory();
				var declaration1 = factory1.Load<US.IJobDeclaration>(declaration.PK);
				var invoiceLines1 = ((BaseJobDeclaration)declaration1).InvoiceLines; // This is loaded by the BusinessObject workflow.

				// Let's change the tariff and save to make sure that the cache mechanism will get data from already loaded InvoiceLines.
				invoiceLines1[0].JI_Tariff = "44444444";
				Factory.Save();

				var commodities1 = ((IComplianceCommodityRiskStatusProvider)declaration1).Commodities.ToArray(); // This is loaded not by the cache workflow because we've already loaded InvoiceLines and we can get data from them.

				CombineAssertions(() =>
				{
					AssertEquals(1, commodities1.Length);
					AssertEquals(1, invoiceLines1.Count);
					AssertEquals(invoiceLines1[0].JI_Tariff, commodities1[0].HarmonizedCode);
				});

				// Let's change the tariff and NOT save to make sure that the cache is updated when InvoiceLines change.
				invoiceLines1[0].JI_Tariff = "55555555";

				var commodities2 = ((IComplianceCommodityRiskStatusProvider)declaration1).Commodities.ToArray(); // This is loaded not by the cache workflow because we've already loaded InvoiceLines and we can get data from them.

				CombineAssertions(() =>
				{
					AssertEquals(1, commodities2.Length);
					AssertEquals(1, invoiceLines1.Count);
					AssertEquals(invoiceLines1[0].JI_Tariff, commodities2[0].HarmonizedCode);
				});
			}
		}

		public void TestDeclarationInvoiceLineCache_CommoditiesLoadedFromCache()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<US.IJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.AddNewInvoiceLine();
				invoiceLine1.JI_Tariff = "33333333";
				Factory.Save();

				var factory1 = new BusinessObjectFactory();
				var declaration1 = factory1.Load<US.IJobDeclaration>(declaration.PK);
				var commodities1 = ((IComplianceCommodityRiskStatusProvider)declaration1).Commodities.ToArray();

				Assert(!declaration1.IsInvoiceLinesLoaded); // We've got commodities but InvoiceLines are not yet loaded.

				var invoiceLines1 = ((BaseJobDeclaration)declaration1).InvoiceLines; // This is loaded by the BusinessObject workflow.

				Assert(declaration1.IsInvoiceLinesLoaded); // Now InvoiceLines should be loaded.

				CombineAssertions(() =>
				{
					AssertEquals(1, commodities1.Length);
					AssertEquals(1, invoiceLines1.Count);
					AssertEquals(invoiceLines1[0].JI_Tariff, commodities1[0].HarmonizedCode);
				});

				// Let's change the tariff and NOT save to make sure that the cache is updated when InvoiceLines change.
				invoiceLines1[0].JI_Tariff = "55555555";

				var commodities2 = ((IComplianceCommodityRiskStatusProvider)declaration1).Commodities.ToArray(); // This is loaded not by the cache workflow because we've already loaded InvoiceLines and we can get data from them.

				CombineAssertions(() =>
				{
					AssertEquals(1, commodities2.Length);
					AssertEquals(1, invoiceLines1.Count);
					AssertEquals(invoiceLines1[0].JI_Tariff, commodities2[0].HarmonizedCode);
				});
			}
		}

		#endregion
	}
}
