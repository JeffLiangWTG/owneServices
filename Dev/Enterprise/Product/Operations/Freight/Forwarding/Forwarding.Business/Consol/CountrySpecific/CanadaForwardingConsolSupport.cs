using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business
{
	class CanadaForwardingConsolSupport : CountrySpecificJobSupport<ForwardingConsol>
	{
		ZString masterBillNumInitialValue;
		bool confimed2UpdateControlNumber4UpcomingChangeOfMAWB;

		protected override ZString CountryCode
		{
			get { return ZString.Empty; } // Please don't remove this line, eManifest will not only work for Canada customs but also for Consoles which destination port is Canada
		}

		protected override void RegisterCore()
		{
			base.RegisterCore();
			masterBillNumInitialValue = SupportedBO.JK_MasterBillNum;
			SupportedBO.FactorySavingBeforeTransactionCore += SupportedBO_FactorySavingBeforeTransactionCore;
			SupportedBO.ConsolSaving += Consol_FactorySaving;
			SupportedBO.ConsolSaveSucceeded += SupportedBO_ConsolSaveSucceeded;
		}

		protected override void UnregisterCore()
		{
			base.UnregisterCore();
			SupportedBO.FactorySavingBeforeTransactionCore -= SupportedBO_FactorySavingBeforeTransactionCore;
			SupportedBO.ConsolSaving -= Consol_FactorySaving;
			SupportedBO.ConsolSaveSucceeded -= SupportedBO_ConsolSaveSucceeded;
		}

		void SupportedBO_FactorySavingBeforeTransactionCore(object sender, EventArgs e)
		{
			var mawbChangeDescription = string.Empty;
			var isMAWBGoing2Change = !SupportedBO.MAWBAllocation.IsPreCheckOfDoingAllocateMAWBNotPass();
			var isMAWBChanged = !SupportedBO.JK_MasterBillNum.IsEmpty && SupportedBO.JK_MasterBillNum != masterBillNumInitialValue;
			if (isMAWBGoing2Change)
			{
				mawbChangeDescription = (NoResString)"will be allocated";
			}
			else if (isMAWBChanged)
			{
				mawbChangeDescription = (NoResString)"has been changed";
			}
			if ((isMAWBChanged || isMAWBGoing2Change) && SupportedBO.IsGoingViaIgnoringDomesticRoute(Constants.CountryCodes.Canada) && AdditionalReferenceNumberTypeList.ContainsCode(CanadaAdditionalReferenceNumberTypes.Codes.CCN))
			{
				var cusEntryNumber = GetCCNEntryNumber();
				if (cusEntryNumber != null)
				{
					var popupMessagePre = (SupportedBO.IsAir ? "MAWB" : (NoResString)"Master Bill");
					UpdateCanadaCargoControlNumber(() => cusEntryNumber, () => ZDialogResult.OK == SupportedBO.OnShowConfirmMessageOnGUI?.Invoke($"{popupMessagePre} number {mawbChangeDescription}. Press OK to update the CCN."), isMAWBGoing2Change);
				}
			}
		}

		void Consol_FactorySaving(object sender, EventArgs e)
		{
			if (!SupportedBO.IsDeleted && SupportedBO.HasChanges)
			{
				SetCanadaCargoControlNumberIfNotExist();
				SetCanadaPreviousCargoControlNumberIfNotExist();
			}
			var masterBillNumber = SupportedBO.JK_MasterBillNum;
			if (confimed2UpdateControlNumber4UpcomingChangeOfMAWB && masterBillNumber.Length > MasterBillAirlinePrefixLength)
			{
				var cusEntryNumber = GetCCNEntryNumber();
				if (cusEntryNumber != null)
				{
					cusEntryNumber.CE_EntryNum = masterBillNumber.InsertSafe(MasterBillAirlinePrefixLength, "-");
				}
			}
		}

		void SupportedBO_ConsolSaveSucceeded(object sender, EventArgs e)
		{
			masterBillNumInitialValue = SupportedBO.JK_MasterBillNum;
			confimed2UpdateControlNumber4UpcomingChangeOfMAWB = false;
		}

		CusEntryNumber GetCCNEntryNumber()
		{
			return SupportedBO.Numbers.Cast<CusEntryNumber>().FirstOrDefault(o => o.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN && o.CE_RN_NKCountryCode == Constants.CountryCodes.Canada);
		}

		public ZString GetCarrierCode()
		{
			return GetCustomsRegNoByType(OrgCusCode.CodeTypes.CarrierCode);
		}

		ZString GetCarrierPrincipalCode()
		{
			return GetCustomsRegNoByType(OrgCusCode.CodeTypes.CarrierPrincipalCode);
		}

		ZString GetCustomsRegNoByType(ZString codeType)
		{
			ZString result = "";
			if (SupportedBO.ShippingLine != null)
			{
				OrgCusCode orgCustomCode = SupportedBO.ShippingLine.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(codeType, Core.Constants.CountryCodes.Canada);
				if (orgCustomCode != null)
				{
					result = orgCustomCode.OK_CustomsRegNo;
				}
			}
			return result;
		}

		void SetCanadaCargoControlNumberIfNotExist()
		{
			if (SupportedBO.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.Canada) && AdditionalReferenceNumberTypeList.ContainsCode(CanadaAdditionalReferenceNumberTypes.Codes.CCN))
			{
				if (!SupportedBO.Numbers.Cast<CusEntryNumber>().Any(o => o.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN && o.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Canada))
				{
					UpdateCanadaCargoControlNumber(() => AddCusEntryNumber(CanadaAdditionalReferenceNumberTypes.Codes.CCN, ZString.Empty), () => true);
				}
			}
		}
		const int MasterBillAirlinePrefixLength = 3;

		ZString GetEditedMasterBillNumber(ZString masterBillNumber, ZString carrierCode, ZString carrierPrincipalCode)
		{
			var result = masterBillNumber;
			if (!carrierPrincipalCode.IsEmpty && masterBillNumber.StartsWith(carrierPrincipalCode, StringComparison.OrdinalIgnoreCase))
			{
				result = result.SubstringSafe(carrierPrincipalCode.Length);
			}

			return carrierCode + result;
		}

		void SetCanadaPreviousCargoControlNumberIfNotExist()
		{
			if (SupportedBO.IsDestinationToCanada() && AdditionalReferenceNumberTypeList.ContainsCode(CanadaAdditionalReferenceNumberTypes.Codes.PCN))
			{
				if (!SupportedBO.Numbers.Cast<CusEntryNumber>().Any(o => o.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.PCN && o.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Canada))
				{
					string consolPreCCNCustomization = FreightDataRegistry.Instance.CanadaConsolPreviousCargoControlNumberCustomization.Value;
					if (consolPreCCNCustomization == Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode)
					{
						var carrierCode = GetCarrierCode();
						if (!carrierCode.IsEmpty)
						{
							AddCusEntryNumber(CanadaAdditionalReferenceNumberTypes.Codes.PCN, carrierCode);
						}
					}
					else if (consolPreCCNCustomization == Constants.ConsolidationCCNCustomizationTypes.Code.CCN)
					{
						var ccn = SupportedBO.Numbers.Cast<CusEntryNumber>().FirstOrDefault(o => o.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN && o.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
						if (ccn != null && !ccn.CE_EntryNum.IsEmpty)
						{
							AddCusEntryNumber(CanadaAdditionalReferenceNumberTypes.Codes.PCN, ccn.CE_EntryNum);
						}
					}
				}
			}
		}

		void UpdateCanadaCargoControlNumber(Func<CusEntryNumber> cusEntryNumberSupplier, Func<bool> confirmUpdate, bool isMAWBGoing2Change = false)
		{
			var consolCCNCustomization = FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.Value;
			if (consolCCNCustomization == Constants.ConsolidationCCNCustomizationTypes.Code.MasterBill)
			{
				var masterBillNumber = SupportedBO.JK_MasterBillNum;
				if (SupportedBO.JK_TransportMode == Constants.TransportModes.Air)
				{
					if (isMAWBGoing2Change)
					{
						confimed2UpdateControlNumber4UpcomingChangeOfMAWB = confirmUpdate();
					}
					else if (masterBillNumber.Length > MasterBillAirlinePrefixLength && confirmUpdate())
					{
						cusEntryNumberSupplier().CE_EntryNum = masterBillNumber.InsertSafe(MasterBillAirlinePrefixLength, "-");
					}
				}
				else
				{
					var carrierCode = GetCarrierCode();
					if (!carrierCode.IsEmpty && !masterBillNumber.IsEmpty && confirmUpdate())
					{
						cusEntryNumberSupplier().CE_EntryNum = GetEditedMasterBillNumber(masterBillNumber, carrierCode, GetCarrierPrincipalCode());
					}
				}
			}
		}

		CusEntryNumber AddCusEntryNumber(ZString type, ZString number)
		{
			var customReferenceNumber = SupportedBO.Numbers.AddNew();
			customReferenceNumber.CE_EntryType = type;
			customReferenceNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			customReferenceNumber.CE_EntryNum = number;
			return customReferenceNumber;
		}

		CodeDescriptionPairList AdditionalReferenceNumberTypeList
		{
			get
			{
				if (additionalReferenceNumberTypeList == null)
				{
					additionalReferenceNumberTypeList = (SupportedBO as IAdditionalReferenceNumberTypeProvider)?.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
					?? new CodeDescriptionPairList();
				}
				return additionalReferenceNumberTypeList;
			}
		}
		CodeDescriptionPairList additionalReferenceNumberTypeList;
	}
}
