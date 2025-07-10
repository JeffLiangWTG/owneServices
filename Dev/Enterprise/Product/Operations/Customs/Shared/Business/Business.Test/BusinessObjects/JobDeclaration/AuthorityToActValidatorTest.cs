using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AuthorityToActValidatorTest : TestCaseWithFactory
	{
		public void TestValidateWithCalledSuppliedNotification()
		{
			var importer = Factory.New<OrgHeader>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			using (declaration.SuspendValidationTesting())
			{
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				var a2av = new AuthorityToActValidator("", "This is the overrrden notification message that should be displayed as a warning or error as there is no POA for this organisation.");
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding, new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["POA3", "6354"] != null, "'US' Country and 'EXP' Direction") });
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, "This is the overrrden notification message that should be displayed as a warning or error as there is no POA for this organisation.");
			}
		}

		public void TestHasAnyPowerOfAttorney()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			string[] realPowerOfAttorneyCodes = { Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding };
			string[] fakePowerOfAttorneyCodes = { "THI", "SIS", "NOT" };

			using (declaration.SuspendValidationTesting())
			{
				var importer = Factory.New<OrgHeader>();
				var poaDocument1 = importer.RequiredDocuments.AddNew("POA");
				poaDocument1.EQ_DocType = "POA";
				poaDocument1.EQ_DocDescription = "Power of Attorney";
				poaDocument1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument1.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument1.EQ_ValidToDate = ZDateTime.Today.AddMonths(-1);

				var poaDocument2 = importer.RequiredDocuments.AddNew("POC");
				poaDocument2.EQ_DocType = "POC";
				poaDocument2.EQ_DocDescription = "Power of Attorney Customs";
				poaDocument2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument2.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument2.EQ_ValidToDate = ZDateTime.Today.AddYears(1);

				var poaDocument3 = importer.RequiredDocuments.AddNew("POF");
				poaDocument3.EQ_DocType = "POF";
				poaDocument3.EQ_DocDescription = "Power of Attorney Forwarding";
				poaDocument3.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument3.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument3.EQ_ValidToDate = ZDateTime.Today.AddDays(10);

				declaration.JE_OH_Importer = importer.PK;

				var a2av = new AuthorityToActValidator("Puissance d'Avoue", "");

				AssertEquals(true, a2av.HasAnyPowerOfAttorney(importer.RequiredDocuments, realPowerOfAttorneyCodes));
				AssertEquals(false, a2av.HasAnyPowerOfAttorney(importer.RequiredDocuments, fakePowerOfAttorneyCodes));
			}
		}

		public void TestHasValidPOAWithNoExtraMatch()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (declaration.SuspendValidationTesting())
			{
				var importer = Factory.New<OrgHeader>();
				var poaDocument1 = importer.RequiredDocuments.AddNew("POA");
				poaDocument1.EQ_DocType = "POA";
				poaDocument1.EQ_DocDescription = "Power of Attorney";
				poaDocument1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument1.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument1.EQ_ValidToDate = ZDateTime.Today.AddMonths(-1);
				var poaDocument1Attrib1 = poaDocument1.Attributes.AddNew();
				poaDocument1Attrib1.D0_AttribName = "POA3";

				var poaDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);
				var poaDocAttribute = poaDocument.Attributes.AddNew();
				poaDocAttribute.D0_AttribName = "POA";

				declaration.JE_OH_Importer = importer.PK;
				var countrySpecificNameForPOA = "WHO HAS THE POWER";
				var a2av = new AuthorityToActValidator(countrySpecificNameForPOA, "");
				AssertNoExceptionThrown(() =>
				{
					AssertEquals(true, a2av.HasValidPOA(declaration, importer, new string[] { "POA" }, (x) => true));
				});
			}
		}

		public void TestValidateWithExtraMatch()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			using (declaration.SuspendValidationTesting())
			{
				var importer = Factory.New<OrgHeader>();
				var poaDocument1 = importer.RequiredDocuments.AddNew("POA");
				poaDocument1.EQ_DocType = "POA";
				poaDocument1.EQ_DocDescription = "Power of Attorney";
				poaDocument1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument1.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument1.EQ_ValidToDate = ZDateTime.Today.AddMonths(-1);

				var poaDocument2 = importer.RequiredDocuments.AddNew("POA");
				poaDocument2.EQ_DocType = "POA";
				poaDocument2.EQ_DocDescription = "Power of Attorney";
				poaDocument2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument2.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument2.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
				var poaDocument2Attrib1 = poaDocument2.Attributes.AddNew();
				poaDocument2Attrib1.D0_AttribName = "POA2";
				poaDocument2Attrib1.D0_AttribValue = "2342";

				var poaDocument3 = importer.RequiredDocuments.AddNew("POA");
				poaDocument3.EQ_DocType = "POA";
				poaDocument3.EQ_DocDescription = "Power of Attorney";
				poaDocument3.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument3.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument3.EQ_ValidToDate = ZDateTime.Today.AddDays(10);
				var poaDocument3Attrib1 = poaDocument3.Attributes.AddNew();
				poaDocument3Attrib1.D0_AttribName = "POA3";
				poaDocument3Attrib1.D0_AttribValue = "6354";

				declaration.JE_OH_Importer = importer.PK;

				var countrySpecificNameForPOA = "WHO HAS THE POWER";
				var documentOwner = "organization (eDocs > Document Tracking)";
				var a2av = new AuthorityToActValidator(countrySpecificNameForPOA, "");

				var pOAWillExpireSoon = AuthorityToActValidator.GetPOAWillExpireSoonString(ZDate.Today.AddDays(10).ToString(), documentOwner, countrySpecificNameForPOA);
				var pOAExpired = AuthorityToActValidator.GetPOAExpiredString(ZDate.Today.AddMonths(-1).ToString(), documentOwner, countrySpecificNameForPOA);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["POA3", "6354"] != null, "") });
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["POA3", "2342"] != null, "") });
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["POA2", "2342"] != null, "") });
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["POA3", "6354"] != null, "") });
				AssertHasWarning(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["POA3", "2342"] != null, "") });
				AssertNoWarning(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["POA2", "2342"] != null, "") });
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, pOAExpired);
			}
		}

		public void TestValidateNotAllowPOFAlone()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			using (declaration.SuspendValidationTesting())
			{
				var importer = Factory.New<OrgHeader>();
				var poaDocument1 = importer.RequiredDocuments.AddNew("POF");
				poaDocument1.EQ_DocType = "POF";
				poaDocument1.EQ_DocDescription = "Power of Attorney Forwarding";
				poaDocument1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument1.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
				poaDocument1.EQ_ValidToDate = ZDate.Today.AddMonths(1);

				var countrySpecificNameForPOA = "Power of Attorney";
				var a2av = new AuthorityToActValidator(countrySpecificNameForPOA, "");
				var noPOA = AuthorityToActValidator.GetNoPOADocumentForImporterString(new AuthorityToActValidator().CountrySpecificNameForPOA);

				declaration.JE_OH_Importer = importer.PK;
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC");
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, noPOA);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC");
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, noPOA);

				var poaDocument2 = importer.RequiredDocuments.AddNew("POA");
				poaDocument2.EQ_DocType = "POA";
				poaDocument2.EQ_DocDescription = "Power of Attorney";
				poaDocument2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument2.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
				poaDocument1.EQ_ValidToDate = ZDate.Today.AddMonths(1);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC");
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, noPOA);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC");
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, noPOA);
			}
		}

		public void TestNotifyOnlyAboutTheMostRelevantPOA()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			using (declaration.SuspendValidationTesting())
			{
				var importer = Factory.New<OrgHeader>();
				var poaDocument1 = importer.RequiredDocuments.AddNew("POA");
				poaDocument1.EQ_DocType = "POA";
				poaDocument1.EQ_DocDescription = "Power of Attorney";
				poaDocument1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument1.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
				poaDocument1.EQ_ValidToDate = ZDate.Today.AddMonths(-1);

				var countrySpecificNameForPOA = "WHO HAS THE POWER";
				var a2av = new AuthorityToActValidator(countrySpecificNameForPOA, "");
				var documentOwner = "organization (eDocs > Document Tracking)";
				var pOAWillExpireSoon = AuthorityToActValidator.GetPOAWillExpireSoonString(ZDate.Today.ToString(), documentOwner, countrySpecificNameForPOA);
				var pOAExpired = AuthorityToActValidator.GetPOAExpiredString(ZDate.Today.AddMonths(-1).ToString(), documentOwner, countrySpecificNameForPOA);

				declaration.JE_OH_Importer = importer.PK;
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				var poaDocument2 = importer.RequiredDocuments.AddNew("POA");
				poaDocument2.EQ_DocType = "POA";
				poaDocument2.EQ_DocDescription = "Power of Attorney";
				poaDocument2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument2.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
				poaDocument2.EQ_ValidToDate = ZDate.Today;

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, pOAExpired);
			}
		}

		public void TestValidatePowerOfAttorneyDocumentDates()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			using (declaration.SuspendValidationTesting())
			{
				var importer = Factory.New<OrgHeader>();
				var poaDocument = importer.RequiredDocuments.AddNew("POA");
				poaDocument.EQ_DocType = "POA";
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument.EQ_ValidToDate = ZDateTime.Empty;
				declaration.JE_OH_Importer = importer.PK;

				var countrySpecificNameForPOA = "WHO HAS THE POWER";
				var a2av = new AuthorityToActValidator(countrySpecificNameForPOA, "");

				var documentOwner = "organization (eDocs > Document Tracking)";
				var receivedDateRequired = AuthorityToActValidator.GetReceivedDateRequiredString(countrySpecificNameForPOA, documentOwner);
				var expiryDateRequiredForPeriodicDocument = AuthorityToActValidator.GetExpiryDateRequiredForPeriodicDocumentString(countrySpecificNameForPOA, documentOwner);
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, receivedDateRequired);
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, receivedDateRequired);
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);

				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, receivedDateRequired);
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, receivedDateRequired);
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);

				poaDocument.EQ_DateReceived = ZDateTimeOffset.Today;
				poaDocument.EQ_ValidToDate = ZDateTime.Today;
				var pOAWillExpireSoon = AuthorityToActValidator.GetPOAWillExpireSoonString(ZDate.Today.ToString(), documentOwner, countrySpecificNameForPOA);
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, receivedDateRequired);
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, receivedDateRequired);
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);

				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(60);
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAWillExpireSoon);

				poaDocument.EQ_ValidToDate = ZDateTime.Today;
				var pOAExpired = AuthorityToActValidator.GetPOAExpiredString(ZDate.Today.AddDays(-1).ToString(), documentOwner, countrySpecificNameForPOA);
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, pOAExpired);

				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				declaration.ClearAllNotifications();
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, pOAExpired);

				var poaNotValid = AuthorityToActValidator.GetPOANotValidForConditions(countrySpecificNameForPOA, documentOwner, AuthorityToActValidator.CurrentCountryOrDirectionCondition(declaration.CountryCode));
				poaDocument.EQ_RN_NKRelatedCountry = "Z!";
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, poaNotValid);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, poaNotValid);

				poaDocument.EQ_RN_NKRelatedCountry = ZString.Empty;
				var attrib = poaDocument.Attributes.AddNew();
				attrib.D0_AttribName = "WHO";
				attrib.D0_AttribValue = "WHAT";
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["WHO", "WHERE"] != null, "") }, AuthorityToActValidator.CurrentCountryOrDirectionCondition(declaration.CountryCode));
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, poaNotValid);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["WHO", "WHERE"] != null, "") }, AuthorityToActValidator.CurrentCountryOrDirectionCondition(declaration.CountryCode));
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, poaNotValid);

				attrib.D0_AttribValue = "WHERE";
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["WHO", "WHERE"] != null, "") }, AuthorityToActValidator.CurrentCountryOrDirectionCondition(declaration.CountryCode));
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, poaNotValid);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", new List<(Predicate<JobRequiredDocument>, string)>() { (x => x.Attributes["WHO", "WHERE"] != null, "") }, AuthorityToActValidator.CurrentCountryOrDirectionCondition(declaration.CountryCode));
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, poaNotValid);

				importer.RequiredDocuments.RemoveAndDeleteAll();
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, AuthorityToActValidator.GetNoPOADocumentForImporterString(countrySpecificNameForPOA));

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, AuthorityToActValidator.GetNoPOADocumentForImporterString(countrySpecificNameForPOA));

				poaDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument.EQ_ValidToDate = ZDateTime.Empty;
				receivedDateRequired = AuthorityToActValidator.GetReceivedDateRequiredString(countrySpecificNameForPOA, "declaration (eDocs > Document Tracking)");
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, "POC", "POF");
				AssertHasWarning(declaration.JE_OH_ImporterInfo, receivedDateRequired);
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, AuthorityToActValidator.GetNoPOADocumentForImporterString(countrySpecificNameForPOA));

				declaration.ClearAllNotifications();
				a2av.ValidatePowerOfAttorneyDocumentDates(declaration.JE_OH_ImporterInfo, poaDocument, "declaration (eDocs > Document Tracking)");
				AssertHasWarning(declaration.JE_OH_ImporterInfo, receivedDateRequired);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, "POC", "POF");
				AssertHasMessageError(declaration.JE_OH_ImporterInfo, receivedDateRequired);
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, AuthorityToActValidator.GetNoPOADocumentForImporterString(countrySpecificNameForPOA));

				declaration.ClearAllNotifications();
				a2av.ValidatePowerOfAttorneyDocumentDates(declaration.JE_OH_ImporterInfo, poaDocument, "declaration (eDocs > Document Tracking)");
				AssertHasMessageError(declaration.JE_OH_ImporterInfo, receivedDateRequired);

				poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-1);
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, "POC", "POF");
				AssertNoWarning(declaration.JE_OH_ImporterInfo, receivedDateRequired);

				declaration.ClearAllNotifications();
				a2av.ValidatePowerOfAttorneyDocumentDates(declaration.JE_OH_ImporterInfo, poaDocument, "declaration (eDocs > Document Tracking)");
				AssertNoWarning(declaration.JE_OH_ImporterInfo, receivedDateRequired);

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, "POC", "POF");
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, receivedDateRequired);

				declaration.ClearAllNotifications();
				a2av.ValidatePowerOfAttorneyDocumentDates(declaration.JE_OH_ImporterInfo, poaDocument, "declaration (eDocs > Document Tracking)");
				AssertNoMessageError(declaration.JE_OH_ImporterInfo, receivedDateRequired);
			}
		}

		public void TestValidatePowerOfAttorneyCompanyCode()
		{
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "AAA";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (declaration.SuspendValidationTesting())
			{
				var poaDocument = importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
				poaDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument.EQ_DateReceived = ZDateTime.BrettsBirthday.ToDateTimeOffset(null);
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(1);

				string countrySpecificNameForPOA = "Power of Attorney";
				var a2av = new AuthorityToActValidator(countrySpecificNameForPOA, "");
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding);
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for");

				declaration.ClearAllNotifications();
				var companyCodeAttrib = poaDocument.Attributes.AddNew();
				companyCodeAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding);
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for");

				declaration.ClearAllNotifications();
				companyCodeAttrib.D0_AttribDisplayValue = company1.GC_Code;
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding);
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for");

				declaration.ClearAllNotifications();
				companyCodeAttrib.D0_AttribDisplayValue = GlbCompany.CurrentCompany.GC_Code;
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding);
				AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for");
			}
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestValidateDirection()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);

			using (declaration.SuspendValidationTesting())
			{
				var org = Factory.New<OrgHeader>();
				declaration.JE_OH_Importer = org.PK;
				declaration.JE_OH_Supplier = org.PK;
				declaration.JE_MessageType = "EXP";
				var poaDocument = MakePoaDoc(declaration, org);
				var a2av = new AuthorityToActValidator();

				// Just one doc....			
				var directionAttribute = poaDocument.Attributes.AddNew();
				directionAttribute.D0_AttribName = "DIRECTION";
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, "direction");
				directionAttribute.D0_AttribValue = "IMP";
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, "direction");
				directionAttribute.D0_AttribValue = "EXP";  // same as dec
				declaration.ClearAllNotifications();
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, "direction");

				// Two docs, one for imp and one for exp ... check we get the right one based on JE_MessageType
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(1); // 13 mar
				var secondPoa = MakePoaDoc(declaration, org);
				secondPoa.EQ_ValidToDate = ZDateTime.Today.AddDays(2);  // 14 mar
				var secondAttribute = secondPoa.Attributes.AddNew();
				secondAttribute.D0_AttribName = "DIRECTION";
				secondAttribute.D0_AttribValue = "IMP";
				a2av.Validate(declaration, declaration.Supplier, declaration.JE_OH_SupplierInfo, "POA", "POC", "POF");
				AssertHasWarningContaining(declaration.JE_OH_SupplierInfo, "expire on 13-Mar");
				declaration.JE_MessageType = "IMP";
				a2av.Validate(declaration, declaration.Importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF");
				AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, "expire on 14-Mar");
			}
		}

		public void TestHasValidPOA()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			using (declaration.SuspendValidationTesting())
			{
				var importer = Factory.New<OrgHeader>();
				var poaDocument = importer.RequiredDocuments.AddNew("POA");
				poaDocument.EQ_DocType = "POA";
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddMonths(-1);

				declaration.JE_OH_Importer = importer.PK;

				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);

				var a2av = new AuthorityToActValidator("Power of Attorney", "");

				AssertEquals("No valid POA", false, a2av.HasValidPOA(declaration, importer, new string[] { "POA" }, (x) => true));

				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddMonths(2);
				AssertEquals("Has valid POA", true, a2av.HasValidPOA(declaration, importer, new string[] { "POA" }, (x) => true));
			}
		}

		static JobRequiredDocument MakePoaDoc(BaseJobDeclaration declaration, OrgHeader importer)
		{
			var poaDocument = importer.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddYears(-1);
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
			return poaDocument;
		}
	}
}
