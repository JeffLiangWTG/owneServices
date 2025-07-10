using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(HVLVISFMetaHeader))]
	public class HVLVISFMetaHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSavingChangesOnHeaderSynchronisesAllChildrenISFJobs()
		{
			var isfJob1 = Factory.NewWithValidTestData<CusISFHeader>();
			var isfJob2 = Factory.NewWithValidTestData<CusISFHeader>();

			isfJob1.BF_OwnerReference = "REF1";
			isfJob2.BF_OwnerReference = "REF2";

			var jobCollection = new RelatedJobCollection(Factory);
			jobCollection.Add(isfJob1);
			jobCollection.Add(isfJob2);

			var isfMetaHeader = new HVLVISFMetaHeader(jobCollection);

			isfMetaHeader.FirstImporterSecurityFilingJob.BF_OwnerReference = "CHANGED";
			Factory.Save();

			CombineAssertions("ISF jobs should be synced", () =>
			{
				AssertEquals("isfJob1.BF_OwnerReference should be changed to CHANGED", "CHANGED", isfJob1.BF_OwnerReference);
				AssertEquals("isfJob2.BF_OwnerReference should be changed to CHANGED", "CHANGED", isfJob2.BF_OwnerReference);
			});
		}

		public void TestSavingChangesOnHeaderSynchronisation_BuyingParty()
		{
			AssertDocAddressSynchronized(isf => isf.BuyingParty);
		}

		public void TestSavingChangesOnHeaderSynchronisation_StuffingLocation()
		{
			AssertDocAddressSynchronized(isf => isf.StuffingLocation);
		}

		public void TestSavingChangesOnHeaderSynchronisation_MainShipToParty()
		{
			AssertDocAddressSynchronized(isf => isf.MainShipToParty);
		}

		public void TestSavingChangesOnHeaderSynchronisation_SellingParty()
		{
			AssertDocAddressSynchronized(isf => isf.SellingParty);
		}

		public void TestSavingChangesOnHeaderSynchronisation_Consolidator()
		{
			AssertDocAddressSynchronized(isf => isf.Consolidator);
		}

		void AssertDocAddressSynchronized(Func<CusISFHeader, ISFDocAddress> getAddress)
		{
			var isfJob1 = Factory.NewWithValidTestData<CusISFHeader>();
			var isfJob2 = Factory.NewWithValidTestData<CusISFHeader>();

			var docAddress1 = getAddress(isfJob1);
			var docAddress2 = getAddress(isfJob2);

			docAddress1.E2_AddressOverride = true;
			docAddress2.E2_AddressOverride = true;
			docAddress1.Address1 = "Address 1";
			docAddress2.Address1 = "Address 2";

			var jobCollection = new RelatedJobCollection(Factory);
			jobCollection.Add(isfJob1);
			jobCollection.Add(isfJob2);

			var isfMetaHeader = new HVLVISFMetaHeader(jobCollection);

			getAddress(isfMetaHeader.FirstImporterSecurityFilingJob).Address1 = "CHANGED";
			Factory.Save();

			CombineAssertions("ISF jobs should be synced", () =>
			{
				AssertEquals("Address1 should be changed to CHANGED", "CHANGED", docAddress1.Address1);
				AssertEquals("Address1 should be changed to CHANGED", "CHANGED", docAddress2.Address1);
			});
		}

		public void TestRelatedJobs()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S0001501";

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);
				headers[0].BF_JobReference = "ISF001";
				headers[1].BF_JobReference = "ISF002";

				Factory.Save();

				var metaHeader = new HVLVISFMetaHeader(shipment.Factory, shipment.PK);

				AssertEquals("Expected meta header to contain two related jobs", 2, metaHeader.RelatedJobs.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "ISF001", "ISF002" }, metaHeader.RelatedJobs.OfType<IRelatedJob>().Select(j => j.JobNumber));
				Assert(headers[0].ReferenceDatas.Any(r => r.BB_BillNum == "HVC001"));
				Assert(headers[1].ReferenceDatas.Any(r => r.BB_BillNum == "HVC002"));
			}
		}

		public void TestFirstISFJobNumber()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			var consignment3 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";
			consignment3.HVC_WaybillNumber = "HVC003";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = new BusinessObjectFactory().Load<CusISFHeader>(headerQuery);
				AssertEquals("Expected 3 ISF headers to be created", 3, headers.Length);
				headers[0].BF_JobReference = "ISF0000001";
				headers[1].BF_JobReference = "ISF0000002";
				headers[2].BF_JobReference = "ISF0000003";

				Factory.Save();

				var metaHeader = new HVLVISFMetaHeader(shipment.Factory, shipment.PK);

				AssertEquals("Expected meta header to contain three related jobs", 3, metaHeader.RelatedJobs.Count);
				AssertEquals("Expected the ISF with job reference ISF001 to be the first ISF job due to HVC_DisplayOrder", "ISF0000001", metaHeader.FirstImporterSecurityFilingJob.BF_JobReference);
			}
		}

		public void TestHVLVISFMetaHeaderExcludesCancelledCusISFHeaders()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = Factory.Load<CusISFHeader>(headerQuery);

				AssertEquals("Expected two ISF headers to be created", 2, headers.Length);

				headers[0].BF_JobReference = "ISF0000001";
				headers[0].BF_IsCancelled = true;
				headers[1].BF_JobReference = "ISF0000002";

				Factory.Save();

				var metaHeader = new HVLVISFMetaHeader(shipment.Factory, shipment.PK);

				AssertEquals("Cancelled CusISFHeader jobs should be excluded", 1, metaHeader.RelatedJobs.Count);
				AssertEquals("Expected the ISF with job reference ISF002 to be the first ISF job because the other is cancellec", "ISF0000002", metaHeader.FirstImporterSecurityFilingJob.BF_JobReference);
			}
		}

		public void TestFirstImporterSecurityFilingJob_WhenCreatedViaShipment_IsNotNull()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			var consignment3 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";
			consignment3.HVC_WaybillNumber = "HVC003";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = new BusinessObjectFactory().Load<CusISFHeader>(headerQuery);
				AssertEquals("Expected 3 ISF headers to be created", 3, headers.Length);
				headers[0].BF_JobReference = "ISF0000001";
				headers[1].BF_JobReference = "ISF0000002";
				headers[2].BF_JobReference = "ISF0000003";

				Factory.Save();

				var metaHeader = new HVLVISFMetaHeader(shipment.Factory, shipment.PK);

				AssertNotNull(metaHeader.FirstImporterSecurityFilingJob);
				AssertEquals("ISF0000001", metaHeader.FirstImporterSecurityFilingJob.BF_JobReference);
			}
		}

		public void TestCustomsStatusChangesOnTheFirstHeaderWillNotPropagateToOtherHeaders()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consol.Shipments.Add(shipment);

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			var consignment3 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";
			consignment3.HVC_WaybillNumber = "HVC003";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(Factory);

				Factory.Save();

				var headerQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
				var headers = new BusinessObjectFactory().Load<CusISFHeader>(headerQuery);
				AssertEquals("Expected 3 ISF headers to be created", 3, headers.Length);
				headers[0].BF_JobReference = "ISF0000001";
				headers[1].BF_JobReference = "ISF0000002";
				headers[2].BF_JobReference = "ISF0000003";

				Factory.Save();

				var metaHeader = new HVLVISFMetaHeader(shipment.Factory, shipment.PK);

				AssertNotNull(metaHeader.FirstImporterSecurityFilingJob);

				CombineAssertions("All headers have Customs Status 'NOT'", () =>
				{
					AssertEquals("NOT", ((CusISFHeader)metaHeader.RelatedJobs[0]).BF_CustomsStatus);
					AssertEquals("NOT", ((CusISFHeader)metaHeader.RelatedJobs[1]).BF_CustomsStatus);
					AssertEquals("NOT", ((CusISFHeader)metaHeader.RelatedJobs[2]).BF_CustomsStatus);
				});

				var firstHeaderFromMetaHeader = metaHeader.RelatedJobs[0] as CusISFHeader;
				firstHeaderFromMetaHeader.BF_CustomsStatus = MessageStatusList.Codes.AwaitingISFAdd;

				CombineAssertions("Only the first header should have Custom Status changed", () =>
				{
					AssertEquals("AIO", ((CusISFHeader)metaHeader.RelatedJobs[0]).BF_CustomsStatus);
					AssertEquals("NOT", ((CusISFHeader)metaHeader.RelatedJobs[1]).BF_CustomsStatus);
					AssertEquals("NOT", ((CusISFHeader)metaHeader.RelatedJobs[2]).BF_CustomsStatus);
				});
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var isfJob1 = Factory.NewWithValidTestData<CusISFHeader>();
			var isfJob2 = Factory.NewWithValidTestData<CusISFHeader>();

			var jobCollection = new RelatedJobCollection(Factory);
			jobCollection.Add(isfJob1);
			jobCollection.Add(isfJob2);

			return new HVLVISFMetaHeader(jobCollection);
		}

		#endregion
	}
}
