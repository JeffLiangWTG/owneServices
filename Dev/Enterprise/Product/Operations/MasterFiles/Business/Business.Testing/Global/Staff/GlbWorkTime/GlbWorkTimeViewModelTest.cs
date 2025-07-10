using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbWorkTimeViewModel))]
	sealed class GlbWorkTimeViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var staff = Factory.New<GlbStaff>();
			return new GlbWorkTimeViewModel(staff.WorkTimes, false);
		}
	}
}
