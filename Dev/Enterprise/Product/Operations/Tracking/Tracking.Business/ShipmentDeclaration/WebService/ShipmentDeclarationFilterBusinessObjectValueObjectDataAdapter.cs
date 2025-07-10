using System;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Module;
using Enterprise.Tracking.Business.ShipmentDeclaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Xml Data adapter for ShipDec filter business object
	/// </summary>
	public class ShipentDeclarationFilterValueObjectDataAdapter : ValueObjectDataAdapter<TrackingShipmentFilterBusinessObject, WebShipmentFilter>
	{
		protected override Type ValueObjectCollectionType
		{
			get { return typeof(WebShipmentFilterCollection); }
		}

		public override string RootCollectionElementName
		{
			get { return "WebShipmentFilters"; }
		}

		public override string RootElementName
		{
			get { return "WebShipmentFilter"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebShipmentFilterSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebShipmentFilterSchema; }
		}

		protected override TrackingShipmentFilterBusinessObject FindBusinessObject(WebShipmentFilter value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Not supported");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard coded constant")]
		protected override void ImportFromValueObjectCore(TrackingShipmentFilterBusinessObject filterBizO, WebShipmentFilter value, IValueObjectImportContext context)
		{
			WebShipmentFilter xmlResult = value;
			const string errorContext = "Import WebShipmentFilter into TrackingShipmentFilterBusinessObject";

			if (xmlResult.Number != null && xmlResult.NumberSpecified)
			{
				string numberFilterCode = TrackingShipmentFilterBizOToXmlMappings.Instance.GetEnterpriseCode(xmlResult.Number.NumberSearchField.ToString(), errorContext, context);

				if (numberFilterCode == TrackingShipmentFilterBizOToXmlMappings.AllFilters)
				{
					filterBizO.RemoveExclusivity = true;
					foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.AllNumberFilters)
					{
						ActivateFilter(filterBizO, description, xmlResult.Number, FilterOrCategory.Blue);
					}
				}
				else if (numberFilterCode == TrackingShipmentFilterBizOToXmlMappings.MostCommonNumberFilters)
				{
					foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.MostCommonNumberFilterList)
					{
						ActivateFilter(filterBizO, description, xmlResult.Number, FilterOrCategory.Blue);
					}
				}
				else if (!string.IsNullOrEmpty(numberFilterCode))
				{
					ActivateFilter(filterBizO, numberFilterCode, xmlResult.Number, FilterOrCategory.None);
				}
			}

			if (xmlResult.StatusSpecified)
			{
				string statusFilterCode = ShipmentStatusListToXmlCodeMappings.Instance.GetEnterpriseCode(xmlResult.Status, errorContext, context);
				((ModuleTextFilter)filterBizO[TrackingShipmentFilterBusinessObject.Descriptions.Status]).Property = statusFilterCode;
				filterBizO[TrackingShipmentFilterBusinessObject.Descriptions.Status].IsActive = true;
			}

			if (xmlResult.Date != null && xmlResult.DateSpecified)
			{
				string dateFilterCode = TrackingShipmentFilterBizOToXmlMappings.Instance.GetEnterpriseCode(xmlResult.Date.DateSearchField.ToString(), errorContext, context);

				if (dateFilterCode == TrackingShipmentFilterBizOToXmlMappings.AllFilters)
				{
					foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.AllDateFilters)
					{
						ActivateFilter(filterBizO, description, xmlResult.Date, FilterOrCategory.Brown);
					}
				}
				else if (!string.IsNullOrEmpty(dateFilterCode))
				{
					ActivateFilter(filterBizO, dateFilterCode, xmlResult.Date, FilterOrCategory.None);
				}
			}

			if (xmlResult.Organisation != null && xmlResult.OrganisationSpecified)
			{
				string orgFilterCode = TrackingShipmentFilterBizOToXmlMappings.Instance.GetEnterpriseCode(xmlResult.Organisation.OrganisationSearchField.ToString(), errorContext, context);
				BusinessHelper helper = new BusinessHelper(filterBizO.Factory);

				if (orgFilterCode == TrackingShipmentFilterBizOToXmlMappings.AllFilters)
				{
					foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.AllOrganizationFilters)
					{
						ActivateFilter(filterBizO, description, xmlResult.Organisation, FilterOrCategory.Green, helper);
					}
				}
				else if (orgFilterCode == FreightConstants.OrgFilterTypes.SendingRecvAgent)
				{
					foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.AnyAgentFilterList)
					{
						ActivateFilter(filterBizO, description, xmlResult.Organisation, FilterOrCategory.Green, helper);
					}
				}
				else if (!string.IsNullOrEmpty(orgFilterCode))
				{
					ActivateFilter(filterBizO, orgFilterCode, xmlResult.Organisation, FilterOrCategory.None, helper);
				}
			}

			if (xmlResult.Location != null && xmlResult.LocationSpecified)
			{
				string locationCode = TrackingShipmentFilterBizOToXmlMappings.Instance.GetEnterpriseCode(xmlResult.Location.LocationSearchField.ToString(), errorContext, context);

				if (locationCode == TrackingShipmentFilterBizOToXmlMappings.AllFilters)
				{
					foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.AllLocationFilters)
					{
						ActivateFilter(filterBizO, description, xmlResult.Location, FilterOrCategory.Grey);
					}
				}
				else if (!string.IsNullOrEmpty(locationCode))
				{
					ActivateFilter(filterBizO, locationCode, xmlResult.Location, FilterOrCategory.None);
				}
			}

			if (xmlResult.VoyageFlightSpecified && !xmlResult.VoyageFlight.IsEmpty)
			{
				((VoyageVesselModuleFilter)filterBizO[JobShipmentFilterBusinessObject.Descriptions.FlightVoyageAndVessel]).VoyageFlightNo = xmlResult.VoyageFlight;
				filterBizO[JobShipmentFilterBusinessObject.Descriptions.FlightVoyageAndVessel].IsActive = true;
			}

			if (xmlResult.VesselSpecified && !xmlResult.Vessel.IsEmpty)
			{
				((VoyageVesselModuleFilter)filterBizO[JobShipmentFilterBusinessObject.Descriptions.FlightVoyageAndVessel]).Vessel = xmlResult.Vessel;
				filterBizO[JobShipmentFilterBusinessObject.Descriptions.FlightVoyageAndVessel].IsActive = true;
			}
		}

		ModuleFilter GetFilterWithOrCategory(TrackingShipmentFilterBusinessObject filterBizO, string description, FilterOrCategory category)
		{
			FilterStrip strip = new FilterStrip(filterBizO.ModuleFilters);
			strip.FilterDescription = description;
			strip.OrCategory = category;
			return strip.CurrentModuleFilter;
		}

		void ActivateFilter(TrackingShipmentFilterBusinessObject filterBizO, string description, WebShipmentFilterDate date, FilterOrCategory orCategory)
		{
			if (filterBizO[description] != null && (date.FromDateSpecified || date.ToDateSpecified))
			{
				ModuleDateFilter filter = (ModuleDateFilter)GetFilterWithOrCategory(filterBizO, description, orCategory);
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				if (date.FromDateSpecified)
				{
					filter.Property1 = date.FromDate;
				}
				if (date.ToDateSpecified)
				{
					filter.Property2 = date.ToDate;
				}
				filter.IsActive = true;
			}
		}

		void ActivateFilter(TrackingShipmentFilterBusinessObject filterBizO, string description, WebShipmentFilterNumber number, FilterOrCategory orCategory)
		{
			if (filterBizO[description] != null && number.NumberValueSpecified)
			{
				ModuleNumberFilter filter = (ModuleNumberFilter)GetFilterWithOrCategory(filterBizO, description, orCategory);
				filter.Property = number.NumberValue;
				filter.IsActive = true;
			}
		}

		void ActivateFilter(TrackingShipmentFilterBusinessObject filterBizO, string description, WebShipmentFilterOrganisation org, FilterOrCategory orCategory, BusinessHelper helper)
		{
			if (filterBizO[description] != null && (org.Organisation1Specified || org.Organisation2Specified))
			{
				ModuleFilter filter = GetFilterWithOrCategory(filterBizO, description, orCategory);

				if (filter is ModuleGuidsFilter)
				{
					if (org.Organisation1Specified)
					{
						((ModuleGuidsFilter)filter).Property1 = helper.GetPKFromOrgCode(org.Organisation1);
					}
					if (org.Organisation2Specified)
					{
						((ModuleGuidsFilter)filter).Property2 = helper.GetPKFromOrgCode(org.Organisation2);
					}
				}
				if (filter is ModuleGuidFilter && org.Organisation1Specified)
				{
					((ModuleGuidFilter)filter).Property = helper.GetPKFromOrgCode(org.Organisation1);
				}
				filter.IsActive = true;
			}
		}

		void ActivateFilter(TrackingShipmentFilterBusinessObject filterBizO, string description, WebShipmentFilterLocation location, FilterOrCategory orCategory)
		{
			if (filterBizO[description] != null && (location.Location1Specified || location.Location2Specified))
			{
				ModuleLocationFilter filter = (ModuleLocationFilter)GetFilterWithOrCategory(filterBizO, description, orCategory);
				if (location.Location1Specified)
				{
					filter.Property1 = location.Location1.Value;
				}
				if (location.Location2Specified)
				{
					filter.Property2 = location.Location2.Value;
				}
				filter.IsActive = true;
			}
		}

		protected override void ExportToValueObjectCore(TrackingShipmentFilterBusinessObject bizObj, WebShipmentFilter constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Not supported");
		}
	}
}
