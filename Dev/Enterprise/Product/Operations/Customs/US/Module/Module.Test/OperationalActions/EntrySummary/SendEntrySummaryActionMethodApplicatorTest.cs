using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendEntrySummaryActionMethodApplicator))]
	sealed class SendEntrySummaryActionMethodApplicatorTest : Services.OperationalActions.Support.Testing.OperationalActionMethodApplicatorTest
	{
		public void TestValidationMode()
		{
			AssertEquals(Applicator.ValidationMode, ValidationModes.EntrySummary);
		}

		public void TestCheckContactInfo()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.JE_DeclarationReference = "B000006";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PSC = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			Factory.Save();
			var targets = new BusinessObject[] { declaration };
			var log = SimulateRun(targets, false);
			var applicator = Applicator;
			applicator.ContactName = "TST";
			applicator.Validation.ValidateContactName();
			AssertNoMessageErrorContaining(applicator.ContactNameInfo, "You have not entered a value");
			applicator.ContactName = "";
			applicator.Validation.ValidateContactName();
			AssertHasMessageErrorContaining(applicator.ContactNameInfo, "You have not entered a value");
			applicator.ContactPhone = "abc";
			applicator.Validation.ValidateContactPhone();
			AssertHasMessageErrorContaining(applicator.ContactPhoneInfo, SendEntrySummaryActionMethodApplicatorValidation.InvalidContactPhoneMessage);
			applicator.ContactPhone = "";
			applicator.Validation.ValidateContactPhone();
			AssertHasMessageErrorContaining(applicator.ContactPhoneInfo, "You have not entered a value");
			applicator.ContactPhone = "123456789";
			applicator.Validation.ValidateContactPhone();
			AssertNoMessageErrorContaining(applicator.ContactPhoneInfo, "You have not entered a value");
		}

		public void TestSummaryLog()
		{
			var helper = new Business.Testing.DeclarationTestHelper();
			var declaration1 = helper.GetMergedDutiableDeclarationForACE(Factory);
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var declaration2 = helper.GetMergedDutiableDeclarationForACE(Factory);
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Factory.Save();
			Applicator.SendWithMessageErrors = true;
			Applicator.ContactName = "KNZTest";
			Applicator.ContactPhone = "124578";
			AssertEquals(true, Applicator.SendWithMessageErrors);
			var targets = new BusinessObject[] { declaration1, declaration2 };
			var log = SimulateRun(targets, false);
			Assert(log.MessagesString().Contains("INFO: Message for declaration [HL B00001001] has been sent."));
			Assert(log.MessagesString().Contains("INFO: Message for declaration [HL B00001000] has been sent."));
			Assert(log.MessagesString().Contains("INFO: Click the above declaration(s) to see the message."));
		}

		public void TestApportionWeight()
		{
			var helper = new Business.Testing.DeclarationTestHelper();
			var declaration1 = helper.GetMergedDutiableDeclarationForACE(Factory);
			declaration1.JE_AutoWeightApportion = false;
			declaration1.JE_TotalWeight = 1000m;
			declaration1.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Factory.Save();
			Applicator.SendWithMessageErrors = true;
			Applicator.ApportionWeight = true;
			Applicator.ContactName = "KNZTest";
			Applicator.ContactPhone = "124578";
			AssertEquals(true, Applicator.SendWithMessageErrors);
			var targets = new BusinessObject[] { declaration1 };
			var log = SimulateRun(targets, false);
			Assert("AutoWeightApportion should set to true", declaration1.JE_AutoWeightApportion);
			AssertEquals("Invoice Weight", 1000m, declaration1.Invoices[0].JZ_Weight);
			AssertEquals("Invoice WeightUQ", Core.Constants.Weight.Kilograms, declaration1.Invoices[0].JZ_WeightUQ);
		}

		public void TestSetDefaultValues()
		{
			var contact = Factory.NewWithValidTestData<GlbStaff>();
			contact.GS_FullName = "VEEERRRRYYYY LOOOOOONNNNNNNNGGGGGG NAAAAMMMMMMEEEEEE";
			contact.GS_WorkPhone = "1234567890";
			Factory.Save();
			using (USCustomsDataRegistry.Instance.CargoReleaseFTZContact.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, contact.PK.ToGuid()))
			{
				var applicator = new SendEntrySummaryActionMethodApplicator(new BusinessObjectFactory());
				AssertEquals("VEEERRRRYYYY LOOOOOONNNNNNNNGGGGGG NAAAA", applicator.ContactName);
				AssertEquals("1234567890", applicator.ContactPhone);
			}
		}

		new SendEntrySummaryActionMethodApplicator Applicator => (SendEntrySummaryActionMethodApplicator)base.Applicator;
	}
}
