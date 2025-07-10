using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CanadaForwardingConsolSupportTest : TestCaseWithFactory
	{
		public void TestCanadaCargoControlNumber()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.Country.RN_Code;
			try
			{
				FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.MasterBill);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
				CreateNewConsolWithValidData();
				carrier.CustomsCodes.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Empty number for new consol", "", GetCargoControlNumber(consol));

				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_MasterBillNum = "MB123";
				Factory.Save();
				AssertEquals("Master Bill Number for registry value MBL", "MB1-23", GetCargoControlNumber(consol));

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "MB2";
				consol.Numbers.RemoveAndDeleteAll();
				consol.HasChanges = true;
				Factory.Save();
				AssertEquals("Empty number for not Air", "", GetCargoControlNumber(consol));

				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "XYZA").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
				consol.JK_MasterBillNum = "MB3";
				Factory.Save();
				AssertEquals("Carrier Code + Master Bill Number", "XYZAMB3", GetCargoControlNumber(consol));

				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "MSCU").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
				consol.JK_MasterBillNum = "MSCUMB3";
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Carrier Code + Master Bill Number without Carrier Principal Code", "XYZAMB3", GetCargoControlNumber(consol));

				FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.Non);
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Empty number for registry value NON", "", GetCargoControlNumber(consol));

				consol.JK_RL_NKLoadPort = "CATOR";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Not generated for export", "", GetCargoControlNumber(consol));

				FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.MasterBill);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Afghanistan);
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CATOR";
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Will have CCN when destination starts with CA", "XYZAMB3", GetCargoControlNumber(consol));

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
				CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
				entryTypeList.Add("AAA", (NoResString)"AAA Test Entry Type").IsUnique = true;
				FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

				CreateNewConsolWithValidData();
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "TTG1").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
				consol.JK_RL_NKDischargePort = "CATOR";
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Cargo control number not set when no CCN set in registry", ZString.Empty, GetCargoControlNumber(consol));

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Jamaica);
				entryTypeList = new CustomsReferenceNumberTypeCollection();
				entryTypeList.Add("CCN", (NoResString)"TEST CCN").IsUnique = true;
				FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

				CreateNewConsolWithValidData();
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "TTG2").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
				consol.JK_RL_NKDischargePort = "CABLO";
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Will NOT have CCN as no master bill number exists", ZString.Empty, GetCargoControlNumber(consol));

				consol.JK_MasterBillNum = "MB2";
				Factory.Save();
				AssertEquals("Will have CCN when destination starts with CA and CCN set in registry", "TTG2MB2", GetCargoControlNumber(consol));

				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.Numbers.RemoveAndDeleteAll();
				consol.JK_MasterBillNum = "MB2";
				Factory.Save();
				AssertEquals("Will NOT have CCN as the length of MB is not greater than Airline Prefix", ZString.Empty, GetCargoControlNumber(consol));

				consol.JK_MasterBillNum = "MB2123";
				Factory.Save();
				AssertEquals("Will have CCN when destination starts with CA and CCN set in registry", "MB2-123", GetCargoControlNumber(consol));

				CreateNewConsolWithValidData();
				var transportLeg1 = consol.Transports.AddNew();
				transportLeg1.JW_RL_NKLoadPort = "USPHL";
				transportLeg1.JW_RL_NKDiscPort = "CAYYZ";
				transportLeg1.JW_ETD = ZDateTime.Today;
				var transportLeg2 = consol.Transports.AddNew();
				transportLeg2.JW_RL_NKLoadPort = "CAYYZ";
				transportLeg2.JW_RL_NKDiscPort = "GBPME";
				transportLeg2.JW_ETA = ZDateTime.Today;
				consol.JK_RL_NKDischargePort = "GBPME";
				consol.JK_MasterBillNum = "234";
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "TTG3").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
				Factory.Save();
				AssertEquals("Use CCN for registry value if goods pass through CA", true, GetCargoControlNumberEntryNum(consol) != null);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestMasterBillChanged_ConfirmClicked_CCNNumberUpdate()
		{
			using (FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.MasterBill))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				CreateNewConsolWithValidData();
				carrier.CustomsCodes.RemoveAndDeleteAll();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CATOR";
				consol.JK_TransportMode = Constants.TransportModes.Air;
				var ccnNumber = consol.Numbers.AddNew();
				ccnNumber.CE_EntryType = "CCN";
				ccnNumber.CE_EntryNum = "2";

				consol.OnShowConfirmMessageOnGUI = (a, b, c, d) => ZDialogResult.Cancel;
				consol.JK_MasterBillNum = "MB123";
				Factory.Save();
				AssertEquals("CCN should not update", "2", ccnNumber.CE_EntryNum);

				consol.OnShowConfirmMessageOnGUI = (a, b, c, d) => ZDialogResult.OK;
				consol.JK_MasterBillNum = "MB456";
				Factory.Save();
				AssertEquals("CCN should update", "MB4-56", ccnNumber.CE_EntryNum);
			}
		}

		public void TestMasterBillIsGoingToChange_ConfirmClicked_CCNNumberUpdate()
		{
			using (FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.MasterBill))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "081";
				mawb.JM_MAWB = "00000011";
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_ServiceLevel = "STD";
				mawb.JM_RL_NKPortOfLoading = "CAWND";
				Factory.Save();

				CreateNewConsolWithValidData();
				carrier.CustomsCodes.RemoveAndDeleteAll();
				consol.JK_UniqueConsignRef = "C0001";
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CAWND";
				consol.JK_RL_NKDischargePort = "CATOR";
				consol.JK_MasterBillNum = "081";
				consol.JK_IsNeutralMaster = true;
				var ccnNumber = consol.Numbers.AddNew();
				ccnNumber.CE_EntryType = "CCN";
				ccnNumber.CE_EntryNum = "2";

				consol.OnShowConfirmMessageOnGUI = (a, b, c, d) => ZDialogResult.Cancel;
				Factory.Save();
				AssertEquals("expected mawb 08100000011 allocated to consol", mawb.PK, consol.MAWBAllocation.AllocatedMawb.PK);
				AssertEquals("CCN should not update", "2", ccnNumber.CE_EntryNum);

				consol.JK_IsNeutralMaster = false;
				Factory.Save();
				consol.Reload();
				consol.JK_IsNeutralMaster = true;
				consol.OnShowConfirmMessageOnGUI = (a, b, c, d) => ZDialogResult.OK;
				Factory.Save();
				AssertEquals("CCN should update", "081-00000011", ccnNumber.CE_EntryNum);
			}
		}

		public void TestCanadaPreviousCargoControlNumber()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.Country.RN_Code;
			try
			{
				FreightDataRegistry.Instance.CanadaConsolPreviousCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
				CreateNewConsolWithValidData();
				carrier.CustomsCodes.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Empty number for new consol, if no carrier code", "", GetPreviousCargoControlNumber(consol));

				CreateNewConsolWithValidData();
				carrier.CustomsCodes.RemoveAndDeleteAll();
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				Factory.Save();
				AssertEquals("Empty number for new consol, if no carrier code for Canada", "", GetPreviousCargoControlNumber(consol));

				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "XYZA").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
				consol.JK_RL_NKDischargePort = "CABLO";
				Factory.Save();
				AssertEquals("Valid number for new consol, with carrier code for Canada", "XYZA", GetPreviousCargoControlNumber(consol));

				consol.Numbers[0].CE_EntryNum = "ZUBIN";
				consol.JK_RL_NKDischargePort = "CATOR";
				Factory.Save();
				AssertEquals("Number not changed if it already exists", "ZUBIN", GetPreviousCargoControlNumber(consol));

				FreightDataRegistry.Instance.CanadaConsolPreviousCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.Non);
				consol.Numbers.RemoveAndDeleteAll();
				consol.JK_RL_NKDischargePort = "CAYYZ";
				Factory.Save();
				AssertEquals("Empty number for registry value NON", "", GetPreviousCargoControlNumber(consol));

				FreightDataRegistry.Instance.CanadaConsolPreviousCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.CCN);
				var number = consol.Numbers.AddNew();
				number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				number.CE_EntryNum = "CCN1";
				consol.JK_RL_NKDischargePort = "CATOR";
				Factory.Save();
				AssertEquals("Use CCN for registry value CCN", "CCN1", GetPreviousCargoControlNumber(consol));

				consol.JK_RL_NKLoadPort = "CATOR";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Not generated for export", "", GetPreviousCargoControlNumber(consol));

				FreightDataRegistry.Instance.CanadaConsolPreviousCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode);
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Afghanistan);
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CATOR";
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Will have PCN when destination starts with CA", "XYZA", GetPreviousCargoControlNumber(consol));

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
				CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
				entryTypeList.Add("AAA", (NoResString)"AAA Test Entry Type").IsUnique = true;
				FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);
				FreightDataRegistry.Instance.CanadaConsolPreviousCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode);

				CreateNewConsolWithValidData();
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "TTG1").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
				consol.JK_RL_NKDischargePort = "CATOR";
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Cargo control number not set when no PCN set in registry", ZString.Empty, GetPreviousCargoControlNumber(consol));

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Jamaica);
				entryTypeList = new CustomsReferenceNumberTypeCollection();
				entryTypeList.Add("PCN", (NoResString)"TEST PCN").IsUnique = true;
				FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);
				FreightDataRegistry.Instance.CanadaConsolPreviousCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode);

				CreateNewConsolWithValidData();
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "TTG2").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
				consol.JK_RL_NKDischargePort = "CABLO";
				consol.Numbers.RemoveAndDeleteAll();
				Factory.Save();
				AssertEquals("Will have PCN when destination starts with CA and PCN set in registry", "TTG2", GetPreviousCargoControlNumber(consol));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestCanadaPreviousCargoControlNumberWhenCountryChanges()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.Country.RN_Code;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
				CreateNewConsolWithValidData();
				consol.JK_RL_NKLoadPort = "CATOR";
				consol.JK_RL_NKDischargePort = "AUSYD";
				carrier.CustomsCodes.RemoveAndDeleteAll();
				carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "XYZA").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
				consol.HasChanges = true;
				AssertEquals(0, consol.Numbers.Count);

				GlbCompany.CurrentCompany.SetCountry(oldCountry);
				Factory.Save();
				ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, new ZString[] { CanadaAdditionalReferenceNumberTypes.Codes.CCN, CanadaAdditionalReferenceNumberTypes.Codes.PCN });
				query.AddToFilter(CusEntryNumSchema.CE_ParentID, consol.PK);
				CusEntryNumber[] results = Factory.Load<CusEntryNumber>(query);
				AssertEquals("After country switching number was not created", 0, results.Length);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestConsolOnSaving_AvoidTrigger_UpdateCanadaCargoControlNumber_ConfirmDialog_NoException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			using (FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ConsolidationCCNCustomizationTypes.Code.MasterBill))
			{
				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "001";
				mawb.JM_MAWB = "10000011";
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_ServiceLevel = "STD";
				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = "AGT";
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "CAWND";
				consol.JK_RL_NKDischargePort = "CATOR";
				consol.Transports[0].JW_ETDForBinding = ZDateTime.Now.AddDays(-2);
				consol.Transports[0].JW_ETAForBinding = ZDateTime.Now.AddDays(-1);
				consol.JK_MasterBillNum = "001";
				consol.JK_IsNeutralMaster = true;

				var ccnNumber = consol.Numbers.AddNew();
				ccnNumber.CE_EntryType = "CCN";
				ccnNumber.CE_EntryNum = "2";

				consol.OnShowConfirmMessageOnGUI = (a, b, c, d) => ZDialogResult.OK;
				Factory.Save();
				AssertEquals("CCN should update", "001-10000011", ccnNumber.CE_EntryNum);
			}
		}

		#region Implementation

		ZString GetCargoControlNumber(ForwardingConsol consol)
		{
			CusEntryNumber result = GetCargoControlNumberEntryNum(consol);
			return result == null ? ZString.Empty : result.CE_EntryNum;
		}

		CusEntryNumber GetCargoControlNumberEntryNum(ForwardingConsol consol)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
			CusEntryNumber[] results = (CusEntryNumber[])consol.Numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		ZString GetPreviousCargoControlNumber(ForwardingConsol consol)
		{
			CusEntryNumber result = GetPreviousCargoControlNumberEntryNum(consol);
			return result == null ? ZString.Empty : result.CE_EntryNum;
		}

		CusEntryNumber GetPreviousCargoControlNumberEntryNum(ForwardingConsol consol)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.PCN);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
			CusEntryNumber[] results = (CusEntryNumber[])consol.Numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		void CreateNewConsolWithValidData()
		{
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "CATOR";
			consol.JK_RL_NKLoadPort = "AUSYD";

			carrier = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
		}

		OrgHeader carrier;
		ForwardingConsol consol;

		#endregion
	}
}
