using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceTaiwanValidation : AccComplianceSequenceValidation
	{
		public AccComplianceSequenceTaiwanValidation(AutoAccComplianceSequence parent) : base(parent)
		{
		}

		CodeDescriptionPairList TWComplianceSubTypesToBeReported
		{
			get
			{
				var fTWComplianceSubTypesToBeReported = new CodeDescriptionPairList();
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TXI);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TXP);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TDI);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TDC);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TDP);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TCR);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TCE);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TCD);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TXE);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TXC);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TSX, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TSX);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TSD, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TSD);
				fTWComplianceSubTypesToBeReported.AddPair(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXS, TaiwanComplianceInfo.ComplianceSubTypeDescriptions.TXS);

				return fTWComplianceSubTypesToBeReported;
			}
		}

#if DEBUG
		public CodeDescriptionPairList TWComplianceSubTypesToBeReported_ForTestOnly => TWComplianceSubTypesToBeReported;
#endif

		bool IsSubTypeReportedOfTW => TWComplianceSubTypesToBeReported.GetAllCodes().Any(x => x == Parent.XD_SequenceClass);
		string SubTypesStr => string.Join(", ", TWComplianceSubTypesToBeReported.GetAllCodes().Select(x => x));

		public override void ValidateAll()
		{
			base.ValidateAll();

			CheckSequenceNotModifiedAfterAllocated();
		}

		void CheckSequenceNotModifiedAfterAllocated()
		{
			var msgOfSubType = Res.GetString("55C684C1-5360-4B41-B091-C62AAE8EF82F", @"Sub Type can not be modified after allocated");
			var msgOfPrefix = Res.GetString("3DB31896-377F-4215-99F7-3A311CC3E85C", @"Series Prefix can not be modified after allocated");

			Parent.RemoveRowError(msgOfSubType);
			Parent.RemoveRowError(msgOfPrefix);

			if (Parent.IsInDatabase && (Parent.XD_SequenceClassInfo.HasChanges || Parent.XD_PrefixInfo.HasChanges))
			{
				var factory = new BusinessObjectFactory();
				var sequence = factory.Load<AccComplianceSequence>(Parent.PK);

				if (sequence.IsAllocated)
				{
					if (sequence.XD_SequenceClass != Parent.XD_SequenceClass)
					{
						Parent.AddRowError(msgOfSubType);
					}

					if (sequence.XD_Prefix != Parent.XD_Prefix)
					{
						Parent.AddRowError(msgOfPrefix);
					}
				}
			}
		}

		protected override void CheckXD_Prefix()
		{
			base.CheckXD_Prefix();

			Regex alphaRegex = new Regex("^[a-z]{2}$", RegexOptions.IgnoreCase);
			if (!Parent.XD_PrefixInfo.HasErrors() && !alphaRegex.IsMatch(Parent.XD_Prefix) && IsSubTypeReportedOfTW)
			{
				Parent.XD_PrefixInfo.AddError(Res.GetString("82B88134-7CF3-432A-B6DB-9FB44F195533", "Series prefix must be 2 alpha-characters in length for TW company for these compliance sub types: {0}", SubTypesStr));
			}
		}

		protected override void CheckXD_MaximumNumberDigits()
		{
			base.CheckXD_MaximumNumberDigits();

			if (!Parent.XD_MaximumNumberDigitsInfo.HasErrors())
			{
				if (Parent.XD_MaximumNumberDigits != Parent.LengthOfMaxNumberDigitsForTW && IsSubTypeReportedOfTW)
				{
					Parent.XD_MaximumNumberDigitsInfo.AddError(Res.GetString("32D3819D-D4BE-4CC7-A881-EB718CBE9388", "Max number digits must equal to {0} for TW company for these compliance sub types: {1}", Parent.LengthOfMaxNumberDigitsForTW, SubTypesStr));
				}
			}
		}

		protected override void CheckXD_StartDate()
		{
			base.CheckXD_StartDate();

			if (!Parent.XD_StartDateInfo.HasErrors() && Parent.XD_StartDate.IsEmpty && IsSubTypeReportedOfTW)
			{
				Parent.XD_StartDateInfo.AddError(Res.GetString("9E83BF08-F595-4DCF-B944-AD03C601BDCF", "Valid From can not be empty for these compliance sub types: {0}", SubTypesStr));
			}
		}

		protected override void CheckXD_ExpiryDate()
		{
			base.CheckXD_ExpiryDate();

			if (!Parent.XD_ExpiryDateInfo.HasErrors() && Parent.XD_ExpiryDate.IsEmpty && IsSubTypeReportedOfTW)
			{
				Parent.XD_ExpiryDateInfo.AddError(Res.GetString("6F2BA99E-B702-4C51-8447-A67D991842E2", "Expiry Date can not be empty for these compliance sub types: {0}", SubTypesStr));
			}
		}
	}
}
