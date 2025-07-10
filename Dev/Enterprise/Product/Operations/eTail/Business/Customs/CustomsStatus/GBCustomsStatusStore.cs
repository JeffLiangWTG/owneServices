using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.eTail.Business
{
	class GBCustomsStatusStore : DefaultCustomsStatusStore
	{
		public GBCustomsStatusStore(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override IEnumerable<CustomsStatusInfo> GetCustomsStatusListForCodeType(string codeType)
		{
			if (codeType == RefCusCodeListTypes.Codes.CustomsStatus)
			{
				var billStatusList = (CodeDescriptionPairList)Customs.Common.EntryStatusListHelper.EntryStatusList(Factory, "GBH7", string.Empty);
				foreach (CodeDescriptionPair status in billStatusList)
				{
					var code = status.Code;
					var description = status.Description;
					var releaseStatus = code is EntryStatusList.Codes.Clear
						? HVLVReleaseStatus.Cleared
						: HVLVReleaseStatus.Held;

					yield return new CustomsStatusInfo(code, RefCusCodeListTypes.Codes.CustomsStatus, description, releaseStatus);
				}
			}
		}

		protected override string[] ImportCodeTypes => new[] { RefCusCodeListTypes.Codes.CustomsStatus };

		protected override string GetCodeTypeForImport(bool hasFormalDeclaration)
		{
			return RefCusCodeListTypes.Codes.CustomsStatus;
		}

		protected override ZString CountryCode => CountryCodes.UnitedKingdom;
	}
}
