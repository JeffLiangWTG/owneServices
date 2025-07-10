using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IDischargeList : IMessageSender
	{
		ZString DeclarationOwnerRepresentativeNameAndTitle { get; }
		ZString DeclarationOwnerRepresentativeTaxNo { get; }
		ZString CustomsOffice { get; }
		ZString GoodsLocationName { get; }
		ZString GoodsLocationCode { get; }
		ZString RegistrationNo { get; }
		IEnumerable<IBillBL> Bills { get; }
	}
}
