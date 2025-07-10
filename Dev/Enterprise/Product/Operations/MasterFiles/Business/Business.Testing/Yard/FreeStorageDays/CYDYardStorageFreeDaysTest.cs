using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Yard.Testing
{
	[TestedType(typeof(CYDYardStorageFreeDays))]
	public class CYDYardStorageFreeDaysTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDYardStorageFreeDays>();
		}
	}
}
