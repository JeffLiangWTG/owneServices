using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PPQForm368Data))]
	sealed class PPQForm368DataTest : DeclarationDocumentDataTest<PPQForm368Data>
	{
		public void TestDefault()
		{
			PPQForm368Data data = Factory.New<PPQForm368Data>();
			AssertEquals(JobDeclarationSchema.Constants.Prefix, data.B7_ParentTableCode);
			AssertEquals(CusAddInfoTypeAttribute.Codes.USPPQForm368Data, data.B7_Type);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			var result = factory.New<PPQForm368Data>();
			result.B7_ParentID = dec.PK;
			result.B7_ParentTableCode = dec.TablePrefix;
			result.US_PPQForm368Box13A = "HELLO";
			return result;
		}
	}
}
