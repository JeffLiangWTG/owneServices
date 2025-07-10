using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageManagers.Testing;
using Enterprise.Customs.ZA.Manifest.Business.EDIFACT;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using CUSCAR = Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR;
using MessageSubTypeCodes = Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists.MessageSubTypeCodes;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class MessagingProviderTest : TestCaseWithFactory
	{
		public void TestGetEDIFACTStatusCalculator()
		{
			var statusCalculator = new MessagingProvider().GetEDIFACTStatusCalculator();
			AssertType(typeof(ZA.Business.MessageManagers.EDIFACTStatusCalculator), statusCalculator);
		}

		public void TestGetInterchangeSenderID()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica);
				var provider = new MessagingProvider();
				GlbCompany.CurrentCompany.OrgProxy.SetAgentCode(country, "DJC");
				AssertEquals("DJC", provider.GetInterchangeSenderID(country.Factory));
				GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, country, "DUAL");
				AssertEquals("DJCDUAL", provider.GetInterchangeSenderID(country.Factory));
				GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.CodeTypes.CarrierCode, country, "CCC");
				AssertEquals("DJCDUAL", provider.GetInterchangeSenderID(country.Factory));
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes["\"AGT\", \"ZA\""].Delete();
				AssertEquals("CCCDUAL", provider.GetInterchangeSenderID(country.Factory));
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes["\"CDP\", \"ZA\""].Delete();
				AssertEquals("CCC", provider.GetInterchangeSenderID(country.Factory));
			}
		}

		[TestDate(2017, 11, 2)]
		public void TestMRNForAmendOrDelete()
		{
			ZaAsycudaToCuscarTests.SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "8";
			var message1 = Factory.New<CUSRESEDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message1.EM_MessageType = ZA.Business.SARSEDIMessage.MessageTypes.CUSRES;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			message1.EM_MessageText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+6332A821D8364BBA9A76BE1678FD52BF'LOC+22+ ::ZZZ'GIS+8:120:ZZZ'NAD+AG+00000000'RFF+BH:S700049788'RFF+AAS:RFM'DTM+137:20170419:102'RFF+AFB:CARN0111138D'UNT+10+1'";
			message1.EM_LinkedObject = bill;
			var message2 = Factory.New<CUSRESEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message2.EM_MessageType = ZA.Business.SARSEDIMessage.MessageTypes.CUSRES;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message2.EM_MessageText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+04E362079C1245A48B4A388237F4FE5F'LOC+22+ ::ZZZ'GIS+8:120:ZZZ'NAD+AG+00000000'RFF+BH:S700049788'RFF+AAS:RFM'DTM+137:20170419:102'RFF+AFB:CARN0111138D'UNT+10+1'";
			message2.EM_LinkedObject = bill;
			Factory.Save();
			CusCarMessagingHelper.CreateCusCars(header, new[] { bill }, MessageSubTypeCodes.Codes.Change);
			var message = header.Messages.OfType<EDIMessage>().FirstOrDefault(m => m.EM_MessageSubType == "CHG");
			AssertNotNull(message);
			var lines = message.HumanReadableMessage.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
			var rffAcw = lines.FirstOrDefault(l => l.StartsWith("RFF+ACW"));
			AssertNotNull(rffAcw);
			Assert(rffAcw.EndsWith("04E362079C1245A48B4A388237F4FE5F'"));
		}

		public void TestCanSendMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "RFM";
			var sb = new ZStringBuilder();
			Assert(MessagingProvider.CanSendMessage(header, sb));
			Assert(sb.IsEmpty);
			header.AMA_ManifestType = "";
			Assert(!MessagingProvider.CanSendMessage(header, sb));
			AssertContains("Cannot create manifest message for type", sb.ToString());
		}

		public void TestGetMessageBuilder()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "RFM";
			AssertType<CUSCAR.CUSCARMessageBuilder>(MessagingProvider.GetMessageBuilder(header, MessageSubTypeCodes.Codes.Change, ZString.Empty));
			header.AMA_ManifestType = "";
			AssertNull(MessagingProvider.GetMessageBuilder(header, MessageSubTypeCodes.Codes.Change, ZString.Empty));
		}

		[TestDate(2019, 8, 19)]
		public void TestRequestLatestResponse()
		{
			var header = (AsycudaManifestHeader)AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, ZaManifestTypes.Codes.ALM, ApplicationCodeTypeList.Codes.ShippingLine);
			header.FillWithValidTestData();
			Factory.Save();
			var message = Factory.New<CUSCAREDIMessage>();
			message.EM_MessageText = @"UNH+360+CUSCAR:D:16A:UN:RCG001'BGM+85:::ALM+4702A6AD067441149AAF7809692B4C2C+9'RFF+LO:MAN0000014'NAD+MS+12342342'NAD+DEG'TDT+20++++:172:20'LOC+60'CNI+1+123434:BOL:123434'RFF+BM:11111'LOC+8'LOC+9'GID+1+0'FTX+AAA++9'MEA+AAE+AAB+KGM:0'PCI+24'UNT+16+360'";
			MessagingProvider.RequestLatestResponse(header, message, new MessageNotificationCollector_ForTest());
			var req = header.Messages.Cast<EDIMessage>().Single(x => x.EM_MessageType == "REQ");
			AssertEquals("UNH+2+REQDOC:D:99B:UN:ZZZ01'BGM+785+4702A6AD067441149AAF7809692B4C2C+9'DOC+704+123434::::ALM'DTM+318:20190819:102'NAD+MS+12342342'LIN+1'UNT+7+2'", req.EM_MessageText);
			AssertEquals(Env.CurrentBranch.PK, req.EM_GB);
		}

		public void TestRequestLatestResponse_OtherBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var header = (AsycudaManifestHeader)AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, ZaManifestTypes.Codes.ALM, ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_GB = branch.PK;
			header.FillWithValidTestData();
			Factory.Save();

			var message = Factory.New<CUSCAREDIMessage>();
			message.EM_MessageText = @"UNH+360+CUSCAR:D:16A:UN:RCG001'BGM+85:::ALM+4702A6AD067441149AAF7809692B4C2C+9'RFF+LO:MAN0000014'NAD+MS+12342342'NAD+DEG'TDT+20++++:172:20'LOC+60'CNI+1+123434:BOL:123434'RFF+BM:11111'LOC+8'LOC+9'GID+1+0'FTX+AAA++9'MEA+AAE+AAB+KGM:0'PCI+24'UNT+16+360'";
			MessagingProvider.RequestLatestResponse(header, message, new MessageNotificationCollector_ForTest());
			var req = header.Messages.Cast<EDIMessage>().Single(x => x.EM_MessageType == "REQ");
			AssertEquals(branch.PK, req.EM_GB);
		}
	}
}
