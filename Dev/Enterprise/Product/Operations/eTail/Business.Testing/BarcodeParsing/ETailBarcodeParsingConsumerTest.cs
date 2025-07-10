using System;
using CargoWise.Types;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.BarcodeParsingEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(ETailBarcodeParsingConsumer))]
	class ETailBarcodeParsingConsumerTest : BarcodeParsingConsumerTestCase
	{
		#region Captions

		protected override string ExpectedBuyerCaption => "eTailer";
		protected override string ExpectedRelatedEntityCaption => null;
		protected override string ExpectedSupplierCaption => "Depot";

		#endregion

		#region Collections

		protected override Type ExpectedTypeOfBuyers => typeof(OrganisationsFindBoxCollection); // Dmitry B now owns eTail but is unsure who the eTailer is. this will therefore possibly change once he gets back to us, or even after checkin.
		protected override Type ExpectedTypeOfSuppliers => typeof(PackDepotCollection);

		#endregion

		#region Flags

		protected override bool ExpectedIsBuyerAvailable => true;
		protected override bool ExpectedIsRelatedEntityAvailable => false;
		protected override bool ExpectedIsSupplierAvailable => true;

		#endregion

		#region TestGS1TargetFieldsToDefault

		protected override ZString[] ExpectedGS1TargetFieldsToDefault => Array.Empty<ZString>();

		#endregion

		#region TestRelatedEntityRequirements

		protected override RelatedEntityRequirements ExpectedRelatedEntityRequirements => RelatedEntityRequirements.None;

		#endregion

		#region TestTargetFields

		protected override CodeDescriptionPairList ExpectedTargetFields => new ETailTargetFields();

		#endregion

		#region TestExpectedValidFormats

		public void TestGetValidFieldFormatsForTargetField()
		{
			var consumer = (IBarcodeParsingConsumer)new ETailBarcodeParsingConsumer(Factory);
			const bool IsGS1 = true;

			var allFormatTypes = Enum.GetValues(typeof(FormatType));
			foreach (CodeDescriptionPair targetField in new ETailTargetFields())
			{
				AssertContainsExactElementsInAnyOrder(allFormatTypes, consumer.GetValidFieldFormatsForTargetField(IsGS1, targetField.Code));
				AssertContainsExactElementsInAnyOrder(allFormatTypes, consumer.GetValidFieldFormatsForTargetField(!IsGS1, targetField.Code));
			}
		}

		#endregion

		#region Implementation

		protected override ZString ModuleCode => BarcodeModuleTypes.Codes.ETail;

		#endregion
	}
}
