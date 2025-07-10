using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
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
	sealed class WarehouseCustomsLineDetailsTest : TestCaseWithFactory
	{
		public void TestWarehouseCustomsLineDetailsChangeOfOwnershipMembers()
		{
			// Setup Universal Shipment For ZA
			// Assert that Previous properties are mapped correctly
			using (((IExternalFetchHintSupporter)Factory).SetupCreator())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var helper = new WhsDataTestHelper(Factory);
				var changeOfOwnershipCusProcedure = helper.ChangeOfOwnershipCusProcedure;
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				var importer = helper.Importer;
				var miscServ = importer.MiscServ;
				miscServ.OM_IMPartAttrib1Name = "VIN1";
				miscServ.OM_IMPartAttrib2Name = "ENGINE2";
				miscServ.OM_IMPartAttrib3Name = "COLOUR3";
				declaration.JE_OH_Importer = helper.Importer.PK;
				var owner = helper.Owner;
				var ownerMiscServ = owner.MiscServ;
				ownerMiscServ.OM_IMPartAttrib1Name = "ENGINE2";
				ownerMiscServ.OM_IMPartAttrib2Name = "VIN1";
				ownerMiscServ.OM_IMPartAttrib3Name = "COLOUR3";
				var entryInstruction = (CusEntryInstruction)Factory.New<Integration.Customs.ZA.ICusEntryInstruction>();
				entryInstruction.CEI_JE = declaration.PK;
				entryInstruction.CEI_Style = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_OH_Owner = owner.PK;
				declaration.Invoices.DeleteAll();
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
				invoiceLine.JI_Procedure = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
				invoiceLine.JI_PartAttrib1 = "OPATT1";
				invoiceLine.JI_PartAttrib2 = "OPATT2";
				invoiceLine.JI_PartAttrib3 = "OPATT3";
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartNo.Name] = helper.Part2.OP_PartNum;
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartAttrib1.Name] = "PATT1";
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartAttrib2.Name] = "PATT2";
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartAttrib3.Name] = "PATT3";
				invoiceLine.JI_PreviousEntryNumber = "PENT324";
				invoiceLine.JI_PreviousEntryLineNumber = 2;
				invoiceLine.JI_BondedWhsQuantity = 100m;
				invoiceLine.JI_BondedWhsUnitQty = "NO";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();
				var entry = declaration.ActiveEntryHeaders[0];
				var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(invoiceLine.JI_AddInfo);
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode, (ZString)"P");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1, (ZString)"1");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2, (ZString)"2");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3, (ZString)"3");
				invoiceLine.JI_AddInfo = AddInfoParser.Serialise(addInfos);
				entry.EntryNumber = "ENT3452";
				entry.CH_BondValidToDate = new ZDate("2020-06-29");
				Factory.Save();
				AssertEquals(entry, invoiceLine.CusEntryLine?.Header);
				var manager = (IShipmentDataContextManager)entry.GetUniversalDataContextManager();
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BCO, entry)));
				var shipmentData = (Shipment)writer.GetDataObject(entry);
				IWarehouseCustomsLineDetailsProvider provider = new WarehouseCustomsLineDetailsProvider(shipmentData);
				var lineDetails = provider.GetLineDetails().ToArray();
				AssertEquals(1, lineDetails.Length);
				var lineDetail = lineDetails[0];
				AssertEquals("lineDetail.EntryNumber", "ENT3452", lineDetail.EntryNumber);
				AssertEquals("lineDetail.EntryLineNumber", (ZShort)1, lineDetail.EntryLineNumber);
				AssertEquals("lineDetail.PreviousEntryNumber", "PENT324", lineDetail.PreviousEntryNumber);
				AssertEquals("lineDetail.PreviousEntryLineNumber", (ZShort)2, lineDetail.PreviousEntryLineNumber);
				AssertEquals("lineDetail.InvoiceLine.PartNo", helper.Part.OP_PartNum, lineDetail.InvoiceLine.PartNo);
				AssertEquals("NewOwnerPartAttribute1", "OPATT1", lineDetail.InvoiceLine.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN1").Value);
				AssertEquals("NewOwnerPartAttribute2", "OPATT2", lineDetail.InvoiceLine.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE2").Value);
				AssertEquals("NewOwnerPartAttribute3", "OPATT3", lineDetail.InvoiceLine.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR3").Value);
				AssertEquals("lineDetail.NewOwnerProductCode", helper.Part2.OP_PartNum, lineDetail.NewOwnerProductCode);
				AssertEquals("lineDetail.NewOwnerPartAttribute1", "PATT1", lineDetail.NewOwnerPartAttribute1);
				AssertEquals("lineDetail.NewOwnerPartAttribute2", "PATT2", lineDetail.NewOwnerPartAttribute2);
				AssertEquals("lineDetail.NewOwnerPartAttribute3", "PATT3", lineDetail.NewOwnerPartAttribute3);
				AssertEquals("lineDetail.CustomsDeadline", ZDateTime.Empty, lineDetail.CustomsDeadline);
				AssertEquals("lineDetail.Style", "", lineDetail.Style);
				AssertEquals("lineDetail.Procedure", "", lineDetail.Procedure);
			}
		}

		public void TestSupplierAddress()
		{
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PrimaryPreference = (ZString)"STANDARD",
				CustomsSecondQuantity = 10m,
				CustomsSecondQuantityUnit = new CodeDescriptionPair6Char() { Code = "DTNE" },
				CustomsThirdQuantity = 20m,
				CustomsThirdQuantityUnit = new CodeDescriptionPair6Char() { Code = "BAG" }
			};
			var fallbackDetail = new WarehouseCustomsFallbackDetail();
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertNull(lineDetails.SupplierAddress);
			AssertEquals("PrimaryPreference", "STANDARD", lineDetails.PrimaryPreference);
			AssertEquals("CustomsSecondQuantity", 10m, lineDetails.CustomsSecondQuantity);
			AssertEquals("CustomsSecondQuantityUnit", "DTNE", lineDetails.CustomsSecondQuantityUnit.GetCodeAsUpperCase());
			AssertEquals("CustomsThirdQuantity=20*CustomsThirdQuantityUnit=BAG", lineDetails.AddInfos);
			AssertEquals("ManufacturerAddress:", null, lineDetails.ManufacturerAddress);    // after implementation need to be changed or add a new test

			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertNull(lineDetails.SupplierAddress);
			fallbackDetail.SupplierAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertEquals(fallbackDetail.SupplierAddress, lineDetails.SupplierAddress);
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var org = Factory.New<OrgHeader>();
			var supplierAddress = invoiceLine.AddOrgAddress(writeManager, org, MasterFiles.Integration.DocAddressType.SupplierDocumentaryAddress);
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertEquals(supplierAddress, lineDetails.SupplierAddress);
		}

		public void TestEntryDetails()
		{
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				EntryNumber = "ENT123",
				EntryLineNumber = 1,
				PreviousEntryNumber = "ENT456",
				PreviousEntryLineNumber = 2,
				LineNo = 55,
				DataImportMatchingKey = "ab87654321",
				BondedWHSOrderLineNumber = 3,
				BondedWHSOrderNumber = "W000000023"
			};
			var fallbackDetail = new WarehouseCustomsFallbackDetail();
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertEquals("EntryNumber", "ENT123", lineDetails.EntryNumber);
			AssertEquals("EntryLineNumber", (short)1, lineDetails.EntryLineNumber);
			AssertEquals("PreviousEntryNumber", "ENT456", lineDetails.PreviousEntryNumber);
			AssertEquals("PreviousEntryLineNumber", (short)2, lineDetails.PreviousEntryLineNumber);
			AssertEquals("OrderLineNo", 3, lineDetails.OrderLineNo);
			AssertEquals("OrderNumber", "W000000023", lineDetails.OrderNumber);
			AssertEquals("DataImportMatchingKey", "ab87654321", lineDetails.DataImportMatchingKey);

			invoiceLine.EntryNumber = ZString.Empty;
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertEquals("EntryNumber", Constants.EntryNumberPlaceHolder, lineDetails.EntryNumber);
			AssertEquals("EntryLineNumber", (short)1, lineDetails.EntryLineNumber);

			invoiceLine.EntryNumber = null;
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertEquals("EntryNumber", Constants.EntryNumberPlaceHolder, lineDetails.EntryNumber);
			AssertEquals("EntryLineNumber", (short)1, lineDetails.EntryLineNumber);
		}

		public void TestAdditionalAddInfos()
		{
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			var fallbackDetail = new WarehouseCustomsFallbackDetail();
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertEquals("AdditionalAddInfos", 0, lineDetails.AdditionalAddInfos.Count());
		}

		public void TestRemarks() => CombineAssertions(() =>
		{
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BondedWarehouseRemarks = "Remarks"
			};
			var fallbackDetail = new WarehouseCustomsFallbackDetail()
			{
				IsExWarehouse = false,
			};
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertEquals("Inwards", "Remarks", lineDetails.Remarks);

			fallbackDetail.IsExWarehouse = true;
			AssertEquals("Outwards", "Remarks", lineDetails.Remarks);
		});

		public void TestCustomsFourthQuantity() => CombineAssertions(() =>
		{
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CustomsFourthQuantity = 10.25m,
				CustomsFourthQuantityUnit = new CodeDescriptionPair6Char()
				{
					Code = "MTQ",
					Description = "Cubic meter"
				}
			};
			var fallbackDetail = new WarehouseCustomsFallbackDetail()
			{
				IsExWarehouse = false,
			};
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			var customsFourthQuantityUnit = lineDetails.CustomsFourthQuantityUnit;
			AssertEquals("Inwards: CustomsFourthQuantity", 10.25m, lineDetails.CustomsFourthQuantity);
			AssertEquals("Inwards: CustomsFourthQuantityUnit - Code", "MTQ", customsFourthQuantityUnit.Code);
			AssertEquals("Inwards: CustomsFourthQuantityUnit - Description", "Cubic meter", customsFourthQuantityUnit.Description);

			fallbackDetail.IsExWarehouse = true;
			customsFourthQuantityUnit = lineDetails.CustomsFourthQuantityUnit;
			AssertEquals("Outwards: CustomsFourthQuantity", 10.25m, lineDetails.CustomsFourthQuantity);
			AssertEquals("Outwards: CustomsFourthQuantityUnit - Code", "MTQ", customsFourthQuantityUnit.Code);
			AssertEquals("Outwards: CustomsFourthQuantityUnit - Description", "Cubic meter", customsFourthQuantityUnit.Description);
		});

		public void TestCustomsFifthQuantity() => CombineAssertions(() =>
		{
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CustomsFifthQuantity = 10.25m,
				CustomsFifthQuantityUnit = new CodeDescriptionPair6Char()
				{
					Code = "MTQ",
					Description = "Cubic meter"
				}
			};
			var fallbackDetail = new WarehouseCustomsFallbackDetail()
			{
				IsExWarehouse = false,
			};
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			var customsFifthQuantityUnit = lineDetails.CustomsFifthQuantityUnit;
			AssertEquals("Inwards: CustomsFifthQuantity", 10.25m, lineDetails.CustomsFifthQuantity);
			AssertEquals("Inwards: CustomsFifthQuantityUnit - Code", "MTQ", customsFifthQuantityUnit.Code);
			AssertEquals("Inwards: CustomsFifthQuantityUnit - Description", "Cubic meter", customsFifthQuantityUnit.Description);

			fallbackDetail.IsExWarehouse = true;
			customsFifthQuantityUnit = lineDetails.CustomsFifthQuantityUnit;
			AssertEquals("Outwards: CustomsFifthQuantity", 10.25m, lineDetails.CustomsFifthQuantity);
			AssertEquals("Outwards: CustomsFifthQuantityUnit - Code", "MTQ", customsFifthQuantityUnit.Code);
			AssertEquals("Outwards: CustomsFifthQuantityUnit - Description", "Cubic meter", customsFifthQuantityUnit.Description);
		});

		public void TestTariff() => CombineAssertions(() =>
		{
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				HarmonisedCode = "1010101010"
			};
			var fallbackDetail = new WarehouseCustomsFallbackDetail()
			{
				IsExWarehouse = false,
			};
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertEquals("Inwards", "1010101010", lineDetails.Tariff);

			fallbackDetail.IsExWarehouse = true;
			AssertEquals("Outwards", "1010101010", lineDetails.Tariff);
		});

		public void TestValueForDuty() => CombineAssertions(() =>
		{
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CustomsValue = 1.23m
			};
			var fallbackDetail = new WarehouseCustomsFallbackDetail()
			{
				IsExWarehouse = false,
			};
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertEquals("Inwards", 1.23m, lineDetails.ValueForDuty);

			fallbackDetail.IsExWarehouse = true;
			AssertEquals("Outwards", 1.23m, lineDetails.ValueForDuty);
		});

		public void TestAllocationInfo()
		{
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddInfoGroupCollection = new List<AddInfoGroup>()
				{
					new AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.WarehouseAllocationInfo, Description = "Allocation Info" },
						AddInfoCollection = new List<AddInfo>()
						{
							new AddInfo() { Key = "AllocationKey", Value = "AK_Value" },
							new AddInfo() { Key = "Quantity", Value = "7" }
						}
					},
					new AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.WarehouseAllocationInfo, Description = "Allocation Info" },
						AddInfoCollection = new List<AddInfo>()
						{
							new AddInfo() { Key = "AllocationKey", Value = "AK_Value 2" },
							new AddInfo() { Key = "Quantity", Value = "7.9" }
						}
					},
					new AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.WarehouseCustomsAddInfo, Description = "No Allocation Info" },
						AddInfoCollection = new List<AddInfo>()
						{
							new AddInfo() { Key = "AllocationKey", Value = "InvalidValue" },
							new AddInfo() { Key = "Quantity", Value = "8" }
						}
					},
				}
			};
			var fallbackDetail = new WarehouseCustomsFallbackDetail()
			{
				IsExWarehouse = false,
			};
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertContainsExactElementsInAnyOrder("Inwards", new[] { "AK_Value", "AK_Value 2" }, lineDetails.AllocationInfos.Select(x => x.AllocationKey));

			fallbackDetail.IsExWarehouse = true;
			AssertContainsExactElementsInAnyOrder("Outwards", new[] { "AK_Value", "AK_Value 2" }, lineDetails.AllocationInfos.Select(x => x.AllocationKey));
		}
	}
}
