using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class OpeningSummaryDeclarationProvider : IOpeningSummaryDeclaration
	{
		public OpeningSummaryDeclarationProvider(CusSupportingInfo manifestToOpen, ZString openingStyle, List<CusSupportingInfo> billLevelItems, List<CusSupportingInfo> billLineLevelItems)
		{
			this.manifestToOpen = manifestToOpen;
			this.openingStyle = openingStyle;
			this.billLevelItems = billLevelItems;
			this.billLineLevelItems = billLineLevelItems;
		}
		readonly CusSupportingInfo manifestToOpen;
		readonly ZString openingStyle;
		readonly List<CusSupportingInfo> billLevelItems;
		readonly List<CusSupportingInfo> billLineLevelItems;

		public ZString HowToOpen
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (!manifestToOpen.CSI_SubType.IsEmpty)
				{
					switch (manifestToOpen.CSI_SubType)
					{
						case "M":
							returnValue = "";
							break;
						case "B":
							returnValue = "2";
							break;
						case "L":
							returnValue = "3";
							break;
					}
				}
				return returnValue;
			}
		}

		public ZString InWarehouse => manifestToOpen.CSI_Status == "Y" ? TurkishConstants.AnswerYes : TurkishConstants.AnswerNo;
		public ZString DeclarationNo => manifestToOpen.CSI_ReferenceNumber2;
		public ZString WillOpenAnotherRegime => manifestToOpen.CSI_Procedure == "Y" ? TurkishConstants.AnswerYes : TurkishConstants.AnswerNo;
		public ZString Explanation => manifestToOpen.CSI_Description;
		public ZString OpeningInternalNumber => ZString.Empty;

		public IEnumerable<IOpeningBillofLadings> OpeningBillofLadings
		{
			get
			{
				if (openingStyle != SubTypeListForManifestToOpen.Codes.Manifestlevel)
				{
					if (billLevelItems != null)
					{
						foreach (var manifestBOLToOpen in billLevelItems)
						{
							yield return new OpeningBillofLadingsProvider(manifestBOLToOpen, openingStyle, billLineLevelItems);
						}
					}
					if (billLineLevelItems != null)
					{
						yield return new OpeningBillofLadingsProvider(billLineLevelItems[0], openingStyle, billLineLevelItems);
					}
				}
			}
		}
	}
}
