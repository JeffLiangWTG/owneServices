using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class BizObjAddInfoWithSyncPropertySupporter : BizObjWithIAddInfoManagerWithSchema, IAddInfoWithSyncPropertySupporter
	{
		public BizObjAddInfoWithSyncPropertySupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[AddInfoSyncProperty("DateTime2", typeof(ZDateTime))]
		public override ZDate Z0_DateOnly { get => base.Z0_DateOnly; set => base.Z0_DateOnly = value; }

		[AddInfoSyncProperty("AnotherDate", typeof(ZDate))]
		public override ZDateTime Z0_AnotherDate { get => AddInfoChild.Z0_AnotherDate; set => AddInfoChild.Z0_AnotherDate = value; }

		public override ZPropertyInfo Z0_AnotherDateInfo => GetWrappedZPropertyInfo(Schema.Z0_AnotherDate, (x) => AddInfoChild.Z0_AnotherDateInfo);

		[AddInfoSyncProperty("SmallDateTime", typeof(ZDate))]
		public override ZDateTime Z0_SmallDateTime { get => base.Z0_SmallDateTime; set => base.Z0_SmallDateTime = value; }

		[AddInfoSyncProperty("SparseDate2", typeof(ZDateTime))]
		public ZDateTime Z0_SparseDateWrapped { get => AddInfoChild.Z0_SparseDate; set => AddInfoChild.Z0_SparseDate = value.Date; }

		public ZPropertyInfo Z0_SparseDateWrappedInfo => GetWrappedZPropertyInfo(nameof(Z0_SparseDateWrapped), (x) => AddInfoChild.Z0_SparseDateInfo);

		IAddInfoWithSyncProperty IAddInfoWithSyncPropertySupporter.AddInfo => AddInfo;
	}
}
