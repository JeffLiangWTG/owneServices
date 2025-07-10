using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SailingFilterBuilderTest_ToContainerFilter : SailingFilterBuilderTest<CommonContainer>
	{
		#region Implementation

		protected override CommonContainer[] CreateBusinessObjects(JobSailing sailing)
		{
			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol1.Transports[0].JW_JX = sailing.PK;
			consol1.Transports[0].JW_IsLinked = true;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol2.Transports[0].JW_JX = sailing.PK;
			consol2.Transports[0].JW_IsLinked = false;

			return new CommonContainer[] { consol1.Containers.AddNew(), consol2.Containers.AddNew() };
		}

		protected override ZQuery GetFilter(SailingFilterBuilder builder)
		{
			return builder.ToContainerFilter();
		}

		protected override bool IsLinked(CommonContainer bo)
		{
			return bo.Consol.Transports[0].JW_IsLinked;
		}

		#endregion
	}
}
