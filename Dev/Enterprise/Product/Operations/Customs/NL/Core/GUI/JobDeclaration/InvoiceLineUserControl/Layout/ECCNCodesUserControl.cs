using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	[SuppressControlRequiresTextBasher]
	public partial class ECCNCodesUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public ECCNCodesUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		void ECCNCodesEditButton_Click(object sender, System.EventArgs e)
		{
			var invoiceLine = (JobComInvoiceLine)CurrentDataItem;
			if (invoiceLine != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new ECCNCodesForm(invoiceLine));
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public string ResourceStringBindingMember => nameof(JobComInvoiceLine.ECCNCodesAsString);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
