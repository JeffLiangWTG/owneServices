using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IETradeComplementaryDec : IMessageSender
	{
		/// <summary>
		/// Xml Tag: adiUnvani
		/// </summary>
		ZString DeclarationOwnerRepresentativeNameAndTitle { get; }
		/// <summary>
		/// Xml Tag: vergiTCNo
		/// </summary>
		ZString DeclarationOwnerRepresentativeTaxNo { get; }
		/// <summary>
		/// Xml Tag: beyannameNo
		/// </summary>
		ZString RegistrationNo { get; }
		IEnumerable<IBillComplementary> Bills { get; }
	}
}
