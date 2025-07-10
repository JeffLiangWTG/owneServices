using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ServiceToSelectFromForPrinting))]
	sealed class ServiceToSelectFromForPrintingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			JobService service = Factory.New<JobService>();

			return new ServiceToSelectFromForPrinting(service);
		}
	}
}
