using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface ITax
	{
		///<summary>
		/// Xml Tag: kodu
		///</summary>
		ZString Code
		{
			get;
		}
		///<summary>
		/// Xml Tag: tanimi
		///</summary>
		ZString Description
		{
			get;
		}
		///<summary>
		/// Xml Tag: matrahi
		///</summary>
		ZDecimal Base
		{
			get;
		}
		///<summary>
		/// Xml Tag: orani
		///</summary>
		ZDecimal Rate
		{
			get;
		}
		///<summary>
		/// Xml Tag: tutari
		///</summary>
		ZDecimal Amount
		{
			get;
		}
		///<summary>
		/// Xml Tag: odemeSekli
		///</summary>
		ZString PaymentType
		{
			get;
		}
	}
}
