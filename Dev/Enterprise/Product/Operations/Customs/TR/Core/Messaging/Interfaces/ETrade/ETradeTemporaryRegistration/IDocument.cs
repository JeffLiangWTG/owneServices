using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IDocument
	{
		///<summary>
		/// Xml Tag: kodu
		///</summary>
		ZString Code
		{
			get;
		}
		///<summary>
		/// Xml Tag: belgeTarihi
		///</summary>
		ZDateTime DocumentDate
		{
			get;
		}
		///<summary>
		/// Xml Tag: referansNo
		///</summary>
		ZString ReferenceNo
		{
			get;
		}
		///<summary>
		/// Xml Tag: dogrulama
		///</summary>
		ZString Verfication
		{
			get;
		}
	}
}
