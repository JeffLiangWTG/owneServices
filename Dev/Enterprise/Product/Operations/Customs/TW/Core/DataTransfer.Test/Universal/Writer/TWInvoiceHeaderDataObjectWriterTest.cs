using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	[TestedType(typeof(TWInvoiceHeaderDataObjectWriter))]
	sealed class TWInvoiceHeaderDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		public void TestInvoiceLineCustomsData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var entryInstruction = declaration.CusEntryInstruction;
				var invoice = declaration.Invoices.AddNew();
				var aLength514 = new ZString('A', 514);
				invoice.TW_MarksAndNumbers = aLength514;
				var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_CEI = entryInstruction.PK;
				var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
				var controllingMessageHeader = controllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingAgency = "20";
				controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
				controllingMessageHeader.PermitNumber = "110";
				controllingMessageHeader = controllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingAgency = "DN";
				controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
				controllingMessageHeader.PermitNumber = "220";
				var invoiceLineLinkControllingMsgHeaders = invoiceLine.InvoiceLineLinkControllingMsgHeaders;
				NUnit.Framework.Assert.That(invoiceLineLinkControllingMsgHeaders.Count, Is.EqualTo(2));
				invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
				var helper = new TWDataObjectWriterHelper(Factory.BOFactory);
				var writer = new TWInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), helper);
				helper.AllocateControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[0].ControllingMessageHeaderPK);
				helper.AllocateControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[1].ControllingMessageHeaderPK);
				invoiceLine.PreviousPermitNo = "PRE1";
				invoiceLine.PartyIdentifier = "PI";
				invoiceLine.CertificateNo = "CN";
				invoiceLine.AuthorizedPerson = "AP";
				invoiceLine.TypeApprovalPartyIdentifier = "TPI";
				invoiceLine.TypeApprovalCertificateNo = "TCN";
				invoiceLine.TypeApprovalAuthorizedParty = "TAP";
				invoiceLine.JI_DeclarationGoodsDescription = "XXX";
				invoiceLine.NX101ShippingMarks = "Shipping remarks test";
				var result = writer.GetDataObject(invoice);
				CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(result.MarksAndNumbers.GetValueOrDefault(), Is.EqualTo(aLength514));
					NUnit.Framework.Assert.That(result.CommercialInvoiceLineCollection.Count, Is.EqualTo(1));
					var commercialInvoiceLine = result.CommercialInvoiceLineCollection[0];
					var customsSupportingInformationCollection = commercialInvoiceLine.CustomsSupportingInformationCollection;
					NUnit.Framework.Assert.That(commercialInvoiceLine.AddInfoGroupCollection.Any(group => group.Type.Code.GetValueOrDefault() == "CML" && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "ControllingMessageLink" && innerAddinfo.Value.GetValueOrDefault() == "2")), Is.True);
					NUnit.Framework.Assert.That(commercialInvoiceLine.DetailedDescription.GetValueOrDefault(), Is.EqualTo("XXX").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(customsSupportingInformationCollection.Count, Is.EqualTo(2));
					NUnit.Framework.Assert.That(customsSupportingInformationCollection.Any(support => support.Category.Code.GetValueOrDefault() == "MIF" && support.Type.Code.GetValueOrDefault() == "PI"), Is.True);
					var supportingInformation = customsSupportingInformationCollection.FirstOrDefault(support => support.Category.Code.GetValueOrDefault() == "MIF" && support.Type.Code.GetValueOrDefault() == "PI");
					NUnit.Framework.Assert.That(supportingInformation.ReferenceNumberCollection.Any(information => information.Type.Code.GetValueOrDefault() == "RN1" && information.ReferenceNumber.GetValueOrDefault() == "CN"), Is.True);
					NUnit.Framework.Assert.That(supportingInformation.ReferenceNumberCollection.Any(information => information.Type.Code.GetValueOrDefault() == "RN2" && information.ReferenceNumber.GetValueOrDefault() == "AP"), Is.True);
					NUnit.Framework.Assert.That(customsSupportingInformationCollection.Any(support => support.Category.Code.GetValueOrDefault() == "TAC" && support.Type.Code.GetValueOrDefault() == "TPI"), Is.True);
					supportingInformation = customsSupportingInformationCollection.FirstOrDefault(support => support.Category.Code.GetValueOrDefault() == "TAC" && support.Type.Code.GetValueOrDefault() == "TPI");
					NUnit.Framework.Assert.That(supportingInformation.ReferenceNumberCollection.Any(information => information.Type.Code.GetValueOrDefault() == "RN1" && information.ReferenceNumber.GetValueOrDefault() == "TCN"), Is.True);
					NUnit.Framework.Assert.That(supportingInformation.ReferenceNumberCollection.Any(information => information.Type.Code.GetValueOrDefault() == "RN2" && information.ReferenceNumber.GetValueOrDefault() == "TAP"), Is.True);
					NUnit.Framework.Assert.That(commercialInvoiceLine.AddInfoCollection.Any(addInfo => addInfo.Key.GetValueOrDefault() == "PreviousPermitNumber" && addInfo.Value.GetValueOrDefault() == "PRE1"), Is.True);
					NUnit.Framework.Assert.That(commercialInvoiceLine.AddInfoCollection.Any(addInfo => addInfo.Key.GetValueOrDefault() == "NX101ShippingMarks" && addInfo.Value.GetValueOrDefault() == "Shipping remarks test"), Is.True);
				});
			}
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}

		IDisposable setupCreator;
		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
