using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	using System;
	using CargoWise.IO;
	using NUnit.Framework;

	[TestedType(typeof(CommonPickupDeliveryConfirmValueObjectDataAdapter))]
	sealed class CommonPickupDeliveryConfirmValueObjectDataAdapterTest : ValueObjectDataAdapterTest<CommonPickupDeliveryConfirm, Xsd.ContainerLeg>
	{
		public void TestContainerLeg_Export()
		{
			CommonPickupDeliveryConfirm leg = Factory.New<CommonPickupDeliveryConfirm>();
			leg.EU_PickupDeliveryType = "ANY";
			leg.EU_GoodsSignForBy = "Mr Bob";
			Xsd.ContainerLeg legValue = new CommonPickupDeliveryConfirmValueObjectDataAdapter().ExportToValueObject(leg, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("GoodsRecBy", "Mr Bob", legValue.GoodsRecBy);
			AssertEquals("ContainerLegType", nameof(Xsd.ContainerLegType.DLV), legValue.LegType.ToString());
		}

		#region Overrides for base test

		protected override ValueObjectDataAdapter<CommonPickupDeliveryConfirm, Xsd.ContainerLeg> GetNewBizObjXmlDataAdapter()
		{
			return new CommonPickupDeliveryConfirmValueObjectDataAdapter();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "ContainerLegs"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "ContainerLeg"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			CommonPickupDeliveryConfirm leg = Factory.New<CommonPickupDeliveryConfirm>();
			leg.EU_PickupDeliveryType = "ANY";
			leg.EU_GoodsSignForBy = "Mr Bob";
			return new BusinessObjectAndExpectedOutputFileName(leg, null, ValidationKind.None, ZString.Empty);
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetFullyPopulatedBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			CommonPickupDeliveryConfirm leg = Factory.New<CommonPickupDeliveryConfirm>();
			leg.EU_PickupDeliveryType = "ANY";
			leg.EU_GoodsSignForBy = "Mr Bob";
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.PopulatedContainerLeg.xml", "PopulatedContainerLeg.xml");
			return new BusinessObjectAndExpectedOutputFileName(leg, expectedOutputFilename, ValidationKind.Xsd, "Populated container leg");
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get { return false; }
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		#endregion
	}
}
