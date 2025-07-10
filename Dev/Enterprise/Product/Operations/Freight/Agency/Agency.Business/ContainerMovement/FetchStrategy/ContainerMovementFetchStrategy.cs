using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class ContainerMovementFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ContainerMovementFetchStrategy(ContainerMovement movement)
			: base(movement) { }

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var requireRefContainerStock = false;
			var requireDepotAddress = false;
			var requireJobVoyage = false;
			var requireResponsibleParty = false;
			var requirePrincipal = false;
			var requireJobContainerDetention = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case ContainerMovement.Schema.DepotPort:
						requireDepotAddress = true;
						break;

					case ContainerMovement.Schema.E9_DetentionCompanyCode:
						requireJobContainerDetention = true;
						break;

					case "RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.LocalClient: // hard-coded constant
						requireRefContainerStock = true;
						requireResponsibleParty = true;
						break;

					case "RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.Principal: // hard-coded constant
						requireRefContainerStock = true;
						requirePrincipal = true;
						break;

					case "RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.ExportDetentionFreeDays: // hard-coded constant
					case "RelatedInfo+" + AutoContainerMovementRelatedInfo.Schema.ImportDetentionFreeDays: // hard-coded constant
						requireRefContainerStock = true;
						requireDepotAddress = true;
						requireResponsibleParty = true;
						requirePrincipal = true;
						break;

					default:
						if (column.ColumnName.StartsWith("Stock+", StringComparison.OrdinalIgnoreCase) || column.ColumnName.StartsWith("RelatedInfo+", StringComparison.OrdinalIgnoreCase)) // hard-coded constant
						{
							requireRefContainerStock = true;
						}

						if (column.ColumnName.StartsWith("Voyage+", StringComparison.OrdinalIgnoreCase)) // hard-coded constant
						{
							requireJobVoyage = true;
						}

						if (column.ColumnName.StartsWith(ContainerMovement.Schema.E9_OA_Depot, StringComparison.OrdinalIgnoreCase))
						{
							requireDepotAddress = true;
						}
						break;
				}
			}

			var movement = (ContainerMovement)BusinessObject;

			if (requireRefContainerStock)
			{
				Factory.AddFetchHint(RefContainerStockSchema.PK, movement.E9_R6);
			}

			if (requireDepotAddress)
			{
				Factory.AddFetchHint(OrgAddressSchema.PK, movement.E9_OA_Depot);
			}

			if (requireJobContainerDetention)
			{
				Factory.AddFetchHint(JobContainerDetentionSchema.PK, movement.E9_NC);
			}

			if (requireJobVoyage)
			{
				Factory.AddFetchHint(JobVoyageSchema.PK, movement.E9_JV);
			}

			if (requireResponsibleParty && !movement.E9_OH_ResponsibleParty.IsEmpty)
			{
				Factory.AddFetchHint(OrgHeaderSchema.PK, movement.E9_OH_ResponsibleParty);
			}

			if (requirePrincipal && !movement.E9_OH_Principal.IsEmpty)
			{
				Factory.AddFetchHint(OrgHeaderSchema.PK, movement.E9_OH_Principal);
			}
		}
	}
}


