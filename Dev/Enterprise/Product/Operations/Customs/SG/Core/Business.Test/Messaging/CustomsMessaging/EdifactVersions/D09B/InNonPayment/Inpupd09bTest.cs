using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Edifact;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B.Testing
{
	public class Inpupd09bTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestGenerateBGM()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("BGM+914:::REX+<<MSGNO PLACEHOLDER>>+13'", msg.BGM.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateCST()
		{
			DataProvider.CargoPackingType = CargoPackingTypeCodeList.Codes.PackingType3;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("CST++3+AME'", msg.CST.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateFTX()
		{
			DataProvider.TradersRemarksForMessage = new ZString[] { "This is one string remark" };
			DataProvider.AdditionalMessageInfo.ReasonForAmending = "Amend For Test";
			DataProvider.AdditionalMessageInfo.ExtendingTemporaryImportPeriod = true;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("FTX+AAI+++THIS IS ONE STRING REMARK'FTX+BLO+++Y'FTX+ACF+++AMEND FOR TEST'", msg.FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMessageCNT()
		{
			DataProvider.NumberOfRequestsForUpdate = 3;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("CNT+5:0'CNT+6:3'", msg.CNT.ToString(new UNOASGCharacterSet()));
		}

		#region Implementation
		IINPUPDTestClass DataProvider
		{
			get
			{
				if (fDataProvider == null)
				{
					fDataProvider = new IINPUPDTestClass();
				}

				return fDataProvider;
			}
		}

		IINPUPDTestClass fDataProvider;
		Inpupd09b MessageBuilder
		{
			get
			{
				if (fMessageBuilder == null)
				{
					fMessageBuilder = new Inpupd09b(DataProvider);
				}

				return fMessageBuilder;
			}
		}

		Inpupd09b fMessageBuilder;
		void ResetMessageBuilder()
		{
			fMessageBuilder = null;
		}
		#endregion
	}
}
