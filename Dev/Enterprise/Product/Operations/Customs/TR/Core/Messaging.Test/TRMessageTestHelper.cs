using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.TR.Messaging.Testing
{
	public static class TRMessageTestHelper
	{
		public static ZString GetFileText(ZString fileName, string filePath = "Enterprise.Customs.TR.Messaging.Testing.TestFiles.")
		{
			var result = ZString.Empty;
			using (var stream = typeof(TRMessageTestHelper).Assembly.GetManifestResourceStream(filePath + fileName))
			{
				if (stream != null)
				{
					result = new StreamReader(stream).ReadToEnd();
				}
			}
			return result;
		}

		public static ZBlob GetFileBinary(ZString fileName)
		{
			var result = ZBlob.Empty;
			using (var stream = typeof(TRMessageTestHelper).Assembly.GetManifestResourceStream("Enterprise.Customs.TR.Messaging.Testing.TestFiles." + fileName))
			{
				if (stream != null && stream.Length < int.MaxValue)
				{
					result = new ZBlob(new BinaryReader(stream).ReadBytes((int)stream.Length));
				}
			}
			return result;
		}

		public static ZString GetBodyText(ZString messageType, bool isErrorMessage = false)
		{
			var result = ZString.Empty;
			switch (messageType)
			{
				case TRMessageTypes.Codes.TRE:
					result = isErrorMessage ? GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.") : GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
					break;
				case TRMessageTypes.Codes.TRL:
					result = isErrorMessage ? GetFileText("ETrade.QueryForInspectionLine.QueryForInspectionLineError.xml") : GetFileText("ETrade.QueryForInspectionLine.QueryForInspectionLineSuccess.xml");
					break;
				case TRMessageTypes.Codes.TRO:
					result = isErrorMessage ? GetFileText("Error.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Common.") : GetFileText("Success.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.");
					break;
				case TRMessageTypes.Codes.TRQ:
					result = isErrorMessage ? GetFileText("ETrade.QueryRegistrationNo.QueryRegistrationError.xml") : GetFileText("ETrade.QueryRegistrationNo.QueryRegistrationNoSuccess.xml");
					break;
				case TRMessageTypes.Codes.TRI:
					result = isErrorMessage ? GetFileText("ETrade.QueryForInspectionClerk.QueryForInspectionClerkError.xml") : GetFileText("ETrade.QueryForInspectionClerk.QueryForInspectionClerkSuccess.xml");
					break;
				case TRMessageTypes.Codes.TRS:
					result = isErrorMessage ? GetFileText("ETrade.ExportRegistrationNo.ExportRegistrationNoError.xml") : GetFileText("ETrade.ExportRegistrationNo.ExportRegistrationNoSuccess.xml");
					break;
				case TRMessageTypes.Codes.TRB:
					result = isErrorMessage ? GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForImportError.xml") : GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForImportSuccess.xml");
					break;
				case TRMessageTypes.Codes.TRD:
					result = isErrorMessage ? GetFileText("ETradeImportDischargeListError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.ImportDischargeList.") : GetFileText("ETradeImportDischargeListSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.ImportDischargeList.");
					break;
				case TRMessageTypes.Codes.TCD:
					result = isErrorMessage ? GetFileText("ETrade.ImportSendforComplementaryDeclaration.ImportSendforComplementaryDeclarationError.xml") : GetFileText("ETrade.ImportSendforComplementaryDeclaration.ImportSendforComplementaryDeclarationSuccess.xml");
					break;
				case TRMessageTypes.Codes.TSP:
					result = isErrorMessage ? GetFileText("SPTS.SPTSError.xml") : GetFileText("SPTS.SPTSSuccess.xml");
					break;
				default:
					result = ZString.Empty;
					break;
			}
			return result;
		}

		public static TMessage CreateMessage<TMessage>(BusinessObjectFactory factory, ZString messageType, ZString direction, ZString status, ZString messageText, ZString messageNum, ZString applicationReference)
			where TMessage : TRBaseMessage
		{
			var message = factory.New<TMessage>();
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_MessageText = messageText;
			message.EM_MessageNum = messageNum;
			message.EM_ApplicationReference = applicationReference;
			return message;
		}

		public static EDIInterchange CreateInterchange(BusinessObjectFactory factory, ZString interchangeType, ZString direction, ZString status, ZString bodyText, ZGuid sessionGUID)
		{
			var isTransmit = status == EDIInterchange.Direction.Transmit;
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TRCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_From = isTransmit ? "CW1" : "TR Customs";
			interchange.EI_To = isTransmit ? "TR Customs" : "CW1";
			interchange.EI_Status = status;
			interchange.EI_BodyText = bodyText;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;
			interchange.EI_IsActive = true;
			return interchange;
		}
	}
}
