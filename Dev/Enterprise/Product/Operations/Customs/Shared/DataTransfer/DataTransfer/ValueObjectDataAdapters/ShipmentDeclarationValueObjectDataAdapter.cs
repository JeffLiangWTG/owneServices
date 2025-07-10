using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class ShipmentDeclarationValueObjectDataAdapter : ShipmentValueObjectDataAdapter<ForwardingShipment>
	{
		#region Constructor

		public ShipmentDeclarationValueObjectDataAdapter()
		{
		}

		public ShipmentDeclarationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		public static ShipmentDeclarationValueObjectDataAdapter New()
		{
			return New(EventsWithSourceType.Empty);
		}

		public static ShipmentDeclarationValueObjectDataAdapter New(EventsWithSourceType triggeredByEvents)
		{
			ShipmentDeclarationValueObjectDataAdapter result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				result = New(ObjectFactory.GetType<Integration.Customs.AU.IAUShipmentDeclarationValueObjectDataAdapter>(), triggeredByEvents);
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
			{
				result = New(ObjectFactory.GetType<Integration.Customs.NZ.INZShipmentDeclarationValueObjectDataAdapter>(), triggeredByEvents);
			}
			else
			{
				result = new ShipmentDeclarationValueObjectDataAdapter(triggeredByEvents);
			}
			return result;
		}

		static ShipmentDeclarationValueObjectDataAdapter New(Type type, EventsWithSourceType triggeredByEvents)
		{
			return (ShipmentDeclarationValueObjectDataAdapter)Activator.CreateInstance(type, new object[] { triggeredByEvents });
		}

		protected delegate ShipmentDeclarationValueObjectDataAdapter NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region override

		protected override ForwardingShipment NewBusinessObject(Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			return (ForwardingShipment)context.Factory.New(typeof(ForwardingShipment));
		}

		#region FindBusinessObjects

		protected override ForwardingShipment FindBusinessObject(Xsd.Shipment value, IValueObjectImportContext context)
		{
			ForwardingShipment result = null;

			var candidateShipments = FindBusinessObjects(value, context);

			if (candidateShipments.Length > 0)
			{
				result = GetShipmentWithoutAConsol(candidateShipments, context.Factory);
			}

			return result;
		}

		protected ForwardingShipment[] FindBusinessObjects(Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			var candidateShipments = Array.Empty<ForwardingShipment>();

			var houseIdentifier = shipmentValue.ShipmentIdentifier.FindFirst(Xsd.ShipmentIdentifierType.Housebill);
			var houseBill = houseIdentifier != null ? houseIdentifier.Value : ZString.Empty;

			if (houseBill != "")
			{
				ZString transportMode = (shipmentValue.ShipmentDetails.IsSpecified) ? TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(shipmentValue.ShipmentDetails.TransportMode, (NoResString)"Transport mode", context) : "";
				var portOfOrigin = shipmentValue.ShipmentDetails.PortOfOrigin.Port.Value;
				var portOfDest = shipmentValue.ShipmentDetails.PortofDestination.Port.Value;

				var filter = new ZQuery(JobShipmentSchema.JS_HouseBill, houseBill);
				filter.AddToFilter(JobShipmentSchema.JS_TransportMode, transportMode);
				filter.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, portOfOrigin);
				filter.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, portOfDest);
				candidateShipments = (ForwardingShipment[])context.Factory.Load(BusinessObjectType, filter);
			}

			return candidateShipments;
		}

		#endregion

		protected ZGlobalMutex mutex;

		protected override bool ShouldUpdateExistingObject(ForwardingShipment bizObj, INotifications notifications)
		{
			var result = false;
			var shipment = bizObj;

			mutex = Enterprise.Customs.Common.DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK);
			if (shipment.Declarations.Length == 0 && !mutex.IsLocked)
			{
				result = base.ShouldUpdateExistingObject(bizObj, notifications);
				if (result)
				{
					result = mutex.Lock();
				}
			}
			return result;
		}

		#region ExportToValueObjectCore

		protected override void ExportToValueObjectCore(ForwardingShipment bizObj, Xsd.Shipment constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting is not supported.");
		}

		#endregion

		#endregion

		#region ShipmentWithoutConsolAndDeclaration

		protected ForwardingShipment GetShipmentWithoutAConsol(ForwardingShipment[] shipments, BusinessObjectFactory factory)
		{
			foreach (var candidateShipment in shipments)
			{
				if (candidateShipment.Consols.Count == 0)
				{
					return candidateShipment;
				}
			}

			return null;
		}

		#endregion

		#region UserDeclinedImport

		protected override void OnUserDeclinedImport(ForwardingShipment bizObj, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			var houseIdentifier = shipmentValue.ShipmentIdentifier.FindFirst(Xsd.ShipmentIdentifierType.Housebill);
			var houseBill = houseIdentifier != null ? houseIdentifier.Value : ZString.Empty;
			context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("6aa7022f-d800-451f-9463-0b15012dd7c6", "Shipment with HAWB {0} is already linked to a Consol and/or a Declaration and cannot be imported.", houseBill)));
		}

		#endregion

		#region ImportFromValueObject

		public override void ImportFromValueObject(ForwardingShipment bizObj, Xsd.Shipment value, IValueObjectImportContext context)
		{
			try
			{
				context.Factory.Saved += Factory_Saved;
				base.ImportFromValueObject(bizObj, value, context);
			}
			catch (Exception)
			{
				UnlockMutex();
				context.Factory.Saved -= Factory_Saved;
				throw;
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			UnlockMutex();
			factory.Saved -= Factory_Saved;
		}

		#endregion

		#region OnAfterImportFromValueObjectCore

		protected override void OnAfterImportFromValueObjectCore(ForwardingShipment shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			base.OnAfterImportFromValueObjectCore(shipment, value, context);
			CreateJobDeclaration(shipment, value, context);
		}

		void UnlockMutex()
		{
			if (mutex != null)
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
				mutex = null;
			}
		}

		#endregion

		#region GetJobDeclaration

		protected virtual BaseJobDeclaration GetJobDeclaration(IValueObjectImportContext context)
		{
			return (BaseJobDeclaration)context.Factory.New(typeof(BaseJobDeclaration));
		}

		#endregion

		#region CreateJobDeclaration

		protected void CreateJobDeclaration(BusinessObject bizObj, IValueObject value, IValueObjectImportContext context)
		{
			var shipment = (ForwardingShipment)bizObj;

			var jobDeclaration = GetJobDeclaration(context);
			jobDeclaration.JE_JS = shipment.PK;
			var synchroniser = new JobDeclarationSynchroniser(jobDeclaration);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AddImportEventForDeclaration(jobDeclaration);
		}

		#endregion

		#region AddImportEventForDeclaration

		protected void AddImportEventForDeclaration(BaseJobDeclaration jobDec)
		{
			if (jobDec != null && (!jobDec.IsInDatabase))
			{
				jobDec.Logs.AddNew(Events.DataImport);
			}
		}

		#endregion

	}
}
