using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public enum AdditionalCodesShowType
	{
		TextBoxColumnStyleInfo,
		ZDropEditColumnStyleInfo
	}

	public partial class AdditionalCodesForm : ZChildForm
	{
		public AdditionalCodesForm()
		{
			InitializeComponent();
		}

		public AdditionalCodesForm(CommaSeparatedNumberCollection dataSource, AdditionalCodesShowType showType, ZString title, int maxCount)
			: base(dataSource)
		{
			InitializeComponent();
			this.title = title;
			this.maxCount = maxCount;
			this.Text = title;
			InitGrid(showType);
		}

		readonly ZString title;
		readonly int maxCount;

		void InitGrid(AdditionalCodesShowType showType)
		{
			if (showType == AdditionalCodesShowType.TextBoxColumnStyleInfo)
			{
				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.Caption = title;
				zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				zTextBoxColumnStyleInfo1.ColumnName = "Number";
				zTextBoxColumnStyleInfo1.IsMandatory = true;
				zTextBoxColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
				zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
				this.SealNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			}
			else if (showType == AdditionalCodesShowType.ZDropEditColumnStyleInfo)
			{
				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo1.Caption = title;
				zDropEditColumnStyleInfo1.ColumnName = "Number";
				zDropEditColumnStyleInfo1.IsMandatory = true;
				zDropEditColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
				zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
				this.SealNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			}
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			var commaSeparatedNumberCollection = BusinessEntity as CommaSeparatedNumberCollection;
			if (commaSeparatedNumberCollection != null && commaSeparatedNumberCollection.HasRepeatElement)
			{
				Globals.Message.ShowError(ZString.Format(RepeatedCodes, title));
			}
			else if (BusinessEntity != null && BusinessEntity.Count > maxCount)
			{
				Globals.Message.ShowError(ZString.Format(CodesEntered, maxCount, title));
			}
			else
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				this.Close();
			}
		}
		internal const string CodesEntered = "You have entered more than {0} {1}. Up to {0} {1} are allowed.";
		internal const string RepeatedCodes = "You have entered duplicate {0}.";
		void GaveUpButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
