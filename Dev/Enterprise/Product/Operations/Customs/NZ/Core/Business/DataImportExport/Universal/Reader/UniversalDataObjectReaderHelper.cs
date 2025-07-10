using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class UniversalDataObjectReaderHelper : DataTransfer.Universal.UniversalDataObjectReaderHelper
	{
		public UniversalDataObjectReaderHelper(UniversalObjectFactory factory, ZString sourceCountryCode)
			: base(factory, Core.Constants.CountryCodes.NewZealand, sourceCountryCode)
		{
		}

		protected override ZString? GetCustomsUnitForPackTypeCore(ZString? packType)
		{
			if (packType.HasValue)
			{
				return PackageTypeConverter.GetCustomsPackageType(packType.Value);
			}
			else
			{
				return base.GetCustomsUnitForPackTypeCore(packType);
			}
		}

		protected override ZString? GetFreightUnitForPackTypeCore(ZString? packType)
		{
			if (packType.HasValue)
			{
				return PackageTypeConverter.GetFreightPackageType(packType.Value);
			}
			else
			{
				return base.GetFreightUnitForPackTypeCore(packType);
			}
		}
	}
}
