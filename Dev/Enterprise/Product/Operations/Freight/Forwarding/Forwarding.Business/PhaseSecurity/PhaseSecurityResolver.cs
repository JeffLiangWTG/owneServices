using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business
{
	public class PhaseSecurityResolver : IPhaseSecurityResolver
	{
		public PhaseSecurityResolver(IPhaseSecuritySupportable master)
		{
			Argument.NotNull(master, "master");
			Master = master;
		}

		readonly IPhaseSecuritySupportable Master;

		#region Phase Security

		protected virtual IPhaseSecurity GetPhaseSecurity()
		{
			return null;
		}

		bool IsPhaseSecurityApplicable
		{
			get
			{
				return Globals.IsUserInteractive
					&& !(Master.Factory is ReadOnlyBusinessObjectFactory)
					&& IsPhaseSecurityApplicableCore;
			}
		}

		protected virtual bool IsPhaseSecurityApplicableCore
		{
			get { return true; }
		}

		#endregion

		#region GetPhaseList

		public CodeDescriptionPairList GetPhaseList()
		{
			var result = new CodeDescriptionPairList();

			var phaseSecurity = GetPhaseSecurity();
			if (phaseSecurity != null)
			{
				foreach (IPhase phase in phaseSecurity.Phases)
				{
					result.AddPairIfNotExist(phase.Code, phase.Description);
				}
			}

			return result;
		}

		#endregion

		#region IsEditingAllowed

		public bool IsEditingAllowed
		{
			get
			{
				if (isEditingAllowed == null || PhaseCode != lastCheckedPhaseCode)
				{
					isEditingAllowed = CheckIfEditingAllowedAndInitialiseDependants();
					lastCheckedPhaseCode = PhaseCode;
				}

				return isEditingAllowed.Value;
			}
		}
		ZString lastCheckedPhaseCode = ZString.Empty;
		protected bool? isEditingAllowed;

		protected virtual ZString PhaseCode
		{
			get { return ZString.Empty; }
		}

		bool CheckIfEditingAllowedAndInitialiseDependants()
		{
			ReadOnlyDependants = Enumerable.Empty<IPhaseDependant>();
			MandatoryDependants = Enumerable.Empty<IPhaseDependant>();

			if (!IsPhaseSecurityApplicable || PhaseCode == PhaseConstants.Phase.ALL)
			{
				return true;
			}

			var phaseSecurity = GetPhaseSecurity();
			if (phaseSecurity != null && phaseSecurity.IsEnabled)
			{
				IPhase phase = phaseSecurity.Phases.FirstOrDefault(x => x.Code == PhaseCode);
				if (phase != null)
				{
					IPhaseRule allowedRule = GetAllowedRule(phase.Rules);

					if (allowedRule == null)
					{
						return false;
					}
					else
					{
						string cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", PhaseCode, allowedRule.DepartmentPK.ToString(), allowedRule.Location);
						if (!ReadOnlyDependantsCache.ContainsKey(cacheKey))
						{
							ReadOnlyDependantsCache.Add(cacheKey, allowedRule.ReadOnlyDependants);
						}

						if (!MandatoryDependantsCache.ContainsKey(cacheKey))
						{
							MandatoryDependantsCache.Add(cacheKey, allowedRule.MandatoryDependants);
						}

						ReadOnlyDependants = ReadOnlyDependantsCache[cacheKey];
						MandatoryDependants = MandatoryDependantsCache[cacheKey];
					}
				}
			}

			return true;
		}

		protected IEnumerable<IPhaseDependant> ReadOnlyDependants { get; set; }

		Dictionary<string, IEnumerable<IPhaseDependant>> ReadOnlyDependantsCache
		{
			get
			{
				return
					Master.Factory.GetCachedValue(
						"IPhaseSecurityCache" + Master.GetType().Name,
						() => new Dictionary<string, IEnumerable<IPhaseDependant>>());
			}
		}

		protected IEnumerable<IPhaseDependant> MandatoryDependants { get; private set; }

		Dictionary<string, IEnumerable<IPhaseDependant>> MandatoryDependantsCache
		{
			get
			{
				return
					Master.Factory.GetCachedValue(
						"IPhaseSecurityMandatoryCache" + Master.GetType().Name,
						() => new Dictionary<string, IEnumerable<IPhaseDependant>>());
			}
		}

		IPhaseRule GetAllowedRule(IEnumerable<IPhaseRule> rules)
			=> rules.FirstOrDefault(x => DepartmentMatched(x.DepartmentPK)
													&& x.Location != PhaseConstants.Locations.AnyLocation
													&& LocationMatched(x.Location))
				?? rules.FirstOrDefault(x => DepartmentMatched(x.DepartmentPK)
													&& x.Location == PhaseConstants.Locations.AnyLocation);

		bool DepartmentMatched(ZGuid departmentPK)
		{
			return departmentPK.IsEmpty || departmentPK == GlbDepartment.CurrentDepartment.PK;
		}

		bool LocationMatched(ZString locationCode)
		{
			bool result = false;
			IEnumerable<ZString> relatedUNLOCOs = GetRelatedUNLOCOs();
			foreach (ZString unloco in ResolveUNLOCOs(locationCode))
			{
				if (!unloco.IsEmpty && relatedUNLOCOs.Any(x => x.StartsWith(unloco, StringComparison.Ordinal)))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		protected virtual IEnumerable<ZString> ResolveUNLOCOs(ZString locationCode)
		{
			return Enumerable.Empty<ZString>();
		}

		IEnumerable<ZString> GetRelatedUNLOCOs()
		{
			List<ZString> result = new List<ZString>();

			GlbBranch loginBranch = GlbBranch.CurrentBranch;
			result.Add(loginBranch.GB_RL_NKHomePort);

			foreach (GlbBranchExtraPorts extraPort in loginBranch.ExtraPorts)
			{
				result.Add(extraPort.GY_RL_NKAdditionalBranchRelatedPort);
			}

			return result;
		}

		#endregion

		#region IsPropertyMandatory

		public bool IsPropertyMandatory(ZString propertyName)
		{
			if (!IsEditingAllowed)
			{
				return false;
			}

			if (MandatoryDependants != null)
			{
				return MandatoryDependants.Any(dependant => dependant.Name == propertyName);
			}

			return false;
		}

		#endregion

		#region IsPropertyReadOnly

		public bool IsPropertyReadOnly(ZString propertyName)
		{
			if (!IsEditingAllowed)
			{
				return true;
			}

			if (ReadOnlyDependants != null)
			{
				return ReadOnlyDependants.Any(dependant => string.Equals(dependant.Name, propertyName, StringComparison.Ordinal) || propertyName.StartsWith(dependant.Name + ".", StringComparison.Ordinal));
			}

			return false;
		}

		#endregion

		#region IsChildPropertyReadOnly

		public bool IsChildPropertyReadOnly(ZGuid childPK, ZString childPropertyName)
		{
			if (!IsEditingAllowed)
			{
				return true;
			}

			if (RegisteredChildren.ContainsKey(childPK))
			{
				ZString completePropertyName = RegisteredChildren[childPK] + "." + childPropertyName;
				return IsPropertyReadOnly(completePropertyName);
			}

			return false;
		}

		#endregion

		#region IsChildPropertyMandatory

		public bool IsChildPropertyMandatory(ZGuid childPK, ZString childPropertyName)
		{
			if (IsEditingAllowed && RegisteredChildren.ContainsKey(childPK))
			{
				ZString completePropertyName = RegisteredChildren[childPK] + "." + childPropertyName;
				return IsPropertyMandatory(completePropertyName);
			}

			return false;
		}

		#endregion

		#region RegisteredChildren

		public void RegisterEditableChild(IBusiness child, ZString childName)
		{
			BusinessObject bizo = child as BusinessObject;
			if (bizo != null)
			{
				if (!RegisteredChildren.ContainsKey(bizo.PK))
				{
					RegisteredChildren.Add(bizo.PK, childName);
				}
			}
		}

		protected Dictionary<ZGuid, ZString> RegisteredChildren
		{
			get { return registeredChildren ?? (registeredChildren = new Dictionary<ZGuid, ZString>()); }
		}
		Dictionary<ZGuid, ZString> registeredChildren;

		#endregion

		#region InitialisePhaseDependantMandatoryValidation

		public IEnumerable<ZPropertyInfo> InitialisePhaseDependantMandatoryValidation(IEnumerable<ZPropertyInfo> propertyInfos, ZGuid? childPK = null)
		{
			if (isInitialisingMandatoryValidation)
			{
				return Enumerable.Empty<ZPropertyInfo>();
			}

			using (new DisposableAction(
				() => isInitialisingMandatoryValidation = true,
				() => isInitialisingMandatoryValidation = false))
			{
				return InitialisePhaseDependantMandatoryValidationCore(propertyInfos, childPK);
			}
		}
		bool isInitialisingMandatoryValidation;

		IEnumerable<ZPropertyInfo> InitialisePhaseDependantMandatoryValidationCore(IEnumerable<ZPropertyInfo> propertyInfos, ZGuid? childPK = null)
		{
			if (IsEditingAllowed
				&& (phaseDependantMandatoryValidationProperties.Any()
					|| (PhaseCode != PhaseConstants.Phase.ALL && MandatoryDependants.Any())))
			{
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					var isMandatory = false;
					var key = string.Empty;
					var parentPK = propertyInfo.BizObj != null ? propertyInfo.BizObj.PK : ZGuid.Empty;

					if (childPK == null)
					{
						isMandatory = IsPropertyMandatory(propertyInfo.Name);
						key = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", parentPK, propertyInfo.Name);
					}
					else
					{
						isMandatory = IsChildPropertyMandatory(childPK.Value, propertyInfo.Name);
						key = string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}", parentPK, childPK.Value, propertyInfo.Name);
					}

					if (isMandatory)
					{
						if (!phaseDependantMandatoryValidationProperties.ContainsKey(key))
						{
							var mandatoryValidation = new RunValidationInvoker(() => ValidateIsMandatoryDueToPhase(propertyInfo));
							propertyInfo.AdditionalValidation += mandatoryValidation;
							phaseDependantMandatoryValidationProperties.Add(key, mandatoryValidation);
						}

						yield return propertyInfo;
					}
					else if (!isMandatory && phaseDependantMandatoryValidationProperties.ContainsKey(key))
					{
						propertyInfo.AdditionalValidation -= phaseDependantMandatoryValidationProperties[key];
						phaseDependantMandatoryValidationProperties.Remove(key);
					}
				}
			}
		}
		readonly Dictionary<string, RunValidationInvoker> phaseDependantMandatoryValidationProperties = new Dictionary<string, RunValidationInvoker>();

		void ValidateIsMandatoryDueToPhase(ZPropertyInfo propertyInfo)
		{
			if (IsPropertyValueEmpty(propertyInfo))
			{
				propertyInfo.AddError(GetMandatoryValidationError(propertyInfo));
			}
		}

		protected virtual ZString GetMandatoryValidationError(ZPropertyInfo propertyInfo)
		{
			return Res.GetString("ccddfe0a-c672-48cc-b36f-4868262543b4", "{0} is required due to Phase.", propertyInfo.HumanReadableName);
		}

		bool IsPropertyValueEmpty(ZPropertyInfo propertyInfo)
		{
			// Required for ZGuids held as ZStrings
			if (propertyInfo.Value.GetType() == typeof(ZString) && (ZString)propertyInfo.Value == ZGuid.Empty.ToString())
			{
				return true;
			}

			return propertyInfo.Value.IsEmpty;
		}

		#endregion

		#region Agent UNLOCO and Country

		protected ZString GetAgentUNLOCOIfApplicable(OrgAddress agentAddress)
		{
			return (agentAddress?.Header?.IsProxyOrg(GlbCompany.CurrentCompany) ?? false)
				? agentAddress.OA_RL_NKRelatedPortCode
				: ZString.Empty;
		}

		protected ZString GetAgentCountryIfApplicable(OrgAddress agentAddress)
		{
			if (agentAddress?.Header?.IsProxyOrg(GlbCompany.CurrentCompany) ?? false)
			{
				return agentAddress.OA_RL_NKRelatedPortCode.IsEmpty
					? agentAddress.OA_RN_NKCountryCode
					: GetCountryFromUNLOCO(agentAddress.OA_RL_NKRelatedPortCode);
			}

			return ZString.Empty;
		}

		#endregion

		#region Implementation

		protected ZString GetCountryFromUNLOCO(ZString unloco)
		{
			return unloco.Length == 5 ? unloco.SubstringSafe(0, 2) : ZString.Empty;
		}

		protected bool IsTransit(ZString country, ZString loadCountry, ZString dischargeCountry)
		{
			return !country.IsEmpty && (country != loadCountry && country != dischargeCountry);
		}

		protected IEnumerable<ZString> GetTransitCountries(ZString loadCountry, ZString dischargeCountry, IEnumerable<Transport> transports)
		{
			List<ZString> result = new List<ZString>();
			foreach (Transport transport in transports)
			{
				ZString transportLoadCountry = GetCountryFromUNLOCO(transport.JW_RL_NKLoadPort);
				ZString transportDischargeCountry = GetCountryFromUNLOCO(transport.JW_RL_NKDiscPort);

				if (IsTransit(transportLoadCountry, loadCountry, dischargeCountry))
				{
					result.Add(transportLoadCountry);
				}

				if (IsTransit(transportDischargeCountry, loadCountry, dischargeCountry))
				{
					result.Add(transportDischargeCountry);
				}
			}

			return result.Distinct();
		}

		#endregion
	}
}
