using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonPickupDeliveryConfirmDocumentSupporter))]
	sealed class CommonPickupDeliveryConfirmDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage_Shipment()
		{
			var deliveryConfirm = Factory.New<CommonPickupDeliveryConfirm>();
			deliveryConfirm.EU_JS = ZGuid.Empty;

			var contextArray = new[]
			{
				Constants.DataContext.Shipment,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This confirm is not associated with a shipment.";

			AssertNotFoundMessage(deliveryConfirm, menu, contextArray, true, expectedMessage);

			var shipment = Factory.New<CommonShipment>();
			deliveryConfirm.EU_JS = shipment.PK;

			AssertNotFoundMessage(deliveryConfirm, menu, contextArray, false, ZString.Empty);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "Invalid DataContext: Consol")]
		public void TestRunSheetDocSupporterThrowsExceptionWithInvalidDataContext()
		{
			iConfirm.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Consol, null);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("Constants.DataContext.PickupDeliveryConfirm is Supported", true, iConfirm.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.PickupDeliveryConfirm)));
			AssertEquals("Constants.DataContext.CommonCartageLeg is Supported", true, iConfirm.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CommonCartageLeg)));
			AssertEquals("Constants.DataContext.ContainerLeg is Supported", true, iConfirm.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ContainerLeg)));
			AssertEquals("Constants.DataContext.CartageAdvice is Supported", true, iConfirm.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CartageAdvice)));
			AssertEquals("Constants.DataContext.Shipment is Supported", true, iConfirm.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Shipment)));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.None, iConfirm.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.PickupDeliverConfirm, iConfirm.DocumentSupporter.BusinessContext);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			confirm = Factory.New<CommonPickupDeliveryConfirm>();
			iConfirm = confirm;
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CommonPickupDeliveryConfirm>();
		}

		CommonPickupDeliveryConfirm confirm;
		IDocumentSupportable iConfirm;

		#endregion
	}
}
