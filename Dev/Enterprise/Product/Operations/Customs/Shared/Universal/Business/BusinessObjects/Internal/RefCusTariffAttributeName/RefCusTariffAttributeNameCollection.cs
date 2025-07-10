using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffAttributeNameCollection : ActiveBusinessObjectCollection<RefCusTariffAttributeName>
	{
		public RefCusTariffAttributeNameCollection(ChildTariffViewCollection master) : base(master.Factory, GetQuery(master)) { }

		protected override bool AllowNew => false;

		static ZQuery GetQuery(ChildTariffViewCollection master)
		{
			ZQuery query;
			if (!string.IsNullOrEmpty(master.TariffType))
			{
				query = new ZQuery(RefCusTariffAttributeNameSchema.ZY6_ZZZ_NKDataGrouping, master.DataGroupingCode);
				query.AddToFilter(RefCusTariffAttributeNameSchema.ZY6_ZZI_NKTariffType, SQLComparisonOperator.StartsWith, master.TariffType);
				query.AddToFilter(RefCusTariffAttributeNameSchema.ZY6_ColumnCaption, SQLComparisonOperator.IsNotBlank, ZString.Empty);
			}
			else
			{
				query = new ZQuery();
			}
			return query;
		}
	}
}
