using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class CusEntryLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckMaximumContainersAmount()
	{
		const string messageError = "Container count exceeds maximum of 99";
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ContainerMode = ContainerModes.FCL;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		for (var i = 0; i < Constants.MaximumBusinessObjectsAmounts.MaximumEntryLineContainers; i++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = $"{i}";
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = i;
			package.CW_PackType = $"{i}";
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
		}

		var packagesForInvoiceLinesForBindingOnly = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
		for (var i = 0; i < Constants.MaximumBusinessObjectsAmounts.MaximumEntryLineContainers; i++)
		{
			packagesForInvoiceLinesForBindingOnly[i].IsLinked = true;
		}

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();

		CombineAssertions(() =>
		{
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageError("99 containers", entryLine, messageError);

			var container100 = declaration.CusContainers.AddNew();
			container100.CO_ContainerNumber = "100";
			var package100 = declaration.Packages.AddNew();
			package100.CW_PackQty = 100;
			package100.CW_PackType = "100";
			package100.CW_ContainerNoOrEquipmentNo = container100.CO_ContainerNumber;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[99].IsLinked = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entryHeader = declaration.CustomsEntryHeaders.First();
			entryLine = entryHeader.AllEntryLines.First();

			entryLine.Validation.ValidateAll();
			AssertHasRowMessageError("100 containers", entryLine, messageError);
		});
	}

	public void TestCheckMaximumAdditionalDocumentsAmount()
	{
		const string messageError = "Additional information count exceeds maximum of 99";
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		for (var i = 0; i < 33; i++)
		{
			var document = declaration.AdditionalInfos.AddNew();
			document.CSI_Code = $"d{i}";
			document.CSI_Description = $"d{i}";
		}

		for (var i = 0; i < 33; i++)
		{
			var document = invoice.AdditionalInfos.AddNew();
			document.CSI_Code = $"i{i}";
			document.CSI_Description = $"i{i}";
		}

		for (var i = 0; i < 33; i++)
		{
			var document = invoiceLine.AdditionalInfos.AddNew();
			document.CSI_Code = $"l{i}";
			document.CSI_Description = $"l{i}";
		}

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();

		CombineAssertions(() =>
		{
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageError("99 additional infos", entryLine, messageError);

			var document100 = invoiceLine.AdditionalInfos.AddNew();
			document100.CSI_Code = "100";
			document100.CSI_Description = "100";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entryHeader = declaration.CustomsEntryHeaders.First();
			entryLine = entryHeader.AllEntryLines.First();

			entryLine.Validation.ValidateAll();
			AssertHasRowMessageError("100 additional infos", entryLine, messageError);
		});
	}

	public void TestCheckMaximumPreviousDocumentsAmount()
	{
		const string messageError = "Previous documents count exceeds maximum of 99";
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		for (var i = 0; i < 33; i++)
		{
			var document = declaration.PreviousDocuments.AddNew();
			document.CSI_Code = $"d{i}";
			document.CSI_Description = $"d{i}";
		}

		for (var i = 0; i < 33; i++)
		{
			var document = invoice.PreviousDocuments.AddNew();
			document.CSI_Code = $"i{i}";
			document.CSI_Description = $"i{i}";
		}

		for (var i = 0; i < 33; i++)
		{
			var document = invoiceLine.PreviousDocuments.AddNew();
			document.CSI_Code = $"l{i}";
			document.CSI_Description = $"l{i}";
		}

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First(x => x.PreviousDocumentCount != 0);

		CombineAssertions(() =>
		{
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageError("99 PreviousDocuments", entryLine, messageError);

			var document100 = invoiceLine.PreviousDocuments.AddNew();
			document100.CSI_Code = "100";
			document100.CSI_Description = "100";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entryHeader = declaration.CustomsEntryHeaders.First();
			entryLine = entryHeader.AllEntryLines.First(x => x.PreviousDocumentCount != 0);

			entryLine.Validation.ValidateAll();
			AssertHasRowMessageError("100 PreviousDocuments", entryLine, messageError);
		});
	}

	public void TestCheckMaximumSupportingDocumentsAmount()
	{
		AddLineSupportingDocumentReferenceData();
		const string messageError = "Supporting documents count exceeds maximum of 99";
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		for (var i = 0; i < 49; i++)
		{
			var document = declaration.SupportingDocuments.AddNew();
			document.CSI_Code = $"d{i}";
			document.CSI_Description = $"d{i}";
		}

		for (var i = 0; i < 49; i++)
		{
			var document = invoice.SupportingDocuments.AddNew();
			document.CSI_Code = $"i{i}";
			document.CSI_Description = $"i{i}";
		}

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();

		CombineAssertions(() =>
		{
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageError("99 SupportingDocuments", entryLine, messageError);

			var invoiceLineDocument = invoiceLine.SupportingDocuments.AddNew();
			invoiceLineDocument.CSI_Code = "9001";
			invoiceLineDocument.CSI_Description = "9001";

			var document100 = invoice.SupportingDocuments.AddNew();
			document100.CSI_Code = "100";
			document100.CSI_Description = "100";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entryHeader = declaration.CustomsEntryHeaders.First();
			entryLine = entryHeader.AllEntryLines.First();

			entryLine.Validation.ValidateAll();
			AssertHasRowMessageError("100 SupportingDocuments", entryLine, messageError);
		});
	}

	void AddLineSupportingDocumentReferenceData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
		var attributeNameValuePairs = new Dictionary<string, string[]>();

		const string importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		const string exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		attributeNameValuePairs.Add("Level", new[] { "ITEM" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			new[] { importCodeType, exportCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();
	}
}
