using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public partial class AdditionalCodesForm : ZChildForm
	{
		public AdditionalCodesForm()
		{
			InitializeComponent();
		}

		public AdditionalCodesForm(Business.CusEntryNumCollection dataSource, ZString title)
			: base(dataSource)
		{
			InitializeComponent();
			this.title = title;
			this.Text = title;
			InitGrid();
		}
		readonly ZString title;

		void InitGrid()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.Caption = title;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CE_EntryNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			this.CusEntryNumGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

		void GaveUpButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
