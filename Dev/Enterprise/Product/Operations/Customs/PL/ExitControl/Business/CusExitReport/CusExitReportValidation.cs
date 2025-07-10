using System.Linq;

namespace Enterprise.Customs.PL.ExitControl.Business;

public sealed class CusExitReportValidation(CusExitReport parent) : EU.ExitControl.Business.CusExitReportValidation(parent)
{
	protected override void CheckCER_TransportID()
	{
		if (parent.CER_TransportIDReadOnly)
		{
			return;
		}

		base.CheckCER_TransportID();

		CheckR0049E();
	}

	protected override void CheckCER_RN_NKTransportNationality()
	{
		if (parent.CER_RN_NKTransportNationalityReadOnly)
		{
			return;
		}

		base.CheckCER_RN_NKTransportNationality();

		CheckR0050E();
	}

	void CheckR0049E()
	{
		if (!ExitControlConstants.Rules.R0049ETransportTypeFirstNumbers.Contains(parent.CER_TransportType.SubstringSafe(0, 1)) &&
			parent.CER_TransportID.IsEmpty)
		{
			parent.CER_TransportIDInfo.AddMessageError(Res.GetString("1A1F5DA1-C389-4386-8266-38AB5C0B6B74",
				"[R0049E] A Transport ID is required for a transport type other than one starting '5' or '7'."));
		}
	}

	void CheckR0050E()
	{
		if (parent.CER_RN_NKTransportNationality.IsEmpty &&
			!ExitControlConstants.Rules.R0050ETransportTypeFirstNumbers.Contains(parent.CER_TransportType.SubstringSafe(0, 1)))
		{
			parent.CER_RN_NKTransportNationalityInfo.AddMessageError(Res.GetString("AA1B494A-B271-4643-907F-0420729EEECA",
				"[R0050E] A Transport Nationality is required for a transport type other than one starting with '2', '5' or '7'."));
		}
	}
}
