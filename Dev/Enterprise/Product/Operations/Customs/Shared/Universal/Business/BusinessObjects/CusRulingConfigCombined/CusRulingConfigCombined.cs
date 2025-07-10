using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.RefCusRulingConfigCategories.Codes;
using static Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;
using IBaseJobComInvoiceLine = Enterprise.Integration.Customs.IBaseJobComInvoiceLine;

namespace Enterprise.Customs.Universal
{
	public class CusRulingConfigCombined : AutoCusRulingConfigCombined, ICanDelete
	{
		public CusRulingConfigCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		public ZZRefCusRulingCombined Ruling => Factory.Load<ZZRefCusRulingCombined>(ZZY_ZZX_CusRuling);

		public IBaseJobComInvoiceLine InvoiceLine => Factory.Load<IBaseJobComInvoiceLine>(ZZY_JI_InvoiceLine);

		public bool IsSystem
		{
			get
			{
				var ruling = Ruling;
				return ruling != null && ruling.IsSystem;
			}
		}

		public ZString CategoryDescription
		{
			get
			{
				return Lookups.RulingConfigCategoryList.GetDescriptionFromCode(ZZY_Category);
			}
		}

		public bool IsCategoryValid
		{
			get { return !ZZY_Category.IsEmpty && Lookups.RulingConfigCategoryList.ContainsCode(ZZY_Category); }
		}

		public bool IsDTY => ZZY_Category == DTY;
		public bool IsGST => ZZY_Category == GST;
		public bool IsSIM => ZZY_Category == SIM;
		public bool IsEXC => ZZY_Category == EXC;
		public bool IsDAT => ZZY_Category == DAT;
		public bool IsEXD => ZZY_Category == EXD;

		#region ValueFieldType
		public ZString ValueFieldType => Factory.GetCachedValue(ZString.Format((NoResString)"CusRulingConfigCombined|Type:{0}-Category{1}", ZZY_Type, ZZY_Category), GetValueFieldTypeCore);

		protected virtual ZString GetValueFieldTypeCore() => nameof(FieldType.Text);
		public virtual ZInt ZZY_ValueDecimalPlaces => 2;
		#endregion

		#region ZZY_RateReadOnly
		public bool IsRateReadOnly => Factory.GetCachedValue(GetRateReadOnlyCacheKey(), GetIsRateReadOnlyCore);

		protected virtual ZString GetRateReadOnlyCacheKey() => ZString.Empty;

		protected virtual bool GetIsRateReadOnlyCore() => false;
		#endregion

		#region ZZY_ValueReadOnly
		public bool IsValueReadOnly => Factory.GetCachedValue(GetValueReadOnlyCacheKey(), GetIsValueReadOnlyCore);

		protected virtual ZString GetValueReadOnlyCacheKey() => ZString.Empty;

		protected virtual bool GetIsValueReadOnlyCore() => false;
		#endregion

		#endregion

		#region Override Properties

		[RelatedBusinessObject("Ruling")]
		public override ZGuid ZZY_ZZX_CusRuling
		{
			get { return base.ZZY_ZZX_CusRuling; }
			set { base.ZZY_ZZX_CusRuling = value; }
		}

		[List("Lookups.RulingConfigCategoryList")]
		public override ZString ZZY_Category
		{
			get { return base.ZZY_Category; }
			set
			{
				base.ZZY_Category = value;
				ClearRateAndValueIfNeeded();
			}
		}

		[List("Lookups.RulingConfigTypeList")]
		public override ZString ZZY_Type
		{
			get { return base.ZZY_Type; }
			set
			{
				base.ZZY_Type = value;
				ClearRateAndValueIfNeeded();
			}
		}

		[ReadOnlyMember(nameof(IsRateReadOnly))]
		public override ZDecimal ZZY_Rate
		{
			get { return base.ZZY_Rate; }
			set { base.ZZY_Rate = value; }
		}

		[List("Lookups.RulingConfigValueList")]
		[ReadOnlyMember(nameof(IsValueReadOnly))]
		public override ZString ZZY_Value
		{
			get { return base.ZZY_Value; }
			set { base.ZZY_Value = value; }
		}

		void ClearRateAndValueIfNeeded()
		{
			if (IsRateReadOnly)
			{
				ZZY_Rate = ZDecimal.Zero;
			}
			if (IsValueReadOnly)
			{
				ZZY_Value = ZString.Empty;
			}
		}

		public override bool ReadOnly
		{
			get
			{
				var ruling = Ruling;
				return (ruling != null && ruling.ReadOnly) || base.ReadOnly;
			}
			set { base.ReadOnly = value; }
		}

		#endregion

		#region Override Method

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				CusRulingConfigCombinedSchema.Constants.ZZY_JI_InvoiceLine,
				CusRulingConfigCombinedSchema.Constants.ZZY_ZZX_CusRuling
			};

			return result;
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get
			{
				var ruling = Ruling;
				return ruling == null || ((ICanDelete)ruling).CanDelete;
			}
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get
			{
				var ruling = Ruling;
				return ruling == null ? (NoResString)string.Empty : ((ICanDelete)ruling).ReasonForNotAbleToDelete;
			}
		}

		#endregion

#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZZY_Category = DTY;
			ZZY_Type = AcceptAmount;
			ZZY_Rate = 0;
			ZZY_Value = RefCusRulingConfigValuesForAcceptType.Codes.X;
		}

#endif
	}
}
