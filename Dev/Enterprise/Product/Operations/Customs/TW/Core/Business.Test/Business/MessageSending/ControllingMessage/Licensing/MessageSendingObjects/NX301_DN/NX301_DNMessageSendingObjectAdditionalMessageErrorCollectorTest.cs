using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301_DNMessageSendingObjectAdditionalMessageErrorCollector))]
	sealed class NX301_DNMessageSendingObjectAdditionalMessageErrorCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestValidateDeclarant()
		{
			var localCompanyNameMessageError = "Declarant: You have not entered a Declarant Local Company Name.";
			var localAddressMessageError = "Declarant: You have not entered a Declarant Local Address.";
			var phoneMessageError = "Declarant: You have not entered a Declarant Telephone Number.";
			var emailMessageError = "Declarant: You have not entered a Declarant Email Address.";

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "not entered a Declarant");
				NUnit.Framework.Assert.That(!collector.Contains(localAddressMessageError), NUnit.Framework.Is.True, "not entered a Declarant");
				NUnit.Framework.Assert.That(!collector.Contains(phoneMessageError), NUnit.Framework.Is.True, "not entered a Declarant");
				NUnit.Framework.Assert.That(!collector.Contains(emailMessageError), NUnit.Framework.Is.True, "not entered a Declarant");
			});

			var declarantOrg = Factory.New<OrgHeader>();
			var declarantAddress = declarantOrg.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "not entered a Declarant Local Company Name.");
				NUnit.Framework.Assert.That(collector.Contains(localAddressMessageError), NUnit.Framework.Is.True, "not entered a Declarant Local Address.");
				NUnit.Framework.Assert.That(collector.Contains(phoneMessageError), NUnit.Framework.Is.True, "not entered a Declarant Telephone Number.");
				NUnit.Framework.Assert.That(collector.Contains(emailMessageError), NUnit.Framework.Is.True, "not entered a Declarant Email Address.");
			});

			var declarantLocalAddress = declarantAddress.TranslatedAddresses.AddNew();
			declarantLocalAddress.Language = Core.SharedConstants.Languages.ChineseTraditional;
			declarantLocalAddress.Address1 = "TestAddress";
			declarantLocalAddress.CompanyName = "TestCompany";
			declarantAddress.OA_Phone = "1111111111";
			declarantAddress.OA_Email = "wys@wisetechglobalc.com";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "entered a Declarant Local Company Name.");
				NUnit.Framework.Assert.That(!collector.Contains(localAddressMessageError), NUnit.Framework.Is.True, "entered a Declarant Local Address.");
				NUnit.Framework.Assert.That(!collector.Contains(phoneMessageError), NUnit.Framework.Is.True, "entered a Declarant Telephone Number.");
				NUnit.Framework.Assert.That(!collector.Contains(emailMessageError), NUnit.Framework.Is.True, "entered a Declarant Email Address.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			sendingObjectParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX301_DN);
			collector = new NX301_DNMessageSendingObjectAdditionalMessageErrorCollector(sendingObjectParent);
		}

		JobDeclaration declaration;
		LicensingMessageSendingObjectParent sendingObjectParent;
		NX301_DNMessageSendingObjectAdditionalMessageErrorCollector collector;
	}
}
