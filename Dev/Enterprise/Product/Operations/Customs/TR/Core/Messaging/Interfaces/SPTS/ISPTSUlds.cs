using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface ISPTSUlds
	{
		///<summary>
		/// Xml Tag: UldNumarasi
		///</summary
		ZString UldNumber { get; }

		///<summary>
		/// Xml Tag: UldSiraNumarasi
		///</summary
		ZInt UldOrderNo { get; }
	}
}
