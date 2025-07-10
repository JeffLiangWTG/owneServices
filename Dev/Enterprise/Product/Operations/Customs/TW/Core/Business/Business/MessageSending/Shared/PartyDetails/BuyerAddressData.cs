using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class BuyerAddressData : AddressData
	{
		public BuyerAddressData(OrgHeader header, JobDocAddress jobDocAddress, params ZString[] languages)
			: base(header, jobDocAddress, languages)
		{
		}

		protected override ZString GetEnglishAddressFormatCore() => hasEffectiveAddress ? base.GetEnglishAddressFormatCore() : ZString.Empty;

		protected override ZString GetChineseTraditionalAddressFormatCore() => hasEffectiveAddress ? base.GetChineseTraditionalAddressFormatCore() : ZString.Empty;
	}
}
