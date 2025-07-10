using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.DataTransfer.Freight.Universal;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using EventConstants = CargoWise.EventReference.Constants;
using EZC = Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ConsolDataObjectReader : CommonConsolDataObjectReader<ForwardingConsol>
	{
		public ConsolDataObjectReader(UniversalShipment consolDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IUniversalFreightHelper helper = null)
			: base(consolDataObject, logger, factory)
		{
			logger.IsUpdatingConsol = true;
			this.helper = helper ?? new UniversalForwardingHelper();
			freightJobMawbLink = new FreightJobMawbLink(factory.BOFactory);
		}

		readonly IUniversalFreightHelper helper;
		readonly FreightJobMawbLink freightJobMawbLink;

		protected override ForwardingConsol GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var isAirConsol = dataObject.TransportMode.GetCodeAsUpperCase() == Constants.TransportModes.Air;
			var isCoload = dataObject.ShipmentType.GetCodeAsUpperCase() == Core.Constants.AgentType.CoLoad;
			var wayBillNumber = dataObject.WayBillNumber.GetValueOrDefault();
			var coloadWayBillNumber = dataObject.CoLoadMasterBillNumber.GetValueOrDefault();
			var bookingConfirmationRef = dataObject.BookingConfirmationReference.GetValueOrDefault();
			var coloadBookingConfirmationRef = dataObject.CoLoadBookingConfirmationReference.GetValueOrDefault().Split(ForwardingConsol.BookingReferenceSeperatorCharacters.ToArray()).FirstOrDefault();

			if (isCoload)
			{
				bookingConfirmationRef = bookingConfirmationRef.Split(ForwardingConsol.BookingReferenceSeperatorCharacters.ToArray()).FirstOrDefault();
			}

			var isWaybillNumberLengthCorrect = true;
			if (isAirConsol)
			{
				wayBillNumber = wayBillNumber.Replace("-", "");
				isWaybillNumberLengthCorrect = wayBillNumber.IsEmpty || wayBillNumber.Length == AutoJobMawb.Schema.JM_Airline3DigitPrefixMaxLength + AutoJobMawb.Schema.JM_MAWBMaxLength;
			}

			if (isWaybillNumberLengthCorrect
				&& (!wayBillNumber.IsEmpty || !bookingConfirmationRef.IsEmpty || !coloadWayBillNumber.IsEmpty || !coloadBookingConfirmationRef.IsEmpty))
			{
				var commonConsolReferences = GetCommonConsolReferences();

				var fetchParam = new ConsolFetchParam()
				{
					MasterBillNumber = wayBillNumber,
					BookingConfirmationReference = bookingConfirmationRef,
					CoLoadMasterBillNumber = coloadWayBillNumber,
					CoLoadBookingConfirmationReference = coloadBookingConfirmationRef,
					SCAC = dataObject.GetSCAC(DocAddressType.ShippingLineAddress),
					CoLoadSCAC = dataObject.GetSCAC(DocAddressType.CoLoadWith),
					C1C = dataObject.GetC1C(DocAddressType.ShippingLineAddress),
					CoLoadC1C = dataObject.GetC1C(DocAddressType.CoLoadWith)
				};

				var ruleGroups = ForwardingConsolMatchService.GetConsolFetchRule(fetchParam);
				foreach (var group in ruleGroups)
				{
					var scoredConsols = GetMatchingConsols(group.consolFetchRule, delegate (ForwardingConsol consol)
					{
						return Filter(consol, isAirConsol, group.isCoLoad);
					});

					if (scoredConsols == null)
					{
						continue;
					}

					var highestScore = scoredConsols.Select(scoredConsol => scoredConsol.score).OrderByDescending(score => score).FirstOrDefault();
					var matchedConsols = scoredConsols
						.Where(scoredConsole => scoredConsole.score == highestScore)
						.Select(scoredConsole => scoredConsole.consol)
						.ToArray();

					if (matchedConsols.Length == 1)
					{
						IsUseNonCoLoadToMatchCoLoad = group.consolFetchRule.IsUseNonCoLoadToMatchCoLoad;
						return matchedConsols.First();
					}

					if (!highestScore.IsFullScore(group.consolFetchRule.ScoreRules))
					{
						continue;
					}

					var specificMatcher = new ForwardingConsolSpecificMatcher(matchedConsols, highestScore, group.consolFetchRule.MatchRules, factory.BOFactory, GetCommonConsolReferences(), logger, helper);
					var bestMatch = specificMatcher.GetBestMatch();
					if (bestMatch != null)
					{
						IsUseNonCoLoadToMatchCoLoad = group.consolFetchRule.IsUseNonCoLoadToMatchCoLoad;
						return bestMatch;
					}
				}
			}

			if (dataObject.IsConsolidationAdviceMessage() && dataObject.IsOriginal() && dataObject.SubShipmentCollection != null)
			{
				ForwardingConsol consol = null;
				foreach (var shipmentDataObject in dataObject.SubShipmentCollection)
				{
					var shipment = new ShipmentDataObjectReader(shipmentDataObject, logger, factory, null, this, helper)
						.TryGetExistingBusinessObject();
					if (shipment != null)
					{
						if (!shipment.Consols.Any())
						{
							continue;
						}
						else if (shipment.Consols.Count > 1)
						{
							consol = null;
							break;
						}

						var consolToCheck = shipment.Consols.First() as ForwardingConsol;
						if (consolToCheck.JK_CoLoadMasterBill.IsEmpty && consolToCheck.JK_CoLoadBookingReference.IsEmpty
							&& (consol == null || consolToCheck.PK == consol.PK))
						{
							consol = consolToCheck;
						}
						else
						{
							consol = null;
							break;
						}
					}
				}

				if (consol != null)
				{
					return consol;
				}
			}

			return null;
		}

		bool IsUseNonCoLoadToMatchCoLoad { get; set; }

		(int score, ForwardingConsol consol)[] GetMatchingConsols(ConsolFetchRule consolFecthRule, Func<ForwardingConsol, bool> filter)
		{
			if (!consolFecthRule.MatchRules.Any())
			{
				return null;
			}

			var consolsWithSocre = ForwardingConsolScoreMatcher.MatchAndScore(factory.BOFactory, consolFecthRule);
			if (consolsWithSocre == null)
			{
				return null;
			}

			var filteredConsols = consolsWithSocre
			.Where(socredConsol =>
			{
				return filter.Invoke(socredConsol.consol);
			}).ToArray();

			if (!filteredConsols.Any())
			{
				return null;
			}

			return filteredConsols;
		}

		bool Filter(ForwardingConsol consol, bool isAirConsol, bool? isCoLoad)
		{
			if (consol.JK_IsCancelled)
			{
				return false;
			}

			if (isCoLoad.HasValue && consol.IsCoLoad != isCoLoad)
			{
				return false;
			}

			if (isAirConsol && consol.JK_SystemCreateTimeUtc <= ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value))
			{
				return false;
			}

			if (isAirConsol
				&& consol.MAWBAllocation.AllocatedMawb != null
				&& consol.MAWBAllocation.AllocatedMawb.JM_SystemCreateTimeUtc <= ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value))
			{
				return false;
			}

			return true;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(ForwardingConsol targetBO)
		{
			if (dataObject.IsVGM())
			{
				if (targetBO == null)
				{
					return Res.GetString("1a4a07aa-e64a-4a3c-ba37-503ddb038fe5", "XML file contains {0} service code and cannot find a matched consol.", ServiceCodesList.Codes.VerifiedGrossContainerWeight);
				}

				if (dataObject.ContainerCollection == null || dataObject.ContainerCollection.Count == 0)
				{
					return Res.GetString("1d852f90-6254-49a4-953d-8b3cf3d658b1", "XML file does not contain any containers.");
				}

				if (dataObject.ContainerCollection.Any(c => c.ContainerNumber.GetValueOrDefault().IsEmpty))
				{
					return Res.GetString("8abce101-6baa-4683-82fd-6847b6b5cb49", "The container number of one or multiple specified containers is empty.");
				}

				if (dataObject.ContainerCollection.Any(c => (c.IsEmptyContainer ?? ZBool.False) && (c.GrossWeightVerificationType?.Code ?? ZString.Empty) == Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages))
				{
					return CommonContainerValidation.CannotAllowMethod2PackagesForEmptyContainer;
				}

				var reason = GetReasonForNotAllContainersApplicableForVGM(targetBO);
				if (!reason.IsEmpty)
				{
					return reason;
				}
			}

			if (dataObject.HasVGMSection())
			{
				var vgmValidationResult = dataObject.ValidateVGMProperties();
				if(!vgmValidationResult.IsEmpty)
				{
					return vgmValidationResult;
				}
			}

			if (dataObject.IsConsolidationAdviceMessage())
			{
				if (targetBO == null && dataObject.IsAmendment())
				{
					return Res.GetString("37eca6ef-482b-4baf-83a5-5019879eaa57", "Cannot find any consols for Consolidation Advice amendment message.");
				}

				var reason = GetReasonForInvalidShipmentsForConsolidationAdvice(targetBO);
				if (!reason.IsEmpty)
				{
					return reason;
				}
			}

			if (dataObject != null && dataObject.ContainerCollection != null && dataObject.ContainerCollection.Content == CollectionContent.Partial)
			{
				var containerNumberSet = new HashSet<ZString>();
				foreach (var container in dataObject.ContainerCollection)
				{
					if (container.ContainerNumber.HasValue)
					{
						if (containerNumberSet.Contains(container.ContainerNumber.Value))
						{
							return Res.GetString("D020F12D-1BDF-4284-91B8-7F3D01413D8D", "There are two identical container number in this universal shipment.");
						}
						else
						{
							containerNumberSet.Add(container.ContainerNumber.Value);
						}
					}
				}
			}

			if (logger.TopLevelDataObject.IsAirEBookingMessage())
			{
				var submissionVersion = logger.TopLevelDataObject.GetSubmissionVersion() ?? 0;
				var lastImportedSubmissionVersion = targetBO.GetLastImportedAirBookingShipmentSubmissionVersion();

				if (lastImportedSubmissionVersion.HasValue
					&& lastImportedSubmissionVersion.Value >= submissionVersion)
				{
					return Res.GetString("d7cf9bf5-bcf7-44b9-abac-30aa3d14da8b", "This Universal Shipment submission version is: {0}. An Universal Shipment with the same or newer submission versions ({1}) has already been imported.", submissionVersion, lastImportedSubmissionVersion);
				}
			}

			if (CarrierShipperReferenceMessageEventTransformer.IsVersioningEnabledMessage(dataObject)
				&& CarrierShipperReferenceMessageEventTransformer.IsLowerCarrierShipperReferenceNumberReceived(dataObject, targetBO))
			{
				return Res.GetString("6e803fa9-7587-4aec-98ee-54f39727633b", "Received for previous 'reset to original' version. Check with carrier for possible duplication.");
			}

			var reasonWhenCreditorTypeIsNull = GetReasonForNotAbleToStoreContainerOnDBWhenCreditorTypeIsNull();
			if (!reasonWhenCreditorTypeIsNull.IsEmpty)
			{
				return reasonWhenCreditorTypeIsNull;
			}

			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);

			if (result.IsEmpty && targetBO != null)
			{
				if (!targetBO.JK_IsForwarding)
				{
					result = Res.GetString("18fd882e-4374-438a-b079-0b30a6af0354", "Matched consol {0} is not a Forwarding Consol", targetBO.JK_UniqueConsignRef);
				}
			}

			return result;
		}

		ZString GetReasonForNotAbleToStoreContainerOnDBWhenCreditorTypeIsNull()
		{
			var containerNumbers = dataObject.ContainerCollection?
				.Where(container => container.ContainerPenaltyCollection?.Any(cp => cp.CreditorType == null) == true).Select(c => c.ContainerNumber);

			return (containerNumbers?.Count() > 0) ? Res.GetString("D7E3DE67-8E2A-4AD2-A368-9C893368FD05", "Creditor type in container penalty can not be null. Invalid container numbers ({0})", string.Join(", ", containerNumbers)) : string.Empty;
		}

		ZString GetReasonForNotAllContainersApplicableForVGM(ForwardingConsol consol)
		{
			if (dataObject.ContainerCollection != null)
			{
				var dataObjectContainerNumbers = dataObject.ContainerCollection
					.Select(c => c.ContainerNumber.GetValueOrDefault());

				var containers = consol.Containers.Cast<CommonContainer>().ToArray();

				var allContainersFound = dataObjectContainerNumbers
					.All(containerNumber => containers.Any(c => c.JC_ContainerNum == containerNumber));
				if (!allContainersFound)
				{
					return Res.GetString("cf341d6c-3153-4266-933b-121bc7e510f8", "One or multiple specified containers cannot be found in the matched consol.");
				}

				if (containers.Any(c => !VGMHelper.IsContainerModeApplicableForVGM(c.JC_ContainerMode)))
				{
					return Res.GetString("ead81dd2-0f34-40c9-a08b-ca30e19fa632", "Container Mode should be FCL, GRP or BCN.");
				}
			}

			return ZString.Empty;
		}

		ZString GetReasonForInvalidShipmentsForConsolidationAdvice(ForwardingConsol consol)
		{
			if (dataObject.SubShipmentCollection != null)
			{
				foreach (var subShipment in dataObject.SubShipmentCollection)
				{
					var shipmentNo = subShipment.GetShipmentNumber();
					if (!shipmentNo.IsEmpty)
					{
						var shipment = factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentNo));
						if (shipment != null)
						{
							if (consol == null && shipment.Consols.Count > 0)
							{
								return Res.GetString("471638cd-b00b-43b3-bd94-7bd22f8c3f98", "[*Shipment(s) on Consolidation Advice has been already linked to a Consolidation.*]");
							}
							else if (shipment.Consols.Count > 1 || shipment.Consols.Any(c => c.PK != consol.PK))
							{
								return Res.GetString("bf5627ad-d0bd-4fc8-a1e4-a50543f3456d", "[*Shipment(s) on Consolidation Advice has been linked to another Consolidation.*]");
							}
						}
					}
				}
			}

			return ZString.Empty;
		}

		protected override IMatchingBusinessEntityFinder<ForwardingConsol> GetCombinedReferenceMatcher()
		{
			return new ForwardingConsolMatcher(factory.BOFactory, GetCommonConsolReferences(), logger, helper);
		}

		CommonConsolReferences GetCommonConsolReferences()
		{
			var forwardingConsolReference = new CommonConsolReferences();
			forwardingConsolReference.CarriersBookingReference = dataObject.BookingConfirmationReference.GetValueOrDefault();
			forwardingConsolReference.AgentsReference = dataObject.AgentsReference.GetValueOrDefault();
			forwardingConsolReference.PopulateAdditionalReferences(dataObject, onlyWhenCodesMappedToTarget: !dataObject.IsConsolidationAdviceMessage());
			forwardingConsolReference.LoadPort = dataObject.PortOfLoading.GetUNLOCOAsUpperCase(factory.BOFactory);
			forwardingConsolReference.DischargePort = dataObject.PortOfDischarge.GetUNLOCOAsUpperCase(factory.BOFactory);

			forwardingConsolReference.MatchMainCarrierReferencesToCoLoader = dataObject.IsVGM();
			forwardingConsolReference.MBOLNumber = dataObject.WayBillNumber.GetValueOrDefault();
			forwardingConsolReference.CoLoadBookingConfirmationReference = dataObject.CoLoadBookingConfirmationReference.GetValueOrDefault();
			forwardingConsolReference.CoLoadMasterBillNumber = dataObject.CoLoadMasterBillNumber.GetValueOrDefault();
			forwardingConsolReference.SCAC = dataObject.GetSCAC(DocAddressType.ShippingLineAddress);
			forwardingConsolReference.CoLoadSCAC = dataObject.GetSCAC(DocAddressType.CoLoadWith);
			forwardingConsolReference.IsConsolidationAdvice = dataObject.IsConsolidationAdviceMessage();
			forwardingConsolReference.DocumentaryPurpose = dataObject.GetDocumentaryPurpose();

			return forwardingConsolReference;
		}

		protected override bool ModuleHasReferenceAndPartyIDMatchingEnabled
			=> base.ModuleHasReferenceAndPartyIDMatchingEnabled && !dataObject.IsConsolidationAdviceMessage();

		protected override bool IsImportConsolCostsAllowed(ForwardingConsol targetBO) => !dataObject.IsAirEBookingMessage();

		protected override void PopulateBusinessObject(ForwardingConsol consolBO)
		{
			try
			{
				PopulateBusinessObjectCore(consolBO);
			}
			catch (ExportAWBHeaderReplaceMacrosException e)
			{
				throw new DataObjectReadFailureException(e.Message);
			}
			catch (Exception e) when (e.InnerException is ExportAWBHeaderReplaceMacrosException)
			{
				throw new DataObjectReadFailureException(e.InnerException.Message);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const value")]
		void PopulateBusinessObjectCore(ForwardingConsol consolBO)
		{
			var complianceUniversalDataObjectReader = new ComplianceUniversalDataObjectReader(consolBO);
			complianceUniversalDataObjectReader.InitializeComplianceMaterialChangesSnapshotIfNeeded();

			var isVGM = dataObject.IsVGM();

			if (!isVGM)
			{
				((ISupportDataImporting)consolBO).IsImportingData = true;
			}

			var linkManager = new ContainerLinkManager<ForwardingConsol>(consolBO);

			if (isVGM)
			{
				if (dataObject.ContainerCollection != null)
				{
					var consolContainerCollectionReader = new ConsolContainerCollectionReader<ForwardingContainer, ForwardingConsol>(dataObject.ContainerCollection, logger, factory, consolBO, linkManager);

					foreach (var containerDataObject in dataObject.ContainerCollection)
					{
						containerDataObject.UpdateContainerGrossWeightVerificationDateTime();
					}

					consolContainerCollectionReader.ReadIntoCollection();
				}

				return;
			}

			if (dataObject.IsCO2eResponse())
			{
				var previousCO2eValue = (TotalCO2e: consolBO.GetTotalCO2e(),
										Transports: consolBO.Transports ?? Enumerable.Empty<BusinessObject>());
				consolBO.GetResponseImporter(consolBO.Factory, logger)?.ImportGreenHouseGasEmission(dataObject, consolBO, GetBusinessObjectHumanReadableName(consolBO), previousCO2eValue);
				return;
			}

			SetValue(consolBO, JobConsolSchema.JK_RH_NKConsolCommodity, dataObject.ConsolCommodity?.Code);
			SetValue(consolBO, JobConsolSchema.JK_AgentsReference, dataObject.AgentsReference);
			SetValue(consolBO, JobConsolSchema.JK_AgentType, dataObject.ShipmentType);
			PopulateForwarderHandlingType(consolBO, JobConsolSchema.JK_SendingForwarderHandlingType, dataObject.SendingForwarderHandlingType);
			PopulateForwarderHandlingType(consolBO, JobConsolSchema.JK_ReceivingForwarderHandlingType, dataObject.ReceivingForwarderHandlingType);
			SetValue(consolBO, JobConsolSchema.JK_RS_NKGatewayServiceLevel, dataObject.GatewayServiceLevel);
			SetValue(consolBO, JobConsolSchema.JK_AWBServiceLevel, dataObject.AWBServiceLevel);
			if (IsUseNonCoLoadToMatchCoLoad)
			{
				SetValue(consolBO, JobConsolSchema.JK_CoLoadMasterBill, dataObject.WayBillNumber);
				SetValue(consolBO, JobConsolSchema.JK_CoLoadBookingReference, dataObject.BookingConfirmationReference);
			}
			else
			{
				SetValue(consolBO, JobConsolSchema.JK_BookingReference, dataObject.BookingConfirmationReference);
			}
			SetValue(consolBO, JobConsolSchema.JK_PrepaidCollect, dataObject.PaymentMethod);
			SetValue(consolBO, JobConsolSchema.JK_ReleaseType, dataObject.ReleaseType);
			SetValue(consolBO, JobConsolSchema.JK_TransportMode, dataObject.TransportMode);
			SetValue(consolBO, JobConsolSchema.JK_RL_NKDischargePort, dataObject.PortOfDischarge);
			SetValue(consolBO, JobConsolSchema.JK_RL_NKLoadPort, dataObject.PortOfLoading);
			SetValue(consolBO, JobConsolSchema.JK_RL_NKMasterBillIssuePlace, dataObject.PlaceOfIssue);
			SetValue(consolBO, JobConsolSchema.JK_RL_NKPortOfFirstArrival, dataObject.PortOfFirstArrival);
			SetValue(consolBO, JobConsolSchema.JK_RL_NKFirstForeignPort, dataObject.PortFirstForeign);
			SetValue(consolBO, JobConsolSchema.JK_RL_NKLastForeignPort, dataObject.PortLastForeign);
			SetValue(consolBO, JobConsolSchema.JK_ConsolMode, dataObject.ContainerMode);
			SetValue(consolBO, JobConsolSchema.JK_NoCopyBills, dataObject.NoCopyBills);
			SetValue(consolBO, JobConsolSchema.JK_NoOriginalBills, dataObject.NoOriginalBills);
			SetValue(consolBO, JobConsolSchema.JK_ElectronicBillOfLadingReference, dataObject.ElectronicBillOfLadingReference);
			if (dataObject.TotalPreallocatedWeightUnit != null && dataObject.TotalPreallocatedWeightUnit.Code.HasValue)
			{
				consolBO.WeightVerificationUnit = dataObject.TotalPreallocatedWeightUnit.Code.Value;
			}
			SetValue(consolBO, JobConsolSchema.JK_TotalShipmentActWeightCheck, dataObject.TotalPreallocatedWeight);
			if (dataObject.TotalPreallocatedVolumeUnit != null && dataObject.TotalPreallocatedVolumeUnit.Code.HasValue)
			{
				consolBO.VolumeVerificationUnit = dataObject.TotalPreallocatedVolumeUnit.Code.Value;
			}
			SetValue(consolBO, JobConsolSchema.JK_TotalShipmentActVolumeCheck, dataObject.TotalPreallocatedVolume);
			SetValue(consolBO, JobConsolSchema.JK_TotalShipmentChargableCheck, dataObject.TotalPreallocatedChargeable);

			PopulateOverrideQuantities(consolBO);

			SetValue(consolBO, JobConsolSchema.JK_ConsolChargeableRate, dataObject.ChargeableRate);

			ZBool requiresTemperatureControl = dataObject.RequiresTemperatureControl.GetValueOrDefault();
			SetValue(consolBO, JobConsolSchema.JK_RequiresTemperatureControl, dataObject.RequiresTemperatureControl);
			if (requiresTemperatureControl)
			{
				SetValue(consolBO, JobConsolSchema.JK_RequiredTemperatureMinimum, dataObject.RequiredTemperatureMinimum);
				SetValue(consolBO, JobConsolSchema.JK_RequiredTemperatureMaximum, dataObject.RequiredTemperatureMaximum);
				SetValue(consolBO, JobConsolSchema.JK_RequiredTemperatureUnit, dataObject.RequiredTemperatureUnit?.Code);

				var result = TemperatureHelper.CheckRequiredTemperatures(consolBO);
				if (!result.IsEmpty)
				{
					throw new DataObjectReadFailureException(result);
				}
			}

			if (!IsUseNonCoLoadToMatchCoLoad && consolBO.IsCoLoad)
			{
				SetValue(consolBO, JobConsolSchema.JK_CoLoadMasterBill, dataObject.CoLoadMasterBillNumber);
				SetValue(consolBO, JobConsolSchema.JK_CoLoadBookingReference, dataObject.CoLoadBookingConfirmationReference);
			}

			if (!TrySetMaximumAllowablePackage(consolBO, out var error))
			{
				throw new DataObjectReadFailureException(error);
			}

			PopulateDangerousGoodsFields(consolBO);

			PopulateCustomFields(consolBO);

			PopulateDateDataFields(consolBO);

			if (dataObject.NoteCollection != null)
			{
				new ForwardingConsolNotesCollectionReader(dataObject.NoteCollection, logger, factory, consolBO).ReadIntoCollection();
			}

			AttachMatchedBookingIfPossible(consolBO);

			if (dataObject.ContainerCollection != null)
			{
				var consolContainerCollectionReader = new ConsolContainerCollectionReader<ForwardingContainer, ForwardingConsol>(dataObject.ContainerCollection, logger, factory, consolBO, linkManager);

				consolContainerCollectionReader.ReadIntoCollection();
			}

			if (dataObject.TransportLegCollection != null && !dataObject.IsTransitDataSource())
			{
				var reader = new TransportLegCollectionReader<Transport>(dataObject.TransportLegCollection, logger, factory, consolBO);
				reader.ReadIntoCollection();

				consolBO.Transports.TryToSetTransportType();
			}

			if (dataObject.AdditionalAddressInfoCollection != null)
			{
				foreach (var additionalAddressInfo in dataObject.AdditionalAddressInfoCollection)
				{
					var reader = new JobAddressAdditionalInfoDataObjectReader(additionalAddressInfo, logger, factory, consolBO);
					reader.ReadIntoBusinessObject();
				}
			}

			if (dataObject.SubShipmentCollection != null)
			{
				UnlinkShipmentsAndLog(consolBO);

				foreach (var shipmentDataObject in dataObject.SubShipmentCollection)
				{
					var xmlSessionTracker = logger as IXmlSessionTracker;
					if (xmlSessionTracker != null)
					{
						xmlSessionTracker.StartProcessingASubShipment();
					}

					new ShipmentDataObjectReader(shipmentDataObject, logger, factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO), linkManager, this, helper).ReadIntoBusinessObject();
					if (xmlSessionTracker != null)
					{
						xmlSessionTracker.EndProcessingASubShipment();
					}
				}
			}

			CommonUniversalFreightHelper.RemoveDuplicateTransportsOnShipments(consolBO);

			SetOrgAddresses(consolBO);
			if (!IsUseNonCoLoadToMatchCoLoad)
			{
				PopulateMAWB(consolBO);
			}

			if (dataObject.EntryNumberCollection != null)
			{
				foreach (var entryNumberDataObject in dataObject.EntryNumberCollection)
				{
					var cusEntryNumCollection = consolBO.CusEntryNumsForAllCountries;
					cusEntryNumCollection.Add(new EntryNumberDataObjectReader(entryNumberDataObject, logger, factory, cusEntryNumCollection, consolBO).ReadIntoBusinessObject());
				}
			}

			if (dataObject.AdditionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new ForwardingConsolAdditionalReferenceCollectionReader(dataObject.AdditionalReferenceCollection, logger, factory, consolBO);
				additionalReferenceCollectionReader.ReadIntoCollection();
			}

			SetValue(consolBO, JobConsolSchema.JK_CarrierContractNumber, dataObject.CarrierContractNumber);

			PopulateSpecialHandlingIfNeeded(consolBO);

			if (consolBO.ReferenceNumberShouldBeSplitIntoNumbers(consolBO.JK_BookingReference))
			{
				consolBO.SplitBookingReferenceNumbersIntoAdditionalReferenceCollection();
			}

			ImportAWBHeaderIfNeeded(consolBO);

			if (dataObject.IsAirEBookingMessage())
			{
				var reader = ObjectFactory.Get<IAirBookingResponseConsolCostingDataObjectReader>();
				reader.Import(logger, dataObject.ConsolCosts, consolBO);
			}

			if (dataObject.IsConsolidationAdviceMessage())
			{
				const string eventRef = "from Carrier";
				string eventSubTypeORG = dataObject.IsOriginal() ? (EZC.NoResString)"Original" : (EZC.NoResString)"Amendment";

				consolBO.Logs.CreateOrRecreateEventLog(Events.MessageReceived
					, EstimateActual.Actual
					, ZDateTimeOffset.UtcNow
					, eventRef
					, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, UniversalShipmentExtension.DocumentNameConsolidationAdvice)
					, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageSubType, eventSubTypeORG));
			}

			var isSeaConsol = dataObject.TransportMode.GetCodeAsUpperCase() == Constants.TransportModes.Sea;
			if (isSeaConsol)
			{
				SetValue(consolBO, JobConsolSchema.JK_RL_NKCarrierBookingOffice, dataObject.CarrierBookingOffice);
			}

			if (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.EnableEBLIntegration)
			{
				if ((dataObject.BillType?.Code.HasValue ?? false) && !dataObject.BillType.Code.Value.IsEmpty)
				{
					if (FreightCodePairLists.BillOfLadingBillTypeList().ContainsCode(dataObject.BillType.Code))
					{
						SetValue(consolBO, JobConsolSchema.JK_ElectronicBillOfLadingType, dataObject.BillType.Code);
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("3b7bc746-7a05-40d3-ad99-e7f00c8e70a8", "The Bill Type is not updated because the input value is not a valid code '{0}'.", dataObject.BillType.Code));
					}
				}

				if ((dataObject.BillTerms?.Code.HasValue ?? false) && !dataObject.BillTerms.Code.Value.IsEmpty)
				{
					if (FreightCodePairLists.BillOfLadingBillTermsList().ContainsCode(dataObject.BillTerms.Code))
					{
						SetValue(consolBO, JobConsolSchema.JK_ElectronicBillOfLadingTerms, dataObject.BillTerms.Code);
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("adaf2450-5ee5-4195-8502-2e3c79def12d", "The Bill Terms is not updated because the input value is not a valid code '{0}'.", dataObject.BillTerms.Code));
					}
				}

				PopulateOriginalBillNotes(consolBO);
			}

			complianceUniversalDataObjectReader.SynchronizeComplianceRiskStatusIfNeeded();
		}

		#region PopulateOriginalBillNotes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Dictionary values")]
		static Dictionary<string, string> eBLAddressTypesAndDescriptions
		{
			get
			{
				return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
						{ "Sender", "Sender" },
						{ "Receiver", "Receiver" },
						{ "Originator", "Publisher" },
						{ "Holder", "Holder" },
						{ "FirstHolder", "First Holder" },
						{ "Shipper", "Shipper" },
						{ "Consignee", "Consignee" },
						{ "ToOrder", "To Order" },
						{ "PledgeeHolder", "Pledgee" },
						{ "SurrenderParty", "Surrender Agent" }
				};
			}
		}

		void PopulateOriginalBillNotes(ForwardingConsol consol)
		{
			var note = new OriginalBillNotesService(factory.BOFactory).LoadOrCreateStmNoteForReaderUpdate(consol.PK, JobConsolSchema.Constants.TableName, consol.IsInDatabase, PredefinedNoteTypes.Instance.OriginalBillNotes.ToString());
			var newNoteString = AddOrganizationAddressDetailsToOriginalBillNotes();
			var latestNoteText = GetNoteText(note.ST_NoteText, newNoteString, consol);
	
			var noteRow = GetColumnIndexer(note);
			SetValue(noteRow, StmNoteSchema.ST_NoteText, latestNoteText);
		}

		string GetNoteText(string originalNoteText, string newNoteText, ForwardingConsol consol)
		{
			var wayBillNumber = dataObject.WayBillNumber.GetValueOrDefault();
			var electronicBillOfLadingReference = dataObject.ElectronicBillOfLadingReference.GetValueOrDefault();
			var timeText = ZDateTimeOffset.UtcNow.ToString()
				+ (wayBillNumber.IsEmpty ? string.Empty : $" {wayBillNumber}")
				+ (consol.JK_Calc_BillOfLadingBillStatusDescription.IsEmpty ? string.Empty : $" {consol.JK_Calc_BillOfLadingBillStatusDescription}")
				+ (electronicBillOfLadingReference.IsEmpty ? string.Empty : $" [Reference:{electronicBillOfLadingReference}]");

			return (timeText + System.Environment.NewLine + newNoteText)
				+ (string.IsNullOrEmpty(originalNoteText) ? string.Empty : (System.Environment.NewLine + System.Environment.NewLine + originalNoteText));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Key string")]
		string AddOrganizationAddressDetailsToOriginalBillNotes()
		{
			var newNoteStringBuilder = new ZStringBuilder();
			foreach (var valuePair in eBLAddressTypesAndDescriptions.Where(e => !string.Equals(e.Key, "Sender", StringComparison.OrdinalIgnoreCase) && !string.Equals(e.Key, "Receiver", StringComparison.OrdinalIgnoreCase)))
			{
				var organizationAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(x => string.Equals(valuePair.Key, x.AddressType.GetValueOrDefault(), StringComparison.OrdinalIgnoreCase));
				var organizationAddressDetails = ZString.Empty;

				if (organizationAddress != null)
				{
					if (!organizationAddress.CompanyName.GetValueOrDefault().IsEmpty && organizationAddress.HasAddressDetails())
					{
						organizationAddressDetails = ConcatOrganizationAddressDetails(organizationAddress.CompanyName.GetValueOrDefault(), organizationAddress.Address1.GetValueOrDefault(), organizationAddress.Address2.GetValueOrDefault()
							, organizationAddress.City.GetValueOrDefault(), organizationAddress.State?.Code.GetValueOrDefault() ?? ZString.Empty, organizationAddress.Postcode.GetValueOrDefault()
							, organizationAddress.Country?.Name.GetValueOrDefault() ?? ZString.Empty);
					}
					else
					{
						var triValue = organizationAddress.RegistrationNumberCollection?.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == OrgCusCode.CodeTypes.BoleroTitleRegisterID)?.Value;

						if (!string.IsNullOrEmpty(triValue))
						{
							var query = new ZDBOnlyQuery(typeof(OrgHeader));

							var cusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
							cusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.BoleroTitleRegisterID);
							cusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, triValue);
							query.AddSubQuery(cusCodeSubQuery, JoinCondition.And);

							var orgHeader = factory.LoadTop1<OrgHeader>(query);

							if (orgHeader != null)
							{
								var triCustomsCode = orgHeader.CustomsCodes.Cast<OrgCusCode>()
									.First(x => x.OK_CodeType == OrgCusCode.CodeTypes.BoleroTitleRegisterID && x.OK_CustomsRegNo.EqualsIgnoringCase(triValue.Value));
								var premisesAddress = triCustomsCode.PremisesAddress ?? orgHeader.MainAddress;

								organizationAddressDetails = ConcatOrganizationAddressDetails(premisesAddress.CompanyName, premisesAddress.Address1, premisesAddress.Address2
									, premisesAddress.City, premisesAddress.State, premisesAddress.Postcode
									, premisesAddress.Country?.RN_Desc ?? ZString.Empty);
							}
						}
					}
				}

				if (!organizationAddressDetails.IsEmpty)
				{
					newNoteStringBuilder.AppendLine(valuePair.Value + " : " + organizationAddressDetails);
				}
			}

			return newNoteStringBuilder.ToString().Trim('\r', '\n');
		}

		ZString ConcatOrganizationAddressDetails(ZString companyName, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString country)
		{
			var stringBuilder = new ZStringBuilder();

			stringBuilder.AppendIfNotEmpty(companyName);
			stringBuilder.AppendIfNotEmpty(address1);
			stringBuilder.AppendIfNotEmpty(address2);
			stringBuilder.AppendIfNotEmpty(ZString.Join(" ", new[] { city, state, postCode }));
			stringBuilder.AppendIfNotEmpty(country);

			return stringBuilder.ToStringWithDelimiterBetweenAppends(", ");
		}

		#endregion

		void PopulateForwarderHandlingType(ForwardingConsol consolBO, SchemaColumn schemaColumn, CodeDescriptionPair forwarderHandlingType)
		{
			if (!forwarderHandlingType.TryGetCodeAsUpperCase(out var code))
			{
				return;
			}

			if (string.IsNullOrEmpty(code))
			{
				return;
			}

			if (!consolBO.GatewayHandlingTypeList.ContainsCode(code))
			{
				logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("0225f78e-3fbd-486a-9529-7e8fe1f44fc2", "{0} {1} is invalid.", schemaColumn.Name, code));
				return;
			}

			SetValue(consolBO, schemaColumn, code);
		}

		IEnumerable<ForwardingShipment> UnlinkShipmentsAndLog(ForwardingConsol consolBO)
		{
			var shipmentsToUnlink = new List<ForwardingShipment>();
			if (dataObject.IsConsolidationAdviceMessage())
			{
				shipmentsToUnlink = consolBO.Shipments.Cast<ForwardingShipment>().Where(
						shipment => !dataObject.SubShipmentCollection.Any(
							subshipment => subshipment.GetShipmentNumber() == shipment.JS_UniqueConsignRef)).ToList();
			}

			consolBO.Shipments.RemoveRange(shipmentsToUnlink);
			shipmentsToUnlink.ForEach(shipment => logger.Log(Enterprise.Integration.LogType.Information,
					Res.GetString("f6274af5-e5e8-4d4d-aea2-13d7e4d9b6d5",
					"Shipment {0} has been unlinked from consol {1} for it no longer exists in the Consolidation Advice message.", shipment.JS_UniqueConsignRef, consolBO.JK_UniqueConsignRef)));

			return shipmentsToUnlink;
		}

		#region PopulateOverrideQuantities

		void PopulateOverrideQuantities(ForwardingConsol consolBO)
		{
			var areAllOverridedQuantitiesPresent = AreAllOverridedQuantitiesPresent();

			if (!areAllOverridedQuantitiesPresent && AreSomeOverridedQuantitiesPresent())
			{
				logger.Log(Enterprise.Integration.LogType.Warning,
					Res.GetString("890b3e5f-6052-450f-bf4c-82e6afddd1d5",
					"Some of the carrier corrected details were missing and have been ignored."));
			}

			if (areAllOverridedQuantitiesPresent)
			{
				SetValue(consolBO, JobConsolSchema.JK_OverrideConsolChargeable, true);
				SetValue(consolBO, JobConsolSchema.JK_CorrectedConsolWeight, dataObject.CarrierCorrectedWeight);
				SetValue(consolBO, JobConsolSchema.JK_CorrectedConsolWeightUnit, dataObject.CarrierCorrectedWeightUnit.Code.Value);
				SetValue(consolBO, JobConsolSchema.JK_CorrectedConsolVolume, dataObject.CarrierCorrectedVolume);
				SetValue(consolBO, JobConsolSchema.JK_CorrectedConsolVolumeUnit, dataObject.CarrierCorrectedVolumeUnit.Code.Value);
				SetValue(consolBO, JobConsolSchema.JK_ConsolChargeable, dataObject.CarrierCorrectedChargeable.Value);
			}
		}

		bool AreSomeOverridedQuantitiesPresent()
		{
			return dataObject.CarrierCorrectedWeight.HasValue
				|| dataObject.CarrierCorrectedVolume.HasValue
				|| dataObject.CarrierCorrectedChargeable.HasValue
				|| (dataObject.CarrierCorrectedVolumeUnit?.Code.HasValue ?? false)
				|| (dataObject.CarrierCorrectedWeightUnit?.Code.HasValue ?? false);
		}

		bool AreAllOverridedQuantitiesPresent()
		{
			return dataObject.CarrierCorrectedWeight.HasValue
				&& dataObject.CarrierCorrectedVolume.HasValue
				&& dataObject.CarrierCorrectedChargeable.HasValue
				&& (dataObject.CarrierCorrectedVolumeUnit?.Code.HasValue ?? false)
				&& (dataObject.CarrierCorrectedWeightUnit?.Code.HasValue ?? false);
		}

		#endregion

		void PopulateDateDataFields(ForwardingConsol consolBO)
		{
			if (dataObject.DateCollection != null)
			{
				foreach (var dateDataObject in dataObject.DateCollection)
				{
					switch (dateDataObject.Type)
					{
						case DateType.ShippedOnBoard:
							SetValue(consolBO, JobConsolSchema.JK_ShippedOnBoardDate, dateDataObject.Value);
							break;
						case DateType.BillIssued:
							SetValue(consolBO, JobConsolSchema.JK_MasterBillIssueDate, dateDataObject.Value);
							break;
						case DateType.FirstArrivalInCountry:
							SetValue(consolBO, JobConsolSchema.JK_DatePortOfFirstArrival, dateDataObject.Value);
							break;
						case DateType.FirstForeignArrival:
							SetValue(consolBO, JobConsolSchema.JK_DateFirstForeignPort, dateDataObject.Value);
							break;
						case DateType.LastForeignDeparture:
							SetValue(consolBO, JobConsolSchema.JK_DateLastForeignPort, dateDataObject.Value);
							break;
						case DateType.CutOffDate:
							SetValue(consolBO, JobConsolSchema.JK_ConsolCutOffDate, dateDataObject.Value);
							break;
						case DateType.DepartureReceiptRequested:
							SetValue(consolBO, JobConsolSchema.JK_PackDepotReceiptRequested, dateDataObject.Value);
							break;
						case DateType.ArrivalReceiptRequested:
							SetValue(consolBO, JobConsolSchema.JK_UnpackDepotReceiptRequested, dateDataObject.Value);
							break;
						case DateType.DepartureDispatchRequested:
							SetValue(consolBO, JobConsolSchema.JK_PackDepotDispatchRequested, dateDataObject.Value);
							break;
						case DateType.ArrivalDispatchRequested:
							SetValue(consolBO, JobConsolSchema.JK_UnpackDepotDispatchRequested, dateDataObject.Value);
							break;
					}
				}
			}
		}

		void PopulateDangerousGoodsFields(ForwardingConsol consolBO)
		{
			SetValue(consolBO, JobConsolSchema.JK_IsHazardous, dataObject.IsHazardous);
			if (dataObject.PreallocatedUNDGCollection != null)
			{
				new ConsolRestrictionUNDGDataObjectCollectionReader(dataObject.PreallocatedUNDGCollection, logger, factory, consolBO.ConsolDGRestrictionCollection).ReadIntoCollection();
			}
		}

		void AttachMatchedBookingIfPossible(ForwardingConsol consolBO)
		{
			if (dataObject.SubShipmentCollection != null && dataObject.SubShipmentCollection.Count == 1)
			{
				var dataTarget = dataObject.SubShipmentCollection.First().GetMatchingDataTarget(DataContextType.ForwardingShipment);

				if (dataTarget != null && dataTarget.Key.HasValue && !dataTarget.Key.Value.IsEmpty)
				{
					var query = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, dataTarget.Key);
					query.AddToFilter(JobShipmentSchema.JS_IsBooking, ZBool.True);
					query.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, ZBool.False);

					var booking = factory.Load<ForwardingShipment>(query).FirstOrDefault();

					if (booking != null)
					{
						var consolHelper = new BuildConsolHelper();
						consolHelper.AddBookingsToConsol(consolBO, new[] { booking.PK }, false);
					}
				}
			}
		}

		#region PopulateMAWB

		void PopulateMAWB(ForwardingConsol consolBO)
		{
			if (dataObject.WayBillNumber.HasValue)
			{
				if (!dataObject.WayBillNumber.Value.IsEmpty)
				{
					if (consolBO.IsAir)
					{
						ProcessAirMAWBNumber(consolBO);
					}
					else
					{
						SetValue(consolBO, JobConsolSchema.JK_MasterBillNum, dataObject.WayBillNumber.Value);
					}
				}
				else
				{
					ProcessEmptyMAWBNumber(consolBO);
				}

				consolBO.SuppressAutoGenerateMasterBillNumber = true;
			}
		}

		void ProcessAirMAWBNumber(ForwardingConsol consolBO)
		{
			var waybillWithPrefix = dataObject.WayBillNumber.Value.Replace(" ", "").Replace("-", "");

			int mawbMaxLength = JobMawb.Schema.JM_Airline3DigitPrefixMaxLength + JobMawb.Schema.JM_MAWBMaxLength;
			if (waybillWithPrefix.Length > mawbMaxLength)
			{
				string errorMessage = Res.GetString("dd9444c4-788a-49ce-82b1-b50c14876ac7", "Attempted to import an Air consol with an invalid MAWB number: '{0}'. Maximum of {1} digits are allowed.", waybillWithPrefix, mawbMaxLength);
				throw new DataObjectReadFailureException(errorMessage);
			}

			const int waybillPrefixLen = 3;
			var airlinePrefix = waybillWithPrefix.SubstringSafe(0, waybillPrefixLen);
			var waybill = waybillWithPrefix.SubstringSafe(waybillPrefixLen);

			if ((waybillWithPrefix.Length == mawbMaxLength || waybillWithPrefix.Length == waybillPrefixLen)
				&& ShouldCreateOrAllocateNeutralStock)
			{
				if (consolBO.IsNeutralMAWBPrinted)
				{
					string errorMessage = Res.GetString("dba2f041-55e4-4aca-84cb-4d47955ca85f", "Attempted to import a neutral master consolidation with a new MAWB number {0}. Cannot create/allocate another MAWB to this consolidation, as the Final Master has already been printed.", dataObject.WayBillNumber.Value);
					throw new DataObjectReadFailureException(errorMessage);
				}

				if (waybillWithPrefix.Length == waybillPrefixLen)
				{
					var matchingUnusedMAWB = GetMatchingUnusedMAWB(airlinePrefix, waybill, consolBO.JK_AWBServiceLevel);

					if (matchingUnusedMAWB != null && FreightDataRegistry.Instance.AllocateMAWBNumberFromMAWBStockUponXMLImport.Value)
					{
						consolBO.MAWBAllocation.MarkForReallocation(checkIsNeutralAndNotPrinted: true, checkIsInDatabase: true);
						SetIsNeutralMasterAndMasterBillNumber(consolBO, true, airlinePrefix);
					}
					else
					{
						if (matchingUnusedMAWB == null)
						{
							logger.Log(Enterprise.Integration.LogType.Warning, FreightJobMawbLink.NoMasterBillNumbersInStockMessage);
						}

						if (!FreightDataRegistry.Instance.AllocateMAWBNumberFromMAWBStockUponXMLImport.Value)
						{
							logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("5b544f83-e4fa-48ea-a531-303c8de22fee", "MAWB Number will not be allocated from Stock due to the Registry setting found under Freight > MAWB > Allocate MAWB number from MAWB Stock upon XML import is currently set to No, and the MAWB number in this XML is incomplete."));
						}

						SetIsNeutralMasterAndMasterBillNumber(consolBO, false, airlinePrefix);
					}
				}
				else if (!consolBO.JK_IsNeutralMaster || consolBO.JK_MasterBillNum != waybillWithPrefix)
				{
					var matchingUnusedMAWB = GetMatchingUnusedMAWB(airlinePrefix, waybill, consolBO.JK_AWBServiceLevel);

					if (matchingUnusedMAWB != null)
					{
						consolBO.JK_IsNeutralMaster = true;
						consolBO.MAWBAllocation.MarkForReallocation(matchingUnusedMAWB.PK);
					}
					else if (!consolBO.IsValidForNeutralMaster)
					{
						logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("3eca253a-d6e5-43f0-87ad-64792b55b39a", "MAWB number will not be allocated from Stock because the consol is not valid for Neutral Master."));
						SetIsNeutralMasterAndMasterBillNumber(consolBO, false, airlinePrefix);
					}
					else if (AllowedToCreateMAWBStock(airlinePrefix, waybill))
					{
						var newMAWB = CreateMAWBStock(airlinePrefix, waybill, consolBO.JK_AWBServiceLevel, consolBO.PK, consolBO.Prefix);

						if (newMAWB != null)
						{
							consolBO.MAWBAllocation.MarkForDeallocation(checkMAWBInDatabase: true);
							consolBO.JK_IsNeutralMaster = true;
							consolBO.MAWBAllocation.SetAllocatedMAWBToParent(newMAWB);
						}
						else
						{
							string errorMessage = Res.GetString("ca0421fb-2173-4fbe-950d-42a5f526d8da", "Attempted to import a neutral master consolidation with MAWB number {0} but failed to create a matching MAWB stock.", dataObject.WayBillNumber.Value);
							throw new DataObjectReadFailureException(errorMessage);
						}
					}
					else
					{
						string errorMessage = Res.GetString("b70ad90c-162c-4a4b-bf03-8d15e51c97b7", "Attempted to import a consolidation with MAWB number {0} that is already used in existing MAWB stock but does not match all the criteria. Cannot create a MAWB stock with the same MAWB number.", dataObject.WayBillNumber.Value);
						throw new DataObjectReadFailureException(errorMessage);
					}
				}
			}
			else if (waybillWithPrefix.Length == mawbMaxLength && consolBO.JK_MasterBillNum != waybillWithPrefix)
			{
				SetIsNeutralMasterAndMasterBillNumber(consolBO, false, waybillWithPrefix);
			}
			else if (waybillWithPrefix.Length >= waybillPrefixLen && waybillWithPrefix.Length < mawbMaxLength)
			{
				if (ShouldShowIsNeutralMasterWarning)
				{
					logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("d0431611-8a21-4239-8e9f-e81121706340", "MAWB Number will not be allocated from Stock due to the XML having the {0} element set to false.", "IsNeutralMaster"));
				}

				if (ShouldShowCreateAndAllocateNeutralStockWarning)
				{
					logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("556ec879-c032-432f-87ce-5f22927106bd", "MAWB Number will not be allocated from Stock due to the XML having a {0} value set to false.", "CreateAndAllocateNeutralStock"));
				}

				SetIsNeutralMasterAndMasterBillNumber(consolBO, false, airlinePrefix);
			}

			if (consolBO.JK_OA_ShippingLineAddress.IsEmpty || consolBO.JK_OA_ShippingLineAddress.Equals(Guid.Empty))
			{
				consolBO.UpdateCarrierByMAWB();
			}
		}

		void SetIsNeutralMasterAndMasterBillNumber(ForwardingConsol consolBO, bool isNeutralMaster, ZString masterBillNumber)
		{
			SetValue(consolBO, JobConsolSchema.JK_IsNeutralMaster, isNeutralMaster);
			SetValue(consolBO, JobConsolSchema.JK_MasterBillNum, masterBillNumber);
		}

		bool ShouldCreateOrAllocateNeutralStock =>
			dataObject.IsNeutralMaster != null
				&& dataObject.IsNeutralMaster.Value.HasValue
				&& dataObject.IsNeutralMaster.Value.Value
				&& dataObject.IsNeutralMaster.CreateAndAllocateNeutralStock.HasValue
				&& dataObject.IsNeutralMaster.CreateAndAllocateNeutralStock.Value;

		bool ShouldShowIsNeutralMasterWarning =>
			dataObject.IsNeutralMaster != null
				&& dataObject.IsNeutralMaster.Value.HasValue
				&& !dataObject.IsNeutralMaster.Value.Value;

		bool ShouldShowCreateAndAllocateNeutralStockWarning =>
			dataObject.IsNeutralMaster != null
				&& dataObject.IsNeutralMaster.Value.HasValue
				&& dataObject.IsNeutralMaster.Value.Value
				&& dataObject.IsNeutralMaster.CreateAndAllocateNeutralStock.HasValue
				&& !dataObject.IsNeutralMaster.CreateAndAllocateNeutralStock.Value;

		void ProcessEmptyMAWBNumber(ForwardingConsol consolBO)
		{
			if (!consolBO.JK_MasterBillNum.IsEmpty)
			{
				if (consolBO.IsAir)
				{
					if (ShouldCreateOrAllocateNeutralStock)
					{
						string errorMessage = Res.GetString("59af6427-fdd4-4064-ac1f-7b42f9475469", "Attempted to import a neutral master consolidation without MAWB number.");
						throw new DataObjectReadFailureException(errorMessage);
					}
					else
					{
						SetIsNeutralMasterAndMasterBillNumber(consolBO, false, ZString.Empty);
					}
				}
				else
				{
					SetValue(consolBO, JobConsolSchema.JK_MasterBillNum, ZString.Empty);
				}
			}
		}

		JobMawb CreateMAWBStock(ZString airlinePrefix, ZString waybill, ZString serviceLevel, ZGuid parentID, ZString parentPrefix)
		{
			var newMAWB = factory.BOFactory.New<JobMawb>();
			newMAWB.JM_Airline3DigitPrefix = airlinePrefix;
			newMAWB.JM_MAWB = waybill;
			newMAWB.JM_ServiceLevel = (serviceLevel.IsEmpty) ? new ZString(OrgCarrierServiceLevel.AllCode) : serviceLevel;
			newMAWB.JM_GC_Company = GlbCompany.CurrentCompany.PK;
			newMAWB.JM_GB = GlbBranch.CurrentBranch.PK;
			newMAWB.JM_ParentID = parentID;
			newMAWB.JM_ParentTableCode = parentPrefix;

			return newMAWB;
		}

		ZQuery GetMatchingMAWBQuery(ZString airlinePrefix, ZString waybill, ZString serviceLevel)
		{
			var query = new ZQuery(JobMawbSchema.JM_Airline3DigitPrefix, airlinePrefix);

			if (!waybill.IsEmpty)
			{
				query.AddToFilter(JobMawbSchema.JM_MAWB, waybill);
			}

			query.AddToFilter(JobMawbSchema.JM_IsPrinted, false);
			query.AddToFilter(JobMawbSchema.JM_IsPaper, false);
			query.AddToFilter(JobMawbSchema.JM_ServiceLevel, SQLComparisonOperator.Equal, serviceLevel);
			query.AddToFilter(JobMawbSchema.JM_OH_AllocatedTo, null);
			query.AddToFilter(JobMawbSchema.JM_ParentID, null);
			query.AddToFilter(JobMawbSchema.JM_ParentTableCode, ZString.Empty);
			query.AddToFilter(freightJobMawbLink.GetCompanyAndBranchFilter(airlinePrefix));

			return query;
		}

		JobMawb GetMatchingUnusedMAWB(ZString airlinePrefix, ZString waybill, ZString serviceLevel)
			=> factory.BOFactory.Load<JobMawb>(GetMatchingMAWBQuery(airlinePrefix, waybill, serviceLevel)).FirstOrDefault()
				?? factory.BOFactory.Load<JobMawb>(GetMatchingMAWBQuery(airlinePrefix, waybill, OrgCarrierServiceLevel.AllCode)).FirstOrDefault();

		bool AllowedToCreateMAWBStock(ZString airlinePrefix, ZString waybill)
		{
			var existingMAWBWithSameNumber = freightJobMawbLink.LoadByAirlineAndMawbNo(airlinePrefix, waybill, ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value), false);
			return existingMAWBWithSameNumber == null;
		}

		#endregion

		void ImportAWBHeaderIfNeeded(ForwardingConsol consolBO)
		{
			if (dataObject.CarrierDocumentsOverride != null && dataObject.CarrierDocumentsOverride.AWBHeader != null)
			{
				new AWBHeaderDataObjectReader(dataObject.CarrierDocumentsOverride.AWBHeader, logger, factory, consolBO).ReadIntoBusinessObject();
			}
		}

		void PopulateSpecialHandlingIfNeeded(ForwardingConsol consolBO)
		{
			if (dataObject.SpecialHandlingCollection != null)
			{
				new SpecialHandlingCollectionDataObjectReader(dataObject.SpecialHandlingCollection.ToArray(), factory, consolBO).ReadIntoCollection();
			}
		}

		#region PopulateCustomFields

		void PopulateCustomFields(ForwardingConsol consolBO)
		{
			var usedCustomField = new CustomLabelsCustomizedFieldDataObjectReader(logger).PopulateCustomFields(JobConsolSchema.Instance, consolBO, dataObject, new ForwardingConsol.CustomLabelsProvider(consolBO));
			PopulateWorkflowCustomFields(consolBO, dataObject, usedCustomField);
		}

		#endregion

		protected override bool ShouldUpdateBO
		{
			get
			{
				return eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.Value;
			}
		}

		void SetOrgAddresses(ForwardingConsol consolBO)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				OrganizationAddress creditorAddress = null;

				foreach (var orgAddressDataObject in dataObject.OrganizationAddressCollection.Where(o => !eBLAddressTypesAndDescriptions.ContainsKey(o.AddressType.GetValueOrDefault())))
				{
					if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.ArrivalCTOAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_ArrivalCTOAddress, null, orgAddressDataObject);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.ArrivalCFSLocalTransportAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_ArrivalUnpackCFSTransportAddress, null, orgAddressDataObject);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.ContainerYardEmptyPickupAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_ContainerYardEmptyPickupAddress, null, orgAddressDataObject);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.ContainerYardEmptyReturnAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_ContainerYardEmptyReturnAddress, null, orgAddressDataObject);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.Creditor))
					{
						if (creditorAddress == null)
						{
							creditorAddress = orgAddressDataObject;
						}
					}
					else if (consolBO.IsCoLoad && orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.CoLoadWith))
					{
						creditorAddress = orgAddressDataObject;
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.DepartureCTOAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_DepartureCTOAddress, null, orgAddressDataObject);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.DepartureCFSLocalTransportAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_DeparturePackCFSTransportAddress, null, orgAddressDataObject);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.DepartureCFSAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_PackDepotAddress, null, orgAddressDataObject);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.ReceivingForwarderAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_ReceivingForwarderAddress, null, orgAddressDataObject, OrganisationTypes.Forwarder, OrganisationsSubTypeList.Descriptions.ReceivingForwarder);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.SendingForwarderAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_SendingForwarderAddress, null, orgAddressDataObject, OrganisationTypes.Forwarder, OrganisationsSubTypeList.Descriptions.SendingForwarder);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.ShippingLineAddress))
					{
						SetOrgAddressWithFallback(consolBO, JobConsolSchema.JK_OA_ShippingLineAddress, null, orgAddressDataObject, OrganisationTypes.Carrier);
					}
					else if (orgAddressDataObject.AddressType.GetValueOrDefault() == nameof(DocAddressType.ArrivalCFSAddress))
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_UnpackDepotAddress, null, orgAddressDataObject);
					}
					else
					{
						var jobDocAddress = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory).GetMatchedOrNew(consolBO);

						if (jobDocAddress != null)
						{
							consolBO.DocAddresses.Add(jobDocAddress);
						}
					}
				}

				if (creditorAddress != null)
				{
					if (consolBO.IsCoLoad)
					{
						SetOrgAddressWithFallback(consolBO, JobConsolSchema.JK_OA_CreditorAddress, null, creditorAddress, OrganisationTypes.Creditor);
					}
					else
					{
						SetValue(consolBO, JobConsolSchema.JK_OA_CreditorAddress, null, creditorAddress, OrganisationTypes.Creditor);
					}
				}
			}

			DefaultAgentAddresses(consolBO);
		}

		void DefaultAgentAddresses(ForwardingConsol consolBO)
		{
			if (dataObject.OrganizationAddressCollection == null || !dataObject.OrganizationAddressCollection.Any(a => a.AddressType.GetValueOrDefault() == nameof(DocAddressType.ReceivingForwarderAddress)))
			{
				var portOfDischarge = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, dataObject.PortOfDischarge?.Code?.ToString());
				var receivingForwarderAddress = portOfDischarge?.GetBestAgent(null, dataObject.TransportMode?.Code ?? ZString.Empty, AgentDirectionList.Codes.Import, false);

				SetValuesFromAddress(consolBO, JobConsolSchema.JK_OA_ReceivingForwarderAddress, null, receivingForwarderAddress);
			}

			if (dataObject.OrganizationAddressCollection == null || !dataObject.OrganizationAddressCollection.Any(a => a.AddressType.GetValueOrDefault() == nameof(DocAddressType.SendingForwarderAddress)))
			{
				var portOfLoading = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, dataObject.PortOfLoading?.Code?.ToString());
				var sendingForwarderAddress = portOfLoading?.GetBestAgent(null, dataObject.TransportMode?.Code ?? ZString.Empty, AgentDirectionList.Codes.Export, false);

				SetValuesFromAddress(consolBO, JobConsolSchema.JK_OA_SendingForwarderAddress, null, sendingForwarderAddress);
			}
		}

		void SetValue(ForwardingConsol consolBO, SchemaColumn orgAddress, SchemaColumn orgHeader, OrganizationAddress addressDataObject, OrganisationTypes orgCategory, string orgType = null)
		{
			if (addressDataObject.RemoveAddressFromBizo(logger, consolBO, orgAddress, orgHeader))
			{
				return;
			}

			var addressBO = new ConsolOrganisationDataObjectReader(addressDataObject, logger, factory, consolBO).GetMatched(consolBO, orgCategory, orgType);
			SetValuesFromAddress(consolBO, orgAddress, orgHeader, addressBO);
		}

		void SetValue(ForwardingConsol consolBO, SchemaColumn orgAddress, SchemaColumn orgHeader, OrganizationAddress addressDataObject)
		{
			if (addressDataObject.RemoveAddressFromBizo(logger, consolBO, orgAddress, orgHeader))
			{
				return;
			}

			var addressBO = new ConsolOrganisationDataObjectReader(addressDataObject, logger, factory, consolBO).GetMatched();
			SetValuesFromAddress(consolBO, orgAddress, orgHeader, addressBO);
		}

		void SetOrgAddressWithFallback(ForwardingConsol consolBO, SchemaColumn orgAddress, SchemaColumn orgHeader, OrganizationAddress addressDataObject, OrganisationTypes orgCategory)
		{
			if (addressDataObject.RemoveAddressFromBizo(logger, consolBO, orgAddress, orgHeader))
			{
				return;
			}

			var addressBO = new ConsolOrganisationDataObjectReader(addressDataObject, logger, factory, consolBO).GetMatched(consolBO, orgCategory);

			if (IsNewBO)
			{
				SetValuesFromAddress(consolBO, orgAddress, orgHeader, addressBO);
			}
			else
			{
				var orgAddressPK = (ZGuid)consolBO[orgAddress];

				if (orgAddressPK.IsEmpty
						|| (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled
								&& (factory.Load<OrgAddress>(orgAddressPK)?.OA_OH ?? Guid.Empty).Equals(OrgHeader.UnmatchedOrganisationPK)))
				{
					var fallbackOrgAddress = TryGetOrgAddressByRegistrationNumber(addressDataObject, OrgCusCode.CodeTypes.CargoWiseOneCarrierCode) ?? TryGetOrgAddressByRegistrationNumber(addressDataObject, OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);

					if (fallbackOrgAddress != null)
					{
						SetValuesFromAddress(consolBO, orgAddress, orgHeader, fallbackOrgAddress);
					}
				}
				else if (addressDataObject.CompanyName.HasValue && addressDataObject.Address1.HasValue)
				{
					SetValuesFromAddress(consolBO, orgAddress, orgHeader, addressBO);
				}
			}
		}

		OrgAddress TryGetOrgAddressByRegistrationNumber(OrganizationAddress addressDataObject, string codeType, string countryCode = null)
		{
			var customsRegNo = addressDataObject?.RegistrationNumberCollection?.FirstOrDefault(number => (number.Type?.Code ?? ZString.Empty) == codeType && number.Value.HasValue)?.Value.GetValueOrDefault();

			if (!customsRegNo.HasValue)
			{
				return null;
			}

			var filter = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, customsRegNo.Value);
			filter.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);

			if (!string.IsNullOrEmpty(countryCode))
			{
				filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			}

			var orgCusCode = factory.LoadTop1<OrgCusCode>(filter);
			return orgCusCode?.Organisation?.Addresses?.MainAddress;
		}

		static void SetValuesFromAddress(BusinessObject consolBO, SchemaColumn orgAddress, SchemaColumn orgHeader, OrgAddress addressBO)
		{
			if (addressBO != null)
			{
				consolBO[orgAddress] = addressBO.PK;

				if (orgHeader != null)
				{
					consolBO[orgHeader] = addressBO.OA_OH;
				}
			}
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.ForwardingConsol; }
		}

		bool TrySetMaximumAllowablePackage(ForwardingConsol consolBO, out ZString error)
		{
			error = ZString.Empty;

			SetValue(consolBO, JobConsolSchema.JK_MaximumAllowablePackageLength, dataObject.MaximumAllowablePackageLength);
			SetValue(consolBO, JobConsolSchema.JK_MaximumAllowablePackageWidth, dataObject.MaximumAllowablePackageWidth);
			SetValue(consolBO, JobConsolSchema.JK_MaximumAllowablePackageHeight, dataObject.MaximumAllowablePackageHeight);

			var hasUnitValue = dataObject.MaximumAllowablePackageLengthUnit != null
				&& dataObject.MaximumAllowablePackageLengthUnit.Code.GetValueOrDefault() != ZString.Empty;

			var hasDimensionValue = (dataObject.MaximumAllowablePackageLength.HasValue && dataObject.MaximumAllowablePackageLength > 0)
				|| (dataObject.MaximumAllowablePackageWidth.HasValue && dataObject.MaximumAllowablePackageWidth > 0)
				|| (dataObject.MaximumAllowablePackageHeight.HasValue && dataObject.MaximumAllowablePackageHeight > 0);

			if (hasDimensionValue && !hasUnitValue)
			{
				error = Res.GetString("40CD4305-B761-4299-9B70-7BD8ADD6A69E",
					"{0} is required when {1}, {2} or {3} is greater than zero.",
					nameof(dataObject.MaximumAllowablePackageLengthUnit),
					nameof(dataObject.MaximumAllowablePackageWidth),
					nameof(dataObject.MaximumAllowablePackageLength),
					nameof(dataObject.MaximumAllowablePackageHeight));
				return false;
			}

			if (hasUnitValue)
			{
				if (!Constants.Length.Codes.Contains((string)dataObject.MaximumAllowablePackageLengthUnit.Code.Value))
				{
					error = Res.GetString("91EE193C-4BEA-4A65-8806-74C9CEA9C903",
						"Invalid {0}. Value must be one of valid length units: MM, CM, M, KM, MI, IN, FT, YD.",
						nameof(dataObject.MaximumAllowablePackageLengthUnit));
					return false;
				}

				SetValue(consolBO, JobConsolSchema.JK_MaximumAllowablePackageUnit, dataObject.MaximumAllowablePackageLengthUnit.Code.Value);
			}

			return true;
		}
	}
}
