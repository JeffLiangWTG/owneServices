using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ManifestToOpenPack : AutoCusTRPreviousDocumentItem, IClusterKeyWorker
	{
		public ManifestToOpenPack(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		ManifestToOpenBill Bill => fBill ?? (fBill = Factory.Load<ManifestToOpenBill>(TPI_TPD));
		ManifestToOpenBill fBill;

		#region IClusterKeyWorker Members

		Type IClusterKeyWorker.ParentBizObjType => typeof(ManifestToOpenBill);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)TPI_TPDInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)TPI_ClusterKeyInfo;

		#endregion

		protected override CusTRPreviousDocumentItemValidation GetNewValidation() => new ManifestToOpenPackValidation(this);

		public new ManifestToOpenPackValidation Validation => (ManifestToOpenPackValidation)base.Validation;

		protected override CusTRPreviousDocumentItemLookups GetNewLookups() => new ManifestToOpenPackLookups(this);

		public new ManifestToOpenPackLookups Lookups => (ManifestToOpenPackLookups)base.Lookups;

		[ReadOnlyMember(nameof(BillIncludeAllItems))]
		[ResourceStringData("a937500f-9ac2-4b61-8dd7-bd367bd219b7", Caption = "Line No.")]
		public override ZShort TPI_LineNumber { get => base.TPI_LineNumber; set => base.TPI_LineNumber = value; }

		public bool BillIncludeAllItems => Bill?.TPD_IncludeAllItems ?? false;
		public bool BillIsInWarehouse => Bill?.TPD_IsInWarehouse ?? false;

		[ReadOnlyMember(nameof(IsQuantityReadOnly))]
		[ResourceStringData("ed0159be-bbcd-4cb5-9432-50553fa38ba5", Caption = "Quantity")]
		public override ZDecimal TPI_Quantity { get => base.TPI_Quantity; set => base.TPI_Quantity = value; }

		[ReadOnlyMember(nameof(IsWarehouseCodeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(ManifestToOpenPackLookups.WarehouseCodeList))]
		[ResourceStringData("d98ff649-9a86-4781-a768-564e03a60df3", Caption = "Warehouse Code")]
		public override ZString TPI_WarehouseCode { get => base.TPI_WarehouseCode; set => base.TPI_WarehouseCode = value; }

		public bool IsQuantityReadOnly => IsBillIncludeAllItemsAndLineNumberNotEmpty;

		public bool IsWarehouseCodeReadOnly => IsBillIncludeAllItemsAndLineNumberNotEmpty;

		public bool IsBillIncludeAllItemsAndLineNumberNotEmpty => BillIncludeAllItems || TPI_LineNumber.IsEmpty;
	}
}
