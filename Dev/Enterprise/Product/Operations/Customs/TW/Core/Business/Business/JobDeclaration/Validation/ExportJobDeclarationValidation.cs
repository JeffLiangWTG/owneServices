using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ExportJobDeclarationValidation : JobDeclarationValidation
	{
		public ExportJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();

			var targetInfo = Parent.JE_OH_SupplierInfo;
			if (!Parent.JE_OH_Supplier.IsEmpty && !HasGovernmentVATCodeOrRodIdCardOrPassportNumber(Parent.Supplier))
			{
				targetInfo.AddMessageError(ValidationConstants.Declaration.ShouldHasGovernmentVATCodeOrRodIdCardOrPassportNumber);
			}
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			var declaration = Parent;
			if (!declaration.JE_RL_NKOrigin.IsEmpty && !declaration.IsZ99PortOfOrigin)
			{
				ListValidation.IfInvalidCode(GetPortNotificationType(), declaration.JE_RL_NKOriginInfo, declaration.Lookups.Origins, ValidationHelper.GetOriginInvalidCodeMessage(declaration));
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			var declaration = Parent;
			var targetInfo = declaration.JE_RL_NKFinalDestinationInfo;
			if (!declaration.JE_RL_NKFinalDestination.IsEmpty && !declaration.IsZ99FinalDestination)
			{
				ListValidation.IfInvalidCode(GetPortNotificationType(), targetInfo, declaration.Lookups.FinalDestinations, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));
			}
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
		}

		protected override void CheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(ZPropertyInfo targetInfo)
		{
			base.CheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(targetInfo);

			var declaration = Parent;
			if (declaration.FreeTradeZoneDeclarationTypes)
			{
				if (declaration.IsFreeTradeZoneDocumentaryAddress)
				{
					if (declaration.CusEntryInstruction.CEI_WHSMonth.IsEmpty)
					{
						targetInfo.AddWarning(ValidationConstants.Declaration.AutomaticallyImporterDeclareEntryNumber(targetInfo.HumanReadableName));
					}
					else
					{
						targetInfo.AddWarning(ValidationConstants.Declaration.AutomaticallyDeclareNIL(targetInfo.HumanReadableName));
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		protected override void CheckJE_OH_Consignee()
		{
			base.CheckJE_OH_Consignee();
			var declaration = Parent;
			var style = declaration.CusEntryInstruction.CEI_Style;
			if ((style == Constants.DeclarationTypes.Export.G3 || style == Constants.DeclarationTypes.Export.G5) && (declaration.Importer?.CountryCode ?? ZString.Empty) == Enterprise.Core.Constants.CountryCodes.Taiwan)
			{
				var consignee = declaration.IntermConsignee;
				var targetInfo = declaration.JE_OH_ConsigneeInfo;
				if (consignee == null)
				{
					targetInfo.AddMessageError(Res.GetString("ec50a5fc-45e9-468b-9bf3-352c9e45978b", "You have not selected a Consignee."));
				}
				else
				{
					var languages = SharedHelper.GetEnglishLanguageCodes();
					var hasEnglishAddress = languages.Contains(consignee.MainAddress.Language) || consignee.MainAddress.TranslatedAddresses.Cast<OrgTranslatedAddress>().Any(x => languages.Contains(x.Language));
					if (!hasEnglishAddress)
					{
						targetInfo.AddMessageError(Res.GetString("e3da9f25-a609-41fa-90eb-d9e69527398e", "The selected Consignee does not have English address."));
					}

					if (!OrgHeaderHelper.CheckHasCusCode(consignee, Core.Constants.CountryCodes.Taiwan, new string[] { OrgCusCode.CodeTypes.VATCode,
						OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID }))
					{
						targetInfo.AddMessageError(Res.GetString("f20c4e3c-2810-4334-8ec1-4852cc89c464", "A valid TW-VAT or TW-PID or TW-PAS number is required for Consignee. To create a valid TW-VAT or TW-PID or TW-PAS, visit Organization > Details > Config > Registration."));
					}
				}
			}
		}
	}
}
