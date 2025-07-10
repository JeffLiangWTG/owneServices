using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class PreviousBondedCusSupportingValidation : Customs.Business.CusSupportingInfoValidation
	{
		public PreviousBondedCusSupportingValidation(PreviousBondedCusSupporting parent)
			: base(parent)
		{
		}

		public new PreviousBondedCusSupporting Parent => (PreviousBondedCusSupporting)base.Parent;

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();

			var previousBonded = Parent;
			if (!previousBonded.CSI_ReferenceNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(previousBonded.CSI_LineNoInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var previousBonded = Parent;
			var invoiceLine = previousBonded.Parent;

			var csiReferenceNumberInfo = previousBonded.CSI_ReferenceNumberInfo;
			var csiReferenceNumber = previousBonded.CSI_ReferenceNumber;
			var cisLineNo = previousBonded.CSI_LineNo;

			if (!cisLineNo.IsEmpty || ShouldCheckPreviousBondedEntryNumberNotEmpty(invoiceLine))
			{
				MandatoryValidation.MessageErrorIfNotEntered(csiReferenceNumberInfo);
			}
			if (!csiReferenceNumber.IsEmpty && csiReferenceNumber.Length != 14)
			{
				csiReferenceNumberInfo.AddMessageError(ValidationConstants.PreviousBonded.PreviousBondedEntryNumberLength);
			}
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(csiReferenceNumberInfo);
		}

		bool ShouldCheckPreviousBondedEntryNumberNotEmpty(JobComInvoiceLine invoiceLine)
		{
			bool result = false;
			var declarationType = invoiceLine?.Declaration?.CusEntryInstruction?.CEI_Style ?? ZString.Empty;
			if (!declarationType.IsEmpty && PreviousBondedEntryNumberNotEmptyFuncMap.ContainsKey(declarationType))
			{
				result = PreviousBondedEntryNumberNotEmptyFuncMap[declarationType].Invoke(invoiceLine);
			}
			return result;
		}

		ImmutableDictionary<string, Func<JobComInvoiceLine, bool>> previousBondedEntryNumberNotEmptyFuncMap;

		ImmutableDictionary<string, Func<JobComInvoiceLine, bool>> PreviousBondedEntryNumberNotEmptyFuncMap
		{
			get
			{
				if (previousBondedEntryNumberNotEmptyFuncMap == null)
				{
					previousBondedEntryNumberNotEmptyFuncMap = ImmutableDictionary.CreateRange(new Dictionary<string, Func<JobComInvoiceLine, bool>>
					{
						{ Constants.DeclarationTypes.Import.B6,IsImportDutyTreatmentEFAndHasFTZCustomsRegNo },
						{ Constants.DeclarationTypes.Import.F2,IsImportDutyTreatmentEFAndHasFTZCustomsRegNo },
						{ Constants.DeclarationTypes.Import.D2,NumberIsNotEmpty },
						{ Constants.DeclarationTypes.Import.D7,NumberIsNotEmpty },
						{ Constants.DeclarationTypes.Export.B9,IsExportDutyTreatmentYZAndHasFTZCustomsRegNo },
						{ Constants.DeclarationTypes.Export.F4,IsExportDutyTreatmentYZAndHasFTZCustomsRegNo },
						{ Constants.DeclarationTypes.Export.D5,IsIsExportAndToBondedWarehouseIsEmpty }
					});
				}
				return previousBondedEntryNumberNotEmptyFuncMap;
			}
		}

		bool IsIsExportAndToBondedWarehouseIsEmpty(JobComInvoiceLine invoiceLine)
		{
			var declaration = invoiceLine.Declaration;
			return declaration != null && declaration.IsExport && declaration.CusEntryInstruction.CEI_OA_Warehouse2.IsEmpty;
		}
		bool NumberIsNotEmpty(JobComInvoiceLine invoiceLine)
		{
			return true;
		}

		bool IsImportDutyTreatmentEFAndHasFTZCustomsRegNo(JobComInvoiceLine invoiceLine)
		{
			var declaration = invoiceLine.Declaration;
			return declaration != null && declaration.IsImport && invoiceLine.JI_Procedure == DutyTreatmentEF && HasFTZCustomsRegNo(declaration.CusEntryInstruction.Warehouse);
		}

		bool IsExportDutyTreatmentYZAndHasFTZCustomsRegNo(JobComInvoiceLine invoiceLine)
		{
			var declaration = invoiceLine.Declaration;
			return declaration != null && declaration.IsExport && invoiceLine.JI_Procedure == DutyTreatmentYZ && HasFTZCustomsRegNo(declaration.CusEntryInstruction.Warehouse2);
		}

		bool HasFTZCustomsRegNo(OrgAddress orgAddress)
		{
			var customsRegNo = orgAddress?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.FTZ, Core.Constants.CountryCodes.Taiwan) ?? ZString.Empty;
			return !customsRegNo.IsEmpty;
		}

		const string DutyTreatmentEF = "EF";
		const string DutyTreatmentYZ = "YZ";
	}
}
