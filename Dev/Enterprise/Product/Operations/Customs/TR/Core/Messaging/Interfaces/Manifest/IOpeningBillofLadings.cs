using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IOpeningBillofLadings
	{
		///<summary>
		/// Xml Tag: dahiliNoAcilanSenet
		///</summary>
		ZString InternalNoOpenBill { get; }

		///<summary>
		/// Xml Tag: AcilanSenetNo
		///</summary>
		ZString OpenedBillNumber { get; }

		///<summary>
		/// Xml Tag: DahiliNoAcilanSenet
		///</summary>
		ZString InternalNoOpenBill2 { get; }

		///<summary>
		/// Xml Tag: OzbyAcmaSatirlari
		///</summary>
		IEnumerable<IOpeningLadingLines> OpeningLadingLines { get; }
	}
}
