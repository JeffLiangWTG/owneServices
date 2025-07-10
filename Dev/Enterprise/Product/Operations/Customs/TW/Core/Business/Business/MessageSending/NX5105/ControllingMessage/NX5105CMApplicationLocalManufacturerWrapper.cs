using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105CMApplicationLocalManufacturerWrapper : NX5105PartyDetailsWrapper
	{
		public NX5105CMApplicationLocalManufacturerWrapper(JobDocAddress orgAddress) : base(orgAddress?.Address, orgAddress?.Address, System.Array.Empty<string>())
		{
			jobDocAddress = Argument.NotNull(orgAddress, nameof(orgAddress));
		}

		readonly JobDocAddress jobDocAddress;

		protected override ZString Communications1IdCore => EnglishAddress.Phone;

		protected override AddressData GetEnglishAddress() => new AddressData(jobDocAddress, SharedHelper.GetEnglishLanguageCodes());

		protected override AddressData GetChineseTraditionalAddress() => new AddressData(jobDocAddress, Core.SharedConstants.Languages.ChineseTraditional);
	}
}
