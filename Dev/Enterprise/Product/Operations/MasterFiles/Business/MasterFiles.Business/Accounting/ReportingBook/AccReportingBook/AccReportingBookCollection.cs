using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccReportingBook)]
	public class AccReportingBookCollection : BusinessObjectCollection<AccReportingBook>
	{
		public AccReportingBookCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccReportingBookCollection(BusinessObjectFactory factory, ZGuid companyPK) : base(factory)
		{
			CompanyPK = companyPK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZDBOnlyQuery(typeof(AccReportingBook));
			var subQuery = new ZDBOnlySubQuery(typeof(AccAlternateChart), AccAlternateChartSchema.PK, AccReportingBookSchema.ARB_AAC_AlternateChart);
			subQuery.AddToFilter(AccAlternateChartSchema.AAC_GC_Company, CompanyPK.IsValid ? CompanyPK : GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(JoinCondition.Or, AccAlternateChartSchema.AAC_GC_Company, DBNull.Value);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		ZGuid CompanyPK { get; }
	}
}
