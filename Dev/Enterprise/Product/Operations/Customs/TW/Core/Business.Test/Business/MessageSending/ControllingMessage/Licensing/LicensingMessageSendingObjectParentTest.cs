using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(LicensingMessageSendingObjectParent))]
	abstract class LicensingMessageSendingObjectParentAbstractTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return LicensingMessageSendingObjectParentForTesting;
		}

		[ExpectNoExceptions]
		public void TestSendingObjectsCollectionType()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SendingObjectsCollection, NUnit.Framework.Is.TypeOf<LicensingMessageSendingObjectCollection>());
		}

		[ExpectNoExceptions]
		public void TestSecurityRightToSendWithMessageErrors()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SecurityCheckpointToSendWithMessageError, NUnit.Framework.Is.EqualTo(Env.Security.CustomsDeclarationSendWithMessageErrors));
		}

		[ExpectNoExceptions]
		public void TestMessageSendingObjectParentDefaultValues()
		{
			var testWrapper = LicensingMessageSendingObjectParentForTesting;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testWrapper, NUnit.Framework.Is.Not.EqualTo(default(LicensingMessageSendingObjectParent)));
				NUnit.Framework.Assert.That(!testWrapper.BizObjValidationMessageErrors.IsEmpty, NUnit.Framework.Is.True, "Wrapper should have error messages because of many necessary information is empty defaultly");
				NUnit.Framework.Assert.That(testWrapper.SendingObjectsCollection.Count, NUnit.Framework.Is.EqualTo(1), "Wrapper should have 1 SendingObjects by default");
			});
		}

		[ExpectNoExceptions]
		public void TestShowReasonDescription()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.ShowReasonDescription, NUnit.Framework.Is.EqualTo(ExpectedShowReasonDescription), "Default value");
		}

		[ExpectNoExceptions]
		public void TestIsSupportingDocumentsNeededMessage()
		{
			if (ExpectedIsSupportingDocumentsNeededMessage)
			{
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.IsSupportingDocumentsNeededMessage, NUnit.Framework.Is.True, $"{LicensingMessageSendingObjectParentForTesting.MessageType} message is a SupportingDocuments needed message");
			}
			else
			{
				NUnit.Framework.Assert.That(!LicensingMessageSendingObjectParentForTesting.IsSupportingDocumentsNeededMessage, NUnit.Framework.Is.True, $"{LicensingMessageSendingObjectParentForTesting.MessageType} message is not a SupportingDocuments needed message");
			}
		}

		protected virtual bool ExpectedIsSupportingDocumentsNeededMessage => true;

		protected virtual bool ExpectedShowReasonDescription => false;

		protected abstract LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting();

		protected LicensingMessageSendingObjectParent LicensingMessageSendingObjectParentForTesting => licensingMessageSendingObjectParentForTesting ??= CreateLicensingMessageSendingObjectParentForTesting();
		LicensingMessageSendingObjectParent licensingMessageSendingObjectParentForTesting;
	}

	[TestedType(typeof(LicensingMessageSendingObjectParent))]
	sealed class LicensingMessageSendingObjectParentBaseOnlyTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectParentTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX101), NUnit.Framework.Is.TypeOf<NX101LicensingMessageSendingObjectParent>(), "NX101");
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX201_01), NUnit.Framework.Is.TypeOf<NX201_01LicensingMessageSendingObjectParent>(), "NX201_01");
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX201_07), NUnit.Framework.Is.TypeOf<NX201_07LicensingMessageSendingObjectParent>(), "NX201_07");
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX301), NUnit.Framework.Is.TypeOf<NX301LicensingMessageSendingObjectParent>(), "NX301");
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX301_AX), NUnit.Framework.Is.TypeOf<NX301_AXLicensingMessageSendingObjectParent>(), "NX301_AX");
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX301_DN), NUnit.Framework.Is.TypeOf<NX301_DNLicensingMessageSendingObjectParent>(), "NX301_DN");
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX401), NUnit.Framework.Is.TypeOf<NX401LicensingMessageSendingObjectParent>(), "NX401");
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX601), NUnit.Framework.Is.TypeOf<NX601LicensingMessageSendingObjectParent>(), "NX601");
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX603), NUnit.Framework.Is.TypeOf<NX603LicensingMessageSendingObjectParent>(), "NX603");
				NUnit.Framework.Assert.That(LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, "NXXX"), NUnit.Framework.Is.EqualTo(default(LicensingMessageSendingObjectParent)), "Should be null - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestGetNewMessageErrorCollector()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var messageHeader1 = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var messageHeader2 = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader2.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var messageHeader3 = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader3.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			(messageHeader1.Validation as IValidationInternals).Validate(messageHeader1.TW1_ProcessingUnitInfo, () => messageHeader1.TW1_ProcessingUnitInfo.AddMessageError("Test Message Error For messageHeader1"));
			(messageHeader2.Validation as IValidationInternals).Validate(messageHeader2.TW1_ProcessingUnitInfo, () => messageHeader2.TW1_ProcessingUnitInfo.AddMessageError("Test Message Error For messageHeader2"));
			(messageHeader3.Validation as IValidationInternals).Validate(messageHeader3.TW1_ProcessingUnitInfo, () => messageHeader3.TW1_ProcessingUnitInfo.AddMessageError("Test Message Error For messageHeader3"));
			var invoiceHeader = decl.Invoices.AddNew();
			var invoiceLine = decl.InvoiceLines.AddNew();
			(invoiceLine.Validation as IValidationInternals).Validate(invoiceLine.JI_TextileWidthInfo, () => invoiceLine.JI_TextileWidthInfo.AddMessageError("Test Message Error For invoiceLine.JI_TextileWidth"));
			(invoiceLine.Validation as IValidationInternals).Validate(invoiceLine.JI_TextileWidthUQInfo, () => invoiceLine.JI_TextileWidthUQInfo.AddMessageError("Test Message Error For invoiceLine.JI_TextileWidthUQ"));

			CombineAssertions("Check Preconditions", () =>
			{
				AssertHasMessageError(invoiceLine.JI_TextileWidthInfo, "Test Message Error For invoiceLine.JI_TextileWidth");
				AssertHasMessageError(invoiceLine.JI_TextileWidthUQInfo, "Test Message Error For invoiceLine.JI_TextileWidthUQ");
			});

			var parent = new NX101LicensingMessageSendingObjectParentTest.NX101LicensingMessageSendingObjectParentForTest(decl);
			parent.SendingObjectsCollection.Cast<LicensingMessageSendingObject>().FirstOrDefault(x => x.Header.PK == messageHeader1.PK).ShouldSend = true;
			parent.SendingObjectsCollection.Cast<LicensingMessageSendingObject>().FirstOrDefault(x => x.Header.PK == messageHeader2.PK).ShouldSend = false;

			var messageErrors = string.Join("\r\n", parent.ExposedMessageErrorCollector().Select(messageError => messageError.Message));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageErrors, NUnit.Framework.Does.Contain("Test Message Error For messageHeader1"), "Should include message errors from CusTWControllingMessageHeader which need to be sent");
				NUnit.Framework.Assert.That(messageErrors, NUnit.Framework.Does.Not.Contain("Test Message Error For messageHeader2"), "Should not include message errors from CusTWControllingMessageHeader which ShouldSend is false");
				NUnit.Framework.Assert.That(messageErrors, NUnit.Framework.Does.Not.Contain("Test Message Error For messageHeader3"), "Should not include message errors from CusTWControllingMessageHeader which not need to be sent");
				NUnit.Framework.Assert.That(messageErrors, NUnit.Framework.Does.Not.Contain("Test Message Error For invoiceLine.JI_TextileWidth"), "Should not include message errors of invoice lines' JI_TextileWidth");
				NUnit.Framework.Assert.That(messageErrors, NUnit.Framework.Does.Not.Contain("Test Message Error For invoiceLine.JI_TextileWidthUQ"), "Should not include message errors of invoice lines' JI_TextileWidthUQ");
			});

			decl.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			decl.JE_OH_Consignee = ZGuid.Empty;
			var declarantOrg = Factory.New<OrgHeader>();
			var declarantAddress = declarantOrg.Addresses.AddNew();
			decl.JE_OA_DeclarantAddress = declarantAddress.PK;
			CombineAssertions($"NX101 Additional MessageError", () =>
			{
				NUnit.Framework.Assert.That(parent.ExposedMessageErrorCollector().Contains("Declarant: You have not entered a Declarant Local Company Name."), NUnit.Framework.Is.True, "Message 1");
				NUnit.Framework.Assert.That(parent.ExposedMessageErrorCollector().Contains("Declarant: You have not entered a Declarant Local Address."), NUnit.Framework.Is.True, "Message 2");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalWarnings()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTestSendingObject>();
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			declaration.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("MessageType Add Warning");
			};
			var sendingObjParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX101);
			var messageSendingObject = sendingObjParent.SendingObjectsCollection.Cast<LicensingMessageSendingObject>().FirstOrDefault();
			declaration.RunPreSaveValidation();
			NUnit.Framework.Assert.That(sendingObjParent.AdditionalWarnings.ToString(), NUnit.Framework.Does.Contain("MessageType Add Warning"));
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			return LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, controllingMessageHeader.TW1_ControllingMessageType);
		}

		protected override bool ExpectedIsSupportingDocumentsNeededMessage => false;
	}
}
