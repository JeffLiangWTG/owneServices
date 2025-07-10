using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	class CartageReferences : IReferencesParent
	{
		public ZGuid ClientOrganization { get; set; }
		public ZString ClientOrderNumber { get; set; }

		public ZString WaybillNumber { get; set; }
		public ZString QuoteNumber { get; set; }
		public ZString ConnoteNumber { get; set; }

		public List<KeyValuePair<ZString, ZString>> References { get; set; }
	}
}
