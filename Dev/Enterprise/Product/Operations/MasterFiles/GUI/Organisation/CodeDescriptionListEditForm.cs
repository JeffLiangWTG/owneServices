using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CodeDescriptionListEditForm : ZChildForm
	{
		public CodeDescriptionListEditForm(string caption, int codeMaxLength)
		{
			this.Text = caption;
			if (codeMaxLength > 0)
			{
				CodeDescriptionListEditControl.CodeMaxLength = codeMaxLength;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}

		public CodeDescriptionListEditForm(string caption)
			: this(caption, 0)
		{
		}

		#region Code Description Pair List

		public ZBlob CodeDescriptionList
		{
			get { return fCodeDescriptionList; }
			set { fCodeDescriptionList = value; }
		}

		ZBlob fCodeDescriptionList;

		void CodeDescriptionListEditForm_Load(object sender, System.EventArgs e)
		{
			if (CodeDescriptionList.IsEmpty)
			{
				CodeDescriptionListEditControl.FieldValue = null;
			}
			else
			{
				CodeDescriptionListEditControl.FieldValue = fCodeDescriptionList;
			}
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			fCodeDescriptionList = CodeDescriptionListEditControl.FieldValue;
		}

		#endregion
	}
}
