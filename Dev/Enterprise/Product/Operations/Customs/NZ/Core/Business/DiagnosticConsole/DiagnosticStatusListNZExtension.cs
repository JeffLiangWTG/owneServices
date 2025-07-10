using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business
{
	public partial class DiagnosticStatusListNZ
	{
		public static ZString GetExtendedStatusDescription(ZString statusCode)
		{
			switch (statusCode)
			{
				case Codes.TestMessageAtQUEStatus:
					return Res.GetString("0A70CED3-7F8F-42A3-B019-0BB5A6090F64", "Check that the 'NZ Customs Message Sender' Service Task is running.\r\nCheck the 'NZ Customs Message Sender' Service Task log.");
				case Codes.TestMessageStatusInvalid:
				case Codes.NoOutInterchange:
				case Codes.NoOutMessage:
				case Codes.OutInterchangeStatusInvalid:
				case Codes.LookingForOutgoingItem:
					return Res.GetString("F7385864-63A8-4CC3-9A64-33FBBF1537B9", "Check the 'eHub Outbound Messages' Service Task log.");
				case Codes.OutgoingItemAtQUEStatus:
					return Res.GetString("201F4582-1D95-4716-9F18-2A883DD1126D", "Check that the 'eHub Outbound Messages' Service Task is running.\r\nCheck the 'eHub Outbound Messages' Service Task log.");
				case Codes.OutMessageStatusInvalid:
					return Res.GetString("5404488A-6484-4576-BA6F-3364188E81E8", "Check the 'eHub Outbound Messages' Service Task log.");
				case Codes.WaitingForACK:
					return Res.GetString("8270DFAC-B899-4717-9A9E-80CFE3559889", "No acknowledgement has been received from Customs.\r\n\tCheck that the 'eHub Inbound Messages' Service Task is running, and check its log\r\nCheck if there are any messages from Customs that have failed.\r\n\tOther possible causes are:\r\n\tCustoms are down or experiencing problems, check the Customs web site and with Customs\r\n\tYour digital certificate is invalid or not registered with Customs.");
				case Codes.WaitingForResponse:
					return Res.GetString("2EFD8EFB-67BC-4CD2-90DF-2399BBF50455", "No business reply has been received from Customs.\r\nSince the message has been acknowledged this indicates that your digital certificate and the message channel to and from Customs are OK.\r\nCustoms may be down or experiencing problems, check the Customs website and with Customs.\r\nCheck the 'eHub Inbound Messages' service task log.");
				case Codes.WaitingForTestMessageResponse:
					return Res.GetString("706F073D-EECE-4434-ACD6-C06105C280EA", "No reply message has been received.\r\nCheck if there are any inbound messages that have failed.\r\nCheck the 'eHub Inbound Messages' service task log.");
				case Codes.OutInterchangeRejected:
					return "This is most likely caused by an invalid (or incompatible) setup in " + Core.Constants.ProductName + " or at Customs.\r\nCheck that your company is registered to send TSW messages.";
				case Codes.OKUnexpectedStatus:
				case Codes.BusinessReplyAtQUEStatus:
					return Res.GetString("4E7B2D41-2F92-423E-8D10-D3B15E1F4ACF", "Check the 'NZ Customs Message Processor' Service Task log.");
				case Codes.InInterchangeAtQUEStatus:
				case Codes.InInterchangeProcessed:
				case Codes.InInterchangeStatusInvalid:
					return Res.GetString("2802256D-F10A-4D57-827B-26D5E633C221", "Check that the 'NZ Customs Message Processor' Service Task is running.\r\nCheck the 'NZ Customs Message Processor' Service Task log.");
				default:
					return Res.GetString("F3695FA4-2959-4F23-90A5-7C79A30E2FE5", "No additional information available");
			}
		}
	}
}
