using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer;
using NUnit.Framework;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	public sealed class NCATKDataObjectWriterConfigurationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			header.TW1_ControllingAgency = "FT";
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			header.TW1_FunctionalReferenceId = "C";
			var configuration = new NCATKDataObjectWriterConfiguration(header, null);
			var pks = configuration.CAHeaderPKsToPopulate;
			NUnit.Framework.Assert.That(pks.Count(), Is.EqualTo(1));
			NUnit.Framework.Assert.That(pks.Any(x => x == header.PK), Is.True);
			pks = configuration.EntryHeaderPKsToPopulate;
			NUnit.Framework.Assert.That(!pks.Any(), Is.True);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = "X101";
			var wrapper = new NCATKMessageSendingObjectParent(declaration, "X101");
			configuration = new NCATKDataObjectWriterConfiguration(messageHeader, wrapper);
			pks = configuration.CAHeaderPKsToPopulate;
			NUnit.Framework.Assert.That(pks.Count(), Is.EqualTo(1));
			NUnit.Framework.Assert.That(pks.Any(x => x == messageHeader.PK), Is.True);
			pks = configuration.EntryHeaderPKsToPopulate;
			NUnit.Framework.Assert.That(pks.Count(), Is.EqualTo(1));
			NUnit.Framework.Assert.That(pks.Any(x => x == entryHeader.PK), Is.True);
		}
	}
}
