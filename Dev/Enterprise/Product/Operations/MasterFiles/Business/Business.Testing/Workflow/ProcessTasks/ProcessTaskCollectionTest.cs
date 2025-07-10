using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class ProcessTaskCollectionTest<T> : BusinessObjectCollectionTestCase where T : ProcessTaskCollection
	{
		#region Implementation

		protected new T Collection
		{
			get { return (T)base.Collection; }
		}

		protected sealed override BusinessObjectCollection GetCollectionToTest()
		{
			return GetCollectionToTestCore();
		}
		protected abstract T GetCollectionToTestCore();

		protected override void SetUp()
		{
			base.SetUp();
			disposable = ProcessTaskCollection.CanCreateTaskCollection();
		}

		IDisposable disposable;

		protected override void TearDown()
		{
			base.TearDown();
			disposable.Dispose();
		}

		#endregion
	}
}
