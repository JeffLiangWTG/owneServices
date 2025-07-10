using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI;

[ToolboxItem(true)]
public class SimilarOrgsDisplayGrid : ZDisplayGrid
{
	public SimilarOrgsDisplayGrid()
	{
		BeginInit();
		AllowNavigation = false;
		CaptionVisible = false;
		zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|1a8a51e6-612e-4ce2-8db5-b290f2719a8d", "Rank");
		zCalcEditColumnStyleInfo1.ColumnName = "OS_Rank";
		zCalcEditColumnStyleInfo1.Decimals = 0;
		zCalcEditColumnStyleInfo1.IsMandatory = true;
		zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
		zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|9a4a258e-71f4-4d94-8871-85c6ff2e1573", "Code");
		zTextBoxColumnStyleInfo1.ColumnName = "OH_Code";
		zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|5b94ebe9-837e-46b1-826e-09ad1d73b799", "Full Name");
		zTextBoxColumnStyleInfo2.ColumnName = "OH_FullName";
		zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|5d44332f-b280-4f32-8310-cd482636771b", "Street", "Address 1", "");
		zTextBoxColumnStyleInfo3.ColumnName = "OH_Calc_Address1";
		zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
		zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|f490008f-7620-4acf-800e-721df52affb0", "Street 2", "Address 2", "");
		zTextBoxColumnStyleInfo4.ColumnName = "OH_Calc_Address2";
		zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
		zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|166da56e-3005-4e64-b181-5aa64a42bc5f", "City", "City", "");
		zTextBoxColumnStyleInfo5.ColumnName = "OH_Calc_City";
		zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|9902c540-fcf0-455a-b9e4-fdd9e69b5b06", "State", "State", "");
		zTextBoxColumnStyleInfo6.ColumnName = "OH_Calc_State";
		zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|b58d3747-2f2d-4659-8d47-5af34878365b", "P. Code", "Post Code", "");
		zTextBoxColumnStyleInfo7.ColumnName = "OH_Calc_PostCode";
		zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
		zTextBoxColumnStyleInfo8.ColumnName = "OS_UNLOCO";
		zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|b184c720-209c-457a-be35-e51228e1a294", "Phone", "Phone", "");
		zTextBoxColumnStyleInfo9.ColumnName = "OH_Calc_Phone";
		zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|20e913b9-30ef-408c-a0c8-36859d9d5cb0", "Fax", "Fax", "");
		zTextBoxColumnStyleInfo10.ColumnName = "OH_Calc_Fax";
		zTextBoxColumnStyleInfo10.IsVisible = false;
		zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|1fb229bc-3f51-4af1-afd4-b0fccb063283", "Reg. #", "Business Reg No.", "");
		zTextBoxColumnStyleInfo11.ColumnName = "LocalBusinessNumber";
		zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgsDisplayGrid|f88e870b-f358-4d00-9702-eb4a7a4ba04b", "Email", "Email", "");
		zTextBoxColumnStyleInfo12.ColumnName = "OH_Calc_Email";
		zTextBoxColumnStyleInfo12.IsVisible = false;

		ColumnStyles.Add(zCalcEditColumnStyleInfo1);
		ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		ColumnStyles.Add(zTextBoxColumnStyleInfo3);
		ColumnStyles.Add(zTextBoxColumnStyleInfo4);
		ColumnStyles.Add(zTextBoxColumnStyleInfo5);
		ColumnStyles.Add(zTextBoxColumnStyleInfo6);
		ColumnStyles.Add(zTextBoxColumnStyleInfo7);
		ColumnStyles.Add(zTextBoxColumnStyleInfo8);
		ColumnStyles.Add(zTextBoxColumnStyleInfo9);
		ColumnStyles.Add(zTextBoxColumnStyleInfo10);
		ColumnStyles.Add(zTextBoxColumnStyleInfo11);
		ColumnStyles.Add(zTextBoxColumnStyleInfo12);
		GridId = "68b5ef28-c070-4f62-89ab-27cb910ec7de";
		HeaderForeColor = System.Drawing.SystemColors.ControlText;
		IsWholeRowSelectedOnClick = true;
		LayoutKey = "SimilarOrgsDisplayGrid";
		ShouldSetErrorsOnTabPage = false;
		EndInit();
	}

	readonly ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
	readonly ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
}
