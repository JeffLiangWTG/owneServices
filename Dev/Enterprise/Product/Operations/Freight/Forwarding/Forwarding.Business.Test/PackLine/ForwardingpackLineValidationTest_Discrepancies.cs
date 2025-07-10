using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingpackLineValidationTest_Discrepancies : BusinessObjectValidationTestCase
	{
		UNDGSubstance Substance1, Substance2;
		OrgContact Contact;
		ForwardingPackLine PackingLine;
		ForwardingShipment Shipment;
		PkgPackageJob PackageJob;

		readonly string warningSubstring = "Discrepancies on the following properties are detected between the pack line and its packages:";

		string GetDiscrepancyMessage()
		{
			var warning = PackingLine.JL_PackLineIdInfo.Notifications.GetWarnings().FirstOrDefault(t => t.Message.StartsWith(warningSubstring));
			return warning?.Message ?? "";
		}

		string GetDiscrepancyList()
		{
			var message = GetDiscrepancyMessage();
			if (message.IsNullOrEmpty())
			{
				return "";
			}

			var found = message.Substring(warningSubstring.Length).Split(',');
			var list = new List<string>();
			foreach (var item in found)
			{
				var s = item.Trim();
				if (!s.IsNullOrEmpty())
				{
					list.Add(s);
				}
			}
			list.Sort();
			return string.Join(", ", list);
		}

		/// <summary>
		/// Checks that the discrepancies found exactly match those in the function parameter, in any order.
		/// </summary>
		void AssertDiscrepancies(params string[] discrepancies)
		{
			var matched = GetDiscrepancyList();
			if (discrepancies.Length == 0)
			{
				AssertEquals("No discrepancies", "", matched);
				return;
			}

			Array.Sort(discrepancies);
			var expected = string.Join(", ", discrepancies);

			AssertEquals("Discrepancies", expected, matched);
		}

		public void TestWarningsOnPackline_Discrepancies()
		{
			CreatePackingLine(2,
				new PackageTemplate
				{
					PackID = "PKG1"
				},
				new PackageTemplate
				{
					PackID = "PKG2"
				});

			FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies();

			PackingLine.JL_Height = 1;
			PackingLine.JL_Width = 1;
			PackingLine.JL_Length = 1;
			PackingLine.JL_UnitOfDimension = "Z";
			PackingLine.JL_RH_NKCommodityCode = "C1";
			PackingLine.JL_HarmonisedCode = "H1";
			PackingLine.JL_Description = "detailed";
			PackingLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
			PackingLine.JL_MarksAndNumbers = "number001";
			PackingLine.JL_PackageCount = 0;
			PackingLine.JL_ActualVolume = 1;
			PackingLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicInches;
			PackingLine.JL_ActualWeight = 1;
			PackingLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			PackingLine.JL_RequiresTemperatureControl = true;
			PackingLine.JL_RequiredTemperatureMinimum = 1;
			PackingLine.JL_RequiredTemperatureMaximum = 1;
			PackingLine.JL_RequiredTemperatureUnit = "T";
			PackingLine.JL_InspectionTypeCode = "VCK";
			PackingLine.JL_IsHighRisk = true;
			PackingLine.JL_AdditionalInspectionTypeCode = "XRY";

			PackingLine.UNDGs.DeleteAll();

			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies(
				"Commodity",
				"Goods Description",
				"Harmonized Code",
				"Height",
				"Length",
				"Dimension Unit",
				"Marks & Numbers",
				"Pack Count",
				"Pack Type",
				"Required Temperature Maximum",
				"Required Temperature Minimum",
				"Required Temperature Unit",
				"Requires Temperature Control",
				"Volume",
				"Weight",
				"Width",
				"Inspection",
				"Is High Risk",
				"Additional Inspection");
		}

		public void TestDiscrepancy_VolumeAndWeightConversions()
		{
			CreatePackingLine(2,
				new PackageTemplate
				{
					PackID = "PKG1",
					Volume = 4,
					VolumeUQ = "L",
					Weight = 6000,
					WeightUQ = "MG"
				},
				new PackageTemplate
				{
					PackID = "PKG2",
					Volume = 0.004m,
					VolumeUQ = "M3",
					Weight = 0.006m,
					WeightUQ = "KG"
				}
			);

			PackingLine.JL_ActualVolumeUQ = "M3";
			PackingLine.JL_ActualVolume = 0.008m;
			PackingLine.JL_ActualWeightUQ = "G";
			PackingLine.JL_ActualWeight = 12;

			FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies();

			PackingLine.JL_ActualVolume = 0.009m;
			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies("Volume");

			PackingLine.JL_ActualWeight = 13;
			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies("Volume", "Weight");
		}

		public void TestDiscrepancy_GoodsDescription()
		{
			CreatePackingLine(2,
				new PackageTemplate
				{
					PackID = "PKG1"
				},
				new PackageTemplate
				{
					PackID = "PKG2"
				});

			FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PackingLine.JL_Description = "Description\r\n123";
			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies("Goods Description");

			PackingLine.PkgPackageCollection[0].KP_GoodsDescription = "Description 123";
			PackingLine.PkgPackageCollection[1].KP_GoodsDescription = "Description 123";

			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies();

			PackingLine.PkgPackageCollection[0].KP_GoodsDescription = "Description 1234";
			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies("Goods Description");
		}

		public void TestPackCountSinglePackage()
		{
			CreatePackingLine(2,
				new PackageTemplate
				{
					PackID = "PKG1",
					Weight = 40,
					Volume = 40,
					Qty = 2
				});

			FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies();
		}

		public void TestPackCountSinglePackageWithDiscrepancy()
		{
			CreatePackingLine(1,
				new PackageTemplate
				{
					PackID = "PKG1",
					Weight = 40,
					Volume = 40,
					Qty = 2
				});

			FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies("Pack Count");
		}

		public void TestPackCountMultiplePackages()
		{
			CreatePackingLine(3,
				new PackageTemplate
				{
					PackID = "PKG1",
					Qty = 2
				},
				new PackageTemplate
				{
					PackID = "PKG2",
					Qty = 1
				});

			FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies();
		}

		public void TestPackCountMultiplePackagesWithDiscrepancy()
		{
			CreatePackingLine(2,
				new PackageTemplate
				{
					PackID = "PKG1",
					Qty = 2
				},
				new PackageTemplate
				{
					PackID = "PKG2",
					Qty = 1
				});

			FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PackingLine.Validation.ValidateAll();
			AssertDiscrepancies("Pack Count");
		}

		protected override void SetUp()
		{
			base.SetUp();

			var packageParent = Factory.New<DummyBusinessObject>();

			PackageJob = Factory.New<PkgPackageJob>();
			PackageJob.KJ_ParentID = packageParent.PK;
			PackageJob.KJ_ParentTableCode = packageParent.TablePrefix;

			Contact = Factory.NewWithValidTestData<OrgContact>();
			Contact.OC_ContactName = "contact";
			Contact.OC_Email = "123@test.com";
			Contact.OC_Phone = "123";
			Substance1 = Factory.New<UNDGSubstance>();
			Substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			Substance1.DG_Code = "0190";
			Substance2 = Factory.New<UNDGSubstance>();
			Substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			Substance2.DG_Code = "0191";

			Substance1.DG_LQMaxAmt = 50000; // for IsLimitedQuantity = true
			Substance1.DG_LQMaxAmtUQ = "L";
			Substance2.DG_LQMaxAmt = 50000;
			Substance2.DG_LQMaxAmtUQ = "L";
		}

		void CreatePackingLine(int packageCount, params PackageTemplate[] packageTemplates)
		{
			Shipment = CreateShipmentPacklineWithPackages(
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
				packageTemplates);
			AssertEquals("prerequisite: shipment has 1 packline", 1, Shipment.OuterPackLines.Count);
			PackingLine = Shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			AssertEquals($"prerequisite: {packageTemplates.Length} packages were created", packageTemplates.Length, PackingLine.PkgPackageCollection.Count);

			PackingLine.JL_PackageCount = packageCount;

			// sensible defaults can be overridden
			PackingLine.JL_Height = 20;
			PackingLine.JL_Width = 20;
			PackingLine.JL_Length = 20;
			PackingLine.JL_UnitOfDimension = "M";
			PackingLine.JL_RH_NKCommodityCode = PackingLine.PkgPackageCollection[0].KP_RH_NKCommodityCode;
			PackingLine.JL_HarmonisedCode = PackingLine.PkgPackageCollection[0].KP_HSCode;
			PackingLine.JL_Description = PackingLine.PkgPackageCollection[0].KP_GoodsDescription;
			PackingLine.JL_F3_NKPackType = PackingLine.PkgPackageCollection[0].KP_F3_NKPackType;
			PackingLine.JL_MarksAndNumbers = PackingLine.PkgPackageCollection[0].KP_MarksAndNumbers;
			PackingLine.JL_ActualVolumeUQ = PackingLine.PkgPackageCollection[0].KP_VolumeUQ;
			PackingLine.JL_ActualVolume = 40;
			PackingLine.JL_ActualWeightUQ = PackingLine.PkgPackageCollection[0].KP_WeightUQ;
			PackingLine.JL_ActualWeight = 40;
			PackingLine.JL_RequiresTemperatureControl = false;

			var packingLineUNDG = PackingLine.UNDGs.AddNew();
			SetDGDataItem(packingLineUNDG);
			packingLineUNDG.DI_DGWeight = 2;
			packingLineUNDG.DI_DGVolume = 2;
		}

		void SetDGDataItem(UNDGDataItem dataItem, UNDGSubstance substance = null)
		{
			dataItem.DI_DG = substance?.PK ?? Substance1.PK;
			dataItem.DI_DGFlashPoint = 5;
			dataItem.DI_IMOClass = "1";
			dataItem.DI_MPMarinePollutant = "1";
			dataItem.DI_TechnicalName = "1";
			dataItem.DI_UnitOfVolume = "L";
			dataItem.DI_UnitOfWeight = "KG";
			dataItem.DI_F3_NKPackType = "TST";
			dataItem.DI_DGWeight = 1;
			dataItem.DI_DGVolume = 1;
			dataItem.DI_OC_DGContact = Contact.PK;
			dataItem.DI_IsLimitedQuantity = true;
		}

		ForwardingShipment CreateShipmentPacklineWithPackages(ZString transitWarehouseStatus, IEnumerable<PackageTemplate> packageTemplates)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OriginTransitWarehouseStatus = transitWarehouseStatus;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			AddPkgPackageTemplates(packLine, packageTemplates);

			return shipment;
		}

		void AddPkgPackageTemplates(PackLine packLine, IEnumerable<PackageTemplate> packageTemplates)
		{
			foreach (var packageTemplate in packageTemplates)
			{
				var package = packLine.PkgPackageCollection.AddNew();
				package.KP_PackageID = packageTemplate.PackID;
				package.KP_KJ_ParentPackageJob = PackageJob.PK;
				package.KP_GoodsDescription = packageTemplate.GoodsDescription;
				package.KP_F3_NKPackType = packageTemplate.PackType;
				package.KP_PackageQty = packageTemplate.Qty;
				package.KP_Weight = packageTemplate.Weight;
				package.KP_WeightUQ = packageTemplate.WeightUQ;
				package.KP_Length = packageTemplate.Length;
				package.KP_Width = packageTemplate.Width;
				package.KP_Height = packageTemplate.Height;
				package.KP_DimensionUQ = packageTemplate.UnitOfDimension;
				package.KP_Volume = packageTemplate.Volume;
				package.KP_VolumeUQ = packageTemplate.VolumeUQ;

				package.KP_RequiredTemperatureMaximum = 5;
				package.KP_RequiredTemperatureMinimum = 5;
				package.KP_RequiresTemperatureControl = false;
				package.KP_RequiredTemperatureUnit = "Z";

				SetDGDataItem(package.UNDGs.AddNew());
			}
		}

		sealed class PackageTemplate
		{
			public ZString PackID { get; set; } = "PKG1";
			public ZString PackType { get; set; } = Core.Constants.PkgUnit.Carton;
			public ZString GoodsDescription { get; set; } = "books";
			public ZInt Qty { get; set; } = 1;
			public ZDecimal Weight { get; set; } = 20;
			public ZString WeightUQ { get; set; } = Core.Constants.Weight.Kilograms;
			public ZDecimal Volume { get; set; } = 20;
			public ZString VolumeUQ { get; set; } = Core.Constants.Volume.CubicMetres;
			public ZDecimal Length { get; set; } = 20;
			public ZDecimal Width { get; set; } = 20;
			public ZDecimal Height { get; set; } = 20;
			public ZString UnitOfDimension { get; set; } = "M";
		}
	}
}
