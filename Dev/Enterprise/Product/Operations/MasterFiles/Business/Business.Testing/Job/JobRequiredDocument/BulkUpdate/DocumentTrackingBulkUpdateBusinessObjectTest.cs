using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DocumentTrackingBulkUpdateBusinessObject))]
	sealed class DocumentTrackingBulkUpdateBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSavingBizOUpdatesSelectedDocuments()
		{
			BulkUpdateBizO.SelectedDocuments.AddDocToBulkUpdate(HXDDocToUpdate1.PK, null);

			BulkUpdateBizO.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-5);
			BulkUpdateBizO.EQ_SntToCustomsBroker = ZDateTime.Now.AddDays(-4);
			BulkUpdateBizO.EQ_RcvFromCustomsBroker = ZDateTime.Now.AddDays(-3);
			BulkUpdateBizO.EQ_ReturnToShipper = ZDateTime.Now.AddDays(-2);

			Factory.Save();

			AssertEquals("Selected document dates should be updated", BulkUpdateBizO.EQ_DateReceived, HXDDocToUpdate1.EQ_DateReceived);
			AssertEquals("Selected document dates should be updated", BulkUpdateBizO.EQ_SntToCustomsBroker, HXDDocToUpdate1.EQ_SntToCustomsBroker);
			AssertEquals("Selected document dates should be updated", BulkUpdateBizO.EQ_RcvFromCustomsBroker, HXDDocToUpdate1.EQ_RcvFromCustomsBroker);
			AssertEquals("Selected document dates should be updated", BulkUpdateBizO.EQ_ReturnToShipper, HXDDocToUpdate1.EQ_ReturnToShipper);

			AssertEquals("Unselected documents shouldn't be updated", ZDateTimeOffset.Empty, HXDDocToUpdate2.EQ_DateReceived);
			AssertEquals("Unselected documents shouldn't be updated", ZDateTime.Empty, HXDDocToUpdate2.EQ_SntToCustomsBroker);
			AssertEquals("Unselected documents shouldn't be updated", ZDateTime.Empty, HXDDocToUpdate2.EQ_RcvFromCustomsBroker);
			AssertEquals("Unselected documents shouldn't be updated", ZDateTime.Empty, HXDDocToUpdate2.EQ_ReturnToShipper);
		}

		public void TestSecurityOfHXDFields()
		{
			bool oldSentToBroker = Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerBulkUpdate.IsAllowed;
			bool oldReceivedFromBroker = Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerBulkUpdate.IsAllowed;
			bool oldReturnToShipper = Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperBulkUpdate.IsAllowed;

			try
			{
				BulkUpdateBizO.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;
				BulkUpdateBizO.EQ_DateReceived = ZDateTimeOffset.Now;
				BulkUpdateBizO.EQ_SntToCustomsBroker = ZDateTime.Now;
				BulkUpdateBizO.EQ_RcvFromCustomsBroker = ZDateTime.Now;

				Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerBulkUpdate.IsAllowed = false;
				Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerBulkUpdate.IsAllowed = false;
				Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperBulkUpdate.IsAllowed = false;
				AssertEquals("User without security permissions shouldn't be able to edit HXD fields", true, BulkUpdateBizO.EQ_SntToCustomsBrokerInfo.ReadOnly);
				AssertEquals("User without security permissions shouldn't be able to edit HXD fields", true, BulkUpdateBizO.EQ_RcvFromCustomsBrokerInfo.ReadOnly);
				AssertEquals("User without security permissions shouldn't be able to edit HXD fields", true, BulkUpdateBizO.EQ_ReturnToShipperInfo.ReadOnly);

				Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerBulkUpdate.IsAllowed = true;
				Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerBulkUpdate.IsAllowed = true;
				Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperBulkUpdate.IsAllowed = true;
				AssertEquals("User with security permissions should be able to edit HXD fields", false, BulkUpdateBizO.EQ_SntToCustomsBrokerInfo.ReadOnly);
				AssertEquals("User with security permissions should be able to edit HXD fields", false, BulkUpdateBizO.EQ_RcvFromCustomsBrokerInfo.ReadOnly);
				AssertEquals("User with security permissions should be able to edit HXD fields", false, BulkUpdateBizO.EQ_ReturnToShipperInfo.ReadOnly);
			}
			finally
			{
				Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerBulkUpdate.IsAllowed = oldSentToBroker;
				Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerBulkUpdate.IsAllowed = oldReceivedFromBroker;
				Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperBulkUpdate.IsAllowed = oldReturnToShipper;
			}
		}

		public void TestIrrelevantValidationIsOverridden()
		{
			BulkUpdateBizO.Validation.ValidateAll();
			AssertNoErrors("Base validation for EQ_DocType should have been overridden", BulkUpdateBizO.EQ_DocTypeInfo);
			AssertNoErrors("Base validation for EQ_DocNumber should have been overridden", BulkUpdateBizO.EQ_DocNumberInfo);
		}

		public void TestAllNeededValidationsOverriden()
		{
			DocumentTrackingBulkUpdateBusinessObject obj = Factory.New<DocumentTrackingBulkUpdateBusinessObject>();
			Assert("Precondition", !obj.HasErrors);
			obj.Validation.ValidateAll();
			if (obj.HasErrors)
			{
				StringCollectionX propertiesWithNotifications = new StringCollectionX();
				foreach (ZPropertyInfo info in obj.PropertiesWithNotifications)
				{
					propertiesWithNotifications.Add(info.Name);
				}
				Fail("Shouldn't have any errors. Properties with notifications: " + string.Join(", ", propertiesWithNotifications.ToArray()) + ". Please override appropriate validations.");
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Implementation

		JobRequiredDocument HXDDocToUpdate1, HXDDocToUpdate2;
		DocumentTrackingBulkUpdateBusinessObject BulkUpdateBizO;

		protected override void SetUp()
		{
			base.SetUp();

			IDocsAndCartageParent shipment1 = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)),
				TestBusinessObjectKind.MinimumRequiredToSave);
			HXDDocToUpdate1 = shipment1.RequiredDocumentsProvider.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.HeXiaoDan);
			HXDDocToUpdate1.EQ_DocNumber = "DOC1";

			IDocsAndCartageParent shipment2 = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)),
				TestBusinessObjectKind.MinimumRequiredToSave);
			HXDDocToUpdate2 = shipment2.RequiredDocumentsProvider.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.HeXiaoDan);
			HXDDocToUpdate2.EQ_DocNumber = "DOC2";

			BulkUpdateBizO = Factory.New<DocumentTrackingBulkUpdateBusinessObject>();
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}

		#endregion
	}
}
