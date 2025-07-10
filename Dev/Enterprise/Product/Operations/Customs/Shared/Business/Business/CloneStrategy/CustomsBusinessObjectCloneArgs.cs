using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class CustomsBusinessObjectCloneArgs
	{
		public static BusinessObjectCloneArgs GetCloneArgs(CloneType cloneType, Type typeOfBusinessObjectToClone)
		{
			BusinessObjectCloneArgs result = null;

			switch (cloneType)
			{
				case CloneType.CountryToCountryCopy:
					result = GetCloneArgsRecursivelyUntilFindKey(typeOfBusinessObjectToClone, CompleteArgsForCountryToCountryCopy);
					break;

				case CloneType.CountryToCountryCopyWithinShipment:
					result = GetCloneArgsRecursivelyUntilFindKey(typeOfBusinessObjectToClone, CompleteArgsForCountryToCountryCopyWithinShipment);
					break;

				case CloneType.TemplateCopy:
					result = GetCloneArgsRecursivelyUntilFindKey(typeOfBusinessObjectToClone, CompleteArgsForTemplateCopy);
					break;

				case CloneType.DeepTemplateCopy:
					result = GetCloneArgsRecursivelyUntilFindKey(typeOfBusinessObjectToClone, CompleteArgsForDeepTemplateCopy);
					break;
			}

			return result;
		}

		public static BusinessObjectCloneArgs GetCloneArgs(CloneType cloneType, Type typeOfBusinessObjectToClone, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		{
			BusinessObjectCloneArgs result = GetCloneArgs(cloneType, typeOfBusinessObjectToClone);

			if (result != null && alternativeFactoryToInstantiateCloneIn != null)
			{
				result = new BusinessObjectCloneArgs(alternativeFactoryToInstantiateCloneIn, result.GetExcludedColumns(), result.TypeToCloneAs, result.PerformRowCopyWithoutTriggeringValidationAndSetter);
			}

			return result;
		}

		static BusinessObjectCloneArgs GetCloneArgsRecursivelyUntilFindKey(Type typeOfBusinessObjectToClone, Dictionary<Type, BusinessObjectCloneArgs> completeArgs)
		{
			BusinessObjectCloneArgs result = null;

			if (typeOfBusinessObjectToClone != null)
			{
				completeArgs.TryGetValue(typeOfBusinessObjectToClone, out result);

				if (result == null)
				{
					result = GetCloneArgsRecursivelyUntilFindKey(typeOfBusinessObjectToClone.BaseType, completeArgs);
				}
			}

			return result;
		}

		#region CountryToCountryCopy

		[ThreadStatic]
		static Dictionary<Type, BusinessObjectCloneArgs> completeArgsForCountryToCountryCopyWithinShipment;

		static Dictionary<Type, BusinessObjectCloneArgs> CompleteArgsForCountryToCountryCopyWithinShipment
		{
			get
			{
				if (completeArgsForCountryToCountryCopyWithinShipment == null)
				{
					completeArgsForCountryToCountryCopyWithinShipment = GetArgs(copyWithinShipment: true);
				}
				return completeArgsForCountryToCountryCopyWithinShipment;
			}
		}

		[ThreadStatic]
		static Dictionary<Type, BusinessObjectCloneArgs> completeArgsForCountryToCountryCopy;

		static Dictionary<Type, BusinessObjectCloneArgs> CompleteArgsForCountryToCountryCopy
		{
			get { return completeArgsForCountryToCountryCopy ?? (completeArgsForCountryToCountryCopy = GetArgs(copyWithinShipment: false)); }
		}

		static Dictionary<Type, BusinessObjectCloneArgs> GetArgs(bool copyWithinShipment)
		{
			Dictionary<Type, BusinessObjectCloneArgs> result = new Dictionary<Type, BusinessObjectCloneArgs>();

			var excludedDeclarationColumns = new List<string>(new string[]
				{
					JobDeclarationSchema.Constants.JE_EntryStatus,
					JobDeclarationSchema.Constants.JE_MessageStatus,
					JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus,
					JobDeclarationSchema.Constants.JE_DeclarationReference,
					JobDeclarationSchema.Constants.JE_EntryAuthorisationDate,
					JobDeclarationSchema.Constants.JE_EntrySubmittedDate,
					JobDeclarationSchema.Constants.JE_MessageSubType,
					JobDeclarationSchema.Constants.JE_PaymentMethod,
					JobDeclarationSchema.Constants.JE_AddInfo,
					JobDeclarationSchema.Constants.JE_NAddInfo,
					JobDeclarationSchema.Constants.JE_GB,
					JobDeclarationSchema.Constants.JE_GC,
					JobDeclarationSchema.Constants.JE_ApplicationCode,
					JobDeclarationSchema.Constants.JE_GS_NKCusAgent,
					JobDeclarationSchema.Constants.JE_PaidBy,
					JobDeclarationSchema.Constants.JE_GS_NKCustomsCommencedUser,
					JobDeclarationSchema.Constants.JE_CustomsCommencedDate,
					JobDeclarationSchema.Constants.JE_WarehouseTransactionStatus
				});

			if (!copyWithinShipment)
			{
				excludedDeclarationColumns.Add(JobDeclarationSchema.Constants.JE_JS);
			}

			result.Add(typeof(BaseJobDeclaration), new BusinessObjectCloneArgs(excludedDeclarationColumns, typeof(BaseJobDeclaration), true));

			result.Add(typeof(Bill), new BusinessObjectCloneArgs(new string[]
				{
					CusDecHouseBillSchema.Constants.CU_JE,
					CusDecHouseBillSchema.Constants.CU_CU_ParentBill,
					CusDecHouseBillSchema.Constants.CU_Status,
					CusDecHouseBillSchema.Constants.CU_AddInfo
				}, typeof(Bill), true));

			var excludedContainerColumns = new List<string>(new string[]
				{
					CusContainerSchema.Constants.CO_JE,
					CusContainerSchema.Constants.CO_MessageStatus,
					CusContainerSchema.Constants.CO_AddInfo
				});

			if (!copyWithinShipment)
			{
				excludedContainerColumns.Add(CusContainerSchema.Constants.CO_JC);
			}

			result.Add(typeof(BaseCusContainer), new BusinessObjectCloneArgs(excludedContainerColumns, typeof(BaseCusContainer), true));

			result.Add(typeof(BasePackingGroup), new BusinessObjectCloneArgs(new string[]
				{
					CusDecHouseContainerPivotSchema.Constants.CR_CargoStatus,
					CusDecHouseContainerPivotSchema.Constants.CR_CO_Container,
					CusDecHouseContainerPivotSchema.Constants.CR_CU_HouseBill
				}, typeof(BasePackingGroup), true));

			result.Add(typeof(BasePackage), new BusinessObjectCloneArgs(new string[]
				{
					CusDecHouseContainerPackSchema.Constants.CW_CR_HouseContainer
				}, typeof(BasePackage), true));

			result.Add(typeof(BaseJobComInvoiceGroupHeader), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvoiceHeaderSchema.Constants.JZ_JE,
					JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK,
					JobComInvoiceHeaderSchema.Constants.JZ_AddInfo
				}, typeof(BaseJobComInvoiceGroupHeader), true));

			result.Add(typeof(BaseJobComInvoiceHeader), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvoiceHeaderSchema.Constants.JZ_JE,
					JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK,
					JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill,
					JobComInvoiceHeaderSchema.Constants.JZ_AddInfo,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRateType,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrLandedCostExRate,
					JobComInvoiceHeaderSchema.Constants.JZ_PaymentExRate
				}, typeof(BaseJobComInvoiceHeader), true));

			result.Add(typeof(BaseJobComInvoiceLine), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvoiceLineSchema.Constants.JI_JZ,
					JobComInvoiceLineSchema.Constants.JI_AddInfo,
					JobComInvoiceLineSchema.Constants.JI_NAddInfo,
					JobComInvoiceLineSchema.Constants.JI_CC,
					JobComInvoiceLineSchema.Constants.JI_CL,
					JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty,
					JobComInvoiceLineSchema.Constants.JI_Tariff
				}, typeof(BaseJobComInvoiceLine), true));

			//BaseJobComInvHeaderCharge is abstract and cannot be used as typeToCloneAs. TypeDecider wont have enough information when Factory.New(typeToCloneAs) happens
			result.Add(typeof(BaseGroupInvoiceCharge), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvHeaderChargeSchema.Constants.J7_ParentID,
					JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode
				}, typeof(BaseGroupInvoiceCharge), true));

			result.Add(typeof(BaseInvoiceCharge), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvHeaderChargeSchema.Constants.J7_ParentID,
					JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode
				}, typeof(BaseInvoiceCharge), true));

			result.Add(typeof(BaseApportionedCharge), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvHeaderChargeSchema.Constants.J7_ParentID,
					JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode
				}, typeof(BaseApportionedCharge), true));

			result.Add(typeof(BaseInvoiceLineCharge), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvHeaderChargeSchema.Constants.J7_ParentID,
					JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode
				}, typeof(BaseInvoiceLineCharge), true));

			result.Add(typeof(BaseInvoiceLineApportionedCharge), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvHeaderChargeSchema.Constants.J7_ParentID,
					JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode
				}, typeof(BaseInvoiceLineApportionedCharge), true));

			result.Add(typeof(Transport), new BusinessObjectCloneArgs(new string[]
				{
					JobConsolTransportSchema.Constants.JW_ParentGUID,
				}, typeof(Transport), true));

			result.Add(typeof(CusEntryInstruction), new BusinessObjectCloneArgs(new string[]
				{
					CusEntryInstructionSchema.Constants.CEI_JE,
					CusEntryInstructionSchema.Constants.CEI_DateForDuty,
				}, typeof(CusEntryInstruction), true));

			result.Add(typeof(CusPackingList), new BusinessObjectCloneArgs(new string[]
				{
					CusPackingListSchema.Constants.CUL_JE,
					CusPackingListSchema.Constants.CUL_JZ,
				}, typeof(CusPackingList), true));

			result.Add(typeof(CusPackage), new BusinessObjectCloneArgs(new string[]
				{
					PkgPackageSchema.Constants.KP_KJ_ParentPackageJob,
					PkgPackageSchema.Constants.KP_KP_ParentPackage,
					PkgPackageSchema.Constants.KP_Sequence,
				}, typeof(CusPackage), true));

			result.Add(typeof(CusPackableItem), new BusinessObjectCloneArgs(new string[]
				{
					CusPackableItemSchema.Constants.CUI_CUL,
					CusPackableItemSchema.Constants.CUI_ClusterKey,
					CusPackableItemSchema.Constants.CUI_JI,
				}, typeof(CusPackableItem), true));

			return result;
		}

		#endregion

		#region TemplateCopy Args

		[ThreadStatic]
		static Dictionary<Type, BusinessObjectCloneArgs> completeArgsForTemplateCopy;

		static Dictionary<Type, BusinessObjectCloneArgs> CompleteArgsForTemplateCopy
		{
			get { return completeArgsForTemplateCopy ?? (completeArgsForTemplateCopy = GetTemplateCopyArgs()); }
		}

		static Dictionary<Type, BusinessObjectCloneArgs> GetTemplateCopyArgs()
		{
			Dictionary<Type, BusinessObjectCloneArgs> result = GetDeepTemplateCopyArgs();
			BusinessObjectCloneArgs args;
			if (!result.TryGetValue(typeof(BaseJobDeclaration), out args))
			{
				args = new BusinessObjectCloneArgs(Array.Empty<string>(), typeof(BaseJobDeclaration), true);
				result.Add(typeof(BaseJobDeclaration), args);
			}
			args.AddExcludedColumns(new string[]
				{
					JobDeclarationSchema.Constants.JE_MasterBill,
					JobDeclarationSchema.Constants.JE_HouseBill,
					JobDeclarationSchema.Constants.JE_VesselName,
					JobDeclarationSchema.Constants.JE_VoyageFlightNo,
					JobDeclarationSchema.Constants.JE_Folio,
					JobDeclarationSchema.Constants.JE_EntryStatus,
					JobDeclarationSchema.Constants.JE_MessageStatus,
					JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus,
					JobDeclarationSchema.Constants.JE_DeclarationReference,
					JobDeclarationSchema.Constants.JE_EntryAuthorisationDate,
					JobDeclarationSchema.Constants.JE_EntrySubmittedDate,
					JobDeclarationSchema.Constants.JE_OwnerRef,
					JobDeclarationSchema.Constants.JE_JS,
					JobDeclarationSchema.Constants.JE_GS_NKCusAgent,
					JobDeclarationSchema.Constants.JE_GS_NKCustomsCommencedUser,
					JobDeclarationSchema.Constants.JE_CustomsCommencedDate,
					JobDeclarationSchema.Constants.JE_WarehouseTransactionStatus
				});
			return result;
		}

		[ThreadStatic]
		static Dictionary<Type, BusinessObjectCloneArgs> completeArgsForDeepTemplateCopy;

		static Dictionary<Type, BusinessObjectCloneArgs> CompleteArgsForDeepTemplateCopy
		{
			get { return completeArgsForDeepTemplateCopy ?? (completeArgsForDeepTemplateCopy = GetDeepTemplateCopyArgs()); }
		}

		static Dictionary<Type, BusinessObjectCloneArgs> GetDeepTemplateCopyArgs()
		{
			Dictionary<Type, BusinessObjectCloneArgs> result = new Dictionary<Type, BusinessObjectCloneArgs>();
			result.Add(typeof(BaseJobDeclaration), new BusinessObjectCloneArgs(new string[]
				{
					JobDeclarationSchema.Constants.JE_EntryStatus,
					JobDeclarationSchema.Constants.JE_MessageStatus,
					JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus,
					JobDeclarationSchema.Constants.JE_DeclarationReference,
					JobDeclarationSchema.Constants.JE_EntryAuthorisationDate,
					JobDeclarationSchema.Constants.JE_EntrySubmittedDate,
					JobDeclarationSchema.Constants.JE_JS,
					JobDeclarationSchema.Constants.JE_GS_NKCusAgent,
					JobDeclarationSchema.Constants.JE_GS_NKCustomsCommencedUser,
					JobDeclarationSchema.Constants.JE_CustomsCommencedDate,
					JobDeclarationSchema.Constants.JE_WarehouseTransactionStatus
				}, true));

			result.Add(typeof(CusEntryHeader), new BusinessObjectCloneArgs(new string[] {
				CusEntryHeaderSchema.Constants.CH_JE,
			}, true));

			result.Add(typeof(CusEntryInstruction), new BusinessObjectCloneArgs(new string[]
				{
					CusEntryInstructionSchema.Constants.CEI_JE,
					CusEntryInstructionSchema.Constants.CEI_DateForDuty,
				}, true));

			result.Add(typeof(Bill), new BusinessObjectCloneArgs(new string[]
				{
					CusDecHouseBillSchema.Constants.CU_JE,
					CusDecHouseBillSchema.Constants.CU_CU_ParentBill,
					CusDecHouseBillSchema.Constants.CU_Status
				}, true));

			result.Add(typeof(BaseCusContainer), new BusinessObjectCloneArgs(new string[]
				{
					CusContainerSchema.Constants.CO_JC,
					CusContainerSchema.Constants.CO_JE,
					CusContainerSchema.Constants.CO_MessageStatus
				}, true));

			result.Add(typeof(BasePackingGroup), new BusinessObjectCloneArgs(new string[]
				{
					CusDecHouseContainerPivotSchema.Constants.CR_CargoStatus,
					CusDecHouseContainerPivotSchema.Constants.CR_CO_Container,
					CusDecHouseContainerPivotSchema.Constants.CR_CU_HouseBill
				}, true));

			result.Add(typeof(BasePackage), new BusinessObjectCloneArgs(new string[]
				{
					CusDecHouseContainerPackSchema.Constants.CW_CR_HouseContainer
				}, true));

			result.Add(typeof(BaseJobComInvoiceGroupHeader), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvoiceHeaderSchema.Constants.JZ_JE,
					JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK,
				}, true));

			result.Add(typeof(BaseJobComInvoiceHeader), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvoiceHeaderSchema.Constants.JZ_JE,
					JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK,
					JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill
				}, true));

			result.Add(typeof(BaseJobComInvoiceLine), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvoiceLineSchema.Constants.JI_JZ,
					JobComInvoiceLineSchema.Constants.JI_CL,
					JobComInvoiceLineSchema.Constants.JI_CEI
				}, true));

			result.Add(typeof(BaseJobComInvHeaderCharge), new BusinessObjectCloneArgs(new string[]
				{
					JobComInvHeaderChargeSchema.Constants.J7_ParentID,
					JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode
				}, true));

			result.Add(typeof(Transport), new BusinessObjectCloneArgs(new string[]
				{
					JobConsolTransportSchema.Constants.JW_ParentGUID,
				}, true));

			result.Add(typeof(JobDocAddress), new BusinessObjectCloneArgs(new string[]
				{
					JobDocAddressSchema.Constants.E2_ParentID,
				}, true));

			result.Add(typeof(CusSupportingInfo), new BusinessObjectCloneArgs(new string[]
				{
					CusSupportingInfoSchema.Constants.CSI_ParentID,
				}, true));

			result.Add(typeof(CusPackingList), new BusinessObjectCloneArgs(new string[]
				{
					CusPackingListSchema.Constants.CUL_JE,
					CusPackingListSchema.Constants.CUL_JZ,
				}, true));

			result.Add(typeof(CusPackage), new BusinessObjectCloneArgs(new string[]
				{
					PkgPackageSchema.Constants.KP_KJ_ParentPackageJob,
					PkgPackageSchema.Constants.KP_KP_ParentPackage,
					PkgPackageSchema.Constants.KP_Sequence,
				}, true));

			result.Add(typeof(CusPackableItem), new BusinessObjectCloneArgs(new string[]
				{
					CusPackableItemSchema.Constants.CUI_CUL,
					CusPackableItemSchema.Constants.CUI_ClusterKey,
					CusPackableItemSchema.Constants.CUI_JI,
				}, typeof(CusPackableItem), true));

			result.Add(typeof(CusLineTariffDetail), new BusinessObjectCloneArgs(new string[]
				{
					CusLineTariffDetailSchema.Constants.BZ_ParentID,
				}, true));

			return result;
		}

		#endregion
	}
}
