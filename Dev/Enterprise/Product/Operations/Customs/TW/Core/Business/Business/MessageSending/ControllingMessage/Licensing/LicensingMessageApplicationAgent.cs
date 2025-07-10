using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageApplicationAgent : IPartyDetails
	{
		protected OrgAddress Declarant { get; }
		protected OrgHeader OrgHeader { get; }
		protected OrgCusCode OrgCusCode { get; }

		public LicensingMessageApplicationAgent(OrgAddress declarant)
		{
			Declarant = Argument.NotNull(declarant, nameof(declarant));
			if (declarant.Header is OrgHeader orgHeader)
			{
				OrgHeader = orgHeader;
				OrgCusCode = orgHeader.GetOrgCusCode(new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID });
			}
		}

		public ZString ID => OrgCusCode?.OK_CustomsRegNo ?? ZString.Empty;

		public ZString Name => Declarant.GetChineseName();

		public ZString ChineseName => ZString.Empty;

		public ZString MainManufacturer => ZString.Empty;

		public ZString TypeCode => OrgHeaderHelper.GetPartyIdentifierCode(OrgCusCode?.OK_CodeType);

		public IAddress Address => new AddressWrapper(chineseLine: Declarant.GetChineseAddress());

		public IEnumerable<ICommunication> Communications
		{
			get
			{
				var phone = Declarant.OA_Phone;
				return phone.IsEmpty ? null : new[] { new CommunicationWrapper(phone, MessageConstants.CommunicationTypeIDs.TE) };
			}
		}

		public ZString CustomsControlID => ZString.Empty;

		public ZString PaymentOnAccountBusinessID => ZString.Empty;

		public ZString RoleCode => ZString.Empty;

		public ZString SubBoxID => ZString.Empty;

		public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

		public ZString ContactName => OrgHeader.ContactsActive.Cast<OrgContact>().FirstOrDefault()?.Name ?? ZString.Empty;

		public ZString OwnerName => ZString.Empty;

		public ZString UndertakeCode => ZString.Empty;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
	}
}
