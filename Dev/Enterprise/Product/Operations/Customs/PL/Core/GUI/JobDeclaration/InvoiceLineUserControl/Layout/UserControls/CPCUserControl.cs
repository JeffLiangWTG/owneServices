using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class CPCUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
{
	public CPCUserControl()
	{
		InitializeComponent();
		Extensions = new DefaultControlExtensionCollection(this);
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		Extensions.SetDataBinding(dataSource, dataMember);
	}

	public Control Host => this;

	public IControlExtensionCollection Extensions { get; }

	public string ResourceStringBindingMember => nameof(JobComInvoiceLine.ProcedureCodeBase);

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Extensions.Dispose();
		}
		base.Dispose(disposing);
	}
}
