using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TRImportExportMessage))]
	public class TRImportExportMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<TRImportExportMessage>();
			AssertEquals("Default value: EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
		}

		public void TestMessageNum()
		{
			var declaration = Factory.New<JobDeclaration>();
			var message1 = Factory.New<TRImportExportMessage>();
			message1.EM_LinkUniqueID = declaration.PK;
			message1.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message1.EM_MessageType = TRMessageTypes.Codes.DKO;
			Factory.Save();
			AssertEquals("00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<TRImportExportMessage>();
			message2.EM_LinkUniqueID = declaration.PK;
			message2.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message2.EM_MessageType = TRMessageTypes.Codes.DKO;
			Factory.Save();
			AssertEquals("00000000000002", message2.EM_MessageNum);
		}

		public void TestEM_MessageInterpretation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B60007441";
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKPortOfLoading = "TRADA";
			declaration.JE_RL_NKPortOfArrival = "TRIST";
			declaration.JE_VoyageFlightNo = "VOYAGEEEE";

			var orgSupplier = Factory.New<OrgHeader>();
			orgSupplier.OH_Code = "xSupplier";
			orgSupplier.OH_FullName = "xSupplier Full Name";
			orgSupplier.OH_RL_NKClosestPort = "TR";
			orgSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "1234567890123");
			orgSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890024379");
			var addressSupplier = orgSupplier.MainAddress;
			addressSupplier.OA_OH = orgSupplier.PK;
			addressSupplier.CompanyName = "xSupplier Company Name";
			addressSupplier.Address1 = "xSupplierAdress1";
			addressSupplier.Address2 = "xSupplierAdress2";
			addressSupplier.OA_Phone = "02122122691";
			addressSupplier.OA_Fax = "02122122692";
			addressSupplier.City = "ISTANBUL";
			addressSupplier.Postcode = "340301";
			addressSupplier.OA_RN_NKCountryCode = "TR";
			declaration.JE_OH_Supplier = orgSupplier.PK;

			var orgConsignee = Factory.New<OrgHeader>();
			orgConsignee.OH_Code = "xConsignee";
			orgConsignee.OH_FullName = "xConsignee Full Name";
			orgConsignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890024399");
			var addressConsignee = orgConsignee.MainAddress;
			addressConsignee.OA_OH = orgConsignee.PK;
			addressConsignee.CompanyName = "xConsignee Company Name";
			addressConsignee.Address1 = "xConsigneeAdress1";
			addressConsignee.Address2 = "xConsigneeAdress2";
			addressConsignee.OA_Phone = "02122122693";
			addressConsignee.OA_Fax = "02122122694";
			addressConsignee.City = "IZMIR";
			addressConsignee.Postcode = "340302";
			addressConsignee.OA_RN_NKCountryCode = "TR";
			declaration.JE_OH_Importer = orgConsignee.PK;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertMessageInterpretation(cusEntryHeader.PK, TRMessageTypes.Codes.DKO, "Declaration.Expected.DKOControlMessage.xml");
			AssertMessageInterpretation(cusEntryHeader.PK, TRMessageTypes.Codes.DTE, "Declaration.Expected.DTERegistrationMessage.xml");
			AssertQueryInterpretation(cusEntryHeader.PK, TRMessageTypes.Codes.DK1, "Declaration.Expected.IslemSonucGetir2.xml");
			AssertQueryInterpretation(cusEntryHeader.PK, TRMessageTypes.Codes.DT1, "Declaration.Expected.IslemSonucGetir2.xml");
			AssertQueryInterpretation(cusEntryHeader.PK, TRMessageTypes.Codes.DT2, "Declaration.Expected.IslemSorgula3.xml");
			AssertQueryInterpretation(cusEntryHeader.PK, TRMessageTypes.Codes.DT3, "Declaration.Expected.IslemSonucGetir4.xml");
		}

		void AssertMessageInterpretation(ZGuid entryHeaderPK, ZString messageType, ZString xmlFileName)
		{
			var message = Factory.New<TRImportExportMessage>();
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = entryHeaderPK;
			message.EM_MessageText = TRMessageTestHelper.GetFileText(xmlFileName);
			message.EM_MessageInterpretation = message.EM_MessageText;

			var messageInterpretation = message.EM_MessageInterpretation;
			CombineAssertions(messageType + " Message Interpretation", () =>
			{
				if (messageType == TRMessageTypes.Codes.DKO)
				{
					Assert("EM_MessageInterpretation should contains 'successfully'", messageInterpretation.Contains("Declaration Control Message for job B00173681 sent successfully."));
				}
				else
				{
					Assert("EM_MessageInterpretation should contains 'successfully'", messageInterpretation.Contains("Declaration Registration Message for job B00173681 sent successfully."));
				}
				Assert("EM_MessageInterpretation should contains 'Job Number'", messageInterpretation.Contains("<td>Job Number:</td><td>ULU-B00173681</td>"));
				Assert("EM_MessageInterpretation should contains 'Entry Type'", messageInterpretation.Contains("<td>Entry Type:</td><td>IMP</td>"));
				Assert("EM_MessageInterpretation should contains 'Customs Procedure Code'", messageInterpretation.Contains("<td>Customs Procedure Code:</td><td>2100</td>"));
				Assert("EM_MessageInterpretation should contains 'Customs Office'", messageInterpretation.Contains("<td>Customs Office:</td><td>070700</td>"));
				Assert("EM_MessageInterpretation should contains 'Box Quantity'", messageInterpretation.Contains("<td>Box Quantity:</td><td>10</td>"));
				Assert("EM_MessageInterpretation should contains 'Supplier'", messageInterpretation.Contains("<td>Supplier:</td><td>xSupplier Company Name</td>"));
				Assert("EM_MessageInterpretation should contains 'Importer'", messageInterpretation.Contains("<td>Importer:</td><td>xConsignee Company Name</td>"));
				Assert("EM_MessageInterpretation should contains 'Invoice Total'", messageInterpretation.Contains("<td>Invoice Total:</td><td>100.0000 USD</td>"));
				Assert("EM_MessageInterpretation should contains 'Load Port'", messageInterpretation.Contains("<td>Load Port:</td><td>TRADA</td>"));
				Assert("EM_MessageInterpretation should contains 'Discharge Port'", messageInterpretation.Contains("<td>Discharge Port:</td><td>TRIST</td>"));
				Assert("EM_MessageInterpretation should contains 'Vessel'", messageInterpretation.Contains("<td>Vessel:</td><td>VESSSELLLLLLLLLLL</td>"));
				Assert("EM_MessageInterpretation should contains 'Voyage'", messageInterpretation.Contains("<td>Voyage:</td><td>VOYAGEEEE</td>"));
			});
		}

		void AssertQueryInterpretation(ZGuid entryHeaderPK, ZString messageType, ZString xmlFileName)
		{
			var message = Factory.New<TRImportExportMessage>();
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = entryHeaderPK;
			message.EM_MessageText = TRMessageTestHelper.GetFileText(xmlFileName);
			message.EM_MessageInterpretation = message.EM_MessageText;

			var messageInterpretation = message.EM_MessageInterpretation;
			CombineAssertions(messageType + " Message Interpretation", () =>
			{
				Assert("EM_MessageInterpretation should contains 'successfully'", messageInterpretation.Contains("Declaration Message Type " + messageType + " sent successfully."));
				if (messageType == TRMessageTypes.Codes.DT2)
				{
					Assert("EM_MessageInterpretation should contains 'Job Number'", messageInterpretation.Contains("<tr><td>Job Number:</td><td>MAN0000001|20201224104</td></tr>"));
					Assert("EM_MessageInterpretation should contains 'Query Date'", messageInterpretation.Contains("<tr><td>Query Date:</td><td>2022-09-08</td></tr>"));
				}
				else
				{
					Assert("EM_MessageInterpretation should contains 'Query GUID'", messageInterpretation.Contains("<tr><td>Query GUID:</td><td>b5d51907-d1bf-4852-91c6-5c33e535a167</td></tr>"));
				}
			});
		}

		public void TestMessageTextIndentedXml()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B60007441";
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKPortOfLoading = "TRADA";
			declaration.JE_RL_NKPortOfArrival = "TRIST";
			declaration.JE_VoyageFlightNo = "VOYAGEEEE";

			var orgSupplier = Factory.New<OrgHeader>();
			orgSupplier.OH_Code = "xSupplier";
			orgSupplier.OH_FullName = "xSupplier Full Name";
			orgSupplier.OH_RL_NKClosestPort = "TR";
			orgSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "1234567890123");
			orgSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890024379");
			var addressSupplier = orgSupplier.MainAddress;
			addressSupplier.OA_OH = orgSupplier.PK;
			addressSupplier.CompanyName = "xSupplier Company Name";
			addressSupplier.Address1 = "xSupplierAdress1";
			addressSupplier.Address2 = "xSupplierAdress2";
			addressSupplier.OA_Phone = "02122122691";
			addressSupplier.OA_Fax = "02122122692";
			addressSupplier.City = "ISTANBUL";
			addressSupplier.Postcode = "340301";
			addressSupplier.OA_RN_NKCountryCode = "TR";
			declaration.JE_OH_Supplier = orgSupplier.PK;

			var orgConsignee = Factory.New<OrgHeader>();
			orgConsignee.OH_Code = "xConsignee";
			orgConsignee.OH_FullName = "xConsignee Full Name";
			orgConsignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890024399");
			var addressConsignee = orgConsignee.MainAddress;
			addressConsignee.OA_OH = orgConsignee.PK;
			addressConsignee.CompanyName = "xConsignee Company Name";
			addressConsignee.Address1 = "xConsigneeAdress1";
			addressConsignee.Address2 = "xConsigneeAdress2";
			addressConsignee.OA_Phone = "02122122693";
			addressConsignee.OA_Fax = "02122122694";
			addressConsignee.City = "IZMIR";
			addressConsignee.Postcode = "340302";
			addressConsignee.OA_RN_NKCountryCode = "TR";
			declaration.JE_OH_Importer = orgConsignee.PK;

			var messageData = new byte[] { 0x00, 0x01, 0xFF, 0x01 };
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			var messageDTE = Factory.New<TRImportExportMessage>();
			messageDTE.EM_MessageType = TRMessageTypes.Codes.DTE;
			messageDTE.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageDTE.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			messageDTE.EM_LinkUniqueID = cusEntryHeader.PK;
			messageDTE.EM_MessageData = new ZBlob(messageData);
			messageDTE.EM_MessageText = TRMessageTestHelper.GetFileText("Declaration.Expected.DKOControlMessage.xml");
			messageDTE.EM_MessageInterpretation = messageDTE.EM_MessageText;

			var indentedXml = messageDTE.EM_MessageTextIndentedXml;
			AssertEquals("EM_MessageTextIndentedXml should not contains e-Sign Data", true, indentedXml.Contains("soapenv"));

			var messageDTEReceive = Factory.New<TRImportExportMessage>();
			messageDTEReceive.EM_MessageType = TRMessageTypes.Codes.DTE;
			messageDTEReceive.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			messageDTEReceive.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			messageDTEReceive.EM_LinkUniqueID = cusEntryHeader.PK;
			messageDTEReceive.EM_MessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><TescilResponse xmlns=""http://Gumruk.BizTalk.Integration""><Root xmlns=""http://schemas.microsoft.com/BizTalk/2003/Any""><Response xmlns=""""><RefID>ULU-B60007655</RefID><Guid>6f65c6e4-23c0-4c2e-bf2d-651d71e514bb</Guid><Durum>İşleminiz başlamıştır.Teşekkür ederiz.</Durum></Response></Root></TescilResponse></s:Body></s:Envelope>";
			messageDTEReceive.EM_MessageInterpretation = messageDTEReceive.EM_MessageText;
			var expected = TRMessageTestHelper.GetFileText("Declaration.Incoming.DTEResultSuccess.xml");
			indentedXml = messageDTEReceive.EM_MessageTextIndentedXml;
			AssertEquals(expected, indentedXml);
		}
	}
}
