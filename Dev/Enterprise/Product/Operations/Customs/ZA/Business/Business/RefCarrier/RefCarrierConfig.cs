using System.Collections.Generic;
using CargoWise.Types;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.ZA.Business
{
	public class RefCarrierConfig : IRefCarrierConfig
	{
		public IEnumerable<ZString> MandatoryAttributes => [RefCarrierAttributeNames.MASTER, RefCarrierAttributeNames.CARGOCARRIER];

		public ZBool IsSetDefaultCarrierType => true;
	}
}
