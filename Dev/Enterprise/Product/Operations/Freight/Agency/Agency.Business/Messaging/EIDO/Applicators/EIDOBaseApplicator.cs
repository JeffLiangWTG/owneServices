using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public abstract class EIDOBaseApplicator : ReleaseImportOrderActionMethodApplicator
	{
		protected EIDOBaseApplicator(string name, ReleaseImportOrderSettings settings)
			: base(name)
		{
			if (settings.ErrorBehaviour == OperationalActionErrorBehaviourList.Codes.Skip)
			{
				validationErrorLevel = OperationalActionLogErrorLevel.Warning;
			}
			else
			{
				validationErrorLevel = OperationalActionLogErrorLevel.Error;
			}
		}

		public static class EventParameterValues
		{
			public const string EIDO = "E-IDO";
			[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Define paramter")]
			public const string EIDOWithdrawCancelRequest = "E-IDO Withdraw/Cancel Request";
			public const string OneStop = "1-stop";
		}

		protected sealed override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var message = GetEIDOErrorMessage();

			if (!string.IsNullOrEmpty(message))
			{
				log.Notify(OperationalActionLogErrorLevel.Error, message);
			}
			else if (targets.Length > 0)
			{
				EIDOBusinessObjectValidation.RegisterForFactory(targets[0].Factory);
				var validTargets = new Dictionary<BillOfLading, List<BillOfLadingContainer>>();

				var bills = new List<BillOfLading>();
				var containers = new List<BillOfLadingContainer>();

				foreach (var bizObj in targets)
				{
					var shipment = bizObj as BillOfLading;
					var container = bizObj as BillOfLadingContainer;

					if (shipment != null)
					{
						if (shipment.JS_PackingMode != Constants.ContainerModes.FCL)
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
								Res.GetString("7b8e7060-9fb1-4261-b108-0a310351b65d", "{0:G} is not a FCL shipment, skipping."), HyperlinkHelper.Link(shipment));
						}
						else
						{
							bills.Add(shipment);
							shipment.Factory.AddFetchHint(JobContainerSchema.JC_JS_FCLBookingOnlyLink, shipment.PK);
						}
					}
					else if (container != null)
					{
						containers.Add(container);
						container.Factory.AddFetchHint(JobShipmentSchema.PK, container.JC_JS_FCLBookingOnlyLink);
					}
					else
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Dont know how to handle a '{0}'.", bizObj.GetType().FullName));
					}
				}

				foreach (var bill in bills)
				{
					AddTargets(validTargets, bill, bill.RealContainers.ToArray<BillOfLadingContainer>());
				}

				foreach (var container in containers)
				{
					AddTargets(validTargets, container.Booking, new BillOfLadingContainer[] { container });
				}

				AddFetchHints(validTargets.Keys);
				ApplyShipments(log, validTargets);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected abstract void ApplyShipments(IOperationalActionSectionLog log, Dictionary<BillOfLading, List<BillOfLadingContainer>> targets);

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected bool ValidateSet(IOperationalActionSectionLog log, KeyValuePair<BillOfLading, List<BillOfLadingContainer>> pair)
		{
			if (!pair.Key.IsBillOfLadingStage)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error,
					Res.GetString("68dcc103-9b5f-4007-aeec-08af9f72fa15", "'{0:G}' is a booking and not eligible for E-IDO messaging."),
					HyperlinkHelper.Link(pair.Key));
			}
			else if (!ImportExportHelper.IsBranchCountry(pair.Key.JS_RL_NKDestination))
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					Res.GetString("d67ea711-c72d-4819-9f05-2e3e6d20b5ca", "'{0:G}' is released in another country/region and is not eligible for E-IDO messaging, skipping."),
					HyperlinkHelper.Link(pair.Key));
			}
			else if (pair.Key.JS_PackingMode != Core.Constants.ContainerModes.FCL)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					Res.GetString("7b8e7060-9fb1-4261-b108-0a310351b65d", "{0:G} is not a FCL shipment, skipping."),
					HyperlinkHelper.Link(pair.Key));
			}
			else if (HasErrorsOrMessageErrors(pair.Key))
			{
				string messageFormat =
					Res.GetString("3c60e11a-6a12-4e6c-aea9-25627f0725a0", "'{0:G}' has errors and/or message errors.\r\nYou will need to correct these before an E-IDO message can be sent for it.");

				log.NotifyFormat(ValidationErrorLevel, messageFormat, HyperlinkHelper.Link(pair.Key));
			}
			else if (pair.Key.Principal == null)
			{
				string messageFormat =
					Res.GetString("772b9ef3-78e9-4c06-95ea-6d5c8d889dae", "{0} does not have a principal set.");

				log.NotifyFormat(OperationalActionLogErrorLevel.Error, messageFormat, HyperlinkHelper.Link(pair.Key));
			}
			else if (Detail.GetIdentity(pair.Key.JS_OH_DeliveryAgent) == null)
			{
				string messageFormat =
					Res.GetString("20ac6b09-a7f7-41b1-87b8-86f8f6920fb1", "E-IDO messaging has not been enabled for the principal {0}.\r\nYou can enable it in the 'Liner & Agency -> E-IDO Messaging' registry option", pair.Key.Principal.OH_Code);

				log.NotifyFormat(OperationalActionLogErrorLevel.Error, messageFormat);
			}
			else
			{
				return true;
			}

			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "workflow processor message")]
		public string GetEIDOErrorMessage()
		{
			var result = "";
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;

			if (!Detail.Identities.Any())
			{
				result = (NoResString)"E-IDO messaging has not been enabled.\r\nYou can enable it in the 'Liner & Agency -> E-IDO Messaging' registry option.";
			}
			else if (orgProxy == null)
			{
				result = (NoResString)"The current branch does not have an org. proxy.";
			}
			else if (orgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.OneStopCode, Core.Constants.CountryCodes.Australia).IsEmpty)
			{
				result = "The current branch's org. proxy does not have a 1-Stop code entered.";
			}

			return result;
		}

		public bool HasErrorsOrMessageErrors(BillOfLading shipment)
		{
			shipment.MarkAsNeedingValidation();
			shipment.OuterPackLines.MarkAsNeedingValidation();
			shipment.RealContainers.MarkAsNeedingValidation();
			shipment.DocAddresses.MarkAsNeedingValidation();

			shipment.RunPreSaveValidation();

			return shipment.HasErrors || shipment.HasMessageErrors;
		}

		protected static void AddFetchHints(IEnumerable<BillOfLading> shipments)
		{
			foreach (BusinessObjectFactory factory in ExtractFactories(shipments))
			{
				// tables not used by agency but are incorrectly poked by base
				factory.SeedQueryCache(CusContainerSchema.Constants.TableName, new ZQuery());
				factory.SeedQueryCache(JobConShipLinkSchema.Constants.TableName, new ZQuery());
				factory.SeedQueryCache(JobContainerMoveSchema.Constants.TableName, new ZQuery());

				// should have already loaded all the shipments we care about. (co-loads are a forwarding concept and not relevant to agency).
				factory.SeedQueryCache(JobShipmentSchema.Constants.TableName, new ZQuery());
			}

			foreach (BillOfLading shipment in shipments)
			{
				shipment.Factory.AddFetchHint(JobContainerSchema.JC_JS_FCLBookingOnlyLink, shipment.PK);
				shipment.Factory.AddFetchHint(new FetchHint(JobPackLinesSchema.JL_JS, shipment.PK, JobPackLinesSchema.JL_DetailedDescription, JobPackLinesSchema.JL_MarksAndNumbers));
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKOrigin);
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKDestination);
				shipment.Factory.AddFetchHint(OrgAddressSchema.PK, shipment.JS_OA_BookedShippingLineAddress);
				shipment.Factory.AddFetchHint(OrgHeaderSchema.PK, shipment.JS_OH_DeliveryAgent);
				shipment.Factory.AddFetchHint(OrgCusCodeSchema.OK_OA_PremisesAddress, shipment.JS_OA_BookedShippingLineAddress);
				shipment.Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, shipment.JS_OH_DeliveryAgent);
				shipment.Factory.AddFetchHint(OrgCompanyDataSchema.OB_OH, shipment.JS_OH_DeliveryAgent);
				shipment.Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, shipment.PK);
				shipment.Factory.AddFetchHint(typeof(JobSailing), shipment.JS_JX);
				shipment.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, shipment.PK);
				shipment.Factory.AddFetchHint(CusHAWBSchema.CS_JS, shipment.PK);
				shipment.Factory.AddFetchHint(JobDeclarationSchema.JE_JS, shipment.PK);
				shipment.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, shipment.PK);
				shipment.Factory.AddFetchHint(StmALogSchema.SL_Parent, shipment.PK);

				shipment.Factory.AddFetchHint(JobHeaderSchema.Instance, new ZQuery(
					new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK),
					new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)
				));
			}

			foreach (BillOfLading shipment in shipments)
			{
				using (shipment.GetValidationSuspender())
				{
					JobSailing sailing;
					if ((sailing = shipment.Sailing) != null)
					{
						shipment.Factory.AddFetchHint(OrgAddressSchema.PK, sailing.JX_JB_ArrivalCTOAddress);
					}

					foreach (BillOfLadingPackLine packline in shipment.OuterPackLines)
					{
						shipment.Factory.AddFetchHint(JobContainerPackPivotSchema.J6_JL, packline.PK);
					}

					foreach (BillOfLadingContainer container in shipment.RealContainers)
					{
						shipment.Factory.AddFetchHint(RefContainerStockSchema.R6_ContainerNum, container.JC_ContainerNum);
						shipment.Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, container.PK);
						shipment.Factory.AddFetchHint(JobContainerPackPivotSchema.J6_JC, container.PK);
						shipment.Factory.AddFetchHint(RefContainerSchema.PK, container.JC_RC);
						shipment.Factory.AddFetchHint(OrgAddressSchema.PK, container.JC_OA_ArrivalContainerYardAddress);
						shipment.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, container.PK);
					}

					foreach (JobDocAddress address in shipment.DocAddresses)
					{
						shipment.Factory.AddFetchHint(OrgAddressSchema.PK, address.E2_OA_Address);
					}
				}
			}

			AddFetchHintsForSailings(shipments);
		}

		static IEnumerable<BusinessObjectFactory> ExtractFactories(IEnumerable<BillOfLading> shipments)
		{
			Dictionary<BusinessObjectFactory, bool> lookup = new Dictionary<BusinessObjectFactory, bool>();

			foreach (BillOfLading bill in shipments)
			{
				lookup[bill.Factory] = true;
			}

			return lookup.Keys;
		}

		static void AddFetchHintsForSailings(IEnumerable<BillOfLading> shipments)
		{
			var sailings = new List<JobSailing>();

			foreach (BillOfLading shipment in shipments)
			{
				JobSailing sailing;

				if ((sailing = shipment.Sailing) != null)
				{
					sailings.Add(sailing);
				}

				foreach (BillOfLadingContainer container in shipment.RealContainers)
				{
					if (container.ArrivalContainerYardAddress != null)
					{
						shipment.Factory.AddFetchHint(OrgHeaderSchema.PK, container.ArrivalContainerYardAddress.OA_OH);
						shipment.Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, container.ArrivalContainerYardAddress.OA_OH);
					}
				}

				foreach (JobDocAddress address in shipment.DocAddresses)
				{
					if (address.Address != null)
					{
						shipment.Factory.AddFetchHint(OrgHeaderSchema.PK, address.Address.OA_OH);
						shipment.Factory.AddFetchHint(OrgAddressSchema.OA_OH, address.Address.OA_OH);
						shipment.Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, address.Address.OA_OH);
					}
				}
			}

			foreach (BillOfLading shipment in shipments)
			{
				JobSailing sailing;

				foreach (Transport transport in shipment.Transports)
				{
					if ((sailing = transport.Sailing) != null)
					{
						sailings.Add(sailing);
					}
				}
			}

			foreach (JobSailing sailing in sailings)
			{
				OrgAddress ctoAddress;
				VoyageDestination destination = sailing.Destination;

				sailing.Factory.SeedQueryCache(JobSailingSchema.Constants.TableName, new ZQuery(JobSailingSchema.JX_JA, sailing.JX_JA));
				sailing.Factory.SeedQueryCache(JobSailingSchema.Constants.TableName, new ZQuery(JobSailingSchema.JX_JB, sailing.JX_JB));
				sailing.Factory.AddFetchHint(JobVoyCountrySchema.J0_JV, destination.JB_JV);
				sailing.Factory.AddFetchHint(JobVoyOriginSchema.JA_JV, destination.JB_JV);
				sailing.Factory.AddFetchHint(JobVoyDestinationSchema.JB_JV, destination.JB_JV);
				sailing.Factory.AddFetchHint(JobTradeLaneVoyageSchema.NB_JV, destination.JB_JV);

				if ((ctoAddress = destination.ArrivalCTOAddress) != null)
				{
					sailing.Factory.AddFetchHint(OrgHeaderSchema.PK, ctoAddress.OA_OH);
					sailing.Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, ctoAddress.OA_OH);
				}
			}
		}

		protected OperationalActionLogErrorLevel ValidationErrorLevel
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return validationErrorLevel; }
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalActionLogErrorLevel validationErrorLevel;

		void AddTargets(Dictionary<BillOfLading, List<BillOfLadingContainer>> targets, BillOfLading shipment, IEnumerable<BillOfLadingContainer> containers)
		{
			List<BillOfLadingContainer> list;

			if (targets.TryGetValue(shipment, out list))
			{
				list.AddRange(containers);
			}
			else
			{
				targets.Add(shipment, new List<BillOfLadingContainer>(containers));
			}
		}

		protected EIDOMessagingHeader Detail
		{
			get { return detail ?? (detail = AgencyRegistry.Instance.EIDOMessagingDetails.Value); }
		}
		EIDOMessagingHeader detail;

		public static KeyValuePair<string, string>[] GetParamtersForEvent(Event eventToCheck)
		{
			var paramters = new Dictionary<string, string>();
			paramters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department] = EventParameterValues.OneStop;
			paramters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType] = Equals(eventToCheck, Events.MessageWithdrawCancelRequest)
				? EventParameterValues.EIDOWithdrawCancelRequest
				: EventParameterValues.EIDO;

			return paramters.ToArray();
		}
	}
}


