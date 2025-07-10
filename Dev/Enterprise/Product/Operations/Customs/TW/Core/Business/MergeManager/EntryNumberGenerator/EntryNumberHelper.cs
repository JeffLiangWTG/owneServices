using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	public static class EntryNumberHelper
	{
		#region rule3
		public static bool IsCategoryA(this CusEntryInstruction entryInstruction)
		{
			var ceiStyle = entryInstruction.CEI_Style;
			var declaration = entryInstruction?.JobDeclaration;
			var result = ceiStyle == Constants.DeclarationTypes.Import.G1
				|| ceiStyle == Constants.DeclarationTypes.Export.G3
				|| ceiStyle == Constants.DeclarationTypes.Export.G5
				|| ceiStyle == Constants.DeclarationTypes.Import.G7
				|| ceiStyle == Constants.DeclarationTypes.Import.F1
				|| ceiStyle == Constants.DeclarationTypes.Import.L1;
			if (!result && declaration != null)
			{
				switch (ceiStyle)
				{
					case Constants.DeclarationTypes.Import.D8:
					case Constants.DeclarationTypes.Import.B6:
						if (IsBondedIdOfSupplierEmpty(declaration) || !IsBondedTypeOfSupplierFTZ(declaration))
						{
							result = true;
						}
						break;
					case Constants.DeclarationTypes.Export.B8:
					case Constants.DeclarationTypes.Export.B9:
					case Constants.DeclarationTypes.Export.D5:
						if (IsBondedIdOfImporterEmpty(declaration) || !IsBondedTypeOfImporterFTZ(declaration))
						{
							result = true;
						}
						break;
					case Constants.DeclarationTypes.Export.F5:
						var finalDestinationCode = declaration.JE_RL_NKFinalDestination;
						result = !IsTaiwanCountryCode(finalDestinationCode);
						break;
				}
			}
			return result;
		}
		#endregion

		public static EntryNumberGeneratorCategory GetEntryNumberGeneratorCategory(this CusEntryInstruction entryInstruction)
		{
			var result = EntryNumberGeneratorCategory.None;
			if (entryInstruction.IsCategoryA())
			{
				if (entryInstruction.IsCategoryA1())
				{
					result = EntryNumberGeneratorCategory.A1;
				}
				else
				{
					result = EntryNumberGeneratorCategory.A;
				}
			}
			else if (entryInstruction.IsCategoryB())
			{
				result = EntryNumberGeneratorCategory.B;
			}
			else if (entryInstruction.IsCategoryC())
			{
				result = EntryNumberGeneratorCategory.C;
			}
			return result;
		}

		public static bool IsCategoryA1(this CusEntryInstruction entryInstruction)
		{
			return (entryInstruction.JobDeclaration?.GetDeclarationCustomsOffice(entryInstruction)?.ZZD_IsAir ?? false) && (entryInstruction.GetEntryInstructionCustomsOffice()?.ZZD_IsSea ?? false);
		}

		static bool IsBondedIdOfSupplierEmpty(JobDeclaration declaration)
		{
			return declaration.SupplierDocumentaryAddress.CBPCode.IsEmpty;
		}

		static bool IsBondedIdOfImporterEmpty(JobDeclaration declaration)
		{
			return declaration.ImporterDocumentaryAddress.CBPCode.IsEmpty;
		}

		internal static bool IsBondedTypeOfSupplierFTZ(JobDeclaration declaration)
		{
			return declaration.SupplierDocumentaryAddress.CBPCodeType == OrgCusCode.TaiwanCodeTypes.FTZ;
		}

		static bool IsBondedTypeOfImporterFTZ(JobDeclaration declaration)
		{
			return declaration.ImporterDocumentaryAddress.CBPCodeType == OrgCusCode.TaiwanCodeTypes.FTZ;
		}

		public static bool IsCategoryB(this CusEntryInstruction entryInstruction)
		{
			var ceiStyle = entryInstruction.CEI_Style;
			return ceiStyle == Constants.DeclarationTypes.Export.B1
				|| ceiStyle == Constants.DeclarationTypes.Export.B2
				|| ceiStyle == Constants.DeclarationTypes.Import.G2;
		}

		#region rule2
		public static bool IsCategoryC(this CusEntryInstruction entryInstruction)
		{
			var ceiStyle = entryInstruction.CEI_Style;
			var declaration = entryInstruction?.JobDeclaration;
			var result = ceiStyle == Constants.DeclarationTypes.Import.F2
				|| ceiStyle == Constants.DeclarationTypes.Import.F3
				|| ceiStyle == Constants.DeclarationTypes.Export.F4
				|| ceiStyle == Constants.DeclarationTypes.Export.D1
				|| ceiStyle == Constants.DeclarationTypes.Import.D2
				|| ceiStyle == Constants.DeclarationTypes.Import.D7;
			if (!result && declaration != null)
			{
				switch (ceiStyle)
				{
					case Constants.DeclarationTypes.Import.D8:
					case Constants.DeclarationTypes.Import.B6:
						if (!IsBondedIdOfSupplierEmpty(declaration) && IsBondedTypeOfSupplierFTZ(declaration))
						{
							result = true;
						}
						break;
					case Constants.DeclarationTypes.Export.B8:
					case Constants.DeclarationTypes.Export.B9:
					case Constants.DeclarationTypes.Export.D5:
						if (!IsBondedIdOfImporterEmpty(declaration) && IsBondedTypeOfImporterFTZ(declaration))
						{
							result = true;
						}
						break;
					case Constants.DeclarationTypes.Export.F5:
						var finalDestinationCode = declaration.JE_RL_NKFinalDestination;
						result = IsTaiwanCountryCode(finalDestinationCode);
						break;
				}
			}
			return result;
		}
		#endregion

		public static ZZRefCusCodeListCombined GetDeclarationCustomsOffice(this JobDeclaration declaration, CusEntryInstruction entryInstruction)
		{
			return TWRefCusCodeListLoader.GetCustomsOffice(declaration.Factory, declaration.JE_CustomsOffice, GetDateOfValuation(entryInstruction));
		}

		public static ZZRefCusCodeListCombined GetEntryInstructionCustomsOffice(this CusEntryInstruction entryInstruction)
		{
			return TWRefCusCodeListLoader.GetCustomsOffice(entryInstruction.Factory, entryInstruction.CEI_CustomsOffice, GetDateOfValuation(entryInstruction));
		}

		static ZDateTime GetDateOfValuation(CusEntryInstruction entryInstruction)
		{
			var result = entryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;
			if (!result.IsValid)
			{
				result = ZDateTime.Today;
			}
			return result;
		}

		static bool IsTaiwanCountryCode(ZString portCode)
		{
			return portCode.Left(2) == CountryCodes.Taiwan;
		}
	}
}
