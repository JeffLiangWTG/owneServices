using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IBillComplementary
	{
		/// <summary>
		/// Xml Tag: tasimaSenediNo
		/// </summary>
		ZString BillNo { get; }
		/// <summary>
		/// Xml Tag: aliciVergiTCNo
		/// </summary>
		ZString ConsigneeTaxIDNo { get; }
		/// <summary>
		/// Xml Tag: teslimTarihi
		/// </summary>
		ZDateTime DeliveryDate { get; }
	}
}
