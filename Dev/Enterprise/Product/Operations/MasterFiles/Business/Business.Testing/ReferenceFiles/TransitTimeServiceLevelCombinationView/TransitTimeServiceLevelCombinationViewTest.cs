using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TransitTimeServiceLevelCombinationView))]
	internal class TransitTimeServiceLevelCombinationViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestServiceLevel_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(TransitTimeServiceLevelCombinationView.TSC_CodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(ResourceStringData.ShortCaption), "Code", resourceStringData.ShortCaption);
				AssertEquals(nameof(ResourceStringData.Caption), "Code", resourceStringData.Caption);
				AssertEquals(nameof(ResourceStringData.FullDescription), "The code of this service level.", resourceStringData.FullDescription);
			});
		}

		public void TestHumanReadableName()
		{
			var factory = new BusinessObjectFactory();
			var transitTime = factory.NewWithValidTestData<RefTransitTime>();
			transitTime.RTT_RS_NKServiceLevel = "STD";
			transitTime.RTT_FZ_OriginInternationalZone = ZGuid.Empty;
			transitTime.RTT_FZ_DestinationInternationalZone = ZGuid.Empty;
			transitTime.RTT_FZ_OriginInternationalZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUEC")).PK;
			transitTime.RTT_FZ_DestinationInternationalZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USEC")).PK;
			transitTime.RTT_Mode = Core.Constants.RateMode.AIR;
			factory.Save();

			var view = factory.LoadTop1<TransitTimeServiceLevelCombinationView>(new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_RTT, SQLComparisonOperator.Equal, transitTime.PK));
			AssertEquals("STD Zone Transit: AUEC - USEC", view?.HumanReadableName);
		}

		#region Implementation

		TransitTimeServiceLevelCombinationView TransitTimeServiceLevelCombinationView;

		protected override void SetUp()
		{
			base.SetUp();
			TransitTimeServiceLevelCombinationView = Factory.NewWithValidTestData<TransitTimeServiceLevelCombinationView>();
		}

		#endregion

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Can not save nor delete view", condition: true);
		}
	}
}
