using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	public sealed class TWInvoiceLineAdditionalAddInfoGroupCollectionDataObjectWriterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateCollection()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
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
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";
			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			NUnit.Framework.Assert.That(invoiceLineLinkControllingMsgHeaders.Count, Is.EqualTo(3));
			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;
			var helper = new TWDataObjectWriterHelper(Factory);
			var writer = new TWInvoiceLineAdditionalAddInfoGroupCollectionDataObjectWriter(line, helper);
			var collection = writer.CreateCollection();
			NUnit.Framework.Assert.That(!collection.Any(), Is.True);
			helper.AllocateControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[0].ControllingMessageHeaderPK);
			collection = writer.CreateCollection();
			NUnit.Framework.Assert.That(collection.Count(), Is.EqualTo(1));
			var first = collection.First();
			NUnit.Framework.Assert.That(first.Type.Code, Is.EqualTo("CML").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(first.Type.Description, Is.EqualTo("Controlling Message Links").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(first.AddInfoCollection.Count, Is.EqualTo(1));
			var addInfo = first.AddInfoCollection.First();
			NUnit.Framework.Assert.That(addInfo.Key, Is.EqualTo("ControllingMessageLink").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(addInfo.Value, Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			helper.AllocateControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[1].ControllingMessageHeaderPK);
			helper.AllocateControllingMessageHeaderLink(invoiceLineLinkControllingMsgHeaders[2].ControllingMessageHeaderPK);
			collection = writer.CreateCollection();
			NUnit.Framework.Assert.That(collection.Count(), Is.EqualTo(1));
			first = collection.First();
			NUnit.Framework.Assert.That(first.Type.Code, Is.EqualTo("CML").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(first.Type.Description, Is.EqualTo("Controlling Message Links").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(first.AddInfoCollection.Count, Is.EqualTo(3));
			addInfo = first.AddInfoCollection[0];
			NUnit.Framework.Assert.That(addInfo.Key, Is.EqualTo("ControllingMessageLink").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(addInfo.Value, Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			addInfo = first.AddInfoCollection[1];
			NUnit.Framework.Assert.That(addInfo.Key, Is.EqualTo("ControllingMessageLink").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(addInfo.Value, Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			addInfo = first.AddInfoCollection[2];
			NUnit.Framework.Assert.That(addInfo.Key, Is.EqualTo("ControllingMessageLink").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(addInfo.Value, Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = false;
			collection = writer.CreateCollection();
			NUnit.Framework.Assert.That(collection.Count(), Is.EqualTo(1));
			first = collection.First();
			NUnit.Framework.Assert.That(first.Type.Code, Is.EqualTo("CML").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(first.Type.Description, Is.EqualTo("Controlling Message Links").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(first.AddInfoCollection.Count, Is.EqualTo(2));
			addInfo = first.AddInfoCollection[0];
			NUnit.Framework.Assert.That(addInfo.Key, Is.EqualTo("ControllingMessageLink").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(addInfo.Value, Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			addInfo = first.AddInfoCollection[1];
			NUnit.Framework.Assert.That(addInfo.Key, Is.EqualTo("ControllingMessageLink").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(addInfo.Value, Is.EqualTo("3").Using(CustomComparers.TypeComparison));
		}
	}
}
