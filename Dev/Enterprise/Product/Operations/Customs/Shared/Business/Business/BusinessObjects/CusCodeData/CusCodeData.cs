using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow()]
	public abstract class CusCodeData : AutoCusCodeData, Integration.Customs.ICusCodeData, IOldParentIDProvider
	{
		protected CusCodeData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusCodeDataTypeDecider TypeDecider = new CusCodeDataTypeDecider();

		public new class Schema : AutoCusCodeData.Schema
		{
			public const string Description = "Description";
		}

		public virtual ZString Description
		{
			get { return CY_Code.IsEmpty ? "" : Lookups.CY_CodeList.GetDescriptionFromCode(CY_Code); }
		}

		public virtual ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		public BusinessObject Parent
		{
			get { return parentLoaders.LoadBusinessObject(Factory, CY_ParentTableCode, CY_ParentID); }
			set { parentLoaders.SetTablePrefixAndPK(value, CY_ParentTableCodeInfo, CY_ParentIDInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusCodeDataLookups.CY_CodeList))]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set { base.CY_Code = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusCodeDataLookups.CY_DataList))]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString CY_ParentTableCode
		{
			get { return base.CY_ParentTableCode; }
			set { base.CY_ParentTableCode = value; }
		}

		public override ZGuid CY_ParentID
		{
			get { return base.CY_ParentID; }
			set
			{
				var oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID)
				{
					oldNoneEmptyParentID = !oldValue.IsEmpty && CY_ParentID.IsEmpty ? oldValue : ZGuid.Empty;
				}
			}
		}

		ZGuid IOldParentIDProvider.OldParentID => oldNoneEmptyParentID;
		ZGuid oldNoneEmptyParentID { get; set; }

		public void MarkParentAsNeedingValidation()
		{
			BusinessObject parent = Parent;
			if (parent != null)
			{
				parent.MarkAsNeedingValidation();
			}
		}

		public void MarkParentAsNeedingValidationIncludingChildren()
		{
			BusinessObject parent = Parent;
			if (parent != null)
			{
				parent.MarkAsNeedingValidationIncludingChildren();
			}
		}

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				if (CY_ParentTableCode.IsEmpty || !CY_ParentID.IsValid)
				{
					var fieldName = CY_ParentID.IsEmpty ? GetType().FullName + ".CY_ParentID" : GetType().FullName + ".CY_ParentTableCode";
					var parent = Parent;
					ErrorReporter.ReportOnce(fieldName + " Is Empty", string.Format("{0}{1} must be specified. The parent table code is {2}", fieldName, parent == null ? "" : string.Format((NoResString)" (Parent:{0})", parent.GetType().FullName), CY_ParentTableCode));
				}
			}

			base.OnSaving();
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				using (CY_ParentID.IsEmpty ? this.EnableOldParentID() : DisposableAction.NoAction)
				{
					FetchForLoadChildEditableObjectsIfNeeded();
					this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				}
			}
			base.Delete();
		}

		internal protected abstract TypeLoaderCollection parentLoaders { get; }

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public virtual bool CY_DataAllowWesternEuropeanCharactersOnly => true;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (CY_ParentID.IsEmpty)
			{
				CY_ParentID = ZGuid.NewZGuid();
			}

			if (CY_ParentTableCode.IsEmpty)
			{
				CY_ParentTableCode = CusCodeDataSchema.Constants.Prefix;
			}
		}
#endif
	}
}
