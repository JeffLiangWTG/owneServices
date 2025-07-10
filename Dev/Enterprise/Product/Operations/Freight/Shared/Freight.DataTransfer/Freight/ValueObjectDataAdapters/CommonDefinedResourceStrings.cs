using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.DataTransfer
{
	public static class CommonDefinedResourceStrings
	{
		public static void SetPackageTypeProperty(ZPropertyInfo propertyInfo, ZString packageType, ZString packageContextMsg, IValueObjectImportContext context, bool isSpecified)
		{
			string enterprisePackType = !String.IsNullOrEmpty(packageType) ?
				new PkgUnitXmlCodeMappingsIncludingReferenceFiles(propertyInfo.BizObj.Factory).GetEnterpriseCode(packageType, packageContextMsg, null) : string.Empty;

			if (enterprisePackType != null)
			{
				context.SetPropertyInfoValue(propertyInfo, enterprisePackType, isSpecified);
			}
			else
			{
				ZString mappedPackType = context.ConvertRawStringToZTypeValue<ZString>(packageType, ForeignKeyType.PackTypeCodeNK);

				if (mappedPackType.Length > propertyInfo.MaxLength)
				{
					string maxLengthWarningMessage = Res.GetString("47454bd2-4fe6-4a71-b76e-0af79421e90f", "{0} has exceeded the maximum length allowed by the system. When you edit this record, the package type field will error. Please use Code Mapping to map this value to the appropriate {1} package type", packageContextMsg, Core.Constants.ProductName);

					context.SetPropertyInfoValue(propertyInfo, mappedPackType, ForeignKeyType.PackTypeCodeNK, maxLengthWarningMessage);
				}
				else
				{
					string undefinedPackTypeWarningMessage = Res.GetString("2fdf98cb-4a7d-454f-bdac-5379147975f9", "{0} is not valid. When you edit this record, the package type field will error. To avoid this error you can either use Code Mapping to map this value to the appropriate {1} package type or you can add this value to the reference files (Reference Files -> Package Types)", packageContextMsg, Core.Constants.ProductName);

					if (packageType == mappedPackType)
					{
						context.Notify(new WarningNotification(undefinedPackTypeWarningMessage));
					}

					context.SetPropertyInfoValue(propertyInfo, mappedPackType, ForeignKeyType.PackTypeCodeNK);
				}
			}
		}
	}
}
