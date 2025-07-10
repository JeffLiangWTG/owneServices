using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business
{
	public class BaseCusStorageDocPivot : AutoCusStorageDocPivot
	{
		public static readonly BaseCusStorageDocPivotTypeDecider TypeDecider = new BaseCusStorageDocPivotTypeDecider();

		public BaseCusStorageDocPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public BusinessObject Parent
		{
			get
			{
				if (parent == null || parent.IsDeleted)
				{
					parent = parentLoaders?.LoadBusinessObject(Factory, CSD_ParentTableCode, CSD_ParentID);
				}
				return parent;
			}
			set
			{
				if (parent != value)
				{
					parentLoaders?.SetTablePrefixAndPK(value, CSD_ParentTableCodeInfo, CSD_ParentIDInfo);
					parent = value;
				}
			}
		}
		BusinessObject parent;

		public override ZGuid CSD_ParentID
		{
			get => base.CSD_ParentID;
			set
			{
				var oldValue = CSD_ParentID;
				if (!IsCopying && oldValue != value)
				{
					parent = null;
				}

				base.CSD_ParentID = value;
			}
		}

		public override ZString CSD_ParentTableCode
		{
			get => base.CSD_ParentTableCode;
			set
			{
				var oldValue = CSD_ParentTableCode;
				if (!IsCopying && oldValue != value)
				{
					parent = null;
				}

				base.CSD_ParentTableCode = value;
			}
		}

		protected virtual TypeLoaderCollection parentLoaders { get; }

		public IeDoc Document
		{
			get
			{
				var docRef = CSD_StorageDocReference;
				if (document == null && docRef.IsValid)
				{
					var eDocKey = docRef.ToGuid();
					document = (Parent as ICusStorageDocPivotTypeSupporter)?.EDocCollections.Select(x => x.GetFromUniqueKey(eDocKey)).FirstOrDefault(x => x != null);
				}
				return document;
			}
		}
		IeDoc document;

		protected void ResetDocument()
		{
			document = null;
		}

		public ZString FileName => Document?.FileName ?? ZString.Empty;

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new CusStorageDocPivotUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;
	}
}
