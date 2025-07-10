using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.WebCFS.Web
{
	public partial class ContainerDetails : BasePage
	{
		protected override BusinessObject GetNewDataSource()
		{
			var containerPK = GetGuidFromParameter("Ref");

			return Factory.Load<CFSContainer>(containerPK);
		}

		protected override void OnPreBind()
		{
			base.OnPreBind();
			SetupPackLinesGrid();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PackLinesPanel.Visible = !ShouldNotShowPackLinesGrid;
			NoPackLinesGridLabel.Visible = ShouldNotShowPackLinesGrid;
		}

		protected bool ShouldNotShowPackLinesGrid
		{
			get
			{
				return (Container == null || Container.JC_LCLUnpack.IsEmpty || Container.JC_LCLUnpack > ZDateTime.Now);
			}
		}

		CFSContainer Container
		{
			get { return DataSource as CFSContainer; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification alters desired class name")]
		void SetupPackLinesGrid()
		{
			ZBindToChecker.CheckBindTo((string)((CFSPackLine)null).Shipment.JS_HouseBill);
			PackLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("019d9041-7bad-4fc0-b7bc-8a96ec4b392a", "House Bill #"), "Shipment.JS_HouseBill"));

			ZBindToChecker.CheckBindTo((int)((CFSPackLine)null).JL_PackageCount);
			PackLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("370b76d2-67a4-420b-bb9f-1dbe2fd2260e", "Packs"), CFSPackLine.Schema.JL_PackageCount));

			ZBindToChecker.CheckBindTo((string)((CFSPackLine)null).JL_F3_NKPackType);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((CFSPackLine)null).JL_F3_NKPackType_List);
			ZDropDownListColumn packTypeColumn = new ZDropDownListColumn(Res.GetString("84339b26-7080-4620-bb28-550217bec77a", "Pack Type"), CFSPackLine.Schema.JL_F3_NKPackType, "JL_F3_NKPackType_List");
			packTypeColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			PackLinesGrid.Columns.Add(packTypeColumn);

			ZBindToChecker.CheckBindTo((int)((CFSPackLine)null).JL_ActualWeight);
			PackLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("8fddb792-798b-4e4b-82f8-aa2d44cda7e0", "Weight"), CFSPackLine.Schema.JL_ActualWeight));

			ZBindToChecker.CheckBindTo((string)((CFSPackLine)null).JL_ActualWeightUQ);
			ZBindToChecker.CheckBindTo(((CFSPackLine)null).JL_ActualWeightUQ_List);
			ZDropDownListColumn weightUnitsColumn = new ZDropDownListColumn(Res.GetString("e7ff8482-7f81-45a4-bea9-b506233e1d1d", "Weight Units"), CFSPackLine.Schema.JL_ActualWeightUQ, "JL_ActualWeightUQ_List");
			weightUnitsColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			PackLinesGrid.Columns.Add(weightUnitsColumn);

			ZBindToChecker.CheckBindTo((int)((CFSPackLine)null).JL_ActualVolume);
			PackLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("fc6fb0a5-3132-40b0-8fbf-bf5995cfe91a", "Volume"), CFSPackLine.Schema.JL_ActualVolume));

			ZBindToChecker.CheckBindTo((string)((CFSPackLine)null).JL_ActualVolumeUQ);
			ZBindToChecker.CheckBindTo(((CFSPackLine)null).JL_ActualVolumeUQ_List);
			ZDropDownListColumn volumeUnitsColumn = new ZDropDownListColumn(Res.GetString("db362e8c-0950-4a00-8679-c52aff8e3633", "Volume Units"), CFSPackLine.Schema.JL_ActualVolumeUQ, "JL_ActualVolumeUQ_List");
			volumeUnitsColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			PackLinesGrid.Columns.Add(volumeUnitsColumn);

			ZBindToChecker.CheckBindTo((string)((CFSPackLine)null).CustomsStatusDescription);
			PackLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("c397de5f-b7d4-429e-a767-1aded1fcc127", "Customs Status"), "CustomsStatusDescription"));

			ZBindToChecker.CheckBindTo(((CFSPackLine)null).ArrivalAtCFS);
			ZDateTimeColumn arrivalAtCFSColumn = new ZDateTimeColumn(Res.GetString("204da948-1844-4e59-be07-cd640c053bff", "Arrival at CFS") + " ", "ArrivalAtCFS");
			arrivalAtCFSColumn.DateTimeFormat = ZDateTimePickerFormat.Long;
			PackLinesGrid.Columns.Add(arrivalAtCFSColumn);
		}
	}
}
