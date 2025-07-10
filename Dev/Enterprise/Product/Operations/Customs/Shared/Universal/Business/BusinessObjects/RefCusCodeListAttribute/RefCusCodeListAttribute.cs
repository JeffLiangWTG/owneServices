using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusCodeListAttribute : AutoRefCusCodeListAttribute
	{
		public RefCusCodeListAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusCodeListAttribute.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RefCusCodeListAttribute[] Load(ZString dataGroupingCode, ZDateTime valuationDate, ZString codeType, ZString code, ZString attributeName)
			{
				var result = new List<RefCusCodeListAttribute>();
				var query = new ZQuery(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, dataGroupingCode);
				query.AddToFilter(RefCusCodeListSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, valuationDate);
				query.AddToFilter(RefCusCodeListSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, valuationDate);
				query.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, codeType);
				query.AddToFilter(RefCusCodeListSchema.ZZD_Code, code);
				foreach (var cusCodeList in Factory.Load<RefCusCodeList>(query))
				{
					result.AddRange(cusCodeList.Attributes.Where(y => y.ZZE_ZXE_NKName.EqualsIgnoringCase(attributeName)));
				}
				return result.ToArray();
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefCusCodeListAttribute);
			}
		}

		[RelatedBusinessObject("CusCodeList")]
		public override ZGuid ZZE_ZZD_CodeList
		{
			get { return base.ZZE_ZZD_CodeList; }
			set { base.ZZE_ZZD_CodeList = value; }
		}

		public RefCusCodeList CusCodeList
		{
			get { return Factory.Load<RefCusCodeList>(ZZE_ZZD_CodeList); }
		}
	}
}
