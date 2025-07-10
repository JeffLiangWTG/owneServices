using Enterprise.Customs.US.ISF.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ISFContainerColumnProvider))]
	sealed class ISFContainerColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZDropDownListColumn("Desc.Code", CusISFEquip.Schema.BE_EquipCode) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly, ColumnKey = WebTracker.Grids.ISFContainer.DescriptionCode });
			AddDefaultsColumn(new ZTextEditColumn("Container Number", CusISFEquip.Schema.BE_ContainerNum) { ColumnKey = WebTracker.Grids.ISFContainer.ContainerNumber });
			AddDefaultsColumn(new ZTextEditColumn("ISO", CusISFEquip.Schema.BE_ContainerISO) { ColumnKey = WebTracker.Grids.ISFContainer.ISO });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ISFContainerColumnProvider();
		}
	}
}
