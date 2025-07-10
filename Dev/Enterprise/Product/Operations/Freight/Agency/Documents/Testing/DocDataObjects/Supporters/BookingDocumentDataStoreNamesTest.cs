using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	public class BookingDocumentDataStoreNamesTest : TestCaseWithFactory
	{
		public void TestDataStoreNames()
		{
			var dataStoreNames = typeof(BookingDocumentDataStoreNames).GetFields()
				.Select(f => f.GetValue(null))
				.ToArray();

			var query = new ZDBOnlyQuery(typeof(StmMenuTemplatePivot));
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_DataStoreName, dataStoreNames);

			var templateSubQuery = new ZDBOnlySubQuery(typeof(StmTemplate), StmTemplateSchema.PK);
			templateSubQuery.AddToFilter(StmTemplateSchema.SO_TemplateType, Core.Constants.StmMenuItemTypes.Forms);

			var menuItemSubQuery = new ZDBOnlySubQuery(typeof(StmMenuItem), StmMenuItemSchema.PK);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.AgencyBooking));

			query.AddSubQuery(StmMenuTemplatePivotSchema.SI_SO, templateSubQuery, JoinCondition.And);
			query.AddSubQuery(StmMenuTemplatePivotSchema.SI_SU, menuItemSubQuery, JoinCondition.And);

			var pivots = Factory.Load<StmMenuTemplatePivot>(query);

			var dataStoreNamesInDatabase = pivots
				.Select(p => p.SI_DataStoreName.ToString())
				.ToArray();

			AssertContainsExactElementsInAnyOrder($"all data store names in {nameof(BookingDocumentDataStoreNames)} are present",
				new HashSet<string>(dataStoreNamesInDatabase), dataStoreNames);
		}
	}
}
