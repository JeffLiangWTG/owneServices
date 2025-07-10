using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AddInfoFilterRepositoryTest : TestCaseWithFactory
	{
		public void TestGetAddInfoQuery_WithExactComparison()
		{
			var classif1 = Factory.New<BaseCusClassification>();
			classif1.CC_LookupCode = "1=P1*OP2";
			classif1.CC_AddInfo = "Property=1*OurProperty=2";

			var classif2 = Factory.New<BaseCusClassification>();
			classif2.CC_LookupCode = "2=P2*OP1*TP3";
			classif2.CC_AddInfo = "Property=2*OurProperty=1*TheirProperty=3";

			var classif3 = Factory.New<BaseCusClassification>();
			classif3.CC_LookupCode = "3=TP3*OP2*P1";
			classif3.CC_AddInfo = "TheirProperty=3*OurProperty=2*Property=1";

			var classif4 = Factory.New<BaseCusClassification>();
			classif4.CC_LookupCode = "4=OP2*TP4*P3*YP1";
			classif4.CC_AddInfo = "OurProperty=2*TheirProperty=4*Property=3*YourProperty=1";

			var classif5 = Factory.New<BaseCusClassification>();
			classif5.CC_LookupCode = "5=P333*YP222*OP111";
			classif5.CC_AddInfo = "Property=333*YourProperty=222*OurProperty=333";

			// Property=1
			var query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, "1", CusClassificationSchema.CC_AddInfo, "Property");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			var classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 2, classifications.Length);
			AssertEquals("Loaded Classification", classif1.CC_LookupCode, classifications[0].CC_LookupCode);
			AssertEquals("Loaded Classification", classif3.CC_LookupCode, classifications[1].CC_LookupCode);

			// Property=2
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, "2", CusClassificationSchema.CC_AddInfo, "Property");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif2.CC_LookupCode, classifications[0].CC_LookupCode);

			// Property=3
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, "3", CusClassificationSchema.CC_AddInfo, "Property");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif4.CC_LookupCode, classifications[0].CC_LookupCode);

			// Property=4
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, "4", CusClassificationSchema.CC_AddInfo, "Property");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 0, classifications.Length);

			// OurProperty=1
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, "1", CusClassificationSchema.CC_AddInfo, "OurProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif2.CC_LookupCode, classifications[0].CC_LookupCode);

			// OurProperty=2
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, "2", CusClassificationSchema.CC_AddInfo, "OurProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 3, classifications.Length);
			AssertEquals("Loaded Classification", classif1.CC_LookupCode, classifications[0].CC_LookupCode);
			AssertEquals("Loaded Classification", classif3.CC_LookupCode, classifications[1].CC_LookupCode);
			AssertEquals("Loaded Classification", classif4.CC_LookupCode, classifications[2].CC_LookupCode);

			// TheirProperty=1
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, "1", CusClassificationSchema.CC_AddInfo, "TheirProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 0, classifications.Length);

			// YourProperty=1
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, "1", CusClassificationSchema.CC_AddInfo, "YourProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif4.CC_LookupCode, classifications[0].CC_LookupCode);

			// YourProperty=222
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, "222", CusClassificationSchema.CC_AddInfo, "YourProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif5.CC_LookupCode, classifications[0].CC_LookupCode);
		}

		public void TestGetAddInfoQuery_WithStartsWithComparison()
		{
			var classif1 = Factory.New<BaseCusClassification>();
			classif1.CC_LookupCode = "1=P11*OP22";
			classif1.CC_AddInfo = "Property=11*OurProperty=22";

			var classif2 = Factory.New<BaseCusClassification>();
			classif2.CC_LookupCode = "2=P22*OP123*TP33";
			classif2.CC_AddInfo = "Property=22*OurProperty=123*TheirProperty=33";

			var classif3 = Factory.New<BaseCusClassification>();
			classif3.CC_LookupCode = "3=TP13*OP31*YP22*P44";
			classif3.CC_AddInfo = "TheirProperty=13*OurProperty=31*YourProperty=22*Property=44";

			var classif4 = Factory.New<BaseCusClassification>();
			classif4.CC_LookupCode = "4=OP222*TP44*P31*YP11";
			classif4.CC_AddInfo = "OurProperty=222*TheirProperty=44*Property=31*YourProperty=11";

			var classif5 = Factory.New<BaseCusClassification>();
			classif5.CC_LookupCode = "5=YP2233*TP3322*OP23*P144";
			classif5.CC_AddInfo = "YourProperty=2233*TheirProperty=3322*OurProperty=23*Property=144";

			// Property LIKE 1%
			var query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "1", CusClassificationSchema.CC_AddInfo, "Property");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			var classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 2, classifications.Length);
			AssertEquals("Loaded Classification", classif1.CC_LookupCode, classifications[0].CC_LookupCode);
			AssertEquals("Loaded Classification", classif5.CC_LookupCode, classifications[1].CC_LookupCode);

			// Property LIKE 11%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "11", CusClassificationSchema.CC_AddInfo, "Property");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif1.CC_LookupCode, classifications[0].CC_LookupCode);

			// Property LIKE 2%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "2", CusClassificationSchema.CC_AddInfo, "Property");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif2.CC_LookupCode, classifications[0].CC_LookupCode);

			// Property LIKE 33%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "33", CusClassificationSchema.CC_AddInfo, "Property");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 0, classifications.Length);

			// OurProperty LIKE 1%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "1", CusClassificationSchema.CC_AddInfo, "OurProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif2.CC_LookupCode, classifications[0].CC_LookupCode);

			// OurProperty LIKE 2%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "2", CusClassificationSchema.CC_AddInfo, "OurProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 3, classifications.Length);
			AssertEquals("Loaded Classification", classif1.CC_LookupCode, classifications[0].CC_LookupCode);
			AssertEquals("Loaded Classification", classif4.CC_LookupCode, classifications[1].CC_LookupCode);
			AssertEquals("Loaded Classification", classif5.CC_LookupCode, classifications[2].CC_LookupCode);

			// OurProperty LIKE 22%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "22", CusClassificationSchema.CC_AddInfo, "OurProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 2, classifications.Length);
			AssertEquals("Loaded Classification", classif1.CC_LookupCode, classifications[0].CC_LookupCode);
			AssertEquals("Loaded Classification", classif4.CC_LookupCode, classifications[1].CC_LookupCode);

			// OurProperty LIKE 3%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "3", CusClassificationSchema.CC_AddInfo, "OurProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif3.CC_LookupCode, classifications[0].CC_LookupCode);

			// OurProperty LIKE 33%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "33", CusClassificationSchema.CC_AddInfo, "OurProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 0, classifications.Length);

			// TheirProperty LIKE 332%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "332", CusClassificationSchema.CC_AddInfo, "TheirProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 1, classifications.Length);
			AssertEquals("Loaded Classification", classif5.CC_LookupCode, classifications[0].CC_LookupCode);

			// YourProperty LIKE 22%
			query = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, "22", CusClassificationSchema.CC_AddInfo, "YourProperty");
			query.OrderBy = CusClassificationSchema.CC_LookupCode.Name;
			classifications = Factory.Load<BaseCusClassification>(query);
			AssertEquals("Loaded Classification Count", 2, classifications.Length);
			AssertEquals("Loaded Classification", classif3.CC_LookupCode, classifications[0].CC_LookupCode);
			AssertEquals("Loaded Classification", classif5.CC_LookupCode, classifications[1].CC_LookupCode);
		}
	}
}
