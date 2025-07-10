using System;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class VesselRoutingVoyageDataAdapter : ValueObjectDataAdapter<VesselRoutingVoyage, Xsd.Schedule>
	{
		public VesselRoutingVoyageDataAdapter(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		#region ValueObjectDataAdapter Overrides

		public override string RootCollectionElementName
		{
			get { return (NoResString)"Schedules"; }
		}

		public override string RootElementName
		{
			get { return (NoResString)"Schedule"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SchedulesSchema; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleScheduleSchema; }
		}

		protected override void ImportFromValueObjectCore(VesselRoutingVoyage bizObj, Xsd.Schedule value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(VesselRoutingVoyage voyage, Xsd.Schedule voyageValue, IValueObjectExportContext context)
		{
			voyageValue.Carrier = new OrganisationValueObjectDataAdapter(OrganisationTypes.Carrier).ExportToValueObject(voyage.LineOperator, context);
			voyageValue.TransportMode = Xsd.TransportMode.SEA;

			Xsd.ScheduleSailing sailing = new Xsd.ScheduleSailing();
			RefVessel vessel = voyage.GetVessel(Factory);
			sailing.VesselName = (vessel == null) ? ZString.Empty : vessel.RV_Name;
			sailing.VoyageNo = voyage.E8_Voyage;
			voyageValue.Item = sailing;

			foreach (VesselRoutingPortPair portPair in voyage.PortPairs)
			{
				if (portPair.E9_IsSelected)
				{
					Xsd.SailingWithLoadDischargePorts portPairValue = sailing.Sailings.AddNew();
					ExportPortPair(portPair, portPairValue, context);
				}
			}
		}

		void ExportPortPair(VesselRoutingPortPair portPair, Xsd.SailingWithLoadDischargePorts portPairValue, IValueObjectExportContext context)
		{
			portPairValue.DepartureReference = portPair.E9_DataProviderReference;
			portPairValue.LoadPort = portPair.E9_RL_NKLoadPort;
			portPairValue.DischargePort = portPair.E9_RL_NKDischargePort;
			portPairValue.IsPublished = portPair.E9_Publish;
			portPairValue.IsPublishedSpecified = true;

			portPairValue.ETD = portPair.E9_ETD;
			portPairValue.ATD = portPair.E9_ATD;
			portPairValue.ETA = portPair.E9_ETA;
			portPairValue.ATA = portPair.E9_ATA;

			portPairValue.FCLDates.CutOffDate = portPair.E9_CargoCutOff;
			portPairValue.FCLDates.ReceivalCommencesDate = portPair.E9_ExportReceivalCommences;
			portPairValue.FCLDates.AvailableDate = portPair.E9_ImportAvailability;
			portPairValue.FCLDates.StorageDate = portPair.E9_ImportStorageCommences;
		}

		#endregion
	}
}
