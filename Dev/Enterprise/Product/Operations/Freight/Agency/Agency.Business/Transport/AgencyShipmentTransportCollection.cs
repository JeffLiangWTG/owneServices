using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyShipmentTransportCollection : TransportCollection
	{
		public AgencyShipmentTransportCollection(AgencyShipment shipment)
			: base(shipment)
		{
			this.shipment = shipment;
			this.shipment.JS_JXInfo.ValueChanged += JS_JXInfo_ValueChanged;
			this.CountChanged += AgencyShipmentTransportCollection_CountChanged;
		}

		void AgencyShipmentTransportCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			shipment.DefaultSailing();
		}

		void JS_JXInfo_ValueChanged(object sender, EventArgs e)
		{
			if (mainTransport != null && mainTransport.IsDeleted)
			{
				mainTransport = null;
			}

			ISupportDataImporting supportDataImporting = shipment;

			if (!supportDataImporting.IsImportingData)
			{
				SetMainSailing(shipment.JS_JX);
			}
		}

		public override void Load()
		{
			using (SuspendCountChanged(args => OnCountChanged(new CollectionCountChangedEventArgs(false, null))))
			using (shipment.SuspendSettingHasChanges())
			{
				base.Load();
				SetMainSailing(shipment.JS_JX, true);
			}
		}

		void SetMainSailing(ZGuid sailingPK, bool suspendSettingHasChanges = false)
		{
			if (sailingPK.IsValid)
			{
				if (mainTransport == null)
				{
					mainTransport = this.Cast<Transport>()
						.FirstOrDefault(GetMainLinkedSeaTransportPredicate(sailingPK));

					if (mainTransport == null)
					{
						mainTransport = AddNew();

						using (GetTransportSuspenders(mainTransport, suspendSettingHasChanges))
						{
							mainTransport.JW_IsLinked = true;
							mainTransport.JW_TransportMode = Constants.TransportModes.Sea;
							mainTransport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
						}
					}
				}

				using (GetTransportSuspenders(mainTransport, suspendSettingHasChanges))
				{
					mainTransport.JW_JX = sailingPK;
				}
			}
			else if (mainTransport != null)
			{
				if (mainTransport.JW_TransportType == Constants.TransportPlanningType.MainVessel
					&& !mainTransport.IsSettingIsLinked)
				{
					mainTransport.Delete();
				}

				mainTransport = null;
			}
		}

		IDisposable GetTransportSuspenders(Transport transport, bool suspendSettingHasChanges)
		{
			IDisposable validationSuspender = transport.GetValidationSuspender();
			IDisposable settingHasChangesSuspender = suspendSettingHasChanges
				? transport.SuspendSettingHasChanges()
				: null;

			return new DisposableAction(() =>
			{
				validationSuspender.Dispose();

				if (settingHasChangesSuspender != null)
				{
					settingHasChangesSuspender.Dispose();
				}
			});
		}

		Func<Transport, bool> GetMainLinkedSeaTransportPredicate(ZGuid sailingPK)
		{
			return transport =>
				transport.JW_JX == sailingPK &&
				transport.JW_TransportMode == Constants.TransportModes.Sea &&
				transport.JW_TransportType == Constants.TransportPlanningType.MainVessel;
		}

		Transport mainTransport;
		readonly AgencyShipment shipment;
	}
}


