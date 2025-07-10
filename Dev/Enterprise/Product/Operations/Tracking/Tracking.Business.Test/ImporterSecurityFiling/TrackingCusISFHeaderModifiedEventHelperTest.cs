using CargoWise.EntityFramework;
using Enterprise.Tracking.Business.ImporterSecurityFiling;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingCusISFHeaderModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			var cusISFHeader = Factory.New<TrackingCusISFHeader>();
			cusISFHeader.UserEditableNoteHelper.EditableNoteText = "Test";
			return cusISFHeader;
		}
	}
}
