using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Moq;
using IForwardingConsol = Enterprise.Integration.Forwarding.IForwardingConsol;
using IForwardingShipment = Enterprise.Integration.Forwarding.IForwardingShipment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ComplianceRiskHelperTest : TestCaseWithFactory
	{
		public void TestCheckIfComplianceRiskEnabled()
		{
			AssertCheckIfComplianceRiskEnabledCore("DummyBusinessObject 1", typeof(DummyBusinessObject), enableComplianceRisk: false, allowViewType: false, expectedResult: false);
			AssertCheckIfComplianceRiskEnabledCore("DummyBusinessObject 2", typeof(DummyBusinessObject), enableComplianceRisk: true, allowViewType: false, expectedResult: false);
			AssertCheckIfComplianceRiskEnabledCore("DummyBusinessObject 3", typeof(DummyBusinessObject), enableComplianceRisk: false, allowViewType: true, expectedResult: false);
			AssertCheckIfComplianceRiskEnabledCore("DummyBusinessObject 4", typeof(DummyBusinessObject), enableComplianceRisk: true, allowViewType: true, expectedResult: false);
			AssertCheckIfComplianceRiskEnabledCore("DummyWithComplianceRiskStatusProvider 1", typeof(DummyWithComplianceRiskStatusProvider), enableComplianceRisk: false, allowViewType: false, expectedResult: false);
			AssertCheckIfComplianceRiskEnabledCore("DummyWithComplianceRiskStatusProvider 2", typeof(DummyWithComplianceRiskStatusProvider), enableComplianceRisk: true, allowViewType: false, expectedResult: true);
			AssertCheckIfComplianceRiskEnabledCore("DummyWithComplianceRiskStatusProvider 3", typeof(DummyWithComplianceRiskStatusProvider), enableComplianceRisk: false, allowViewType: true, expectedResult: false);
			AssertCheckIfComplianceRiskEnabledCore("DummyWithComplianceRiskStatusProvider 4", typeof(DummyWithComplianceRiskStatusProvider), enableComplianceRisk: true, allowViewType: true, expectedResult: true);
			AssertCheckIfComplianceRiskEnabledCore("DummyWithViewComplianceRiskStatusProvider 1", typeof(DummyWithViewComplianceRiskStatusProvider), enableComplianceRisk: false, allowViewType: false, expectedResult: false);
			AssertCheckIfComplianceRiskEnabledCore("DummyWithViewComplianceRiskStatusProvider 2", typeof(DummyWithViewComplianceRiskStatusProvider), enableComplianceRisk: true, allowViewType: false, expectedResult: false);
			AssertCheckIfComplianceRiskEnabledCore("DummyWithViewComplianceRiskStatusProvider 3", typeof(DummyWithViewComplianceRiskStatusProvider), enableComplianceRisk: false, allowViewType: true, expectedResult: false);
			AssertCheckIfComplianceRiskEnabledCore("DummyWithViewComplianceRiskStatusProvider 4", typeof(DummyWithViewComplianceRiskStatusProvider), enableComplianceRisk: true, allowViewType: true, expectedResult: true);

			void AssertCheckIfComplianceRiskEnabledCore(string message, Type bizoType, bool enableComplianceRisk, bool allowViewType, bool expectedResult)
			{
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
				{
					AssertEquals(message, expectedResult, ComplianceRiskHelper.CheckIfComplianceRiskEnabled(bizoType, allowViewType));
				}
			}
		}

		public void TestCheckIfComplianceRiskEnabledWithBizO()
		{
			AssertCheckIfComplianceRiskEnabledCore((BusinessObject)Factory.New<IForwardingShipment>(), true, true);
			AssertCheckIfComplianceRiskEnabledCore((BusinessObject)Factory.New<IForwardingShipment>(), false, false);
			AssertCheckIfComplianceRiskEnabledCore((BusinessObject)Factory.New<IForwardingConsol>(), true, true);
			AssertCheckIfComplianceRiskEnabledCore((BusinessObject)Factory.New<IForwardingConsol>(), false, false);
			var booking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			AssertCheckIfComplianceRiskEnabledCore((BusinessObject)booking, true, true);
			AssertCheckIfComplianceRiskEnabledCore((BusinessObject)booking, false, false);

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			AssertCheckIfComplianceRiskEnabledDeclaration(declaration as BusinessObject, true, true);
			AssertCheckIfComplianceRiskEnabledDeclaration(declaration as BusinessObject, false, false);

			void AssertCheckIfComplianceRiskEnabledCore(BusinessObject jobBizO, bool enableCompliance, bool expectedResult)
			{
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
						ComplianceWiseRegistryHelper.SetValue(enableCompliance)))
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableCompliance))
				{
					AssertEquals(expectedResult, ComplianceRiskHelper.CheckIfComplianceRiskEnabled(jobBizO));
				}
			}

			void AssertCheckIfComplianceRiskEnabledDeclaration(BusinessObject jobBizO, bool enableCompliance, bool expectedResult)
			{
				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enableCompliance);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				using (ObjectFactory.Substitute(featureControlMock.Object))
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals(expectedResult, ComplianceRiskHelper.CheckIfComplianceRiskEnabled(jobBizO));
				}
			}
		}

		void AssertJobEndDateAndIsCurrent(string message, ZString? jobStatus, ZDateTime jsDepartTime, ZDateTime jsArriveTime, BusinessObject parentBizO, IEnumerable<ComplianceRouting> routings, ZDateTime expectJobEndDate, bool expectIsCurrent)
		{
			var (actuallyIsCurrent, actuallyJobEndDate) = ComplianceRiskHelper.GetJobEndDateAndIsCurrent(jobStatus, jsDepartTime, jsArriveTime, parentBizO,routings);
			CombineAssertions(message,() =>
			{
				AssertDateTimeWithinOneSecond( "Job End Date", actuallyJobEndDate.ToDateTime(), expectJobEndDate.ToDateTime());
				AssertEquals("IsCurrent", expectIsCurrent, actuallyIsCurrent);
			});
		}

		public void TestCheckJobEndDateAndIfComplianceRiskJobCurrent()
		{
			using(OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 12))
			{
				var source = Factory.NewWithValidTestData<OrgHeader>();

				AssertJobEndDateAndIsCurrent("When job status is closed, job should not be current", JobHeaderStatus.Closed.Code, ZDateTime.Now, ZDateTime.Empty, source, null, ZDateTime.Now, false);

				AssertJobEndDateAndIsCurrent("When job is not saved to database yet, job should be current", JobHeaderStatus.Working.Code, ZDateTime.Empty, ZDateTime.Empty, source, null, ZDateTime.Now.AddMonths(OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value), true);

				var dummyBizOForTest = Factory.New<DummyBusinessObjectForCurrentJobTest>();
				dummyBizOForTest.IsSavedByFactoryOverride = true;
				AssertJobEndDateAndIsCurrent("When job's not saved by factory and not in database, job should still be current", JobHeaderStatus.Working.Code, ZDateTime.Empty, ZDateTime.Empty, dummyBizOForTest, null, ZDateTime.Now.AddMonths(OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value), true);

				dummyBizOForTest.IsSavedByFactoryOverride = true;
				dummyBizOForTest.SetIsInDatabase(true);
				AssertJobEndDateAndIsCurrent("When ETD or ETA is less than 7 days old, job should be current", JobHeaderStatus.Working.Code, ZDateTime.Now, ZDateTime.Now, dummyBizOForTest, null, ZDateTime.Now, true);

				dummyBizOForTest.SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-5);
				AssertJobEndDateAndIsCurrent("When ADD event time is in 12 months, job should be current", JobHeaderStatus.Working.Code, ZDateTime.Empty, ZDateTime.Empty, dummyBizOForTest, null, ZDateTime.Now.AddMonths(OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value - 5), true);

				dummyBizOForTest.SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-13);
				AssertJobEndDateAndIsCurrent("When ADD event time is more than 12 months, job should not be current", JobHeaderStatus.Working.Code, ZDateTime.Empty, ZDateTime.Empty, dummyBizOForTest, null, ZDateTime.Now.AddMonths(OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value - 13), false);

				var routing = new ComplianceRouting[] { new ComplianceRouting { ATD = ZDateTime.Empty, ATA = ZDateTime.Empty, ETD = ZDateTime.Empty, ETA = ZDateTime.Now.AddDays(-5) } };
				AssertJobEndDateAndIsCurrent("When ADD event time is more than 12 months, but with a routing's ETA less than 7 days old, job should not current. Job End Date should be 7days before the newest date.", JobHeaderStatus.Working.Code, ZDateTime.Now.AddDays(-6), ZDateTime.Now.AddDays(-7), dummyBizOForTest, routing, ZDateTime.Now.AddDays(-5), true);
			}
		}

		public void TestRegistryCommercialInvoiceEnabled()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				AssertEquals("Global Commercial Invoice feature registry: true", true, ComplianceRiskHelper.IsGlobalCommercialInvoiceEnabled);
			}

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(false)))
			{
				AssertEquals("Global Commercial Invoice feature registry: false", false, ComplianceRiskHelper.IsGlobalCommercialInvoiceEnabled);
			}
		}

		public void TestRegistryFreightEnableComplianceWise()
		{
			AssertModuleRegistry(module: true, expectedResult: true);
			AssertModuleRegistry(module: false, expectedResult: false);

			void AssertModuleRegistry(bool module, bool expectedResult)
			{
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(module)))
				{
					AssertEquals("Module registry", expectedResult, ComplianceRiskHelper.IsFreightEnabledComplianceWise);
				}
			}
		}

		public void TestRegistryLinerAgencyEnableComplianceWise()
		{
			AssertModuleRegistry(module: true, expectedResult: true);
			AssertModuleRegistry(module: false, expectedResult: false);

			void AssertModuleRegistry(bool module, bool expectedResult)
			{
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(module)))
				{
					AssertEquals("Module registry", expectedResult, ComplianceRiskHelper.IsLinerAgencyEnabledComplianceWise);
				}
			}
		}

		public void TestRegistryCustomsEnableComplianceWise()
		{
			AssertModuleRegistry(enableComplianceRisk: true, licenceFeatureEnabled: true, expectedResult: true);
			AssertModuleRegistry(enableComplianceRisk: true, licenceFeatureEnabled: false, expectedResult: false);
			AssertModuleRegistry(enableComplianceRisk: true, licenceFeatureEnabled: null, expectedResult: false);
			AssertModuleRegistry(enableComplianceRisk: false, licenceFeatureEnabled: true, expectedResult: false);

			void AssertModuleRegistry(bool enableComplianceRisk, bool? licenceFeatureEnabled, bool expectedResult)
			{
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
				{
					if (licenceFeatureEnabled.HasValue)
					{
						var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = licenceFeatureEnabled.Value };
						var featureDataMock = new Mock<IFeatureData>();
						var featureControlMock = new Mock<IFeatureControlManager>();
						featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
						featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

						using (ObjectFactory.Substitute(featureControlMock.Object))
						{
							AssertEquals(licenceFeatureEnabled.Value, ComplianceRiskFeatureControlHelper.HasIntegrateComplinaceWiseToCustomsDeclarationModuleEnabled());
							AssertEquals(expectedResult, ComplianceRiskHelper.IsCustomsEnabledComplianceWise);
						}
					}
					else
					{
						AssertEquals(false, ComplianceRiskFeatureControlHelper.HasIntegrateComplinaceWiseToCustomsDeclarationModuleEnabled());
						AssertEquals("Module registry", expectedResult, ComplianceRiskHelper.IsCustomsEnabledComplianceWise);
					}
				}
			}
		}

		public void TestIsComplianceCommodityScreeningEnable()
		{
			AssertIsComplianceCommodityScreeningEnable(enableComplianceRisk: true, licenceFeatureEnabled: true, expectedResult: true);
			AssertIsComplianceCommodityScreeningEnable(enableComplianceRisk: true, licenceFeatureEnabled: false, expectedResult: false);
			AssertIsComplianceCommodityScreeningEnable(enableComplianceRisk: true, licenceFeatureEnabled: null, expectedResult: false);
			AssertIsComplianceCommodityScreeningEnable(enableComplianceRisk: false, licenceFeatureEnabled: true, expectedResult: false);

			void AssertIsComplianceCommodityScreeningEnable(bool enableComplianceRisk, bool? licenceFeatureEnabled, bool expectedResult)
			{
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				{
					if (licenceFeatureEnabled.HasValue)
					{
						var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = licenceFeatureEnabled.Value };
						var featureDataMock = new Mock<IFeatureData>();
						var featureControlMock = new Mock<IFeatureControlManager>();
						featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
						featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ComplianceWiseCommodityScreening, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

						using (ObjectFactory.Substitute(featureControlMock.Object))
						{
							AssertEquals(licenceFeatureEnabled.Value, ComplianceRiskFeatureControlHelper.HasComplianceWiseCommodityScreeningEnable());
							AssertEquals(expectedResult, ComplianceRiskHelper.IsComplianceCommodityScreeningEnable);
						}
					}
					else
					{
						AssertEquals(false, ComplianceRiskFeatureControlHelper.HasComplianceWiseCommodityScreeningEnable());
						AssertEquals(expectedResult, ComplianceRiskHelper.IsComplianceCommodityScreeningEnable);
					}
				}
			}
		}

		public void TestIsCustomsEnabledManageRiskStatusOnCommercialInvoice()
		{
			AssertModuleRegistry(enableCustomsFeature: true, enablCommodityScreenFeature: true, enableManageRiskStatusOnInvoice: true, expectedResult: true);
			AssertModuleRegistry(enableCustomsFeature: true, enablCommodityScreenFeature: true, enableManageRiskStatusOnInvoice: false, expectedResult: false);
			AssertModuleRegistry(enableCustomsFeature: true, enablCommodityScreenFeature: true, enableManageRiskStatusOnInvoice: null, expectedResult: false);
			AssertModuleRegistry(enableCustomsFeature: true, enablCommodityScreenFeature: false, enableManageRiskStatusOnInvoice: true, expectedResult: false);
			AssertModuleRegistry(enableCustomsFeature: false, enablCommodityScreenFeature: true, enableManageRiskStatusOnInvoice: true, expectedResult: false);

			void AssertModuleRegistry(bool enableCustomsFeature, bool enablCommodityScreenFeature, bool? enableManageRiskStatusOnInvoice, bool expectedResult)
			{
				var complianceWiseEnableCustoms = new ComplianceRiskFeatureControlRule { Enabled = enableCustomsFeature };
				var complianceWiseEnableCommodityScreen = new ComplianceRiskFeatureControlRule { Enabled = enablCommodityScreenFeature };
				var featureDataMock1 = new Mock<IFeatureData>();
				featureDataMock1.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseEnableCustoms)).Returns(true);
				var featureDataMock2 = new Mock<IFeatureData>();
				featureDataMock2.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseEnableCommodityScreen)).Returns(true);
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock1.Object));
				featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ComplianceWiseCommodityScreening, CancellationToken.None)).Returns(Task.FromResult(featureDataMock2.Object));

				if (enableManageRiskStatusOnInvoice.HasValue)
				{
					var complianceWiseEnableManageRisk = new ComplianceRiskFeatureControlRule { Enabled = enableManageRiskStatusOnInvoice.Value };
					var featureDataMock3 = new Mock<IFeatureData>();
					featureDataMock3.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseEnableManageRisk)).Returns(true);
					featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice, CancellationToken.None)).Returns(Task.FromResult(featureDataMock3.Object));
				}

				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ObjectFactory.Substitute(featureControlMock.Object))
				{
					if (enableManageRiskStatusOnInvoice.HasValue)
					{
						AssertEquals(enableManageRiskStatusOnInvoice.Value, ComplianceRiskFeatureControlHelper.HasIntegrateComplinaceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice());
						AssertEquals(expectedResult, ComplianceRiskHelper.IsCustomsEnabledManageRiskStatusOnCommercialInvoice);
					}
					else
					{
						AssertEquals(false, ComplianceRiskFeatureControlHelper.HasIntegrateComplinaceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice());
						AssertEquals(expectedResult, ComplianceRiskHelper.IsCustomsEnabledManageRiskStatusOnCommercialInvoice);
					}
				}
			}
		}

		public void TestGetCommercialInvoiceLines()
		{
			var declaration1 = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declaration2 = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declaration3 = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var invoice1 = declaration1.Invoices.AddNew();

			var invoiceLine1 = invoice1.AddNewInvoiceLine();
			invoiceLine1.JI_PartNo = "HXUTEST";
			invoiceLine1.JI_Tariff = ZString.Empty;

			var invoiceLine2 = invoice1.AddNewInvoiceLine();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_Tariff = "11111111";
			invoiceLine2.JI_NDescription = "TESTING1";
			invoiceLine2.JI_CountryOfOrigin = "CN";

			var invoiceLine3 = invoice1.AddNewInvoiceLine();
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine3.JI_Tariff = "22222222";
			invoiceLine3.JI_NDescription = "TESTING2";

			var invoice2 = declaration2.Invoices.AddNew();

			var invoiceLine4 = invoice2.AddNewInvoiceLine();
			invoiceLine4.JI_PartNo = "HXUTEST";
			invoiceLine4.JI_Tariff = "33333333";
			invoiceLine4.JI_Description = "Description";
			invoiceLine4.JI_NDescription = "NDescription";

			var invoiceLine5 = invoice2.AddNewInvoiceLine();
			invoiceLine5.JI_PartNo = "HXUTEST";
			invoiceLine5.JI_Tariff = "   ";

			Factory.Save();

			var parentId = ZGuid.NewZGuid();
			var invoiceLines = ComplianceRiskHelper.GetCommercialInvoiceLines(new[] { declaration1.JE_ClusterKey, declaration2.JE_ClusterKey, declaration3.JE_ClusterKey }, "WCO", "DUMMY Source", parentId, "Commodity Source", Factory);
			AssertEquals(3, invoiceLines.Count);

			var declarationLines = invoiceLines[declaration1.JE_ClusterKey].Concat(invoiceLines[declaration2.JE_ClusterKey]).Concat(invoiceLines[declaration3.JE_ClusterKey]);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				(invoiceLine2.JI_CountryOfOrigin, parentId, "WCO", invoiceLine2.JI_Tariff, "DUMMY Source", "Commodity Source", "TESTING1"),
				(invoiceLine3.JI_CountryOfOrigin, parentId, "WCO", invoiceLine3.JI_Tariff, "DUMMY Source", "Commodity Source", "TESTING2"),
				(invoiceLine4.JI_CountryOfOrigin, parentId, "WCO", invoiceLine4.JI_Tariff, "DUMMY Source", "Commodity Source", "Description"),
			}, declarationLines.Select(u => (u.Origin, u.ParentJobID, u.GroupingOrCountry.ToString(), u.HarmonizedCode, u.Source.ToString(), u.CommoditySource.ToString(), u.GoodsDescription.ToString())));
		}

		public void TestGetCommercialInvoiceLines_Declaration()
		{
			var declaration1 = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var invoice1 = declaration1.Invoices.AddNew();

			var invoiceLine1 = invoice1.AddNewInvoiceLine();
			invoiceLine1.JI_PartNo = "HXUTEST";
			invoiceLine1.JI_Tariff = ZString.Empty;
			invoiceLine1.JI_NDescription = "EMPTYTARIFF1";
			invoiceLine1.JI_CountryOfOrigin = "AU";

			var invoiceLine2 = invoice1.AddNewInvoiceLine();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_Tariff = "11111111";
			invoiceLine2.JI_NDescription = "TESTING1";
			invoiceLine2.JI_CountryOfOrigin = "CN";

			var invoiceLine3 = invoice1.AddNewInvoiceLine();
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine3.JI_Tariff = "22222222";
			invoiceLine3.JI_NDescription = "TESTING2";
			invoiceLine3.JI_CountryOfOrigin = "US";

			var invoice2 = declaration1.Invoices.AddNew();

			var invoiceLine4 = invoice2.AddNewInvoiceLine();
			invoiceLine4.JI_PartNo = "HXUTEST";
			invoiceLine4.JI_Tariff = "33333333";
			invoiceLine4.JI_Description = "Description";
			invoiceLine4.JI_NDescription = "NDescription";
			invoiceLine4.JI_CountryOfOrigin = "AU";

			var invoiceLine5 = invoice2.AddNewInvoiceLine();
			invoiceLine5.JI_PartNo = "HXUTEST";
			invoiceLine5.JI_Tariff = "   ";
			invoiceLine5.JI_NDescription = "EMPTYTARIFF2";
			invoiceLine5.JI_CountryOfOrigin = "NZ";

			var invoiceLine6 = invoice2.AddNewInvoiceLine();
			invoiceLine6.JI_PartNo = "HXUTEST";
			invoiceLine6.JI_Tariff = ZString.Empty;
			invoiceLine6.JI_NDescription = "EMPTYTARIFF3";
			invoiceLine6.JI_CountryOfOrigin = ZString.Empty;

			Factory.Save();

			var parentId = ZGuid.NewZGuid();
			var invoiceLineCache = ComplianceRiskHelper.GetCommercialInvoiceLines(declaration1.JE_ClusterKey, "WCO", "DUMMY Source", parentId, "Commodity Source", Factory);

			AssertEquals(3, invoiceLineCache.Commodities.Length);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				(invoiceLine2.JI_CountryOfOrigin, parentId, "WCO", invoiceLine2.JI_Tariff, "DUMMY Source", "Commodity Source", "TESTING1"),
				(invoiceLine3.JI_CountryOfOrigin, parentId, "WCO", invoiceLine3.JI_Tariff, "DUMMY Source", "Commodity Source", "TESTING2"),
				(invoiceLine4.JI_CountryOfOrigin, parentId, "WCO", invoiceLine4.JI_Tariff, "DUMMY Source", "Commodity Source", "Description"),
			}, invoiceLineCache.Commodities.Select(u => (u.Origin, u.ParentJobID, u.GroupingOrCountry.ToString(), u.HarmonizedCode, u.Source.ToString(), u.CommoditySource.ToString(), u.GoodsDescription.ToString())));

			AssertEquals(5, invoiceLineCache.CountriesOfOrigin.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "CN", "US", "AU", "NZ" }, invoiceLineCache.CountriesOfOrigin);
		}

		public void TestRegistryJobEntitiesCachingEnabled()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetJobEntitiesCaching(true)))
			{
				AssertEquals("Job Entities Caching feature registry: true", true, ComplianceRiskHelper.IsJobEntitiesCachingEnabled);
			}

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetJobEntitiesCaching(false)))
			{
				AssertEquals("Job Entities Caching feature registry: false", false, ComplianceRiskHelper.IsJobEntitiesCachingEnabled);
			}
		}
	}

	class DummyWithComplianceRiskStatusProvider : DummyBusinessObject, ICompliancePartyRiskStatusProvider, IComplianceLocationRiskStatusProvider, IComplianceCommodityRiskStatusProvider
	{
		public DummyWithComplianceRiskStatusProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDateTime EffectiveDate => throw new NotImplementedException();
		public IEnumerable<IScreeningParty> Parties => throw new NotImplementedException();
		public IEnumerable<IComplianceLocation> Locations => throw new NotImplementedException();
		public IEnumerable<IComplianceCommodity> Commodities => throw new NotImplementedException();
		public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => throw new NotImplementedException();
		public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => throw new NotImplementedException();
		public ZGuid ParentID => throw new NotImplementedException();
		public ZString ParentTableCode => throw new NotImplementedException();
		public ComplianceRiskSupport ComplianceRiskSupport { get; } = ComplianceRiskSupport.None;
		public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (false, ZDateTime.BrettsBirthday);
		public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => throw new NotImplementedException();
		public ZBool IsEnabledComplianceWise => true;
		public ZBool IsEditingCommoditySupported => true;
		CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;
		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();
		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();
		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();
		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => throw new NotImplementedException();
		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => throw new NotImplementedException();
		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => throw new NotImplementedException();
		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => throw new NotImplementedException();
	}

	[ViewComplianceRiskStatusProvider(ProviderBusinessObjectType = typeof(DummyWithComplianceRiskStatusProvider))]
	class DummyWithViewComplianceRiskStatusProvider : DummyWithViewQuotedBooking
	{
		public DummyWithViewComplianceRiskStatusProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}

	class DummyWithViewQuotedBooking : DummyBusinessObject, IViewQuotedBooking, IViewComplianceRiskStatusProvider
	{
		public DummyWithViewQuotedBooking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		#region IDeniedPartyScreeningPartyProvider Members

		public ZGuid VB_JS { get; set; }

		public ZGuid VB_TH { get; set; }

		public ZGuid VB_GC { get; set; }

		public IJobHeader Job { get; }

		IComplianceItemRiskStatusProvider IViewComplianceRiskStatusProvider.GetProviderBusinessObject() => throw new NotImplementedException();

		#endregion
	}

	class DummyBusinessObjectForCurrentJobTest : DummyBusinessObject, IStmALogParent, IAuditDetails
	{
		public DummyBusinessObjectForCurrentJobTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			OverrideIsSavedByFactory = true;
			IsSavedByFactoryOverride = false;
		}

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => [];

		bool IStmALogParent.IsDeleted => throw new NotImplementedException();

		ZGuid IStmALogParent.LogsParentPK => PK;

		string IStmALogParent.LogsParentTableName => throw new NotImplementedException();

		bool IStmALogParent.DeferFiringWorkflow => throw new NotImplementedException();

		Logs IStmALogProvider.Logs => throw new NotImplementedException();

		BusinessObjectFactory IStmALogProvider.LogsFactory => throw new NotImplementedException();

		void IStmALogParent.ProcessLog(IStmALog log)
		{
			throw new NotImplementedException();
		}

		public ZDateTime SystemCreateTimeUtc { get; set; }

		public ZString SystemCreateUser { get; set; }

		public ZDateTime SystemLastEditTimeUtc { get; set; }

		public ZString SystemLastEditUser { get; set; }
	}
}
