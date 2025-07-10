using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ShipmentDocumentNamesTest : TestCaseWithFactory
	{
		public void TestDocumentNames()
		{
			var documentNames = typeof(ShipmentDocumentNames).GetFields()
				.Select(f => f.GetValue(null).ToString())
				.Except(ExcludedDocumentNames)
				.ToArray();

			var query = new ZDBOnlyQuery(typeof(StmMenuTemplatePivot));
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_DocumentTitle, documentNames);

			var templateSubQuery = new ZDBOnlySubQuery(typeof(StmTemplate), StmTemplateSchema.PK);
			templateSubQuery.AddToFilter(StmTemplateSchema.SO_TemplateType, Core.Constants.StmMenuItemTypes.Forms);

			var menuItemSubQuery = new ZDBOnlySubQuery(typeof(StmMenuItem), StmMenuItemSchema.PK);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.Shipment));

			query.AddSubQuery(StmMenuTemplatePivotSchema.SI_SO, templateSubQuery, JoinCondition.And);
			query.AddSubQuery(StmMenuTemplatePivotSchema.SI_SU, menuItemSubQuery, JoinCondition.And);

			var pivots = Factory.Load<StmMenuTemplatePivot>(query);

			var dataStoreNamesInDatabase = pivots
				.Select(p => p.SI_DocumentTitle.ToString())
				.ToArray();

			AssertContainsExactElementsInAnyOrder($"all document names in {nameof(ShipmentDocumentNames)} are present",
				new HashSet<string>(dataStoreNamesInDatabase), documentNames);
		}

		readonly string[] ExcludedDocumentNames = new[]
		{
			"Bill Of Lading" ,
			"Cargo Receipt Advice",
			"Draft Bill of Lading",
			"COOUS Certificate of Origin"	// Will be updated in the FormBuilder WI
		};
	}
}
