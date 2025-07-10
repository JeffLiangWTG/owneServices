using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.ExportAWB.ExportAWBHeader.Helpers
{
	static class SlacHelper
	{
		public static ZInt GetShippingLoadAndCount(ForwardingShipment shipment, out ZInt sumFromPackline)
		{
			if (shipment == null)
			{
				sumFromPackline = ZInt.Zero;
				return ZInt.Zero;
			}

			var outerInnerMatched = false;
			var outerInnerUnmatch = false;
			var dictPacklinePackageCount = new Dictionary<ZGuid, ZInt>();

			foreach (var outerPackLine in shipment.OuterPackLines.Cast<ForwardingPackLine>())
			{
				dictPacklinePackageCount[outerPackLine.PK] = outerPackLine.JL_PackageCount;

				if (outerPackLine.InnerPackLines.Count > 0)
				{
					outerInnerMatched = true;
					dictPacklinePackageCount[outerPackLine.PK] = ZInt.Zero;

					foreach (var innerPackline in (outerPackLine.InnerPackLines))
					{
						dictPacklinePackageCount[outerPackLine.PK] += innerPackline.JL_PackageCount;
					}
				}
				else
				{
					outerInnerUnmatch = true;
				}
			}

			sumFromPackline = Math.Max(dictPacklinePackageCount.Values.Sum(x => x), shipment.JS_TotalPackageCount);
			return outerInnerMatched && outerInnerUnmatch ? sumFromPackline : shipment.JS_TotalPackageCount;
		}

		public static ZInt GetShippingLoadAndCount(ForwardingConsol consol)
		{
			var consolSlac = ZInt.Zero;
			var shipmentsForTotalling = consol.ShipmentsForTotalling.Cast<ForwardingShipment>().ToArray();
			var shouldUsePackline = shipmentsForTotalling.Any(shipment => shipment.InnerPackLines.ToArray().Any(x => (x as ForwardingPackLine)?.JL_PackageCount > 0));

			foreach (var shipment in shipmentsForTotalling)
			{
				var shipmentSlac = GetShippingLoadAndCount(shipment, out var sumFromPackline);
				consolSlac += shouldUsePackline ? sumFromPackline : shipmentSlac;
			}

			return consolSlac;
		}
	}
}
