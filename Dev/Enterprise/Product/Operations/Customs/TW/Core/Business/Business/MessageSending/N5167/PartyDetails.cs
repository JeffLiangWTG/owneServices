using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5167
{
	public class PartyDetails : IPartyDetails
	{
		public PartyDetails(string id = null, string customsControlID = null, string subBoxID = null, string roleCode = null)
		{
			if (!string.IsNullOrEmpty(id))
			{
				ID = id;
			}

			if (!string.IsNullOrEmpty(customsControlID))
			{
				CustomsControlID = customsControlID;
			}

			if (!string.IsNullOrEmpty(subBoxID))
			{
				SubBoxID = subBoxID;
			}

			if (!string.IsNullOrEmpty(roleCode))
			{
				RoleCode = roleCode;
			}
		}

		public ZString ID { get; private set; }

		public ZString Name => ZString.Empty;

		public ZString ChineseName => ZString.Empty;

		public ZString TypeCode => ZString.Empty;

		public ZString CustomsControlID { get; }

		public ZString PaymentOnAccountBusinessID => ZString.Empty;

		public ZString RoleCode { get; }

		public ZString SubBoxID { get; }

		public IAddress Address => null;

		public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

		public IEnumerable<ICommunication> Communications => null;

		public ZString ContactName => ZString.Empty;

		public ZString MainManufacturer => null;

		public ZString UndertakeCode => null;

		public ZString OwnerName => null;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
	}
}
