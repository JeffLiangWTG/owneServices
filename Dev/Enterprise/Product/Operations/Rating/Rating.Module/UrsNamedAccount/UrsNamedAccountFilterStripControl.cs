using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public partial class UrsNamedAccountFilterStripControl : ZFilterStripControl
	{
		public UrsNamedAccountFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBO)
			: base(gridCollection, filterBO)
		{
			InitializeComponent();
		}

		#region Overrides

		protected override void InitialiseGridCore()
		{
			BindingSource.SetBindingMember(Grid, "CarrierNamedAccounts");

			Grid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZCodeFindBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("53d60ee2-cbcb-466c-a370-fb4c518a68a1", "Foreign Name"),
					ColumnName = "ONA_ForeignName",
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
					IsMandatory = true,
					ModuleID = ModuleIDs.UrsNamedAccount,
				},
				new ZOrganisationFindBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("9fd0703f-8cca-4a19-a26c-f83d8affe0fc", "Organization"),
					ColumnName = "ONA_OH_Organization",
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsMandatory = true,
				},
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("f19961e3-885e-424e-ac4f-bc5a03ffd227", "Organization Name"),
					ColumnName = "Organization+OH_FullName",
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(300),
					IsReadOnly = true,
				},
				new ZGuidFindBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("1006e843-485c-41dc-b9f1-db8f7c4e4a6a", "Carrier"),
					ColumnName = "ONA_OH_Carrier",
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsMandatory = true,
				},
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("c5e10c49-f051-479c-b1dc-7aa48e30bec9", "Carrier Name"),
					ColumnName = "Carrier+OH_FullName",
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(300),
					IsReadOnly = true,
				},
			});
		}

		protected override int MaxFilterStripPanelHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(92);

		protected override bool CanSaveColumnLayouts => false;

		protected override bool ShouldAddEmptyFilterStripOnReset => true;

		#endregion
	}
}
