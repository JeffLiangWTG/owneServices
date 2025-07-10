using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingNote))]
	public class SterlingNoteTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingNote();
		}

		public void TestSterlingNoteRecord()
		{
			AssertEquals("Parsed record is different from expected", ExpectedSterlingNoteRecord1, SterlingForTest.NoteInfo[0].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingNoteRecord2, SterlingForTest.NoteInfo[1].Record);
		}
		const string ExpectedSterlingNoteRecord1 = "NTS|CustomNote|NoteData1|2008-01-01 02:01:01 +11:00>\r\n";
		const string ExpectedSterlingNoteRecord2 = "NTS|BookingNotes|NoteData2|2008-01-01 03:01:01 +11:00>\r\n";
	}
}
