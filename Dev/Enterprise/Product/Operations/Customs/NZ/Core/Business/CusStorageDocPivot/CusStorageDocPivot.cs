using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Schema;
using CusHAWB = Enterprise.Customs.NZ.Business.Express.CusHAWB;
using CusMAWB = Enterprise.Customs.NZ.Business.Express.CusMAWB;

namespace Enterprise.Customs.NZ.Business
{
	public sealed class CusStorageDocPivot : BaseCusStorageDocPivot, ITSWAttachment
	{
		public CusStorageDocPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("F283F39D-8585-4B2D-959B-3EBE0C51A206", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(CusStorageDocPivotLookups.AttachmentTypes))]
		public override ZString CSD_DocType
		{
			get => base.CSD_DocType;
			set => base.CSD_DocType = value;
		}

		[ResourceStringData("1141804A-F1CE-48A3-A39F-B863CD1BFFF2", Caption = "eDoc")]
		[List(nameof(Lookups) + "." + nameof(CusStorageDocPivotLookups.AvailableEDocs), "PK", "Code", AllowOnlyTheseValues = true)]
		public override ZGuid CSD_StorageDocReference
		{
			get => base.CSD_StorageDocReference;
			set => base.CSD_StorageDocReference = value;
		}

		#region Lookups

		protected override Customs.Business.CusStorageDocPivotLookups GetNewLookups()
		{
			return new CusStorageDocPivotLookups(this);
		}

		public new CusStorageDocPivotLookups Lookups => (CusStorageDocPivotLookups)base.Lookups;

		#endregion

		#region Validation

		protected override Customs.Business.CusStorageDocPivotValidation GetNewValidation()
		{
			switch (CSD_ParentTableCode)
			{
				case CusSCAOceanBillSchema.Constants.Prefix:
					return new CusStorageDocPivotSCAOceanBillValidation(this);
				case CusSCAHouseSchema.Constants.Prefix:
					return new CusStorageDocPivotSCAHouseValidation(this);
				case CusMAWBSchema.Constants.Prefix:
					return new CusStorageDocPivotMAWBValidation(this);
				case CusHAWBSchema.Constants.Prefix:
					return new CusStorageDocPivotHAWBValidation(this);
				default:
					return new CusStorageDocPivotValidation(this);
			}
		}

		public new CusStorageDocPivotValidation Validation => (CusStorageDocPivotValidation)base.Validation;

		#endregion

		protected override TypeLoaderCollection parentLoaders
		{
			get
			{
				var result = new TypeLoaderCollection();
				result.Add(typeof(CusSCAOceanBill));
				result.Add(typeof(CusSCAHouse));
				result.Add(typeof(CusMAWB));
				result.Add(typeof(CusHAWB));

				return result;
			}
		}

		#region ITSWAttachment

		ZGuid ITSWAttachment.UniqueIdentifier => CSD_StorageDocReference;

		ZString ITSWAttachment.DocType => CSD_DocType;

		ZString ITSWAttachment.FileName => Lookups.AvailableEDocs[CSD_StorageDocReference]?.Code ?? ZString.Empty;

		ZBool ITSWAttachment.FileTooBig => Lookups.AvailableEDocs[CSD_StorageDocReference]?.Description?.EndsWith(StorageDocList.FileTooBigIndicator, StringComparison.OrdinalIgnoreCase) ?? false;

		#endregion
	}
}
