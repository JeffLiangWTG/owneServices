using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsDetailsChangeOfRegimeBaseTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WarehouseCustomsDetailsChangeOfRegimeForTest(null));
		}

		public void TestOutOfRegimeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new CommercialInfo();
			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader };
			var commercialInvoiceLine = new CommercialInvoiceLine { EntryInstructionLink = 1 };
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction> { new EntryInstruction { Link = 1, Procedure = "AA" } });

			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Factory.Save();
			commercialInvoiceLine.Procedure = "51BB";
			var warehouseCustomsDetailsChangeOfRegimeForProcedure1 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment);
			AssertEquals(CustomsRegime.InwardProcessing, warehouseCustomsDetailsChangeOfRegimeForProcedure1.OutOfRegimeType);

			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "DD", ZString.Empty, "description2", "EXP");
			procedure2.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.No;
			Factory.Save();
			commercialInvoiceLine.Procedure = "51DD";
			var warehouseCustomsDetailsChangeOfRegimeForProcedure2 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment);
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegimeForProcedure2.OutOfRegimeType);

			var shipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var warehouseCustomsDetailsChangeOfRegime2 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment2);
			AssertNull("Precondition", shipment2.CommercialInfo);
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime2.OutOfRegimeType);

			var shipment3 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { CommercialInfo = new CommercialInfo() };
			var warehouseCustomsDetailsChangeOfRegime3 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment3);
			AssertNull("Precondition", shipment3.CommercialInfo.CommercialInvoiceCollection);
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime3.OutOfRegimeType);

			var shipment4 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() }
			};
			var warehouseCustomsDetailsChangeOfRegime4 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment4);
			AssertEquals("Precondition", 0, shipment4.CommercialInfo.CommercialInvoiceCollection.Count);
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime4.OutOfRegimeType);

			var shipment5 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader> { new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) }
				},
			};
			var warehouseCustomsDetailsChangeOfRegime5 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment5);
			AssertNull("Precondition", shipment5.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection);
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime5.OutOfRegimeType);

			var shipment6 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { new CommercialInvoiceLine { EntryInstructionLink = 1 } }))
					},
				},
			};
			var warehouseCustomsDetailsChangeOfRegime6 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment6);
			AssertNull("Precondition", shipment6.EntryInstructionCollection);
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime6.OutOfRegimeType);
		}

		public void TestNewWarehouse()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new CommercialInfo();
			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader };
			var commercialInvoiceLine = new CommercialInvoiceLine { EntryInstructionLink = 1 };
			commercialInvoiceLine.EntryInstructionLink = 1;
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });
			var organizationAddress1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = AddressTypes.Warehouse2 };
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction> { new EntryInstruction { OrganizationAddressCollection = new List<OrganizationAddress> { organizationAddress1 }, Link = 1 } });

			var warehouseCustomsDetailsChangeOfRegime = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment);
			AssertEquals(organizationAddress1, warehouseCustomsDetailsChangeOfRegime.NewWarehouse);

			var shipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var warehouseCustomsDetailsChangeOfRegime2 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment2);
			AssertNull("Precondition", shipment2.CommercialInfo);
			AssertNull(warehouseCustomsDetailsChangeOfRegime2.NewWarehouse);

			var shipment3 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { CommercialInfo = new CommercialInfo() };
			var warehouseCustomsDetailsChangeOfRegime3 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment3);
			AssertNull("Precondition", shipment3.CommercialInfo.CommercialInvoiceCollection);
			AssertNull(warehouseCustomsDetailsChangeOfRegime3.NewWarehouse);

			var shipment4 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() }
			};
			var warehouseCustomsDetailsChangeOfRegime4 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment4);
			AssertEquals("Precondition", 0, shipment4.CommercialInfo.CommercialInvoiceCollection.Count);
			AssertNull(warehouseCustomsDetailsChangeOfRegime4.NewWarehouse);

			var shipment5 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader> { new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) }
				}
			};
			var warehouseCustomsDetailsChangeOfRegime5 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment5);
			AssertNull("Precondition", shipment5.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection);
			AssertNull(warehouseCustomsDetailsChangeOfRegime5.NewWarehouse);

			var shipment6 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>  new DataObjectList<CommercialInvoiceLine> { new CommercialInvoiceLine { EntryInstructionLink = 1 } })),
					},
				},
			};
			var warehouseCustomsDetailsChangeOfRegime6 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment6);
			AssertNull("Precondition", shipment6.EntryInstructionCollection);
			AssertNull(warehouseCustomsDetailsChangeOfRegime6.NewWarehouse);

			var shipment7 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>  new DataObjectList<CommercialInvoiceLine> { new CommercialInvoiceLine { EntryInstructionLink = 1 } })),
					},
				},
			};
			var entryInstruction7 = new EntryInstruction { Link = 1 };
			shipment7.SetEntryInstructionCollection(() => new List<EntryInstruction> { entryInstruction7 });
			var warehouseCustomsDetailsChangeOfRegime7 = new WarehouseCustomsDetailsChangeOfRegimeForTest(shipment7);
			AssertNull("Precondition", entryInstruction7.OrganizationAddressCollection);
			AssertNull(warehouseCustomsDetailsChangeOfRegime7.NewWarehouse);
		}

		class WarehouseCustomsDetailsChangeOfRegimeForTest : WarehouseCustomsDetailsChangeOfRegimeBase
		{
			public WarehouseCustomsDetailsChangeOfRegimeForTest(Shipment shipment) : base(shipment)
			{
			}

			public override CustomsRegime IntoRegimeType => throw new NotImplementedException();
		}
	}
}
