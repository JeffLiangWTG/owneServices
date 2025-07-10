using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IETradeQueryForInspectionLine : IMessageSender
	{
		/// <summary>
		/// Xml Tag :kullaniciAdi 
		/// </summary>
		///
		ZString UserName { get; }

		/// <summary>
		/// Xml Tag :kullaniciSifre 
		/// </summary>
		ZString UserPassword { get; }

		/// <summary>
		/// Xml Tag :tescilNo 
		/// </summary>
		ZString RegistrationNo { get; }
	}
}
