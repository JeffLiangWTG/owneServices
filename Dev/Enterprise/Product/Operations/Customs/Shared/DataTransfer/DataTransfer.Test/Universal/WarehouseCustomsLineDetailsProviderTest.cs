using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsProviderTest : TestCaseWithFactory
	{
		public void TestGetLineDetails()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var testFileName = "UniversalShipmentMapToBonded1";
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var logger = new DummyLogger();

			PopulateUniversalShipment(shipment, logger, testFileName);
			var shipmentAddress = shipment.OrganizationAddressCollection.FindBestSupplierMatch();
			AssertNotNull(shipmentAddress);
			var invoiceLines = shipment.CommercialInfo.CommercialInvoiceCollection.SelectMany(x => x.CommercialInvoiceLineCollection);
			var invoiceLine1 = invoiceLines.First(x => x.LineNo == 1);
			invoiceLine1.CustomsValue = 100m;
			invoiceLine1.CountryOfOrigin = new Country() { Code = Core.Constants.CountryCodes.NewCaledonia };
			invoiceLine1.CustomsQuantity = 110m;
			invoiceLine1.CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "U1" };
			invoiceLine1.CustomsSecondQuantity = 112m;
			invoiceLine1.CustomsSecondQuantityUnit = new CodeDescriptionPair6Char() { Code = "UU1" };
			invoiceLine1.CustomsThirdQuantity = 11m;
			invoiceLine1.CustomsThirdQuantityUnit = new CodeDescriptionPair6Char() { Code = "UN1" };
			invoiceLine1.HarmonisedCode = "TRF1";
			invoiceLine1.PrimaryPreference = "PP1";
			var invoiceLine2 = invoiceLines.First(x => x.LineNo == 2);
			invoiceLine2.CustomsValue = 200m;
			invoiceLine2.CountryOfOrigin = new Country() { Code = Core.Constants.CountryCodes.Australia };
			invoiceLine2.CustomsQuantity = 220m;
			invoiceLine2.CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "U2" };
			invoiceLine2.CustomsSecondQuantity = 222m;
			invoiceLine2.CustomsSecondQuantityUnit = new CodeDescriptionPair6Char() { Code = "UU2" };
			invoiceLine2.CustomsThirdQuantity = 21m;
			invoiceLine2.CustomsThirdQuantityUnit = new CodeDescriptionPair6Char() { Code = "UN2" };
			invoiceLine2.HarmonisedCode = "TRF2";
			invoiceLine2.PrimaryPreference = "PP2";
			var invoiceLine3 = invoiceLines.First(x => x.LineNo == 3);
			invoiceLine3.CustomsValue = 300m;
			invoiceLine3.CountryOfOrigin = new Country() { Code = Core.Constants.CountryCodes.UnitedStates };
			invoiceLine3.CustomsQuantity = 330m;
			invoiceLine3.CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "U3" };
			invoiceLine3.CustomsSecondQuantity = 332m;
			invoiceLine3.CustomsSecondQuantityUnit = new CodeDescriptionPair6Char() { Code = "UU3" };
			invoiceLine3.CustomsThirdQuantity = 31m;
			invoiceLine3.CustomsThirdQuantityUnit = new CodeDescriptionPair6Char() { Code = "UN3" };
			invoiceLine3.HarmonisedCode = "TRF3";
			invoiceLine3.PrimaryPreference = "PP3";

			shipment.EntryHeaderCollection.Add(new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				EntryNumberCollection = new List<UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber>(new[]
				{
					new UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber() { Number = "CCCCCC", Type = new EntryType() { Code = "IMP" } }
				}),
				EntryLineCollection = new List<EntryLine>(new[] { new EntryLine() { LineNumber = 1 }, new EntryLine() { LineNumber = 2 } })
			});
			var invoice3 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INVOICE3",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine() { LineNo = 1, EntryNumber = "CCCCCC", EntryLineNumber = 1, CustomsValue = 123m, BondedWarehouseQuantity = 1 }
							})));
			var invoiceAddress = invoice3.AddOrgAddress(writeManager, org1, MasterFiles.Integration.DocAddressType.SupplierPickupDeliveryAddress);
			var invoice4Line1 = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				EntryNumber = "CCCCCC",
				EntryLineNumber = 2,
				CustomsValue = 456m,
				BondedWarehouseQuantity = 1
			};
			var invoiceLineAddress = invoice4Line1.AddOrgAddress(writeManager, org2, MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress);
			var invoice4Line2 = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 2,
				EntryNumber = "CCCCCC",
				EntryLineNumber = 3,
				CustomsValue = 789m,
				BondedWarehouseQuantity = 2
			};
			shipment.CommercialInfo.SubGroupCollection = new List<CommercialInfo>(new[]
			{
				new CommercialInfo()
				{
					Name = "SUBGROUP1",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						invoice3
					}),
					SubGroupCollection = new List<CommercialInfo>(new[]
					{
						new CommercialInfo()
						{
							Name = "SUBGROUP2",
							CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
							{
								new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
								{
									InvoiceNumber = "INVOICE4",
								}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[]
									{
										invoice4Line1, invoice4Line2
									})))
							})
						}
					})
				}
			});

			var provider = new WarehouseCustomsLineDetailsProvider(shipment) as IWarehouseCustomsLineDetailsProvider;
			var lineDetails = provider.GetLineDetails();

			var lineDetail1 = lineDetails.Single(x => x.InvoiceLine.LineNo == 1 && x.EntryNumber.Value == "AAAAAA");
			AssertEquals((ZShort)1, lineDetail1.EntryLineNumber);
			AssertEquals(100m, lineDetail1.ValueForDuty);
			AssertEquals(Core.Constants.CountryCodes.NewCaledonia, lineDetail1.CountryOfOrigin.Code);
			AssertEquals(110m, lineDetail1.CustomsQuantity);
			AssertEquals("U1", lineDetail1.CustomsQuantityUnit.Code);
			AssertEquals(112m, lineDetail1.CustomsSecondQuantity);
			AssertEquals("UU1", lineDetail1.CustomsSecondQuantityUnit.Code);
			AssertEquals(11m, lineDetail1.CustomsThirdQuantity);
			AssertEquals("UN1", lineDetail1.CustomsThirdQuantityUnit.Code);
			AssertEquals("TRF1", lineDetail1.Tariff);
			AssertEquals("PP1", lineDetail1.PrimaryPreference);

			var lineDetail2 = lineDetails.Single(x => x.InvoiceLine.LineNo == 2 && x.EntryNumber.Value == "BBBBBB");
			AssertEquals((ZShort)2, lineDetail2.EntryLineNumber);
			AssertEquals(200m, lineDetail2.ValueForDuty);
			AssertEquals(Core.Constants.CountryCodes.Australia, lineDetail2.CountryOfOrigin.Code);
			AssertEquals(220m, lineDetail2.CustomsQuantity);
			AssertEquals("U2", lineDetail2.CustomsQuantityUnit.Code);
			AssertEquals(222m, lineDetail2.CustomsSecondQuantity);
			AssertEquals("UU2", lineDetail2.CustomsSecondQuantityUnit.Code);
			AssertEquals(21m, lineDetail2.CustomsThirdQuantity);
			AssertEquals("UN2", lineDetail2.CustomsThirdQuantityUnit.Code);
			AssertEquals("TRF2", lineDetail2.Tariff);
			AssertEquals("PP2", lineDetail2.PrimaryPreference);

			var lineDetail3 = lineDetails.Single(x => x.InvoiceLine.LineNo == 3 && x.EntryNumber.Value == "BBBBBB");
			AssertEquals((ZShort)3, lineDetail3.EntryLineNumber);
			AssertEquals(300m, lineDetail3.ValueForDuty);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, lineDetail3.CountryOfOrigin.Code);
			AssertEquals(330m, lineDetail3.CustomsQuantity);
			AssertEquals("U3", lineDetail3.CustomsQuantityUnit.Code);
			AssertEquals(332m, lineDetail3.CustomsSecondQuantity);
			AssertEquals("UU3", lineDetail3.CustomsSecondQuantityUnit.Code);
			AssertEquals(31m, lineDetail3.CustomsThirdQuantity);
			AssertEquals("UN3", lineDetail3.CustomsThirdQuantityUnit.Code);
			AssertEquals("TRF3", lineDetail3.Tariff);
			AssertEquals("PP3", lineDetail3.PrimaryPreference);

			var lineDetail4 = lineDetails.Single(x => x.InvoiceLine.LineNo == 1 && x.EntryNumber.Value == "CCCCCC" && x.EntryLineNumber == 1);
			AssertEquals(123m, lineDetail4.ValueForDuty);
			AssertEquals(invoiceAddress, lineDetail4.SupplierAddress);

			var lineDetail5 = lineDetails.Single(x => x.InvoiceLine.LineNo == 1 && x.EntryNumber.Value == "CCCCCC" && x.EntryLineNumber == 2);
			AssertEquals(456m, lineDetail5.ValueForDuty);
			AssertEquals(invoiceLineAddress, lineDetail5.SupplierAddress);

			var lineDetail6 = lineDetails.Single(x => x.InvoiceLine.LineNo == 2 && x.EntryNumber.Value == "CCCCCC" && x.EntryLineNumber == 3);
			AssertEquals(789m, lineDetail6.ValueForDuty);
			AssertEquals(shipmentAddress, lineDetail6.SupplierAddress);
		}

		void PopulateUniversalShipment(Shipment shipment, IXmlImportLogger logger, string testFileName)
		{
			using (var stream = (SubStreamableStream)File.OpenRead(TestFileHelper.GetPathForUniversalTestFiles(testFileName + ".xml")))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}
		}

		public void TestProviderWithNoAddInfoSchema()
		{
			var testFileName = "UniversalShipmentMapToBonded1";
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var logger = new DummyLogger();

			PopulateUniversalShipment(shipment, logger, testFileName);
			var shipmentAddress = shipment.OrganizationAddressCollection.FindBestSupplierMatch();
			AssertNotNull(shipmentAddress);
			shipment.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo() { Key = "Quantity", Value = "7" }
			});
			var invoiceLines = shipment.CommercialInfo.CommercialInvoiceCollection.SelectMany(x => x.CommercialInvoiceLineCollection);
			shipment.CommercialInfo.CommercialInvoiceCollection[0].AddInfoCollection = new List<AddInfo>()
			{
				new AddInfo() { Key = "Quantity", Value = "7" }
			};
			var invoiceLine1 = invoiceLines.First(x => x.LineNo == 1);
			invoiceLine1.BondedWarehouseQuantity = 1;
			IEnumerable<IWarehouseCustomsLineDetails> lineDetails;
			var provider = new WarehouseCustomsLineDetailsProviderWithNoAddInfoForTesting(shipment, null, JobComInvoiceHeaderSchema.Instance) as IWarehouseCustomsLineDetailsProvider;
			AssertNoExceptionThrown(() => lineDetails = provider.GetLineDetails().ToArray());

			provider = new WarehouseCustomsLineDetailsProviderWithNoAddInfoForTesting(shipment, null, null);
			AssertNoExceptionThrown(() => lineDetails = provider.GetLineDetails().ToArray());
		}

		class WarehouseCustomsLineDetailsProviderWithNoAddInfoForTesting : WarehouseCustomsLineDetailsProvider<BaseJobDeclaration, BaseJobComInvoiceHeader>
		{
			public WarehouseCustomsLineDetailsProviderWithNoAddInfoForTesting(Shipment shipment, ITableSchema declarationAddInfoSchema, ITableSchema invoiceAddInfoSchema)
				: base(shipment)
			{
				this.declarationAddInfoSchema = declarationAddInfoSchema;
				this.invoiceAddInfoSchema = invoiceAddInfoSchema;
			}

			readonly ITableSchema declarationAddInfoSchema;
			readonly ITableSchema invoiceAddInfoSchema;

			protected override ITableSchema GetDeclarationAddInfoSchema() => declarationAddInfoSchema;

			protected override ITableSchema GetInvoiceAddInfoSchema() => invoiceAddInfoSchema;

			protected override IEnumerable<string> GetInvoiceLineAddInfosApplicableForInwardWarehousing()
			{
				yield return "AdditionalTerms";
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
