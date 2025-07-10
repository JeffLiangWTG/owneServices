using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ConsolDocumentDataStoreNamesTest : TestCaseWithFactory
	{
		public void TestDataStoreNames()
		{
			var dataStoreNames = typeof(ConsolDocumentDataStoreNames).GetFields()
				.Select(f => f.GetValue(null))
				.Except(ExcludedDataStoreNames)
				.ToArray();

			var query = new ZDBOnlyQuery(typeof(StmMenuTemplatePivot));
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_DataStoreName, dataStoreNames);

			var templateSubQuery = new ZDBOnlySubQuery(typeof(StmTemplate), StmTemplateSchema.PK);
			templateSubQuery.AddToFilter(StmTemplateSchema.SO_TemplateType, Core.Constants.StmMenuItemTypes.Forms);

			var menuItemSubQuery = new ZDBOnlySubQuery(typeof(StmMenuItem), StmMenuItemSchema.PK);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.Consol));

			query.AddSubQuery(StmMenuTemplatePivotSchema.SI_SO, templateSubQuery, JoinCondition.And);
			query.AddSubQuery(StmMenuTemplatePivotSchema.SI_SU, menuItemSubQuery, JoinCondition.And);

			var pivots = Factory.Load<StmMenuTemplatePivot>(query);

			var dataStoreNamesInDatabase = pivots
				.Select(p => p.SI_DataStoreName.ToString())
				.ToArray();

			AssertContainsExactElementsInAnyOrder($"all data store names in {nameof(ConsolDocumentDataStoreNames)} are present",
				new HashSet<string>(dataStoreNamesInDatabase), dataStoreNames);
		}

		IEnumerable<string> ExcludedDataStoreNames
		{
			get
			{
				yield return ConsolDocumentDataStoreNames.DemandeDeTracingImport; // TRC doesn't have form/UI
				yield return ConsolDocumentDataStoreNames.DemandeDeTracingExport; // TRC doesn't have form/UI
			}
		}
	}
}
