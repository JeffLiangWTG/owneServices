using System.Collections.Generic;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;

namespace Enterprise.Tracking.Business
{
	public class ShipmentToDeclarationFilterHelper : FilterStripBOMappingHelper
	{
		public ShipmentToDeclarationFilterHelper(TrackingShipmentFilterBusinessObject shipmentFilterBO, JobDeclarationFilterBusinessObject declarationFilterBO)
			: base(shipmentFilterBO, declarationFilterBO)
		{
			conversions = new Dictionary<string, string>();
			exemptions = new List<string>();
		}

		public void MapFilters()
		{
			if (sourceFilterBO != null)
			{
				RegisterConversion((NoResString)"Shipment #", DeclarationFilterConstants.NumberFilterTypes.DeclarationReference);
				RegisterConversion((NoResString)"Booking Reference #", DeclarationFilterConstants.NumberFilterTypes.DeclarationReference);
				RegisterConversion((NoResString)"Commercial Invoice #", DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber);
				RegisterConversion((NoResString)"Order #", DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef);
				RegisterConversion((NoResString)"Active Status", TrackingDeclarationFilterConstants.ActiveStatus);
				RegisterConversion((NoResString)"Status", TrackingDeclarationFilterConstants.Status);
				RegisterConversion("ETA", DeclarationFilterConstants.DateFilterTypes.DateOfArrival);
				RegisterConversion("ETD", TrackingDeclarationFilterConstants.DateAtOrigin);
				RegisterConversion((NoResString)"First Port of Arrival Date", DeclarationFilterConstants.DateFilterTypes.FirstArrival);
				RegisterConversion((NoResString)"Goods Delivered", DeclarationFilterConstants.DateFilterTypes.GoodsDelivered);
				RegisterConversion((NoResString)"Domestic / International", TrackingDeclarationFilterConstants.DomesticInternational);
				RegisterConversion((NoResString)"Load / Discharge", DeclarationFilterConstants.PortFilterTypes.LoadDischarge);
				RegisterConversion((NoResString)"Origin / Destination", DeclarationFilterConstants.PortFilterTypes.OriginDestination);
				RegisterConversion((NoResString)"Consignee Company Name", TrackingDeclarationFilterConstants.ImporterCompanyName);
				RegisterConversion(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value + (NoResString)" Company Name", TrackingDeclarationFilterConstants.SupplierCompanyName);
				RegisterConversion((NoResString)"Shipment Send / Receive Forwarders", TrackingDeclarationFilterConstants.SendReceiveForwarders);
				RegisterConversion((NoResString)"Service Level", DeclarationFilterConstants.ServiceLevel);
				RegisterConversion((NoResString)"Created Time", TrackingDeclarationFilterConstants.CreatedTime);
				RegisterConversion((NoResString)"Last Edit Time", TrackingDeclarationFilterConstants.LastEditTime);
				RegisterConversion((NoResString)"Shipper's Reference #", DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef);
				RegisterConversion(TrackingDeclarationFilterConstants.DeclarationCountry, DeclarationFilterConstants.Country);

				// These are handled manually so are exempted
				exemptions.Add(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value + (NoResString)" / Consignee");
				MapModuleGuidsFiltersSwapped(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value + (NoResString)" / Consignee", DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier);
				RegisterMapModuleFilters<WorkflowModuleFilter>((NoResString)"Last Completed Milestone");
				RegisterMapModuleFilters<WorkflowModuleTextFilter>((NoResString)"Milestone Completed");
				RegisterMapModuleFilters<WorkflowModuleFilter>((NoResString)"Milestone Date");
				RegisterMapModuleFilters<WorkflowModuleFilter>((NoResString)"Next Milestone");

				destinationFilterBO.ReturnNoResultsQuery = false;

				foreach (ModuleFilter sourceFilter in sourceFilterBO)
				{
					if (sourceFilter.IsActive && !sourceFilter.IsEmpty)
					{
						ModuleFilter destinationFilter = destinationFilterBO[sourceFilter.Description];
						if (destinationFilter != null && destinationFilter.GetType().IsAssignableFrom(sourceFilter.GetType()))
						{
							destinationFilter.CopyTransientProperties(sourceFilter);
						}
						else
						{
							string destinationDescription;
							if (conversions.TryGetValue(sourceFilter.Description, out destinationDescription))
							{
								destinationFilter = destinationFilterBO[destinationDescription];
								if (destinationFilter != null && destinationFilter.GetType().IsAssignableFrom(sourceFilter.GetType()))
								{
									destinationFilter.CopyTransientProperties(sourceFilter);
								}
							}
							else
							{
								if (!exemptions.Contains(sourceFilter.Description) && (destinationFilter == null || destinationFilter.OrCategory == FilterOrCategory.None))
								{
									destinationFilterBO.ReturnNoResultsQuery = true;
									break;
								}
							}
						}
					}
				}
			}
		}

		readonly List<string> exemptions;
		readonly Dictionary<string, string> conversions;
		void RegisterConversion(string origin, string destination)
		{
			conversions.Add(origin, destination);
		}

		void RegisterMapModuleFilters<T>(string filterName)
			where T : ModuleFilter
		{
			exemptions.Add(filterName);
			MapModuleFilters<T>(filterName);
		}
	}
}
