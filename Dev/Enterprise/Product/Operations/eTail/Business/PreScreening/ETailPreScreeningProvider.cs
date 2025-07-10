using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;

namespace Enterprise.eTail.Business
{
	public class ETailPreScreeningProvider
	{
		public ETailPreScreeningProvider(IHVLVPrescreeningDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public ETailPreScreeningProvider(IHVLVPrescreeningDataProvider dataProvider, Action<string, int> progressUpdate) : this(dataProvider)
		{
			this.progressUpdate = progressUpdate;
		}

		public static bool IsPreScreeningHVLVDetailsEnabled => ValidationConfiguration.IsEnabled;
		public bool HasPreScreeningRules => ValidationConfiguration.Rules.Count > 0;

		static HVLVDetailsPreScreeningConfiguration ValidationConfiguration => HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.Value;

		readonly Dictionary<string, HVLVPreScreeningRule[]> matchedRuleDictionary = new Dictionary<string, HVLVPreScreeningRule[]>();
		readonly IHVLVPrescreeningDataProvider dataProvider;
		readonly Action<string, int> progressUpdate;

		public ETailPreScreeningResponse ScreeningResult => screeningResult;
		ETailPreScreeningResponse screeningResult;

		public void Screen()
		{
			screeningResult = null;
			if (!IsPreScreeningHVLVDetailsEnabled)
			{
				screeningResult = new ETailPreScreeningRegistryDisabledResponse();
			}
			else if (dataProvider.Consignments == null || !dataProvider.Consignments.Any())
			{
				screeningResult = new ETailPreScreeningEmptyConsignmentCollectionResponse(dataProvider.TableCode, dataProvider.Entity.PK.ToGuid());
			}
			else
			{
				ProcessConsignments(dataProvider.Consignments.OfType<HVLVConsignment>());
			}
		}

		public void SendPreScreeningNotificationEmail()
		{
			var processor = new PreScreeningNotificationProcessor();

			var failedResults = screeningResult.Results?.Where(r => r.PreScreeningStatus == HVLVConsignmentPreScreeningStatusCodes.Codes.Failed).ToArray();

			if (!failedResults.IsNullOrEmpty())
			{
				processor.SendPreScreeningResult(dataProvider.Entity, failedResults);
			}
		}

		public void AddLog()
		{
			if (screeningResult?.Results?.Count > 0)
			{
				var preScreeningLogParameters = new []
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, Core.Constants.EventReferenceParameterReasons.PreScreened),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Total, screeningResult.Results.Count.ToString())
				};

				dataProvider.Logs.AddNew(AutoEvents.HVLVReady, preScreeningLogParameters);
			}
		}

		public void SyncScreeningResult()
		{
			if (screeningResult.Results != null)
			{
				foreach (var result in screeningResult.Results.OfType<HVLVConsignmentPreScreeningResult>())
				{
					result.Consignment.HVC_PreScreeningStatus = result.PreScreeningStatus;
				}
			}
		}

		void ProcessConsignments(IEnumerable<HVLVConsignment> consignments)
		{
			var stopWatch = Stopwatch.StartNew();

			screeningResult = new ETailPreScreeningResponse();

			var screenedConsignmentCount = 0;
			var unscreenedConsignments = consignments.Where(x => x.HVC_IsActive && HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown == x.HVC_PreScreeningStatus);
			var consignmentsCount = unscreenedConsignments.Count();
			foreach (var consignment in unscreenedConsignments)
			{
				var result = PreScreenHVLVConsignmentCore(consignment);
				if (result.Finished)
				{
					screeningResult.AddPreScreeningResult(result.Results.FirstOrDefault());
				}

				if (progressUpdate != null)
				{
					screenedConsignmentCount++;
					var timeleft = (consignmentsCount - screenedConsignmentCount) / (screenedConsignmentCount * 1000F / stopWatch.ElapsedMilliseconds);
					var message = Res.GetString("b11418f2-0e5a-49a8-8b26-558c8b8c08db", "Pre-Screening {0}/{1} Time left: {2:0.00} seconds", screenedConsignmentCount, consignmentsCount, timeleft);
					var percentComplete = 100 * screenedConsignmentCount / consignmentsCount;
					progressUpdate.Invoke(message, percentComplete);
				}
			}

			stopWatch.Stop();
		}

		ETailPreScreeningResponse PreScreenHVLVConsignmentCore(HVLVConsignment consignment)
		{
			var response = new ETailPreScreeningResponse();
			if (consignment == null)
			{
				response = new ETailPreScreeningConsignmentNotFoundResponse(Guid.Empty);
			}
			else
			{
				var consignmentResult = new HVLVConsignmentPreScreeningResult(consignment);
				var transportMode = consignment.ManifestedOnShipment?.TransportMode ?? ZString.Empty;
				var matchedRuleKey = transportMode + dataProvider.ETailerOrgCode + consignment.OriginCountry + consignment.DestinationCountry;

				if (!matchedRuleDictionary.TryGetValue(matchedRuleKey, out var mostMatchedRules))
				{
					mostMatchedRules = ValidationConfiguration.GetMostMatchedPreScreeningRules(dataProvider.TableCode, transportMode, dataProvider.ETailerOrgCode, consignment.OriginCountry, consignment.DestinationCountry);
					matchedRuleDictionary.Add(matchedRuleKey, mostMatchedRules);
				}

				consignment.ClearPreScreeningDetails();
				if (mostMatchedRules != null)
				{
					foreach (var rule in mostMatchedRules)
					{
						foreach (HVLVPreScreeningField field in rule.Fields)
						{
							var preScreeningAction = GetPreScreeningAction(field, consignment);
							preScreeningAction.PerformPreScreening(consignmentResult);
						}
					}
				}

				response.AddPreScreeningResult(consignmentResult);
			}

			return response;
		}

		static PreScreeningAction GetPreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consignment)
		{
			var preScreeningAction = default(PreScreeningAction);
			if (field.IsSpecialCharactersField)
			{
				preScreeningAction = new CheckSpecialCharactersAction(field, consignment);
			}
			else if (field.IsDeminimusField)
			{
				preScreeningAction = new CheckDeminimusValueAction(field, consignment);
			}
			else if (field.IsMacrosField)
			{
				preScreeningAction = new CheckUserDefinedMacrosAction(field, consignment);
			}
			else if (field.IsHSCodeField)
			{
				preScreeningAction = new CheckItemLineTariffAction(field, consignment);
			}
			else
			{
				switch (field.FieldName)
				{
					case HVLVConsignmentSchema.Constants.HVC_GoodsDescription:
					preScreeningAction = new CheckGoodsDescriptionAction(field, consignment);
					break;
					default:
					preScreeningAction = new CheckNormalPropertyAction(field, consignment);
					break;
				}
			}

			return preScreeningAction;
		}

#if DEBUG

		public void ClearRuleMatchingCache_ForTest()
		{
			matchedRuleDictionary.Clear();
		}

#endif
	}

	public class ETailPreScreeningService : IETailPreScreeningService
	{
		public ETailPreScreeningService(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		BusinessObjectFactory Factory { get; }

		public IETailPreScreeningResponse PreScreenHVLVConsignment(Guid consignmentPK)
		{
			var consignment = Factory.Load<HVLVConsignment>(consignmentPK);
			var screeningProvider = new ETailPreScreeningProvider(consignment);

			screeningProvider.Screen();
			screeningProvider.AddLog();
			screeningProvider.SyncScreeningResult();
			return screeningProvider.ScreeningResult;
		}

		public IETailPreScreeningResponse PreScreenHVLVConsignmentCollection(string entityTableCode, Guid entityPK)
		{
			ETailPreScreeningProvider screeningProvider;
			if (entityTableCode == JobShipmentSchema.Constants.Prefix)
			{
				var shipment = Factory.Load<ForwardingShipment>(entityPK);
				if (shipment?.GetHVLVConsignmentHeader() is { } consignmentHeader)
				{
					screeningProvider = new ETailPreScreeningProvider(consignmentHeader);
				}
				else
				{
					return new ETailPreScreeningConsignmentParentNotFoundResponse(entityTableCode, entityPK);
				}
			}
			else
			{
				if (Factory.Load(entityTableCode, entityPK) is IHVLVPrescreeningDataProvider dataProvider)
				{
					screeningProvider = new ETailPreScreeningProvider(dataProvider);
				}
				else
				{
					return new ETailPreScreeningConsignmentParentNotFoundResponse(entityTableCode, entityPK);
				}
			}

			screeningProvider.Screen();
			screeningProvider.SyncScreeningResult();
			screeningProvider.AddLog();
			screeningProvider.SendPreScreeningNotificationEmail();
			return screeningProvider.ScreeningResult;
		}
	}
}
