using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageSending;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageApplication))]
	sealed class LicensingMessageApplicationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			header.TW1_ProcessingUnit = "PKG";
			header.TW1_PaymentMethod = "PP";
			header.BulkPaymentID = "id01";
			header.TW1_AppointmentDate = ZDate.BrettsBirthday;
			header.TW1_AppointmentPeriod = "8";
			header.TW1_InspectionRegistrationNumber = "9";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.FunctionalReferenceID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "FunctionalReferenceID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(application.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(application.PurposeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "PurposeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(application.TypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(application.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(2), "AdditionalDocuments");
				NUnit.Framework.Assert.That(application.BankAccount.ToString(), NUnit.Framework.Is.Null.Or.Empty, "BankAccount - should be [null] or [empty]");
				NUnit.Framework.Assert.That(application.ContactOffice, NUnit.Framework.Is.EqualTo("PKG").Using(CustomComparers.TypeComparison), "ContactOffice");
				NUnit.Framework.Assert.That(application.Payment.ReferenceID, NUnit.Framework.Is.EqualTo("id01").Using(CustomComparers.TypeComparison), "Payment.ReferenceID");
				NUnit.Framework.Assert.That(application.Payment.MethodCode, NUnit.Framework.Is.EqualTo("PP").Using(CustomComparers.TypeComparison), "Payment.MethodCode");
				NUnit.Framework.Assert.That(application.ResponsibleGovernmentAgency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ResponsibleGovernmentAgency - should be [null] or [empty]");
				NUnit.Framework.Assert.That(application.Appointment.ReservationDate, NUnit.Framework.Is.EqualTo(ZDate.BrettsBirthday).Using(CustomComparers.TypeComparison), "Appointment.ReservationDate");
				NUnit.Framework.Assert.That(application.Appointment.ReservationPeriodCode, NUnit.Framework.Is.EqualTo("8").Using(CustomComparers.TypeComparison), "Appointment.ReservationPeriodCode");
				NUnit.Framework.Assert.That(application.ApprovalAuthenticationInformation, NUnit.Framework.Is.EqualTo("9").Using(CustomComparers.TypeComparison), "ApprovalAuthenticationInformation");
				NUnit.Framework.Assert.That(application.ItemGroupReferenceSequenceNumerics, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<CargoWise.Types.ZInt>)), "ItemGroupReferenceSequenceNumerics - should be [null]");
				NUnit.Framework.Assert.That(application.BankAccount.ToString(), NUnit.Framework.Is.Null.Or.Empty, "BankAccount - should be [null] or [empty]");
			});
		}

		[TestDate(2024, 3, 5)]
		[ExpectNoExceptions]
		public void TestAuthorizedInformation_Import()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "Org1";
			var doc = org1.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
			doc.EQ_ValidToDate = ZDateTime.Now.AddDays(1);
			doc.EQ_DocNumber = "12345";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "Org2";

			declaration.JE_MessageType = "IMP";
			declaration.JE_OA_DeclarantAddress = org1.MainAddress.PK;

			header.TW1_OH_Importer = org1.PK;
			application = new LicensingMessageApplication(header, null);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison), "AuthorizedTypeCode is 3");
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDocument)), "AdditionalDocument is null - should be [null]");
			});

			declaration.JE_OA_DeclarantAddress = org2.MainAddress.PK;
			application = new LicensingMessageApplication(header, null);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "AuthorizedTypeCode is 1");
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument.ID, NUnit.Framework.Is.EqualTo("12345").Using(CustomComparers.TypeComparison), "AdditionalDocument.ID is '12345'");
			});

			doc.EQ_ValidToDate = ZDateTime.Now.AddDays(-1);
			application = new LicensingMessageApplication(header, null);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "AuthorizedTypeCode is 2");
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDocument)), "AdditionalDocument is null - should be [null]");
			});

			doc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			doc.EQ_ValidToDate = ZDateTime.Now.AddDays(1);
			application = new LicensingMessageApplication(header, null);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "AuthorizedTypeCode is 2");
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDocument)), "AdditionalDocument is null - should be [null]");
			});
		}

		[TestDate(2024, 3, 5)]
		[ExpectNoExceptions]
		public void TestAuthorizedInformation_Export()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "Org1";
			var doc = org1.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
			doc.EQ_ValidToDate = ZDateTime.Now.AddDays(1);
			doc.EQ_DocNumber = "12345";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "Org2";

			declaration.JE_MessageType = "EXP";
			declaration.JE_OA_DeclarantAddress = org1.MainAddress.PK;

			header.TW1_OH_Supplier = org1.PK;
			application = new LicensingMessageApplication(header, null);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison), "AuthorizedTypeCode is 3");
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDocument)), "AdditionalDocument is null - should be [null]");
			});

			declaration.JE_OA_DeclarantAddress = org2.MainAddress.PK;
			application = new LicensingMessageApplication(header, null);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "AuthorizedTypeCode is 1");
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument.ID, NUnit.Framework.Is.EqualTo("12345").Using(CustomComparers.TypeComparison), "AdditionalDocument.ID is '12345'");
			});

			doc.EQ_ValidToDate = ZDateTime.Now.AddDays(-1);
			application = new LicensingMessageApplication(header, null);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "AuthorizedTypeCode is 2");
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDocument)), "AdditionalDocument is null - should be [null]");
			});

			doc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			doc.EQ_ValidToDate = ZDateTime.Now.AddDays(1);
			application = new LicensingMessageApplication(header, null);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "AuthorizedTypeCode is 2");
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDocument)), "AdditionalDocument is null - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestDeclarer()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.Declarer, NUnit.Framework.Is.TypeOf<LicensingMessageApplicationApplicant>());
				NUnit.Framework.Assert.That(application.Declarer, NUnit.Framework.Is.EqualTo(application.Applicant), "Declarer should be same as Applicant");
			});
		}

		[ExpectNoExceptions]
		public void TestAgent()
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			NUnit.Framework.Assert.That(application.Agent, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));

			var org = new TestTWCreator(Factory).CreateOrganization();
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			NUnit.Framework.Assert.That(application.Agent, NUnit.Framework.Is.TypeOf<LicensingMessageApplicationAgent>());
		}

		[ExpectNoExceptions]
		public void TestApplicant()
		{
			NUnit.Framework.Assert.That(application.Applicant, NUnit.Framework.Is.TypeOf<LicensingMessageApplicationApplicant>());
		}

		[ExpectNoExceptions]
		public void TestLabels()
		{
			var collection = header.ProductLabelRanges;
			var productLabelRange1 = collection.AddNew();
			productLabelRange1.TW0_Status = "2";
			var productLabelRange2 = collection.AddNew();
			productLabelRange2.TW0_Status = "1";
			var productLabelRange3 = collection.AddNew();
			productLabelRange3.TW0_Status = "2";
			var labels = application.Labels.ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(labels.Length, NUnit.Framework.Is.EqualTo(2), "Should have 2 labels");
				NUnit.Framework.Assert.That(labels[0].StatusNameCode == "1" && labels[1].StatusNameCode == "2", NUnit.Framework.Is.True, "Labels should ordered by TW0_Status");
				NUnit.Framework.Assert.That(labels[1].LabelDetails.Count(), NUnit.Framework.Is.EqualTo(2), "Labels[0] should contents 2 elements");
				NUnit.Framework.Assert.That(labels[0].LabelDetails.First(), NUnit.Framework.Is.TypeOf<LabelDetail>());
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformation()
		{
			header.TW1_RequestDescription = "RequestDescription";
			header.TW1_SampleReturnAddress = "ChineseLine";
			header.TW1_SamplingReductionReason = "SamplingReductionReason";
			header.TW1_ElectronicReceipt = true;
			header.TW1_ProofOfPaper = true;
			header.TW1_ApplyForSampleReturn = true;
			header.BulkApplicationID = "BulkID";
			header.TW1_PortOfBulkCommodity = "a";
			var additionalInfo = application.AdditionalInformation;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(additionalInfo.StatementDescription, NUnit.Framework.Is.EqualTo("RequestDescription").Using(CustomComparers.TypeComparison), "StatementDescription");
				NUnit.Framework.Assert.That(additionalInfo.AddressChineseLine, NUnit.Framework.Is.EqualTo("ChineseLine").Using(CustomComparers.TypeComparison), "AddressChineseLine");
				NUnit.Framework.Assert.That(additionalInfo.DeductionSample, NUnit.Framework.Is.EqualTo("SamplingReductionReason").Using(CustomComparers.TypeComparison), "DeductionSample");
				NUnit.Framework.Assert.That(additionalInfo.ElectronicReceipt, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "ElectronicReceipt");
				NUnit.Framework.Assert.That(additionalInfo.ProvedPaper, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "ProvedPaper");
				NUnit.Framework.Assert.That(additionalInfo.ReturnSample, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "ReturnSample");
				NUnit.Framework.Assert.That(additionalInfo.BulkApplicationID, NUnit.Framework.Is.EqualTo("BulkID").Using(CustomComparers.TypeComparison), "BulkApplicationID");
				NUnit.Framework.Assert.That(additionalInfo.BulkPortCode, NUnit.Framework.Is.EqualTo("a").Using(CustomComparers.TypeComparison), "BulkPortCode");
			});

			header.TW1_ProofOfPaper = false;
			header.TW1_ApplyForSampleReturn = false;
			additionalInfo = application.AdditionalInformation;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(additionalInfo.ProvedPaper, NUnit.Framework.Is.EqualTo(ZString.Empty), "ProvedPaper");
				NUnit.Framework.Assert.That(additionalInfo.ReturnSample, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison), "ReturnSample");
			});
		}

		[ExpectNoExceptions]
		public void TestLocalManufacturer()
		{
			var localProcessorAddress = header.LocalProcessorAddress;
			localProcessorAddress.E2_AddressOverride = true;
			localProcessorAddress.E2_Phone = "0226789456";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(application.LocalManufacturer, NUnit.Framework.Is.TypeOf<LicensingMessageApplicationLocalManufacturer>());
				NUnit.Framework.Assert.That(application.LocalManufacturer.Communications.FirstOrDefault().ID, NUnit.Framework.Is.EqualTo("0226789456").Using(CustomComparers.TypeComparison), "Phone");
			});
		}

		public void TestWine()
		{
			header.TW1_PrePermitNumber = "52889317";
			header.TW1_PreWineInspectionStatus = PreviousImportedWineInspectionStatusList.Codes._3;
			header.EthanolPermitNumbers.AddNew().CSI_ReferenceNumber = "Ref 1";
			header.EthanolPermitNumbers.AddNew().CSI_ReferenceNumber = "Ref 2";
			CombineAssertions(() =>
			{
				var wine = application.Wine;
				NUnit.Framework.Assert.That(wine.GovernmentProcedurePreviousCode, NUnit.Framework.Is.EqualTo(PreviousImportedWineInspectionStatusList.Codes._3).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wine.PreviousDocument.ID, NUnit.Framework.Is.EqualTo("52889317").Using(CustomComparers.TypeComparison));
				AssertContainsExactElementsInExactOrder(wine.AdditionalDocuments.Select(a => a.ID), new[] { "Ref 1", "Ref 2" });
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(new byte[1], "sample.pdf", Core.Constants.FileFormats.PDF);
			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(new byte[1], "Test.xls", Core.Constants.FileFormats.XLS);
			var messageSendingObject = new LicensingMessageSendingObjectForTest(header);
			var docLine1 = messageSendingObject.SupportingDocuments.AddNew();
			docLine1.EDoc = doc1.UniqueKey;
			var docLine2 = messageSendingObject.SupportingDocuments.AddNew();
			docLine2.EDoc = doc2.UniqueKey;
			application = new LicensingMessageApplication(header, messageSendingObject);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		IApplication application;

		class LicensingMessageSendingObjectForTest : LicensingMessageSendingObject
		{
			public LicensingMessageSendingObjectForTest(CusTWControllingMessageHeader header) : base(header)
			{
			}

			protected override ZString GetEM_MessageTypeCore() => ZString.Empty;

			protected override ITWMessageBuilder GetMessageBuilder() => null;
		}
	}
}
