using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USCACCaseSchema.Constants.U5_CaseNumber), DescriptionProperty(USCACCaseSchema.Constants.U5_ShortDescription)]
	public sealed class USCACCase : AutoUSCACCase, IACCase, IControllerIDProvider
	{
		public USCACCase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoUSCACCase.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public bool Exists(ZString tariffNumber, ZString country, ZString caseNumberPrefix)
			{
				country = country.ToUpper();
				caseNumberPrefix = caseNumberPrefix.ToUpper();
				return Factory.GetCachedValue(tariffNumber + "|" + country + "|" + caseNumberPrefix, () =>
				{
					var result = false;
					if (IsDataValid(tariffNumber, country, caseNumberPrefix))
					{
						var dictionary = GetDictionary(caseNumberPrefix);
						if (dictionary.TryGetValue(country, out var tariffDictionary))
						{
							if (tariffDictionary.TryGetValue(tariffNumber, out result) && result)
							{
								return true;
							}

							for (int lengthOfSubString = 9; lengthOfSubString >= 2; lengthOfSubString--)
							{
								var partialTariffNumber = tariffNumber.Left(lengthOfSubString);
								if (tariffDictionary.TryGetValue(partialTariffNumber, out result) && result)
								{
									return true;
								}
							}
						}
						else
						{
							tariffDictionary = new Dictionary<ZString, bool>();
							dictionary.Add(country, tariffDictionary);
						}

						result = ExistsInDatabase(dictionary, tariffDictionary, tariffNumber, country, caseNumberPrefix);
					}

					return result;
				});
			}

			public void AddToNeedToLoadIfNeeded(ZString tariffNumber, ZString country, ZString caseNumberPrefix)
			{
				country = country.ToUpper();
				caseNumberPrefix = caseNumberPrefix.ToUpper();
				if (IsDataValid(tariffNumber, country, caseNumberPrefix))
				{
					var dictionary = GetDictionary(caseNumberPrefix);
					if (!dictionary.TryGetValue(country, out var tariffDictionary))
					{
						tariffDictionary = new Dictionary<ZString, bool>();
						dictionary.Add(country, tariffDictionary);
					}

					if (!TariffExistsOrPartialMatched(tariffDictionary, tariffNumber))
					{
						var needToLoadDictionary = GetNeedToLoadDictionary(caseNumberPrefix);
						AddToNeedToLoad(needToLoadDictionary, tariffDictionary, tariffNumber, country);
					}
				}
			}

			bool IsDataValid(ZString tariffNumber, ZString country, ZString caseNumberPrefix) => tariffNumber.Length == 10 && !country.IsEmpty && !caseNumberPrefix.IsEmpty;

			bool TariffExistsOrPartialMatched(Dictionary<ZString, bool> tariffDictionary, ZString tariffNumber)
			{
				var result = TariffExists(tariffDictionary, tariffNumber);
				if (!result)
				{
					for (int lengthOfSubString = 2; lengthOfSubString <= 10; lengthOfSubString++)
					{
						if (TariffExists(tariffDictionary, tariffNumber.Left(lengthOfSubString)))
						{
							return true;
						}
					}
				}
				return result;
			}

			bool TariffExists(Dictionary<ZString, bool> tariffDictionary, ZString tariffNumber) => tariffDictionary.TryGetValue(tariffNumber, out var exists) && exists;

			void AddToNeedToLoad(Dictionary<ZString, List<ZString>> needToLoadDictionary, Dictionary<ZString, bool> tariffDictionary, ZString tariffNumber, ZString country)
			{
				if (!needToLoadDictionary.TryGetValue(country, out var tariffHashSet))
				{
					tariffHashSet = new List<ZString>();
					needToLoadDictionary.Add(country, tariffHashSet);
				}
				if (!tariffHashSet.Contains(tariffNumber))
				{
					tariffHashSet.Add(tariffNumber);
					for (int lengthOfSubString = 2; lengthOfSubString < 10; lengthOfSubString++)
					{
						var partialTariff = tariffNumber.Left(lengthOfSubString);
						if (!tariffHashSet.Contains(partialTariff) && !tariffDictionary.ContainsKey(partialTariff))
						{
							tariffHashSet.Add(partialTariff);
						}
					}
				}
			}

			Dictionary<ZString, List<ZString>> GetNeedToLoadDictionary(ZString caseNumberPrefix) => Factory.GetCachedValue($"USCACCaseNeedToLoadDictionary|{caseNumberPrefix}", () => new Dictionary<ZString, List<ZString>>());
			Dictionary<ZString, Dictionary<ZString, bool>> GetDictionary(ZString caseNumberPrefix) => Factory.GetCachedValue($"USCACCaseExistsDictionary|{caseNumberPrefix}", () => new Dictionary<ZString, Dictionary<ZString, bool>>());

			const int MaximumNumberOfTariffPerQuery = 1000;

			bool ExistsInDatabase(Dictionary<ZString, Dictionary<ZString, bool>> dictionary, Dictionary<ZString, bool> tariffDictionary, ZString tariffNumber, ZString country, ZString caseNumberPrefix)
			{
				var needToLoadDictionary = GetNeedToLoadDictionary(caseNumberPrefix);
				AddToNeedToLoad(needToLoadDictionary, tariffDictionary, tariffNumber, country);
				var numberOfTariffsRemaining = MaximumNumberOfTariffPerQuery;
				var newDataLoaded = false;
				var currentProcessingDictionary = new Dictionary<ZString, Dictionary<ZString, bool>>();
				var collection = LoadCollection(caseNumberPrefix);
				var tariffNumbers = new Dictionary<ZString, List<ZString>>();
				foreach (var data in needToLoadDictionary)
				{
					while (data.Value.Count > 0)
					{
						var isoCountryCode = data.Key;
						if (!currentProcessingDictionary.TryGetValue(isoCountryCode, out var currentProcessingCountryTariffs))
						{
							currentProcessingCountryTariffs = new Dictionary<ZString, bool>();
							currentProcessingDictionary.Add(isoCountryCode, currentProcessingCountryTariffs);
						}
						var numberOfTariffs = data.Value.Count;
						if (numberOfTariffs < numberOfTariffsRemaining)
						{
							tariffNumbers.Add(isoCountryCode, new List<ZString>(data.Value));
							data.Value.ForEach(tariffValue => currentProcessingCountryTariffs.Add(tariffValue, false));
							numberOfTariffsRemaining = numberOfTariffsRemaining - numberOfTariffs;
							data.Value.Clear();
						}
						else
						{
							var tarffValues = data.Value.Take(numberOfTariffsRemaining).ToList();
							tariffNumbers.Add(isoCountryCode, tarffValues);
							tarffValues.ForEach(tariffValue => currentProcessingCountryTariffs.Add(tariffValue, false));
							data.Value.RemoveRange(0, numberOfTariffsRemaining);
							numberOfTariffsRemaining = 0;
						}

						if (numberOfTariffsRemaining == 0)
						{
							if (LoadData(currentProcessingDictionary, dictionary, collection, tariffNumbers))
							{
								newDataLoaded = true;
							}
							numberOfTariffsRemaining = MaximumNumberOfTariffPerQuery;
							tariffNumbers.Clear();
						}
					}
				}

				if (tariffNumbers.Count > 0)
				{
					if (LoadData(currentProcessingDictionary, dictionary, collection, tariffNumbers))
					{
						newDataLoaded = true;
					}
				}
				UpdateDictionary(currentProcessingDictionary, dictionary);

				return newDataLoaded && TariffExistsOrPartialMatched(tariffDictionary, tariffNumber);
			}

			void UpdateDictionary(Dictionary<ZString, Dictionary<ZString, bool>> currentProcessingDictionary, Dictionary<ZString, Dictionary<ZString, bool>> dictionary)
			{
				currentProcessingDictionary.ForEach(currentProcessingData =>
				{
					var currentProcessingCountryTariffs = currentProcessingData.Value;
					if (!dictionary.TryGetValue(currentProcessingData.Key, out var tariffs))
					{
						tariffs = new Dictionary<ZString, bool>();
						dictionary.Add(currentProcessingData.Key, tariffs);
					}
					currentProcessingCountryTariffs.Keys.ForEach(processedTariffNumber =>
					{
						if (!tariffs.ContainsKey(processedTariffNumber))
						{
							tariffs.Add(processedTariffNumber, false);
						}
					});
				});
			}

			bool LoadData(Dictionary<ZString, Dictionary<ZString, bool>> currentProcessingDictionary, Dictionary<ZString, Dictionary<ZString, bool>> dictionary, DynamicBusinessObjectCollection collection, Dictionary<ZString, List<ZString>> tariffNumbers)
			{
				var newCollection = collection.Cast<DynamicBusinessObject>().Select(x => (((ZString)x[USCACCaseSchema.U5_ISOCountryCode]).ToUpper(), ((ZString)x[USCACCaseTariffSchema.U9_TariffNumber]).ToUpper()));
				var caseList = new List<(ZString, ZString)>();
				foreach (var tariffNumber in tariffNumbers)
				{
					caseList.AddRange(newCollection.Where(x => x.Item1 == tariffNumber.Key && tariffNumber.Value.Contains(x.Item2)).ToArray());
				}
				foreach (var groupData in caseList.GroupBy(x => x.Item1))
				{
					var isoCountryCode = groupData.Key;
					if (!dictionary.TryGetValue(isoCountryCode, out var tariffs))
					{
						tariffs = new Dictionary<ZString, bool>();
						dictionary.Add(isoCountryCode, tariffs);
					}

					var currentProcessingCountryTariffs = currentProcessingDictionary[isoCountryCode];

					foreach (var tariffData in groupData.Select(x => x.Item2))
					{
						if (!tariffs.ContainsKey(tariffData))
						{
							tariffs.Add(tariffData, true);
							currentProcessingCountryTariffs.Remove(tariffData);
						}
					}
				}

				return caseList.Count > 0;
			}

			DynamicBusinessObjectCollection LoadCollection(ZString caseNumberPrefix)
			{
				return Factory.GetCachedValue(string.Format("ACCase_{0}", caseNumberPrefix), () =>
				{
					var collection = new DynamicBusinessObjectCollection(Factory);
					var query = new ZQuery(USCACCaseSchema.U5_CaseStatus, ACCaseStatusList.Codes.AC);
					query.AddToFilter(JoinCondition.And, USCACCaseSchema.U5_CaseNumber, SQLComparisonOperator.StartsWith, caseNumberPrefix);
					var filterParameterisedText = query.ParameterisedText;
					collection.Load($@"
SELECT DISTINCT U5_ISOCountryCode, U9_TariffNumber
FROM {USCACCaseSchema.Constants.TableName}
INNER JOIN {USCACCaseTariffSchema.Constants.TableName} ON U9_CaseNumber = U5_CaseNumber
WHERE {filterParameterisedText.ParameterisedQueryText}
ORDER BY U5_ISOCountryCode, U9_TariffNumber", filterParameterisedText.Parameters);
					return collection;
				});
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(USCACCase);
			}
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return U5_CaseNumber.StartsWith("A") ? "Anti Dumping Case" : "Countervailing Duty Case"; }
		}

		#region Properties For Binding In Module

		[DecimalPlaces(4)]
		public ZDecimal LatestAdValoremRate
		{
			get
			{
				var latestRate = CaseRates.GetDepositRate(ZDate.Today);
				return latestRate != null ? latestRate.U6_AdValoremRate : ZDecimal.Zero;
			}
		}

		public ZString LatestSpecificRate
		{
			get
			{
				var latestRate = CaseRates.GetDepositRate(ZDate.Today);
				return latestRate != null && latestRate.U6_SpecificRate > 0 ? IFeeCalculationDataProviderExtensionMethods.AmountPerUnit(latestRate.U6_SpecificRate, latestRate.U6_Unit) : ZString.Empty;
			}
		}

		public ZString Tariffs
		{
			get
			{
				if (!tariffs.HasValue)
				{
					ZString result = ZString.Empty;

					if (CaseTariffs.Count > 5)
					{
						result = "> 5 Tariffs (double click line to view)";
					}
					else
					{
						foreach (var antiDumpingTariff in CaseTariffs)
						{
							result += antiDumpingTariff.U9_TariffNumber + ", ";
						}
					}

					tariffs = result.Trim().TrimEnd(',');
				}
				return tariffs.Value;
			}
		}
		ZString? tariffs;

		#endregion

		[List(nameof(Lookups) + "." + nameof(USCACCaseLookups.CaseStatusList))]
		public override ZString U5_CaseStatus
		{
			get { return base.U5_CaseStatus; }
			set { base.U5_CaseStatus = value; }
		}

		public ZString CaseStatusDesc
		{
			get { return Lookups.CaseStatusList.GetDescriptionFromCode(U5_CaseStatus) ?? ZString.Empty; }
		}

		[List(nameof(Lookups) + "." + nameof(USCACCaseLookups.CountryCodeList))]
		public override ZString U5_ISOCountryCode
		{
			get { return base.U5_ISOCountryCode; }
			set { base.U5_ISOCountryCode = value; }
		}

		public ZString FormattedPhone1
		{
			get { return GetFormattedPhoneNumber(U5_Phone1); }
		}

		public ZString FormattedPhone2
		{
			get { return GetFormattedPhoneNumber(U5_Phone2); }
		}

		ZString GetFormattedPhoneNumber(ZString wholePhoneNumber)
		{
			int length = wholePhoneNumber.IndexOf("|") > 0 ? wholePhoneNumber.IndexOf("|") : 10;

			ZString result = wholePhoneNumber.SubstringSafe(0, length);

			if (result.Length == 10)
			{
				result = "(" + result.Left(3) + ")" + result.SubstringSafe(3, 3) + "-" + result.SubstringSafe(6);
			}

			if (wholePhoneNumber.Contains("|"))
			{
				result += "(Ex." + wholePhoneNumber.SubstringSafe(wholePhoneNumber.IndexOf("|") + 1) + ")";
			}

			return result;
		}

		public USCACCaseRate GetDepositRate(ZDate effectiveDate)
		{
			return CaseRates.GetDepositRate(effectiveDate);
		}

		[ChildEditable(true)]
		public USCACCaseBondCashCollection BondCashIndicators
		{
			get
			{
				if (bondCashIndicators == null)
				{
					bondCashIndicators = new USCACCaseBondCashCollection(this);
					RegisterEditableChildObject(bondCashIndicators);
				}
				return bondCashIndicators;
			}
		}
		USCACCaseBondCashCollection bondCashIndicators;

		[ChildEditable(true)]
		public USCACCaseEventCollection CaseEvents
		{
			get
			{
				if (caseEvents == null)
				{
					caseEvents = new USCACCaseEventCollection(this);
					RegisterEditableChildObject(caseEvents);
				}
				return caseEvents;
			}
		}
		USCACCaseEventCollection caseEvents;

		[ChildEditable(true)]
		public USCACCaseLiqSuspensionCollection LiqSuspensions
		{
			get
			{
				if (liqSuspensions == null)
				{
					liqSuspensions = new USCACCaseLiqSuspensionCollection(this);
					RegisterEditableChildObject(liqSuspensions);
				}
				return liqSuspensions;
			}
		}
		USCACCaseLiqSuspensionCollection liqSuspensions;

		[ChildEditable(true)]
		public USCACCaseRateCollection CaseRates
		{
			get
			{
				if (caseRates == null)
				{
					caseRates = new USCACCaseRateCollection(this);
					RegisterEditableChildObject(caseRates);
					caseRates.ApplySort(USCACCaseRateSchema.U6_EffectiveDate.Name, System.ComponentModel.ListSortDirection.Descending);
				}
				return caseRates;
			}
		}
		USCACCaseRateCollection caseRates;

		[ChildEditable(true)]
		public USCACCaseTariffCollection CaseTariffs
		{
			get
			{
				if (caseTariffs == null)
				{
					caseTariffs = new USCACCaseTariffCollection(this);
					RegisterEditableChildObject(caseTariffs);
				}
				return caseTariffs;
			}
		}
		USCACCaseTariffCollection caseTariffs;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new USCACCaseFetchStrategy(this);
		}

		class USCACCaseFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public USCACCaseFetchStrategy(USCACCase acCase)
				: base(acCase)
			{
			}

			new USCACCase BusinessObject
			{
				get { return (USCACCase)base.BusinessObject; }
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();

				Factory.AddFetchHint(USCACCaseBondCashSchema.U8_CaseNumber, BusinessObject.U5_CaseNumber);
				Factory.AddFetchHint(USCACCaseEventSchema.U7_CaseNumber, BusinessObject.U5_CaseNumber);
				Factory.AddFetchHint(USCACCaseLiqSuspensionSchema.UN_CaseNumber, BusinessObject.U5_CaseNumber);
				Factory.AddFetchHint(USCACCaseRateSchema.U6_CaseNumber, BusinessObject.U5_CaseNumber);
				Factory.AddFetchHint(USCACCaseTariffSchema.U9_CaseNumber, BusinessObject.U5_CaseNumber);
			}
		}

		ZString IACCase.CaseNumber
		{
			get { return U5_CaseNumber; }
		}

		ZString IACCase.CountryCode
		{
			get { return U5_ISOCountryCode; }
		}

		bool IACCase.IsEffective
		{
			get { return U5_CaseStatus == ACCaseStatusList.Codes.AC; }
		}

		bool IACCase.IsReportable(ZDateTime effectiveDate)
		{
			var orderedSuspensions = LiqSuspensions?.OfType<USCACCaseLiqSuspension>().Where(x => x.UN_InactivatedDate.IsEmpty).OrderBy(x => x.UN_EffectiveDate);
			var latestStartSus = orderedSuspensions?.Where(x => x.UN_Action == ActionSTART && x.UN_EffectiveDate <= effectiveDate)?.LastOrDefault();
			var latestStopSus = orderedSuspensions?.Where(x => x.UN_Action == ActionSTOP && x.UN_EffectiveDate < effectiveDate)?.LastOrDefault();

			return latestStartSus != null && (latestStopSus == null || latestStartSus.UN_EffectiveDate >= latestStopSus.UN_EffectiveDate);
		}

		const string ActionSTART = "START";
		const string ActionSTOP = "STOP";

		ZString IACCase.CaseStatus
		{
			get { return U5_CaseStatus; }
		}

		CodeDescriptionPairList IACCase.CaseStatusList
		{
			get { return Factory.GetCachedValue<ACCaseStatusList>(); }
		}

		ZString IACCase.ManufacturerIDCode
		{
			get { return U5_ManufacturerMID; }
		}

		bool IACCase.IsCashRequired(ZDate effectiveDate)
		{
			return BondCashIndicators.IsCashRequired(effectiveDate);
		}

		IEnumerable<ZString> IACCase.RelatedTariffs
		{
			get
			{
				return from USCACCaseTariff dumpingTariff in CaseTariffs
					   select dumpingTariff.U9_TariffNumber;
			}
		}

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.US.USCACCase; }
		}

		#endregion
	}
}
