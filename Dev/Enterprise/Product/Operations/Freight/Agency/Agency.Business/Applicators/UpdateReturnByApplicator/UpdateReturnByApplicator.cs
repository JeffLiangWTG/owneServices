using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class UpdateReturnByApplicator : AutoUpdateReturnByApplicator
	{
		public UpdateReturnByApplicator()
			: base(Res.GetString("dd58264f-741c-479d-bf3a-a25dcd1fc64a", "Update Return By")) { }

		protected sealed override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length > 0)
			{
				Dictionary<BillOfLading, List<BillOfLadingContainer>> validTargets = new Dictionary<BillOfLading, List<BillOfLadingContainer>>();

				List<BillOfLading> bills = new List<BillOfLading>();
				List<BillOfLadingContainer> containers = new List<BillOfLadingContainer>();

				foreach (BusinessObject bizObj in targets)
				{
					BillOfLading shipment;
					BillOfLadingContainer container;

					if ((shipment = bizObj as BillOfLading) != null)
					{
						if (shipment.JS_PackingMode != Constants.ContainerModes.FCL)
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
								Res.GetString("5b1322e8-007a-4953-9725-3cb8bcb11d2c", "{1} is not a FCL shipment, skipping."),
								null,
								HyperlinkHelper.Link(shipment));
						}
						else
						{
							bills.Add(shipment);
							shipment.Factory.AddFetchHint(JobContainerSchema.JC_JS_FCLBookingOnlyLink, shipment.PK);
							shipment.Factory.AddFetchHint(StmALogSchema.SL_Parent, shipment.PK);
							shipment.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, shipment.PK);
						}
					}
					else if ((container = bizObj as BillOfLadingContainer) != null)
					{
						containers.Add(container);
						container.Factory.AddFetchHint(JobShipmentSchema.PK, container.JC_JS_FCLBookingOnlyLink);
						container.Factory.AddFetchHint(StmALogSchema.SL_Parent, container.PK);
						container.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, container.PK);
					}
					else
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Dont know how to handle a '{0}'.", bizObj.GetType().FullName));
					}
				}

				foreach (BillOfLading bill in bills)
				{
					AddTargets(validTargets, bill, bill.RealContainers.ToArray<BillOfLadingContainer>());
				}

				foreach (BillOfLadingContainer container in containers)
				{
					AddTargets(validTargets, container.Booking, new BillOfLadingContainer[] { container });
				}

				AddFetchHints(validTargets);
				UpdateContainers(log, validTargets);
			}
		}

		static void UpdateContainers(IOperationalActionSectionLog log, Dictionary<BillOfLading, List<BillOfLadingContainer>> targets)
		{
			log.SetSectionProgressMax(targets.Count);

			foreach (KeyValuePair<BillOfLading, List<BillOfLadingContainer>> pair in targets)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
					Res.GetString("287fa33f-e11f-4b2e-a5e2-3a9e8f718e58", "Processing containers on {1}:"),
					null,
					HyperlinkHelper.Link(pair.Key));

				foreach (BillOfLadingContainer container in pair.Value)
				{
					UpdateContainerCore(log, container);
				}

				log.BumpSectionProgress();
			}
		}

		static void UpdateContainerCore(IOperationalActionSectionLog log, BillOfLadingContainer container)
		{
			ZDateTime oldValue = container.JC_EmptyReturnedBy;
			IContainerDefaultingStrategy strategy = container.NewContainerDefaultingStrategy();
			var (requiredBy, _, _) = strategy == null ? (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty) : strategy.CalculateRequiredBy();

			if (container.JC_ContainerCode.IsEmpty)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error,
					Res.GetString("245d69e6-f28d-4fc2-b946-baf7f66845b7",
									"Containers exist without a Container Number & Container Type. Correct the errors on Bill of Lading {0}",
									container.Booking.JS_UniqueConsignRef));
			}
			else if (requiredBy.IsEmpty)
			{
				if (oldValue.IsEmpty)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
						Res.GetString("d7c187df-5ffe-4ebf-9de6-0bdcca795171", "Was not able to determine the return required by date for {1}. Leaving the value empty."),
						null,
						HyperlinkHelper.Link(container));
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
						Res.GetString("eea4f5c2-5cf4-4bc2-8aff-66b7840c2a1e", "Was not able to determine the return required by date for {1}. Leaving the value as '{2}'."),
						null,
						HyperlinkHelper.Link(container),
						oldValue.ToShortDateString());
				}
			}
			else if (requiredBy.Date != oldValue.Date)
			{
				container.JC_EmptyReturnedBy = requiredBy;

				if (oldValue.IsEmpty)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
						Res.GetString("265eeef1-f0df-4c62-bf39-955dacdeb0d6", "The return required by date for {1} was updated from empty to '{2}'."),
						null,
						HyperlinkHelper.Link(container),
						requiredBy.ToShortDateString());
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
						Res.GetString("9c2efe67-20e8-4f08-898c-1fae905cf42a", "The return required by date for {1} was updated from '{2}' to '{3}'."),
						null,
						HyperlinkHelper.Link(container),
						oldValue.ToShortDateString(),
						requiredBy.ToShortDateString());
				}

				UpdateImportDetentionableMovements(log, container);
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
					Res.GetString("28a20438-f66d-4ae2-a87c-fd5fbb6d09d7", "The return required by date for {1} is already correct as '{2}'."),
					null,
					HyperlinkHelper.Link(container),
					oldValue.ToShortDateString());

				UpdateImportDetentionableMovements(log, container);
			}
		}

		internal static void UpdateContainer(IOperationalActionSectionLog log, BillOfLadingContainer container)
		{
			if (log != null && container != null)
			{
				UpdateContainerCore(log, container);
			}
		}

		static void UpdateImportDetentionableMovements(IOperationalActionSectionLog log, BillOfLadingContainer container)
		{
			ZQuery importDetentionableMovements = new ZQuery(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.GetMovementCodesForDetention(DetentionInvoiceType.Codes.Import));

			foreach (ContainerMovement movement in container.Movements.Find(importDetentionableMovements))
			{
				short oldDetentionDays = movement.E9_DetentionDays;
				short newDetentionDays = movement.DetentionStrategy.GetDefaultDetentionDays(movement).GetValueOrDefault(0);

				if (oldDetentionDays != newDetentionDays)
				{
					if (movement.DetentionPosted)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
							Res.GetString("76249640-c3ad-4606-afc1-0ea0bf9cfd75", "Cannot update the detention days for the {1} movement from {2} to {3} as it already has a posted invoice on {4}."),
							null,
							HyperlinkHelper.Link(movement),
							oldDetentionDays,
							newDetentionDays,
							HyperlinkHelper.Link(movement.Detention));
					}
					else
					{
						movement.E9_DetentionDays = newDetentionDays;

						if (movement.DetentionInvoiced)
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
								Res.GetString("f1f19699-b525-4be0-92d2-622587d6ae54", "Updating the detention days for the {1} movement from {2} to {3} however it is currently attached to the detention invoice {4}."),
								null,
								HyperlinkHelper.Link(movement),
								oldDetentionDays,
								newDetentionDays,
								HyperlinkHelper.Link(movement.Detention));
						}
						else
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
								Res.GetString("e858d216-d0ac-4369-ab97-48dee083110b", "Updating the detention days for the {1} movement from {2} to {3}."),
								null,
								HyperlinkHelper.Link(movement),
								oldDetentionDays,
								newDetentionDays);
						}
					}
				}
			}
		}

		static void AddTargets(Dictionary<BillOfLading, List<BillOfLadingContainer>> targets, BillOfLading shipment, IEnumerable<BillOfLadingContainer> containers)
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

		static void AddFetchHints(IEnumerable<KeyValuePair<BillOfLading, List<BillOfLadingContainer>>> list)
		{
			List<JobDocAddress> consignees = new List<JobDocAddress>();

			foreach (BusinessObjectFactory factory in ExtractFactories(list))
			{
				factory.SeedQueryCache(CusContainerSchema.Constants.TableName, new ZQuery());
			}

			foreach (KeyValuePair<BillOfLading, List<BillOfLadingContainer>> pair in list)
			{
				BillOfLading shipment = pair.Key;
				shipment.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, shipment.PK);
				shipment.Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, shipment.PK);
				shipment.Factory.AddFetchHint(OrgHeaderSchema.PK, shipment.JS_OH_DeliveryAgent);
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKOrigin);
				shipment.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKDestination);

				ZQuery jobFilter = new ZQuery();
				jobFilter.AddToFilter(JobHeaderSchema.JH_ParentID, shipment.PK);
				jobFilter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				shipment.Factory.AddFetchHint(JobHeaderSchema.Instance, jobFilter);

				foreach (BillOfLadingContainer container in pair.Value)
				{
					container.Factory.AddFetchHint(RefContainerSchema.PK, container.JC_RC);
					container.Factory.AddFetchHint(RefContainerStockSchema.R6_ContainerNum, container.JC_ContainerNum);
				}
			}

			List<ZGuid> voyagePKs = new List<ZGuid>();

			foreach (KeyValuePair<BillOfLading, List<BillOfLadingContainer>> pair in list)
			{
				AgencyShipment shipment = pair.Key;
				JobDocAddress address = shipment.ConsigneeDocumentaryAddress;

				if (address != null && !address.E2_AddressOverride)
				{
					shipment.Factory.AddFetchHint(OrgAddressSchema.PK, address.E2_OA_Address);
					consignees.Add(shipment.ConsigneeDocumentaryAddress);
				}

				foreach (Transport transport in shipment.TransportsIncludingRelated)
				{
					transport.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, transport.JW_RL_NKLoadPort);
					transport.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, transport.JW_RL_NKDiscPort);

					JobVoyage voyage = transport.Voyage;

					if (voyage != null)
					{
						voyagePKs.Add(voyage.PK);
					}
				}

				if (voyagePKs.Count > 0)
				{
					foreach (BillOfLadingContainer container in pair.Value)
					{
						RefContainerStock stock = container.Stock;

						if (stock != null && stock.IsInDatabase)
						{
							ZQuery filter = new ZQuery();
							filter.AddToFilter(JobContainerMoveSchema.E9_JV, voyagePKs);
							filter.AddToFilter(JobContainerMoveSchema.E9_R6, stock.PK);

							container.Factory.AddFetchHint(JobContainerMoveSchema.Instance, filter);
						}
					}

					voyagePKs.Clear();
				}
			}

			foreach (JobDocAddress address in consignees)
			{
				OrgAddress orgAddress = address.Address;

				if (orgAddress != null)
				{
					orgAddress.Factory.AddFetchHint(OrgHeaderSchema.PK, orgAddress.OA_OH);
				}
			}
		}

		static IEnumerable<BusinessObjectFactory> ExtractFactories(IEnumerable<KeyValuePair<BillOfLading, List<BillOfLadingContainer>>> list)
		{
			Dictionary<BusinessObjectFactory, bool> lookup = new Dictionary<BusinessObjectFactory, bool>();

			foreach (KeyValuePair<BillOfLading, List<BillOfLadingContainer>> pair in list)
			{
				lookup[pair.Key.Factory] = true;
			}

			return lookup.Keys;
		}
	}
}
