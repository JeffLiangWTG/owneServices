using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class CTZCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<CartageZoneDistanceCalculator, Xsd.CTZCalculator>
	{
		#region Import

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void ImportFromValueObjectCore(RateLine rateLine, CartageZoneDistanceCalculator calculator, Xsd.CTZCalculator calculatorXSD, IValueObjectImportContext context)
		{
			calculator.EquipmentType = calculatorXSD.Equipment;

			if (calculatorXSD.ConversionFactorSpecified)
			{
				rateLine.ConversionFactor = new ConversionFactor(calculatorXSD.ConversionFactor, calculatorXSD.ConversionFactorNumeratorUnit, calculatorXSD.ConversionFactorDenominatorUnit);
			}

			calculator.CartageZones.RemoveAndDeleteAll();

			bool hasUnknownCartages = false;

			foreach (Xsd.CTZZone zoneXSD in calculatorXSD.Zones)
			{
				if (zoneXSD.SimpleRate.MinimumSpecified || zoneXSD.SimpleRate.MaximumSpecified || zoneXSD.SimpleRate.BaseRateSpecified || zoneXSD.SimpleRate.PerUnitSpecified || zoneXSD.RateItems.IsSpecified)
				{
					ZString zoneName = zoneXSD.Code == "Standard" ? ZString.Empty : zoneXSD.Code;
					CartageZone zone = GetZoneWithACIFallback(zoneName, calculator);
					if (zone != null)
					{
						if (ValidateRateLineItem(zoneXSD.SimpleRate, zoneXSD.RateItems, context))
						{
							if (zoneXSD.SimpleRate.MinimumSpecified)
							{
								ImportRateLineItem(zone.ZoneRateLineItems.AddNew(), Calculator.Items.Operator.MIN, zoneXSD.SimpleRate.Minimum, zone);
							}

							if (zoneXSD.SimpleRate.BaseRateSpecified)
							{
								ImportRateLineItem(zone.ZoneRateLineItems.AddNew(), Calculator.Items.Operator.BAS, zoneXSD.SimpleRate.BaseRate, zone);
							}

							if (zoneXSD.SimpleRate.MaximumSpecified)
							{
								ImportRateLineItem(zone.ZoneRateLineItems.AddNew(), Calculator.Items.Operator.MAX, zoneXSD.SimpleRate.Maximum, zone);
							}

							if (zoneXSD.SimpleRate.PerUnitSpecified)
							{
								ImportRateLineItem(zone.ZoneRateLineItems.AddNew(), Calculator.Items.Operator.UNT, zoneXSD.SimpleRate.PerUnit, zone);
							}
							else
							{
								ImportRateItemWithOperatorAndBreak(calculator, zoneXSD.RateItems, zone, context);
							}
						}
					}
					else
					{
						context.Notify(new ErrorNotification(RateErrorType.UnknownCartageZone, zoneXSD.Code));
						hasUnknownCartages = true;
					}
				}
			}

			if (hasUnknownCartages)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("136ac9e7-e0d2-4d5c-94af-3e3accf2e930", "Please set up Transport Zone Sets in 'Admin-> Location -> Transport Zone Sets' before continuing.")));
			}
		}

		static void ImportRateLineItem(RateLineItem lineItem, ZString rateOperator, ZDecimal amount, CartageZone zone)
		{
			SetZoneOnRateItem(lineItem, zone);
			lineItem.TM_Type = rateOperator;
			lineItem.TM_RelevantValue = amount;
		}

		CartageZone GetZoneWithACIFallback(string zoneName, CartageZoneDistanceCalculator calculator)
		{
			CartageZone zone = calculator.CartageZones.FindZone(zoneName);
			if (zone == null && (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada))
			{
				// Order is important
				calculator.UseACIZones = true;
				calculator.ReloadCartageZones();
				calculator.CartageZones.Load();

				zone = calculator.CartageZones.FindZone(zoneName);
				if (zone == null)
				{
					calculator.UseACIZones = false;
					calculator.ReloadCartageZones();
					calculator.CartageZones.Load();
				}
			}

			return zone;
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, CartageZoneDistanceCalculator calculator, Xsd.CTZCalculator calculatorXSD, INotifications notifications)
		{
			calculatorXSD.Equipment = calculator.EquipmentType;

			if (!rateLine.ConversionFactor.IsEmpty)
			{
				calculatorXSD.ConversionFactor = rateLine.ConversionFactor.Factor;
				calculatorXSD.ConversionFactorNumeratorUnit = rateLine.ConversionFactor.NumeratorUnit;
				calculatorXSD.ConversionFactorDenominatorUnit = rateLine.ConversionFactor.DenominatorUnit;
			}

			foreach (CartageZone cartageZone in calculator.CartageZones)
			{
				Xsd.CTZZone zoneXSD = calculatorXSD.Zones.AddNew();

				zoneXSD.Code = cartageZone.ZoneName.IsEmpty ? (ZString)(NoResString)"Standard" : cartageZone.ZoneName; // Hard-coded constant

				foreach (RateLineItem lineItem in cartageZone.ZoneRateLineItems)
				{
					switch (lineItem.TM_Type)
					{
						case Calculator.Items.Operator.MIN:
							zoneXSD.SimpleRate.Minimum = lineItem.TM_RelevantValue;
							break;

						case Calculator.Items.Operator.BAS:
							zoneXSD.SimpleRate.BaseRate = lineItem.TM_RelevantValue;
							break;

						case Calculator.Items.Operator.UNT:
							zoneXSD.SimpleRate.PerUnit = lineItem.TM_RelevantValue;
							break;

						case Calculator.Items.Operator.MAX:
							zoneXSD.SimpleRate.Maximum = lineItem.TM_RelevantValue;
							break;
					}
				}

				ExportRateItemWithOperatorAndBreak(calculator, zoneXSD.RateItems, cartageZone, notifications);
			}
		}

		protected override RateLineItemsView GetRateLineItems(CartageZoneDistanceCalculator calculator, CartageZone zone)
		{
			return zone.ZoneRateLineItems;
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.CTZCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return CartageZoneDistanceCalculator.Code; }
		}

		#endregion
	}
}

