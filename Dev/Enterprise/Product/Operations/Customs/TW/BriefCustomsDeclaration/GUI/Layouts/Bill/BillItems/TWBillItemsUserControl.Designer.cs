using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	partial class TWBillItemsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BillItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TaxesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TaxesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BillItemsGrid)).BeginInit();
			this.BillItemsGrid.SuspendLayout();
			this.TaxesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxesGrid)).BeginInit();
			this.TaxesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill);
			// 
			// BillItemsGrid
			// 
			this.BillItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillItemsGrid, "PackedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_UnitPrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_RX_NKGoodsValueCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_Model)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_Brand)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_Remarks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).ModeOfStatisticsOrDutyTreatment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsQty2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsUQ2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_RN_NKGoodsOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_Preference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).FormattedAdValoremDutyRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).FormattedSpecificDutyRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsBuyerPartNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsSupplierPartNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_PreviousEntryNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_PreviousEntryLineNo)));
			this.BillItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "API_CustomsValue";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "API_CustomsQty";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "API_CustomsUQ";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "API_UnitPrice";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "API_GoodsValue";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "API_RX_NKGoodsValueCurrency";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "API_LineNo";
			zCalcEditColumnStyleInfo5.Decimals = 0;
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.MaxValue = 9999;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "API_GoodsDescription";
			zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "API_Model";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "API_Brand";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo2.ColumnName = "API_Remarks";
			zMultiLineTextBoxColumnInfo2.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo1.ColumnName = "API_FormattedTariff";
			tariffColumnStyleInfo1.DefaultCollectionIndex = 0;
			tariffColumnStyleInfo1.NeedLoadNomenclatureWhenTariffNotFound = false;
			tariffColumnStyleInfo1.NeedLoadParentDataGroup = true;
			tariffColumnStyleInfo1.SelectNomenclatureModes = null;
			tariffColumnStyleInfo1.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			tariffColumnStyleInfo1.TariffType = null;
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "ModeOfStatisticsOrDutyTreatment";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "API_NetWeight";
			zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "API_NetWeightUQ";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "API_CustomsQty2";
			zCalcEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "API_CustomsUQ2";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "API_RN_NKGoodsOrigin";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo4.ColumnName = "API_Preference";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "FormattedAdValoremDutyRate";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.ColumnName = "FormattedSpecificDutyRate";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.ColumnName = "API_CustomsBuyerPartNo";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo8.ColumnName = "API_CustomsSupplierPartNo";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zTextBoxColumnStyleInfo9.ColumnName = "API_PreviousEntryNo";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "API_PreviousEntryLineNo";
			zCalcEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.BillItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.BillItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.BillItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.BillItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.BillItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.BillItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BillItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.BillItemsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.BillItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BillItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BillItemsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.BillItemsGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.BillItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BillItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.BillItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.BillItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.BillItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.BillItemsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.BillItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.BillItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.BillItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.BillItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.BillItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.BillItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.BillItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.BillItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillItemsGrid.GridId = "8E31038F-729B-4572-B4B0-1FCD4E8EE886";
			this.BillItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillItemsGrid.LayoutKey = "BillItemsGrid";
			this.BillItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillItemsGrid.MaximumRows = 9999;
			this.BillItemsGrid.Name = "BillItemsGrid";
			this.BillItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 67, true);
			this.BillItemsGrid.TabIndex = 0;
			// 
			// TaxesGroupBox
			// 
			this.TaxesGroupBox.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("0f5d9a13-28ce-467e-9e99-838b1872154d", "Taxes");
			this.TaxesGroupBox.Controls.Add(this.TaxesGrid);
			this.TaxesGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TaxesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 67, true);
			this.TaxesGroupBox.Name = "TaxesGroupBox";
			this.TaxesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 142, true);
			this.TaxesGroupBox.TabIndex = 1;
			this.TaxesGroupBox.TabStop = false;
			// 
			// TaxesGrid
			// 
			this.TaxesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxesGrid, "PackedItems.AsycudaTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_RateOverrideReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_ChargeTypeDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_TariffDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_MethodOfPayment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItemTax)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaTaxes)).SyncRoot)).AET_MethodOfPaymentDesc)));
			this.TaxesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.ColumnName = "AET_RateOverrideReasonCode";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.ColumnName = "AET_ChargeType";
			zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo10.ColumnName = "AET_ChargeTypeDesc";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "AET_Tariff";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "AET_TariffDesc";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "AET_Rate";
			zCalcEditColumnStyleInfo9.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = "AET_MethodOfCalculation";
			zDropEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "AET_BaseValue";
			zCalcEditColumnStyleInfo10.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "AET_ChargeAmount";
			zCalcEditColumnStyleInfo11.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo8.ColumnName = "AET_MethodOfPayment";
			zDropEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.ColumnName = "AET_MethodOfPaymentDesc";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.TaxesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.TaxesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.TaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.TaxesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.TaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.TaxesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.TaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.TaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.TaxesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.TaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.TaxesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxesGrid.GridId = "a2f36714-f6f2-4ade-8812-be1c5c948bb1";
			this.TaxesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxesGrid.LayoutKey = "TaxesGrid";
			this.TaxesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.TaxesGrid.Name = "TaxesGrid";
			this.TaxesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 125, true);
			this.TaxesGrid.TabIndex = 0;
			// 
			// TWBillItemsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BillItemsGrid);
			this.Controls.Add(this.TaxesGroupBox);
			this.Name = "TWBillItemsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 209, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BillItemsGrid)).EndInit();
			this.BillItemsGrid.ResumeLayout(false);
			this.BillItemsGrid.PerformLayout();
			this.TaxesGroupBox.ResumeLayout(false);
			this.TaxesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxesGrid)).EndInit();
			this.TaxesGrid.ResumeLayout(false);
			this.TaxesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGrid BillItemsGrid;
		private ZArchitecture.GUI.ZGroupBox TaxesGroupBox;
		private ZGrid TaxesGrid;
	}
}
