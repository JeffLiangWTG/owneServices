using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public abstract class SingleCusSupportingInfo : CusSupportingInfo
	{
		public SingleCusSupportingInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public virtual IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_CodeInfo;
			yield return CSI_ReferenceNumberInfo;
			yield return CSI_ReferenceNumber2Info;
			yield return CSI_DescriptionInfo;
		}

		bool IsEmpty => !GetUsedFieldsInfos().Any(x => !x.Value.IsEmpty);

		public override bool IsSavedByFactory => IsInDatabase ? base.IsSavedByFactory : !(IsDeleted || IsEmpty);

		public override void OnSaving()
		{
			if (!IsDeleted && IsInDatabase && IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}
	}
}
