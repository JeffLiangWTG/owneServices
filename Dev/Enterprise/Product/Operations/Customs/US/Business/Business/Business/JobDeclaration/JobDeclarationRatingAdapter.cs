using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationRatingAdapter<T> : BaseJobDeclarationRatingAdapter<T>, IAutoRatingCustomsInfo
		where T : JobDeclaration
	{
		public JobDeclarationRatingAdapter(T parent) : base(parent)
		{
			Argument.NotNull(parent, "parent");
			this.parent = parent;
		}

		readonly T parent;

		#region IAutoRatingCustomsInfo Members

		EntryInfoCollection IAutoRatingCustomsInfo.Entries
		{
			get
			{
				var result = new EntryInfoCollection();

				var query = new ZQuery(CusEntryHeaderSchema.CH_MessageType, new string[]
				{
					CusEntryHeaderMessageTypeList.Codes.Export, CusEntryHeaderMessageTypeList.Codes.EntrySummary
				});

				var activeEntryHeaders = parent.ActiveEntryHeaders.Find(query);

				foreach (CusEntryHeader header in activeEntryHeaders)
				{
					//Only top level entry lines & invoice lines
					result.AddNew(header.EntryLines.Count(), header.TotalNonSecondaryInvoiceLinesCount, header.CustomsValue);
				}

				return result;
			}
		}

		ZString IAutoRatingCustomsInfo.MessageSubType
		{
			get { return parent.US_EntryType; }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices
		{
			get
			{
				var result = new InvoiceInfoCollection();

				foreach (JobComInvoiceHeader invoice in parent.Invoices)
				{
					result.AddNew(new Money(invoice.JZ_InvoiceAmount, invoice.Invoice_Currency), invoice.JobComInvoiceLines.TotalNonSecondaryInvoiceLines, invoice.Supplier, invoice.TotalNonSecondaryEntrySummaryLinesCount);
				}

				return result;
			}
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get { return 0; }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerShipment
		{
			get
			{
				var result = new InvoiceInfoCollection();

				var tariffInfos = Parent.InvoiceLines.OfType<JobComInvoiceLine>()
					.SelectMany(x => new[]
					{
						x.JI_Tariff,
						x.US_SupTariff,
						x.US_SupAdditionalTariff1.Replace(".", ""),
						x.US_SupAdditionalTariff2.Replace(".", ""),
						x.US_SupAdditionalTariff3.Replace(".", ""),
						x.US_SupAdditionalTariff4.Replace(".", ""),
						x.US_SupAdditionalTariff5.Replace(".", "")
					})
					.Where(t => !string.IsNullOrWhiteSpace(t))
					.ToArray();

				result.AddNew(Money.Empty, tariffInfos.Distinct().Count(), null, tariffInfos.Length);

				return result;
			}
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerInvoice
		{
			get
			{
				var result = new InvoiceInfoCollection();

				foreach (JobComInvoiceHeader invoice in Parent.Invoices)
				{
					var tariffInfos = invoice.InvoiceLines.OfType<JobComInvoiceLine>()
						.SelectMany(x => new[]
						{
							x.JI_Tariff,
							x.US_SupTariff,
							x.US_SupAdditionalTariff1.Replace(".", ""),
							x.US_SupAdditionalTariff2.Replace(".", ""),
							x.US_SupAdditionalTariff3.Replace(".", ""),
							x.US_SupAdditionalTariff4.Replace(".", ""),
							x.US_SupAdditionalTariff5.Replace(".", "")
						})
						.Where(t => !string.IsNullOrWhiteSpace(t));

					var entryLines = invoice.InvoiceLines.OfType<JobComInvoiceLine>()
						.Count(x => !string.IsNullOrWhiteSpace(x.JI_Tariff));

					result.AddNew(Money.Empty, tariffInfos.Distinct().Count(), null, entryLines);
				}

				return result;
			}
		}

		protected override void AddAdditionalRatingMoneyAmounts(MoneyType amounts)
		{
			base.AddAdditionalRatingMoneyAmounts(amounts);

			if (parent.IsImport)
			{
				if (parent.US_BondType == BondTypeList.Codes.SingleTransactionBond)
				{
					amounts.Add(MoneyType.ValueType.SingleTransactionBondAmount, new Money(parent.US_BondAmount, JobDeclaration.GetLocalCurrency()));
				}
				if (parent.Shipment is ForwardingShipment shipment)
				{
					amounts.Add(MoneyType.ValueType.InsuranceValue, new Money(shipment.JS_InsuranceValue, shipment.InsuranceCurrency));
				}
				else
				{
					amounts.Add(MoneyType.ValueType.InsuranceValue, new Money(parent.JE_InsuranceValue, parent.InsuranceCurrency));
				}
			}
		}

		protected override void AddAdditionalRatingMeasurements(RateableMeasureSet measurements)
		{
			base.AddAdditionalRatingMeasurements(measurements);

			var additionalMeasureTypes = new[]
			{
				MeasureType.FDALine, MeasureType.PNFDALine, MeasureType.FCCLine, MeasureType.DOTLine,
				MeasureType.LaceyLine, MeasureType.NMFS370, MeasureType.NMFSAMR, MeasureType.NMFSCOA,
				MeasureType.NMFSSIM, MeasureType.AMS, MeasureType.APHIS, MeasureType.ATF, MeasureType.DDTC,
				MeasureType.FSIS, MeasureType.FWS, MeasureType.NMFSHMS, MeasureType.PST, MeasureType.HFC,
				MeasureType.TTB, MeasureType.VNE, MeasureType.ODS, MeasureType.TSCA,
				MeasureType.OMCLine, MeasureType.CPSCLine, MeasureType.DEALine, MeasureType.TCC, MeasureType.NOP,
				MeasureType.FDADisclaim, MeasureType.OMCDisclaim, MeasureType.CPSCDisclaim, MeasureType.DEADisclaim,
				MeasureType.FCCDisclaim, MeasureType.DOTDisclaim, MeasureType.LaceyDisclaim, MeasureType.NMFS370Disclaim,
				MeasureType.NMFSAMRDisclaim, MeasureType.NMFSHMSDisclaim, MeasureType.AMSDisclaim, MeasureType.AMSNOPDisclaim,
				MeasureType.APHISDisclaim, MeasureType.FSISDisclaim, MeasureType.FWSDisclaim, MeasureType.PSTDisclaim,
				MeasureType.TTBDisclaim, MeasureType.VNEDisclaim, MeasureType.ODSDisclaim, MeasureType.TSCADisclaim,
				MeasureType.HFCDisclaim, MeasureType.DeliveryOrders, MeasureType.SteelLicenses, MeasureType.SG_TPLCertificate,
				MeasureType.CA_NAFTA_TPLCertificate, MeasureType.MX_NAFTA_TPLCertificate, MeasureType.BeefExportCertificate,
				MeasureType.DiamondCertificate, MeasureType.ATPDEACertificate, MeasureType.AU_FTA_ExportCertificate,
				MeasureType.MXCementLicense, MeasureType.CAFTA_TPLCertificate, MeasureType.ALBCertificate, MeasureType.CottonShirtingFabricLicense,
				MeasureType.HaitiEarnedAllowance, MeasureType.AgriculturalLicense, MeasureType.CAExportSugarCertificate,
				MeasureType.WoolLicense, MeasureType.CBTPACertificate, MeasureType.AGOATextileProvisionNumber, MeasureType.OtherNonStandardVisa,
				MeasureType.USDASugarCertificate, MeasureType.OrganicProductExemptionCertificate, MeasureType.AMSCertificateOfExemption,
				MeasureType.DominicanRepublicEarnedAllowanceProgramCertificate, MeasureType.MexicanSugarExportLicense,
				MeasureType.GeneralNote15cWaiverCertificate, MeasureType.AluminumLicenses, MeasureType.CanadianUSMCA_TPLCertificate,
				MeasureType.MexicanUSMCA_TPLCertificate, MeasureType.ArgentineWhiteGrapeJuiceConcentrateExportLicense, MeasureType.KRExportSteelCertificate,
				MeasureType.VISANumbers, MeasureType.PGALines, MeasureType.PGADisclaims, MeasureType.HTS9902Line, MeasureType.HTS9903Line
			};
			var additionalMeasureTypeCounts = additionalMeasureTypes.ToDictionary(x => x, x => 0);
			var declaredCount = 0;
			var disclaimedCount = 0;
			if (parent.IsImport)
			{
				foreach (JobComInvoiceLine invoiceLine in parent.InvoiceLines)
				{
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_TTBInd, MeasureType.TTB, MeasureType.TTBDisclaim, () => invoiceLine.TTBLines.Count, () =>
					{
						invoiceLine.TTBLines.OfType<TTBLine>().ForEach(x => additionalMeasureTypeCounts[MeasureType.TCC] += x.COLAAndCertificates.Count);
					});

					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_FDAIndicator, MeasureType.FDALine, MeasureType.FDADisclaim, () => parent.IsACEFDARelevant ? invoiceLine.ACE_FDALines.Count : invoiceLine.FDAs.Count, () =>
					{
						additionalMeasureTypeCounts[MeasureType.PNFDALine] += invoiceLine.ACE_FDALines.OfType<ACEFDA>().Count(x => x.IsPriorNoticeForAutoRating);
					});

					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_NMFSSIMPInd, MeasureType.NMFSSIM, null, () => invoiceLine.NMFSSIMPLines.Count());
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_NMFSCOAInd, MeasureType.NMFSCOA, null, () => invoiceLine.NMFSCOALines.Count());
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_ATFInd, MeasureType.ATF, null, () => invoiceLine.ATFLines.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_DDTCInd, MeasureType.DDTC, null, () => invoiceLine.ShouldDeclareACEDDTCData ? 1 : 0);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_ODSInd, MeasureType.ODS, MeasureType.ODSDisclaim, () => invoiceLine.IsODSIndBeDeclared ? 1 : 0);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_TSCAInd, MeasureType.TSCA, MeasureType.TSCADisclaim, () => invoiceLine.IsTSCAIndBeDeclared ? 1 : 0);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_NHTSAIndicator, MeasureType.DOTLine, MeasureType.DOTDisclaim, () => parent.IsACECargoCertificationMode ? invoiceLine.NHTSALines.Count : invoiceLine.DOTs.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_FWSInd, MeasureType.FWS, MeasureType.FWSDisclaim, () => invoiceLine.FWSHeaders.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_FSISInd, MeasureType.FSIS, MeasureType.FSISDisclaim, () => invoiceLine.FSISLines.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_DEAInd, MeasureType.DEALine, MeasureType.DEADisclaim, () => invoiceLine.DEAHeaders.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_FCCIndicator, MeasureType.FCCLine, MeasureType.FCCDisclaim, () => invoiceLine.FCCs.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_OMCInd, MeasureType.OMCLine, MeasureType.OMCDisclaim, () => invoiceLine.OMCHeaders.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_CPSCInd, MeasureType.CPSCLine, MeasureType.CPSCDisclaim, () => invoiceLine.CPSCHeaders.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_APHISInd, MeasureType.APHIS, MeasureType.APHISDisclaim, () => invoiceLine.APHISHeaders.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_LaceyIndicator, MeasureType.LaceyLine, MeasureType.LaceyDisclaim, () => invoiceLine.LaceyActLines.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_VNEInd, MeasureType.VNE, MeasureType.VNEDisclaim, () => invoiceLine.VehicleLines.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_PSTIndicator, MeasureType.PST, MeasureType.PSTDisclaim, () => invoiceLine.PSTLines.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_HFCInd, MeasureType.HFC, MeasureType.HFCDisclaim, () => invoiceLine.USHFCHeaders.Count);
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_NMFS370Ind, MeasureType.NMFS370, MeasureType.NMFS370Disclaim, () => invoiceLine.NMFS370Lines.Count());
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_NMFSAMRInd, MeasureType.NMFSAMR, MeasureType.NMFSAMRDisclaim, () => invoiceLine.NMFSAMRLines.Count());
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_NMFSHMSInd, MeasureType.NMFSHMS, MeasureType.NMFSHMSDisclaim, () => invoiceLine.NMFSHMSLines.Count());
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_AMSInd, MeasureType.AMS, MeasureType.AMSDisclaim, () => invoiceLine.AMSLines.Cast<AMS>().Count(x => !x.IsNOPProgram));
					AddAdditionalRatingMeasurementsForPGA(invoiceLine.US_NOPInd, MeasureType.NOP, MeasureType.AMSNOPDisclaim, () => invoiceLine.AMSLines.Cast<AMS>().Count(x => x.IsNOPProgram));
					AddAdditionalRatingMeasurements(MeasureType.HTS9902Line, () => invoiceLine.ProvProgTariff.StartsWith("9902") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9903Line, () => invoiceLine.ProvProgTariff.StartsWith("9903") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9902Line, () => invoiceLine.JI_Tariff.StartsWith("9902") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9903Line, () => invoiceLine.JI_Tariff.StartsWith("9903") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9902Line, () => invoiceLine.US_SupAdditionalTariff1.StartsWith("9902") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9903Line, () => invoiceLine.US_SupAdditionalTariff1.StartsWith("9903") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9902Line, () => invoiceLine.US_SupAdditionalTariff2.StartsWith("9902") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9903Line, () => invoiceLine.US_SupAdditionalTariff2.StartsWith("9903") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9902Line, () => invoiceLine.US_SupAdditionalTariff3.StartsWith("9902") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9903Line, () => invoiceLine.US_SupAdditionalTariff3.StartsWith("9903") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9902Line, () => invoiceLine.US_SupAdditionalTariff4.StartsWith("9902") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9903Line, () => invoiceLine.US_SupAdditionalTariff4.StartsWith("9903") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9902Line, () => invoiceLine.US_SupAdditionalTariff5.StartsWith("9902") ? 1 : 0);
					AddAdditionalRatingMeasurements(MeasureType.HTS9903Line, () => invoiceLine.US_SupAdditionalTariff5.StartsWith("9903") ? 1 : 0);
				}

				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.SteelLicenses, LicencePermitTypeList.Codes._01);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.SG_TPLCertificate, LicencePermitTypeList.Codes._02);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.CA_NAFTA_TPLCertificate, LicencePermitTypeList.Codes._03);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.MX_NAFTA_TPLCertificate, LicencePermitTypeList.Codes._04);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.BeefExportCertificate, LicencePermitTypeList.Codes._05);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.DiamondCertificate, LicencePermitTypeList.Codes._06);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.ATPDEACertificate, LicencePermitTypeList.Codes._07);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.AU_FTA_ExportCertificate, LicencePermitTypeList.Codes._08);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.MXCementLicense, LicencePermitTypeList.Codes._09);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.CAFTA_TPLCertificate, LicencePermitTypeList.Codes._10);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.ALBCertificate, LicencePermitTypeList.Codes._11);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.CottonShirtingFabricLicense, LicencePermitTypeList.Codes._12);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.HaitiEarnedAllowance, LicencePermitTypeList.Codes._13);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.AgriculturalLicense, LicencePermitTypeList.Codes._14);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.CAExportSugarCertificate, LicencePermitTypeList.Codes._16);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.WoolLicense, LicencePermitTypeList.Codes._17);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.CBTPACertificate, LicencePermitTypeList.Codes._18);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.AGOATextileProvisionNumber, LicencePermitTypeList.Codes._19);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.OtherNonStandardVisa, LicencePermitTypeList.Codes._20);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.USDASugarCertificate, LicencePermitTypeList.Codes._21);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.OrganicProductExemptionCertificate, LicencePermitTypeList.Codes._22);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.AMSCertificateOfExemption, LicencePermitTypeList.Codes._23);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.DominicanRepublicEarnedAllowanceProgramCertificate, LicencePermitTypeList.Codes._25);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.MexicanSugarExportLicense, LicencePermitTypeList.Codes._26);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.GeneralNote15cWaiverCertificate, LicencePermitTypeList.Codes._27);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.AluminumLicenses, LicencePermitTypeList.Codes._28);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.CanadianUSMCA_TPLCertificate, LicencePermitTypeList.Codes._29);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.MexicanUSMCA_TPLCertificate, LicencePermitTypeList.Codes._30);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.ArgentineWhiteGrapeJuiceConcentrateExportLicense, LicencePermitTypeList.Codes._31);
				AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType.KRExportSteelCertificate, LicencePermitTypeList.Codes.KR);

				AddAdditionalRatingMeasurements(MeasureType.VISANumbers, () => parent.InvoiceLines
					.OfType<JobComInvoiceLine>()
					.Where(x => !string.IsNullOrEmpty(x.US_VisaNo))
					.Select(x => x.US_VisaNo)
					.Distinct()
					.Count());

				AddAdditionalRatingMeasurements(MeasureType.DeliveryOrders, () => parent.DeliveryOrderHeaders.Count);
				additionalMeasureTypeCounts[MeasureType.PGALines] = declaredCount;
				additionalMeasureTypeCounts[MeasureType.PGADisclaims] = disclaimedCount;
			}

			additionalMeasureTypeCounts
				.Where(x => x.Value > 0)
				.ForEach(x => measurements.SetQuantity(x.Key, x.Value, string.Empty));

			void AddAdditionalRatingMeasurementsForPGA(ZString indicator, MeasureType declaredMeasureType, MeasureType? disclaimedMeasureType, Func<ZInt> declaredCountFunc, Action addExtraPGAMeasurementsAction = null)
			{
				if (indicator == OGAIndicatorList.Codes.Disclaimed && disclaimedMeasureType.HasValue)
				{
					additionalMeasureTypeCounts[disclaimedMeasureType.Value]++;
					disclaimedCount++;
				}
				else if (declaredCountFunc != null)
				{
					var count = declaredCountFunc();
					additionalMeasureTypeCounts[declaredMeasureType] += count;
					declaredCount += count;
				}

				if (addExtraPGAMeasurementsAction != null)
				{
					addExtraPGAMeasurementsAction();
				}
			}

			void AddAdditionalRatingMeasurements(MeasureType measureType, Func<ZInt> countFunc)
			{
				if (countFunc != null)
				{
					additionalMeasureTypeCounts[measureType] += countFunc();
				}
			}

			void AddAdditionalRatingMeasurementsForUniqueLicenses(MeasureType measureType, string code)
			{
				AddAdditionalRatingMeasurements(measureType, () => parent.InvoiceLines
					.OfType<JobComInvoiceLine>()
					.SelectMany<JobComInvoiceLine, LicenceAndPermit>(x => x.LicenceAndPermits)
					.Where(lp => lp.CY_Code == code)
					.Select(lp => lp.CY_Data)
					.Distinct()
					.Count());
			}
		}
		#endregion
	}
}
