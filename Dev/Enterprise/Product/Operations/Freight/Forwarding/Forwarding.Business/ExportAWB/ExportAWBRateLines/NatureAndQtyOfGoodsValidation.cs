using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class NatureAndQtyOfGoodsValidation : Forwarding.AWB.Business.NatureAndQtyOfGoodsValidation
	{
		public NatureAndQtyOfGoodsValidation(NatureAndQtyOfGoods parent) : base(parent)
		{
		}

		protected new NatureAndQtyOfGoods Parent => (NatureAndQtyOfGoods)base.Parent;

		protected override void CheckText()
		{
			base.CheckText();

			if (Parent.ParentRateLine.IsHSCodeLine)
			{
				if (HasDestination(Core.Constants.CountryCodes.Argentina) && !IsValidHSCode(Parent.Text))
				{
					Parent.TextInfo.AddMessageError(HSCodeErrorMessageForArgentina);
				}

				if (!IsValidHSCode(Parent.Text) || IsHSCodeMissing)
				{
					if (HasInboundToICS2Zone && !IsConsigneeAdvanceCargoReportingSelfFilerSet)
					{
						AddEUHSCodeMessage(HSCodeErrorMessageForEU);
					}
					if (IsImportToExportFromTransitingThroughUnitedArabEmirates)
					{
						Parent.TextInfo.AddMessageError(HSCodeErrorMessageForUnitedArabEmirates);
					}
				}
			}

			if (IsHSCodeMissing && IsHSCodeMandatory)
			{
				if (HasInboundToICS2Zone && !IsConsigneeAdvanceCargoReportingSelfFilerSet)
				{
					AddEUHSCodeMessage(HSCodeErrorMessageForEU);
				}
				if (IsImportToExportFromTransitingThroughUnitedArabEmirates)
				{
					Parent.TextInfo.AddMessageError(HSCodeErrorMessageForUnitedArabEmirates);
				}
				if (!HasInboundToICS2Zone && !IsImportToExportFromTransitingThroughUnitedArabEmirates
					&& (!Parent.ParentRateLine?.Master?.AWBRateLines?.Cast<ExportAWBRateLine>()?.Any(l => l.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode) ?? false))
				{
					Parent.TextInfo.AddMessageError(HSCodeErrorMessageForMAWB);
				}
			}

			AddHSCodeMessageForBasel();
			AddHsCodeMessageForMorocco();
		}

		bool IsHSCodeMissing
		{
			get
			{
				var ratelines = Parent.ParentRateLine?.Master?.AWBRateLines?.Cast<ExportAWBRateLine>();
				if (ratelines is null)
				{
					return false;
				}

				return Parent.ParentRateLine.Master.GetShipmentReferencesWithoutHSCode().Count != 0;
			}
		}

		bool IsHSCodeMandatory
		{
			get
			{
				var ratelines = Parent.ParentRateLine?.Master?.AWBRateLines?.Cast<ExportAWBRateLine>();

				if (ratelines == null)
				{
					return false;
				}

				var firstGoodsLine = ratelines.FirstOrDefault(l => l.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription);
				var isMandatory = Parent.ParentRateLine.RequireHSCode && Parent.ParentRateLine == firstGoodsLine;
				return isMandatory;
			}
		}

		string HSCodeErrorMessageForArgentina
		{
			get
			{
				var master = Parent.ParentRateLine?.Master;
				if (master != null)
				{
					var shipmentRefences = master.GetShipmentReferencesWithoutHSCode();

					return shipmentRefences.Count > 0
						? Res.GetString("7368a132-0ab4-41b7-afca-3b8eb752af46", "For Shipments destined to Argentina HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one {0}.", string.Join(", ", shipmentRefences))
						: Res.GetString("78c72d57-0bed-495f-9079-5a5f1f2581eb", "For Shipments destined to Argentina HS Code length must be between 6 and 18 digits long.");
				}
				return string.Empty;
			}
		}

		string HSCodeErrorMessageForEU
		{
			get
			{
				var master = Parent.ParentRateLine?.Master;
				if (master != null)
				{
					var shipmentRefences = master.GetShipmentReferencesWithoutHSCode();

					return shipmentRefences.Count > 0
						? Res.GetString("6e97e098-46d3-4bf7-a5fb-522af8f2fd79", "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one {0}.", string.Join(", ", shipmentRefences))
						: Res.GetString("361b1bba-d28a-424e-855e-e81192dc574a", "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long.");
				}
				return string.Empty;
			}
		}

		string HSCodeErrorMessageForUnitedArabEmirates
		{
			get
			{
				var master = Parent.ParentRateLine?.Master;
				if (master != null)
				{
					var shipmentRefences = master.GetShipmentReferencesWithoutHSCode();

					return shipmentRefences.Count > 0
						? Res.GetString("45711b96-f276-406c-8eff-4c267fb5c06f", "For inbound, outbound and transiting shipments to/from/via UAE HS Code length must be between 6 and 18 digits long. An HS code is missing from at least one {0}.", string.Join(", ", shipmentRefences))
						: Res.GetString("af80e83b-5810-4ab1-a9b8-5270fd018682", "For inbound, outbound and transiting shipments to/from/via UAE HS Code length must be between 6 and 18 digits long.");
				}
				return string.Empty;
			}
		}

		string HSCodeErrorMessageForMAWB
		{
			get
			{
				var destination = Parent.ParentRateLine.Master.DestinationCountry?.Description ?? ZString.Empty;
				return IsAttachedToMAWB
					? Res.GetString("47B57920-6AFA-4698-B688-18BC6C212AE3", "HS code is mandatory for destination {0}. Enter it on the pack line(s) or override and select H identifier to include it.", destination)
					: Res.GetString("1ac5b587-abaa-4a62-a04b-bb7331f58bc5", "HS code is mandatory for destination {0}.", destination);
			}
		}

		bool HasInboundToICS2Zone => Parent.ParentRateLine.Master?.HasInboundToICS2Zone ?? false;

		ZBool IsConsigneeAdvanceCargoReportingSelfFilerSet => Parent.ParentRateLine.Master?.EH_IsConsigneeDeclarantForAdvanceCargoReporting ?? false;

		bool IsImportToExportFromTransitingThroughUnitedArabEmirates => Parent.ParentRateLine.Master?.IsImportToExportFromTransitingThroughUnitedArabEmirates ?? false;

		bool IsAnyDischargePortInMorocco => Parent.ParentRateLine.Master?.IsAnyDischargePortInMorocco ?? false;

		bool HasDestination(string destinationCountryCode)
		{
			return Parent.ParentRateLine.Master?.DestinationCountry?.Code.ToString() == destinationCountryCode;
		}

		bool IsValidHSCode(ZString text)
		{
			return IsAttachedToMAWB
				? Regex.IsMatch(text.Replace("HS Code: ", string.Empty), "^[\\d]{6,18}$")
				: Regex.IsMatch(text.Replace("HS Codes: ", string.Empty), "^([\\d]{6,18},? ?)+$");
		}

		void AddEUHSCodeMessage(string message)
		{
			if (!Parent.ParentRateLine.Master.IsAWBOverridden)
			{
				if (Parent.ParentRateLine.Master.ConsigneeCategory != OrgConstants.Category.NaturalPersonIndividual ||
					Parent.ParentRateLine.Master.ShipperCategory != OrgConstants.Category.NaturalPersonIndividual)
				{
					Parent.TextInfo.AddMessageError(message);
				}
			}
			else
			{
				Parent.TextInfo.AddWarning(message);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Search prefix string")]
		void AddHSCodeMessageForBasel()
		{
			var master = Parent.ParentRateLine?.Master;
			if (master == null || master.SwitzerlandDepartureFlightCode != "CHBSL")
			{
				return;
			}

			if (!master.ShipmentNumberWithEmptyHSCode.IsEmpty)
			{
				var firstRateLineWithHSCode = master.AWBRateLines.Cast<ExportAWBRateLine>().FirstOrDefault(x => x.NatureAndQtyOfGoods.Text.StartsWith("HS Codes:") || (x.NatureAndQtyOfGoods.Text.StartsWith("HS Code:")));
				if ((firstRateLineWithHSCode == null && Parent.ParentRateLine == master.AWBRateLine1)
					|| firstRateLineWithHSCode != null && Parent.ParentRateLine == firstRateLineWithHSCode)
				{
					Parent.TextInfo.AddMessageError(Res.GetString("f9b5efc1-1d2c-4357-9fed-dd13979dcec1", "For exports from Basel, Switzerland by airfreight, to meet RFS (Road Feeder Service) customs filing requirements, the HS Code is mandatory. A HS Code is missing from at least one Packline of {0}.", master.ShipmentNumberWithEmptyHSCode));
				}
			}
		}

		void AddHsCodeMessageForMorocco()
		{
			if (!IsAnyDischargePortInMorocco)
			{
				return;
			}

			var master = Parent.ParentRateLine?.Master;
			if (master == null)
			{
				return;
			}

			AddHsCodeLengthErrorHAWB();

			var shipmentReferences = master.GetShipmentReferencesWithoutHSCode();
			if (shipmentReferences.Any())
			{
				AddHsCodeMissingMessageForMorocco(master, shipmentReferences);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Search prefix string")]
		void AddHsCodeMissingMessageForMorocco(ExportAWBHeader master, List<ZString> shipmentReferences)
		{
			var firstRateLineWithHSCode = master.AWBRateLines.Cast<ExportAWBRateLine>().FirstOrDefault(x =>
				x.NatureAndQtyOfGoods.Text.StartsWith("HS Codes:") ||
				(x.NatureAndQtyOfGoods.Text.StartsWith("HS Code:")));

			if ((firstRateLineWithHSCode == null && Parent.ParentRateLine == master.AWBRateLine1) || firstRateLineWithHSCode != null && Parent.ParentRateLine == firstRateLineWithHSCode)
			{
				Parent.TextInfo.AddMessageError(Res.GetString("07af7652-1bd4-4e48-ac6e-7e0d94b2a1e2",
					"For inbound shipments to Morocco, an HS code is mandatory. An HS code is missing from at least one {0}.",
					string.Join(", ", shipmentReferences)));
			}
		}

		void AddHsCodeLengthErrorHAWB()
		{
			if (Parent.ParentRateLine.IsHSCodeLine && !IsValidHSCode(Parent.Text) && !IsAttachedToMAWB)
			{
				Parent.TextInfo.AddMessageError(Res.GetString("8d8db717-2779-4f83-8c75-ca5c679b49a1",
					"Harmonized Commodity Code must be between 6 and 18 characters long."));
			}
		}
	}
}
