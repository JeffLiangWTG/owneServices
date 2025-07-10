using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105CMManufacturerWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(manufacturer.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader1 = controllingMessageHeaders.AddNew();
			controllingMessageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.NX601).IsLinkedCMHeader = true;
			NUnit.Framework.Assert.That(manufacturer.ID, NUnit.Framework.Is.EqualTo("0123456789").Using(CustomComparers.TypeComparison));

			var controllingMessageHeader2 = controllingMessageHeaders.AddNew();
			controllingMessageHeader2.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.NX601).IsLinkedCMHeader = false;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.NX603).IsLinkedCMHeader = true;
			NUnit.Framework.Assert.That(manufacturer.ID, NUnit.Framework.Is.EqualTo("0123456789").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddressInfo()
		{
			NUnit.Framework.Assert.That(manufacturer.Address.CountrySubDivisionID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(manufacturer.Address.CountrySubDivisionName, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader1 = controllingMessageHeaders.AddNew();
			controllingMessageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.NX601).IsLinkedCMHeader = true;
			NUnit.Framework.Assert.That(manufacturer.Address.CountrySubDivisionID, NUnit.Framework.Is.EqualTo("NSW").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(manufacturer.Address.CountrySubDivisionName, NUnit.Framework.Is.EqualTo("NSW").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			NUnit.Framework.Assert.That(manufacturer.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ICommunication>)));

			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader1 = controllingMessageHeaders.AddNew();
			controllingMessageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.NX601).IsLinkedCMHeader = true;
			NUnit.Framework.Assert.That(manufacturer.Communications.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.MessageType == ControllingMessageTypeList.Codes.NX601).IsLinkedCMHeader = true;
			manufacturerAddress.E2_Contact = "John Smith";
			NUnit.Framework.Assert.That(manufacturer.ContactName, NUnit.Framework.Is.EqualTo("John Smith").Using(CustomComparers.TypeComparison));

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			NUnit.Framework.Assert.That(manufacturer.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty), "Should be empty when CMHeader MessageType is not NX601");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			manufacturerAddress = invoiceLine.ManufacturerDocAddress;
			var address = new TestTWCreator(Factory).CreateOrganizationForManufacturer().MainAddress;
			address.StateCode = "NSW";
			address.OA_Address1 = "ADDRESS 1";
			address.OA_Address2 = "ADDRESS 2";
			address.OA_Phone = "0987888888";
			address.OA_Email = "test@wisetechglobal.com";
			manufacturerAddress.E2_OA_Address = address.PK;
		}

		IPartyDetails manufacturer => new NX5105CMManufacturerWrapper(manufacturerAddress, invoiceLine);
		TWJobDocAddress manufacturerAddress;
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
	}
}
