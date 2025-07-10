using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(AdditionalInfo))]
sealed class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
{
	public void TestSetDefaultValues()
	{
		var addInfo = GetAdditionalInfo(Factory);
		AssertEquals("CSI_Status", ZString.Empty, addInfo.CSI_Status);
	}

	public void TestValidationType()
	{
		CombineAssertions(() =>
		{
			AssertType<ImportAdditionalInfoValidation>("Import", GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Import).Validation);
			AssertType<ExportAdditionalInfoValidation>("Export", GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export).Validation);
		});
	}

	public void TestCSI_SubType_Caption()
	{
		AssertEquals("Kind", DataBoundResourceStrings.GetDataForProperty(GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export).CSI_SubTypeInfo).Caption);
	}

	public void TestCSI_Description_ReadOnly()
	{
		var additionalInfo = GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export);
		CombineAssertions(() =>
		{
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
			AssertEquals("Is not readonly", false, additionalInfo.CSI_DescriptionInfo.ReadOnly);
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.REF;
			AssertEquals("Is readonly", true, additionalInfo.CSI_DescriptionInfo.ReadOnly);

			using (PLCustomsDataRegistry.Instance.PCSEmailChannel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PL701071897800000"))
			{
				additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
				additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._POW01;
				AssertEquals("Is not readonly when PCSEmailChannel is pre-configured and Code is not equal to PCS01", false, additionalInfo.CSI_DescriptionInfo.ReadOnly);

				additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._PCS01;
				AssertEquals("Is readonly when PCSEmailChannel is pre-configured and Code equals to PCS01", true, additionalInfo.CSI_DescriptionInfo.ReadOnly);
			}

			additionalInfo = GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Import);
			AssertEquals("Import", false, additionalInfo.CSI_DescriptionInfo.ReadOnly);
		});
	}

	public void TestCSI_Description_ClearedIfNotAdditionalFormation()
	{
		var additionalInfo = GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export);
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		additionalInfo.CSI_Description = "Description";
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		AssertEquals("Is cleared", ZString.Empty, additionalInfo.CSI_Description);
	}

	public void TestCSI_ReferenceNumber_ReadOnly()
	{
		var additionalInfo = GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export);
		CombineAssertions(() =>
		{
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.REF;
			AssertEquals("Is not readonly", false, additionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
			AssertEquals("Is readonly", true, additionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
		});
	}

	public void TestCSI_ReferenceNumber_ClearedIfAdditionalFormation()
	{
		var additionalInfo = GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export);
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		additionalInfo.CSI_ReferenceNumber = "Reference";
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		AssertEquals("CSI_ReferenceNumber is cleared", ZString.Empty, additionalInfo.CSI_ReferenceNumber);

		additionalInfo.CSI_Description = "Description";
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		AssertEquals("CSI_Description is cleared", ZString.Empty, additionalInfo.CSI_Description);

		using (PLCustomsDataRegistry.Instance.PCSEmailChannel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PL701071897800000"))
		{
			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._PCS01;
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
			AssertEquals("CSI_Description is pre-configured when Code equals to PCS01", "PL701071897800000", additionalInfo.CSI_Description);
		}
	}

	public void TestCSI_ReferenceNumber_MaxLength()
	{
		var additionalInfo = GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export);
		AssertEquals(70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
	}

	public void TestCSI_SubType_MaxLength()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Export", 3, GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export).CSI_SubTypeInfo.MaxLength);
			AssertEquals("Import", 5, GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Import).CSI_SubTypeInfo.MaxLength);
		});
	}

	public void TestParentInfo()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationDocument = declaration.AdditionalInfos.AddNew();
		var entryInstructionDocument = declaration.CustomsEntryInstructions.AddNew().AdditionalInfos.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceDocument = invoice.AdditionalInfos.AddNew();
		var invoiceLineDocument = invoice.InvoiceLines.AddNew().AdditionalInfos.AddNew();

		CombineAssertions(() =>
		{
			CheckDocumentParent("Declaration level", declarationDocument, isDeclarationLevel: true);
			CheckDocumentParent("EntryInstruction level", entryInstructionDocument, isEntryInstructionLevel: true);
			CheckDocumentParent("Invoice level", invoiceDocument, isInvoiceLevel: true);
			CheckDocumentParent("Invoice Line level", invoiceLineDocument, isInvoiceLineLevel: true);
		});

		void CheckDocumentParent(string message, CusSupportingInfo document, bool isDeclarationLevel = false, bool isEntryInstructionLevel = false, bool isInvoiceLevel = false, bool isInvoiceLineLevel = false)
		{
			AssertEquals($"{message} ParentAsJobDeclaration", isDeclarationLevel, document.Parent is JobDeclaration);
			AssertEquals($"{message} ParentAsEntryInstruction", isEntryInstructionLevel, document.Parent is CusEntryInstruction);
			AssertEquals($"{message} ParentAsInvoiceHeader", isInvoiceLevel, document.Parent is JobComInvoiceHeader);
			AssertEquals($"{message} ParentAsInvoiceLine", isInvoiceLineLevel, document.Parent is JobComInvoiceLine);
		}
	}

	public void TestCSI_Code_Caption()
	{
		AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(GetAdditionalInfo(Factory).CSI_CodeInfo).Caption);
	}

	public void TestCSI_Code_SetsPreConfiguredDescription()
	{
		var additionalInfo = GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export);
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		additionalInfo.CSI_Description = "Description";
		AssertEquals("Gets original value", "Description", additionalInfo.CSI_Description);
		using (PLCustomsDataRegistry.Instance.PCSEmailChannel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PL701071897800000"))
		{
			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._POW01;
			AssertEquals("Gets original value when Code is not equal to PCS01", "Description", additionalInfo.CSI_Description);

			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._PCS01;
			AssertEquals("Gets pre-configured value", "PL701071897800000", additionalInfo.CSI_Description);
		}
	}

	protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return declaration.AdditionalInfos.AddNew();
		yield return declaration.CustomsEntryInstructions.AddNew().AdditionalInfos.AddNew();
		var invoice = declaration.Invoices.AddNew();
		yield return invoice.AdditionalInfos.AddNew();
		yield return invoice.InvoiceLines.AddNew().AdditionalInfos.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject() => GetAdditionalInfo(Factory);

	static AdditionalInfo GetAdditionalInfo(BusinessObjectFactory factory, string messageType = null)
	{
		var declaration = factory.New<JobDeclaration>();
		if (!string.IsNullOrEmpty(messageType))
		{
			declaration.JE_MessageType = messageType;
		}
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		return invoiceLine.AdditionalInfos.AddNew();
	}
}
