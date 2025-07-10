using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class ImportJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();

			var targetInfo = Parent.JE_OH_SupplierInfo;
			if (!Parent.JE_OH_Supplier.IsEmpty)
			{
				var supplier = Parent.Supplier;
				if (supplier != null && !OrgHeaderHelper.HasAddressOfLanguage(supplier.Addresses.MainAddress, Core.Constants.Languages.English))
				{
					targetInfo.AddMessageError(ValidationConstants.Declaration.TheOrganizationShouldHaveEnglishAddress);
				}
			}
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			var targetInfo = Parent.JE_OH_ImporterInfo;
			if (!Parent.JE_OH_Importer.IsEmpty && !HasGovernmentVATCodeOrRodIdCardOrPassportNumber(Parent.Importer))
			{
				targetInfo.AddMessageError(ValidationConstants.Declaration.ShouldHasGovernmentVATCodeOrRodIdCardOrPassportNumber);
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_PaymentMethodInfo);
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ExportDateInfo);
		}

		void CheckETAShouldBeSameAsDeclarationDate(ZPropertyInfo targetInfo, ZDateTime dateOfArrival, ZDateTime dateForDuty)
		{
			if (dateOfArrival != dateForDuty)
			{
				targetInfo.AddMessageError(Res.GetString("b5d69fd6-170e-4d32-be7e-04d97dd1b230", "Arrival Date of Shipment should be the same as Declaration Date."));
			}
		}

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();

			var parent = Parent;
			var targetInfo = Parent.JE_DateOfArrivalInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			var entryInstruction = parent.CusEntryInstruction;
			var declarationType = entryInstruction.CEI_Style;
			var dateOfArrival = parent.JE_DateOfArrival;
			var dateForDuty = entryInstruction.CEI_DateForDuty;

			switch (declarationType)
			{
				case Constants.DeclarationTypes.Import.D2:
				case Constants.DeclarationTypes.Import.D7:
					CheckETAShouldBeSameAsDeclarationDate(targetInfo, dateOfArrival, dateForDuty);
					break;
				case Constants.DeclarationTypes.Import.F2:
				case Constants.DeclarationTypes.Import.F3:
					if (entryInstruction.CEI_WHSMonth.IsEmpty)
					{
						CheckETAShouldBeSameAsDeclarationDate(targetInfo, dateOfArrival, dateForDuty);
					}
					break;
				case Constants.DeclarationTypes.Import.B6:
				case Constants.DeclarationTypes.Import.D8:
					if (entryInstruction.CEI_WHSMonth.IsEmpty && EntryNumberHelper.IsBondedTypeOfSupplierFTZ(parent))
					{
						CheckETAShouldBeSameAsDeclarationDate(targetInfo, dateOfArrival, dateForDuty);
					}
					break;
				case Constants.DeclarationTypes.Import.G2:
					if (dateOfArrival < dateForDuty)
					{
						targetInfo.AddMessageError(Res.GetString("33fc813d-1d65-405b-8e6d-9462a948fa60", "Arrival Date of Shipment should be greater or equal to Declaration Date."));
					}
					break;
				default:
					break;
			}
		}
		
		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			var declaration = Parent;
			if (!declaration.JE_RL_NKFinalDestination.IsEmpty && !declaration.IsZ99FinalDestination)
			{
				ListValidation.IfInvalidCode(GetPortNotificationType(), declaration.JE_RL_NKFinalDestinationInfo, declaration.Lookups.FinalDestinations, ValidationHelper.GetFinalDestinationInvalidCodeMessage(declaration));
			}
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			var declaration = Parent;
			var originInfo = declaration.JE_RL_NKOriginInfo;
			MandatoryValidation.MessageErrorIfNotEntered(originInfo);

			var isZ99PortOfOrigin = declaration.IsZ99PortOfOrigin;
			if (!declaration.JE_RL_NKOrigin.IsEmpty && !isZ99PortOfOrigin)
			{
				ListValidation.IfInvalidCode(GetPortNotificationType(), originInfo, declaration.Lookups.Origins, ValidationHelper.GetOriginInvalidCodeMessage(declaration));
			}
		}

		protected override void CheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(ZPropertyInfo targetInfo)
		{
			base.CheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(targetInfo);

			var declaration = Parent;
			if (declaration.FreeTradeZoneDeclarationTypes)
			{
				if (declaration.IsFreeTradeZoneDocumentaryAddress)
				{
					targetInfo.AddWarning(ValidationConstants.Declaration.AutomaticallySupplierDeclareEntryNumber(targetInfo.HumanReadableName));
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}
	}
}
