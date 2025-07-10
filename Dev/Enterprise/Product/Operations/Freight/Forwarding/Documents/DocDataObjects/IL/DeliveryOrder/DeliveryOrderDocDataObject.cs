using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class DeliveryOrderDocDataObject : DocDataObject, IDataSourceProvider
	{
		public DeliveryOrderDocDataObject(ZString sourceType, ZString sourceID, BusinessObjectFactory factory)
			: base(factory)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region Forwarder

		public Address Forwarder
		{
			get => forwarder;
			set => forwarder = SetChild(forwarder, value);
		}
		Address forwarder;

		#endregion Forwarder

		#region ForwarderVat

		public ZString ForwarderVat
		{
			get => forwarderVat;
			set
			{
				if (SetNonPersistentPropertyValue(ForwarderVatInfo, ref forwarderVat, value))
				{
					Validate(ForwarderVatInfo);
				}
			}
		}
		ZString forwarderVat;

		public ZPropertyInfo ForwarderVatInfo => GetZPropertyInfo(nameof(ForwarderVat));

		#endregion ForwarderVat

		#region CustomsBroker

		public Address CustomsBroker
		{
			get => customsBroker;
			set => customsBroker = SetChild(customsBroker, value);
		}
		Address customsBroker;

		#endregion CustomsBroker

		#region CustomsBrokerVat

		public ZString CustomsBrokerVat
		{
			get => customsBrokerVat;
			set
			{
				if (SetNonPersistentPropertyValue(CustomsBrokerVatInfo, ref customsBrokerVat, value))
				{
					Validate(CustomsBrokerVatInfo);
				}
			}
		}
		ZString customsBrokerVat;

		public ZPropertyInfo CustomsBrokerVatInfo => GetZPropertyInfo(nameof(CustomsBrokerVat));

		#endregion CustomsBrokerVat

		#region DeliveryOrderNumber

		public ZString DeliveryOrderNumber { get; set; }

		#endregion DeliveryOrderNumber

		#region DeliverySite

		public ICodeDescription DeliverySite
		{
			get => deliverySite;
			set => deliverySite = SetChild(deliverySite, value);
		}
		ICodeDescription deliverySite;

		#endregion DeliverySite

		#region CargoIdentifierType

		public ICodeDescription CargoIdentifierType
		{
			get => cargoIdentifierType;
			set => cargoIdentifierType = SetChild(cargoIdentifierType, value);
		}
		ICodeDescription cargoIdentifierType;

		#endregion CargoIdentifierType

		#region ManifestNumber

		public ZString ManifestNumber
		{
			get => manifestNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ManifestNumberInfo, ref manifestNumber, value))
				{
					Validate(ManifestNumberInfo);
				}
			}
		}
		ZString manifestNumber;

		public ZPropertyInfo ManifestNumberInfo => GetZPropertyInfo(nameof(ManifestNumber));

		#endregion ManifestNumber

		#region DealNumber

		public ZString DealNumber
		{
			get => dealNumber;
			set
			{
				if (SetNonPersistentPropertyValue(DealNumberInfo, ref dealNumber, value))
				{
					Validate(DealNumberInfo);
				}
			}
		}
		ZString dealNumber;

		public ZPropertyInfo DealNumberInfo => GetZPropertyInfo(nameof(DealNumber));

		#endregion DealNumber

		#region ReceiverType

		public ICodeDescription ReceiverType
		{
			get => receiverType;
			set => receiverType = SetChild(receiverType, value);
		}
		ICodeDescription receiverType;
		
		#endregion ReceiverType

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion
	}
}
