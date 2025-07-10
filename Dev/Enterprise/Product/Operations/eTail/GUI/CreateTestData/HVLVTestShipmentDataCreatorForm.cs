using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI;

public partial class HVLVTestShipmentDataCreatorForm : ZChildForm
{
	public HVLVTestShipmentDataCreatorForm(HVLVTestDataConfiguration configuration) : base(configuration)
	{
		InitializeComponent();
	}

	void createButton_Click(object sender, System.EventArgs e)
	{
		((HVLVTestDataConfiguration)DataSource).CreateData();
		DialogResult = DialogResult.OK;
		Close();
	}
}
