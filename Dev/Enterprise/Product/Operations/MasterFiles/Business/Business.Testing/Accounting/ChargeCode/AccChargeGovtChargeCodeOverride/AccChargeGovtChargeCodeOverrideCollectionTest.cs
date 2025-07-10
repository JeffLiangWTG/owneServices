using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeGovtChargeCodeOverrideCollection))]
	sealed class AccChargeGovtChargeCodeOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var govtCodeOverride = chargeCode.GovtChargeCodeOverrides.AddNew();
			govtCodeOverride.ACG_CostSellAll = AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All;
			govtCodeOverride.ACG_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			govtCodeOverride.ACG_Direction = Core.Constants.FreightShipmentDirection.Code.Domestic;
			govtCodeOverride.ACG_TransportMode = Constants.TransportModes.Air;
			govtCodeOverride.ACG_GovtChargeCode = "123456";
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				govtCodeOverride.ACG_TransportMode = Constants.TransportModes.AirSea;
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.GovtChargeCodeOverrides.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestDefaults()
		{
			var collection = (AccChargeGovtChargeCodeOverrideCollection)GetCollectionToTest();
			var govtChargeCodeOverride = collection.AddNew();
			AssertEquals("ACG_AC", collection.Master.PK, govtChargeCodeOverride.ACG_AC);
			AssertEquals("Cost/Sell", "ALL", govtChargeCodeOverride.ACG_CostSellAll);
			AssertEquals("Job Type", ZString.Empty, govtChargeCodeOverride.ACG_JobType);
			AssertEquals("Transport Mode", "ALL", govtChargeCodeOverride.ACG_TransportMode);
			AssertEquals("Service Direction", "ALL", govtChargeCodeOverride.ACG_Direction);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
			=> new AccChargeGovtChargeCodeOverrideCollection(Factory.NewWithValidTestData<AccChargeCode>());

		public void TestAddRemove()
		{
			var collection = (AccChargeGovtChargeCodeOverrideCollection)GetCollectionToTest();
			var item = collection.AddNew();
			AssertEquals("Add: AO_ParentID", collection.Master.PK, item.ACG_AC);

			collection.RemoveAll();
			AssertEquals("Remove: AO_ParentID", ZGuid.Empty, item.ACG_AC);
		}

		public void TestGetGovtChargeCode()
		{
			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyVoyageAccountingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Domestic, Constants.TransportModes.All),
				},
				notEqualExpecteds: null,
				fallbackToDefaults: null
			);

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Domestic, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.CrossTrade, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: null,
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
				},
				notEqualExpecteds: null,
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.All),
				}
			);

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: null,
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
				}
			);
		}

		public void TestGetGovtChargeCodes()
		{
			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyVoyageAccountingCode, Directions.Import, Constants.TransportModes.All),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
				},
				fallbackToDefaults: null
			);

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Domestic, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Unknown, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
				},
				fallbackToDefaults: new[] {
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Sea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.AirSea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.All),
				},
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Sea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.AirSea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.All),
				},
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.BrokerageCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.InlandWaterwayTransport),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.BrokerageCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.FixedTransportInstallations),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.BrokerageCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Rail),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.BrokerageCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.BrokerageCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.InlandWaterwayTransport),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.BrokerageCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.BrokerageCode, Directions.Import, Constants.TransportModes.InlandWaterwayTransport),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.BrokerageCode, Directions.Export, Constants.TransportModes.FixedTransportInstallations),
					(CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.Export, Constants.TransportModes.Rail),
					(CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.Import, Constants.TransportModes.Sea),
				},
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.BrokerageCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.QuotedBookingCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Road),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.QuotedBookingCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.Sea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.QuotedBookingCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.QuotedBookingCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Road),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.QuotedBookingCode, Core.Constants.FreightShipmentDirection.Code.Domestic, Constants.TransportModes.Road),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.QuotedBookingCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Road),
				},
				equalExpecteds: new[] {
					(CostSell.Revenue, JobInvoicingConsumerTypes.QuotedBookingCode, Directions.Export, Constants.TransportModes.Road),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.QuotedBookingCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.QuotedBookingCode, Directions.Domestic, Constants.TransportModes.Road)
				},
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, Directions.Export, Constants.TransportModes.Road),
				});

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.OneOffQuotationCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Road),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.OneOffQuotationCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.Sea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.OneOffQuotationCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.OneOffQuotationCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Road),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.OneOffQuotationCode, Core.Constants.FreightShipmentDirection.Code.Domestic, Constants.TransportModes.Rail),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.OneOffQuotationCode, Directions.Import, Constants.TransportModes.Road),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.OneOffQuotationCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.OneOffQuotationCode, Directions.Domestic, Constants.TransportModes.Rail)
				},
				fallbackToDefaults: new[] {
					(CostSell.Revenue, JobInvoicingConsumerTypes.BrokerageCode, Directions.Import, Constants.TransportModes.Road),
				});

			AssertGetGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost, JobInvoicingConsumerTypes.ForwardingConsolCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Revenue, JobInvoicingConsumerTypes.QuotedBookingCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All)
				},
				equalExpecteds: new[]
				{
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Air),
				},
				notEqualExpecteds: new []
				{
					(CostSell.Cost, JobInvoicingConsumerTypes.ForwardingConsolCode, Directions.Import, Constants.TransportModes.Storage),
					(CostSell.Revenue, JobInvoicingConsumerTypes.QuotedBookingCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.QuotedBookingCode, Directions.Export, Constants.TransportModes.Sea),
				},
				fallbackToDefaults: new []
				{
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ForwardingConsolCode, Directions.Export, Constants.TransportModes.Air),
					(CostSell.Cost, JobInvoicingConsumerTypes.QuotedBookingCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ForwardingConsolCode, Directions.Import, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.GatewayConsolCode, Directions.Import, Constants.TransportModes.Storage),
				}
			);
		}

		void AssertGetGovtChargeCode((ZString CostSellAll, ZString JobType, ZString ServiceDirectionCode, ZString TransportMode) expected,
			(string CostSellAll, string JobType, string ServiceDirection, string TransportMode)[] settings,
			(CostSell CostOrSell, string JobType, Directions ServiceDirection, string TransportMode)[] equalExpecteds,
			(CostSell CostOrSell, string JobType, Directions ServiceDirection, string TransportMode)[] notEqualExpecteds,
			(CostSell CostOrSell, string JobType, Directions ServiceDirection, string TransportMode)[] fallbackToDefaults)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			foreach (var setting in settings)
			{
				var overrideGovt = chargeCode.GovtChargeCodeOverrides.AddNew();
				overrideGovt.ACG_CostSellAll = setting.CostSellAll;
				overrideGovt.ACG_JobType = setting.JobType;
				overrideGovt.ACG_Direction = setting.ServiceDirection;
				overrideGovt.ACG_TransportMode = setting.TransportMode;
				overrideGovt.ACG_GovtChargeCode = "1234";
			}

			Factory.Save();

			var expectedGovt = chargeCode.GovtChargeCodeOverrides
				.Select(x => x)
				.FirstOrDefault(x =>
					x.ACG_JobType == expected.JobType
					&& x.ACG_CostSellAll == expected.CostSellAll
					&& x.ACG_Direction == expected.ServiceDirectionCode
					&& x.ACG_TransportMode == expected.TransportMode);

			AssertNotNull(expectedGovt);
			Assert(expectedGovt.PK.IsValid);

			CombineAssertions(() =>
			{
				foreach (var equalExpected in equalExpecteds)
				{
					AssertEquals(expectedGovt.PK,
						chargeCode.GovtChargeCodeOverrides.GetGovtChargeCode(Factory, chargeCode.PK,
							ConfigurationMatcherHelper.GetParameters(
								equalExpected.CostOrSell,
								equalExpected.JobType,
								equalExpected.ServiceDirection,
								equalExpected.TransportMode,
								GlbBranch.CurrentBranch, null, null)
						).PK
					);
				}
			});

			if (notEqualExpecteds != null)
			{
				Assert(notEqualExpecteds.Any());
				CombineAssertions(() =>
				{
					foreach (var notEqualExpected in notEqualExpecteds)
					{
						AssertNotEquals(expectedGovt.PK,
							chargeCode.GovtChargeCodeOverrides.GetGovtChargeCode(Factory, chargeCode.PK,
							ConfigurationMatcherHelper.GetParameters(
								notEqualExpected.CostOrSell,
								notEqualExpected.JobType,
								notEqualExpected.ServiceDirection,
								notEqualExpected.TransportMode,
								GlbBranch.CurrentBranch, null, null)
							).PK
						);
					}
				});
			}

			if (fallbackToDefaults != null)
			{
				Assert(fallbackToDefaults.Any());
				CombineAssertions(() =>
				{
					foreach (var fallbackToDefault in fallbackToDefaults)
					{
						AssertNull(
							chargeCode.GovtChargeCodeOverrides.GetGovtChargeCode(Factory, chargeCode.PK,
							ConfigurationMatcherHelper.GetParameters(
								fallbackToDefault.CostOrSell,
								fallbackToDefault.JobType,
								fallbackToDefault.ServiceDirection,
								fallbackToDefault.TransportMode,
								GlbBranch.CurrentBranch, null, null)
							)
						);
					}
				});
			}

			expectedGovt.Delete();
			chargeCode.Delete();
			Factory.Save();
		}

		public void TestGetDirectionCode()
		{
			var expectedList = new[] {
				(Directions.Unknown, (string)ZString.Empty),
				(Directions.Import, Core.Constants.FreightShipmentDirection.Code.Import),
				(Directions.Export, Core.Constants.FreightShipmentDirection.Code.Export),
				(Directions.Domestic, Core.Constants.FreightShipmentDirection.Code.Domestic),
				(Directions.CrossTrade, Core.Constants.FreightShipmentDirection.Code.Other),
			};
			AssertArrayEqualsByElements(
				Enum.GetNames(typeof(Directions)).OrderBy(x => x).ToArray(),
				expectedList.Select(x => x.Item1.ToString()).OrderBy(x => x).ToArray()
			);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			foreach (var expectedSetting in expectedList)
			{
				AssertEquals(expectedSetting.Item2, chargeCode.GovtChargeCodeOverrides.GetDirectionCode(expectedSetting.Item1));
			}

			AssertEquals(ZString.Empty, chargeCode.GovtChargeCodeOverrides.GetDirectionCode(null));
		}
	}
}
