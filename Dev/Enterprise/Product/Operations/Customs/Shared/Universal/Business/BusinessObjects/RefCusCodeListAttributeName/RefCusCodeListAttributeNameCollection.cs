using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeListAttributeNameCollection : ActiveBusinessObjectCollection<RefCusCodeListAttributeName>
	{
		public RefCusCodeListAttributeNameCollection(ZZRefCusCodeListCombinedCollection master) : base(master.Factory, GetQuery(master)) { }

		protected override bool AllowNew => false;

		static ZQuery GetQuery(ZZRefCusCodeListCombinedCollection master)
		{
			ZQuery query;
			if (master.DataGroupingCodes != null && master.CodeTypes != null)
			{
				query = new ZQuery(RefCusCodeListAttributeNameSchema.ZXE_ZZZ_NKDataGrouping, master.DataGroupingCodes);
				query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_ZZK_NKCodeType, master.CodeTypes);
				query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_IsValueMandatory, true);
				query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_ColumnCaption, SQLComparisonOperator.IsNotBlank, ZString.Empty);
			}
			else
			{
				query = ZQuery.NoResultQuery;
			}
			return query;
		}
	}
}
