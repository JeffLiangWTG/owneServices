using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Integration.AWB;
using Enterprise.Messaging.Business.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	/**
	 * <summary>
	 * A consolidation list message (FHL) is a type of <see cref="CargoIMP">CargoIMP</see> message
	 * used to provide a list of House Airway Bills (HAWBs).
	 * </summary>
	 */
	public sealed class FHL : FBase
	{
		public enum Version { No2 = 2, No4 = 4, None = 0 }

		public FHL(IFWBMessageDetailsProvider fwbDetailsProvider, IFHLMessageDetailsProvider fhlDetailsProvider, Version version)
			: base(fwbDetailsProvider)
		{
			this.version = version;
			FHLDetailsProvider = fhlDetailsProvider;
		}

		readonly IFHLMessageDetailsProvider FHLDetailsProvider;
		readonly Version version;

		public override ZString StandardMessageIdentifier
		{
			get { return MessageTypes.FHL; }
		}

		protected override CharType OCIMUFormatType
		{
			get
			{
				return CharType.Text;
			}
		}

		protected override CharType OCIMDFormatType
		{
			get
			{
				return CharType.Text;
			}
		}

		protected override ZString MessageTypeVersionNumber
		{
			get
			{
				switch (version)
				{
					case Version.No2:
						return "2";
					case Version.No4:
						return "4";
					default:
						throw new InvalidOperationException("FHL Version." + version.ToString() + " support not implemented properly.");
				}
			}
		}

		protected override void ConstructMessage()
		{
			AddMasterAWBConsignmentDetail();
			AddHouseWaybillSummaryDetails();
			AddFreeTextDescriptionOfGoods();
			AddOtherCustomsInformation();
			AddOCIForAcidNumbers(FHLDetailsProvider.HandlingInformation);
			AddOCIForCustomsEntryNumbers();
			AddShipper();
			AddConsignee();
			AddChargeDeclarations(FHLDetailsProvider, false);
		}

		void AddShipper()
		{
			Elements.AddOptionalHeader(GetPartyElements(false, "SHP", FHLDetailsProvider.ShipperAccount, FHLDetailsProvider.ShipperName, FHLDetailsProvider.ShipperAddress, FHLDetailsProvider.ShipperAddress2, FHLDetailsProvider.ShipperPlace, FHLDetailsProvider.ShipperState, FHLDetailsProvider.ShipperCountryCode, FHLDetailsProvider.ShipperPostCode, FHLDetailsProvider.ShipperContactCode, FHLDetailsProvider.ShipperContactDetail.KeepNumericCharacters()));
		}

		void AddConsignee()
		{
			Elements.AddOptionalHeader(GetPartyElements(false, "CNE", FHLDetailsProvider.ConsigneeAccount, FHLDetailsProvider.ConsigneeName, FHLDetailsProvider.ConsigneeAddress, FHLDetailsProvider.ConsigneeAddress2, FHLDetailsProvider.ConsigneePlace, FHLDetailsProvider.ConsigneeState, FHLDetailsProvider.ConsigneeCountryCode, FHLDetailsProvider.ConsigneePostCode, FHLDetailsProvider.ConsigneeContactCode, FHLDetailsProvider.ConsigneeContactDetail.KeepNumericCharacters()));
		}

		void AddOCIForCustomsEntryNumbers()
		{
			if (FHLDetailsProvider.ShipperCountryCode == Constants.CountryCodes.Kenya)
			{
				var entryNum = string.Join("/", FHLDetailsProvider.CustomsEntryNumbers.Where(c => c.Type == CusEntryNumberTypes.Standard.ClearancePermitNumber && !c.Number.IsEmpty).Select(c => c.Number));
				if (!entryNum.IsNullOrEmpty())
				{
					AddOCILine(Constants.CountryCodes.Kenya, "EXP", "M", entryNum);
				}
			}
		}

		void AddMasterAWBConsignmentDetail()
		{
			ElementList masterAWBConsignmentDetail = new ElementList();
			masterAWBConsignmentDetail.AddLineIdentifier("MBI");
			masterAWBConsignmentDetail.AddSlant();

			masterAWBConsignmentDetail.AddHeader(AWBIdentification);
			masterAWBConsignmentDetail.AddHeader(AWBOriginAndDestination);
			masterAWBConsignmentDetail.AddHeader(AWBQuantityDetail);
			masterAWBConsignmentDetail.AddCRLF();

			Elements.AddHeader(masterAWBConsignmentDetail);
		}

		void AddHouseWaybillSummaryDetails()
		{
			ElementList hBS = new ElementList();
			hBS.AddLineIdentifier("HBS");
			hBS.AddSlant();
			//HWB Serial Number
			hBS.AddValue(new Format(12, 1, CharType.AlphaNumeric), FHLDetailsProvider.HouseBill);
			hBS.AddSlant();

			//Origin and Destination
			ElementList orgDest = new ElementList();
			//Origin			
			orgDest.AddValue(new Format(3, CharType.Alpha), FHLDetailsProvider.AWBOriginCode);
			//Destination
			orgDest.AddValue(new Format(3, CharType.Alpha), FHLDetailsProvider.AirportOfDestinationCode);
			hBS.AddHeader(orgDest);

			hBS.AddSlant();

			//House Waybill Totals
			ElementList totals = new ElementList();
			//Number of Pieces // this should be the slac 
			totals.AddValue(new Format(4, 0, CharType.Numeric), FHLDetailsProvider.TotalNoOfPieces);
			totals.AddSlant();
			string weightCode = "";
			if (FHLDetailsProvider.AWBRateLines.Any())
			{
				weightCode = FHLDetailsProvider.AWBRateLines.First().WeightInLBsOrKGs;
			}
			//Weight Code
			totals.AddValue(new Format(1, CharType.Alpha), weightCode);
			//Weight
			totals.AddValue(new Format(7, 0, CharType.NumericWithDecimal), FHLDetailsProvider.TotalGrossWeight);

			if (version == Version.No4)
			{
				totals.AddSlant();
				if (FHLDetailsProvider.ShippingLoadAndCount > 0)
				{
					totals.AddValue(new Format(5, 0, CharType.Numeric), FHLDetailsProvider.ShippingLoadAndCount);
				}
			}

			hBS.AddHeader(totals);

			//Nature Of Goods
			ElementList natureOfGoods = new ElementList();
			natureOfGoods.AddSlant();

			if (Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL && !FHLDetailsProvider.ManifestDescriptionOfGoods.IsEmpty)
			{
				natureOfGoods.AddValue(new Format(15, 0, CharType.Text), FHLDetailsProvider.ManifestDescriptionOfGoods);
			}
			else
			{
				natureOfGoods.AddValue(new Format(15, 0, CharType.Text), FHLDetailsProvider.AWBRateLines.First().NatureAndQtyOfGoodsDescription.Left(15).Trim());
			}

			hBS.AddHeader(natureOfGoods);

			if (version == Version.No4 && FWBDetailsProvider.AWBSpecialHandlingItems.Any())
			{
				var specialHandlingCodes = new ElementList();
				specialHandlingCodes.AddCRLF();

				foreach (ExportAWBSpecialHandling specialHandlingItem in FWBDetailsProvider.AWBSpecialHandlingItems)
				{
					specialHandlingCodes.AddSlant();
					specialHandlingCodes.AddValue(new Format(3, CharType.Alpha), specialHandlingItem.EP_SpecialHandling);
				}

				hBS.AddHeader(specialHandlingCodes);
			}

			hBS.AddCRLF();

			Elements.AddHeader(hBS);
		}

		void AddFreeTextDescriptionOfGoods()
		{
			ElementList freeTextDesc = new ElementList();
			freeTextDesc.AddLineIdentifier("TXT");

			var detailedGoodsDescription = FHLDetailsProvider.DetailedGoodsDescription.IsEmpty ? FHLDetailsProvider.NatureAndQtyOfGoods.Trim() : FHLDetailsProvider.DetailedGoodsDescription.Trim();
			//do once more to cater for the remaining chars - if Desc is a multiple of 65, additional one will be ignored because the element is optional and empty
			int numberOfRepeats = Math.Min((detailedGoodsDescription.Length / 65) + 1, 9);
			for (int i = 0; i < numberOfRepeats; i++)
			{
				ElementList tXTDescSegment = new ElementList();
				tXTDescSegment.AddSlant();
				tXTDescSegment.AddValue(new Format(65, 0, CharType.Text), detailedGoodsDescription.SubstringSafe(i * 65, 65));
				tXTDescSegment.AddCRLF();
				freeTextDesc.AddOptionalHeader(tXTDescSegment);
			}

			Elements.AddOptionalHeader(freeTextDesc);

			if (!detailedGoodsDescription.IsEmpty)
			{
				AddHarmonisedCodes();
			}
		}

		void AddHarmonisedCodes()
		{
			const int maxHTSItems = 9;
			const int minHTSChars = 6;
			const int maxHTSChars = 18;

			var harmonisedCodes = FHLDetailsProvider.GetAvailableHarmonisedCodes().ToArray()
				.Take(maxHTSItems);
			if (harmonisedCodes.Any())
			{
				var harmonisedCodesElementList = new ElementList();
				harmonisedCodesElementList.AddLineIdentifier("HTS");

				foreach (var code in harmonisedCodes)
				{
					harmonisedCodesElementList.AddSlant();
					harmonisedCodesElementList.AddValue(new Format(maxHTSChars, minHTSChars, CharType.AlphaNumeric), code.PadLeft(minHTSChars, '0'));
					harmonisedCodesElementList.AddCRLF();
				}

				Elements.AddOptionalHeader(harmonisedCodesElementList);
			}
		}

		void AddOtherCustomsInformation()
		{
			if (version != Version.No4)
			{
				return;
			}

			AddOCIForMRNs(FHLDetailsProvider.MovementReferenceNumbers);
			AddOCIForGDRNs(FHLDetailsProvider.GoodsDeclarationReferenceNumbers);
			AddOCIExportStatements(FHLDetailsProvider);
			AddVATAndContactNumbersToOCI(FHLDetailsProvider);

			var unnoValues = FHLDetailsProvider.DGUNNOValues();
			if (unnoValues.Any())
			{
				AddDGCodes(unnoValues);
			}

			Elements.AddOptionalHeader(OCISection);
		}
	}
}
