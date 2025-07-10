using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EmailSenderConfiguration))]
	sealed class EmailSenderConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocMangerInfoFactoryShouldBeBusinessFactory()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "TST";
			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test";
			trigger.TriggerConditions.TriggerEventCode = Events.Assigned.Code;
			factory.Save();

			var emailSenderConfiguration = new EmailSenderConfiguration(orgHeader);
			var orgHeader2 = emailSenderConfiguration.ContextItemSource.DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs.Load<OrgHeader>(orgHeader.PK);
			var workflowItemsTrigger = orgHeader2.WorkflowItems.Triggers[0];
			workflowItemsTrigger.P9_Description = "TestV2";
			trigger.P9_Description = "TestV3";

			AssertNoExceptionThrown(() => BusinessObjectFactory.SaveTogether(new[] { factory,  emailSenderConfiguration.ContextItemSource.DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs }));
		}

		public void TestUseBusinessEntityFactoryAsInternalShouldBeTrue()
		{
			var sup = (ISendEmailSource)Factory.New<OrgHeader>();
			Assert(!sup.DocManagerInfo.UseBusinessEntityFactoryAsInternal);
			var configuration = new EmailSenderConfiguration(sup);
			_ = configuration.EDocsDocumentTypes;
			Assert(sup.DocManagerInfo.UseBusinessEntityFactoryAsInternal);
		}

		#region BusinessObjectOverrides

		protected override BusinessObject GetNewBusinessObject()
		{
			ISendEmailSource sup = Factory.New<OrgHeader>();
			return new EmailSenderConfiguration(sup);
		}

		#endregion

		public void TestEDocsDocumentTypesShouldNotBeIncluded()
		{
			var eDocsDocumentTypesProperty = typeof(EmailSenderConfiguration).GetProperty("EDocsDocumentTypes");
			var attribute = eDocsDocumentTypesProperty.GetAttribute<DocumentFieldExcludeFromMapAttribute>();
			AssertNotNull(attribute);
		}

		public void TestCtor()
		{
			ISendEmailSource sup = Factory.New<OrgHeader>();
			EmailSenderConfiguration obj = new EmailSenderConfiguration(sup);
			AssertEquals("Parent object", sup, obj.ContextItemSource);
		}

		public void TestDeliveryMethod_SetsTemplateAndMandatoryBodyFlag()
		{
			ISendEmailSource sup = Factory.New<OrgHeader>();
			EmailSenderConfiguration obj = new EmailSenderConfiguration(sup);
			ZGuid randomGuid = ZGuid.NewZGuid();
			obj.HtmlEmail.Template = randomGuid;
			AssertEquals("Precondition", randomGuid, obj.HtmlEmail.Template);
			AssertEquals("Precondition", true, obj.HtmlEmail.FilledBodyIsMandatory);

			obj.DeliveryMethod = MessageDeliveryMethod.EMail;
			AssertEquals("MessageDeliveryMethod.EMail - Unchanged Template", randomGuid, obj.HtmlEmail.Template);
			AssertEquals("MessageDeliveryMethod.EMail - Unchanged Mandatory Body Flag", true, obj.HtmlEmail.FilledBodyIsMandatory);

			obj.DeliveryMethod = MessageDeliveryMethod.EMailViaMailClient;
			AssertEquals("MessageDeliveryMethod.EMailViaMailClient - Unchanged Template", Guid.Empty, obj.HtmlEmail.Template);
			AssertEquals("MessageDeliveryMethod.EMailViaMailClient - Unchanged Mandatory Body Flag", false, obj.HtmlEmail.FilledBodyIsMandatory);
		}

		public void TestGetEmail_SetsBusinessEntityOnEmailIfSourceIsOne()
		{
			var org = Factory.New<OrgHeader>();
			var emailSenderConfig = new EmailSenderConfiguration(org);
			var email = emailSenderConfig.GetEmail();
			AssertEquals(org.PK, email.BusinessEntityID);
			AssertEquals(org.TablePrefix, email.BusinessEntityTableCode);

			var nonBizObjSource = new DummySendEmailSource();
			nonBizObjSource.AddressBookSelectionForTest = new AddressBookSelection();
			emailSenderConfig = new EmailSenderConfiguration(nonBizObjSource);
			email = emailSenderConfig.GetEmail();
			AssertEquals(ZGuid.Empty, email.BusinessEntityID);
			AssertEquals(string.Empty, email.BusinessEntityTableCode);
		}

		public void TestValidateSelectedRecipientsCount()
		{
			BO.HtmlEmail.ToEmailAddress = "mail@mail.com";
			BO.ValidateSelectedRecipientsCount();
			AssertEquals(false, BO.SelectedRecipientsCountInfo.HasErrors());

			BO.HtmlEmail.ToEmailAddress = "";
			BO.ValidateSelectedRecipientsCount();
			AssertEquals(true, BO.SelectedRecipientsCountInfo.HasErrors());

			BO.HtmlEmail.ToEmailAddress = "mail@mail.com";
			BO.RunPreSaveValidation();
			AssertEquals("RunPreSaveValidation should call ValidateSelectedRecipientsCount", false, BO.SelectedRecipientsCountInfo.HasErrors());
		}

		public void TestValidateSaveToEDocs()
		{
			BO.DeliveryMethod = MessageDeliveryMethod.EMail;
			AssertEquals("Precondition: CanSaveToEDocs", true, BO.CanSaveToEDocs);
			BO.RunPreSaveValidation();
			Assert("Precondition: no errors", !BO.SaveToEDocsDocumentTypeInfo.HasErrors());

			BO.SaveToEDocsDocumentType = "ZZZ";
			BO.RunPreSaveValidation();
			Assert("ZZZ does not exist. Invalid", BO.SaveToEDocsDocumentTypeInfo.HasErrors());

			BO.SaveToEDocsDocumentType = "MSC";
			BO.RunPreSaveValidation();
			Assert("MSC is category ALL, system document type. Valid", !BO.SaveToEDocsDocumentTypeInfo.HasErrors());

			RefDocType docTypeSCL = Factory.New<RefDocType>();
			docTypeSCL.RT_DocType = "UD1";
			docTypeSCL.RT_ReferenceType = Constants.ReferenceTypes.SupplyChainLogistics;

			RefDocType docTypeCSR = Factory.New<RefDocType>();
			docTypeCSR.RT_DocType = "EML";
			docTypeCSR.RT_ReferenceType = Constants.ReferenceTypes.ClientSupplierRelationship;
			Factory.Save();
			EmailSenderConfiguration bOCSR = new EmailSenderConfiguration(fOrg);
			AssertEquals("Precondition: CanSaveToEDocs", true, bOCSR.CanSaveToEDocs);

			bOCSR.SaveToEDocsDocumentType = "UD1";
			bOCSR.RunPreSaveValidation();
			Assert("UD1 is category SCL. Invalid", bOCSR.SaveToEDocsDocumentTypeInfo.HasErrors());

			bOCSR.SaveToEDocsDocumentType = "EML";
			bOCSR.RunPreSaveValidation();
			Assert("EML is category CSR. Valid", !bOCSR.SaveToEDocsDocumentTypeInfo.HasErrors());
		}

		#region Implementation

		readonly BusinessObjectFactory fFactory = new BusinessObjectFactory();
		EmailSenderConfiguration BO;
		OrgHeader fOrg;

		protected override void SetUp()
		{
			base.SetUp();
			fOrg = fFactory.New<OrgHeader>();
			fOrg.OH_Code = "---";
			fOrg.OH_FullName = "fullname";
			fOrg.MainAddress.OA_Address1 = "addr";

			BO = new EmailSenderConfiguration(fOrg);
			BO.DeliveryMethod = MessageDeliveryMethod.EMailViaMailClient;

			OrgContact newContact1 = fOrg.Contacts.AddNew();
			OrgContact newContact = fOrg.Contacts.AddNew();
		}

		#endregion
	}
}
