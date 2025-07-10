using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbDepartmentCollection))]
	sealed class GlbDepartmentCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbDepartmentCollection>
	{
		protected override GlbDepartmentCollection GetCollectionToTest()
		{
			return new GlbDepartmentCollection(Factory);
		}

		public void TestAddOfItemThatIsInTheDB()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbDepartmentCollection collection = new GlbDepartmentCollection(factory, new ZQuery());
			collection.AdditionalFilter = ZQuery.NoResultQuery;
			AssertEquals("Collection should be empty", 0, collection.Count);

			GlbDepartment department = factory.Load<GlbDepartment>(TestCaseHelper.GetFirstPKFromTable(GlbDepartment.Schema.TableName));
			collection.AdditionalFilter = new ZQuery(department.PKSchemaColumn, department.PK);
			AssertEquals("Collection should have one element", 1, collection.Count);
			AssertEquals("Collection should return added deparment", department, collection[0]);
		}

		public void TestCollectionRespectsProductivityWise()
		{
			using (BrandingFactory.ConfigureTemporary(() => new CargoWiseOneBranding()))
			{
				AssertEquals("PRE: We're in CW1 Mode", "CargoWise", BrandingFactory.Instance.ProductName);

				var deptBundle = MakeDepartmentsAndReturnRelatedQueryAndFreightDepartment();

				Factory.Save();

				var collection = new GlbDepartmentCollection(Factory, deptBundle.Query);

				AssertEquals("We have all our departments in CW1 Mode", 4, collection.ToArray().Length);

				using (BrandingFactory.ConfigureTemporary(() => new ProductivityWiseBranding()))
				{
					collection = new GlbDepartmentCollection(Factory, deptBundle.Query); // remake it in PWise

					CombineAssertions("We correctly cut freighty departments in PW mode", () =>
					{
						AssertEquals(3, collection.ToArray().Length);
						AssertCollectionNotContains(deptBundle.FreightDept, collection);
					});
				}

				using (BrandingFactory.ConfigureTemporary(() => new CargoWiseOneBranding()))
				{
					collection = new GlbDepartmentCollection(Factory, deptBundle.Query); // remake it in CW1

					AssertEquals("We have all our departments in CW1 Mode", 4, collection.ToArray().Length);
				}
			}
		}

		(ZQuery Query, GlbDepartment FreightDept) MakeDepartmentsAndReturnRelatedQueryAndFreightDepartment()
		{
			var query = new ZQuery(GlbDepartmentSchema.GE_SystemCode, true)
			{
				MaximumRows = 2 // get 2 pre-existing departments
			};
			var systemDepartments = Factory.Load<GlbDepartment>(query);

			AssertEquals("PRE: We succesfully got 2 system departments", 2, systemDepartments.Length);

			var miscSystemDept = systemDepartments[0];
			miscSystemDept.GE_Misc = true;

			var notMiscSystemDept = systemDepartments[1];
			notMiscSystemDept.GE_Misc = false;

			var userCreatedFreighty = Factory.NewWithValidTestData<GlbDepartment>();
			userCreatedFreighty.GE_SystemCode = false;
			userCreatedFreighty.GE_Misc = false;
			userCreatedFreighty.GE_Sea = true; // make it obviously freighty

			var userCreatedNotFreighty = Factory.NewWithValidTestData<GlbDepartment>();
			userCreatedNotFreighty.GE_SystemCode = false;
			userCreatedNotFreighty.GE_Misc = true;

			return (new ZQuery(GlbDepartmentSchema.PK, new[] { miscSystemDept.PK, notMiscSystemDept.PK, userCreatedFreighty.PK, userCreatedNotFreighty.PK }), notMiscSystemDept);
		}
	}
}
