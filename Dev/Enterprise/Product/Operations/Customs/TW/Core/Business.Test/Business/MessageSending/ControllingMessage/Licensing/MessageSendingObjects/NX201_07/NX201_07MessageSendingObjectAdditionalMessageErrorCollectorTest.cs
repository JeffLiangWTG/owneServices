using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_07MessageSendingObjectAdditionalMessageErrorCollector))]
	sealed class NX201_07MessageSendingObjectAdditionalMessageErrorCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestValidateDeclarant()
		{
			var localCompanyNameMessageError = "Declarant: You have not entered a Declarant Local Company Name.";
			var localAddressMessageError = "Declarant: You have not entered a Declarant Local Address.";

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "not entered a Declarant");
				NUnit.Framework.Assert.That(!collector.Contains(localAddressMessageError), NUnit.Framework.Is.True, "not entered a Declarant");
			});

			var declarantOrg = Factory.New<OrgHeader>();
			var declarantAddress = declarantOrg.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "not entered a Declarant Local Company Name.");
				NUnit.Framework.Assert.That(collector.Contains(localAddressMessageError), NUnit.Framework.Is.True, "not entered a Declarant Local Address.");
			});

			var declarantLocalAddress = declarantAddress.TranslatedAddresses.AddNew();
			declarantLocalAddress.Language = Core.SharedConstants.Languages.ChineseTraditional;
			declarantLocalAddress.Address1 = "TestAddress";
			declarantLocalAddress.CompanyName = "TestCompany";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "entered a Declarant Local Company Name.");
				NUnit.Framework.Assert.That(!collector.Contains(localAddressMessageError), NUnit.Framework.Is.True, "entered a Declarant Local Address.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_07;
			sendingObjectParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX201_07);
			collector = new NX201_07MessageSendingObjectAdditionalMessageErrorCollector(sendingObjectParent);
		}

		JobDeclaration declaration;
		LicensingMessageSendingObjectParent sendingObjectParent;
		NX201_07MessageSendingObjectAdditionalMessageErrorCollector collector;
	}
}
