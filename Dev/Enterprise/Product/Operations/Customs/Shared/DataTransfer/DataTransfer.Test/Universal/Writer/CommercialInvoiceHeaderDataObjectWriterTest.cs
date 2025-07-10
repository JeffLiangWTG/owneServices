using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest
	{
		public void TestCollectionContentAttribute()
		{
			var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
			var writer1 = new StandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice1)));
			var declarationDataObject1 = writer1.GetDataObject(invoice1);
			var content = declarationDataObject1.CommercialInfo.CommercialInvoiceCollection.Content;
			AssertNull(content);

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice2 = declaration.Invoices.AddNew();
			var writer2 = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var declarationDataObject2 = writer2.GetDataObject(declaration);
			content = declarationDataObject2.CommercialInfo.CommercialInvoiceCollection.Content;
			AssertEquals(CollectionContent.Complete, content);
		}

		public void TestCommercialInvoiceLineCollectionWriterStrategy()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BCO, invoice),
				writerStrategy: new DataObjectWriterStrategyTestClass(s => s != nameof(CommercialInvoiceHeader.CommercialInvoiceLineCollection))),
				new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			var commercialInvoiceHeader = writer.GetDataObject(invoice);
			AssertNull("CommercialInvoiceLineCollection - writerStrategy not allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);

			writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BCO, invoice)),
				new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			commercialInvoiceHeader = writer.GetDataObject(invoice);
			AssertNotNull("CommercialInvoiceLineCollection - writerStrategy allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);
		}

		public void TestExportPackingLinkCollection()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "M";

			var invoice = declaration.Invoices.AddNew();
			var pack1 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			pack1.CW_MarksAndNos = "Marks 1";
			pack1.CW_PackQty = 1;
			pack1.CW_PackType = "1B";

			var pack2 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			pack2.CW_PackQty = 2;
			pack2.CW_PackType = "AE";
			pack2.CW_MarksAndNos = "Marks 2";

			var pack3 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			pack3.CW_PackQty = 3;
			pack3.CW_PackType = "PA";
			pack3.CW_MarksAndNos = "Marks 3";
			pack3.CW_CW_Parent = pack2.PK;

			var npbo1 = (InvoiceHeaderPackagePivot)invoice.PackagesPivot.AddPivotFor(pack1);
			var npbo2 = (InvoiceHeaderPackagePivot)invoice.PackagesPivot.AddPivotFor(pack2);
			var npbo3 = (InvoiceHeaderPackagePivot)invoice.PackagesPivot.AddPivotFor(pack3);

			npbo1.CHZ_NumberOfPacks = 1;
			npbo2.CHZ_NumberOfPacks = 2;
			npbo3.CHZ_NumberOfPacks = 3;

			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BCO, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			var invoiceData = writer.GetDataObject(invoice);

			AssertEquals(2, invoiceData.PackingLinkCollection.Count);
			AssertEquals(1, invoiceData.PackingLinkCollection[0].PackingLineLink);
			AssertEquals(1m, invoiceData.PackingLinkCollection[0].PackedQuantity);
			AssertEquals(2, invoiceData.PackingLinkCollection[1].PackingLineLink);
			AssertEquals(3m, invoiceData.PackingLinkCollection[1].PackedQuantity);
		}

		public void TestChangeOfOwnershipDataMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var boFactory = Factory.BOFactory;
				var helper = new WhsDataTestHelper(boFactory);
				var changeOfOwnershipCusProcedure = helper.ChangeOfOwnershipCusProcedure;
				var importer = helper.Importer;
				var importerMiscServ = importer.MiscServ;
				importerMiscServ.OM_IMPartAttrib1Name = "VIN1";
				importerMiscServ.OM_IMPartAttrib2Name = "ENGINE2";
				importerMiscServ.OM_IMPartAttrib3Name = "COLOUR3";
				var importerPartRelation = helper.Part.RelatedOrganisations.FindByOrganisationAndRelationship(importer, OrgPartRelation.RelationshipTypes.Owner);
				importerPartRelation.OU_UsePartAttrib1 = true;
				importerPartRelation.OU_UsePartAttrib2 = true;
				importerPartRelation.OU_UsePartAttrib3 = true;
				var owner = helper.Owner;
				var ownerMiscServ = owner.MiscServ;
				ownerMiscServ.OM_IMPartAttrib1Name = "COLOUR1";
				ownerMiscServ.OM_IMPartAttrib2Name = "VIN2";
				ownerMiscServ.OM_IMPartAttrib3Name = "ENGINE3";
				var ownerPartRelation = helper.OwnerPart.RelatedOrganisations.FindByOrganisationAndRelationship(owner, OrgPartRelation.RelationshipTypes.Owner);
				ownerPartRelation.OU_UsePartAttrib1 = true;
				ownerPartRelation.OU_UsePartAttrib2 = true;
				ownerPartRelation.OU_UsePartAttrib3 = true;
				var declaration = (BaseJobDeclaration)boFactory.New<ZA.IJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.JE_OH_Importer = importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_OH_Owner = owner.PK;

				declaration.Invoices.DeleteAll();
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
				invoiceLine.JI_PartAttrib1 = "OPATT1";
				invoiceLine.JI_PartAttrib2 = "OPATT2";
				invoiceLine.JI_PartAttrib3 = "OPATT3";
				invoiceLine.JI_Procedure = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartNo.Name] = helper.OwnerPart.OP_PartNum;
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartAttrib1.Name] = "PATT1";
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartAttrib2.Name] = "PATT2";
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartAttrib3.Name] = "PATT3";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.SaveForTesting();
				var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(invoiceLine.JI_AddInfo);
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode, (ZString)"P");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1, (ZString)"1");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2, (ZString)"2");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3, (ZString)"3");
				invoiceLine.JI_AddInfo = AddInfoParser.Serialise(addInfos);
				Factory.SaveForTesting();
				var entry = invoiceLine.CusEntryLine.Header;
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BCO, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode), relatedEntry: entry);
				var invoiceData = writer.GetDataObject(invoice);
				AssertEquals(1, invoiceData.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				var addInfoDatas = invoiceLineData.AddInfoCollection;
				AssertEquals("NewOwnerProductCode", helper.OwnerPart.OP_PartNum, addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode).Value);
				AssertEquals("NewOwnerPartAttribute1", "PATT1", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1).Value);
				AssertEquals("NewOwnerPartAttribute2", "PATT2", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2).Value);
				AssertEquals("NewOwnerPartAttribute3", "PATT3", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3).Value);
				AssertEquals("invoiceLineData.PartNo", helper.Part.OP_PartNum, invoiceLineData.PartNo);
				AssertEquals("VIN1", "OPATT1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN1").Value);
				AssertEquals("ENGINE2", "OPATT2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE2").Value);
				AssertEquals("COLOUR3", "OPATT3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR3").Value);
				AssertNull("COLOUR1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR1"));
				AssertNull("VIN2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN2"));
				AssertNull("ENGINE3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE3"));

				writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode), relatedEntry: entry);
				invoiceData = writer.GetDataObject(invoice);
				AssertEquals(1, invoiceData.CommercialInvoiceLineCollection.Count);
				invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				addInfoDatas = invoiceLineData.AddInfoCollection;
				AssertNull("NewOwnerProductCode", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode));
				AssertNull("NewOwnerPartAttribute1", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1));
				AssertNull("NewOwnerPartAttribute2", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2));
				AssertNull("NewOwnerPartAttribute3", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3));
				AssertEquals("invoiceLineData.PartNo", helper.OwnerPart.OP_PartNum, invoiceLineData.PartNo);
				AssertNull("VIN1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN1"));
				AssertNull("ENGINE2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE2"));
				AssertNull("COLOUR3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR3"));
				AssertEquals("COLOUR1", "PATT1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR1").Value);
				AssertEquals("VIN2", "PATT2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN2").Value);
				AssertEquals("ENGINE3", "PATT3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE3").Value);

				ownerMiscServ.OM_IMPartAttrib2Name = "";
				Factory.SaveForTesting();
				writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BCO, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode), relatedEntry: entry);
				invoiceData = writer.GetDataObject(invoice);
				AssertEquals(1, invoiceData.CommercialInvoiceLineCollection.Count);
				invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				addInfoDatas = invoiceLineData.AddInfoCollection;
				AssertEquals("NewOwnerProductCode", helper.OwnerPart.OP_PartNum, addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode).Value);
				AssertEquals("NewOwnerPartAttribute1", "PATT1", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1).Value);
				AssertNull("NewOwnerPartAttribute2", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2));
				AssertEquals("NewOwnerPartAttribute3", "PATT3", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3).Value);
				AssertEquals("invoiceLineData.PartNo", helper.Part.OP_PartNum, invoiceLineData.PartNo);
				AssertEquals("VIN1", "OPATT1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN1").Value);
				AssertEquals("ENGINE2", "OPATT2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE2").Value);
				AssertEquals("COLOUR3", "OPATT3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR3").Value);
				AssertNull("COLOUR1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR1"));
				AssertNull("VIN2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN2"));
				AssertNull("ENGINE3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE3"));
			}
		}

		public void TestChangeOfOwnershipDataMappings_WithSerialNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var boFactory = Factory.BOFactory;
				var helper = new WhsDataTestHelper(boFactory);
				var changeOfOwnershipCusProcedure = helper.ChangeOfOwnershipCusProcedure;
				var importer = helper.Importer;
				var importerMiscServ = importer.MiscServ;
				importerMiscServ.OM_IMPartAttrib1Name = "VIN1";
				importerMiscServ.OM_IMPartAttrib2Name = "ENGINE2";
				importerMiscServ.OM_IMPartAttrib3Name = "COLOUR3";
				var importerPartRelation = helper.Part.RelatedOrganisations.FindByOrganisationAndRelationship(importer, OrgPartRelation.RelationshipTypes.Owner);
				importerPartRelation.OU_UsePartAttrib1 = true;
				importerPartRelation.OU_UsePartAttrib2 = true;
				importerPartRelation.OU_UsePartAttrib3 = true;
				importerPartRelation.OU_UseSerialNumber = true;
				var owner = helper.Owner;
				var ownerMiscServ = owner.MiscServ;
				ownerMiscServ.OM_IMPartAttrib1Name = "COLOUR1";
				ownerMiscServ.OM_IMPartAttrib2Name = "VIN2";
				ownerMiscServ.OM_IMPartAttrib3Name = "ENGINE3";
				var ownerPartRelation = helper.OwnerPart.RelatedOrganisations.FindByOrganisationAndRelationship(owner, OrgPartRelation.RelationshipTypes.Owner);
				ownerPartRelation.OU_UsePartAttrib1 = true;
				ownerPartRelation.OU_UsePartAttrib2 = true;
				ownerPartRelation.OU_UsePartAttrib3 = true;
				ownerPartRelation.OU_UseSerialNumber = true;
				var declaration = (BaseJobDeclaration)boFactory.New<ZA.IJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.JE_OH_Importer = importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_OH_Owner = owner.PK;

				declaration.Invoices.DeleteAll();
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
				invoiceLine.JI_PartAttrib1 = "OPATT1";
				invoiceLine.JI_PartAttrib2 = "OPATT2";
				invoiceLine.JI_PartAttrib3 = "OPATT3";
				invoiceLine.JI_SerialNumber = "OSerialNum";
				invoiceLine.JI_Procedure = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartNo.Name] = helper.OwnerPart.OP_PartNum;
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartAttrib1.Name] = "PATT1";
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartAttrib2.Name] = "PATT2";
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartAttrib3.Name] = "PATT3";
				invoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerSerialNum.Name] = "PSerialNum";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.SaveForTesting();
				var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(invoiceLine.JI_AddInfo);
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode, (ZString)"P");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1, (ZString)"1");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2, (ZString)"2");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3, (ZString)"3");
				addInfos.Update(Constants.AddInfoKeys.InvoiceLine.NewOwnerSerialNumber, (ZString)"SN");
				invoiceLine.JI_AddInfo = AddInfoParser.Serialise(addInfos);
				Factory.SaveForTesting();
				var entry = invoiceLine.CusEntryLine.Header;
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BCO, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode), relatedEntry: entry);
				var invoiceData = writer.GetDataObject(invoice);
				AssertEquals(1, invoiceData.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				var addInfoDatas = invoiceLineData.AddInfoCollection;
				AssertEquals("NewOwnerProductCode", helper.OwnerPart.OP_PartNum, addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode).Value);
				AssertEquals("NewOwnerPartAttribute1", "PATT1", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1).Value);
				AssertEquals("NewOwnerPartAttribute2", "PATT2", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2).Value);
				AssertEquals("NewOwnerPartAttribute3", "PATT3", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3).Value);
				AssertEquals("invoiceLineData.PartNo", helper.Part.OP_PartNum, invoiceLineData.PartNo);
				AssertEquals("VIN1", "OPATT1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN1").Value);
				AssertEquals("ENGINE2", "OPATT2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE2").Value);
				AssertEquals("COLOUR3", "OPATT3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR3").Value);
				AssertEquals("NewOwnerSerialNumber", "PSerialNum", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerSerialNumber).Value);
				AssertEquals("Serial Number", "OSerialNum", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "Serial Number").Value);
				AssertNull("COLOUR1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR1"));
				AssertNull("VIN2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN2"));
				AssertNull("ENGINE3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE3"));

				writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode), relatedEntry: entry);
				invoiceData = writer.GetDataObject(invoice);
				AssertEquals(1, invoiceData.CommercialInvoiceLineCollection.Count);
				invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				addInfoDatas = invoiceLineData.AddInfoCollection;
				AssertNull("NewOwnerProductCode", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode));
				AssertNull("NewOwnerPartAttribute1", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1));
				AssertNull("NewOwnerPartAttribute2", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2));
				AssertNull("NewOwnerPartAttribute3", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3));
				AssertNull("NewOwnerSerialNumber", addInfoDatas.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.NewOwnerSerialNumber));
				AssertEquals("invoiceLineData.PartNo", helper.OwnerPart.OP_PartNum, invoiceLineData.PartNo);
				AssertNull("VIN1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN1"));
				AssertNull("ENGINE2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE2"));
				AssertNull("COLOUR3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR3"));
				AssertEquals("Serial Number", "PSerialNum", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "Serial Number").Value);
				AssertEquals("COLOUR1", "PATT1", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "COLOUR1").Value);
				AssertEquals("VIN2", "PATT2", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "VIN2").Value);
				AssertEquals("ENGINE3", "PATT3", invoiceLineData.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == "ENGINE3").Value);
			}
		}

		public void TestBondedWarehouseDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var helper = new WhsDataTestHelper(Factory.BOFactory);
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = helper.Importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_OA_Warehouse2 = helper.Warehouse.MainAddress.PK;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = JobMessageTypeList.Codes.Import;
				entry.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				invoiceLine.JI_BondedWhsQuantity = 10m;
				invoiceLine.JI_BondedWhsUnitQty = "NO";
				invoiceLine.JI_BondedWarehouseRemarks = "Test Remarks";
				invoiceLine.JI_BondedWHSOrderNumber = "Order Number";
				invoiceLine.JI_BondedWHSOrderLineNumber = 4;

				var writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entry)));
				var shipmentData = writer.GetDataObject(entry);
				var invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				AssertEquals("invoiceLineData.BondedWarehouseQuantity", 10m, invoiceLineData.BondedWarehouseQuantity);
				AssertEquals("invoiceLineData.BondedWarehouseQuantityUnit.Code", "NO", invoiceLineData.BondedWarehouseQuantityUnit.Code);
				AssertEquals("invoiceLineData.BondedWarehouseQuantityUnit.Description", "Unit", invoiceLineData.BondedWarehouseQuantityUnit.Description);
				AssertEquals("invoiceLineData.BondedWarehouseRemarks", "Test Remarks", invoiceLineData.BondedWarehouseRemarks);
				AssertEquals("invoiceLineData.BondedWHSOrderNumber", "Order Number", invoiceLineData.BondedWHSOrderNumber);
				AssertEquals("invoiceLineData.BondedWHSOrderLineNumber", (ZShort)4, invoiceLineData.BondedWHSOrderLineNumber);

				foreach (var recipientRoleType in new[] { RecipientRoleType.BWI, RecipientRoleType.BWR, RecipientRoleType.BCO })
				{
					invoiceLine.JI_PartNo = ZString.Empty;
					invoiceLine.ComponentInventoryCollection.RemoveAll();
					invoiceLine.JI_BondedWhsQuantity = 10m;
					writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, entry)));
					shipmentData = writer.GetDataObject(entry);
					invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
					AssertNull("invoiceLineData.BondedWarehouseQuantity", invoiceLineData.BondedWarehouseQuantity);
					AssertNull("invoiceLineData.BondedWarehouseQuantityUnit", invoiceLineData.BondedWarehouseQuantityUnit);
					AssertNull("invoiceLineData.BondedWarehouseRemarks", invoiceLineData.BondedWarehouseRemarks);
					AssertNull("invoiceLineData.BondedWHSOrderNumber", invoiceLineData.BondedWHSOrderNumber);
					AssertNull("invoiceLineData.BondedWHSOrderLineNumber", invoiceLineData.BondedWHSOrderLineNumber);

					invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
					writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, entry)));
					shipmentData = writer.GetDataObject(entry);
					invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
					AssertEquals("invoiceLineData.BondedWarehouseQuantity", 10m, invoiceLineData.BondedWarehouseQuantity);
					AssertEquals("invoiceLineData.BondedWarehouseQuantityUnit.Code", "NO", invoiceLineData.BondedWarehouseQuantityUnit.Code);
					AssertEquals("invoiceLineData.BondedWarehouseQuantityUnit.Description", "Unit", invoiceLineData.BondedWarehouseQuantityUnit.Description);
					AssertEquals("invoiceLineData.BondedWarehouseRemarks", "Test Remarks", invoiceLineData.BondedWarehouseRemarks);
					AssertEquals("invoiceLineData.BondedWHSOrderNumber", "Order Number", invoiceLineData.BondedWHSOrderNumber);
					AssertEquals("invoiceLineData.BondedWHSOrderLineNumber", (ZShort)4, invoiceLineData.BondedWHSOrderLineNumber);

					invoiceLine.JI_BondedWhsQuantity = 0;
					writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, entry)));
					shipmentData = writer.GetDataObject(entry);
					invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
					AssertEquals("invoiceLineData.BondedWarehouseQuantity should be written out when Invoice Line has no Allocated Entry.", 0m, invoiceLineData.BondedWarehouseQuantity);

					invoiceLine.ComponentInventoryCollection.AddNew();
					invoiceLine.JI_BondedWhsQuantity = 10;
					writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, entry)));
					shipmentData = writer.GetDataObject(entry);
					invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
					AssertEquals("invoiceLineData.BondedWarehouseQuantity should be written out when Invoice Line has Allocated Entry and JI_BondedWhsQuantity > 0.", 10m, invoiceLineData.BondedWarehouseQuantity);

					invoiceLine.JI_BondedWhsQuantity = 0;
					writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, entry)));
					shipmentData = writer.GetDataObject(entry);
					invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
					AssertNull("invoiceLineData.BondedWarehouseQuantity shouldn't be written out when Invoice Line has allocated entry but JI_BondedWhsQuantity = 0.", invoiceLineData.BondedWarehouseQuantity);
				}
			}
		}

		public void TestBondedWarehouseDetailsIncluded_InwardProcessing()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("IWDPROC", Core.Constants.CountryCodes.Latvia, ZDateTime.Today, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var procedure = CreateCusProcedureForInwardProcessingTest(Core.Constants.CountryCodes.Latvia);

				var helper = new WhsDataTestHelper(Factory.BOFactory);

				helper.Importer.CompanyData.OB_CusInventoryForInwardProcessing = true;
				Factory.SaveForTesting();

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = helper.Importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = procedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				entryInstruction.CEI_OA_Warehouse2 = helper.Warehouse.MainAddress.PK;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = JobMessageTypeList.Codes.Import;
				entry.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_PartNo = helper.Part.OP_PartNum;

				invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;
				invoiceLine.JI_BondedWhsQuantity = 10m;
				invoiceLine.JI_BondedWhsUnitQty = "NO";
				invoiceLine.JI_BondedWarehouseRemarks = "Test Remarks";
				invoiceLine.JI_BondedWHSOrderNumber = "Order Number";
				invoiceLine.JI_BondedWHSOrderLineNumber = 4;

				var recipientRoleType = RecipientRoleType.BWI;
				var writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, entry)));
				var shipmentData = writer.GetDataObject(entry);

				var invoiceLineData = shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				CombineAssertions(() =>
				{
					AssertEquals("invoiceLineData.BondedWarehouseQuantity", 10m, invoiceLineData.BondedWarehouseQuantity);
					AssertEquals("invoiceLineData.BondedWarehouseQuantityUnit.Code", "NO", invoiceLineData.BondedWarehouseQuantityUnit.Code);
					AssertEquals("invoiceLineData.BondedWarehouseRemarks", "Test Remarks", invoiceLineData.BondedWarehouseRemarks);
					AssertEquals("invoiceLineData.BondedWHSOrderNumber", "Order Number", invoiceLineData.BondedWHSOrderNumber);
					AssertEquals("invoiceLineData.BondedWHSOrderLineNumber", (ZShort)4, invoiceLineData.BondedWHSOrderLineNumber);
				});
			}
		}

		public void TestBillInfoOnCommercialInvoiceHeader()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;

			var bill = AddNewBill(declaration, null, BillTypeList.Codes.MasterBill, "masterbill1");
			AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill1");

			bill = AddNewBill(declaration, null, BillTypeList.Codes.MasterBill, "masterbill2");
			AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill2a");
			var billForInvoiceHeader = AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill2b");
			AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill2c");

			bill = AddNewBill(declaration, null, BillTypeList.Codes.MasterBill, "masterbill3");
			AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill3");

			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JZ_CU_RelatedHouseBill = billForInvoiceHeader.PK;

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			invoiceMock.Setup(m => m.SupportsRelatedBill).Returns(true);
			var invoiceData = writer.GetDataObject(invoice);
			invoiceMock.Verify(m => m.SupportsRelatedBill, Times.Once);

			AssertEquals(invoiceData.BillNumber, "housebill2b");
			AssertEquals(invoiceData.BillType.Code, WayBillTypeList.Codes.House);

			invoice.JZ_CU_RelatedHouseBill = ZGuid.Empty;
			invoiceData = writer.GetDataObject(invoice);
			invoiceMock.Verify(m => m.SupportsRelatedBill, Times.Exactly(2));
			AssertEquals(true, invoiceData.BillNumber.HasValue);
			AssertEquals(true, invoiceData.AgreedExchangeRate.HasValue);
			AssertEquals(invoiceData.BillNumber, "");

			invoiceMock.Setup(m => m.SupportsRelatedBill).Returns(false);
			invoiceData = writer.GetDataObject(invoice);
			invoiceMock.Verify(m => m.SupportsRelatedBill, Times.Exactly(3));
			AssertEquals(false, invoiceData.BillNumber.HasValue);
		}

		public void TestCommercialInvoiceHeaderMappings()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;

			var bill = AddNewBill(declaration, null, BillTypeList.Codes.MasterBill, "masterbill1");
			AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill1");

			bill = AddNewBill(declaration, null, BillTypeList.Codes.MasterBill, "masterbill2");
			AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill2a");
			AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill2b");
			AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill2c");

			bill = AddNewBill(declaration, null, BillTypeList.Codes.MasterBill, "masterbill3");
			AddNewBill(declaration, bill, BillTypeList.Codes.HouseBill, "housebill3");

			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			var localCurrencyCode = invoice.LocalCurrencyCode;
			var localCurrency = invoice.LocalCurrency;

			var note1 = invoice.Notes.AddNew(true, "SILLY DATA 2", "GOODBYE WORLD");
			var note2 = invoice.Notes.AddNew(true, "SILLY DATA 1", "HELLO WORLD");

			var groupHeader = invoice.GroupHeader;
			var groupCharge1 = SetupJobComInvHeaderCharge(groupHeader.Charges.AddNew(), ZBool.True, 1000m, Common.CustomsChargeTypeList.Codes.OverseasInsurance, Core.Constants.CurrencyCodes.Australia, ChargeDistributeByList.Codes.Value, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.30m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.3m, Core.Constants.PaymentType.Collect);
			var groupCharge2 = SetupJobComInvHeaderCharge(groupHeader.Charges.AddNew(), ZBool.False, 2000m, Common.CustomsChargeTypeList.Codes.OverseasFreight, Core.Constants.CurrencyCodes.UnitedStates, ChargeDistributeByList.Codes.Weight, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.50m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.4m, Core.Constants.PaymentType.Prepaid);
			var charge1 = SetupJobComInvHeaderCharge(invoice.Charges.AddNew(), ZBool.False, 4000m, Common.CustomsChargeTypeList.Codes.DeductionCharge, Core.Constants.CurrencyCodes.NewZealand, ChargeDistributeByList.Codes.Volume, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.10m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.5m, Core.Constants.PaymentType.Prepaid);
			var charge2 = SetupJobComInvHeaderCharge(invoice.Charges.AddNew(), ZBool.True, 6000m, Common.CustomsChargeTypeList.Codes.PackingCost, Core.Constants.CurrencyCodes.Singapore, ChargeDistributeByList.Codes.Value, ZString.Empty, 1m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.6m, Core.Constants.PaymentType.Collect);
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var invoiceLine1 = invoice.JobComInvoiceLines[0];
			var invoiceLine1Charge1 = SetupJobComInvHeaderCharge(invoiceLine1.Charges.AddNew(), ZBool.False, 400m, Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, Core.Constants.CurrencyCodes.Australia, ChargeDistributeByList.Codes.Value, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 0.9m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.1m, Core.Constants.PaymentType.Prepaid);
			var invoiceLine1Charge2 = SetupJobComInvHeaderCharge(invoiceLine1.Charges.AddNew(), ZBool.True, 600m, Common.CustomsChargeTypeList.Codes.Discount, Core.Constants.CurrencyCodes.Indonesia, ChargeDistributeByList.Codes.Value, ZString.Empty, 1m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.2m, Core.Constants.PaymentType.Collect);

			var invoiceLine2 = invoice.JobComInvoiceLines[1];
			var invoiceLine2Charge1 = SetupJobComInvHeaderCharge(invoiceLine2.Charges.AddNew(), ZBool.False, 400m, Common.CustomsChargeTypeList.Codes.OtherCharges, Core.Constants.CurrencyCodes.Australia, ChargeDistributeByList.Codes.Value, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 0.9m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.2m, Core.Constants.PaymentType.Prepaid);
			var invoiceLine2Charge2 = SetupJobComInvHeaderCharge(invoiceLine2.Charges.AddNew(), ZBool.True, 600m, Common.CustomsChargeTypeList.Codes.LandingCharges, Core.Constants.CurrencyCodes.Indonesia, ChargeDistributeByList.Codes.Value, ZString.Empty, 1m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.1m, Core.Constants.PaymentType.Collect);

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			declaration.ResumeApportionment();
			var invoiceData = writer.GetDataObject(invoice);
			AssertContents(invoiceData);

			var noteCollection = invoiceData.NoteCollection;
			AssertNotNull("noteCollection", noteCollection);
			AssertEquals("noteCollection.Count", 2, noteCollection.Count);
			AssertContents(noteCollection[0], true, "SILLY DATA 1", "HELLO WORLD");
			AssertContents(noteCollection[1], true, "SILLY DATA 2", "GOODBYE WORLD");

			var invoiceChargeCollection = invoiceData.CommercialChargeCollection;
			AssertNotNull(invoiceChargeCollection);
			AssertEquals(8, invoiceChargeCollection.Count);
			AssertContents(invoiceChargeCollection[0], ZBool.False, 57134.76m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.DeductionCharge, Common.CustomsChargeTypeList.Descriptions.DeductionCharge, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Volume, ChargeDistributeByList.Descriptions.Volume), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 1.10m, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.5m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(invoiceChargeCollection[1], ZBool.True, 600m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Indonesia, "Indonesian Rupiah"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 0.92d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.2m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[2], ZBool.False, 444.44m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, Common.CustomsChargeTypeList.Descriptions.ForeignInlandFreight, 35), GetCodeDescriptionPair(localCurrencyCode, localCurrency.RX_Desc), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.1m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[3], ZBool.True, 600m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.LandingCharges, Common.CustomsChargeTypeList.Descriptions.LandingCharges, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Indonesia, "Indonesian Rupiah"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 0.92d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.1m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[4], ZBool.False, 45707.81m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Weight, ChargeDistributeByList.Descriptions.Weight), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZBool.True, 0.4m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[5], ZBool.True, 34280.86m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.3m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[6], ZBool.False, 444.44m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), GetCodeDescriptionPair(localCurrencyCode, localCurrency.RX_Desc), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.2m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[7], ZBool.True, 68561.71m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.PackingCost, Common.CustomsChargeTypeList.Descriptions.PackingCost, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.6m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));

			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			declaration.ResumeApportionment();
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];
			var commercialInvoiceLine1ChargeCollection = commercialInvoiceLine1.CommercialChargeCollection;
			AssertNotNull(commercialInvoiceLine1ChargeCollection);
			AssertEquals(6, commercialInvoiceLine1ChargeCollection.Count);
			AssertContents(commercialInvoiceLine1ChargeCollection[0], ZBool.False, 23932.18m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.DeductionCharge, Common.CustomsChargeTypeList.Descriptions.DeductionCharge, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Volume, ChargeDistributeByList.Descriptions.Volume), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.5m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine1ChargeCollection[1], ZBool.True, 600m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Indonesia, "Indonesian Rupiah"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 0.92d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.2m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(commercialInvoiceLine1ChargeCollection[2], ZBool.False, 400m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, Common.CustomsChargeTypeList.Descriptions.ForeignInlandFreight, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 0.9m, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.1m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine1ChargeCollection[3], ZBool.False, 19145.74m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Weight, ChargeDistributeByList.Descriptions.Weight), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZBool.True, 0.4m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine1ChargeCollection[4], ZBool.True, 14359.31m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.3m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(commercialInvoiceLine1ChargeCollection[5], ZBool.True, 28718.61m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.PackingCost, Common.CustomsChargeTypeList.Descriptions.PackingCost, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.6m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));

			var commercialInvoiceLine2 = commercialInvoiceLineCollection[1];
			var commercialInvoiceLine2ChargeCollection = commercialInvoiceLine2.CommercialChargeCollection;
			declaration.ResumeApportionment();
			AssertNotNull(commercialInvoiceLine2ChargeCollection);
			AssertEquals(6, commercialInvoiceLine2ChargeCollection.Count);
			AssertContents(commercialInvoiceLine2ChargeCollection[0], ZBool.False, 33202.58m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.DeductionCharge, Common.CustomsChargeTypeList.Descriptions.DeductionCharge, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Volume, ChargeDistributeByList.Descriptions.Volume), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.5m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine2ChargeCollection[1], ZBool.True, 600m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.LandingCharges, Common.CustomsChargeTypeList.Descriptions.LandingCharges, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Indonesia, "Indonesian Rupiah"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 0.92d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.1m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(commercialInvoiceLine2ChargeCollection[2], ZBool.False, 26562.07m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Weight, ChargeDistributeByList.Descriptions.Weight), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZBool.True, 0.4m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine2ChargeCollection[3], ZBool.True, 19921.55m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.3m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(commercialInvoiceLine2ChargeCollection[4], ZBool.False, 400m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 0.9m, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.2m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine2ChargeCollection[5], ZBool.True, 39843.10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.PackingCost, Common.CustomsChargeTypeList.Descriptions.PackingCost, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.6m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
		}

		public void TestInvoiceHeaderCusCodeDataMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var usDeclaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
				var invoice = usDeclaration.Invoices.AddNew();
				var invoiceCusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)invoice;
				var relatedDocumentString = "RLD";
				var relatedDocumentAirWaybillNumber = "AW";
				Type relatedDocumentType = null;
				invoiceCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(relatedDocumentString, out relatedDocumentType);
				var relatedDocument = (CusCodeData)Factory.New(relatedDocumentType);
				relatedDocument.Parent = invoice;
				relatedDocument.CY_Code = relatedDocumentAirWaybillNumber;
				relatedDocument.CY_Data = "AWB23423";
				Factory.SaveForTesting();
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedStates));
				var invoiceData = writer.GetDataObject(invoice);
				AssertEquals("invoiceData.CustomsReferenceCollection.Count", 1, invoiceData.CustomsReferenceCollection.Count);
				AssertContents(invoiceData.CustomsReferenceCollection[0], GetCodeDescriptionPair(relatedDocumentString, "Related Document"), GetCodeDescriptionPair(relatedDocumentAirWaybillNumber, "Air Waybill Number"), "AWB23423", 0, ZBool.False);
			}
		}

		public void TestInvoiceHeaderCustomFieldsMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.SetUserDefinedValue("CustomField1", (ZString)"Hello World");
			invoice.SetUserDefinedValue("CustomField2", ZDateTime.BrettsBirthday);
			invoice.SetUserDefinedValue("CustomField3", (ZDecimal)200.5);
			invoice.SetUserDefinedValue("CustomField4", (ZBool)true);

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedKingdom));
			var invoiceData = writer.GetDataObject(invoice);

			AssertEquals("Count of CustomizedFieldCollection", 4, invoiceData.CustomizedFieldCollection.Count);
			invoiceData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "CustomField1", "Hello World");
			invoiceData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "CustomField2", ZDateTime.BrettsBirthday.ToISO8601String());
			invoiceData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "CustomField3", "200.5");
			invoiceData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "CustomField4", "true");
		}

		public void TestInvoiceLineCustomsValueMapping()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedStates));
			var invoiceData = writer.GetDataObject(invoice);
			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
			AssertEquals("invoiceLineData.CustomsValue", 1000m, invoiceLineData.CustomsValue);
		}

		public void TestInvoiceLineWorkflowCustomFeildsMapping()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetUserDefinedValue("CustomField1", (ZString)"Hello World");
			invoiceLine.SetUserDefinedValue("CustomField2", ZDateTime.BrettsBirthday);
			invoiceLine.SetUserDefinedValue("CustomField3", (ZDecimal)200.5);
			invoiceLine.SetUserDefinedValue("CustomField4", (ZBool)true);

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedKingdom));
			var invoiceData = writer.GetDataObject(invoice);
			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];

			AssertEquals("Count of CustomizedFieldCollection", 4, invoiceLineData.CustomizedFieldCollection.Count);
			invoiceLineData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "CustomField1", "Hello World");
			invoiceLineData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "CustomField2", ZDateTime.BrettsBirthday.ToISO8601String());
			invoiceLineData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "CustomField3", "200.5");
			invoiceLineData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "CustomField4", "true");
		}

		public void TestInvoiceLineCusAddInfoAndCusCodeDataMappings()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCusAddInfoSupporter = (ICusAddInfoTypeSupporter)invoiceLine;
			Type dotType = null;
			invoiceLineCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDOT, out dotType);
			var dot = (CusAddInfo)Factory.New(dotType);
			dot.B7_ParentID = invoiceLine.PK;
			dot.B7_ParentTableCode = invoiceLine.TablePrefix;
			dot.B7_AddInfoData = USDOTAddInfoSchema.Constants.US_DOTCommercialDesc.Substring(3) + "=DESC1234";
			Type fdaType = null;
			invoiceLineCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USFDA, out fdaType);
			var fda = (CusAddInfo)Factory.New(fdaType);
			fda.B7_ParentID = invoiceLine.PK;
			fda.B7_ParentTableCode = invoiceLine.TablePrefix;
			fda.B7_AddInfoData = USFDAAddInfoSchema.Constants.US_FDACommercialDesc.Substring(3) + "=FDADESC123";

			var invoiceLineCusCodeDataSupporter = (ICusCodeDataTypeSupporter)invoiceLine;
			var feeString = "FEE";
			var feeMerchandiseProcessing = "499";
			Type feeType = null;
			invoiceLineCusCodeDataSupporter.GetCusCodeDataTypes().TryGetValue(feeString, out feeType);
			var fee = (CusCodeData)Factory.New(feeType);
			fee.Parent = invoiceLine;
			fee.CY_Code = feeMerchandiseProcessing;
			fee.CY_Data = "100";
			Factory.SaveForTesting();
			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedStates));
			var invoiceData = writer.GetDataObject(invoice);
			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
			AssertEquals("invoiceLineData.AddInfoGroupCollection.Count", 2, invoiceLineData.AddInfoGroupCollection.Count);
			AssertContents(invoiceLineData.AddInfoGroupCollection[0], GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USFDA, "FDA"), new List<AddInfo>(new[] { new AddInfo() { Key = USFDAAddInfoSchema.Constants.US_FDACommercialDesc.Substring(3), Value = "FDADESC123" } }));
			AssertContents(invoiceLineData.AddInfoGroupCollection[1], GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USDOT, "DOT"), new List<AddInfo>(new[] { new AddInfo() { Key = USDOTAddInfoSchema.Constants.US_DOTCommercialDesc.Substring(3), Value = "DESC1234" } }));

			AssertEquals("invoiceLineData.CustomsReferenceCollection.Count", 1, invoiceLineData.CustomsReferenceCollection.Count);
			AssertContents(invoiceLineData.CustomsReferenceCollection[0], GetCodeDescriptionPair(feeString, "Fee"), GetCodeDescriptionPair(feeMerchandiseProcessing, "499 Desc from DB"), "100", 0, ZBool.False);
		}

		public void TestShiptToPartyAddressInInvoiceHeaderIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var shipToParty = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			shipToParty.OH_IsConsignee = ZBool.True;

			var shipToPartyAddress = shipToParty.Addresses.AddNew(OrgAddressType.Office, false);
			shipToPartyAddress.OA_Address1 = "ADR";
			shipToPartyAddress.OA_City = "AE";
			shipToPartyAddress.OA_PostCode = "0123";

			invoice.JZ_OA_ShipToPartyAddress = shipToPartyAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var shipToPartyDataObject = invoiceData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ShipToParty));
			AssertEquals("manufacturerDataObject.Address1", "ADR", shipToPartyDataObject.Address1);
			AssertEquals("manufacturerDataObject.AddressShortCode", "ADR", shipToPartyDataObject.AddressShortCode.Value);
		}

		public void TestShiptToPartyAddressInInvoiceLineIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);

			Assert(invoice.JobComInvoiceLines.Any());
			var invoiceLine1 = invoice.JobComInvoiceLines[0];

			var shiptToParty = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			shiptToParty.OH_IsConsignor = ZBool.True;

			var shiptToPartyAddress = shiptToParty.Addresses.AddNew(OrgAddressType.Office, false);
			shiptToPartyAddress.OA_Address1 = "ADR";
			shiptToPartyAddress.OA_City = "AE";
			shiptToPartyAddress.OA_PostCode = "0123";

			invoiceLine1.JI_OA_ShipToPartyAddress = shiptToPartyAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];

			var shiptToPartyDataObject = commercialInvoiceLine1.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ShipToParty));
			AssertEquals("ShiptToPartyDataObject.Address1", "ADR", shiptToPartyDataObject.Address1);
			AssertEquals("ShiptToPartyDataObject.AddressShortCode", "ADR", shiptToPartyDataObject.AddressShortCode.Value);
		}

		public void TestSellerAddressInInvoiceHeaderIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var seller = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			seller.OH_IsConsignor = ZBool.True;

			var sellerAddress = seller.Addresses.AddNew(OrgAddressType.Office, false);
			sellerAddress.OA_Address1 = "ADR";
			sellerAddress.OA_City = "AE";
			sellerAddress.OA_PostCode = "0123";

			invoice.JZ_OA_SellerAddress = sellerAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var sellerDataObject = invoiceData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.Seller);
			AssertEquals("manufacturerDataObject.Address1", "ADR", sellerDataObject.Address1);
			AssertEquals("manufacturerDataObject.AddressShortCode", "ADR", sellerDataObject.AddressShortCode.Value);
		}

		public void TestSelleryAddressInInvoiceLineIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);

			Assert(invoice.JobComInvoiceLines.Any());
			var invoiceLine1 = invoice.JobComInvoiceLines[0];

			var seller = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			seller.OH_IsConsignor = ZBool.True;

			var sellerAddress = seller.Addresses.AddNew(OrgAddressType.Office, false);
			sellerAddress.OA_Address1 = "ADR";
			sellerAddress.OA_City = "AE";
			sellerAddress.OA_PostCode = "0123";

			invoiceLine1.JI_OA_Seller = sellerAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];

			var sellerDataObject = commercialInvoiceLine1.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.Seller);
			AssertEquals("SellerDataObject.Address1", "ADR", sellerDataObject.Address1);
			AssertEquals("SellerDataObject.AddressShortCode", "ADR", sellerDataObject.AddressShortCode.Value);
		}

		public void TestCommercialInvoiceLineParentIDMappping()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_Description = "1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_Description = "2";

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);
			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			AssertEquals("commercialInvoiceLineCollection.Count", 2, commercialInvoiceLineCollection.Count);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];
			var commercialInvoiceLine2 = commercialInvoiceLineCollection[1];

			CombineAssertions(delegate
			{
				AssertEquals("commercialInvoiceLine1.LineNo", 1, commercialInvoiceLine1.LineNo);
				AssertEquals("commercialInvoiceLine1.Description", "1", commercialInvoiceLine1.Description);
				AssertEquals("commercialInvoiceLine1.ParentLineNo", 0, commercialInvoiceLine1.ParentLineNo);

				AssertEquals("commercialInvoiceLine2.LineNo", 2, commercialInvoiceLine2.LineNo);
				AssertEquals("commercialInvoiceLine2.Description", "2", commercialInvoiceLine2.Description);
				AssertEquals("commercialInvoiceLine2.ParentLineNo", 1, commercialInvoiceLine2.ParentLineNo);
			});
		}

		public void TestInvoiceLineAdditionalLinkDataMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var usDeclaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
				usDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = usDeclaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV32423";
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_Description = "LINE1";
				var entry1 = usDeclaration.CustomsEntryHeaders.AddNew();
				entry1.CH_MessageType = "ENS";
				entry1.CH_BGMReference = "IMP3242";
				var cusEntryNumber = entry1.CusEntryNumber;
				var entryLine1 = entry1.MergedLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				var entryLine2 = entry1.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;
				var entry2 = usDeclaration.CustomsEntryHeaders.AddNew();
				entry2.CH_BGMReference = "323KDS";
				entry2.CH_MessageType = "CRL";
				var entryLine3 = entry2.MergedLines.AddNew();
				entryLine3.CL_LineNumber = 1;
				var additionalInvoiceLineEntryLineLink1 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
				additionalInvoiceLineEntryLineLink1.BU_CL = entryLine2.PK;
				additionalInvoiceLineEntryLineLink1.BU_JI = invoiceLine1.PK;
				var additionalInvoiceLineEntryLineLink2 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
				additionalInvoiceLineEntryLineLink2.BU_CL = entryLine1.PK;
				additionalInvoiceLineEntryLineLink2.BU_JI = invoiceLine1.PK;
				var additionalInvoiceLineEntryLineLink3 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
				additionalInvoiceLineEntryLineLink3.BU_CL = entryLine3.PK;
				additionalInvoiceLineEntryLineLink3.BU_JI = invoiceLine1.PK;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_Description = "LINE2";
				Factory.SaveForTesting();
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedStates));
				var invoiceData = writer.GetDataObject(invoice);
				AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 2, invoiceData.CommercialInvoiceLineCollection.Count);
				var invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection[0];
				var invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection[1];
				if (invoiceLine2Data.Description.GetValueOrDefault() == "LINE1")
				{
					invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection[1];
					invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection[0];
				}
				AssertEquals("Description", "LINE1", invoiceLine1Data.Description);
				AssertEquals("invoiceLine1Data.EntryReferenceCollection.Count", 3, invoiceLine1Data.EntryReferenceCollection.Count);
				var entryReferenceData1 = invoiceLine1Data.EntryReferenceCollection.First(x => x.LineNumber.GetValueOrDefault() == 1 && x.Reference.GetValueOrDefault() == "IMP3242");
				var entryReferenceData2 = invoiceLine1Data.EntryReferenceCollection.First(x => x.LineNumber.GetValueOrDefault() == 2 && x.Reference.GetValueOrDefault() == "IMP3242");
				var entryReferenceData3 = invoiceLine1Data.EntryReferenceCollection.First(x => x.LineNumber.GetValueOrDefault() == 1 && x.Reference.GetValueOrDefault() == "323KDS");
				AssertContents(entryReferenceData1, 1, "IMP3242", GetCodeDescriptionPair("ENS", "Entry Summary"));
				AssertContents(entryReferenceData2, 2, "IMP3242", GetCodeDescriptionPair("ENS", "Entry Summary"));
				AssertContents(entryReferenceData3, 1, "323KDS", GetCodeDescriptionPair("CRL", "Cargo Release"));
			}
		}

		public void TestManufacturerAddressInInvoiceHeaderIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var manufacturer = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			manufacturer.OH_IsConsignor = ZBool.True;

			var manufacturerAddress = manufacturer.Addresses.AddNew(OrgAddressType.Office, false);
			manufacturerAddress.OA_Address1 = "ADR";
			manufacturerAddress.OA_City = "AE";
			manufacturerAddress.OA_PostCode = "0123";

			invoice.JZ_OH_Manufacturer = manufacturer.PK;
			invoice.JZ_OA_ManufacturerAddress = manufacturerAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var manufacturerDataObject = invoiceData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Manufacturer));
			AssertEquals("manufacturerDataObject.Address1", "ADR", manufacturerDataObject.Address1);
			AssertEquals("manufacturerDataObject.AddressShortCode", "ADR", manufacturerDataObject.AddressShortCode.Value);
		}

		public void TestManufacturerAddressInInvoiceLineIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);

			Assert(invoice.JobComInvoiceLines.Any());
			var invoiceLine1 = invoice.JobComInvoiceLines[0];

			var manufacturer = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			manufacturer.OH_IsConsignor = ZBool.True;

			var manufacturerAddress = manufacturer.Addresses.AddNew(OrgAddressType.Office, false);
			manufacturerAddress.OA_Address1 = "ADR";
			manufacturerAddress.OA_City = "AE";
			manufacturerAddress.OA_PostCode = "0123";

			invoiceLine1.JI_OA_ManufacturerAddress = manufacturerAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];

			var manufacturerDataObject = commercialInvoiceLine1.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Manufacturer));
			AssertEquals("manufacturerDataObject.Address1", "ADR", manufacturerDataObject.Address1);
			AssertEquals("manufacturerDataObject.AddressShortCode", "ADR", manufacturerDataObject.AddressShortCode.Value);
		}

		public void TestConsigneeAddressInInvoiceLineIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);

			Assert(invoice.JobComInvoiceLines.Any());
			var invoiceLine1 = invoice.JobComInvoiceLines[0];

			var consignee = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			consignee.OH_IsConsignor = ZBool.True;

			var consigneeAddress = consignee.Addresses.AddNew(OrgAddressType.Office, false);
			consigneeAddress.OA_Address1 = "ADR";
			consigneeAddress.OA_City = "AE";
			consigneeAddress.OA_PostCode = "0123";

			invoiceLine1.JI_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];

			var consigneeDataObject = commercialInvoiceLine1.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsigneeAddress));
			AssertEquals("consigneeDataObject.Address1", "ADR", consigneeDataObject.Address1);
			AssertEquals("consigneeDataObject.AddressShortCode", "ADR", consigneeDataObject.AddressShortCode.Value);
		}

		public void TestSoldToPartyAddressInInvoiceHeaderIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var soldToParty = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			soldToParty.OH_IsConsignee = ZBool.True;

			var soldToPartyAddress = soldToParty.Addresses.AddNew(OrgAddressType.Office, false);
			soldToPartyAddress.OA_Address1 = "ADR";
			soldToPartyAddress.OA_City = "AE";
			soldToPartyAddress.OA_PostCode = "0123";

			invoice.JZ_OA_SoldToPartyAddress = soldToPartyAddress.PK;
			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var soldtopartyDataObject = invoiceData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.SoldToParty);
			AssertEquals("soldtopartyDataObject.Address1", "ADR", soldtopartyDataObject.Address1);
			AssertEquals("soldtopartyDataObject.AddressShortCode", "ADR", soldtopartyDataObject.AddressShortCode.Value);
		}

		public void TestBuyerAgentInvoiceHeaderIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var buyerAgent = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			buyerAgent.OH_IsConsignee = ZBool.True;

			var buyerAgentAddress = buyerAgent.MainAddress;
			buyerAgentAddress.OA_Address1 = "ADR";
			buyerAgentAddress.OA_City = "AE";
			buyerAgentAddress.OA_PostCode = "0123";
			invoice.JZ_OH_BuyerAgent = buyerAgent.PK;
			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var buyerAgentDataObject = invoiceData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.BuyingAgent);
			AssertEquals("buyerAgentDataObject.Address1", "ADR", buyerAgentDataObject.Address1);
			AssertEquals("buyerAgentDataObject.AddressShortCode", "ADR", buyerAgentDataObject.AddressShortCode.Value);
		}

		public void TestSellingAgentInvoiceHeaderIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var sellingAgent = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			sellingAgent.OH_IsConsignor = ZBool.True;

			var sellingAgentAddress = sellingAgent.MainAddress;
			sellingAgentAddress.OA_Address1 = "ADR";
			sellingAgentAddress.OA_City = "AE";
			sellingAgentAddress.OA_PostCode = "0123";
			invoice.JZ_OH_SellingAgent = sellingAgent.PK;
			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var sellingAgentDataObject = invoiceData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.SellingAgent);
			AssertEquals("sellingAgentDataObject.Address1", "ADR", sellingAgentDataObject.Address1);
			AssertEquals("sellingAgentDataObject.AddressShortCode", "ADR", sellingAgentDataObject.AddressShortCode.Value);
		}

		public void TestConsigneeInInvoiceHeaderIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var consignee = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;

			var consigneeAddress = consignee.MainAddress;
			consigneeAddress.OA_Address1 = "ADR";
			consigneeAddress.OA_City = "AE";
			consigneeAddress.OA_PostCode = "0123";
			invoice.JZ_OH_Consignee = consignee.PK;
			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var consigneeDataObject = invoiceData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.IntermediateConsignee);
			AssertEquals("ConsigneeDataObject.Address1", "ADR", consigneeDataObject.Address1);
			AssertEquals("ConsigneeDataObject.AddressShortCode", "ADR", consigneeDataObject.AddressShortCode.Value);
		}

		public void TestSoldToPartyAddressInInvoiceLineIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);

			Assert(invoice.JobComInvoiceLines.Any());
			var invoiceLine1 = invoice.JobComInvoiceLines[0];

			var soldtoparty = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			soldtoparty.OH_IsConsignee = ZBool.True;

			var soldtopartyAddress = soldtoparty.Addresses.AddNew(OrgAddressType.Office, false);
			soldtopartyAddress.OA_Address1 = "ADR";
			soldtopartyAddress.OA_City = "AE";
			soldtopartyAddress.OA_PostCode = "0123";

			invoiceLine1.JI_OA_SoldToPartyAddress = soldtopartyAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];

			var soldtopartyDataObject = commercialInvoiceLine1.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.SoldToParty);
			AssertEquals("soldtopartyDataObject.Address1", "ADR", soldtopartyDataObject.Address1);
			AssertEquals("soldtopartyDataObject.AddressShortCode", "ADR", soldtopartyDataObject.AddressShortCode.Value);
		}

		public void TestExporterAddressInInvoiceHeaderIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var exporter = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			exporter.OH_IsConsignee = ZBool.True;

			var exporterAddress = exporter.Addresses.AddNew(OrgAddressType.Office, false);
			exporterAddress.OA_Address1 = "ADR";
			exporterAddress.OA_City = "AE";
			exporterAddress.OA_PostCode = "0123";

			invoice.JZ_OA_ExporterAddress = exporterAddress.PK;
			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var exporterDataObject = invoiceData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Exporter));
			AssertEquals("exporterDataObject.Address1", "ADR", exporterDataObject.Address1);
			AssertEquals("exporterDataObject.AddressShortCode", "ADR", exporterDataObject.AddressShortCode.Value);
		}

		public void TestExportCustomsFourthQuantity()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsFourthQuantity = 10.25m;
			invoiceLine.JI_CustomsFourthUnitQty = "MTQ";

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			var invoiceData = writer.GetDataObject(invoice);
			var commercialInvoiceLine = invoiceData.CommercialInvoiceLineCollection.Single();
			CombineAssertions(() =>
			{
				AssertEquals("CustomsFourthQuantity", 10.25m, commercialInvoiceLine.CustomsFourthQuantity);
				AssertEquals("CustomsFourthQuantityUnit.Code", "MTQ", commercialInvoiceLine.CustomsFourthQuantityUnit.Code);
			});
		}

		public void TestExportCustomsFifthQuantity()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsFifthQuantity = 15.55m;
			invoiceLine.JI_CustomsFifthUnitQty = "KGM";

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			var invoiceData = writer.GetDataObject(invoice);
			var commercialInvoiceLine = invoiceData.CommercialInvoiceLineCollection.Single();
			CombineAssertions(() =>
			{
				AssertEquals("CustomsFifthQuantity", 15.55m, commercialInvoiceLine.CustomsFifthQuantity);
				AssertEquals("CustomsFifthQuantityUnit.Code", "KGM", commercialInvoiceLine.CustomsFifthQuantityUnit.Code);
			});
		}

		public void TestExporterAddressInInvoiceLineIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);

			Assert(invoice.JobComInvoiceLines.Any());
			var invoiceLine1 = invoice.JobComInvoiceLines[0];

			var exporter = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			exporter.OH_IsConsignee = ZBool.True;

			var exporterAddress = exporter.Addresses.AddNew(OrgAddressType.Office, false);
			exporterAddress.OA_Address1 = "ADR";
			exporterAddress.OA_City = "AE";
			exporterAddress.OA_PostCode = "0123";

			invoiceLine1.JI_OA_ExporterAddress = exporterAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];

			var exporterDataObject = commercialInvoiceLine1.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Exporter));
			AssertEquals("exporterDataObject.Address1", "ADR", exporterDataObject.Address1);
			AssertEquals("exporterDataObject.AddressShortCode", "ADR", exporterDataObject.AddressShortCode.Value);
		}

		public void TestConsigneeAddressInInvoiceHeaderIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var consignee = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;

			var consigneeAddress = consignee.Addresses.AddNew(OrgAddressType.Office, false);
			consigneeAddress.OA_Address1 = "ADR";
			consigneeAddress.OA_City = "AE";
			consigneeAddress.OA_PostCode = "0123";

			invoice.JZ_OA_ConsigneeAddress = consigneeAddress.PK;
			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var consigneeAddressObject = invoiceData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.UltimateConsignee);
			AssertEquals("consigneeAddressObject.Address1", "ADR", consigneeAddressObject.Address1);
			AssertEquals("consigneeAddressObject.AddressShortCode", "ADR", consigneeAddressObject.AddressShortCode.Value);
		}

		public void TestUltimateConsigneeAddressInInvoiceLineIsExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);

			Assert(invoice.JobComInvoiceLines.Any());
			var invoiceLine1 = invoice.JobComInvoiceLines[0];

			var consignee = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;

			var consigneeAddress = consignee.Addresses.AddNew(OrgAddressType.Office, false);
			consigneeAddress.OA_Address1 = "ADR";
			consigneeAddress.OA_City = "AE";
			consigneeAddress.OA_PostCode = "0123";

			invoiceLine1.JI_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);

			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];

			var consigneeAddressObject = commercialInvoiceLine1.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.UltimateConsignee);
			AssertEquals("consigneeAddressObject.Address1", "ADR", consigneeAddressObject.Address1);
			AssertEquals("consigneeAddressObject.AddressShortCode", "ADR", consigneeAddressObject.AddressShortCode.Value);
		}

		public void TestCommercialInvoiceLineRelatedDataMappings()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OH_Importer = org2.PK;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			var groupHeader = invoice.GroupHeader;
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var invoiceLine1 = invoice.JobComInvoiceLines[0];

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_TariffNum = "1020102010";
			classification.CC_LookupCode = "LOOKUP1";
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = "EXP";
			entryHeader.EntryNumber = "EN3234";
			entryHeader.CH_EntryStatus = "010";
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2023, 10, 24);
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var order = Factory.New<Freight.Forwarding.Orders.Business.Order>();
			order.JD_OrderNumber = "ORD1";
			order.BuyerPK = declaration.JE_OH_Importer;
			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "0001";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_PSN = "I am very dangerous";
			subs.DG_FlashPoint = "-4 cc";
			subs.DG_Code = "0001A";

			SetupJobComInvoiceLineRelatedData(invoiceLine1, entryLine.PK, classification.PK, orderLine2.PK, "0001A");
			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);
			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];
			var commercialInvoiceLine2 = commercialInvoiceLineCollection[1];

			CombineAssertions(delegate
			{
				AssertEquals("commercialInvoiceLine1.ClassificationCode", "LOOKUP1", commercialInvoiceLine1.ClassificationCode);
				AssertEquals("commercialInvoiceLine1.EntryNumber", "EN3234", commercialInvoiceLine1.EntryNumber);
				AssertEquals("commercialInvoiceLine1.EntryLineNumber", (short)1, commercialInvoiceLine1.EntryLineNumber);
				AssertEquals("commercialInvoiceLine1.OrderLineLink", 2, commercialInvoiceLine1.OrderLineLink);
				AssertEquals("commercialInvoiceLine1.EntryStatus", "010", commercialInvoiceLine1.EntryStatus);
				AssertEquals("commercialInvoiceLine1.EntryReleaseDate", new ZDateTime(2023, 10, 24), commercialInvoiceLine1.EntryReleaseDate);

				AssertNull("commercialInvoiceLine2.EntryNumber", commercialInvoiceLine2.EntryNumber);
				AssertNull("commercialInvoiceLine2.EntryLineNumber", commercialInvoiceLine2.EntryLineNumber);

				var hazardousMaterialData = commercialInvoiceLine1.HazardousMaterial;
				AssertEquals("hazardousMaterialData.Code", "HZ234", hazardousMaterialData.Code);
				AssertNotNull("hazardousMaterialData.CodeType", hazardousMaterialData.CodeType);
				AssertEquals("hazardousMaterialData.CodeType.Code", "U", hazardousMaterialData.CodeType.Code);
				AssertEquals("hazardousMaterialData.CodeType.Description", "UN Code", hazardousMaterialData.CodeType.Description);

				AssertEquals("hazardousMaterialData.UNDGCollection.Count", 1, hazardousMaterialData.UNDGCollection.Count);

				var undgData = hazardousMaterialData.UNDGCollection[0];
				AssertEquals("undgData.UNDGCode", "0001A", undgData.UNDGCode);
				AssertEquals("undgData.TechicalName", "TECHNAME", undgData.TechicalName);
				AssertNotNull("commercialInvoiceLine1.CustomizedFieldCollection", commercialInvoiceLine1.CustomizedFieldCollection);
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "PART1", "PATT1");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "PART2", "PATT2");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "PART3", "PATT3");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "Serial Number", "SerialNum");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING1", "CATT1");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING2", "CATT2");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING3", "CATT3");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING4", "CATT4");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING5", "CATT5");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE1", ZDateTime.BrettsBirthday.AddDays(1).ToISO8601String());
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE2", ZDateTime.BrettsBirthday.AddDays(2).ToISO8601String());
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE3", ZDateTime.BrettsBirthday.AddDays(3).ToISO8601String());
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL1", "101.1");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL2", "102.2");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL3", "103.3");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "FLAG1", "true");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "FLAG2", "false");
				commercialInvoiceLine1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "FLAG3", "true");
			});
		}

		public void TestEntryIntructionLinkOnInvoiceLine()
		{
			var testOrg = Factory.NewWithValidTestData<OrgAddress>();
			testOrg.Address1 = "TESTADDR1";
			var testInstruction1 = Factory.NewWithValidTestData<CusEntryInstruction>();
			testInstruction1.CEI_Style = "11";
			testInstruction1.CEI_Description = "Desc1";
			testInstruction1.CEI_MergeBy = "TRF";
			testInstruction1.CEI_OA_Warehouse = testOrg.PK;

			var testInstruction2 = Factory.NewWithValidTestData<CusEntryInstruction>();
			testInstruction2.CEI_Style = "12";
			testInstruction2.CEI_Description = "Desc2";

			var testHeader = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			var testLine = testHeader.InvoiceLines.AddNew();
			testLine.JI_RN_NKCountryOfExport = "CN";
			testLine.JI_CountryOfOrigin = "IT";
			testLine.JI_StateOrRegionOfOrigin = "PA";
			testLine.JI_CustomsQuantity = 11;
			testLine.JI_CustomsSecondQuantity = 12;
			testLine.JI_CustomsThirdQuantity = 13;
			testLine.JI_Procedure = "1200";
			testLine.JI_CEI = testInstruction1.PK;

			var helper = new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.SouthAfrica);
			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, testInstruction1)), helper);
			CombineAssertions(() =>
			{
				var resultHeader = writer.GetDataObject(testHeader);
				AssertEquals("InvoiceLineCount", 1, resultHeader.CommercialInvoiceLineCollection.Count);
				var resultLine = resultHeader.CommercialInvoiceLineCollection.FirstOrDefault();
				AssertEquals("CN", resultLine.CountryOfExport.Code);
				AssertEquals("China", resultLine.CountryOfExport.Name);
				AssertEquals("IT", resultLine.CountryOfOrigin.Code);
				AssertEquals("Italy", resultLine.CountryOfOrigin.Name);
				AssertEquals("PA", resultLine.StateOfOrigin.Code);
				AssertEquals("Palermo", resultLine.StateOfOrigin.Name);
				AssertEquals("SICILIA", resultLine.StateOfOrigin.Region);
				AssertEquals("1200", resultLine.Procedure);
				AssertEquals(null, resultLine.EntryInstructionLink);

				helper.AllocateEntryInstructionLink(testInstruction1.PK);
				helper.AllocateEntryInstructionLink(testInstruction2.PK);

				testLine.JI_CEI = testInstruction2.PK;
				resultHeader = writer.GetDataObject(testHeader);
				resultLine = resultHeader.CommercialInvoiceLineCollection.FirstOrDefault();
				AssertEquals(2, resultLine.EntryInstructionLink);

				testLine.JI_CEI = testInstruction1.PK;
				resultHeader = writer.GetDataObject(testHeader);
				resultLine = resultHeader.CommercialInvoiceLineCollection.FirstOrDefault();
				AssertEquals(1, resultLine.EntryInstructionLink);
			});
		}

		public void TestInvoiceLinePreviousEntryNumberMapping()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-10).ToDateTime());
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_PreviousEntryNumber = "123456789";
				invoiceLine.JI_PreviousEntryLineNumber = 2;

				Factory.SaveForTesting();

				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var invoiceData = writer.GetDataObject(invoice);
				var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
				AssertNotNull(commercialInvoiceLineCollection);
				var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];

				CombineAssertions(delegate
				{
					AssertEquals("commercialInvoiceLine1.PreviousEntryNumber", "123456789", commercialInvoiceLine1.PreviousEntryNumber);
					AssertEquals("commercialInvoiceLine1.PreviousEntryLineNumber", (ZShort)2, commercialInvoiceLine1.PreviousEntryLineNumber);
				});
			}
		}

		public void TestDocAddressesOnInvoiceHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var docAddresses = (IDocAddresses)invoice;
				var consigneeAddress = AddDocAddress(docAddresses, DocAddressType.ConsigneeAddress, "BOB");
				var finalConsigneeAddress1 = AddDocAddress(docAddresses, DocAddressType.FinalConsigneeAddress, "JOE");
				var finalConsigneeAddress2 = AddDocAddress(docAddresses, DocAddressType.FinalConsigneeAddress, "WENDY");
				finalConsigneeAddress2.E2_AddressSequence = 1;
				finalConsigneeAddress1.E2_AddressSequence = 2;
				var exporter = AddDocAddress(docAddresses, DocAddressType.Exporter, "JACK");
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var organizationAddressCollection = writer.GetDataObject(invoice).OrganizationAddressCollection.GroupBy(x => x.AddressType.GetValueOrDefault()).ToDictionary((x) => x.Key);
				AssertEquals("No ConsigneeAddress", false, organizationAddressCollection.ContainsKey(nameof(DocAddressType.ConsigneeAddress)));
				var finalConsigneeAddresses = organizationAddressCollection[nameof(DocAddressType.FinalConsigneeAddress)].ToArray();
				AssertEquals("FinalConsigneeAddress", 2, finalConsigneeAddresses.Length);
				AssertOrganizationAddress(finalConsigneeAddresses[0], DocAddressType.FinalConsigneeAddress, "WENDY");
				AssertOrganizationAddress(finalConsigneeAddresses[1], DocAddressType.FinalConsigneeAddress, "JOE");
				var exporters = organizationAddressCollection[nameof(DocAddressType.Exporter)].ToArray();
				AssertEquals("Exporter", 1, exporters.Length);
				AssertOrganizationAddress(exporters[0], DocAddressType.Exporter, "JACK");
			}
		}

		public void TestAdditionalTermsOnInvoiceHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_IncoTermPlace = "BOB'S PLACE";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice);
				AssertEquals("AdditionalTerms", "BOB'S PLACE", result.AdditionalTerms);
			}
		}

		public void TestDeliveryTermsOnInvoiceHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_AdditionalTerms = "Delivery Terms Invoice";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice);
				AssertEquals("AdditionalTerms", "Delivery Terms Invoice", result.DeliveryTerms);
			}
		}

		public void TestRelatedIndicatorOnInvoiceHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RelatedIndicator = "E";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice);
				AssertEquals(MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Exempt, result.RelatedIndicator.Code.Value);
				AssertEquals(MasterFiles.Business.Customs.RelatedIndicatorList.Descriptions.Exempt, result.RelatedIndicator.Description.Value);
			}
		}

		public void TestValuationCodeOnInvoiceHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RelatedIndicator = "Y";
				invoice.JZ_ValuationCode = "1";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice);
				AssertEquals(MasterFiles.Business.Customs.ZA.ValuationCodeList.Codes.Section1, result.ValuationCode.Code.Value);
				AssertEquals(MasterFiles.Business.Customs.ZA.ValuationCodeList.Descriptions.Section1, result.ValuationCode.Description.Value);
			}
		}

		public void TestDocAddressesOnInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var docAddresses = (IDocAddresses)invoiceLine;
				var consigneeAddress = AddDocAddress(docAddresses, DocAddressType.ConsigneeAddress, "BOB");
				var customsSupervisingOffice1 = AddDocAddress(docAddresses, DocAddressType.CustomsSupervisingOffice, "JOE");
				var customsSupervisingOffice2 = AddDocAddress(docAddresses, DocAddressType.CustomsSupervisingOffice, "WENDY");
				customsSupervisingOffice2.E2_AddressSequence = 1;
				customsSupervisingOffice1.E2_AddressSequence = 2;
				var exporter = AddDocAddress(docAddresses, DocAddressType.Exporter, "JACK");
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var organizationAddressCollection = writer.GetDataObject(invoice).CommercialInvoiceLineCollection[0].OrganizationAddressCollection.GroupBy(x => x.AddressType.GetValueOrDefault()).ToDictionary((x) => x.Key);
				AssertEquals("No ConsigneeAddress", false, organizationAddressCollection.ContainsKey(nameof(DocAddressType.ConsigneeAddress)));
				AssertEquals("No Exporter", false, organizationAddressCollection.ContainsKey(nameof(DocAddressType.Exporter)));
				var customsSupervisingOffices = organizationAddressCollection[nameof(DocAddressType.CustomsSupervisingOffice)].ToArray();
				AssertEquals("CustomsSupervisingOffice", 2, customsSupervisingOffices.Length);
				AssertOrganizationAddress(customsSupervisingOffices[0], DocAddressType.CustomsSupervisingOffice, "WENDY");
				AssertOrganizationAddress(customsSupervisingOffices[1], DocAddressType.CustomsSupervisingOffice, "JOE");
			}
		}

		public void TestConcessionOrderOnInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_ConcessionOrder = "CON324";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice);
				AssertEquals("result.CommercialInvoiceLineCollection.Count", 1, result.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = result.CommercialInvoiceLineCollection[0];
				AssertEquals("invoiceLineData.ConcessionOrder", "CON324", invoiceLineData.ConcessionOrder);
			}
		}

		public void TestRelatedIndicatorOnInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_RelatedIndicator = "E";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice);
				AssertEquals("result.CommercialInvoiceLineCollection.Count", 1, result.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = result.CommercialInvoiceLineCollection[0];
				AssertEquals(MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Exempt, invoiceLineData.RelatedIndicator.Code.Value);
				AssertEquals(MasterFiles.Business.Customs.RelatedIndicatorList.Descriptions.Exempt, invoiceLineData.RelatedIndicator.Description.Value);
			}
		}

		public void TestValuationCodeOnInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_RelatedIndicator = "Y";
				invoiceLine.JI_ValuationCode = "1";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice);
				AssertEquals("result.CommercialInvoiceLineCollection.Count", 1, result.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = result.CommercialInvoiceLineCollection[0];
				AssertEquals("invoiceLineData.ValuationCode.Code", "1", invoiceLineData.ValuationCode.Code.Value);
				AssertEquals("invoiceLineData.ValuationCode.Description", "Transaction value of the imported goods", invoiceLineData.ValuationCode.Description.Value);
			}
		}

		public void TestValuetionMarkupOnInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_ValuationMarkup = 20.5m;
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice).CommercialInvoiceLineCollection.FirstOrDefault();
				AssertEquals(20.5m, result.ValuationMarkup);
			}
		}

		public void TestTaxTypeOnInvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateTaxOrFee("TXX", 20m, Core.Constants.CountryCodes.SouthAfrica);
			Factory.SaveForTesting();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_ZZF_NKTaxType = "TXX";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice).CommercialInvoiceLineCollection.FirstOrDefault();
				AssertEquals("TXX", result.TaxType.Code.Value);
				AssertEquals("TXX DESC", result.TaxType.Description.Value);
			}
		}

		public void TestBrandModelOnInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_BrandName = "BrandName";
				invoiceLine.JI_Model = "Model";
				invoiceLine.JI_NDescription = "LocalDescription";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice);
				AssertEquals("result.CommercialInvoiceLineCollection.Count", 1, result.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = result.CommercialInvoiceLineCollection[0];
				AssertEquals("invoiceLineData.BrandName", "BrandName", invoiceLineData.BrandName);
				AssertEquals("invoiceLineData.Model", "Model", invoiceLineData.Model);
				AssertEquals("invoiceLineData.LocalDescription", "LocalDescription", invoiceLineData.LocalDescription);
			}
		}

		public void TestCustomAttributesOnInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CustomAttrib1 = "test custom attrib 1";
				invoiceLine.JI_CustomAttrib2 = "test custom attrib 2";
				invoiceLine.JI_CustomAttrib3 = "test custom attrib 3";
				invoiceLine.JI_CustomAttrib4 = "test custom attrib 4";
				invoiceLine.JI_CustomAttrib5 = "test custom attrib 5";
				invoiceLine.JI_CustomAttrib6 = "test custom attrib 6";
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice);
				var invoiceLineData = result.CommercialInvoiceLineCollection[0];
				CombineAssertions(() =>
				{
					var attributeCollection = invoiceLineData.CustomAttributeCollection;
					AssertEquals("result.CommercialInvoiceLineCollection.Count", 1, result.CommercialInvoiceLineCollection.Count);
					AssertEquals(6, invoiceLineData.CustomAttributeCollection.Count);
					Assert("CustomFirstAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute1 && x.Value.GetValueOrDefault() == "test custom attrib 1"));
					Assert("CustomSecondAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute2 && x.Value.GetValueOrDefault() == "test custom attrib 2"));
					Assert("CustomThirdAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute3 && x.Value.GetValueOrDefault() == "test custom attrib 3"));
					Assert("CustomFourthAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute4 && x.Value.GetValueOrDefault() == "test custom attrib 4"));
					Assert("CustomFifthAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute5 && x.Value.GetValueOrDefault() == "test custom attrib 5"));
					Assert("CustomSixthAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute6 && x.Value.GetValueOrDefault() == "test custom attrib 6"));
				});

				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CustomAttrib1 = "test custom attrib 1";
				invoiceLine.JI_CustomAttrib3 = "test custom attrib 3";
				invoiceLine.JI_CustomAttrib5 = "test custom attrib 5";
				invoiceLine.JI_CustomAttrib6 = "test custom attrib 6";
				writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				result = writer.GetDataObject(invoice);
				invoiceLineData = result.CommercialInvoiceLineCollection[0];
				CombineAssertions(() =>
				{
					var attributeCollection = invoiceLineData.CustomAttributeCollection;
					AssertEquals("result.CommercialInvoiceLineCollection.Count", 1, result.CommercialInvoiceLineCollection.Count);
					AssertEquals(4, invoiceLineData.CustomAttributeCollection.Count);
					Assert("CustomFirstAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute1 && x.Value.GetValueOrDefault() == "test custom attrib 1"));
					Assert("CustomThirdAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute3 && x.Value.GetValueOrDefault() == "test custom attrib 3"));
					Assert("CustomFifthAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute5 && x.Value.GetValueOrDefault() == "test custom attrib 5"));
					Assert("CustomSixthAttribute", attributeCollection.Any(x => x.Key.GetValueOrDefault() == Constants.CustomAttributeKeys.CustomAttribute6 && x.Value.GetValueOrDefault() == "test custom attrib 6"));
				});
			}
		}

		public void TestAdditionalTariffBill()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "22A");
			Factory.SaveForTesting();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew() as Business.IAdditionalLineTariffDetailParent;
				var lineTariff1 = invoiceLine.CusLineTariffDetails.AddNew();
				lineTariff1.BZ_Type = "12A";
				lineTariff1.BZ_Tariff = "12ATariff";
				lineTariff1.BZ_Value = 11m;
				var lineTariff2 = invoiceLine.CusLineTariffDetails.AddNew();
				lineTariff2.BZ_Type = "22A";
				lineTariff2.BZ_Tariff = "22ATariff";
				lineTariff2.BZ_Value = 22m;
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
				var result = writer.GetDataObject(invoice).CommercialInvoiceLineCollection.FirstOrDefault().AdditionalLineTariffDetailCollection;
				{
					AssertEquals(2, result.Count);
					var first = result.FirstOrDefault(x => x.Type.Code.Value == "12A");
					AssertNull(first.Type.Description);
					AssertEquals("12ATariff", first.Tariff);
					AssertEquals(11m, first.Value);

					var second = result.FirstOrDefault(x => x.Type.Code.Value == "22A");
					AssertEquals("22A DESC", second.Type.Description.Value);
					AssertEquals("22ATariff", second.Tariff);
					AssertEquals(22m, second.Value);
				}
			}
		}

		public void TestBuyerAddressAndSupplierAddressAndIntermCneAddressAreExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var invoiceMock = CreateInvoiceMock(declaration);
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();

			var testOrg = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			testOrg.OH_Code = "INCTESORG";
			testOrg.OH_IsConsignor = ZBool.True;
			var buyerAddress = testOrg.Addresses.AddNew();
			buyerAddress.OA_Address1 = "TEST BUYER ADDRESS";
			var supplierAddress = testOrg.Addresses.AddNew();
			supplierAddress.OA_Address1 = "TEST SUPPLIER ADDRESS";
			var intermCneAddress = testOrg.Addresses.AddNew();
			intermCneAddress.OA_Address1 = "TEST INTERMCNE ADDRESS";

			invoice.JZ_OA_BuyerAddress = buyerAddress.PK;
			invoice.JZ_OA_SupplierAddress = supplierAddress.PK;
			invoice.JZ_OA_IntermediateConsigneeAddress = intermCneAddress.PK;
			Factory.SaveForTesting();

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), CurrentCompanyHelper);
			var invoiceData = writer.GetDataObject(invoice);
			var buyerAddressObj = invoiceData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.BuyerAddress);
			AssertNotNull("Buyer Address should be exported", buyerAddressObj);
			AssertEquals("Buyer Address should matches", buyerAddress.OA_Address1, buyerAddressObj.Address1);
			var supplierAddressObj = invoiceData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.SupplierAddress);
			AssertNotNull("Supplier Address should be exported", supplierAddressObj);
			AssertEquals("Supplier Address should matches", supplierAddress.OA_Address1, supplierAddressObj.Address1);
			var intermCneAddressObj = invoiceData.OrganizationAddressCollection.FirstOrDefault(Constants.AddressTypes.IntermediateConsignee);
			AssertNotNull("Interm Address should be exported", intermCneAddressObj);
			AssertEquals("Interm Address should matches", intermCneAddress.OA_Address1, intermCneAddressObj.Address1);
		}

		public void TestPopulateCommercialInvoiceLineFields()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoiceHeader = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
				invoiceHeader.JZ_JE = declaration.PK;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoiceHeader)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.Australia));
				var output = writer.GetDataObject(invoiceHeader);
				AssertEquals(1, output.CommercialInvoiceLineCollection.Count);
				var commercialInvoiceLine = output.CommercialInvoiceLineCollection[0];
				AssertNull(commercialInvoiceLine.DetailedDescription);

				invoiceLine.JI_Description = "AAA";
				var writer1 = new CommercialInvoiceHeaderDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoiceHeader)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.Australia));
				output = writer1.GetDataObject(invoiceHeader);
				AssertEquals(1, output.CommercialInvoiceLineCollection.Count);
				commercialInvoiceLine = output.CommercialInvoiceLineCollection[0];
				AssertEquals(new ZString?("AAA"), commercialInvoiceLine.DetailedDescription);
			}
		}

		public void TestExportJobComInvLineComponentInventory()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var component1 = invoiceLine.ComponentInventoryCollection.AddNew();
			component1.JIV_AllocationKey = "Test";
			component1.JIV_QuantityToDraw = 4;
			var component2 = invoiceLine.ComponentInventoryCollection.AddNew();
			component2.JIV_AllocationKey = "Test 2";
			component2.JIV_QuantityToDraw = 6.5;

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			var invoiceData = writer.GetDataObject(invoice);
			var commercialInvoiceLine = invoiceData.CommercialInvoiceLineCollection.Single();

			AssertEquals(2, commercialInvoiceLine.AddInfoGroupCollection.Count);
			var addInfoGroup1 = commercialInvoiceLine.AddInfoGroupCollection[0];
			var addInfoGroup2 = commercialInvoiceLine.AddInfoGroupCollection[1];
			AssertEquals("AddInfoGroup1.Type.Code", "ALI", addInfoGroup1.Type.Code);
			AssertEquals("AddInfoGroup1.Type.Description", "Allocation Info", addInfoGroup1.Type.Description);
			AssertEquals("AddInfoGroup2.Type.Code", "ALI", addInfoGroup1.Type.Code);
			AssertEquals("AddInfoGroup2.Type.Description", "Allocation Info", addInfoGroup1.Type.Description);

			AssertEquals(2, addInfoGroup1.AddInfoCollection.Count);
			var addInfo1 = addInfoGroup1.AddInfoCollection[0];
			var addInfo2 = addInfoGroup1.AddInfoCollection[1];
			AssertEquals(2, addInfoGroup2.AddInfoCollection.Count);
			var addInfo3 = addInfoGroup2.AddInfoCollection[0];
			var addInfo4 = addInfoGroup2.AddInfoCollection[1];
			CombineAssertions(() =>
			{
				AssertEquals("AllocationKey Key", "AllocationKey", addInfo1.Key);
				AssertEquals("AllocationKey Value", "Test", addInfo1.Value);
				AssertEquals("QuantityToDraw Key", "Quantity", addInfo2.Key);
				AssertEquals("QuantityToDraw Value", "4", addInfo2.Value);
				AssertEquals("AllocationKey Key", "AllocationKey", addInfo3.Key);
				AssertEquals("AllocationKey Value", "Test 2", addInfo3.Value);
				AssertEquals("QuantityToDraw Key", "Quantity", addInfo4.Key);
				AssertEquals("QuantityToDraw Value", "6.5", addInfo4.Value);
			});
		}

		void AssertOrganizationAddress(OrganizationAddress organizationAddress, DocAddressType addressType, ZString companyName)
		{
			AssertEquals("AddressType", addressType.ToString(), organizationAddress.AddressType);
			AssertEquals("CompanyName", companyName, organizationAddress.CompanyName);
			AssertEquals("Address1", companyName + "ADD 1", organizationAddress.Address1);
		}

		JobDocAddress AddDocAddress(IDocAddresses bizObj, DocAddressType addressType, ZString companyName)
		{
			var docAddress = bizObj.DocAddresses.AddNew(addressType);
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = companyName;
			docAddress.E2_Address1 = companyName + "ADD 1";
			return docAddress;
		}

		Mock<BaseJobComInvoiceHeader> CreateInvoiceMock()
		{
			var invoiceMock = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var invoice = invoiceMock.Object;
			var lookupsMock = new Mock<JobComInvoiceHeaderLookups>(invoice);
			lookupsMock.CallBase = true;
			invoiceMock.Protected().Setup<JobComInvoiceHeaderLookups>("GetNewLookups").Returns(lookupsMock.Object);
			lookupsMock.Setup(m => m.MessageStatusList).Returns(new CustomsEntryStatusList());
			return invoiceMock;
		}

		Mock<BaseJobComInvoiceHeader> CreateInvoiceMock(BaseJobDeclaration declaration)
		{
			var invoiceMock = CreateInvoiceMock();
			var invoice = invoiceMock.Object;
			invoice.JZ_JE = declaration.PK;
			declaration.Invoices.Add(invoice);
			return invoiceMock;
		}

		BaseJobComInvoiceHeader SetupJobComInvoiceHeaderOnly(BaseJobComInvoiceHeader invoice, ZString invoiceNumber, ZGuid buyerPK, ZGuid supplierPK, ZDecimal invoiceAmount, ZString invoiceCurrency, ZDateTime invoiceDate, ZString incoTerm, ZDecimal volume, ZString volumeUQ, ZDecimal weight, ZString weightUQ, ZDecimal netWeight, ZString netWeightUQ, ZString invoiceCurrExRateType, ZDecimal invoiceCurrExRate, ZDecimal landedCostExchangeRate, ZString paymentNumber, ZDecimal paymentAmount, ZDecimal paymentExchangeRate, ZDateTime paymentDate, ZString messageStatus, ZDecimal noOfPacks)
		{
			invoice.JZ_InvoiceNumber = invoiceNumber;
			invoice.JZ_OH_Buyer = buyerPK;
			invoice.JZ_OH_Supplier = supplierPK;
			invoice.JZ_InvoiceAmount = invoiceAmount;
			invoice.JZ_RX_NKInvoice_Currency = invoiceCurrency;
			invoice.JZ_InvoiceDate = invoiceDate;
			invoice.JZ_ValuationDateOverride = invoiceDate.AddDays(2);
			invoice.JZ_IncoTerm = incoTerm;
			invoice.JZ_Volume = volume;
			invoice.JZ_VolumeUQ = volumeUQ;
			invoice.JZ_Weight = weight;
			invoice.JZ_WeightUQ = weightUQ;
			invoice.JZ_NetWeight = netWeight;
			invoice.JZ_NetWeightUQ = netWeightUQ;
			invoice.JZ_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			invoice.JZ_InvoiceCurrExRateType = invoiceCurrExRateType;
			invoice.JZ_InvoiceCurrExRate = invoiceCurrExRate;
			invoice.JZ_InvoiceCurrLandedCostExRate = landedCostExchangeRate;
			invoice.JZ_PaymentNo = paymentNumber;
			invoice.JZ_PaymentAmount = paymentAmount;
			invoice.JZ_PaymentExRate = paymentExchangeRate;
			invoice.JZ_PaymentDate = paymentDate;
			invoice.JZ_MessageStatus = messageStatus;
			invoice.JZ_NoOfPacks = noOfPacks;
			return invoice;
		}

		BaseJobComInvoiceHeader SetupJobComInvoiceHeader(BaseJobComInvoiceHeader invoice, ZString invoiceNumber, ZGuid buyerPK, ZGuid supplierPK, ZDecimal invoiceAmount, ZString invoiceCurrency, ZDateTime invoiceDate, ZString incoTerm, ZDecimal volume, ZString volumeUQ, ZDecimal weight, ZString weightUQ, ZDecimal netWeight, ZString netWeightUQ, ZString invoiceCurrExRateType, ZDecimal invoiceCurrExRate, ZDecimal landedCostExchangeRate, ZString paymentNumber, ZDecimal paymentAmount, ZDecimal paymentExchangeRate, ZDateTime paymentDate, ZString messageStatus, ZDecimal noOfPacks)
		{
			invoice = SetupJobComInvoiceHeaderOnly(invoice, invoiceNumber, buyerPK, supplierPK, invoiceAmount, invoiceCurrency, invoiceDate, incoTerm, volume, volumeUQ, weight, weightUQ, netWeight, netWeightUQ, invoiceCurrExRateType, invoiceCurrExRate, landedCostExchangeRate, paymentNumber, paymentAmount, paymentExchangeRate, paymentDate, messageStatus, noOfPacks);
			var invoiceLine1Mock = CreateInvoiceLineMock(invoice, false);
			SetupJobComInvoiceLine(invoiceLine1Mock.Object);

			var invoiceLine2Mock = CreateInvoiceLineMock(invoice, true);
			SetupJobComInvoiceLine2(invoiceLine2Mock.Object);
			return invoice;
		}

		BaseJobComInvoiceHeader SetupJobComInvoiceHeader(BaseJobComInvoiceHeader invoice)
		{
			return SetupJobComInvoiceHeader(invoice, "INV3243", GetOrganizationBO_WUFSHIJNB(invoice.Factory).PK, GetOrganizationBO_CRAHOLSYD(invoice.Factory).PK, 3420.34m, Core.Constants.CurrencyCodes.Australia, new ZDateTime(2011, 4, 3), Core.Constants.IncoTerms.FreeOnBoard, 14.72m, Core.Constants.Volume.CubicMetres, 2.53m, Core.Constants.Weight.Tonnes, 11.11m, Core.Constants.Weight.Kilograms, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.25m, 1.50m, "P12345", 1500m, 1.75m, new ZDateTime(2011, 3, 3), Enterprise.Customs.Common.US.CustomsEntryStatusList.Codes.ClearElectronicInvoiceOriginal, 10m);
		}

		BaseJobComInvoiceHeader SetupJobComInvoiceHeader2(BaseJobComInvoiceHeader invoice)
		{
			return SetupJobComInvoiceHeader(invoice, "INV6854", GetOrganizationBO_CRAHOLSYD(invoice.Factory).PK, GetOrganizationBO_WUFSHIJNB(invoice.Factory).PK, 8685.54m, Core.Constants.CurrencyCodes.NewZealand, new ZDateTime(2011, 4, 2), Core.Constants.IncoTerms.CostInsuranceAndFreight, 96.87m, Core.Constants.Volume.CubicYards, 86.69m, Core.Constants.Weight.Kilotonnes, 22.22m, Core.Constants.Weight.Kilograms, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, ZString.Empty, ZDecimal.Zero);
		}

		Mock<BaseJobComInvoiceLine> CreateInvoiceLineMock(BaseJobComInvoiceHeader invoice, bool dontEmitEntryDetailsInUniversalXML)
		{
			var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
			var invoiceLine = invoiceLineMock.Object;
			var helper = new Mock<JobComInvoiceLineLookups>(invoiceLine);
			helper.CallBase = true;
			helper.Setup(m => m.CustomsUQList).Returns(new RefPackTypeCollection(Factory.BOFactory).GetAsCodeDescriptionPairWithStandardUnits());
			var hazardousMaterialCodeQualifierList = new ZArchitecture.Core.CodeDescriptionPairList();
			hazardousMaterialCodeQualifierList.AddPair("U", "UN Code");
			helper.Setup(m => m.HazardousMaterialCodeQualifierList).Returns(hazardousMaterialCodeQualifierList);
			invoiceLineMock.Protected().Setup<JobComInvoiceLineLookups>("GetNewLookups").Returns(helper.Object);
			invoiceLine.JI_JZ = invoice.PK;

			if (dontEmitEntryDetailsInUniversalXML)
			{
				invoiceLineMock.Protected().Setup<bool>("IncludeEntryDetailsInUniversalXMLCore").Returns(false);
			}

			invoice.JobComInvoiceLines.Add(invoiceLine);
			return invoiceLineMock;
		}

		BaseJobComInvoiceLine SetupJobComInvoiceLine(BaseJobComInvoiceLine invoiceLine, ZShort lineNo, ZString harmonisedCode, ZString description, ZDecimal invoiceQuantity, ZString invoiceUQ, ZDecimal linePrice, ZDecimal unitPrice, ZString partNo, ZDecimal volume, ZString volumeUQ, ZDecimal weight, ZString weightUQ, ZString orderNumber, ZDecimal netWeight, ZString netWeightUQ, ZDecimal customsQuantity, ZString customsQuantityUnit, ZDecimal bondedWarehouseQuantity, ZString bondedWarehouseQuantityUnit, ZString countryOfOrigin, ZString commodity, ZString containerMode, ZString matchingKey)
		{
			invoiceLine.JI_LineNo = lineNo;
			invoiceLine.JI_MatchingKey = matchingKey;
			invoiceLine.JI_Tariff = harmonisedCode;
			invoiceLine.JI_Description = description;
			invoiceLine.JI_InvoiceQuantity = invoiceQuantity;
			invoiceLine.JI_InvoiceUQ = invoiceUQ;
			invoiceLine.JI_LinePrice = linePrice;
			invoiceLine.UnitPrice = unitPrice;
			invoiceLine.JI_PartNo = partNo;
			invoiceLine.JI_Volume = volume;
			invoiceLine.JI_VolumeUQ = volumeUQ;
			invoiceLine.JI_Weight = weight;
			invoiceLine.JI_WeightUQ = weightUQ;
			invoiceLine.JI_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			invoiceLine.JI_OrderNumber = orderNumber;
			invoiceLine.JI_NetWeight = netWeight;
			invoiceLine.JI_NetWeightUQ = netWeightUQ;
			invoiceLine.JI_CustomsQuantity = customsQuantity;
			invoiceLine.JI_CustomsUnitQty = customsQuantityUnit;
			invoiceLine.JI_BondedWhsQuantity = bondedWarehouseQuantity;
			invoiceLine.JI_BondedWhsUnitQty = bondedWarehouseQuantityUnit;
			invoiceLine.JI_CountryOfOrigin = countryOfOrigin;
			invoiceLine.JI_RH_NKCommodity_Code = commodity;
			invoiceLine.JI_ContainerMode = containerMode;
			return invoiceLine;
		}

		RefCommodityCode CommodityCode1
		{
			get
			{
				if (commodityCode1 == null)
				{
					commodityCode1 = Factory.New<RefCommodityCode>();
					commodityCode1.RH_Code = "US1!";
					commodityCode1.RH_Description = "US COMMODITY DESCRIPTION 1";
				}
				return commodityCode1;
			}
		}
		RefCommodityCode commodityCode1;

		RefCommodityCode CommodityCode2
		{
			get
			{
				if (commodityCode2 == null)
				{
					commodityCode2 = Factory.New<RefCommodityCode>();
					commodityCode2.RH_Code = "US2!";
					commodityCode2.RH_Description = "US COMMODITY DESCRIPTION 2";
				}
				return commodityCode2;
			}
		}
		RefCommodityCode commodityCode2;

		BaseJobComInvoiceLine SetupJobComInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			var result = SetupJobComInvoiceLine(invoiceLine, 1, "1010101010", "GOODS", 1040.50m, Core.Constants.PkgUnit.Box, 4140.53m, 4600.13m, "PART12", 3.2m, Core.Constants.Volume.CubicYards, 202.92m, Core.Constants.Weight.Hectograms, "ORDER1", 1.555m, Core.Constants.Weight.Tonnes, 10m, Core.Constants.PkgUnit.Package, 15m, Core.Constants.PkgUnit.Dozen, Core.Constants.CountryCodes.Australia, CommodityCode1.RH_Code, Core.Constants.ContainerModes.BreakBulk, "MK1001");
			result.JI_ClassUsageComment = "TEST LINE1 Class Usage Comment";
			result.JI_GS_NKClassUsageCommentReviewer = GlbStaff.CurrentUser.GS_Code;

			return result;
		}

		BaseJobComInvoiceLine SetupJobComInvoiceLine2(BaseJobComInvoiceLine invoiceLine)
		{
			var result = SetupJobComInvoiceLine(invoiceLine, 2, "2020202020", "BAD", 968.45m, Core.Constants.PkgUnit.Package, 6953.85m, 6856.85m, "PART89", 1.69m, Core.Constants.Volume.CubicInches, 365.88m, Core.Constants.Weight.Ounces, "ORDER2", 526502m, Core.Constants.Weight.Grams, 25m, Core.Constants.PkgUnit.Crate, 30m, Core.Constants.PkgUnit.Keg, Core.Constants.CountryCodes.NewZealand, CommodityCode2.RH_Code, Core.Constants.ContainerModes.Containerised, "MK1002");
			result.JI_ClassUsageComment = "TEST LINE2 Class Usage Comment";
			result.JI_GS_NKClassUsageCommentReviewer = string.Empty;

			return result;
		}

		BaseJobComInvoiceLine SetupJobComInvoiceLineRelatedData(BaseJobComInvoiceLine invoiceLine, ZGuid entryLinePK, ZGuid classificationPK, ZGuid orderLinePK, ZString substanceCode)
		{
			invoiceLine.JI_CC = classificationPK;
			invoiceLine.JI_CL = entryLinePK;
			invoiceLine.JI_JO = orderLinePK;
			invoiceLine.JI_HazMatCode = "HZ234";
			invoiceLine.JI_HazMatCodeQualifier = "U";
			var undg = invoiceLine.UNDGs.AddNew();
			var unno = substanceCode.SubstringSafe(0, UNDGSubstanceSchema.DG_UNNO.MaxLength);
			var variant = substanceCode.SubstringSafe(UNDGSubstanceSchema.DG_UNNO.MaxLength, UNDGSubstanceSchema.DG_Variant.MaxLength);
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, unno, variant, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO).FirstOrDefault();
			undg.LinkDefault(subs);
			undg.DI_TechnicalName = "TECHNAME";

			var org = invoiceLine.Declaration.ConfigOrg;
			SetupPartAttrib1(org, "PART1", PartAttributeTypeList.Codes.VIN);
			SetupPartAttrib2(org, "PART2", PartAttributeTypeList.Codes.Mandatory);
			SetupPartAttrib3(org, "PART3", PartAttributeTypeList.Codes.BatchNumber);
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute1, "STRING1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute2, "STRING2");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute3, "STRING3");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute4, "STRING4");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute5, "STRING5");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute6, "STRING6");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomDate1, "DATE1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomDate2, "DATE2");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomDate3, "DATE3");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomDecimal1, "DECIMAL1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomDecimal2, "DECIMAL2");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomDecimal3, "DECIMAL3");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomFlag1, "FLAG1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomFlag2, "FLAG2");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomFlag3, "FLAG3");
			AddCustomsLabel(org, Core.Constants.CustomLabels.ComInvoiceLine.CustomText1, "TEXT1");
			invoiceLine.JI_PartAttrib1 = "PATT1";
			invoiceLine.JI_PartAttrib2 = "PATT2";
			invoiceLine.JI_PartAttrib3 = "PATT3";
			invoiceLine.JI_SerialNumber = "SerialNum";
			invoiceLine.JI_CustomAttrib1 = "CATT1";
			invoiceLine.JI_CustomAttrib2 = "CATT2";
			invoiceLine.JI_CustomAttrib3 = "CATT3";
			invoiceLine.JI_CustomAttrib4 = "CATT4";
			invoiceLine.JI_CustomAttrib5 = "CATT5";
			invoiceLine.JI_CustomAttrib6 = "CATT6";
			invoiceLine.JI_CustomDate1 = ZDateTime.BrettsBirthday.AddDays(1);
			invoiceLine.JI_CustomDate2 = ZDateTime.BrettsBirthday.AddDays(2);
			invoiceLine.JI_CustomDate3 = ZDateTime.BrettsBirthday.AddDays(3);
			invoiceLine.JI_CustomDate4 = ZDateTime.BrettsBirthday.AddDays(4);
			invoiceLine.JI_CustomDate5 = ZDateTime.BrettsBirthday.AddDays(5);
			invoiceLine.JI_CustomDecimal1 = 101.1m;
			invoiceLine.JI_CustomDecimal2 = 102.2m;
			invoiceLine.JI_CustomDecimal3 = 103.3m;
			invoiceLine.JI_CustomDecimal4 = 104.4m;
			invoiceLine.JI_CustomDecimal5 = 105.5m;
			invoiceLine.JI_CustomFlag1 = ZBool.True;
			invoiceLine.JI_CustomFlag2 = ZBool.False;
			invoiceLine.JI_CustomFlag3 = ZBool.True;
			invoiceLine.JI_CustomFlag4 = ZBool.False;
			invoiceLine.JI_CustomFlag5 = ZBool.True;
			invoiceLine.JI_CustomTextBlob1 = "A BIG BLOB";
			return invoiceLine;
		}

		delegate void AssertOrganizationDelegate(string message, OrganizationAddress addressData, string addressType);

		void AssertContentsWithLookingAtChildren(CommercialInvoiceHeader invoiceData, ZString invoiceNumber, AssertOrganizationDelegate assertCommercialInvoiceHeaderBuyer, AssertOrganizationDelegate assertCommercialInvoiceHeaderSupplier, ZDecimal invoiceAmount, ICodeDescription invoiceCurrency, ZDateTime invoiceDate, ICodeDescription incoTerm, ZDecimal volume, ICodeDescription volumeUnit, ZDecimal weight, ICodeDescription weightUnit, ZDecimal netWeight, ICodeDescription netWeightUQ, ICodeDescription invoiceCurrExRateType, ZDecimal invoiceCurrExRate, ZDecimal landedCostExchangeRate, ZString paymentNumber, ZDecimal paymentAmount, ZDecimal paymentExchangeRate, ZDateTime paymentDate, ICodeDescription messageStatus, ZDecimal noOfPacks)
		{
			AssertNotNull("Precondition: invoiceData", invoiceData);

			CombineAssertions(delegate
			{
				AssertEquals("invoiceData.InvoiceNumber", invoiceNumber, invoiceData.InvoiceNumber);
				AssertEquals("invoiceData.InvoiceAmount", invoiceAmount, invoiceData.InvoiceAmount);
				AssertNotNull("invoiceData.InvoiceCurrency", invoiceData.InvoiceCurrency);
				AssertEquals("invoiceData.InvoiceCurrency.Code", invoiceCurrency.Code, invoiceData.InvoiceCurrency.Code);
				AssertEquals("invoiceData.InvoiceCurrency.Description", invoiceCurrency.Description, invoiceData.InvoiceCurrency.Description);
				AssertEquals("invoiceData.InvoiceDate", invoiceDate, invoiceData.InvoiceDate);
				AssertEquals("invoiceData.ValuationDateOverride", invoiceDate.AddDays(2), invoiceData.ValuationDateOverride);
				AssertNotNull("invoiceData.IncoTerm", invoiceData.IncoTerm);
				AssertEquals("invoiceData.IncoTerm.Code", incoTerm.Code, invoiceData.IncoTerm.Code);
				AssertEquals("invoiceData.IncoTerm.Description", incoTerm.Description, invoiceData.IncoTerm.Description);
				AssertEquals("invoiceData.Volume", volume, invoiceData.Volume);
				AssertNotNull("invoiceData.VolumeUnit", invoiceData.VolumeUnit);
				AssertEquals("invoiceData.VolumeUnit.Code", volumeUnit.Code, invoiceData.VolumeUnit.Code);
				AssertEquals("invoiceData.VolumeUnit.Description", volumeUnit.Description, invoiceData.VolumeUnit.Description);
				AssertEquals("invoiceData.Weight", weight, invoiceData.Weight);
				AssertNotNull("invoiceData.WeightUnit", invoiceData.WeightUnit);
				AssertEquals("invoiceData.WeightUnit.Code", weightUnit.Code, invoiceData.WeightUnit.Code);
				AssertEquals("invoiceData.WeightUnit.Description", weightUnit.Description, invoiceData.WeightUnit.Description);
				AssertEquals("invoiceData.NetWeight", netWeight, invoiceData.NetWeight);
				AssertNotNull("invoiceData.NetWeightUQ", invoiceData.NetWeightUQ);
				AssertEquals("invoiceData.NetWeightUQ.Code", netWeightUQ.Code, invoiceData.NetWeightUQ.Code);
				AssertEquals("invoiceData.NetWeightUQ.Description", netWeightUQ.Description, invoiceData.NetWeightUQ.Description);
				AssertEquals("invoiceData.LandedCostExchangeRate", landedCostExchangeRate, invoiceData.LandedCostExchangeRate);
				AssertEquals("invoiceData.PaymentNumber", paymentNumber, invoiceData.PaymentNumber);
				AssertEquals("invoiceData.PaymentAmount", paymentAmount, invoiceData.PaymentAmount);
				AssertEquals("invoiceData.PaymentExchangeRate", paymentExchangeRate, invoiceData.PaymentExchangeRate);
				AssertEquals("invoiceData.PaymentDate", paymentDate, invoiceData.PaymentDate);
				AssertNotNull("invoiceData.MessageStatus", invoiceData.MessageStatus);
				AssertEquals("invoiceData.MessageStatus.Code", messageStatus.Code, invoiceData.MessageStatus.Code);
				AssertEquals("invoiceData.MessageStatus.Description", messageStatus.Description, invoiceData.MessageStatus.Description);
				AssertEquals("invoiceData.NoOfPacks", noOfPacks, invoiceData.NoOfPacks);

				AssertNotNull("Precondition: invoiceData.AddInfoCollection", invoiceData.AddInfoCollection);
				AddInfoCollectionCreatorTest.AssertContents(invoiceData.AddInfoCollection);
			});
			assertCommercialInvoiceHeaderSupplier("invoiceData.Supplier", invoiceData.Supplier, AddressTypes.Supplier);
			assertCommercialInvoiceHeaderBuyer("invoiceData.Buyer", invoiceData.Buyer, AddressTypes.Importer);
		}

		void AssertContents(CommercialInvoiceHeader invoiceData, ZString invoiceNumber, AssertOrganizationDelegate assertCommercialInvoiceHeaderBuyer, AssertOrganizationDelegate assertCommercialInvoiceHeaderSupplier, ZDecimal invoiceAmount, ICodeDescription invoiceCurrency, ZDateTime invoiceDate, ICodeDescription incoTerm, ZDecimal volume, ICodeDescription volumeUnit, ZDecimal weight, ICodeDescription weightUnit, ZDecimal netWeight, ICodeDescription netWeightUQ, ICodeDescription invoiceCurrExRateType, ZDecimal invoiceCurrExRate, ZDecimal landedCostExchangeRate, ZString paymentNumber, ZDecimal paymentAmount, ZDecimal paymentExchangeRate, ZDateTime paymentDate, ICodeDescription messageStatus, ZDecimal noOfPacks)
		{
			AssertContentsWithLookingAtChildren(invoiceData, invoiceNumber, assertCommercialInvoiceHeaderBuyer, assertCommercialInvoiceHeaderSupplier, invoiceAmount, invoiceCurrency, invoiceDate, incoTerm, volume, volumeUnit, weight, weightUnit, netWeight, netWeightUQ, invoiceCurrExRateType, invoiceCurrExRate, landedCostExchangeRate, paymentNumber, paymentAmount, paymentExchangeRate, paymentDate, messageStatus, noOfPacks);
			AssertNotNull("Precondition: invoiceData.CommercialInvoiceLineCollection", invoiceData.CommercialInvoiceLineCollection);
			AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 2, invoiceData.CommercialInvoiceLineCollection.Count);
			AssertContents(invoiceData.CommercialInvoiceLineCollection[0]);
			AssertContents2(invoiceData.CommercialInvoiceLineCollection[1]);
		}

		void AssertContents(CommercialInvoiceHeader invoiceData)
		{
			AssertContents(invoiceData, "INV3243", AssertOrganizationBO_WUFSHIJNB, AssertOrganizationBO_CRAHOLSYD, 3420.34m, GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), new ZDateTime(2011, 4, 3), GetCodeDescriptionPair(Core.Constants.IncoTerms.FreeOnBoard, "Free On Board"), 14.72m, GetCodeDescriptionPair(Core.Constants.Volume.CubicMetres, "Cubic Meters"), 2.53m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 11.11m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 1.25m, 1.50m, "P12345", 1500m, 1.75m, new ZDateTime(2011, 3, 3), GetCodeDescriptionPair(Enterprise.Customs.Common.US.CustomsEntryStatusList.Codes.ClearElectronicInvoiceOriginal, Enterprise.Customs.Common.US.CustomsEntryStatusList.Descriptions.ClearElectronicInvoiceOriginal), 10m);
		}

		void AssertContents2(CommercialInvoiceHeader invoiceData)
		{
			AssertContents(invoiceData, "INV6854", AssertOrganizationBO_CRAHOLSYD, AssertOrganizationBO_WUFSHIJNB, 8685.54m, GetCodeDescriptionPair(Core.Constants.CurrencyCodes.NewZealand, "New Zealand Dollar"), new ZDateTime(2011, 4, 2), GetCodeDescriptionPair(Core.Constants.IncoTerms.CostInsuranceAndFreight, "Cost, Insurance And Freight"), 96.87m, GetCodeDescriptionPair(Core.Constants.Volume.CubicYards, "Cubic Yards"), 86.69m, GetCodeDescriptionPair(Core.Constants.Weight.Kilotonnes, "Kilotons"), 22.22m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, GetCodeDescriptionPair(Enterprise.Customs.Common.US.CustomsEntryStatusList.Codes.NotSent, Enterprise.Customs.Common.US.CustomsEntryStatusList.Descriptions.NotSent), ZDecimal.Zero);
		}

		void AssertContents(CommercialInvoiceLine invoiceLineData, ZInt lineNo, ZString harmonisedCode, ZString formattedTariff, ZString description, ZDecimal invoiceQuantity, ICodeDescription invoiceQuantityUnit, ZDecimal linePrice, ZDecimal unitPrice, ZString partNo, ZDecimal volume, ICodeDescription volumeUnit, ZDecimal weight, ICodeDescription weightUnit, ZString orderNumber, ZDecimal netWeight, ICodeDescription netWeightUnit, ZDecimal customsQuantity, ICodeDescription customsQuantityUnit, ICodeDescription countryOfOrigin, ICodeDescription commodity, ICodeDescription containerMode, ZString matchingKey, string classUsageComment = "", string classUsageCommentReviewer = "")
		{
			AssertNotNull("Precondition: invoiceLineData", invoiceLineData);
			CombineAssertions(delegate
			{
				AssertEquals("invoiceLineData.LineNo", lineNo, invoiceLineData.LineNo);
				AssertEquals("invoiceLineData.HarmonisedCode", harmonisedCode, invoiceLineData.HarmonisedCode);
				AssertEquals("invoiceLineData.FormattedTariff", formattedTariff, invoiceLineData.FormattedTariff);
				AssertEquals("invoiceLineData.Description", description, invoiceLineData.Description);
				AssertEquals("invoiceLineData.InvoiceQuantity", invoiceQuantity, invoiceLineData.InvoiceQuantity);
				AssertNotNull("invoiceLineData.InvoiceQuantityUnit", invoiceLineData.InvoiceQuantityUnit);
				AssertEquals("invoiceLineData.InvoiceQuantityUnit.Code", invoiceQuantityUnit.Code, invoiceLineData.InvoiceQuantityUnit.Code);
				AssertEquals("invoiceLineData.InvoiceQuantityUnit.Description", invoiceQuantityUnit.Description, invoiceLineData.InvoiceQuantityUnit.Description);
				AssertEquals("invoiceLineData.LinePrice", linePrice, invoiceLineData.LinePrice);
				AssertEquals("invoiceLineData.UnitPrice", unitPrice, invoiceLineData.UnitPrice);
				AssertEquals("invoiceLineData.PartNo", partNo, invoiceLineData.PartNo);
				AssertEquals("invoiceLineData.Volume", volume, invoiceLineData.Volume);
				AssertNotNull("invoiceLineData.VolumeUnit", invoiceLineData.VolumeUnit);
				AssertEquals("invoiceLineData.VolumeUnit.Code", volumeUnit.Code, invoiceLineData.VolumeUnit.Code);
				AssertEquals("invoiceLineData.VolumeUnit.Description", volumeUnit.Description, invoiceLineData.VolumeUnit.Description);
				AssertEquals("invoiceLineData.Weight", weight, invoiceLineData.Weight);
				AssertNotNull("invoiceLineData.WeightUnit", invoiceLineData.WeightUnit);
				AssertEquals("invoiceLineData.WeightUnit.Code", weightUnit.Code, invoiceLineData.WeightUnit.Code);
				AssertEquals("invoiceLineData.WeightUnit.Description", weightUnit.Description, invoiceLineData.WeightUnit.Description);
				AssertEquals("invoiceLineData.OrderNumber", orderNumber, invoiceLineData.OrderNumber);
				AssertEquals("invoiceLineData.NetWeight", netWeight, invoiceLineData.NetWeight);
				AssertNotNull("invoiceLineData.NetWeightUnit", invoiceLineData.NetWeightUnit);
				AssertEquals("invoiceLineData.NetWeightUnit.Code", netWeightUnit.Code, invoiceLineData.NetWeightUnit.Code);
				AssertEquals("invoiceLineData.NetWeightUnit.Description", netWeightUnit.Description, invoiceLineData.NetWeightUnit.Description);
				AssertEquals("invoiceLineData.CustomsQuantity", customsQuantity, invoiceLineData.CustomsQuantity);
				AssertNotNull("invoiceLineData.CustomsQuantityUnit", invoiceLineData.CustomsQuantityUnit);
				AssertEquals("invoiceLineData.CustomsQuantityUnit.Code", customsQuantityUnit.Code, invoiceLineData.CustomsQuantityUnit.Code);
				AssertEquals("invoiceLineData.CustomsQuantityUnit.Description", customsQuantityUnit.Description, invoiceLineData.CustomsQuantityUnit.Description);
				AssertNull("invoiceLineData.BondedWarehouseQuantity", invoiceLineData.BondedWarehouseQuantity);
				AssertNull("invoiceLineData.BondedWarehouseQuantityUnit", invoiceLineData.BondedWarehouseQuantityUnit);
				AssertNotNull("invoiceLineData.CountryOfOrigin", invoiceLineData.CountryOfOrigin);
				AssertEquals("invoiceLineData.CountryOfOrigin.Code", countryOfOrigin.Code, invoiceLineData.CountryOfOrigin.Code);
				AssertEquals("invoiceLineData.CountryOfOrigin.Name", countryOfOrigin.Description, invoiceLineData.CountryOfOrigin.Name);
				AssertNotNull("invoiceLineData.Commodity", invoiceLineData.Commodity);
				AssertEquals("invoiceLineData.Commodity.Code", commodity.Code, invoiceLineData.Commodity.Code);
				AssertEquals("invoiceLineData.Commodity.Description", commodity.Description, invoiceLineData.Commodity.Description);
				AssertNotNull("invoiceLineData.ContainerMode", invoiceLineData.ContainerMode);
				AssertEquals("invoiceLineData.ContainerMode.Code", containerMode.Code, invoiceLineData.ContainerMode.Code);
				AssertEquals("invoiceLineData.ContainerMode.Description", containerMode.Description, invoiceLineData.ContainerMode.Description);
				AssertEquals("invoiceLineData.DataImportMatchingKey", matchingKey, invoiceLineData.DataImportMatchingKey);
				AssertEquals("invoiceLineData.ClassUsageComment", classUsageComment, invoiceLineData.ClassUsageComment);
				AssertEquals("invoiceLineData.ClassUsageCommentStaff", classUsageCommentReviewer, invoiceLineData.ClassUsageCommentStaff?.Code ?? ZString.Empty);
				AssertNotNull("Precondition: invoiceLineData.AddInfoCollection", invoiceLineData.AddInfoCollection);
				AddInfoCollectionCreatorTest.AssertContents(invoiceLineData.AddInfoCollection);
			});
		}

		void AssertContents(CommercialInvoiceLine invoiceLineData)
		{
			AssertContents(invoiceLineData
				, 1
				, "1010101010"
				, "1010.10.10 10"
				, "GOODS"
				, 1040.50m
				, GetCodeDescriptionPair(Core.Constants.PkgUnit.Box, "Box")
				, 4786435.26m
				, 4600.13m
				, "PART12"
				, 3.2m
				, GetCodeDescriptionPair(Core.Constants.Volume.CubicYards, "Cubic Yards")
				, 202.92m
				, GetCodeDescriptionPair(Core.Constants.Weight.Hectograms, "Hectograms")
				, "ORDER1"
				, 1.555m
				, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 10m
				, GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, "Package")
				, GetCodeDescriptionPair(Core.Constants.CountryCodes.Australia, "Australia")
				, GetCodeDescriptionPair(CommodityCode1.RH_Code, CommodityCode1.RH_Description)
				, GetCodeDescriptionPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk)
				, "MK1001"
				, "TEST LINE1 Class Usage Comment"
				, GlbStaff.CurrentUser.GS_Code);
		}

		void AssertContents2(CommercialInvoiceLine invoiceLineData)
		{
			AssertContents(invoiceLineData
				, 2
				, "2020202020"
				, "2020.20.20 20"
				, "BAD"
				, 968.45m
				, GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, "Package")
				, 6640516.38m
				, 6856.85m
				, "PART89"
				, 1.69m
				, GetCodeDescriptionPair(Core.Constants.Volume.CubicInches, "Cubic Inches")
				, 365.88m
				, GetCodeDescriptionPair(Core.Constants.Weight.Ounces, "Ounces")
				, "ORDER2"
				, 526502m
				, GetCodeDescriptionPair(Core.Constants.Weight.Grams, "Grams")
				, 25m
				, GetCodeDescriptionPair(Core.Constants.PkgUnit.Crate, "Crate")
				, GetCodeDescriptionPair(Core.Constants.CountryCodes.NewZealand, "New Zealand")
				, GetCodeDescriptionPair(CommodityCode2.RH_Code, CommodityCode2.RH_Description)
				, GetCodeDescriptionPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised)
				, "MK1002"
				, "TEST LINE2 Class Usage Comment"
				, string.Empty);
		}

		void AssertContents(EntryReference entryReferenceData, ZShort? lineNumber, ZString? reference, ICodeDescription type)
		{
			AssertEquals("entryReferenceData.LineNumber", lineNumber, entryReferenceData.LineNumber);
			AssertEquals("entryReferenceData.Reference", reference, entryReferenceData.Reference);
			if (type == null)
			{
				AssertNull("entryReferenceData.Type", entryReferenceData.Type);
			}
			else
			{
				AssertEquals("entryReferenceData.Type.Code", type.Code, entryReferenceData.Type.Code);
				AssertEquals("entryReferenceData.Type.Description", type.Description, entryReferenceData.Type.Description);
			}
		}

		Bill AddNewBill(BaseJobDeclaration declaration, Bill parentBill, ZString billType, ZString billNumber)
		{
			var result = declaration.Bills.AddNew();
			result.CU_BillNum = billNumber;
			result.CU_BillType = billType;
			if (parentBill != null)
			{
				result.CU_CU_ParentBill = parentBill.PK;
			}
			return result;
		}

		RefCusProcedure CreateCusProcedureForInwardProcessingTest(ZString dataGrouping)
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			refDataHelper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var procedure = refDataHelper.CreateRefCusProcedure(dataGrouping, ZString.Empty, "IP", "01", ZString.Empty, "IP DESC", "IMP", intoWarehouse: false, group: "IP");
			procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Factory.SaveForTesting();
			return procedure;
		}

		sealed class CommercialInvoiceHeaderDataObjectWriterForTest : CommercialInvoiceHeaderDataObjectWriter
		{
			public CommercialInvoiceHeaderDataObjectWriterForTest(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, CusEntryHeader relatedEntry = null) : base(manager, helper, landedCostDataWriter, relatedEntry)
			{
			}

			protected override void PopulateCommercialInvoiceLineFields(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
			{
				if (!invoiceLine.JI_Description.IsEmpty)
				{
					invoiceLineData.DetailedDescription = invoiceLine.JI_Description;
				}
			}
		}
	}
}
