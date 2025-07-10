using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public abstract class HasSalesRelationFilter : ModuleFilter
	{
		#region Constructor

		protected HasSalesRelationFilter(ZString description)
			: base(description)
		{
			MultilingualDescription = ResString.GetMultilingualString("DA136F03-AE2C-4D52-9E50-6D893C6EDB92", "Has Sales Relation");
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.SalesRelationActivity; }
		}

		#endregion

		#region IsActive

		protected override void OnIsActiveChanged()
		{
			base.OnIsActiveChanged();
			if (IsActive)
			{
				TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			}
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			BoolProperty = ZBool.True;
			TypeProperty = ZString.Empty;
		}

		protected override bool IsEmptyCore => TypeProperty.IsEmpty || TypePropertyInfo.HasErrors();

		#endregion

		#region BoolProperty

		public ZBool BoolProperty
		{
			get { return boolProperty; }
			set
			{
				if (boolProperty != value)
				{
					boolProperty = value;
					BoolPropertyInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZBool boolProperty = ZBool.True;

		public abstract ZPropertyInfo BoolPropertyInfo { get; }

		#endregion

		#region TypeProperty

		[List(nameof(TypeList))]
		public virtual ZString TypeProperty
		{
			get { return typeProperty; }
			set
			{
				if (typeProperty != value)
				{
					typeProperty = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateTypeProperty();
					}
					TypePropertyInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		protected ZString typeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;

		public abstract ZPropertyInfo TypePropertyInfo { get; }

		public abstract ICodeDescriptionPairList TypeList { get; }

		public abstract ModuleIdentifier ModuleId { get; }

		#endregion

		#region Validation

		public new HasSalesRelationFilterValidation Validation
		{
			get { return (HasSalesRelationFilterValidation)base.Validation; }
		}

		#endregion

		#region Query

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException();
		}

		protected override object[] QueryDelegateParameters
		{
			get { return null; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotSupportedException();
		}

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			BoolProperty = ZBool.True;
			this.TypeProperty = RandomString(MaxLength);
		}

#endif
		#endregion
	}
}
