using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocAddressCollection))]
	sealed class JobDocAddressCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobDocAddressCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return JobDocAddress.New(Parent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Parent = new JobDocAddressParentForTesting(Factory);
		}

		JobDocAddressParentForTesting Parent;

		#endregion
	}
}
