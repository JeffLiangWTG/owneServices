using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class ExporterAddressData : AddressData
	{
		public ExporterAddressData(OrgHeader header, JobDocAddress jobDocAddress, params ZString[] languages)
			: base(header, jobDocAddress, languages)
		{
		}

		protected override ZString GetEnglishAddressFormatCore() => hasEffectiveAddress ? base.GetEnglishAddressFormatCore() : ZString.Empty;

		protected override ZString GetChineseTraditionalAddressFormatCore() => hasEffectiveAddress ? base.GetChineseTraditionalAddressFormatCore() : ZString.Empty;
	}
}
