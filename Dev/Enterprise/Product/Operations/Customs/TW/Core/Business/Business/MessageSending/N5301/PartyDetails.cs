using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5301
{
	public class PartyDetails : IPartyDetails
	{
		public PartyDetails(string id, string customsControlID = null, string subBoxID = null, string roleCode = null, string name = null, string chineseName = null, string typeCode = null)
		{
			ID = id;

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

			if (!string.IsNullOrEmpty(name))
			{
				Name = name;
			}

			if (!string.IsNullOrEmpty(chineseName))
			{
				ChineseName = chineseName;
			}

			if (!string.IsNullOrEmpty(typeCode))
			{
				TypeCode = typeCode;
			}
		}

		public ZString ID { get; private set; }

		public ZString Name { get; private set; }

		public ZString ChineseName { get; private set; }

		public ZString TypeCode { get; private set; }

		public ZString CustomsControlID { get; private set; }

		public ZString PaymentOnAccountBusinessID => ZString.Empty;

		public ZString RoleCode { get; private set; }

		public ZString SubBoxID { get; private set; }

		public IAddress Address => null;

		public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

		public IEnumerable<ICommunication> Communications => null;

		public ZString ContactName => null;

		public ZString MainManufacturer => null;

		public ZString UndertakeCode => null;

		public ZString OwnerName => null;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
	}
}
