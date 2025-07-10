using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsGuarantee : EU.NCTS.Business.NctsGuarantee
{
	public NctsGuarantee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	public override ZString PW_BondFiledPort
	{
		get => base.PW_BondFiledPort;
		set
		{
			if (!IsCopying)
			{
				base.PW_BondFiledPort = value;
			}
		}
	}

	protected override bool CopyCustomsOfficeFromGuaranteeHeader => false;
}
