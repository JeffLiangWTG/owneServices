using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestsSubclassesOf(typeof(IPackingParent), typeof(PackingParentTestCase.TestExcludePackingParentHasTestCase))]
	public abstract class PackingParentTestCase<T> : TestCaseWithFactory where T : IPackingParent
	{
		#region TestPackageJobIsDeletedOnParentJobDelete

		public virtual void TestPackageJobIsDeletedOnParentJobDelete()
		{
			var parent = GetNewParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parent);
			packageJob.Packages.AddNew();

			(parent as BusinessObject).Delete();
			AssertEquals("The related PackageJob was not deleted. Override Delete() on your BusinessObject and delete it.", true, packageJob.IsDeleted);
		}

		#endregion

		#region TestParentJobSpecifiedRequiresParentJobToSupportRTUSLogNoteType

		public void TestParentJobSpecifiedRequiresParentJobToSupportRTUSRequestLogNoteType()
		{
			var parent = GetNewParent();
			if (parent.ParentJobType != ParentJobType.None)
			{
				AssertEquals("If Packing Parent has a ParentJobType specified, it should be a Note Parent and support the RTUSRequestLog Note Type.", true,
					parent is IStmNoteParent noteParent && noteParent.NoteTypes.Cast<PredefinedNoteType>().Contains(PredefinedNoteTypes.Instance.RTUSRequestLog));
			}
			else
			{
				// nothing to test
				Assert(true);
			}
		}

		#endregion

		#region TestJobNoIsNotEmptyAfterSave

		public virtual void TestJobNoIsNotEmptyAfterSave()
		{
			var parent = GetNewParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parent);

			Factory.Save(); // JobNo is generally a number fountain
			AssertEquals("After saving the PackageJob the IPackingParent.JobNo was empty. Generation of Package IDs will fail.", false, packageJob.ParentJob.JobNo.IsEmpty);
		}

		public virtual void TestNotificationTypeForInvalidContainerNumber()
		{
			var parent = GetNewParent();

			AssertEquals("NotificationTypeForInvalidContainerNumber should be correct", NotificationTypes.None, parent.NotificationTypeForInvalidContainerNumber);
		}

		#endregion

		#region Implementation

		protected abstract T GetNewParent();

		#endregion
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class PackingParentTestCase
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		[AttributeUsage(AttributeTargets.Class)]
		public sealed class TestExcludePackingParentHasTestCase : Attribute
		{
		}
	}
}
