using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRollupOrGroupCollection))]
	sealed class InvoiceRollupOrGroupCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<InvoiceRollupOrGroupCollection>
	{
		public void TestGetBestMatchForSeaTransportType()
		{
			InvoiceRollupOrGroupCollection testCollection = GetCollectionToTest();
			InvoiceRollupOrGroup invoiceOrGroupSetting = testCollection.AddNew();
			invoiceOrGroupSetting.JobType = "SHP";
			invoiceOrGroupSetting.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceOrGroupSetting.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			invoiceOrGroupSetting.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceOrGroupSetting.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
			invoiceOrGroupSetting.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			invoiceOrGroupSetting.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

			AssertEquals(null, testCollection.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Shipment.Code));
			AssertEquals(invoiceOrGroupSetting, testCollection.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, OrgConstants.ModesForGroupOrSubTotal.Codes.FCL, JobInvoicingConsumerTypes.Shipment.Code));
			AssertEquals(null, testCollection.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.Rail, OrgConstants.ModesForGroupOrSubTotal.Codes.Rail, JobInvoicingConsumerTypes.Shipment.Code));
			AssertEquals(null, testCollection.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.Road, OrgConstants.ModesForGroupOrSubTotal.Codes.Road, JobInvoicingConsumerTypes.Shipment.Code));
		}

		public void TestAllowRemoveCore()
		{
			InvoiceRollupOrGroupCollection testCollection = GetCollectionToTest();
			Assert(!testCollection.AllowRemove);

			testCollection.AddNew();
			Assert(!testCollection.AllowRemove);

			testCollection.AddNew();
			Assert(testCollection.AllowRemove);

			testCollection.Remove(testCollection[0]);
			Assert(!testCollection.AllowRemove);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override InvoiceRollupOrGroupCollection GetCollectionToTest()
		{
			return new InvoiceRollupOrGroupCollection(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InvoiceRollupOrGroup();
		}

		#endregion
	}
}
