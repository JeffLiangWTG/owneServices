using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class ContainerHelper
	{
		#region Constructors

		public ContainerHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		#endregion

		#region Methods

		public RefContainerCollection List(string shippingMode)
		{
			var filter = new ZQuery();

			if (!string.IsNullOrEmpty(shippingMode))
			{
				filter.AddToFilter(RefContainerSchema.RC_ShippingMode, GetContainerTransportMode(shippingMode));
			}

			filter.AddToFilter(RefContainerSchema.RC_IsActive, ZBool.True);

			return new RefContainerCollection(Factory, filter);
		}

		#endregion

		#region Properties

		public BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		#endregion

		#region Implementation

		readonly BusinessObjectFactory factory;

		string GetContainerTransportMode(string shippingMode)
		{
			// Rail uses Sea containers
			return shippingMode == Core.Constants.TransportModes.Rail ? Core.Constants.TransportModes.Sea : shippingMode;
		}

		#endregion
	}
}
