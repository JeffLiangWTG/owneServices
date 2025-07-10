using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmFieldChangeLogCollection))]
	sealed class StmFieldChangeLogCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StmFieldChangeLogCollection>
	{
		#region Implementation

		protected override StmFieldChangeLogCollection GetCollectionToTest()
		{
			StmChangeLog changeLog = Dummy.FieldChangeLogs.AddNew();
			return changeLog.FieldChanges;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StmFieldChangeLog(Factory);
		}

		DummyWithChangeLogging Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithChangeLogging>()); }
		}
		DummyWithChangeLogging dummy;

		#endregion
	}
}
