using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(StaffNumberRangesPlugIn))]
	sealed class StaffNumberRangesPlugInTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest() => new StaffNumberRangesPlugIn(Factory.NewWithValidTestData<GlbStaff>());

		public void TestCheckpoints()
		{
			using (var plugIn = new StaffNumberRangesPlugInForTesting(Factory.NewWithValidTestData<GlbStaff>()))
			{
				AssertEquals(Env.Licence.Core, plugIn.LicenceCheckpointForTesting);
			}
		}

		public void TestGetNewUserControl()
		{
			using (var plugIn = (StaffNumberRangesPlugIn)GetPlugInToTest())
			{
				AssertType<NumberRangesUserControl>(plugIn.UserControl);
			}
		}

		public void TestEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			using (var plugIn = (StaffNumberRangesPlugIn)GetPlugInToTest())
			{
				AssertEquals("PlugIn should NOT be enabled when Current Country NOT Mexico", expected: false, plugIn.Enabled);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			using (var plugIn = (StaffNumberRangesPlugIn)GetPlugInToTest())
			{
				AssertEquals("PlugIn should be enabled when Current Country Mexico", expected: true, plugIn.Enabled);
			}
		}

		sealed class StaffNumberRangesPlugInForTesting : StaffNumberRangesPlugIn
		{
			public StaffNumberRangesPlugInForTesting(GlbStaff staff)
			: base(staff)
			{
			}

			public LicenceCheckpoint LicenceCheckpointForTesting => base.LicenceCheckPoint;
		}
	}
}
