using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISPackageIdentifierWrapper : IDISPackageIdentifier
	{
		public DISPackageIdentifierWrapper(BusinessObjectFactory factory, ZString documentLabel, ZString importerOfRecordNum)
		{
			Argument.NotNull(factory, nameof(factory));
			this.factory = factory;
			this.documentLabel = documentLabel;
			this.importerOfRecordNumber = importerOfRecordNum;
		}
		readonly ZString documentLabel;
		readonly ZString importerOfRecordNumber;
		readonly BusinessObjectFactory factory;

		ZString IDISPackageIdentifier.PackageCategory
		{
			get
			{
				return factory.GetCachedValue(documentLabel + "_USDISPackageCategory_" + ZDateTime.Today.ToShortDateString(), () =>
				{
					var result = ZString.Empty;
					var uSDISPackageCategory = new RefCusCodeListAttribute.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today,
							Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, documentLabel, RefCusCodeListAttributeTypes.Codes.USDISPackageCategory).FirstOrDefault();
					if (uSDISPackageCategory != null)
					{
						result = uSDISPackageCategory.ZZE_Value;
					}
					return result;
				});
			}
		}

		ZString IDISPackageIdentifier.ImporterOfRecordNumber
		{
			get { return importerOfRecordNumber; }
		}
	}
}
