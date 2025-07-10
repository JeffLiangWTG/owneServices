using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Core;
using CusHAWB = Enterprise.Customs.NZ.Business.Express.CusHAWB;
using CusMAWB = Enterprise.Customs.NZ.Business.Express.CusMAWB;

namespace Enterprise.Customs.NZ.Business
{
	public sealed class CusStorageDocPivotLookups : Customs.Business.CusStorageDocPivotLookups
	{
		public CusStorageDocPivotLookups(AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		new CusStorageDocPivot Parent => (CusStorageDocPivot)base.Parent;

		public CodeDescriptionPairList AttachmentTypes
		{
			get
			{
				var isExport = (Parent.Parent as CusSCAOceanBill)?.IsExport ??
							(Parent.Parent as CusSCAHouse)?.IsExport ??
							(Parent.Parent as CusMAWB)?.IsExport ??
							(Parent.Parent as CusHAWB)?.MAWB?.IsExport ?? false;
				if (isExport)
				{
					return Factory.GetCachedValue<AttachmentTypeListPerMsg.AttachmentTypeListForCRE>();
				}
				else
				{
					return Factory.GetCachedValue<AttachmentTypeList>();
				}
			}
		}

		public StorageDocList AvailableEDocs
		{
			get
			{
				var docPivotParent = (ICusStorageDocPivotParent)Parent.Parent;
				var docList = new StorageDocList();
				docPivotParent?.EDocCollections?.ForEach(e => docList.SetupDocList(e));

				return docList;
			}
		}
	}
}
