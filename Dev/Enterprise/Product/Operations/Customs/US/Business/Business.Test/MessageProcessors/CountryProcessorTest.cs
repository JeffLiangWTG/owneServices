using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class CountryProcessorTest : ABIProcessorTest<ACECountryProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestF102()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new ACECountryProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
"F102XA100             ALBERTA                       CANADIAN DOLLARCAD0122A");

			processor.Message = message;
			processor.Process();

			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			USCCountry[] countries = Factory.Load<USCCountry>(query);
			AssertEquals(1, countries.Length);

			USCCountry country = countries[0];

			AssertEquals("XA", country.UC_Code);
			AssertEquals("1", country.UC_RateColumnIndicator);
			AssertEquals(ZDateTime.Empty, country.UC_RateColumnBeginDate);
			AssertEquals(ZDateTime.Empty, country.UC_RateColumnEndDate);
			AssertEquals(ZBool.False, country.UC_RestrictionIndicator);
			AssertEquals(ZDateTime.Empty, country.UC_RestrictionIndicatorBeginDate);
			AssertEquals(ZDateTime.Empty, country.UC_RestrictionIndicatorEndDate);
			AssertEquals(ZBool.False, country.UC_GSPIndicator);
			AssertEquals(ZDateTime.Empty, country.UC_GSPBeginDate);
			AssertEquals(ZDateTime.Empty, country.UC_GSPEndDate);
			AssertEquals("", country.UC_SPICode);
			AssertEquals(ZDateTime.Empty, country.UC_SPIBeginDate);
			AssertEquals(ZDateTime.Empty, country.UC_SPIEndDate);
			AssertEquals("ALBERTA", country.UC_Name);
			AssertEquals("CANADIAN DOLLAR", country.UC_CurrencyName);
			AssertEquals("CAD", country.UC_CurrencyCode);
			AssertEquals(ZBool.False, country.UC_DrawbackEligibility);
			AssertEquals("", country.UC_SheduleCCountryCode);
			AssertEquals("", country.UC_SpecialTradeProgramsIndicator);
			AssertEquals(ZDateTime.Empty, country.UC_SpecialTradeProgramsBeginDate);
			AssertEquals(ZDateTime.Empty, country.UC_SpecialTradeProgramsEndDate);
			AssertEquals(ZBool.False, country.UC_LesserDevelopedCountry);
			AssertEquals("", country.UC_MiscellaneousSPIIndicator);
			AssertEquals(ZDateTime.Empty, country.UC_MiscellaneousSPIBeginDate);
			AssertEquals(ZDateTime.Empty, country.UC_MiscellaneousSPIEndDate);
		}

		public void TestF102WithNoCountry()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new ACECountryProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
"F102  100             ALBERTA                       CANADIAN DOLLARCAD0122A");

			processor.Message = message;
			processor.Process();

			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			USCCountry[] countries = Factory.Load<USCCountry>(query);
			AssertEquals(0, countries.Length);
		}

		public void TestEndDateAdjustment()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new ACECountryProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
"F102AL100010102123139 ALBERTA                       CANADIAN DOLLARCAD0122A",
"F202AL10101021231391010102123139E010102123139D1010102123139PA010102123139");
			processor.Message = message;
			processor.Process();

			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			USCCountry[] countries = Factory.Load<USCCountry>(query);
			AssertEquals(1, countries.Length);

			USCCountry country = countries[0];
			AssertEquals(new ZDate(2002, 01, 01), country.UC_RateColumnBeginDate);
			AssertEquals(new ZDate(2039, 12, 31), country.UC_RateColumnEndDate);
			AssertEquals(new ZDate(2002, 01, 01), country.UC_RestrictionIndicatorBeginDate);
			AssertEquals(new ZDate(2039, 12, 31), country.UC_RestrictionIndicatorEndDate);
			AssertEquals(new ZDate(2002, 01, 01), country.UC_SpecialTradeProgramsBeginDate);
			AssertEquals(new ZDate(2039, 12, 31), country.UC_SpecialTradeProgramsEndDate);
			AssertEquals(new ZDate(2002, 01, 01), country.UC_MiscellaneousSPIBeginDate);
			AssertEquals(new ZDate(2039, 12, 31), country.UC_MiscellaneousSPIEndDate);
			AssertEquals(new ZDate(2002, 01, 01), country.UC_SPIBeginDate);
			AssertEquals(new ZDate(2039, 12, 31), country.UC_SPIEndDate);
			AssertEquals(new ZDate(2002, 01, 01), country.UC_GSPBeginDate);
			AssertEquals(new ZDate(2039, 12, 31), country.UC_GSPEndDate);
		}

		protected override void EndToEndCore()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new ACECountryProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
"F102AL201070193999999 ALBANIA                       LEK            ALL14810",
"F202AL2010173110192");
			processor.Message = message;
			processor.Process();

			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			USCCountry[] countries = Factory.Load<USCCountry>(query);
			AssertEquals(1, countries.Length);

			var email = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<ZArchitecture.Environment.AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
