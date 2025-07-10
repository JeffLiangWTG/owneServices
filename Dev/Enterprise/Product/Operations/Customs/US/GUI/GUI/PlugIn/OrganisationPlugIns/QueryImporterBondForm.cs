using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class QueryImporterBondForm : ZChildForm
	{
		public QueryImporterBondForm(ZString numberToQuery)
		{
			this.numberToQuery = numberToQuery;
		}

		public readonly ZString numberToQuery;

		public bool IsOKToSendMessage
		{
			get;
			set;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void QueryImporterBondForm_Load(object sender, System.EventArgs e)
		{
			NumberTextBox.Text = numberToQuery;
		}

		internal void btnSend_Click(object sender, System.EventArgs e)
		{
			IsOKToSendMessage = true;
		}

		internal void btnCancel_Click(object sender, System.EventArgs e)
		{
			IsOKToSendMessage = false;
		}
	}
}
