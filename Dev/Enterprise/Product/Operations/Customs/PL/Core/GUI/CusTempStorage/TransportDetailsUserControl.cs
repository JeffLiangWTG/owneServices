using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.CusTempStorage;

public partial class TransportDetailsUserControl : ZUserControl
{
	public TransportDetailsUserControl()
	{
		InitializeComponent();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		header = DataSource as CusTempStorageJobHeader;

		if (header != null)
		{
			header.SJH_TransportModeInfo.ValueChanged -= SJH_TransportModeInfo_ValueChanged;
		}

		base.OnAfterFirstBinding(e);

		if (header != null)
		{
			header.SJH_TransportModeInfo.ValueChanged += SJH_TransportModeInfo_ValueChanged;
			SJH_TransportModeInfo_ValueChanged(null, null);
		}
	}
	CusTempStorageJobHeader header;

	void SJH_TransportModeInfo_ValueChanged(object sender, EventArgs e)
	{
		if (header != null)
		{
			switch (header.SJH_TransportMode)
			{
				case TransportTypeList.Codes.Air:
					VesselCodeFindBox.Visible = false;
					TransportRegistrationNumTextBox.Visible = true;
					TransportRegistrationNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("65C80279-BC20-40A7-9FC8-3CEFAD62AA47", "Flight Number");
					break;
				case TransportTypeList.Codes.Sea:
				case TransportTypeList.Codes.InlandWaterwayTransport:
					TransportRegistrationNumTextBox.Visible = false;
					VesselCodeFindBox.Visible = true;
					break;
				default:
					VesselCodeFindBox.Visible = false;
					TransportRegistrationNumTextBox.Visible = true;
					TransportRegistrationNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("B37E39E8-42F0-43B6-BF12-D65445189C2F", "Transport Reg. No.");
					break;
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (header != null)
		{
			header.SJH_TransportModeInfo.ValueChanged -= SJH_TransportModeInfo_ValueChanged;
		}
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}
}
