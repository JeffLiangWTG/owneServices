using System;
using System.Globalization;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	internal static class HyperlinkHelper
	{
		public static LogHyperlink Link(BillOfLading shipment)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment));
			}

			return new LogControllerLink(shipment.JS_UniqueConsignRef, ControllerIDs.AgencyBillOfLading, shipment.PK);
		}

		public static LogHyperlink Link(BillOfLadingContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			return new LogControllerLink(container.JC_ContainerCode, ControllerIDs.AgencyBillContainers, container.PK);
		}

		public static LogHyperlink Link(RefContainerStock stock)
		{
			if (stock == null)
			{
				throw new ArgumentNullException(nameof(stock));
			}

			return new LogControllerLink(stock.R6_ContainerNum, ControllerIDs.AgencyContainerManager, stock.PK);
		}

		public static LogHyperlink Link(ContainerMovement movement)
		{
			if (movement == null)
			{
				throw new ArgumentNullException(nameof(movement));
			}

			string description = movement.Lookups.MovementCodeList.GetDescriptionFromCode(movement.E9_MovementType);

			if (string.IsNullOrEmpty(description))
			{
				description = movement.E9_MovementType;

				if (string.IsNullOrEmpty(description))
				{
					description = "<EMPTY>";
				}
			}

			return new LogControllerLink(description, ControllerIDs.AgencyContainerMove, movement.PK);
		}

		public static LogHyperlink Link(ContainerDetention detention)
		{
			if (detention == null)
			{
				throw new ArgumentNullException(nameof(detention));
			}

			string text;

			if (detention.NC_GC == GlbCompany.CurrentCompany.PK)
			{
				text = detention.NC_JobNumber;
			}
			else
			{
				text = string.Format(CultureInfo.InvariantCulture, "{0}({1})", detention.NC_JobNumber, detention.Company.GC_Code);
			}

			return new LogControllerLink(text, ControllerIDs.AgencyContainerDetention, detention.PK);
		}
	}
}


