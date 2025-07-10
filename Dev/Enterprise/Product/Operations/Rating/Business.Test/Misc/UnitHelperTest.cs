using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class UnitHelperTest : RatingTestCase
	{
		public void TestGetWeightVolumeWithTeu()
		{
			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("FCL", "SEA", "AUSYD", "USLAX").RateLines[0];
			var line = rateLine.RateLineItems.AddNew();
			AssertEquals("Twenty foot equivalent unit", line.Lookups.WeightVolumes.GetDescriptionFromCode(QuantityUnit.TU));
		}

		public void TestGetWeightVolumeWithoutTeu()
		{
			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("FCL", "AIR", "AUSYD", "USLAX").RateLines[0];
			var line = rateLine.RateLineItems.AddNew();
			var weightVolumes = line.Lookups.WeightVolumes;
			AssertNull("Don't have to return Twenty foot equivalent unit", weightVolumes.GetDescriptionFromCode(QuantityUnit.TU));
		}

		public void TestUnitMultiple()
		{
			var helper = new UnitHelper();
			var pairList = helper.GetUnitMultiple(Factory);

			AssertEquals(5, pairList.Count);
			AssertEquals(string.Empty, pairList[0].Code);
			AssertEquals("10", pairList[1].Code);
			AssertEquals("100", pairList[2].Code);
			AssertEquals("1000", pairList[3].Code);
			AssertEquals("10000", pairList[4].Code);
		}

		public void TestGetForwardingAndCustomsUnits()
		{
			AssertEquals(@"DT - Decitons
G - Grams
HG - Hectograms
KG - Kilograms
KT - Kilotons
LB - Pounds
LT - Pounds Troy
MC - Metric Carat
MG - Milligrams
OT - Ounces Troy
OZ - Ounces
T - Tonnes
TL - Long Tons (2240 lb)
TN - Short Tons (2000 lb)
CC - Cubic Centimeters
CF - Cubic Feet
CI - Cubic Inches
CY - Cubic Yards
D3 - Cubic Decimeters
GA - US Gallons
GI - Imperial Gallons
L - Liter
M3 - Cubic Meters
ML - Mega Liter
TE - Tea Chest
CN - Container
HB - House Bill
LW - Lowest Bill
PK - Package
KM - Kilometer
MI - Mile
HR - Hour
DY - Day
WK - Week
SV - Service Occurrence
TU - Twenty foot equivalent unit
LM - Loading Meter
BAG - Bag
BBG - Bulk Bag
BBK - Break Bulk
BLC - Bale, Compressed
BLU - Bale, Uncompressed
BND - Bundle
BOT - Bottle
BOX - Box
BSK - Basket
CAS - Case
COI - Coil
CRD - Cradle
CRT - Crate
CTN - Carton
CYL - Cylinder
DOZ - Dozen
DRM - Drum
ENV - Envelope
GRS - Gross
KEG - Keg
MIX - Mix
PAI - Pail
PCE - Piece
PKG - Package
PLT - Pallet
REL - Reel
RLL - Roll
ROR - Roll-on/roll-off
SHT - Sheet
SKD - Skid
SPL - Spool
TOT - Tote
TUB - Tube
UNT - Unit", UnitHelper.GetForwardingAndCustomsUnits(Factory, CountryCodes.VietNam).ElementsAsString);
		}
	}
}
