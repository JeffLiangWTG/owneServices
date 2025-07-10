using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.Testing
{
	public class BaseRateLinesAndItemsControlTest : RatingTestCase
	{
		class MockControl : BaseRateLinesAndItemsControl
		{
			protected override void RateLinesGrid_DragDrop(object sender, DragEventArgs e)
			{
				RateLinesGrid_DragDropCalled = true;
			}

			protected override void RateLinesGrid_DragEnter(object sender, DragEventArgs e)
			{
				RateLinesGrid_DragEnterCalled = true;
			}

			public IRateLine ExposedInsertRelatedRateLine(RelatedRateLine line, RateEntry entry) //to test protected method
			{
				return base.InsertRelatedRateLine(line, entry);
			}

			public bool RateLinesGrid_DragDropCalled;
			public bool RateLinesGrid_DragEnterCalled;
		}

		public void TestDragAndDropHandlers()
		{
			MethodInfo onDragEnter = typeof(ZGrid).GetMethod("OnDragEnter", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo onDragDrop = typeof(ZGrid).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);

			using (MockControl control = new MockControl())
			{
				onDragEnter.Invoke(control.RateLinesGrid, new object[] { null });
				Assert(control.RateLinesGrid_DragEnterCalled);
				onDragDrop.Invoke(control.RateLinesGrid, new object[] { null });
				Assert(control.RateLinesGrid_DragDropCalled);
			}
		}

		public void TestInsertRelatedRateline_FromTariff_FromCompanyTariffLevel1_SetApplyToLine()
		{
			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("DST", "AIR", "", "AUSYD");
			var tariffLine = tariffEntry.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			tariff.Factory.Save();

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var quoteEntry = quote.AddRateEntry("DST", "AIR", "", "AUSYD");

			var relatedLine = Factory.Load<RelatedRateLine>(tariffLine.PK);

			using (var mockControl = new MockControl())
			{
				var insertedLine = mockControl.ExposedInsertRelatedRateLine(relatedLine, quoteEntry);
				var ctbCalculator = (CompanyTariffOrCostBasedCalculator)insertedLine.Calculator;

				Assert(relatedLine.IsTariff);
				Assert(insertedLine.TL_CompanyTariffLevel <= 1);
				AssertEquals((ZString)relatedLine.PK.ToString(), ctbCalculator.ApplyToLine);
			}
		}

		[ExpectNoExceptions]
		public void TestInsertRelatedRateline_FromCosting_EntryReadOnly()
		{
			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("DST", "AIR", "", "AUSYD");
			var tariffLine = tariffEntry.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			tariff.Factory.Save();

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var readOnlyEntry = quote.AddRateEntry("DST", "AIR", "", "AUSYD");
			readOnlyEntry.ReadOnly = true;

			var relatedLine = Factory.Load<RelatedRateLine>(tariffLine.PK);

			using (var mockControl = new MockControl())
			{
				var insertedLine = mockControl.ExposedInsertRelatedRateLine(relatedLine, readOnlyEntry);
				AssertNull("Parent entry is readonly, should not insert line", insertedLine);
			}
		}

		public void TestInsertRelatedRateline_CompanyTariffLevel2_ToClientRate_ApplyToLineNotSet()
		{
			var tariff1 = Helper.NewCompanyTariff();
			var entry1 = tariff1.AddRateEntry("DST", "AIR", "", "AUSYD");
			entry1.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			tariff1.Factory.Save();

			var tariff2 = Helper.NewCompanyTariff();
			var entry2 = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST)[0];
			var line2 = entry2.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			tariff2.Factory.Save();

			AssertEquals("RateLine set to correct tariff level", (ZByte)2, line2.TL_CompanyTariffLevel);

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var quoteEntry = quote.AddRateEntry("DST", "AIR", "", "AUSYD");

			var relatedLine = Factory.Load<RelatedRateLine>(line2.PK);

			using (var mockControl = new MockControl())
			{
				var insertedLine = mockControl.ExposedInsertRelatedRateLine(relatedLine, quoteEntry);
				var applyToLine = insertedLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().ApplyToLine;

				AssertEquals(insertedLine.TL_CompanyTariffLevel, (ZByte)2);
				AssertNullOrEmpty(applyToLine);
			}
		}

		#region Equipment Type for Cost/CompanyTariff Based Calculator

		public void TestInsertRelatedRateLine_CTGWithEquipmentType_CST()
		{
			var chargeCode = Helper.ChargeCodes.New("TCART", "Transport Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var costing = Helper.NewCosting(TransportProvider1);
			var costingRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "", removeLines: true);
			var costingRateLine = costingRateEntry.AddRateLine(chargeCode, CartageCalculator.Code, "KG");
			costingRateLine.Calculator.EquipmentType = LCLAIREquipmentNeeded.HandHaulier;
			costingRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			var clientRate = Helper.NewClientRate(Consignee);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "", removeLines: true);
			var clientRateLine = clientRateEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode, "KG");
			var clientRateCalculator = clientRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			clientRateCalculator.PerUnitPercent = 20m;

			var relatedLine = Factory.Load<RelatedRateLine>(costingRateLine.PK);

			using (var mockControl = new MockControl())
			{
				var insertedLine = mockControl.ExposedInsertRelatedRateLine(relatedLine, clientRateEntry);
				var insertedCalculator = insertedLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
				AssertEquals("EquipmentType", LCLAIREquipmentNeeded.HandHaulier, insertedCalculator.EquipmentType);
			}
		}

		public void TestInsertRelatedRateLine_CTGWithEquipmentType_CTB()
		{
			var chargeCode = Helper.ChargeCodes.New("TCART", "Transport Cartage", CartageZoneDistanceCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			Factory.Save();

			var tariff = Helper.NewCompanyTariff();
			var tarifRateEntry = tariff.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "", removeLines: true);
			var tarifRateLine = tarifRateEntry.AddRateLine(chargeCode, CartageCalculator.Code, "KG");
			tarifRateLine.Calculator.EquipmentType = LCLAIREquipmentNeeded.HandHaulier;
			tarifRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;
			tariff.Factory.Save();

			var clientRate = Helper.NewClientRate(Consignee);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.LRO, "AU", "", removeLines: true);
			var clientRateLine = clientRateEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, "KG");
			var clientRateCalculator = clientRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			clientRateCalculator.PerUnitPercent = 20m;

			var relatedLine = Factory.Load<RelatedRateLine>(tarifRateLine.PK);

			using (var mockControl = new MockControl())
			{
				var insertedLine = mockControl.ExposedInsertRelatedRateLine(relatedLine, clientRateEntry);
				var insertedCalculator = insertedLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
				AssertEquals("EquipmentType", LCLAIREquipmentNeeded.HandHaulier, insertedCalculator.EquipmentType);
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestDisposedControlCallingManager()
		{
			MockControl control = new MockControl();
			control.Dispose();
			control.SetDataBinding(null, null);
		}
	}
}
