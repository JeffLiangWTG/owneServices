using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterData.Common.Deduplication.Integration
{
	public interface IEDIOrgHeader
	{
		ZString LicenceEnterpriseID { get; }
		ZString LicenceEnterpriseCode { get; }
		ZString CompanyCode { get; }
		List<ZString> ProductId { get; }
	}
}
