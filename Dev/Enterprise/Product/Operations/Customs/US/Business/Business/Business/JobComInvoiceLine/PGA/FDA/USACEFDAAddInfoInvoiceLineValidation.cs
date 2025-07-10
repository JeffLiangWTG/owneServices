using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.US.Business.ACEFDAJobDocAddressForInvoiceLineValidation;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.US.Business
{
	class USACEFDAAddInfoInvoiceLineValidation : USACEFDAAddInfoValidation
	{
		public USACEFDAAddInfoInvoiceLineValidation(AutoUSACEFDAAddInfo parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			FDA.ClearRowNotifications();
			base.ValidateAll();

			EnsureThatAtLeastOneContainerSelectedForPN();
		}

		bool IsACECargoReleaseOrStandalonePNValidationMode
		{
			get
			{
				var fda = FDA;
				return fda != null && fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice;
			}
		}

		bool IsACECargoReleaseValidationMode
		{
			get
			{
				var fda = FDA;
				return fda != null && fda.IsACECargoReleaseValidationMode;
			}
		}

		void EnsureThatAtLeastOneContainerSelectedForPN()
		{
			var fda = FDA;
			if (fda != null && fda.IsPriorNotice && fda.US_ProcessingCode != FDAProcessingCodeList.Codes.FOO_CCW && !fda.HasPNCorPND &&
				fda.InvoiceLine != null &&
				fda.InvoiceLine.Declaration.IsContainerised && fda.InvoiceLine.ContainersPivot.Count == 0)
			{
				fda.AddRowMessageError(AtLeastOneContainerRequiredForPN);
			}
		}
		internal const string AtLeastOneContainerRequiredForPN = "At least one container is mandatory for Prior Notice reporting.";

		protected override void CheckUS_ProgramCode()
		{
			base.CheckUS_ProgramCode();

			var fda = FDA;
			if (IsACECargoReleaseOrStandalonePNValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProgramCodeInfo, "Program Code");

				if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.FOO)
				{
					ValidateAffirmationOfComplianceCodesForFood(Parent.US_ProgramCodeInfo);
				}
				else if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.RAD)
				{
					if (fda != null && fda.AffirmationCodes.Count == 0)
					{
						Parent.US_ProgramCodeInfo.AddWarning(AOCRequiredForRAD);
					}
				}

				if (Parent.US_ProgramCode != FDAProgramCodeList.Codes.FOO && (FDA?.InvoiceLine?.HasFDAAdmissibilityReviewRequirement ?? false))
				{
					Parent.US_ProgramCodeInfo.AddMessageError(FD4MustUseFOOD);
				}
			}

			ValidateDocAddressRelatedToProgramCode();
			ValidateUS_FSVPImporterAddress();
		}
		internal const string AOCRequiredForRAD = "Affirmation of Compliance Codes are required if FDA Form 2877 is required.";
		internal const string FD4MustUseFOOD = "Tariffs flagged FD4 must use program code \"FOOD\".";

		void ValidateDocAddressRelatedToProgramCode()
		{
			if (EnableDocAddressForFDA && FDA is ACEFDA fda)
			{
				ValidateDocAddressRelatedToProgramCodeCore(fda.LaboratoryDocAddress, DocAddressTypes.Codes.Laboratory);
				ValidateDocAddressRelatedToProgramCodeCore(fda.DeliverToPartyDocAddress, DocAddressTypes.Codes.GoodsDeliveredTo);
				ValidateDocAddressRelatedToProgramCodeCore(fda.FSVPImporterDocAddress, DocAddressTypes.Codes.FSVPImporter);
				ValidateDocAddressRelatedToProgramCodeCore(fda.ShipperDocAddress, DocAddressTypes.Codes.Shipper);
				ValidateDocAddressRelatedToProgramCodeCore(fda.FDAImporterDocAddress, DocAddressTypes.Codes.ImporterDocumentaryAddress);
				ValidateDocAddressRelatedToProgramCodeCore(fda.GoodsOwnerDocAddress, DocAddressTypes.Codes.GoodsOwner);
				ValidateDocAddressRelatedToProgramCodeCore(fda.InitialImporterDocAddress, DocAddressTypes.Codes.InitialImporter);
				ValidateDocAddressRelatedToProgramCodeCore(fda.ManufacturerDocAddress, DocAddressTypes.Codes.Manufacturer);

				void ValidateDocAddressRelatedToProgramCodeCore(JobDocAddress docAddress, string docAddressType)
				{
					CheckDocAddressWhetherRequired(fda, docAddress, docAddressType, Parent.US_ProgramCodeInfo);
					if (docAddress != null)
					{
						docAddress.Validation.ValidateE2_OA_Address();
						docAddress.Validation.ValidateE2_AddressType();
					}
				}
			}
		}

		internal const string ManufacturerRequired = "Manufacturer is required.";
		internal const string ManufacturerOrAltAddressRequired = "One of Consolidator/Grower/Manufacturer is required.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ValidateAffirmationOfComplianceCodesForFood(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			if (fda != null && fda.IsPriorNotice && fda.US_ProcessingCode != FDAProcessingCodeList.Codes.FOO_CCW && fda.InvoiceLine != null)
			{
				var transportMode = fda.InvoiceLine.Declaration.JE_TransportMode;

				if (transportMode == TransportTypeList.Codes.Rail && !fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.RNO))
				{
					propertyInfo.AddMessageError(RNORequired);
				}
			}

			if (fda != null)
			{
				if (fda.IsLACF || fda.IsAcidified)
				{
					var isStandalonePN = fda.InvoiceLine != null && fda.InvoiceLine.Declaration != null && fda.InvoiceLine.Declaration.IsACEStandalonePNWithoutENSAndCRL;
					if (!isStandalonePN && !fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.FCE))
					{
						propertyInfo.AddMessageError(ZString.Format(FCEOrSIDRequited, ACE_AffirmationOfComplianceList.Codes.FCE));
					}

					if (!isStandalonePN && !fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.SID))
					{
						propertyInfo.AddMessageError(ZString.Format(FCEOrSIDRequited, ACE_AffirmationOfComplianceList.Codes.SID));
					}

					if (!fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.VOL)
						&& fda.US_CanDim1.IsEmpty && fda.US_CanDim2.IsEmpty && fda.US_CanDim3.IsEmpty && fda.US_PackageTrackNumber.IsEmpty)
					{
						propertyInfo.AddMessageError(ContainerMeasurementsORVOLIsRequited);
					}
				}
			}
		}
		internal const string RNORequired = "Affirmation Code 'RNO - Rail Car Number' is required for Prior Notice reporting when Transport Mode is Rail.";
		internal const string FCEOrSIDRequited = "Affirmation Code '{0}' is required if product is Low-Acid Food (LACF) or Acidified Food (AF).";
		internal const string ContainerMeasurementsORVOLIsRequited = "Either Container Measurements information or Affirmation Code 'VOL' should be entered if product is Low-Acid Food (LACF) or Acidified Food (AF).";

		protected override void CheckUS_ProcessingCode()
		{
			base.CheckUS_ProcessingCode();

			var isCodeMandatory = Parent.US_ProgramCode != FDAProgramCodeList.Codes.COS && IsACECargoReleaseOrStandalonePNValidationMode;

			if (Parent.US_ProcessingCode.IsEmpty && isCodeMandatory)
			{
				Parent.US_ProcessingCodeInfo.AddMessageError(GovernmentAgencyProcessingCodeRequired);
			}

			if (!Parent.US_ProcessingCode.IsEmpty && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				var fda = FDA;
				new ACEAffirmationCodesCombinationValidation().ValidateAffirmationOfCodesForProcessingCodeAndIntendedUseCode(fda, Parent.US_ProcessingCodeInfo);

				if (fda != null)
				{
					if (fda.IsProductConstituentElementRequired && fda.ProductConstituentElements.Count == 0)
					{
						Parent.US_ProcessingCodeInfo.AddMessageError(AtLeastOneProductConstituentElementRequired);
					}
				}
			}

			ValidateUS_FSVPImporterAddress();
		}
		internal const string GovernmentAgencyProcessingCodeRequired = "Government Agency Processing Code is mandatory.";
		internal const string AtLeastOneProductConstituentElementRequired = "At least one Active Ingredients is required.";

		protected override void CheckUS_IntendedUseCode()
		{
			base.CheckUS_IntendedUseCode();

			var fda = FDA;

			var intendedUseCodeRequired = IsACECargoReleaseOrStandalonePNValidationMode &&
							Parent.US_ProgramCode != FDAProgramCodeList.Codes.COS &&
							Parent.US_ProcessingCode != FDAProcessingCodeList.Codes.DRU_PHN &&
							Parent.US_ProgramCode != FDAProgramCodeList.Codes.FOO &&
							(Parent.US_ProgramCode != FDAProgramCodeList.Codes.VME || Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.VME_ADR);

			if (intendedUseCodeRequired && Parent.US_IntendedUseCode.IsEmpty)
			{
				Parent.US_IntendedUseCodeInfo.AddMessageError(IntendedUseCodeRequired);
			}

			if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.DEV)
			{
				ValidateAffirmationCodesMandatoryIntendedUseCodeDEV(Parent.US_IntendedUseCodeInfo);
				ValidateAffirmationCodesLimitIntendedUseCodeDEV(Parent.US_IntendedUseCodeInfo);
			}
			else if (fda.IsBiologics)
			{
				if (Parent.US_IntendedUseCode != FDAIntendedUseCodesHelper.Codes._082000 && Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_HCT && (Parent.US_ProductCode.StartsWith("57K", StringComparison.CurrentCulture) || Parent.US_ProductCode.StartsWith("57M", StringComparison.CurrentCulture)))
				{
					Parent.US_IntendedUseCodeInfo.AddWarning(IntendedUseCodeBIO_HCTWarning);
				}

				ValidateAffirmationCodesLimitIntendedUseCodeBIO(Parent.US_IntendedUseCodeInfo);
			}
			else if (fda.US_ProgramCode == FDAProgramCodeList.Codes.DRU)
			{
				ValidateAffirmationCodesLimitIntendedUseCodeDRU(Parent.US_IntendedUseCodeInfo);
			}
			else if (fda.US_ProgramCode == FDAProgramCodeList.Codes.VME)
			{
				ValidateAffirmationCodesLimitIntendedUseCodeVME(Parent.US_IntendedUseCodeInfo);
			}
			else if (fda.US_ProgramCode == FDAProgramCodeList.Codes.COS)
			{
				ValidateAffirmationCodesLimitIntendedUseCodeCOS(Parent.US_IntendedUseCodeInfo);
			}

			if (Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes.UNK)
			{
				Parent.US_IntendedUseCodeInfo.AddWarning(IntendedUseCodeUNKWarning);
			}

			ValidateUS_ProductCode();
			ValidateUS_ProcessingCode();
		}
		internal const string IntendedUseCodeRequired = "Intended Use Code is mandatory.";
		internal const string IntendedUseCodeUNKWarning = "Submitting 'UNK' as the Intended Use Code will subject the entry to manual review, which will cause delays.";
		internal const string IntendedUseCodeBIO_HCTWarning = "For entries of human hemotopoietic stem cell and reproductive tissue (BIO/HCT/ Product Code 57K and 57M), use Intended Use Code 082.000. 082.000 Intended Use Code is for the immediate use by authorized medical officials in the medical treatment of humans.  However, several other Intended Use Codes could also be applicable.";

		#region DEV Validate Affirmation Codes Mandatory Intended Use Code 

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ValidateAffirmationCodesMandatoryIntendedUseCodeDEV(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				switch (Parent.US_IntendedUseCode)
				{
					case FDAIntendedUseCodesHelper.Codes._081001:
					case FDAIntendedUseCodesHelper.Codes._081002:
					case FDAIntendedUseCodesHelper.Codes._081005:
					case FDAIntendedUseCodesHelper.Codes._140000:
					case FDAIntendedUseCodesHelper.Codes.UNK:
						ValidateFor081_140XXX(propertyInfo, fda);
						break;
					case FDAIntendedUseCodesHelper.Codes._081003:
						ValidateFor081003(propertyInfo, fda);
						break;
					case FDAIntendedUseCodesHelper.Codes._081004:
						ValidateFor081004(propertyInfo, fda);
						break;
					case FDAIntendedUseCodesHelper.Codes._180015:
						if (!fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.IDE))
						{
							propertyInfo.AddMessageError(AOCRequiredFor180015);
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._920001:
					case FDAIntendedUseCodesHelper.Codes._950001:
						ValidateFor920001_950001(propertyInfo, fda);
						break;
					case FDAIntendedUseCodesHelper.Codes._920002:
						ValidateFor920002(propertyInfo, fda);
						break;
					case FDAIntendedUseCodesHelper.Codes._970000:
						ValidateFor970000(propertyInfo, fda);
						break;
					case FDAIntendedUseCodesHelper.Codes._970001:
						ValidateFor970001(propertyInfo, fda);
						break;
					case FDAIntendedUseCodesHelper.Codes._081007:
					case FDAIntendedUseCodesHelper.Codes._081008:
						if (!fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.CPT))
						{
							propertyInfo.AddMessageError(AOCRequiredFor081007_081008);
						}
						break;
					default:
						break;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ValidateAffirmationCodesLimitIntendedUseCodeDEV(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				var includeREDCodes = Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DEV_RED;
				HashSet<string> validCodeList = null;
				switch (Parent.US_IntendedUseCode)
				{
					case FDAIntendedUseCodesHelper.Codes._081001:
					case FDAIntendedUseCodesHelper.Codes._081002:
					case FDAIntendedUseCodesHelper.Codes._140000:
					case FDAIntendedUseCodesHelper.Codes.UNK:
						validCodeList = GetCachedDEVAOCCodes(1, includeREDCodes, () => new string[] { "DEV", "DFE", "LST", "IRC", "LWC", "PM#", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._081003:
						validCodeList = GetCachedDEVAOCCodes(2, includeREDCodes, () => new string[] { "DDM", "DFE", "KIT", "LST", "IRC", "LWC", "PM#", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._081004:
						validCodeList = GetCachedDEVAOCCodes(3, includeREDCodes, () => new string[] { "KIT", "DEV", "DFE", "LST", "PM#", "LWC", "IRC", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._081005:
						validCodeList = GetCachedDEVAOCCodes(4, includeREDCodes, () => new string[] { "DEV", "DFE", "LST", "DA", "IND", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._081007:
						validCodeList = GetCachedDEVAOCCodes(5, includeREDCodes, () => new string[] { "CPT", "LST", "PM#", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._081008:
						validCodeList = GetCachedDEVAOCCodes(6, includeREDCodes, () => new string[] { "CPT", "DA", "IND", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._180015:
						validCodeList = GetCachedDEVAOCCodes(7, includeREDCodes, () => new string[] { "IDE", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._170000:
						validCodeList = GetCachedDEVAOCCodes(8, includeREDCodes, () => new string[] { "DFE", "LST", "IRC", "LWC", "PM#", "DDM" });
						if (includeREDCodes)
						{
							validCodeList.Remove("IFE");
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._920001:
					case FDAIntendedUseCodesHelper.Codes._920002:
					case FDAIntendedUseCodesHelper.Codes._950001:
						validCodeList = GetCachedDEVAOCCodes(9, includeREDCodes, () => new string[] { "DDM", "DFE", "IRC", "LST", "LWC", "PM#", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._950002:
						validCodeList = GetCachedDEVAOCCodes(9, includeREDCodes, () => new string[] { "DDM", "DFE", "IRC", "LST", "LWC", "PM#", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._970000:
						validCodeList = GetCachedDEVAOCCodes(10, includeREDCodes, () => new string[] { "DEV", "DFE", "IFE", "LST", "DI", "ERR" });
						break;
					case FDAIntendedUseCodesHelper.Codes._970001:
						validCodeList = GetCachedDEVAOCCodes(11, includeREDCodes, () => new string[] { "IFE", "CPT", "DDM", "LST", "DI", "ERR" });
						break;
				}
				if (validCodeList != null)
				{
					ValidateLimitAOCCodeByIntendedUseCode(propertyInfo, fda, validCodeList);
				}
			}
		}

		HashSet<string> GetCachedDEVAOCCodes(int id, bool includeREDCodes, Func<IEnumerable<string>> getCodes)
		{
			var key = System.FormattableString.Invariant($"{id}{includeREDCodes}");
			var dictionary = Parent.Factory.GetCachedValue("DEVAOCCodes", () => new Dictionary<string, HashSet<string>>());
			if (!dictionary.TryGetValue(key, out HashSet<string> result))
			{
				var codes = getCodes();
				if (includeREDCodes)
				{
					codes = codes.Union(new[] { "RA1", "RA2", "RA3", "RA4", "RA5", "RA6", "RA7", "RB1", "RB2", "RC1", "RC2", "RD1", "RD2", "RD3", "ACC", "ANC", "MDL", "ERR", "IFE", "CCM" });
				}
				result = new HashSet<string>(codes);
				dictionary.Add(key, result);
			}
			return result;
		}

		void ValidateFor081_140XXX(ZPropertyInfo propertyInfo, ACEFDA fda)
		{
			var affirmationCodes = fda.AffirmationCodes.OfType<ACEAffirmationCode>();

			if (!affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DEV)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DFE)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.LST))
			{
				propertyInfo.AddMessageError(AOCRequiredFor081001AndUNK);
			}
		}
		internal const string AOCRequiredFor081001 = "Affirmation Codes DEV, DFE and LST are mandatory if Intended Use Code is one of the following: 081.001, 081.002, 081.005, 140.000.";
		internal const string AOCRequiredFor081001AndUNK = "Affirmation Codes DEV, DFE and LST are mandatory if Intended Use Code is one of the following: 081.001, 081.002, 081.005, 140.000, UNK.";

		void ValidateFor081003(ZPropertyInfo propertyInfo, ACEFDA fda)
		{
			var affirmationCodes = fda.AffirmationCodes.OfType<ACEAffirmationCode>();

			if (!affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DDM)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DFE)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.KIT)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.LST))
			{
				propertyInfo.AddMessageError(AOCRequiredFor081003);
			}
		}
		internal const string AOCRequiredFor081003 = "Affirmation Codes DDM, DFE, KIT and LST are mandatory if Intended Use Code is 081.003.";

		void ValidateFor081004(ZPropertyInfo propertyInfo, ACEFDA fda)
		{
			var affirmationCodes = fda.AffirmationCodes.OfType<ACEAffirmationCode>();
			if (!affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.KIT)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DEV)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DFE)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.LST))
			{
				propertyInfo.AddMessageError(AOCRequiredFor081004);
			}
		}
		internal const string AOCRequiredFor081004 = "Affirmation Codes KIT, DEV, DFE and LST are mandatory if Intended Use Code is 081.004.";

		void ValidateFor920001_950001(ZPropertyInfo propertyInfo, ACEFDA fda)
		{
			var affirmationCodes = fda.AffirmationCodes.OfType<ACEAffirmationCode>();
			if (!affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DDM)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.LST))
			{
				propertyInfo.AddMessageError(AOCRequiredFor920001_950001);
			}
		}
		internal const string AOCRequiredFor180015 = "Affirmation Code IDE is mandatory if Intended Use Code is 180.015.";
		internal const string AOCRequiredFor920001_950001 = "Affirmation Codes DDM and LST are mandatory if Intended Use Code is 920.001 or 950.001.";
		internal const string AOCRequiredFor081007_081008 = "Affirmation Code CPT is mandatory if Intended Use Code is 081.007 or 081.008.";

		void ValidateFor920002(ZPropertyInfo propertyInfo, ACEFDA fda)
		{
			var affirmationCodes = fda.AffirmationCodes.OfType<ACEAffirmationCode>();
			if (!affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DFE)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DDM)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.LST))
			{
				propertyInfo.AddMessageError(AOCRequiredFor920002);
			}
		}
		internal const string AOCRequiredFor920002 = "Affirmation Codes DFE, DDM and LST are mandatory if Intended Use Code is 920.002.";

		void ValidateFor970000(ZPropertyInfo propertyInfo, ACEFDA fda)
		{
			var affirmationCodes = fda.AffirmationCodes.OfType<ACEAffirmationCode>();
			if (!affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DEV)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DFE)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.IFE)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.LST))
			{
				propertyInfo.AddMessageError(AOCRequiredFor970000);
			}
		}
		internal const string AOCRequiredFor970000 = "Affirmation Codes DEV, DFE, IFE and LST are mandatory if Intended Use Code is 970.000.";

		void ValidateFor970001(ZPropertyInfo propertyInfo, ACEFDA fda)
		{
			var affirmationCodes = fda.AffirmationCodes.OfType<ACEAffirmationCode>();
			if (!affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.IFE)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.CPT)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.DDM)
				|| !affirmationCodes.Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.LST))
			{
				propertyInfo.AddMessageError(AOCRequiredFor970001);
			}
		}
		internal const string AOCRequiredFor970001 = "Affirmation Codes IFE, CPT, DDM and LST are mandatory if Intended Use Code is 970.001.";
		#endregion

		#region DRU Validate Affirmation Codes By Intended Use Code

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ValidateAffirmationCodesLimitIntendedUseCodeDRU(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				HashSet<string> validCodeList = null;
				switch (Parent.US_IntendedUseCode)
				{
					case FDAIntendedUseCodesHelper.Codes._080012:
						if (fda.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_804)
						{
							validCodeList = new HashSet<string>(new[] { "REG", "DLS", "DA", "FSR", "PRN" });
						}
						else
						{
							validCodeList = new HashSet<string>(new[] { "REG", "DLS", "DA", "PLR", "FSR", "PRN" });
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._130000:
					case FDAIntendedUseCodesHelper.Codes._150007:
						validCodeList = new HashSet<string>(new[] { "REG", "DLS", "DA" });
						break;
					case FDAIntendedUseCodesHelper.Codes._150013:
						validCodeList = new HashSet<string>(new[] { "REG", "DLS" });
						break;
					case FDAIntendedUseCodesHelper.Codes._150017:
					case FDAIntendedUseCodesHelper.Codes._155009:
						validCodeList = new HashSet<string>(new[] { "REG", "DLS", "DA", "LST", "PM#", "IDE" });
						break;
					case FDAIntendedUseCodesHelper.Codes._180009:
						validCodeList = new HashSet<string>(new[] { "IND" });
						break;
					case FDAIntendedUseCodesHelper.Codes._920000:
						validCodeList = new HashSet<string>(new[] { "REG", "DLS", "DA", "IND" });
						break;
					case FDAIntendedUseCodesHelper.Codes._980000:
						validCodeList = new HashSet<string>(new[] { "REG", "DLS" });
						break;
				}
				if (validCodeList != null)
				{
					ValidateLimitAOCCodeByIntendedUseCode(propertyInfo, fda, validCodeList);
				}
			}
		}

		#endregion

		#region VME Validate Affirmation Codes By Intended Use Code

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ValidateAffirmationCodesLimitIntendedUseCodeVME(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				HashSet<string> validCodeList = null;
				switch (Parent.US_IntendedUseCode)
				{
					case FDAIntendedUseCodesHelper.Codes._085003:
						validCodeList = new HashSet<string>(new[] { "REG", "NDC", "VAN", "VNA", "VFL", "VFD" });
						break;
					case FDAIntendedUseCodesHelper.Codes._150013:
						validCodeList = new HashSet<string>(new[] { "REG", "NDC" });
						break;
					case FDAIntendedUseCodesHelper.Codes._150020:
						validCodeList = new HashSet<string>(new[] { "REG", "NDC", "VAN", "VNA" });
						break;
					case FDAIntendedUseCodesHelper.Codes._180009:
					case FDAIntendedUseCodesHelper.Codes._180018:
						validCodeList = new HashSet<string>(new[] { "VIN" });
						break;
					case FDAIntendedUseCodesHelper.Codes._980000:
						validCodeList = new HashSet<string>(new[] { "REG", "NDC", "VAN", "VNA", "VFL", "VFD" });
						break;
				}
				if (validCodeList != null)
				{
					ValidateLimitAOCCodeByIntendedUseCode(propertyInfo, fda, validCodeList);
				}
			}
		}

		#endregion

		#region DRU Validate Affirmation Codes By Intended Use Code

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ValidateAffirmationCodesLimitIntendedUseCodeBIO(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				HashSet<string> validCodeList = null;
				switch (Parent.US_IntendedUseCode)
				{
					case FDAIntendedUseCodesHelper.Codes._180009:
						if (FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "IND", "REG" });
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._080000:
						if (FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "BLN", "STN", "REG", "DLS" });
						}
						else if (FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_080000(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "DA", "NDA", "AND", "REG", "DLS" });
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._082000:
						if (FDAProcessingCodeList.IsAOCRequiredForBIO_HCT_082000(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "HCT", "HRN" });
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._180016:
						if (FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "BLN", "STN", "REG", "DLS" });
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._155000:
						if (FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "BLN", "STN", "REG", "DLS" });
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._150007:
						if (FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "BLN", "STN", "REG", "DLS", "IND" });
						}
						else if (FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_150007(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "DA", "REG", "DLS", "IND" });
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._140000:
					case FDAIntendedUseCodesHelper.Codes._110000:
						if (FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_140000(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "BLN", "STN", "DA", "IND" });
						}
						break;
					case FDAIntendedUseCodesHelper.Codes._170000:
					case FDAIntendedUseCodesHelper.Codes._940000:
						if (FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_170000(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "BLN", "STN", "DA", "IND", "HCT", "HRN" });
						}
						break;

					case FDAIntendedUseCodesHelper.Codes._970000:
						if (FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(fda.US_ProcessingCode))
						{
							validCodeList = new HashSet<string>(new[] { "IFE" });
						}
						break;
				}
				if (validCodeList != null)
				{
					ValidateLimitAOCCodeByIntendedUseCode(propertyInfo, fda, validCodeList);
				}
			}
		}

		#endregion

		#region COS Validate Affirmation Codes By Intended Use Code

		void ValidateAffirmationCodesLimitIntendedUseCodeCOS(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				ValidateLimitAOCCodeByIntendedUseCode(propertyInfo, fda, new HashSet<string>(new[] { "COS", "ERR", "IFE" }));
			}
		}

		#endregion

		void ValidateLimitAOCCodeByIntendedUseCode(ZPropertyInfo propertyInfo, ACEFDA fda, HashSet<string> validCodeList)
		{
			var invalidCode = fda.AffirmationCodes.OfType<ACEAffirmationCode>().Select(x => x.CY_Code).Where(x => !x.IsEmpty).FirstOrDefault(x => !validCodeList.Contains(x));
			if (!invalidCode.IsEmpty)
			{
				propertyInfo.AddMessageError(ZString.Format(LimitAOCCodeByIntendedUseCodeMessage, invalidCode, fda.US_IntendedUseCode, string.Join(", ", validCodeList)));
			}
		}
		internal const string LimitAOCCodeByIntendedUseCodeMessage = "Affirmation Code '{0}' is not allowed for Intended Use Code '{1}'; allowed codes are {2}.";

		protected override void CheckUS_ProductCode()
		{
			base.CheckUS_ProductCode();

			if (IsACECargoReleaseOrStandalonePNValidationMode)
			{
				if (Parent.US_ProductCode.IsEmpty)
				{
					Parent.US_ProductCodeInfo.AddMessageError(ProductCodeRequired);
				}
				else if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.FOO)
				{
					var fda = FDA;
					if (fda != null && fda.IsPriorNotice && fda.US_ProcessingCode != FDAProcessingCodeList.Codes.FOO_CCW)
					{
						CheckProductCodeFormatForPriorNotice(fda, Parent.US_ProductCodeInfo);
					}
				}
			}
			ValidateUS_ProgramCode();
			ValidateUS_FSVPImporterAddress();
		}
		internal const string ProductCodeRequired = "FDA Product Code is mandatory.";

		void CheckProductCodeFormatForPriorNotice(ACEFDA fda, ZPropertyInfo propertyInfo)
		{
			var industryCode = fda.IndustryCode;
			var classCode = fda.ClassCode;
			var subClassCode = fda.SubClassCode;

			var industryCodeInSpecificRange = new ZString("07,09,69,70,71,72").OccurrencesIgnoringCase(industryCode) == 1;
			var industryCodeInRange2To5 = ZInt.ParseSafe(industryCode, 0).IsInRange(2, 5);
			var industryCodeInRange12To18 = ZInt.ParseSafe(industryCode, 0).IsInRange(12, 18);
			var industryCodeInRange20To42 = ZInt.ParseSafe(industryCode, 0).IsInRange(20, 42);
			var industryCodeInRange45To46 = ZInt.ParseSafe(industryCode, 0).IsInRange(45, 46);
			var product50WithSpecificClassCodes = industryCode == "50" && new ZString("C,D,E,F,G,L").OccurrencesIgnoringCase(classCode) == 1;
			var product52D = industryCode == "52" && classCode.EqualsIgnoringCase("D");

			var product54WithSpecificClassCodes = industryCode == "54" && new ZString("A,B,C,L,M").OccurrencesIgnoringCase(subClassCode) == 1;

			if (!industryCodeInSpecificRange && !industryCodeInRange2To5 && !industryCodeInRange12To18 && !industryCodeInRange20To42 && !industryCodeInRange45To46
				&& !product50WithSpecificClassCodes && !product52D && !product54WithSpecificClassCodes)
			{
				propertyInfo.AddMessageError(ProductCodeFormatForPriorNotice);
			}
		}
		internal const string ProductCodeFormatForPriorNotice = @"Product code should confirm following requirements for Prior Notice reporting:
Industry Code IN ('07','09', '69', '70', '71', '72')
OR Industry Code BETWEEN '02' and '05'
OR Industry Code BETWEEN '12' and '18'
OR Industry Code BETWEEN '20' and '42'
OR Industry Code BETWEEN '45' and '46'
OR Industry Code = '50' and Class Code in ('C', 'D', 'E', 'F', 'G', 'L')
OR Industry Code + Class Code = '52D'
OR Industry Code = '54' and Subclass Code in ('A','B','C','L','M').";

		protected override void CheckUS_ProdCountry()
		{
			base.CheckUS_ProdCountry();

			var fda = FDA;
			if (fda != null)
			{
				if (fda.US_ProdCountry.IsEmpty && IsACECargoReleaseOrStandalonePNValidationMode)
				{
					if (fda.US_ProgramCode == FDAProgramCodeList.Codes.TOB || fda.US_ProgramCode == FDAProgramCodeList.Codes.COS)
					{
						Parent.US_ProdCountryInfo.AddMessageError(ProdCountryRequired);
					}

					if (fda.US_ProgramCode == FDAProgramCodeList.Codes.FOO)
					{
						Parent.US_ProdCountryInfo.AddMessageError(ProdCountryRequiredForFood);
					}
				}

				var manufacturerAddress = fda.EnableDocAddressForFDA ? (IAddressDetails)fda.ManufacturerDocAddress : fda.ManufacturerAddress;
				if (manufacturerAddress != null)
				{
					CheckProdCountryShouldSameCountryOfManufacturer(Parent.US_ProdCountryInfo, fda, manufacturerAddress);
				}

				fda.AddInfoValidation.ValidateUS_ManufacturerAddress();
			}
		}
		internal const string ProdCountryRequired = "Production Country is mandatory.";
		internal const string ProdCountryRequiredForFood = "Production Country/Place of Growth is mandatory for Food reporting.";

		internal static void CheckProdCountryShouldSameCountryOfManufacturer(ZPropertyInfo propertyInfo, ACEFDA fda, IAddressDetails addressDetails)
		{
			if (fda != null && !fda.US_ProdCountry.IsEmpty && fda.US_ProdCountry != Core.Constants.CountryCodes.UnitedStates && addressDetails != null)
			{
				var countryOfManufacturer = addressDetails.Country;
				if (countryOfManufacturer == Core.Constants.CountryCodes.PuertoRico)
				{
					propertyInfo.AddMessageError(ShouldBeUSWhenManufacturerIsPR);
				}
			}
		}

		internal const string ShouldBeUSWhenManufacturerIsPR = "If Manufactuer is in Puerto Rico, Country of Growth/Production must be 'US'";

		protected override void CheckUS_SourceCountry()
		{
			base.CheckUS_SourceCountry();

			if (IsACECargoReleaseOrStandalonePNValidationMode)
			{
				var fda = FDA;
				if (fda != null)
				{
					if ((fda.IsBiologics || fda.US_ProgramCode == FDAProgramCodeList.Codes.DRU ||
						fda.US_ProgramCode == FDAProgramCodeList.Codes.DEV || IsRadiationEmitting || fda.US_ProgramCode == FDAProgramCodeList.Codes.VME) &&
						fda.US_ProdCountry.IsEmpty && Parent.US_SourceCountry.IsEmpty)
					{
						Parent.US_SourceCountryInfo.AddMessageError(SourceCountryRequired);
					}
					else if (!Parent.US_SourceCountry.IsEmpty && (fda.IsFood || fda.US_ProgramCode == FDAProgramCodeList.Codes.COS))
					{
						Parent.US_SourceCountryInfo.AddMessageError(SourceCountryNotAllowed);
					}
				}
			}
		}
		internal const string SourceCountryRequired = "Country of Production or Source Country is required for Biologic, Drugs, Medical Devices, Radiation-Emitting Products or Animal Drugs and Devices reporting.";
		internal const string SourceCountryNotAllowed = "Source Country is Not Allowed for Program Codes FOO or COS.";

		protected override void CheckUS_Qty1()
		{
			base.CheckUS_Qty1();

			if (IsACECargoReleaseOrStandalonePNValidationMode)
			{
				Check_Qty(Parent.US_Qty1Info, Parent.US_UQ1Info);

				var fda = FDA;
				if (Parent.US_Qty1.IsEmpty)
				{
					if (fda.IsPriorNotice && fda.US_ProcessingCode != FDAProcessingCodeList.Codes.FOO_CCW)
					{
						Parent.US_Qty1Info.AddMessageError(BaseQtyRequired);
					}
					else if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_804)
					{
						Parent.US_Qty1Info.AddMessageError(QuantityRequiredForSection804);
					}
					else
					{
						var is2877 = (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DEV_RED || Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.RAD_REP)
							&& fda.AffirmationCodes.Cast<ACEAffirmationCode>().Any(x => x.CY_Code.EqualsIgnoringCase("ACC") || x.CY_Code.EqualsIgnoringCase("ANC"));

						if (is2877)
						{
							Parent.US_Qty1Info.AddMessageError(BaseQtyRequiredFor2877);
						}
						else
						{
							Parent.US_Qty1Info.AddWarning(BaseQtyWarning);
						}
					}
				}
			}
		}
		internal const string BaseQtyRequired = "Base Quantity is mandatory and should be entered.";
		internal const string BaseQtyRequiredFor2877 = "Base Quantity is mandatory if the product requires a 2877.";
		internal const string BaseQtyWarning = "Although quantity is optional, transmitting the quantity accurately will assist in reviewing the product in a timely manner. Failure to transmit the quantity records may result in delays while the missing information is obtained by FDA.";
		internal const string QuantityRequiredForSection804 = "For the Section 804 Importation Program, Quantity is mandatory.";

		protected override void CheckUS_UQ1()
		{
			base.CheckUS_UQ1();

			if (IsACECargoReleaseOrStandalonePNValidationMode)
			{
				Check_UQ(Parent.US_Qty1Info, Parent.US_UQ1Info);

				if (!Parent.US_UQ1.IsEmpty && IsRadiationEmitting && Parent.US_UQ1 != ACE_FDABaseUQList.Codes.PCS)
				{
					Parent.US_UQ1Info.AddMessageError(BaseUQRequiredPCSForRAD);
				}

				if (!Parent.US_UQ1.IsEmpty && Parent.US_ProgramCode == FDAProgramCodeList.Codes.TOB &&
					Parent.US_UQ1 != FDABaseUQList.Codes.PCS &&
					Parent.US_UQ1 != FDABaseUQList.Codes.NO &&
					Parent.US_UQ1 != FDABaseUQList.Codes.DOZ &&
					Parent.US_UQ1 != FDABaseUQList.Codes.DPC &&
					Parent.US_UQ1 != FDABaseUQList.Codes.BBL &&
					Parent.US_UQ1 != FDABaseUQList.Codes.FOZ &&
					Parent.US_UQ1 != FDABaseUQList.Codes.GAL &&
					Parent.US_UQ1 != FDABaseUQList.Codes.L &&
					Parent.US_UQ1 != FDABaseUQList.Codes.ML &&
					Parent.US_UQ1 != FDABaseUQList.Codes.PTL &&
					Parent.US_UQ1 != FDABaseUQList.Codes.QTL &&
					Parent.US_UQ1 != FDABaseUQList.Codes.G &&
					Parent.US_UQ1 != FDABaseUQList.Codes.KG &&
					Parent.US_UQ1 != FDABaseUQList.Codes.LB)
				{
					Parent.US_UQ1Info.AddMessageError(UQRequiredForTOB);
				}
			}
		}

		protected override void CheckUS_Qty2()
		{
			base.CheckUS_Qty2();
			Check_Qty(Parent.US_Qty2Info, Parent.US_UQ2Info);
		}

		protected override void CheckUS_Qty3()
		{
			base.CheckUS_Qty3();
			Check_Qty(Parent.US_Qty3Info, Parent.US_UQ3Info);
		}

		protected override void CheckUS_Qty4()
		{
			base.CheckUS_Qty4();
			Check_Qty(Parent.US_Qty4Info, Parent.US_UQ4Info);
		}

		protected override void CheckUS_Qty5()
		{
			base.CheckUS_Qty5();
			Check_Qty(Parent.US_Qty5Info, Parent.US_UQ5Info);
		}

		protected override void CheckUS_Qty6()
		{
			base.CheckUS_Qty6();
			Check_Qty(Parent.US_Qty6Info, Parent.US_UQ6Info);
		}

		void Check_Qty(ZPropertyInfo qtyInfo, ZPropertyInfo uqInfo)
		{
			if (IsACECargoReleaseOrStandalonePNValidationMode)
			{
				if (!qtyInfo.Value.IsEmpty && (ZDecimal)qtyInfo.Value < ZDecimal.Zero)
				{
					qtyInfo.AddMessageError(QTYShoudGreaterThanZero);
				}
				else if (qtyInfo.Value.IsEmpty && !uqInfo.Value.IsEmpty)
				{
					qtyInfo.AddMessageError(QTYRequired);
				}
			}
		}

		internal const string QTYRequired = "Quantity is required if Unit if Measure is entered.";
		internal const string QTYShoudGreaterThanZero = "Qutantity must be greater than zero.";
		internal const string UQRequired = "Unit Of Measure is required if Quantity is entered.";
		internal const string BaseUQRequiredPCSForRAD = "The Base Unit should be 'PCS' for Radiation Emitting.";
		internal const string UQRequiredForTOB = "Unit Of Measure should be 'PCS','DOZ','DPC','NO','BBL','FOZ','GAL','L','ML','PTL','QTL','G','KG' or 'LB' when reporting Tobacco to FDA.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override void CheckUS_ManufacturerAddress()
		{
			base.CheckUS_ManufacturerAddress();

			if (FDA is ACEFDA fda && !EnableDocAddressForFDA)
			{
				if (IsACECargoReleaseOrStandalonePNValidationMode)
				{
					if (fda.US_ManufacturerAddress.IsEmpty && IsACECargoReleaseOrStandalonePNValidationMode)
					{
						if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.TOB)
						{
							if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.TOB_INV)
							{
								Parent.US_ManufacturerAddressInfo.AddMessageError(LabRequiredForIVN);
							}
							else if (!Parent.US_ProducerType.IsEmpty)
							{
								Parent.US_ManufacturerAddressInfo.AddMessageError(LabRequiredForProducerType);
							}
						}
						else
						{
							var firmType = fda.US_ProducerType == ProducerFirmTypeList.Codes.C ? ProducerFirmTypeList.Descriptions.C :
								(fda.US_ProducerType == ProducerFirmTypeList.Codes.G ? ProducerFirmTypeList.Descriptions.G : "Manufacturer");
							Parent.US_ManufacturerAddressInfo.AddMessageError(string.Format(ManufacturerRequiredPattern, firmType));
						}
					}
				}

				CheckProdCountryShouldSameCountryOfManufacturer(Parent.US_ManufacturerAddressInfo, fda, fda.ManufacturerAddress);
				fda.AddInfoValidation.ValidateUS_ProdCountry();
			}
		}
		internal const string ManufacturerRequiredPattern = "{0} is required for FDA reporting.";
		internal const string LabRequiredForIVN = "Laboratory is mandatory for Tobacco reporting when Processing Code is 'INV - Investigational'.";
		internal const string LabRequiredForProducerType = "Laboratory Organization required when Laboratory type is not blank.";

		protected override void CheckUS_DimUQ()
		{
			base.CheckUS_DimUQ();

			if ((Parent.US_CanDim1 > 0 || Parent.US_CanDim2 > 0 || Parent.US_CanDim3 > 0) && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DimUQInfo, "dimension UQ");
			}
		}

		protected override void CheckUS_DeliverToPartyAddress()
		{
			base.CheckUS_DeliverToPartyAddress();

			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode && !EnableDocAddressForFDA)
			{
				if (fda.US_DeliverToPartyAddress.IsEmpty)
				{
					Parent.US_DeliverToPartyAddressInfo.AddMessageError(DeliverToPartyRequired);
				}
				else
				{
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_DeliverToPartyAddressInfo, fda.DeliverToPartyAddress, ShouldCheckStateForAddress);
				}
			}
		}
		internal const string DeliverToPartyRequired = "Deliver to Party Address is mandatory.";

		protected override void CheckUS_BrandName()
		{
			base.CheckUS_BrandName();

			if (Parent.US_BrandName.IsEmpty && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				var isMandatoryForTobacco = Parent.US_ProgramCode == FDAProgramCodeList.Codes.TOB && Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.TOB_CSU;

				if (FDAProcessingCodeList.IsBrandNameMandatory(Parent.US_ProcessingCode) || isMandatoryForTobacco)
				{
					Parent.US_BrandNameInfo.AddMessageError(BrandNameRequired);
				}
				else if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.DEV)
				{
					Parent.US_BrandNameInfo.AddWarning(BrandNameWarning);
				}
				else if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.DRU || Parent.US_ProgramCode == FDAProgramCodeList.Codes.VME)
				{
					Parent.US_BrandNameInfo.AddWarning(BrandNameWarning2);
				}
			}
		}
		internal const string BrandNameRequired = "Brand Name is mandatory.";
		internal const string BrandNameWarning = "If the brand name is provided on the invoice or otherwise available it should be declared here.";
		internal const string BrandNameWarning2 = "FDA highly encourages the transmission of brand name. Failure to provide brand name could result in delays if FDA requests documents to obtain this information.";

		protected override void CheckUS_Description()
		{
			base.CheckUS_Description();
			if (IsACECargoReleaseOrStandalonePNValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DescriptionInfo, "Extended Description");
			}
		}

		protected override void CheckUS_FDAImporterAddress()
		{
			base.CheckUS_FDAImporterAddress();

			var fda = FDA;
			if (!EnableDocAddressForFDA)
			{
				var isMandatory = IsACECargoReleaseValidationMode && !fda.IsACEStandalonePNWithoutENSAndCRL;
				if (fda.US_FDAImporterAddress.IsEmpty && isMandatory)
				{
					Parent.US_FDAImporterAddressInfo.AddMessageError(FDAImpRequired);
				}
				var importerAddress = fda.FDAImporterAddress;
				if (importerAddress != null)
				{
					var isPGAContactRequired = fda.US_ProgramCode != FDAProgramCodeList.Codes.FOO || fda.US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_CCW;

					if (isPGAContactRequired)
					{
						OrganisationValidation.ValidatePGAContact(fda.US_FDAImporterAddressInfo, OrgHeaderWrapper.New(importerAddress));
					}

					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_FDAImporterAddressInfo, importerAddress, ShouldCheckStateForAddress);
				}
			}
		}
		internal const string FDAImpRequired = "FDA Importer Address is mandatory.";

		protected override void CheckUS_FSVPImporterAddress()
		{
			base.CheckUS_FSVPImporterAddress();

			var fda = FDA;
			if (fda != null && !fda.IsACEStandalonePNWithoutENSAndCRL && !EnableDocAddressForFDA)
			{
				var fsvpImporterAddress = fda.FSVPImporterAddress;
				if (fda.IsFSVPImpRequired)
				{
					if (fsvpImporterAddress == null)
					{
						Parent.US_FSVPImporterAddressInfo.AddMessageError(FSVPImpRequired);
					}
					else
					{
						var pgaContactDetails = OrgHeaderWrapper.New(fsvpImporterAddress, new string[] { ContactAllocationType.USFSV, ContactAllocationType.USPGA });
						OrganisationValidation.ValidatePGAEmail(fda.US_FSVPImporterAddressInfo, pgaContactDetails);
						OrganisationValidation.ValidatePGAAddress(fda.US_FSVPImporterAddressInfo, fsvpImporterAddress);
					}
				}
				else if (fsvpImporterAddress != null)
				{
					Parent.US_FSVPImporterAddressInfo.AddMessageError(FSVPImpShouldNotBeSent);
				}
			}
		}

		internal const string FSVPImpRequired = "FSVP Importer Address is mandatory";
		internal const string FSVPImpShouldNotBeSent = @"You have entered FSVP Importer which is not required for this FDA line. It is required when all of the following conditions are met. 
	1. GOVT Agency progam code is FOO.
	2. Processing code is NSF, PRO, ADD, DSU or FEE.
	3. Industry code is neither 16 nor 32.
	4. AoC FSX or RNE is not declared.";

		protected override void CheckUS_ProducerAddress()
		{
			base.CheckUS_ProducerAddress();

			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode && !EnableDocAddressForFDA)
			{
				if (fda.US_ProducerAddress.IsEmpty && fda.US_ProgramCode == FDAProgramCodeList.Codes.DEV)
				{
					Parent.US_ProducerAddressInfo.AddMessageError(DeviceInitialImporterRequired);
				}
				else if (fda.US_ProducerAddress.IsEmpty && Parent.US_ProgramCode == FDAProgramCodeList.Codes.TOB)
				{
					Parent.US_ProducerAddressInfo.AddMessageError(ManufRequiredForTob);
				}

				OrganisationValidation.ValidateStateForPGAAddress(Parent.US_ProducerAddressInfo, fda.ProducerAddress, ShouldCheckStateForAddress);
			}
		}
		internal const string DeviceInitialImporterRequired = "Device Initial Importer is mandatory for Medical Devices reporting.";
		internal const string ManufRequiredForTob = "Manufacturer is mandatory for Tobacco reporting.";

		protected override void CheckUS_ShipmentCountry()
		{
			base.CheckUS_ShipmentCountry();

			var fda = FDA;
			if (fda != null && fda.US_ShipmentCountry.IsEmpty && fda.IsPriorNotice && fda.US_ProcessingCode != FDAProcessingCodeList.Codes.FOO_CCW)
			{
				Parent.US_ShipmentCountryInfo.AddMessageError(ShipmentCountryRequired);
			}
		}
		internal const string ShipmentCountryRequired = "Country of Shipment is mandatory for Prior Notice reporting.";

		protected override void CheckUS_ProducerType()
		{
			base.CheckUS_ProducerType();

			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				if (fda.US_ProducerType.IsEmpty && fda.US_ProgramCode != FDAProgramCodeList.Codes.TOB) //Tobacco validated separately using his own rules
				{
					Parent.US_ProducerTypeInfo.AddMessageError(FirmTypeRequired);
				}

				if (fda.US_ProgramCode == FDAProgramCodeList.Codes.FOO)
				{
					if (fda.IsPriorNotice && fda.US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_NSF && fda.US_ProducerType != ProducerFirmTypeList.Codes.G && fda.US_ProducerType != ProducerFirmTypeList.Codes.C)
					{
						Parent.US_ProducerTypeInfo.AddMessageError(ProducerTypeRequired);
					}
					else if (!fda.IsPriorNotice && fda.US_ProducerType != ProducerFirmTypeList.Codes.M)
					{
						Parent.US_ProducerTypeInfo.AddMessageError(ProducerTypeForNonPriorNotice);
					}

					if (fda.US_ProcessingCode != FDAProcessingCodeList.Codes.FOO_NSF && fda.US_ProducerType == ProducerFirmTypeList.Codes.C)
					{
						Parent.US_ProducerTypeInfo.AddMessageError(ConsolidatorMustUseNSF);
					}
					else if (fda.US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_NSF && fda.US_ProducerType == ProducerFirmTypeList.Codes.M)
					{
						Parent.US_ProducerTypeInfo.AddMessageError(NSFMismatchManufacturer);
					}
				}

				if (fda.US_ProgramCode == FDAProgramCodeList.Codes.TOB && fda.US_ProcessingCode == FDAProcessingCodeList.Codes.TOB_INV && fda.US_ProducerType.IsEmpty)
				{
					Parent.US_ProducerTypeInfo.AddMessageError(LaboratoryTypeRequired);
				}

				if (fda.US_ProgramCode != FDAProgramCodeList.Codes.FOO && fda.US_ProgramCode != FDAProgramCodeList.Codes.TOB && !fda.US_ProducerType.IsEmpty && fda.US_ProducerType != ProducerFirmTypeList.Codes.M)
				{
					Parent.US_ProducerTypeInfo.AddMessageError(ProducerTypeShouldBeManufacturer);
				}

				ValidateUS_ProdCountry();
				ValidateUS_ManufacturerAddress();
			}
		}
		internal const string ProducerTypeRequired = "Producer Firm Type is required when reporting Food in natural state. Should be 'C - Consolidator' or 'G - Grower'.";
		internal const string FirmTypeRequired = "Firm Type is required.";
		internal const string ProducerTypeForNonPriorNotice = "Producer Firm Type should be 'M - Manufacturer' if Prior Notice has been previously satisfied.";
		internal const string LaboratoryTypeRequired = "Laboratory Type is required when reporting Tobacco with Processing Code 'INV - Investigational'.";
		internal const string ProducerTypeShouldBeManufacturer = "Producer Type should be Manufacturer. Consolidator/Grower producer firm types are allowed only for Food reporting.";
		internal const string ConsolidatorMustUseNSF = "Firm Type of Consolidator is not allowed unless the Processing Code is Natural State Food (NSF).";
		internal const string NSFMismatchManufacturer = "Firm Type of Manufacturer is not allowed for Natural State Food (NSF).";

		protected override void CheckUS_OwnerAddress()
		{
			base.CheckUS_OwnerAddress();

			var fda = FDA;
			var processingCode = fda.US_ProcessingCode;
			if (fda != null)
			{
				if (fda.IsPriorNotice && processingCode != FDAProcessingCodeList.Codes.FOO_CCW)
				{
					if (fda.US_OwnerAddress.IsEmpty)
					{
						Parent.US_OwnerAddressInfo.AddMessageError(OwnerRequired);
					}
					else
					{
						OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OwnerAddressInfo, fda.OwnerAddress, ShouldCheckStateForAddress);
					}
				}
				else if (!fda.US_OwnerAddress.IsEmpty && !fda.IsFood)
				{
					Parent.US_OwnerAddressInfo.AddMessageError(OwnerOnlyRequiredForFood);
				}
			}
		}
		internal const string OwnerRequired = "Owner Address is mandatory for Prior Notice reporting.";
		internal const string OwnerOnlyRequiredForFood = "Owner Code should only be reported for Food items(Prior Notice)";

		protected override void CheckUS_PND()
		{
			base.CheckUS_PND();

			if (Parent.US_PND)
			{
				if (!FDA.FDAPriorNoticeAndAdmissibilityReviewMayBeRequired)
				{
					Parent.US_PNDInfo.AddMessageError(ValidationConstants.PriorNotice.DisclaimedAndNotFD3);
				}
				if (Parent.US_FDAForcePN)
				{
					Parent.US_PNDInfo.AddMessageError(ValidationConstants.PriorNotice.ForcedFDAIsDisclaimed);
				}
			}
		}

		protected override void CheckUS_PNC()
		{
			base.CheckUS_PNC();

			if (!Parent.US_PNC.IsEmpty && Parent.US_PNC.Length != 12)
			{
				Parent.US_PNCInfo.AddMessageError(ValidationConstants.PriorNotice.ConfirmationNumberFormat);
			}
		}

		protected override void CheckUS_PFR()
		{
			base.CheckUS_PFR();

			var regNumber = Parent.US_PFR;
			if (FDA.IsPriorNotice && FDA.US_ProcessingCode != FDAProcessingCodeList.Codes.FOO_CCW && !FDA.HasPNCorPND && Parent.US_ProducerType == ProducerFirmTypeList.Codes.M &&
				regNumber.IsEmpty && Parent.US_FME.IsEmpty && IsACECargoReleaseOrStandalonePNValidationMode)
			{
				Parent.US_PFRInfo.AddMessageError(ValidationConstants.PriorNotice.FoodFacilityRegistrationNumber);
			}

			if (!regNumber.IsEmpty && !Regex.IsMatch(regNumber, @"^[0-9]{11}$"))
			{
				Parent.US_PFRInfo.AddMessageError(FoodRegNoShouldBe11Digits);
			}
		}
		internal const string FoodRegNoShouldBe11Digits = "Food Facility Registration Number should be 11 digits.";

		protected override void CheckUS_OA_ShipperAddress()
		{
			base.CheckUS_OA_ShipperAddress();

			var fda = FDA;
			if (fda != null && IsACECargoReleaseOrStandalonePNValidationMode && !EnableDocAddressForFDA && fda.US_OA_ShipperAddress.IsEmpty)
			{
				Parent.US_OA_ShipperAddressInfo.AddMessageError(ShipperRequired);
			}
		}
		internal const string ShipperRequired = "Shipper Address is mandatory for FDA reporting.";

		protected override void CheckUS_InvCurrValue()
		{
			base.CheckUS_InvCurrValue();

			if (Parent.US_InvCurrValue.IsEmpty)
			{
				if (IsACECargoReleaseValidationMode && Parent.US_ProgramCode != FDAProgramCodeList.Codes.COS)
				{
					Parent.US_InvCurrValueInfo.AddWarning(InvCurrValueRequiredNew);
				}
			}
			else if (Parent.US_InvCurrValue > 0)
			{
				var fda = this.FDA;
				if (fda != null && fda.IsACEStandalonePNWithoutENSAndCRL)
				{
					Parent.US_InvCurrValueInfo.AddMessageError(InvCurrValueNotRequiredForPriorNotice);
				}
			}

			if (Parent.US_InvCurrValue < 0m)
			{
				Parent.US_InvCurrValueInfo.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
			}

			ValidateUS_UnitValue();
		}
		internal const string InvCurrValueRequiredNew = "Although Line Value is optional, transmitting the value will assist in reviewing the product in a timely manner.  Failure to transmit the value may result in delays while the missing information is obtained by FDA.";
		internal const string InvCurrValueRequiredOld = "FDA Line Value is required.";

		internal const string InvCurrValueNotRequiredForPriorNotice = "FDA Line Value is not required for Standalone Prior Notice.";

		protected override void CheckUS_TrackingStatus()
		{
			base.CheckUS_TrackingStatus();
			ValidateUS_TotalValue();
		}

		protected override void CheckUS_TotalValue()
		{
			base.CheckUS_TotalValue();

			var invoiceLine = FDA.InvoiceLine;
			if (invoiceLine != null && invoiceLine.IsOGAValueUpToDate)
			{
				var roundedCV = invoiceLine.GetEnteredValueForOGA();
				if (roundedCV > 0m)
				{
					if (Parent.US_TotalValue > roundedCV)
					{
						Parent.US_TotalValueInfo.AddMessageError(FDAValueShouldBeLessThanCustomsValue + roundedCV);
					}
					else
					{
						var roundedTotalFDAValue = 0m;
						foreach (ACEFDA fdaLine in invoiceLine.ACE_FDALines)
						{
							if (!fdaLine.IsDeletedOrBeingDeleted())
							{
								roundedTotalFDAValue += fdaLine.US_TotalValue;
							}
						}

						if (roundedTotalFDAValue > roundedCV)
						{
							Parent.US_TotalValueInfo.AddMessageError(ZString.Format(FDATotalValue, roundedTotalFDAValue, roundedCV));
						}
					}
				}

				if (Parent.US_TotalValue > 9999999999m)
				{
					Parent.US_TotalValueInfo.AddMessageError(FDAValuesShouldBeLessThanTenTrillion);
				}
				else if (Parent.US_TotalValue == 0m && !FDA.IsACEStandalonePNWithoutENSAndCRL)
				{
					Parent.US_TotalValueInfo.AddWarning(FDAValueCannotBeZero);
				}
			}
		}
		internal const string FDAValueShouldBeLessThanCustomsValue = "USD FDA Value should be less than Customs Value, $";
		internal const string FDATotalValue = "The rounded total of all FDA Values ({0}) should be less than the rounded Customs Value ({1}).";
		internal const string FDAValuesShouldBeLessThanTenTrillion = "The Total USD FDA Value should be less than 9,999,999,999. Please adjust Inv. Curr. FDA Value accordingly";
		internal const string FDAValueCannotBeZero = "FDA Value cannot be 0. If the value rounds down to less than $1, you can manually override Inv Currency Value as needed.";

		protected override void CheckUS_UQ2()
		{
			base.CheckUS_UQ2();
			CheckMatchUQ(Parent.US_Qty2Info, Parent.US_UQ2Info);
		}

		protected override void CheckUS_UQ3()
		{
			base.CheckUS_UQ3();
			CheckMatchUQ(Parent.US_Qty3Info, Parent.US_UQ3Info);
		}

		protected override void CheckUS_UQ4()
		{
			base.CheckUS_UQ4();
			CheckMatchUQ(Parent.US_Qty4Info, Parent.US_UQ4Info);
		}

		protected override void CheckUS_UQ5()
		{
			base.CheckUS_UQ5();
			CheckMatchUQ(Parent.US_Qty5Info, Parent.US_UQ5Info);
		}

		protected override void CheckUS_UQ6()
		{
			base.CheckUS_UQ6();
			CheckMatchUQ(Parent.US_Qty6Info, Parent.US_UQ6Info);
		}

		void CheckMatchUQ(ZPropertyInfo qtyInfo, ZPropertyInfo uqInfo)
		{
			if (FDA.InvoiceLine != null && FDA.InvoiceLine.Part != null)
			{
				var lastFDAQtyHasNotQty = FDA.GetLastFDAQty(false);
				if (lastFDAQtyHasNotQty.LastFDAQtyInfo != null && lastFDAQtyHasNotQty.LastFDAQtyInfo.Name == qtyInfo.Name)
				{
					var dataProvider = new ACEFDA.UnitConverterDataProviderWithoutProduct(FDA.InvoiceLine);
					var unitConverter = new UnitConverter(dataProvider);

					var converterValue = unitConverter.Convert(FDA.InvoiceLine.JI_InvoiceQuantity, FDA.InvoiceLine.JI_InvoiceUQ, uqInfo.Value.ToString());
					var converBaseValue = unitConverter.Convert(FDA.InvoiceLine.JI_InvoiceQuantity, FDA.InvoiceLine.JI_InvoiceUQ, FDA.US_UQ1);
					if (converterValue.IsEmpty && converBaseValue.IsEmpty)
					{
						uqInfo.AddWarning("UQ does not match the invoice UQ.");
					}
				}
			}
			if (IsACECargoReleaseOrStandalonePNValidationMode)
			{
				Check_UQ(qtyInfo, uqInfo);
			}
		}

		protected override void CheckUS_UnitValue()
		{
			base.CheckUS_UnitValue();
			var fda = FDA;

			if (fda.IsACEStandalonePNWithoutENSAndCRL)
			{
				if (fda.US_UnitValue > 0)
				{
					Parent.US_UnitValueInfo.AddMessageError(UnitValueNotRequiredForPriorNotice);
				}
			}
			else
			{
				var defaultUnitValueFromBaseQty = fda.GetDefaultUnitValueFromBaseQty();
				if (defaultUnitValueFromBaseQty.HasValue && !fda.US_UnitValue.IsEmpty && fda.US_UnitValue != defaultUnitValueFromBaseQty.Value)
				{
					Parent.US_UnitValueInfo.AddWarning(ZString.Format(UnitValueNotNotEqualDefaultValue, fda.US_InvCurrValue, decimal.Round(fda.GetCalculatedRunningBaseQty(), 2), fda.US_UQ1, defaultUnitValueFromBaseQty));
				}
			}
		}

		void Check_UQ(ZPropertyInfo qtyInfo, ZPropertyInfo uqInfo)
		{
			if (!qtyInfo.Value.IsEmpty && uqInfo.Value.IsEmpty)
			{
				uqInfo.AddMessageError(UQRequired);
			}
		}

		internal const string UnitValueNotRequiredForPriorNotice = "FDA Unit Value is not required for Standalone Prior Notice.";
		internal const string UnitValueNotNotEqualDefaultValue = "Unit Value should be based on the Base Quantity. ${0} / {1} {2} = {3}.";

		protected override void CheckUS_FDAForcePN()
		{
			base.CheckUS_FDAForcePN();

			var fda = FDA;
			if (fda != null && fda.ShouldForcePriorNotice && !fda.US_FDAForcePN)
			{
				Parent.US_FDAForcePNInfo.AddMessageError(ZString.Format(ForcePriorNoticeShouldBeTicked, fda.US_ProductCode));
			}

			ValidateUS_FSVPImporterAddress();
		}
		internal const string ForcePriorNoticeShouldBeTicked = "You haven't forced sending Prior Notice, however this is required for product code '{0}'.";
	}
}
