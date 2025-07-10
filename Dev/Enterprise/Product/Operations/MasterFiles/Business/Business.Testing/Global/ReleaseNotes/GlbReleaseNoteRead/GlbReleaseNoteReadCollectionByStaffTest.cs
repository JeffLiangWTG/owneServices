using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbReleaseNoteReadCollectionByStaff))]
	sealed class GlbReleaseNoteReadCollectionByStaffTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbReleaseNoteReadCollectionByStaff(GlbStaff.CurrentUser);
		}
	}
}
