using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class PartyWrapper : IPartyDetails
	{
		public PartyWrapper(string id = null, string name = null, string chineseName = null, string mainManufacturer = null, string typeCode = null, IAddress address = null, IEnumerable<ICommunication> communications = null, string customsControlID = null, string paymentOnAccountBusinessID = null, string roleCode = null, string subBoxID = null, ILPCOAuthorizedParty lpcoAuthorizedParty = null, string contactName = null, string ownerName = null, string undertakeCode = null, IEnumerable<IAdditionalInformation> additionalInformations = null)
		{
			ID = id;
			Name = name;
			ChineseName = chineseName;
			MainManufacturer = mainManufacturer;
			TypeCode = typeCode;
			Address = address;
			Communications = communications;
			CustomsControlID = customsControlID;
			PaymentOnAccountBusinessID = paymentOnAccountBusinessID;
			RoleCode = roleCode;
			SubBoxID = subBoxID;
			LPCOAuthorizedParty = lpcoAuthorizedParty;
			ContactName = contactName;
			OwnerName = ownerName;
			UndertakeCode = undertakeCode;
			AdditionalInformations = additionalInformations;
		}

		public ZString ID { get; }

		public ZString Name { get; }

		public ZString ChineseName { get; }

		public ZString MainManufacturer { get; }

		public ZString TypeCode { get; }

		public IAddress Address { get; }

		public IEnumerable<ICommunication> Communications { get; }

		public ZString CustomsControlID { get; }

		public ZString PaymentOnAccountBusinessID { get; }

		public ZString RoleCode { get; }

		public ZString SubBoxID { get; }

		public ILPCOAuthorizedParty LPCOAuthorizedParty { get; }

		public ZString ContactName { get; }

		public ZString OwnerName { get; }

		public ZString UndertakeCode { get; }

		public IEnumerable<IAdditionalInformation> AdditionalInformations { get; }
	}
}
