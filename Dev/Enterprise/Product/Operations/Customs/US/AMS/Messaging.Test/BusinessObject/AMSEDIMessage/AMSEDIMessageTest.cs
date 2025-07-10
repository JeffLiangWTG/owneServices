using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	[TestedType(typeof(AMSEDIMessage))]
	class AMSEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEM_LinkObject()
		{
			var moveHeader = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			var message = Factory.New<AMSEDIMessage>();
			message.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = moveHeader.PK;
			AssertEquals(moveHeader, message.EM_LinkedObject);
		}

		public void TestEM_MessageSubTypeDescription()
		{
			var message = Factory.New<AMSEDIMessage>();
			message.EM_MessageSubType = AMSMessageSubTypeList.Codes.PaperlessInBondOrVesselArrival;
			AssertEquals(AMSMessageSubTypeList.Descriptions.PaperlessInBondOrVesselArrival, message.EM_MessageSubTypeDescription);
		}

		public void TestEM_MessageNum()
		{
			var message = Factory.New<AMSEDIMessage>();
			message.EM_MessageText = "HI " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " WORLD";
			Env.NumberFountains.EDIFACTNumberFountain("M", GlbCompany.CurrentCompany.LicenceKeyIdentifier, CBPEDIInterchange.InterchangePartyIDs.AMSMailbox).SetNext(Factory, 12345678);
			Factory.Save();
			AssertEquals("EDIEDIDAT12345678", message.EM_MessageNum);
			AssertEquals("HI EDIEDIDAT12345678 WORLD", message.EM_MessageText);
			Env.NumberFountains.EDIFACTNumberFountain("M", GlbCompany.CurrentCompany.LicenceKeyIdentifier, CBPEDIInterchange.InterchangePartyIDs.AMSMailbox).SetNext(Factory, 1);
		}

		public void TestEM_MessageNumIsGeneratedBasedOnCompanyIdentifier()
		{
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "Z2Z";
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "Z2Z";
			company2.Branches.Add(branch2);
			var company3 = Factory.New<GlbCompany>();
			company3.GC_Code = "Z3Z";
			var branch3 = Factory.New<GlbBranch>();
			branch3.GB_Code = "Z3Z";
			company3.Branches.Add(branch3);
			var header1 = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header1.BH_GB = branch2.PK;
			var moveHeader1 = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			moveHeader1.BM_BH = header1.PK;
			var message1 = Factory.New<AMSEDIMessage>();
			message1.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message1.EM_LinkUniqueID = moveHeader1.PK;
			message1.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			header1.BH_CarrierSCAC = ZString.Empty;
			var expectedMessageNum1 = company2.LicenceKeyIdentifier + Env.NumberFountains.EDIFACTNumberFountain("M", company2.LicenceKeyIdentifier, CBPEDIInterchange.InterchangePartyIDs.AMSMailbox).PeekPreliminary(Factory);
			var header2 = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header2.BH_GB = branch2.PK;
			header2.BH_CarrierSCAC = "Ott3";
			var moveHeader2 = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			moveHeader2.BM_BH = header2.PK;
			var message2 = Factory.New<AMSEDIMessage>();
			message2.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message2.EM_LinkUniqueID = moveHeader2.PK;
			message2.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			var expectedMessageNum2 = company2.LicenceKeyIdentifier + (Env.NumberFountains.EDIFACTNumberFountain("M", company2.LicenceKeyIdentifier, CBPEDIInterchange.InterchangePartyIDs.AMSMailbox).PeekPreliminary(Factory) + 1);
			var header3 = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header3.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader3 = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			moveHeader3.BM_BH = header3.PK;
			var message3 = Factory.New<AMSEDIMessage>();
			message3.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message3.EM_LinkUniqueID = moveHeader3.PK;
			message3.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			header3.BH_CarrierSCAC = ZString.Empty;
			var expectedMessageNum3 = GlbCompany.CurrentCompany.LicenceKeyIdentifier + Env.NumberFountains.EDIFACTNumberFountain("M", GlbCompany.CurrentCompany.LicenceKeyIdentifier, CBPEDIInterchange.InterchangePartyIDs.AMSMailbox).PeekPreliminary(Factory);
			var message4 = Factory.New<AMSEDIMessage>();
			message4.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			message4.EM_GB = branch3.PK;
			var expectedMessageNum4 = company3.LicenceKeyIdentifier + Env.NumberFountains.EDIFACTNumberFountain("M", company3.LicenceKeyIdentifier, CBPEDIInterchange.InterchangePartyIDs.AMSMailbox).PeekPreliminary(Factory);
			Factory.Save();
			AssertEquals("The number should be based on the company 2 identifier", expectedMessageNum1, message1.EM_MessageNum);
			AssertEquals("The number should be based on the company 2 identifier", expectedMessageNum2, message2.EM_MessageNum);
			AssertEquals("The number should be based on the current company identifier", expectedMessageNum3, message3.EM_MessageNum);
			AssertEquals("The number should be based on the company 3 identifier", expectedMessageNum4, message4.EM_MessageNum);
		}

		public void TestExceeding9999Limit()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ASDD", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			var builder = new ZStringBuilder();
			var messageBlockString = new EQUC01().Serialise();
			for (var i = 0; i < 9996; i++)
			{
				builder.Append(messageBlockString);
			}

			var messageText = new INPD01()
			{ Description = AMSEDIMessage.AMSMessageNumberPlaceHolder }.Serialise() + builder.ToString();
			CombineAssertions(delegate
			{
				var message = Factory.New<AMSEDIMessage>();
				message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
				message.EM_MessageText = messageText;
				AssertNoExceptionThrown(delegate
				{
					Factory.Save();
				});
				AssertEquals(true, message.IsInDatabase);
				message = Factory.New<AMSEDIMessage>();
				message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
				message.EM_MessageText = messageText + messageBlockString;
				AssertExceptionThrown(typeof(ZCannotSaveException), delegate
				{
					Factory.Save();
				});
				AssertEquals(false, message.IsInDatabase);
			});
		}

		public void TestTransmitBlocks()
		{
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			message.EM_MessageText = new APLACR()
			{ ApplicationIdentifier = Customs.US.Messaging.Business.MessageBuildingBlocks.AMSApplicationIdentifierCodeList.Codes.ManifestCreate }.Serialise() + new INPB01().Serialise() + new APLZCR().Serialise();
			AssertMultilineASCIIEquals("Message Interprestation", @"-----------------APLACR-----------------
 Application Identifier (14-15) :MI

-----------------INPB01-----------------
 Manifest Quantity (21-30) :0
 Weight (36-45)            :0

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0
", message.EM_MessageInterpretation);
		}

		public void TestReceiveBlocks()
		{
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_MessageText = new APLACR()
			{ ApplicationIdentifier = Customs.US.Messaging.Business.MessageBuildingBlocks.AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse }.Serialise() + new OUTR01().Serialise() + new APLZCR().Serialise();
			AssertMultilineASCIIEquals("Message Interprestation", @"-----------------APLACR-----------------
 Application Identifier (14-15) :CR

-----------------OUTR01-----------------

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0
", message.EM_MessageInterpretation);
		}

		public void TestTransmitReceiveBlocksForACE()
		{
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			message.EM_MessageOwner = Constants.ACE;
			message.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			message.EM_MessageText = @"ACR          MI                                                                 M01    11ITA VESSEL               0351      000003                              M02_DAMC9                                                                       P012811021411                                                                   J01                                                                             B01            535500000000096CTN  0000004838KGN                                B020000000106CMKARACHI                      D031D00101520     53550             B04OB 34534534535                                                               S01IOFSDIHL                           JLSDFIJLU USCHI IL 66666                  U01N/A                                NA                                        N012020 LOGISTICS                     54451                                     N0254451                                                                        N032020 LOGISTICS                CL                                             N01TACSPO DISTRIBUTING PTY LTD        980 LYTTON ROAD                           N02980 LYTTON ROAD                                                              N03MURARRIE                      AU                                             C01APLU8009009   45                            FR0                    22P1L     D00           000000000000000089KG                                              D010000000007DSRFSDF                                                    CTN     D02JHKJKHJKHJK                                                                  C01OTRI7878780   46                            FR0                    22P1L     C02121564646545454654654                                                        D00           000000000000004654KG                                              D010000000043EWSRWERWER                                                 CTN     D02WERWERWER                                                                    D00           000000000000000045KG                                              D010000000044DFFSDFDSD                                                  CTN     D02SDFSDFSDFSD                                                                  C01NC                                            0                        L     D00           000000000000000050KG                                              D010000000002HJKLKJK                                                    CTN     D02HJKDSFSDFSDF                                                                 ZCR                               00000";
			AssertEquals(typeof(US.Messaging.Business.MessageBuildingBlocks.ACE.Common.INPM01), message.MessageBlock.MessageBlocks[0].GetType());
			var receivedMessage = Factory.New<AMSEDIMessage>();
			receivedMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			receivedMessage.EM_MessageOwner = Constants.ACE;
			receivedMessage.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			receivedMessage.EM_MessageText = "ACR          MR11090508021300495                                                M01OTT110AUAPL EMERALD            89   00001000077 7819369                      M02676666_OTT1339                                                               P01390109301100001    0000                                                      J01OTT1                                                                         B01676666      413230000000010PLT  0000010000KGN                   OTT1         W01676666                              042 INVALID QUANTITY                     W01676666                              042 INVALID QUANTITY                     W01676666                              060 XXXX  BILL REJECTED  XXXX            W02OTT11109050802110100100001000000000000000000010000000023                     ZCR          MI                   00009                                         ";
			AssertEquals(typeof(US.Messaging.Business.MessageBuildingBlocks.ACE.Common.INPM01), receivedMessage.MessageBlock.MessageBlocks[0].GetType());
		}

		public void TestGetMessageBlockApplicationCode()
		{
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			message.EM_MessageOwner = Constants.ACE;
			message.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			message.EM_MessageNum = "11501";
			var messageblockApplicationCode = message.GetMessageBlockApplicationCode();
			AssertEquals(Constants.ACE, messageblockApplicationCode);
			var response = Factory.New<AMSEDIMessage>();
			response.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			response.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			response.EM_MessageNum = "11501";
			messageblockApplicationCode = response.GetMessageBlockApplicationCode();
			AssertEquals(Constants.ACE, messageblockApplicationCode);
		}
	}
}
