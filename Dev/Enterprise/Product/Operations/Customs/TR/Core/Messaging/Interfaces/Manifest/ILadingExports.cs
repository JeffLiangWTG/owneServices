using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface ILadingExports
	{
		///<summary>
		/// Xml Tag: BrutAgirlik
		///</summary>
		ZDecimal GrossWeight { get; }

		///<summary>
		/// Xml Tag: KapAdedi
		///</summary>
		ZInt BoxQuantity { get; }

		///<summary>
		/// Xml Tag: Numarasi
		///</summary>
		ZString ReferenceNumber { get; }

		///<summary>
		/// Xml Tag: ParcaliMi
		///</summary>
		ZString IsSubType { get; }

		///<summary>
		/// Xml Tag: Tipi
		///</summary>
		ZString IsProcedure { get; }
	}
}
