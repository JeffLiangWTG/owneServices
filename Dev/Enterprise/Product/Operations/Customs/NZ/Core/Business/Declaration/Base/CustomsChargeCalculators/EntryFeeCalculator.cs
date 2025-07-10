using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators
{
	class EntryFeeCalculator
	{
		internal class FeeChargeCalculator
		{
			public FeeChargeCalculator(ZDateTime transmitDate, BusinessObjectFactory factory)
			{
				this.transmitDate = transmitDate;
				this.factory = factory;
			}
			readonly ZDateTime transmitDate;
			readonly BusinessObjectFactory factory;

			public decimal InwardCargoTransactionFeeAir => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.InwardCargoTransactionFeeAir);

			public decimal OutwardCargoTransactionFeeSea => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.OutwardCargoTransactionFeeSea);

			public decimal OutwardCargoTransactionFeeAir => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.OutwardCargoTransactionFeeAir);

			public decimal OutwardReportTransactionFeeSea => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.OutwardReportTransactionFeeSea);

			public decimal OutwardReportTransactionFeeAir => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.OutwardReportTransactionFeeAir);

			public decimal ImportEntryTransactionFee => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.ImportEntryTransactionFee);

			/// <summary>
			/// Gets the Biosecurity System Entry Levy (BSEL).
			/// </summary>
			/// <remarks>
			/// Also known as Biosecurity Ris Screening Levy prior to July 2010.
			/// </remarks>
			internal decimal BiosecuritySystemEntryLevy => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.BiosecuritySystemEntryLevy);

			public decimal ExportEntryTransactionFeeSecureExportPartners => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.ExportEntryTransactionFeeSecureExportPartners);

			public decimal ExportEntryTransactionFeeNonSecureExportPartners => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.ExportEntryTransactionFeeNonSecureExportPartners);

			public decimal InwardCargoTransactionFeeSea => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.InwardCargoTransactionFeeSea);

			public ZDecimal Deminimus => GetFee(RateTypes.Deminimus);
			public ZDecimal LowValue => GetFee(UniversalReferenceConstants.TaxOrFeeCodes.LowValue);

			RefCusTaxOrFee.Loader Loader => loader ??= new RefCusTaxOrFee.Loader(factory);
			RefCusTaxOrFee.Loader loader;

			ZDecimal GetFee(ZString feeCode)
			{
				return Loader.GetTaxOrFee(feeCode, transmitDate);
			}
		}

		internal EntryFeeCalculator(JobDeclaration declaration)
		{
			transmitDate = ZDateTime.Today;
			factory = declaration?.Factory ?? new BusinessObjectFactory();
			if (declaration != null && !declaration.EntryFeeUnPayable)
			{
				transmitDate = declaration.JE_EDITransmitDate.IsValid ? declaration.JE_EDITransmitDate : declaration.CachedTodaysDate;
				var feeCharge = new FeeChargeCalculator(transmitDate, factory);

				if (declaration.IsFormalEntry)
				{
					if (declaration.IsImport)
					{
						if (!IsImportTransactionFeeExempt(declaration))
						{
							entryFeeValue = feeCharge.ImportEntryTransactionFee;
							mafLevyValue = feeCharge.BiosecuritySystemEntryLevy;
						}
					}
					else if (declaration.IsExport)
					{
						var entryFeeTotal = (declaration.IsExportedUnderSecureExportPartnershipScheme) ? feeCharge.ExportEntryTransactionFeeSecureExportPartners : feeCharge.ExportEntryTransactionFeeNonSecureExportPartners;
						entryFeeValue = entryFeeTotal;
					}
				}
				else if (declaration.IsECIWriteoff)
				{
					if (declaration.IsImport)
					{
						if (declaration.IsAir)
						{
							entryFeeValue = feeCharge.InwardCargoTransactionFeeAir;
						}
						else if (declaration.IsSea)
						{
							entryFeeValue = feeCharge.InwardCargoTransactionFeeSea;
						}
					}
					else if (declaration.IsExport)
					{
						if (declaration.IsAir)
						{
							entryFeeValue = feeCharge.OutwardCargoTransactionFeeAir;
						}
						else if (declaration.IsSea)
						{
							entryFeeValue = feeCharge.OutwardCargoTransactionFeeSea;
						}
					}
				}
			}
		}
		readonly BusinessObjectFactory factory;
		readonly ZDateTime transmitDate;
		readonly ZDecimal entryFeeValue;
		readonly ZDecimal mafLevyValue;

		bool IsImportTransactionFeeExempt(JobDeclaration declaration)
		{
			if (transmitDate < july2010)
			{
				return declaration.IsTemporary
					|| declaration.IsBond
					|| declaration.IsSight
					|| declaration.IsPrivateImport
					|| declaration.IsDutyDeminimus;
			}

			return declaration.IsSight || declaration.IsDiplomatic || declaration.IsPrimaryIndustriesImportDeclaration;
		}

		#region EntryFeeValue, EntryFeeGST

		protected ZDecimal EntryFeeValue => Utilities.Round(entryFeeValue, 2);

		protected ZDecimal EntryFeeGST => GSTCalculator.GetGST(factory, entryFeeValue, transmitDate);

		#endregion

		#region MAFLevyValue, MAFLevyGST

		protected ZDecimal MAFLevyValue => Utilities.Round(mafLevyValue, 2);

		protected ZDecimal MAFLevyGST => GSTCalculator.GetGST(factory, mafLevyValue, transmitDate);

		#endregion

		internal ZDecimal TotalFeePayable => EntryFeeValue + EntryFeeGST + MAFLevyValue + MAFLevyGST;

		public void SetEntryFeesAndLeviesOn(CusEntryHeader entryHeader)
		{
			entryHeader.EntryFeeAmount = EntryFeeValue + MAFLevyValue;
			entryHeader.EntryFeeGST = EntryFeeGST + MAFLevyGST;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1022:ThreadStaticSetInStaticInitializerRule", Justification = "Baseline issue")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2019:Improper 'ThreadStatic' field initialization", Justification = "Baseline issue")]
		[ThreadStatic]
		static readonly ZDateTime july2010 = new ZDateTime(2010, 7, 1);
	}
}
