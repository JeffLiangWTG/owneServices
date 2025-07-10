using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	public class AdditionalInformationValidation : Customs.Business.CusCodeDataValidation
	{
		public AdditionalInformationValidation(AdditionalInformation regoNumber)
			: base(regoNumber)
		{
		}

		public new AdditionalInformation Parent
		{
			get { return (AdditionalInformation)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();
			var code = Parent.CY_Code;
			if (!code.IsEmpty && !Parent.CY_CodeInfo.HasNotifications())
			{
				var entryLine = Parent.Parent;
				if (entryLine != null)
				{
					var header = entryLine.Header;
					if (header != null)
					{
						var order = Parent.CY_Order;
						var additionalInformationCodes = entryLine.AdditionalInformationCodes.OfType<AdditionalInformation>();
						var assessmentDate = header.EntryInstructionAssessmentDate;
						var codeList = Parent.Lookups.CY_CodeList;
						foreach (var pairCode in ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Parent.Factory, assessmentDate, code, RefCusCodeListAttributeTypes.Codes.Pair))
						{
							var relatedAddInfo = additionalInformationCodes.FirstOrDefault(addInfo => addInfo.CY_Code == pairCode && addInfo.CY_Order == order);
							if (relatedAddInfo == null)
							{
								Parent.CY_CodeInfo.AddMessageError(ValidationConstants.AdditionalInformation.MissingAdditionalInformationCode(codeList.GetDescriptionFromCode(pairCode), pairCode));
							}
							else
							{
								relatedAddInfo.Validation.ValidateCY_Code();
							}
						}
						foreach (var notPairCode in ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Parent.Factory, assessmentDate, code, RefCusCodeListAttributeTypes.Codes.NotPair))
						{
							if (entryLine.AdditionalInformationCodes[notPairCode] != null)
							{
								Parent.CY_CodeInfo.AddMessageError(ValidationConstants.AdditionalInformation.AdditionalInformationCodeCannotBeUsedTogether(code, notPairCode));
							}
						}
						if (code == UniversalReferenceConstants.AdditionalInformation.BondHolder && entryLine.IsLine1)
						{
							CheckAllLinesHaveBNDEntered(entryLine.Header);
						}
					}
				}
			}
			ValidateCY_Data();

			((CusEntryLineValidation)Parent?.Parent?.Validation)?.ValidateDiamondLevyValueAndAmount();
		}

		void CheckAllLinesHaveBNDEntered(CusEntryHeader entryHeader)
		{
			if (entryHeader != null && entryHeader.MergedLines.Cast<CusEntryLine>().Any(entryLine => entryLine.AdditionalInformationCodes[UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount] == null))
			{
				Parent.CY_CodeInfo.AddMessageError(ValidationConstants.AdditionalInformation.AllLinesNeedBNDWhenBHRIsEntered);
			}
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			var code = Parent.CY_Code;
			if (!code.IsEmpty)
			{
				var header = Parent.Parent?.Header;
				if (header != null)
				{
					var data = Parent.CY_Data;
					var assessmentDate = header.EntryInstructionAssessmentDate;
					if (data.IsEmpty || data == "0")
					{
						if (code == UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount && header.Line1HasBondHolder)
						{
							Parent.CY_DataInfo.AddMessageError(ValidationConstants.AdditionalInformation.AllLinesNeedBNDWhenBHRIsEntered);
						}
						else if (!ZARefCusCodeListTypes.GetAddInWithAllowEmptyAttribute(Parent.Factory, assessmentDate).ContainsCode(code) && !ZARefCusCodeListTypes.GetAddInWithEmptyAttribute(Parent.Factory, assessmentDate).ContainsCode(code))
						{
							Parent.CY_DataInfo.AddMessageError(ValidationConstants.AdditionalInformation.AdditionalInfoCodeRequireData(code));
						}
					}
					else if (ZARefCusCodeListTypes.GetAddInWithEmptyAttribute(Parent.Factory, assessmentDate).ContainsCode(code))
					{
						Parent.CY_DataInfo.AddMessageError(ValidationConstants.AdditionalInformation.AdditionalInfoCodeShouldNotHaveData(code));
					}

					if (data.Contains(" ", StringComparison.OrdinalIgnoreCase))
					{
						var allowSpaceFlag = Parent.RefCusCode?.GetAttribute(RefCusCodeListAttributeTypes.Codes.AllowSpace) ?? ZString.Empty;
						if (allowSpaceFlag == UniversalReferenceConstants.RefCusCodeListAttributes.Values.NotAllowSpace)
						{
							Parent.CY_DataInfo.AddError(ValidationConstants.AdditionalInformation.AdditionalInfoCodeRequireNoSpaceData(code));
						}
					}
				}
			}
			ValidateCY_Code();
		}
	}
}
