using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Edifact;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B.Testing
{
	public class Tnpupd09bTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestGenerateSegmentGroup1()
		{
			DataProvider.DeclarantId = "ORGSG00001";
			LicencesAndDocumentsTestClass[] documents = new LicencesAndDocumentsTestClass[2];
			documents[0] = new LicencesAndDocumentsTestClass();
			documents[0].LicenceNumber = "LIC123-3456-1";
			documents[1] = new LicencesAndDocumentsTestClass();
			documents[1].LicenceNumber = "LIC123-3456-2";
			DataProvider.LicencesAndDocuments = documents;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GTR;
			DataProvider.PreviousPermitNumber = "PR012345";
			ZString[] additionals = new ZString[1];
			additionals[0] = new ZString("ADD12034");
			DataProvider.AdditionalRecipients = additionals;
			var msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("TNPUPD - SG1", "RFF+MS:ORGS.ORGSG00001'RFF+DM:LIC123-3456-1'RFF+DM:LIC123-3456-2'RFF+ACE:PR012345'RFF+MR:ADD12034'", msg.Group1.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateFTX()
		{
			DataProvider.TradersRemarksForMessage = new ZString[] { "This is one string remark" };
			DataProvider.AdditionalMessageInfo.ReasonForAmending = "A123456789012345678901234567890123456789012345678901234567890123456789B123456789012345678901234567890123456789012345678901234567890123456789C123456789012345678901234567890123456789012345678901234567890123456789D123456789012345678901234567890123456789012345678901234567890123456789";
			DataProvider.AdditionalMessageInfo.ExtendingTemporaryImportPeriod = true;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("In TN4.1 all 280 characters of reason can be sent in first element of FTX segment - no longer need to split across 4 elements", "FTX+AAI+++THIS IS ONE STRING REMARK'FTX+ACF+++A123456789012345678901234567890123456789012345678901234567890123456789B123456789012345678901234567890123456789012345678901234567890123456789C123456789012345678901234567890123456789012345678901234567890123456789D123456789012345678901234567890123456789012345678901234567890123456789'FTX+BLO+++Y'", msg.FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateBGM()
		{
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("BGM+914+<<MSGNO PLACEHOLDER>>+13'", msg.BGM.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateCST()
		{
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("CST+++AME'", msg.CST.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateCNT()
		{
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("CNT+5:0'CNT+6:0'", msg.CNT.ToString(new UNOASGCharacterSet()));
		}

		#region Implementation
		TNPDECTestClass DataProvider
		{
			get
			{
				if (fDataProvider == null)
				{
					fDataProvider = new TNPDECTestClass();
				}

				return fDataProvider;
			}
		}

		TNPDECTestClass fDataProvider;
		Tnpupd09b MessageBuilder
		{
			get
			{
				if (fMessageBuilder == null)
				{
					fMessageBuilder = new Tnpupd09b(DataProvider);
				}

				return fMessageBuilder;
			}
		}

		Tnpupd09b fMessageBuilder;
		void ResetMessageBuilder()
		{
			fMessageBuilder = null;
		}
		#endregion
	}
}
