using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

class CusTempStorageRegHeaderValueChangedAnnouncer : IDisposable, IInvoicesProviderValueChangedAnnouncer
{
	readonly CusTempStorageRegHeader tempStorageRegHeader;

	public event EventHandler OnValueChanged;

	public CusTempStorageRegHeaderValueChangedAnnouncer(CusTempStorageRegHeader header)
	{
		this.tempStorageRegHeader = header;
		tempStorageRegHeader.SRH_ReferenceInfo.ValueChanged += CusTempStorageRegHeaderValueChanged;
	}

	public void Dispose()
	{
		tempStorageRegHeader.SRH_ReferenceInfo.ValueChanged -= CusTempStorageRegHeaderValueChanged;
	}

	void CusTempStorageRegHeaderValueChanged(object sender, EventArgs e)
	{
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(sender, e);
		}
	}
}
