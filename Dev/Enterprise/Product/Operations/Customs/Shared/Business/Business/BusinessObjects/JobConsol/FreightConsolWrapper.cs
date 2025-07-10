using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class FreightConsolWrapper
	{
		public FreightConsolWrapper(ForwardingConsol consol)
		{
			this.Consol = consol;
		}

		public readonly ForwardingConsol Consol;

		public ZBool IsTransshipment
		{
			get
			{
				ZBool import = false, export = false;
				foreach (Transport transPlan in Consol.Transports)
				{
					if (transPlan.LoadPort.RL_RN_NKCountryCode == GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode)
					{
						export = true;
					}
					else if (transPlan.DiscPort.RL_RN_NKCountryCode == GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode)
					{
						import = true;
					}
				}
				return import && export;
			}
		}

		public ZString GetReportingCarrier()
		{
			ZString result = ZString.Empty;
			if (Consol.JK_MasterBillNum.IsEmpty && Consol.JK_AgentType == Core.Constants.AgentType.OnBoardCourier)
			{
				result = "000-";
			}
			else if (ImportExportHelper.IsBranchCountry(Consol.JK_RL_NKLoadPort))
			{
				result = GlbCompany.CurrentCompany.OrgProxy.OH_FullName.Left(35);
			}
			else
			{
				if (Consol.SendingForwarder != null)
				{
					result = Consol.SendingForwarder.OH_FullName.Left(35);
				}
			}
			return result;
		}
	}
}
