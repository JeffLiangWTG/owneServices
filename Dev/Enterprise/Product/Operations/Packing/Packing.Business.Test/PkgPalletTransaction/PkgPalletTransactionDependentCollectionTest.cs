using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPalletTransactionDependentCollection))]
	public class PkgPalletTransactionDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPalletTransactionDependentCollection>
	{
		public void TestSetDefaultsForNewChildSetsRelatedJob()
		{
			var master = Factory.New<DummyPalletMaster>();
			var collection = new PkgPalletTransactionDependentCollection(master);
			var transaction = collection.AddNew();
			AssertEquals(master.PK, transaction.KTR_ParentID);
			AssertEquals(master.TablePrefix, transaction.KTR_ParentTableCode);
		}

		public override void TestRemoveFromRelationship()
		{
			// Can't remove from this collection - setting KTR_ParentID to ZGuid.Empty will simply revert the change so this test cannot run
			Assert(true);
		}

		protected override PkgPalletTransactionDependentCollection GetCollectionToTest()
		{
			var master = Factory.New<DummyPalletMaster>();
			return new PkgPalletTransactionDependentCollection(master);
		}
	}

	#region Test Objects

	class DummyPalletMaster : DummyBusinessObject, IPalletTransactionParent
	{
		public DummyPalletMaster(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		GlbBranch IPalletTransactionParent.RelevantBranch
		{
			get { return BranchToReturn; }
		}

		public GlbBranch BranchToReturn { get; set; }

		ZString IPalletTransactionParent.GetJobDescription(Type contextType)
		{
			if (contextType == typeof(DummyPivot))
			{
				return "ZAYRYALELOLA Pivot";
			}
			else if (contextType == typeof(DummyWithPacking))
			{
				return "ZAYRYALELOLA Packing";
			}

			return "ZAYRYALELOLA";
		}

		IEnumerable<string> IPalletTransactionParent.JobReferences
		{
			get { return new[] { "ABC", "XYZ" }; }
		}

		IDocAddress IPalletTransactionParent.TransferFrom(string transferType) { return TransferFromAddressForTest; }

		public IDocAddress TransferFromAddressForTest { get; set; }

		IDocAddress IPalletTransactionParent.TransferTo(string transferType) { return TransferToAddressForTest; }

		public IDocAddress TransferToAddressForTest { get; set; }
	}

	#endregion
}
