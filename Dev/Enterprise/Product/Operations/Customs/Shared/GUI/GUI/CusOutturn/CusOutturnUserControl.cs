using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public partial class CusOutturnUserControl : ZUserControl
	{
		public CusOutturnUserControl()
		{
			InitializeComponent();
			SetSeaControlsVisibility(false);
			SetLastMessageDatelsVisibility(false);
			OutturnsGrid.AfterBind += new EventHandler(OutturnsGrid_AfterBind);
			OutturnsGrid.GridId = "GridLayoutDGu8fdRQgEtBCLuEeuYwuA==";
		}

		#region Binding

		public virtual void SetBindPrepend(ZString bindToPrepend)
		{
			StatusTextBox.BindTo = bindToPrepend + StatusTextBox.BindTo;
			DateTimeOfOutturnDateEdit.BindTo = bindToPrepend + DateTimeOfOutturnDateEdit.BindTo;
			DateTimeOfUnloadDateEdit.BindTo = bindToPrepend + DateTimeOfUnloadDateEdit.BindTo;
			OutturnsGrid.BindTo = bindToPrepend + OutturnsGrid.BindTo;
		}

		void OutturnsGrid_AfterBind(object sender, EventArgs e)
		{
			SetSeaControlsVisibility(lastSeaVisibilitySet);
			SetLastMessageDatelsVisibility(lastLastMessageDatelsVisibilitySet);
		}

		#endregion

		#region Visibility

		public void RemoveStatusColumn()
		{
			OutturnsGrid.SetAvailability(false, "CustomsStatus+Description");
		}

		public void RemoveColumnFromGrid(ZString columnToRemove)
		{
			OutturnsGrid.RemoveFromAvailableColumns(columnToRemove);
		}

		public void SetLastMessageDatelsVisibility(bool visible)
		{
			OutturnsGrid.SetAvailability(visible, CusOutturnSchema.Constants.C5_LastMessageDate);
			lastLastMessageDatelsVisibilitySet = visible;
		}
		bool lastLastMessageDatelsVisibilitySet;

		public void SetSeaControlsVisibility(bool visible)
		{
			DateTimeOfUnloadDateEdit.Visible = visible;
			DateTimeOfUnloadLabel.Visible = visible;

			OutturnsGrid.SetAvailability(visible, SeaColumns);
			lastSeaVisibilitySet = visible;
		}
		bool lastSeaVisibilitySet;

		internal string[] SeaColumns
		{
			get
			{
				return new string[]
					{
						CusOutturnSchema.Constants.C5_SealIntactIndicator,
						CusOutturnSchema.Constants.C5_ContainerNumber,
						CusOutturnSchema.Constants.C5_HouseBill,
						CusOutturnSchema.Constants.C5_MasterBill,
						CusOutturnSchema.Constants.C5_OuterPackUnits,
						CusOutturnSchema.Constants.C5_CargoReceiptDate,
						CusOutturnSchema.Constants.C5_CargoUnpackDate,
						CusOutturnSchema.Constants.C5_ReceiptOnlyIndicator,
						CusOutturnSchema.Constants.C5_CargoType,
						CusOutturnSchema.Constants.C5_PackagesUnits
					};
			}
		}

		#endregion

		void OutturnSearchTextBox_TextChanged(object sender, EventArgs e)
		{
			OutturnsGrid.SetIndexFromSearchString(OutturnSearchTextBox.Text, CusOutturn.Schema.ParentStringRepresentation);
		}
	}
}
