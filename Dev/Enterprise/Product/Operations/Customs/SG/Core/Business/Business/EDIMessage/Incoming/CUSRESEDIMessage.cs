using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Edifact;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CUSRESEDIMessage : SGEDIMessage
	{
		public CUSRESEDIMessage(BusinessObjectFactory factory, DataRow row)
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
					messageInterpretation = GetAppropriateTradeNetVersionForCusres;
				}
				return messageInterpretation;
			}
		}
		ZString messageInterpretation;

		#region Implementation

		ZString GetAppropriateTradeNetVersionForCusres
		{
			get
			{
				var result = ZString.Empty;

				var cusres09b = (Edifact.D09B.Messages.CUSRES.CUSRESMessage)GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
				if (cusres09b != null && IsTradeNet4_1Message(cusres09b.UNH[0].MessageIdentifier.AssociationAssignedCode))
				{
					result = D09bCusres(cusres09b);
				}
				else
				{
					result = "CANCELLATION APPROVAL";
				}

				return result;
			}
		}

		ZString D09bCusres(Edifact.D09B.Messages.CUSRES.CUSRESMessage cusres09b)
		{
			var result = "CANCELLATION APPROVAL\r\n\r\n";

			var agencyOfApprovalCode = cusres09b.BGM[0].DocumentMessageName.DocumentName;
			result += GetApprovingAgency(agencyOfApprovalCode) + "\r\n";
			result += cusres09b.FTX[0].TextLiteral.FreeText1.Trim() + cusres09b.FTX[0].TextLiteral.FreeText2.Trim() + "\r\n";

			return result;
		}

		ZString GetApprovingAgency(ZString agencyCode)
		{
			switch (agencyCode)
			{
				case "AAC":
					return "Controlling Agency Approval of Request of Cancellation of Permit";
				case "AAO":
					return "Controlling Agency Approval of Request of Cancellation of Certificate";
				case "CAC":
					return "Singapore Customs Approval of Request of Cancellation of Permit";
				case "CAO":
					return "Singapore Customs Approval of Request of Cancellation of Certificate";
			}
			return agencyCode;
		}

		#endregion
	}
}
