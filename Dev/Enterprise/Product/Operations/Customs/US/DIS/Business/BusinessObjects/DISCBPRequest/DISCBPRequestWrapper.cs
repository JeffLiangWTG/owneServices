using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISCBPRequestWrapper : IDISCBPRequest
	{
		public DISCBPRequestWrapper(DISCBPRequest cbpRequest)
		{
			this.cbpRequest = cbpRequest;
		}

		readonly DISCBPRequest cbpRequest;

		ZString IDISCBPRequest.ID
		{
			get
			{
				switch (cbpRequest.ID)
				{
					case MiscCBPRequestIDList.Codes.Unknown:
						return MiscCBPRequestIDList.Descriptions.Unknown;
					case MiscCBPRequestIDList.Codes.Unsolicited:
						return MiscCBPRequestIDList.Descriptions.Unsolicited;
					default:
						return cbpRequest.ID;
				}
			}
		}

		ZDateTime IDISCBPRequest.RequestDate
		{
			get { return cbpRequest.RequestDate; }
		}

		ZString IDISCBPRequest.Type
		{
			get { return cbpRequest.Type; }
		}
	}
}
