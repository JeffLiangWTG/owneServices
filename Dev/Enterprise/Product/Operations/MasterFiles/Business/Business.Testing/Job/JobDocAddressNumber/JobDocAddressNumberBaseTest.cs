using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocAddressNumber))]
	sealed class JobDocAddressNumberBaseTest : CargoWise.EntityFramework.Testing.BusinessObjectBaseTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDocAddressNumber>();
	}
}
