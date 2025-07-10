using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	public sealed class TWDataObjectWriterHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		public void TestPopulateCusEntryInstructionCustomsReferences()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CusEntryInstruction;
			var writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory.BOFactory));
			var result = writer.GetDataObject(entryInstruction);
			NUnit.Framework.Assert.That(!(result.CustomsReferenceCollection?.Any(reference => reference.Type.Code == new ZString?("CDD")) ?? false), Is.True);
			var declarationDuplicate = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicate.CY_Code = "1";
			declarationDuplicate.Copy = 10;
			declarationDuplicate = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicate.CY_Code = "2";
			declarationDuplicate.Copy = 1;
			result = writer.GetDataObject(entryInstruction);
			NUnit.Framework.Assert.That(result.CustomsReferenceCollection.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(result.CustomsReferenceCollection.Any(reference => reference.Type.Code.GetValueOrDefault() == "CDD" && reference.SubType.Code.GetValueOrDefault() == "1" && reference.Reference.GetValueOrDefault() == "10"), Is.True);
			NUnit.Framework.Assert.That(result.CustomsReferenceCollection.Any(reference => reference.Type.Code.GetValueOrDefault() == "CDD" && reference.SubType.Code.GetValueOrDefault() == "2" && reference.Reference.GetValueOrDefault() == "1"), Is.True);
		}

		[ExpectNoExceptions]
		public void TestPopulateJobDeclarationCustomsReferences()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var helper = new TWDataObjectWriterHelper(declaration.Factory);
			var result = helper.GetAdditionalCustomsReferenceDataFor(declaration, null);
			NUnit.Framework.Assert.That(!(result?.Any(reference => reference.Type.Code == new ZString?("DRF")) ?? false), Is.True);
			var reservedField = declaration.ReservedFields.AddNew();
			reservedField.CY_Code = "1";
			reservedField.CY_Data = "2";
			reservedField = declaration.ReservedFields.AddNew();
			reservedField.CY_Code = "3";
			reservedField.CY_Data = "4";
			result = helper.GetAdditionalCustomsReferenceDataFor(declaration, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			NUnit.Framework.Assert.That(result.Count(), Is.EqualTo(2));
			NUnit.Framework.Assert.That(result.Any(reference => reference.Type.Code == new ZString?("DRF") && reference.Type.Description == new ZString?("Declaration Reserved Field") && reference.Reference == new ZString?("1") && reference.ReferencedEntityDescription == new ZString?("2")), Is.True);
			NUnit.Framework.Assert.That(result.Any(reference => reference.Type.Code == new ZString?("DRF") && reference.Type.Description == new ZString?("Declaration Reserved Field") && reference.Reference == new ZString?("3") && reference.ReferencedEntityDescription == new ZString?("4")), Is.True);
		}

		[ExpectNoExceptions]
		public void TestJobComInvoiceLineCustomsReferences_ReservedFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			var helper = new TWDataObjectWriterHelper(invoiceLine.Factory);
			var result = helper.GetAdditionalCustomsReferenceDataFor(invoiceLine, null);
			NUnit.Framework.Assert.That(!(result?.Any(reference => reference.Type.Code == new ZString?("DRF")) ?? false), Is.True);
			var reservedField = invoiceLine.ReservedFields.AddNew();
			reservedField.CY_Code = "1";
			reservedField.CY_Data = "2";
			reservedField = invoiceLine.ReservedFields.AddNew();
			reservedField.CY_Code = "3";
			reservedField.CY_Data = "4";
			result = helper.GetAdditionalCustomsReferenceDataFor(invoiceLine, null);
			NUnit.Framework.Assert.That(result.Count(), Is.EqualTo(2));
			NUnit.Framework.Assert.That(result.Any(reference => reference.Type.Code == new ZString?("DRF") && reference.Type.Description == new ZString?("Declaration Reserved Field") && reference.Reference == new ZString?("1") && reference.ReferencedEntityDescription == new ZString?("2")), Is.True);
			NUnit.Framework.Assert.That(result.Any(reference => reference.Type.Code == new ZString?("DRF") && reference.Type.Description == new ZString?("Declaration Reserved Field") && reference.Reference == new ZString?("3") && reference.ReferencedEntityDescription == new ZString?("4")), Is.True);
		}

		[ExpectNoExceptions]
		public void TestJobComInvoiceLineCustomsReferences()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var writer = new TWInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory.BOFactory));
			var result = writer.GetDataObject(invoice);
			NUnit.Framework.Assert.That(result.CommercialInvoiceLineCollection.Count, Is.EqualTo(1));
			var commercialInvoiceLine = result.CommercialInvoiceLineCollection[0];
			NUnit.Framework.Assert.That(!(commercialInvoiceLine.CustomsReferenceCollection?.Any(reference => reference.Type.Code == new ZString?("CNN")) ?? false), Is.True);
			invoiceLine.JI_Tariff = "87";
			var chassis = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "1";
			chassis = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "2";
			result = writer.GetDataObject(invoice);
			NUnit.Framework.Assert.That(result.CommercialInvoiceLineCollection.Count, Is.EqualTo(1));
			commercialInvoiceLine = result.CommercialInvoiceLineCollection[0];
			var carChassisNumbers = commercialInvoiceLine.CustomsReferenceCollection.Where(reference => reference.Type.Code == new ZString?("CCN"));
			NUnit.Framework.Assert.That(carChassisNumbers.Count(), Is.EqualTo(2));
			NUnit.Framework.Assert.That(carChassisNumbers.Any(carChassis => carChassis.Reference == new ZString?("1")), Is.True);
			NUnit.Framework.Assert.That(carChassisNumbers.Any(carChassis => carChassis.Reference == new ZString?("2")), Is.True);
			result = writer.GetDataObject(invoice);
			NUnit.Framework.Assert.That(result.CommercialInvoiceLineCollection.Count, Is.EqualTo(1));
			commercialInvoiceLine = result.CommercialInvoiceLineCollection[0];
			NUnit.Framework.Assert.That(!(commercialInvoiceLine.CustomsReferenceCollection?.Any(reference => reference.Type.Code == new ZString?("AGA")) ?? false), Is.True);
			NUnit.Framework.Assert.That(!(commercialInvoiceLine.CustomsReferenceCollection?.Any(reference => reference.Type.Code == new ZString?("TWF")) ?? false), Is.True);
			var assignedNumber = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedNumber.JG_ReferenceNumber = "1";
			var foodData = invoiceLine.FoodDataCollection.AddNew();
			foodData.CY_Data = "2";
			foodData.Content = 1;
			result = writer.GetDataObject(invoice);
			commercialInvoiceLine = result.CommercialInvoiceLineCollection[0];
			NUnit.Framework.Assert.That(commercialInvoiceLine.CustomsReferenceCollection.Count, Is.EqualTo(4));
			NUnit.Framework.Assert.That(commercialInvoiceLine.CustomsReferenceCollection.Any(reference => reference.Type.Code == new ZString?("AGA") && reference.Reference == new ZString?("1")), Is.True);
			NUnit.Framework.Assert.That(commercialInvoiceLine.CustomsReferenceCollection.Any(reference => reference.Type.Code == new ZString?("TWF") && reference.Reference == new ZString?("2") && reference.ReferencedEntityDescription == new ZString?("1")), Is.True);
		}

		[ExpectNoExceptions]
		public void TestAllocateControllingMessageHeaderLink()
		{
			var factory = new BusinessObjectFactory();
			var jobDeclartion = factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";
			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			NUnit.Framework.Assert.That(invoiceLineLinkControllingMsgHeaders.Count, Is.EqualTo(2));
			var helper = new TWDataObjectWriterHelper(factory);
			helper.AllocateControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[0].PK);
			NUnit.Framework.Assert.That(helper.GetAllocatedControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[0].PK), Is.EqualTo(new ZString?("1")));
			NUnit.Framework.Assert.That(helper.GetAllocatedControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[1].PK), Is.EqualTo("").Using(CustomComparers.TypeComparison));
			helper.AllocateControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[1].PK);
			NUnit.Framework.Assert.That(helper.GetAllocatedControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[1].PK), Is.EqualTo(new ZString?("2")));
		}

		[ExpectNoExceptions]
		public void TestGetWayBillType()
		{
			var helper = new TWDataObjectWriterHelper(Factory.BOFactory);
			NUnit.Framework.Assert.That(helper.GetWayBillType("CN").Code.Value, Is.EqualTo("CNN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(helper.GetWayBillType("HB").Code.Value, Is.EqualTo("HWB").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(helper.GetWayBillType("MB").Code.Value, Is.EqualTo("MWB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsReferences_StorageAndShippingConditions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var writer = new TWInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory.BOFactory));
			var storageAndShippingCondition1 = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
			storageAndShippingCondition1.JG_ReferenceNumber = "2";

			var storageAndShippingCondition2 = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
			storageAndShippingCondition2.JG_ReferenceNumber = "3";

			var result = writer.GetDataObject(invoice);
			var commercialInvoiceLine = result.CommercialInvoiceLineCollection[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(commercialInvoiceLine.CustomsReferenceCollection.Count, Is.EqualTo(2), "Should have two SSC records");
				NUnit.Framework.Assert.That(commercialInvoiceLine.CustomsReferenceCollection.Any(reference => reference.Type.Code.GetValueOrDefault() == "SSC" && reference.Reference.GetValueOrDefault() == "2" && reference.Type.Description.GetValueOrDefault() == "Storage and Shipping Condition"), Is.True, "Contain the record(Code:SSC, Type:Storage and Shipping Condition, Reference:2)");
				NUnit.Framework.Assert.That(commercialInvoiceLine.CustomsReferenceCollection.Any(reference => reference.Type.Code.GetValueOrDefault() == "SSC" && reference.Reference.GetValueOrDefault() == "3" && reference.Type.Description.GetValueOrDefault() == "Storage and Shipping Condition"), Is.True, "Contain the record(Code:SSC, Type:Storage and Shipping Condition, Reference:3)");
			});
		}
	}
}
