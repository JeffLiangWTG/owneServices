using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public sealed class GoodsDeclarationReferenceNumber : IAWBGoodsDeclarationReferenceNumberMessageDetailsProvider
	{
		public List<ZString> Numbers { get; set; }
		public ZString CountryOfIssue { get; set; }
		public ZString MovementCode { get; set; }
	}
}
