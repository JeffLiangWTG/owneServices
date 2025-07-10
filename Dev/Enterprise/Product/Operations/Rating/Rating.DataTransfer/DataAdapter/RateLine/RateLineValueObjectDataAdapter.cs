using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer.Calculators;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public class RateLineValueObjectDataAdapter : ValueObjectDataAdapter<RateLine, Xsd.RateLine>
	{
		public RateLineValueObjectDataAdapter(RateEntry rateEntry)
		{
			if (rateEntry == null)
			{
				throw new ArgumentNullException(nameof(rateEntry));
			}

			this.rateEntry = rateEntry;
		}

		readonly RateEntry rateEntry;

		#region Properties

		public override string RootCollectionElementName
		{
			get { return "RateLines"; }
		}

		public override string RootElementName
		{
			get { return "RateLine"; }
		}

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.SingleRateLineSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return RatingXmlSchemaDefinitions.Instance.RatesSchema; }
		}

		protected override bool AllowDifferentImportContextFactory
		{
			get { return true; }
		}

		#endregion

		#region Helpers

		protected override RateLine NewBusinessObject(Xsd.RateLine rateLineXSD, IValueObjectImportContext context)
		{
			return rateEntry.RateLines.AddNew();
		}

		protected override RateLine FindBusinessObject(Xsd.RateLine rateLineXSD, IValueObjectImportContext context)
		{
			RateLine rateLine = null;

			if (rateLineXSD.IsSpecified && !rateLineXSD.ChargeCode.IsEmpty)
			{
				AccChargeCode chargeCode = RateImportHelper.Instance.FindChargeCode(rateLineXSD.ChargeCode, context);

				if (chargeCode != null)
				{
					RateLine[] candidateRateLines = (RateLine[])rateEntry.RateLines.Find(new ZQuery(RateLinesSchema.TL_AC, chargeCode.PK));

					if (candidateRateLines.Length == 1)
					{
						rateLine = candidateRateLines[0];
					}
				}
			}

			return rateLine;
		}

		#endregion

		#region Import

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			notifications.Notify(new BusinessObjectCreatedOrUpdatedNotification(bizObj) { UpdateRecordCountOnlyWithoutMessage = true });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected override void ImportFromValueObjectCore(RateLine rateLine, Xsd.RateLine rateLineXSD, IValueObjectImportContext context)
		{
			if (rateLineXSD != null)
			{
				AccChargeCode chargeCode = RateImportHelper.Instance.FindChargeCode(rateLineXSD.ChargeCode, context);

				if (rateLine.TL_TI.IsEmpty)
				{
					rateLine.TL_TI = rateEntry.PK;
				}

				if (chargeCode != null)
				{
					try
					{
						rateLine.LockCalculator = true;
						rateLine.TL_AC = chargeCode.PK;
					}
					finally
					{
						rateLine.LockCalculator = false;
					}

					var conversionFactor = rateLine.ConversionFactor;

					context.SetPropertyInfoValueIfValueNotEmpty(rateLine.TL_RateDescInfo, chargeCode.AC_Desc);
					context.SetPropertyInfoValueIfValueNotEmpty(rateLine.TL_ConversionFactorInfo, conversionFactor.Factor.ToString("F", Culture.CurrentCompanyCountryCulture));
					context.SetPropertyInfoValueIfValueNotEmpty(rateLine.TL_FactorNumeratorInfo, conversionFactor.NumeratorUnit);
					context.SetPropertyInfoValueIfValueNotEmpty(rateLine.TL_FactorDenominatorInfo, conversionFactor.DenominatorUnit);
					context.SetPropertyInfoValue(rateLine.TL_RX_NKCurrencyInfo, rateLineXSD.Currency, ForeignKeyType.CurrencyNK, rateLineXSD.CurrencySpecified);
					context.SetPropertyInfoValue(rateLine.TL_ConditionInfo, rateLineXSD.Condition.Type, rateLineXSD.Condition.TypeSpecified);
					context.SetPropertyInfoValue(rateLine.TL_ConditionalExpressionInfo, rateLineXSD.Condition.Value, rateLineXSD.Condition.ValueSpecified);

					if (rateLineXSD.ActualPercentageSpecified)
					{
						int actualPercentageXSD = rateLineXSD.ActualPercentage;
						rateLine.TL_ActualPercentage = (ZByte)actualPercentageXSD;
					}

					if (rateLineXSD.UseActualSpecified)
					{
						context.SetPropertyInfoValue(rateLine.UseOnlyActualWeightMeasureInfo, rateLineXSD.UseActual.ToString(), rateLineXSD.UseActualSpecified);
					}

					context.SetPropertyInfoValue(rateLine.TL_RoundingInfo, rateLineXSD.Rounding, rateLineXSD.RoundingSpecified);

					if (rateLineXSD.RateCalculator.ItemSpecified)
					{
						IRateCalculatorGenerator generator = CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(rateLineXSD.RateCalculator.Item);

						if (generator != null)
						{
							rateLine.TL_RateCalculator = generator.CalculatorType;
							rateLine.RateLineItems.RemoveAndDeleteAll();
							generator.ImportFromValueObject(rateLine, rateLineXSD.RateCalculator.Item, context);
						}
					}

					if (rateLine.RequiresWeightVolume() || !string.IsNullOrEmpty(rateLine.Calculator.DefaultWeightVolume))
					{
						context.SetPropertyInfoValue(rateLine.TL_WeightVolumeInfo, rateLineXSD.Units, rateLineXSD.UnitsSpecified);
						context.SetPropertyInfoValueIfValueNotEmpty(rateLine.TL_WeightVolumeMultipleInfo, rateLineXSD.UnitsMultiple.ToString());
					}
					else
					{
						context.SetPropertyInfoValue(rateLine.TL_WeightVolumeInfo, ZString.Empty);
						context.SetPropertyInfoValueIfValueNotEmpty(rateLine.TL_WeightVolumeMultipleInfo, ZDecimal.Zero.ToString());
					}

					if (rateLine.TL_WeightVolume == RatingConstants.Units.CN)
					{
						context.SetPropertyInfoValue(rateLine.TL_ContainerOwnershipInfo, rateLineXSD.ContainerOwnership);
					}

					NoteValueObjectDataAdapter noteAdapter = new NoteValueObjectDataAdapter();
					noteAdapter.ImportNotesAndAttachToBusinessObjectNotes(rateLine.Notes, rateLineXSD.Notes, context);
				}
				else
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("f1c2e403-4dd9-43d1-b10b-70f7bac3b7ce", "Invalid Charge Code ({0})", rateLineXSD.ChargeCode)));
				}
			}
		}

		#endregion

		#region Export

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected override void ExportToValueObjectCore(RateLine rateLine, Xsd.RateLine rateLineXSD, IValueObjectExportContext context)
		{
			if (rateLine != null && rateLine.ChargeCode != null)
			{
				rateLineXSD.ChargeCode = rateLine.ChargeCode.AC_Code;
				rateLineXSD.Units = rateLine.TL_WeightVolume;
				rateLineXSD.Currency = rateLine.TL_RX_NKCurrency;
				rateLineXSD.Rounding = rateLine.TL_Rounding;
				rateLineXSD.Description = rateLine.ChargeCode.AC_Desc;

				if (rateLine.TL_WeightVolume == RatingConstants.Units.CN)
				{
					rateLineXSD.ContainerOwnership = rateLine.TL_ContainerOwnership;
				}

				if (!rateLine.TL_Condition.IsEmpty && !(rateLine.TL_Condition == RateLineConditions.UserDefined && rateLine.TL_ConditionalExpression.IsEmpty))
				{
					rateLineXSD.Condition.Type = rateLine.TL_Condition;
					rateLineXSD.Condition.Value = rateLine.TL_ConditionalExpression;
				}

				rateLineXSD.ActualPercentage = rateLine.TL_ActualPercentage;

				if (!rateLine.TL_WeightVolumeMultiple.IsEmpty)
				{
					rateLineXSD.UnitsMultiple = rateLine.TL_WeightVolumeMultiple;
				}

				if (!rateLine.UseOnlyActualWeightMeasure.IsEmpty)
				{
					rateLineXSD.UseActual = rateLine.UseOnlyActualWeightMeasure ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				}

				IRateCalculatorGenerator generator = CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(rateLine.TL_RateCalculator);
				if (generator != null)
				{
					rateLineXSD.RateCalculator.Item = (Xsd.RateCalculator)Activator.CreateInstance(generator.CalculatorSchemaType, Array.Empty<object>());
					generator.ExportToValueObject(rateLine, rateLineXSD.RateCalculator.Item, context);
				}

				rateLineXSD.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(rateLine.Notes, context);
			}
		}

		#endregion
	}
}

