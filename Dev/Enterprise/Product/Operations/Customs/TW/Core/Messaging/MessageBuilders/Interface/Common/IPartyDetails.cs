using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IPartyDetails
	{
		ZString ID { get; }

		ZString Name { get; }

		ZString ChineseName { get; }

		ZString TypeCode { get; }

		ZString CustomsControlID { get; }

		ZString PaymentOnAccountBusinessID { get; }

		ZString RoleCode { get; }

		ZString SubBoxID { get; }

		IAddress Address { get; }

		ILPCOAuthorizedParty LPCOAuthorizedParty { get; }

		IEnumerable<ICommunication> Communications { get; }

		ZString ContactName { get; }

		ZString OwnerName { get; }

		ZString MainManufacturer { get; }

		ZString UndertakeCode { get; }

		IEnumerable<IAdditionalInformation> AdditionalInformations { get; }
	}
}
