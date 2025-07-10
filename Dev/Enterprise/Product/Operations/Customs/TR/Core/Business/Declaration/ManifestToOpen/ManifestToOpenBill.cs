using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ManifestToOpenBill : AutoCusTRPreviousDocument, IClusterKeyWorker
	{
		public ManifestToOpenBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		ManifestToOpenHeader Manifest => fManifest ?? (fManifest = Factory.Load<ManifestToOpenHeader>(TPD_CE_EntryNumber));
		ManifestToOpenHeader fManifest;

		JobDeclaration Declaration => fDeclaration ?? (fDeclaration = Factory.Load<JobDeclaration>(Manifest.CE_ParentID));
		JobDeclaration fDeclaration;

		#region IClusterKeyWorker implementations

		Type IClusterKeyWorker.ParentBizObjType => typeof(JobDeclaration);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)Manifest.CE_ParentIDInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(ManifestToOpenPack), CusTRPreviousDocumentItemSchema.TPI_TPD);
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)TPD_ClusterKeyInfo;

		#endregion

		[ChildEditable(true)]
		public ManifestToOpenPackCollection Packs
		{
			get
			{
				if (packs == null)
				{
					packs = new ManifestToOpenPackCollection(this);
					RegisterEditableChildObject(packs);
				}
				return packs;
			}
		}
		ManifestToOpenPackCollection packs;

		[ResourceStringData("c32e5295-d812-4aad-af33-6c3f4037b75b", Caption = "Bill No")]
		public override ZString TPD_DocumentNumber
		{
			get => base.TPD_DocumentNumber;
			set
			{
				var oldValue = base.TPD_DocumentNumber;
				base.TPD_DocumentNumber = value;
				if (!IsCopying && oldValue != TPD_DocumentNumber)
				{
					Manifest?.MarkAsNeedingValidation();
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("cb92008f-6c8a-4706-9e65-2c108866720a", Caption = "All")]
		public override ZBool TPD_IncludeAllItems
		{
			get => base.TPD_IncludeAllItems;
			set
			{
				var oldValue = base.TPD_IncludeAllItems;
				base.TPD_IncludeAllItems = value;
				if (!IsCopying && oldValue != TPD_IncludeAllItems)
				{
					Packs.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("39a3c86d-ebd4-48be-96be-487387e75d61", Caption = "In Warehouse?")]
		public override ZBool TPD_IsInWarehouse
		{
			get => base.TPD_IsInWarehouse;
			set
			{
				var oldValue = base.TPD_IsInWarehouse;
				base.TPD_IsInWarehouse = value;
				if (!IsCopying && oldValue != TPD_IsInWarehouse)
				{
					Packs.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("16511926-75c3-42f8-a64d-e82e67d8cbe6", Caption = "Other Procedure?")]
		public override ZBool TPD_IsOtherProcedure { get => base.TPD_IsOtherProcedure; set => base.TPD_IsOtherProcedure = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			TPD_IncludeAllItems = true;
			TPD_IsInWarehouse = true;
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				Packs.DeleteAll();
				base.Delete();
			}
		}
	}
}
