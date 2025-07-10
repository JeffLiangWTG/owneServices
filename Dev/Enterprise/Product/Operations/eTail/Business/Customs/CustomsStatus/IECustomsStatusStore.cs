using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.eTail.Business
{
	class IECustomsStatusStore : DefaultCustomsStatusStore
	{
		public IECustomsStatusStore(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override IEnumerable<CustomsStatusInfo> GetCustomsStatusListForCodeType(string codeType)
		{
			if (codeType == RefCusCodeListTypes.Codes.CustomsStatus)
			{
				var aisEntryStatusList = new AISEntryStatusList();
				var statusListType = typeof(AISEntryStatusList);
				var codesClassType = statusListType.GetNestedType((NoResString)"Codes");
				foreach (var codeField in codesClassType.GetFields())
				{
					var codeName = codeField.Name;
					var code = (string)codeField.GetValue(null);
					var releaseStatus = code is AISEntryStatusList.Codes.Released
						? HVLVReleaseStatus.Cleared
						: HVLVReleaseStatus.Held;
					var description = aisEntryStatusList.GetDescriptionFromCode(code);

					yield return new CustomsStatusInfo(code, RefCusCodeListTypes.Codes.CustomsStatus, $"{codeName} - {description}", releaseStatus);
				}
			}
		}

		protected override string[] ImportCodeTypes => new[] { RefCusCodeListTypes.Codes.CustomsStatus };

		protected override string[] ExportCodeTypes => new[] { RefCusCodeListTypes.Codes.CustomsStatus };

		protected override string GetCodeTypeForImport(bool hasFormalDeclaration)
		{
			return RefCusCodeListTypes.Codes.CustomsStatus;
		}

		protected override ZString CountryCode => CountryCodes.Ireland;
	}
}
