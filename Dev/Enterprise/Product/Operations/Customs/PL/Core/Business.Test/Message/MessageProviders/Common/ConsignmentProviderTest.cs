using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

public class ConsignmentProviderTest : DataProviderTestCase<ConsignmentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null EntryHeader", "Value cannot be null.\r\nParameter name: entryHeader",
				() => new AESConsignmentProvider(null));
			AssertExceptionThrown<ArgumentNullException>("Null Declaration", "Value cannot be null.\r\nParameter name: entryHeader.Declaration",
				() => new AESConsignmentProvider(Factory.New<CusEntryHeader>()));
			AssertExceptionThrown<ArgumentNullException>("Null EntryInstruction", "Value cannot be null.\r\nParameter name: entryHeader.EntryInstruction",
				() => new AESConsignmentProvider(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
		});
	}

	public void TestContainerIndicator()
	{
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
		CombineAssertions(() =>
		{
			AssertEquals("Declaration ContainerMode is LCL", 1, GetProvider().ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("Declaration ContainerMode is not LCL, FCL, ULD, CNT", 0, GetProvider().ContainerIndicator);
			entryInstruction.CEI_SubStyle = SubStyleCodes.B;
			AssertNull("ContainerIndicator null, CEI_SubStyle is B", GetProvider().ContainerIndicator);
		});
	}

	public void TestInlandModeOfTransport_WhenOfficeOfExportIsUsed()
	{
		declaration.JE_OfficeOfEntryExit = "ABC";
		declaration.JE_CustomsOffice = "ABC";
		declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;

		CombineAssertions(() =>
		{
			AssertNull("CustomsOfficeOfExport equals CustomsOfficeOfExitDeclared", GetProvider().InlandModeOfTransport);

			declaration.JE_CustomsOffice = "XYZ";
			AssertEquals("JE_CustomsOffice is not equal CustomsOfficeOfExitDeclared", "1", GetProvider().InlandModeOfTransport);

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			entryInstruction.CEI_SubStyle = SubStyleCodes.A;
			AssertEquals("CEI_Substyle is not B, C, E, F", "1", GetProvider().InlandModeOfTransport);

			entryInstruction.CEI_SubStyle = SubStyleCodes.B;
			AssertNull("CEI_SubStyle is B", GetProvider().InlandModeOfTransport);

			entryInstruction.CEI_SubStyle = SubStyleCodes.A;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			AssertEquals("CEI_Procedure is not 10", "1", GetProvider().InlandModeOfTransport);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._10;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
			AssertEquals("JE_MessageSubType is not CO", "1", GetProvider().InlandModeOfTransport);

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._10;
			AssertNull("JE_EntryStyle CO and CEI_Procedure 10", GetProvider().InlandModeOfTransport);
		});
	}

	public void TestInlandModeOfTransport_BasedOnCustomsOfficeOfPresentation()
	{
		declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
		declaration.JE_OfficeOfEntryExit = "ABC";

		var euOfficeCode = declaration.CustomsOffices.AddNew();
		euOfficeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		euOfficeCode.CY_Data = "ABC";
		declaration.CustomsOfficesForBinding.Add(euOfficeCode);

		CombineAssertions(() =>
		{
			AssertNull("CustomsOfficeOfPresentation equals CustomsOfficeOfExitDeclared", GetProvider().InlandModeOfTransport);

			declaration.JE_OfficeOfEntryExit = "XYZ";
			AssertEquals("CustomsOfficeOfPresentation is not equal CustomsOfficeOfExitDeclared", "1", GetProvider().InlandModeOfTransport);

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			entryInstruction.CEI_SubStyle = SubStyleCodes.A;
			AssertEquals("CEI_Substyle is not B, C, E, F", "1", GetProvider().InlandModeOfTransport);

			entryInstruction.CEI_SubStyle = SubStyleCodes.B;
			AssertNull("CEI_SubStyle is B", GetProvider().InlandModeOfTransport);

			entryInstruction.CEI_SubStyle = SubStyleCodes.A;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			AssertEquals("CEI_Procedure is not 10", "1", GetProvider().InlandModeOfTransport);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._10;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
			AssertEquals("JE_MessageSubType is not CO", "1", GetProvider().InlandModeOfTransport);

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._10;
			AssertNull("JE_EntryStyle CO and CEI_Procedure 10", GetProvider().InlandModeOfTransport);
		});
	}

	public void TestTransportEquipments()
	{
		var equipment1 = declaration.Equipments.AddNew();
		var seal1 = equipment1.Seals.AddNew();
		seal1.BK_SealNumber = "asd1";
		var equipment2 = declaration.Equipments.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Single Equipment - count", 1, GetProvider().TransportEquipments.Count);
			AssertEquals("Single Equipment - sequence numbers", "1", string.Join(",", GetProvider().TransportEquipments.Select(x => x.SequenceNumber)));

			var seal2 = equipment2.Seals.AddNew();
			seal2.BK_SealNumber = "asd2";
			AssertEquals("Equipments - count", 2, GetProvider().TransportEquipments.Count);
			AssertEquals("Equipments - sequence numbers", "1,2", string.Join(",", GetProvider().TransportEquipments.Select(x => x.SequenceNumber)));

			seal1.Delete();
			seal2.Delete();
			entryInstruction.ZG_SealsCount = 0;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "123";
			invoiceLine.ContainersPivot.AddPivotFor(container);
			AssertEquals("Only container - count", 1, GetProvider().TransportEquipments.Count);
			AssertEquals("Only container - sequence numbers", "1", string.Join(",", GetProvider().TransportEquipments.Select(x => x.SequenceNumber)));

			seal1 = equipment1.Seals.AddNew();
			seal1.BK_SealNumber = "asd1";
			AssertEquals("Seal and container - count", 2, GetProvider().TransportEquipments.Count);
			AssertEquals("Seal and container - sequence numbers", "1,2", string.Join(",", GetProvider().TransportEquipments.Select(x => x.SequenceNumber)));

			seal1.Delete();
			var newInvoiceLine = invoice.InvoiceLines.AddNew();
			newInvoiceLine.ContainersPivot.AddPivotFor(container);
			var lineMerger = new LineMerger(declaration);
			lineMerger.DoMerge();
			AssertEquals("No duplicate containers - count", 1, GetProvider().TransportEquipments.Count);
			AssertEquals("All goods in one container - goods references count", 0, GetProvider().TransportEquipments.First().GoodsReferences.Count);

			invoice.InvoiceLines.AddNew();
			lineMerger.DoMerge();
			AssertEquals("Not all goods in one container - goods references count", 2, GetProvider().TransportEquipments.First().GoodsReferences.Count);
		});
	}

	public void TestLocationOfGoods() => AssertNotNull(GetProvider().LocationOfGoods);

	public void TestDepartureTransportMeans()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportModeInland = "";
			AssertEquals("Departure Transport Means is empty", 0, GetProvider().DepartureTransportMeans.Count);

			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Mail;
			AssertEquals("Departure Transport Means is not empty", 1, GetProvider().DepartureTransportMeans.Count);

			entryInstruction.CEI_Procedure = ProcedureCodes._21;
			AssertEquals("Departure Transport Means is empty", 0, GetProvider().DepartureTransportMeans.Count);
		});
	}

	protected override ConsignmentProvider GetProvider() => new ConsignmentProvider(entryHeader);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Description = "invoice line 1";

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
	CusEntryInstruction entryInstruction;
}
