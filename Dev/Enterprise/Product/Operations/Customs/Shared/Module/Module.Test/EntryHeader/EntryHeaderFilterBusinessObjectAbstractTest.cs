using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	public abstract class EntryHeaderFilterBusinessObjectAbstractTest : FilterStripBusinessObjectTestCase
	{
		public void TestJobNumberFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var jobNumberFilter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.JobNumber];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.JobNumber, jobNumberFilter.LocalizedDescription);
		}

		public void TestSubmissionDateFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var submissionDateFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.SubmissionDate];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.SubmissionDate, submissionDateFilter.LocalizedDescription);
		}

		public virtual void TestReleaseDateFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var releaseDateFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.ReleaseDate];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.ReleaseDate, releaseDateFilter.LocalizedDescription);
		}

		public virtual void TestEntryStatusFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var etryStatusFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.EntryStatus];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.EntryStatus, etryStatusFilter.LocalizedDescription);
		}

		public void TestMessageStatusFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var messageStatusFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.MessageStatus];
			AssertEquals(EntryHeaderFilterBusinessObject.Constants.MessageStatus, messageStatusFilter.LocalizedDescription);
		}

		public virtual void TestMasterBillFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var masterBillFilter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.NumberFilterTypes.MasterBill];
			AssertEquals(DeclarationFilterConstants.NumberFilterTypes.MasterBill, masterBillFilter.LocalizedDescription);
		}

		public virtual void TestHouseBillFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			var houseBillFilter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.NumberFilterTypes.HouseBill];
			AssertEquals(DeclarationFilterConstants.NumberFilterTypes.HouseBill, houseBillFilter.LocalizedDescription);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
