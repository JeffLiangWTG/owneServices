using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface ILadingLines
	{
		///<summary>
		/// Xml Tag: BrutAgirlik
		///</summary>
		ZDecimal GrossWeight { get; }

		///<summary>
		/// Xml Tag: EsyaBilgileri
		///</summary>
		IEnumerable<IGoodsInformation> GoodsInformation { get; }

		///<summary>
		/// Xml Tag: KapAdedi
		///</summary>
		ZInt PackQuantity { get; }

		///<summary>
		/// Xml Tag: KapCinsi
		///</summary>
		ZString PackType { get; }

		///<summary>
		/// Xml Tag: KonteynerTipi
		///</summary>
		ZString ContainerType { get; }

		///<summary>
		/// Xml Tag: MarkaNo
		///</summary>
		ZString ContainerNumber { get; }

		///<summary>
		/// Xml Tag: MuhurNumarasi
		///</summary>
		ZString SealNumber { get; }

		///<summary>
		/// Xml Tag: NetAgirlik
		///</summary>
		ZDecimal NetWeight { get; }

		///<summary>
		/// Xml Tag: OlcuBirimi
		///</summary>
		ZString WeightUQ { get; }

		///<summary>
		/// Xml Tag: SatirNo
		///</summary>
		ZInt LineNo { get; }

		///<summary>
		/// Xml Tag: KonteynerYukDurumu
		///</summary>
		ZString ContainerLoadStatus { get; }
	}
}
