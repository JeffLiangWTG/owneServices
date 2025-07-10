using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Freight.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using IntegratedCountryEntryStatus = Enterprise.Customs.Common.Shared.IntegratedCountryCommonEntryStatusList;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class JobDeclarationEventParentFinder : EventParentFinder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translateable constant")]
		const string containerAutomationDataProvider = "WTG Tracking & Automation";

		public JobDeclarationEventParentFinder(BusinessObjectFactory factory, JobDeclarationDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected sealed override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var result = GetLogParentsForEventUsingContextCore(eventDataObject);

			if (result != null && IsContainerAutomationEvent(eventDataObject))
			{
				return result.Where(IsBizoValidContainerAutomationLogParent).ToArray();
			}

			return result;
		}

		protected virtual BusinessObject[] GetLogParentsForEventUsingContextCore(UniversalEvent eventDataObject)
		{
			if (eventDataObject.IsUSCATAIRMessageEvent(DataContextType.CustomsDeclaration))
			{
				return ObjectFactory.Get<IUSCATAIRMessageEventParentFinder>().GetLogParentsForEventUsingContext(eventDataObject, factory);
			}
			else if (eventDataObject.IsAirImportCustomsMessageEvent())
			{
				return GetMatchAirImportCustomsDeclarations(eventDataObject);
			}
			else
			{
				return GenerateLogParentsForEventUsingContext(eventDataObject);
			}
		}

		bool IsBizoValidContainerAutomationLogParent(BusinessObject bizo) => !(bizo is BaseJobDeclaration declaration) || declaration.JE_JS.IsEmpty;

		BusinessObject[] GetEventContext(UniversalEvent eventDataObject)
		{
			BusinessObject[] result = null;
			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			var isAir = !eventValueObject.Context.MAWBNumber.IsEmpty || !eventValueObject.Context.HAWBNumber.IsEmpty;
			var masterbill = isAir ? eventValueObject.Context.MAWBNumber : eventValueObject.Context.MBOLNumber.GetValueOrDefault();
			var housebill = isAir ? eventValueObject.Context.HAWBNumber : eventValueObject.Context.HBOLNumber.GetValueOrDefault();

			var declarations = GetDeclarations(masterbill, housebill, isAir);
			if (declarations != null && declarations.Length > 0)
			{
				var list = new List<BusinessObject>();
				foreach (var declaration in declarations)
				{
					list.AddRange(GetRelevantParent(eventValueObject, isAir, declaration));
				}
				result = list.ToArray();
			}

			if (result == null && !isAir)
			{
				result = GetMatchedSeaContainers(eventDataObject);
			}

			return result;
		}

		CommonContainer[] GetMatchedSeaContainers(UniversalEvent eventDataObject)
		{
			var matchedContainers = new List<CommonContainer>();
			var xmlEvent = eventDataObject as IXmlEventValueObject;
			if (xmlEvent != null)
			{
				var containerNumbers = xmlEvent.Context.ContainerNumbers;

				if (containerNumbers != null && containerNumbers.Any())
				{
					var lloydsNumber = xmlEvent.Context.LloydsNumber.GetValueOrDefault();
					var vesselName = xmlEvent.Context.VesselName.GetValueOrDefault();
					var voyageNumber = xmlEvent.Context.VoyageNumber.GetValueOrDefault();
					var containers = FindMatchedContainers(containerNumbers, lloydsNumber, vesselName, voyageNumber);
					if (containers != null && containers.Any())
					{
						matchedContainers.AddRange(containers);
					}
				}
			}

			return matchedContainers.Any() ? matchedContainers.ToArray() : null;
		}

		CommonContainer[] FindMatchedContainers(List<ZString> containerNumbers, ZString lloydsNumber, ZString vesselNameInEvent, ZString voyageNumber)
		{
			CommonContainer[] matchedContainers = null;

			var vesselName = vesselNameInEvent;
			if (vesselName.IsEmpty && !lloydsNumber.IsEmpty)
			{
				var vessel = FindVessel(lloydsNumber);
				if (vessel != null)
				{
					vesselName = vessel.RV_Code;
				}
			}

			if (!vesselName.IsEmpty && !voyageNumber.IsEmpty)
			{
				var declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_JS, null);
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_VesselName, vesselName);
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_VoyageFlightNo, voyageNumber);
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_TransportMode, TransportTypeList.Codes.Sea);

				var query = new ZDBOnlyQuery(typeof(BaseCusContainer));
				query.AddToFilter(CusContainerSchema.CO_ContainerNumber, containerNumbers);
				query.AddSubQuery(CusContainerSchema.CO_JE, declarationQuery, JoinCondition.And);

				var containers = factory.Load<BaseCusContainer>(query);
				matchedContainers = containers.Where(c => c.JobContainer != null && c.JobContainer.JC_JK.IsEmpty).Select(c => c.JobContainer).ToArray();
			}

			return matchedContainers;
		}

		RefVessel FindVessel(ZString lloydsNumber)
		{
			RefVessel vessel = null;

			if (!lloydsNumber.IsEmpty)
			{
				var findVesselQuery = new ZDBOnlyQuery(typeof(RefVessel));
				findVesselQuery.AddToFilter(RefVesselSchema.RV_LloydsNumber, lloydsNumber);
				vessel = factory.LoadTop1<RefVessel>(findVesselQuery);
			}

			return vessel;
		}

		public BusinessObject[] GenerateLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			var declarationReference = eventValueObject.Context.DeclarationReference;
			var countryOfIssue = eventValueObject.Context.EntryNumberCountryOfIssue;
			var entryNumber = eventValueObject.Context.EntryNumber;
			var entryNumberType = eventValueObject.Context.EntryNumberType;

			if (!entryNumber.IsEmpty && !entryNumberType.IsEmpty && !countryOfIssue.IsEmpty)
			{
				var declaration = GetDeclaration(declarationReference, eventDataObject.DataContext?.CompanyCodeToImportInto ?? ZString.Empty, eventDataObject.DataContext?.CountryCodeToImportInto ?? ZString.Empty);
				var linkToDeclaration = false;
				if (declaration != null || declarationReference.IsEmpty)
				{
					var entryNumberBO = GetRelatedEntryNumberBO(entryNumber, entryNumberType, countryOfIssue, declaration);
					CusEntryHeader entry = null;

					if (entryNumberBO != null)
					{
						if (entryNumberBO.CE_ParentTable == CusEntryHeaderSchema.Constants.TableName)
						{
							entry = factory.Load<CusEntryHeader>(entryNumberBO.CE_ParentID);
							declaration = declaration ?? entry.Declaration;
						}
						else if (entryNumberBO.CE_ParentTable == JobDeclarationSchema.Constants.TableName)
						{
							declaration = declaration ?? factory.Load<BaseJobDeclaration>(entryNumberBO.CE_ParentID);
							linkToDeclaration = true;
						}
					}

					if (declaration != null && countryOfIssue == declaration.CountryCode)
					{
						using (SetupEnvironmentIfNeeded(declaration))
						{
							if (declaration.IsDeclarationIntegrated)
							{
								if (linkToDeclaration)
								{
									return new[] { (BusinessObject)declaration };
								}
								else
								{
									if (entry == null)
									{
										entry = MatchOnBGMReferenceOrCreate(eventValueObject, declaration);
									}

									if (entry != null)
									{
										UpdateEntryDetails(entry, eventDataObject);
									}
								}
							}

							if (entry != null)
							{
								return new[] { (BusinessObject)entry };
							}
							return GetRelatedEntryBOs(declaration, declarationReference, eventValueObject.Context.MessageType);
						}
					}
					else
					{
						logger.Log(LogType.Error, Res.GetString("3B73FC97-8DB0-4C50-8BA6-546298CB3CC4",
							"Cannot find any matching declaration/entry number combination for Declaration Reference: '{0}', Entry Type: '{1}' Entry Number: '{2}', Country of Issue: '{3}'.",
							declarationReference, entryNumberType, entryNumber, countryOfIssue));
						return null;
					}
				}
				else
				{
					logger.Log(LogType.Error, Res.GetString("D22A9CCD-7522-4298-8B2F-A99E8460D38D", "Cannot find any matching declaration for declaration reference '{0}'.", declarationReference));
					return null;
				}
			}

			return GetEventContext(eventDataObject);
		}

		IDisposable SetupEnvironmentIfNeeded(BaseJobDeclaration declaration)
		{
			return declaration.JE_GB.IsValid && GlbBranch.CurrentBranch.PK != declaration.JE_GB ? DisposableEnvironment.ForBranch(declaration.JE_GB.ToGuid()) : new DisposableObject();
		}

		protected virtual BusinessObject[] GetRelatedEntryBOs(BaseJobDeclaration declaration, ZString declarationReference, ZString? messageType)
		{
			BusinessObject[] result = null;
			if (!declarationReference.IsEmpty || messageType.HasValue)
			{
				var entries = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
				if (!declarationReference.IsEmpty)
				{
					entries = entries.Where(x => x.CH_BGMReference == declarationReference);
				}
				if (messageType.HasValue)
				{
					entries = entries.Where(x => x.CH_MessageType == messageType.Value);
				}
				result = entries.ToArray();
				result = result.Length == 0 ? null : result;
			}
			return result;
		}

		CusEntryHeader MatchOnBGMReferenceOrCreate(IXmlEventValueObject eventValueObject, BaseJobDeclaration declaration)
		{
			var expectedBGMReference = declaration.JE_DeclarationReference + "/" + eventValueObject.Context.EntryNumber;
			if (expectedBGMReference.Length <= CusEntryHeader.Schema.CH_BGMReferenceMaxLength)
			{
				foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
				{
					if ((entry.CH_BGMReference == expectedBGMReference || entry.CH_BGMReference == eventValueObject.Context.EntryNumber)
						&&
						entry.CH_MessageType == eventValueObject.Context.EntryNumberType)
					{
						return entry;
					}
				}

				logger.Log(LogType.Information, Res.GetString("ADFAB40A-0CEB-41E3-90C0-716C88C2BD74",
					"Existing entries collection of {0}, with keys [{1}], did not have a matching record.",
					declaration.ActiveEntryHeaders.Count, declaration.ActiveEntryHeaders.BGMReferencesAsCommaDelimitedString));

				var result = declaration.ActiveEntryHeaders.AddNew();
				SetValue(result.CH_MessageTypeInfo, eventValueObject.Context.EntryNumberType);
				SetValue(result.CH_BGMReferenceInfo, expectedBGMReference);

				logger.Log(LogType.Information, Res.GetString("1B76E4CE-A5DE-4EE8-9436-3F328F6FAACE",
					"Created new customs entry header with BGM reference = {0}, and message type = {1}.", result.CH_BGMReference, result.CH_MessageType));
				return result;
			}
			else
			{
				logger.Log(LogType.Error, Res.GetString("2AC9777F-5691-4E4B-8E84-2DDE11D6E7C7",
					"Cannot create a new Entry for Declaration {0} as the combination of Declaration Reference and Entry Number ('{1}') is longer than {2} characters.",
					declaration.JE_DeclarationReference, @expectedBGMReference, CusEntryHeader.Schema.CH_BGMReferenceMaxLength));
			}

			return null;
		}

		void SetValue(ZPropertyInfo info, ZString value)
		{
			info.Value = value.Left(info.MaxLength);
		}

		protected virtual void UpdateEntryDetails(CusEntryHeader entry, UniversalEvent eventDataObject)
		{
			UpdateEntryStatus(entry, eventDataObject);

			if (!entry.CH_EntrySubmittedDate.IsValid && CheckSubmittedDate(eventDataObject))
			{
				entry.CH_EntrySubmittedDate = eventDataObject.EventTime.GetValueOrDefault().ToZDateTime();
			}

			if (!entry.CH_EntryReleaseDate.IsValid && CheckReleaseDate(eventDataObject, ZDateTime.Today, entry.Declaration.Country))
			{
				entry.CH_EntryReleaseDate = eventDataObject.EventTime.GetValueOrDefault().ToZDateTime();
			}
		}

		protected void UpdateEntryStatus(CusEntryHeader entry, UniversalEvent eventDataObject)
		{
			var eventType = eventDataObject.EventType.GetValueOrDefault();
			var eventReference = eventDataObject.EventReference.GetValueOrDefault();

			if (CanUpdateEntryStatus(eventType))
			{
				var entryStatus = GetEntryStatus(eventType, eventReference);
				if (!entryStatus.IsEmpty)
				{
					SetValue(entry.CH_EntryStatusInfo, entryStatus);
				}
			}
		}

		protected virtual bool CanUpdateEntryStatus(ZString eventType)
		{
			return eventType.EqualsIgnoringCase(Events.CustomsEntryStatusCode);
		}

		CusEntryNumber GetRelatedEntryNumberBO(ZString entryNumber, ZString entryNumberType, ZString entryNumberCountryOfIssue, BaseJobDeclaration declaration)
		{
			var entryNumberQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryNumberType);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, entryNumberCountryOfIssue);

			if (declaration == null)
			{
				var subQueryForJob = new ZDBOnlyQuery(typeof(CusEntryNumber));
				var jobSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				subQueryForJob.AddSubQuery(CusEntryNumSchema.CE_ParentID, jobSubQuery, JoinCondition.And);

				var subQueryForEntry = new ZDBOnlyQuery(typeof(CusEntryNumber));
				var entrySubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.PK);
				subQueryForEntry.AddSubQuery(CusEntryNumSchema.CE_ParentID, entrySubQuery, JoinCondition.And);

				var subQuery = new ZQuery();
				subQuery.AddToFilter(subQueryForJob);
				subQuery.AddToFilter(subQueryForEntry, JoinCondition.Or);

				entryNumberQuery.AddToFilter(subQuery);
			}
			else
			{
				var parentIDQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, declaration.PK);
				var numberQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
				var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.PK);
				entryHeaderSubQuery.AddToFilter(CusEntryHeaderSchema.CH_JE, declaration.PK);
				numberQuery.AddSubQuery(CusEntryNumSchema.CE_ParentID, entryHeaderSubQuery, JoinCondition.And);

				var query = new ZQuery();
				query.AddToFilter(parentIDQuery);
				query.AddToFilter(numberQuery, JoinCondition.Or);

				entryNumberQuery.AddToFilter(query);
			}

			return factory.LoadTop1<CusEntryNumber>(entryNumberQuery);
		}

		protected BusinessObject[] GetRelevantParent(IXmlEventValueObject eventValueObject, bool isAir, BaseJobDeclaration declaration)
		{
			using (SetupEnvironmentIfNeeded(declaration))
			{
				var containerNumbers = isAir ? eventValueObject.Context.ULDIdentifications : eventValueObject.Context.ContainerNumbers;
				var containers = GetContainerFromDeclarationIfxmlEventIsForIt(declaration, eventValueObject, containerNumbers);
				var eventTypeShouldLinkTransport = ShouldEventTypeLinkTransport(eventValueObject);
				if (containers == null
					|| containers.Length == 0
					|| (containerNumbers.Count == 1 && (isAir || declaration.IsSea) && eventTypeShouldLinkTransport))
				{
					var transport = GetTransportFromDeclarationIfxmlEventIsForIt(declaration, eventValueObject, isAir);
					if (transport != null)
					{
						return new[] { transport };
					}
				}

				return containers != null && containers.Any() ? containers : new[] { declaration };
			}
		}

		CommonContainer[] GetContainerFromDeclarationIfxmlEventIsForIt(BaseJobDeclaration declaration, IXmlEventValueObject eventValueObject, List<ZString> containerNumbers)
		{
			var isContainerAutomationEvent = IsContainerAutomationEvent(eventValueObject);
			if (containerNumbers != null && containerNumbers.Any())
			{
				return declaration
					.CusContainers
					.Where(c => containerNumbers.Contains(c.CO_ContainerNumber) && !(isContainerAutomationEvent && IsContainerDeclarationLinked(c)))
					.Select(c => c.JobContainer)
					.ToArray();
			}

			return null;
		}

		bool ShouldEventTypeLinkTransport(IXmlEventValueObject eventType)
		{
			var type = eventType.EventType;
			return EventTransformerHelper.IsVesselEvent(type)
				|| type == AutoEvents.ArrivalCode
				|| type == AutoEvents.DepartureCode
				|| type == AutoEvents.FreightLoadedCode;
		}

		bool IsContainerAutomationEvent(IXmlEventValueObject eventValueObject)
			=> eventValueObject.DataContext != null
			&& eventValueObject.DataContext.DataProviderForCodeMapping.EqualsIgnoringCase(containerAutomationDataProvider);

		bool IsContainerDeclarationLinked(BaseCusContainer container) => container.Declaration != null && !container.Declaration.JE_JS.IsEmpty;

		Transport GetTransportFromDeclarationIfxmlEventIsForIt(BaseJobDeclaration declaration, IXmlEventValueObject eventValueObject, bool isAir)
		{
			Transport result = null;
			var transportsInLegOrder = GetTransportInLegOrder(declaration.Transports);
			if (isAir)
			{
				var flightNo = eventValueObject.Context.FlightNumber.GetValueOrDefault();
				if (!flightNo.IsEmpty)
				{
					foreach (var transport in transportsInLegOrder)
					{
						if (transport.IsAir && transport.IsVoyageFlightMatched(flightNo))
						{
							result = transport;
							break;
						}
					}
				}
			}
			else
			{
				var vesselName = eventValueObject.Context.VesselName.GetValueOrDefault();
				var voyageNumber = eventValueObject.Context.VoyageNumber.GetValueOrDefault();
				if (!vesselName.IsEmpty && !voyageNumber.IsEmpty)
				{
					foreach (var transport in transportsInLegOrder)
					{
						if (!transport.IsAir && transport.JW_Vessel == vesselName && transport.JW_VoyageFlight == voyageNumber)
						{
							result = transport;
							break;
						}
					}
				}
			}
			return result;
		}

		Transport[] GetTransportInLegOrder(TransportCollection transports)
		{
			var legOrderFilter = new ZQuery();
			legOrderFilter.OrderBy = JobConsolTransportSchema.JW_LegOrder.Name;
			return (Transport[])transports.Find(legOrderFilter);
		}

		BaseJobDeclaration[] GetDeclarations(ZString masterbill, ZString housebill, bool isAir)
		{
			BaseJobDeclaration[] result = null;

			if (!masterbill.IsEmpty || !housebill.IsEmpty)
			{
				var declarationFilter = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

				if (!masterbill.IsEmpty)
				{
					var masterbillSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
					masterbillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);

					var masterBillNumFilter = new ZQuery(CusDecHouseBillSchema.CU_BillNum, GetMasterbillIgnoreHyphenAndSpace(masterbill));
					masterBillNumFilter.AddToFilter(JoinCondition.Or, CusDecHouseBillSchema.CU_BillNum, masterbill);
					masterbillSubQuery.AddToFilter(masterBillNumFilter);

					declarationFilter.AddSubQuery(masterbillSubQuery, JoinCondition.And);
				}
				if (!housebill.IsEmpty)
				{
					var housebillSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
					housebillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.HouseBill);
					housebillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, housebill);
					declarationFilter.AddSubQuery(housebillSubQuery, JoinCondition.And);
				}

				if (isAir)
				{
					declarationFilter.AddToFilter(JobDeclarationSchema.JE_TransportMode, Core.Constants.TransportModes.Air);
				}
				else
				{
					declarationFilter.AddToFilter(JobDeclarationSchema.JE_TransportMode, SQLComparisonOperator.NotEqual, Core.Constants.TransportModes.Air);
				}

				result = factory.Load<BaseJobDeclaration>(declarationFilter);
				var maxTTLMasterBill = 4;
				if (maxTTLMasterBill > 0)
				{
					var oldestMasterBillAllowed = ZDateTime.Now.AddMonths(-maxTTLMasterBill);
					var oldDeclarations = result.Where(x => x.JE_SystemCreateTimeUtc < oldestMasterBillAllowed).ToArray();
					if (oldDeclarations != null)
					{
						foreach (var dec in oldDeclarations)
						{
							logger.Log(Integration.LogType.Warning, Res.GetString("93266389-de40-4af2-b639-afe76885bfc4", "Declaration {0} has been excluded due to it being more than {1} months old.", dec.JE_DeclarationReference, maxTTLMasterBill));
						}
					}
					result = result.Except(oldDeclarations).ToArray();
				}
			}
			return result;
		}

		BaseJobDeclaration[] GetMatchAirImportCustomsDeclarations(UniversalEvent eventDataObject)
		{
			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			var masterbill = eventValueObject.Context.MAWBNumber;
			var housebill = eventValueObject.Context.HAWBNumber;
			var declarationFilter = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			declarationFilter.AddToFilter(JobDeclarationSchema.JE_MasterBill, GetMasterbillIgnoreHyphenAndSpace(masterbill));
			declarationFilter.AddToFilter(JobDeclarationSchema.JE_HouseBill, housebill);
			declarationFilter.AddToFilter(JobDeclarationSchema.JE_TransportMode, Core.Constants.TransportModes.Air);
			declarationFilter.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
			var result = factory.Load<BaseJobDeclaration>(declarationFilter);

			var count = result.Length;
			if (count == 1)
			{
				UpdateDeclarationLandedPieces(result[0], eventDataObject.EventParameters.Quantity.GetValueOrDefault());
			}
			else if (count > 1)
			{
				logger.Log(LogType.Warning, Res.GetString("1994F7E8-8EB2-4B19-9836-657796935871", "Matching failed. There are more than one declaration matched."));
				result = null;
			}
			return result;
		}

		ZString GetMasterbillIgnoreHyphenAndSpace(ZString masterbill)
		{
			return masterbill.Replace("-", "").Replace(" ", "");
		}

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			var logParent = logParents[0];

			var declaration = logParent as BaseJobDeclaration;
			if (declaration != null)
			{
				var relevantParents = GetRelevantParent(eventData, declaration.IsAir, declaration);

				logParent = relevantParents != null && relevantParents.Any() ? relevantParents[0] : logParent;
				if (logParent == declaration)
				{
					UpdateDeclarationEntryStatusForIntegratedCountry(declaration, eventData);
				}

				var visualizableDocumentSupporter = declaration.GetSupporter();
				var documentEventParent = visualizableDocumentSupporter?.GetEventParent(eventData);

				if (documentEventParent is BusinessObject documentEventParentBO)
				{
					return new BusinessObject[] { logParent, documentEventParentBO };
				}
			}

			return new BusinessObject[] { logParent };
		}

		void UpdateDeclarationEntryStatusForIntegratedCountry(BaseJobDeclaration declaration, UniversalEvent eventData)
		{
			var customsCountryOfJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(declaration.CountryCode);
			if (IntegratedCountryHelper.IsInterfaceEnabledCompany(declaration.JE_GC, customsCountryOfJurisdiction) && (declaration.IsInterface || !IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(customsCountryOfJurisdiction)))
			{
				var eventType = eventData.EventType.GetValueOrDefault();
				if (eventType == Events.CustomsEntryStatusCode)
				{
					var eventReference = eventData.EventReference.GetValueOrDefault();
					if (!eventReference.IsEmpty &&
						(eventReference == IntegratedCountryEntryStatus.Codes.Acknowledged ||
						 eventReference == IntegratedCountryEntryStatus.Codes.Submitted ||
						 declaration.Lookups.EntryStatusList.ContainsCode(eventReference)))
					{
						declaration.JE_EntryStatus = eventReference;

						if (eventReference == IntegratedCountryEntryStatus.Codes.Acknowledged)
						{
							declaration.LogCustomsCommencedIfNeeded();
						}
					}
				}
			}
		}

		void UpdateDeclarationLandedPieces(BaseJobDeclaration declaration, ZInt quantity)
		{
			declaration.JE_LandedPieces = quantity;
		}

		BaseJobDeclaration GetDeclaration(ZString declarationReference, ZString companyCode, ZString countryCode)
		{
			return JobDeclarationEventHelper.GetJobDeclarationFor(factory, GetDeclarationReference(declarationReference), companyCode, countryCode);
		}

		protected virtual ZString GetDeclarationReference(ZString declarationReference)
		{
			var values = declarationReference.Split('/');
			return values.Length > 1 ? values[0] : declarationReference;
		}

		protected virtual ZString GetEntryStatus(ZString eventType, ZString eventReference)
		{
			return eventReference;
		}

		protected virtual bool CheckSubmittedDate(UniversalEvent eventDataObject)
		{
			return false;
		}

		protected virtual bool CheckReleaseDate(UniversalEvent eventDataObject, ZDateTime dateForMap, RefCountry country)
		{
			if (country != null)
			{
				return CustomsStatusAttributeHelper.ShouldUpdateReleaseDate(factory, eventDataObject.EventReference.GetValueOrDefault(), country.Code, dateForMap);
			}
			else
			{
				return false;
			}
		}
	}
}
