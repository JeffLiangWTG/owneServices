//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryInstructionValidation
//
//    This class should be used for overriding validation in AutoCusEntryInstructionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using AreaTypes = Enterprise.Customs.Business.BondedWarehousingHelper.Constants.AreaTypes;
namespace Enterprise.Customs.Business
{
	public class CusEntryInstructionValidation : AutoCusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(AutoCusEntryInstruction parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			base.ValidateAll();
			ValidateAgainstLinkedEntryHeaders();
			ValidateNoDuplicateInstructions();
		}

		public void ValidateAgainstLinkedEntryHeaders() => ValidateAgainstLinkedEntryHeadersCore();

		protected virtual void ValidateAgainstLinkedEntryHeadersCore()
		{
			var errorMessage = Res.GetString("8A467651-3604-4FDE-A33F-5D32C4A7F2BA", "The Entry Instruction is linked to multiple Entry Headers. Adding a new Entry Instruction could solve the problem. If you are attempting to save after making a change please also perform 'Generate Entries (Merge)' before attempting to save.");
			Parent.RemoveRowError(errorMessage);
			Parent.RemoveRowMessageError(errorMessage);
			var declaration = Parent.JobDeclaration;
			if (declaration != null && !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction && !Parent.AllowedToBeLinkedToMultipleEntryHeaders)
			{
				var entries = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Where(x => x.CH_CEI_Instruction == Parent.PK);
				if (entries.Select(x => x.CH_MessageType).GroupBy(x => x).Any(x => x.Take(2).Count() > 1))
				{
					if (Parent.AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError)
					{
						Parent.AddRowMessageError(errorMessage);
					}
					else
					{
						Parent.AddRowError(errorMessage);
					}
				}
			}
		}

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			ValidateStyleList();
		}

		protected virtual void ValidateStyleList()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_StyleInfo);
		}

		protected override void CheckCEI_OA_Warehouse()
		{
			base.CheckCEI_OA_Warehouse();
			var declaration = GetDeclarationForEntryInstructionThatNeedValidation();
			if (declaration != null)
			{
				var parent = Parent;
				var entryHeader = parent.EntryHeader;
				var targetInfo = parent.CEI_OA_WarehouseInfo;
				if (entryHeader != null)
				{
					if (entryHeader.IsInDatabase)
					{
						CheckWHSTransactionExists(targetInfo, () =>
						{
							var oldValue = targetInfo.OriginalValue;
							var currentValue = targetInfo.Value;
							return !oldValue.Equals(currentValue);
						});
					}
					if (!entryHeader.IsBondedWarehousingDisabled && entryHeader.IsBondedWarehousingFieldValidationRequired && parent.IsWarehouseRequiredForWarehouseValidation)
					{
						var address = parent.Warehouse;
						if (address == null)
						{
							var importer = declaration.Importer;
							targetInfo.AddMessageError(CusEntryHeader.BondedWarehouseIsRequiredForOutwardBondedWarehousing(importer == null ? ZString.Empty : importer.OH_Code, entryHeader.EntryHeaderDescriptiveMenuItemText));
						}
						else
						{
							ValidateWarehouseAddressIsInValidCountry(targetInfo, address, declaration, entryHeader);
						}
					}
				}

				var isSupplierWarehouseClient = declaration.Supplier?.OH_IsWarehouseClient ?? false;
				if (!targetInfo.Value.IsEmpty && parent.IsOutOfInwardProcessing && isSupplierWarehouseClient && (parent.Warehouse.GetWhsWarehouse()?.Areas.Cast<IWhsArea>().All(a => a.WA_AreaType != AreaTypes.InwardProcessing) ?? true))
				{
					targetInfo.AddMessageError(Res.GetString("bd16733e-16a6-46a6-9388-3accf4a668e4", "When Job is Out of Inward Processing, the From Warehouse must have an Inward Processing Location."));
				}
			}
		}

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();
			var declaration = GetDeclarationForEntryInstructionThatNeedValidation();
			if (declaration != null)
			{
				var parent = Parent;
				var address = parent.Warehouse2;
				var targetInfo = parent.CEI_OA_Warehouse2Info;
				var entryHeader = parent.EntryHeader;
				var importer = declaration.Importer;
				if (entryHeader != null)
				{
					if (entryHeader.IsInDatabase)
					{
						CheckWHSTransactionExists(targetInfo, () =>
						{
							var oldValue = targetInfo.OriginalValue;
							var currentValue = targetInfo.Value;
							return !oldValue.Equals(currentValue);
						});
					}
					if (!entryHeader.IsBondedWarehousingDisabled && entryHeader.IsBondedWarehousingFieldValidationRequired && parent.IsWarehouse2RequiredForWarehouseValidation)
					{
						if (address == null)
						{
							targetInfo.AddMessageError(CusEntryHeader.BondedWarehouseIsRequiredForInwardBondedWarehousing(importer?.OH_Code, entryHeader.EntryHeaderDescriptiveMenuItemText));
						}
						else
						{
							ValidateWarehouseAddressIsInValidCountry(targetInfo, address, declaration, entryHeader);
						}
					}
				}

				var declarantOrImporterIsWarehouseClient = importer?.OH_IsWarehouseClient ?? declaration.DeclarantAddress?.Header?.OH_IsWarehouseClient ?? false;
				if (!targetInfo.Value.IsEmpty && parent.IsIntoInwardProcessing
					&& declarantOrImporterIsWarehouseClient
					&& parent.IsWarehouse2RequiredForWarehouseValidation
					&& (address.GetWhsWarehouse()?.Areas.Cast<IWhsArea>().All(a => a.WA_AreaType != AreaTypes.InwardProcessing) ?? true))
				{
					targetInfo.AddMessageError(Res.GetString("e0765442-e4c8-4045-b754-9aa295a2538d", "When Job is Into Inward Processing, the To Warehouse must have an Inward Processing Location."));
				}
			}
		}

		protected virtual ZBool CheckDuplicatedInstruction => true;

		protected virtual void ValidateWarehouseAddressIsInValidCountry(ZPropertyInfo wareHouseInfo, OrgAddress address, BaseJobDeclaration declaration, CusEntryHeader entryHeader)
		{
			if (declaration.IsWarehouseAddressOutsideOfDeclarationCountry(address))
			{
				wareHouseInfo.AddMessageError(CusEntryHeader.BondedWarehouseAddressShouldBeInsideDeclarationCountry(declaration.Importer?.OH_Code ?? ZString.Empty, entryHeader.EntryHeaderDescriptiveMenuItemText, declaration.Country?.RN_DescMultilingual ?? ZString.Empty));
			}
		}

		protected BaseJobDeclaration GetDeclarationForEntryInstructionThatNeedValidation()
		{
			var result = Parent.JobDeclaration;
			if (result != null && !result.AreMultipleEntryInstructionsAllowed)
			{
				result = null;
			}
			return result;
		}

		void ValidateNoDuplicateInstructions()
		{
			if (CheckDuplicatedInstruction)
			{
				var parent = Parent;
				var provider = parent.JobDeclaration?.CustomsEntryInstructionProvider;

				if (provider != null)
				{
					var comparer = provider.EntryInstructionComparer;
					var cusEntryInstructions = provider.CustomsEntryInstructions.Cast<CusEntryInstruction>();

					if (comparer != null && cusEntryInstructions.Any(a => a.PK != parent.PK && comparer.Compare(a, parent) == ZInt.Zero))
					{
						parent.AddRowNotification(new Notification(comparer.GetUniquenessNotificationSeverity(), Res.GetString("2a0e587d-a2b7-4a92-b5a5-ee029f52a4b7", "This Entry Instruction already exists in the instructions list")));
					}
				}
			}
		}

		void CheckWHSTransactionExists(ZPropertyInfo info, Func<bool> hasValueChanged)
		{
			var entryHeader = Parent.EntryHeader;
			if (entryHeader != null && entryHeader.IsInDatabase && entryHeader.HasWHSTransaction && !info.HasErrors() && hasValueChanged != null && hasValueChanged())
			{
				info.AddError(Res.GetString("E7E940E3-C47A-4807-8EF2-60EA6500B3AE", "There is an Inventory transaction created against this Entry Instruction.\r\nPlease cancel it before changing this value."));
			}
		}
	}
}
