using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface ISPTSBillLines
	{
		///<summary>
		/// Xml Tag: SatirSiraNumarasi
		///</summary
		ZInt LineOrderNo { get; }

		///<summary>
		/// Xml Tag: SatirCntNumarasi
		///</summary
		ZString LineContainerNo { get; }
	}
}
