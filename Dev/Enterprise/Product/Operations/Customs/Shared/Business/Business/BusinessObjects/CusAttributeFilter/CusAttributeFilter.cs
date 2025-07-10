using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(BaseCusClassPartPivot), "Attributes")]
	public class CusAttributeFilter : AutoCusAttributeFilter, IPartProvider, ITypeDeciderContext
	{
		public CusAttributeFilter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusAttributeFilterTypeDecider TypeDecider = new CusAttributeFilterTypeDecider();

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new AttributeFetchStrategy(this);
		}

		class AttributeFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public AttributeFetchStrategy(CusAttributeFilter attribute)
				: base(attribute)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				var attribute = (CusAttributeFilter)BusinessObject;
				Factory.AddFetchHint(CusClassPartPivotSchema.PK, attribute.BG_CI);
			}
		}

		#endregion

		public struct AttributeValue
		{
			public AttributeFilterName Name;
			public ZString Value;
		}

		public enum AttributeFilterName
		{
			Unknown,
			AT1,
			AT2,
			AT3
		}

		public AttributeFilterName FilterName
		{
			get
			{
				var result = AttributeFilterName.Unknown;
				if (IsAttribute3)
				{
					result = AttributeFilterName.AT3;
				}
				else if (IsAttribute2)
				{
					result = AttributeFilterName.AT2;
				}
				else if (IsAttribute1)
				{
					result = AttributeFilterName.AT1;
				}

				return result;
			}
		}

		public bool IsAttribute1 => BG_AttributeName == nameof(AttributeFilterName.AT1);

		public bool IsAttribute2 => BG_AttributeName == nameof(AttributeFilterName.AT2);

		public bool IsAttribute3 => BG_AttributeName == nameof(AttributeFilterName.AT3);

		public BaseCusClassPartPivot Pivot => Factory.Load<BaseCusClassPartPivot>(BG_CI);

		#region IPartProvider Members

		public MasterFiles.Business.OrgSupplierPart Part
		{
			get
			{
				var pivot = Pivot;
				return pivot != null ? pivot.Part : null;
			}
		}

		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country
		{
			get
			{
				var result = ZString.Empty;
				if (Pivot is BaseCusClassPartPivot pivot)
				{
					result = pivot.CI_RN_NKCountry;
					if (result.IsEmpty && pivot.Classification is BaseCusClassification classification)
					{
						result = classification.CC_RN_NKCountryCode;
					}
				}
				return result.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : result;
			}
		}

		#endregion

		#region Implementation

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning());

			result.Add(CusAttributeFilter.Schema.BG_CI);

			return result;
		}

		#endregion
	}
}
