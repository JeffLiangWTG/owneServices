using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CommonHelper))]
	sealed class CommonHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCheckIsCarRelatedTariff()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.CheckIsCarRelatedTariff("86XXXXXX"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.CheckIsCarRelatedTariff("87XXXXXX"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.CheckIsCarRelatedTariff("XXXXXXXX"), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestFormattedEntryNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.FormattedEntryNumber(""), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.FormattedEntryNumber("CABB0999900026"), NUnit.Framework.Is.EqualTo("CA/BB/09/999/00026").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.FormattedEntryNumber("BB  0912300001"), NUnit.Framework.Is.EqualTo("BB/  /09/123/00001").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.FormattedEntryNumber("B"), NUnit.Framework.Is.EqualTo("B////").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestGetEntryNumberPart5()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.GetEntryNumberPart5(""), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetEntryNumberPart5("CABB0999900026"), NUnit.Framework.Is.EqualTo("00026").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetEntryNumberPart5("BB  0912300001"), NUnit.Framework.Is.EqualTo("00001").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetEntryNumberPart5("B"), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestRemoveEntryNumberPlaceHolder()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.RemoveEntryNumberPlaceHolder(new ZString("<!-- placeholder:EntryNumber -->")), NUnit.Framework.Is.EqualTo(ZString.Empty));
				NUnit.Framework.Assert.That(CommonHelper.RemoveEntryNumberPlaceHolder(new ZString("<!-- placeholder:EntryNumber -->1111")), NUnit.Framework.Is.EqualTo("<!-- placeholder:EntryNumber -->1111").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestUpdateIfNotEmpty()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_AlcoholCountryRegion = "2";
			var info = invoiceLine.JI_AlcoholCountryRegionInfo;
			CommonHelper.UpdateIfNotEmpty(info, new ZString("3"));
			NUnit.Framework.Assert.That(invoiceLine.JI_AlcoholCountryRegion, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			CommonHelper.UpdateIfNotEmpty(info, ZString.Empty);
			NUnit.Framework.Assert.That(invoiceLine.JI_AlcoholCountryRegion, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestZeroConvertToEmptyString()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.ZeroConvertToEmptyString(ZShort.Zero), NUnit.Framework.Is.EqualTo(ZString.Empty));
				NUnit.Framework.Assert.That(CommonHelper.ZeroConvertToEmptyString(9999), NUnit.Framework.Is.EqualTo("9999").Using(CustomComparers.TypeComparison));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestIStorageDocsBaseCollections()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
			var doc3 = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var doc4 = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
			var allEDocs = new IStorageDocsBaseCollection[] { null, declaration.DocManagerInfo.AllEDocs, shipment.DocManagerInfo.AllEDocs };
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(allEDocs.GetFromUniqueKey(doc1.UniqueKey.ToGuid()), NUnit.Framework.Is.EqualTo(doc1));
				NUnit.Framework.Assert.That(allEDocs.GetFromUniqueKey(doc2.UniqueKey.ToGuid()), NUnit.Framework.Is.EqualTo(doc2));
				NUnit.Framework.Assert.That(allEDocs.GetFromUniqueKey(doc3.UniqueKey.ToGuid()), NUnit.Framework.Is.EqualTo(doc3));
				NUnit.Framework.Assert.That(allEDocs.GetFromUniqueKey(doc4.UniqueKey.ToGuid()), NUnit.Framework.Is.EqualTo(doc4));
				NUnit.Framework.Assert.That(allEDocs.FirstIeDoc(), NUnit.Framework.Is.EqualTo(doc1));
			});
		}

		[ExpectNoExceptions]
		public void TestGetFormattedStringForRateFormulaDerivedFrom()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.GetFormattedStringForRateFormulaDerivedFrom("0"), NUnit.Framework.Is.EqualTo("0%").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetFormattedStringForRateFormulaDerivedFrom("0.15"), NUnit.Framework.Is.EqualTo("15%").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetFormattedStringForRateFormulaDerivedFrom("1"), NUnit.Framework.Is.EqualTo("100%").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetFormattedStringForRateFormulaDerivedFrom("15/LTR"), NUnit.Framework.Is.EqualTo("15/LTR").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetFormattedStringForRateFormulaDerivedFrom("other text"), NUnit.Framework.Is.EqualTo("other text").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetFormattedStringForRateFormulaDerivedFrom(""), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestGetDecimalForRateFormulaDerivedFrom()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.GetDecimalForRateFormulaDerivedFrom("0"), NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
				NUnit.Framework.Assert.That(CommonHelper.GetDecimalForRateFormulaDerivedFrom("0.15"), NUnit.Framework.Is.EqualTo(0.15m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetDecimalForRateFormulaDerivedFrom("1"), NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetDecimalForRateFormulaDerivedFrom("15/LTR"), NUnit.Framework.Is.EqualTo(15m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetDecimalForRateFormulaDerivedFrom("other text"), NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
				NUnit.Framework.Assert.That(CommonHelper.GetDecimalForRateFormulaDerivedFrom(""), NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
			});
		}

		[ExpectNoExceptions]
		public void TestShouldDefaultIsIncludedInIvoiceLineToTrue()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CIF", CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CIF", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CIF", CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CIF", CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CFR", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CFR", CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CFR", CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("C&I", CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("C&I", CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("C&I", CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CIP", CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CIP", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("DAT", CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("DAT", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("DAP", CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("DAP", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("DDP", CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("DDP", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("DPU", CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("DPU", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CPT", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("", ""), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("AAA", "BBB"), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CIF", ""), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CFR", ""), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("CI", ""), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("FOB", ""), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("FOB", CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("FOB", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("FOB", CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("FOB", CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("", CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("", CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("", CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.ShouldDefaultIsIncludedInIvoiceLineToTrue("", CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.EqualTo(false));
			});
		}

		[ExpectNoExceptions]
		public void TestShouldResetDefaultIsIncludedInAmount()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeTypeList.Codes.AdditionCharge), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeTypeList.Codes.AdditionCharge), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndInsurance, CustomsChargeTypeList.Codes.AdditionCharge), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndInsurance, CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndInsurance, CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndInsurance, CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.CostAndInsurance, CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeTypeList.Codes.AdditionCharge), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeTypeList.Codes.AdditionCharge), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.ExWorks, CustomsChargeTypeList.Codes.AdditionCharge), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.ExWorks, CustomsChargeTypeList.Codes.ForeignInlandFreight), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.ExWorks, CustomsChargeTypeList.Codes.PackingCost), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.ExWorks, CustomsChargeTypeList.Codes.ExWorks), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.ExWorks, CustomsChargeTypeList.Codes.OverseasInsurance), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!CommonHelper.ShouldResetDefaultIsIncludedInAmount(Core.Constants.IncoTerms.ExWorks, CustomsChargeTypeList.Codes.OverseasFreight), NUnit.Framework.Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestGetTWTransportCode()
		{
			var dicSeaContainerMode = new Dictionary<ZString, ZString>()
			{
				{ ContainerModeList.Codes.BreakBulk, TransportCodeList.Codes.SeaPackedSundryGoods },
				{ ContainerModeList.Codes.Containerized, TransportCodeList.Codes.SeaContainer },
				{ ContainerModeList.Codes.Bulk, TransportCodeList.Codes.SeaBulkGoods },
				{ ContainerModeList.Codes.OwnPropulsion, TransportCodeList.Codes.SeaSelfPropelledGoods },
				{ ContainerModeList.Codes.HandCarry, TransportCodeList.Codes.SeaPassengerOrCREW },
				{ ContainerModeList.Codes.Express, TransportCodeList.Codes.SeaExpressDelivery },
				{ ContainerModeList.Codes.Mail, TransportCodeList.Codes.SeaMail },
				{ ContainerModeList.Codes.FixedTransportInstallation, TransportCodeList.Codes.SeaAndAirFixedTransportInstallation },
				{ ContainerModeList.Codes.Pipeline, TransportCodeList.Codes.SeaAndAirPipeline },
				{ ContainerModeList.Codes.PowerLine, TransportCodeList.Codes.SeaAndAirPowerLine },
				{ ContainerModeList.Codes.Other, TransportCodeList.Codes.Other }
			};
			var dicAirContainerMode = new Dictionary<ZString, ZString>()
			{
				{ ContainerModeList.Codes.Loose, TransportCodeList.Codes.AirNotExpressDelivery },
				{ ContainerModeList.Codes.Express, TransportCodeList.Codes.AirExpressDelivery },
				{ ContainerModeList.Codes.OwnPropulsion, TransportCodeList.Codes.AirSelfPropelledGoods },
				{ ContainerModeList.Codes.HandCarry, TransportCodeList.Codes.AirPassengerOrCREW },
				{ ContainerModeList.Codes.Mail, TransportCodeList.Codes.AirMail },
				{ ContainerModeList.Codes.FixedTransportInstallation, TransportCodeList.Codes.SeaAndAirFixedTransportInstallation },
				{ ContainerModeList.Codes.Pipeline, TransportCodeList.Codes.SeaAndAirPipeline },
				{ ContainerModeList.Codes.PowerLine, TransportCodeList.Codes.SeaAndAirPowerLine },
				{ ContainerModeList.Codes.Other, TransportCodeList.Codes.Other }
			};
			var bizO = Factory.New<JobDeclaration>();
			foreach (CodeDescriptionPair transportType in bizO.Lookups.TransportTypeList)
			{
				foreach (CodeDescriptionPair cargoId in bizO.Lookups.CargoIdTypeList)
				{
					var transportMode = transportType.Code;
					var containerMode = cargoId.Code;
					switch (transportMode)
					{
						case TransportTypeList.Codes.Sea:
							dicSeaContainerMode.TryGetValue(containerMode, out var seatWTransportCode);
							NUnit.Framework.Assert.That(CommonHelper.GetTWTransportCode(transportMode, containerMode), NUnit.Framework.Is.EqualTo(seatWTransportCode));
							break;
						case TransportTypeList.Codes.Air:
							dicAirContainerMode.TryGetValue(containerMode, out var airtWTransportCode);
							NUnit.Framework.Assert.That(CommonHelper.GetTWTransportCode(transportMode, containerMode), NUnit.Framework.Is.EqualTo(airtWTransportCode));
							break;
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestFindOrCreateWithNumberType()
		{
			var countryCode = Core.Constants.CountryCodes.Taiwan;
			var addressNumbers = new JobDocAddressNumberCollection(Factory.New<JobDocAddress>());
			var addressNumber1 = addressNumbers.AddNew("X3", countryCode);
			var addressNumber2 = addressNumbers.AddNew("X2", countryCode);
			var addressNumber3 = addressNumbers.AddNew("X1", countryCode);
			var addressNumber = addressNumbers.FindOrCreateWithNumberType("", countryCode, new string[] { "X1", "X2", "X3" });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(addressNumber.PK, NUnit.Framework.Is.EqualTo(addressNumber3.PK));
				NUnit.Framework.Assert.That(addressNumber2.IsDeleted, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(addressNumber1.IsDeleted, NUnit.Framework.Is.True);
			});
			addressNumber = addressNumbers.FindOrCreateWithNumberType("", countryCode, new string[] { "X2", "X1", "X3" });
			NUnit.Framework.Assert.That(addressNumber.PK, NUnit.Framework.Is.EqualTo(addressNumber3.PK));
			addressNumber = addressNumbers.FindOrCreateWithNumberType("", countryCode, new string[] { "X4", "X5" });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(addressNumber, NUnit.Framework.Is.EqualTo(default(Enterprise.MasterFiles.Business.JobDocAddressNumber)));
				NUnit.Framework.Assert.That(addressNumbers.Count, NUnit.Framework.Is.EqualTo(1));
			});
			addressNumber = addressNumbers.FindOrCreateWithNumberType("X4", countryCode, new string[] { "X4", "X5" });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(addressNumbers.Count, NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(addressNumber.E2N_NumberType, NUnit.Framework.Is.EqualTo("X4").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(addressNumber.E2N_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(countryCode).Using(CustomComparers.TypeComparison));
			});
			addressNumber = addressNumbers.FindOrCreateWithNumberType("X2", countryCode, new string[] { "X2", "X1", "X3" });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(addressNumber.PK, NUnit.Framework.Is.EqualTo(addressNumber3.PK));
				NUnit.Framework.Assert.That(addressNumbers.Count, NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(addressNumber.E2N_NumberType, NUnit.Framework.Is.EqualTo("X2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(addressNumber.E2N_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(countryCode).Using(CustomComparers.TypeComparison));
			});
			addressNumber = addressNumbers.FindOrCreateWithNumberType("X2", Core.Constants.CountryCodes.UnitedStates, new string[] { "X2", "X1", "X3" });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(addressNumber.PK, NUnit.Framework.Is.Not.EqualTo(addressNumber3.PK));
				NUnit.Framework.Assert.That(addressNumbers.Count, NUnit.Framework.Is.EqualTo(3));
				NUnit.Framework.Assert.That(addressNumber.E2N_NumberType, NUnit.Framework.Is.EqualTo("X2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(addressNumber.E2N_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.UnitedStates).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestGetVesselDescription()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "VESSEL 1";
			vessel1.RV_LloydsNumber = "8811928";
			vessel1.RV_RadioCallSign = "8811927";
			vessel1.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "VESSEL 2";
			vessel2.RV_RadioCallSign = "8811924";
			vessel2.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Code = "VESSEL 3";
			vessel3.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.GetVesselDescription(vessel1), NUnit.Framework.Is.EqualTo("8811928").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetVesselDescription(vessel2), NUnit.Framework.Is.EqualTo("8811924").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetVesselDescription(vessel3), NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.GetVesselDescription(null), NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestConvertToNilWhenEmpty()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.ConvertToNilWhenEmpty(ZString.Empty), NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CommonHelper.ConvertToNilWhenEmpty("Test"), NUnit.Framework.Is.EqualTo("Test").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestIsMatchEntryNumber()
		{
			var entryNumberGeneratorProvider = Factory.New<EntryNumberGeneratorProviderForTest>();
			entryNumberGeneratorProvider.EntryNumberPart1 = "AA";
			entryNumberGeneratorProvider.EntryNumberPart2 = "BB";
			entryNumberGeneratorProvider.EntryNumberDate = new ZDateTime(2020, 01, 01);
			entryNumberGeneratorProvider.CustomsBrokerageBoxNumber = "123";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "AA  0912300001"), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "BB  0912300001"), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "AA  0812300001"), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "AA  0932100001"), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "AABB0912300001"), NUnit.Framework.Is.EqualTo(false));
			});

			entryNumberGeneratorProvider.SequenceNumber = "ABC12";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "AA  0912300001"), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "AA  09123ABC12"), NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "BB  09123ABC12"), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "AA  08123ABC12"), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "AA  09321ABC12"), NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, "AABB09123ABC12"), NUnit.Framework.Is.EqualTo(false));
			});
		}
	}
}
