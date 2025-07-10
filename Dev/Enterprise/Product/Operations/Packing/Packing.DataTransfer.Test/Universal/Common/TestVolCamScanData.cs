using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	sealed class TestVolCamScanData : TestCaseWithFactory
	{
		#region VolCamXML

		readonly string ValidVolCamEvent = VolCamEventBuilder("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10CM|WID=20CM|HGT=30CM|VOL=6000CC");

		static string VolCamEventBuilder(string eventReference) => $@"<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>SSC</EventType>
		<EventReference>{eventReference}</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>TransitReceive</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>GoodsItemID</Type>
				<Value>TESTBARCODE123456789</Value>
			</Context>
			<Context>
				<Type>WarehouseCode</Type>
				<Value>TRA</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region TestGetBarcode

		public void TestGetBarcode()
		{
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ValidVolCamEvent);

			AssertEquals("Should correctly get barcode", "TESTBARCODE123456789", VolCamScanData.GetBarcode(xmlEvent));
		}

		#endregion

		#region TestGetWarehouseCode

		public void TestGetWarehouseCode()
		{
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(ValidVolCamEvent);

			AssertEquals("Should correctly get WarehouseCode", "TRA", VolCamScanData.GetWarehouseCode(xmlEvent));
		}

		#endregion

		#region TestPackagesShouldNotUpdateInvalidInfo

		#region PackagesShouldNotUpdateIfEventIsInvalid

		public void TestPackagesShouldNotUpdateIfEventIsInvalid()
		{
			PackagesShouldNotUpdate("FAC=TWS|LOC=AUSYD|WGT=1.200|LEN=10|WID=20|HGT=30|VOL=6000");
		}

		#endregion

		#region TestPackagesShouldNotUpdateDimensionsIfEventDimensionsAreInvalid

		public void TestPackagesShouldNotUpdateDimensionsIfDimensionsMissingUQ()
		{
			PackagesShouldNotUpdate("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10|WID=20|HGT=30|VOL=6000CY", exceptWeight: true, exceptVolume: true);
		}

		#endregion

		#region TestPackagesShouldNotUpdateDimensionsIfDimensionsUQsAreNotInTheSameSystem

		public void TestPackagesShouldNotUpdateDimensionsIfDimensionsUQsAreNotInTheSameSystem()
		{
			PackagesShouldNotUpdate("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=17.22CM|WID=22IN|HGT=20CM|VOL=1.524CY", exceptWeight: true, exceptVolume: true);
		}

		#endregion

		#region TestPackagesShouldOnlyUpdateNonNegativeInfo

		public void TestPackagesShouldOnlyUpdateNonNegativeInfo()
		{
			PackagesShouldNotUpdate("FAC=TWS|LOC=AUSYD|WGT=-1.200KG|LEN=17.22CM|WID=22CM|HGT=20CM|VOL=1.524CY", exceptDimensions: true, exceptVolume: true);
			PackagesShouldNotUpdate("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=-17.22CM|WID=22CM|HGT=20CM|VOL=1.524CY", exceptWeight: true, exceptVolume: true);
			PackagesShouldNotUpdate("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=17.22CM|WID=-22CM|HGT=20CM|VOL=1.524CY", exceptWeight: true, exceptVolume: true);
			PackagesShouldNotUpdate("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=17.22CM|WID=22CM|HGT=-20CM|VOL=1.524CY", exceptWeight: true, exceptVolume: true);
			PackagesShouldNotUpdate("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=17.22CM|WID=22CM|HGT=20CM|VOL=-1.524CY", exceptWeight: true, exceptDimensions: true);
		}

		#endregion

		public void PackagesShouldNotUpdate(string eventString, bool exceptWeight = false, bool exceptDimensions = false, bool exceptVolume = false)
		{
			var eventDeserializer = new XmlEventDeserializer();

			var originalPackage = Factory.New<PkgPackage>();
			originalPackage.KP_Weight = 1;
			originalPackage.KP_Width = 2;
			originalPackage.KP_Height = 3;
			originalPackage.KP_Length = 4;
			originalPackage.KP_Volume = 5;

			var xmlEvent = (UniversalEvent)(eventDeserializer.Parse(VolCamEventBuilder(eventString)));
			VolCamScanData.PopulatePackageFromEvent(xmlEvent, originalPackage);

			if (!exceptWeight)
			{
				AssertEquals("Original package shouldn't have changed", 1m, originalPackage.KP_Weight);
			}
			if (!exceptDimensions)
			{
				AssertEquals("Original package shouldn't have changed", 2m, originalPackage.KP_Width);
				AssertEquals("Original package shouldn't have changed", 3m, originalPackage.KP_Height);
				AssertEquals("Original package shouldn't have changed", 4m, originalPackage.KP_Length);
			}
			if (!exceptVolume && !exceptDimensions)
			{
				AssertEquals("Original package shouldn't have changed", 5m, originalPackage.KP_Volume);
			}
		}

		#endregion

		#region TestPackageShouldUpdateValueAndUnits

		#region TestPackageShouldUpdateValueAndUnitsWhenImperialToMetric

		public void TestPackageShouldUpdateValueAndUnitsWhenImperialToMetric()
		{
			PackageShouldUpdateValueAndUnits("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=15CM|WID=20CM|HGT=30CM|VOL=1.524M3",
				"LB", 1, "KG", 1.200m,
				10, 15, 10, 20,
				10, 30, "IN", "CM",
				"CY", 0.021952m, "M3", 1.524m);
		}

		#endregion

		#region TestPackageShouldUpdateValueAndUnitsWhenMetricToImperial

		public void TestPackageShouldUpdateValueAndUnitsWhenMetricToImperial()
		{
			PackageShouldUpdateValueAndUnits("FAC=TWS|LOC=AUSYD|WGT=4.5LB|LEN=17.22IN|WID=22IN|HGT=30IN|VOL=1.342CY",
				"KG", 2, "LB", 4.5m,
				20, 17.22m, 15.2m, 22,
				13.333m, 30, "CM", "IN",
				"M3", 0.03553m, "CY", 1.342m);
		}

		#endregion

		#region TestPackageShouldUpdateValueAndUnitsWhenMetricToMetric

		public void TestPackageShouldUpdateValueAndUnitsWhenMetricToMetric()
		{
			PackageShouldUpdateValueAndUnits("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=15CM|WID=20CM|HGT=30CM|VOL=1.524M3",
				"KG", 1, "KG", 1.200m,
				10, 15, 10, 20,
				10, 30, "CM", "CM",
				"M3", 0.021952m, "M3", 1.524m);
		}

		#endregion

		#region TestPackageShouldUpdateValueAndUnitsWhenImperialToImperial

		public void TestPackageShouldUpdateValueAndUnitsWhenImperialToImperial()
		{
			PackageShouldUpdateValueAndUnits("FAC=TWS|LOC=AUSYD|WGT=4.5LB|LEN=17.22IN|WID=22IN|HGT=30IN|VOL=1.342CY",
				"LB", 2, "LB", 4.5m,
				20, 17.22m, 15.2m, 22,
				13.333m, 30, "IN", "IN",
				"CY", 0.03553m, "CY", 1.342m);
		}

		#endregion

		#region TestPackageShouldUpdateWithMixedUnits

		public void TestPackageShouldUpdateWithMixedUnitsWhenWeightIsMetricVolumeIsMetric()
		{
			PackageShouldUpdateValueAndUnits("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10IN|WID=20IN|HGT=30IN|VOL=1.524M3",
				string.Empty, 0, "KG", 1.200m,
				0, 10, 0, 20,
				0, 30, string.Empty, "IN",
				string.Empty, 0, "M3", 1.524m);
		}

		public void TestPackageShouldUpdateWithMixedUnitsWhenWeightIsImperialVolumeIsImperial()
		{
			PackageShouldUpdateValueAndUnits("FAC=TWS|LOC=AUSYD|WGT=1.200LB|LEN=10CM|WID=20CM|HGT=30CM|VOL=1.524CY",
				string.Empty, 0, "LB", 1.200m,
				0, 10, 0, 20,
				0, 30, string.Empty, "CM",
				string.Empty, 0, "CY", 1.524m);
		}

		public void TestPackageShouldUpdateWithMixedUnitsWhenWeightIsImperialVolumeIsMetric()
		{
			PackageShouldUpdateValueAndUnits("FAC=TWS|LOC=AUSYD|WGT=1.200LB|LEN=10CM|WID=20CM|HGT=30CM|VOL=1.524M3",
				string.Empty, 0, "LB", 1.200m,
				0, 10, 0, 20,
				0, 30, string.Empty, "CM",
				string.Empty, 0, "M3", 1.524m);
		}

		public void TestPackageShouldUpdateWithMixedUnitsWhenWeightIsMetricVolumeIsImperial()
		{
			PackageShouldUpdateValueAndUnits("FAC=TWS|LOC=AUSYD|WGT=1.200KG|LEN=10IN|WID=20IN|HGT=30IN|VOL=1.524CY",
				string.Empty, 0, "KG", 1.200m,
				0, 10, 0, 20,
				0, 30, string.Empty, "IN",
				string.Empty, 0, "CY", 1.524m);
		}

		#endregion

		void PackageShouldUpdateValueAndUnits(string eventString,
			string weightUQ, decimal weightValue, string expectedWeightUQ, decimal expectedWeightValue,
			decimal lengthValue, decimal expectedLengthValue, decimal widthValue, decimal expectedWidthValue,
			decimal heightValue, decimal expectedHeightValue, string dimensionUQ, string expectedDimensionUQ,
			string volumeUQ, decimal volumeValue, string expectedVolumeUQ, decimal expectedVolumeValue)
		{
			var eventDeserializer = new XmlEventDeserializer();

			var originalPackage = Factory.New<PkgPackage>();
			originalPackage.KP_Weight = weightValue;
			originalPackage.KP_WeightUQ = weightUQ;

			originalPackage.KP_Width = widthValue;
			originalPackage.KP_Height = heightValue;
			originalPackage.KP_Length = lengthValue;
			originalPackage.KP_DimensionUQ = dimensionUQ;

			originalPackage.KP_Volume = volumeValue;
			originalPackage.KP_VolumeUQ = volumeUQ;

			using (PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedVolumeUQ))
			using (PackingRegistry.Instance.DimensionUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedDimensionUQ))
			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedWeightUQ))
			{
				var xmlEvent = (UniversalEvent)eventDeserializer.Parse(VolCamEventBuilder(eventString));
				VolCamScanData.PopulatePackageFromEvent(xmlEvent, originalPackage);

				AssertPackageIsUpdated(originalPackage, expectedWeightUQ, expectedWeightValue, expectedDimensionUQ, expectedLengthValue,
					expectedWidthValue, expectedHeightValue, expectedVolumeUQ, expectedVolumeValue);
			}
		}

		void AssertPackageIsUpdated(PkgPackage originalPackage, string expectedWeightUQ, decimal expectedWeightValue, string expectedDimensionUQ,
			decimal expectedLengthValue, decimal expectedWidthValue, decimal expectedHeightValue, string expectedVolumeUQ, decimal expectedVolumeValue)
		{
			AssertEquals("Package should update to Metric UoM - Weight", expectedWeightUQ, originalPackage.KP_WeightUQ);
			AssertEquals("Package should update to Metric value - Weight", expectedWeightValue, originalPackage.KP_Weight);

			AssertEquals("Package should maintain the same unit - Dimension", expectedDimensionUQ, originalPackage.KP_DimensionUQ);
			AssertEquals("Package should update to Metric value - Length", expectedLengthValue, originalPackage.KP_Length);
			AssertEquals("Package should update to Metric value - Width", expectedWidthValue, originalPackage.KP_Width);
			AssertEquals("Package should update to Metric value - Height", expectedHeightValue, originalPackage.KP_Height);

			AssertEquals("Package should update to Metric UoM - Volume", expectedVolumeUQ, originalPackage.KP_VolumeUQ);
			AssertEquals("Package should update to Metric value - Volume", expectedVolumeValue, originalPackage.KP_Volume);
		}

		#endregion

	}
}
