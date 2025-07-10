using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AllCusEntryLineCollection<CusEntryLine>))]
	sealed class GenericAllCusEntryLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			return new AllCusEntryLineCollection<CusEntryLine>(jobDeclaration.CustomsEntryHeaders.AddNew());
		}
	}
}
