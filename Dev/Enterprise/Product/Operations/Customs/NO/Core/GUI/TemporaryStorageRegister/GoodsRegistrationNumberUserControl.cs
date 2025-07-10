using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

partial class GoodsRegistrationNumberUserControl : ZUserControl
{
	public GoodsRegistrationNumberUserControl()
	{
		InitializeComponent();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		EnableAndDisableControls();
	}

	void GenerateGoodsNumberButton_Click(object sender, EventArgs e)
	{
		if (CurrentDataItem is not IGoodsRegistrationNumberManager header ||
			CurrentDataItem is not BusinessObject bizObj)
		{
			return;
		}

		var factory = bizObj.Factory;
		var generatorObject = new GoodsRegistrationNumberGeneratorObject(header, factory);
		using var dialog = new GenerateGoodsRegistrationNumberForm(generatorObject);

		if (ZFormModaliser.ShowDialogAndDispose(dialog) == DialogResult.OK)
		{
			factory.Save();
		}
	}

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		if (CurrentDataItem is IInvoicesProviderValueChangedAnnouncerProvider header)
		{
			tempStorageRegHeaderValueChangedAnnouncer = header.GetValueChangedAnnouncer();
			if (tempStorageRegHeaderValueChangedAnnouncer != null)
			{
				tempStorageRegHeaderValueChangedAnnouncer.OnValueChanged += new EventHandler(CusTempStorageRegHeaderValueChangedAnnouncer_OnValueChanged);
			}
		}
	}

	IInvoicesProviderValueChangedAnnouncer tempStorageRegHeaderValueChangedAnnouncer;

	void CusTempStorageRegHeaderValueChangedAnnouncer_OnValueChanged(object sender, EventArgs e)
	{
		EnableAndDisableControls();
	}

	void EnableAndDisableControls()
	{
		GenerateGoodsNumberButton.Enabled = !(CurrentDataItem is CusTempStorageRegHeader { SRH_Reference.IsEmpty: false });
	}
}
