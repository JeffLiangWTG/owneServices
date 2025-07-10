namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;

	public partial class MAFPlugInSupportDeclarationWrapper
	{
		class MAFCommodityEntryLineWrapper : IMAFCommodity
		{
			public MAFCommodityEntryLineWrapper(CusEntryLine entryLine)
			{
				this.entryLine = entryLine;
				invoiceLine = entryLine.RandomLine;
			}

			#region Implementation of IMAFCommodity

			public ZString GoodsType
			{
				get { return invoiceLine.EffectiveMAF_GoodsType; }
			}

			public ZString GoodsDescription
			{
				get { return entryLine.CL_Description; }
			}

			public IEnumerable<ZString> TariffCodes
			{
				get { yield return invoiceLine.JI_Tariff.Replace(".", ""); }
			}

			public IEnumerable<IMAFMeasurement> GoodsMeasurements
			{
				get
				{
					var goodsMeasurements = new List<IMAFMeasurement>();
					foreach (JobComInvoiceLine line in entryLine.InvoiceLines)
					{
						var unit = line.EffectiveMAF_MeasurementUQ;
						if (!unit.IsEmpty)
						{
							var measurement = goodsMeasurements.Find(m => m.MeasurementUQ == unit) as MAFMeasurement;
							if (measurement == null)
							{
								measurement = new MAFMeasurement { MeasurementUQ = unit };
								goodsMeasurements.Add(measurement);
							}
							measurement.MeasurementValue += line.EffectiveMAF_MeasurementValue;
						}
					}

					if (goodsMeasurements.Count == 0 && invoiceLine.JI_CustomsUnitQty.IsEmpty)
					{
						goodsMeasurements.Add(new MAFMeasurement { MeasurementValue = 0, MeasurementUQ = MeasurementUQList.Codes.kilograms });
					}
					return goodsMeasurements;
				}
			}

			public bool? IsNew
			{
				get
				{
					return YesNoUnknownList.GetValueForCode(invoiceLine.JI_MAF_NewGoods)
						?? invoiceLine.ProhibitedCodes.GetElementWithThisCode(ProhibitedCodeList.Codes.MpiUsedGoods) == null;
				}
			}

			public ZInt MergedLineNumber
			{
				get { return entryLine.CL_LineNumber; }
			}

			public bool RequirePermits
			{
				get
				{
					var declaration = entryLine.Declaration;
					return entryLine.PermitCodes.Count == 0
						   && NZCTariffsPermitsApplyTo.Load(entryLine.Factory, entryLine.CL_AdValoremTariff, declaration.IsImport, declaration.IsExport) != null;
				}
			}

			public IMAFMeasurement Quantity
			{
				get { return new MAFMeasurement { MeasurementValue = entryLine.CustomsQuantity, MeasurementUQ = entryLine.CustomsUnitQty }; }
			}

			public IMAFMeasurement Measure
			{
				get
				{
					var measure = new MAFMeasurement();
					if (entryLine.EffectiveGrossWeight.Amount != 0)
					{
						measure.MeasurementValue = entryLine.EffectiveGrossWeight.Amount;
						measure.MeasurementUQ = entryLine.EffectiveGrossWeight.Unit;
					}
					else if (entryLine.EffectiveVolume.Amount != 0)
					{
						measure.MeasurementValue = entryLine.EffectiveVolume.Amount;
						measure.MeasurementUQ = entryLine.EffectiveVolume.Unit;
					}
					return measure;
				}
			}

			#endregion

			readonly JobComInvoiceLine invoiceLine;
			readonly CusEntryLine entryLine;
		}
	}
}
