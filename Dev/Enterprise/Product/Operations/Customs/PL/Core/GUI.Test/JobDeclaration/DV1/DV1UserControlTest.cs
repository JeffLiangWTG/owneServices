using System;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class DV1UserControlTest : EU.GUI.Testing.DV1UserControlTest
{
	protected override Type ExpectedGridUserControl() => typeof(DV1GridUserControl);

	protected override EU.GUI.DV1UserControl GetNewDV1UserControl() => new DV1UserControl();

	protected override Type ExpectedDynamicDV1DetailsLayout() => typeof(DV1DetailsLayout);
}
