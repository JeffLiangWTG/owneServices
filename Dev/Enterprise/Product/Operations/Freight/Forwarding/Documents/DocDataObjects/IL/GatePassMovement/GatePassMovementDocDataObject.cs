using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class GatePassMovementDocDataObject : DocDataObject, IDataSourceProvider
	{
		public GatePassMovementDocDataObject(ZString sourceType, ZString sourceID, BusinessObjectFactory factory)
			: base(factory)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region GatepassMovementNumber

		public ZString GatePassMovementNumber { get; set; }

		#endregion GatepassMovementNumber

		#region ProcessType

		public ZString ProcessType { get; set; }

		#endregion ProcessType

		#region OriginSite

		public ICodeDescription OriginSite
		{
			get => originSite;
			set => originSite = SetChild(originSite, value);
		}
		ICodeDescription originSite;

		#endregion OriginSite

		#region DestinationSite 

		public ICodeDescription DestinationSite
		{
			get => destinationSite;
			set => destinationSite = SetChild(destinationSite, value);
		}
		ICodeDescription destinationSite;

		#endregion DestinationSite

		#region CargoType

		public ICodeDescription CargoType
		{
			get => cargoType;
			set => cargoType = SetChild(cargoType, value);
		}
		ICodeDescription cargoType;

		#endregion CargoType

		#region TransportMethod

		public ICodeDescription TransportMethod
		{
			get => transportMethod;
			set => transportMethod = SetChild(transportMethod, value);
		}
		ICodeDescription transportMethod;

		#endregion TransportMethod

		#region CargoIdentifierType

		public ICodeDescription CargoIdentifierType
		{
			get => cargoIdentifierType;
			set => cargoIdentifierType = SetChild(cargoIdentifierType, value);
		}
		ICodeDescription cargoIdentifierType;

		#endregion CargoIdentifierType

		#region CargoIdentifierKey1

		public ZString CargoIdentifierKey1
		{
			get => cargoIdentifierKey1;
			set
			{
				if (SetNonPersistentPropertyValue(CargoIdentifierKey1Info, ref cargoIdentifierKey1, value))
				{
					Validate(CargoIdentifierKey1Info);
				}
			}
		}
		ZString cargoIdentifierKey1;

		public ZPropertyInfo CargoIdentifierKey1Info => GetZPropertyInfo(nameof(CargoIdentifierKey1));

		#endregion CargoIdentifierKey1

		#region CargoIdentifierKey2

		public ZString CargoIdentifierKey2
		{
			get => cargoIdentifierKey2;
			set
			{
				if (SetNonPersistentPropertyValue(CargoIdentifierKey2Info, ref cargoIdentifierKey2, value))
				{
					Validate(CargoIdentifierKey2Info);
				}
			}
		}
		ZString cargoIdentifierKey2;

		public ZPropertyInfo CargoIdentifierKey2Info => GetZPropertyInfo(nameof(CargoIdentifierKey2));

		#endregion CargoIdentifierKey2

		#region CargoIdentifierKey3

		public ZString CargoIdentifierKey3
		{
			get => cargoIdentifierKey3;
			set
			{
				if (SetNonPersistentPropertyValue(CargoIdentifierKey3Info, ref cargoIdentifierKey3, value))
				{
					Validate(CargoIdentifierKey2Info);
				}
			}
		}
		ZString cargoIdentifierKey3;

		public ZPropertyInfo CargoIdentifierKey3Info => GetZPropertyInfo(nameof(CargoIdentifierKey3));

		public ZBool CargoIdentifierKey3IsVisible
		{
			get => cargoIdentifierKey3IsVisible;
			set
			{
				if (SetNonPersistentPropertyValue(CargoIdentifierKey3Info, ref cargoIdentifierKey3IsVisible, value))
				{
					Validate(cargoIdentifierKey3IsVisibleInfo);
				}
			}
		}
		ZBool cargoIdentifierKey3IsVisible;

		public ZPropertyInfo cargoIdentifierKey3IsVisibleInfo => GetZPropertyInfo(nameof(CargoIdentifierKey3IsVisible));

		#endregion CargoIdentifierKey3	

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion
	}
}
