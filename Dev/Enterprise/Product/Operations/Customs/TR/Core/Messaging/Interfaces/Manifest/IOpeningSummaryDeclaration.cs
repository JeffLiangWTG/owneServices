using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IOpeningSummaryDeclaration
	{
		///<summary>
		/// Xml Tag: AcmaSekli
		///</summary
		ZString HowToOpen { get; }

		///<summary>
		/// Xml Tag: AmbardaMi
		///</summary
		ZString InWarehouse { get; }

		///<summary>
		/// Xml Tag: BeyannameNo
		///</summary
		ZString DeclarationNo { get; }

		///<summary>
		/// Xml Tag: BaskaRejimleAcilacakMi
		///</summary
		ZString WillOpenAnotherRegime { get; }

		///<summary>
		/// Xml Tag: Aciklama
		///</summary
		ZString Explanation { get; }

		///<summary>
		/// Xml Tag: DahiliNoAcma
		///</summary
		ZString OpeningInternalNumber { get; }

		///<summary>
		/// Xml Tag: OzbyAcmaSenetleri
		///</summary
		IEnumerable<IOpeningBillofLadings> OpeningBillofLadings { get; }
	}
}
