using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class HouseBillLogoTest : TestCaseWithFactory
	{
		public void TestImage()
		{
			const string hblTypeCode = "XXX";
			const string imageCode = "ZZZ";

			var hblRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			var hblRegistryObject = hblRegistryObjectCollection.AddNew();
			hblRegistryObject.Code = hblTypeCode;
			hblRegistryObject.Description = (NoResString)"test";
			hblRegistryObject.LogoCode = imageCode;
			hblRegistryObject.PrintLogoInFormBuilder = PrintLogoOptions.Codes.All;
			hblRegistryObject.PrePrinted = false;

			var imageCollection = new RegistryImageCollection();
			var image = imageCollection.AddNew();
			image.Code = imageCode;
			image.Description = (NoResString)"XYZ";
			image.Image = new Bitmap(10, 10);

			using (FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetTemporaryValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hblRegistryObjectCollection))
			using (FreightDataRegistry.Instance.HouseBillOfLadingLogoImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, imageCollection))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBillOfLadingType = hblTypeCode;

				var houseBillLogo = new HouseBillLogo(shipment, true);

				AssertNotNull("Image", houseBillLogo.Image);
			}
		}
	}
}
