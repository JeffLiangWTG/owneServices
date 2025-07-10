using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	[CodeProperty(nameof(Code)), DescriptionProperty(nameof(Description))]
	public sealed class AirlineConfigCommodityBusinessObject : NonPersistentBusinessObject
	{
		public AirlineConfigCommodityBusinessObject(AirlineConfigCommodity commodity)
		{
			this.commodity = Argument.NotNull(commodity, nameof(commodity));
		}

		readonly AirlineConfigCommodity commodity;

		public ZString Code => commodity.Code;
		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		public ZString Description => commodity.Description;
		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		public ZString SpecialHandlingCodesForBinding => SpecialHandlingCodes != null ? string.Join(", ", SpecialHandlingCodes) : string.Empty;
		public ZPropertyInfo SpecialHandlingCodesForBindingInfo => GetZPropertyInfo(nameof(SpecialHandlingCodesForBinding));

		public IReadOnlyCollection<string> SpecialHandlingCodes => commodity.SpecialHandlingCodes;
	}
}
