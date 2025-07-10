using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class HouseBillTermsAndConditionsTest : TestCaseWithFactory
	{
		public void TestImage()
		{
			const string hblTypeCode = "XXX";
			const string termsAndConditionsCode = "YYY";
			const string imageCode = "ZZZ";

			var hblRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			var hblRegistryObject = hblRegistryObjectCollection.AddNew();
			hblRegistryObject.Code = hblTypeCode;
			hblRegistryObject.TermsAndConditionsCode = termsAndConditionsCode;
			hblRegistryObject.Description = (NoResString)"test";
			hblRegistryObject.LogoCode = imageCode;
			hblRegistryObject.PrintLogoInFormBuilder = PrintLogoOptions.Codes.All;
			hblRegistryObject.PrePrinted = false;

			var termsAndConditionsCollection = new HouseBillOfLadingTermsAndConditionsCollection();
			var termAndCondition = termsAndConditionsCollection.AddNew();
			termAndCondition.Code = termsAndConditionsCode;
			termAndCondition.DeliveryMode = nameof(PrintCopyType.ALL);
			termAndCondition.Description = (NoResString)"XYZ";
			termAndCondition.Image = new Bitmap(10, 10);

			using (FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetTemporaryValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hblRegistryObjectCollection))
			using (FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, termsAndConditionsCollection))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBillOfLadingType = hblTypeCode;

				var houseBillTermsAndConditions = new HouseBillTermsAndConditions(shipment);

				AssertNotNull("Image", houseBillTermsAndConditions.Image);
			}
		}
	}
}
