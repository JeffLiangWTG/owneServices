using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class TestExportAWBRateLineValidation : BusinessObjectValidationTestCase
	{
		public void TestCheckER_NoOfPiecesOrRCP()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();
			ExportAWBRateLineValidation validation = new ExportAWBRateLineValidation(rateLine);

			rateLine.ER_NoOfPiecesOrRCP = "1234";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(false, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarnings());

			rateLine.ER_NoOfPiecesOrRCP = "123A";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be alphabetical characters only."));
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be 3 characters in length."));

			rateLine.ER_NoOfPiecesOrRCP = "2.3";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be alphabetical characters only."));

			rateLine.ER_NoOfPiecesOrRCP = "QQ";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be 3 characters in length."));

			rateLine.ER_NoOfPiecesOrRCP = "QQWE";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be 3 characters in length."));

			rateLine.ER_NoOfPiecesOrRCP = "JED";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(false, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarnings());
		}

		public void TestCheckER_GrossWeight_WithOverriddenDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_AWBWeight = collection.AddNew();
			defaultNumberOfDecimals_AWBWeight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_AWBWeight.UnitOfMeasure = Core.Constants.Weight.Pounds;
			defaultNumberOfDecimals_AWBWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_AWBWeight.RoundingMode = RoundingModes.Up;
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var header = Factory.New<ConsolExportAWBHeader>();

			var rateLineSavedBeforeNewRoundingRule1 = Factory.New<ExportAWBRateLineForTesting>();
			rateLineSavedBeforeNewRoundingRule1.ER_WeightInLBsOrKGs = "L";
			rateLineSavedBeforeNewRoundingRule1.ER_GrossWeight = 1.02;
			rateLineSavedBeforeNewRoundingRule1.ER_EH = header.PK;

			var rateLineSavedBeforeNewRoundingRule2 = Factory.New<ExportAWBRateLineForTesting>();
			rateLineSavedBeforeNewRoundingRule2.ER_GrossWeight = 1.02;
			rateLineSavedBeforeNewRoundingRule1.ER_WeightInLBsOrKGs = "K";
			rateLineSavedBeforeNewRoundingRule2.ER_EH = header.PK;
			Factory.Save();

			Action<ExportAWBRateLineForTesting, bool> assertGrossWeightValidation = (existingRateLine, messageErrorExpected) =>
			{
				var rateLine = Factory.Load<ExportAWBRateLine>(existingRateLine.PK);

				var validation = new ExportAWBRateLineValidation(rateLine);
				validation.ValidateER_GrossWeight();

				string expectedMessage = string.Format(
					"A new validation has been added to check that gross weight has only 1 decimal value.\r\n" +
					"The overridden weight is currently {0} - please re-enter the weight with only 1 decimal.", rateLine.ER_GrossWeight);

				AssertEquals(messageErrorExpected, rateLine.ER_GrossWeightInfo.GetMessageErrors().Contains(expectedMessage));
			};

			assertGrossWeightValidation(rateLineSavedBeforeNewRoundingRule1, false);
			assertGrossWeightValidation(rateLineSavedBeforeNewRoundingRule2, true);
		}

		public void TestSetErEh_ShouldNotLoadErEh()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			var primaryHeader = Factory.New<ConsolExportAWBHeader>();
			rateLine.ER_EH = primaryHeader.PK;
			var newHeader = Factory.New<ConsolExportAWBHeader>();
			BusinessObjectFactory.StartLogging();
			rateLine.ER_EH = newHeader.PK;
			var loadLog = BusinessObjectFactory.DebugLog;
			BusinessObjectFactory.StopLogging();

			Assert("Should not load ExportAWBHeader", !loadLog.Contains("LOAD Enterprise.Freight.Forwarding.AWB.Business.ExportAWBHeader"));
		}

		#region Implementation

		class ExportAWBRateLineForTesting : ExportAWBRateLine
		{
			public ExportAWBRateLineForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZDecimal ER_GrossWeight
			{
				get { return base.ER_GrossWeight; }
				set
				{
					SetPropertyValue(ER_GrossWeightInfo, value);
				}
			}

			public override ZDecimal ER_ChargeableWeight
			{
				get { return base.ER_ChargeableWeight; }
				set
				{
					SetPropertyValue(ER_ChargeableWeightInfo, value);
				}
			}
		}

		#endregion
	}
}
