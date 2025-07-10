using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Integration;
using static Enterprise.Freight.Integration.IGlobalCommercialInvoiceComplianceProcessor;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	/// <summary>
	/// Global Commercial Invoice Business Object.
	/// Contains the headers and lines for a Global Commercial Invoice.
	/// It is a non-persistent business object, however the headers and lines are persistent.
	/// </summary>
	public class GlobalCommercialInvoiceBusinessObject : NonPersistentBusinessObject, IGlobalCommercialInvoiceComplianceProvider
	{
		/// <summary>
		/// The host (parent) business object.
		/// Normally a Shipment or Booking.
		/// </summary>
		public BusinessObject HostBusinessObject { get; }

		/// <summary>
		/// The header collection for the Global Commercial Invoice.
		/// </summary>
		public GlobalCommercialInvoiceHeaderCollection Headers { get; }

		/// <summary>
		/// The line collection for the Global Commercial Invoice.
		/// </summary>
		public GlobalCommercialInvoiceLineIntegratedCollection Lines { get; }

		/// <summary>
		/// Instantiates a new Global Commercial Invoice Business Object.
		/// </summary>
		/// <param name="hostBusinessEntity">The host (parent) business object. Normally a Shipment or Booking.</param>
		public GlobalCommercialInvoiceBusinessObject(IBusiness hostBusinessEntity)
			: this(hostBusinessEntity, null, null)
		{
		}

		/// <summary>
		/// Instantiates a new Global Commercial Invoice Business Object
		/// with provided header and line collections.
		/// </summary>
		/// <param name="hostBusinessEntity">The host (parent) business object. Normally a Shipment or Booking.</param>
		/// <param name="headers">The provided header collection for the Global Commercial Invoice.</param>
		/// <param name="lines">The provided line collection for the Global Commercial Invoice.</param>
		public GlobalCommercialInvoiceBusinessObject(
			IBusiness hostBusinessEntity,
			GlobalCommercialInvoiceHeaderCollection headers,
			GlobalCommercialInvoiceLineIntegratedCollection lines)
		{
			if (hostBusinessEntity is IGlobalCommercialInvoiceJobProvider provider)
			{
				HostBusinessObject = (BusinessObject)hostBusinessEntity;

				Headers = headers ?? new GlobalCommercialInvoiceHeaderCollection(HostBusinessObject.Factory, provider.ParentID, provider.ParentTableCode);
				Lines = lines ?? new GlobalCommercialInvoiceLineIntegratedCollection(HostBusinessObject, Headers);

				HostBusinessObject.RegisterEditableChildObject(Headers);
				HostBusinessObject.RegisterEditableChildObject(Lines);

				Headers.CollectionCountChange += new CollectionCountChangedEventHandler(RecalculateCompliancePartiesLocations_HeadersCountChange);
				Lines.CollectionCountChange += new CollectionCountChangedEventHandler(RecalculateComplianceCommodities_LinesCountChange);
			}
			else
			{
				throw new ArgumentException($"The host business entity must implement {nameof(IGlobalCommercialInvoiceProvider)}.", nameof(hostBusinessEntity));
			}
		}

		#region Compliance Integration

		/// <summary>
		/// We ensure the invoice header information propagates with the compliance risks for all parties involved.
		/// </summary>
		IEnumerable<IScreeningParty> IGlobalCommercialInvoiceComplianceProvider.Parties
		{
			get
			{
				if (parties is null || Headers.Any(header => header.HasChanges))
				{
					parties = GlobalCommercialInvoiceComplianceProcessor.GetParties(HostBusinessObject, Headers);
				}
				return parties;
			}
		}
		IEnumerable<IScreeningParty> parties;

		/// <summary>
		/// We ensure the invoice header information propagates with the compliance risks for all locations involved.
		/// </summary>
		IEnumerable<IComplianceLocation> IGlobalCommercialInvoiceComplianceProvider.Locations
		{
			get
			{
				if (locations is null || Headers.Any(header => header.HasChanges))
				{
					locations = GlobalCommercialInvoiceComplianceProcessor.GetLocations(HostBusinessObject, Headers);
				}
				return locations;
			}
		}
		IEnumerable<IComplianceLocation> locations;

		/// <summary>
		/// We ensure the invoice line tariff information propagates with the compliance risks for all commodities involved.
		/// </summary>
		IEnumerable<IComplianceCommodity> IGlobalCommercialInvoiceComplianceProvider.Commodities
		{
			get
			{
				if (commodities is null || Lines.Any(line => line.HasChanges) || Headers.Any(header => header.HasChanges))
				{
					commodities = GlobalCommercialInvoiceComplianceProcessor.GetCommodities(HostBusinessObject, Headers, Lines);
				}
				return commodities;
			}
		}
		IEnumerable<IComplianceCommodity> commodities;

		void RecalculateCompliancePartiesLocations_HeadersCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			parties = null;
			locations = null;
		}

		void RecalculateComplianceCommodities_LinesCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			commodities = null;
		}

		#endregion
	}
}
