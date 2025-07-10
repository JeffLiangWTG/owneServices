using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageSending;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5105CMApplication))]
	sealed class NX5105CMApplicationTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NX5105CMApplication(null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new NX5105CMApplication(Factory.New<CusTWControllingMessageHeader>(), null));
			AssertExceptionThrown<ArgumentNullException>(() => new NX5105CMApplication(null, new[] { Factory.New<CusEntryLine>() }));
			AssertNoExceptionThrown(() => new NX5105CMApplication(header, new[] { Factory.New<CusEntryLine>() }));
		}

		[ExpectNoExceptions]
		public void TestBankAccount()
		{
			declaration.JE_OtherBankAccount = "1099000001";
			NUnit.Framework.Assert.That(application.BankAccount, NUnit.Framework.Is.EqualTo("1099000001").Using(CustomComparers.TypeComparison));
			declaration.JE_OtherBankAccount = ZString.Empty;
			NUnit.Framework.Assert.That(application.BankAccount, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestContactOffice()
		{
			header.TW1_ProcessingUnit = "10";
			NUnit.Framework.Assert.That(application.ContactOffice, NUnit.Framework.Is.EqualTo("10").Using(CustomComparers.TypeComparison));
			header.TW1_ProcessingUnit = ZString.Empty;
			NUnit.Framework.Assert.That(application.ContactOffice, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestPaymentMethodCode()
		{
			NUnit.Framework.Assert.That(application.Payment, NUnit.Framework.Is.TypeOf<PaymentWrapper>());
			header.TW1_PaymentMethod = "7";
			NUnit.Framework.Assert.That(application.Payment.MethodCode, NUnit.Framework.Is.EqualTo("7").Using(CustomComparers.TypeComparison));
			header.TW1_PaymentMethod = ZString.Empty;
			NUnit.Framework.Assert.That(application.Payment.MethodCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestResponsibleGovernmentAgency()
		{
			header.TW1_ControllingAgency = "VP";
			NUnit.Framework.Assert.That(application.ResponsibleGovernmentAgency, NUnit.Framework.Is.EqualTo("VP").Using(CustomComparers.TypeComparison));
			header.TW1_ControllingAgency = ZString.Empty;
			NUnit.Framework.Assert.That(application.ResponsibleGovernmentAgency, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[TestDate(2020, 09, 18)]
		[ExpectNoExceptions]
		public void TestAppointment()
		{
			NUnit.Framework.Assert.That(application.Appointment, NUnit.Framework.Is.TypeOf<NX5105CMApplication.Appointment>());
			header.TW1_AppointmentDate = ZDateTime.Today.Date;
			header.TW1_AppointmentPeriod = "P";
			NUnit.Framework.Assert.That(application.Appointment.ReservationDate, NUnit.Framework.Is.EqualTo(ZDateTime.Today.Date).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(application.Appointment.ReservationPeriodCode, NUnit.Framework.Is.EqualTo("P").Using(CustomComparers.TypeComparison));
			header.TW1_AppointmentDate = ZDateTime.Empty.Date;
			header.TW1_AppointmentPeriod = ZString.Empty;
			NUnit.Framework.Assert.That(application.Appointment.ReservationDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty.Date).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(application.Appointment.ReservationPeriodCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#region AuthorizedInformation
		[ExpectNoExceptions]
		public void TestAuthorizedInformation()
		{
			declaration.JE_OH_Importer = ZGuid.Empty;
			NUnit.Framework.Assert.That(application.AuthorizedInformation, NUnit.Framework.Is.EqualTo(default(IAuthorizedInformation)));
			var importer = new TestTWCreator(Factory).CreateOrganizationForImporter();
			declaration.JE_OH_Importer = importer.PK;
			NUnit.Framework.Assert.That(application.AuthorizedInformation, NUnit.Framework.Is.EqualTo(default(IAuthorizedInformation)));
			AssertAuthorizedInformation(importer, Core.Constants.RefDocTypes.PowerOfAttorney, "6666666", Core.Constants.CountryCodes.Taiwan, "2", ZString.Empty);
			AssertAuthorizedInformation(importer, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, "88888", Core.Constants.CountryCodes.Australia, "2", ZString.Empty);
			AssertAuthorizedInformation(importer, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, "213542", Core.Constants.CountryCodes.Taiwan, "1", "213542");
		}

		[ExpectNoExceptions]
		void AssertAuthorizedInformation(OrgHeader orgHeader, ZString docType, ZString docNumber, ZString country, ZString expectTypeCode, ZString expectId)
		{
			var requiredDocument = orgHeader.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = docType;
			requiredDocument.EQ_DocNumber = docNumber;
			requiredDocument.EQ_RN_NKRelatedCountry = country;
			NUnit.Framework.Assert.That(application.AuthorizedInformation, NUnit.Framework.Is.TypeOf<NX5105CMApplication.AuthorizedInformation>());
			NUnit.Framework.Assert.That(application.AuthorizedInformation.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo(expectTypeCode));
			if (string.IsNullOrEmpty(expectId))
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(IAdditionalDocument)));
			}
			else
			{
				NUnit.Framework.Assert.That(application.AuthorizedInformation.AdditionalDocument.ID, NUnit.Framework.Is.EqualTo(expectId));
			}
		}

		#endregion
		[ExpectNoExceptions]
		public void TestDeclarer()
		{
			declaration.JE_OH_Importer = ZGuid.Empty;
			NUnit.Framework.Assert.That(application.Declarer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			var importer = new TestTWCreator(Factory).CreateOrganizationForImporter();
			declaration.JE_OH_Importer = importer.PK;
			NUnit.Framework.Assert.That(application.Declarer, NUnit.Framework.Is.Not.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(application.Declarer, NUnit.Framework.Is.TypeOf<NX5105CMApplicationDeclarerWrapper>());
		}

		[ExpectNoExceptions]
		public void TestItemGroupReferenceSequenceNumerics()
		{
			NUnit.Framework.Assert.That(application.ItemGroupReferenceSequenceNumerics.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(application.ItemGroupReferenceSequenceNumerics.First(), NUnit.Framework.Is.EqualTo(10).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLocalManufacturer()
		{
			NUnit.Framework.Assert.That(application.LocalManufacturer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			var importer = new TestTWCreator(Factory).CreateOrganizationForImporter();
			header.LocalProcessorAddress.E2_OA_Address = importer.MainAddress.PK;
			NUnit.Framework.Assert.That(application.LocalManufacturer, NUnit.Framework.Is.Not.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(application.LocalManufacturer, NUnit.Framework.Is.TypeOf<NX5105CMApplicationLocalManufacturerWrapper>());
		}

		[ExpectNoExceptions]
		public void TestLabels()
		{
			NUnit.Framework.Assert.That(application.Labels.Count(), NUnit.Framework.Is.EqualTo(0));
			var label1group1 = header.ProductLabelRanges.AddNew();
			label1group1.TW0_Status = "A";
			NUnit.Framework.Assert.That(application.Labels.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(application.Labels.First().StatusNameCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(application.Labels.First().LabelDetails.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(application.Labels.First().LabelDetails.First(), NUnit.Framework.Is.TypeOf<LabelDetail>());
			var label2group1 = header.ProductLabelRanges.AddNew();
			label2group1.TW0_Status = "A";
			NUnit.Framework.Assert.That(application.Labels.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(application.Labels.First().LabelDetails.Count(), NUnit.Framework.Is.EqualTo(2));
			var label3group2 = header.ProductLabelRanges.AddNew();
			label3group2.TW0_Status = "B";
			NUnit.Framework.Assert.That(application.Labels.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestWine()
		{
			NUnit.Framework.Assert.That(application.Wine, NUnit.Framework.Is.TypeOf<NX5105CMApplicationWine>());
		}

		[ExpectNoExceptions]
		public void TestNoImplementation()
		{
			NUnit.Framework.Assert.That(application.AdditionalDocuments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalDocument>)));
		}

		[ExpectNoExceptions]
		public void TestFunctionalReferenceID()
		{
			NUnit.Framework.Assert.That(application.FunctionalReferenceID, NUnit.Framework.Is.EqualTo(SharedHelper.GetFunctionalReferenceIDPlaceHolderWithPK(header.PK)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			header.PermitNumber = "FSFS";
			NUnit.Framework.Assert.That(application.ID, NUnit.Framework.Is.EqualTo("FSFS").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPurposeCode()
		{
			header.TW1_Purpose = "52";
			NUnit.Framework.Assert.That(application.PurposeCode, NUnit.Framework.Is.EqualTo("52").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			header.TW1_BusinessType = "B";
			NUnit.Framework.Assert.That(application.TypeCode, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformation()
		{
			NUnit.Framework.Assert.That(application.AdditionalInformation, NUnit.Framework.Is.TypeOf<NX5105CMApplicationAdditionalInformation>());
		}

		[ExpectNoExceptions]
		public void TestAgent()
		{
			var testDataHelper = new TestTWCreator(Factory);
			var orgHeader = testDataHelper.CreateOrganization();
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			NUnit.Framework.Assert.That(application.Agent, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			NUnit.Framework.Assert.That(application.Agent, NUnit.Framework.Is.Not.EqualTo(default(IPartyDetails)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			entryLine.CL_LineNumber = 10;
			application = new NX5105CMApplication(header, new[] { entryLine });
		}

		IApplication application;
		CusTWControllingMessageHeader header;
		JobDeclaration declaration;
	}
}
