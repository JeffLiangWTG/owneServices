using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SalesValueAssociationPivotFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_AssociatedEntity()
		{
			AddAssociationPivotsForFetchStrategyTest();

			var properties = new[]
				{
					"AssociatedEntity+EntityType",
					"AssociatedEntityLastEditTime",
					"AssociatedEntityLastEditUser",
				};

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgSalesValueAssociationPivotSchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgOpportunitySchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgSalesCallSchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgHeaderSchema.Constants.TableName, 1);

			foreach (var property in properties)
			{
				AssertFetchForViewDbHits(new[] { property }, expectedDbHits);
			}
			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		public void TestFetchForView_AssociatedDate()
		{
			AddAssociationPivotsForFetchStrategyTest();

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgSalesValueAssociationPivotSchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgOpportunitySchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgSalesCallSchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgSalesSchema.Constants.TableName, 1);

			var properties = new[]
				{
					"AssociatedDate",
					"AssociatingUser",
				};

			foreach (var property in properties)
			{
				AssertFetchForViewDbHits(new[] { property }, expectedDbHits);
			}
			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		#region Implementation

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits)
		{
			var viewFactory = new BusinessObjectFactory();
			var testPivotCollection = viewFactory.Load<OrgSalesValueAssociationPivot>(new ZQuery(OrgSalesValueAssociationPivotSchema.PK, testPivotPks));

			foreach (var pivot in testPivotCollection)
			{
				pivot.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var pivot in testPivotCollection)
			{
				foreach (var columnName in viewColumnNames)
				{
					var propertyDescriptorCollection = PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(OrgSalesValueAssociationPivot));
					var propertyDescriptor = propertyDescriptorCollection.Find(columnName, false);
					AssertNotNull("Precondition: Should have property [" + columnName + "]", propertyDescriptor);
					propertyDescriptor.GetValue(pivot);
				}
			}

			AssertDbHits(expectedDbHits, viewFactory);
		}

		void AddAssociationPivotsForFetchStrategyTest()
		{
			testPivotPks = new List<ZGuid>();

			for (var i = 0; i < 5; i++)
			{
				var sales = Factory.NewWithValidTestData<OrgSales>();
				testPivotPks.Add(sales.SalesAssociationPivotCollectionGlobal.AddNew(Factory.NewWithValidTestData<OrgHeader>()).PK);
				testPivotPks.Add(sales.SalesAssociationPivotCollectionGlobal.AddNew(Factory.NewWithValidTestData<OrgOpportunity>()).PK);
				testPivotPks.Add(sales.SalesAssociationPivotCollectionGlobal.AddNew(Factory.NewWithValidTestData<OrgSalesCall>()).PK);
			}

			Factory.Save();
		}

		List<ZGuid> testPivotPks;

		#endregion
	}
}
