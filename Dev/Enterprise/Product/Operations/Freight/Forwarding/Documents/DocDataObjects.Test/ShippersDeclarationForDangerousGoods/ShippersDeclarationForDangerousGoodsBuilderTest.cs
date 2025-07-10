using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ShippersDeclarationForDangerousGoodsBuilderTest : TestCaseWithFactory
	{
		#region TestPopulateAddresses

		public void TestPopulateAddresses()
		{
			var shipment = CreateShipment();
			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			AssertionHelper.AssertAddressData(shipment.Consignor, decl.Shipper);
			AssertionHelper.AssertAddressData(shipment.Consignee, decl.Consignee);
			AssertionHelper.AssertAddressData(GlbBranch.CurrentBranch.OrgProxy, decl.Company);
		}

		#endregion

		#region TestPopulateOtherInfo

		public void TestPopulateOtherInfo()
		{
			var shipment = CreateShipment();
			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			AssertEquals(nameof(ShippersDeclarationForDangerousGoodsDetail.ShippersReferenceNumber),
				"HOUSEBILL001", decl.ShippersReferenceNumber);

			AssertEquals(nameof(ShippersDeclarationForDangerousGoodsDetail.AirWaybillNumber),
				"0810000001", decl.AirWaybillNumber);

			AssertEquals(nameof(ShippersDeclarationForDangerousGoodsDetail.Signatory),
				GlbStaff.CurrentUser.GS_FullName, decl.Signatory);

			AssertEquals(nameof(ShippersDeclarationForDangerousGoodsDetail.PlaceOfSignature),
				GlbBranch.CurrentBranch.GB_City.Left(17), decl.PlaceOfSignature);

			AssertEquals(nameof(ShippersDeclarationForDangerousGoodsDetail.DateOfSignature),
				false, decl.DateOfSignature.IsEmpty);

			AssertEquals(nameof(ShippersDeclarationForDangerousGoodsDetail.EmergencyContact.FullName),
				"Pumpernickel", decl.EmergencyContact.FullName);

			AssertEquals(nameof(ShippersDeclarationForDangerousGoodsDetail.EmergencyContact.Phone),
				"8000 1234", decl.EmergencyContact.Phone);
		}

		#endregion

		#region TestPopulateLocations

		public void TestPopulateLocations()
		{
			var shipment = CreateShipment();
			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			AssertEquals(nameof(ShippersDeclarationForDangerousGoodsDetail.AirportOfDeparture),
				"AUSYD - Sydney", decl.AirportOfDeparture.ToString());

			AssertEquals(nameof(ShippersDeclarationForDangerousGoodsDetail.AirportOfDestination),
				"SGSIN - Singapore", decl.AirportOfDestination.ToString());
		}

		#endregion

		#region TestNatureAndQuantityOfDangerousGoodsLines

		public void TestNatureAndQuantityOfDangerousGoodsLines()
		{
			var shipment = CreateShipment();
			AssertEquals("prerequisite: shipment has 1 packingline", 1, shipment.OuterPackLines.Count);

			var undg = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single().UNDGs.AddNew();
			undg.DI_DG = CreateUNDGSubstance("1410").PK;
			undg.DI_DGWeight = 1;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var packingLine = shipment.OuterPackLines.AddNew();

			var undg2 = packingLine.UNDGs.AddNew();
			undg2.DI_DG = CreateUNDGSubstance("2941").PK;
			undg2.DI_DGWeight = 1;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg3 = packingLine.UNDGs.AddNew();
			undg3.DI_DG = CreateUNDGSubstance("3164a").PK;
			undg3.DI_DGWeight = 1;
			undg3.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg4 = packingLine.UNDGs.AddNew();
			undg4.DI_DG = CreateUNDGSubstance("3373a").PK;
			undg4.DI_DGWeight = 1;
			undg4.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg5 = packingLine.UNDGs.AddNew();
			undg5.DI_DG = CreateUNDGSubstance("1845a").PK;
			undg5.DI_DGWeight = 1;
			undg5.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();

			AssertContainsExactElementsInAnyOrder("nature and quantity of goods",
				new[]
				{
					"1845",
					"0143",
					"1410",
					"2941"
				},
				new[]
				{
					page.NatureAndQuantity1?.UNCode,
					page.NatureAndQuantity2?.UNCode,
					page.NatureAndQuantity3?.UNCode,
					page.NatureAndQuantity4?.UNCode,
					page.NatureAndQuantity5?.UNCode,
					page.NatureAndQuantity6?.UNCode,
					page.NatureAndQuantity7?.UNCode,
					page.NatureAndQuantity8?.UNCode,
					page.NatureAndQuantity9?.UNCode,
					page.NatureAndQuantity10?.UNCode
				}.Where(c => c.HasValue && !c.Value.IsEmpty).Select(c => c.ToString()));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_IncludePackInformation()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			AssertEquals("Pre-requisite: only 1 dg item.", 1, packline.UNDGs.Count);

			var undg = packline.UNDGs.AddNew();
			undg.DI_DG = CreateUNDGSubstance("2941").PK;
			undg.DI_DGWeight = 1;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();
			Assert("Include individual pack information irrespective of number of lines.", decl.NatureAndQuantity1.IncludePackInformation);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_ItemCountAndType()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.JL_PackageCount = 10;
			packline.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var dgItem = packline.UNDGs.First();
			dgItem.DI_PackageCount = 5;
			dgItem.DI_F3_NKPackType = Core.Constants.PkgUnit.Bottle;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();
			AssertEquals("Package count should come from DG item", 5, decl.NatureAndQuantity1.PackCount);
			AssertEquals("Package type should come from DG item", Core.Constants.PkgUnit.Bottle, decl.NatureAndQuantity1.PackageType.Code);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_QuantityRoundingScale()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.UNDGs.Single().DI_IsLimitedQuantity = true;
			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance1.DG_UNNO = "A01";
			substance1.DG_LQMaxAmtType = LimitedQuantityTypes.GLMCode;
			substance1.DG_LQMaxAmt = 30;
			substance1.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance2.DG_UNNO = "A02";
			substance2.DG_LQMaxAmtType = LimitedQuantityTypes.GLMCode;
			substance2.DG_LQMaxAmt = 30;
			substance2.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			var substance3 = Factory.New<UNDGSubstance>();
			substance3.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance3.DG_UNNO = "A03";
			substance3.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			substance3.DG_LQMaxAmt = 50;
			substance3.DG_LQMaxAmtUQ = Core.Constants.Volume.Litre;

			var substance4 = Factory.New<UNDGSubstance>();
			substance4.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance4.DG_UNNO = "A04";
			substance4.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			substance4.DG_LQMaxAmt = 50;
			substance4.DG_LQMaxAmtUQ = Core.Constants.Volume.Litre;

			var undg1 = packline.UNDGs.AddNew();
			undg1.DI_PackageCount = 5;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg1.DI_DG = substance1.PK;
			undg1.DI_DGWeight = 5100.001;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Grams;

			var undg2 = packline.UNDGs.AddNew();
			undg2.DI_PackageCount = 1;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg2.DI_DG = substance2.PK;
			undg2.DI_DGWeight = 5.001;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg3 = packline.UNDGs.AddNew();
			undg3.DI_PackageCount = 1;
			undg3.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg3.DI_DG = substance3.PK;
			undg3.DI_DGVolume = 6100.001;
			undg3.DI_UnitOfVolume = Core.Constants.Volume.CubicCentimeters;

			var undg4 = packline.UNDGs.AddNew();
			undg4.DI_PackageCount = 1;
			undg4.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg4.DI_DG = substance4.PK;
			undg4.DI_DGVolume = 6.032;
			undg4.DI_UnitOfVolume = Core.Constants.Volume.Litre;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();
			AssertArrayEqualsByElements("generated lines",
				new[]
				{
						"0143b 1 Pallet x 1 L ",
						"A01 5 Pallet x 1.020 KG G",
						"A02 1 Pallet x 5.001 KG G",
						"A03 1 Pallet x 6.100 L ",
						"A04 1 Pallet x 6.032 L ",
						"All Packed in One (Pallet(s) x 1).",
						"Q = 0.6",
						"Total Gross Weight: 10.101 KG."
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_QValue()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.UNDGs.Single().DI_IsLimitedQuantity = true;

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance1.DG_UNNO = "A01";
			substance1.DG_LQMaxAmtType = LimitedQuantityTypes.GLMCode;
			substance1.DG_LQMaxAmt = 30;
			substance1.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance2.DG_UNNO = "A02";
			substance2.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			substance2.DG_LQMaxAmt = 40;
			substance2.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			var substance3 = Factory.New<UNDGSubstance>();
			substance3.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance3.DG_UNNO = "A03";
			substance3.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			substance3.DG_LQMaxAmt = 50;
			substance3.DG_LQMaxAmtUQ = Core.Constants.Volume.Litre;

			var substance4 = Factory.New<UNDGSubstance>();
			substance4.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance4.DG_UNNO = "A04";
			substance4.DG_LQMaxAmtType = LimitedQuantityTypes.FOBCode;
			substance4.DG_LQMaxAmt = 50;
			substance4.DG_LQMaxAmtUQ = Core.Constants.Volume.Litre;

			var undg1 = packline.UNDGs.AddNew();
			undg1.DI_PackageCount = 5;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg1.DI_DG = substance1.PK;
			undg1.DI_DGWeight = 5;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg2 = packline.UNDGs.AddNew();
			undg2.DI_PackageCount = 5;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg2.DI_DG = substance2.PK;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg3 = packline.UNDGs.AddNew();
			undg3.DI_PackageCount = 1;
			undg3.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;
			undg3.DI_DG = substance3.PK;
			undg3.DI_DGVolume = 5;
			undg3.DI_UnitOfVolume = Core.Constants.Volume.Litre;

			var undg4 = packline.UNDGs.AddNew();
			undg4.DI_PackageCount = 1;
			undg4.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;
			undg4.DI_DG = substance4.PK;
			undg4.DI_DGVolume = 10;
			undg4.DI_UnitOfVolume = Core.Constants.Volume.Litre;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();
			AssertArrayEqualsByElements("generated lines",
				new[]
				{
						"0143b 1 Pallet x 1 L ",
						"A01 5 Pallet x 1 KG G",
						"A02 5 Pallet x 2 KG ",
						"A03 1 Box x 5 L ",
						"A04 1 Box x 10 L ",
						"All Packed in One (Pallet(s) x 1).",
						"Q = 0.6",
						"Total Gross Weight: 5 KG."
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		public void TestDGDispalyInMultilinesOnShipperDeclaration()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.UNDGs.Single().DI_IsLimitedQuantity = true;

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance1.DG_UNNO = "A01";
			substance1.DG_LQMaxAmtType = LimitedQuantityTypes.GLMCode;
			substance1.DG_LQMaxAmt = 30;
			substance1.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance2.DG_UNNO = "A02";
			substance2.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			substance2.DG_LQMaxAmt = 40;
			substance2.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			var undg1 = packline.UNDGs.AddNew();
			undg1.DI_PackageCount = 5;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg1.DI_DG = substance1.PK;
			undg1.DI_DGWeight = 5;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg2 = packline.UNDGs.AddNew();
			undg2.DI_PackageCount = 5;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg2.DI_DG = substance2.PK;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();
			AssertEquals("DG dispaly in multi lines in shipper declaration form", FormatNatureAndQuantityLines(page).Length, 6);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_QValue_NoGrossWeightWhenNLMTypeOnly()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.UNDGs.Single().DI_IsLimitedQuantity = true;

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance1.DG_UNNO = "A01";
			substance1.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			substance1.DG_LQMaxAmt = 30;
			substance1.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance2.DG_UNNO = "A02";
			substance2.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			substance2.DG_LQMaxAmt = 40;
			substance2.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			var substance3 = Factory.New<UNDGSubstance>();
			substance3.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance3.DG_UNNO = "A03";
			substance3.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			substance3.DG_LQMaxAmt = 50;
			substance3.DG_LQMaxAmtUQ = Core.Constants.Volume.Litre;

			var undg1 = packline.UNDGs.AddNew();
			undg1.DI_PackageCount = 5;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg1.DI_DG = substance1.PK;
			undg1.DI_DGWeight = 5;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg2 = packline.UNDGs.AddNew();
			undg2.DI_PackageCount = 5;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg2.DI_DG = substance2.PK;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg3 = packline.UNDGs.AddNew();
			undg3.DI_PackageCount = 1;
			undg3.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;
			undg3.DI_DG = substance3.PK;
			undg3.DI_DGVolume = 5;
			undg3.DI_UnitOfVolume = Core.Constants.Volume.Litre;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();
			AssertArrayEqualsByElements("generated lines",
				new[]
				{
						"0143b 1 Pallet x 1 L ",
						"A01 5 Pallet x 1 KG ",
						"A02 5 Pallet x 2 KG ",
						"A03 1 Box x 5 L ",
						"All Packed in One (Pallet(s) x 1).",
						"Q = 0.6"
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_DryIceAsOnlyItem()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var dryIceSubstance = Factory.New<UNDGSubstance>();
			dryIceSubstance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			dryIceSubstance.DG_UNNO = "1845";

			var dgItem = packline.UNDGs.Single();
			dgItem.DI_PackageCount = 5;
			dgItem.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			dgItem.DI_DG = dryIceSubstance.PK;
			dgItem.DI_DGWeight = 10;
			dgItem.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("No DG declaration should be generated when only dry ice.", 0, decl.Pages.Count);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_DryIcePackedWithOtherDGSubs_Lessthan30KG()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var dryIceSubstance = Factory.New<UNDGSubstance>();
			dryIceSubstance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			dryIceSubstance.DG_UNNO = "1845";

			var undg = packline.UNDGs.AddNew();
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg.DI_DG = dryIceSubstance.PK;
			undg.DI_DGWeight = 10;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();
			AssertArrayEqualsByElements("Dry ice should be included when less than 30kg (considered it is used to wrapped other DG item)",
				new[]
				{
						"0143b 1 Pallet x 1 L ",
						"1845 5 Pallet x 2 KG ",
						"All Packed in One (Pallet(s) x 1)."
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_DryIceWrappedOtherDGSubs_Largerthan30KG()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var dryIceSubstance = Factory.New<UNDGSubstance>();
			dryIceSubstance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			dryIceSubstance.DG_UNNO = "1845";

			var undg = packline.UNDGs.AddNew();
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg.DI_DG = dryIceSubstance.PK;
			undg.DI_DGWeight = 40;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();
			AssertArrayEqualsByElements("Dry ice should not be included when over 30 kg (considered it is individual element)", new[]
			{
				"0143b 1 Pallet x 1 L ",
			}, FormatNatureAndQuantityLines(decl));
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_Quantity()
		{
			var shipment = CreateShipment();
			var departureConsol = shipment.DepartureConsol;
			var departureTransport = departureConsol.MostInterestingTransportForBinding.Cast<Freight.Business.Transport>().FirstOrDefault();
			departureTransport.JW_IsCargoOnly = false;

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_UNNO = "A01";
			substance1.DG_Code = "A01";
			substance1.DG_CargoMaxAmtUQ = "L";
			substance1.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance1.DG_LQMaxAmtUQ = "KG";
			substance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_UNNO = "A02";
			substance2.DG_Code = "A02";
			substance2.DG_CargoMaxAmtUQ = "ml";
			substance2.DG_LQ2OrPaxMaxAmtUQ = "mg";
			substance2.DG_LQMaxAmtUQ = "g";
			substance2.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			var substance3 = Factory.New<UNDGSubstance>();
			substance3.DG_UNNO = "A03";
			substance3.DG_Code = "A03";
			substance3.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			var substance4 = Factory.New<UNDGSubstance>();
			substance4.DG_UNNO = "A04";
			substance4.DG_Code = "A04";
			substance4.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var undg = packline.UNDGs[0];
			undg.DI_DG = substance1.PK;
			undg.DI_DGWeight = 1.1m;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg.DI_DGVolume = 2.2m;
			undg.DI_UnitOfVolume = Core.Constants.Volume.Litre;
			undg.DI_PackageCount = 1;

			var undg2 = packline.UNDGs.AddNew();
			undg2.DI_DG = substance2.PK;
			undg2.DI_DGWeight = 300m;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Grams;
			undg2.DI_DGVolume = 0.5;
			undg2.DI_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			undg2.DI_PackageCount = 1;

			var undg3 = packline.UNDGs.AddNew();
			undg3.DI_DG = substance3.PK;
			undg3.DI_DGWeight = 300m;
			undg3.DI_UnitOfWeight = Core.Constants.Weight.Grams;
			undg3.DI_PackageCount = 1;

			var undg4 = packline.UNDGs.AddNew();
			undg4.DI_DG = substance4.PK;
			undg4.DI_DGVolume = 0.5;
			undg4.DI_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			undg4.DI_PackageCount = 1;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			CombineAssertions(() =>
			{
				AssertEquals("NatureAndQuantity1.Quantity.Value", 1.1m, decl.NatureAndQuantity1.Quantity.Value);
				AssertEquals("NatureAndQuantity1.Quantity.Unit", "KG", decl.NatureAndQuantity1.Quantity.Unit.Code);

				AssertEquals("NatureAndQuantity2.Quantity.Value", 0.3m, decl.NatureAndQuantity2.Quantity.Value);
				AssertEquals("NatureAndQuantity2.Quantity.Unit", "KG", decl.NatureAndQuantity2.Quantity.Unit.Code);

				AssertEquals("NatureAndQuantity3.Quantity.Value", 0.3m, decl.NatureAndQuantity3.Quantity.Value);
				AssertEquals("NatureAndQuantity3.Quantity.Unit", "KG", decl.NatureAndQuantity3.Quantity.Unit.Code);

				AssertEquals("NatureAndQuantity4.Quantity.Value", 500m, decl.NatureAndQuantity4.Quantity.Value);
				AssertEquals("NatureAndQuantity4.Quantity.Unit", "L", decl.NatureAndQuantity4.Quantity.Unit.Code);
			});

			departureTransport.JW_IsCargoOnly = true;

			decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			CombineAssertions(() =>
			{
				AssertEquals("NatureAndQuantity1.Quantity.Value", 2.2m, decl.NatureAndQuantity1.Quantity.Value);
				AssertEquals("NatureAndQuantity1.Quantity.Unit", "L", decl.NatureAndQuantity1.Quantity.Unit.Code);

				AssertEquals("NatureAndQuantity2.Quantity.Value", 500m, decl.NatureAndQuantity2.Quantity.Value);
				AssertEquals("NatureAndQuantity2.Quantity.Unit", "L", decl.NatureAndQuantity2.Quantity.Unit.Code);

				AssertEquals("NatureAndQuantity3.Quantity.Value", 0.3m, decl.NatureAndQuantity3.Quantity.Value);
				AssertEquals("NatureAndQuantity3.Quantity.Unit", "KG", decl.NatureAndQuantity3.Quantity.Unit.Code);

				AssertEquals("NatureAndQuantity4.Quantity.Value", 500m, decl.NatureAndQuantity4.Quantity.Value);
				AssertEquals("NatureAndQuantity4.Quantity.Unit", "L", decl.NatureAndQuantity4.Quantity.Unit.Code);
			});

			undg.DI_IsLimitedQuantity = true;
			undg2.DI_IsLimitedQuantity = true;
			undg3.DI_IsLimitedQuantity = true;
			undg4.DI_IsLimitedQuantity = true;

			decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			CombineAssertions(() =>
			{
				AssertEquals("NatureAndQuantity1.Quantity.Value", 1.1m, decl.NatureAndQuantity1.Quantity.Value);
				AssertEquals("NatureAndQuantity1.Quantity.Unit", "KG", decl.NatureAndQuantity1.Quantity.Unit.Code);

				AssertEquals("NatureAndQuantity2.Quantity.Value", 0.3m, decl.NatureAndQuantity2.Quantity.Value);
				AssertEquals("NatureAndQuantity2.Quantity.Unit", "KG", decl.NatureAndQuantity2.Quantity.Unit.Code);

				AssertEquals("NatureAndQuantity3.Quantity.Value", 0.3m, decl.NatureAndQuantity3.Quantity.Value);
				AssertEquals("NatureAndQuantity3.Quantity.Unit", "KG", decl.NatureAndQuantity3.Quantity.Unit.Code);

				AssertEquals("NatureAndQuantity4.Quantity.Value", 500m, decl.NatureAndQuantity4.Quantity.Value);
				AssertEquals("NatureAndQuantity4.Quantity.Unit", "L", decl.NatureAndQuantity4.Quantity.Unit.Code);
			});
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_UNCodePrefix()
		{
			var shipment = CreateShipment();

			var undgSubstance8000Air = Factory.New<UNDGSubstance>();
			undgSubstance8000Air.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			undgSubstance8000Air.DG_UNNO = "8000";
			undgSubstance8000Air.DG_Mode = Constants.TransportModes.Air;

			var undgSubstance6969Air = Factory.New<UNDGSubstance>();
			undgSubstance6969Air.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			undgSubstance6969Air.DG_UNNO = "6969";
			undgSubstance6969Air.DG_Mode = Constants.TransportModes.Air;

			var undgSubstance6969Sea = Factory.New<UNDGSubstance>();
			undgSubstance6969Sea.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			undgSubstance6969Sea.DG_UNNO = "6969";
			undgSubstance6969Sea.DG_Code = "6969a";
			undgSubstance6969Sea.DG_Variant = "a";
			undgSubstance6969Sea.DG_Mode = Constants.TransportModes.Sea;

			var packLine = shipment.OuterPackLines[0];

			var undg8000Air = packLine.UNDGs[0];
			undg8000Air.DI_DG = undgSubstance8000Air.PK;
			undg8000Air.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance8000Air);

			var undg6969Air = packLine.UNDGs.AddNew();
			undg6969Air.DI_DG = undgSubstance6969Air.PK;
			undg6969Air.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance6969Air);

			var undg6969Sea = packLine.UNDGs.AddNew();
			undg6969Sea.DI_DG = undgSubstance6969Sea.PK;
			undg6969Sea.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance6969Sea);

			var declaration = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();
			var natureAndQuantityLines = new[]
			{
				declaration.NatureAndQuantity1,
				declaration.NatureAndQuantity2,
				declaration.NatureAndQuantity3
			};

			AssertCollectionContains(
				"When UNNO = 8000 and Mode = Air, prefix should be ID.",
				natureAndQuantityLines,
				line => line.UNCode.Equals("8000") && line.Variant.IsEmpty && line.UNCodePrefix.Equals("ID")
			);

			AssertCollectionContains(
				"When UNNO = 6969 and Mode = Air, prefix should be UN.",
				natureAndQuantityLines,
				line => line.UNCode.Equals("6969") && line.Variant.IsEmpty && line.UNCodePrefix.Equals("UN")
			);

			AssertCollectionContains(
				"When UNNO != 8000 and Mode != Air, prefix should be UN.",
				natureAndQuantityLines,
				line => line.UNCode.Equals("6969") && line.Variant.Equals("a") && line.UNCodePrefix.Equals("UN")
			);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_Quantity_WithSubRisks()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var dryIceSubstance = Factory.New<UNDGSubstance>();
			dryIceSubstance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			dryIceSubstance.DG_UNNO = "1845";
			dryIceSubstance.DG_SubLabel1 = "4";
			dryIceSubstance.DG_Class = "8";

			var dryIceSubstance2 = Factory.New<UNDGSubstance>();
			dryIceSubstance2.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			dryIceSubstance2.DG_UNNO = "1843";
			dryIceSubstance2.DG_SubLabel1 = "4";
			dryIceSubstance2.DG_SubLabel2 = "2";
			dryIceSubstance2.DG_Class = "6.1";

			var undg = packline.UNDGs.AddNew();
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg.DI_DG = dryIceSubstance.PK;
			undg.DI_DGWeight = 10;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg2 = packline.UNDGs.AddNew();
			undg2.DI_PackageCount = 5;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg2.DI_DG = dryIceSubstance2.PK;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();

			var formatNatureAndQuantityLine = page
				.AsEnumerable()
				.Where(x => x.LineType == NatureAndQuantityOfDangerousGoodsLineType.Detail)
				.Select(line => $"{line.UNCode}{line.Variant} {line.PackCount} {line.PackageType.Description} x {line.Quantity?.Value} {line.Quantity?.Unit.Code} {line.Class}")
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.ToArray();

			AssertArrayEqualsByElements("SubRisks must be included",
				new[]
				{
						"0143b 1 Pallet x 1 L ",
						"1843 5 Pallet x 2 KG 6.1 (4,2)",
						"1845 5 Pallet x 2 KG 8 (4)"
				},
				formatNatureAndQuantityLine);

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		#endregion

		#region TestPopulatePages

		public void TestPopulatePages()
		{
			var shipment = CreateShipment();

			for (var i = 0; i < 22; i++)
			{
				var packingLine = shipment.OuterPackLines.AddNew();

				var undg = packingLine.UNDGs.AddNew();
				undg.DI_DG = CreateUNDGSubstance("1410").PK;
				undg.DI_DGWeight = 1;
				undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			}

			var declarations = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("created 3 pages", 3, declarations.Pages.Count);
			AssertLinesHaveUniqueIdentifiers(declarations);
		}

		#endregion

		#region TestNoPagesForNonApplicableDangerousGoods

		public void TestNoPagesForNonApplicableDangerousGoods()
		{
			var shipment = CreateShipment();

			var packingLine = (ForwardingPackLine)shipment.OuterPackLines.Single();
			var undg = packingLine.UNDGs.Single();
			undg.DI_DG = CreateUNDGSubstance("3090").PK;
			undg.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionII;

			var declarations = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("created no pages", 0, declarations.Pages.Count);
		}

		#endregion

		#region TestQuantityIndicatorForApplicableDangerousGoods

		public void TestQuantityIndicatorForApplicableDangerousGoods()
		{
			var shipment = CreateShipment();
			var departureTransport = shipment.DepartureConsol.MostInterestingTransportForBinding.Cast<Freight.Business.Transport>().FirstOrDefault();
			departureTransport.JW_IsCargoOnly = false;

			var packingLine = (ForwardingPackLine)shipment.OuterPackLines.Single();
			var undg = packingLine.UNDGs.Single();
			undg.DI_DG = CreateUNDGSubstance("2941").PK;
			undg.DI_DGWeight = 1;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg2 = packingLine.UNDGs.AddNew();
			undg2.DI_DG = CreateUNDGSubstance("1941").PK;
			undg2.Substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode;
			undg2.DI_DGWeight = 1;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg3 = packingLine.UNDGs.AddNew();
			undg3.DI_DG = CreateUNDGSubstance("3082").PK;
			undg3.Substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode;
			undg3.DI_DGWeight = 1;
			undg3.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undg4 = packingLine.UNDGs.AddNew();
			undg4.DI_DG = CreateUNDGSubstance("0143b").PK;
			undg4.DI_DGWeight = 1;
			undg4.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var declarations = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var decl = declarations.Pages.Single();

			AssertContainsExactElementsInAnyOrder("quantity indicators",
				new[]
				{
					"0143",
					"1941 G",
					"2941",
					"3082 G"
				},
				new[]
				{
					decl.NatureAndQuantity1,
					decl.NatureAndQuantity2,
					decl.NatureAndQuantity3,
					decl.NatureAndQuantity4
				}.Select(n => $"{n.UNCode} {n.QuantityIndicator}".Trim()));

			AssertLinesHaveUniqueIdentifiers(declarations);
		}

		#endregion

		#region TestHasOverpack

		public void TestHasOverpack()
		{
			var shipment = CreateShipment();

			var packingLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var undg1 = packingLine.UNDGs.Single();
			undg1.DI_HasOverpack = false;
			undg1.DI_PackageCount = 1;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg2 = packingLine.UNDGs.AddNew();
			undg2.DI_DG = CreateUNDGSubstance("2250").PK;
			undg2.DI_DGVolume = 1;
			undg2.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg2.DI_HasOverpack = true;
			undg2.DI_PackageCount = 1;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Cylinder;

			var undg3 = packingLine.UNDGs.AddNew();
			undg3.DI_DG = CreateUNDGSubstance("1230").PK;
			undg3.DI_DGVolume = 1;
			undg3.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg3.DI_HasOverpack = true;
			undg3.DI_PackageCount = 1;
			undg3.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg4 = packingLine.UNDGs.AddNew();
			undg4.DI_DG = CreateUNDGSubstance("3333").PK;
			undg4.DI_DGVolume = 1;
			undg4.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg4.DI_HasOverpack = false;
			undg4.DI_PackageCount = 2;
			undg4.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();

			AssertArrayEqualsByElements("generated lines",
				new[]
				{
					"1230 1 Pallet x 1 L ",
					"2250 1 Cylinder x 1 L ",
					"Overpack Used",
					"0143b 1 Pallet x 1 L ",
					"3333 2 Pallet x 0.5 L "
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		#endregion

		#region TestOverpack

		public void TestOverpack()
		{
			var shipment = CreateShipment();

			var packingLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var undgSubstance2250 = CreateUNDGSubstance("2250");

			var undg1 = packingLine.UNDGs.Single();
			undg1.DI_DG = undgSubstance2250.PK;
			undg1.DI_DGWeight = 1;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg1.DI_HasOverpack = false;
			undg1.DI_PackageCount = 1;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg2 = packingLine.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2250.PK;
			undg2.DI_DGWeight = 1;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg2.DI_HasOverpack = true;
			undg2.DI_OverpackID = "A1";
			undg2.DI_PackageCount = 1;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg3 = packingLine.UNDGs.AddNew();
			undg3.DI_DG = undgSubstance2250.PK;
			undg3.DI_DGWeight = 1;
			undg3.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg3.DI_HasOverpack = true;
			undg3.DI_OverpackID = "A2";
			undg3.DI_PackageCount = 1;
			undg3.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg4 = packingLine.UNDGs.AddNew();
			undg4.DI_DG = CreateUNDGSubstance("3333").PK;
			undg4.DI_DGWeight = 1;
			undg4.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg4.DI_HasOverpack = false;
			undg4.DI_PackageCount = 1;
			undg4.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();

			AssertArrayEqualsByElements("generated lines",
				new[]
				{
					"2250 1 Pallet x 1 KG ",
					"Overpack used x 1",
					"Overpack ID# A1",
					"Total quantity per overpack 1 KG",
					"2250 1 Pallet x 1 KG ",
					"Overpack used x 1",
					"Overpack ID# A2",
					"Total quantity per overpack 1 KG",
					"2250 1 Pallet x 1 KG ",
					"3333 1 Box x 1 KG "
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		public void TestPacklineOrder_PacklineWithOverPackFirst()
		{
			var shipment = CreateShipment();

			var packingLine1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var undgSubstance1005 = CreateUNDGSubstance("1005");

			var undg = packingLine1.UNDGs.Single();
			undg.DI_DG = undgSubstance1005.PK;
			undg.DI_DGWeight = 1;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg.DI_HasOverpack = false;
			undg.DI_PackageCount = 1;
			undg.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var packingLine2 = shipment.OuterPackLines.AddNew();

			var undgSubstance2250 = CreateUNDGSubstance("2250");

			var undg1 = packingLine2.UNDGs.AddNew();
			undg1.DI_DG = undgSubstance2250.PK;
			undg1.DI_PackageCount = 200;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;
			undg1.DI_DGWeight = 40;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg1.DI_HasOverpack = true;
			undg1.DI_OverpackID = "AA44";

			var undg2 = packingLine2.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2250.PK;
			undg2.DI_PackageCount = 100;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg2.DI_HasOverpack = true;
			undg2.DI_OverpackID = "AA44";

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();

			AssertArrayEqualsByElements("generated lines",
				new[]
				{
					"2250 200 Box x 0.2 KG ",
					"2250 100 Box x 0.1 KG ",
					"Overpack used x 1",
					"Overpack ID# AA44",
					"Total quantity per overpack 50 KG",
					"1005 1 Pallet x 1 KG "
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		#endregion

		#region TestOverpackByVolume

		public void TestOverpackByVolume()
		{
			var shipment = CreateShipment();

			var packingLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var undgSubstance2250 = CreateUNDGSubstance("2250");
			undgSubstance2250.DG_CargoMaxAmt = 50;
			undgSubstance2250.DG_CargoMaxAmtUQ = "L";

			var undg1 = packingLine.UNDGs.Single();
			undg1.DI_DG = undgSubstance2250.PK;
			undg1.DI_DGVolume = 2;
			undg1.DI_UnitOfVolume = Core.Constants.Volume.Litre;
			undg1.DI_HasOverpack = true;
			undg1.DI_OverpackID = "A1";
			undg1.DI_PackageCount = 1;
			undg1.DI_F3_NKPackType = "BOT";

			var undg2 = packingLine.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2250.PK;
			undg2.DI_DGVolume = 2;
			undg2.DI_UnitOfVolume = Core.Constants.Volume.Litre;
			undg2.DI_HasOverpack = true;
			undg2.DI_OverpackID = "A1";
			undg2.DI_PackageCount = 1;
			undg2.DI_F3_NKPackType = "BOT";

			var undg3 = packingLine.UNDGs.AddNew();
			undg3.DI_DG = undgSubstance2250.PK;
			undg3.DI_DGVolume = 2;
			undg3.DI_UnitOfVolume = Core.Constants.Volume.Litre;
			undg3.DI_HasOverpack = true;
			undg3.DI_OverpackID = "A2";
			undg3.DI_PackageCount = 1;
			undg3.DI_F3_NKPackType = "BOT";

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();

			AssertArrayEqualsByElements("generated lines",
				new[]
				{
					"2250 1 Bottle x 2 L ",
					"2250 1 Bottle x 2 L ",
					"Overpack used x 1",
					"Overpack ID# A1",
					"Total quantity per overpack 4 L",
					"2250 1 Bottle x 2 L ",
					"Overpack used x 1",
					"Overpack ID# A2",
					"Total quantity per overpack 2 L"
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		#endregion

		#region TestOverpackGrouping

		public void TestOverpackGrouping()
		{
			var shipment = CreateShipment();

			var packingLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var undgSubstance2250 = CreateUNDGSubstance("2250");

			var undg1 = packingLine.UNDGs.Single();
			undg1.DI_DG = undgSubstance2250.PK;
			undg1.DI_PackageCount = 200;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;
			undg1.DI_DGWeight = 40;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg1.DI_HasOverpack = true;
			undg1.DI_OverpackID = "AA44";

			var undg2 = packingLine.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2250.PK;
			undg2.DI_PackageCount = 100;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg2.DI_HasOverpack = true;
			undg2.DI_OverpackID = "AA44";

			var undg3 = packingLine.UNDGs.AddNew();
			undg3.DI_DG = undgSubstance2250.PK;
			undg3.DI_PackageCount = 100;
			undg3.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;
			undg3.DI_DGWeight = 30;
			undg3.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg3.DI_HasOverpack = true;
			undg3.DI_OverpackID = "AA60";

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();

			// UNDG packlines are displayed according to average quantity. Grouping by overpack quantity to be implemented on WI00485220.
			AssertArrayEqualsByElements("generated lines",
				new[]
				{
					"2250 200 Box x 0.2 KG ",
					"2250 100 Box x 0.1 KG ",
					"Overpack used x 1",
					"Overpack ID# AA44",
					"Total quantity per overpack 50 KG",
					"2250 100 Box x 0.3 KG ",
					"Overpack used x 1",
					"Overpack ID# AA60",
					"Total quantity per overpack 30 KG"
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		#endregion

		#region TestExceptedQuantity

		public void TestExceptedQuantity()
		{
			var shipment = CreateShipment();

			var packingLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance1.DG_CargoMaxAmtUQ = "KG";
			substance1.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E1;
			substance1.DG_UNNO = "1234";

			var undg1 = packingLine.UNDGs.Single();
			undg1.DI_DG = substance1.PK;
			undg1.DI_DGWeight = 1.5;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg1.DI_HasOverpack = false;
			undg1.DI_PackageCount = 1;
			undg1.DI_F3_NKPackType = "BOX";

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			AssertArrayEqualsByElements("generated lines - undg1 exceeds excepted quantity",
				new[]
				{
					"1234 1 Box x 1.5 KG "
				},
				FormatNatureAndQuantityLines(decl));

			undg1.DI_DGWeight = 10;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Grams;

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance2.DG_CargoMaxAmtUQ = "KG";
			substance2.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E2;
			substance2.DG_UNNO = "2345";

			var undg2 = packingLine.UNDGs.AddNew();
			undg2.DI_DG = substance2.PK;
			undg2.DI_DGWeight = 60;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Grams;
			undg2.DI_PackageCount = 2;
			undg2.DI_F3_NKPackType = "PLT";
			undg1.DI_HasOverpack = true;

			decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			AssertArrayEqualsByElements("generated lines - excludes undg1 because it is within excepted quantity",
				new[]
				{
					"2345 2 Pallet x 0.03 KG "
				},
				FormatNatureAndQuantityLines(decl));
		}

		#endregion

		#region TestMergeLines

		public void TestMergeLines()
		{
			var shipment = CreateShipment();

			var packingLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var undgSubstance2250 = CreateUNDGSubstance("2250");

			var undg1 = packingLine.UNDGs.Single();
			undg1.DI_DG = undgSubstance2250.PK;
			undg1.DI_DGVolume = 1;
			undg1.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg1.DI_HasOverpack = false;
			undg1.DI_PackageCount = 1;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg2 = packingLine.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2250.PK;
			undg2.DI_DGVolume = 1;
			undg2.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg2.DI_HasOverpack = true;
			undg2.DI_PackageCount = 1;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg3 = packingLine.UNDGs.AddNew();
			undg3.DI_DG = undgSubstance2250.PK;
			undg3.DI_DGVolume = 3;
			undg3.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg3.DI_HasOverpack = true;
			undg3.DI_PackageCount = 1;
			undg3.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg4 = packingLine.UNDGs.AddNew();
			undg4.DI_DG = undgSubstance2250.PK;
			undg4.DI_DGWeight = 7;
			undg4.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg4.DI_HasOverpack = false;
			undg4.DI_PackageCount = 1;
			undg4.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg5 = packingLine.UNDGs.AddNew();
			undg5.DI_DG = undgSubstance2250.PK;
			undg5.DI_DGWeight = 30;
			undg5.DI_UnitOfWeight = Core.Constants.Weight.Pounds;
			undg5.DI_HasOverpack = false;
			undg5.DI_PackageCount = 1;
			undg5.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg6 = packingLine.UNDGs.AddNew();
			undg6.DI_DG = CreateUNDGSubstance("3333").PK;
			undg6.DI_DGVolume = 1;
			undg6.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg6.DI_HasOverpack = false;
			undg6.DI_PackageCount = 1;
			undg6.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();

			AssertArrayEqualsByElements("generated lines",
				new[]
				{
					"2250 1 Pallet x 1 L ",
					"2250 1 Pallet x 3 L ",
					"Overpack Used",
					"2250 1 Pallet x 1 L ",
					"2250 1 Pallet x 7 KG ",
					"2250 1 Pallet x 13.608 KG ",
					"3333 1 Box x 1 L "
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		#endregion

		public void TestSummaryNatureAndQuantityOfDangerousGoodsLine()
		{
			var shipment = CreateShipment();

			var packline1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline1.JL_F3_NKPackType = "PLT"; 
			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 2;

			var undgSubstance2250 = CreateUNDGSubstance("2250");

			var undg1 = packline1.UNDGs.Single();
			undg1.DI_DG = undgSubstance2250.PK;
			undg1.DI_DGVolume = 1;
			undg1.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg1.DI_HasOverpack = false;
			undg1.DI_PackageCount = 1;
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg2 = packline1.UNDGs.AddNew();
			undg2.DI_DG = undgSubstance2250.PK;
			undg2.DI_DGVolume = 1;
			undg2.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg2.DI_HasOverpack = true;
			undg2.DI_PackageCount = 1;
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg3 = packline1.UNDGs.AddNew();
			undg3.DI_DG = undgSubstance2250.PK;
			undg3.DI_DGVolume = 3;
			undg3.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg3.DI_HasOverpack = true;
			undg3.DI_PackageCount = 1;
			undg3.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg4 = packline1.UNDGs.AddNew();
			undg4.DI_DG = undgSubstance2250.PK;
			undg4.DI_DGWeight = 7;
			undg4.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg4.DI_HasOverpack = false;
			undg4.DI_PackageCount = 1;
			undg4.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg5 = packline2.UNDGs.AddNew();
			undg5.DI_DG = undgSubstance2250.PK;
			undg5.DI_DGWeight = 30;
			undg5.DI_UnitOfWeight = Core.Constants.Weight.Pounds;
			undg5.DI_HasOverpack = false;
			undg5.DI_PackageCount = 1;
			undg5.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var undg6 = packline2.UNDGs.AddNew();
			undg6.DI_DG = CreateUNDGSubstance("3333").PK;
			undg6.DI_DGVolume = 1;
			undg6.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg6.DI_HasOverpack = false;
			undg6.DI_PackageCount = 1;
			undg6.DI_F3_NKPackType = Core.Constants.PkgUnit.Box;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			var page = decl.Pages.Single();

			AssertArrayEqualsByElements("generated lines",
				new[]
				{
					"2250 1 Pallet x 1 L ",
					"2250 1 Pallet x 3 L ",
					"Overpack Used",
					"2250 1 Pallet x 1 L ",
					"2250 1 Pallet x 7 KG ",
					"2250 1 Pallet x 13.608 KG ",
					"3333 1 Box x 1 L ",
					"All Packed in One (Pallet(s) x 2)."
				},
				FormatNatureAndQuantityLines(page));

			AssertLinesHaveUniqueIdentifiers(decl);
		}

		public void TestDGDShowOnlyIATDG()
		{
			var shipment = CreateShipment();

			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Pumpernickel";
			contact.OC_Phone = "8000 1234";
			contact.OC_OH = shipment.Consignor.PK;

			var packline = shipment.OuterPackLines[0];

			var undg = shipment.OuterPackLines[0].UNDGs.AddNew();
			undg.DI_DG = CreateUNDGSubstance("8000").PK;
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;

			undg = packline.UNDGs.AddNew();
			undg.DI_DG = CreateUNDGSubstance("8001").PK;
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1759";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg = packline.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;

			var dec = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			AssertEquals("NatureAndQuantity1 should equals 0143", "0143", dec.NatureAndQuantity1.UNCode);
			AssertEquals("NatureAndQuantity2 should equals 8000", "8000", dec.NatureAndQuantity2.UNCode);
			AssertEquals("NatureAndQuantity3 should equals 8001", "8001", dec.NatureAndQuantity3.UNCode);
			AssertNullOrEmpty("NatureAndQuantity4 should be empty", dec.NatureAndQuantity4.UNCode);
			AssertNullOrEmpty("NatureAndQuantity5 should be empty", dec.NatureAndQuantity5.UNCode);
			AssertNullOrEmpty("NatureAndQuantity6 should be empty", dec.NatureAndQuantity6.UNCode);
			AssertNullOrEmpty("NatureAndQuantity7 should be empty", dec.NatureAndQuantity7.UNCode);
			AssertNullOrEmpty("NatureAndQuantity8 should be empty", dec.NatureAndQuantity8.UNCode);
			AssertNullOrEmpty("NatureAndQuantity9 should be empty", dec.NatureAndQuantity9.UNCode);
			AssertNullOrEmpty("Natantity10 should be empty", dec.NatureAndQuantity10.UNCode);
		}

		public void TestDGDShowLimitedQuantitiesPacking()
		{
			var shipment = CreateShipment();

			var undg1 = shipment.OuterPackLines[0].UNDGs.AddNew();
			var subInstance1 = CreateUNDGSubstance("1104");
			subInstance1.DG_LQMaxAmtType = "";
			subInstance1.DG_PackIns = "Y344";
			subInstance1.DG_PaxPackIns = "355";
			subInstance1.DG_CargoPackIns = "366";
			subInstance1.DG_LQ2OrPaxMaxAmt = 30;
			subInstance1.DG_LQ2OrPaxMaxAmtUQ = Core.Constants.Weight.Kilograms;
			subInstance1.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			subInstance1.DG_LQMaxAmt = 40;
			subInstance1.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;
			subInstance1.DG_CargoMaxAmt = 100;
			subInstance1.DG_CargoMaxAmtUQ = Core.Constants.Weight.Kilograms;
			undg1.DI_DG = subInstance1.PK;
			undg1.DI_PackageCount = 1;
			undg1.DI_DGVolume = 10;
			undg1.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg1.DI_DGWeight = 10;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg1.DI_IsLimitedQuantity = true;

			var undg2 = shipment.OuterPackLines[0].UNDGs.AddNew();
			var subInstance2 = CreateUNDGSubstance("1129");
			subInstance2.DG_PackIns = "Y341";
			subInstance2.DG_PaxPackIns = "353";
			subInstance2.DG_CargoPackIns = "364";
			subInstance2.DG_LQ2OrPaxMaxAmt = 30;
			subInstance2.DG_LQ2OrPaxMaxAmtUQ = Core.Constants.Weight.Kilograms;
			subInstance2.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			subInstance2.DG_LQMaxAmt = 40;
			subInstance2.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;
			subInstance2.DG_CargoMaxAmt = 50;
			subInstance2.DG_CargoMaxAmtUQ = Core.Constants.Weight.Kilograms;
			undg2.DI_DG = subInstance2.PK;
			undg2.DI_PackageCount = 1;
			undg2.DI_DGVolume = 20;
			undg2.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg2.DI_DGWeight = 20;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg2.DI_IsLimitedQuantity = false;

			shipment.Consols[0].Transports[0].JW_IsCargoOnly = true;
			var dec = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();
			AssertEquals("NatureAndQuantity1 PackingInstruction should equals Y344", "Y344", dec.NatureAndQuantity2.PackingInstruction);
			AssertEquals("NatureAndQuantity2 PackingInstruction should be DG_PaxPackIns", "353", dec.NatureAndQuantity3.PackingInstruction);

			shipment.Consols[0].Transports[0].JW_IsCargoOnly = false;
			dec = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();
			AssertEquals("NatureAndQuantity2 PackingInstruction should be DG_PaxPackIns", "353", dec.NatureAndQuantity3.PackingInstruction);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_SpecialProvisionDescription()
		{
			var shipment = CreateShipment();
			var undg = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single().UNDGs.AddNew();
			undg.DI_DG = CreateUNDGSubstance("2941").PK;
			undg.DI_DGWeight = 1;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var specialProvisionAttribute = Factory.New<ViewUNDGAttribute>();
			specialProvisionAttribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SpecialProvisions;
			specialProvisionAttribute.DA_Descriptor = "A1";
			specialProvisionAttribute.DA_DG = undg.Substance.PK;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();
			AssertEquals("NatureAndQuantity Special Provision should equals A1", "A1", decl.NatureAndQuantity2.SpecialProvisionDescriptor);
		}

		public void TestDGDShowPackingInstructionSection()
		{
			var shipment = CreateShipment();
			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packLine = shipment.OuterPackLines.AddNew();
			var dataItem = packLine.UNDGs.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "3480";
			substance.DG_CargoMaxAmt = 20;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "L";
			substance.DG_LQ2OrPaxMaxAmtUQ = "L";
			substance.DG_PackIns = "Y642";
			substance.DG_PaxPackIns = "965";
			substance.DG_CargoPackIns = "663";

			dataItem.DI_DGVolume = 1;
			dataItem.DI_PackageCount = 1;
			dataItem.DI_UnitOfVolume = "L";
			dataItem.DI_DG = substance.PK;
			dataItem.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionIB;

			shipment.Consols[0].Transports[0].JW_IsCargoOnly = true;
			var shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("965 - IB", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			dataItem.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionIA;
			shipment.Consols[0].Transports[0].JW_IsCargoOnly = true;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("965", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);
		}

		public void TestEmptyMessageError()
		{
			var shipment = CreateShipment();

			var shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("MessageError Should be:", string.Empty, shippersDeclarationForDangerousGoods.ErrorMessage);

			var undg = shipment.OuterPackLines[0].UNDGs[0];
			var errorMessage = "There are no airfreight dangerous goods substances entered that require this form. Lithium battery substances UN 3090, 3091, 3480, and 3481 (all variants) packed under Section II, as well as UN 1845 (when the only DG substance), 2807, 3164, 3245, and 3373 do not require the Shipper's Declaration for Dangerous Goods.";

			foreach (var code in new string[] { "3480", "3481A", "3481B", "3090", "3091A", "3091B" })
			{
				var sectionList = (code == "3480" || code == "3090") ? new string[] { "IA", "IB", "II" } : new string[] { "I", "II" };

				foreach (var section in sectionList)
				{
					var excludedSubstance = CreateUNDGSubstance(code, variant: code.Length > 4 ? code.Substring(4) : "");
					excludedSubstance.DG_PackIns = "Y341";
					excludedSubstance.DG_PaxPackIns = "353";
					excludedSubstance.DG_CargoPackIns = "364";
					excludedSubstance.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
					excludedSubstance.DG_LQMaxAmt = 40;
					excludedSubstance.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;
					excludedSubstance.DG_ExceptedQuantityCode = "E5";

					undg.DI_DG = excludedSubstance.PK;
					undg.DI_DGVolume = 20;
					undg.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
					undg.DI_DGWeight = 20;
					undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
					undg.DI_IsLimitedQuantity = true;
					undg.DI_PackingInstructionSection = section;

					shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();

					if (section == "II")
					{
						AssertEquals("Error message should not be:", errorMessage, shippersDeclarationForDangerousGoods.ErrorMessage);
					}
					else
					{
						AssertNotEquals("Error message should be:", errorMessage, shippersDeclarationForDangerousGoods.ErrorMessage);
					}
				}
			}

			var exceptedQuantitySubstance = CreateUNDGSubstance("3005");
			exceptedQuantitySubstance.DG_PackIns = "Y341";
			exceptedQuantitySubstance.DG_PaxPackIns = "353";
			exceptedQuantitySubstance.DG_CargoPackIns = "364";
			exceptedQuantitySubstance.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			exceptedQuantitySubstance.DG_LQMaxAmt = 40;
			exceptedQuantitySubstance.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;
			exceptedQuantitySubstance.DG_LQ2OrPaxMaxAmtUQ = "L";
			exceptedQuantitySubstance.DG_ExceptedQuantityCode = "E5";

			undg.DI_DG = exceptedQuantitySubstance.PK;
			undg.DI_UnitOfVolume = "L";
			undg.DI_DGVolume = 0.200;

			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("MessageError Should be:", "There are no air freight dangerous goods substances entered that require this form. Any DG that is in excepted quantities will not be shown in the form.", shippersDeclarationForDangerousGoods.ErrorMessage);

			undg.DI_DGVolume = 0;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("MessageError Should be:", "There are no air freight dangerous goods substances entered with weight or volume specified. Weight or volume is required to ascertain if the DG is in excepted quantities. Any DG that is in excepted quantities will not be shown in the form.", shippersDeclarationForDangerousGoods.ErrorMessage);

			var dryIceQuantitySubstance = CreateUNDGSubstance("1845");
			dryIceQuantitySubstance.DG_PackIns = "Y341";
			dryIceQuantitySubstance.DG_PaxPackIns = "353";
			dryIceQuantitySubstance.DG_CargoPackIns = "364";
			dryIceQuantitySubstance.DG_LQMaxAmtType = LimitedQuantityTypes.NLMCode;
			dryIceQuantitySubstance.DG_LQMaxAmt = 40;
			dryIceQuantitySubstance.DG_LQMaxAmtUQ = Core.Constants.Weight.Kilograms;

			undg.DI_DG = dryIceQuantitySubstance.PK;
			undg.DI_DGVolume = 20;
			undg.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg.DI_DGWeight = 20;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg.DI_IsLimitedQuantity = true;

			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("MessageError Should be:", errorMessage, shippersDeclarationForDangerousGoods.ErrorMessage);

			shipment.OuterPackLines.RemoveAll();

			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("MessageError Should be:", "There must be at least one air freight dangerous goods substance entered to issue this form.", shippersDeclarationForDangerousGoods.ErrorMessage);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_TechnicalName()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.JL_PackageCount = 20;
			packline.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			var dgItem = packline.UNDGs.First();
			dgItem.DI_PackageCount = 10;
			dgItem.DI_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			dgItem.DI_TechnicalName = "Tech Name";

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();
			AssertEquals("Technical name should come from DG item", "Tech Name", decl.NatureAndQuantity1.TechnicalName);
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_EqualityComparerTechnicalName()
		{
			var shipment = CreateShipment();

			var packingLine = shipment.OuterPackLines.AddNew();

			var substance1 = CreateUNDGSubstance("2959");

			var undg2 = packingLine.UNDGs.AddNew();
			undg2.DI_DG = substance1.PK;
			undg2.DI_PackageCount = 5;
			undg2.DI_DGWeight = 1;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg2.DI_TechnicalName = "Tech Name";

			var undg3 = packingLine.UNDGs.AddNew();
			undg3.DI_DG = substance1.PK;
			undg3.DI_PackageCount = 5;
			undg3.DI_DGWeight = 1;
			undg3.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg3.DI_TechnicalName = "Tech Name";

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			AssertContainsExactElementsInAnyOrder("nature and quantity of goods",
				new[]
				{
					"0143",
					"2959",
					"2959"
				},
				new[]
				{
					decl.NatureAndQuantity1?.UNCode,
					decl.NatureAndQuantity2?.UNCode,
					decl.NatureAndQuantity3?.UNCode,
					decl.NatureAndQuantity4?.UNCode,
					decl.NatureAndQuantity5?.UNCode,
					decl.NatureAndQuantity6?.UNCode,
					decl.NatureAndQuantity7?.UNCode,
					decl.NatureAndQuantity8?.UNCode,
					decl.NatureAndQuantity9?.UNCode,
					decl.NatureAndQuantity10?.UNCode
				}.Where(c => c.HasValue && !c.Value.IsEmpty).Select(c => c.ToString()));
			CombineAssertions(() =>
			{
				AssertEquals("2", 5, decl.NatureAndQuantity2?.PackCount);
				AssertEquals("3", 5, decl.NatureAndQuantity3?.PackCount);
			});
		}

		public void TestNatureAndQuantityOfDangerousGoodsLines_IncludeNECInformation()
		{
			var shipment = CreateShipment();
			var departureConsol = shipment.DepartureConsol;
			var departureTransport = departureConsol.MostInterestingTransportForBinding.Cast<Freight.Business.Transport>().FirstOrDefault();
			departureTransport.JW_IsCargoOnly = false;

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_UNNO = "A01";
			substance1.DG_Code = "A01";
			substance1.DG_CargoMaxAmtUQ = "L";
			substance1.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance1.DG_LQMaxAmtUQ = "KG";
			substance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var undg = packline.UNDGs[0];
			undg.DI_DG = substance1.PK;
			undg.DI_DGWeight = 1.1m;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg.DI_DGVolume = 2.2m;
			undg.DI_UnitOfVolume = Core.Constants.Volume.Litre;
			undg.DI_PackageCount = 1;
			undg.DI_NECWeight = 1;
			undg.DI_NECWeightUQ = Core.Constants.Weight.Kilograms;

			var decl = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build().Pages.Single();

			AssertEquals("NEC Weight should come from DG item", undg.DI_NECWeight, decl.NatureAndQuantity1.NECWeight.Value);
			AssertEquals("NEC Weight Units should come from DG item", undg.DI_NECWeightUQ, decl.NatureAndQuantity1.NECWeight.Unit.Code);
		}

		public void TestProperShippingNameForNOS()
		{
			var shipment = CreateShipment();
			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packLine = shipment.OuterPackLines.AddNew();
			var dataItem = packLine.UNDGs.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "2395";
			substance.DG_Code = "2395";
			substance.DG_PSN = "Isobutyryl chloride";
			dataItem.DI_PackageCount = 1;
			dataItem.DI_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			dataItem.DI_DG = substance.PK;

			var shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();

			AssertEquals("Isobutyryl chloride", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.ProperShippingName);

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "0190";
			substance.DG_Code = "0190";
			substance.DG_PSN = "Samples, explosive";
			var attribute = substance.QualifyingDescriptiveTexts.AddNew();
			attribute.DA_Descriptor = "other than initiating explosives";
			attribute.DA_Language = "EN";
			dataItem.DI_DG = substance.PK;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("Samples, explosive", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.ProperShippingName);

			dataItem.DI_IsNotOtherwiseSpecified = true;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("Samples, explosive, n.o.s., other than initiating explosives", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.ProperShippingName);

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "3535";
			substance.DG_Code = "3535a";
			substance.DG_PSN = "Toxic solid, flammable, inorganic";
			attribute = substance.QualifyingDescriptiveTexts.AddNew();
			attribute.DA_Descriptor = "A5";
			attribute.DA_Language = "EN";
			dataItem.DI_DG = substance.PK;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("Toxic solid, flammable, inorganic", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.ProperShippingName);

			dataItem.DI_IsNotOtherwiseSpecified = true;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("Toxic solid, flammable, inorganic, n.o.s.", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.ProperShippingName);

			dataItem.DI_IsHighwayRouteControlledQuantity = true;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("Toxic solid, flammable, inorganic, n.o.s., HRCQ", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.ProperShippingName);
		}

		public void TestPermittedTransportTypeForVolume()
		{
			var shipment = CreateShipment();
			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packLine = shipment.OuterPackLines.AddNew();
			var dataItem = packLine.UNDGs.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "1234";
			substance.DG_CargoMaxAmt = 20;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "L";
			substance.DG_LQ2OrPaxMaxAmtUQ = "L";
			substance.DG_PackIns = "Y642";
			substance.DG_PaxPackIns = "655";
			substance.DG_CargoPackIns = "663";

			dataItem.DI_DGVolume = 1;
			dataItem.DI_PackageCount = 1;
			dataItem.DI_UnitOfVolume = "L";

			dataItem.DI_DG = substance.PK;
			shipment.Consols[0].Transports[0].JW_IsCargoOnly = false;
			var shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("PASSENGER AND CARGO AIRCRAFT", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals("655", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			shipment.Consols[0].Transports[0].JW_IsCargoOnly = true;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("PASSENGER AND CARGO AIRCRAFT", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals("655", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			shipment.Consols[0].Transports[0].JW_IsCargoOnly = false;
			dataItem.DI_DGVolume = 10;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("CARGO AIRCRAFT ONLY", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals("663", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			dataItem.DI_DGVolume = 40;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("FORBIDDEN", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals(ZString.Empty, shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			dataItem.DI_DGVolume = 1;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("CARGO AIRCRAFT ONLY", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals("663", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			substance.DG_CargoPackAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("FORBIDDEN", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals(ZString.Empty, shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);
		}

		public void TestPermittedTransportTypeForWeight()
		{
			var shipment = CreateShipment();
			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packLine = shipment.OuterPackLines.AddNew();
			var dataItem = packLine.UNDGs.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "1234";
			substance.DG_CargoMaxAmt = 20;
			substance.DG_LQ2OrPaxMaxAmt = 5;
			substance.DG_CargoMaxAmtUQ = "KG";
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_PackIns = "Y642";
			substance.DG_PaxPackIns = "655";
			substance.DG_CargoPackIns = "663";

			dataItem.DI_DGWeight = 1;
			dataItem.DI_PackageCount = 1;
			dataItem.DI_UnitOfWeight = "KG";

			dataItem.DI_DG = substance.PK;
			shipment.Consols[0].Transports[0].JW_IsCargoOnly = false;
			var shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("PASSENGER AND CARGO AIRCRAFT", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals("655", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			shipment.Consols[0].Transports[0].JW_IsCargoOnly = true;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("PASSENGER AND CARGO AIRCRAFT", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals("655", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			shipment.Consols[0].Transports[0].JW_IsCargoOnly = false;
			dataItem.DI_DGWeight = 10;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("CARGO AIRCRAFT ONLY", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals("663", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			dataItem.DI_DGWeight = 40;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("FORBIDDEN", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals(ZString.Empty, shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			dataItem.DI_DGWeight = 1;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("CARGO AIRCRAFT ONLY", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals("663", shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);

			substance.DG_CargoPackAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoodsBuilder(shipment).Build();
			AssertEquals("FORBIDDEN", shippersDeclarationForDangerousGoods.Pages.Single().PermittedTransportType);
			AssertEquals(ZString.Empty, shippersDeclarationForDangerousGoods.Pages.Single().NatureAndQuantity1.PackingInstruction);
		}

		#region Implementation

		UNDGSubstance CreateUNDGSubstance(string code, string unno = "", string standard = UNDGSubstanceStandardTypes.IATA, string variant = "")
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = string.IsNullOrEmpty(unno) ? code.Substring(0, 4) : unno;
			substance.DG_Code = code;
			substance.DG_Standard = standard;
			substance.DG_Variant = variant;

			return substance;
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HOUSEBILL001";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MR Consignee";
			consignee.OH_RL_NKClosestPort = "SGSIN";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "MYKUL";
			departureConsol.JK_BookingReference = "BKG001";
			departureConsol.JK_MasterBillNum = "081-0000001";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_UniqueConsignRef = "CONSOL0002";
			arrivalConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			arrivalConsol.JK_RL_NKLoadPort = "MYKUL";
			arrivalConsol.JK_RL_NKDischargePort = "SGSIN";
			arrivalConsol.JK_BookingReference = "BKG002";
			arrivalConsol.JK_MasterBillNum = "001-0000001";

			var container = departureConsol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 1;
			packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline1.JL_ActualWeight = 2000;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline1.JL_ActualVolume = 1.3;
			packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			container.PackLines.Add(packline1);

			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Pumpernickel";
			contact.OC_Phone = "8000 1234";
			contact.OC_OH = shipper.PK;

			var undg = packline1.UNDGs.AddNew();
			undg.DI_DG = CreateUNDGSubstance("0143b", "0143", UNDGSubstanceStandardTypes.IATA, "b").PK;
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg.DI_PackageCount = 1;
			undg.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			Factory.Save();

			return shipment;
		}

		string[] FormatNatureAndQuantityLines(ShippersDeclarationForDangerousGoodsDetail decl)
		{
			return decl
				.AsEnumerable()
				.Select(FormatNatureAndQuantityLine)
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.ToArray();
		}

		string FormatNatureAndQuantityLine(NatureAndQuantityOfDangerousGoodsLine line)
		{
			switch (line.LineType)
			{
				case NatureAndQuantityOfDangerousGoodsLineType.Detail:
					return line.IncludePackInformation
						? $"{line.UNCode}{line.Variant} {line.PackCount} {line.PackageType.Description} x {line.Quantity?.Value} {line.Quantity?.Unit.Code} {line.QuantityIndicator}"
						: $"{line.UNCode}{line.Variant} {line.Quantity?.Value} {line.Quantity?.Unit.Code} {line.QuantityIndicator}";

				case NatureAndQuantityOfDangerousGoodsLineType.Summary:
					return line.Description;

				default:
					return null;
			}
		}

		void AssertLinesHaveUniqueIdentifiers(ShippersDeclarationForDangerousGoods decl)
		{
			var lines = decl.Pages.SelectMany(p => p.AsEnumerable());
			AssertEquals("Identifiers must be unique", lines.Count(), lines.Distinct().Count());

			var detailLineIdentifiers = lines.Where(l => l.LineType == NatureAndQuantityOfDangerousGoodsLineType.Detail).Select(l => l.Identifier);
			if (detailLineIdentifiers.Any())
			{
				Assert("Details lines should have Int32 Identifier", detailLineIdentifiers.All(i => i.GetType() == typeof(Int32)));
			}

			var otherLineIdentifiers = lines.Where(l => l.LineType != NatureAndQuantityOfDangerousGoodsLineType.Detail).Select(l => l.Identifier);
			if (otherLineIdentifiers.Any())
			{
				Assert("Non-Detail lines should have auto-assigned Guid Identifier.", otherLineIdentifiers.All(i => i.GetType().FullName == typeof(ZGuid).FullName));
			}
		}

		#endregion
	}
}
