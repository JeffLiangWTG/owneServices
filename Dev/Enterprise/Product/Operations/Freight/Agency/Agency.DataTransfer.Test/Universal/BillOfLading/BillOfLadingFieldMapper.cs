using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class BillOfLadingFieldMapper : CommonShipmentFieldMapper<BillOfLading>
	{
		public BillOfLadingFieldMapper(BillOfLading billOfLading) : base(billOfLading)
		{
			this.billOfLading = billOfLading;
		}

		readonly BillOfLading billOfLading;
		protected override IEnumerable<string> MapContainers()
		{
			return billOfLading.RealContainers.Cast<BillOfLadingContainer>().Select(ContainerToString);
		}
	}
}
