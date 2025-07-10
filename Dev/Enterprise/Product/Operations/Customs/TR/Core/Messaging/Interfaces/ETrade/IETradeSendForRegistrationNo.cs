using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IETradeSendForRegistrationNo : IMessageSender
	{
		/// <summary>
		/// Xml Tag: adiUnvani
		/// </summary>
		ZString DeclarantNameAndTitle { get; }

		/// <summary>
		/// Xml Tag :vergiTCNo
		/// </summary>
		ZString DeclarantIDTaxNo { get; }

		/// <summary>
		/// Xml Tag :gumrukIdaresi 
		/// </summary>
		ZString CustomsOffice { get; }

		/// <summary>
		/// Xml Tag :geciciTescilNo 
		/// </summary>
		ZString TemporaryRegistrationNo { get; }
	}
}
