using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Freight.SailingScheduleDataVendor.Res;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class VesselRoutingVoyageImporter
	{
		public VesselRoutingVoyageImporter(VesselRoutingVoyage[] voyages)
		{
			this.Voyages = voyages;
		}

		#region Import

		public void Import(INotifications baseNotifications)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NotificationBuffer notifications = new NotificationBuffer(baseNotifications);

			ValueObjectImportContext importContext = new ValueObjectImportContext(factory, notifications);
			ImportToFactory(importContext);

			if (!notifications.HasErrors)
			{
				try
				{
					factory.Save();
					notifications.Notify(new InfoNotification(Res.GetString("64ffee99-fa19-4248-85c4-c7e963ed547e", "Sailing schedules imported successfully.")));
				}
				catch (ZSaveException ex)
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
				}
			}
		}

		protected void ImportToFactory(ValueObjectImportContext importContext)
		{
			bool @continue = EnsureVesselsExist(importContext);
			if (@continue)
			{
				foreach (VesselRoutingVoyage voyage in Voyages)
				{
					ImportVoyageToFactory(voyage, importContext);
				}
			}
		}

		#endregion

		#region ImportVoyageToFactory

		void ImportVoyageToFactory(VesselRoutingVoyage voyage, ValueObjectImportContext importContext)
		{
			string voyageHeading = Res.GetString("aa47c406-e6e1-40f6-9ac3-62985323e225", "Importing Voyage (");
			RefVessel vessel = voyage.GetVessel(importContext.Factory);
			if (vessel != null)
			{
				voyageHeading += Res.GetString("1f95f6c4-e502-4dae-8342-07785c0b61cf", "Vessel Name='{0}", vessel.RV_Name);
			}

			voyageHeading += Res.GetString("8ccde359-8e17-4b32-a8bf-ebc884d1e013", "' Voyage='{0}')", voyage.E8_Voyage);
			importContext.Notify(new InfoNotification(voyageHeading));
			importContext.Notify(new InfoNotification(new string('-', voyageHeading.Length)));

			Xsd.Schedule scheduleValue = new VesselRoutingVoyageDataAdapter(importContext.Factory).ExportToValueObject(voyage, new ValueObjectExportContext(importContext));
			new ScheduleValueObjectDataAdapterNoConfirmUserForUpdate().CreateOrUpdateFromValueObject(scheduleValue, importContext);

			importContext.Notify(new NewlineNotification());
		}

		class ScheduleValueObjectDataAdapterNoConfirmUserForUpdate : ScheduleValueObjectDataAdapter
		{
			protected override SailingValueObjectDataAdapter NewSailingDataAdapter(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vesselName, ZString voyageNo, ZGuid carrierPK)
			{
				return new SailingValueObjectDataAdapterNoConfirmUserForUpdate(transportMode, loadPort, dischargePort, vesselName, voyageNo, carrierPK);
			}
		}

		class SailingValueObjectDataAdapterNoConfirmUserForUpdate : SailingValueObjectDataAdapter
		{
			public SailingValueObjectDataAdapterNoConfirmUserForUpdate(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vesselName, ZString voyageNo, ZGuid carrierPK)
				: base(transportMode, loadPort, dischargePort, vesselName, voyageNo, carrierPK)
			{
			}

			protected override bool ConfirmUpdateOfExistingBusinessObject(JobSailing bizObj, INotifications notifications)
			{
				return true;
			}
		}

		#endregion

		#region EnsureVesselsExist

		bool EnsureVesselsExist(ValueObjectImportContext importContext)
		{
			bool actionTaken = false;
			foreach (VesselRoutingVoyage voyage in Voyages)
			{
				if (voyage.Vessel == null)
				{
					actionTaken = true;
				}
				if (!EnsureVesselExists(voyage, importContext))
				{
					return false;
				}
			}
			if (actionTaken)
			{
				importContext.Notify(new NewlineNotification());
			}
			return true;
		}

		bool EnsureVesselExists(VesselRoutingVoyage voyage, ValueObjectImportContext importContext)
		{
			bool result = true;
			RefVessel vessel = voyage.GetVessel(importContext.Factory);
			if (vessel == null)
			{
				RefVessel[] vessels = importContext.Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, voyage.E8_LloydsNumber));
				if (vessels.Length == 0 && !ExistsVesselWithName(importContext.Factory, voyage.E8_VesselName))
				{
					CreateNewVessel(voyage, importContext);
				}
				else
				{
					result = ResolveVesselNameAmbiguity(voyage, importContext);
				}
			}
			return result;
		}

		void CreateNewVessel(VesselRoutingVoyage voyage, ValueObjectImportContext importContext)
		{
			RefVessel newVessel = importContext.Factory.New<RefVessel>();
			newVessel.RV_Name = voyage.E8_VesselName;
			newVessel.RV_LloydsNumber = voyage.E8_LloydsNumber;
			importContext.Notify(new InfoNotification(Res.GetString("a5060b25-a613-4a17-9847-ffc0e81060c4", "Vessel '{0}' with Lloyds number '{1}' created", newVessel.RV_Name, newVessel.RV_LloydsNumber)));
		}

		bool ResolveVesselNameAmbiguity(VesselRoutingVoyage voyage, ValueObjectImportContext importContext)
		{
			QueryUserSelectVesselFromLloydsNumber vesselSelect = new QueryUserSelectVesselFromLloydsNumber(importContext.Factory, voyage.E8_VesselName, voyage.E8_LloydsNumber);
			importContext.QueryUser(vesselSelect);

			bool result = true;
			if (vesselSelect.SelectedVessel != null)
			{
				vesselSelect.SelectedVessel.RV_Name = voyage.E8_VesselName;
				vesselSelect.SelectedVessel.RV_LloydsNumber = voyage.E8_LloydsNumber;
				importContext.Notify(new InfoNotification(Res.GetString("f31d4681-6649-4399-bd27-be60b51e6eed", "Vessel '{0}' was chosen as the match for Lloyds number '{1}'", vesselSelect.SelectedVessel.RV_Name, vesselSelect.SelectedVessel.RV_LloydsNumber)));
			}
			else
			{
				importContext.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("3353282b-d27c-458b-881c-a79695c856bd", "The import was canceled at the user's request")));
				result = false;
			}
			return result;
		}

		bool ExistsVesselWithName(BusinessObjectFactory factory, ZString vesselName)
		{
			return RefVessel.LookupVesselByName(vesselName, factory).FirstOrDefault() != null;
		}

		#endregion

		#region Implementation

		readonly VesselRoutingVoyage[] Voyages;

		#endregion
	}
}
