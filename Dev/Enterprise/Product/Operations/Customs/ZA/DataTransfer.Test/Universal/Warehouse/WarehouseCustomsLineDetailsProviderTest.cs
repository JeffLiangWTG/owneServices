using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsProviderTest : TestCaseWithFactory
	{
		public void TestGetLineDetails()
		{
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "AB1", "12", "", "AB1 DESC", "", outOfWarehouse: true);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "AB2", "12", "", "AB2 DESC", "", outOfWarehouse: true);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "DC1", "12", "", "DC1 DESC", "", intoWarehouse: true);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "DC2", "12", "", "DC2 DESC", "", intoWarehouse: true);
			Factory.Save();
			var shipment = CreateShipment("DC1", "DC2", DataContextType.WarehouseReceive);
			var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(shipment);
			var lines = new List<IWarehouseCustomsLineDetails>(provider.GetLineDetails());
			AssertWarehouseCustomsLineDetails(lines[0], "12345000000023", 1, null, null, "EngineNumber=ENG32425*VIN=VIN23423*OriginalProcedureCode=DC1", new ZDateTime(2020, 6, 30), "DC1", "AB12");
			AssertWarehouseCustomsLineDetails(lines[1], "12345000000034", 2, null, null, "EngineNumber=ENG86765*VIN=VIN54564*OriginalProcedureCode=DC2", new ZDateTime(2020, 6, 1), "DC2", "DC12");
			shipment = CreateShipment("AB1", "AB2", DataContextType.WarehouseOrder);
			provider = new WarehouseCustomsLineDetailsProvider(shipment);
			lines = new List<IWarehouseCustomsLineDetails>(provider.GetLineDetails());
			AssertWarehouseCustomsLineDetails(lines[0], "12345000000023", 1, "12345000000012", 3, "", new ZDateTime(2020, 6, 30), "AB1", "AB12");
			AssertWarehouseCustomsLineDetails(lines[1], "12345000000034", 2, "12345000000012", 4, "", new ZDateTime(2020, 6, 1), "AB2", "DC12");
		}

		Shipment CreateShipment(ZString cei_Style1, ZString cei_Style2, DataContextType dataContextType)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(dataContextType, null);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUPINV",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV123",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[] { InvoiceLine1, InvoiceLine2 })))
					})
				}
			};
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[]
				{
					new EntryInstruction()
					{
						Style = cei_Style1,
						Link = 1
					},
					new EntryInstruction()
					{
						Style = cei_Style2,
						Link = 2
					}
				}));

			shipment.SetEntryHeaderCollection(() => new List<EntryHeader>(new[]
				{
					new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Reference = "Entry1",
						EntryInstructionLink = 1,
						BondValidToDate = new ZDateTime(2020, 6, 30)
					},
					new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Reference = "Entry2",
						EntryInstructionLink = 2,
						BondValidToDate = new ZDateTime(2020, 6, 1)
					},
				}));
			return shipment;
		}

		CommercialInvoiceLine InvoiceLine1
		{
			get
			{
				if (invoiceLine1 == null)
				{
					invoiceLine1 = new CommercialInvoiceLine()
					{
						LineNo = 1,
						EntryLineNumber = 1,
						EntryNumber = "12345000000023",
						BondedWarehouseQuantity = 2000m,
						BondedWarehouseQuantityUnit = new CodeDescriptionPair() { Code = "KGM" },
						EntryInstructionLink = 1,
						Procedure = "AB12",
						PreviousEntryNumber = "12345000000012",
						PreviousEntryLineNumber = 3,
						AddInfoCollection = new List<UniversalAddInfo>(new[]
						{
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_ImportTariff.Substring(3), Value = "9902" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride.Substring(3), Value = "USD" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_VIN.Substring(3), Value = "VIN23423" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_EngineNumber.Substring(3), Value = "ENG32425" }
						})
					};
				}
				return invoiceLine1;
			}
		}
		CommercialInvoiceLine invoiceLine1;

		CommercialInvoiceLine InvoiceLine2
		{
			get
			{
				if (invoiceLine2 == null)
				{
					invoiceLine2 = new CommercialInvoiceLine()
					{
						LineNo = 1,
						EntryLineNumber = 2,
						EntryNumber = "12345000000034",
						BondedWarehouseQuantity = 1m,
						BondedWarehouseQuantityUnit = new CodeDescriptionPair() { Code = "KGM" },
						EntryInstructionLink = 2,
						Procedure = "DC12",
						PreviousEntryNumber = "12345000000012",
						PreviousEntryLineNumber = 4,
						AddInfoCollection = new List<UniversalAddInfo>(new[]
						{
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_ImportTariff.Substring(3), Value = "9902" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride.Substring(3), Value = "USD" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_VIN.Substring(3), Value = "VIN54564" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.JI_EngineNumber.Substring(3), Value = "ENG86765" }
						})
					};
				}
				return invoiceLine2;
			}
		}
		CommercialInvoiceLine invoiceLine2;

		void AssertWarehouseCustomsLineDetails(IWarehouseCustomsLineDetails lineDetails, ZString entryNumber, ZShort entryLineNumber, ZString? previousEntryNumber, ZShort? previousEntryLineNumber, ZString addInfos, ZDateTime customsDeadline, ZString inwardStyle, ZString inwardProcedure)
		{
			AssertEquals("EntryNumber", entryNumber, lineDetails.EntryNumber);
			AssertEquals("EntryLineNumber", entryLineNumber, lineDetails.EntryLineNumber);
			AssertEquals("PreviousEntryNumber", previousEntryNumber, lineDetails.PreviousEntryNumber);
			AssertEquals("PreviousEntryLineNumber", previousEntryLineNumber, lineDetails.PreviousEntryLineNumber);
			AssertEquals("AddInfos", addInfos, lineDetails.AddInfos);
			AssertEquals("CustomsDeadline", customsDeadline, lineDetails.CustomsDeadline);
			AssertEquals("InwardStyle", inwardStyle, lineDetails.Style);
			AssertEquals("InwardProcedure", inwardProcedure, lineDetails.Procedure);
		}
	}
}
