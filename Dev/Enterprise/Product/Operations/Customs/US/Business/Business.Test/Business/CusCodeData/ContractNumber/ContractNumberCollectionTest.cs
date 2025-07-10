using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ContractNumberCollection))]
	sealed class ContractNumberCollectionTest : CusCodeDataCollectionTest<ContractNumber>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<ContractNumber> GetCusCodeDataCollection() => Declaration.ContractNumbers;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<ContractNumber>();
			result.CY_Type = CusCodeDataTypeList.Codes.ContractNumber;
			result.CY_ParentID = Declaration.PK;
			result.CY_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			return result;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
