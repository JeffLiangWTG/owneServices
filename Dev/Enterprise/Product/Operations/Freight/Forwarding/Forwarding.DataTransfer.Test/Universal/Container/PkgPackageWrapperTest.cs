using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.DataTransfer.ForwardingPkgPackageDataObjectWriter;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(PkgPackageWrapper))]
	public class PkgPackageWrapperTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var package = packLine.PkgPackageCollection.AddNew();

			return new PkgPackageWrapper
			{
				Package = package,
				PackLine = packLine
			};
		}

		#endregion
	}
}
