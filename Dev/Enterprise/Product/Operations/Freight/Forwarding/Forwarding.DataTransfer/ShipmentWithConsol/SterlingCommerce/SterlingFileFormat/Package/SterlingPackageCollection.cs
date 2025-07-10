using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingPackageCollection : NonPersistentBusinessObjectCollection<SterlingPackage>
	{
		public SterlingPackageCollection(SterlingCommerceConsolAndShipmentExporter master)
		{
			this.Master = master;
			UpdateCollection();
		}
		readonly SterlingCommerceConsolAndShipmentExporter Master;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SterlingPackage();
		}

		public void UpdateCollection()
		{
			if (Master.Shipment.ShipmentDetails.Packages != null)
			{
				foreach (Xsd.Package packToAdd in Master.Shipment.ShipmentDetails.Packages)
				{
					SterlingPackage pack = this.AddNew();
					pack.Source = packToAdd;
				}
			}
		}
	}
}

