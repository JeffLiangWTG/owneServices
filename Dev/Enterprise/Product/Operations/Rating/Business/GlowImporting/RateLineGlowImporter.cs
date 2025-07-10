using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateLineGlowImporter : IGlowCustomImporter
	{
		/// <summary>
		/// Given a `parent` of type RateLine,
		/// Import the given childHeader/Values as a Calculator
		///
		/// This method assumes that the RateLine has already had non-child fields
		/// set up according to the importing process. In particular, the RateCalculatorType.
		/// By virtue of setting the RateCalculatorType, there may already be some RateLineItems
		/// created to represent default values of the calculator's properties.
		///
		/// Once this function returns, the childHeaders and childValues have been parsed and stored
		/// in either the calculator's properties or as additional RateLineItem.
		///
		/// Keep in mind that a RateLine has exactly one calculator, but the data for that one
		/// calculator may be expressed as multiple RateLineItems and hence multiple calls to this
		/// method. So, it may be that a XYZ calculator for a rate line is called 50 times to set
		/// up that one calculator.
		///
		/// It is also an assumption that the data being imported is "valid" and that the user
		/// interface will validate against any invalid data that may be imported.
		/// </summary>
		/// <returns>
		/// True if success. False if errors.
		/// If it errors, the whole importing process MUST halt since the provided RateLine is
		/// deleted.
		/// </returns>
		public bool ImportChildlessChildren(BusinessObject parent, INotifications logger, int rowIndex, string[] childHeaders, ImportPreviewLineDetails[] childValuesLineDetails)
		{
			var rateLine = parent as RateLine ?? throw new ArgumentException("Expected a RateLine", nameof(parent));
			if (rateLine.IsDeleted)
			{
				// No point including the childHeader and childValues since the current invocation
				// to this function has done nothing. It is the previous invocation that is most
				// helpful but we don't record it. We cant even look at the rateLine as it is deleted
				return false;
			}

			var context = new ValueObjectImportContext(parent.Factory, logger);

			// Create a list of pairs and filter out null values
			var childHeadersValues = childHeaders
				.Zip(childValuesLineDetails, (header, value) => new { Header = header, Value = value })
				.Where(x => x.Value != null)
				.ToList();

			// Filter the calculator values to be provided to the calculator-importing to only be
			// those which correspond to that calculator. This has the effect of ignoring those
			// which come from a relationship.
			var calculatorValues = childHeadersValues
				.Where(x => !x.Header.Contains(RelationshipSeparator))
				.ToDictionary(x => x.Header, x => x.Value.PreviewLineDetails);

			// Identify those values which come from some relationship rather than directly from
			// a column
			var relationshipValues = childHeadersValues
				.Where(x => x.Header.Contains(RelationshipSeparator))
				.ToDictionary(x => x.Header, x => x.Value.PreviewLineDetails);

			if (!TryGetImporterInstance(context, rateLine.RateCalculatorType, out var importer))
			{
				return false;
			}

			var factory = parent.Factory;
			var success = false;
			try
			{
				success = importer.Import(rateLine, context, calculatorValues, relationshipValues);
			}
			catch (FormatConversionException ex)
			{
				var relationshipValuesDetails = childHeadersValues
					.Where(x => x.Header.Contains(RelationshipSeparator))
					.ToDictionary(x => x.Header, x => x.Value);

				var calculatorValuesDetails = childHeadersValues
					.Where(x => !x.Header.Contains(RelationshipSeparator))
					.ToDictionary(x => x.Header, x => x.Value);

				var allCalculatorValues = calculatorValuesDetails
					.Select(kp => $"({kp.Key}:{kp.Value.PreviewLineDetails}) Column {kp.Value.ColumnIndex}") // Not translatable
					.Union(relationshipValuesDetails.Values.Select(kp => kp.PreviewLineDetails));
				var calculatorInputValues = string.Join(", ", allCalculatorValues);

				var message = ResString.GetMultilingualString("505812c6-5b9a-4c63-8f81-c4dcc287080a", "Could not set values for a calculator of type '{0}'. Values were '{1}'. Could not convert value '{2}'. Error in Row {3}", rateLine.TL_RateCalculator, calculatorInputValues, ex.FieldValueCouldNotConvert, rowIndex);

				context.Notifications.AddError(message);
			}
			finally
			{
				if (!success)
				{
					// When importing the child of a RateLine fails then we delete the whole
					// RateLine as it would otherwise be left in an inconsistent state.
					// This is in a finally-block to also execute when an unexpected exception
					// occurs.
					rateLine.Delete();

					var message = ResString.GetMultilingualString("702c981e-19b8-4230-96e5-af4e2330dbfc", "{0} importing has failed. The related incomplete {1} has been deleted as a result", nameof(RateLineItem), nameof(RateLine));
					context.Notifications.AddError(message);
				}
			}

			return success;
		}

		bool TryGetImporterInstance(ValueObjectImportContext context, CalculatorType rateCalculatorType, out RateLineItemGlowImporter instance)
		{
			if (!ImporterMapping.TryGetValue(rateCalculatorType, out var importType))
			{
				context.Notifications.AddError(ResString.GetMultilingualString("b79614e4-4d90-4ebf-9032-436f1f2d10b4", "Calculator '{0}' not supported", rateCalculatorType));
				instance = null;
				return false;
			}

			instance = Activator.CreateInstance(importType) as RateLineItemGlowImporter;
			return true;
		}

		public void ConvertCustomLine(BusinessObject parent, string code, INotifications logger, int rowIndex, string propertyName)
		{
			var rateLine = parent as RateLine ?? throw new ArgumentException("Expected a RateLine", nameof(parent));
			var orgSupplierPart = parent.Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, code));
			if (orgSupplierPart != null && propertyName == "Product_Number")
			{
				rateLine.TL_OP_ProductNumber = orgSupplierPart.PK;
			}
			else
			{
				logger.AddError($"Row {rowIndex}, No Product exists with product number: {code}");
			}
		}

		public bool IsEmptyValueAllowed(string propertyName)
		{
			return false;
		}

		readonly Dictionary<CalculatorType, Type> ImporterMapping = new Dictionary<CalculatorType, Type>
		{
			{ CalculatorType.Agency                 , typeof(AgencyCalculatorGlowImporter) },
			{ CalculatorType.Cartage                , typeof(CartageCalculatorGlowImporter) },
			{ CalculatorType.CartageZoneDistance    , typeof(CartageZoneDistanceCalculatorGlowImporter) },
			{ CalculatorType.Combined               , typeof(CombinedCalculatorGlowImporter) },
			{ CalculatorType.CompanyTariffBased     , typeof(CompanyTariffBasedCalculatorGlowImporter) },
			{ CalculatorType.CostBased              , typeof(CostBasedCalculatorGlowImporter) },
			{ CalculatorType.DisbursementInterest   , typeof(DisbursementInterestCalculatorGlowImporter) },
			{ CalculatorType.Equalization           , typeof(EqualizationCalculatorGlowImporter) },
			{ CalculatorType.ExcludeCompanyTariffs  , typeof(ExcludeCompanyTariffsCalculatorGlowImporter) },
			{ CalculatorType.FirstPlusAdditional    , typeof(FirstPlusAdditionalCalculatorGlowImporter) },
			{ CalculatorType.Flat                   , typeof(FlatCalculatorGlowImporter) },
			{ CalculatorType.FlatPlusPerUnit        , typeof(FlatPlusPerUnitCalculatorGlowImporter) },
			{ CalculatorType.FreightInclusive       , typeof(FreightInclusiveCalculatorGlowImporter) },
			{ CalculatorType.HighestCharge          , typeof(HighestChargeCalculatorGlowImporter) },
			{ CalculatorType.HighestRate            , typeof(HighestRateCalculatorGlowImporter) },
			{ CalculatorType.HousebillReleaseType   , typeof(HousebillReleaseTypeCalculatorGlowImporter) },
			{ CalculatorType.Minimum                , typeof(MinimumCalculatorGlowImporter) },
			{ CalculatorType.MinimumOrPerUnit       , typeof(MinimumOrPerUnitCalculatorGlowImporter) },
			{ CalculatorType.Note                   , typeof(NoteCalculatorGlowImporter) },
			{ CalculatorType.PackageCount           , typeof(PackageCountCalculatorGlowImporter) },
			{ CalculatorType.Percentage             , typeof(PercentageCalculatorGlowImporter) },
			{ CalculatorType.PercentageBreaks       , typeof(PercentageBreaksCalculatorGlowImporter) },
			{ CalculatorType.ProfitShareRebate      , typeof(ProfitShareRebateCalculatorGlowImporter) },
			{ CalculatorType.SplitMonthBilling      , typeof(SplitMonthBillingCalculatorGlowImporter) },
			{ CalculatorType.Time                   , typeof(TimeCalculatorGlowImporter) },
			{ CalculatorType.Unit                   , typeof(UnitCalculatorGlowImporter) },
			{ CalculatorType.ValueRange             , typeof(ValueRangeCalculatorGlowImporter) },
			{ CalculatorType.WarehouseLocationType  , typeof(WarehouseLocationTypeCalculatorGlowImporter) },
			{ CalculatorType.WarehousePack          , typeof(WarehousePackCalculatorGlowImporter) },
		};

		const string RelationshipSeparator = ".";

		bool IGlowCustomImporter.ShouldCustomizeChildrenImport => true;
	}
}
