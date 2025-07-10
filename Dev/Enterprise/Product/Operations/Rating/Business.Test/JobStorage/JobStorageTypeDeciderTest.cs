using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Warehouse;

namespace Enterprise.Rating.Business.Testing
{
	public class JobStorageTypeDeciderTest : TestCaseWithFactory
	{
		#region TestGetTypeForBinding

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(JobStorage), new JobStorageTypeDecider().GetTypeForBinding());
		}

		#endregion

		#region TestGetTypeForNew

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(JobStorage), new JobStorageTypeDecider().GetTypeForNew());
		}

		#endregion

		#region TestGetTypeForLoad

		public void TestGetTypeForLoad()
		{
			var whsInvoice = Factory.New<IWhsInvoice>();
			var otherJobStorage = Factory.New<IWhsInvoice>();
			((JobStorage)otherJobStorage).ET_StorageType = "XYZ";

			var typeDecider = new JobStorageTypeDecider();
			AssertEquals(ObjectFactory.GetType<IWhsInvoice>(), typeDecider.GetTypeForLoad(((IBusinessObjectInternals)whsInvoice).Row, Factory));
			AssertEquals(typeof(JobStorage), typeDecider.GetTypeForLoad(((IBusinessObjectInternals)otherJobStorage).Row, Factory));
		}

		#endregion
	}
}
