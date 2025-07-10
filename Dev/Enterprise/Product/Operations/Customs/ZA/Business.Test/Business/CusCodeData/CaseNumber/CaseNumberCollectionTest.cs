using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CaseNumberCollection))]
	sealed class CaseNumberCollectionTest : CusCodeDataCollectionTest<CaseNumber>
	{
		protected override CusCodeDataCollection<CaseNumber> GetCusCodeDataCollection() => new CaseNumberCollection(Factory.New<CusEntryInstruction>());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CaseNumber>();
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			result.CY_ParentID = cusEntryInstruction.PK;
			result.CY_ParentTableCode = cusEntryInstruction.TablePrefix;
			return result;
		}
	}
}
