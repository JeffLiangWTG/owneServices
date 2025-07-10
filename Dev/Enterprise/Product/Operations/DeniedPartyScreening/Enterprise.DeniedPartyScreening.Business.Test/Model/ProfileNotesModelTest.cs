using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class ProfileNotesModelTest : TestCase
	{
		public void TestConstructor()
		{
			var model = new ProfileNotesModel("ABC");
			AssertEquals("ABC", model.ProfileNotes);
		}
	}
}
