using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Edifact;

namespace Enterprise.Customs.SG.V4.Business
{
	public class APERAKEDIMessage : SGEDIMessage
	{
		public APERAKEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (messageInterpretation.IsEmpty)
				{
					messageInterpretation = GetAppropriateTradeNetVersionForAperak;
				}
				return messageInterpretation;
			}
		}
		ZString messageInterpretation;

		#region Implementation

		ZString GetAppropriateTradeNetVersionForAperak
		{
			get
			{
				var result = ZString.Empty;

				var aperak09b = (Edifact.D09B.Messages.APERAK.APERAKMessage)GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
				if (aperak09b != null && IsTradeNet4_1Message(aperak09b.UNH[0].MessageIdentifier.AssociationAssignedCode))
				{
					result = D09bAperak(aperak09b);
				}
				else
				{
					var aPERAK = (Edifact.D05B.Messages.APERAK.APERAKMessage)GetAutoEdifactMessageUsingNamedFactory(Sg05bEdifactMessageFactory.SG4MessageFactory, new UNOASGCharacterSet());
					if (aPERAK != null)
					{
						result = D05bAperak(aPERAK);
					}
				}

				return result;
			}
		}

		ZString D05bAperak(Edifact.D05B.Messages.APERAK.APERAKMessage aPERAK)
		{
			var result = "ERROR : ";

			result += aPERAK.BGM[0].DocumentMessageName.DocumentName.StartsWith("A") ? "CONTROLLING AGENCY" : "SINGAPORE CUSTOMS";
			result += " REJECTION";

			foreach (Edifact.D05B.Messages.APERAK.SegmentGroup4 sg4 in aPERAK.Group4)
			{
				result += "\r\n\r\n";
				result += "CODE : " + sg4.ERC[0].ApplicationErrorDetail.ApplicationErrorCode + "\r\n\r\n";
				result += (sg4.FTX[0].TextLiteral.FreeText1.Trim() + "\r\n" + sg4.FTX[0].TextLiteral.FreeText2).Trim();
			}

			return result;
		}

		ZString D09bAperak(Edifact.D09B.Messages.APERAK.APERAKMessage aperak09b)
		{
			var agencyRejectionCode = aperak09b.BGM[0].DocumentMessageName.DocumentName;
			var result = GetRejectionType(agencyRejectionCode);

			foreach (Edifact.D09B.Messages.APERAK.SegmentGroup4 sg4 in aperak09b.Group4)
			{
				result += "\r\n\r\n";
				result += "CODE : " + sg4.ERC[0].ApplicationErrorDetail.ApplicationErrorCode + "\r\n";
				result += (sg4.FTX[0].TextLiteral.FreeText1.Trim() + "\r\n" + sg4.FTX[0].TextLiteral.FreeText2).Trim();

				foreach (Edifact.D09B.Messages.APERAK.SegmentGroup5 sg5 in sg4.Group5)
				{
					result += "\r\n";
					result += "Line Number: " + sg5.RFF[0].Reference.ReferenceIdentifier;
				}
			}

			return result;
		}

		ZString GetRejectionType(ZString rejectionCode)
		{
			switch (rejectionCode)
			{
				case "AQR":
					return "Controlling Agency Query Declaration/Update";
				case "ARD":
					return "Controlling Agency Reject Declaration";
				case "ARA":
					return "Controlling Agency Reject Request for Update (Amendment)";
				case "ARC":
					return "Controlling Agency Reject Request for Update (Cancellation)";
				case "ARE":
					return "Controlling Agency Reject Request for Update (Amendment and Refund)";
				case "ARR":
					return "Controlling Agency Reject Request for Update (Refund)";
				case "CQR":
					return "Singapore Customs Query Declaration/Certificate/Update";
				case "CRD":
					return "Singapore Customs Reject Declaration";
				case "CRA":
					return "Singapore Customs Reject Request for Update (Amendment)";
				case "CRC":
					return "Singapore Customs Reject Request for Update (Cancellation)";
				case "CRE":
					return "Singapore Customs Reject Request for Update (Amendment and Refund)";
				case "CRR":
					return "Singapore Customs Reject Request for Update (Refund)";
				default:
					return "ERROR : SINGAPORE CUSTOMS REJECTION";
			}
		}

		#endregion
	}
}
