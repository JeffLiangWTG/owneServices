using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefContainer)]
	public class RefContainerCollection : ActiveBusinessObjectCollection<RefContainer>, Integration.IRefContainerCollection
	{
		public RefContainerCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefContainerCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		public RefContainerCollection(BusinessObjectFactory factory, string transportMode)
			: this(factory)
		{
			this.TransportMode = transportMode;
			AddTransportModeDefault();
		}

		public RefContainerCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		#region Transport Mode

		void AddTransportModeDefault()
		{
			if (!TransportMode.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transport Mode", "Property", TransportMode));
			}
		}

		public readonly ZString TransportMode;

		protected override void SetDefaultsForNewElementCore(RefContainer newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (!TransportMode.IsEmpty)
			{
				newElement.RC_ShippingMode = TransportMode;
			}
		}

		#endregion
	}
}
