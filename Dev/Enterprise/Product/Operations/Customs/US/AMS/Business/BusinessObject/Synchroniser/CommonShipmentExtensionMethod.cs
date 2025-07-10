using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public static class CommonShipmentExtensionMethod
	{
		public static IEnumerable<PackLine> MergeOuterPackLinesIntoInnerPackLines(this CommonShipment shipment)
		{
			var linkedOuterPckLines = new HashSet<ZGuid>();
			foreach (PackLine innerPackLine in shipment.InnerPackLines)
			{
				if (!innerPackLine.JL_JL_OuterPackLine.IsEmpty && innerPackLine.JL_PackageCount > 0)
				{
					linkedOuterPckLines.Add(innerPackLine.JL_JL_OuterPackLine);
					yield return innerPackLine;
				}
			}
			foreach (PackLine outerPackLine in shipment.OuterPackLines)
			{
				if (!linkedOuterPckLines.Contains(outerPackLine.PK))
				{
					yield return outerPackLine;
				}
			}
		}

		public static PackLine GetInnerPackLineLinkedOuterPackLine(this CommonShipment shipment, PackLine innerPackLine)
		{
			return shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(f => f.PK == innerPackLine.JL_JL_OuterPackLine);
		}
	}
}
