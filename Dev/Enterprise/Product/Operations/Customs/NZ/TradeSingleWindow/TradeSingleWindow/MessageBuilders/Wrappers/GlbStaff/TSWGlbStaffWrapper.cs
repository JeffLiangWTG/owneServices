using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class TSWGlbStaffWrapper : IDeclarant
	{
		public TSWGlbStaffWrapper(GlbStaff declarant)
		{
			this.declarant = Argument.NotNull(declarant, "Declarant cannot be null");
			wrapper = declarant.GetNZWrapper();
		}

		readonly GlbStaff declarant;
		readonly MasterFiles.Integration.Customs.NZ.INZGlbStaffWrapper wrapper;

		public ZString DeclarantID
		{
			get { return wrapper.NZBPassword.GP_UserID; }
		}

		public IEnumerable<ICommunication> Communications
		{
			get
			{
				if (!declarant.GS_EmailAddress.IsEmpty)
				{
					yield return new Communication(declarant.GS_EmailAddress, CommunicationTypeList.Codes.EM);
				}

				if (!declarant.GS_WorkPhone.IsEmpty)
				{
					yield return new Communication(declarant.GS_WorkPhone, CommunicationTypeList.Codes.TE);
				}

				if (!declarant.GS_MobilePhone.IsEmpty)
				{
					yield return new Communication(declarant.GS_MobilePhone, CommunicationTypeList.Codes.AL);
				}

				if (!declarant.GS_FaxNum.IsEmpty)
				{
					yield return new Communication(declarant.GS_FaxNum, CommunicationTypeList.Codes.FX);
				}
			}
		}

		public ZString DeclarantPinEncrypted
		{
			get { return wrapper.NZBPassword.GP_CurrentPassword.SubstringSafe(0, EDIMessage.Schema.EM_MessageOwnerMaxLength); }
		}
	}
}
