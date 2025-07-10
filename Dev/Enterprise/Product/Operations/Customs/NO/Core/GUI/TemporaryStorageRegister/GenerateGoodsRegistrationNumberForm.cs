using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

partial class GenerateGoodsRegistrationNumberForm : ZChildForm
{
	public GenerateGoodsRegistrationNumberForm(GoodsRegistrationNumberGeneratorObject parent) : base(parent)
	{
		dataProvider = Argument.NotNull(parent, nameof(parent));
		InitializeComponent();
	}

	public override string FormHeading => Res.GetString("3D97BB06-3CD2-436B-8EBA-F0403A38BCB1", "Create Goods Number for given Date and Customs Warehouse ID");

	void CreateNextButton_Click(object sender, EventArgs e)
	{
		if (dataProvider.HasMessageErrors)
		{
			return;
		}

		dataProvider.SetGoodsRegistrationNumber();
		DialogResult = DialogResult.OK;
		Close();
	}

	void CancelButton_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	readonly GoodsRegistrationNumberGeneratorObject dataProvider;
}
