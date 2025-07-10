using System;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Xml serialisation data adapter for consols collection on Tracking Shipment
	/// </summary>
	public class TrackingConsolValueObjectDataAdapter : ValueObjectDataAdapter<TrackingConsol, Xsd.WebConsol>
	{
		protected override Type ValueObjectCollectionType
		{
			get { return typeof(Xsd.WebConsolCollection); }
		}

		public override string RootCollectionElementName
		{
			get { return "WebConsols"; }
		}

		public override string RootElementName
		{
			get { return "WebConsol"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebConsolSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebConsolsSchema; }
		}

		protected override TrackingConsol FindBusinessObject(Xsd.WebConsol value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(TrackingConsol bizObj, Xsd.WebConsol value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public override TrackingConsol CreateOrUpdateFromValueObject(Xsd.WebConsol value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public Xsd.WebConsolCollection ExportToValueObjectCollection(TrackingConsolManyToManyCollection collection, IValueObjectExportContext context)
		{
			return (Xsd.WebConsolCollection)ToValueObjectCollection(collection, context);
		}

		protected override void ExportToValueObjectCore(TrackingConsol consol, Xsd.WebConsol result, IValueObjectExportContext context)
		{
			if (consol != null)
			{
				if (!consol.JK_ConsolMode.IsEmpty)
				{
					result.ConsolMode = consol.JK_ConsolMode;
				}

				if (consol.VoyOrigin != null)
				{
					result.LoadPort = Xsd.UNLOCO.FromPort(consol.VoyOrigin.PortOfLoading);
				}

				if (consol.VoyDestination != null)
				{
					result.DischargePort = Xsd.UNLOCO.FromPort(consol.VoyDestination.PortOfDischarge);
				}

				if (!consol.JK_MasterBillNum.IsEmpty)
				{
					result.MasterBill = consol.JK_MasterBillNum;
				}

				if (!consol.JK_TransportMode.IsEmpty)
				{
					result.TransportMode = consol.JK_TransportMode;
				}

				if (!consol.JK_JX_JV_NKVessel.IsEmpty)
				{
					result.VesselName = consol.JK_JX_JV_NKVessel;
				}

				if (!consol.JK_JX_JV_VoyageFlight.IsEmpty)
				{
					result.VoyageFlight = consol.JK_JX_JV_VoyageFlight;
				}

				if (consol.Transports.DepartureTransport != null &&
						!consol.Transports.DepartureTransport.JW_ATD.IsEmpty)
				{
					result.ATD = consol.Transports.DepartureTransport.JW_ATD;
				}

				if (consol.Transports.ArrivalTransport != null &&
						!consol.Transports.ArrivalTransport.JW_ATA.IsEmpty)
				{
					result.ATA = consol.Transports.ArrivalTransport.JW_ATA;
				}
			}
		}
	}
}
