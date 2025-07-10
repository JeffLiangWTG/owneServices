using System;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class BulkMovementsApplicator : AutoBulkMovementsApplicator
	{
		public BulkMovementsApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("381a04b7-74cd-4ee1-a112-8bb1181b287a", "Bulk Movements"), factory) { }

		[List("Lookups.MovementTypes")]
		public override ZString MovementType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.MovementType; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.MovementType = value;
				DepotAddressPK_ZAddress.DefaultAddressType = ContainerMovementTypes.GetDefaultAddressType(value);
			}
		}

		public OrgAddress DepotAddress
		{
			get { return Factory.Load<OrgAddress>(DepotAddressPK); }
		}

		protected override ZString GetDepotPort()
		{
			OrgAddress address = DepotAddress;

			if (address != null)
			{
				RefUNLOCO port = address.EffectiveRelatedPortCode;

				return port == null ? ZString.Empty : port.RL_Code;
			}

			return ZString.Empty;
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);

			foreach (BusinessObject bo in targets)
			{
				RefContainerStock stock;
				BillOfLadingContainer container;

				if ((stock = bo as RefContainerStock) != null)
				{
					AddMovementToStock(log, stock, null);
				}
				else if ((container = bo as BillOfLadingContainer) != null)
				{
					AddMovementToContainer(log, container);
				}

				log.BumpSectionProgress();
			}
		}

		void AddMovementToContainer(IOperationalActionSectionLog log, BillOfLadingContainer container)
		{
			BillOfLading shipment = container.Booking;
			RefContainerStock stock;
			JobVoyage voyage = FindVoyage(log, shipment);

			if ((stock = container.Stock) != null)
			{
				AddMovementToStock(log, stock, voyage);
			}
			else if (container.JC_ContainerNum.IsEmpty)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error,
					Res.GetString("9f57a716-d2f0-4897-8cc6-5de6819e0310", "Found a {1} without a container number."),
					null,
					new LogControllerLink(Res.GetString("90db2f0f-4a01-4760-b48e-9e8762671b85", "container"), ControllerIDs.AgencyBillContainers, container.PK));
			}
			else if (!ContainerNumberValidation.IsValidContainerNumber(container.JC_ContainerNum) &&
					!AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.Value)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("a583e569-0f02-47cc-ba0b-df53cdad9ec6", "{1} is not a valid container number."), null, HyperlinkHelper.Link(container));
			}
			else if (container.JC_RC.IsEmpty)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("1fb329b5-c09d-4106-b36f-4c54814e861c", "{1} does not have a container type."), null, HyperlinkHelper.Link(container));
			}
			else
			{
				stock = container.Factory.New<RefContainerStock>();
				stock.R6_ContainerNum = container.JC_ContainerNum;
				stock.R6_RC = container.JC_RC;

				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("1c16db7f-8dae-4a78-84d7-7c8d8edddb15", "Adding {1} to the Container Manager Module"), null, HyperlinkHelper.Link(stock));

				AddMovementToStock(log, stock, voyage);
			}
		}

		void AddMovementToStock(IOperationalActionSectionLog log, RefContainerStock stock, JobVoyage voyage)
		{
			ContainerMovement duplicateMovement = ContainerMovementHelper.FindDuplicate(stock, MovementType, MovementDate);

			if (duplicateMovement != null)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					Res.GetString("addc57ee-4524-491f-aee7-b0f883afe724", "{1} already has a {2} movement at {3:dd-MMM-yy HH:mm}"),
					null, HyperlinkHelper.Link(stock), HyperlinkHelper.Link(duplicateMovement), MovementDate);
			}
			else
			{
				ContainerMovement movement = stock.Movements.AddNew();
				movement.E9_MovementType = MovementType;
				movement.E9_MovementDate = MovementDate;
				movement.E9_OA_Depot = DepotAddressPK;

				if (voyage != null)
				{
					movement.E9_JV = voyage.PK;
				}
			}
		}

		JobVoyage FindVoyage(IOperationalActionSectionLog log, BillOfLading shipment)
		{
			JobVoyage voyage = FindVoyage(shipment, DepotPort, ContainerMovementTypes.GetDirection(MovementType), MovementDate);

			if (voyage == null && shipment.Sailing != null)
			{
				voyage = shipment.Sailing.Voyage;
			}

			if (voyage == null)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("8d8df490-8c7f-494c-b9ae-029fe9cfa840", "No voyage found for {1}."), null, HyperlinkHelper.Link(shipment));
			}

			return voyage;
		}

		static JobVoyage FindVoyage(BillOfLading shipment, ZString port, ZString direction, ZDateTime movementDate)
		{
			// Same Port        4
			// Same Country     2
			// Same Direction   1
			// If all else equal, pick the port with the closer estimated date.

			int rank = 0;
			ZDateTime portDate = ZDateTime.Empty;
			JobSailing sailing = null;

			int originBonus;
			int destinationBonus;

			switch (direction)
			{
				case MovementDirection.Codes.Departure:
					originBonus = 1;
					destinationBonus = 0;
					break;

				case MovementDirection.Codes.Arrival:
					originBonus = 0;
					destinationBonus = 1;
					break;

				default:
					originBonus = 0;
					destinationBonus = 0;
					break;
			}

			foreach (Transport leg in shipment.TransportsIncludingRelated)
			{
				int newRank;
				JobSailing newsailing = leg.Sailing;

				if (newsailing == null)
				{
					continue;
				}

				newRank = RankPort(newsailing.JX_JA_RL_NKPortOfLoading, port) + originBonus;

				if (newRank > rank)
				{
					rank = newRank;
					sailing = newsailing;
					portDate = newsailing.JX_JA_E_DEP;
				}
				else if (newRank == rank && IsCloserThanTo(newsailing.JX_JA_E_DEP, portDate, movementDate))
				{
					sailing = newsailing;
					portDate = newsailing.JX_JA_E_DEP;
				}

				newRank = RankPort(newsailing.JX_JB_RL_NKPortOfDischarge, port) + destinationBonus;

				if (newRank > rank)
				{
					rank = newRank;
					sailing = newsailing;
					portDate = newsailing.JX_JB_E_ARV;
				}
				else if (newRank == rank && IsCloserThanTo(newsailing.JX_JB_E_ARV, portDate, movementDate))
				{
					sailing = newsailing;
					portDate = newsailing.JX_JB_E_ARV;
				}
			}

			return sailing == null ? null : sailing.Voyage;
		}

		static int RankPort(ZString port, ZString reqPort)
		{
			if (port.Length != 5 || reqPort.Length != 5)
			{
				return 0;
			}
			else if (port == reqPort)
			{
				return 4;
			}
			else if (port.Left(2) == reqPort.Left(2))
			{
				return 2;
			}
			else
			{
				return 0;
			}
		}

		static bool IsCloserThanTo(ZDateTime firstDate, ZDateTime secondDate, ZDateTime referenceDate)
		{
			if (referenceDate.IsEmpty || firstDate.IsEmpty)
			{
				return false;
			}
			else if (secondDate.IsEmpty)
			{
				return true;
			}
			else
			{
				long diff1 = Math.Abs((firstDate - referenceDate).Ticks);
				long diff2 = Math.Abs((secondDate - referenceDate).Ticks);
				return diff1 < diff2;
			}
		}

		public BulkMovementsApplicatorLookups Lookups
		{
			get { return lookups ?? (lookups = new BulkMovementsApplicatorLookups(this)); }
		}
		BulkMovementsApplicatorLookups lookups;
	}
}
