using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsBusinessObjectCloneArgsTest : TestCaseWithFactory
	{
		public void TestGetCloneArgsForCountrySpecificDeclaration()
		{
			AssertNotNull(CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>()));
			AssertNull("should not be a problem with an invalid type", CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, ObjectFactory.GetType<Integration.Forwarding.IForwardingConsol>()));
		}

		public void TestForJobDeclaration()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BaseJobDeclaration));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_EntryStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_MessageStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_MessageSubType));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_DeclarationReference));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_JS));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_PaymentMethod));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GB));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_ApplicationCode));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GS_NKCusAgent));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_PaidBy));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GS_NKCustomsCommencedUser));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_CustomsCommencedDate));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_WarehouseTransactionStatus));

			AssertEquals(typeof(BaseJobDeclaration), args.TypeToCloneAs);
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopyWithinShipment, typeof(BaseJobDeclaration));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_EntryStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_MessageStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_MessageSubType));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_DeclarationReference));
			AssertEquals("If copied within a shipment, JE_JS should be copied.", false, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_JS));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_PaymentMethod));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GB));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_ApplicationCode));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GS_NKCusAgent));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_PaidBy));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GS_NKCustomsCommencedUser));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_CustomsCommencedDate));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_WarehouseTransactionStatus));

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BaseJobDeclaration));

			AssertNotNull(args);
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_EntryStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_MessageStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_EntrySubmittedDate));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_EntryAuthorisationDate));
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_MessageSubType));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_DeclarationReference));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_JS));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_VesselName));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_VoyageFlightNo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_Folio));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_OwnerRef));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GS_NKCusAgent));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GS_NKCustomsCommencedUser));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_CustomsCommencedDate));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_WarehouseTransactionStatus));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.DeepTemplateCopy, typeof(BaseJobDeclaration));

			AssertNotNull(args);
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_EntryStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_MessageStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_EntrySubmittedDate));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_EntryAuthorisationDate));
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_MessageSubType));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_DeclarationReference));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_JS));
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_VesselName));
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_VoyageFlightNo));
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_Folio));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GS_NKCusAgent));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_GS_NKCustomsCommencedUser));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_CustomsCommencedDate));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobDeclarationSchema.Constants.JE_WarehouseTransactionStatus));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
		}

		public void TestForCusEntryHeader()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(CusEntryHeader));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusEntryHeaderSchema.Constants.CH_JE));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
		}

		public void TestForCusDecHouseBill()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(Bill));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseBillSchema.Constants.CU_CU_ParentBill));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseBillSchema.Constants.CU_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseBillSchema.Constants.CU_JE));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseBillSchema.Constants.CU_Status));

			AssertEquals(typeof(Bill), args.TypeToCloneAs);
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(Bill));

			AssertNotNull(args);
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(CusDecHouseBillSchema.Constants.CU_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseBillSchema.Constants.CU_CU_ParentBill));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseBillSchema.Constants.CU_JE));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseBillSchema.Constants.CU_Status));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForCusContainer()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BaseCusContainer));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_JE));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_JC));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_MessageStatus));

			AssertEquals(typeof(BaseCusContainer), args.TypeToCloneAs);
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopyWithinShipment, typeof(BaseCusContainer));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_JE));
			AssertEquals("If copied within shipment, CO_JC should point to the same jobcontainer", false, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_JC));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_MessageStatus));

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BaseCusContainer));

			AssertNotNull(args);
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_JE));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_JC));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusContainerSchema.Constants.CO_MessageStatus));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForCusDecHouseContainerPivot()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BasePackingGroup));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseContainerPivotSchema.Constants.CR_CargoStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseContainerPivotSchema.Constants.CR_CO_Container));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseContainerPivotSchema.Constants.CR_CU_HouseBill));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(BasePackingGroup), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BasePackingGroup));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseContainerPivotSchema.Constants.CR_CargoStatus));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseContainerPivotSchema.Constants.CR_CO_Container));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseContainerPivotSchema.Constants.CR_CU_HouseBill));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForCusDecHouseContainerPack()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BasePackage));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseContainerPackSchema.Constants.CW_CR_HouseContainer));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(BasePackage), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BasePackage));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusDecHouseContainerPackSchema.Constants.CW_CR_HouseContainer));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForInvoiceHeader()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BaseJobComInvoiceHeader));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceHeaderSchema.Constants.JZ_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceHeaderSchema.Constants.JZ_JE));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(BaseJobComInvoiceHeader), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BaseJobComInvoiceHeader));

			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobComInvoiceHeaderSchema.Constants.JZ_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceHeaderSchema.Constants.JZ_JE));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForInvoiceLine()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BaseJobComInvoiceLine));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_JZ));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_CL));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_CC));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_Tariff));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(BaseJobComInvoiceLine), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BaseJobComInvoiceLine));

			AssertNotNull(args);
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_AddInfo));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_JZ));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_CL));
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_CC));
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty));
			AssertEquals("Excluded Column", false, args.IsExcludedFromCloning(JobComInvoiceLineSchema.Constants.JI_Tariff));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestGroupInvoiceCharge()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BaseGroupInvoiceCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(BaseGroupInvoiceCharge), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BaseGroupInvoiceCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForInvoiceCharge()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BaseInvoiceCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(BaseInvoiceCharge), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BaseInvoiceCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForInvoiceApportionedCharge()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BaseApportionedCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(BaseApportionedCharge), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BaseApportionedCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForInvoiceLineCharge()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BaseInvoiceLineCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(BaseInvoiceLineCharge), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BaseInvoiceLineCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForInvoiceLineApportionedCharge()
		{
			BusinessObjectCloneArgs args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopy, typeof(BaseInvoiceLineApportionedCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(BaseInvoiceLineApportionedCharge), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(BaseInvoiceLineApportionedCharge));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentID));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
		}

		public void TestForCusEntryInstruction()
		{
			var args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopyWithinShipment, typeof(CusEntryInstruction));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusEntryInstructionSchema.Constants.CEI_JE));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusEntryInstructionSchema.Constants.CEI_DateForDuty));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(typeof(CusEntryInstruction), args.TypeToCloneAs);

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(CusEntryInstruction));

			AssertNotNull(args);
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusEntryInstructionSchema.Constants.CEI_JE));
			AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusEntryInstructionSchema.Constants.CEI_DateForDuty));
			AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertNull(args.TypeToCloneAs);
		}

		public void TestForCusPackingList()
		{
			var args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopyWithinShipment, typeof(CusPackingList));
			CombineAssertions(() =>
			{
				AssertNotNull(args);
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusPackingListSchema.Constants.CUL_JE));
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusPackingListSchema.Constants.CUL_JZ));
				AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
				AssertEquals(typeof(CusPackingList), args.TypeToCloneAs);
			});

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(CusPackingList));

			CombineAssertions(() =>
			{
				AssertNotNull(args);
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusPackingListSchema.Constants.CUL_JE));
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusPackingListSchema.Constants.CUL_JZ));
				AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
				AssertNull(args.TypeToCloneAs);
			});
		}

		public void TestForCusPackage()
		{
			var args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.CountryToCountryCopyWithinShipment, typeof(CusPackage));

			CombineAssertions(() =>
			{
				AssertNotNull(args);
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(PkgPackageSchema.Constants.KP_KP_ParentPackage));
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(PkgPackageSchema.Constants.KP_KJ_ParentPackageJob));
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(PkgPackageSchema.Constants.KP_Sequence));
				AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
				AssertEquals(typeof(CusPackage), args.TypeToCloneAs);
			});

			args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(CusPackage));

			CombineAssertions(() =>
			{
				AssertNotNull(args);
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(PkgPackageSchema.Constants.KP_KP_ParentPackage));
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(PkgPackageSchema.Constants.KP_KJ_ParentPackageJob));
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(PkgPackageSchema.Constants.KP_Sequence));
				AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
				AssertNull(args.TypeToCloneAs);
			});
		}

		public void TestForCusLineTariffDetail()
		{
			var args = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(CusLineTariffDetail));

			CombineAssertions(() =>
			{
				AssertNotNull(args);
				AssertEquals("Excluded Column", true, args.IsExcludedFromCloning(CusLineTariffDetailSchema.Constants.BZ_ParentID));
				AssertEquals(true, args.PerformRowCopyWithoutTriggeringValidationAndSetter);
				AssertNull(args.TypeToCloneAs);
			});
		}
	}
}
