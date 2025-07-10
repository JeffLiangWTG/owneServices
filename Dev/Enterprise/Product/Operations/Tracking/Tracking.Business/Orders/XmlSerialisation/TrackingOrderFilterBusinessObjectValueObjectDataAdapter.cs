using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Adapter to load Order filter business object from XML
	/// </summary>
	public class TrackingOrderFilterValueObjectDataAdapter : ValueObjectDataAdapter<TrackingOrderFilterBusinessObject_Old_ForWeb, Xsd.WebOrderFilter>
	{
		protected override Type ValueObjectCollectionType
		{
			get { return typeof(Xsd.WebOrderFilterCollection); }
		}

		public override string RootCollectionElementName
		{
			get { return "WebOrderFilters"; }
		}

		public override string RootElementName
		{
			get { return "WebOrderFilter"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebOrderFilterSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebOrderFilterSchema; }
		}

		protected override TrackingOrderFilterBusinessObject_Old_ForWeb FindBusinessObject(Xsd.WebOrderFilter value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Not supported");
		}

		public override TrackingOrderFilterBusinessObject_Old_ForWeb CreateOrUpdateFromValueObject(Xsd.WebOrderFilter value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(TrackingOrderFilterBusinessObject_Old_ForWeb bizObj, Xsd.WebOrderFilter value, IValueObjectImportContext context)
		{
			TrackingOrderFilterBusinessObject_Old_ForWeb filterBO = bizObj;
			if (filterBO != null)
			{
				Xsd.WebOrderFilter xmlResult = value;
				string errorContext = (NoResString)"TrackingOrder Filter business object"; // developer constant
				if (xmlResult.Number != null)
				{
					filterBO.JD_NumberFilterType = OrderNumberSearchFieldToXmlCodeMappings.Instance.GetEnterpriseCode(xmlResult.Number.NumberSearchField.ToString(), errorContext, context);
					filterBO.JD_Number = xmlResult.Number.NumberValue;
				}
				if (xmlResult.Date != null)
				{
					filterBO.JD_DateFilterType = OrderDateSearchFieldToXmlCodeMappings.Instance.GetEnterpriseCode(xmlResult.Date.DateSearchField.ToString(), errorContext, context);
					if (!xmlResult.Date.FromDate.IsEmpty)
					{
						filterBO.JD_FromDate = (ZDateTime)xmlResult.Date.FromDate;
					}

					if (!xmlResult.Date.ToDate.IsEmpty)
					{
						filterBO.JD_ToDate = (ZDateTime)xmlResult.Date.ToDate;
					}
				}
				if (xmlResult.Organisation != null)
				{
					filterBO.JD_OrgFilterType = OrderOrganisationSearchFieldToXmlCodeMappings.Instance.GetEnterpriseCode(xmlResult.Organisation.OrganisationSearchField.ToString(), errorContext, context);
					filterBO.Org1Code = xmlResult.Organisation.Organisation1;
					filterBO.Org2Code = xmlResult.Organisation.Organisation2;
				}
				if (xmlResult.Location != null)
				{
					filterBO.JD_PortFilterType = OrderLocationSearchFieldToXmlCodeMappings.Instance.GetEnterpriseCode(xmlResult.Location.LocationSearchField.ToString(), errorContext, context);
					if (xmlResult.Location.Location1 != null)
					{
						filterBO.JD_RL_NKPort1 = xmlResult.Location.Location1.Value;
					}

					if (xmlResult.Location.Location2 != null)
					{
						filterBO.JD_RL_NKPort2 = xmlResult.Location.Location2.Value;
					}
				}
				if (!xmlResult.VoyageFlight.IsEmpty)
				{
					filterBO.Voyage = xmlResult.VoyageFlight;
				}

				if (!xmlResult.Vessel.IsEmpty)
				{
					filterBO.Vessel = xmlResult.Vessel;
				}

				if (!xmlResult.OrderStatus.IsEmpty)
				{
					filterBO.JD_OrderStatus = xmlResult.OrderStatus.SubstringSafe(0, filterBO.JD_OrderStatusInfo.MaxLength);
				}
			}
		}

		protected override void ExportToValueObjectCore(TrackingOrderFilterBusinessObject_Old_ForWeb bizObj, Xsd.WebOrderFilter constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException();
		}
	}
}
