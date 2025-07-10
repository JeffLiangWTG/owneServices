using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
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
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusSupportingInfo : AutoCusSupportingInfo, Integration.Customs.ICusSupportingInfo, IOldParentIDProvider, ISynchroniserReadOnlyMembersProvider, IDataModelSupporter
	{
		public CusSupportingInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusSupportingInfoTypeDecider TypeDecider = new CusSupportingInfoTypeDecider();

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public new const int CSI_DescriptionMaxLength = 512;
		}

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.CodeList))]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.CustomsOfficeList))]
		public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.ProcedureList))]
		public override ZString CSI_Procedure { get => base.CSI_Procedure; set => base.CSI_Procedure = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.StatusList))]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.SubTypeList))]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.TypeList))]
		public override ZString CSI_Type { get => base.CSI_Type; set => base.CSI_Type = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.PackTypeList))]
		public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.IssuerTypeList))]
		public override ZString CSI_IssuerType { get => base.CSI_IssuerType; set => base.CSI_IssuerType = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.UnitOfQuantityList))]
		public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.UnitOfQuantity2List))]
		public override ZString CSI_UnitOfQuantity2 { get => base.CSI_UnitOfQuantity2; set => base.CSI_UnitOfQuantity2 = value; }

		[List(nameof(Lookups) + "." + nameof(CusSupportingInfoLookups.UnitOfQuantity3List))]
		public override ZString CSI_UnitOfQuantity3 { get => base.CSI_UnitOfQuantity3; set => base.CSI_UnitOfQuantity3 = value; }

		[MaxLength(Schema.CSI_DescriptionMaxLength)]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		[LightValidationTestExempt]
		public override ZGuid CSI_ParentID
		{
			get { return base.CSI_ParentID; }
			set
			{
				var oldValue = CSI_ParentID;
				base.CSI_ParentID = value;
				if (!IsCopying && oldValue != CSI_ParentID)
				{
					oldNoneEmptyParentID = !oldValue.IsEmpty && CSI_ParentID.IsEmpty ? oldValue : ZGuid.Empty;
				}
			}
		}

		ZGuid IOldParentIDProvider.OldParentID => oldNoneEmptyParentID;
		ZGuid oldNoneEmptyParentID { get; set; }

		public BusinessObject Parent
		{
			get
			{
				if (parentIsNull)
				{
					SetParentFromParentTableCodeAndParentIDIfPossible();
				}
				return fParent;
			}
			internal set
			{
				SetParent(value);
			}
		}
		BusinessObject fParent;

		void SetParentFromParentTableCodeAndParentIDIfPossible()
		{
			if (!CSI_ParentTableCode.IsEmpty && !CSI_ParentID.IsEmpty)
			{
				fParent = Factory.Load(CSI_ParentTableCode, CSI_ParentID);
			}
		}

		protected void SetParent(BusinessObject parent)
		{
			fParent = parent;
			if (fParent != null && (CSI_ParentID != fParent.PK || CSI_ParentTableCode != fParent.TablePrefix))
			{
				CSI_ParentID = fParent.PK;
				CSI_ParentTableCode = fParent.TablePrefix;

				if (!CSI_DataModel.IsEmpty)
				{
					CSI_DataModel = ZString.Empty;
				}
			}
			if (parentIsNull)
			{
				ErrorReporter.ReportOnce("CusSupportingInfo-NullParent", string.Format(CultureInfo.InvariantCulture, "Parent of CusSupportingInfo was set to null. Type={0}; ParentID/Code={1}/{2}", GetType().FullName, CSI_ParentID, CSI_ParentTableCode));
			}
		}

		public override ZString CSI_DataModel
		{
			get => base.CSI_DataModel;
			set
			{
				this.ReportDataModelErrorIfNeeded(CSI_DataModelInfo, value);
				base.CSI_DataModel = value;
			}
		}

		bool parentIsNull => fParent == null && (!fParent?.IsNull ?? true);

		public override void OnSaving()
		{
			if (!IsInDatabase && !IsDeleted)
			{
				PopulateDataModelIfNeeded();

				if (!CSI_ParentID.IsEmpty && CSI_ParentTableCode.IsEmpty)
				{
					var fieldName = GetType().FullName + ".CSI_ParentTableCode";
					var parent = Parent;
					ErrorReporter.ReportOnce(fieldName + " Is Empty", string.Format(CultureInfo.InvariantCulture, "{0}{1} must be specified.", fieldName, parent == null ? "" : string.Format(CultureInfo.InvariantCulture, (NoResString)" (Parent:{0})", parent.GetType().FullName)));
				}
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				using (CSI_ParentID.IsEmpty ? this.EnableOldParentID() : DisposableAction.NoAction)
				{
					FetchForLoadChildEditableObjectsIfNeeded();
					this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				}
			}
			base.Delete();
		}

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());

		SetterSuspender setterSuspender;

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());
		List<string> synchroniserReadOnlyMembers;

		protected virtual bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property) => MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		#endregion

		#region Cloning
		public new CusSupportingInfo Clone()
		{
			return (CusSupportingInfo)base.Clone();
		}

		public new CusSupportingInfo Clone(BusinessObjectCloneArgs args)
		{
			return (CusSupportingInfo)base.Clone(args);
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new string[] { Schema.CSI_ParentID, Schema.CSI_ParentTableCode };
		}
		#endregion

		#region IDataModelSupporter

		public void PopulateDataModelIfNeeded() => this.PopulateDataModelIfNeededCore();

		protected virtual void PopulateDataModelIfNeededCore()
		{
			if (Parent != null && SupportsDataModelParentTablePrefixes.Contains(Parent.TablePrefix))
			{
				this.PopulateDataModelFromParentIfNeeded(Parent as IDataModelSupporter);
			}
		}

		ZString IDataModelSupporter.DataModel { get => CSI_DataModel; set => CSI_DataModel = value; }

		static string[] SupportsDataModelParentTablePrefixes => new[]
		{
			JobDeclarationSchema.Constants.Prefix,
			JobComInvoiceHeaderSchema.Constants.Prefix,
			JobComInvoiceLineSchema.Constants.Prefix,
			CusEntryInstructionSchema.Constants.Prefix,
			CusEntryLineSchema.Constants.Prefix,
			CusClassPartPivotSchema.Constants.Prefix,
			CusEntryHeaderSchema.Constants.Prefix,
		};

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (CSI_ParentTableCode.IsEmpty)
			{
				CSI_ParentTableCode = "CSI";
			}
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new CusSupportingInfoTestDataHelper();
		}

		class CusSupportingInfoTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
			{
				if (property.Name != Schema.CSI_ParentTableCode)
				{
					base.PopulateUniqueString(property, propertyPath, maxLength);
				}
			}
		}
#endif

	}
}
