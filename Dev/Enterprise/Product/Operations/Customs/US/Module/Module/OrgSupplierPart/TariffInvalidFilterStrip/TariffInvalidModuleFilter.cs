using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module
{
	public delegate ZQuery GetTariffInvalidQuery(ZDate expiredDate);
	public class TariffInvalidModuleFilter : ModuleTextFilter
	{
		public TariffInvalidModuleFilter(ZString description, GetTariffInvalidQuery queryDelegate)
			: base(description, DummyQuery)
		{
			tariffInvalidQuery = queryDelegate;
		}
		readonly GetTariffInvalidQuery tariffInvalidQuery;

		public ZDate ExpiredDate
		{
			get => fExpiredDate;
			set
			{
				var oldValue = ExpiredDate;
				if (oldValue != value)
				{
					fExpiredDate = value;
					ExpiredDateInfo.RefreshBinding();
				}
			}
		}
		ZDate fExpiredDate;

		public ZPropertyInfo ExpiredDateInfo => GetZPropertyInfo(nameof(ExpiredDate));

		protected override ZQuery GetQuery()
		{
			var query = new ZQuery();
			if (!IsEmpty)
			{
				query = tariffInvalidQuery(ExpiredDate);
			}
			return query;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			ExpiredDate = ZDate.Today;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && ExpiredDate.IsEmpty;

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			// currently not serialised
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			// currently not serialised
		}

		static ZQuery DummyQuery(SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery();
	}
}
