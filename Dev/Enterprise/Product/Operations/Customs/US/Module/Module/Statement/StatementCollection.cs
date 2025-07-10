using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	class StatementCollection : BusinessObjectCollection<CusStatementHeader>
	{
		public StatementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
			return result;
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new CusStatementHeaderCollectionFetchStrategy(this);

		sealed class CusStatementHeaderCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
		{
			public CusStatementHeaderCollectionFetchStrategy(StatementCollection collection)
				: base(collection)
			{
			}

			new StatementCollection Collection => (StatementCollection)base.Collection;

			protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
			{
				base.FetchForViewCore(businessObjects, columns);
				var factory = Collection.Factory;
				var cusStatementLineChargeRequiredFetchForView = columns.FirstOrDefault(x => x.ColumnName == CusStatementHeader.Schema.TotalDeferredTax) != null;
				if (cusStatementLineChargeRequiredFetchForView)
				{
					var monthlyStatements = new List<CusStatementHeader>();
					var dailyStatementPKs = new List<ZGuid>();
					foreach (CusStatementHeader statement in businessObjects)
					{
						if (statement.IsMonthlyStatement)
						{
							monthlyStatements.Add(statement);
						}
						else
						{
							dailyStatementPKs.Add(statement.PK);
						}
					}

					if (monthlyStatements.Count > 0)
					{
						var dailyStatements = factory.Load<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_B2_PeriodicStatement, monthlyStatements.Select(x => x.PK)));
						dailyStatementPKs.AddRange(dailyStatements.Select(x => x.PK));
					}
					var linesQuery = new ZQuery(CusStatementLineSchema.B3_B2, dailyStatementPKs);
					var lines = factory.Load<CusStatementLine>(linesQuery);
					if (lines.Length > 0)
					{
						foreach (var line in lines)
						{
							factory.AddFetchHint(CusStatementLineChargeSchema.B4_B3, line.PK);
						}
					}
				}
			}
		}
	}
}
