namespace Enterprise.Customs.NL.GUI;

partial class CusAuthorisationForm
{
	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private new void InitializeComponent()
	{
		GoodsLocationColumnStyleInfo goodsLocationColumnStyleInfo1 = new GoodsLocationColumnStyleInfo();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.CusAuthorisationHeader);
		// 
		// AuthorisationRuleGrid
		//
		goodsLocationColumnStyleInfo1.CaptionResourceString = Res.GetData("19C46289-8DAD-452F-A1D6-152108F57D69", "Goods Location");
		goodsLocationColumnStyleInfo1.ColumnName = "GoodsLocationDescription";
		goodsLocationColumnStyleInfo1.DefaultCollectionIndex = 0;
		goodsLocationColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		this.AuthorisationRuleGrid.ColumnStyles.Add(goodsLocationColumnStyleInfo1);
	}

	#endregion
}
