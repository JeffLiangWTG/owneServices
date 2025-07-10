using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusQuota : AutoRefCusQuota
	{
		public RefCusQuota(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZXQ_StartDate = ZDateTime.MinSmallDateTimeValue;
			ZXQ_EndDate = ZDateTime.MaxSmallDateTimeValue;
		}

		[RelatedBusinessObject(nameof(DataGrouping))]
		public override ZString ZXQ_ZZZ_NKDataGrouping
		{
			get => base.ZXQ_ZZZ_NKDataGrouping;
			set => base.ZXQ_ZZZ_NKDataGrouping = value;
		}

		public RefDataGrouping DataGrouping => Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZXQ_ZZZ_NKDataGrouping);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusQuota.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public RefCusQuota GetFirst(ZString dataGroupingCode, ZDateTime date, ZString orderNumber, bool includeParentDataGrouping = true)
			{
				var query = GetFullFilter(Factory, dataGroupingCode, date, orderNumber, includeParentDataGrouping);
				return Factory.LoadTop1<RefCusQuota>(query);
			}

			public static ZQuery GetFullFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZDateTime date, ZString orderNumber, bool includeParentDataGrouping = true)
			{
				if (dataGroupingCode.IsEmpty || orderNumber.IsEmpty || !date.IsValid)
				{
					return ZQuery.NoResultQuery;
				}

				var query = new ZQuery(RefCusQuotaSchema.ZXQ_OrderNumber, orderNumber);
				query.AddToFilter(RefCusQuotaSchema.ZXQ_StartDate, SQLComparisonOperator.LessThanOrEqualTo, date);
				query.AddToFilter(RefCusQuotaSchema.ZXQ_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, date);

				if (includeParentDataGrouping)
				{
					query.AddToFilter(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, RefCusQuotaSchema.ZXQ_ZZZ_NKDataGrouping, dataGroupingCode));
				}
				else
				{
					query.AddToFilter(RefCusQuotaSchema.ZXQ_ZZZ_NKDataGrouping, dataGroupingCode);
				}

				return query;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusQuota);
		}
	}
}
