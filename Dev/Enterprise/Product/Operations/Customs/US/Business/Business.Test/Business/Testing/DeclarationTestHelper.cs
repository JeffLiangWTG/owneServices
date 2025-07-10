using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DeclarationTestHelper : MasterFilesTestHelper
	{
		public DeclarationTestHelper()
		{
		}

		public DeclarationTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public string GetUnknownMessage()
		{
			APLB b = new APLB();
			APLY y = new APLY();
			b.ApplicationIdentifier = "XX";
			y.ApplicationIdentifier = "XX";
			return
				b.Serialise() +
				@"XYZ123".PadRight(80) +
				y.Serialise();
		}

		public string GetUnknownMessage2()
		{
			APLB b = new APLB();
			b.UserData = "10000";
			APLY y = new APLY();
			b.ApplicationIdentifier = "EI";
			y.ApplicationIdentifier = "EI";
			ENSEB ensEB = new ENSEB();
			ensEB.NarrativeMessage = "\"A\" AND \"B\" REC DP/FLR/OFFICE CONFLICT";
			return
				b.Serialise() +
				ensEB.Serialise() +
				y.Serialise();
		}

		public static GlbStaff CreateStaff(BusinessObjectFactory factory)
		{
			GlbGroup group = factory.New<GlbGroup>();
			group.GG_Code = "~Z";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "X1";
			staff.GS_LoginName = "x1";
			staff.GS_EmailAddress = "brett@pretend.email.com";
			return staff;
		}

		public static void SetupForSendMessage()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			var exportFiler = new ExportEntryFilerID();
			exportFiler.EntryFilerID = "364331434";
			exportFiler.EntryFilerIDType = "D";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(companyPK, Guid.Empty, Guid.Empty, exportFiler);

			USCustomsDataRegistry.Instance.ReceiverDistrictPort.SetValue(companyPK, Guid.Empty, Guid.Empty, "3901");
			USCustomsDataRegistry.Instance.ARecordOfficeCode.SetValue(companyPK, Guid.Empty, Guid.Empty, "89");
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbGroup group = factory.New<GlbGroup>();
			group.GG_Code = "~Z";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "X1";
			staff.GS_LoginName = "x1";
			staff.GS_EmailAddress = "brett@pretend.email.com";

			var fakeyStaff = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff.AddNew();
			fakeyStaff.GS_Code = "X2";
			fakeyStaff.GS_LoginName = "x2";
			fakeyStaff.GS_EmailAddress = "brett2@pretend.email.com";

			factory.Save();

			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(companyPK, Guid.Empty, Guid.Empty, filer);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(companyPK, Guid.Empty, Guid.Empty, "8888");
		}

		public static void SetupBranchSpecificInBondNumberRanges()
		{
			ClearOutExistingRegistries();
			GlbBranchDependentCollection branches = GlbCompany.CurrentCompany.Branches;
			long startNumber = NumberFountains.USMinimumInBondNumber - 1;
			long numbersPerBranch = Convert.ToInt64(Math.Floor((decimal)NumberFountains.USMaximumInBondNumber / branches.Count));
			foreach (GlbBranch branch in branches)
			{
				InBondNumberRange numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
				numberRange.StartNumber = ++startNumber;
				startNumber += numbersPerBranch;
				numberRange.LastNumber = Math.Min(startNumber, NumberFountains.USMaximumInBondNumber);
				numberRange.RunOutWarningLimitNumber = 60000;
				USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, numberRange);
			}
		}

		public static void SetupCompanySpecificInBondNumberRange()
		{
			ClearOutExistingRegistries();
			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			InBondNumberRange numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			numberRange.StartNumber = NumberFountains.USMinimumInBondNumber;
			numberRange.LastNumber = NumberFountains.USMaximumInBondNumber;
			numberRange.RunOutWarningLimitNumber = 60000;
			USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.SetValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, numberRange);
		}

		static void ClearOutExistingRegistries()
		{
			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			GlbBranchDependentCollection branches = GlbCompany.CurrentCompany.Branches;
			((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).DeleteValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			foreach (GlbBranch branch in branches)
			{
				((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).DeleteValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
			}
		}

		public static CustomsNumberViewStmNums SetupCompanySpecificFormalEntryNumber(ZString entryFilerCode, bool deleteExisting = true)
		{
			return SetupCompanySpecificFormalEntryNumber(GlbBranch.CurrentBranch, entryFilerCode, deleteExisting);
		}

		public static CustomsNumberViewStmNums SetupCompanySpecificFormalEntryNumber(GlbBranch branch, ZString entryFilerCode, bool deleteExisting = true)
		{
			var setting = SetupNumsSettingWithoutDataMatchingEntryFilerCode(branch, entryFilerCode, deleteExisting);
			return SetupCompanySpecificFormalEntryNumber(setting, branch);
		}

		public static CustomsNumberViewStmNums SetupCompanySpecificFormalEntryNumber(ACEEntryStmNumsSetting setting, GlbBranch branch)
		{
			var ensSetting = setting.Provider.GetSetting(NumberRangeTypeList.Codes.CustomsEntry);
			setting.AddForTesting(branch.GB_GC, 1L, ensSetting.DefaultTypeRangeMax().Value);
			return setting.GetFirstAvailableOrLastSequence(branch); // must use different factory as SN_ID is set once saved
		}

		static ACEEntryStmNumsSetting SetupNumsSettingWithoutDataMatchingEntryFilerCode(GlbBranch branch, ZString entryFilerCode, bool deleteExisting)
		{
			SetEntryFilerCode(entryFilerCode);
			var setting = ACEEntryStmNumsSetting.New(branch, entryFilerCode);
			if (deleteExisting)
			{
				setting.Provider.CustomsNumberWrappers.OfType<USCustomsNumberViewStmNumsWrapper>().Where(x => x.IsCustomsEntry && x.AppliesTo == entryFilerCode).Select(x => x.StmNums).DeleteAll();
				setting.Company.Factory.Save();
			}
			return setting;
		}

		public static void SetReconInterestInRegistry(ZDate startDate, ZDate endDate, ZDecimal rate)
		{
			ReconInterestRateCollection collection = USCustomsDataRegistry.Instance.ReconInterestRates.Value;
			collection.AddNew(startDate, endDate, rate);
			USCustomsDataRegistry.Instance.ReconInterestRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public static CustomsNumberViewStmNums SetupBranchSpecificFormalEntryNumber(ZString entryFilerCode, bool deleteExisting = true)
		{
			return SetupBranchSpecificFormalEntryNumber(GlbBranch.CurrentBranch, entryFilerCode, deleteExisting);
		}

		public static CustomsNumberViewStmNums SetupBranchSpecificFormalEntryNumber(GlbBranch mainBranch, ZString entryFilerCode, bool deleteExisting = true)
		{
			var setting = SetupNumsSettingWithoutDataMatchingEntryFilerCode(mainBranch, entryFilerCode, deleteExisting);
			return SetupBranchSpecificFormalEntryNumber(setting, mainBranch);
		}

		public static CustomsNumberViewStmNums SetupBranchSpecificFormalEntryNumber(ACEEntryStmNumsSetting setting, GlbBranch mainBranch)
		{
			var branches = setting.Company.Branches.ToArray();
			long startNumber = 0;
			long numbersPerBranch = Convert.ToInt64(Math.Floor((decimal)USCustomsNumberViewStmNumsSetting.USMaximumFormalEntryNumber / branches.Length));
			foreach (var branch in branches)
			{
				var minValue = ++startNumber;
				startNumber += numbersPerBranch;
				var maxValue = Math.Min(startNumber, USCustomsNumberViewStmNumsSetting.USMaximumFormalEntryNumber);
				setting.AddForTesting(branch.PK, minValue, maxValue);
			}
			return setting.GetFirstAvailableOrLastSequence(mainBranch);
		}

		public static void SetEntryFilerIDDetails(ZString entryFilerID, ZString entryFilerIDType)
		{
			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = entryFilerID;
			filer.EntryFilerIDType = entryFilerIDType;
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
		}

		public static void SetEntryFilerCode(ZString entryFilerCode)
		{
			EntryFiler filer = new EntryFiler();
			filer.EntryFilerCode = entryFilerCode;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
		}

		public static void SetBRecordOfficeCode(string officeCode)
		{
			USCustomsDataRegistry.Instance.BRecordOfficeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, officeCode);
		}

		public static IDisposable SetReciprocalFlagForCurrentCompany(bool isReciprocal)
		{
			return new ReciprocalSetter(isReciprocal);
		}

		class ReciprocalSetter : IDisposable
		{
			public ReciprocalSetter(bool isReciprocal)
			{
				originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
				GlbCompany.CurrentCompany.GC_IsReciprocal = isReciprocal;

				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
				try
				{
					GlbCompany.CurrentCompany.Factory.Save();
				}
				finally
				{
					((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
				}
			}

			readonly bool originalReciprocal;

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
				try
				{
					GlbCompany.CurrentCompany.Factory.Save();
				}
				finally
				{
					((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
				}
			}

			#endregion
		}

		public static void SetProcessingDistrictPortCode(ZString processingDistrictPortCode)
		{
			SetProcessingDistrictPortCode(processingDistrictPortCode, GlbBranch.CurrentBranch);
		}

		public static void SetProcessingDistrictPortCode(ZString processingDistrictPortCode, GlbBranch branch)
		{
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, processingDistrictPortCode);
		}

		public static void SetPreparerOfficeCode(ZString preparerOfficeCode)
		{
			USCustomsDataRegistry.Instance.BRecordOfficeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, preparerOfficeCode);
		}

		public static JobDeclaration GetDeclarationForeBond(BusinessObjectFactory factory)
		{
			var groupX1 = factory.New<GlbGroup>();
			groupX1.GG_Code = "X9";
			var staffX1 = groupX1.Staff.AddNew();
			staffX1.GS_Code = "X9";
			staffX1.GS_LoginName = "X9";
			staffX1.GS_EmailAddress = "dong@pretend.email.com";

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_InsuranceAgent = "AAA";
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.JE_GS_NKCusAgent = staffX1.GS_Code;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader.EntryNumber = "EB181120";

			return declaration;
		}

		public JobDeclaration GetMergedDutiableDeclaration(BusinessObjectFactory factory, ZString? entryNumber = null)
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCAName = "Blah";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";
			if (entryNumber.HasValue)
			{
				declaration.ImportEntryNumber = entryNumber.Value;
			}
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition", 45m, invoiceLine.CusEntryLine.DutyAmount);
			return declaration;
		}

		public JobDeclaration GetMergedDutiableDeclarationForACE(BusinessObjectFactory factory)
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCAName = "Blah";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition", 45m, invoiceLine.CusEntryLine.DutyAmount);
			return declaration;
		}

		public static JobDeclaration GetMergedDeclarationWithFDALines(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VesselName = "APL VESSEL TESTING";
			declaration.JE_VoyageFlightNo = "E123";
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			declaration.FDAStatus = FDAEntryLevelDispositionCodeList.Codes._02;

			var broker = factory.New<GlbStaff>();
			broker.GS_Code = "OOO";
			broker.GS_FullName = "TEST BROKER 1";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			var invoice1 = declaration.Invoices.AddNew();

			var inv1_line1 = invoice1.InvoiceLines.AddNew();
			inv1_line1.US_SupTariff = "9801001049";//no fda requirement
			inv1_line1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			inv1_line1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			inv1_line1.JI_Tariff = "8512300030";// radar detector, no FDA requirement
			inv1_line1.JI_Description = "COMMERCIAL DESCRIPTION";
			inv1_line1.US_SPI = Core.Constants.CountryCodes.Australia;

			var inv1_line2 = invoice1.JobComInvoiceLines.AddNew();
			inv1_line2.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			inv1_line2.JI_LinePrice = 1200;
			inv1_line2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Italy;

			var inv1_line2_fda1 = inv1_line2.FDAs.AddNew();
			inv1_line2_fda1.US_FDAValue = 1000m;
			inv1_line2_fda1.US_FDALineNo = 1;

			var inv1_line2_fda2 = inv1_line2.FDAs.AddNew();
			inv1_line2_fda2.US_FDAValue = 200m;
			inv1_line2_fda2.US_FDALineNo = 2;
			inv1_line2_fda2.US_FDAConfirmDate = ZDateTime.Today;
			inv1_line2_fda2.US_PNC = "123456789654";

			var manufacturer = factory.New<OrgHeader>();
			inv1_line2_fda2.US_FDAManufacturerAddress = manufacturer.MainAddress.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var inv2_line1 = invoice2.JobComInvoiceLines.AddNew();
			inv2_line1.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			inv2_line1.JI_LinePrice = 1200;
			var inv2_line1_fda1 = inv2_line1.FDAs.AddNew();
			inv2_line1_fda1.US_FDAValue = 1000m;
			inv2_line1_fda1.US_FDALineNo = 1;

			var inv2_line2 = invoice2.JobComInvoiceLines.AddNew();
			inv2_line2.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			inv2_line2.JI_LinePrice = 500;
			var inv2_line2_fda1 = inv2_line2.FDAs.AddNew();
			inv2_line2_fda1.US_FDAValue = 1000m;
			inv2_line2_fda1.US_FDALineNo = 1;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}

		public ReconDeclaration GetDutiableReconDeclaration(BusinessObjectFactory factory)
		{
			JobDeclaration declaration = GetMergedDutiableDeclaration(factory, "~7854378");
			factory.Save();//to serialise AddInfo to copied below

			JobDeclaration declaration2 = factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration2);
			ReconOriginalEntryHeader reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~7854378";

			new ReconImportEntryRetriever(reconDec).ImportLines();
			reconEntry.US_R_DutyRateDate = ZDateTime.Today;

			return reconDec;
		}

		public JobDeclaration CreateSimpleImportDeclaration(BusinessObjectFactory factory)
		{
			var result = factory.New<JobDeclaration>();
			SetUpImportDeclarationData(result);
			CreateAndSetUpBillData(result);
			CreateAndInvoiceDetails(result);
			return result;
		}

		[NUnit.Framework.TestDate(2006, 11, 29)]
		public Mock<JobDeclaration> CreateSimpleImportDeclarationMoq(BusinessObjectFactory factory)
		{
			Mock<JobDeclaration> result = factory.NewMoq<JobDeclaration>();
			SetUpImportDeclarationData(result.Object);
			CreateAndSetUpBillData(result.Object);
			CreateAndInvoiceDetails(result.Object);
			return result;
		}

		[NUnit.Framework.TestDate(2006, 11, 29)]
		public JobDeclaration FillInTestDataForInBondMessaging(BusinessObjectFactory factory)
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.US_EnableINB = true;
			declaration.US_EntryType = "";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 11, 29);
			SetUpImportDeclarationData(declaration);
			declaration.Transports[0].JW_IsLinked = false;
			CreateAndSetUpBillData(declaration);
			return declaration;
		}

		[NUnit.Framework.TestDate(2006, 11, 29)]
		public void SetUpImportDeclarationData(JobDeclaration declaration)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "54901", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTransportModeForCusCodeList(port.PK, TransportTypeList.Codes.Sea);
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			Factory.Save();

			BusinessObjectFactory factory = declaration.Factory;
			RefVessel vessel = factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, "APL EMERALD TESTTESTTEST");
			if (vessel == null)
			{
				vessel = factory.New<RefVessel>();
				vessel.RV_Code = "APL EMERALD TESTTESTTEST";
				vessel.RV_LloydsNumber = "9077123";
			}

			usCarrier = factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "AAAS"));
			if (usCarrier == null)
			{
				usCarrier = factory.New<USCarrierCombined>();
				usCarrier.UI_Code = "AAAS";
			}
			usCarrier.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;

			RefUNLOCO loco = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "TH123");
			if (loco == null)
			{
				loco = factory.New<RefUNLOCO>();
				loco.RL_Code = "TH123";
				RefLocoMap locoMap = loco.RefLocoMaps.AddNew();
				locoMap.RY_LocalPortCode = "54901";
				locoMap.RY_RN = GlbCompany.CurrentCompany.Country.PK;
				locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
				locoMap.RY_IsSystem = true;
			}
			loco = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "US456");
			if (loco == null)
			{
				loco = factory.New<RefUNLOCO>();
				loco.RL_Code = "US456";
				RefLocoMap locoMap = loco.RefLocoMaps.AddNew();
				locoMap.RY_LocalPortCode = "3901";
				locoMap.RY_RN = GlbCompany.CurrentCompany.Country.PK;
				locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
				locoMap.RY_IsSystem = true;
			}

			OrgHeader inbondCarrier = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "INBCARTEST1");
			if (inbondCarrier == null)
			{
				inbondCarrier = factory.New<OrgHeader>();
				inbondCarrier.OH_Code = "INBCARTEST1";
				inbondCarrier.FillWithValidTestData();
				inbondCarrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "69-9999999JC");
				inbondCarrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, usCarrier.UI_Code);
			}

			OrgHeader shippingLine = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SHPIMPTST1");
			if (shippingLine == null)
			{
				shippingLine = factory.New<OrgHeader>();
				shippingLine.FillWithValidTestData();
				shippingLine.OH_Code = "SHPIMPTST1";
				shippingLine.OH_IsShippingLine = true;
				shippingLine.OH_IsShippingProvider = true;
				shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, usCarrier.UI_Code);
				shippingLine.OH_RL_NKClosestPort = "THBKK";
			}

			OrgHeader supplier = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SUPIMPTST1");
			if (supplier == null)
			{
				supplier = factory.New<OrgHeader>();
				supplier.OH_Code = "SUPIMPTST1";
				supplier.FillWithValidTestData();
				supplier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "72-9999999JC");
			}

			OrgHeader importer = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPIMPTST1");
			if (importer == null)
			{
				importer = factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				importer.OH_Code = "IMPIMPTST1";
				importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "89-9999999JC");
				importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				JobRequiredDocument poaDocument = importer.RequiredDocuments.AddNew("POA");
				poaDocument.EQ_DocType = "POA";
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(60);
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			}

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_InbondType = EntryTypeList.Codes.ImmediateTransportation;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_TotalNoOfPacksPackType = "";
			declaration.JE_VesselName = vessel.RV_Code;
			declaration.JE_VoyageFlightNo = "00101";
			declaration.JE_RL_NKPortOfLoading = "TH123";
			declaration.US_SchDArrival = "8888";
			declaration.US_SchDLoading = "54901";
			declaration.US_ImportConveyanceName = "ADMIRALENGRACHT";
			declaration.US_SchDUSDestination = "3901";
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.US_EntryDate = ZDateTime.Today.AddDays(1);

			CusContainer container = declaration.CusContainers.AddNew();
			RefContainer refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "FRTST1");
			if (refContainer == null)
			{
				refContainer = factory.New<RefContainer>();
				refContainer.RC_Code = "FRTST1";
				refContainer.SetCountrySpecificContainerCode("FR", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			}
			container.CO_RC = refContainer.PK;
			container.CO_ContainerNumber = "CRUX1234562";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		}

		void CreateAndSetUpBillData(JobDeclaration declaration)
		{
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "001821004";
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
			bill.US_UI_NKBillIssuerSCAC = usCarrier.UI_Code;

			bill.US_Weight = 100m;
			bill.US_WeightUQ = "KG";
			bill.US_Volume = 100m;
			bill.US_VolumeUQ = "M3";

			bill.CU_NoOfPacks = 10;
			bill.CU_PackType = BillUS_ManifestUQForTest;
			bill.US_GoodsValueInLocalCurrency = 15000m;

			bill.ConsigneeAddress.E2_AddressOverride = true;
			bill.ConsigneeAddress.E2_CompanyName = "FREEMAN DECORATING COMPANY";
			bill.ConsigneeAddress.E2_Address1 = "5040 W. ROOSEVELT ROAD,";
			bill.ConsigneeAddress.E2_Address2 = "CHICAGO, ILLINOIS, 60650, U.S.A.";
			bill.ConsigneeAddress.E2_City = "Chicago";
			bill.ConsigneeAddress.E2_RN_NKCountryCode = "US";
			bill.ConsigneeAddress.E2_Postcode = "61101";
			bill.ConsigneeAddress.E2_State = "IL";

			bill.ForeignShipperAddress.E2_AddressOverride = true;
			bill.ForeignShipperAddress.E2_CompanyName = "ADVANCED INTERNATIONAL FREIGHT";
			bill.ForeignShipperAddress.E2_Address1 = "27 FLORENCE STRET";
			bill.ForeignShipperAddress.E2_Address2 = "FORTITUDE";
			bill.ForeignShipperAddress.E2_City = "VALLEY";
			bill.ForeignShipperAddress.E2_State = "QLD";
			bill.ForeignShipperAddress.E2_RN_NKCountryCode = "AU";
			bill.ForeignShipperAddress.E2_Postcode = "4008";

			Package package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = declaration.CusContainers[0].CO_ContainerNumber;
			package.CW_HouseBill = bill.CU_BillUniqueCode;
		}
		internal const string BillUS_ManifestUQForTest = ShippingOrPackingingUnitList.Codes.Package;

		void CreateAndInvoiceDetails(JobDeclaration declaration)
		{
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST INVOICE";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 5000m, Core.Constants.CurrencyCodes.UnitedStates);

			OrgHeader consignee = declaration.Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789");
			invoice.JZ_OA_ConsigneeAddress = consignee.MainAddress.PK;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;

			invoice.JZ_RN_NKDefaultOrigin = "AU";
			invoice.US_UC_NKCountryOfExport = "AU";
			invoice.US_DateOfExport = new ZDateTime(2007, 4, 13);

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4903.00.00 00";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_Description = "Children book";
			invoiceLine.JI_InvoiceQuantity = 1500m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.US_SecondarySPI = "X";
			invoiceLine.US_SPI = "AU";
			invoiceLine.JI_LinePrice = 15000m;
		}

		public JobDeclaration CreateExportDeclaration(ZString transportMode)
		{
			var scheduleB1 = ScheduleB8714950000;
			var scheduleB2 = ScheduleB2401105130;
			var scheduleB3 = ScheduleB2401105160;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = Consignor.PK;
			declaration.SupplierDocumentaryAddress.ContactPK = ConsignorContact.PK;
			declaration.JE_OH_Importer = Consignee.PK;
			declaration.ImporterDocumentaryAddress.ContactPK = ConsigneeContact.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = transportMode;
			declaration.US_InbondType = InbondTypeList.Codes.IEWarehouseWithdrawal;
			declaration.US_ImportEntryNo = "IMP123456";
			declaration.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			declaration.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			declaration.JE_RL_NKPortOfLoading = USLAX.Code;
			declaration.JE_ExportDate = ZDateTime.Now;
			declaration.US_SchDArrival = "60267";
			declaration.US_EntryDate = declaration.JE_ExportDate.AddDays(10);
			declaration.US_SchDExport = "3901";
			declaration.US_DateOfExport = declaration.JE_ExportDate.AddDays(1);
			ZString reference = declaration.JE_ExportDate.ToString("yyyyMMdd");
			declaration.JE_HouseBill = "H" + reference;
			declaration.JE_MasterBill = "M" + reference;
			declaration.US_TransportReference = "B" + reference;
			declaration.US_StateOfOrigin = UnitedStates.States[5].RW_Code;
			declaration.US_ForeignTradeZone = "FTZ";
			declaration.JE_GoodsDescription = "MOTORCAR PARTS";
			declaration.JE_OwnerRef = "OWNREF12";
			declaration.US_LicenseType = USAESLicenseCode.Codes.C40;
			declaration.US_LicenseNo = "LICC40";
			declaration.US_ECCN = "1E345";
			declaration.US_ExportCode = ExportInformationCodeList.Codes.OS;
			declaration.JE_TotalWeight = 2500.500m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalVolume = 12.400m;
			declaration.JE_TotalVolumeUnit = "M3";
			declaration.JE_ContainerCount = 1;
			declaration.JE_TotalNoOfPacks = 100;
			declaration.JE_TotalNoOfPacksPackType = AESUnitOfMeasureList.Codes.Pieces;
			declaration.JE_OH_ShippingLine = ShippingLine.PK;
			declaration.JE_OH_Forwarder = ExportForwarder.PK;
			declaration.JE_OH_Consignee = IntermediateConsignee.PK;

			JobComInvoiceHeader invoice1 = CreateInvoice(declaration, "INVOICE1", 8000m, USD.RX_Code);
			invoice1.US_HazardousCargo = YesNoDefaultList.Codes.No;
			JobComInvoiceLine invoice1Line1 = CreateInvoiceLine(invoice1, scheduleB1, 40, 2000m, 400m, "KG", 1.5m, "M3");
			JobComInvoiceLine invoice1Line2 = CreateInvoiceLine(invoice1, scheduleB2, 19, 6000m, 200.5m, "KG", 1.4m, "M3");
			invoice1Line2.JI_CustomsSecondUnitQty = scheduleB2.ZZ1_ZZ8_UQ2;
			invoice1Line2.JI_CustomsSecondQuantity = 1400m;
			invoice1Line2.US_ExportCode = ExportInformationCodeList.Codes.OI;
			invoice1Line2.US_ECCN = "2E345";
			invoice1Line2.US_LicenseType = USAESLicenseCode.Codes.C40;
			invoice1Line2.US_LicenseNo = "LICC39";

			JobComInvoiceHeader invoice2 = CreateInvoice(declaration, "INVOICE2", 12000m, USD.RX_Code);
			JobComInvoiceLine invoice2Line1 = CreateInvoiceLine(invoice2, scheduleB3, 1, 10000m, 1500m, "KG", 8m, "M3");
			UpdateInvoiceLineVehicleDetail(invoice2Line1, VehicleIDTypeList.Codes.VIN, "VIN123456", "TITLE1232", UnitedStates.States[4].RW_Code);
			invoice2Line1.US_LicenseType = USAESLicenseCode.Codes.SCA;
			invoice2Line1.US_ExportCode = ExportInformationCodeList.Codes.OS;
			invoice2Line1.US_DDTCITARExemptionNo = "123.16B3";
			invoice2Line1.US_DDTCRegistrationNo = "REG234";
			invoice2Line1.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
			invoice2Line1.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.No;
			invoice2Line1.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.ClassifiedArticlesTechnicalDataAndDefenseServicesNotOtherwiseEnumerated;
			invoice2Line1.US_DDTCUnit = DDTCUnitOfMeasureList.Codes.Lots;
			invoice2Line1.US_DDTCQuantity = 120m;

			JobComInvoiceLine invoice2Line2 = CreateInvoiceLine(invoice2, scheduleB1, 40, 2000m, 400m, "KG", 1.5m, "M3");
			return declaration;
		}

		public void UpdateInvoiceLineVehicleDetail(JobComInvoiceLine invoiceLine, ZString vehicleIDType, ZString vehicleID, ZString vehicleTitleNo, ZString vehicleTitleState)
		{
			invoiceLine.US_IsUsedVehicle = true;
			invoiceLine.US_VehicleIDType = vehicleIDType;
			invoiceLine.US_VehicleID = vehicleID;
			invoiceLine.US_VehicleTitleNo = vehicleTitleNo;
			invoiceLine.US_VehicleTitleState = vehicleTitleState;
		}

		public JobComInvoiceHeader CreateInvoice(JobDeclaration declaration, ZString invoiceNo, ZDecimal invoiceAmount, ZString invoiceCurrency)
		{
			JobComInvoiceHeader invoice = (declaration != null) ? declaration.Invoices.AddNew() : Factory.New<JobComInvoiceHeader>();
			invoice.JZ_InvoiceNumber = invoiceNo;
			invoice.JZ_InvoiceAmount = invoiceAmount;
			invoice.JZ_RX_NKInvoice_Currency = invoiceCurrency;
			return invoice;
		}

		public JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoice, TariffView tariff, ZDecimal quantity, ZDecimal linePrice, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ)
		{
			JobComInvoiceLine line = (invoice != null) ? invoice.JobComInvoiceLines.AddNew() : Factory.New<JobComInvoiceLine>();
			line.JI_Tariff = tariff.ZZ1_TariffCode;
			line.JI_Description = "COMMERCIAL DESCRIPTION";
			line.JI_InvoiceQuantity = quantity;
			line.JI_InvoiceUQ = tariff.ZZ1_ZZ8_UQ1;
			line.JI_CustomsQuantity = quantity;
			line.JI_CustomsUnitQty = tariff.ZZ1_ZZ8_UQ1;
			if (!line.JI_CustomsSecondUnitQty.IsEmpty)
			{
				line.JI_CustomsSecondQuantity = quantity;
			}
			line.JI_LinePrice = linePrice;
			line.JI_Description = tariff.ZZ1_Description;
			line.JI_Weight = weight;
			line.JI_WeightUQ = weightUQ;
			line.JI_Volume = volume;
			line.JI_VolumeUQ = volumeUQ;
			line.US_MarksAndNumbers = line.JI_InvoiceQuantity.ToString() + " " + line.JI_InvoiceUQ;
			line.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			return line;
		}

		public static PGA CreateLaceyActData(PGA pGA)
		{
			pGA.US_PGACommercialDescription = "Softwood Pulpwood";
			pGA.US_PGALineItemNumber = 1;
			pGA.US_InvCurrPGAValue = 10000m;
			pGA.US_PGALineValue = 10000m;

			var constituentElement = pGA.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Spruce";
			constituentElement.US_PGAPercentOfConstituentElement = 20m;
			constituentElement.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement.US_PGAUnitOfMeasure = "M3";

			var scientificData = constituentElement.ScientificDataCollection.AddNew();
			scientificData.US_PGAScientificGenusName = "PICEA";
			scientificData.US_PGAScientificSpeciesName = "GLAUCA";
			scientificData.US_PGACountryCode = "CA";
			return pGA;
		}

		public static NMFSLine SetupNMFSCOAData(NMFSLine nmfsLine, JobComInvoiceLine line, BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, TariffConditionValueTypes.Codes.PGA);
			factory.Save();
			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "1010101010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, zzTariff.PK, "test", true, false, ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, GovernmentAgencyProgramCodeList.Codes.COA);
			factory.Save();

			var tariff = factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			line.JI_Tariff = "1010101010";
			line.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;

			nmfsLine.US_LineNo = 1;
			nmfsLine.US_Confidential = true;
			nmfsLine.US_SpeciesCode = "ADD";
			nmfsLine.US_SourceType = "HCF";
			nmfsLine.US_IFTPPermitNumber = "123456789";
			var details = nmfsLine.HarvestingDetails.AddNew();
			details.US_GearStartDate = new ZDateTime(2023, 02, 15);
			details.US_GearType = "GIL";
			details.US_HarvestedCountry = "AU";
			details.US_OceanAreaOfCatch = "CAR";
			return nmfsLine;
		}

		public static void AddContainerForInvoiceLine(JobComInvoiceLine invoiceLine, ZString containerNumber, bool isForInvoiceLine)
		{
			var container = invoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerNumber).IsForInvoiceLine = isForInvoiceLine;
		}

		public static void AssociateContainerWithLaceyActLine(PGA pga, ZString containerNumber)
		{
			var relatedContainer = pga.ContainersForInvoiceLine.FindByContainerNumber(containerNumber);
			relatedContainer.IsForPGALine = true;
		}

		public JobDeclaration CreateSeaExportDeclaration()
		{
			JobDeclaration dec = CreateExportDeclaration(TransportTypeList.Codes.Sea);
			dec.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			dec.JE_VesselName = VesselWithCountry.RV_Code;
			dec.JE_VoyageFlightNo = "E123";
			CusContainer cusContainer = CreateCusContainer(dec, "TURE1234560", "SL123", Container40US, Core.Constants.ContainerModes.FCL);
			LinkInvoiceWithContainer(dec.Invoices[0], cusContainer);
			cusContainer = CreateCusContainer(dec, "DGFS1234560", "SL453", Container20US, Core.Constants.ContainerModes.BreakBulk);
			LinkInvoiceWithContainer(dec.Invoices[1], cusContainer);
			return dec;
		}

		public void LinkInvoiceWithContainer(JobComInvoiceHeader invoice, CusContainer container)
		{
			foreach (JobComInvoiceLine line in invoice.JobComInvoiceLines)
			{
				foreach (Customs.Business.NonPersistentCusContainer lineContainer in line.ContainersForInvoiceLinesForBindingOnly)
				{
					Customs.Business.BaseCusContainer baseContainer = lineContainer.Container;
					if (baseContainer != null && baseContainer.PK == container.PK)
					{
						lineContainer.IsForInvoiceLine = true;
						break;
					}
				}
			}
		}

		public CusContainer CreateCusContainer(JobDeclaration dec, ZString containerNo, ZString sealNo, RefContainer container, ZString mode)
		{
			CusContainer cusContainer = (dec != null) ? dec.CusContainers.AddNew() : Factory.New<CusContainer>();
			cusContainer.CO_ContainerNumber = containerNo;
			cusContainer.CO_Seal = sealNo;
			cusContainer.CO_RC = container.PK;
			cusContainer.CO_FCL_LCL_AIR = mode;
			return cusContainer;
		}

		public JobDeclaration CreateAirExportDeclaration()
		{
			JobDeclaration dec = CreateExportDeclaration(TransportTypeList.Codes.Air);
			dec.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			dec.JE_VoyageFlightNo = "QF1233";
			return dec;
		}

		public override OrgHeader CreateConsignor()
		{
			OrgHeader org = base.CreateConsignor();
			org.OH_RL_NKClosestPort = USCHI.Code;
			org.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.Tariff;
			UpdateOrAddCustomsRegNo(org, "45-1234487", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, UnitedStates);
			org.MainAddress.OA_PostCode = "62522";
			return org;
		}

		public override OrgHeader CreateConsignee()
		{
			OrgHeader org = base.CreateConsignee();
			org.OH_RL_NKClosestPort = AUSYD.Code;
			return org;
		}

		public override OrgHeader CreateExportForwarder()
		{
			OrgHeader org = base.CreateExportForwarder();
			org.OH_RL_NKClosestPort = USLAX.Code;
			org.MainAddress.OA_PostCode = "90054";
			UpdateOrAddCustomsRegNo(org, "85-5688579", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, UnitedStates);
			return org;
		}

		public override OrgHeader CreateImportForwarder()
		{
			OrgHeader org = base.CreateImportForwarder();
			org.OH_RL_NKClosestPort = NZAKL.Code;
			return org;
		}

		public OrgHeader CreateOrganisation(ZString threeLetterCode, ZString fullName, ZString closestPort, ZString address1, ZString city, ZString phone)
		{
			OrgHeader org = CreateOrganisation(fullName, closestPort, address1, city, phone);
			org.OH_Code = threeLetterCode + new Random().Next(1000000).ToString();
			return org;
		}

		#region IntermediateConsignee
		public OrgHeader IntermediateConsignee
		{
			get
			{
				if (fConsignor == null)
				{
					fConsignor = CreateIntermediateConsignee();
				}
				return fConsignor;
			}
		}
		OrgHeader fConsignor;

		public virtual OrgHeader CreateIntermediateConsignee()
		{
			OrgHeader org = CreateOrganisation("INTERMEDIATE CONSIGNEE NAME", AUSYD.Code);
			org.OH_IsConsignee = true;
			return org;
		}
		#endregion

		#region IntermediateConsigneeContact
		public OrgContact IntermediateConsigneeContact
		{
			get
			{
				if (fIntermediateConsigneeContact == null)
				{
					fIntermediateConsigneeContact = UpdateContact(IntermediateConsignee.Contacts.AddNew(), "INTERMEDIATE CONSIGNEE CONTACT");
				}
				return fIntermediateConsigneeContact;
			}
		}
		OrgContact fIntermediateConsigneeContact;
		#endregion

		#region AESResponseCode

		#region C055
		public USCAESResponseCode C055
		{
			get
			{
				if (c055 == null)
				{
					c055 = GetAESResponseCodeCreateNewIfNotExists("055", "INFORMATIONAL", "USPPI ID/TYPE: -USPPI ID/Type-");
				}
				return c055;
			}
		}
		USCAESResponseCode c055;
		#endregion

		#region C056
		public USCAESResponseCode C056
		{
			get
			{
				if (c056 == null)
				{
					c056 = GetAESResponseCodeCreateNewIfNotExists("056", "INFORMATIONAL", "SHIPMENT REFERENCE NBR:-Shipment Ref Number- ");
				}
				return c056;
			}
		}
		USCAESResponseCode c056;
		#endregion

		#region C700
		public USCAESResponseCode C700
		{
			get
			{
				if (c700 == null)
				{
					c700 = GetAESResponseCodeCreateNewIfNotExists("700", "COMPLIANCE", "SHIPMENT REPORTED LATE; OPT 2");
				}
				return c700;
			}
		}
		USCAESResponseCode c700;
		#endregion

		#region C960
		public USCAESResponseCode C960
		{
			get
			{
				if (c960 == null)
				{
					c960 = GetAESResponseCodeCreateNewIfNotExists("960", "FATAL", "BATCH REJECTED; RESOLVE AND RETRANSMIT");
				}
				return c960;
			}
		}
		USCAESResponseCode c960;
		#endregion

		#region C970
		public USCAESResponseCode C970
		{
			get
			{
				if (c970 == null)
				{
					c970 = GetAESResponseCodeCreateNewIfNotExists("970", "FATAL", "SHIPMENT REJECTED; RESOLVE & RETRANSMIT");
				}
				return c970;
			}
		}
		USCAESResponseCode c970;
		#endregion

		#region C971
		public USCAESResponseCode C971
		{
			get
			{
				if (c971 == null)
				{
					c971 = GetAESResponseCodeCreateNewIfNotExists("971", "WARNING", "SHIPMENT ADDED; MUST CORRECT WARNINGS");
				}
				return c971;
			}
		}
		USCAESResponseCode c971;
		#endregion

		#region C972
		public USCAESResponseCode C972
		{
			get
			{
				if (c972 == null)
				{
					c972 = GetAESResponseCodeCreateNewIfNotExists("972", "VERIFY", "SHIPMENT ADDED; MUST VERIFY");
				}
				return c972;
			}
		}
		USCAESResponseCode c972;
		#endregion

		#region C973
		public USCAESResponseCode C973
		{
			get
			{
				if (c973 == null)
				{
					c973 = GetAESResponseCodeCreateNewIfNotExists("973", "COMPLIANCE", "SHIPMENT ADDED; COMPLIANCE ALERT");
				}
				return c973;
			}
		}
		USCAESResponseCode c973;
		#endregion

		#region C974
		public USCAESResponseCode C974
		{
			get
			{
				if (c974 == null)
				{
					c974 = GetAESResponseCodeCreateNewIfNotExists("974", "NOTIFICATION", "SHIPMENT ADDED");
				}
				return c974;
			}
		}
		USCAESResponseCode c974;
		#endregion

		#region C977
		public USCAESResponseCode C977
		{
			get
			{
				if (c977 == null)
				{
					c977 = GetAESResponseCodeCreateNewIfNotExists("977", "COMPLIANCE", "SHIPMENT REPLACED; COMPLIANCE ALERT");
				}
				return c977;
			}
		}
		USCAESResponseCode c977;
		#endregion

		#region C978
		public USCAESResponseCode C978
		{
			get
			{
				if (c978 == null)
				{
					c978 = GetAESResponseCodeCreateNewIfNotExists("978", "NOTIFICATION", "SHIPMENT REPLACED");
				}
				return c978;
			}
		}
		USCAESResponseCode c978;
		#endregion

		public USCAESResponseCode GetAESResponseCodeCreateNewIfNotExists(ZString code, ZString severity, ZString narrativeText)
		{
			USCAESResponseCode result = Factory.LoadFromNaturalKey<USCAESResponseCode>(USCAESResponseCodeSchema.UY_Code, code);
			if (result == null)
			{
				result = Factory.New<USCAESResponseCode>();
				result.UY_Code = code;
				result.UY_Severity = severity;
				result.UY_NarrativeText = narrativeText;
			}
			return result;
		}
		#endregion

		#region HTS Tariff

		#region TariffHelper
		public USCTariffTestCase TariffHelper
		{
			get
			{
				if (fTariffHelper == null)
				{
					fTariffHelper = new USCTariffTestCase(Factory);
				}
				return fTariffHelper;
			}
		}
		USCTariffTestCase fTariffHelper;
		#endregion

		public USCTariff CreateNewTariffIfNotExists(ZString tariff, ZDateTime dateFrom, ZDateTime dateTo, ZString unit1, ZString unit2, ZString unit3, ZString shortDescription)
		{
			return TariffHelper.CreateNewTariffIfNotExists(tariff, dateFrom, dateTo, unit1, unit2, unit3, shortDescription);
		}

		public USCTariff Tariff2710119000
		{
			get { return TariffHelper.Tariff2710119000; }
		}

		public USCTariff Tariff8703105030
		{
			get { return TariffHelper.Tariff8703105030; }
		}

		public void MessageMustContainElement<T>(MQEDIMessage message) where T : MessageBlock
		{
			Assert("Message should contain this block", GetTypes<T>(message).Count > 0);
		}

		public void MessageMustContainElement<T>(JobDeclaration declaration) where T : MessageBlock
		{
			MessageMustContainElement<T>((MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0]);
		}

		public void MessageMustNotContainElement<T>(MQEDIMessage message) where T : MessageBlock
		{
			Assert("Message should not contain this block", GetTypes<T>(message).Count == 0);
		}

		public void MessageMustNotContainElement<T>(JobDeclaration declaration) where T : MessageBlock
		{
			MessageMustNotContainElement<T>((MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0]);
		}

		public List<T> GetTypes<T>(JobDeclaration declaration) where T : MessageBlock
		{
			MQEDIMessage message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];
			return GetTypes<T>(message);
		}

		public List<T> GetTypes<T>(MQEDIMessage mqMessage) where T : MessageBlock
		{
			List<T> result = new List<T>();
			ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			string message = mqMessage.EM_MessageText;
			block.Deserialise(BlockPadder.Pad(message));
			if (typeof(T) == typeof(APLB))
			{
				result.Add((T)(object)block.B);
			}
			else if (typeof(T) == typeof(APLY))
			{
				result.Add((T)(object)block.Y);
			}
			else
			{
				foreach (MessageBlock mb in block.MessageBlocks)
				{
					T mbasT = mb as T;
					if (mbasT != null)
					{
						result.Add(mbasT);
					}
				}
			}
			return result;
		}

		public void MessageMustContain(JobDeclaration declaration, params string[] strings)
		{
			foreach (string s in strings)
			{
				AssertEquals("Message contains " + s, true, declaration.CustomsEntryHeaders[0].Messages[0].EM_MessageText.Contains(s));
			}
		}

		public void SetUpFDARequiredData(JobComInvoiceLine invoiceLine, OrgHeader manufacturer, BusinessObjectFactory factory)
		{
			FDA fda = invoiceLine.FDAs.AddNew();

			OrgHeader fdaOrg = CreateOrganisation("FDA", "MR FDA", USLAX.Code, "21 BOB STREET", "BUILDER", "18686884");
			invoiceLine.Declaration.US_FDAContactName = "Jessie Jam";
			invoiceLine.Declaration.US_FDAContactPhoneNo = "3273958841";
			invoiceLine.Declaration.US_FDAContactEmail = "jessie.james@longcompanyname.com";

			fda.US_FDACommercialDesc = "TEST";

			var newFactory = new BusinessObjectFactory();
			var testHelper = new UniversalReferenceTestDataHelper(newFactory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType,
			"24DCS18", "ALFALFA BEANS (SEEDS), JUICE OR DRINK;GLASS;ULTRAPASTEURIZED", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var fdaProduct = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "24DCS18", Core.Constants.CountryCodes.UnitedStates, listType, ZDateTime.Today);
			fda.US_FDAProductCode = fdaProduct.ZZD_Code;
			fda.US_UC_NKFDAProduction = Core.Constants.CountryCodes.Australia;

			fda.US_FDAQty1 = 9000m;
			fda.US_FDAMeasure1 = FDABaseUQList.Codes.KG;

			if (invoiceLine.JI_LinePrice > 0)
			{
				fda.US_InvCurrFDAValue = 1;
			}

			var fdaBillsAvailable = fda.BillsAvailable;
			if (fdaBillsAvailable.Count > 0)
			{
				fdaBillsAvailable[0].IsForFDALine = true;
			}

			fda.US_PFR = "12345678901";
			fda.US_PFT = ProducerFirmTypeList.Codes.G;
			if (invoiceLine.ConsigneeOrgAddress != null)
			{
				invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			}

			if (manufacturer != null)
			{
				OrgHeader importer = invoiceLine.Declaration.Importer;
				OrgHeader supplier = manufacturer;
				if (supplier.BuyerLinks.Count == 0)
				{
					OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew();
					link.OL_OH_Buyer = importer.PK;
					link.OL_RelatedParty = "N";
				}

				JobComInvoiceHeader invoice = invoiceLine.InvoiceHeader;
				ZString oldCountryOfOrigin = invoice.US_UC_NKCountryOfOrigin;
				ZString oldCountryOfExport = invoice.US_UC_NKCountryOfExport;
				invoice.JZ_OH_Supplier = manufacturer.PK;
				invoice.US_UC_NKCountryOfOrigin = oldCountryOfOrigin;
				invoice.US_UC_NKCountryOfExport = oldCountryOfExport;
			}

			ZString mid = ((IFDALine)fda).ManufacturerNumber;

			if (!mid.IsEmpty)
			{
				fda.US_UC_NKFDAProduction = mid.Left(2);
			}
		}

		#endregion

		#region Schedule B

		#region ScheduleBHelper
		public UniversalReferenceTestDataHelper ScheduleBHelper
		{
			get
			{
				if (fScheduleBHelper == null)
				{
					fScheduleBHelper = new UniversalReferenceTestDataHelper(Factory);
				}
				return fScheduleBHelper;
			}
		}
		UniversalReferenceTestDataHelper fScheduleBHelper;
		#endregion

		RefCusTariffType ScheduleTariffType =>
			ScheduleBHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);

		public TariffView CreateNewScheduleBIfNotExists(ZString tariff, ZString unit1, ZString unit2, ZString description)
		{
			var type = ScheduleTariffType;
			Factory.Save();
			var scheduleB = ScheduleBHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, type.PK, tariff, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			if (!description.IsEmpty && scheduleB.ZZ1_Description.IsEmpty)
			{
				scheduleB.ZZ1_Description = description;
			}
			if (!unit1.IsEmpty && scheduleB.ZZ1_ZZ8_UQ1.IsEmpty)
			{
				ScheduleBHelper.CreateTariffUOM(scheduleB, "CU1", unit1);
			}
			if (!unit2.IsEmpty && scheduleB.ZZ1_ZZ8_UQ2.IsEmpty)
			{
				ScheduleBHelper.CreateTariffUOM(scheduleB, "CU2", unit2);
			}
			return scheduleB;
		}

		#region Code2401105130
		public TariffView ScheduleB2401105130
		{
			get
			{
				if (fCode2401105130 == null)
				{
					fCode2401105130 = CreateNewScheduleBIfNotExists("2401105130", AESUnitOfMeasureList.Codes.Kilograms, AESUnitOfMeasureList.Codes.ContentKilograms, "FLUE-CURED CIG LEAF TOB NT STEM/STRIP LT 35% WRPPR");
				}
				return fCode2401105130;
			}
		}
		TariffView fCode2401105130;
		#endregion

		#region Code2401105160
		public TariffView ScheduleB2401105160
		{
			get
			{
				if (fCode2401105160 == null)
				{
					fCode2401105160 = CreateNewScheduleBIfNotExists("2401105160", AESUnitOfMeasureList.Codes.Kilograms, AESUnitOfMeasureList.Codes.ContentKilograms, "BURLEY CIG LEAF TOBACCO NT STEM/STRIP LT 35% WRPPR");
				}
				return fCode2401105160;
			}
		}
		TariffView fCode2401105160;
		#endregion

		#region Code8714950000
		public TariffView ScheduleB8714950000
		{
			get
			{
				if (fCode8714950000 == null)
				{
					fCode8714950000 = CreateNewScheduleBIfNotExists("8714950000", AESUnitOfMeasureList.Codes.Number, "", "SADDLES FOR VEHICLES OF HEADING 8711 TO 8713");
				}
				return fCode8714950000;
			}
		}
		TariffView fCode8714950000;
		#endregion

		#endregion

		internal static MQEDIMessage CreateTransmittedSTUMsg(BusinessObjectFactory factory, ZString msgNum, ZDateTime createDate, ZString msgText)
		{
			var message = factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = msgText;
			message.EM_SystemCreateTimeUtc = createDate;
			message.EM_MessageNum = msgNum;
			return message;
		}

		internal static MQEDIMessage CreateIncomingSTUMsg(BusinessObjectFactory factory, ZString msgNum, ZDateTime createTime, ZString msgText)
		{
			var message = factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = msgText;
			message.EM_SystemCreateTimeUtc = createTime;
			message.EM_MessageNum = msgNum;
			return message;
		}

		internal static void SetupOrganizationsForACEInvoice(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory)
		{
			var importer2 = factory.New<OrgHeader>();
			importer2.OH_Code = "IMP" + new Random().Next(1000000).ToString();
			importer2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "BBBBBB123");
			invoiceHeader.JZ_OA_ConsigneeAddress = importer2.MainAddress.PK;

			var manufacturer = factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Test Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();

			var manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.OA_Address1 = "new address";
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturerAddress.PK;

			var seller = factory.New<OrgHeader>();
			seller.OH_FullName = "Test Selling Party(Seller)";
			seller.OH_Code = "SE" + new Random().Next(1000000).ToString();
			invoiceHeader.JZ_OA_SellerAddress = seller.MainAddress.PK;

			var soldToParty = factory.New<OrgHeader>();
			soldToParty.OH_FullName = "AUTO ELECTRICAL DISTRIBUTORS PTY LTD TEST TEST TES";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			soldToParty.OH_RL_NKClosestPort = "MXGGG";
			soldToParty.MainAddress.OA_Address1 = "UNIT 1, 210 ROBINSON ROAD GEEBUN DOWNTOWN FOR TEST";
			soldToParty.MainAddress.OA_Address2 = "GEEBUNG, QLD GEEBUN DOWNTOWN FOR TEST GEEBUN DOWNT";
			soldToParty.MainAddress.OA_City = "GEEBUN CITY FOR LENGHT TE";
			soldToParty.MainAddress.OA_PostCode = "4034654521";
			invoiceHeader.JZ_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			var shipToParty = factory.New<OrgHeader>();
			shipToParty.OH_FullName = "IAN TEST SHIP TO PARTY";
			shipToParty.OH_Code = "SIP" + new Random().Next(1000000).ToString();
			invoiceHeader.JZ_OA_ShipToPartyAddress = shipToParty.MainAddress.PK;

			var exporter = factory.New<OrgHeader>();
			exporter.OH_FullName = "IAN TEST EXPORTER";
			exporter.OH_Code = "EXP" + new Random().Next(1000000).ToString();
			invoiceHeader.JZ_OA_ExporterAddress = exporter.MainAddress.PK;

			var shipper = factory.New<OrgHeader>();
			shipper.OH_FullName = "IAN TEST SHIPPER";
			shipper.OH_Code = "SHP" + new Random().Next(1000000).ToString();
			invoiceHeader.JZ_OA_ShipperAddress = shipper.MainAddress.PK;

			var distributor = factory.New<OrgHeader>();
			distributor.OH_FullName = "IAN TEST DISTRIBUTOR";
			distributor.OH_Code = "DIS" + new Random().Next(1000000).ToString();
			invoiceHeader.JZ_OA_DistributorAddress = distributor.MainAddress.PK;

			var packager = factory.New<OrgHeader>();
			packager.OH_FullName = "IAN TEST PACKAGER";
			packager.OH_Code = "PKG" + new Random().Next(1000000).ToString();
			invoiceHeader.JZ_OA_PackagerAddress = packager.MainAddress.PK;
		}

		internal static void SetupBillsForSimplifiedEntryDeclaration(JobDeclaration declaration)
		{
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "TestMB1";
			masterBill.US_UI_NKBillIssuerSCAC = "OTT1";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "Test HB1";
			houseBill.US_UI_NKBillIssuerSCAC = "OTT1";

			var subHouseBill = houseBill.ChildBills.AddNew();
			subHouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_BillNum = "Test Sub HB1";
			subHouseBill.US_UI_NKBillIssuerSCAC = "OTT1";
			subHouseBill.CU_NoOfPacks = 130;
			subHouseBill.CU_PackType = "PCS";

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "Test Master Bill 2";
			masterBill2.US_UI_NKBillIssuerSCAC = "XXXW";

			var houseBill2 = masterBill2.ChildBills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "Test House Bill 2";
			houseBill2.US_UI_NKBillIssuerSCAC = "XXXY";
			houseBill2.CU_NoOfPacks = 12;
			houseBill2.CU_PackType = "NN";

			var houseBill3 = declaration.Bills.AddNew();
			houseBill3.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill3.CU_BillNum = "House Bill 3";
			houseBill3.CU_NoOfPacks = 11;
			houseBill3.CU_PackType = "KG";
			houseBill3.US_UI_NKBillIssuerSCAC = "OTT1";
		}

		static OrgContact CreateIfNotExistContactForAllocation(OrgContactDependentCollection contacts, string type)
		{
			var contact = contacts.GetContactForAllocation(type);
			if (contact != null)
			{ return contact; }

			contact = contacts.AddNew();
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = type;
			return contact;
		}

		public static void AddPGAContact(OrgAddress orgAddress, string firstName, string lastName, string phoneNo, string email, string fax, bool isActive = true)
		{
			var wrapper = OrgHeaderWrapper.New(orgAddress);
			var contact = CreateIfNotExistContactForAllocation(wrapper.organisation.Contacts, OrgConstants.ContactAllocationType.USPGA);
			AddContactCore(contact, firstName, lastName, phoneNo, email, fax, isActive);
		}

		public static void AddContactCore(OrgContact contact, string firstName, string lastName, string phoneNo, string email, string fax, bool isActive = true)
		{
			if (firstName != null || lastName != null)
			{
				var firstNameNotNull = firstName ?? string.Empty;
				var lastNameNotNull = lastName ?? string.Empty;
				contact.OC_ContactName = string.Join(" ", firstNameNotNull, lastNameNotNull).Trim();
			}

			if (phoneNo != null)
			{ contact.OC_Phone = phoneNo; }
			if (email != null)
			{ contact.OC_Email = email; }
			if (fax != null)
			{ contact.OC_Fax = fax; }

			contact.OC_IsActive = isActive;
		}

		public static void AddPGAContact(OrgHeader orgHeader, string firstName, string lastName, string phoneNo, string email, string fax, bool isActive = true)
		{
			var wrapper = OrgHeaderWrapper.New(orgHeader);
			var contact = CreateIfNotExistContactForAllocation(wrapper.organisation.Contacts, OrgConstants.ContactAllocationType.USPGA);
			AddContactCore(contact, firstName, lastName, phoneNo, email, fax, isActive);
		}

		public static void AddFSVPContact(OrgAddress orgAddress, string firstName, string lastName, string phoneNo, string email, string fax, bool isActive = true)
		{
			var wrapper = OrgHeaderWrapper.New(orgAddress);
			var contact = CreateIfNotExistContactForAllocation(wrapper.organisation.Contacts, OrgConstants.ContactAllocationType.USFSV);
			AddContactCore(contact, firstName, lastName, phoneNo, email, fax, isActive);
		}

		public const string ValidITNumber1ForTesting = "257700052";
		public const string ValidITNumber2ForTesting = "257700166";
		USCarrierCombined usCarrier;
	}
}
