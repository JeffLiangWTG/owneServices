//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefLocoMapValidation
//
//    This class should be used for overriding validation in AutoRefLocoMapValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefLocoMapValidation : AutoRefLocoMapValidation
	{
		public RefLocoMapValidation(AutoRefLocoMap parent) : base(parent)
		{
		}

		protected new RefLocoMap Parent
		{
			get { return (RefLocoMap)base.Parent; }
		}

		protected override void CheckRY_SystemUsage()
		{
			base.CheckRY_SystemUsage();
			if (Parent.RY_IsSystem)
			{
				ListValidation.IfInvalidCode(NotificationType.Warning, Parent.RY_SystemUsageInfo, Parent.Lookups.RY_SystemUsage_List, InvalidSystemUsageWarningMessage);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.RY_SystemUsageInfo);
			}

			CheckPCSCountry();

			CheckDuplicateCountryAndSystemUsageCombinations();
		}

		void CheckDuplicateCountryAndSystemUsageCombinations()
		{
			var loadPort = Parent.LocoPort;
			if (loadPort != null)
			{
				var notificationType = GetDuplicateCountryAndSystemUsageCombinationsNotificationType();
				foreach (RefLocoMap locoMap in loadPort.RefLocoMaps)
				{
					if (Parent != locoMap && Parent.RY_RN == locoMap.RY_RN && Parent.RY_SystemUsage == locoMap.RY_SystemUsage)
					{
						Parent.RY_SystemUsageInfo.AddNotification(notificationType, DuplicateCountryAndSystemUsageCombinations);
						break;
					}
				}
			}
		}

		void CheckPCSCountry()
		{
			if (Parent.RY_SystemUsage == LocoMapSystemUsageList.Codes.PCS)
			{
				if (!Constants.CountryCodes.IsFranceOrTerritory(Parent.Country?.Code))
				{
					Parent.RY_SystemUsageInfo.AddError(TheCountryOfPCSIsNotFranceOrTerritory);
				}
				else if (!Parent.Country.Code.Equals(Parent.LocoPort?.Country?.Code))
				{
					Parent.RY_SystemUsageInfo.AddError(TheCountryOfPCSNotEqualToCountryOfThePort);
				}
			}
		}

		protected virtual CargoWise.ComponentModel.INotificationType GetDuplicateCountryAndSystemUsageCombinationsNotificationType()
		{
			if (Parent.RY_SystemUsage == LocoMapSystemUsageList.Codes.PCS)
			{
				return NotificationType.Error;
			}
			return (Parent.RY_RN == Constants.CountryGuids.UnitedStates || Parent.RY_RN == Constants.CountryGuids.China || Parent.RY_RN == Constants.CountryGuids.Turkey || Parent.RY_RN == Constants.CountryGuids.Mexico) ? NotificationType.Warning : NotificationType.Error;
		}

		public static string TheCountryOfPCSIsNotFranceOrTerritory
		{
			get { return Res.GetString("26b6b33f-b59f-4ff1-9087-6a6ee28c5cd5", "PCS can be used for France and its Overseas Territories only."); }
		}

		public static string TheCountryOfPCSNotEqualToCountryOfThePort
		{
			get { return Res.GetString("266ed4e9-f394-4b1c-8ac2-d345bb8f4d8f", "PCS can be used when country is same as country of the port."); }
		}

		public static string DuplicateCountryAndSystemUsageCombinations
		{
			get { return Res.GetString("2618fa9c-f870-4e5a-a09f-54c7a1e873dd", "There are duplicate combinations of Country/Region and System Usage."); }
		}

		public static string InvalidSystemUsageWarningMessage
		{
			get { return Res.GetString("ff87da77-5226-4f51-a375-eb7bcc6af46c", "This Usage code is not currently valid and may require a later system version to make this code effective"); }
		}

		protected override void CheckRY_RN()
		{
			base.CheckRY_RN();
			MandatoryValidation.CheckEntered(Parent.RY_RNInfo);
		}

		protected override void CheckRY_LocalPortCode()
		{
			base.CheckRY_LocalPortCode();

			if (Parent.RY_SystemUsage == LocoMapSystemUsageList.Codes.PCS)
			{
				MandatoryValidation.CheckEntered(Parent.RY_LocalPortCodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.RY_LocalPortCodeInfo);
			}
			else if (!Parent.RY_LocalPortCode.IsEmpty)
			{
				if (Parent.IsUSScheduleDUsage || Parent.IsCACustomsPortUsage)
				{
					var locoMaps = Parent.Factory.Load<RefLocoMap>(SimilarCodesFilter);
					System.Collections.Generic.List<string> unlocos = new System.Collections.Generic.List<string>();
					foreach (RefLocoMap locoMap in locoMaps)
					{
						if (!unlocos.Contains(locoMap.RY_RL_NKLocoPort))
						{
							unlocos.Add(locoMap.RY_RL_NKLocoPort);
						}
					}
					if (unlocos.Count > 0)
					{
						var builder = new ZStringBuilder(unlocos);
						Parent.RY_LocalPortCodeInfo.AddWarning(string.Format(DuplicateCodeMessage, builder.ToStringWithDelimiterBetweenAppends(", ")));
					}
				}

				if (Parent.RY_RN == Core.Constants.CountryGuids.Iceland && Parent.RY_SystemUsage == OrgCusCode.IcelandCodeTypes.CustomsOfficeCode
					&& (!Parent.RY_LocalPortCode.IsLettersOnlyOrEmpty || Parent.RY_LocalPortCode.Length > 5))
				{
					Parent.RY_LocalPortCodeInfo.AddError(Res.GetString("04235501-f3d4-4f1a-b7c2-f05d4a85699f", "5 alpha characters are allowed for {0}", OrgCusCode.IcelandCodeTypes.CustomsOfficeCode));
				}
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Creates SQL errors")]
		internal const string DuplicateCodeMessage = "This code is already in used on {0}. Please confirm that this is correct.";

		ZQuery SimilarCodesFilter
		{
			get
			{
				var usageFilter = new ZQuery();
				var result = new ZQuery();
				if (Parent.RY_RN == Core.Constants.CountryGuids.UnitedStates)
				{
					usageFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.All);
					usageFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.Air);
					usageFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.Sea);
					usageFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.SCD);
					result.AddToFilter(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.UnitedStates);
				}
				else if (Parent.RY_RN == Core.Constants.CountryGuids.Canada)
				{
					usageFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, CALocoMapSystemUsageList.Codes.All);
					usageFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, CALocoMapSystemUsageList.Codes.Air);
					usageFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, CALocoMapSystemUsageList.Codes.Sea);
					usageFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, CALocoMapSystemUsageList.Codes.Oth);
					result.AddToFilter(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.Canada);
				}
				result.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, SQLComparisonOperator.NotEqual, Parent.RY_RL_NKLocoPort);
				result.AddToFilter(RefLocoMapSchema.RY_LocalPortCode, Parent.RY_LocalPortCode);
				result.AddToFilter(usageFilter);
				result.OrderBy = RefLocoMapSchema.Constants.RY_RL_NKLocoPort;
				return result;
			}
		}
	}
}
