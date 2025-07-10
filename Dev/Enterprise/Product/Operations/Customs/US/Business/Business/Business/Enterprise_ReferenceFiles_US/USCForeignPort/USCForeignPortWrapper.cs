using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public interface IForeignRegionalDistrictPort
	{
		ZString PortName { get; }
		ZString PortCode { get; }
	}

	public class USCForeignPortWrapper : IForeignRegionalDistrictPort
	{
		public enum Type
		{
			AES,
			Common,
			InBond
		}

		public USCForeignPortWrapper(ZZRefCusCodeListCombined foreignPort)
		{
			this.foreignPort = foreignPort;
		}
		readonly ZZRefCusCodeListCombined foreignPort;

		ZString IForeignRegionalDistrictPort.PortName
		{
			get { return foreignPort != null ? foreignPort.ZZD_Description : ZString.Empty; }
		}

		ZString IForeignRegionalDistrictPort.PortCode
		{
			get { return foreignPort != null ? foreignPort.ZZD_Code : ZString.Empty; }
		}
	}
}
