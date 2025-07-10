using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.DataTransfer.Universal.AirManifest;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.UniversalDataBuss.DataObjects;

	partial class AirManifestDataObjectReaderTest
	{
		public void TestHAWBHasActiveMessage_NoErrorReported()
		{
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			hawb.CS_HAWB = hawbDataObject.WayBillNumber.Value;
			Factory.SaveForTesting();

			Assert("Precondition: hawb has active message", ETailCusHAWBDataObjectReader.IsMessagingActive(hawb));
			AssertEquals("Precondition, hawb is matched", hawb, Reader.ReadIntoBusinessObject());

			Assert("Should be no error", !Reader.Logger.Logs.Any(m => m.Type == Integration.LogType.Error));
		}

		public void TestHAWBHasActiveMessage_ShouldSkipReading()
		{
			Reader.FillHouseBill(hawb);
			AssertEquals("Precondition: goods value should be updated to 5.28M if hawb has no active message", 5.281M, hawb.CS_GoodsValue);

			hawb.CS_GoodsValue = 2.44M;
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			hawb.CS_HAWB = hawbDataObject.WayBillNumber.Value;
			Factory.SaveForTesting();

			Assert("Precondition: hawb has active message", ETailCusHAWBDataObjectReader.IsMessagingActive(hawb));
			AssertEquals("Precondition, hawb is matched", hawb, Reader.ReadIntoBusinessObject());

			AssertEquals("Precondition, hawb goods value stays 0 as reader skipped it", 2.44M, hawb.CS_GoodsValue);
		}

		public void TestETailHAWBGoodsValueMapping_FallbackWhenNull()
		{
			Reader.FillHouseBill(hawb);
			AssertEquals(5.281M, hawb.CS_GoodsValue);

			hawbDataObject.CommercialInfo = null;

			Reader.FillHouseBill(hawb);
			AssertEquals(6.178M, hawb.CS_GoodsValue);
		}

		public void TestETailHAWBGoodsValueMapping_FallbackWhenZero()
		{
			Reader.FillHouseBill(hawb);
			AssertEquals(5.281M, hawb.CS_GoodsValue);

			hawbDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single().CustomsValue = 0;
			Reader.FillHouseBill(hawb);
			AssertEquals(6.178M, hawb.CS_GoodsValue);
		}

		public void TestETailHAWBGoodsDescriptionMapping_FallbackWhenNull()
		{
			Reader.FillHouseBill(hawb);
			AssertEquals("INVOICE DESC", hawb.CS_GoodsDescription);

			hawbDataObject.CommercialInfo = null;

			Reader.FillHouseBill(hawb);
			AssertEquals("BAG OF CRAP", hawb.CS_GoodsDescription);

			hawbDataObject.PackingLineCollection.Single().GoodsDescription = null;

			Reader.FillHouseBill(hawb);
			AssertEquals("GOODS FOR TESTING", hawb.CS_GoodsDescription);
		}

		public void TestHouseBillGoodsDescriptionMapping_FallbackWhenEmpty()
		{
			hawbDataObject.PackingLineCollection.Single().GoodsDescription = string.Empty;
			Reader.FillHouseBill(hawb);
			AssertEquals("INVOICE DESC", hawb.CS_GoodsDescription);

			hawbDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single().Description = string.Empty;
			Reader.FillHouseBill(hawb);
			AssertEquals("GOODS FOR TESTING", hawb.CS_GoodsDescription);
		}

		public void TestETailHAWBGoodsOriginMapping_FallbackWhenNull()
		{
			Reader.FillHouseBill(hawb);
			AssertEquals("CN", hawb.CS_RN_NKGoodsOrigin);

			hawbDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single().CustomsSupportingInformationCollection = null;

			Reader.FillHouseBill(hawb);
			AssertEquals("CA", hawb.CS_RN_NKGoodsOrigin);

			hawbDataObject.CommercialInfo = null;

			Reader.FillHouseBill(hawb);
			AssertEquals("US", hawb.CS_RN_NKGoodsOrigin);
		}

		public void TestETailHAWBGoodsOriginMapping_FallbackWhenCodeIsEmpty()
		{
			Reader.FillHouseBill(hawb);
			AssertEquals("CN", hawb.CS_RN_NKGoodsOrigin);

			hawbDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single().CustomsSupportingInformationCollection.Single().Country.Code = ZString.Empty;

			Reader.FillHouseBill(hawb);
			AssertEquals("CA", hawb.CS_RN_NKGoodsOrigin);

			hawbDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single().CountryOfOrigin.Code = ZString.Empty;

			Reader.FillHouseBill(hawb);
			AssertEquals("US", hawb.CS_RN_NKGoodsOrigin);
		}

		public void TestETailHAWBPiecesManifestedMapping()
		{
			Reader.FillHouseBill(hawb);
			AssertEquals((ZShort)2, hawb.CS_PiecesManifested);

			var largeValue = (ZDecimal)int.MaxValue + 10.0m;
			hawbDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single().CustomsQuantity = largeValue;
			var exception = AssertExceptionThrown<DataObjectReadFailureException>(() => Reader.FillHouseBill(hawb));
			AssertEquals("Should include error information", $"Value '{largeValue}' of 'Customs Quantity' is too large. It cannot be greater than {int.MaxValue}", exception.Message);

			hawbDataObject.CommercialInfo = null;

			Reader.FillHouseBill(hawb);
			AssertEquals((ZShort)1, hawb.CS_PiecesManifested);
		}

		public void TestETailHAWHarmonisedTariffNumsMapping()
		{
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			AssertEquals("pre-condition", "IMP", mawb.EntryType);
			Reader.FillHouseBill(hawb);
			AssertEquals("1111.11.11.11", hawb.CS_HarmonisedTariffNums);

			mawb.CM_RL_NKLoadPort = "NZAKL";
			mawb.CM_RL_NKDischargePort = "USLAX";
			AssertEquals("pre-condition", "EXP", mawb.EntryType);
			Reader.FillHouseBill(hawb);

			Reader.FillHouseBill(hawb);
			AssertEquals("2222.22.22.22", hawb.CS_HarmonisedTariffNums);
		}

		public void TestOnlyFillSubsequenceOnesToHAWBItems()
		{
			Reader.FillHouseBillAdditionalItems(hawb);
			Assert(!hawb.CusHAWBItemsCollection.Any());

			var firstPackingLine = hawbDataObject.PackingLineCollection.Single();
			hawbDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance));

			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals(1, hawb.CusHAWBItemsCollection.Count);
			AssertEquals("GOODS FOR TESTING", hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().Single().CHI_GoodsDescription);

			firstPackingLine.PackedItemCollection.Add(new PackedItem() { CommercialInvoiceLineLink = 999 });
			hawbDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Add(new CommercialInvoiceLine()
			{
				Link = 999,
				Description = "Another Invoice Line"
			});

			hawbDataObject.PackingLineCollection.Clear();
			hawbDataObject.PackingLineCollection.Add(firstPackingLine);
			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals(1, hawb.CusHAWBItemsCollection.Count);
			AssertEquals("ANOTHER INVOICE LINE", hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().Last().CHI_GoodsDescription);
		}

		public void TestETailHAWBItemsWeightMapping_FallbackWhenZero()
		{
			var invoiceLine = AddAnotherPackedItemAndInvoiceLine();
			invoiceLine.Weight = 2.314;
			Reader.FillHouseBillAdditionalItems(hawb);

			AssertEquals(2.314M, hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_Weight);

			invoiceLine.Weight = 0;
			invoiceLine.NetWeight = 4.365;
			Reader.FillHouseBillAdditionalItems(hawb);

			AssertEquals(4.365M, hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_Weight);

			invoiceLine.NetWeight = 0;

			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals(1.23M, hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_Weight);
		}

		public void TestETailHAWBItemsGoodsDescriptionMapping()
		{
			hawbDataObject.PackingLineCollection.Single().GoodsDescription = ZString.Empty;

			var invoiceLine = AddAnotherPackedItemAndInvoiceLine();
			invoiceLine.Description = "book";
			Reader.FillHouseBillAdditionalItems(hawb);

			AssertEquals("BOOK", hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_GoodsDescription);

			invoiceLine.Description = ZString.Empty;
			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals("GOODS FOR TESTING", hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_GoodsDescription);
		}

		public void TestETailHAWBItemsGoodsValueMapping()
		{
			var invoiceLine = AddAnotherPackedItemAndInvoiceLine();
			invoiceLine.CustomsValue = 95.024M;
			Reader.FillHouseBillAdditionalItems(hawb);

			AssertEquals(95.024M, hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_GoodsValue);

			invoiceLine.CustomsValue = 0;

			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals(6.178M, hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_GoodsValue);
		}

		public void TestETailHAWBItemsGoodsOriginMapping()
		{
			var invoiceLine = AddAnotherPackedItemAndInvoiceLine();
			invoiceLine.CountryOfOrigin = new Country() { Code = "AD" };
			invoiceLine.CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>()
			{
				new CustomsSupportingInformation() { Country = new Country() { Code = "AE" } }
			};

			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals("AE", hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_RN_NKGoodsOrigin);

			invoiceLine.CustomsSupportingInformationCollection.Single().Country.Code = ZString.Empty;

			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals("AD", hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_RN_NKGoodsOrigin);

			invoiceLine.CountryOfOrigin.Code = ZString.Empty;

			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals("US", hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_RN_NKGoodsOrigin);
		}

		public void TestETailHAWBItemsPieceCountMapping()
		{
			var invoiceLine = AddAnotherPackedItemAndInvoiceLine();
			invoiceLine.CustomsQuantity = 134M;
			Reader.FillHouseBillAdditionalItems(hawb);

			AssertEquals(134, hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_PieceCount);

			var largeValue = (ZDecimal)int.MaxValue + 10.0m;
			invoiceLine.CustomsQuantity = largeValue;
			var exception = AssertExceptionThrown<DataObjectReadFailureException>(() => Reader.FillHouseBillAdditionalItems(hawb));
			AssertEquals("Should include error information", $"Value '{largeValue}' of 'Customs Quantity' is too large. It cannot be greater than {int.MaxValue}", exception.Message);

			invoiceLine.CustomsQuantity = null;

			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals(1, hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_PieceCount);
		}

		public void TestETailHAWItemsHarmonisedTariffNumsMapping()
		{
			var invoiceLine = AddAnotherPackedItemAndInvoiceLine();
			invoiceLine.HarmonisedCode = "3333.33.33.33";
			invoiceLine.CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>()
			{
				new CustomsSupportingInformation()
				{
					Tariff = "4444.44.44.44"
				}
			};

			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			AssertEquals("pre-condition", "IMP", mawb.EntryType);
			Reader.FillHouseBillAdditionalItems(hawb);
			AssertEquals("3333.33.33.33", hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_HarmonisedTariffNums);

			mawb.CM_RL_NKLoadPort = "NZAKL";
			mawb.CM_RL_NKDischargePort = "USLAX";
			AssertEquals("pre-condition", "EXP", mawb.EntryType);
			Reader.FillHouseBillAdditionalItems(hawb);

			Reader.FillHouseBill(hawb);
			AssertEquals("4444.44.44.44", hawb.CusHAWBItemsCollection.Cast<CusHAWBItems>().First().CHI_HarmonisedTariffNums);
		}

		public void TestETailHAWBShipmentPKMapping()
		{
			Reader.FillHouseBill(hawb);
			AssertEquals("Shipment PK", Shipment.PK, hawb.CS_JS);
		}

		public void TestEtailHAWBMasterHouseBillMapping()
		{
			Reader.FillHousBillCountrySpecificDetails(hawb);
			AssertEquals("HAWB master house bill", "MB1123", hawb.CS_MasterHouseBill);
		}

		#region Implementation
		ETailCusHAWBDataObjectReaderForTest Reader => _reader ?? (_reader = new ETailCusHAWBDataObjectReaderForTest(hawbDataObject, mawbDataObject, mawb, hawb, Shipment.PK));
		ETailCusHAWBDataObjectReaderForTest _reader;
		Shipment mawbDataObject;
		Shipment hawbDataObject;
		CusMAWB mawb => _mawb ?? (_mawb = Factory.New<CusMAWB>());
		CusMAWB _mawb;

		CusHAWB hawb => _hawb ?? (_hawb = mawb.ChildBills.AddNew());
		CusHAWB _hawb;

		ForwardingShipment Shipment => _shipment ?? (_shipment = Factory.NewWithValidTestData<ForwardingShipment>());
		ForwardingShipment _shipment;

		protected override void SetUp()
		{
			base.SetUp();
			mawbDataObject = SetupAirCargoMaster("MB1123", ZString.Empty);
			hawbDataObject = SetupAirCargoHouse("HB1123-1");
			hawbDataObject.GoodsValue = 6.178M;
			hawbDataObject.SetPackingLineCollection(() =>
			{
				var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackType = new PackageType() { Code = "BBG" },
					Weight = 1.23M,
					GoodsDescription = "Bag of crap",
					WeightUnit = new UnitOfWeight() { Code = "g" }
				};
				packingLine.SetPackedItemCollection(() => new List<PackedItem>()
				{
					new PackedItem()
					{
						CommercialInvoiceLineLink = 888
					}
				});
				return new DataObjectList<PackingLine>() { packingLine };
			});
			hawbDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
					Country = new Country() { Code = "US" }
				}
			});
			hawbDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine>()
						{
							new CommercialInvoiceLine()
							{
								Link = 888,
								CustomsValue = 5.281M,
								Description = "Invoice Desc",
								HarmonisedCode = "1111.11.11.11",
								CustomsQuantity = 2M,
								CountryOfOrigin = new Country() { Code = "CA" },
								CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>()
								{
									new CustomsSupportingInformation()
									{
										Country = new Country() { Code = "CN" },
										Tariff = "2222.22.22.22"
									}
								}
							}
						}))
				}
			};
		}

		CommercialInvoiceLine AddAnotherPackedItemAndInvoiceLine()
		{
			hawbDataObject.PackingLineCollection.Single().PackedItemCollection.Add(new PackedItem()
			{
				CommercialInvoiceLineLink = 100
			});
			var result = new CommercialInvoiceLine()
			{
				Link = 100,
				Description = "New Invoice Line"
			};
			hawbDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Add(result);
			return result;
		}

		#endregion
	}

	public class ETailCusHAWBDataObjectReaderForTest : ETailCusHAWBDataObjectReader
	{
		public ETailCusHAWBDataObjectReaderForTest(Shipment shipmentDataObject, Shipment mawbDataObject, CusMAWB mawb, CusHAWB masterHouse, ZGuid shipmentPK)
			: base(shipmentDataObject, mawbDataObject, new LoggerForTest(new LoggingInformation()), new AirManifestDataObjectReaderHelper(new UniversalObjectFactory(), Core.Constants.CountryCodes.NewZealand), mawb, masterHouse, shipmentPK)
		{
		}

		public IXmlImportLogger Logger => logger;

		public void FillHouseBill(CusHAWB hawb)
		{
			FillFirstPackingLine(hawb);
		}

		public void FillHouseBillAdditionalItems(CusHAWB hawb)
		{
			FillSubSequentPackingLines(hawb);
		}

		public void FillHousBillCountrySpecificDetails(CusHAWB hawb)
		{
			FillCountrySpecificDetails(hawb);
		}
	}

	sealed class LoggerForTest : IXmlImportLogger
	{
		public LoggerForTest(LoggingInformation logger)
		{
			this.logger = logger;
		}
		readonly LoggingInformation logger;

		public bool IsUpdatingConsol { get; set; }
		public bool HasIgnoredModule { get; set; }

		public ITopLevelDataObject TopLevelDataObject => null;

		public IDataContextDataObject TopLevelDataContext => null;

		public bool OrgMatchingDisabled => false;

		public IEnumerable<ISimpleLog> Logs => logger.Logs;

		public void FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
		}

		public void Log(Integration.LogType type, string message)
		{
			logger.Log(type, message);
		}

		public void LogBoth(Integration.LogType type, string message)
		{
		}

		public void LogErrorToServiceTaskOnly(string message)
		{
		}

		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
		{
		}

		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }
	}
}
