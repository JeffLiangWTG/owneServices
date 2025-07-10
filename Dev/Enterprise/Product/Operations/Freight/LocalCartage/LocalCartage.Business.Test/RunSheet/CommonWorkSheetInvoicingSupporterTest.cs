using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonWorkSheetInvoicingSupporter))]
	public class CommonWorkSheetInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestPrecondition()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var invoicingSupporter = new CommonWorkSheetInvoicingSupporter(workSheet, cartage);
			AssertEquals("Container Mode", Constants.ContainerModes.LCL, invoicingSupporter.ContainerMode);
			AssertEquals("Actual Volume", 0m, invoicingSupporter.ActualVolume);
			AssertEquals("Actual Volume (Unit)", ZString.Empty, invoicingSupporter.ActualVolumeUnit);
			AssertEquals("Actual Weight", 0m, invoicingSupporter.ActualWeight);
			AssertEquals("Actual Weight (Unit)", ZString.Empty, invoicingSupporter.ActualWeightUnit);
			AssertEquals("Container Count", 0, invoicingSupporter.ContainerCount);
			AssertEquals("TEU Count", 0m, invoicingSupporter.TEUCount);
			AssertEquals("Actual Chargeable", 0m, invoicingSupporter.ActualChargeable);
			AssertEquals("Actual Chargeable (Unit)", ZString.Empty, invoicingSupporter.ActualChargeableUnit);
			AssertEquals("Consumer Type", JobInvoicingConsumerTypes.LocalCartage, invoicingSupporter.ConsumerType);
		}

		public void TestMeasures_Containerised()
		{
			// Arrange
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var legs = Helper.CreateCartageLegs(cartage, 3);
			foreach (var leg in legs)
			{
				var move = leg.BookedCtgMove;
				move.EW_BookedVolume = 1m;
				move.EW_VolumeUQ = "M3";
				move.Container.JC_GrossWeight = 2m;
				move.Container.JC_GrossWeightUQ = "KG";
				move.Container.JC_ContainerCount = 3;
				move.Container.RefContainer.RC_TEU = 1;
				move.EW_BookedPackCount = 4;
				move.EW_F3_NKPackType = "UNT";
				leg.JU_EY_RunSheet = workSheet.PK;
			}

			// Act
			var invoicingSupporter = new CommonWorkSheetInvoicingSupporter(workSheet, cartage);
			// Assert
			AssertEquals("Container Mode", Constants.ContainerModes.FCL, invoicingSupporter.ContainerMode);
			AssertEquals("Actual Volume", 3m, invoicingSupporter.ActualVolume);
			AssertEquals("Actual Volume (Unit)", "M3", invoicingSupporter.ActualVolumeUnit);
			AssertEquals("Actual Weight", 6m, invoicingSupporter.ActualWeight);
			AssertEquals("Actual Weight (Unit)", "KG", invoicingSupporter.ActualWeightUnit);
			AssertEquals("Container Count", 9, invoicingSupporter.ContainerCount);
			AssertEquals("TEU Count", 9m, invoicingSupporter.TEUCount);
			AssertEquals("Actual Chargeable", 0m, invoicingSupporter.ActualChargeable);
			AssertEquals("Actual Chargeable (Unit)", "UNT", invoicingSupporter.ActualChargeableUnit);
		}

		public void TestMeasures_UnitConversion()
		{
			// Arrange
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var legs = Helper.CreateCartageLegs(cartage, 3).ToArray();
			workSheet.CartageLegs.AddRange(legs);
			var firstMove = legs[0].BookedCtgMove;
			firstMove.EW_BookedVolume = 1m;
			firstMove.EW_VolumeUQ = "M3";
			firstMove.Container.JC_GrossWeight = 2m;
			firstMove.Container.JC_GrossWeightUQ = "KG";
			legs[0].JU_EY_RunSheet = workSheet.PK;
			foreach (var leg in legs.Skip(1))
			{
				var move = leg.BookedCtgMove;
				move.EW_BookedVolume = 1000000;
				move.EW_VolumeUQ = "CC"; // 1,000,000 CC = 1 M3
				move.Container.JC_GrossWeight = 2000;
				move.Container.JC_GrossWeightUQ = "G";
				leg.JU_EY_RunSheet = workSheet.PK;
			}

			// Act
			var invoicingSupporter = new CommonWorkSheetInvoicingSupporter(workSheet, cartage);
			// Assert
			AssertEquals("Expecting M3 as volume unit", "M3", invoicingSupporter.ActualVolumeUnit);
			AssertEquals("Expecting 3 M3 as total volume", 3m, invoicingSupporter.ActualVolume);
			AssertEquals("Expecting KG as weight unit", "KG", invoicingSupporter.ActualWeightUnit);
			AssertEquals("Expecting 6 KG as total weight", 6m, invoicingSupporter.ActualWeight);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CommonCartage>();
		}

		public CommonWorkSheetInvoicingSupporterTest()
		{
			Helper = new TestCommonWorkSheetHelper(Factory);
		}

		readonly TestCommonWorkSheetHelper Helper;
	}
}
