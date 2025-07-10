using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class OGARequirementCalculator
	{
		public OGARequirementCalculator(BusinessObjectFactory factory, Func<USCTariff> getImportTariff, Func<USCTariff> getImportSupTariff, Func<ZDateTime> getDutyDate, Func<ZString> getCountryOfOrigin)
		{
			this.factory = factory;
			this.getImportTariff = getImportTariff;
			this.getImportSupTariff = getImportSupTariff;
			this.getDutyDate = getDutyDate;
			this.getCountryOfOrigin = getCountryOfOrigin;
		}

		readonly BusinessObjectFactory factory;
		readonly Func<USCTariff> getImportTariff;
		readonly Func<USCTariff> getImportSupTariff;
		readonly Func<ZDateTime> getDutyDate;
		readonly Func<ZString> getCountryOfOrigin;

		public ZString FDARequirementCode
		{
			get { return FDARequirementCodeAndDesc.Code; }
		}

		public ZString ACEFDARequirementCode
		{
			get { return ACE_FDARequirementCodeAndDesc.Code; }
		}

		public ZString ODSRequirementCode
		{
			get { return ODSRequirementCodeAndDesc.Code; }
		}

		public ZString VNERequirementCode
		{
			get { return VNERequirementCodeAndDesc.Code; }
		}

		public ZString PSTRequirementCode
		{
			get { return PSTRequirementCodeAndDesc.Code; }
		}

		public ZString HFCRequirementCode
		{
			get { return HFCRequirementCodeAndDesc.Code; }
		}

		public ZString TSCARequirementCode
		{
			get { return TSCARequirementCodeAndDesc.Code; }
		}

		public ZString TTBRequirementCode
		{
			get { return TTBRequirementCodeAndDesc.Code; }
		}

		public ZString APHISRequirementCode
		{
			get { return APHISRequirementCodeAndDesc.Code; }
		}

		public ZString FDARequirementDesc
		{
			get { return FDARequirementCodeAndDesc.Description + (string.IsNullOrEmpty(FDARequirementCodeAndDesc.Code) ? "" : " (" + FDARequirementCodeAndDesc.Code + ")"); }
		}

		public ZString ACEFDARequirementDesc
		{
			get { return ACE_FDARequirementCodeAndDesc.Description + (string.IsNullOrEmpty(ACE_FDARequirementCodeAndDesc.Code) ? "" : " (" + ACE_FDARequirementCodeAndDesc.Code + ")"); }
		}

		public ZString DOTRequirementDesc
		{
			get { return DOTRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(DOTRequirementCodeAndDesc.Code) ? "" : " (" + DOTRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString FSISRequirementDesc
		{
			get { return FSISRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(FSISRequirementCodeAndDesc.Code) ? "" : " (" + FSISRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString FSISRequirementCode
		{
			get { return FSISRequirementCodeAndDesc.Code; }
		}

		public ZString ODSRequirementDesc
		{
			get { return ODSRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(ODSRequirementCodeAndDesc.Code) ? "" : " (" + ODSRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString VNERequirementDesc
		{
			get { return VNERequirementCodeAndDesc.Description + (string.IsNullOrEmpty(VNERequirementCodeAndDesc.Code) ? "" : " (" + VNERequirementCodeAndDesc.Code + ")"); }
		}

		public ZString PSTRequirementDesc
		{
			get { return PSTRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(PSTRequirementCodeAndDesc.Code) ? "" : " (" + PSTRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString HFCRequirementDesc
		{
			get { return HFCRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(HFCRequirementCodeAndDesc.Code) ? "" : " (" + HFCRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString TSCARequirementDesc
		{
			get { return TSCARequirementCodeAndDesc.Description + (string.IsNullOrEmpty(TSCARequirementCodeAndDesc.Code) ? "" : " (" + TSCARequirementCodeAndDesc.Code + ")"); }
		}

		public ZString APHISRequirementDesc
		{
			get { return APHISRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(APHISRequirementCodeAndDesc.Code) ? "" : " (" + APHISRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString NMFS370RequirementDesc
		{
			get { return NMFS370RequirementCodeAndDesc.Description + (string.IsNullOrEmpty(NMFS370RequirementCodeAndDesc.Code) ? "" : " (" + NMFS370RequirementCodeAndDesc.Code + ")"); }
		}

		public ZString NMFS370RequirementCode
		{
			get { return NMFS370RequirementCodeAndDesc.Code; }
		}

		public ZString NMFSCOARequirementDesc
		{
			get { return NMFSCOARequirementCodeAndDesc.Description; }
		}

		public ZString NMFSAMRRequirementDesc
		{
			get { return NMFSAMRRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(NMFSAMRRequirementCodeAndDesc.Code) ? "" : " (" + NMFSAMRRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString NMFSAMRRequirementCode
		{
			get { return NMFSAMRRequirementCodeAndDesc.Code; }
		}

		public ZString NMFSHMSRequirementDesc
		{
			get { return NMFSHMSRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(NMFSHMSRequirementCodeAndDesc.Code) ? "" : " (" + NMFSHMSRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString NMFSHMSRequirementCode
		{
			get { return NMFSHMSRequirementCodeAndDesc.Code; }
		}

		public ZString NMFSSIMRequirementDesc
		{
			get { return NMFSSIMRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(NMFSSIMRequirementCodeAndDesc.Code) ? "" : " (" + NMFSSIMRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString NMFSSIMPRequirementCode
		{
			get { return NMFSSIMRequirementCodeAndDesc.Code; }
		}

		public ZString ACELaceyRequirementDesc
		{
			get
			{
				var result = NoRequirement;

				var aceRequirements = ACELaceyActRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(ACELaceyActRequirementCodeAndDesc.Code) ? "" : " (" + ACELaceyActRequirementCodeAndDesc.Code + ")");
				if (!string.IsNullOrEmpty(aceRequirements))
				{
					result = aceRequirements;
				}
				return result;
			}
		}

		public ZString ACELaceyRequirementCode
		{
			get { return ACELaceyActRequirementCodeAndDesc.Code; }
		}

		public ZString FWSRequirementDesc
		{
			get { return FWSRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(FWSRequirementCodeAndDesc.Code) ? "" : " (" + FWSRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString FWSRequirementCode
		{
			get { return FWSRequirementCodeAndDesc.Code; }
		}

		public ZString LaceyRequirementDesc
		{
			get
			{
				var result = NoRequirement;

				var tariffs = new USCTariff[] { getImportTariff(), getImportSupTariff() };
				var dutyDate = getDutyDate();
				foreach (USCTariff tariff in tariffs)
				{
					if (tariff != null && tariff.Applies(TariffRuleList.Codes.LaceyAct, dutyDate))
					{
						result = LaceyMayBeRequired;
						break;
					}
				}
				return result;
			}
		}
		internal const string LaceyMayBeRequired = "Lacey Act Data May Be Required";

		public ZString NHTSARequirementDesc
		{
			get { return NHTSARequirementCodeAndDesc.Description + (string.IsNullOrEmpty(NHTSARequirementCodeAndDesc.Code) ? "" : " (" + NHTSARequirementCodeAndDesc.Code + ")"); }
		}

		public ZString NHTSARequirementCode
		{
			get { return NHTSARequirementCodeAndDesc.Code; }
		}

		public ZString TTBRequirementDesc
		{
			get { return TTBRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(TTBRequirementCodeAndDesc.Code) ? "" : " (" + TTBRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString OMCRequirementDesc
		{
			get { return OMCRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(OMCRequirementCodeAndDesc.Code) ? "" : " (" + OMCRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString OMCRequirementCode
		{
			get { return OMCRequirementCodeAndDesc.Code; }
		}

		public ZString AMSRequirementDesc
		{
			get { return AMSRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(AMSRequirementCodeAndDesc.Code) ? "" : " (" + AMSRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString AMSRequirementCode
		{
			get { return AMSRequirementCodeAndDesc.Code; }
		}

		public ZString NOPRequirementDesc
		{
			get { return NOPRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(NOPRequirementCodeAndDesc.Code) ? "" : " (" + NOPRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString NOPRequirementCode
		{
			get { return NOPRequirementCodeAndDesc.Code; }
		}

		public ZString CPSCRequirementDesc
		{
			get { return CPSCRequirementCodeAndDesc.Description + (string.IsNullOrEmpty(CPSCRequirementCodeAndDesc.Code) ? "" : " (" + CPSCRequirementCodeAndDesc.Code + ")"); }
		}

		public ZString CPSCRequirementCode
		{
			get { return CPSCRequirementCodeAndDesc.Code; }
		}

		public ZString DEARequirementDesc
		{
			get { return DEARequirementCodeAndDesc.Description + (string.IsNullOrEmpty(DEARequirementCodeAndDesc.Code) ? "" : " (" + DEARequirementCodeAndDesc.Code + ")"); }
		}
		public ZString DEARequirementCode
		{
			get { return DEARequirementCodeAndDesc.Code; }
		}

		#region Implementation

		CodeDescriptionPair DOTRequirementCodeAndDesc
		{
			get
			{
				if (fDOTRequirementCodeAndDesc == null)
				{
					fDOTRequirementCodeAndDesc = CalculateOGARequirementCodeAndDescIfNecessary(factory.GetCachedValue<OGARequirementList>(), "DT");
				}
				return fDOTRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair fDOTRequirementCodeAndDesc;

		CodeDescriptionPair FDARequirementCodeAndDesc
		{
			get
			{
				if (fFDARequirementCodeAndDesc == null)
				{
					fFDARequirementCodeAndDesc = CalculateOGARequirementCodeAndDescIfNecessary(factory.GetCachedValue<OGARequirementList>(), "FD");
				}
				return fFDARequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair fFDARequirementCodeAndDesc;

		CodeDescriptionPair FSISRequirementCodeAndDesc
		{
			get
			{
				if (fsisRequirementCodeAndDesc == null)
				{
					fsisRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.FS4, OGARequirementList.Codes.FS3);
				}
				return fsisRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair fsisRequirementCodeAndDesc;

		CodeDescriptionPair ODSRequirementCodeAndDesc
		{
			get
			{
				if (odsRequirementCodeAndDesc == null)
				{
					odsRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.EP2, OGARequirementList.Codes.EP1);
				}
				return odsRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair odsRequirementCodeAndDesc;

		CodeDescriptionPair TSCARequirementCodeAndDesc
		{
			get
			{
				if (tscaRequirementCodeAndDesc == null)
				{
					tscaRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.EP8, OGARequirementList.Codes.EP7);
					if (string.IsNullOrEmpty(tscaRequirementCodeAndDesc.Code))
					{
						tscaRequirementCodeAndDesc = CalculatePGARequirementsFromTariffRule(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.EP8, OGARequirementList.Codes.EP7);
					}
				}
				return tscaRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair tscaRequirementCodeAndDesc;

		CodeDescriptionPair VNERequirementCodeAndDesc
		{
			get
			{
				if (vneRequirementCodeAndDesc == null)
				{
					vneRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.EP4, OGARequirementList.Codes.EP3);
				}
				return vneRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair vneRequirementCodeAndDesc;

		CodeDescriptionPair PSTRequirementCodeAndDesc
		{
			get
			{
				if (pstRequirementCodeAndDesc == null)
				{
					pstRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.EP6, OGARequirementList.Codes.EP5);
				}
				return pstRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair pstRequirementCodeAndDesc;

		CodeDescriptionPair HFCRequirementCodeAndDesc
		{
			get
			{
				if (hfcRequirementCodeAndDesc == null)
				{
					hfcRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.EH2, OGARequirementList.Codes.EH1);
				}
				return hfcRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair hfcRequirementCodeAndDesc;

		CodeDescriptionPair ACELaceyActRequirementCodeAndDesc
		{
			get
			{
				if (aceLaceyActRequirementCodeAndDesc == null)
				{
					aceLaceyActRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.AL2, OGARequirementList.Codes.AL1);
				}
				return aceLaceyActRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair aceLaceyActRequirementCodeAndDesc;

		CodeDescriptionPair APHISRequirementCodeAndDesc
		{
			get
			{
				if (aphisRequirementCodeAndDesc == null)
				{
					aphisRequirementCodeAndDesc = CalculateOGARequirementCodeAndDescIfNecessary(factory.GetCachedValue<OGARequirementList>(), "AQ", USCTariff.Schema.UE_PGACodes);
				}
				return aphisRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair aphisRequirementCodeAndDesc;

		CodeDescriptionPair NMFS370RequirementCodeAndDesc
		{
			get
			{
				if (nmfs370RequirementCodeAndDesc == null)
				{
					nmfs370RequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.NM2, OGARequirementList.Codes.NM1);
				}
				return nmfs370RequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair nmfs370RequirementCodeAndDesc;

		CodeDescriptionPair NMFSAMRRequirementCodeAndDesc
		{
			get
			{
				if (nmfsAMRRequirementCodeAndDesc == null)
				{
					nmfsAMRRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.NM4, OGARequirementList.Codes.NM3);
				}
				return nmfsAMRRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair nmfsAMRRequirementCodeAndDesc;

		CodeDescriptionPair NMFSHMSRequirementCodeAndDesc
		{
			get
			{
				if (nmfsHMSRequirementCodeAndDesc == null)
				{
					nmfsHMSRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.NM6, OGARequirementList.Codes.NM5);
				}
				return nmfsHMSRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair nmfsHMSRequirementCodeAndDesc;

		CodeDescriptionPair NMFSSIMRequirementCodeAndDesc
		{
			get
			{
				if (nmfsSIMRequirementCodeAndDesc == null)
				{
					nmfsSIMRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.NM8, "");
				}
				return nmfsSIMRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair nmfsSIMRequirementCodeAndDesc;

		CodeDescriptionPair ACE_FDARequirementCodeAndDesc
		{
			get
			{
				if (ace_FDARequirementCodeAndDesc == null)
				{
					ace_FDARequirementCodeAndDesc = CalculateOGARequirementCodeAndDescIfNecessary(factory.GetCachedValue<OGARequirementList>(), "FD", USCTariff.Schema.UE_PGACodes);
				}
				return ace_FDARequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair ace_FDARequirementCodeAndDesc;

		CodeDescriptionPair FWSRequirementCodeAndDesc
		{
			get
			{
				if (fwsRequirementCodeAndDesc == null)
				{
					fwsRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.FW2, OGARequirementList.Codes.FW1);
					if (string.IsNullOrEmpty(fwsRequirementCodeAndDesc.Code))
					{
						fwsRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.FW2, OGARequirementList.Codes.FW3);
					}
				}
				return fwsRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair fwsRequirementCodeAndDesc;

		CodeDescriptionPair CPSCRequirementCodeAndDesc
		{
			get
			{
				if (cpscRequirementCodeAndDesc == null)
				{
					cpscRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.CP2, OGARequirementList.Codes.CP1);
				}
				return cpscRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair cpscRequirementCodeAndDesc;

		CodeDescriptionPair OMCRequirementCodeAndDesc
		{
			get
			{
				if (omcRequirementCodeAndDesc == null)
				{
					omcRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.OM2, OGARequirementList.Codes.OM1);
				}
				return omcRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair omcRequirementCodeAndDesc;

		CodeDescriptionPair NHTSARequirementCodeAndDesc
		{
			get
			{
				if (fNHTSARequirementCodeAndDesc == null)
				{
					fNHTSARequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.DT2, OGARequirementList.Codes.DT1);
				}
				return fNHTSARequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair fNHTSARequirementCodeAndDesc;

		CodeDescriptionPair TTBRequirementCodeAndDesc
		{
			get
			{
				if (fTTBRequirementCodeAndDesc == null)
				{
					fTTBRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.TB2, OGARequirementList.Codes.TB1);
					if (string.IsNullOrEmpty(fTTBRequirementCodeAndDesc.Code))
					{
						fTTBRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.TB2, OGARequirementList.Codes.TB3);
					}
				}
				return fTTBRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair fTTBRequirementCodeAndDesc;

		CodeDescriptionPair CalculatePGARequirementsFromTariffRule(CodeDescriptionPairList list, ZString highSeverityCode, ZString lowSeverityCode)
		{
			return CalculatePGARequirements(list, highSeverityCode, lowSeverityCode, (x, y) => x.Applies(GetTariffRuleCodeFromRequirementCode(y), getDutyDate()));
		}

		CodeDescriptionPair AMSRequirementCodeAndDesc
		{
			get
			{
				if (aMSRequirementCodeAndDesc == null)
				{
					aMSRequirementCodeAndDesc = CalculateOGARequirementCodeAndDescIfNecessary(factory.GetCachedValue<OGARequirementList>(), "AM", USCTariff.Schema.UE_PGACodes, OGARequirementList.Codes.AM7, OGARequirementList.Codes.AM8);
				}

				return aMSRequirementCodeAndDesc;
			}
		}

		CodeDescriptionPair aMSRequirementCodeAndDesc;

		CodeDescriptionPair NOPRequirementCodeAndDesc
		{
			get
			{
				if (nOPRequirementCodeAndDesc == null)
				{
					nOPRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.AM8, OGARequirementList.Codes.AM7);
				}

				return nOPRequirementCodeAndDesc;
			}
		}

		CodeDescriptionPair nOPRequirementCodeAndDesc;

		CodeDescriptionPair DEARequirementCodeAndDesc
		{
			get
			{
				if (deaRequirementCodeAndDesc == null)
				{
					deaRequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), "", OGARequirementList.Codes.DE1);
				}
				return deaRequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair deaRequirementCodeAndDesc;

		CodeDescriptionPair NMFSCOARequirementCodeAndDesc
		{
			get
			{
				if (nmfsCOARequirementCodeAndDesc == null)
				{
					nmfsCOARequirementCodeAndDesc = CalculatePGARequirements(factory.GetCachedValue<OGARequirementList>(), OGARequirementList.Codes.COA);
				}
				return nmfsCOARequirementCodeAndDesc;
			}
		}
		CodeDescriptionPair nmfsCOARequirementCodeAndDesc;

		ZString GetTariffRuleCodeFromRequirementCode(ZString requirement)
		{
			switch (requirement)
			{
				case OGARequirementList.Codes.TB2:
				case OGARequirementList.Codes.TB1:
					return requirement;

				default:
					return "";
			}
		}

		CodeDescriptionPair CalculatePGARequirements(CodeDescriptionPairList list, ZString highSeverityCode, ZString lowSeverityCode)
		{
			return CalculatePGARequirements(list, highSeverityCode, lowSeverityCode, (x, y) => x.UE_PGACodes.Split(3).Contains(y));
		}

		CodeDescriptionPair CalculatePGARequirements(CodeDescriptionPairList list, ZString agencyCode)
		{
			var countryOfOrigin = getCountryOfOrigin();
			var dutyDate = getDutyDate();
			return CalculatePGARequirements(list, agencyCode, ZString.Empty, (tariff, pgaCode) =>
			{
				var isApplicable = false;
				var tariffNumber = tariff.UE_Tariff;
				if (!tariffNumber.IsEmpty && dutyDate.IsValid && !countryOfOrigin.IsEmpty && !pgaCode.IsEmpty)
				{
					isApplicable = USRefTariffDataLoader.IsTariffMatchCondition(factory, tariffNumber, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA, TariffConditionValueTypes.Codes.PGA, pgaCode, countryOfOrigin, dutyDate);
				}

				return isApplicable;
			});
		}

		CodeDescriptionPair CalculatePGARequirements(CodeDescriptionPairList list, ZString highSeverityCode, ZString lowSeverityCode, Func<USCTariff, ZString, bool> isApplicable)
		{
			USCTariff tariffWithHighestReq = null;
			var requirementLevelCode = ZString.Empty;

			var tariffs = new USCTariff[] { getImportTariff(), getImportSupTariff() };
			foreach (USCTariff tariff in tariffs)
			{
				if (tariff != null)
				{
					if (isApplicable(tariff, lowSeverityCode) && tariffWithHighestReq == null)
					{
						tariffWithHighestReq = tariff;
						requirementLevelCode = lowSeverityCode;
					}
					if (isApplicable(tariff, highSeverityCode))
					{
						tariffWithHighestReq = tariff;
						requirementLevelCode = highSeverityCode;
						break;
					}
				}
			}
			return tariffWithHighestReq != null ? (CodeDescriptionPair)list[requirementLevelCode, StringComparison.Ordinal] : new CodeDescriptionPair("", NoRequirement);
		}

		CodeDescriptionPair CalculateOGARequirementCodeAndDescIfNecessary(CodeDescriptionPairList list, ZString firstTwoOGACode, string columnName = USCTariff.Schema.UE_OGACodes, params string[] pgaCodesToExclude)
		{
			USCTariff tariffWithHighestReq = null;

			var tariffs = new USCTariff[] { getImportTariff(), getImportSupTariff() };
			foreach (USCTariff tariff in tariffs)
			{
				if (tariff != null)
				{
					if (tariffWithHighestReq == null || GetTariffWithHigerRequirement(tariffWithHighestReq, tariff, firstTwoOGACode, columnName) == tariff)
					{
						tariffWithHighestReq = tariff;
					}
				}
			}

			CodeDescriptionPair result = null;

			if (tariffWithHighestReq != null)
			{
				ZString code = GetOGARequirementLevel(tariffWithHighestReq, firstTwoOGACode, columnName, pgaCodesToExclude);

				result = (CodeDescriptionPair)list[code, StringComparison.OrdinalIgnoreCase];
			}

			if (result == null)
			{
				result = new CodeDescriptionPair("", NoRequirement);
			}

			return result;
		}

		USCTariff GetTariffWithHigerRequirement(USCTariff tariff1, USCTariff tariff2, ZString firstTwoOGACode, string columnName, params string[] pgaCodesToExclude)
		{
			ZString requirementLevel1 = GetOGARequirementLevel(tariff1, firstTwoOGACode, columnName, pgaCodesToExclude);

			ZString requirementLevel2 = GetOGARequirementLevel(tariff2, firstTwoOGACode, columnName, pgaCodesToExclude);

			return requirementLevel1 > requirementLevel2 ? tariff1 : tariff2;
		}

		ZString GetOGARequirementLevel(USCTariff tariff, ZString firstTwoOGACode, string columnName, params string[] pgaCodesToExclude)
		{
			ZString result = ZString.Empty;

			if (tariff != null)
			{
				ZString codes = tariff[columnName].ToString();
				foreach (string ogaRequirement in codes.Split(3))
				{
					if (pgaCodesToExclude == null || !pgaCodesToExclude.Contains(ogaRequirement))
					{
						if (ogaRequirement.StartsWith(firstTwoOGACode, StringComparison.OrdinalIgnoreCase))
						{
							result = ogaRequirement;
							break;
						}
					}
				}
			}

			return result;
		}

		public const string NoRequirement = "None";

		public void Initialise()
		{
			fFDARequirementCodeAndDesc = null;
			fDOTRequirementCodeAndDesc = null;
			odsRequirementCodeAndDesc = null;
			vneRequirementCodeAndDesc = null;
			fsisRequirementCodeAndDesc = null;
			nmfs370RequirementCodeAndDesc = null;
			nmfsAMRRequirementCodeAndDesc = null;
			nmfsHMSRequirementCodeAndDesc = null;
			nmfsSIMRequirementCodeAndDesc = null;
			ace_FDARequirementCodeAndDesc = null;
			fNHTSARequirementCodeAndDesc = null;
			aceLaceyActRequirementCodeAndDesc = null;
			aphisRequirementCodeAndDesc = null;
			omcRequirementCodeAndDesc = null;
			fTTBRequirementCodeAndDesc = null;
			fwsRequirementCodeAndDesc = null;
			pstRequirementCodeAndDesc = null;
			hfcRequirementCodeAndDesc = null;
			tscaRequirementCodeAndDesc = null;
			cpscRequirementCodeAndDesc = null;
			deaRequirementCodeAndDesc = null;
			aMSRequirementCodeAndDesc = null;
			nOPRequirementCodeAndDesc = null;
			nmfsCOARequirementCodeAndDesc = null;
		}

		#endregion
	}
}
