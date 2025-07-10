using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolPacklineHelperTest : TestCaseWithFactory
	{
		public void TestConsolPacklineHelper_LoosePacklinesOrULDPacklinesAreValid()
		{
			var consol = Factory.New<ForwardingConsol>();

			AssertEquals(ConsolPacklineHelper.MissingPreRequisitesErrorMessage, consol.GetPacklinesValidationMessage());

			consol.Shipments.AddNew();
			consol.Shipments[0].OuterPackLines.AddNew();
			consol.Shipments[0].OuterPackLines[0].JL_Length = 0;
			consol.Shipments[0].OuterPackLines[0].JL_Width = 0;
			consol.Shipments[0].OuterPackLines[0].JL_Height = 0;
			consol.Shipments[0].OuterPackLines[0].JL_ActualWeight = 0;
			consol.Shipments[0].OuterPackLines[0].JL_ActualVolume = 0;
			consol.Shipments[0].OuterPackLines[0].JL_PackageCount = 0;
			Factory.Save();

			AssertEquals(ConsolPacklineHelper.TotalShipmentWeightErrorMessage, consol.GetPacklinesValidationMessage());

			consol.Shipments[0].JS_ActualWeight = 1;
			AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, consol.GetPacklinesValidationMessage());

			consol.Shipments[0].OuterPackLines[0].JL_Length = 1;
			AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, consol.GetPacklinesValidationMessage());

			consol.Shipments[0].OuterPackLines[0].JL_Width = 1;
			AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, consol.GetPacklinesValidationMessage());

			consol.Shipments[0].OuterPackLines[0].JL_Height = 1;
			AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, consol.GetPacklinesValidationMessage());

			consol.Shipments[0].OuterPackLines[0].JL_ActualWeight = 1;
			AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, consol.GetPacklinesValidationMessage());

			consol.Shipments[0].OuterPackLines[0].JL_ActualVolume = 2;
			AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, consol.GetPacklinesValidationMessage());

			consol.Shipments[0].OuterPackLines[0].JL_PackageCount = 3;
			AssertEquals(ZString.Empty, consol.GetPacklinesValidationMessage());

			consol.Shipments[0].OuterPackLines[0].JL_Height = 0;
			var container = consol.Containers.AddNew();
			AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, consol.GetPacklinesValidationMessage());

			consol.Shipments[0].OuterPackLines[0].SetContainer(consol, container);
			AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, consol.GetPacklinesValidationMessage());
		}

		public void TestConsolPacklineHelper_GetContainersValidationMessage()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(ZString.Empty, consol.GetContainersValidationMessage());

			var container = consol.Containers.AddNew();
			AssertNull(container.RefContainer);
			AssertEquals(ConsolPacklineHelper.ContainersErrorMessage, consol.GetContainersValidationMessage());

			container.JC_ContainerNum = "A12345";
			AssertEquals(ConsolPacklineHelper.ContainersErrorMessage, consol.GetContainersValidationMessage());

			container.JC_ContainerNum = "A123";
			AssertEquals(ConsolPacklineHelper.ContainersErrorMessage, consol.GetContainersValidationMessage());

			container.JC_ContainerCount = 1;
			AssertEquals(ConsolPacklineHelper.ContainersErrorMessage, consol.GetContainersValidationMessage());

			container.JC_GrossWeight = 2000m;
			AssertEquals(ConsolPacklineHelper.ContainersErrorMessage, consol.GetContainersValidationMessage());

			container.JC_TotalHeight = 1m;
			AssertEquals(ConsolPacklineHelper.ContainersErrorMessage, consol.GetContainersValidationMessage());

			container.JC_TotalWidth = 1m;
			AssertEquals(ConsolPacklineHelper.ContainersErrorMessage, consol.GetContainersValidationMessage());

			container.JC_TotalLength = 1m;
			container.JC_ContainerNum = "AKE12345NZ";
			AssertEquals(ZString.Empty, consol.GetContainersValidationMessage());

			container.JC_ContainerNum = ZString.Empty;
			AssertEquals(ZString.Empty, consol.GetContainersValidationMessage());
		}

		public void TestGetPacklinesValidationMessage()
		{
			TestGetPacklinesValidationMessageCase(100, new List<(ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZInt)>
			{
				(1, 1, 1, 1, 1, 1)
			}, totalShipmentWeightShouldBeValid: true, packlinesShouldBeValid: true);

			TestGetPacklinesValidationMessageCase(100, new List<(ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZInt)>
			{
				(0, 1, 1, 1, 1, 1)
			}, totalShipmentWeightShouldBeValid: true, packlinesShouldBeValid: true);

			TestGetPacklinesValidationMessageCase(100, new List<(ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZInt)>
			{
				(1, 0, 1, 1, 1, 1)
			}, totalShipmentWeightShouldBeValid: true, packlinesShouldBeValid: false);

			TestGetPacklinesValidationMessageCase(100, new List<(ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZInt)>
			{
				(1, 1, 0, 1, 1, 1)
			}, totalShipmentWeightShouldBeValid: true, packlinesShouldBeValid: false);

			TestGetPacklinesValidationMessageCase(100, new List<(ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZInt)>
			{
				(1, 1, 1, 0, 1, 1)
			}, totalShipmentWeightShouldBeValid: true, packlinesShouldBeValid: false);

			TestGetPacklinesValidationMessageCase(100, new List<(ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZInt)>
			{
				(1, 1, 1, 1, 0, 1)
			}, totalShipmentWeightShouldBeValid: true, packlinesShouldBeValid: false);

			TestGetPacklinesValidationMessageCase(100, new List<(ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZInt)>
			{
				(1, 1, 1, 1, 1, 0)
			}, totalShipmentWeightShouldBeValid: true, packlinesShouldBeValid: false);

			TestGetPacklinesValidationMessageCase(0, new List<(ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZDecimal, ZInt)>
			{
				(1, 1, 1, 1, 1, 1)
			}, totalShipmentWeightShouldBeValid: false, packlinesShouldBeValid: false);
		}

		void TestGetPacklinesValidationMessageCase(
			ZDecimal shipmentWeight,
			IEnumerable<(ZDecimal JL_ActualWeight, ZDecimal JL_ActualVolume, ZDecimal JL_Length, ZDecimal JL_Width, ZDecimal JL_Height, ZInt JL_PackageCount)> packLineProps,
			bool totalShipmentWeightShouldBeValid, bool packlinesShouldBeValid)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			Factory.Save();

			var arrLineProps = packLineProps.ToArray();
			for (var i = 0; i < arrLineProps.Length; i++)
			{
				consol.Shipments[0].OuterPackLines.AddNew();
				consol.Shipments[0].OuterPackLines[i].JL_ActualWeight = arrLineProps[i].JL_ActualWeight;
				consol.Shipments[0].OuterPackLines[i].JL_Length = arrLineProps[i].JL_Length;
				consol.Shipments[0].OuterPackLines[i].JL_Width = arrLineProps[i].JL_Width;
				consol.Shipments[0].OuterPackLines[i].JL_Height = arrLineProps[i].JL_Height;
				consol.Shipments[0].OuterPackLines[i].JL_PackageCount = arrLineProps[i].JL_PackageCount;
				consol.Shipments[0].OuterPackLines[i].JL_ActualVolume = arrLineProps[i].JL_ActualVolume;
			}

			consol.Shipments[0].JS_ActualWeight = shipmentWeight;

			var validationMessage = consol.GetPacklinesValidationMessage();
			if (totalShipmentWeightShouldBeValid && packlinesShouldBeValid)
			{
				AssertEquals(string.Empty, validationMessage);
			}
			else if (!totalShipmentWeightShouldBeValid)
			{
				AssertEquals(ConsolPacklineHelper.TotalShipmentWeightErrorMessage, validationMessage);
			}
			else if (!packlinesShouldBeValid)
			{
				AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, validationMessage);
			}
		}

		public void TestConsolPacklineHelper_EnablePacklineWeightDistribution()
		{
			var consol = Factory.New<ForwardingConsol>();

			AssertEquals(ConsolPacklineHelper.MissingPreRequisitesErrorMessage, consol.GetPacklinesValidationMessage());

			consol.Shipments.AddNew();
			consol.Shipments[0].OuterPackLines.AddNew();
			consol.Shipments[0].OuterPackLines[0].JL_Length = 1;
			consol.Shipments[0].OuterPackLines[0].JL_Width = 1;
			consol.Shipments[0].OuterPackLines[0].JL_Height = 1;
			consol.Shipments[0].OuterPackLines[0].JL_ActualWeight = 0;
			consol.Shipments[0].OuterPackLines[0].JL_ActualVolume = 1;
			consol.Shipments[0].OuterPackLines[0].JL_PackageCount = 1;

			consol.Shipments.AddNew();
			consol.Shipments[1].OuterPackLines.AddNew();
			consol.Shipments[1].OuterPackLines[0].JL_Length = 2;
			consol.Shipments[1].OuterPackLines[0].JL_Width = 2;
			consol.Shipments[1].OuterPackLines[0].JL_Height = 2;
			consol.Shipments[1].OuterPackLines[0].JL_ActualWeight = 0;
			consol.Shipments[1].OuterPackLines[0].JL_ActualVolume = 2;
			consol.Shipments[1].OuterPackLines[0].JL_PackageCount = 2;
			Factory.Save();

			PacklineWeightDistributionConfiguration defaultValue = new PacklineWeightDistributionConfiguration();
			defaultValue.EnablePacklineWeightDistribution = false;
			defaultValue.EnableActualWeightDistribution = false;
			defaultValue.EnableVolumetricWeightDistribution = false;

			//Both config set to false.
			using (FreightDataRegistry.Instance.PacklineWeightDistribution.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue))
			{
				FreightDataRegistry.Instance.PacklineWeightDistribution.Value.EnableActualWeightDistribution = false;
				FreightDataRegistry.Instance.PacklineWeightDistribution.Value.EnableVolumetricWeightDistribution = false;

				AssertEquals(ConsolPacklineHelper.TotalShipmentWeightErrorMessage, consol.GetPacklinesValidationMessage());
				consol.Shipments[0].JS_ActualWeight = 1;
				AssertEquals(ZString.Empty, consol.GetPacklinesValidationMessage());
			}

			//Actual Weight Distribution = true.
			defaultValue.EnablePacklineWeightDistribution = true;
			defaultValue.EnableActualWeightDistribution = true;
			using (FreightDataRegistry.Instance.PacklineWeightDistribution.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue))
			{
				consol.Shipments[0].JS_ActualWeight = 0;
				AssertEquals(ConsolPacklineHelper.TotalShipmentWeightErrorMessage, consol.GetPacklinesValidationMessage());
				consol.Shipments[0].JS_ActualWeight = 1;
				AssertEquals(ZString.Empty, consol.GetPacklinesValidationMessage());
			}

			//Volumetric Weight Distribution = true.
			defaultValue.EnableActualWeightDistribution = false;
			defaultValue.EnableVolumetricWeightDistribution = true;
			using (FreightDataRegistry.Instance.PacklineWeightDistribution.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue))
			{
				consol.Shipments[0].JS_ActualWeight = 0;
				AssertEquals(ZString.Empty, consol.GetPacklinesValidationMessage());
			}

			//Both config set to true.
			defaultValue.EnableActualWeightDistribution = true;
			defaultValue.EnableVolumetricWeightDistribution = true;
			using (FreightDataRegistry.Instance.PacklineWeightDistribution.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue))
			{
				consol.Shipments[0].JS_ActualWeight = 0;
				AssertEquals(ZString.Empty, consol.GetPacklinesValidationMessage());

				consol.Shipments[0].OuterPackLines[0].JL_ActualVolume = 0;
				AssertEquals(ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + consol.Shipments[0].JS_UniqueConsignRef, consol.GetPacklinesValidationMessage());
			}
		}
	}
}
