using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class QuantityUnitTest : TestCaseWithFactory
	{
		public void TestGetMeasureType()
		{
			var weirdPackType = Factory.NewWithValidTestData<RefPackType>();
			weirdPackType.F3_Code = "KG";
			Factory.Save();

			var expectedMeasureTypes = new Dictionary<string, MeasureType>();
			expectedMeasureTypes.Add(RatingConstants.Units.KG, MeasureType.Weight);
			expectedMeasureTypes.Add(RatingConstants.Units.M3, MeasureType.Volume);
			expectedMeasureTypes.Add(RatingConstants.Units.KM, MeasureType.Unit);
			expectedMeasureTypes.Add(RatingConstants.Units.CM, MeasureType.Length);
			expectedMeasureTypes.Add(RatingConstants.Units.CM2, MeasureType.Area);
			expectedMeasureTypes.Add(RatingConstants.Units.MI, MeasureType.Unit);
			expectedMeasureTypes.Add(RatingConstants.Units.HR, MeasureType.Time);
			expectedMeasureTypes.Add(RatingConstants.Units.DY, MeasureType.Time);
			expectedMeasureTypes.Add(RatingConstants.Units.WK, MeasureType.Time);
			expectedMeasureTypes.Add(RatingConstants.Units.SV, MeasureType.Unidentified);
			expectedMeasureTypes.Add(RatingConstants.Units.CN, MeasureType.ContainerCount);
			expectedMeasureTypes.Add(RatingConstants.Units.HB, MeasureType.Shipment);
			expectedMeasureTypes.Add(RatingConstants.Units.PK, MeasureType.Package);
			expectedMeasureTypes.Add(RatingConstants.Units.LI, MeasureType.Line);
			expectedMeasureTypes.Add(RatingConstants.Units.CP, MeasureType.ChargeablePallet);
			expectedMeasureTypes.Add(RatingConstants.Units.PL, MeasureType.LocationPallet);
			expectedMeasureTypes.Add(RatingConstants.Units.PI, MeasureType.PalletID);
			expectedMeasureTypes.Add(RatingConstants.Units.JU, MeasureType.JobUnit);
			expectedMeasureTypes.Add(RatingConstants.Units.JP, MeasureType.Package);
			expectedMeasureTypes.Add(RatingConstants.Units.JW, MeasureType.JobWeight);
			expectedMeasureTypes.Add(RatingConstants.Units.JV, MeasureType.JobVolume);
			expectedMeasureTypes.Add(RatingConstants.Units.FD, MeasureType.FDALine);
			expectedMeasureTypes.Add(RatingConstants.Units.PN, MeasureType.PNFDALine);
			expectedMeasureTypes.Add(RatingConstants.Units.OM, MeasureType.OMCLine);
			expectedMeasureTypes.Add(RatingConstants.Units.CS, MeasureType.CPSCLine);
			expectedMeasureTypes.Add(RatingConstants.Units.DE, MeasureType.DEALine);
			expectedMeasureTypes.Add(RatingConstants.Units.FC, MeasureType.FCCLine);
			expectedMeasureTypes.Add(RatingConstants.Units.DO, MeasureType.DOTLine);
			expectedMeasureTypes.Add(RatingConstants.Units.LY, MeasureType.LaceyLine);
			expectedMeasureTypes.Add(RatingConstants.Units.LW, MeasureType.LowestBill);
			expectedMeasureTypes.Add(RatingConstants.Units.TU, MeasureType.ContainerCount);
			expectedMeasureTypes.Add(RatingConstants.Units.LM, MeasureType.LoadingMeters);
			expectedMeasureTypes.Add(RatingConstants.Units.FI, MeasureType.CFIALine);
			expectedMeasureTypes.Add(RatingConstants.Units.NR, MeasureType.NRCANLine);
			expectedMeasureTypes.Add(RatingConstants.Units.IC, MeasureType.SITTLine);
			expectedMeasureTypes.Add(RatingConstants.Units.TC, MeasureType.TCLine);
			expectedMeasureTypes.Add(RatingConstants.Units.OP, MeasureType.OtherPGALine);
			expectedMeasureTypes.Add(RatingConstants.Units.HC, MeasureType.HCLine);
			expectedMeasureTypes.Add(RatingConstants.Units.PH, MeasureType.PHACLine);
			expectedMeasureTypes.Add(RatingConstants.Units.EC, MeasureType.ECCCLine);
			expectedMeasureTypes.Add(RatingConstants.Units.DF, MeasureType.DFOLine);
			expectedMeasureTypes.Add(RatingConstants.Units.SC, MeasureType.CNSCLine);
			expectedMeasureTypes.Add(RatingConstants.Units.GC, MeasureType.GACLine);
			expectedMeasureTypes.Add(RatingConstants.Units.N3, MeasureType.NMFS370);
			expectedMeasureTypes.Add(RatingConstants.Units.NA, MeasureType.NMFSAMR);
			expectedMeasureTypes.Add(RatingConstants.Units.NC, MeasureType.NMFSCOA);
			expectedMeasureTypes.Add(RatingConstants.Units.AM, MeasureType.AMS);
			expectedMeasureTypes.Add(RatingConstants.Units.AS, MeasureType.APHIS);
			expectedMeasureTypes.Add(RatingConstants.Units.AF, MeasureType.ATF);
			expectedMeasureTypes.Add(RatingConstants.Units.DC, MeasureType.DDTC);
			expectedMeasureTypes.Add(RatingConstants.Units.FS, MeasureType.FSIS);
			expectedMeasureTypes.Add(RatingConstants.Units.FW, MeasureType.FWS);
			expectedMeasureTypes.Add(RatingConstants.Units.NS, MeasureType.NMFSHMS);
			expectedMeasureTypes.Add(RatingConstants.Units.NP, MeasureType.NMFSSIM);
			expectedMeasureTypes.Add(RatingConstants.Units.PS, MeasureType.PST);
			expectedMeasureTypes.Add(RatingConstants.Units.HF, MeasureType.HFC);
			expectedMeasureTypes.Add(RatingConstants.Units.TB, MeasureType.TTB);
			expectedMeasureTypes.Add(RatingConstants.Units.VE, MeasureType.VNE);
			expectedMeasureTypes.Add(RatingConstants.Units.OD, MeasureType.ODS);
			expectedMeasureTypes.Add(RatingConstants.Units.TS, MeasureType.TSCA);
			expectedMeasureTypes.Add(RatingConstants.Units.CL, MeasureType.TCC);
			expectedMeasureTypes.Add(RatingConstants.Units.NO, MeasureType.NOP);
			expectedMeasureTypes.Add(RatingConstants.Units.ASD, MeasureType.APHISDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.CSD, MeasureType.CPSCDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.DED, MeasureType.DEADisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.DOD, MeasureType.DOTDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.ODD, MeasureType.ODSDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.PSD, MeasureType.PSTDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.HFD, MeasureType.HFCDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.TSD, MeasureType.TSCADisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.VED, MeasureType.VNEDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.FCD, MeasureType.FCCDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.FDD, MeasureType.FDADisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.FWD, MeasureType.FWSDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.LYD, MeasureType.LaceyDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.N3D, MeasureType.NMFS370Disclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.NAD, MeasureType.NMFSAMRDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.NSD, MeasureType.NMFSHMSDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.OMD, MeasureType.OMCDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.TBD, MeasureType.TTBDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.AMD, MeasureType.AMSDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.NOD, MeasureType.AMSNOPDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.FSD, MeasureType.FSISDisclaim);
			expectedMeasureTypes.Add(RatingConstants.Units.DOC, MeasureType.DeliveryOrders);
			expectedMeasureTypes.Add(RatingConstants.Units.L01, MeasureType.SteelLicenses);
			expectedMeasureTypes.Add(RatingConstants.Units.L02, MeasureType.SG_TPLCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L03, MeasureType.CA_NAFTA_TPLCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L04, MeasureType.MX_NAFTA_TPLCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L05, MeasureType.BeefExportCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L06, MeasureType.DiamondCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L07, MeasureType.ATPDEACertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L08, MeasureType.AU_FTA_ExportCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L09, MeasureType.MXCementLicense);
			expectedMeasureTypes.Add(RatingConstants.Units.L10, MeasureType.CAFTA_TPLCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L11, MeasureType.ALBCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L12, MeasureType.CottonShirtingFabricLicense);
			expectedMeasureTypes.Add(RatingConstants.Units.L13, MeasureType.HaitiEarnedAllowance);
			expectedMeasureTypes.Add(RatingConstants.Units.L14, MeasureType.AgriculturalLicense);
			expectedMeasureTypes.Add(RatingConstants.Units.L16, MeasureType.CAExportSugarCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L17, MeasureType.WoolLicense);
			expectedMeasureTypes.Add(RatingConstants.Units.L18, MeasureType.CBTPACertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L19, MeasureType.AGOATextileProvisionNumber);
			expectedMeasureTypes.Add(RatingConstants.Units.L20, MeasureType.OtherNonStandardVisa);
			expectedMeasureTypes.Add(RatingConstants.Units.L21, MeasureType.USDASugarCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L22, MeasureType.OrganicProductExemptionCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L23, MeasureType.AMSCertificateOfExemption);
			expectedMeasureTypes.Add(RatingConstants.Units.L25, MeasureType.DominicanRepublicEarnedAllowanceProgramCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L26, MeasureType.MexicanSugarExportLicense);
			expectedMeasureTypes.Add(RatingConstants.Units.L27, MeasureType.GeneralNote15cWaiverCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L28, MeasureType.AluminumLicenses);
			expectedMeasureTypes.Add(RatingConstants.Units.L29, MeasureType.CanadianUSMCA_TPLCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L30, MeasureType.MexicanUSMCA_TPLCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.L31, MeasureType.ArgentineWhiteGrapeJuiceConcentrateExportLicense);
			expectedMeasureTypes.Add(RatingConstants.Units.LKR, MeasureType.KRExportSteelCertificate);
			expectedMeasureTypes.Add(RatingConstants.Units.VIS, MeasureType.VISANumbers);
			expectedMeasureTypes.Add(RatingConstants.Units.PGA, MeasureType.PGALines);
			expectedMeasureTypes.Add(RatingConstants.Units.PGD, MeasureType.PGADisclaims);
			expectedMeasureTypes.Add(RatingConstants.Units.BOM, MeasureType.BOMKit);
			expectedMeasureTypes.Add(RatingConstants.Units.H92, MeasureType.HTS9902Line);
			expectedMeasureTypes.Add(RatingConstants.Units.H93, MeasureType.HTS9903Line);

			var count = 0;
			foreach (var info in typeof(QuantityUnit).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (info.Name.Length == 2 || info.Name.Length == 3)
				{
					var unitName = (string)info.GetValue(null);
					AssertEquals(unitName, expectedMeasureTypes[unitName], RatingCache.GetMeasureTypeFromUnit(unitName));
					count++;
				}
			}

			AssertEquals("Count", expectedMeasureTypes.Count, count);

			AssertEquals(Constants.PkgUnit.Unit, MeasureType.Unit, RatingCache.GetMeasureTypeFromUnit(Constants.PkgUnit.Unit));

			CombineAssertions("Listed units are not validated in RefPackTypeValidation:",
							  () =>
							  {
								  var units = RefPackTypeValidation.StandardUnitListIncludingRatingUnits;
								  foreach (var pair in expectedMeasureTypes)
								  {
									  Assert(pair.Key, units.Contains(pair.Key));
								  }
							  });

			var excessiveUnits = RefPackTypeValidation.StandardUnitListIncludingRatingUnits
				.Except(expectedMeasureTypes.Keys)
				.Except(RefPackTypeCollection.GetStandardUnitsAsCodeDescriptionPairs().Cast<ICodeDescription>().Select(x => x.Code));

			var excessiveUnitsSB = new ZStringBuilder(excessiveUnits);

			Assert(string.Format("Listed units are not in use and should NOT be validated in RefPackTypeValidation:\r\n{0}", excessiveUnitsSB.ToStringWithNewLineBetweenAppends()),
				   excessiveUnitsSB.IsEmpty);
		}

		public void TestGetUnitList()
		{
			var unexpected = new[]
			{
				QuantityUnit.LI,
				QuantityUnit.PL,
				QuantityUnit.PI
			};
			AssertUnitListByRateTypeAndUnexpected(RateType.Forwarding, unexpected);
			AssertUnitListByRateTypeAndUnexpected(RateType.Customs, unexpected);

			unexpected = new[]
			{
				QuantityUnit.HB,
				QuantityUnit.LW,
				QuantityUnit.HR,
				QuantityUnit.DY,
				QuantityUnit.WK,
				QuantityUnit.SV,
				QuantityUnit.LI,
				QuantityUnit.PL,
				QuantityUnit.PI
			};
			AssertUnitListByRateTypeAndUnexpected(RateType.Shipping, unexpected);

			unexpected = new[]
			{
				QuantityUnit.HB,
				QuantityUnit.LW,
				QuantityUnit.KM,
				QuantityUnit.MI,
				QuantityUnit.LI,
				QuantityUnit.PL,
				QuantityUnit.PI
			};
			AssertUnitListByRateTypeAndUnexpected(RateType.CFS, unexpected);

			unexpected = new[]
			{
				QuantityUnit.HB,
				QuantityUnit.LW,
				QuantityUnit.HR,
				QuantityUnit.LM
			};
			AssertUnitListByRateTypeAndUnexpected(RateType.Warehouse, unexpected, new[] { QuantityUnit.BOM });
			AssertUnitListByRateTypeAndUnexpected(RateType.TransitWarehouse, unexpected);
			AssertUnitListByRateTypeAndUnexpected(RateType.TransitWarehouseTransportationUnit, unexpected);

			unexpected = new[]
			{
				QuantityUnit.HB,
				QuantityUnit.LW,
				QuantityUnit.PK,
				QuantityUnit.CN,
				QuantityUnit.LM,
				QuantityUnit.KM,
				QuantityUnit.MI,
				QuantityUnit.LI,
				QuantityUnit.PL,
				QuantityUnit.PI
			};
			AssertUnitListByRateTypeAndUnexpected(RateType.ContainerYard, unexpected, new[] { QuantityUnit.CM, QuantityUnit.CM2 });

			unexpected = new[]
			{
				QuantityUnit.HB,
				QuantityUnit.LW,
				QuantityUnit.WK,
				QuantityUnit.LI,
				QuantityUnit.PL,
				QuantityUnit.PI
			};
			AssertUnitListByRateTypeAndUnexpected(RateType.TransportBookings, unexpected);

			unexpected = new[]
			{
				QuantityUnit.HB,
				QuantityUnit.LW,
				QuantityUnit.WK,
				QuantityUnit.TU,
				QuantityUnit.LI,
				QuantityUnit.PL,
				QuantityUnit.PI
			};
			AssertUnitListByRateTypeAndUnexpected(RateType.LocalTransport, unexpected);

			var expected = new[]
			{
				QuantityUnit.DY,
				QuantityUnit.WK
			};
			AssertUnitListByRateTypeAndExpected(RateType.ShippingImportDetention, expected);
			AssertUnitListByRateTypeAndExpected(RateType.ShippingExportDetention, expected);
		}

		void AssertUnitListByRateTypeAndUnexpected(RateType rateType, IEnumerable<string> unexpected, IEnumerable<string> additionalExpected = null)
		{
			var actual = UnitHelper.GetUnitList(Factory, rateType, "AU");
			var expected = baseExpected.Except(unexpected).Concat(pkgUnitList).Concat(additionalExpected ?? Enumerable.Empty<string>()).ToArray();

			AssertContainsExactElementsInAnyOrder(rateType.ToString(), expected, GetCodes(actual));
		}

		void AssertUnitListByRateTypeAndExpected(RateType rateType, IEnumerable<string> expected)
		{
			var actual = UnitHelper.GetUnitList(Factory, rateType, "AU");
			expected = expected.Concat(pkgUnitList);

			AssertContainsExactElementsInAnyOrder(rateType.ToString(), expected, GetCodes(actual));
		}

		public void TestIsPkgUnitOrPallet()
		{
			Assert(UnitHelper.IsPkgUnitOrPallet(Constants.PkgUnit.Unit));
			Assert(UnitHelper.IsPkgUnitOrPallet(Constants.PkgUnit.Bottle));
			Assert(UnitHelper.IsPkgUnitOrPallet(Constants.PkgUnit.Pallet));
		}

		public void TestIsLoadingMeter()
		{
			Assert(QuantityUnit.IsLoadingMeter(QuantityUnit.LM));
			Assert(!QuantityUnit.IsLoadingMeter(QuantityUnit.KG));
			Assert(!QuantityUnit.IsLoadingMeter(QuantityUnit.M3));
		}

		public void TestIsDistance()
		{
			AssertEquals(true, QuantityUnit.IsDistance(QuantityUnit.KM));
			AssertEquals(true, QuantityUnit.IsDistance(QuantityUnit.MI));

			AssertEquals(false, QuantityUnit.IsDistance(Constants.PkgUnit.Pallet));
			AssertEquals(false, QuantityUnit.IsDistance(QuantityUnit.M3));
			AssertEquals(false, QuantityUnit.IsDistance(QuantityUnit.KG));
		}

		public void TestIsTopPack()
		{
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ RefPackTypeSchema.Constants.TableName, 1 },
			};

			var factory = new BusinessObjectFactory();
			using (RowFactory.SetCachedTables())
			using (AssertDbHitsForAllFactories("", expectedDbHits, ignoreUnspecified: true))
			{
				AssertEquals(true, UnitHelper.IsTopPack(QuantityUnit.CN, factory));
				AssertEquals(true, UnitHelper.IsTopPack(QuantityUnit.PK, factory));
				AssertEquals(true, UnitHelper.IsTopPack(Constants.PkgUnit.Pallet, factory));
				AssertEquals(true, UnitHelper.IsTopPack(Constants.PkgUnit.Unit, factory));
				AssertEquals(true, UnitHelper.IsTopPack(Constants.PkgUnit.Bottle, factory));
				AssertEquals(true, UnitHelper.IsTopPack(Constants.PkgUnit.Unit, factory));

				AssertEquals(false, UnitHelper.IsTopPack(QuantityUnit.M3, factory));
				AssertEquals(false, UnitHelper.IsTopPack(QuantityUnit.KG, factory));
				AssertEquals(false, UnitHelper.IsTopPack(QuantityUnit.DY, factory));
			}
		}

		static string[] GetCodes(ICodeDescriptionPairList list)
		{
			var result = new List<string>();

			foreach (ICodeDescription pair in list)
			{
				result.Add(pair.Code);
			}

			return result.ToArray();
		}

		readonly string[] baseExpected = new string[]
		{
			Constants.Volume.CubicCentimeters,
			Constants.Volume.CubicFeet,
			Constants.Volume.CubicInches,
			Constants.Volume.CubicYards,
			Constants.Volume.CubicDecimetres,
			Constants.Volume.TeaChest,
			Constants.Volume.Litre,
			Constants.Volume.USGallons,
			Constants.Volume.ImperialGallons,
			Constants.Volume.MegaLitre,
			Constants.Volume.CubicMetres,
			Constants.Weight.Decitons,
			Constants.Weight.Grams,
			Constants.Weight.Hectograms,
			Constants.Weight.Kilograms,
			Constants.Weight.Kilotonnes,
			Constants.Weight.Pounds,
			Constants.Weight.PoundsTroy,
			Constants.Weight.MetricCarat,
			Constants.Weight.Milligrams,
			Constants.Weight.OuncesTroy,
			Constants.Weight.Ounces,
			Constants.Weight.Tonnes,
			Constants.Weight.LongTons,
			Constants.Weight.ShortTons,
			QuantityUnit.CN,
			QuantityUnit.HB,
			QuantityUnit.LW,
			QuantityUnit.PK,
			QuantityUnit.KM,
			QuantityUnit.MI,
			QuantityUnit.HR,
			QuantityUnit.DY,
			QuantityUnit.WK,
			QuantityUnit.SV,
			QuantityUnit.TU,
			QuantityUnit.LI,
			QuantityUnit.PL,
			QuantityUnit.LM,
			QuantityUnit.PI
		};

		readonly string[] pkgUnitList = new string[]
		{
			Constants.PkgUnit.Bag,
			Constants.PkgUnit.BulkBag,
			Constants.PkgUnit.BreakBulk,
			Constants.PkgUnit.BaleCompressed,
			Constants.PkgUnit.BaleUncompressed,
			Constants.PkgUnit.Bundle,
			Constants.PkgUnit.Bottle,
			Constants.PkgUnit.Box,
			Constants.PkgUnit.Basket,
			Constants.PkgUnit.Case,
			Constants.PkgUnit.Coil,
			Constants.PkgUnit.Cradle,
			Constants.PkgUnit.Crate,
			Constants.PkgUnit.Carton,
			Constants.PkgUnit.Cylinder,
			Constants.PkgUnit.Dozen,
			Constants.PkgUnit.Drum,
			Constants.PkgUnit.Envelope,
			Constants.PkgUnit.Gross,
			Constants.PkgUnit.Keg,
			Constants.PkgUnit.Mix,
			Constants.PkgUnit.Pail,
			Constants.PkgUnit.Piece,
			Constants.PkgUnit.Package,
			Constants.PkgUnit.Pallet,
			Constants.PkgUnit.Reel,
			Constants.PkgUnit.Roll,
			Constants.PkgUnit.RollOnRollOff,
			Constants.PkgUnit.Sheet,
			Constants.PkgUnit.Skid,
			Constants.PkgUnit.Spool,
			Constants.PkgUnit.Tote,
			Constants.PkgUnit.Tube,
			Constants.PkgUnit.Unit
		};
	}
}
