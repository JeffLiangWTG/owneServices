using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Edifact;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	public class INPUPDTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestGenerateBGM()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("BGM+914:::REX+<<MSGNO PLACEHOLDER>>+13'", msg.BGM.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateCST()
		{
			DataProvider.CargoPackingType = CargoPackingTypeCodeList.Codes.PackingType3;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("CST++3+AME'", msg.CST.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateFTX()
		{
			DataProvider.TradersRemarksForMessage = new ZString[] { "This is one string remark" };
			DataProvider.AdditionalMessageInfo.ReasonForAmending = "Amend For Test";
			DataProvider.AdditionalMessageInfo.ExtendingTemporaryImportPeriod = true;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("FTX+AAI+++THIS IS ONE STRING REMARK'FTX+BLO+++Y'FTX+ACF+++AMEND FOR TEST'", msg.FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMessageCNT()
		{
			DataProvider.NumberOfRequestsForUpdate = 3;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
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
		INPUPD MessageBuilder
		{
			get
			{
				if (fMessageBuilder == null)
				{
					fMessageBuilder = new INPUPD(DataProvider);
				}

				return fMessageBuilder;
			}
		}

		INPUPD fMessageBuilder;
		void ResetMessageBuilder()
		{
			fMessageBuilder = null;
		}
		#endregion
	}
}
