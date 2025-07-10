using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbReleaseNoteRead))]
	sealed class GlbReleaseNoteReadTest : EnterpriseBusinessObjectTestCase
	{
		public void TestConcurrencyOnClosing()
		{
			GlbReleaseNote note1 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note1.GF_ReleaseNoteDate = ZDateTime.Now;
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var note1inOtherFactory = otherFactory.LoadTop1<GlbReleaseNote>(new ZQuery(GlbReleaseNoteSchema.PK, note1.PK));
			AssertEquals(false, note1.IsCurrentlyRead);
			AssertEquals(false, note1inOtherFactory.IsCurrentlyRead);
			note1.IsCurrentlyRead = true;
			note1inOtherFactory.IsCurrentlyRead = true;
			AssertEquals(true, note1inOtherFactory.IsCurrentlyRead);
			Factory.Save();
			otherFactory.Save();
			var readRecords = Factory.Load<GlbReleaseNoteRead>(new ZQuery());
			AssertEquals(1, readRecords.Length);
		}
	}
}
