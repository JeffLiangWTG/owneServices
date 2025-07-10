using System;
using Enterprise.Rating.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Rateable.Test
{
	public class PartMeasureInfoProviderTest : TestCase
	{
		public void TestInstance()
		{
			var provider1 = PartMeasureInfoProvider.Instance;
			var provider2 = PartMeasureInfoProvider.Instance;
			AssertEquals("is singleton", true, ReferenceEquals(provider1, provider2));
		}

		public void TestAllMeasureTypes()
		{
			var provider = new PartMeasureInfoProvider();

			var allTypes = Enum.GetValues(typeof(MeasureType));

			foreach (MeasureType measureType in allTypes)
			{
				var info = provider.GetPartMeasureInfo(measureType);
				var typeText = measureType.ToString();
				AssertNotNull(typeText, info);

				var partMock = new Mock<IRateablePart>();
				var partListMock = new Mock<IRateablePartList>();
				var part = partMock.Object;

				switch (measureType)
				{
					case MeasureType.Weight:
					case MeasureType.JobWeight:
					case MeasureType.StorageWeight:
					case MeasureType.InnerPacksWeight:
						{
							partMock.Setup(x => x.WeightMeasure).Returns(CreateMeasureValue(1, 2, 3));
							partListMock.Setup(x => x.WeightUnit).Returns("KG");
							AssertEquals(1m, info.GetActualValue(partMock.Object));
							AssertEquals(2m, info.GetClientValue(partMock.Object));
							AssertEquals(3m, info.GetProviderValue(partMock.Object));
							AssertEquals("KG", info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Volume:
					case MeasureType.JobVolume:
					case MeasureType.StorageVolume:
					case MeasureType.InnerPacksVolume:
						{
							partMock.Setup(x => x.VolumeMeasure).Returns(CreateMeasureValue(4, 5, 6));
							partListMock.Setup(x => x.VolumeUnit).Returns("M3");
							AssertEquals(4m, info.GetActualValue(partMock.Object));
							AssertEquals(5m, info.GetClientValue(partMock.Object));
							AssertEquals(6m, info.GetProviderValue(partMock.Object));
							AssertEquals("M3", info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Area:
						{
							partMock.Setup(x => x.AreaMeasure).Returns(CreateMeasureValue(4, 5, 6));
							partListMock.Setup(x => x.AreaUnit).Returns("CM2");
							AssertEquals(4m, info.GetActualValue(partMock.Object));
							AssertEquals(5m, info.GetClientValue(partMock.Object));
							AssertEquals(6m, info.GetProviderValue(partMock.Object));
							AssertEquals("CM2", info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Length:
						{
							partMock.Setup(x => x.LengthMeasure).Returns(CreateMeasureValue(4, 5, 6));
							partListMock.Setup(x => x.LengthUnit).Returns("CM");
							AssertEquals(4m, info.GetActualValue(partMock.Object));
							AssertEquals(5m, info.GetClientValue(partMock.Object));
							AssertEquals(6m, info.GetProviderValue(partMock.Object));
							AssertEquals("CM", info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Shipment:
						{
							partMock.Setup(x => x.ShipmentCount).Returns(9m);
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.LowestBill:
						{
							partMock.Setup(x => x.LowestBillCount).Returns(9m);
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Package:
					case MeasureType.InnerPacksPackage:
						{
							partMock.Setup(x => x.PackageCount).Returns(9m);
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Line:
						{
							partMock.Setup(x => x.LineCount).Returns(9m);
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Unit:
					case MeasureType.JobUnit:
					case MeasureType.StorageUnit:
					case MeasureType.InnerPacksUnit:
					case MeasureType.BOMKit:
						{
							partMock.Setup(x => x.UnitCount).Returns(9m);
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.LoadingMeters:
						{
							partMock.Setup(x => x.LoadingMeterMeasure).Returns(CreateMeasureValue(4, 5, 6));
							AssertEquals(4m, info.GetActualValue(partMock.Object));
							AssertEquals(5m, info.GetClientValue(partMock.Object));
							AssertEquals(6m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Chargeable:
						{
							partMock.Setup(x => x.ChargeableMeasure).Returns(CreateMeasureValue(4, 5, 6));
							partListMock.Setup(x => x.ChargeableUnit).Returns("M3");
							AssertEquals(4m, info.GetActualValue(partMock.Object));
							AssertEquals(5m, info.GetClientValue(partMock.Object));
							AssertEquals(6m, info.GetProviderValue(partMock.Object));
							AssertEquals("M3", info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.ContainerCount:
						{
							AssertEquals(1m, info.GetActualValue(partMock.Object));
							AssertEquals(1m, info.GetClientValue(partMock.Object));
							AssertEquals(1m, info.GetProviderValue(partMock.Object));
							AssertEquals(Core.Constants.BusinessQuantityUnit.Container, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.ChargeablePallet:
						{
							partMock.Setup(x => x.ChargeablePalletCount).Returns(9m);
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.LocationPallet:
						{
							partMock.Setup(x => x.LocationPalletCount).Returns(9m);
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.PalletID:
						{
							AssertEquals(1m, info.GetActualValue(partMock.Object));
							AssertEquals(1m, info.GetClientValue(partMock.Object));
							AssertEquals(1m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.PickupDistance:
						{
							partMock.Setup(x => x.PickupDistance).Returns(9m);
							partListMock.Setup(x => x.PickupDistanceUnit).Returns("MI");
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals("MI", info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.DeliveryDistance:
						{
							partMock.Setup(x => x.DeliveryDistance).Returns(9m);
							partListMock.Setup(x => x.DeliveryDistanceUnit).Returns("MI");
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals("MI", info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Unidentified:
						partMock.Setup(x => x.UnidentifiedCount).Returns(5);
						AssertEquals(5m, info.GetActualValue(partMock.Object));
						AssertEquals(5m, info.GetClientValue(partMock.Object));
						AssertEquals(5m, info.GetProviderValue(partMock.Object));
						AssertEquals("SV", info.GetUnit(partListMock.Object));
						break;
					case MeasureType.WarehousePackage:
						AssertEquals(0m, info.GetActualValue(partMock.Object));
						AssertEquals(0m, info.GetActualValueWithUnit(partMock.Object, "BAG"));
						break;
					case MeasureType.WarehousePackageWeight:
						partListMock.Setup(x => x.WeightUnit).Returns("KG");
						AssertEquals(0m, info.GetActualValue(partMock.Object));
						break;
					case MeasureType.WarehousePackageVolume:
						partListMock.Setup(x => x.WeightUnit).Returns("M3");
						AssertEquals(0m, info.GetActualValue(partMock.Object));
						break;
					case MeasureType.FDALine:
					case MeasureType.PNFDALine:
					case MeasureType.OMCLine:
					case MeasureType.CPSCLine:
					case MeasureType.DEALine:
					case MeasureType.FCCLine:
					case MeasureType.DOTLine:
					case MeasureType.LaceyLine:
					case MeasureType.CFIALine:
					case MeasureType.SITTLine:
					case MeasureType.NRCANLine:
					case MeasureType.TCLine:
					case MeasureType.HCLine:
					case MeasureType.PHACLine:
					case MeasureType.ECCCLine:
					case MeasureType.DFOLine:
					case MeasureType.CNSCLine:
					case MeasureType.GACLine:
					case MeasureType.OtherPGALine:
					case MeasureType.NMFS370:
					case MeasureType.NMFSCOA:
					case MeasureType.NMFSAMR:
					case MeasureType.NMFSHMS:
					case MeasureType.NMFSSIM:
					case MeasureType.AMS:
					case MeasureType.APHIS:
					case MeasureType.ATF:
					case MeasureType.DDTC:
					case MeasureType.FSIS:
					case MeasureType.FWS:
					case MeasureType.PST:
					case MeasureType.HFC:
					case MeasureType.TTB:
					case MeasureType.VNE:
					case MeasureType.ODS:
					case MeasureType.TSCA:
					case MeasureType.TCC:
					case MeasureType.NOP:
					case MeasureType.FDADisclaim:
					case MeasureType.OMCDisclaim:
					case MeasureType.CPSCDisclaim:
					case MeasureType.DEADisclaim:
					case MeasureType.FCCDisclaim:
					case MeasureType.DOTDisclaim:
					case MeasureType.LaceyDisclaim:
					case MeasureType.NMFS370Disclaim:
					case MeasureType.NMFSAMRDisclaim:
					case MeasureType.NMFSHMSDisclaim:
					case MeasureType.AMSDisclaim:
					case MeasureType.AMSNOPDisclaim:
					case MeasureType.APHISDisclaim:
					case MeasureType.FSISDisclaim:
					case MeasureType.FWSDisclaim:
					case MeasureType.PSTDisclaim:
					case MeasureType.HFCDisclaim:
					case MeasureType.TTBDisclaim:
					case MeasureType.VNEDisclaim:
					case MeasureType.ODSDisclaim:
					case MeasureType.TSCADisclaim:
					case MeasureType.DeliveryOrders:
					case MeasureType.SteelLicenses:
					case MeasureType.SG_TPLCertificate:
					case MeasureType.CA_NAFTA_TPLCertificate:
					case MeasureType.MX_NAFTA_TPLCertificate:
					case MeasureType.BeefExportCertificate:
					case MeasureType.DiamondCertificate:
					case MeasureType.ATPDEACertificate:
					case MeasureType.AU_FTA_ExportCertificate:
					case MeasureType.MXCementLicense:
					case MeasureType.CAFTA_TPLCertificate:
					case MeasureType.ALBCertificate:
					case MeasureType.CottonShirtingFabricLicense:
					case MeasureType.HaitiEarnedAllowance:
					case MeasureType.AgriculturalLicense:
					case MeasureType.CAExportSugarCertificate:
					case MeasureType.WoolLicense:
					case MeasureType.CBTPACertificate:
					case MeasureType.AGOATextileProvisionNumber:
					case MeasureType.OtherNonStandardVisa:
					case MeasureType.USDASugarCertificate:
					case MeasureType.OrganicProductExemptionCertificate:
					case MeasureType.AMSCertificateOfExemption:
					case MeasureType.DominicanRepublicEarnedAllowanceProgramCertificate:
					case MeasureType.MexicanSugarExportLicense:
					case MeasureType.GeneralNote15cWaiverCertificate:
					case MeasureType.AluminumLicenses:
					case MeasureType.CanadianUSMCA_TPLCertificate:
					case MeasureType.MexicanUSMCA_TPLCertificate:
					case MeasureType.ArgentineWhiteGrapeJuiceConcentrateExportLicense:
					case MeasureType.KRExportSteelCertificate:
					case MeasureType.VISANumbers:
					case MeasureType.PGALines:
					case MeasureType.PGADisclaims:
					case MeasureType.HTS9902Line:
					case MeasureType.HTS9903Line:
						{
							partMock.Setup(x => x.GetDeclarationLineCount(Moq.It.Is<MeasureType>(arg => arg == measureType))).Returns(9);
							AssertEquals(9m, info.GetActualValue(partMock.Object));
							AssertEquals(9m, info.GetClientValue(partMock.Object));
							AssertEquals(9m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					case MeasureType.Time:
						{
							AssertEquals(0m, info.GetActualValue(partMock.Object));
							AssertEquals(0m, info.GetClientValue(partMock.Object));
							AssertEquals(0m, info.GetProviderValue(partMock.Object));
							AssertEquals(string.Empty, info.GetUnit(partListMock.Object));
							break;
						}
					default:
						{
							Fail("Need a case for " + measureType);
							break;
						}
				}
			}
		}

		static IClientProviderValues CreateMeasureValue(decimal actual, decimal forClient, decimal forProvider)
		{
			var mock = new Mock<IClientProviderValues>();
			mock.Setup(x => x.Actual).Returns(actual);
			mock.Setup(x => x.ForProvider).Returns(forProvider);
			mock.Setup(x => x.ForClient).Returns(forClient);
			return mock.Object;
		}
	}
}
