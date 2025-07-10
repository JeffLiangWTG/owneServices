using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX5105CMApplicationAgentWrapper : PartyDetailsWrapper
	{
		public NX5105CMApplicationAgentWrapper(OrgAddress orgAddress) : base(orgAddress)
		{
		}

		protected override ZString IDCore => orgAddress?.Header?.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode) ?? ZString.Empty;

		protected override ZString NameCore => ChineseNameCore;

		protected override ZString TypeCodeCore => PartyIdentifierCodeList.Codes._58;

		protected override IEnumerable<ICommunication> CommunicationsCore
		{
			get
			{
				if (orgAddress != null)
				{
					if (!orgAddress.OA_Phone.IsEmpty)
					{
						yield return new CommunicationWrapper(orgAddress.OA_Phone, MessageConstants.CommunicationTypeIDs.TE);
					}
					if (!orgAddress.OA_Email.IsEmpty)
					{
						yield return new CommunicationWrapper(orgAddress.OA_Email, MessageConstants.CommunicationTypeIDs.MA);
					}
				}
			}
		}
	}
}
