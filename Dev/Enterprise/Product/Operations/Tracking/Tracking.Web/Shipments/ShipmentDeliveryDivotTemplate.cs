using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ShipmentDeliveryDivotTemplate : ZItemTemplate, INewRowColumnItemTemplate
	{
		public ShipmentDeliveryDivotTemplate(ZNewRowColumn column) : base(column) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override ISelfBindingWebControl GetControl()
		{
			ZDataGrid grid = new ZDataGrid();
			grid.Style[System.Web.UI.HtmlTextWriterStyle.Width] = "100%";
			grid.ItemStyle.CssClass = "DetailsCell";
			grid.HeaderStyle.CssClass = "DetailsHeader";
			grid.CssClass = "DetailsTable";
			grid.AllowEdit = true;

			ZBindToChecker.CheckBindTo((ZInt)((CommonConfirmDivot)null).J8_PackagesDelivered);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("278f6025-9cbb-412c-bca7-0ff9b7028858", "Packs"), CommonConfirmDivot.Schema.J8_PackagesDelivered) { Decimals = 0 });

			ZBindToChecker.CheckBindTo((ZDecimal)((CommonConfirmDivot)null).J8_DeliveryWeight);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("d9d1142c-b789-402b-9afa-527a82bd0d02", "Weight"), CommonConfirmDivot.Schema.J8_DeliveryWeight) { Decimals = 3 });

			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((CommonConfirmDivot)null).PackLine.JL_ActualWeightUQ_List);
			ZBindToChecker.CheckBindTo((ZString)((CommonConfirmDivot)null).PackLine.JL_ActualWeightUQ);
			grid.Columns.Add(new ZDropDownListColumn(Res.GetString("90984406-d4b0-4e6a-820c-38661f627d85", "UW"), "PackLine+JL_ActualWeightUQ", "PackLine+JL_ActualWeightUQ_List"));

			ZBindToChecker.CheckBindTo((ZDecimal)((CommonConfirmDivot)null).J8_DeliveryVolume);
			grid.Columns.Add(new ZCalcEditColumn(Res.GetString("5133468a-9327-4d28-b394-4ab24ff9c3d6", "Volume"), CommonConfirmDivot.Schema.J8_DeliveryVolume) { Decimals = 3 });

			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((CommonConfirmDivot)null).PackLine.JL_ActualVolumeUQ_List);
			ZBindToChecker.CheckBindTo((ZString)((CommonConfirmDivot)null).PackLine.JL_ActualVolumeUQ);
			grid.Columns.Add(new ZDropDownListColumn(Res.GetString("d9c72686-0369-4c8a-b19c-1f16aa87c61c", "UV"), "PackLine+JL_ActualVolumeUQ", "PackLine+JL_ActualVolumeUQ_List"));

			return grid;
		}
	}
}
