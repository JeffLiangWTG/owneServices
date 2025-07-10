using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Express
{
	public class CusRelatedEventsProvider : Customs.Business.CusRelatedEventsProvider, Integration.Customs.NZ.ICusRelatedEventsProvider
	{
		protected override IEnumerable<ZString> GetAvailableApplicationCodes() => new ZString[]
		{
			Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff
		};
	}
}
