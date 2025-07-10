using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class OpeningBillofLadingsProvider : IOpeningBillofLadings
	{
		public OpeningBillofLadingsProvider(CusSupportingInfo manifestToOpen, ZString openingStyle, List<CusSupportingInfo> billLineLevelItems)
		{
			this.manifestToOpen = manifestToOpen;
			this.openingStyle = openingStyle;
			this.billLineLevelItems = billLineLevelItems;
		}
		readonly CusSupportingInfo manifestToOpen;
		readonly ZString openingStyle;
		readonly List<CusSupportingInfo> billLineLevelItems;

		public ZString InternalNoOpenBill => ZString.Empty;
		public ZString OpenedBillNumber => manifestToOpen.CSI_ReferenceNumber;
		public ZString InternalNoOpenBill2 => ZString.Empty;
		public IEnumerable<IOpeningLadingLines> OpeningLadingLines
		{
			get
			{
				if (openingStyle == SubTypeListForManifestToOpen.Codes.Billlinelevel && billLineLevelItems != null)
				{
					foreach (var manifestLineToOpen in billLineLevelItems)
					{
						yield return new OpeningLadingLinesProvider(manifestLineToOpen);
					}
				}
			}
		}
	}
}
