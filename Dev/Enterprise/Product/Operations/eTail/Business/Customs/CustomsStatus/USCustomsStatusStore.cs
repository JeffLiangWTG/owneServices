using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.Business;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business
{
	class USCustomsStatusStore : DefaultCustomsStatusStore
	{
		public USCustomsStatusStore(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override IEnumerable<CustomsStatusInfo> GetCustomsStatusListForCodeType(string codeType)
		{
			if (codeType == RefCusCodeListTypes.Codes.CustomsStatusForInterface)
			{
				var cstiCodeDescriptionPairList = new CRLReleaseStatusList();
				return (from codeDescriptionPair in cstiCodeDescriptionPairList.ToArray()
					let releaseStatus = codeDescriptionPair.Code == "REL"
						? HVLVReleaseStatus.Cleared
						: HVLVReleaseStatus.Held
					select new CustomsStatusInfo(codeDescriptionPair.Code, RefCusCodeListTypes.Codes.CustomsStatusForInterface, codeDescriptionPair.Description, releaseStatus));
			}
			else
			{
				return base.GetCustomsStatusListForCodeType(codeType);
			}
		}

		protected override ZString CountryCode => Constants.CountryCodes.UnitedStates;
	}
}
