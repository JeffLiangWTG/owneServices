using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmChangeLogCollection))]
	sealed class StmChangeLogCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionRelationship()
		{
			StmChangeLog changeLog = Collection.AddNew();
			AssertEquals("SY_ParentID set", Dummy.PK, changeLog.SY_ParentID);
			AssertEquals("SY_ParentTableCode set", DummyBizoSchema.Constants.Prefix, changeLog.SY_ParentTableCode);
		}

		#region Implementation

		new StmChangeLogCollection Collection
		{
			get { return (StmChangeLogCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmChangeLogCollection(Dummy);
		}

		DummyBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyBusinessObject>()); }
		}
		DummyBusinessObject dummy;

		#endregion
	}
}
