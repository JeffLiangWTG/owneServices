using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.Common.US;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.DataTransfer.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Rating.Integration.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants.AWB;
using static Enterprise.Integration.Customs;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;
using IBaseJobComInvoiceHeader = Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Freight.Business.Testing
{
	public class CommonShipmentTest : BaseShipmentTest
	{
		#region CarrierOfBookedShippingLineIsNVOCC

		public void TestCarrierOfBookedShippingLineIsNVOCC()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsShippingLine = false;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OA_BookedShippingLineAddress = org.MainAddress.PK;

			Assert(shipment.CarrierOfBookedShippingLineIsNVOCC);

			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsShippingLine = true;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			Assert(shipment.CarrierOfBookedShippingLineIsNVOCC);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			Assert(!shipment.CarrierOfBookedShippingLineIsNVOCC);
		}

		#endregion

		#region TestCarrierOfBookedShippingLineIsCW1User()

		public void TestCarrierOfBookedShippingLineIsCW1User()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_IsCW1User = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OA_BookedShippingLineAddress = org.MainAddress.PK;

			Assert(shipment.CarrierOfBookedShippingLineIsCW1User);

			shippingLine.RSL_IsCW1User = false;
			Assert(!shipment.CarrierOfBookedShippingLineIsCW1User);

			org.OH_RSL_ShippingLine = ZGuid.Empty;
			Assert(!shipment.CarrierOfBookedShippingLineIsCW1User);

			shipment.JS_OA_BookedShippingLineAddress = ZGuid.Empty;
			Assert(!shipment.CarrierOfBookedShippingLineIsCW1User);
		}

		#endregion

		#region CarrierOfBookedShippingLineHasBookingRequestIntegration

		public void TestTestCarrierOfBookedShippingLineHasBookingRequestIntegration()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_BookingRequestAvailable = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OA_BookedShippingLineAddress = org.MainAddress.PK;

			Assert(shipment.CarrierOfBookedShippingLineHasBookingRequestIntegration);

			shippingLine.RSL_BookingRequestAvailable = false;
			Assert(!shipment.CarrierOfBookedShippingLineHasBookingRequestIntegration);

			org.OH_RSL_ShippingLine = ZGuid.Empty;
			Assert(!shipment.CarrierOfBookedShippingLineHasBookingRequestIntegration);

			shipment.JS_OA_BookedShippingLineAddress = ZGuid.Empty;
			Assert(!shipment.CarrierOfBookedShippingLineHasBookingRequestIntegration);
		}

		#endregion

		#region IOriginDestinationForDocumentDeliveryRestriction

		public void TestDocumentDeliveryRestriction()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USPHL";
			var documentDeliveryRestriction = shipment as IOriginDestinationForDocumentDeliveryRestriction;

			AssertNotNull(documentDeliveryRestriction);
			AssertEquals("OriginCountryCode should be AU", "AU", documentDeliveryRestriction.OriginCountryCode);
			AssertEquals("DestinationCountryCode should be US", "US", documentDeliveryRestriction.DestinationCountryCode);
		}

		#endregion

		#region ChargesDisplay

		public class ChargesDisplayTests : TransactionedTestCase
		{
			public void TestAsAgreedDefaulted()
			{
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = AsAgreedTypes.Codes.Collect;
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB = AsAgreedTypes.Codes.Prepaid;
				var factory = new BusinessObjectFactory();
				var shipment1 = factory.New<CommonShipment>();
				shipment1.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Expected JK_MBLAWBChargesDisplay to be CPD", ChargesApplyHelper.ChargesApplyConstants.CPD, shipment1.JS_HBLAWBChargesDisplay);

				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = AsAgreedTypes.Codes.None;
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB = AsAgreedTypes.Codes.All;
				var shipment2 = factory.New<CommonShipment>();
				shipment2.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Expected JK_MBLAWBChargesDisplay to be NAL", ChargesApplyHelper.ChargesApplyConstants.NAL, shipment2.JS_HBLAWBChargesDisplay);

				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = AsAgreedTypes.Codes.All;
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB = AsAgreedTypes.Codes.None;
				AssertEquals("Expected existing consol to be not changed", ChargesApplyHelper.ChargesApplyConstants.NAL, shipment2.JS_HBLAWBChargesDisplay);
			}

			public void TestChargesDisplaySeaShipmentsAndAir()
			{
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = AsAgreedTypes.Codes.Collect;
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB = AsAgreedTypes.Codes.Prepaid;
				DocumentsDataRegistry.Instance.HBLChargesDefaultDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed);

				var factory = new BusinessObjectFactory();
				var shipment = factory.New<CommonShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Expected JK_MBLAWBChargesDisplay to be CPD", ChargesApplyHelper.ChargesApplyConstants.CPD, shipment.JS_HBLAWBChargesDisplay);

				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				AssertEquals("Expected JK_MBLAWBChargesDisplay to be AGR", DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed, shipment.JS_HBLAWBChargesDisplay);
			}
		}

		#endregion

		#region TestAttachAndDetachEvents

		public void TestAttachAndDetachShipmentFromConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "ConsolRef";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "MasterRef";

			Factory.Save();

			consol.Shipments.Add(shipment);
			AssertEquals("ConsolRef|TYP=Consol", shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single().SL_Reference);

			Factory.Save();

			consol.Shipments.Remove(shipment);
			AssertEquals("ConsolRef|TYP=Consol", shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Detached.Code).Single().SL_Reference);
		}

		public void TestAttachShipmentToConsol_RemoveUnsavedATCEventWhenDetachShipmentFromConsol()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "ShipmentRef";

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "ConsolRef";

			consol.Shipments.Add(shipment);
			AssertEquals(1, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Count());

			consol.Shipments.Remove(shipment);
			AssertEquals("unsaved Attach event is removed when detach", 0, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Count());
			AssertEquals("Detach event is NOT logged", 0, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Detached.Code).Count());
		}

		public void TestAttachAndDetachSubshipment()
		{
			var masterShipment = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment.JS_UniqueConsignRef = "MasterRef";

			var subShipment = Factory.NewWithValidTestData<CommonShipment>();
			subShipment.JS_UniqueConsignRef = "SubshipmentRef";

			Factory.Save();

			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertEquals("MasterRef|TYP=Shipment", subShipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single().SL_Reference);

			Factory.Save();

			subShipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertEquals("MasterRef|TYP=Shipment", subShipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Detached.Code).Single().SL_Reference);
		}

		public void TestRemoveUnsavedATCEventWhenDetachFormColoadShipment()
		{
			var subShipment = Factory.NewWithValidTestData<CommonShipment>();
			subShipment.JS_UniqueConsignRef = "SubshipmentRef";

			var masterShipment = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment.JS_UniqueConsignRef = "MasterRef";

			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertEquals(1, subShipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Count());

			subShipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertEquals("unsaved Attach event is removed when detach", 0, subShipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Count());
			AssertEquals("Detach event is NOT logged", 0, subShipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Detached.Code).Count());
		}

		public void TestAttchSubShipment_FallbackATCEventReferenceToPKWhenSaveFailed()
		{
			var subShipment = Factory.NewWithValidTestData<CommonShipment>();
			var masterShipment = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment.JS_UniqueConsignRef = "MasterRef";

			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			var attachLog = subShipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single();

			using (attachLog.LockForUpdatingKeyFieldsForTesting())
			{
				attachLog.SL_Reference = "MasterRef|TYP=Shipment";
				masterShipment.OnSaved(false);

				AssertEquals(string.Format("{0}|TYP=Shipment", masterShipment.PK), subShipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single().SL_Reference);
			}
		}

		#endregion

		public void TestIsOriginDestinationInUSCustomsCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<CommonShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "PRSJU";
				shipment.JS_RL_NKDestination = "USPHL";
				var anotherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_Code));
				AssertEquals("Export Entry Type List should be shown for Entry Details Drop down list.", shipment.ShipmentCustomsEntryNumber.EntryType_List.CodesAsString, "ITN, NVC"); // EXPORT Entry Type even Direction is Import
				AssertEquals("PR, US", false, shipment.UseImportEntryTypeList); // Use ExportEntryTypeList in US
				shipment.JS_RL_NKDestination = "UKLON";
				AssertEquals("UK, US", false, shipment.UseImportEntryTypeList); // IsImport is false
				shipment.JS_RL_NKOrigin = "PRABC";
				shipment.JS_RL_NKDestination = "VIDEF";
				AssertEquals("PR, VI", false, shipment.UseImportEntryTypeList); // Use ExportEntryTypeList in US
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USPHL";
				AssertEquals("AU, US", true, shipment.UseImportEntryTypeList); // IsImport is true
			}
		}

		#region TestInspectionTypeForShipmentCreatedBeforeBrexit

		public void TestInspectionTypeForShipmentCreatedBeforeBrexit()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "GBLON";
			shipment.JS_RL_NKDestination = "AEDXB";
			shipment.JS_E_DEP = new ZDateTime(2020, 11, 20);
			shipment.JS_SystemCreateTimeUtc = new ZDateTime(2020, 11, 10);

			var number = shipment.LoadOrCreateInspectionTypeCusEntryNumber(Core.Constants.CountryCodes.EuropeanUnion);
			number.CE_EntryNum = BaseJobShipmentLookups.InspectionType_Approved;
			number.CE_SystemCreateTimeUtc = new ZDateTime(2020, 11, 11);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("InspectionTypeCode shows APP for EU company for shipment created before Brexit",
					BaseJobShipmentLookups.InspectionType_Approved, reloadedShipment.JS_InspectionTypeCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			using (GlbCompany.CurrentCompany.Country.TemporarilySetIsPartOfEuropeanUnion(false))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("InspectionTypeCode shows UNK for UK company for shipment created before Brexit",
					"UNK", reloadedShipment.JS_InspectionTypeCode);
			}
		}

		#endregion

		#region Containers

		public void TestContainersCollection()
		{
			var shipment1 = Factory.New<CommonShipment>();
			var shipment2 = Factory.New<CommonShipment>();
			var consol1 = Factory.New<CommonConsol>();
			var consol2 = Factory.New<CommonConsol>();

			consol1.Shipments.Add(shipment1);
			consol1.Shipments.Add(shipment2);
			consol2.Shipments.Add(shipment1);
			consol2.Shipments.Add(shipment2);

			var container1_1 = consol1.Containers.AddNew();
			var container1_2 = consol1.Containers.AddNew();
			var container2_1 = consol2.Containers.AddNew();
			var container2_2 = consol2.Containers.AddNew();

			var packLine1_1 = shipment1.OuterPackLines.AddNew();
			var packLine1_2 = shipment1.OuterPackLines.AddNew();
			var packLine2_1 = shipment2.OuterPackLines.AddNew();
			var packLine2_2 = shipment2.OuterPackLines.AddNew();

			//undo any packing that automatically allocated.
			UnpackAllContainers(shipment1);
			UnpackAllContainers(shipment2);

			container1_1.AddPackLine(packLine1_1);
			container1_1.AddPackLine(packLine2_1);
			container1_2.AddPackLine(packLine1_2);
			container2_1.AddPackLine(packLine1_1);
			container2_1.AddPackLine(packLine2_1);
			container2_2.AddPackLine(packLine2_2);

			AssertContainsExactElementsInAnyOrder(new[] { container1_1, container1_2, container2_1 }, shipment1.Containers);
			AssertContainsExactElementsInAnyOrder(new[] { container1_1, container2_1, container2_2 }, shipment2.Containers);
		}

		public void TestUnpackFromContainer()
		{
			var shipment = Factory.New<CommonShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			var packLine3 = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			container1.AddPackLine(packLine1);
			container2.AddPackLine(packLine2);
			container2.AddPackLine(packLine3);

			AssertContainsExactElementsInAnyOrder(new[] { container1, container2 }, shipment.Containers);
			AssertEquals(2, container2.PackLines.Count);
			AssertEquals(1, packLine2.Containers.Count);
			AssertEquals(1, packLine3.Containers.Count);

			shipment.UnpackFromContainer(container2);

			AssertContainsExactElementsInAnyOrder(new[] { container1 }, shipment.Containers);
			AssertEquals(0, container2.PackLines.Count);
			AssertEquals(0, packLine2.Containers.Count);
			AssertEquals(0, packLine3.Containers.Count);
		}

		void UnpackAllContainers(CommonShipment shipment)
		{
			foreach (var container in shipment.Containers.ToArray())
			{
				shipment.UnpackFromContainer(container);
			}
		}

		#endregion

		#region StrictBizPropertyInfos

		public void TestStrictBizPropertyInfos_ConcurrencyPolicy()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";

			CombineAssertions(() =>
			{
				var message = "The concurrency policy should default to ConcurrencyPolicy.Default.";

				foreach (var info in shipment.StrictBizPropertyInfos)
				{
					AssertEquals(message, ConcurrencyPolicy.Default.AllowMerge, info.ConcurrencyPolicy.AllowMerge);
					AssertEquals(message, ConcurrencyPolicy.Default.CollisionCheck, info.ConcurrencyPolicy.CollisionCheck);
				}
			});

			Factory.Save();

			CombineAssertions(() =>
			{
				var message = "The concurrency policy should is ConcurrencyPolicy.Default as the shipment just saved when it is not InDatabase.";

				foreach (var info in shipment.StrictBizPropertyInfos)
				{
					AssertEquals(message, ConcurrencyPolicy.Default.AllowMerge, info.ConcurrencyPolicy.AllowMerge);
					AssertEquals(message, ConcurrencyPolicy.Default.CollisionCheck, info.ConcurrencyPolicy.CollisionCheck);
				}
			});

			shipment.JS_RL_NKOrigin = "SGSIN";
			Factory.Save();

			CombineAssertions(() =>
			{
				var message = "The concurrency policy should is ConcurrencyPolicy.Strict as the shipment just saved when it is InDatabase.";

				foreach (var info in shipment.StrictBizPropertyInfos)
				{
					AssertEquals(message, ConcurrencyPolicy.Strict.AllowMerge, info.ConcurrencyPolicy.AllowMerge);
					AssertEquals(message, ConcurrencyPolicy.Strict.CollisionCheck, info.ConcurrencyPolicy.CollisionCheck);
				}
			});
		}

		public void TestHasCriticalChangesOnProperty_PackingMode()
		{
			var originalValue = new ZString(Constants.ContainerModes.LCL);
			var newValue = new ZString(Constants.ContainerModes.FCL);

			AssertHasCriticalChangeOnProperty(JobShipmentSchema.JS_PackingMode.Name, originalValue, newValue);
		}

		public void TestHasCriticalChangesOnProperty_TransportMode()
		{
			var originalValue = new ZString(Constants.TransportModes.Sea);
			var newValue = new ZString(Constants.TransportModes.Air);

			AssertHasCriticalChangeOnProperty(JobShipmentSchema.JS_TransportMode.Name, originalValue, newValue);
		}

		public void TestHasCriticalChangesOnProperty_ShipmentType()
		{
			var originalValue = new ZString(Constants.ShipmentTypes.StandardHouse);
			var newValue = new ZString(Constants.ShipmentTypes.CoLoadMaster);

			AssertHasCriticalChangeOnProperty(JobShipmentSchema.JS_ShipmentType.Name, originalValue, newValue);
		}

		void AssertHasCriticalChangeOnProperty(string propetyName, IZType originalValue, IZType newValue)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			var info = shipment.FindPropertyInfo(propetyName);
			info.Value = originalValue;

			AssertEquals("Should be false as the shipment is not in the database", false, shipment.HasCriticalChangesOnProperty(info));

			Factory.Save();

			var newFactory = NewFactory();
			var newShipment = newFactory.Load<CommonShipment>(shipment.PK);

			var newInfo = newShipment.FindPropertyInfo(propetyName);
			newInfo.Value = newValue;

			newFactory.Save();

			AssertEquals("Should be updated by the data refresh bus", newValue, info.Value);
			AssertEquals("Should be false as the change happened at the data row layer", false, info.HasChanges);
			AssertEquals("Should be true as the change didn't do any related business changes", true, shipment.HasCriticalChangesOnProperty(info));
		}

		#endregion

		#region TestShipmentJobHeaderWithTwoDifferentCompanies

		public void TestDoNotRegisterShipmentJobHeaderAsChildEditableForWorkflow()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var job = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();

			AssertNotNull("prerequisite; job has been created", job);

			Assert("shipment job header has not yet been registered as child editable",
					!shipment.IsRegisteredEditableChildObject(job));

			_ = shipment.ShipmentJobHeader;

			Assert("shipment job header has been registered as child editable",
					shipment.IsRegisteredEditableChildObject(job));

			Factory.Save();

			ReleaseFactory();

			var shipment2 = Factory.Load<CommonShipment>(shipment.PK);

			var bizObjWithRelatedEvents = shipment2.BusinessObjectsWithRelatedEvents;
			var job2 = bizObjWithRelatedEvents.FirstOrDefault(bizObj => bizObj.PK == job.PK);

			AssertNotNull("job header has been found in BusinessObjectsWithRelatedEvents", job2);

			Assert("shipment job header found in BusinessObjectsWithRelatedEvents has not been registered as child editable",
					!shipment2.IsRegisteredEditableChildObject(job2));
		}

		public void TestShipmentJobHeaderWithTwoDifferentCompanies()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var job1 = new JobHeader.Loader(Factory, shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(job1);
			Factory.Save();

			var anotherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_Code));
			Assert(anotherCompany.Branches.Count > 0);
			AssertNotEquals(GlbCompany.CurrentCompany.PK, anotherCompany.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var anotherCompanyFactory = new BusinessObjectFactory();
				var job2 = new JobHeader.Loader(anotherCompanyFactory, shipment).TryLoadOrCreateWithoutMutexForTestOnly();
				AssertNotNull(job2);
				AssertNotEquals(job1.PK, job2.PK);
				anotherCompanyFactory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<CommonShipment>(shipment.PK);
			var job1Header = shipmentInNewFactory.ShipmentJobHeader;
			var job1PK = job1Header.PK;
			AssertEquals(1, GetLocalChargesAddrChangedEventHandlerCount(job1Header));

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job2PK = shipmentInNewFactory.ShipmentJobHeader.PK;
				AssertNotEquals("the shipment job should be reload if the login company has changed", job1PK, job2PK);

				Assert("current company shipment job header has been registered as child editable",
				shipmentInNewFactory.IsRegisteredEditableChildObject(shipmentInNewFactory.ShipmentJobHeader));

				Assert("Job header in previous company can be bound to a form we must not unregister it as it will not be validated before saving.",
					shipmentInNewFactory.IsRegisteredEditableChildObject(job1Header));

				AssertEquals(0, GetLocalChargesAddrChangedEventHandlerCount(job1Header));
			}
		}

		int GetLocalChargesAddrChangedEventHandlerCount(JobHeader jobHeader)
		{
			var jobHeaderType = jobHeader.GetType().BaseType;
			var eventField = jobHeaderType.GetField("LocalChargesAddrChanged", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
			if (eventField != null)
			{
				var eventDelegate = (EventHandler)eventField.GetValue(jobHeader);
				var handlerCount = eventDelegate?.GetInvocationList().Length;
				return handlerCount.GetValueOrDefault();
			}

			return 0;
		}

		#endregion

		public void TestRequiredDocumentAddedForImportDirection()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_DEP = ZDateTime.Today;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKDestination = "AUSYD";

			var validToDate = ZDate.Today.AddDays(5);
			var requiredDocument1 = AddNewJobRequiredDoc(consignee, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001", ZGuid.Empty);
			var requiredDocument2 = AddNewJobRequiredDoc(consignee, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0002", consignor.PK);
			var requiredDocument3 = AddNewJobRequiredDoc(consignee, Constants.RefDocTypes.DeliveryOrder, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0003", orgHeader.PK);

			var requiredDocument4 = AddNewJobRequiredDoc(consignor, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0004", ZGuid.Empty);
			var requiredDocument5 = AddNewJobRequiredDoc(consignor, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0005", consignor.PK);
			var requiredDocument6 = AddNewJobRequiredDoc(consignor, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0006", orgHeader.PK);
			Factory.Save();

			var agentsInvoiceDocuments = shipment.DocsAndCartage.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.AgentsInvoice).OrderBy(doc => doc.EQ_DocNumber);
			var deliveryOrderDocuments = shipment.DocsAndCartage.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.DeliveryOrder).OrderBy(doc => doc.EQ_DocNumber);

			AssertEquals("shipment should have 3 AgentsInvoice required documents added.", 2, agentsInvoiceDocuments.Count());
			AssertEquals("shipment should have 1 DeliveryOrder required documents added.", 1, deliveryOrderDocuments.Count());
			AssertEquals("2 AgentsInvoice required documents added were from supplier relationship.", "0001,0002", string.Join(",", agentsInvoiceDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
			AssertEquals("1 DeliveryOrder required documents added was from consignee.", "0003", string.Join(",", deliveryOrderDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
		}

		public void TestRequiredDocumentAddedForSupplierLink()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_DEP = ZDateTime.Today;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKDestination = "AUSYD";

			var validToDate = ZDate.Today.AddDays(5);
			var requiredDocument1 = AddNewJobRequiredDoc(consignee, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001", ZGuid.Empty);
			var requiredDocument2 = AddNewJobRequiredDoc(consignee, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0002", consignor.PK);
			var requiredDocument3 = AddNewJobRequiredDoc(consignee, Constants.RefDocTypes.DeliveryOrder, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0003", orgHeader.PK);

			var supplierLink = consignee.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = consignor.PK;
			var requiredDocument4 = AddNewJobRequiredDoc(supplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0004", ZGuid.Empty);
			var requiredDocument5 = AddNewJobRequiredDoc(supplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0005", consignor.PK);
			var requiredDocument6 = AddNewJobRequiredDoc(supplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0006", orgHeader.PK);
			Factory.Save();

			var agentsInvoiceDocuments = shipment.DocsAndCartage.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.AgentsInvoice).OrderBy(doc => doc.EQ_DocNumber);
			var deliveryOrderDocuments = shipment.DocsAndCartage.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.DeliveryOrder).OrderBy(doc => doc.EQ_DocNumber);

			AssertEquals("shipment should have 3 AgentsInvoice required documents added.", 3, agentsInvoiceDocuments.Count());
			AssertEquals("shipment should have 1 DeliveryOrder required documents added.", 1, deliveryOrderDocuments.Count());
			AssertEquals("3 AgentsInvoice required documents added were from supplier relationship.", "0004,0005,0006", string.Join(",", agentsInvoiceDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
			AssertEquals("1 DeliveryOrder required documents added was from consignee.", "0003", string.Join(",", deliveryOrderDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
		}

		JobRequiredDocument AddNewJobRequiredDoc(IHaveRequiredDocuments org, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDate date, ZString docNumber, ZGuid documentOwner)
		{
			JobRequiredDocument result = org.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			result.EQ_OH_DocumentOwner = documentOwner;
			return result;
		}

		public void TestGetControllingCustomerSecurityCheckPoint()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals(Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir.Code, shipment.GetControllingCustomerSecurityCheckPoint().Code);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerSea.Code, shipment.GetControllingCustomerSecurityCheckPoint().Code);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerRoad.Code, shipment.GetControllingCustomerSecurityCheckPoint().Code);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerRail.Code, shipment.GetControllingCustomerSecurityCheckPoint().Code);

			shipment.JS_TransportMode = Constants.TransportModes.Courier;
			AssertEquals(Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.Code, shipment.GetControllingCustomerSecurityCheckPoint().Code);
		}

		public void TestAdditionalValidationOnShipmentCustomsEntryNumber()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ShipmentCustomsEntryNumber.EntryTypeInfo.AdditionalValidation += () => { shipment.ShipmentCustomsEntryNumber.EntryTypeInfo.AddError("Error added"); };
			shipment.RunPreSaveValidation();
			AssertHasError(shipment.ShipmentCustomsEntryNumber.EntryTypeInfo, "Error added");
		}

		public void TestGetMandatoryControllingAgentEffectiveDateRegistry()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			AssertEquals(FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateAll, shipment.GetMandatoryControllingAgentEffectiveDateRegistry());

			using (FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateAir.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateAir, shipment.GetMandatoryControllingAgentEffectiveDateRegistry());
			}

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			using (FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateSea.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateSea, shipment.GetMandatoryControllingAgentEffectiveDateRegistry());
			}

			shipment.JS_TransportMode = Constants.TransportModes.Road;

			using (FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateRoad.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateRoad, shipment.GetMandatoryControllingAgentEffectiveDateRegistry());
			}

			shipment.JS_TransportMode = Constants.TransportModes.Rail;

			using (FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateRail.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateRail, shipment.GetMandatoryControllingAgentEffectiveDateRegistry());
			}

			shipment.JS_TransportMode = Constants.TransportModes.Courier;

			using (FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateAll.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateAll, shipment.GetMandatoryControllingAgentEffectiveDateRegistry());
			}
		}

		public void TestGetMandatoryControllingCustomerEffectiveDateRegistry()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			AssertEquals(FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateAll, shipment.GetMandatoryControllingCustomerEffectiveDateRegistry());

			using (FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateAir.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateAir, shipment.GetMandatoryControllingCustomerEffectiveDateRegistry());
			}

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			using (FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateSea.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateSea, shipment.GetMandatoryControllingCustomerEffectiveDateRegistry());
			}

			shipment.JS_TransportMode = Constants.TransportModes.Road;

			using (FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateRoad.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateRoad, shipment.GetMandatoryControllingCustomerEffectiveDateRegistry());
			}

			shipment.JS_TransportMode = Constants.TransportModes.Rail;

			using (FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateRail.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateRail, shipment.GetMandatoryControllingCustomerEffectiveDateRegistry());
			}

			shipment.JS_TransportMode = Constants.TransportModes.Courier;

			using (FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateAll.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			{
				AssertEquals(FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateAll, shipment.GetMandatoryControllingCustomerEffectiveDateRegistry());
			}
		}

		public void TestHasRelatedNotes_BothConsigneeAndConsignor()
		{
			TestRelatedNotes_Helper(addConsigneeNotes: true, addConsignorNotes: true);
		}

		public void TestHasRelatedNotes_Consignee()
		{
			TestRelatedNotes_Helper(addConsigneeNotes: true, addConsignorNotes: false);
		}

		public void TestHasRelatedNotes_Consignor()
		{
			TestRelatedNotes_Helper(addConsigneeNotes: false, addConsignorNotes: true);
		}

		public void TestHasRelatedNotes_WhenThereIsNoUnrelatedNotes()
		{
			TestRelatedNotes_Helper(addConsigneeNotes: false, addConsignorNotes: false);
		}

		void TestRelatedNotes_Helper(bool addConsigneeNotes, bool addConsignorNotes)
		{
			var shouldHaveNotes = addConsigneeNotes || addConsignorNotes;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			if (addConsigneeNotes)
			{
				consignee.Notes.AddNew(false, "Internal Work Notes", "Here is some unread text");
			}

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			if (addConsignorNotes)
			{
				consignor.Notes.AddNew(false, "Internal Work Notes", "Here is some more unread text");
			}

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			Assert("PRE: Not in database", !shipment.IsInDatabase);

			Assert("Initially, there are no related notes", !shipment.Notes.HasRelatedNotes);

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			AssertEquals(shouldHaveNotes, shipment.Notes.HasRelatedNotes);
			Factory.Save();

			AssertEquals("Should be consistent after it is saved", shouldHaveNotes, shipment.Notes.HasRelatedNotes);
		}

		public void TestHBLRequiredDoc_Direction()
		{
			AssertHBLDirection("AUSYD", "USLAX", JobRequiredDocument.DocUsage.Import);
			AssertHBLDirection("USLAX", "AUSYD", JobRequiredDocument.DocUsage.Import);
			AssertHBLDirection("GBLON", "USLAX", JobRequiredDocument.DocUsage.Import);
			AssertHBLDirection("AUSYD", "AUBNE", JobRequiredDocument.DocUsage.Domestic);
		}

		void AssertHBLDirection(ZString origin, ZString destination, ZString expectedDirection)
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			var hblDoc = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill);
			AssertEquals(expectedDirection, hblDoc.EQ_DocUsage);
		}

		public void TestNeedsHouseBill_Booking()
		{
			var shipment = Factory.New<CommonShipmentForTest>();

			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "AUSYD";

			string[] transportMode = new string[]
			{
				Constants.TransportModes.Air,
				Constants.TransportModes.Sea,
				Constants.TransportModes.Road,
				Constants.TransportModes.Rail
			};

			foreach (var mode in transportMode)
			{
				shipment.JS_TransportMode = mode;

				shipment.JS_IsBooking = true;
				shipment.JS_IsDirectBooking = false;
				Assert(shipment.NeedsHouseBillForTest);

				shipment.JS_IsDirectBooking = true;
				Assert(!shipment.NeedsHouseBillForTest);

				shipment.JS_IsBooking = false;
				Assert(shipment.NeedsHouseBillForTest);

				shipment.JS_IsDirectBooking = false;
				Assert(shipment.NeedsHouseBillForTest);
			}
		}

		public void TestDefaultJS_RL_NKOrigin() => TestShipmentDefaultPort(GlbBranchDefaultToList.Codes.ShipmentOrigin);

		public void TestDefaultJS_RL_NKDestination() => TestShipmentDefaultPort(GlbBranchDefaultToList.Codes.ShipmentDestination);

		void TestShipmentDefaultPort(string defaultTo)
		{
			SetDepartment(defaultTo == GlbBranchDefaultToList.Codes.ShipmentDestination, defaultTo == GlbBranchDefaultToList.Codes.ShipmentOrigin, false);
			var branch = GlbBranch.CurrentBranch;
			branch.GB_RL_NKHomePort = "AUSYD";
			GlbBranchDefaultPortTestHelper.AddDefaultPort(branch, defaultTo, "SEA", "FCL", "AUMEL");
			branch.Factory.Save();

			var defaultContainerModes = new DefaultContainerModesCollection
			{
				new DefaultContainerModes
				{
					TransportMode = Constants.TransportModes.Sea,
					ContainerMode = Constants.ContainerModes.FCL
				},
				new DefaultContainerModes
				{
					TransportMode = Constants.TransportModes.Air,
					ContainerMode = Constants.ContainerModes.Loose
				}
			};
			using (FreightConfigurationRegistry.Instance.DefaultContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultContainerModes))
			{
				SetDepartmentTransportMode("SEA");
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("AUMEL", defaultTo == GlbBranchDefaultToList.Codes.ShipmentDestination ? shipment.JS_RL_NKDestination : shipment.JS_RL_NKOrigin);

				SetDepartmentTransportMode("AIR");
				shipment = Factory.New<CommonShipment>();
				AssertEquals("AUSYD", defaultTo == GlbBranchDefaultToList.Codes.ShipmentDestination ? shipment.JS_RL_NKDestination : shipment.JS_RL_NKOrigin);
			}
		}

		public void TestDefaultRelatedParty()
		{
			var consignor = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var cnrRelatedParty = Factory.New<OrgHeader>();
			var cneRelatedParty = Factory.New<OrgHeader>();

			consignor.AddRelatedParty(cnrRelatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);
			consignee.AddRelatedParty(cneRelatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);

			var shipment = Factory.New<CommonShipment>();

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Constants.TransportModes.Courier;

			AssertEquals(cnrRelatedParty.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals(cneRelatedParty.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultRelatedParty_WhenTransportModeIsFAS()
		{
			var consignor = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var cnrRelatedParty = Factory.New<OrgHeader>();
			var cneRelatedParty = Factory.New<OrgHeader>();

			consignor.AddRelatedParty(cnrRelatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty, GlbCompany.CurrentCompany);
			consignee.AddRelatedParty(cneRelatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty, GlbCompany.CurrentCompany);

			var shipment = Factory.New<CommonShipment>();

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Constants.TransportModes.AirSea;

			AssertEquals(cnrRelatedParty.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals(cneRelatedParty.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultRelatedParty_WhenTransportModeIsFSA()
		{
			var consignor = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var cnrRelatedParty = Factory.New<OrgHeader>();
			var cneRelatedParty = Factory.New<OrgHeader>();

			consignor.AddRelatedParty(cnrRelatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty, GlbCompany.CurrentCompany);
			consignee.AddRelatedParty(cneRelatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty, GlbCompany.CurrentCompany);

			var shipment = Factory.New<CommonShipment>();

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;

			AssertEquals(cnrRelatedParty.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals(cneRelatedParty.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCompanyUsesLTTAddressFallback()
		{
			var consignee = Factory.New<OrgHeader>();

			var shipment = Factory.New<CommonShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var lttNoAddressOrg = Factory.New<OrgHeader>();
			consignee.SetRelatedParty(lttNoAddressOrg, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, ZString.Empty);

			var lttWithAddressOrg = Factory.New<OrgHeader>();
			var lttAddressPK = shipment.ConsigneeDeliveryAddress.E2_OA_Address;
			consignee.SetRelatedParty(lttAddressPK, lttWithAddressOrg, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty, ZString.Empty);

			AssertEquals(2, consignee.AllRelatedParties.Count);

			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals(lttWithAddressOrg.MainAddress.PK, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);

			var orgRelatedPartyWithAddress = consignee.AllRelatedParties.GetRelatedParty(lttAddressPK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty, ZString.Empty);
			orgRelatedPartyWithAddress.PR_OA = ZGuid.NewZGuid();

			shipment.JS_RL_NKDestination = "AUPER";
			AssertEquals(lttNoAddressOrg.MainAddress.PK, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);
		}

		public void TestDefaultPickupCompanyUsesLTTAddressFallback()
		{
			var consignor = Factory.New<OrgHeader>();

			var shipment = Factory.New<CommonShipment>();
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var lttNoAddressOrg = Factory.New<OrgHeader>();
			consignor.SetRelatedParty(lttNoAddressOrg, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, ZString.Empty);

			var lttWithAddressOrg = Factory.New<OrgHeader>();
			var lttAddressPK = shipment.ConsignorPickupAddress.E2_OA_Address;
			consignor.SetRelatedParty(lttAddressPK, lttWithAddressOrg, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty, ZString.Empty);

			AssertEquals(2, consignor.AllRelatedParties.Count);

			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals(lttWithAddressOrg.MainAddress.PK, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr);

			var orgRelatedPartyWithAddress = consignor.AllRelatedParties.GetRelatedParty(lttAddressPK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty, ZString.Empty);
			orgRelatedPartyWithAddress.PR_OA = ZGuid.NewZGuid();

			shipment.JS_RL_NKOrigin = "AUPER";
			AssertEquals(lttNoAddressOrg.MainAddress.PK, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr);
		}

		public void TestCommunityTransitStatus()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_CommunityTransitStatus = "X";
			AssertEquals("X", shipment.JS_CommunityTransitStatus);
			Factory.Save();
			shipment = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			AssertEquals("X", shipment.JS_CommunityTransitStatus);

			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			Assert(shipment.Lookups.CommunityTransitStatusCodes.ContainsCode("X"));
			Assert(shipment.Lookups.CommunityTransitStatusCodes.ContainsCode("T2LSM"));

			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			Assert(shipment.Lookups.CommunityTransitStatusCodes.ContainsCode("X"));
			Assert(shipment.Lookups.CommunityTransitStatusCodes.ContainsCode("T2LSM"));
		}

		public void TestCloneSubShipmentsLinksToMasterCorrectly()
		{
			var master = Factory.New<CommonShipment>();
			master.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			master.JS_OuterPacks = 0;
			master.JS_ActualWeight = 0m;
			master.JS_ActualVolume = 0m;
			master.JS_JS_ColoadMasterShipmentForBinding = ZGuid.Empty;
			AssertEquals("Precondition - CoLoadShipments count", 0, master.CoLoadShipments.Count);

			var sub1 = (CommonShipment)master.Clone();
			sub1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			sub1.JS_JS_ColoadMasterShipmentForBinding = master.PK;

			AssertEquals("CoLoadShipments count", 1, master.CoLoadShipments.Count);
			AssertEquals("Master outer packs", 0, master.JS_OuterPacks);
			AssertEquals("Master actual weight", 0m, master.JS_ActualWeight);
			AssertEquals("Master actual volume", 0m, master.JS_ActualVolume);

			sub1.JS_OuterPacks = 10;
			sub1.JS_ActualWeight = 1000m;
			sub1.JS_ActualVolume = 10m;
			AssertEquals("Master Outer Packs - Single sub", 10, master.JS_OuterPacks);
			AssertEquals("Master Actual Weight - Single sub", 1000m, master.JS_ActualWeight);
			AssertEquals("Master Actual Volume - Single sub", 10m, master.JS_ActualVolume);

			var sub2 = (CommonShipment)sub1.Clone();

			AssertEquals("CoLoadShipments count", 2, master.CoLoadShipments.Count);
			AssertEquals("Master Outer Packs - Two subs", 20, master.JS_OuterPacks);
			AssertEquals("Master Actual Weight - Two subs", 2000m, master.JS_ActualWeight);
			AssertEquals("Master Actual Volume - Two subs", 20m, master.JS_ActualVolume);

			sub2.JS_OuterPacks = 20;
			sub2.JS_ActualWeight = 2000m;
			sub2.JS_ActualVolume = 20m;
			AssertEquals("Master Outer Packs - Two subs", 30, master.JS_OuterPacks);
			AssertEquals("Master Actual Weight - Two subs", 3000m, master.JS_ActualWeight);
			AssertEquals("Master Actual Volume - Two subs", 30m, master.JS_ActualVolume);
		}

		public void TestCustomsEntryFieldsAreAvailableActionFields()
		{
			PropertyInfo entryNumberTypeInfo = typeof(CommonShipment).GetProperty("CustomsEntryNumberType");
			PropertyInfo entryNumberInfo = typeof(CommonShipment).GetProperty("CustomsEntryNumber");

			ActionFieldAttribute attEntryNumberType = ActionFieldAttribute.Get(entryNumberTypeInfo);
			ActionFieldAttribute attEntryNumber = ActionFieldAttribute.Get(entryNumberInfo);

			AssertNotNull(attEntryNumberType);
			AssertNotNull(attEntryNumber);

			AssertEquals("MaxLength of EntryNumberType should be the maxlength of CE_EntryType", CusEntryNumSchema.CE_EntryType.MaxLength, attEntryNumberType.MaxLength);
			AssertEquals("MaxLength of EntryNumberType should be the maxlength of CE_EntryNum", CusEntryNumSchema.CE_EntryNum.MaxLength, attEntryNumber.MaxLength);
		}

		#region IsDirectShipment

		public void TestIsDirectShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			Assert("Shipment with no Consols is not direct", !shipment.IsDirectShipment);

			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Constants.AgentType.Agent;
			Assert("Direct Shipment must have Direct Consols", !shipment.IsDirectShipment);
			consol.JK_AgentType = Constants.AgentType.Direct;
			Assert("Direct Shipment must have Direct Consols", shipment.IsDirectShipment);

			shipment.JS_JS_ColoadMasterShipment = Factory.New<CommonShipment>().PK;
			shipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			Assert("STD Shipment with Coload Master is direct", shipment.IsDirectShipment);
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			Assert("ASM Shipment with Coload Master is not direct", !shipment.IsDirectShipment);
			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			Assert("Shipment with no Coload Master is direct", shipment.IsDirectShipment);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			Assert("Only StandardHouse shipments or AssemblyMaster shipments with no Master/Lead are direct", !shipment.IsDirectShipment);
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			Assert("Only StandardHouse shipments or AssemblyMaster shipments with no Master/Lead are direct", shipment.IsDirectShipment);
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			Assert("Only StandardHouse shipments or AssemblyMaster shipments with no Master/Lead are direct", shipment.IsDirectShipment);
		}

		#endregion

		#region DocumentTrackingUpdatedOnSaving

		#region TestOnSavingCountryRequiredDocuments

		public void TestOnSavingCountryRequiredDocuments()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInstruction, "", "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc3 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ArrivalNotice, aUS, "", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc4 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BankDraft, "", "", JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc5 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BillOfEntry, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Rail, true);
			RefCountryRequiredDocument requiredDoc6 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.CartageAdvice, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, false);
			RefCountryRequiredDocument requiredDoc7 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ChargeSheet, "US", "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc8 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.DelayAlert, aUS, "US", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			RefCountryRequiredDocument requiredDoc9 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.Sea, true);

			Factory.Save();

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = LocalConsignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = OverseasConsignee.PK;
			shipment.JS_RL_NKOrigin = "AU";
			shipment.JS_RL_NKDestination = "NZ";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();

			AssertNull("Doesn't match, shouldn't be in the list", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BillOfEntry));
			AssertNull(shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.CartageAdvice));
			AssertNull(shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ChargeSheet));
			AssertNull(shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DelayAlert));

			AssertEquals(4, shipment.DocsAndCartage.RequiredDocuments.Count);

			JobRequiredDocument rdAgentInvoice = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", rdAgentInvoice);
			AssertEquals("AgentInvoice: DocUsage should be BTH", "BTH", rdAgentInvoice.EQ_DocUsage);

			JobRequiredDocument rdAgentsInstruction = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInstruction);
			AssertNotNull("Should be in the list", rdAgentsInstruction);
			AssertEquals("DocUsage should be IMP", "IMP", rdAgentsInstruction.EQ_DocUsage);

			JobRequiredDocument rdArrivalNotice = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ArrivalNotice);
			AssertNotNull("Should be in the list", rdArrivalNotice);
			AssertEquals("DocUsage should be EXP", "EXP", rdArrivalNotice.EQ_DocUsage);

			JobRequiredDocument rdBankDraft = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BankDraft);
			AssertNotNull("Should be in the list", rdBankDraft);
			AssertEquals("DocUsage should be BTH", "BTH", rdBankDraft.EQ_DocUsage);

			RefCountry countryAU2 = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			RefCountryRequiredDocument requiredDoc10 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.AgentsInvoice, aUS, aUS, JobRequiredDocument.DocUsage.Domestic, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc11 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.ChargeSheet, aUS, aUS, JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc12 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.DangerousGoodsForm, "", "", JobRequiredDocument.DocUsage.Domestic, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc13 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.EFTRequest, aUS, aUS, JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc14 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.EntryPrint, "", "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc15 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.HouseBill, aUS, "CY", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			RefCountryRequiredDocument requiredDoc16 = GetNewRequiredDoc(countryAU2, Constants.RefDocTypes.MasterHouse, aUS, "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = LocalConsignor.PK;
			shipment2.ConsigneeDocumentaryAddress.OrganisationPK = LocalConsignee.PK;
			shipment2.JS_RL_NKOrigin = "AU";
			shipment2.JS_RL_NKDestination = "AU";
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();

			AssertNull("Doesn't match, shouldn't be in the list", shipment2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ChargeSheet));
			AssertNull(shipment2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));

			JobRequiredDocument rdAgentInvoice2 = shipment2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", rdAgentInvoice2);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", rdAgentInvoice2.EQ_DocUsage);

			JobRequiredDocument dangerousGoodsForm = shipment2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm);
			AssertNotNull("AgentInvoice: Should be in the list", dangerousGoodsForm);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", dangerousGoodsForm.EQ_DocUsage);

			JobRequiredDocument entryPrint = shipment2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.EntryPrint);
			AssertNotNull("AgentInvoice: Should be in the list", entryPrint);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", entryPrint.EQ_DocUsage);

			JobRequiredDocument eFTRequest = shipment2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.EFTRequest);
			AssertNotNull("AgentInvoice: Should be in the list", eFTRequest);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", eFTRequest.EQ_DocUsage);

			JobRequiredDocument masterHouse = shipment2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MasterHouse);
			AssertNotNull("AgentInvoice: Should be in the list", masterHouse);
			AssertEquals("AgentInvoice: DocUsage should be DOM", "DOM", masterHouse.EQ_DocUsage);
		}

		#endregion

		#region TestOnSavingOrganisationRequiredDocuments

		[TestDate(2007, 3, 15, 10, 15, 21)]
		public void TestOnSavingOrganisationRequiredDocuments()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BeneficiaryCertificate, "", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass
			RefCountryRequiredDocument testRequiredDoc6 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.VetinaryCertificate, "", "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "AUSYD";

			JobRequiredDocument requiredDoc3 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0001");
			JobRequiredDocument requiredDoc4 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0002");
			JobRequiredDocument requiredDoc5 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0003");
			JobRequiredDocument requiredDoc6 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0004");
			requiredDoc6.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-2);
			JobRequiredDocument requiredDoc7 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0005");

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = organisation.PK;
			OverseasConsignee.OH_RL_NKClosestPort = "NZAKL";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = OverseasConsignee.PK;
			shipment.JS_RL_NKOrigin = "AU";
			shipment.JS_RL_NKDestination = "NZ";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(1);
			Factory.Save();

			JobRequiredDocument rdSanitaryCertificate = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate);
			AssertNotNull("SanitaryCertificate: Should be in the list", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate));
			AssertEquals("SanitaryCertificate: Period should be periodic", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdSanitaryCertificate.EQ_DocPeriod);

			JobRequiredDocument rdAgentInvoice = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));

			JobRequiredDocument rdVetinaryCertificate = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate);
			AssertNotNull("VetinaryCertificate: Should be in the list", rdVetinaryCertificate);
			AssertEquals("VetinaryCertificate: Date Recieved should be two days ago date", ZDateTimeOffset.Today.AddDays(-2), rdVetinaryCertificate.EQ_DateReceived);

			AssertNotNull(shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertNotNull(shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));
			AssertNotNull(shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));
		}

		#endregion

		#region TestOnSavingBuyerSupplierRequiredDocuments

		[TestDate(2007, 3, 15, 10, 15, 21)]
		public void TestOnSavingBuyerSupplierRequiredDocuments()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BeneficiaryCertificate, "", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_RL_NKClosestPort = "NZAKL";

			JobRequiredDocument requiredDoc3 = GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0001");

			OrgSupplierBuyerLink buyerSupplierLink1 = consignor.BuyerLinks.AddNew(consignee);
			buyerSupplierLink1.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.NewZealand;

			JobRequiredDocument requiredDoc4 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0002");
			JobRequiredDocument requiredDoc5 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0003");
			JobRequiredDocument requiredDoc6 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0004");
			requiredDoc6.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-2);
			JobRequiredDocument requiredDoc7 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0005");
			JobRequiredDocument requiredDoc8 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0006");

			OrgSupplierBuyerLink buyerSupplierLink2 = consignor.BuyerLinks.AddNew(consignee);
			buyerSupplierLink2.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			JobRequiredDocument requiredDoc9 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink2, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-3), ZDateTime.Today.Date.AddDays(3), "0007");

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AU";
			shipment.JS_RL_NKDestination = "SG";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(1);
			Factory.Save();

			JobRequiredDocument rdVetinaryCertificate = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate);
			AssertNotNull("VetinaryCertificate: Should be in the list", rdVetinaryCertificate);
			AssertEquals("VetinaryCertificate: Period should be periodic", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdVetinaryCertificate.EQ_DocPeriod);
			AssertEquals(ZDateTimeOffset.Today.AddDays(-2), rdVetinaryCertificate.EQ_DateReceived);

			JobRequiredDocument rdBeneficiaryCertificate = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate);
			AssertNotNull("BeneficiaryCertificate: Should be in the list", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));

			JobRequiredDocument rdAgentsInvoice = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertEquals("AgentInvoice: Date Received should be two days ago date", ZDateTimeOffset.Today.AddDays(-2), rdAgentsInvoice.EQ_DateReceived);

			AssertNotNull(shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate));
			AssertNotNull(shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));

			shipment = Factory.New<CommonShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AU";
			shipment.JS_RL_NKDestination = "US";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(1);
			Factory.Save();

			rdVetinaryCertificate = shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate);
			AssertNotNull("VetinaryCertificate: Should be in the list", rdVetinaryCertificate);
			AssertEquals("VetinaryCertificate: Period should be periodic", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdVetinaryCertificate.EQ_DocPeriod);
			AssertEquals(ZDateTimeOffset.Today.AddDays(-3), rdVetinaryCertificate.EQ_DateReceived);
		}

		#endregion

		RefCountryRequiredDocument GetNewRequiredDoc(RefCountry country, ZString docType, ZString orig, ZString dest, ZString usage, ZString transport, ZBool isShipment)
		{
			RefCountryRequiredDocument result = country.RequiredDocuments.AddNew();
			result.RD_DocType = docType;
			result.RD_RN_NKOrigin = orig;
			result.RD_RN_NKDestination = dest;
			result.RD_DocUsage = usage;
			result.RD_TransportMode = transport;
			result.RD_OnShipment = isShipment;
			return result;
		}

		JobRequiredDocument GetNewJobRequiredDoc(OrgHeader org, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDateTime date, ZString docNumber)
		{
			JobRequiredDocument result = org.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			return result;
		}

		JobRequiredDocument GetNewBuyerSupplierRequiredDoc(OrgSupplierBuyerLink buyerSupplierLink, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDateTime date, ZString docNumber)
		{
			JobRequiredDocument result = buyerSupplierLink.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			return result;
		}

		#endregion

		#region Pack Type Validation

		public void TestPackTypeValidation()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_F3_NKTotalCountPackType = "XXZ";
			shipment.RunPreSaveValidation();
			AssertNoErrors("Pack Type errors should not occur at CommonShipment level", shipment.JS_F3_NKTotalCountPackTypeInfo);
		}

		public void TestPackLineUpdates()
		{
			FreightPacksDataRegistry.Instance.InnerPackUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXZ");
			FreightPacksDataRegistry.Instance.OuterPackUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXZ");
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_GoodsDescription = "Something";
			AssertHasError("OuterPackLines should be updated", shipment.OuterPackLines[0].JL_F3_NKPackTypeInfo, "Enter a valid Inner Package: Package Type.");
			AssertEquals("InnerPackLines should not be updated at CommonShipment level", 0, shipment.InnerPackLines.Count);
		}

		public void TestSuppressPackLinesUpdate()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.SuppressPackLinesUpdate = true;

			shipment.JS_GoodsDescription = "monkeys";
			AssertEquals("packlines have not been updated", 0, shipment.OuterPackLines.Count);

			shipment.SuppressPackLinesUpdate = false;

			shipment.JS_GoodsDescription = "cottage cheese";
			AssertEquals("packlines have been updated", 1, shipment.OuterPackLines.Count);
		}

		#endregion

		#region UTC Times

		public void TestUTCTimes()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "BRRIO";
			shipment.JS_E_DEP = new ZDateTime(2011, 3, 20, 12, 0, 0);
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_E_ARV = new ZDateTime(2011, 3, 25, 10, 0, 0);

			//BRRIO is 3 hours behind UTC
			AssertEquals("JS_E_DEP_UTC", new ZDateTime(2011, 3, 20, 15, 0, 0), shipment.JS_E_DEP_UTC);
			//HKHKG is 8 hours ahead of UTC
			AssertEquals("JS_E_ARV_UTC", new ZDateTime(2011, 3, 25, 2, 0, 0), shipment.JS_E_ARV_UTC);
		}

		#endregion

		#region OnLoaded JS_InspectionTypeCode

		public void TestJS_InspectionTypeCode_Web()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				bool originalIsWeb = Globals.IsWeb;

				try
				{
					Globals.IsWeb = true;

					var shipment = Factory.NewWithValidTestData<CommonShipment>();
					shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Web;
					Factory.Save();

					Globals.IsWeb = false;

					using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
					{
						var testFactory = new BusinessObjectFactory();
						var testShipment = testFactory.Load<CommonShipment>(shipment.PK);
						AssertEquals(ZString.Empty, testShipment.JS_InspectionTypeCode);
						Assert(!((ILightValidationInternals)testShipment).IsValid);
					}

					using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, FreightDataRegistry.AviationSecurity_Unknown_Code))
					{
						var testFactory = new BusinessObjectFactory();
						var testShipment = testFactory.Load<CommonShipment>(shipment.PK);
						AssertEquals(FreightDataRegistry.AviationSecurity_Unknown_Code, testShipment.JS_InspectionTypeCode);
						Assert(!((ILightValidationInternals)testShipment).IsValid);

						Globals.IsWeb = true;

						testFactory = new BusinessObjectFactory();
						testShipment = testFactory.Load<CommonShipment>(shipment.PK);
						AssertEquals(BaseJobShipmentLookups.InspectionType_Web, testShipment.JS_InspectionTypeCode);
					}
				}
				finally
				{
					Globals.IsWeb = originalIsWeb;
				}
			}
		}

		#endregion

		#region Phase Security

		public void TestIsReadOnlyDueToPhase()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals(false, shipment.IsReadOnlyDueToPhase);
		}

		#endregion

		public void TestUpdateETAWithPortDefaultDeliveryTimeIfEmpty()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_E_ARV = new ZDateTime(2011, 5, 5);
			CommonConsol consol = shipment.Consols.AddNew();
			shipment.JS_RL_NKDestination = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			Transport transport = consol.Transports.AddNew();
			transport.JW_ETA = new ZDateTime(2010, 4, 4);
			shipment.UpdateETAWithPortDefaultDeliveryTimeIfEmpty();
			AssertEquals(new ZDateTime(2011, 5, 5), shipment.JS_E_ARV);
		}

		#region IRoutingSupport members
		public void TestIRoutingSupportMembers()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			var originalTransport = consol.Transports[0];
			var transport1 = consol.Transports.AddNew();
			var transport2 = consol.Transports.AddNew();
			originalTransport.JW_RL_NKLoadPort = "AUSYD";
			originalTransport.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "CACAD";
			AssertEquals(3, ((IRoutingSupport)shipment).TransportsIncludingRelated.Count);
			AssertEquals("SEA", ((IRoutingSupport)shipment).TransportMode);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AIR", ((IRoutingSupport)shipment).TransportMode);
		}

		public void TestTransportsIncludingRelatedReadOnlyDueToPhase()
		{
			var shipment = Factory.New<CommonShipmentForTest>();

			AssertEquals("Not read-only by default", false, shipment.TransportsIncludingRelated.ReadOnly);

			shipment = Factory.New<CommonShipmentForTest>();
			shipment.PropertiesForcedToReadOnlyDueToPhase.Add("SomeOtherChildProperty");
			AssertEquals("Not read-only", false, shipment.TransportsIncludingRelated.ReadOnly);

			shipment = Factory.New<CommonShipmentForTest>();
			shipment.PropertiesForcedToReadOnlyDueToPhase.Add("Routing");
			AssertEquals("Read-only due to phase", true, shipment.TransportsIncludingRelated.ReadOnly);
		}

		public void TestTransportsIncludingRelated_CorrectOrderWhenDuplicateLegOrdersWithNoDates()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = "SEA";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "SEA";

			var consolTransport1 = consol.Transports[0];
			var consolTransport2 = consol.Transports.AddNew();
			var consolTransport3 = consol.Transports.AddNew();

			var shipmentTransport1 = shipment.Transports.AddNew();
			var shipmentTransport2 = shipment.Transports.AddNew();

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CHGVA";

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEHAM";

			shipmentTransport1.JW_RL_NKLoadPort = "AUSYD";
			shipmentTransport1.JW_RL_NKDiscPort = "AUBNE";
			shipmentTransport1.JW_TransportMode = "ROA";
			shipmentTransport1.JW_LegOrder = 1;

			consolTransport1.JW_RL_NKLoadPort = "AUBNE";
			consolTransport1.JW_RL_NKDiscPort = "SGSIN";
			consolTransport1.JW_TransportMode = "SEA";
			consolTransport1.JW_LegOrder = 1;

			consolTransport2.JW_RL_NKLoadPort = "SGSIN";
			consolTransport2.JW_RL_NKDiscPort = "FRCAL";
			consolTransport2.JW_TransportMode = "SEA";
			consolTransport2.JW_LegOrder = 2;

			consolTransport3.JW_RL_NKLoadPort = "FRCAL";
			consolTransport3.JW_RL_NKDiscPort = "DEHAM";
			consolTransport3.JW_TransportMode = "SEA";
			consolTransport3.JW_LegOrder = 3;

			shipmentTransport2.JW_RL_NKLoadPort = "DEHAM";
			shipmentTransport2.JW_RL_NKDiscPort = "CHGVA";
			shipmentTransport2.JW_TransportMode = "ROA";
			shipmentTransport2.JW_LegOrder = 2;

			var orderedTransports = new TransportOrderHelper(shipment.TransportsIncludingRelated).ToList();
			AssertEquals("shipmentTransport1", shipmentTransport1.PK, orderedTransports[0].PK);
			AssertEquals("consolTransport1", consolTransport1.PK, orderedTransports[1].PK);
			AssertEquals("consolTransport2", consolTransport2.PK, orderedTransports[2].PK);
			AssertEquals("consolTransport3", consolTransport3.PK, orderedTransports[3].PK);
			AssertEquals("shipmentTransport2", shipmentTransport2.PK, orderedTransports[4].PK);
		}

		public void TestTransportsIncludingRelated_CorrectFirstAndLastLegMatchingWhenDuplicateLegOrdersWithNoDates()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = "AIR";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "AIR";

			var consolTransport1 = consol.Transports[0];
			var consolTransport2 = consol.Transports.AddNew();
			var consolTransport3 = consol.Transports.AddNew();

			var shipmentTransport1 = shipment.Transports.AddNew();
			var shipmentTransport2 = shipment.Transports.AddNew();

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CHGVA";

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEHAM";

			shipmentTransport1.JW_RL_NKLoadPort = "AUSYD";
			shipmentTransport1.JW_RL_NKDiscPort = "AUBNE";
			shipmentTransport1.JW_TransportMode = "AIR";
			shipmentTransport1.JW_LegOrder = 1;

			consolTransport1.JW_RL_NKLoadPort = "AUBNE";
			consolTransport1.JW_RL_NKDiscPort = "SGSIN";
			consolTransport1.JW_TransportMode = "AIR";
			consolTransport1.JW_LegOrder = 1;

			consolTransport2.JW_RL_NKLoadPort = "SGSIN";
			consolTransport2.JW_RL_NKDiscPort = "FRCAL";
			consolTransport2.JW_TransportMode = "AIR";
			consolTransport2.JW_LegOrder = 2;

			consolTransport3.JW_RL_NKLoadPort = "FRCAL";
			consolTransport3.JW_RL_NKDiscPort = "DEHAM";
			consolTransport3.JW_TransportMode = "AIR";
			consolTransport3.JW_LegOrder = 3;

			shipmentTransport2.JW_RL_NKLoadPort = "DEHAM";
			shipmentTransport2.JW_RL_NKDiscPort = "CHGVA";
			shipmentTransport2.JW_TransportMode = "AIR";
			shipmentTransport2.JW_LegOrder = 2;

			AssertEquals("First air leg from shipment", shipmentTransport1, shipment.TransportsIncludingRelated.FirstLegMatching(x => x.IsAir));
			AssertEquals("Last air leg from shipment", shipmentTransport2, shipment.TransportsIncludingRelated.LastLegMatching(x => x.IsAir));

			shipmentTransport1.JW_TransportMode = "ROA";
			shipmentTransport2.JW_TransportMode = "ROA";

			AssertEquals("First road leg from shipment", shipmentTransport1, shipment.TransportsIncludingRelated.FirstLegMatching(x => x.IsRoad));
			AssertEquals("Last road leg from shipment", shipmentTransport2, shipment.TransportsIncludingRelated.LastLegMatching(x => x.IsRoad));
			AssertEquals("First air leg from consol", consolTransport1, shipment.TransportsIncludingRelated.FirstLegMatching(x => x.IsAir));
			AssertEquals("Last air leg from consol", consolTransport3, shipment.TransportsIncludingRelated.LastLegMatching(x => x.IsAir));
		}

		#endregion

		#region ITemplateReversible

		public void TestReverse()
		{
			CommonShipment shipment = GetShipment();
			OrgHeader randomOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader randomOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_RL_NKOrigin = "AUSYD";

			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "XX";
			shipment.JS_ActualVolume = 0.125m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_TotalPackageCount = 5;

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "Blah";
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = randomOrg.PK;

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_CompanyName = "Blah2";
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			shipment.ConsignorPickupAddress.OrganisationPK = randomOrg2.PK;

			((ITemplateReversible)shipment).Reverse();

			AssertEquals("Consignor Override", true, shipment.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignor Company Name", "Blah", shipment.ConsigneeDocumentaryAddress.E2_CompanyName);
			AssertEquals("Consignor Doc Address Has Correct Parent", shipment.PK, shipment.ConsigneeDocumentaryAddress.E2_ParentID);

			AssertEquals("Consignee Override", false, shipment.ConsignorDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignee PK", randomOrg.PK, shipment.ConsignorDocumentaryAddress.OrganisationPK);
			AssertEquals("Consignee Doc Address Has Correct Parent", shipment.PK, shipment.ConsignorDocumentaryAddress.E2_ParentID);

			AssertEquals("Consignor Delivery Override", true, shipment.ConsignorPickupAddress.E2_AddressOverride);
			AssertEquals("Consignor Delivery Company Name", "Blah2", shipment.ConsignorPickupAddress.E2_CompanyName);
			AssertEquals("Consignor Delivery Doc Address Has Correct Parent", shipment.PK, shipment.ConsignorPickupAddress.E2_ParentID);

			AssertEquals("Consignee Pickup Override", false, shipment.ConsigneeDeliveryAddress.E2_AddressOverride);
			AssertEquals("Consignee Pickup PK", randomOrg2.PK, shipment.ConsigneeDeliveryAddress.OrganisationPK);
			AssertEquals("Consignee Pickup Doc Address Has Correct Parent", shipment.PK, shipment.ConsigneeDeliveryAddress.E2_ParentID);

			AssertEquals("Destination", "USCHI", shipment.JS_RL_NKOrigin);
			AssertEquals("Origin", "AUSYD", shipment.JS_RL_NKDestination);
		}

		public void TestTemplateCopy_DoNotUpdateActualsFromChargeable()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			shipment.JS_ActualWeight = 0m;
			shipment.JS_ActualVolume = 0m;
			shipment.JS_UnitOfWeight = "XX";
			shipment.JS_UnitOfVolume = "CF";

			shipment.JS_ActualChargeable = 100;
			AssertNotEquals("Should update to a new value from chargeable", 0m, shipment.JS_ActualWeight);

			shipment.JS_ActualWeight = 0m;
			AssertEquals("Should still keep 100", 100m, shipment.JS_ActualChargeable);

			var newShipment = (CommonShipment)((ITemplateCopyable)shipment).TemplateCopy();
			AssertEquals("Should not update actual weight value in template copy.", shipment.JS_ActualWeight, newShipment.JS_ActualWeight);
			AssertEquals("Should not update actual volume value in template copy.", shipment.JS_ActualVolume, newShipment.JS_ActualVolume);
			AssertEquals("Actual chargeable value should be cloned.", shipment.JS_ActualChargeable, newShipment.JS_ActualChargeable);
		}

		public void TestTemplateCopy()
		{
			CommonShipment shipment = GetShipment();
			OrgHeader randomOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader randomOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_RL_NKOrigin = "AUSYD";

			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "XX";
			shipment.JS_ActualVolume = 0.125m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_TotalPackageCount = 5;

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "Blah";
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = randomOrg.PK;

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_CompanyName = "Blah2";
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			shipment.ConsignorPickupAddress.OrganisationPK = randomOrg2.PK;

			CommonShipment shipmentCopy = (CommonShipment)((ITemplateCopyable)shipment).TemplateCopy();

			AssertEquals("Consignor Override", true, shipmentCopy.ConsignorDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignor Company Name", "Blah", shipmentCopy.ConsignorDocumentaryAddress.E2_CompanyName);
			AssertEquals("Consignor Doc Address Has Correct Parent", shipmentCopy.PK, shipmentCopy.ConsignorDocumentaryAddress.E2_ParentID);

			AssertEquals("Consignee Override", false, shipmentCopy.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignee PK", randomOrg.PK, shipmentCopy.ConsigneeDocumentaryAddress.OrganisationPK);
			AssertEquals("Consignee Doc Address Has Correct Parent", shipmentCopy.PK, shipmentCopy.ConsigneeDocumentaryAddress.E2_ParentID);

			AssertEquals("Old Consignor Parent Not Changed", shipment.PK, shipment.ConsignorDocumentaryAddress.E2_ParentID);
			AssertEquals("Old Consignee Parent Not Changed", shipment.PK, shipment.ConsigneeDocumentaryAddress.E2_ParentID);

			AssertEquals("Consignor Delivery Override", true, shipmentCopy.ConsigneeDeliveryAddress.E2_AddressOverride);
			AssertEquals("Consignor Delivery Company Name", "Blah2", shipmentCopy.ConsigneeDeliveryAddress.E2_CompanyName);
			AssertEquals("Consignor Delivery Doc Address Has Correct Parent", shipmentCopy.PK, shipmentCopy.ConsigneeDeliveryAddress.E2_ParentID);

			AssertEquals("Consignee Pickup Override", false, shipmentCopy.ConsignorPickupAddress.E2_AddressOverride);
			AssertEquals("Consignee Pickup PK", randomOrg2.PK, shipmentCopy.ConsignorPickupAddress.OrganisationPK);
			AssertEquals("Consignee Pickup Doc Address Has Correct Parent", shipmentCopy.PK, shipmentCopy.ConsignorPickupAddress.E2_ParentID);

			AssertEquals("Old Consignor Parent Not Changed", shipment.PK, shipment.ConsigneeDeliveryAddress.E2_ParentID);
			AssertEquals("Old Consignee Parent Not Changed", shipment.PK, shipment.ConsignorPickupAddress.E2_ParentID);

			AssertEquals("Destination", "USCHI", shipmentCopy.JS_RL_NKDestination);
			AssertEquals("Origin", "AUSYD", shipmentCopy.JS_RL_NKOrigin);

			AssertEquals("Transport mode should be cloned.", shipment.JS_TransportMode, shipmentCopy.JS_TransportMode);
			AssertEquals("Packing mode should be cloned.", shipment.JS_PackingMode, shipmentCopy.JS_PackingMode);
			AssertEquals("Shipment type should be cloned.", shipment.JS_ShipmentType, shipmentCopy.JS_ShipmentType);

			AssertEquals("Persistent values should be cloned.", shipment.JS_ActualWeight, shipmentCopy.JS_ActualWeight);
			AssertEquals("Calculated values should be cloned.", shipment.JS_ActualChargeable, shipmentCopy.JS_ActualChargeable);
			AssertEquals("Should only be one default inner Packline.", 1, shipmentCopy.InnerPackLines.Count);
			AssertEquals("Outer packlines should be cloned.", 1, shipmentCopy.OuterPackLines.Count);
		}

		public void TestTemplateCopySubShipmentWouldNotCopyMasterShipmentLink()
		{
			var master = GetShipment();
			var kid1 = GetShipment();
			var kid2 = GetShipment();
			var kid21 = GetShipment();

			kid1.JS_JS_ColoadMasterShipment = master.PK;
			kid2.JS_JS_ColoadMasterShipment = master.PK;
			kid21.JS_JS_ColoadMasterShipment = kid2.PK;

			AssertEquals("Included all sub-shipments for master", 2, master.CoLoadShipments.Count);
			AssertEquals("1 sub-shipment for kid2", 1, kid2.CoLoadShipments.Count);

			var masterCopy = (CommonShipment)((ITemplateCopyable)master).TemplateCopy();
			AssertEquals("No sub-shipments has been copied", 0, masterCopy.CoLoadShipments.Count);

			var kid1Copy = (CommonShipment)((ITemplateCopyable)kid1).TemplateCopy();
			AssertEquals("No sub-shipments", 0, kid1Copy.CoLoadShipments.Count);
			AssertEquals("JS_JS_ColoadMasterShipment is not copied", ZGuid.Empty, kid1Copy.JS_JS_ColoadMasterShipment);

			var kid2Copy = (CommonShipment)((ITemplateCopyable)kid2).TemplateCopy();
			AssertEquals("No sub-shipments has been copied", 0, kid2Copy.CoLoadShipments.Count);
			AssertEquals("JS_JS_ColoadMasterShipment is not copied", ZGuid.Empty, kid2Copy.JS_JS_ColoadMasterShipment);

			var kid21Copy = (CommonShipment)((ITemplateCopyable)kid21).TemplateCopy();
			AssertEquals("No sub-shipments", 0, kid21Copy.CoLoadShipments.Count);
			AssertEquals("JS_JS_ColoadMasterShipment is not copied", ZGuid.Empty, kid21Copy.JS_JS_ColoadMasterShipment);
		}

		#endregion

		#region GetNewValidation

		public virtual void TestGetNewValidation()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			AssertEquals("Type of Validation", typeof(CommonShipmentValidation), shipment.Validation.GetType());
		}

		#endregion

		#region Consignor Lists

		public void TestConsignorListsAreCachedDirectlyOnShipment()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.ConsigneeDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			var link1 = shipment1.Consignee.SupplierLinks.AddNew();
			link1.OL_OH_Supplier = Factory.New<OrgHeader>().PK;

			var consignorList1 = shipment1.Lookups.Consignor_List;
			Assert("Consignor - Related Consignee should be set.", consignorList1.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignor - Related Consignee should be set to shipment1 consignee.", shipment1.Consignee.PK, (ZGuid)consignorList1.FilterBusinessObjectDefaults["Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			var consignorDefaultOnlyList1 = shipment1.Lookups.ConsignorDefaultOnly_List;
			Assert("Consignor - Related Consignee should be set.", consignorDefaultOnlyList1.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignor - Related Consignee should be set to shipment1 consignee.", shipment1.Consignee.PK, (ZGuid)consignorDefaultOnlyList1.FilterBusinessObjectDefaults["Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.ConsigneeDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			var link2 = shipment2.Consignee.SupplierLinks.AddNew();
			link2.OL_OH_Supplier = Factory.New<OrgHeader>().PK;

			var consignorList2 = shipment2.Lookups.Consignor_List;
			Assert("Consignor - Related Consignee should be set.", consignorList1.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignor - Related Consignee should be set to shipment1 consignee.", shipment1.Consignee.PK, (ZGuid)consignorList1.FilterBusinessObjectDefaults["Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Consignor - Related Consignee should be set.", consignorList2.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignor - Related Consignee should be set to shipment2 consignee.", shipment2.Consignee.PK, (ZGuid)consignorList2.FilterBusinessObjectDefaults["Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			var consignorDefaultOnlyList2 = shipment2.Lookups.ConsignorDefaultOnly_List;
			Assert("Consignor - Related Consignee should be set.", consignorDefaultOnlyList1.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignor - Related Consignee should be set to shipment1 consignee.", shipment1.Consignee.PK, (ZGuid)consignorDefaultOnlyList1.FilterBusinessObjectDefaults["Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Consignor - Related Consignee should be set.", consignorDefaultOnlyList2.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignor - Related Consignee should be set to shipment2 consignee.", shipment2.Consignee.PK, (ZGuid)consignorDefaultOnlyList2.FilterBusinessObjectDefaults["Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		public void TestConsignorListsFilterBusinessObjectDefault_OriginRedefaultsWhenEmpty()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			var consignorList = shipment.Lookups.Consignor_List;
			var consignorDefaultOnlyList = shipment.Lookups.ConsignorDefaultOnly_List;

			Assert("Main UNLOCO should be set.", consignorList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be set to shipment origin.", "AUSYD", (ZString)consignorList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Main UNLOCO should be set.", consignorDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be set to shipment origin.", "AUSYD", (ZString)consignorDefaultOnlyList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			shipment.JS_RL_NKOrigin = ZString.Empty;

			Assert("Main UNLOCO should be re-defaulted.", consignorList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be empty.", ZString.Empty, (ZString)consignorList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Main UNLOCO should be re-defaulted.", consignorDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be empty.", ZString.Empty, (ZString)consignorDefaultOnlyList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			shipment.JS_RL_NKOrigin = "USLAX";

			Assert("Main UNLOCO should be re-defaulted.", consignorList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be set to shipment origin.", "USLAX", (ZString)consignorList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Main UNLOCO should be re-defaulted.", consignorDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be set to shipment origin.", "USLAX", (ZString)consignorDefaultOnlyList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		public void TestConsignorListsFilterBusinessObjectDefault_IndexSearch()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			var consignorList = shipment.Lookups.Consignor_List;
			var consignorDefaultOnlyList = shipment.Lookups.ConsignorDefaultOnly_List;
			const string filterName = "CLOSESTPORT";

			Assert("Main UNLOCO should be set.", consignorList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be set to shipment origin.", "AUSYD", (ZString)consignorList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
			Assert("Main UNLOCO should be set.", consignorDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be set to shipment origin.", "AUSYD", (ZString)consignorDefaultOnlyList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);

			shipment.JS_RL_NKOrigin = ZString.Empty;

			Assert("Main UNLOCO should be re-defaulted.", consignorList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be empty.", ZString.Empty, (ZString)consignorList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
			Assert("Main UNLOCO should be re-defaulted.", consignorDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be empty.", ZString.Empty, (ZString)consignorDefaultOnlyList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);

			shipment.JS_RL_NKOrigin = "USLAX";

			Assert("Main UNLOCO should be re-defaulted.", consignorList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be set to shipment origin.", "USLAX", (ZString)consignorList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
			Assert("Main UNLOCO should be re-defaulted.", consignorDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be set to shipment origin.", "USLAX", (ZString)consignorDefaultOnlyList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
		}

		public void TestShowStringEmptyWhenConsignorRelatedConsigneeOrConsigneeRelatedConsignorIsNull_IndexSearch()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";

			var consignorList = shipment.Lookups.Consignor_List;
			var consignorDefaultOnlyList = shipment.Lookups.ConsignorDefaultOnly_List;

			var consigneeList = shipment.Lookups.Consignee_List;
			var consigneeDefaultOnlyList = shipment.Lookups.ConsigneeDefaultOnly_List;

			const string filterName1 = "CONSIGNORRELATEDCONSIGNEE";
			const string filterName2 = "CONSIGNEERELATEDCONSIGNOR";

			Assert(consignorList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName1 + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals(ZString.Empty, consignorList.FilterBusinessObjectDefaults[filterName1 + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
			Assert(consignorDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName1 + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals(ZString.Empty, consignorDefaultOnlyList.FilterBusinessObjectDefaults[filterName1 + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);

			Assert(consigneeList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName2 + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals(ZString.Empty, consigneeList.FilterBusinessObjectDefaults[filterName2 + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
			Assert(consigneeDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName2 + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals(ZString.Empty, consigneeDefaultOnlyList.FilterBusinessObjectDefaults[filterName2 + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
		}

		#endregion

		#region Consignee Lists

		public void TestConsigneeListsAreCachedDirectlyOnShipment()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			var link1 = shipment1.Consignor.BuyerLinks.AddNew();
			link1.OL_OH_Buyer = Factory.New<OrgHeader>().PK;

			var consigneeList1 = shipment1.Lookups.Consignee_List;
			Assert("Consignee - Related Consignor should be set.", consigneeList1.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignee - Related Consignor should be set to shipment1 consignor.", shipment1.Consignor.PK, consigneeList1.FilterBusinessObjectDefaults["Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			var consigneeDefaultOnlyList1 = shipment1.Lookups.ConsigneeDefaultOnly_List;
			Assert("Consignee - Related Consignor should be set.", consigneeDefaultOnlyList1.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignee - Related Consignor should be set to shipment1 consignor.", shipment1.Consignor.PK, consigneeDefaultOnlyList1.FilterBusinessObjectDefaults["Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			var link2 = shipment2.Consignor.BuyerLinks.AddNew();
			link2.OL_OH_Buyer = Factory.New<OrgHeader>().PK;

			var consigneeList2 = shipment2.Lookups.Consignee_List;
			Assert("Consignee - Related Consignor should be set.", consigneeList1.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignee - Related Consignor should be set to shipment1 consignor.", shipment1.Consignor.PK, consigneeList1.FilterBusinessObjectDefaults["Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Consignee - Related Consignor should be set.", consigneeList2.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignee - Related Consignor should be set to shipment2 consignor.", shipment2.Consignor.PK, consigneeList2.FilterBusinessObjectDefaults["Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			var consigneeDefaultOnlyList2 = shipment2.Lookups.ConsigneeDefaultOnly_List;
			Assert("Consignee - Related Consignor should be set.", consigneeDefaultOnlyList1.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignee - Related Consignor should be set to shipment1 consignor.", shipment1.Consignor.PK, consigneeDefaultOnlyList1.FilterBusinessObjectDefaults["Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Consignee - Related Consignor should be set.", consigneeDefaultOnlyList2.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Consignee - Related Consignor should be set to shipment2 consignor.", shipment2.Consignor.PK, consigneeDefaultOnlyList2.FilterBusinessObjectDefaults["Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		public void TestConsigneeListsFilterBusinessObjectDefaultDestinationRedefaultsWhenEmpty()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			var consigneeList = shipment.Lookups.Consignee_List;
			var consigneeDefaultOnlyList = shipment.Lookups.ConsigneeDefaultOnly_List;

			Assert("Main UNLOCO should be set.", consigneeList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be set to shipment destination.", "AUSYD", (ZString)consigneeList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Main UNLOCO should be set.", consigneeDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be set to shipment destination.", "AUSYD", (ZString)consigneeDefaultOnlyList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			shipment.JS_RL_NKDestination = ZString.Empty;

			Assert("Main UNLOCO should be re-defaulted.", consigneeList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be empty.", ZString.Empty, (ZString)consigneeList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Main UNLOCO should be re-defaulted.", consigneeDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be empty.", ZString.Empty, (ZString)consigneeDefaultOnlyList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			shipment.JS_RL_NKDestination = "USLAX";

			Assert("Main UNLOCO should be re-defaulted.", consigneeList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be set to shipment destination.", "USLAX", (ZString)consigneeList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			Assert("Main UNLOCO should be re-defaulted.", consigneeDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Main UNLOCO  should be set to shipment destination.", "USLAX", (ZString)consigneeDefaultOnlyList.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		public void TestTestConsigneeListsFilterBusinessObjectDefaultDestinationRedefaultsForIndexSearch()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			var consigneeList = shipment.Lookups.Consignee_List;
			var consigneeDefaultOnlyList = shipment.Lookups.ConsigneeDefaultOnly_List;
			const string filterName = "CLOSESTPORT";

			Assert("Main UNLOCO should be set.", consigneeList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be set to shipment destination.", "AUSYD", (ZString)consigneeList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
			Assert("Main UNLOCO should be set.", consigneeDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be set to shipment destination.", "AUSYD", (ZString)consigneeDefaultOnlyList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);

			shipment.JS_RL_NKDestination = ZString.Empty;

			Assert("Main UNLOCO should be re-defaulted.", consigneeList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be empty.", ZString.Empty, (ZString)consigneeList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
			Assert("Main UNLOCO should be re-defaulted.", consigneeDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be empty.", ZString.Empty, (ZString)consigneeDefaultOnlyList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);

			shipment.JS_RL_NKDestination = "USLAX";

			Assert("Main UNLOCO should be re-defaulted.", consigneeList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be set to shipment destination.", "USLAX", (ZString)consigneeList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
			Assert("Main UNLOCO should be re-defaulted.", consigneeDefaultOnlyList.FilterBusinessObjectDefaults.ContainsDefaultFor(filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index));
			AssertEquals("Main UNLOCO  should be set to shipment destination.", "USLAX", (ZString)consigneeDefaultOnlyList.FilterBusinessObjectDefaults[filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property", SearchType.Index].Value);
		}

		#endregion

		#region TestConsigneeConsignorFindBoxListDefaultFromUnmatchOrgNotes

		public void TestConsigneeConsignorFindBoxListDefaultFromUnmatchOrgNotes()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();

			var helper = new UnmatchOrgRecordTestHelper(shipment, ConsigneeRec, ConsignorRec);

			//Consignee
			helper.PopulateOrgDefaultsFromUnmatchedNote(shipment.Lookups.GetType(), "Consignee_List", shipment.Lookups.Consignee_List);
			AssertEquals("Consignee_list should have defaults from unmatchorgnotes", true, shipment.Lookups.Consignee_List.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(shipment.Lookups.Consignee_List.DefaultsForNewChild, ConsigneeRec);

			//Consignor
			helper.PopulateOrgDefaultsFromUnmatchedNote(shipment.Lookups.GetType(), "Consignor_List", shipment.Lookups.Consignor_List);
			AssertEquals("Consignor_list should have defaults from unmatchorgnotes", true, shipment.Lookups.Consignor_List.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(shipment.Lookups.Consignor_List.DefaultsForNewChild, ConsignorRec);
		}

		UnmatchOrgRecord ConsigneeRec
		{
			get
			{
				if (consigneeRec == null)
				{
					consigneeRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignee,
						nameof(OrganisationTypes.Consignee),
						"consignee addr 1",
						"consignee addr 2",
						"consigneeName",
						"2222",
						"NSW",
						"sydney",
						"consignee",
						"cneOwnerCode",
						"");
				}
				return consigneeRec;
			}
		}
		UnmatchOrgRecord consigneeRec;

		UnmatchOrgRecord ConsignorRec
		{
			get
			{
				if (consignorRec == null)
				{
					consignorRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignor,
						nameof(OrganisationTypes.Consignor),
						"consignor addr 1",
						"consignor addr 2",
						"consignorName",
						"1111",
						"NSW",
						"city",
						"consignor",
						"CnrownerCode",
						"");
				}
				return consignorRec;
			}
		}
		UnmatchOrgRecord consignorRec;

		#endregion

		#region TestAddDateEventsCancelsOldOnes

		public void TestAddDateEventsCancelsOldOnes()
		{
			AssertAddDateEventsCancelsOldOnes(JobShipmentSchema.JS_E_ARV, "ARV");
			AssertAddDateEventsCancelsOldOnes(JobShipmentSchema.JS_E_DEP, "DEP");
			AssertAddDateEventsCancelsOldOnesWithMilestoneGuid(JobShipmentSchema.JS_E_ARV, "ARV");
			AssertAddDateEventsCancelsOldOnesWithMilestoneGuid(JobShipmentSchema.JS_E_DEP, "DEP");
		}

		void AssertAddDateEventsCancelsOldOnes(SchemaColumn column, string eventCode)
		{
			ZQuery logsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode);
			logsQuery.OrderBy = StmALogSchema.Constants.SL_EventTime + " DESC";

			var dt0 = ZDateTimeOffset.Today;
			var dt1 = ZDateTimeOffset.Today.AddDays(-5);
			var dt2 = ZDateTimeOffset.Today.AddDays(5);
			var dt3 = ZDateTimeOffset.Today.AddDays(7);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment[column] = dt0.ToZDateTime();
			Factory.Save();

			StmALog[] logs = shipment.Logs.Find(logsQuery);
			AssertEquals(1, logs.Length);
			AssertEquals(dt0, logs[0].SL_EventTimeOffset);
			AssertEquals(false, logs[0].IsCancelled);

			shipment[column] = dt1.ToZDateTime();
			Factory.Save();

			logs = shipment.Logs.Find(logsQuery);
			AssertEquals(2, logs.Length);
			AssertEquals(dt0, logs[0].SL_EventTimeOffset);
			AssertEquals(true, logs[0].IsCancelled);
			AssertEquals(dt1, logs[1].SL_EventTimeOffset);
			AssertEquals(false, logs[1].IsCancelled);

			shipment[column] = dt2.ToZDateTime();
			Factory.Save();

			logs = shipment.Logs.Find(logsQuery);
			AssertEquals(3, logs.Length);
			AssertEquals(dt2, logs[0].SL_EventTimeOffset);
			AssertEquals(false, logs[0].IsCancelled);
			AssertEquals(dt0, logs[1].SL_EventTimeOffset);
			AssertEquals(true, logs[1].IsCancelled);
			AssertEquals(dt1, logs[2].SL_EventTimeOffset);
			AssertEquals(true, logs[2].IsCancelled);

			var log1 = shipment.Logs.AddNew(Events.All[eventCode], "", dt0, true);
			var log2 = shipment.Logs.AddNew(Events.All[eventCode], "To: 01-JAN-12", dt0, true);
			var log3 = shipment.Logs.AddNew(Events.All[eventCode], "From: 01-JAN-12 To: 31-DEC-12", dt0, true);
			var log4 = shipment.Logs.AddNew(Events.All[eventCode], "ABC From: 01-JAN-12 To: 31-DEC-12", dt0, true);

			shipment[column] = dt3.ToZDateTime();
			Factory.Save();

			Assert("Don't just cancel logs on save", !log1.SL_IsCancelled);
			Assert(!log2.SL_IsCancelled);
			Assert(!log3.SL_IsCancelled);
			Assert(!log4.SL_IsCancelled);
		}

		void AssertAddDateEventsCancelsOldOnesWithMilestoneGuid(SchemaColumn column, string eventCode)
		{
			var testDate1 = ZDateTimeOffset.Today;
			var testDate2 = ZDateTimeOffset.Today.AddDays(5);
			var testDate3 = ZDateTimeOffset.Today.AddDays(7);
			var testDate4 = ZDateTimeOffset.Today.AddDays(10);

			var shipment = Factory.New<CommonShipment>();
			var milestoneGuid = Guid.NewGuid();
			var log1 = shipment.Logs.AddNew(Events.All[eventCode], $"ABC To: 01-JAN-12", testDate1, true);
			var log2 = shipment.Logs.AddNew(Events.All[eventCode], $"{milestoneGuid}|To: 01-JAN-12", testDate1, true);
			Factory.Save();

			shipment[column] = testDate2.ToZDateTime();
			Factory.Save();

			Assert($"Log should not be cancelled as it does not start with From/To prefix", !log1.SL_IsCancelled);
			Assert($"Log should be cancelled as it starts with To prefix after milestone guid is skipped", log2.SL_IsCancelled);

			var createdLog = shipment.Logs.Find(log => log.SL_SE_NKEvent == eventCode && log.SL_EventTimeOffset == testDate2).Single();
			createdLog.Cancel();
			var log3 = shipment.Logs.AddNew(Events.All[eventCode], $"ABC From: 01-JAN-12 To: 31-DEC-12", testDate3, true);
			var log4 = shipment.Logs.AddNew(Events.All[eventCode], $"{milestoneGuid}|From: 01-JAN-12 To: 31-DEC-12", testDate3, true);
			Factory.Save();

			shipment[column] = testDate4.ToZDateTime();
			Factory.Save();

			Assert($"Log should not be cancelled as it does not start with From/To prefix", !log1.SL_IsCancelled);
			Assert($"Log should still be cancelled", log2.SL_IsCancelled);
			Assert($"Log should not be cancelled as it does not start with From/To prefix", !log3.SL_IsCancelled);
			Assert($"Log should be cancelled as it starts with From prefix after milestone guid is skipped", log4.SL_IsCancelled);
		}

		#endregion

		#region TestCoLoadMasterHouseBillList

		public void TestCoLoadMasterHouseBillList()
		{
			CommonShipment superMasterShipment = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment masterShipment = superMasterShipment.CoLoadShipments.AddNew();
			CommonShipment subShipment1 = masterShipment.CoLoadShipments.AddNew();
			CommonShipment subShipment2 = superMasterShipment.CoLoadShipments.AddNew();

			superMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			subShipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			superMasterShipment.JS_HouseBill = "SUPERMASTER-BILL";
			masterShipment.JS_HouseBill = "MASTER-BILL";
			subShipment1.JS_HouseBill = "SUB1-BILL";
			subShipment2.JS_HouseBill = "SUB2-BILL";

			AssertEquals("SUPERMASTER-BILL", superMasterShipment.ColoadMasterHouseBillList);
			AssertEquals("SUPERMASTER-BILL=>MASTER-BILL", masterShipment.ColoadMasterHouseBillList);
			AssertEquals("SUPERMASTER-BILL=>MASTER-BILL=>SUB1-BILL", subShipment1.ColoadMasterHouseBillList);
			AssertEquals("SUPERMASTER-BILL=>SUB2-BILL", subShipment2.ColoadMasterHouseBillList);
		}

		#endregion

		public void TestJS_MarksAndNumbersAffectsHasChanges()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			Factory.Save();
			Assert(!shipment.HasChanges);
			shipment.JS_MarksAndNumbers = ZString.Empty;
			Assert(!shipment.HasChanges);
			shipment.JS_MarksAndNumbers = "marks and nums";
			Assert(shipment.HasChanges);
			Factory.Save();
			Assert(!shipment.HasChanges);
			shipment.JS_MarksAndNumbers = ZString.Empty;
			Assert(shipment.HasChanges);
		}

		public void TestDates_DateTimeKind_Unspecified()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, shipment.JS_E_ARV.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, shipment.JS_E_DEP.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, shipment.JS_A_RCV.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, shipment.JS_ShippedOnBoardDate.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, shipment.JS_HouseBillIssueDate.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, shipment.JS_ClientRequestedETA.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, shipment.JS_A_BKD.Kind);
		}

		public void TestMarksAndNumbersNote_ShouldGetMaxLengthFromJS_MarksAndNumbersInfo()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			Factory.Save();
			shipment.JS_MarksAndNumbers = "booo";

			var note = shipment.MarksAndNumbersNote;
			AssertNotNull(note);
			AssertEquals(shipment.JS_MarksAndNumbersInfo.MaxLength, note.NoteTextMaxLength);
		}

		[ExpectNoExceptions]
		public void TestMarksAndNumbers_SettingRichTextDoesNotThrowAnException()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_MarksAndNumbers = "bro";
			Factory.Save();

			shipment.MarksAndNumbersNote.ST_IsCustomDescription = true;
			shipment.JS_MarksAndNumbers = "hey yo!";
		}

		[ExpectNoExceptions]
		public void TestMarksAndNumbersLength()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			ZStringBuilder sb = new ZStringBuilder();

			for (int i = 0; i < 1000; i++)
			{
				sb.Append("1234567890");
			}

			shipment.JS_MarksAndNumbers = sb.ToString();
		}

		#region Pickup Delivery Confirms

		public void TestDontAttachConfirmsToBookings()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.OuterPackLines.AddNew();
			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;
			AssertEquals(1, shipment.DeliveryConfirms.Count);

			CommonShipment booking = Factory.New<CommonShipment>();
			booking.JS_TransportMode = Constants.TransportModes.Air;
			booking.JS_IsCFSRegistered = false;
			booking.JS_IsForwardRegistered = false;
			booking.JS_IsBooking = true;
			booking.OuterPackLines.AddNew();
			booking.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;
			AssertEquals(0, booking.DeliveryConfirms.Count);
		}

		public void TestDontAllowNewConfirmationsForMasterShipments_ThroughDateEntry()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			PackLine packLine = shipment2.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;
			AssertEquals(0, shipment.DeliveryConfirms.Count);
		}

		public void TestDontAllowNewConfirmationsForMasterShipments_ThroughGUI()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			PackLine packLine = shipment2.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			Assert("IsMaster, so don't support adding of confirms", !shipment.DeliveryConfirms.Relationship.SupportsAddToRelationship());
		}

		public void TestMastersShouldShowChildConfirms()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "Master";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_HouseBill = "Sub";
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			PackLine packLine = shipment2.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			shipment2.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;

			AssertEquals("child should have confirm", 1, shipment2.DeliveryConfirms.Count);
			AssertEquals("parent should have confirm", 1, shipment.DeliveryConfirms.Count);
			AssertEquals("should be the same confirm", shipment.DeliveryConfirms[0], shipment2.DeliveryConfirms[0]);
		}

		#region IsComplete

		public void TestIsComplete_Loose()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;

			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm picConfirm = shipment.PickupConfirms.AddNew();
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm dlvConfirm = shipment.DeliveryConfirms.AddNew();
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			dlvConfirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			dlvConfirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			dlvConfirm.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm.TotalDeliveredPackages = 4;
			dlvConfirm.TotalDeliveredPackages = 4;

			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm picConfirm2 = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm dlvConfirm2 = shipment.DeliveryConfirms.AddNew();

			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm2.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			picConfirm2.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			picConfirm2.EU_PickupDeliveryTime = ZDateTime.Now;
			dlvConfirm2.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			dlvConfirm2.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			dlvConfirm2.EU_PickupDeliveryTime = ZDateTime.Now;

			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
		}

		public void TestIsComplete_Loose_Outurn()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = OverseasPort2;
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			packline.JL_Outturn = 10;

			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm picConfirm = shipment.PickupConfirms.AddNew();
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm dlvConfirm = shipment.DeliveryConfirms.AddNew();
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			dlvConfirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			dlvConfirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			dlvConfirm.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm.TotalDeliveredPackages = 4;
			dlvConfirm.TotalDeliveredPackages = 4;

			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm picConfirm2 = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm dlvConfirm2 = shipment.DeliveryConfirms.AddNew();

			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			picConfirm2.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			picConfirm2.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			picConfirm2.EU_PickupDeliveryTime = ZDateTime.Now;
			dlvConfirm2.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			dlvConfirm2.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			dlvConfirm2.EU_PickupDeliveryTime = ZDateTime.Now;

			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
		}

		public void TestIsComplete_Containerised()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			CommonConsol consol = shipment.Consols.AddNew();
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;

			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm container1PicConfirm = container1.OriginConfirm;
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm container2PicConfirm = container2.OriginConfirm;
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			container1PicConfirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			container2PicConfirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			container1PicConfirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			container2PicConfirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			container1PicConfirm.EU_PickupDeliveryTime = ZDateTime.Now;
			container2PicConfirm.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm container1DlvConfirm = container1.DestinationConfirm;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			CommonPickupDeliveryConfirm container2DlvConfirm = container2.DestinationConfirm;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			container1DlvConfirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			container2DlvConfirm.EU_RequestedPickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			container1DlvConfirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			container2DlvConfirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(!shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));

			container1DlvConfirm.EU_PickupDeliveryTime = ZDateTime.Now;
			container2DlvConfirm.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned));
			Assert(shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy));
		}

		#endregion

		public void TestHasUndeliveredPackages()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;

			AssertEquals(false, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(false, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Delivery));

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			AssertEquals(true, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(true, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Delivery));

			shipment.PickupConfirms.AddNew().TotalDeliveredPackages = 5;
			shipment.DeliveryConfirms.AddNew().TotalDeliveredPackages = 5;

			AssertEquals(true, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(true, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Delivery));

			shipment.PickupConfirms.AddNew().TotalDeliveredPackages = 5;

			AssertEquals(false, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(true, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Delivery));

			shipment.DeliveryConfirms.AddNew().TotalDeliveredPackages = 100;
			AssertEquals(false, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(false, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Delivery));
		}

		public void TestHasUndeliveredPackagesPerPackLine()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 4;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 6;

			AssertEquals(true, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Delivery));
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Today.AddDays(4);

			shipment.DeliveryConfirms[0].GetDivot(packLine).J8_PackagesDelivered = 5;
			shipment.DeliveryConfirms[0].GetDivot(packLine1).J8_PackagesDelivered = 4;

			AssertEquals(true, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Delivery));
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Today.AddDays(5);

			shipment.DeliveryConfirms[0].GetDivot(packLine).J8_PackagesDelivered = 5;
			shipment.DeliveryConfirms[0].GetDivot(packLine1).J8_PackagesDelivered = 6;
			AssertEquals(false, shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Delivery));
		}

		#region Changing Shipment Packing Mode Deletes Confirms

		public void TestChangingShipmentContainerModeDeletesIncompatiableConfirms()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();
			var shipment = GetImportShipment();
			shipment.Consols.Add(consol);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.OuterPackLines.AddNew();
			shipment.DeliveryConfirms.AddNew();
			shipment.PickupConfirms.AddNew();
			Factory.Save();

			AssertEquals("Pre-condition", 1, shipment.DeliveryConfirms.Count);
			AssertEquals("Pre-condition", 1, shipment.PickupConfirms.Count);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			Factory.Save();

			AssertEquals("Expected non-containerised confirm to have been deleted", 0, shipment.DeliveryConfirms.Count);
			AssertEquals("Expected non-containerised confirm to have been deleted", 0, shipment.PickupConfirms.Count);

			var containerisedPickupConfirm = container.OriginConfirm;
			var containerisedDeliveryConfirm = container.DestinationConfirm;

			Assert("Expected to have a containerised pick up confirm attached", !container.OriginConfirm.IsDeleted);
			Assert("Expected to have a containerised delivery confirm attached", !container.DestinationConfirm.IsDeleted);

			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			Factory.Save();

			AssertNotEquals(containerisedPickupConfirm, container.OriginConfirm);
			AssertEquals(containerisedDeliveryConfirm, container.DestinationConfirm);
		}

		public void TestShipmentWithInvalidConfirmsCanSaveWhenContainerModeIsChanged()
		{
			var shipment = GetImportShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.OuterPackLines.AddNew();
			var deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			Factory.Save();

			deliveryConfirm.EU_PickupDeliveryTime = ZDateTime.Invalid;

			AssertHasErrors(deliveryConfirm.EU_PickupDeliveryTimeInfo);
			Assert(shipment.HasErrors);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			Factory.Save();

			Assert("Should have no errors as invalid confirm is deleted upon changing packing mode", !shipment.HasErrors);
		}

		public void TestConfirmationsAreNotDeletedWhenPackingModeIsCompatible()
		{
			var shipment = GetImportShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.OuterPackLines.AddNew();
			var confirm = shipment.PickupConfirms.AddNew();
			Factory.Save();

			AssertEquals("Expected non-containerised pick up confirm to be attached", 1, shipment.PickupConfirms.Count);

			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			Factory.Save();

			AssertEquals("Expected confirm to remain as RORO shipments are still non-containerised", 1, shipment.PickupConfirms.Count);
			AssertEquals("Should be the same confirmation", confirm, shipment.PickupConfirms[0]);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			Factory.Save();

			AssertEquals("Expected confirm to remain as RORO shipments are still non-containerised", 1, shipment.PickupConfirms.Count);
			AssertEquals("Should be the same confirmation", confirm, shipment.PickupConfirms[0]);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			Factory.Save();

			AssertEquals("Expected non-containerised confirm to have been deleted as it is no longer compatitable", 0, shipment.PickupConfirms.Count);
		}

		public void TestChangingShipmentType_ConfirmationAllowNewException()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertEquals("Pre-condition", false, shipment.DeliveryConfirms.AllowsNewItems);
			AssertEquals("Pre-condition", false, shipment.PickupConfirms.AllowsNewItems);
			AssertEquals("Pre-condition", false, shipment.OriginCFSArrivals.AllowsNewItems);
			AssertEquals("Pre-condition", false, shipment.OriginCFSDepartures.AllowsNewItems);
			AssertEquals("Pre-condition", false, shipment.DestinationCFSArrivals.AllowsNewItems);
			AssertEquals("Pre-condition", false, shipment.DestinationCFSDepartures.AllowsNewItems);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.OuterPackLines.AddNew();
			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Today.AddDays(1);

			AssertEquals(true, shipment.DeliveryConfirms.AllowsNewItems);
			AssertEquals(true, shipment.PickupConfirms.AllowsNewItems);
			AssertEquals(true, shipment.OriginCFSArrivals.AllowsNewItems);
			AssertEquals(true, shipment.OriginCFSDepartures.AllowsNewItems);
			AssertEquals(true, shipment.DestinationCFSArrivals.AllowsNewItems);
			AssertEquals(false, shipment.DestinationCFSDepartures.AllowsNewItems);
		}

		#endregion

		#endregion

		public void TestIsRoot()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			var packLine = shipment2.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 99;
			packLine.JL_ActualWeight = 111.111m;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine.JL_ActualVolume = 222.222m;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			Factory.Save();

			var shipment_FreshFactory = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			Assert("packlines would not be readonly", !shipment_FreshFactory.OuterPackLines[0].ReadOnly);

			shipment_FreshFactory = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			shipment_FreshFactory.IsRoot = true;
			Assert("packlines would not be readonly", !shipment_FreshFactory.OuterPackLines[0].ReadOnly);

			shipment_FreshFactory = new BusinessObjectFactory().Load<CommonShipment>(shipment2.PK);
			Assert("packlines would not be readonly", !shipment_FreshFactory.OuterPackLines[0].ReadOnly);

			shipment_FreshFactory = new BusinessObjectFactory().Load<CommonShipment>(shipment2.PK);
			shipment_FreshFactory.IsRoot = true;
			Assert("packlines would not be readonly", !shipment_FreshFactory.OuterPackLines[0].ReadOnly);
		}

		public void TestRunGetDocumentLoginOfJobDeclaration()
		{
			var errorMessage = @"Delivery of this document is restricted because:
       The Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this document?
";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var shipment = Factory.NewWithValidTestData<CommonShipment>();
				shipment.JS_IsForwardRegistered = true;
				shipment.JS_IsCFSRegistered = false;
				shipment.JS_IsBooking = false;

				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "TOg5";
				importer.CompanyData.OB_IsDebtor = ZBool.True;
				importer.CompanyData.OB_AROnCreditHold = ZBool.True;

				Factory.Save();
				importer.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				AssertEquals(true, importer.CreditChecker.IsCreditOnHold());
				var dec = Factory.New<IBaseJobDeclaration>();
				((BusinessObject)dec).FillWithValidTestData();
				dec.JE_MessageType = "IMP";
				dec.JE_JS = shipment.PK;
				dec.JE_OH_Importer = importer.PK;
				Factory.Save();

				var commandFilter = new DocumentZQuery("Customs", "B3 (Current Data)");
				var b3Command = Factory.LoadTop1<DocumentCommand>(commandFilter);
				b3Command.Parent = shipment;

				var guiManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>();
				guiManager.Initialise(b3Command.Parent as ICreditControlledBusinessObject);
				var result = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement((BusinessObject)dec, "document", b3Command.PK);
				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSuspendDeclarationForDocuments()
		{
			var shipment = Factory.New<CommonShipment>();
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			AssertNotNull(shipment.DeclarationForDocuments);
			using (shipment.SuspendDeclarationForDocuments())
			{
				AssertNull(shipment.DeclarationForDocuments);
			}
			AssertNotNull(shipment.DeclarationForDocuments);
		}

		#region TestDeclarationForDocuments_CorrectDeclarationForCurrentCompany

		public void TestDeclarationForDocuments_CorrectDeclarationForCurrentCompany()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company3 = Factory.NewWithValidTestData<GlbCompany>();

			var branch1 = company1.Branches.AddNew();
			branch1.FillWithValidTestData();
			var branch2 = company2.Branches.AddNew();
			branch2.FillWithValidTestData();
			var branch3 = company2.Branches.AddNew();
			branch3.FillWithValidTestData();
			var branch4 = company3.Branches.AddNew();
			branch4.FillWithValidTestData();
			var branch5 = company3.Branches.AddNew();
			branch5.FillWithValidTestData();

			Factory.Save();

			var shipment = Factory.New<CommonShipmentForSyncTest>();

			var mockDec1 = new Mock<IJobDeclarationForSyncTest>();
			var mockDec2 = new Mock<IJobDeclarationForSyncTest>();
			var mockDec3 = new Mock<IJobDeclarationForSyncTest>();
			var dec1 = mockDec1.Object;
			var dec2 = mockDec2.Object;
			var dec3 = mockDec3.Object;

			mockDec1.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockDec1.Setup(m => m.JE_GB).Returns(branch1.PK);
			AssertEquals("Dec1 created for branch1", branch1.PK, dec1.JE_GB);

			mockDec2.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockDec2.Setup(m => m.JE_GB).Returns(branch2.PK);
			AssertEquals("dec2 created for branch2", branch2.PK, dec2.JE_GB);

			mockDec3.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockDec3.Setup(m => m.JE_GB).Returns(branch4.PK);
			AssertEquals("dec3 created for branch4", branch4.PK, dec3.JE_GB);

			mockDec1.Setup(m => m.ShouldSynchroniseWithShipmentForDocument).Returns(true);
			mockDec2.Setup(m => m.ShouldSynchroniseWithShipmentForDocument).Returns(true);
			mockDec3.Setup(m => m.ShouldSynchroniseWithShipmentForDocument).Returns(false);

			shipment.SetDeclarations(new IBaseJobDeclaration[] { dec1, dec2, dec3 });

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company1", branch1.GB_GC, GlbCompany.CurrentCompany.PK);

				mockDec1.Setup(m => m.SynchroniseWithShipmentIfNeeded());
				AssertEquals("DeclarationForDocuments is dec1 when branch is branch1", dec1.PK, shipment.DeclarationForDocuments.PK);
				mockDec1.VerifyAll();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company2", branch2.GB_GC, GlbCompany.CurrentCompany.PK);

				mockDec2.Setup(m => m.SynchroniseWithShipmentIfNeeded());
				AssertEquals("DeclarationForDocuments is dec2 when branch is branch2", dec2.PK, shipment.DeclarationForDocuments.PK);
				mockDec2.VerifyAll();
			}

			AssertEquals("Branches 2 and 3 have common company", branch2.GB_GC, branch3.GB_GC);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company2", branch3.GB_GC, GlbCompany.CurrentCompany.PK);
				mockDec2.Verify(m => m.SynchroniseWithShipmentIfNeeded(), Times.Once());
				AssertEquals("DeclarationForDocuments is dec2 when branch is branch3", dec2.PK, shipment.DeclarationForDocuments.PK);
				mockDec2.VerifyAll();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch4.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company3", branch4.GB_GC, GlbCompany.CurrentCompany.PK);

				mockDec3.Verify(m => m.SynchroniseWithShipmentIfNeeded(), Times.Never());
				AssertEquals("DeclarationForDocuments is dec3 when branch is branch4", dec3.PK, shipment.DeclarationForDocuments.PK);
				mockDec3.VerifyAll();
			}

			AssertEquals("Branches 4 and 5 have common company", branch4.GB_GC, branch5.GB_GC);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch5.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company3", branch5.GB_GC, GlbCompany.CurrentCompany.PK);
				mockDec3.Verify(m => m.SynchroniseWithShipmentIfNeeded(), Times.Never());
				AssertEquals("DeclarationForDocuments is dec3 when branch is branch5", dec3.PK, shipment.DeclarationForDocuments.PK);
				mockDec3.VerifyAll();
			}
		}

		public interface IJobDeclarationForSyncTest : IJobDeclarationWithShipmentSynchonisation, IDocumentSupportable
		{
		}

		class CommonShipmentForSyncTest : CommonShipment
		{
			public CommonShipmentForSyncTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override IBaseJobDeclaration[] Declarations => declarations;
			IBaseJobDeclaration[] declarations;

			public void SetDeclarations(IBaseJobDeclaration[] declarations)
			{
				this.declarations = declarations;
			}
		}

		public void TestNctsHeaderForDocuments_CorrectNctsHeaderForCurrentCompany()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company1.Branches.AddNew();
			branch1.FillWithValidTestData();
			var branch2 = company2.Branches.AddNew();
			branch2.FillWithValidTestData();
			var branch3 = company2.Branches.AddNew();
			branch3.FillWithValidTestData();
			Factory.Save();

			var shipment = Factory.New<CommonShipment>();
			var nctsHeader1 = CreateNctsHeader(branch1, shipment);
			var nctsHeader2 = CreateNctsHeader(branch2, shipment, CusInBondApplicationCodeList.Codes.NCTS5);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company1", branch1.GB_GC, GlbCompany.CurrentCompany.PK);
				AssertEquals("NctsHeaderForDocuments is nctsHeader1 when branch is branch1", nctsHeader1.PK, shipment.NctsHeaderForDocuments.PK);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company2", branch2.GB_GC, GlbCompany.CurrentCompany.PK);
				AssertEquals("NctsHeaderForDocuments is nctsHeader2 when branch is branch2", nctsHeader2.PK, shipment.NctsHeaderForDocuments.PK);
			}

			AssertEquals("Branches 2 and 3 have common company", branch2.GB_GC, branch3.GB_GC);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("CurrentCompany is Company2", branch3.GB_GC, GlbCompany.CurrentCompany.PK);
				AssertEquals("NctsHeaderForDocuments is nctsHeader2 when branch is branch3", nctsHeader2.PK, shipment.NctsHeaderForDocuments.PK);
			}

			BusinessObject CreateNctsHeader(GlbBranch branch, CommonShipment commonShipment, string applicationCode = CusInBondApplicationCodeList.Codes.NCTS4)
			{
				var nctsHeader = (BusinessObject)Factory.New<EU.NCTS.ICusInBondHeader>();
				nctsHeader[CusInBondHeaderSchema.BH_ParentID] = commonShipment.PK;
				nctsHeader[CusInBondHeaderSchema.BH_ApplicationCode] = applicationCode;
				nctsHeader[CusInBondHeaderSchema.BH_GB] = branch.PK;
				var nctsDepartureMovement = (BusinessObject)Factory.New<EU.NCTS.IDepartureMovementHeader>();
				nctsDepartureMovement[CusInBondMoveHeaderSchema.BM_BH] = nctsHeader.PK;
				return nctsHeader;
			}
		}

		public void TestNctsHeaderForDocuments_ResolveTypeFromObjectFactoryForDocData()
		{
			var nctsHeaderType = ObjectFactory.GetType<EU.NCTS.ICusInBondHeader>();
			var shipment = Factory.New<CommonShipment>();
			var nctsHeader = (BusinessObject)Factory.New<EU.NCTS.ICusInBondHeader>();
			nctsHeader[CusInBondHeaderSchema.BH_ParentID] = shipment.PK;

			Assert("shipment.NctsHeaderForDocuments is in inheritance hierarchy of resolved type from ObjectFactory", nctsHeaderType.IsInstanceOfType(shipment.NctsHeaderForDocuments));

			var propertyInfo = shipment.GetType().GetProperty(nameof(CommonShipment.NctsHeaderForDocuments));
			Assert("ResolveTypeFromObjectFactoryForDocDataAttribute", Attribute.IsDefined(propertyInfo, typeof(ResolveTypeFromObjectFactoryForDocDataAttribute), false));
		}

		#endregion

		#region EditableChildren

		public void TestEditableChildren()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			CommonConsol consol1 = shipment1.Consols.AddNew();
			CommonContainer container1 = consol1.Containers.AddNew();
			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			packline1.SetContainer(consol1, container1);
			CommonPickupDeliveryConfirm confirm1 = container1.DestinationConfirm;
			AssertEquals("Now the containers should be editable from the shipment when it is ForwardingShipment only", ContainersNeedToBeEditableRegistered, shipment1.IsRegisteredEditableChildObject(shipment1.ContainersForBinding));
			Assert("And should have severed the link with consols", !consol1.IsRegisteredEditableChildObject(consol1.Containers));

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			CommonConsol consol1_ = newFactory.Load<CommonConsol>(consol1.PK);
			CommonContainer container2_ = consol1_.Containers.AddNew();
			CommonShipment shipment1_ = consol1_.Shipments[0];
			PackLine packline2_ = shipment1_.OuterPackLines.AddNew();
			packline2_.SetContainer(consol1_, container2_);

			CommonShipment shipment2_ = consol1_.Shipments.AddNew();
			PackLine packline3_ = shipment2_.OuterPackLines.AddNew();
			packline3_.SetContainer(consol1_, container2_);

			CommonConsol consol2_ = shipment1_.Consols.AddNew();
			CommonContainer container3_ = consol2_.Containers.AddNew();
			packline2_.SetContainer(consol2_, container3_);

			Assert("Normally the shipment doesn't register containers as editable children", !shipment1_.IsRegisteredEditableChildObject(shipment1_.ContainersForBinding));
			Assert("Normally the Consol does", consol1_.IsRegisteredEditableChildObject(consol1_.Containers));
			Assert("Normally the Consol does", consol2_.IsRegisteredEditableChildObject(consol2_.Containers));

			newFactory.Save();

			CommonContainer container2 = Factory.Load<CommonContainer>(container2_.PK);
			CommonContainer container3 = Factory.Load<CommonContainer>(container3_.PK);
			CommonConsol consol2 = Factory.Load<CommonConsol>(consol2_.PK);

			AssertEquals("Containers should be editable from the shipment when it is ForwardingShipment only", ContainersNeedToBeEditableRegistered, shipment1.IsRegisteredEditableChildObject(shipment1.ContainersForBinding));
			Assert("And should have severed the link with consols", !consol1.IsRegisteredEditableChildObject(consol1.Containers));
			Assert("And should have severed the link with consols", !consol2.IsRegisteredEditableChildObject(consol2.Containers));
		}

		#endregion

		#region BOL Checking

		public void TestBOLPrintingLessThanAllowedPackLines()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m))
			{
				CommonShipment shipment = GetShipmentWithRelatedOrg();

				PackLine line1 = shipment.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 400;
				PackLine line2 = shipment.OuterPackLines.AddNew();
				line2.JL_LinePrice = 400;
				line2.JL_HarmonisedCode = "c1";
				PackLine line3 = shipment.OuterPackLines.AddNew();
				line3.JL_LinePrice = 400;
				line3.JL_HarmonisedCode = "c2";

				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
					AssertEquals("shouldn't require BOL confirmation, less packlines than registry", false, shipment.AWBOrHBLPrintingShouldBeConfirmed());
					AssertBizServiceBOLPrintingShouldBeCancelledCalled(false, shipment);
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(currentCountry);
				}
			}
		}

		CommonShipment GetShipmentWithRelatedOrg()
		{
			var newFactory = new BusinessObjectFactory();
			OrgHeader consignor = newFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader relatedParty = newFactory.NewWithValidTestData<OrgHeader>();
			consignor.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, "SEA", "");
			GlbCompany.GetCurrentCompany(newFactory).GC_OH_OrgProxy = relatedParty.PK;
			newFactory.Save();

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			return shipment;
		}

		public void TestBOLPrintingMoreThanAllowedPackLinesCountryNonUS()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m))
			{
				CommonShipment shipment = GetShipmentWithRelatedOrg();

				PackLine line1 = shipment.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 1400;
				PackLine line2 = shipment.OuterPackLines.AddNew();
				line2.JL_LinePrice = 1400;
				line2.JL_HarmonisedCode = "c1";
				PackLine line3 = shipment.OuterPackLines.AddNew();
				line3.JL_LinePrice = 1400;
				line3.JL_HarmonisedCode = "c2";

				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Fiji);
					AssertEquals("shouldn't require BOL confirmation, country is not US", false, shipment.AWBOrHBLPrintingShouldBeConfirmed());
					AssertBizServiceBOLPrintingShouldBeCancelledCalled(false, shipment);
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(currentCountry);
				}
			}
		}

		public void TestBOLPrintingMoreThanAllowedPackLinesCustomsEntryIsNotEmpty()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m))
			{
				CommonShipment shipment = GetShipmentWithRelatedOrg();

				PackLine line1 = shipment.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 1400;
				PackLine line2 = shipment.OuterPackLines.AddNew();
				line2.JL_LinePrice = 1400;
				line2.JL_HarmonisedCode = "c1";
				PackLine line3 = shipment.OuterPackLines.AddNew();
				line3.JL_LinePrice = 1400;
				line3.JL_HarmonisedCode = "c2";

				shipment.CustomsEntryNumber = "no";
				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
					AssertEquals("shouldn't require BOL confirmation, customs entry is not empty", false, shipment.AWBOrHBLPrintingShouldBeConfirmed());
					AssertBizServiceBOLPrintingShouldBeCancelledCalled(false, shipment);
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(currentCountry);
				}
			}
		}

		public void TestBOLPrintingMoreThanAllowedPackLinesNoRelatedParty()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m))
			{
				CommonShipment shipment = Factory.New<CommonShipment>();

				PackLine line1 = shipment.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 1400;
				PackLine line2 = shipment.OuterPackLines.AddNew();
				line2.JL_LinePrice = 1400;
				line2.JL_HarmonisedCode = "c1";
				PackLine line3 = shipment.OuterPackLines.AddNew();
				line3.JL_LinePrice = 1400;
				line3.JL_HarmonisedCode = "c2";

				shipment.CustomsEntryNumber = "no";
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
				{
					AssertEquals("shouldn't require BOL confirmation, customs entry is not empty", false, shipment.AWBOrHBLPrintingShouldBeConfirmed());
					AssertBizServiceBOLPrintingShouldBeCancelledCalled(false, shipment);
				}
			}
		}

		public void TestBOLPrintingMoreThanAllowedPackLinesNoRelatedParty_DontCheckCompany()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m))
			{
				CommonShipment shipment = Factory.New<CommonShipment>();

				PackLine line1 = shipment.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 1400;
				PackLine line2 = shipment.OuterPackLines.AddNew();
				line2.JL_LinePrice = 1400;
				line2.JL_HarmonisedCode = "c1";
				PackLine line3 = shipment.OuterPackLines.AddNew();
				line3.JL_LinePrice = 1400;
				line3.JL_HarmonisedCode = "c2";

				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
					using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentAuditShouldCheckCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						AssertEquals("shouldn't require BOL confirmation, it'll check the company", false, shipment.AWBOrHBLPrintingShouldBeConfirmed());
						AssertBizServiceBOLPrintingShouldBeCancelledCalled(false, shipment);
					}

					using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentAuditShouldCheckCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						AssertEquals("should require BOL confirmation, it won't check the company", true, shipment.AWBOrHBLPrintingShouldBeConfirmed());
						AssertBizServiceBOLPrintingShouldBeCancelledCalled(true, shipment);
					}

					using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentAuditShouldCheckCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						shipment = GetShipmentWithRelatedOrg();

						line1 = shipment.OuterPackLines.AddNew();
						line1.JL_HarmonisedCode = "c2";
						line1.JL_LinePrice = 1400;

						AssertEquals("should require BOL confirmation, it will check the company which has a related org", true, shipment.AWBOrHBLPrintingShouldBeConfirmed());
						AssertBizServiceBOLPrintingShouldBeCancelledCalled(true, shipment);
					}
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(currentCountry);
				}
			}
		}

		public void TestBOLPrintingMoreThanAllowedPackLinesCustomsEntryIsEmptyCountryUSHasSecurity_Cancel()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m))
			{
				CommonShipment shipment = GetShipmentWithRelatedOrg();
				shipment.JS_UniqueConsignRef = "SS1";
				PackLine line1 = shipment.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 1400;
				PackLine line2 = shipment.OuterPackLines.AddNew();
				line2.JL_LinePrice = 1400;
				line2.JL_HarmonisedCode = "c1";
				PackLine line3 = shipment.OuterPackLines.AddNew();
				line3.JL_LinePrice = 1400;
				line3.JL_HarmonisedCode = "c2";

				shipment.CustomsEntryNumber = "";

				Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;

				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
					AssertEquals("should require BOL confirmation", true, shipment.AWBOrHBLPrintingShouldBeConfirmed());
					AssertBizServiceBOLPrintingShouldBeCancelledCalled(true, shipment);
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(currentCountry);
				}
			}
		}

		public void TestBOLPrintingMoreThanAllowedPackLinesCustomsEntryIsEmptyCountryUSHasSecurity_OK()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m))
			{
				CommonShipment shipment = GetShipmentWithRelatedOrg();
				shipment.JS_UniqueConsignRef = "SS1";
				PackLine line1 = shipment.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 1400;
				PackLine line2 = shipment.OuterPackLines.AddNew();
				line2.JL_LinePrice = 1400;
				line2.JL_HarmonisedCode = "c1";
				PackLine line3 = shipment.OuterPackLines.AddNew();
				line3.JL_LinePrice = 1400;
				line3.JL_HarmonisedCode = "c2";

				shipment.CustomsEntryNumber = "";

				Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;

				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
					AssertEquals("should require BOL confirmation", true, shipment.AWBOrHBLPrintingShouldBeConfirmed());
					AssertBizServiceBOLPrintingShouldBeCancelledCalled(true, shipment);
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(currentCountry);
				}
			}
		}

		public void TestBOLPrintingMoreThanAllowedPackLinesCustomsEntryIsEmptyCountryUSDoesntHaveSecurity()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m))
			{
				CommonShipment shipment = GetShipmentWithRelatedOrg();
				shipment.JS_UniqueConsignRef = "SS1";
				PackLine line1 = shipment.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 1400;
				PackLine line2 = shipment.OuterPackLines.AddNew();
				line2.JL_LinePrice = 1400;
				line2.JL_HarmonisedCode = "c1";
				PackLine line3 = shipment.OuterPackLines.AddNew();
				line3.JL_LinePrice = 1400;
				line3.JL_HarmonisedCode = "c2";

				shipment.CustomsEntryNumber = "";

				Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = false;

				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
					AssertEquals("should require BOL confirmation", true, shipment.AWBOrHBLPrintingShouldBeConfirmed());
					AssertBizServiceBOLPrintingShouldBeCancelledCalled(true, shipment);
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(currentCountry);
				}
			}
		}

		void AssertBizServiceBOLPrintingShouldBeCancelledCalled(bool expectBOLPrintingShouldBeCancelledCalled, CommonShipment shipment)
		{
			var queryProvider = new Mock<ICommonShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);

			Factory.SetValue(() => queryProvider.Object);

			if (expectBOLPrintingShouldBeCancelledCalled)
			{
				queryProvider.Setup(m => m.ConfirmBOLPrinting(It.IsAny<CommonShipment>())).Returns(false);
			}
			else
			{
				queryProvider.Setup(m => m.ConfirmBOLPrinting(It.IsAny<CommonShipment>()));
			}
			DocumentEventsForTest events = new DocumentEventsForTest();
			CommonShipmentDocumentSupporter supporter = new CommonShipmentDocumentSupporter(shipment);
			supporter.Initialise(events);

			StmMenuItem bolMenuItem = Factory.New<StmMenuItem>();
			bolMenuItem.SU_MenuName = "Bill Of Lading XYZ";
			events.FireDocumentPrintRequested(new DocumentCancelEventArgs(bolMenuItem));

			queryProvider.Verify(m => m.ConfirmBOLPrinting(It.IsAny<CommonShipment>()),
				expectBOLPrintingShouldBeCancelledCalled ? Times.AtLeastOnce() : Times.Never());
		}

		#endregion

		#region ImportPickupAddress

		public void TestImportPickupAddress()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			CommonConsol consol = CreateImportConsol(shipment);
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			address.OA_Address1 = "UnpackDepotAddress";
			consol.JK_OA_UnpackDepotAddress = address.PK;

			AssertNotNull("Pickup From Address is not null", shipment.ImportPickUpAddress);
			AssertEquals("Pickup From Address field is Arrival Consol's Arrival depot address", address.OA_Address1.Trim(), shipment.ImportPickUpAddress.OA_Address1);
			AssertEquals("Pickup From City field is Arrival Consol's Arrival depot address", address.OA_City.Trim(), shipment.ImportPickUpAddress.OA_City);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Pickup From Address field is Arrival Consol's Arrival depot address UnpackDepotAddress", address.OA_Address1.Trim(), shipment.ImportPickUpAddress.OA_Address1);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OA_UnpackDepotAddress = address.PK;
			AssertEquals("Pickup From Address field is Arrival Consol's Arrival depot address", address.OA_Address1.Trim(), shipment.ImportPickUpAddress.OA_Address1);
			AssertEquals("Pickup From City field is Arrival Consol's Arrival depot address", address.OA_City.Trim(), shipment.ImportPickUpAddress.OA_City);

			var cTOArrival = Factory.NewWithValidTestData<OrgAddress>();
			cTOArrival.OA_Address1 = "ArrivalCTOAddress";
			consol.JK_OA_ArrivalCTOAddress = cTOArrival.PK;

			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("PickUp From Address field is CTO Arrival Address", cTOArrival.OA_Address1.Trim(), shipment.ImportPickUpAddress.OA_Address1);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("PickUp From Address field is CTO Arrival Address", cTOArrival.OA_Address1.Trim(), shipment.ImportPickUpAddress.OA_Address1);

			var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
			importReleaseDepot.OA_Address1 = "ImportReleaseDepot";
			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
			AssertEquals("Should Be Import Release Depot", importReleaseDepot.OA_Address1, shipment.ImportPickUpAddress.OA_Address1);
		}

		CommonConsol CreateImportConsol(CommonShipment shipment)
		{
			CommonConsol consol = shipment.Consols.AddNew();

			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;
			return consol;
		}

		#endregion

		#region DepotOrCTOHeading

		public void TestDepotOrCTOHeading()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FTL;
			AssertEquals(shipment.DepotOrCTOHeading, "DEPOT");
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(shipment.DepotOrCTOHeading, "CTO");
		}

		#endregion

		public void TestJS_A_RCVAttribute()
		{
			PropertyInfo info = PropertyInfoFetcher.GetFromLowestSubclass(typeof(CommonShipment), JobShipmentSchema.JS_A_RCV.Name);

			bool correctAttr = false;

			foreach (object attr in info.GetCustomAttributes(false))
			{
				correctAttr = attr.GetType() == typeof(EventDatePropertyAttribute) && ((EventDatePropertyAttribute)attr).EventType == AutoEvents.InterimReceiptProduced.Code
					&& ((EventDatePropertyAttribute)attr).EstimateActual == EstimateActual.Actual;
				if (correctAttr)
				{
					break;
				}
			}

			AssertEquals("JS_A_RCV should have attribute [EventDateProperty(AutoEvents.InterimReceiptProduced, EstimateActual.Actual)]", true, correctAttr);
		}

		#region TestIsReceived

		public void TestIsReceived()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			Assert("Not expecting booking to be received.", !shipment.IsReceived);

			shipment.JS_A_RCV = ZDateTime.Today;
			Assert("Expecting booking to be received.", shipment.IsReceived);

			shipment.JS_A_RCV = ZDateTime.Empty;
			shipment.JS_InterimReceipt = "9999";
			Assert("Expecting booking to be received.", shipment.IsReceived);

			shipment.JS_A_RCV = ZDateTime.Today;
			Assert("Expecting booking to be received.", shipment.IsReceived);

			shipment.JS_InterimReceipt = ZString.Empty;
			shipment.JS_A_RCV = ZDateTime.Empty;
			Assert("Not expecting booking to be received.", !shipment.IsReceived);
		}

		#endregion

		public void TestDecForDocMessageTypes()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var shipment = Factory.New<CommonShipment>();

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_MessageType = "IMP";
			AssertEquals(",IMP,", shipment.DecForDocMessageTypes);

			declaration.JE_MessageType = "EXP";
			AssertNotNull("DeclarationForDocuments", shipment.DeclarationForDocuments);
			AssertEquals(",EXP,", shipment.DecForDocMessageTypes);
		}

		#region ExportStatement
		public void TestExportStatement()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			CountryExportStatementSetting uSCountrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value["US"];
			AssertNotNull("PreCondition: USCountrySettingCollection", uSCountrySettingCollection);
			AssertNotEquals("Statements for US", 0, uSCountrySettingCollection.Statements.Count);
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("IsTranshipped", false, shipment.IsCrossTrade());
			AssertEquals("IsExport", true, shipment.IsExport());
			shipment.DocsAndCartage.JP_ExportStatement = "";
			AssertNull("ExportStatementSetting", shipment.ExportStatementSetting);
			AssertEquals("ExportStatement", "", shipment.ExportStatement);

			shipment.DocsAndCartage.JP_ExportStatement = uSCountrySettingCollection.Statements[0].Code;
			AssertNotNull("ExportStatementSetting", shipment.ExportStatementSetting);
			AssertNull("DeclarationForDocuments", shipment.DeclarationForDocuments);
			string shipmentExportStatement = new ShipmentExportStatementCreator(shipment, shipment.ExportStatementSetting, "MM/dd/yyyy").ExportStatement;
			AssertEquals("ExportStatement", shipmentExportStatement, shipment.ExportStatement);

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_MessageType = "IMP";
			AssertEquals("IMP", shipment.DecForDocMessageType);

			declaration.JE_MessageType = "EXP";
			AssertNotNull("DeclarationForDocuments", shipment.DeclarationForDocuments);
			AssertEquals("EXP", shipment.DecForDocMessageType);
			AssertEquals("ExportStatement", shipmentExportStatement, shipment.ExportStatement);

			BusinessObject entryHeader = (BusinessObject)Factory.New<ICusEntryHeader>();
			entryHeader[CusEntryHeaderSchema.Constants.CH_JE] = declaration.PK;

			IExportStatement declarationForDocuments = shipment.DeclarationForDocuments as IExportStatement;
			AssertEquals("ExportStatement", "AES", shipment.ExportStatement);

			shipment.DocsAndCartage.JP_ExportStatement = "";
			AssertEquals("ExportStatement", "", shipment.ExportStatement);

			shipment.JS_RL_NKOrigin = "NZAKL";
			AssertEquals("IsTranshipped", true, shipment.IsCrossTrade());
			AssertEquals("IsExport", false, shipment.IsExport());
			shipment.DocsAndCartage.JP_ExportStatement = uSCountrySettingCollection.Statements[0].Code;
			AssertEquals("ExportStatement", declarationForDocuments.GetExportStatement(shipment.ExportStatementSetting), shipment.ExportStatement);
		}

		public void TestExportStatementSetting()
		{
			CountryExportStatementSetting uSCountrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value["US"];
			AssertNotNull("PreCondition: USCountrySettingCollection", uSCountrySettingCollection);
			AssertNotEquals("Statements for US", 0, uSCountrySettingCollection.Statements.Count);

			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertNull(shipment.ExportStatementSetting);

			shipment.JS_RL_NKOrigin = "USLAX";
			AssertNull(shipment.ExportStatementSetting);

			shipment.DocsAndCartage.JP_ExportStatement = uSCountrySettingCollection.Statements[0].Code;
			AssertNotNull(shipment.ExportStatementSetting);

			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertNull(shipment.ExportStatementSetting);
		}

		public void TestExportStatement_FallbackToShipmentWhenCustomsReturnEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				CountryExportStatementSetting uSCountrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value["US"];

				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.DocsAndCartage.JP_ExportStatement = uSCountrySettingCollection.Statements[0].Code;

				BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
				declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
				declaration[JobDeclarationSchema.Constants.JE_MessageStatus] = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;

				BusinessObject entryHeader = (BusinessObject)Factory.New<ICusEntryHeader>();
				entryHeader[CusEntryHeaderSchema.Constants.CH_JE] = declaration.PK;

				IExportStatement declarationForDocuments = shipment.DeclarationForDocuments as IExportStatement;
				AssertEquals("Export statement fallback to shipment when customs return empty export statement", "AES", shipment.ExportStatement);
			}
		}

		public void TestExportStatement_CargoIMP_USTerritories()
		{
			TestExportStatement_CargoIMP(Constants.CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestExportStatement_CargoIMP(Constants.CountryCodes.Guam, "GUGUM", "AUSYD");
			TestExportStatement_CargoIMP(Constants.CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestExportStatement_CargoIMP(Constants.CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestExportStatement_CargoIMP(Constants.CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestExportStatement_CargoIMP(Constants.CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestExportStatement_CargoIMP(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PRF",
						"AES", "AES Proof of Filing Citation", CusEntryNumberTypes.UnitedStates.ITN, "", "UDF", true,
						true, true, true, true, true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PDU",
						"AESPOST", "Postdeparture Citation-USPPI", "SHP", "DOE", "UDF", true, true, true, true, true,
						true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "LOW",
						"NOEEI §30.37(a)", "NOEEI §30.37(a) - Low Value (<$2501)", "", "", "UDF", true, true, true,
						true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var filer = new ExportEntryFilerID();
					filer.EntryFilerID = "111111111";
					filer.EntryFilerIDType = "D";
					ObjectFactory.Get<US.IUSCustomsDataRegistry>().ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
					CommonShipment shipment = Factory.New<CommonShipment>();
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = destination;

					shipment.DocsAndCartage.JP_ExportStatement = "PRF";

					var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
					cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
					cusEntryNumber1.CE_EntryNum = "X20100101987654";

					var exportStatement = shipment.ExportStatement_CargoIMP;
					AssertEquals("X20100101987654", exportStatement.Trim());

					shipment.DocsAndCartage.JP_ExportStatement = "PDU";
					shipment.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var consignor = Factory.New<OrgHeader>();
					consignor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12345678912");
					shipment.ConsignorPK = consignor.PK;

					exportStatement = shipment.ExportStatement_CargoIMP;
					AssertEquals("12345678912 20101001", exportStatement.Trim());

					shipment.DocsAndCartage.JP_ExportStatement = "DWN";
					shipment.JS_E_DEP = new ZDateTime(2010, 10, 01);

					exportStatement = shipment.ExportStatement_CargoIMP;
					AssertEquals("111111111 20101001", exportStatement.Trim());

					shipment.DocsAndCartage.JP_ExportStatement = "LOW";
					exportStatement = shipment.ExportStatement_CargoIMP;
					AssertEquals(String.Empty, exportStatement.Trim());
				}
			}
		}
		#endregion

		#region TestPackLocations

		public void TestPackLocations()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals("Should be no pack locations on CommonShipment (pack count is 0)", 0, shipment.PackLocations.Count);

			shipment.JS_OuterPacks = 5;
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			AssertEquals("Should be pack locations on Shipment", 2, shipment.PackLocations.Count);
		}

		#endregion

		#region TestOnSavingShipment

		public void TestOnSavingShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.OnSavingShipment += delegate
			{
				Assert(true);
			};

			shipment.Factory.Save();
		}

		#endregion

		#region TestOnShipmentSaveed

		public void TestOnShipmentSaveed()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.OnShipmentSaved += delegate
			{
				Assert(true);
			};

			shipment.Factory.Save();
		}

		#endregion

		#region TestConsigneeName, TestConsignorName

		public void TestConsigneeNameOrPKMaxLength()
		{
			CommonShipment shipment = GetShipment();

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(JobDocAddressSchema.E2_CompanyName.MaxLength, shipment.ConsigneeNameOrPKInfo.MaxLength);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			AssertEquals(ZGuid.Empty.ToString().Length, shipment.ConsigneeNameOrPKInfo.MaxLength);
		}

		public void TestConsignorNameOrPKMaxLength()
		{
			CommonShipment shipment = GetShipment();

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(JobDocAddressSchema.E2_CompanyName.MaxLength, shipment.ConsignorNameOrPKInfo.MaxLength);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			AssertEquals(ZGuid.Empty.ToString().Length, shipment.ConsignorNameOrPKInfo.MaxLength);
		}

		#endregion

		#region TestArrivalConsolForDocuments
		public void TestArrivalConsolForDocuments()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_UniqueConsignRef = "CONSOL1";
			consol1.JK_RL_NKDischargePort = OverseasPort;
			consol1.JK_RL_NKLoadPort = HomePort;

			CommonConsol arrivalConsol = shipment.ArrivalConsolForDocuments;
			AssertEquals("Arrival Consol", consol1.JK_UniqueConsignRef, arrivalConsol.JK_UniqueConsignRef);

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_UniqueConsignRef = "CONSOL2";
			consol2.JK_RL_NKDischargePort = OverseasPort2;
			consol2.JK_RL_NKLoadPort = OverseasPort;

			arrivalConsol = shipment.ArrivalConsolForDocuments;
			AssertEquals("Arrival Consol", consol2.JK_UniqueConsignRef, arrivalConsol.JK_UniqueConsignRef);

			CommonConsol consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol3.JK_UniqueConsignRef = "CONSOL3";
			consol3.JK_RL_NKDischargePort = AlternateHomePort;
			consol3.JK_RL_NKLoadPort = OverseasPort2;

			arrivalConsol = shipment.ArrivalConsolForDocuments;
			AssertEquals("Arrival Consol", consol3.JK_UniqueConsignRef, arrivalConsol.JK_UniqueConsignRef);
		}
		#endregion

		#region TestDepartureConsolForDocuments
		public void TestDepartureConsolForDocuments()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_UniqueConsignRef = "CONSOL1";
			consol1.JK_RL_NKDischargePort = HomePort;
			consol1.JK_RL_NKLoadPort = OverseasPort;

			CommonConsol departureConsol = shipment.DepartureConsolForDocuments;
			AssertEquals("Departure Consol", consol1.JK_UniqueConsignRef, departureConsol.JK_UniqueConsignRef);

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_UniqueConsignRef = "CONSOL2";
			consol2.JK_RL_NKDischargePort = OverseasPort;
			consol2.JK_RL_NKLoadPort = OverseasPort2;

			departureConsol = shipment.DepartureConsolForDocuments;
			AssertEquals("Departure Consol", consol2.JK_UniqueConsignRef, departureConsol.JK_UniqueConsignRef);

			CommonConsol consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol3.JK_UniqueConsignRef = "CONSOL3";
			consol3.JK_RL_NKDischargePort = OverseasPort2;
			consol3.JK_RL_NKLoadPort = AlternateHomePort;

			departureConsol = shipment.DepartureConsolForDocuments;
			AssertEquals("Departure Consol", consol3.JK_UniqueConsignRef, departureConsol.JK_UniqueConsignRef);
		}
		#endregion

		#region TestDepartureArrivalConsol DRT Shipment With pre-carriage and on-forwarding AGT consols

		public void TestDepartureArrivalConsol_DRTShipmentWithPreCarriageOnForwardingAGTConsols()
		{
			var (shipment, departureConsol, arrivalConsol) = SetUpDRTShipmentAttachedToPreCarriageOnForwardingAGTConsols(Constants.TransportModes.Sea);
			AssertEquals(departureConsol, shipment.DepartureConsol);
			AssertEquals(arrivalConsol, shipment.ArrivalConsol);
		}

		public void TestDepartureArrivalConsolForDocuments_DRTShipmentWithPreCarriageOnForwardingAGTConsols()
		{
			var (shipment, departureConsol, arrivalConsol) = SetUpDRTShipmentAttachedToPreCarriageOnForwardingAGTConsols(Constants.TransportModes.Sea);
			AssertEquals(departureConsol, shipment.DepartureConsolForDocuments);
			AssertEquals(arrivalConsol, shipment.ArrivalConsolForDocuments);
		}

		public void TestMostInterestingDepartureConsol_DRTShipmentWithPreCarriageOnForwardingAGTConsols()
		{
			var (shipment, departureConsol, _) = SetUpDRTShipmentAttachedToPreCarriageOnForwardingAGTConsols(Constants.TransportModes.Sea);
			AssertEquals(departureConsol, shipment.MostInterestingDepartureConsol);
		}

		public void TestCurrentBranchDepartureAirConsol_DRTShipmentWithPreCarriageOnForwardingAGTConsols()
		{
			var (shipment, directConsolFromUS, directConsolFromSG) = SetUpDRTShipmentAttachedToPreCarriageOnForwardingAGTConsols(Constants.TransportModes.Air);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				AssertEquals(directConsolFromUS, shipment.CurrentBranchDepartureAirConsol);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Singapore))
			{
				AssertEquals(directConsolFromSG, shipment.CurrentBranchDepartureAirConsol);
			}
		}

		(CommonShipment, CommonConsol, CommonConsol) SetUpDRTShipmentAttachedToPreCarriageOnForwardingAGTConsols(ZString transportMode)
		{
			var today = ZDateTime.Today;
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_UniqueConsignRef = "DRT shipment";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var agentPreCarriageConsol = FreightTestHelper.GetConsol<CommonConsol>("AGT Pre-carriage", transportMode, "AGT", "USCHI", "USLAX", today, today.AddDays(1), Factory, shipment);
			agentPreCarriageConsol.JK_MasterBillNum = "1111";
			var directDepartureConsol = FreightTestHelper.GetConsol<CommonConsol>("DRT Departure", transportMode, "DRT", "USLAX", "SGSIN", today.AddDays(2), today.AddDays(3), Factory, shipment);
			directDepartureConsol.JK_MasterBillNum = "2222";
			var directArrivalConsol = FreightTestHelper.GetConsol<CommonConsol>("DRT Arrival", transportMode, "DRT", "SGSIN", "AUSYD", today.AddDays(4), today.AddDays(5), Factory, shipment);
			directArrivalConsol.JK_MasterBillNum = "3333";
			var agentOnForwardingConsol = FreightTestHelper.GetConsol<CommonConsol>("AGT On-forwarding", transportMode, "AGT", "AUSYD", "AUMEL", today.AddDays(6), today.AddDays(7), Factory, shipment);
			agentOnForwardingConsol.JK_MasterBillNum = "4444";

			Factory.Save();
			return (shipment, directDepartureConsol, directArrivalConsol);
		}

		#endregion

		#region TestContainersToSelectFrom
		public void TestContainersToSelectFrom()
		{
			CommonShipment shipment = GetShipment();
			PackLine pack1 = shipment.OuterPackLines.AddNew();
			PackLine pack2 = shipment.OuterPackLines.AddNew();

			CommonConsol consol = shipment.Consols.AddNew();
			CommonContainer cont1 = consol.Containers.AddNew();
			CommonContainer cont2 = consol.Containers.AddNew();
			CommonContainer cont3 = consol.Containers.AddNew();

			cont1.PackLines.Add(pack1);
			cont3.PackLines.Add(pack2);

			ContainerToSelectFromForPrintingCollection containers = ((CommonShipmentDocumentSupporter)((IDocumentSupportable)shipment).DocumentSupporter).GetContainersToSelectFrom(true);
			AssertEquals("Containers To select from count", 2, containers.Count);
			AssertEquals(containers[0].Container.PK, cont1.PK);
			AssertEquals(containers[1].Container.PK, cont3.PK);
		}
		#endregion

		#region TestContainersToSelectFromForIMO
		public void TestContainersToSelectFromForIMO()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "NZAKL";
			PackLine pack1 = shipment.OuterPackLines.AddNew();
			PackLine pack2 = shipment.OuterPackLines.AddNew();

			CommonConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "SGSIN";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "NZAKL";

			CommonContainer cont1 = consol1.Containers.AddNew();
			CommonContainer cont2 = consol1.Containers.AddNew();
			CommonContainer cont3 = consol2.Containers.AddNew();
			CommonContainer cont4 = consol2.Containers.AddNew();

			cont1.PackLines.Add(pack1);
			cont2.PackLines.Add(pack2);
			cont3.PackLines.Add(pack1);
			cont4.PackLines.Add(pack2);

			ContainerToSelectFromForPrintingCollection containers = ((CommonShipmentDocumentSupporter)((IDocumentSupportable)shipment).DocumentSupporter).GetContainersToSelectFrom(false);
			AssertEquals("Containers To select from count", 0, containers.Count);

			pack1.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			pack2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;

			containers = ((CommonShipmentDocumentSupporter)((IDocumentSupportable)shipment).DocumentSupporter).GetContainersToSelectFrom(false);
			AssertEquals("Containers To select from count", 2, containers.Count);
			AssertEquals(containers[0].Container.PK, cont1.PK);
			AssertEquals(containers[1].Container.PK, cont2.PK);
		}
		#endregion

		#region TestDebtors

		public void TestDebtors()
		{
			CommonShipment shipment = GetShipment();
			AssertEquals("Should be empty", 0, shipment.Debtors.Count);

			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;

			var code = Factory.NewWithValidTestData<AccChargeCode>();

			JobCharge lineCharge1 = CreateLineCharge(header, header.LocalChargesPK, 10.000M, code.PK);
			JobCharge lineCharge2 = CreateLineCharge(header, header.LocalChargesPK, 30.000M, code.PK);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, header.LocalChargesPK));
			JobCharge lineCharge3 = CreateLineCharge(header, orgHeader.PK, 20.000M, code.PK);
			lineCharge3.JR_OH_SellAccount = orgHeader.PK;

			JobCharge lineCharge4 = CreateLineCharge(header, ZGuid.Empty, 40.000M, code.PK);
			AssertNull("Sell Account should be null", lineCharge4.SellAccount);

			AssertNotNull(shipment.Debtors);
			AssertEquals("Should contain 2 Debtors", 2, shipment.Debtors.Count);
		}

		JobCharge CreateLineCharge(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK)
		{
			JobCharge lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = jobHeaderBisObj.PK;
			lineCharge.JR_GE = jobHeaderBisObj.JH_GE;
			lineCharge.JR_GB = jobHeaderBisObj.JH_GB;
			lineCharge.JR_AC = chargeCodePK;
			lineCharge.JR_LocalSellAmt = amount;
			lineCharge.JR_OH_SellAccount = localChargesPK;
			return lineCharge;
		}

		#endregion

		#region TestCartageAdvicePrintedForPrintForExportShipment

		public void TestCartageAdvicePrintedForPrintForExportShipment()
		{
			CommonShipmentForTest shipment = Factory.NewWithValidTestData<CommonShipmentForTest>();
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKOrigin = "AUSYD";

			DocumentZQuery filter = new DocumentZQuery("Customs", "Pre-Alert");
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);

			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is not filled in", shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);

			filter = new DocumentZQuery("Shipment", "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.DEP));
			menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is not filled in", shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);

			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is filled in", !shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment.Consols.Add(consol);
			CommonContainer container1 = consol.Containers.AddNew();

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeight = 20.2m;
			packLine.JL_ActualVolume = 2.2m;
			packLine.JL_JS = shipment.PK;
			container1.PackLines.Add(packLine);
			Factory.Save();

			shipment.DocsAndCartage.JP_PickupCartageAdvised = ZDateTime.Empty;

			Assert("Local Transport Advised time is not filled in", shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);

			shipment.SelectedContainersToPrintForTest.Add(container1);
			filter = new DocumentZQuery("Shipment", "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.DEP));
			menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is not filled in", shipment.Containers.First().JC_DepartureCartageAdvised.IsEmpty);
			Assert("Local Transport Advised time is not filled in", shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);

			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is not filled in", !shipment.Containers.First().JC_DepartureCartageAdvised.IsEmpty);
			Assert("Local Transport Advised time is filled in", !shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);
			AssertEquals(1, shipment.SelectedContainersToPrintForTest.Count);
		}

		#endregion

		#region TestCartageAdvicePrintedForEmailFaxForExportShipment
		public void TestCartageAdvicePrintedForEmailFaxForExportShipment()
		{
			CommonShipmentForTest shipment = Factory.New<CommonShipmentForTest>();
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKOrigin = "AUSYD";

			DocumentZQuery filter = new DocumentZQuery("Shipment", "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.DEP));
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is filled in", !shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);
		}
		#endregion

		#region TestCartageAdvicePrintedForPrintForImportShipment
		public void TestCartageAdvicePrintedForPrintForImportShipment()
		{
			CommonShipmentForTest shipment = Factory.NewWithValidTestData<CommonShipmentForTest>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "SGSIN";

			DocumentZQuery filter = new DocumentZQuery("Customs", "Pre-Alert");
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);

			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is not filled in", shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);

			filter = new DocumentZQuery("Shipment", "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.ARV));
			menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is not filled in", shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);

			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is filled in", !shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment.Consols.Add(consol);
			CommonContainer container1 = consol.Containers.AddNew();

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeight = 20.2m;
			packLine.JL_ActualVolume = 2.2m;
			packLine.JL_JS = shipment.PK;
			container1.PackLines.Add(packLine);
			Factory.Save();

			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Empty;

			Assert("Local Transport Advised time is not filled in", shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);

			shipment.SelectedContainersToPrintForTest.Add(container1);
			filter = new DocumentZQuery("Shipment", "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.ARV));
			menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is not filled in", shipment.Containers.First().JC_ArrivalCartageAdvised.IsEmpty);
			Assert("Local Transport Advised time is not filled in", shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);

			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is not filled in", !shipment.Containers.First().JC_ArrivalCartageAdvised.IsEmpty);
			Assert("Local Transport Advised time is filled in", !shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);
			AssertEquals(1, shipment.SelectedContainersToPrintForTest.Count);
		}
		#endregion

		#region TestCartageAdvicePrintedForEmailForImportShipment
		public void TestCartageAdvicePrintedForEmailForImportShipment()
		{
			CommonShipmentForTest shipment = Factory.New<CommonShipmentForTest>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "SGSIN";

			DocumentZQuery filter = new DocumentZQuery("Shipment", "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.ARV));
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Local Transport Advised time is filled in", !shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);
		}
		#endregion

		#region TestCartageAdvicePrintedForFaxForImportShipment
		public void TestCartageAdvicePrintedForFaxForImportShipment()
		{
			CommonShipmentForTest shipment = Factory.New<CommonShipmentForTest>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "SGSIN";

			DocumentZQuery filter = new DocumentZQuery("Shipment", "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.ARV));
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Cartage Advised time is filled in", !shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);
		}
		#endregion

		#region TestCartageAdvicePrintedForPrintForDomesticShipment

		public void TestCartageAdvicePrintedForPrintForDomesticShipment()
		{
			CommonShipmentForTest shipment = Factory.New<CommonShipmentForTest>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = AlternateHomePort;

			DocumentZQuery filter = new DocumentZQuery("Shipment", "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.DEP));
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Cartage Advised time is filled in for pickup", !shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);
			Assert("Cartage Advised time is filled in for pickup", shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);

			shipment.DocsAndCartage.JP_PickupCartageAdvised = ZDateTime.Empty;

			filter = new DocumentZQuery("Shipment", "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.ARV));
			menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
			shipment.DocumentPrintedEvent(this, eventArgs);
			Assert("Cartage Advised time is filled in for delivery", !shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty);
			Assert("Cartage Advised time is filled in for delivery", shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty);
		}

		#endregion

		#region Test HAWB & HouseBill Printed

		[TestDate(2017, 7, 1)]
		public void TestHouseBillNotPrintedIfIsDraft()
		{
			var shipment = Factory.New<CommonShipmentForTest>();
			Factory.Save();
			AssertEquals("Precon: JS_HouseBillIssueDate is not set", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			var filter = new DocumentZQuery(CargoWise.Definitions.BusinessContext.Shipment, CommonShipmentDocumentSupporter.DocumentNames.BillOfLading);

			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			var events = new DocumentEventsForTest();
			var supporter = new CommonShipmentDocumentSupporter(shipment);
			supporter.Initialise(events);
			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, true));
			AssertEquals("JS_HouseBillIssueDate is not set", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false));
			AssertEquals("JS_HouseBillIssueDate is set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);
		}

		[TestDate(2017, 7, 1)]
		public void TestHouseBillPrinted()
		{
			var shipment = Factory.New<CommonShipmentForTest>();
			Factory.Save();
			AssertEquals("Precon: JS_HouseBillIssueDate is not set", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			var filter = new DocumentZQuery(CargoWise.Definitions.BusinessContext.Shipment, CommonShipmentDocumentSupporter.DocumentNames.BillOfLading);

			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			var events = new DocumentEventsForTest();
			var supporter = new CommonShipmentDocumentSupporter(shipment);
			supporter.Initialise(events);
			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem));

			AssertEquals("JS_HouseBillIssueDate is set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);
		}

		[TestDate(2017, 7, 1)]
		public void TestHouseBillEmailed_Faxed()
		{
			var shipment = Factory.New<CommonShipmentForTest>();
			Factory.Save();
			AssertEquals("Precon: JS_HouseBillIssueDate is not set", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			var filter = new DocumentZQuery(CargoWise.Definitions.BusinessContext.Shipment, CommonShipmentDocumentSupporter.DocumentNames.BillOfLading);

			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			var events = new DocumentEventsForTest();
			var supporter = new CommonShipmentDocumentSupporter(shipment);
			supporter.Initialise(events);
			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem));

			AssertEquals("JS_HouseBillIssueDate is set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);
		}

		[TestDate(2017, 7, 1)]
		public void TestHouseBillPrinted_DoesNotOverride()
		{
			var shipment = Factory.New<CommonShipmentForTest>();
			Factory.Save();
			AssertEquals("Precon: JS_HouseBillIssueDate is not set", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			var filter = new DocumentZQuery(CargoWise.Definitions.BusinessContext.Shipment, CommonShipmentDocumentSupporter.DocumentNames.BillOfLading);
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			var events = new DocumentEventsForTest();
			var supporter = new CommonShipmentDocumentSupporter(shipment);
			supporter.Initialise(events);
			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem));

			AssertEquals("JS_HouseBillIssueDate is set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);

			shipment.JS_HouseBillIssueDate = ZDateTime.Today.AddDays(5);
			AssertEquals("JS_HouseBillIssueDate is set to new value", ZDateTime.Today.AddDays(5), shipment.JS_HouseBillIssueDate);

			events.FireDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem));

			AssertEquals("JS_HouseBillIssueDate should not be overriden", ZDateTime.Today.AddDays(5), shipment.JS_HouseBillIssueDate);
		}

		#endregion

		#region TestGetDocumentDataStateMessageForDocumentRequiringMerge
		public void TestGetDocumentDataStateMessageForDocumentRequiringMergeNZ()
		{
			CommonShipment shipment = GetShipment();
			shipment.FillWithValidTestData();

			Assert(((CommonShipmentDocumentSupporter)shipment.DocumentSupporter).GetDocumentDataStateForDocumentFromDeclaration(GetMenuItem("Entry Print")).ErrorMessage.Contains("can not be generated until after the Declaration is made."));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			BusinessObject declaration = (BusinessObject)Factory.New<NZ.IJobDeclaration>();
			declaration.FillWithValidTestData();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_MessageSubType] = "FML";

			shipment.Factory.Save();

			AssertContains("NZ Declaration Merge Error", Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, ((CommonShipmentDocumentSupporter)shipment.DocumentSupporter).GetDocumentDataStateForDocumentFromDeclaration(GetMenuItem("Entry Print")).ErrorMessage);
		}

		#endregion

		#region TestGetDocumentDataStateMessageForDocumentRequiringMergeWith2DecsAgainstShipment
		public void TestGetDocumentDataStateMessageForDocumentRequiringMergeWith2DecsAgainstShipment()
		{
			CommonShipment shipmentDS = Factory.New<CommonShipment>();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			GlbCompany companyInAU = newFactory.New<GlbCompany>();
			companyInAU.FillWithValidTestData();
			companyInAU.GC_RN_NKCountryCode = "AU";
			GlbBranch branchInAU = newFactory.New<GlbBranch>();
			branchInAU.FillWithValidTestData();
			branchInAU.GB_RL_NKHomePort = "AUSYD";
			branchInAU.GB_GC = companyInAU.PK;

			BusinessObject aUDeclaration = (BusinessObject)newFactory.New<AU.IJobDeclaration>();
			aUDeclaration[JobDeclarationSchema.Constants.JE_JS] = shipmentDS.PK;
			aUDeclaration[JobDeclarationSchema.Constants.JE_GB] = branchInAU.PK;
			aUDeclaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";

			BusinessObject aUGroupHeader = (BusinessObject)newFactory.New<AU.IJobComInvoiceGroupHeader>();
			aUGroupHeader[JobComInvoiceHeaderSchema.JZ_GroupInvoice.Name] = true;
			aUGroupHeader[JobComInvoiceHeaderSchema.JZ_GB.Name] = branchInAU.PK;
			aUGroupHeader[JobComInvoiceHeaderSchema.JZ_JE.Name] = aUDeclaration.PK;

			BusinessObject aUInvoiceHeader = (BusinessObject)newFactory.New<AU.IJobComInvoiceHeader>();
			aUInvoiceHeader[JobComInvoiceHeaderSchema.JZ_GroupInvoice.Name] = false;
			aUInvoiceHeader[JobComInvoiceHeaderSchema.JZ_GB.Name] = branchInAU.PK;
			aUInvoiceHeader[JobComInvoiceHeaderSchema.JZ_JE.Name] = aUDeclaration.PK;
			aUInvoiceHeader[JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK.Name] = aUGroupHeader.PK;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			GlbBranch branchInNZ = newFactory.New<GlbBranch>();
			branchInNZ.FillWithValidTestData();
			branchInNZ.GB_RL_NKHomePort = "NZAKL";
			branchInNZ.GB_GC = GlbCompany.CurrentCompany.PK;

			BusinessObject nZDeclaration = (BusinessObject)newFactory.New<NZ.IJobDeclaration>();
			nZDeclaration[JobDeclarationSchema.Constants.JE_JS] = shipmentDS.PK;
			nZDeclaration[JobDeclarationSchema.Constants.JE_GB] = branchInNZ.PK;
			nZDeclaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
			nZDeclaration[JobDeclarationSchema.Constants.JE_MessageSubType] = "FML";
			newFactory.Save();

			AssertContains("NZ Declaration Merge Error", Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, ((CommonShipmentDocumentSupporter)shipmentDS.DocumentSupporter).GetDocumentDataStateForDocumentFromDeclaration(GetMenuItem("Entry Print")).ErrorMessage);
		}
		#endregion

		#region TestOrganisationsForCreditChecks

		public void TestOrganisationsForCreditChecks()
		{
			var shipment = Factory.New<CommonShipment>();
			var creditControlled = shipment as ICreditControlledDocumentDelivery;
			var job = new JobHeader.Loader(shipment).TryCreate();
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignor));
			var consignee = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignee, consignor.PK));
			var chargeDebtor = Factory.NewWithValidTestData<OrgHeader>();

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			job.LocalChargesPK = localClient.PK;
			jobCharge.JR_OH_SellAccount = chargeDebtor.PK;

			var orgCreditControlCollection = AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var orgCreditControlJobChargeDefault = orgCreditControlCollection.Cast<OrgsEvaluatedForCreditControl>().FirstOrDefault(x =>
				x.JobType == JobInvoicingConsumerTypes.Shipment.Code &&
				x.DirectionCode == Constants.FreightShipmentDirection.Code.All &&
				x.Mode == Constants.TransportModes.All &&
				x.INCOTerm == AccountingMasterFilesConstants.INCOTermCodes.All &&
				x.FreightPaymentTerm == AccountingMasterFilesConstants.FreightPaymentTermCodes.All &&
				x.OrganizationType == OrgCodes.AllDebtors);
			AssertNotNull("AllDebtors should be disabled by default", orgCreditControlJobChargeDefault);

			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(3, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignor));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(localClient));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			var orgCreditControlConsignor = orgCreditControlCollection.AddNew();
			orgCreditControlConsignor.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			orgCreditControlConsignor.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			orgCreditControlConsignor.Mode = Constants.TransportModes.All;
			orgCreditControlConsignor.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			orgCreditControlConsignor.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
			orgCreditControlConsignor.OrganizationType = OrgCodes.Consignor;
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);

			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(2, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(localClient));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			var orgCreditControlConsignee = orgCreditControlCollection.AddNew();
			orgCreditControlConsignee.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			orgCreditControlConsignee.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			orgCreditControlConsignee.Mode = Constants.TransportModes.All;
			orgCreditControlConsignee.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			orgCreditControlConsignee.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
			orgCreditControlConsignee.OrganizationType = OrgCodes.Consignee;
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);

			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(1, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(localClient));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			var orgCreditControlLocalClient = orgCreditControlCollection.AddNew();
			orgCreditControlLocalClient.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			orgCreditControlLocalClient.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			orgCreditControlLocalClient.Mode = Constants.TransportModes.All;
			orgCreditControlLocalClient.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			orgCreditControlLocalClient.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
			orgCreditControlLocalClient.OrganizationType = OrgCodes.LocalClient;
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);

			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(0, creditControlled.OrganisationsForCreditChecks.Length);
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			orgCreditControlCollection.RemoveAndDelete(orgCreditControlJobChargeDefault);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);

			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(1, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(chargeDebtor));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		public void TestOrganisationsForCreditChecks_ControllingCustomersAndControllingAgents()
		{
			base.SetUp();

			var controllingAgent = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var controllingCustomer = Factory.LoadTop1<OrgHeader>(new ZQuery().AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, controllingAgent.PK));
			var shipment = Factory.New<CommonShipment>();
			var creditControlled = shipment as ICreditControlledDocumentDelivery;
			shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.Addresses.AddNew().PK;
			shipment.ControllingAgentDocumentaryAddress.E2_OA_Address = controllingAgent.Addresses.AddNew().PK;

			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(0, creditControlled.OrganisationsForCreditChecks.Length);

			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(2, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(controllingAgent));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(controllingCustomer));

			var orgCreditControlCollection = AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			var orgCreditControl = orgCreditControlCollection.AddNew();
			orgCreditControl.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			orgCreditControl.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			orgCreditControl.Mode = Constants.TransportModes.All;
			orgCreditControl.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			orgCreditControl.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
			orgCreditControl.OrganizationType = OrgCodes.ControllingCustomer;
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);

			AssertEquals(1, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(controllingAgent));
			Assert(!creditControlled.OrganisationsForCreditChecks.Contains(controllingCustomer));
		}

		public void TestOrganisationsForCreditChecksCachedRegistryIsClearedOnFactorySave()
		{
			var shipment = Factory.New<CommonShipment>();
			var creditControlled = shipment as ICreditControlledDocumentDelivery;
			var job = new JobHeader.Loader(shipment).TryCreate();
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignor));
			var consignee = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignee, consignor.PK));
			var chargeDebtor = Factory.NewWithValidTestData<OrgHeader>();

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			job.LocalChargesPK = localClient.PK;
			jobCharge.JR_OH_SellAccount = chargeDebtor.PK;

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(3, creditControlled.OrganisationsForCreditChecks.Length);
			AssertEquals("OrganizationsEvaluatedForCreditControl registry should be accessed once, and then cached in factory, as reading the default value has poor performance.", 1, AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly);

			AssertEquals(3, creditControlled.OrganisationsForCreditChecks.Length);
			AssertEquals("OrganizationsEvaluatedForCreditControl registry should be cached in factory.", 1, AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly);

			Factory.Save();

			AssertEquals(3, creditControlled.OrganisationsForCreditChecks.Length);
			AssertEquals("OrganizationsEvaluatedForCreditControl registry cache should be invalidated after factory save.", 2, AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly);

			CombineAssertions("Precondition: Cached value is in use, so should contain all three Orgs", () =>
			{
				Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignor));
				Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee));
				Assert(creditControlled.OrganisationsForCreditChecks.Contains(localClient));
			});

			var orgCreditControlCollectionFromRegistry = AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			var orgCreditControlConsignor = orgCreditControlCollectionFromRegistry.AddNew();
			orgCreditControlConsignor.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			orgCreditControlConsignor.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			orgCreditControlConsignor.Mode = Constants.TransportModes.All;
			orgCreditControlConsignor.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			orgCreditControlConsignor.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
			orgCreditControlConsignor.OrganizationType = OrgCodes.Consignor;
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, orgCreditControlCollectionFromRegistry);

			CombineAssertions("Cached value remains in use, so should contain all three Orgs", () =>
			{
				Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignor));
				Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee));
				Assert(creditControlled.OrganisationsForCreditChecks.Contains(localClient));
			});

			Factory.Save();

			CombineAssertions("Registry value should be re-loaded after Factory.Save(), so should no longer contain Consignor", () =>
			{
				Assert(!creditControlled.OrganisationsForCreditChecks.Contains(consignor));
				Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee));
				Assert(creditControlled.OrganisationsForCreditChecks.Contains(localClient));
			});
		}

		public void TestOrganisationsForCreditChecksCachedRegistryIsPerCompany()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var orgCreditControlCollectionCurrentCompany = AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.GetFallBackValueAtAllLevels(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var orgCreditControlConsignor = orgCreditControlCollectionCurrentCompany.AddNew();
			orgCreditControlConsignor.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			orgCreditControlConsignor.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			orgCreditControlConsignor.Mode = Constants.TransportModes.All;
			orgCreditControlConsignor.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			orgCreditControlConsignor.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
			orgCreditControlConsignor.OrganizationType = OrgCodes.Consignor;
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollectionCurrentCompany);

			var otherCompany = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, currentCompany.PK)).First(gc => gc.HasActiveBranch && gc.GC_IsActive);
			var orgCreditControlCollectionOtherCompany = AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.GetFallBackValueAtAllLevels(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var orgCreditControlConsignee = orgCreditControlCollectionOtherCompany.AddNew();
			orgCreditControlConsignee.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			orgCreditControlConsignee.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			orgCreditControlConsignee.Mode = Constants.TransportModes.All;
			orgCreditControlConsignee.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			orgCreditControlConsignee.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
			orgCreditControlConsignee.OrganizationType = OrgCodes.Consignee;
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollectionOtherCompany);

			var shipment = Factory.New<CommonShipment>();
			var creditControlled = shipment as ICreditControlledDocumentDelivery;
			var job = new JobHeader.Loader(shipment).TryCreate();
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;

			var consignor = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignor));
			var consignee = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignee, consignor.PK));
			var chargeDebtor = Factory.NewWithValidTestData<OrgHeader>();

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			jobCharge.JR_OH_SellAccount = chargeDebtor.PK;

			Factory.Save();

			AssertEquals(1, creditControlled.OrganisationsForCreditChecks.Length);
			Assert("Current company should contain Consignee but not Consignor", creditControlled.OrganisationsForCreditChecks.Contains(consignee));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals(1, creditControlled.OrganisationsForCreditChecks.Length);
				Assert("Other company should contain Consignor but not Consignee", creditControlled.OrganisationsForCreditChecks.Contains(consignor));
			}
		}

		#endregion

		#region GetDataStateBeforeRunTests

		public void TestGetDataStateBeforeRunForLandedCosting()
		{
			var shipment = Factory.New<CommonShipment>();
			var jobDeclaration = Factory.New<IBaseJobDeclaration>();
			jobDeclaration.JE_JS = shipment.PK;
			jobDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;

			var commandFilter = new DocumentZQuery("Shipment", "Landed Costing");
			var command = Factory.LoadTop1<DocumentCommand>(commandFilter);

			DocumentSupporterDataState result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Not valid as there is no LC attached", false, result.IsValid);

			BusinessObject landedCostHeader = (BusinessObject)Factory.New<LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = jobDeclaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = "JE";

			result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Invalid as there are no LC lines", false, result.IsValid);
		}

		public void TestGetDataStateBeforeRunForBillOfLading()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			((ICreditControlledDocumentDelivery)shipment).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(Shipment_OnGetDocumentLogin);

			DocumentZQuery commandFilter = new DocumentZQuery("Shipment", "Bill of Lading");
			var command = Factory.LoadTop1<DocumentCommand>(commandFilter);

			DocumentSupporterDataState result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			Assert("State is valid", result.IsValid);

			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "SEA";
			PackLine shipPackLine1 = shipment.OuterPackLines.AddNew();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container 1";
			container1.JC_SealNum = "Seal Num";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.JC_DeliveryMode = "CY/CY";
			shipPackLine1.SetContainer(consol, container1);
			result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			Assert("State is invalid as the Container does not have a type", !result.IsValid);

			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20NOR"));
			container1.JC_RC = containerCode1.PK;
			result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			Assert("State is valid", result.IsValid);

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "Code";
			consignor.OH_FullName = "Consignor";
			consignor.MainAddress.OA_Address1 = "Address 1";
			consignor.OH_IsDebtor = true;
			consignor.MiscServ.OM_AROnCreditHold = ZBool.True;
			shipment.ConsignorPK = consignor.PK;
			Factory.Save();
			shipment.Consignor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			Assert("State is invalid as e.ContinueToPrint is false", !result.IsValid);

			result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			Assert("State is valid as e.ContinueToPrint is true", result.IsValid);

			ZQuery filter = new ZQuery(StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, shipment.PK);
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.HoldStatusOverride.Code);
			var log = Factory.LoadTop1<StmALog>(filter);
			AssertNotNull("Hold Status Override event should be saved", log);
		}

		public void TestGetDataStateBeforeRunForElectronicBL()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			DocumentZQuery commandFilter = new DocumentZQuery("Shipment", "Send Electronic Original Bill of Lading");
			var command = Factory.LoadTop1<DocumentCommand>(commandFilter);

			DocumentSupporterDataState result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("No Consignor is set up for the Shipment", ZBool.False, result.IsValid);

			OrgHeader cnor = OrgHeader.New(Factory);
			cnor.MiscServ.OM_EXAllowedToPrintOriginalBL = ZBool.False;
			shipment.ConsignorPK = cnor.PK;

			result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("State is false as Consignor is not allowed to print Bill", ZBool.False, result.IsValid);

			ZString errorMsg = "This Consignor is not approved to print the Original Bill of Lading.";
			errorMsg += "\n";
			errorMsg += "You can modify this in the Consignor tab of the Organization form.";
			AssertEquals("Error msg lets the user know Cnor is not approved to be sent Original Bills", errorMsg, result.ErrorMessage);

			cnor.MiscServ.OM_EXAllowedToPrintOriginalBL = ZBool.True;
			result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("State is still false as Consignor doesnt have the contact set up to receive the doc", ZBool.False, result.IsValid);

			errorMsg = "This Consignor does not have a Contact set up to receive Electronic Bill of Lading.";
			errorMsg += "\n";
			errorMsg += "Please set the selected Consignor's contact to include the 'Send Electronic Original Bill of Lading' document menu in Organizations / Contacts / Documents To Receive.";
			AssertEquals("Error msg lets the user know Cnor doesnt have the right contacts set up for this doc", errorMsg, result.ErrorMessage);

			OrgContact cnorContact = cnor.Contacts.AddNew();
			OrgDocument docToReceive = cnorContact.Documents.AddNew();
			docToReceive.OD_SU_MenuItem = command.PK;

			result = shipment.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("State is true as Consignor is allowed to print Bill and contact for the doc is properly set up", ZBool.True, result.IsValid);
		}

		#endregion

		#region Weight / Volume / Chargeable / Loading Meters

		public void TestUpdateShipmentFromOuterPackLines()
		{
			AssertEquals("precondition", Constants.Weight.Kilograms, Env.Registry.FreightWeightUnit);
			AssertEquals("precondition", Constants.Volume.CubicMetres, Env.Registry.FreightVolumeUnit);

			CommonShipment shipment = GetShipment();
			shipment.JS_OuterPacks = 0;
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Bag;
			shipment.JS_ActualWeight = 0M;
			shipment.JS_ActualVolume = 0M;
			shipment.JS_UnitOfWeight = Constants.Weight.LongTons;
			shipment.JS_UnitOfVolume = Constants.Volume.TeaChest;
			shipment.JS_LoadingMeters = 1m;

			shipment.OuterPackLines.RemoveAndDeleteAll();
			AssertEquals("Precondition: no outer packlines, one auto-added line is removed", 0, shipment.OuterPackLines.Count);

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = Constants.PkgUnit.Box;
			packLine1.JL_ActualWeight = 10;
			packLine1.JL_ActualVolume = 1;
			packLine1.JL_ActualWeightUQ = Constants.Weight.LongTons;
			packLine1.JL_ActualVolumeUQ = Constants.Volume.TeaChest;
			packLine1.JL_LoadingMeters = 1.024m;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = Constants.PkgUnit.Box;
			packLine2.JL_ActualWeight = 20;
			packLine2.JL_ActualVolume = 2;
			packLine2.JL_ActualWeightUQ = Constants.Weight.LongTons;
			packLine2.JL_ActualVolumeUQ = Constants.Volume.TeaChest;
			packLine2.JL_LoadingMeters = 2.048m;

			shipment.UpdateShipmentFromOuterPackLines();
			AssertEquals("Packs: Should now be 3", 3, shipment.JS_OuterPacks);
			AssertEquals("Package type updated from packlines", Constants.PkgUnit.Box, shipment.JS_F3_NKPackType);
			AssertEquals("Weight updated from packlines", 30m, shipment.JS_ActualWeight);
			AssertEquals("Volume updated from packlines", 3m, shipment.JS_ActualVolume);
			AssertEquals("Loading meters updated from packlines", 3.072m, shipment.JS_LoadingMeters);
		}

		public void TestUpdateActualChargeableFromOuterPackLines()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var defaultNumberOfDecimals_AWBWeight = collection.AddNew();
			defaultNumberOfDecimals_AWBWeight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_AWBWeight.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_AWBWeight.NumberOfDecimals = 3;
			defaultNumberOfDecimals_AWBWeight.RoundingMode = RoundingModes.Up;

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<CommonShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = LocalConsignor.PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = OverseasConsignee.PK;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

				shipment.JS_ActualVolume = 0.009m;
				AssertEquals("Precondition", 1.5m, shipment.JS_ActualChargeable);

				var packLine1 = shipment.OuterPackLines.Count == 0 ? shipment.OuterPackLines.AddNew() : shipment.OuterPackLines[0];
				packLine1.JL_PackageCount = 1;
				packLine1.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
				packLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
				packLine1.JL_Length = 10m;
				packLine1.JL_Width = 11m;
				packLine1.JL_Height = 12m;
				packLine1.JL_UnitOfDimension = Constants.Length.Centimetres;
				packLine1.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = 1;
				packLine2.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
				packLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
				packLine2.JL_Length = 13m;
				packLine2.JL_Width = 14m;
				packLine2.JL_Height = 15m;
				packLine2.JL_UnitOfDimension = Constants.Length.Centimetres;
				packLine2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

				var packLine3 = shipment.OuterPackLines.AddNew();
				packLine3.JL_PackageCount = 1;
				packLine3.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
				packLine3.JL_ActualWeightUQ = Constants.Weight.Kilograms;
				packLine3.JL_Length = 16m;
				packLine3.JL_Width = 17m;
				packLine3.JL_Height = 18m;
				packLine3.JL_UnitOfDimension = Constants.Length.Centimetres;
				packLine3.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

				var packLine4 = shipment.OuterPackLines.AddNew();
				packLine4.JL_PackageCount = 2;
				packLine4.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
				packLine4.JL_ActualWeightUQ = Constants.Weight.Kilograms;
				packLine4.JL_Length = 16m;
				packLine4.JL_Width = 17m;
				packLine4.JL_Height = 18m;
				packLine4.JL_UnitOfDimension = Constants.Length.Centimetres;
				packLine4.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
				packLine4.JL_ActualVolume = 0.005;

				shipment.UpdateShipmentFromOuterPackLines();
				AssertEquals(0.015m, shipment.JS_ActualVolume);
				AssertEquals(2.5m, shipment.JS_ActualChargeable);

				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				var shipmentInNewFactory = newFactory.Load<CommonShipment>(shipment.PK);

				shipmentInNewFactory.UpdateShipmentFromOuterPackLines();
				AssertEquals(0.015m, shipment.JS_ActualVolume);
				AssertEquals(2.5m, shipmentInNewFactory.JS_ActualChargeable);
			}
		}

		public void TestGetWeightForDoc()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_ActualWeight = 10M;
			shipment.JS_ManifestedWeight = 9M;
			shipment.JS_DocumentedWeight = 8M;

			AssertEquals(10M, shipment.GetWeightForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual));
			AssertEquals(10M, shipment.GetWeightWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual).Item1);

			AssertEquals(9M, shipment.GetWeightForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier));
			AssertEquals(9M, shipment.GetWeightWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);

			AssertEquals(8M, shipment.GetWeightForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client));
			AssertEquals(8M, shipment.GetWeightWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client).Item1);
		}

		public void TestGetWeightWithScaleForDoc()
		{
			CommonShipment shipment = GetShipment();

			AssertEquals(JobShipmentSchema.JS_ActualWeight.Scale, shipment.GetWeightWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual).Item2);

			AssertEquals(JobShipmentSchema.JS_ManifestedWeight.Scale, shipment.GetWeightWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier).Item2);

			AssertEquals(JobShipmentSchema.JS_DocumentedWeight.Scale, shipment.GetWeightWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client).Item2);
		}

		public void TestRoundingRoundDownDecimalToItsPrecisionAndScaleOnDataImport()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var registryEntryWeight = new DefaultNumberOfDecimals();
			registryEntryWeight.UnitOfMeasure = Constants.Weight.Kilograms;
			registryEntryWeight.TransportMode = Constants.TransportModes.Air;
			registryEntryWeight.NumberOfDecimals = 1;
			registryEntryWeight.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryWeight);

			var registryEntryVolume = new DefaultNumberOfDecimals();
			registryEntryVolume.UnitOfMeasure = Constants.Volume.CubicMetres;
			registryEntryVolume.TransportMode = Constants.TransportModes.Air;
			registryEntryVolume.NumberOfDecimals = 1;
			registryEntryVolume.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryVolume);

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_UniqueConsignRef = "Shipment1001";
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			using (shipment.GetValidationSuspender())
			{
				shipment.JS_ActualWeight = 999999.999m;
				shipment.JS_ActualVolume = 999999.999m;
				shipment.JS_ActualChargeable = 999999.999m;
				shipment.JS_DocumentedWeight = 999999.999m;
				shipment.JS_DocumentedVolume = 999999.999m;
				shipment.JS_DocumentedChargeable = 999999.999m;
				shipment.JS_ManifestedWeight = 999999.999m;
				shipment.JS_ManifestedVolume = 999999.999m;
				shipment.JS_ManifestedChargeable = 999999.999m;
			}

			AssertEquals((ZDecimal)999999.9, shipment.JS_ActualWeight);
			AssertEquals((ZDecimal)999999.9, shipment.JS_ActualVolume);
			AssertEquals((ZDecimal)999999.9, shipment.JS_ActualChargeable);
			AssertEquals((ZDecimal)999999.9, shipment.JS_DocumentedWeight);
			AssertEquals((ZDecimal)999999.9, shipment.JS_DocumentedVolume);
			AssertEquals((ZDecimal)999999.9, shipment.JS_DocumentedChargeable);
			AssertEquals((ZDecimal)999999.9, shipment.JS_ManifestedWeight);
			AssertEquals((ZDecimal)999999.9, shipment.JS_ManifestedVolume);
			AssertEquals((ZDecimal)999999.9, shipment.JS_ManifestedChargeable);

			shipment.JS_UnitOfWeight = Constants.Weight.Milligrams;
			AssertEquals((ZDecimal)999999.9, shipment.JS_ActualWeight);
			AssertEquals((ZDecimal)999999.9, shipment.JS_ManifestedWeight);
			AssertEquals((ZDecimal)999999.9, shipment.JS_DocumentedWeight);
		}

		public void TestJS_ActualWeight_NotWithinSqlPrecisionAndScale()
		{
			var shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "Shipment1001";
			shipment.JS_ActualWeight = 1000m;
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			using (shipment.GetValidationSuspender())
			{
				shipment.JS_ActualWeight = 1995840m;
			}

			AssertEquals("CommonShipment_NotWithinSqlPrecisionAndScale", ErrorReporter.LastKeyReported);
			AssertEquals(string.Format("Shipment PK = {0}, JS_UniqueConsignRef = Shipment1001\r\nJS_ActualWeight previous value = 1000, new value = 1995840\r\n", shipment.PK), ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestJS_ActualVolume_NotWithinSqlPrecisionAndScale()
		{
			var shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "Shipment1001";
			shipment.JS_ActualVolume = 3m;
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			using (shipment.GetValidationSuspender())
			{
				shipment.JS_ActualVolume = 1995840m;
			}

			AssertEquals("CommonShipment_NotWithinSqlPrecisionAndScale", ErrorReporter.LastKeyReported);
			AssertEquals(string.Format("Shipment PK = {0}, JS_UniqueConsignRef = Shipment1001\r\nJS_ActualVolume previous value = 3, new value = 1995840\r\n", shipment.PK), ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGetVolumeForDoc()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_ActualVolume = 10M;
			shipment.JS_ManifestedVolume = 9M;
			shipment.JS_DocumentedVolume = 8M;

			AssertEquals(10M, shipment.GetVolumeForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual));
			AssertEquals(10M, shipment.GetVolumeWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual).Item1);

			AssertEquals(9M, shipment.GetVolumeForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier));
			AssertEquals(9M, shipment.GetVolumeWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);

			AssertEquals(8M, shipment.GetVolumeForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client));
			AssertEquals(8M, shipment.GetVolumeWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client).Item1);
		}

		public void TestGetVolumeWithScaleForDoc()
		{
			CommonShipment shipment = GetShipment();

			AssertEquals(JobShipmentSchema.JS_ActualVolume.Scale, shipment.GetVolumeWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual).Item2);

			AssertEquals(JobShipmentSchema.JS_ManifestedVolume.Scale, shipment.GetVolumeWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier).Item2);

			AssertEquals(JobShipmentSchema.JS_DocumentedVolume.Scale, shipment.GetVolumeWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client).Item2);
		}

		public void TestGetChargeableForDoc()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_ActualChargeable = 10M;
			shipment.JS_ManifestedChargeable = 9M;
			shipment.JS_DocumentedChargeable = 8M;

			AssertEquals(10M, shipment.GetChargeableForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual));
			AssertEquals(10M, shipment.GetChargeableWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual).Item1);

			AssertEquals(9M, shipment.GetChargeableForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier));
			AssertEquals(9M, shipment.GetChargeableWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);

			AssertEquals(8M, shipment.GetChargeableForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client));
			AssertEquals(8M, shipment.GetChargeableWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client).Item1);
		}

		public void TestGetChargeableWithScaleForDoc()
		{
			CommonShipment shipment = GetShipment();

			AssertEquals(JobShipmentSchema.JS_ActualChargeable.Scale, shipment.GetChargeableWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual).Item2);

			AssertEquals(JobShipmentSchema.JS_ManifestedChargeable.Scale, shipment.GetChargeableWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier).Item2);

			AssertEquals(JobShipmentSchema.JS_DocumentedChargeable.Scale, shipment.GetChargeableWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client).Item2);
		}

		public void TestGetLoadingMetersForDoc()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_LoadingMeters = 10m;
			shipment.JS_ManifestedLoadingMeters = 9m;
			shipment.JS_DocumentedLoadingMeters = 8m;

			AssertEquals(10m, shipment.GetLoadingMetersForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual));
			AssertEquals(10M, shipment.GetLoadingMetersWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual).Item1);

			AssertEquals(9m, shipment.GetLoadingMetersForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier));
			AssertEquals(9M, shipment.GetLoadingMetersWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);

			AssertEquals(8m, shipment.GetLoadingMetersForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client));
			AssertEquals(8M, shipment.GetLoadingMetersWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client).Item1);
		}

		public void TestGetLoadingMetersWithScaleForDoc()
		{
			CommonShipment shipment = GetShipment();

			AssertEquals(JobShipmentSchema.JS_ActualChargeable.Scale, shipment.GetLoadingMetersWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Actual).Item2);

			AssertEquals(JobShipmentSchema.JS_ManifestedChargeable.Scale, shipment.GetLoadingMetersWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier).Item2);

			AssertEquals(JobShipmentSchema.JS_DocumentedChargeable.Scale, shipment.GetLoadingMetersWithScaleForDoc(Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client).Item2);
		}

		#endregion

		#region Document Auto-Delivery Tests

		public void TestTransportMode()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = "";
			AssertEquals("Shipment TransportMode", Constants.TransportModes.Sea, shipment.TransportMode);
			AssertEquals("IDocumentAutoDelivery TransportMode", Constants.TransportModes.Sea, shipment.DocumentSupporter.TransportMode);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("IDocumentAutoDelivery TransportMode", Constants.TransportModes.Sea, shipment.DocumentSupporter.TransportMode);

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("IDocumentAutoDelivery TransportMode", Constants.TransportModes.Sea, shipment.DocumentSupporter.TransportMode);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = "";
			AssertEquals("IDocumentAutoDelivery TransportMode", Constants.TransportModes.Air, shipment.DocumentSupporter.TransportMode);
		}

		public void TestContainerMode()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = "";
			AssertEquals("IDocumentAutoDelivery TransportMode", Constants.TransportModes.Sea, shipment.DocumentSupporter.TransportMode);
			AssertEquals("IDocumentAutoDelivery ContainerMode", "", shipment.DocumentSupporter.ContainerMode);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("IDocumentAutoDelivery ContainerMode", Constants.ContainerModes.FCL, shipment.DocumentSupporter.ContainerMode);

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("IDocumentAutoDelivery ContainerMode", Constants.ContainerModes.LCL, shipment.DocumentSupporter.ContainerMode);
		}

		public void TestIsImport()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort2;
			AssertEquals("IDocumentAutoDelivery IsImport", false, shipment.DocumentSupporter.IsImport);

			shipment.JS_RL_NKOrigin = OverseasPort2;
			shipment.JS_RL_NKDestination = HomePort;
			AssertEquals("IDocumentAutoDelivery IsImport", true, shipment.DocumentSupporter.IsImport);
		}

		public void TestLocalPort()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "HKHKG";

			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUCNS";
			consol.JK_RL_NKDischargePort = "CNSHA";

			ContactType[] contacts =
			{
				ContactType.ShippingLine,

				ContactType.FreightAgent,

				ContactType.Consignor,
				ContactType.ExportAirFreightAgent,
				ContactType.ExportFreightAgent,
				ContactType.ExportSeaFreightAgent,

				ContactType.Consignee,
				ContactType.ImportAirFreightAgent,
				ContactType.ImportFreightAgent,
				ContactType.ImportSeaFreightAgent,
			};

			DocumentDirection[] directions = { DocumentDirection.ANY, DocumentDirection.DEP, DocumentDirection.ARV };

			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendLine("     ANY   DEP   ARV  ");

			foreach (ContactType contact in contacts)
			{
				string code = contact.Code;
				string anyPort = shipment.DocumentSupporter.LocalPort(contact, DocumentDirection.ANY);
				string depPort = shipment.DocumentSupporter.LocalPort(contact, DocumentDirection.DEP);
				string arvPort = shipment.DocumentSupporter.LocalPort(contact, DocumentDirection.ARV);

				builder.Append(code);
				builder.Append(' ', 4 - code.Length);
				builder.Append(anyPort);
				builder.Append(' ', 6 - anyPort.Length);
				builder.Append(depPort);
				builder.Append(' ', 6 - depPort.Length);
				builder.Append(arvPort);
				builder.Append(' ', 6 - arvPort.Length);
				builder.AppendLine();
			}

			const string expected = @"
     ANY   DEP   ARV  
SHP                   
FWD       AUBNE HKHKG 
CNR AUBNE AUBNE AUBNE 
FEA AUBNE AUBNE AUBNE 
FWE AUBNE AUBNE AUBNE 
FES AUBNE AUBNE AUBNE 
CNE HKHKG HKHKG HKHKG 
FIA HKHKG HKHKG HKHKG 
FWI HKHKG HKHKG HKHKG 
FIS HKHKG HKHKG HKHKG 
";

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestForeignPort()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "HKHKG";

			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUCNS";
			consol.JK_RL_NKDischargePort = "CNSHA";

			ContactType[] contacts =
			{
				ContactType.ShippingLine,

				ContactType.FreightAgent,

				ContactType.Consignor,
				ContactType.ExportAirFreightAgent,
				ContactType.ExportFreightAgent,
				ContactType.ExportSeaFreightAgent,

				ContactType.Consignee,
				ContactType.ImportAirFreightAgent,
				ContactType.ImportFreightAgent,
				ContactType.ImportSeaFreightAgent,
			};

			DocumentDirection[] directions = { DocumentDirection.ANY, DocumentDirection.DEP, DocumentDirection.ARV };

			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendLine("     ANY   DEP   ARV  ");

			foreach (ContactType contact in contacts)
			{
				string code = contact.Code;
				string anyPort = shipment.DocumentSupporter.ForeignPort(contact, DocumentDirection.ANY);
				string depPort = shipment.DocumentSupporter.ForeignPort(contact, DocumentDirection.DEP);
				string arvPort = shipment.DocumentSupporter.ForeignPort(contact, DocumentDirection.ARV);

				builder.Append(code);
				builder.Append(' ', 4 - code.Length);
				builder.Append(anyPort);
				builder.Append(' ', 6 - anyPort.Length);
				builder.Append(depPort);
				builder.Append(' ', 6 - depPort.Length);
				builder.Append(arvPort);
				builder.Append(' ', 6 - arvPort.Length);
				builder.AppendLine();
			}

			const string expected = @"
     ANY   DEP   ARV  
SHP       HKHKG AUBNE 
FWD       HKHKG AUBNE 
CNR HKHKG HKHKG HKHKG 
FEA HKHKG HKHKG HKHKG 
FWE HKHKG HKHKG HKHKG 
FES HKHKG HKHKG HKHKG 
CNE AUBNE AUBNE AUBNE 
FIA AUBNE AUBNE AUBNE 
FWI AUBNE AUBNE AUBNE 
FIS AUBNE AUBNE AUBNE 
";

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestSubShipmentConsigneeNotificationGoesToForwarder()
		{
			CommonShipment shipment = GetShipment();
			CommonShipment masterShipment = GetShipment();

			OrgHeader masterConsignee = Factory.New<OrgHeader>();
			masterConsignee.OH_FullName = "Master Consignee";
			masterConsignee.MiscServ.OM_FWDealDirectlyWithUltimates = ZBool.False;

			masterShipment.ConsigneePK = masterConsignee.PK;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			IDocumentDeliveryContact contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("Sub CommonShipment notification goes to Forwarder of Master Shipment", masterShipment.Consignee.OH_FullName, contact.OrgHeader.FullName);
		}

		public void TestGetContactOrganisationSupportsNotifyParty()
		{
			CommonShipment shipment = GetShipment();
			OrgHeader notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			IDocumentDeliveryContact contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.NotifyParty, DocumentDirection.ANY);
			AssertEquals(notifyParty.PK, contact.OrgHeader.PK);
		}

		public void TestGetContactOrganisationForOverseasAgent()
		{
			CommonShipment shipment = GetShipment();

			OrgHeader overseasAgent = Factory.New<OrgHeader>();
			overseasAgent.OH_FullName = "Overseas Agent";

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.AgentCollectPK = overseasAgent.PK;

			IDocumentDeliveryContact contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.Payables, DocumentDirection.ANY);
			AssertEquals("Payables documents to Agent", "Overseas Agent", contact.OrgHeader.FullName);
		}

		public void TestGetContactOrganisationForExportBroker()
		{
			CommonShipment shipment = GetShipment();

			OrgHeader exportBroker = Factory.New<OrgHeader>();
			exportBroker.OH_FullName = "Export Broker";

			shipment.JS_OH_ExportBroker = exportBroker.PK;

			IDocumentDeliveryContact contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportBroker, DocumentDirection.ANY);
			AssertEquals("Export Broker contact", "Export Broker", contact.OrgHeader.FullName);
		}

		public void TestGetContactOrganisationForImportBroker()
		{
			CommonShipment shipment = GetShipment();

			OrgHeader importBroker = Factory.New<OrgHeader>();
			importBroker.OH_FullName = "Import Broker";

			shipment.JS_OH_ImportBroker = importBroker.PK;

			IDocumentDeliveryContact contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ImportBroker, DocumentDirection.ANY);
			AssertEquals("Import Broker contact", "Import Broker", contact.OrgHeader.FullName);
		}

		public void TestImportBroker()
		{
			SetBranches("AUSYD", "NZAKL", "USMIA");

			OrgHeader importBroker = Factory.New<OrgHeader>();
			OrgHeader melImportBroker = Factory.New<OrgHeader>();
			OrgHeader consignee = Factory.New<OrgHeader>();

			consignee.SetRelatedParty(importBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			consignee.SetRelatedParty(melImportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty, "AUMEL");

			AssertIsImportBroker("KZALA", "AUSYD", consignee, importBroker.PK);
			AssertIsImportBroker("KZALA", "CATOR", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "NZAKL", consignee, importBroker.PK);
			AssertIsImportBroker("KZALA", "USMIA", consignee, importBroker.PK);
			AssertIsImportBroker("KZALA", "AUMEL", consignee, melImportBroker.PK);
			AssertIsImportBroker("KZALA", "AUBNE", "KZALA", "AUMEL", consignee, melImportBroker.PK);
			AssertIsImportBroker("KZALA", "AUMEL", "KZALA", "AUBNE", consignee, importBroker.PK);
		}

		public void TestImportBroker_WhenIsNotUserInteractive()
		{
			var previousIsUserInteractiveValue = Globals.IsUserInteractive;
			Globals.IsUserInteractive = false;
			SetBranches("AUSYD", "NZAKL", "USMIA");

			var importBroker = Factory.New<OrgHeader>();
			var melImportBroker = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();

			consignee.SetRelatedParty(importBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			consignee.SetRelatedParty(melImportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty, "AUMEL");

			AssertIsImportBroker("KZALA", "AUSYD", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "CATOR", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "NZAKL", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "USMIA", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "AUMEL", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "AUBNE", "KZALA", "AUMEL", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "AUMEL", "KZALA", "AUBNE", consignee, ZGuid.Empty);

			Globals.IsUserInteractive = previousIsUserInteractiveValue;
		}

		public void TestImportBroker_WhenIsInSaveTransaction()
		{
			SetBranches("AUSYD", "NZAKL", "USMIA");
			Factory.IsInSaveTransaction = true;

			var importBroker = Factory.New<OrgHeader>();
			var melImportBroker = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();

			consignee.SetRelatedParty(importBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			consignee.SetRelatedParty(melImportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty, "AUMEL");

			AssertIsImportBroker("KZALA", "AUSYD", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "CATOR", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "NZAKL", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "USMIA", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "AUMEL", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "AUBNE", "KZALA", "AUMEL", consignee, ZGuid.Empty);
			AssertIsImportBroker("KZALA", "AUMEL", "KZALA", "AUBNE", consignee, ZGuid.Empty);
		}

		void AssertIsImportBroker(ZString shipmentOrigin, ZString shipmentDestination, OrgHeader consignee, ZGuid expectedImportBrokerPK)
		{
			AssertIsImportBroker(shipmentOrigin, shipmentDestination, ZString.Empty, ZString.Empty, consignee, expectedImportBrokerPK);
		}

		void AssertIsImportBroker(ZString shipmentOrigin, ZString shipmentDestination, ZString consolOrigin, ZString consolDestination, OrgHeader consignee, ZGuid expectedImportBrokerPK)
		{
			consignee.OH_RL_NKClosestPort = shipmentDestination;

			CommonShipment shipment = Factory.New<CommonShipment>();

			if (consolOrigin != ZString.Empty && consolDestination != ZString.Empty)
			{
				CommonConsol consol = Factory.New<CommonConsol>();
				consol.JK_RL_NKLoadPort = consolOrigin;
				consol.JK_RL_NKDischargePort = consolDestination;
				consol.Shipments.Add(shipment);
			}

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = shipmentOrigin;
			shipment.JS_RL_NKDestination = shipmentDestination;
			shipment.ConsigneePK = consignee.PK;

			AssertEquals(string.Format("Incorrect import broker for {0} shipment {1} -> {2}", shipment.JS_TransportMode, shipment.JS_RL_NKOrigin, shipment.JS_RL_NKDestination),
				expectedImportBrokerPK, shipment.JS_OH_ImportBroker);
		}

		public void TestExportBroker()
		{
			SetBranches("AUSYD", "NZAKL", "USMIA");

			OrgHeader airExportBroker = Factory.New<OrgHeader>();
			OrgHeader seaExportBroker = Factory.New<OrgHeader>();
			OrgHeader melExportBroker = Factory.New<OrgHeader>();
			OrgHeader consignor = Factory.New<OrgHeader>();

			consignor.SetRelatedParty(airExportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			consignor.SetRelatedParty(seaExportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			consignor.SetRelatedParty(melExportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty, "AUMEL");

			AssertIsExportBroker(Core.Constants.TransportModes.Air, "AUSYD", "KZALA", consignor, airExportBroker.PK);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUSYD", "KZALA", consignor, seaExportBroker.PK);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "CATOR", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "CATOR", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "NZAKL", "KZALA", consignor, airExportBroker.PK);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "NZAKL", "KZALA", consignor, seaExportBroker.PK);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "USMIA", "KZALA", consignor, airExportBroker.PK);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "USMIA", "KZALA", consignor, seaExportBroker.PK);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "AUMEL", "KZALA", consignor, melExportBroker.PK);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUMEL", "KZALA", consignor, melExportBroker.PK);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUBNE", "KZALA", "AUMEL", "KZALA", consignor, melExportBroker.PK);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUMEL", "KZALA", "AUBNE", "KZALA", consignor, seaExportBroker.PK);
		}

		public void TestExportBroker_WhenIsNotUserInteractive()
		{
			Globals.IsUserInteractive = false;
			SetBranches("AUSYD", "NZAKL", "USMIA");

			OrgHeader airExportBroker = Factory.New<OrgHeader>();
			OrgHeader seaExportBroker = Factory.New<OrgHeader>();
			OrgHeader melExportBroker = Factory.New<OrgHeader>();
			OrgHeader consignor = Factory.New<OrgHeader>();

			consignor.SetRelatedParty(airExportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			consignor.SetRelatedParty(seaExportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			consignor.SetRelatedParty(melExportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty, "AUMEL");

			AssertIsExportBroker(Core.Constants.TransportModes.Air, "AUSYD", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUSYD", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "CATOR", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "CATOR", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "NZAKL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "NZAKL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "USMIA", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "USMIA", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "AUMEL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUMEL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUBNE", "KZALA", "AUMEL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUMEL", "KZALA", "AUBNE", "KZALA", consignor, ZGuid.Empty);
		}

		public void TestExportBroker_WhenIsInSaveTransaction()
		{
			SetBranches("AUSYD", "NZAKL", "USMIA");
			Factory.IsInSaveTransaction = true;

			OrgHeader airExportBroker = Factory.New<OrgHeader>();
			OrgHeader seaExportBroker = Factory.New<OrgHeader>();
			OrgHeader melExportBroker = Factory.New<OrgHeader>();
			OrgHeader consignor = Factory.New<OrgHeader>();

			consignor.SetRelatedParty(airExportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			consignor.SetRelatedParty(seaExportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			consignor.SetRelatedParty(melExportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty, "AUMEL");

			AssertIsExportBroker(Core.Constants.TransportModes.Air, "AUSYD", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUSYD", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "CATOR", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "CATOR", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "NZAKL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "NZAKL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "USMIA", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "USMIA", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Air, "AUMEL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUMEL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUBNE", "KZALA", "AUMEL", "KZALA", consignor, ZGuid.Empty);
			AssertIsExportBroker(Core.Constants.TransportModes.Sea, "AUMEL", "KZALA", "AUBNE", "KZALA", consignor, ZGuid.Empty);
		}

		void AssertIsExportBroker(ZString shipmentTransportType, ZString shipmentOrigin, ZString shipmentDestination, OrgHeader consignor, ZGuid expectedExportBrokerPK)
		{
			AssertIsExportBroker(shipmentTransportType, shipmentOrigin, shipmentDestination, ZString.Empty, ZString.Empty, consignor, expectedExportBrokerPK);
		}

		void AssertIsExportBroker(ZString shipmentTransportType, ZString shipmentOrigin, ZString shipmentDestination, ZString consolOrigin, ZString consolDestination, OrgHeader consignor, ZGuid expectedExportBrokerPK)
		{
			consignor.OH_RL_NKClosestPort = shipmentOrigin;

			CommonShipment shipment = Factory.New<CommonShipment>();

			if (consolOrigin != ZString.Empty && consolDestination != ZString.Empty)
			{
				CommonConsol consol = Factory.New<CommonConsol>();
				consol.JK_RL_NKLoadPort = consolOrigin;
				consol.JK_RL_NKDischargePort = consolDestination;
				consol.Shipments.Add(shipment);
			}

			shipment.JS_TransportMode = shipmentTransportType;
			shipment.JS_RL_NKOrigin = shipmentOrigin;
			shipment.JS_RL_NKDestination = shipmentDestination;
			shipment.ConsignorPK = consignor.PK;

			AssertEquals(string.Format("Incorrect export broker for {0} shipment {1} -> {2}", shipment.JS_TransportMode, shipment.JS_RL_NKOrigin, shipment.JS_RL_NKDestination),
				expectedExportBrokerPK, shipment.JS_OH_ExportBroker);
		}

		void SetBranches(ZString homeBranchLocation, params ZString[] otherBranchesLocations)
		{
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = homeBranchLocation;

			TestCaseHelper.ClearTable(TagRuleSchema.Constants.TableName);

			foreach (GlbBranch branch in Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK)))
			{
				branch.Delete();
			}

			if (otherBranchesLocations != null && otherBranchesLocations.Length > 0)
			{
				for (int i = 0; i < otherBranchesLocations.Length; i++)
				{
					GlbBranch otherBranch = currentBranch.Company.Branches.AddNew();
					otherBranch.GB_Code = string.Format("XX{0}", i);
					otherBranch.GB_RL_NKHomePort = otherBranchesLocations[i];
				}
			}

			Factory.Save();
		}

		public void TestGetContactOrganisationToForwarder()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort2;

			OrgHeader arvSendingAgent = Factory.New<OrgHeader>();
			arvSendingAgent.OH_FullName = "Arv Sending Agent";
			OrgHeader arvReceivingAgent = Factory.New<OrgHeader>();
			arvReceivingAgent.OH_FullName = "Arv Receiving Agent";
			OrgHeader depSendingAgent = Factory.New<OrgHeader>();
			depSendingAgent.OH_FullName = "Dep Sending Agent";
			OrgHeader depReceivingAgent = Factory.New<OrgHeader>();
			depReceivingAgent.OH_FullName = "Dep Receiving Agent";

			CommonConsol depConsol = shipment.Consols.AddNew();
			depConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			depConsol.JK_RL_NKLoadPort = HomePort;
			depConsol.JK_RL_NKDischargePort = OverseasPort;
			depConsol.JK_OA_SendingForwarderAddress = depSendingAgent.MainAddress.PK;
			depConsol.JK_OA_ReceivingForwarderAddress = depReceivingAgent.MainAddress.PK;

			CommonConsol arvConsol = shipment.Consols.AddNew();
			arvConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			arvConsol.JK_RL_NKLoadPort = OverseasPort;
			arvConsol.JK_RL_NKDischargePort = OverseasPort2;
			arvConsol.JK_OA_SendingForwarderAddress = arvSendingAgent.MainAddress.PK;
			arvConsol.JK_OA_ReceivingForwarderAddress = arvReceivingAgent.MainAddress.PK;

			IDocumentDeliveryContact contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY);
			AssertEquals("notification goes to Export Freight Agent", depConsol.SendingForwarder.PK, contact.OrgHeader.PK);

			contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY);
			AssertEquals("notification goes to Export Freight Agent", depConsol.SendingForwarder.PK, contact.OrgHeader.PK);

			contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ExportAirFreightAgent, DocumentDirection.ANY);
			AssertEquals("notification goes to Export Freight Agent", depConsol.SendingForwarder.PK, contact.OrgHeader.PK);

			OrgHeader departureOrg = Factory.New<OrgHeader>();
			OrgHeader arrivalOrg = Factory.New<OrgHeader>();
			OrgAddress departureCTO = departureOrg.Addresses.AddNew();
			OrgAddress arrivalCTO = arrivalOrg.Addresses.AddNew();
			depConsol.JK_OA_DepartureCTOAddress = departureCTO.PK;
			arvConsol.JK_OA_ArrivalCTOAddress = arrivalCTO.PK;

			contact = shipment.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.CTO, DocumentDirection.DEP);
			AssertEquals("Contact Type should be pickup CTO", departureOrg.PK, contact.OrgHeader.PK);

			contact = shipment.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.CTO, DocumentDirection.ARV);
			AssertEquals("Contact Type should be deliver to CTO", arrivalOrg.PK, contact.OrgHeader.PK);
		}

		public void TestGetContactOrganisationImportFreightAgent()
		{
			DocumentDirection[] supportedDirections = new DocumentDirection[] { DocumentDirection.ANY, DocumentDirection.ARV, DocumentDirection.DEP };
			IContactType[] supportedContactTypes = new IContactType[] { ContactType.ImportFreightAgent, ContactType.ImportAirFreightAgent, ContactType.ImportSeaFreightAgent };

			foreach (DocumentDirection documentDirection in supportedDirections)
			{
				foreach (IContactType contactType in supportedContactTypes)
				{
					TestGetContactOrganisationImportFreightAgent(documentDirection, contactType);
				}
			}
		}

		void TestGetContactOrganisationImportFreightAgent(DocumentDirection documentDirection, IContactType contactType)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			AssertNull("Prerequsite", shipment.DeliveryAgent);
			AssertNull("Prerequsite", shipment.ArrivalConsolForDocuments);

			IDocumentDeliveryContact documentContact = shipment.DocumentSupporter.GetContactOrganisation(string.Empty, contactType, documentDirection);
			AssertNull(documentContact);

			CommonConsol arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUMEL";
			arrivalConsol.JK_RL_NKDischargePort = "AUPER";
			OrgHeader arrivalConsolReceivingForwarder = Factory.New<OrgHeader>();
			arrivalConsol.JK_OA_ReceivingForwarderAddress = arrivalConsolReceivingForwarder.MainAddress.PK;

			AssertNull("Prerequsite", shipment.DeliveryAgent);
			AssertEquals("Prerequsite", arrivalConsol, shipment.ArrivalConsolForDocuments);

			documentContact = shipment.DocumentSupporter.GetContactOrganisation(string.Empty, contactType, documentDirection);
			AssertNotNull(documentContact);
			AssertEquals(arrivalConsolReceivingForwarder, documentContact.OrgHeader);

			OrgHeader deliveryAgentHeader = Factory.New<OrgHeader>();
			shipment.JS_OH_DeliveryAgent = deliveryAgentHeader.PK;

			AssertEquals("Prerequsite", deliveryAgentHeader, shipment.DeliveryAgent);

			documentContact = shipment.DocumentSupporter.GetContactOrganisation(string.Empty, contactType, documentDirection);
			AssertNotNull(documentContact);
			AssertEquals(deliveryAgentHeader, documentContact.OrgHeader);
		}

		public void TestSubShipmentUltimateConsignee()
		{
			CommonShipment masterShipment = GetShipment();
			CommonShipment shipment = GetShipment();

			OrgHeader masterConsignee = Factory.New<OrgHeader>();
			masterConsignee.OH_FullName = "Master Consignee";
			masterConsignee.MiscServ.OM_FWDealDirectlyWithUltimates = ZBool.True;

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			ultimateConsignee.MiscServ.OM_FWDealDirectlyWithUltimates = ZBool.True;

			masterShipment.ConsigneePK = masterConsignee.PK;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			shipment.ConsigneePK = ultimateConsignee.PK;

			IDocumentDeliveryContact contact = shipment.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("Sub CommonShipment notification goes to Consignee of Sub Shipment", shipment.Consignee.OH_FullName, contact.OrgHeader.FullName);
		}

		public void TestImportCartageOrganisation()
		{
			CommonShipment shipment = GetShipment();
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			shipment.DocsAndCartage.DeliveryCartageCoPK = header.PK;
			AssertEquals("Import org is Import cartage", header.OH_FullName, shipment.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV).OrgHeader.FullName);
		}

		public void TestExportCartageOrganisation()
		{
			CommonShipment shipment = GetShipment();
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			shipment.DocsAndCartage.PickupCartageCoPK = header.PK;
			AssertEquals("Export org is Export cartage", header.OH_FullName, shipment.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.DEP).OrgHeader.FullName);
		}

		public void TestGetOverriddenDeliveryDetails_ImportCartage()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address1 = org.MainAddress;
			OrgAddress address2 = org.Addresses.AddNew();
			OrgAddress address3 = org.Addresses.AddNew();
			CommonShipment shipment = GetShipment();

			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = address2.PK;
			AssertEquals("Export org is Import cartage", address2.PK, ((BusinessObject)shipment.DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.LocalTransport, DocumentDirection.ARV)).PK);

			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = address3.PK;
			AssertEquals("Export org is Import cartage", address3.PK, ((BusinessObject)shipment.DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.LocalTransport, DocumentDirection.ARV)).PK);
		}

		public void TestGetOverriddenDeliveryDetails_ExportCartage()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address1 = org.MainAddress;
			OrgAddress address2 = org.Addresses.AddNew();
			OrgAddress address3 = org.Addresses.AddNew();
			CommonShipment shipment = GetShipment();

			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = address2.PK;
			AssertEquals("Export org is Export cartage", address2.PK, ((BusinessObject)shipment.DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.LocalTransport, DocumentDirection.DEP)).PK);

			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = address3.PK;
			AssertEquals("Export org is Export cartage", address3.PK, ((BusinessObject)shipment.DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.LocalTransport, DocumentDirection.DEP)).PK);
		}

		public void TestShippingLineContactOrganisation()
		{
			CommonShipment shipment = GetShipment();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;
			AssertEquals("Contact org is ShippingLine", org.PK, shipment.DocumentSupporter.GetContactOrganisation("", ContactType.ShippingLine, DocumentDirection.ARV).OrgHeader.PK);
		}

		public void TestFilterInvoicing()
		{
			CommonShipment aShipment = GetShipment();
			AssertEquals("JobHeader for this Shipment does not exist yet", "", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSC));

			JobHeader aJobHeader = Factory.NewJobForTesting<JobHeader>();
			aJobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			aJobHeader.JH_ParentID = aShipment.PK;
			aJobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			aJobHeader.JH_GC = GlbCompany.CurrentCompany.PK;

			AssertEquals("JobHeader should exist for this Shipment", "INV", aShipment.DocumentSupporter.GetFilterValue(DocumentFilters.MSC));
		}

		public void TestLocalChargesContact()
		{
			CommonShipment shipment = GetShipment();
			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = "JS";
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader localCharge = Factory.New<OrgHeader>();
			localCharge.OH_FullName = "Local Charge test";
			localCharge.OH_Code = "TESTZZ";
			localCharge.Addresses.AddNew(OrgAddressType.Office, true);
			OrgContact contact = localCharge.Contacts.AddNew();
			contact.OC_ContactName = "Testing";

			header.LocalChargesPK = localCharge.PK;

			AssertLocalChargesContact(shipment, ContactType.Receivables);
			AssertLocalChargesContact(shipment, ContactType.LocalClient);
		}

		void AssertLocalChargesContact(CommonShipment shipment, ContactType type)
		{
			AssertEquals("Local Charge test", shipment.DocumentSupporter.GetContactOrganisation("", type, DocumentDirection.ARV).OrgHeader.FullName);
		}

		#endregion

		#region IDocManagerSupport Members

		public void TestDocManagerCode()
		{
			CommonShipment shipment = GetShipment();
			AssertEquals("Code should be SHP. Any change in the doc manager code must also be changed in document scanning lookup", "SHP", ((IDocManagerSupport)shipment).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region IEDocsProvider Members

		public void TestGetEDocsProviderSupporter()
		{
			IEDocsProvider shipment = GetShipment();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), shipment.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region SuppressFlightDetails

		public void TestHasActualRCVPassed()
		{
			IFlightDetailsSuppression bizO = GetShipment();
			AssertEquals("IsActualRCVGreaterThanNow with null date", ZBool.False, bizO.HasActualRCVPassed);

			((CommonShipment)bizO).DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Today.AddDays(1);
			AssertEquals("HasActualRCVPassed with tomorrow date", ZBool.False, bizO.HasActualRCVPassed);

			((CommonShipment)bizO).DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Today.AddDays(-1);
			AssertEquals("HasActualRCVPassed with yesterday date", ZBool.True, bizO.HasActualRCVPassed);
		}

		public void TestHasETDPassed()
		{
			IFlightDetailsSuppression bizO = GetShipment();
			AssertEquals("no consol", ZBool.False, bizO.HasETDPassed);

			CommonConsol consol = CreateExportConsol(((CommonShipment)bizO));
			Transport transport = consol.Transports[0];

			transport.JW_ETD = ZDateTime.Today.AddDays(2);
			AssertEquals("HasETDPassed with tomorrows date", ZBool.False, bizO.HasETDPassed);

			transport.JW_ETD = ZDateTime.Today.AddDays(-1);
			AssertEquals("HasETDPassed with yesterdays date", ZBool.True, bizO.HasETDPassed);
		}

		#endregion

		#region TestSettingOrgHeaderForRequirement

		public void TestSettingOrgHeaderForRequirement()
		{
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.ConsigneeDocumentaryAddress);
			requirement.AddLinkedRequirement(new JobDocAddressRequirement(DocAddressType.ConsigneePickupDeliveryAddress));
			CommonShipment parent = CommonShipment.New(Factory);
			using (parent.GetValidationSuspender())
			{
				parent.DocAddressManager.AddRequirement(requirement);
				JobDocAddress docAddress = parent.DocAddresses.FindOrCreateWithRequirement(requirement);

				OrgHeader orgH = OrgHeader.New(Factory);

				AssertEquals("Initial DocAddresses should be 2.", 2, parent.DocAddresses.Count);
				AssertEquals("Initial DocAddress should be empty.", ZGuid.Empty, docAddress.E2_OA_Address);

				parent.DocAddresses.FindOrCreateWithDocAddressType(requirement.SupportedDocAddressTypes[0]);
				docAddress.OrganisationPK = orgH.PK;
				AssertEquals("DocAddresses should be 2.", 2, parent.DocAddresses.Count);
				AssertEquals("DocAddress should be new OrgH's Main Address.", orgH.MainAddress.PK, docAddress.E2_OA_Address);
				AssertEquals("Supported DocAddress should be new OrgH's Main Address also.", orgH.MainAddress.PK, parent.DocAddresses[1].E2_OA_Address);

				JobDocAddress docAddress2 = parent.DocAddresses[1];
				OrgAddress orgA = orgH.Addresses.AddNew();
				docAddress2.E2_OA_Address = orgA.PK;
				AssertEquals("Supported DocAddress should be new OrgH's Address.", orgA.PK, parent.DocAddresses[1].E2_OA_Address);

				OrgHeader orgH2 = OrgHeader.New(Factory);
				OrgAddress orgA2 = orgH2.Addresses.AddNew();
				docAddress.OrganisationPK = orgH2.PK;

				AssertEquals("DocAddresses should be 2.", 2, parent.DocAddresses.Count);
				AssertEquals("DocAddress should be new OrgH2's Main Address.", orgH2.MainAddress.PK, docAddress.E2_OA_Address);
				AssertEquals("Supported DocAddress should be new OrgH2's Main Address also.", orgH2.MainAddress.PK, docAddress2.E2_OA_Address);

				OrgHeader orgH3 = OrgHeader.New(Factory);
				docAddress2.OrganisationPK = orgH.PK;
				docAddress.OrganisationPK = orgH3.PK;
				AssertEquals("DocAddresses should be 2.", 2, parent.DocAddresses.Count);
				AssertEquals("DocAddress should be new OrgH3's Main Address.", orgH3.MainAddress.PK, docAddress.E2_OA_Address);
				AssertEquals("Supported DocAddress should have retained original value.", orgH.MainAddress.PK, docAddress2.E2_OA_Address);

				docAddress.OrganisationPK = ZGuid.Empty;
				AssertEquals("DocAddresses should be 2.", 2, parent.DocAddresses.Count);
				AssertEquals("DocAddress should be empty.", ZGuid.Empty, docAddress.E2_OA_Address);
				AssertEquals("Supported DocAddress should have retained original value.", orgH.MainAddress.PK, docAddress2.E2_OA_Address);

				docAddress.OrganisationPK = docAddress2.OrganisationPK;
				docAddress.OrganisationPK = ZGuid.Empty;
				AssertEquals("DocAddresses should be 2.", 2, parent.DocAddresses.Count);
				AssertEquals("DocAddress should be empty.", ZGuid.Empty, docAddress.E2_OA_Address);
				AssertEquals("Supported DocAddress should be empty.", ZGuid.Empty, docAddress2.E2_OA_Address);

				docAddress2.OrganisationPK = ZGuid.Empty;
				docAddress.OrganisationPK = orgH3.PK;
				AssertEquals("DocAddresses should be 2.", 2, parent.DocAddresses.Count);
				AssertEquals("DocAddress should be new OrgH3's Main Address.", orgH3.MainAddress.PK, docAddress.E2_OA_Address);
				AssertEquals("Supported DocAddress should have updated also.", orgH3.MainAddress.PK, docAddress2.E2_OA_Address);
			}
		}

		public void TestOrgHeaderAcrossConsigneeAndConsignor()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			OrgHeader consignee = OrgHeader.New(Factory);
			OrgHeader consignor = OrgHeader.New(Factory);

			AssertEquals("JobDocAddresses are empty.", 0, shipment.DocAddresses.Count);
			shipment.ConsigneePK = consignee.PK;
			int docAddressesCount = shipment.DocAddresses.Count;
			AssertEquals("Consignee Requirement's JobDocAddresses are filled.", true, docAddressesCount > 2);
			AssertEquals("Consignee Documentary JobDocAddresses are filled.", consignee.PK, shipment.ConsigneeDocumentaryAddress.OrganisationPK);
			AssertEquals("Consignee Delivery JobDocAddresses are filled.", consignee.PK, shipment.ConsigneeDeliveryAddress.OrganisationPK);
			AssertEquals("Consignee Requirement's JobDocAddresses should not have increased.", docAddressesCount, shipment.DocAddresses.Count);

			shipment.ConsignorPK = consignor.PK;
			docAddressesCount = shipment.DocAddresses.Count;
			AssertEquals("Consignor Requirement's JobDocAddresses are filled.", true, shipment.DocAddresses.Count > 4);
			AssertEquals("Consignor Documentary JobDocAddresses are filled.", consignor.PK, shipment.ConsignorDocumentaryAddress.OrganisationPK);
			AssertEquals("Consignor Pickup JobDocAddresses are filled.", consignor.PK, shipment.ConsignorPickupAddress.OrganisationPK);
			AssertEquals("Consignor Requirement's JobDocAddresses should not have increased.", docAddressesCount, shipment.DocAddresses.Count);
		}

		public void TestOrgHeaderAcrossConsigneeAndConsignorThroughDocAddress()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			OrgHeader consignee = OrgHeader.New(Factory);
			OrgHeader consignor = OrgHeader.New(Factory);

			AssertEquals("JobDocAddresses are empty.", 0, shipment.DocAddresses.Count);
			using (shipment.GetValidationSuspender())
			{
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				AssertNotNull(shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress));
				AssertNotNull(shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneePickupDeliveryAddress));
				AssertEquals("Consignee Documentary JobDocAddresses are filled.", consignee.PK, shipment.ConsigneeDocumentaryAddress.OrganisationPK);
				AssertEquals("Consignee Delivery JobDocAddresses are filled.", consignee.PK, shipment.ConsigneeDeliveryAddress.OrganisationPK);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				AssertNotNull(shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress));
				AssertNotNull(shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress));
				AssertEquals("Consignor Documentary JobDocAddresses are filled.", consignor.PK, shipment.ConsignorDocumentaryAddress.OrganisationPK);
				AssertEquals("Consignor Pickup JobDocAddresses are filled.", consignor.PK, shipment.ConsignorPickupAddress.OrganisationPK);
			}
		}

		#endregion

		#region TestGetWrappersForARInvoice

		public void TestGetWrappersForARInvoice()
		{
			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			GlbBranch currentCompanyBranch = currentCompany.Branches[0];

			GlbCompany otherCompany = Factory.New<GlbCompany>();
			GlbBranch otherCompanyBranch = Factory.New<GlbBranch>();
			otherCompanyBranch.GB_GC = otherCompany.PK;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";

			AccTransactionHeader invoiceOrg1CurrentCompany = GetInvoiceForTest(org1, currentCompanyBranch, "00001001");
			AccTransactionHeader invoiceOrg1OtherCompany = GetInvoiceForTest(org1, otherCompanyBranch, "00001002");
			AccTransactionHeader invoiceOrg2CurrentCompany = GetInvoiceForTest(org2, currentCompanyBranch, "00001003");
			AccTransactionHeader invoiceOrg2OtherCompany = GetInvoiceForTest(org2, otherCompanyBranch, "00001004");

			CommonShipment shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "S00009999";
			JobHeader job = new JobHeader.Loader(shipment).TryCreate();
			invoiceOrg1CurrentCompany.AH_JH = job.PK;
			invoiceOrg1OtherCompany.AH_JH = job.PK;
			invoiceOrg2CurrentCompany.AH_JH = job.PK;
			invoiceOrg2OtherCompany.AH_JH = job.PK;

			Factory.Save();

			DocumentWrapper[] org1InvoiceWrappers = shipment.GetWrappersForARInvoice(org1);
			AssertNotNull("AR Invoice Wrappers for Org1 should not be null", org1InvoiceWrappers);
			AssertEquals("Number of AR Invoice Wrappers for Org1", 1, org1InvoiceWrappers.Length);
			ZGuid businessObjectPK = ((BusinessObject)org1InvoiceWrappers[0].WrappedObject).PK;
			AssertEquals("Wrapper BusinessObject", invoiceOrg1CurrentCompany.PK, businessObjectPK);

			DocumentWrapper[] org1GenericInvoiceWrappers = shipment.GetWrappersForARInvoice(org1, true);
			AssertNotNull("Generic Invoice Wrappers for Org1 should not be null", org1GenericInvoiceWrappers);
			AssertEquals("Number of Generic Invoice Wrappers for Org1", 1, org1GenericInvoiceWrappers.Length);
			businessObjectPK = ((BusinessObject)org1GenericInvoiceWrappers[0].WrappedObject).PK;
			AssertEquals("Generic Wrapper BusinessObject", shipment.PK, businessObjectPK);

			DocumentWrapper[] org2InvoiceWrappers = shipment.GetWrappersForARInvoice(org2);
			AssertNotNull("AR Invoice Wrappers for Org2 should not be null", org2InvoiceWrappers);
			AssertEquals("Number of AR Invoice Wrappers for Org2", 1, org2InvoiceWrappers.Length);
			businessObjectPK = ((BusinessObject)org2InvoiceWrappers[0].WrappedObject).PK;
			AssertEquals("Wrapper BusinessObject", invoiceOrg2CurrentCompany.PK, businessObjectPK);

			DocumentWrapper[] org2GenericInvoiceWrappers = shipment.GetWrappersForARInvoice(org2, true);
			AssertNotNull("Generic Invoice Wrappers for Org2 should not be null", org2GenericInvoiceWrappers);
			AssertEquals("Number of Generic Invoice Wrappers for Org2", 1, org2GenericInvoiceWrappers.Length);
			businessObjectPK = ((BusinessObject)org2GenericInvoiceWrappers[0].WrappedObject).PK;
			AssertEquals("Generic Wrapper BusinessObject", shipment.PK, businessObjectPK);
		}

		AccTransactionHeader GetInvoiceForTest(OrgHeader organisation, GlbBranch branch, ZString invoiceNumber)
		{
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = TransactionTypes.Invoice;
			newInvoice.AH_OH = organisation.PK;
			newInvoice.AH_TransactionNum = invoiceNumber;
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = branch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			return newInvoice;
		}

		public void TestJobHasShipmentAsParent()
		{
			var shipment = Factory.New<CommonShipment>();
			new JobHeader.Loader(shipment).TryLoadOrCreateWithMutex();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);

			AssertEquals(@"This is controversial if we need to set parent loading Job or not.
I think by default we should because you would logically expect JobHeader.Parent to never be null because it can always be loaded by JH_ParentID, JH_ParentTableCode.
In other words, from dbo.JobHeader perspectice we don't want to load the Parent operational job automatically for performance considerations, but Shipment.Job.Parent should be the Shipment because it's already loaded.
The problem may happen with TransportBookings where Shipment and Booking share JobHeader.
Hitting the CommonShipment.Job property while in the context of TransportBooking will swap JobHeader.Parent to Shipment which is undesirable.
If you happen to run into this kind for problem, consider business object context on a Factory preventing the undesirable swap", reloadedShipment.Job.Parent, reloadedShipment);
		}

		public void TestCreateShipmentJobHeaderWithMutex()
		{
			var shipment = GetShipment();
			Factory.Save();

			AssertEquals("", shipment.CreateShipmentJobHeaderWithMutex());

			try
			{
				var anotherFactory = new BusinessObjectFactory();
				var shipmentInAnotherFactory = (CommonShipment)anotherFactory.Load(shipment.GetType(), shipment.PK);
				AssertEquals(
	@"You have created the job S00001000 on another form, but haven't saved it yet.
Please close or save other forms that use job S00001000 to continue.",
					shipmentInAnotherFactory.CreateShipmentJobHeaderWithMutex());
			}
			finally
			{
				shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestJobNumberOfGetWrappersForARInvoice()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			Factory.Save();

			AccTransactionHeader invoice1Org1CurrentCompany = GetInvoiceForTest(org1, GlbBranch.CurrentBranch, "00001001", "S00009999");
			AccTransactionHeader invoice2Org1CurrentCompany = GetInvoiceForTest(org1, GlbBranch.CurrentBranch, "00001002", "S00009999/A");
			AccTransactionHeader invoice3Org1CurrentCompany = GetInvoiceForTest(org1, GlbBranch.CurrentBranch, "00001003", "S000099999");
			Factory.Save();

			CommonShipment shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "S00009999";
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.Parent = shipment;
			invoice1Org1CurrentCompany.AH_JH = job.PK;
			invoice2Org1CurrentCompany.AH_JH = job.PK;
			invoice3Org1CurrentCompany.AH_JH = job.PK;
			Factory.Save();

			DocumentWrapper[] org1InvoiceWrappers = shipment.GetWrappersForARInvoice(org1);
			AssertEquals("Number of AR Invoice Wrappers for Org1", 2, org1InvoiceWrappers.Length);
		}

		AccTransactionHeader GetInvoiceForTest(OrgHeader organisation, GlbBranch branch, ZString invoiceNumber, ZString uniqueConsignRef)
		{
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = organisation.PK;
			newInvoice.AH_TransactionNum = invoiceNumber;
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = branch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = uniqueConsignRef;

			return newInvoice;
		}

		#endregion

		#region IsRoadLoadingMetersEnabled

		public void TestIsRoadLoadingMetersEnabled()
		{
			string[] allTransportModes = typeof(Constants.TransportModes).GetFields().Select(fieldInfo => (string)fieldInfo.GetValue(null)).ToArray();
			AssertEquals("Precondition", true, allTransportModes.Length > 0);

			CommonShipment shipment = Factory.New<CommonShipment>();
			foreach (string transportMode in allTransportModes)
			{
				shipment.JS_TransportMode = transportMode;

				FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals(false, shipment.IsRoadLoadingMetersEnabled);

				FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertEquals(shipment.JS_TransportMode == Constants.TransportModes.Road, shipment.IsRoadLoadingMetersEnabled);
			}
		}

		public void TestLoadingMetersReadonly()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Road;

			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition", true, shipment.IsRoadLoadingMetersEnabled);

			AssertEquals(false, shipment.JS_LoadingMetersInfo.ReadOnly);
			AssertEquals(false, shipment.JS_DocumentedLoadingMetersInfo.ReadOnly);
			AssertEquals(false, shipment.JS_ManifestedLoadingMetersInfo.ReadOnly);

			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition", false, shipment.IsRoadLoadingMetersEnabled);

			AssertEquals(true, shipment.JS_LoadingMetersInfo.ReadOnly);
			AssertEquals(true, shipment.JS_DocumentedLoadingMetersInfo.ReadOnly);
			AssertEquals(true, shipment.JS_ManifestedLoadingMetersInfo.ReadOnly);
		}

		#endregion

		#region OuterPacks CheckTotalsDiffer

		public void TestCheckTotalsDiffer()
		{
			AssertCheckTotalsDiffer(JobShipmentSchema.JS_OuterPacks.Name, JobPackLinesSchema.JL_PackageCount.Name);
			AssertCheckTotalsDiffer(JobShipmentSchema.JS_F3_NKPackType.Name, JobPackLinesSchema.JL_F3_NKPackType.Name);
			AssertCheckTotalsDiffer(JobShipmentSchema.JS_ActualWeight.Name, JobPackLinesSchema.JL_ActualWeight.Name);
			AssertCheckTotalsDiffer(JobShipmentSchema.JS_ActualVolume.Name, JobPackLinesSchema.JL_ActualVolume.Name);
		}

		void AssertCheckTotalsDiffer(string shipmentPropertyName, string packlinePropertyName)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ZPropertyInfoHash[shipmentPropertyName].SetValueFromString("10");

			shipment.OuterPackLines.RemoveAndDeleteAll();

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.ZPropertyInfoHash[packlinePropertyName].SetValueFromString("10");

			bool eventHandlerWasCalled = false;
			shipment.UpdateShipmentTotalsPackQuantityVariation += (sender, args) => eventHandlerWasCalled = true;

			AssertEquals("Precondition: shipment value = packlines value", packLine[packlinePropertyName], shipment[shipmentPropertyName]);

			shipment.CheckTotalsDiffer();
			AssertEquals("Shipment value not updated", "10", shipment[shipmentPropertyName].ToString());
			AssertEquals("Event not raised", false, eventHandlerWasCalled);

			eventHandlerWasCalled = false;
			packLine.ZPropertyInfoHash[packlinePropertyName].SetValueFromString("20");

			AssertEquals("Precondition: shipment value", "10", shipment[shipmentPropertyName].ToString());
			AssertEquals("Precondition: packlines value", "20", packLine[packlinePropertyName].ToString());

			shipment.CheckTotalsDiffer();
			AssertEquals("Shipment value updated", "20", shipment[shipmentPropertyName].ToString());
			AssertEquals("Should have raised event", true, eventHandlerWasCalled);
		}

		public void TestCheckTotalsDifferUsingLoadingMeters()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_LoadingMeters = 10m;

			shipment.OuterPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.AddNew().JL_LoadingMeters = 20m;

			bool eventHandlerWasCalled = false;
			shipment.UpdateShipmentTotalsPackQuantityVariation += (sender, args) => eventHandlerWasCalled = true;

			AssertEquals("Precondition", false, shipment.IsRoadLoadingMetersEnabled);

			shipment.CheckTotalsDiffer();
			AssertEquals("Shipment value not updated", 10m, shipment.JS_LoadingMeters);
			AssertEquals("Event not raised", false, eventHandlerWasCalled);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("Precondition", true, shipment.IsRoadLoadingMetersEnabled);

			shipment.CheckTotalsDiffer();
			AssertEquals("Shipment value updated", 20m, shipment.JS_LoadingMeters);
			AssertEquals("Event was raised", true, eventHandlerWasCalled);
		}

		public void TestCheckTotalsDifferWorksWhenThePackTypeChanges()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 8;
			line1.JL_F3_NKPackType = Constants.PkgUnit.Pallet;

			int callcount = 0;
			shipment.UpdateShipmentTotalsPackQuantityVariation += (sender, args) => callcount++;

			shipment.CheckTotalsDiffer();
			CombineAssertions(delegate
			{
				AssertEquals("Pre-Condition: Pack Count", 8, shipment.JS_OuterPacks);
				AssertEquals("Pre-Condition: Pack Type", Constants.PkgUnit.Pallet, shipment.JS_F3_NKPackType);
			});

			line1.JL_F3_NKPackType = Constants.PkgUnit.Drum;
			callcount = 0;
			shipment.CheckTotalsDiffer();
			CombineAssertions(delegate
			{
				AssertEquals("Pack Unit Changed: Call", 1, callcount);
				AssertEquals("Pack Unit Changed: Pack Count", 8, shipment.JS_OuterPacks);
				AssertEquals("Pack Unit Changed: Pack Type", Constants.PkgUnit.Drum, shipment.JS_F3_NKPackType);
			});

			callcount = 0;
			shipment.CheckTotalsDiffer();
			CombineAssertions(delegate
			{
				AssertEquals("Nothing Changed: Call", 0, callcount);
				AssertEquals("Nothing Changed: Pack Count", 8, shipment.JS_OuterPacks);
				AssertEquals("Nothing Changed: Pack Type", Constants.PkgUnit.Drum, shipment.JS_F3_NKPackType);
			});
		}

		public void TestCheckTotalsDifferDoesNotWorkForTheAssemblyMaster()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OuterPacks = 0;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.CoLoadShipments.AddNew();

			shipment.OuterPackLines.AddNew().JL_PackageCount = 1;
			shipment.OuterPackLines.AddNew().JL_PackageCount = 2;

			int callCount = 0;
			shipment.UpdateShipmentTotalsPackQuantityVariation += delegate
			{ callCount++; };

			shipment.CheckTotalsDiffer();
			AssertEquals("Should NOT have any Packs", 0, shipment.JS_OuterPacks);
			AssertEquals("should have not raised event", 0, callCount);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.OuterPackLines.AddNew().JL_PackageCount = 2;

			shipment.CheckTotalsDiffer();
			AssertEquals("should have raised event for HLV", 1, callCount);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shipment.OuterPackLines.AddNew().JL_PackageCount = 2;

			shipment.CheckTotalsDiffer();
			AssertEquals("should have raised event for HLS", 2, callCount);
		}

		#endregion

		#region IRelatedJobNumber Members

		public void TestIRelatedJobNumberMembers()
		{
			CommonShipment shipment = GetShipment();
			string[] jobNumbers = ((IRelatedJobNumber)shipment).JobNumber;
			AssertEquals("Number of elements in JobNumber", 1, jobNumbers.Length);
			AssertEquals("JobNumber must be equal Shipment.JS_UniqueConsignRef", shipment.JS_UniqueConsignRef, jobNumbers[0]);
		}

		#endregion

		#region Implementation

		StmMenuItem GetMenuItem(string documentName)
		{
			return Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, documentName));
		}

		protected override CommonShipment GetShipment()
		{
			return Factory.New<CommonShipment>();
		}

		CommonConsol CreateExportConsol(CommonShipment shipment)
		{
			CommonConsol consol = shipment.Consols.AddNew();

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			transport.JW_RL_NKDiscPort = "USCHI";
			return consol;
		}

		int CountForLoginEvent;
		[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in LastDocLoginMessage")]
		ZString LastDocLoginMessage;
		void Shipment_OnGetDocumentLogin(object sender, SecurityLoginEventArgs e)
		{
			LastDocLoginMessage = e.LoginPromptMessage;
			if (CountForLoginEvent == 0)
			{
				CountForLoginEvent = 1;
				e.IsAllowedToProceed = ZBool.False;
				e.MessageToShowWhenNotAllowed = (NoResString)"Should not run";
			}
			else
			{
				CountForLoginEvent = 0;
				e.IsAllowedToProceed = ZBool.True;
				e.MessageToShowWhenNotAllowed = (NoResString)"";
			}
		}

		#endregion

		#region TestOverLengthNumberGeneration

		public void TestOverLengthNumberGeneration()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();

			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ServerCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter].Include = true;

			FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			CommonShipment shipment = Factory.New<CommonShipment>();

			AssertExceptionThrown(typeof(GeneratedOverLengthCodeException), Factory.Save);
		}

		#endregion

		#region TestDefaultReleaseType

		public void TestDefaultReleaseType()
		{
			AssertEquals(ZString.Empty, FreightDataRegistry.Instance.ReleaseType.Value);
			CommonShipment shipment = GetShipment();
			AssertEquals(ZString.Empty, shipment.JS_ReleaseType);

			FreightDataRegistry.Instance.ReleaseType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "OBR");
			shipment = GetShipment();
			AssertEquals("OBR", shipment.JS_ReleaseType);
		}

		#endregion

		#region TestDontCapTheLengthOfTheDetailedGoodsDescriptionArtificially

		public void TestDontCapTheLengthOfTheDetailedGoodsDescriptionArtificially()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals(StmNoteSchema.ST_NoteText.MaxLength, shipment.DetailedGoodsDescriptionNoteTextInfo.MaxLength);
		}

		#endregion

		#region TestShouldValidateDeliveryAndPickupCartageCoBeingSame

		public void TestShouldValidateDeliveryAndPickupCartageCoBeingSame()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			IShipmentWithDocsAndCartage iShipment = shipment;
			Assert("Should be validating Pickup and Delivery Cartage Co the same", iShipment.ShouldValidateDeliveryAndPickupCartageCoBeingSame());

			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			Assert("Should be validating Pickup and Delivery Cartage Co the same", iShipment.ShouldValidateDeliveryAndPickupCartageCoBeingSame());

			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			Assert("Should be validating Pickup and Delivery Cartage Co the same", iShipment.ShouldValidateDeliveryAndPickupCartageCoBeingSame());

			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = AlternateHomePort;
			Assert("Should NOT be validating Pickup and Delivery Cartage Co the same", !iShipment.ShouldValidateDeliveryAndPickupCartageCoBeingSame());

			shipment.JS_RL_NKOrigin = AlternateHomePort;
			shipment.JS_RL_NKDestination = HomePort;
			Assert("Should NOT be validating Pickup and Delivery Cartage Co the same", !iShipment.ShouldValidateDeliveryAndPickupCartageCoBeingSame());
		}

		#endregion

		#region TestSyncMeasuresWithInnerPackLines

		public void TestSyncMeasuresWithInnerPackLines()
		{
			CommonShipment ship = Factory.New<CommonShipment>();
			PackLine outer = ship.OuterPackLines.AddNew();
			outer.JL_ActualVolume = 111;
			outer.JL_ActualWeight = 222;
			ship.UpdateShipmentFromOuterPackLines();
			bool userSaysNo = true;
			ship.UpdatingShipmentVolumeFromPacks += (sender, e) => e.Cancel = userSaysNo;
			PackLine inner = ship.InnerPackLines.AddNew();
			inner.JL_ActualVolume = 333;
			inner.JL_ActualWeight = 333;
			inner.JL_PackageCount = 5;
			PackLine inner2 = ship.InnerPackLines.AddNew();
			inner2.JL_ActualVolume = 444;
			inner2.JL_ActualWeight = 444;
			inner2.JL_PackageCount = 15;
			ship.SyncMeasuresWithInnerPackLines();
			AssertEquals((ZDecimal)222, ship.JS_ActualWeight);
			AssertEquals((ZDecimal)111, ship.JS_ActualVolume);
			AssertEquals(0, ship.JS_TotalPackageCount);
			userSaysNo = false;
			ship.SyncMeasuresWithInnerPackLines();
			AssertEquals((ZDecimal)222, ship.JS_ActualWeight);
			AssertEquals((ZDecimal)111, ship.JS_ActualVolume);
			AssertEquals(20, ship.JS_TotalPackageCount);
			AssertEquals((ZDecimal)111, ship.OuterPackLines[0].JL_ActualVolume);
			AssertEquals((ZDecimal)222, ship.OuterPackLines[0].JL_ActualWeight);
		}

		#endregion

		#region TestDeliveryAddressDoesNotUpdatedWhenOverriden

		public void TestDeliveryAddressDoesNotUpdatedWhenOverriden()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			OrgAddress deliveryAddress = header.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_Address1 = "Address1";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			OrgAddress deliveryAddress2 = header.Addresses.AddNew();
			deliveryAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress2.OA_RL_NKRelatedPortCode = "AUBNE";
			deliveryAddress2.OA_Address1 = "Address2";

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.ConsigneeDeliveryAddress.OrganisationPK = header.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = header.PK;
			AssertEquals(shipment.ConsigneeDeliveryAddress.E2_Address1, "Address2");

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("still delivery 2 because overriden", shipment.ConsigneeDeliveryAddress.E2_Address1, "Address2");
		}

		#endregion

		#region TestPickupAddressDoesNotUpdatedWhenOverriden

		public void TestPickupAddressDoesNotUpdatedWhenOverriden()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "USLAX";

			OrgAddress pickupAddress = header.Addresses.AddNew();
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			pickupAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup);
			pickupAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			pickupAddress.OA_Address1 = "Address1";

			OrgAddress pickupAddress2 = header.Addresses.AddNew();
			pickupAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);
			pickupAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			pickupAddress2.OA_RL_NKRelatedPortCode = "AUBNE";
			pickupAddress2.OA_Address1 = "Address2";

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.ConsignorPickupAddress.OrganisationPK = header.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = header.PK;
			shipment.JS_RL_NKOrigin = "AUBNE";
			AssertEquals(shipment.ConsignorPickupAddress.E2_Address1, "Address2");
			AssertEquals(shipment.ConsignorDocumentaryAddress.E2_Address1, "Address2");

			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals(shipment.ConsignorPickupAddress.E2_Address1, "Address2");
			AssertEquals(shipment.ConsignorDocumentaryAddress.E2_Address1, "Address1");
		}

		#endregion

		#region TestDeliveryAddressIsBasedOnDestination

		public void TestDeliveryAddressIsBasedOnDestination()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "USLAX";

			OrgAddress deliveryAddress = header.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			OrgAddress deliveryAddress2 = header.Addresses.AddNew();
			deliveryAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			deliveryAddress2.OA_RL_NKRelatedPortCode = "AUBNE";

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.ConsigneeDeliveryAddress.OrganisationPK = header.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = header.PK;

			shipment.JS_RL_NKDestination = "AUBNE";
			AssertEquals(shipment.ConsigneeDeliveryAddress.E2_OA_Address, deliveryAddress2.PK);
			AssertEquals(shipment.ConsigneeDocumentaryAddress.E2_OA_Address, deliveryAddress2.PK);

			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals(shipment.ConsigneeDeliveryAddress.E2_OA_Address, deliveryAddress.PK);
			AssertEquals(shipment.ConsigneeDocumentaryAddress.E2_OA_Address, deliveryAddress.PK);
			AssertEquals("AUSYD", shipment.JS_RL_NKDestination);
		}

		#endregion

		#region TestPickUpAddressIsBasedOnOrigin

		public void TestPickUpAddressIsBasedOnOrigin()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "USLAX";

			OrgAddress pickupAddress = header.Addresses.AddNew();
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			pickupAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup);
			pickupAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			OrgAddress pickupAddress2 = header.Addresses.AddNew();
			pickupAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);
			pickupAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			pickupAddress2.OA_RL_NKRelatedPortCode = "AUBNE";

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.ConsignorPickupAddress.OrganisationPK = header.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = header.PK;
			shipment.JS_RL_NKOrigin = "AUBNE";
			AssertEquals(shipment.ConsignorPickupAddress.E2_OA_Address, pickupAddress2.PK);
			AssertEquals(shipment.ConsignorDocumentaryAddress.E2_OA_Address, pickupAddress2.PK);

			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals(shipment.ConsignorPickupAddress.E2_OA_Address, pickupAddress.PK);
			AssertEquals(shipment.ConsignorDocumentaryAddress.E2_OA_Address, pickupAddress.PK);
			AssertEquals("AUSYD", shipment.JS_RL_NKOrigin);
		}

		#endregion

		#region TestShortAndDetailedGoodsDescriptionShouldBeIndependant

		public void TestShortAndDetailedGoodsDescriptionShouldBeIndependantOfEachOther()
		{
			CommonShipment shipment = GetShipment();
			shipment.DetailedGoodsDescriptionNoteText = "Detailed Goods Description";
			shipment.JS_GoodsDescription = "Short Goods Description";

			AssertEquals("Detailed Goods Description", shipment.DetailedGoodsDescriptionNoteText);
			AssertEquals("Short Goods Description", shipment.JS_GoodsDescription);
		}

		#endregion

		#region Domestic Freight

		public void TestDomesticSetFromDepartment()
		{
			GlbDepartment.CurrentDepartment.GE_Domestic = true;
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals(true, shipment.IsDomesticFreight);

			GlbDepartment.CurrentDepartment.GE_Domestic = false;
			shipment = Factory.New<CommonShipment>();
			AssertEquals(false, shipment.IsDomesticFreight);
		}

		public void TestDomesticFreightSetsPorts()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.IsDomesticFreight = true;
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, shipment.JS_RL_NKOrigin);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, shipment.JS_RL_NKDestination);
		}

		public void TestDomesticFreightClearsIncoTerm()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.JS_INCO = "EXW";
			shipment.IsDomesticFreight = true;
			AssertEquals(ZString.Empty, shipment.JS_INCO);

			string domesticInco = shipment.Lookups.JS_INCO_List[0].Code;
			shipment.JS_INCO = domesticInco;
			shipment.IsDomesticFreight = true;
			AssertEquals(domesticInco, shipment.JS_INCO);

			shipment.IsDomesticFreight = false;
			AssertEquals(ZString.Empty, shipment.JS_INCO);
		}

		public void TestDomesticSetAutomaticallyWhenSettingPorts()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.IsDomesticFreight = false;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals(false, shipment.IsDomesticFreight);

			shipment.JS_RL_NKDestination = "AUMEL";
			AssertEquals(true, shipment.IsDomesticFreight);

			shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals(false, shipment.IsDomesticFreight);

			shipment.JS_RL_NKDestination = "AUMEL";
			AssertEquals(true, shipment.IsDomesticFreight);

			shipment.JS_RL_NKOrigin = "USLAX";
			AssertEquals(false, shipment.IsDomesticFreight);
		}

		public void TestDomesticFreightSetOnLoading()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonShipment reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
			AssertEquals(true, reloadedShipment.IsDomesticFreight);
			AssertEquals(false, reloadedShipment.HasChanges);
		}

		public void TestDomesticValidation()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.IsDomesticFreight = true;
			AssertHasErrors(shipment.IsDomesticFreightInfo);

			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.IsDomesticFreight = true;
			AssertNoErrors(shipment.IsDomesticFreightInfo);

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.IsDomesticFreight = true;
			AssertHasErrors(shipment.IsDomesticFreightInfo);
		}

		#endregion

		#region TestIHaveInternalCartageGetPackLines

		public void TestIHaveInternalCartageGetPackLines()
		{
			CommonShipment shipment = GetShipment();
			PackLine line = shipment.OuterPackLines.AddNew();
			line.JL_ActualWeight = 10.2m;

			IPackLineInfo[] packs = ((IHaveInternalCartage)shipment).GetPackLines();
			AssertEquals("GetPackLines", 1, packs.Length);
			AssertSame(line, packs[0]);
		}

		#endregion

		#region TestCNRCNEPickupAndDeliveryAddressRefreshes

		class ShipmentWithCNECNRAddressRefresh : CommonShipment
		{
			public ShipmentWithCNECNRAddressRefresh(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ConsolCollection GetNewConsolCollection()
			{
				throw new Exception("The method or operation is not implemented.");
			}

			protected override void ConsignorPickupAddressChanged()
			{
				PickupAddressFlagHit = true;
			}

			protected override void ConsigneeDeliveryAddressChanged()
			{
				DeliveryAddressFlagHit = true;
			}

			public bool PickupAddressFlagHit;
			public bool DeliveryAddressFlagHit;

			protected override void RoundMeasurePropertiesOnTransportModeChangedCore()
			{
				// do nothing, as GetNewConsolCollection is not implemented
			}
		}

		public void TestCNRCNEPickupAndDeliveryAddressRefreshes()
		{
			ShipmentWithCNECNRAddressRefresh shipment = Factory.New<ShipmentWithCNECNRAddressRefresh>();
			Assert("PRECONDITION", !shipment.DeliveryAddressFlagHit);
			Assert("PRECONDITION", !shipment.PickupAddressFlagHit);

			OrgHeader org = Factory.New<OrgHeader>();

			shipment.ConsignorPickupAddress.E2_OA_Address = org.MainAddress.PK;
			Assert(shipment.PickupAddressFlagHit);
			Assert(!shipment.DeliveryAddressFlagHit);

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = org.MainAddress.PK;
			Assert(shipment.PickupAddressFlagHit);
			Assert(shipment.DeliveryAddressFlagHit);
		}

		#endregion

		#region TestConsigneeName, TestConsignorName

		public void TestConsigneeNameOrPK()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "LEB";
			CommonShipment shipment = GetShipment();

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "ANZ";
			AssertEquals("ANZ", shipment.ConsigneeNameOrPK);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(org.PK.ToString(), shipment.ConsigneeNameOrPK);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty.ToString(), shipment.ConsigneeNameOrPK);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;
			ZString code = RelatedBusinessObjectAttribute.GetCodeForGuid(shipment.ConsigneeNameOrPKInfo);
			AssertEquals("Shipment.ConsigneeNameOrPK should be Org.PK", org.PK, new ZGuid(shipment.ConsigneeNameOrPKInfo.Value));
			AssertEquals("Ensure that the RelatedBusinessObjectAttribute is correct.", "LEB", code);
		}

		public void TestConsignorNameOrPK()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "LEB";
			CommonShipment shipment = GetShipment();

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "ANZ";
			AssertEquals("ANZ", shipment.ConsignorNameOrPK);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(org.PK.ToString(), shipment.ConsignorNameOrPK);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty.ToString(), shipment.ConsignorNameOrPK);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
			ZString code = RelatedBusinessObjectAttribute.GetCodeForGuid(shipment.ConsignorNameOrPKInfo);
			AssertEquals("Shipment.ConsignorNameOrPK should be Org.PK", org.PK, new ZGuid(shipment.ConsignorNameOrPKInfo.Value));
			AssertEquals("Ensure that the RelatedBusinessObjectAttribute is correct.", "LEB", code);
		}

		public void TestSettingConsigneeNameOrPkWithInvalidGuid()
		{
			CommonShipment shipment = GetShipment();
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;

			shipment.ConsigneeNameOrPK = "not a guid";
			AssertEquals(ZGuid.Empty.ToString(), shipment.ConsigneeNameOrPK);
		}

		public void TestSettingConsignorNameOrPkWithInvalidGuid()
		{
			CommonShipment shipment = GetShipment();
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;

			shipment.ConsignorNameOrPK = "not a guid";
			AssertEquals(ZGuid.Empty.ToString(), shipment.ConsignorNameOrPK);
		}

		public void TestConsigneeFieldType()
		{
			CommonShipment shipment = GetShipment();

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(nameof(FieldType.Text), shipment.ConsigneeFieldType);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			AssertEquals(nameof(FieldType.OrganisationGuid), shipment.ConsigneeFieldType);
		}

		public void TestConsignorFieldType()
		{
			CommonShipment shipment = GetShipment();

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(nameof(FieldType.Text), shipment.ConsignorFieldType);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			AssertEquals(nameof(FieldType.OrganisationGuid), shipment.ConsignorFieldType);
		}

		public void TestConsignorDocumentaryAddress_ConsigneeNotAccessed()
		{
			var shipment = Factory.New<CommonShipment>();

			IDocAddresses addresses = shipment;
			var requirement = addresses.GetDocAddressRequirement(DocAddressType.NotifyParty);
			requirement.IsMandatory = true;
			shipment.SetReadOnlyIncludingChildren(true);

			var accessProperty = shipment.ConsignorDocumentaryAddress;

			var fConsigneeDocumentaryAddress = shipment.GetType().GetField("fConsigneeDocumentaryAddress",
				BindingFlags.NonPublic |
				BindingFlags.Instance);
			AssertEquals("ConsigneeDocumentaryAddress should not have been accessed", null, fConsigneeDocumentaryAddress.GetValue(shipment));
		}

		#endregion

		#region TestConsigneeContact

		public void TestConsigneeContact()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.ConsigneeContact = "Bob";
			AssertEquals("Bob", shipment.ConsigneeDocumentaryAddress.E2_Contact);

			shipment.ConsigneeDocumentaryAddress.E2_Contact = "Frank";
			AssertEquals("Frank", shipment.ConsigneeContact);
		}

		#endregion

		#region TestConsignorContact

		public void TestConsignorContact()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.ConsignorContact = "Bob";
			AssertEquals("Bob", shipment.ConsignorDocumentaryAddress.E2_Contact);

			shipment.ConsignorDocumentaryAddress.E2_Contact = "Frank";
			AssertEquals("Frank", shipment.ConsignorContact);
		}

		#endregion

		#region TestValidateConsignorAndConsignee

		public void TestValidateConsignorAndConsignee()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_ShipmentType = Constants.AgentType.Agent;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";
			shipment.ConsignorPK = consignor.PK;

			AssertNoErrors("CONSIGNOR is a valid Consignor Organisation", shipment.ConsignorPKInfo);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";
			shipment.ConsigneePK = consignee.PK;

			AssertNoErrors("CONSIGNEE is a valid Consignee Organisation", shipment.ConsigneePKInfo);

			shipment.ConsignorPK = consignee.PK;
			AssertHasError(shipment.ConsignorPKInfo, "This organization is not a valid Consignor");

			shipment.ConsigneePK = consignor.PK;
			AssertHasError(shipment.ConsigneePKInfo, "This organization is not a valid Consignee");

			shipment.JS_ShipmentType = Constants.AgentType.CoLoad;
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.OH_Code = "FORWARDER";
			forwarder.OH_IsForwarder = true;

			shipment.ConsignorPK = forwarder.PK;
			AssertNoErrors("FORWARDER is a valid forwarder Organisation", shipment.ConsignorPKInfo);

			shipment.ConsignorPK = consignor.PK;
			AssertHasErrors("CONSIGNOR is not a valid forwarder Organisation", shipment.ConsigneePKInfo);

			var receivingForwader = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwader.OH_IsForwarder = true;
			receivingForwader.OH_Code = "RFORWARDER";

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			shipment.ConsigneePK = consignee.PK;
			AssertHasError(shipment.ConsigneePKInfo, "This organization is not a valid Forwarder");

			shipment.ConsigneePK = receivingForwader.PK;
			AssertNoError(shipment.ConsigneePKInfo, "This organization is not a valid Forwarder");
		}

		#endregion

		#region TestNotifyContact

		public void TestNotifyContact()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.NotifyContact = "Bob";
			AssertEquals("Bob", shipment.NotifyPartyDocumentaryAddress.E2_Contact);

			shipment.NotifyPartyDocumentaryAddress.E2_Contact = "Frank";
			AssertEquals("Frank", shipment.NotifyContact);
		}

		#endregion

		#region TestNotifyPartyCompanyCode

		public void TestNotifyPartyCompanyCode()
		{
			OrgHeader notifyPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			notifyPartyOrg.OH_Code = "NOTIFYORG";

			OrgContact contact = notifyPartyOrg.Contacts.AddNew();
			contact.OC_ContactName = "Jimmy";

			OrgDocument doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.NotifyParty.Code;

			Factory.Save();

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.NotifyPartyDocumentaryAddress.E2_Contact = "Jimmy";
			AssertEquals("Preconditions: Blank Notify Party", true, shipment.NotifyPartyCompanyCode.IsEmpty);

			shipment.NotifyPartyContactPK = contact.PK;
			AssertEquals("Notify Party OrgCode match.", "NOTIFYORG", shipment.NotifyPartyCompanyCode);
			AssertEquals("NotifyPartyCompanyCode should be readonly.", true, shipment.NotifyPartyCompanyCodeInfo.ReadOnly);

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("NotifyParty has overridden address - empty string expected for NotifyPartyCompanyCode.", true, shipment.NotifyPartyCompanyCode.IsEmpty);

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = false;
			AssertEquals("Notify Party OrgCode match.", "NOTIFYORG", shipment.NotifyPartyCompanyCode);
		}

		#endregion

		#region TestNotifyParty2

		public void TestSupportNotifyParty2()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			IDocAddresses addresses = shipment;

			AssertCollectionContains("should support notify party 2", DocAddressType.NotifyParty2, addresses.SupportedAddressTypes);
			AssertEquals(shipment.NotifyParty2DocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.NotifyParty2).DefaultDocAddressType);
		}

		#endregion

		#region TestNotifyParty3

		public void TestSupportNotifyParty3()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			IDocAddresses addresses = shipment;

			AssertCollectionContains("should support notify party 3", DocAddressType.NotifyParty3, addresses.SupportedAddressTypes);
			AssertEquals(shipment.NotifyParty3DocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.NotifyParty3).DefaultDocAddressType);
		}

		#endregion

		#region TestControllingCustomerAddress

		public void TestControllingCustomerAddress()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			OrgHeader org = Factory.New<OrgHeader>();

			JobDocAddress scp = shipment.ControllingCustomerAddress;
			AssertNotNull("ControllingCustomerAddress", scp);
			AssertNull("controllingCustomer", shipment.ControllingCustomer);
			AssertEquals("ControllingCustomerAddressNameOrPK", ZGuid.Empty.ToString(), shipment.ControllingCustomerNameOrPK);

			scp.OrganisationPK = org.PK;
			AssertEquals(org, shipment.ControllingCustomer);
			AssertEquals(org.PK.ToString(), shipment.ControllingCustomerNameOrPK);
			AssertEquals(nameof(FieldType.OrganisationGuid), shipment.ControllingCustomerFieldType);

			scp.E2_AddressOverride = true;
			AssertNull("ControllingCustomer", shipment.ControllingCustomer);
			AssertEquals("ControllingCustomerAddressNameOrPK", "", shipment.ControllingCustomerNameOrPK);
			AssertEquals(nameof(FieldType.Text), shipment.ControllingCustomerFieldType);

			scp.E2_CompanyName = "aaa";
			AssertNull("ControllingCustomer", shipment.ControllingCustomer);
			AssertEquals("aaa", shipment.ControllingCustomerNameOrPK);
		}

		#endregion

		#region TestSupportPickupAgent

		public void TestSupportPickupAgent()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			IDocAddresses addresses = shipment;

			AssertCollectionContains("should support pickup agent", DocAddressType.PickupAgent, addresses.SupportedAddressTypes);
			AssertEquals(shipment.PickupAgentDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.PickupAgent).DefaultDocAddressType);
		}

		#endregion

		#region TestJS_TransportModeDefaultsInco

		public void TestTransportModeSetsIncoTerm()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals(shipment.JS_INCO, ZString.Empty);
			shipment.JS_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Transport mode = 'COU' should change Incoterm to registry value", shipment.JS_INCO, FreightDataRegistry.Instance.CourierIncoTerm.Value);
			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			shipment.JS_INCO = Constants.IncoTerms.DeliveredAtFrontier;
			shipment.JS_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Transport mode = 'COU' does not changes incoterm because inco is not empty", shipment.JS_INCO, Constants.IncoTerms.DeliveredAtFrontier);
		}

		#endregion

		#region TestDontClearShipmentNumberIfManualShipmentNumberEntryIsAllowed

		public void TestDontClearShipmentNumberIfManualShipmentNumberEntryIsAllowed()
		{
			bool oldValue = Env.Registry.AllowManualShipmentEntry;
			Env.Registry.AllowManualShipmentEntry = true;
			try
			{
				CommonShipment shipment = CommonShipment.New(Factory);

				shipment.JS_UniqueConsignRef = "BlahBlahBlah";

				shipment.OnSaved(false);
				AssertEquals("Should not reset the CommonShipment number when failing to save if manual CommonShipment number entry is allowed", "BlahBlahBlah", shipment.JS_UniqueConsignRef);
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = false;
			}
		}

		#endregion

		#region TestETDETASavedOnSaving

		public void TestETDETASavedOnSaving()
		{
			CommonShipment shipment = GetShipment();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			ZDateTime originalDEP = ZDateTime.Now;
			ZDateTime originalARV = ZDateTime.Now.AddDays(10);
			shipment.JS_E_DEP = originalDEP;
			shipment.JS_E_ARV = originalARV;

			foreach (StmALog log in shipment.Logs.GetAllLogs())
			{
				Assert("ETD log should not have been added", !(log.SL_EventDescription == Events.Departure.Description && log.SL_Reference == originalDEP.ToShortDateString()));
				Assert("ETA log should not have been added", !(log.SL_EventDescription == Events.Arrival.Description && log.SL_Reference == originalARV.ToShortDateString()));
			}

			Factory.Save();

			bool etdFound = false;
			bool etaFound = false;
			foreach (StmALog log in shipment.Logs.GetAllLogs())
			{
				etdFound = etdFound || (log.SL_EventDescription == Events.Departure.Description && !log.IsCancelled && log.SL_Reference == "To: " + originalDEP.ToShortDateString());
				etaFound = etaFound || (log.SL_EventDescription == Events.Arrival.Description && !log.IsCancelled && log.SL_Reference == "To: " + originalARV.ToShortDateString());
			}

			Assert("ETD log should have been added", etdFound);
			Assert("ETA log should have been added", etaFound);

			ZDateTime newDEP = ZDateTime.Now.AddDays(5);
			ZDateTime newARV = ZDateTime.Now.AddDays(15);
			shipment.JS_E_DEP = newDEP;
			shipment.JS_E_ARV = newARV;

			Factory.Save();

			etdFound = false;
			etaFound = false;
			foreach (StmALog log in shipment.Logs.GetAllLogs())
			{
				etdFound = etdFound || (log.SL_EventDescription == Events.Departure.Description && !log.IsCancelled && log.SL_Reference == "From: " + originalDEP.ToShortDateString() + " To: " + newDEP.ToShortDateString());
				etaFound = etaFound || (log.SL_EventDescription == Events.Arrival.Description && !log.IsCancelled && log.SL_Reference == "From: " + originalARV.ToShortDateString() + " To: " + newARV.ToShortDateString());
			}

			Assert("ETD log should have been added", etdFound);
			Assert("ETA log should have been added", etaFound);

			shipment.JS_HouseBill = "Newbill";
			Factory.Save();

			int iCount = shipment.Logs.Find(log => log.SL_EventDescription == Events.Departure.Description
			|| log.SL_EventDescription == Events.Arrival.Description).Count();
			AssertEquals("No additional Arrival or Departure records should be added", 4, iCount);

			shipment.JS_E_DEP = ZDateTime.Empty;
			shipment.JS_E_ARV = ZDateTime.Empty;

			Assert(shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.DepartureCode).All(l => l.IsCancelled));
			Assert(shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.ArrivalCode).All(l => l.IsCancelled));
		}

		public void TestDateEventsSpecifyIsEstimate()
		{
			CommonShipment shipment = GetImportShipment();

			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);
			StmALog arrivalLog = shipment.Logs.MostRecentLogByEventTime(Events.Arrival);
			StmALog departureLog = shipment.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNotNull("Arrival log should have been created", arrivalLog);
			AssertEquals("Arrival log should be an estimate", true, arrivalLog.SL_IsEstimate);
			AssertNotNull("Departure log should have been created", departureLog);
			AssertEquals("Departure log should be an estimate", true, departureLog.SL_IsEstimate);
		}

		[ExpectNoExceptions]
		public void TestSettingETAToEmptyDoesntCauseError()
		{
			CommonShipment shipment = GetImportShipment();
			shipment.JS_E_ARV = ZDateTime.Now;
			Factory.Save();
			shipment.JS_E_ARV = ZDateTime.Empty;
			Factory.Save();
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S00000001";
			shipment.JS_HouseBill = "housebill";
			AssertEquals("HumanReadableName with populated identifiers", "Shipment S00000001 (House Bill='HOUSEBILL')", shipment.HumanReadableName);
			shipment.JS_UniqueConsignRef = "";
			shipment.JS_HouseBill = "";
			AssertEquals("HumanReadableName with no identifiers", "Shipment", shipment.HumanReadableName);
		}

		#endregion

		#region TestHumanReadableShortcutName

		public void TestHumanReadableShortcutName()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertEquals("Shortcut with no identifiers", string.Empty, shipment.HumanReadableShortcutName);

			shipment.JS_UniqueConsignRef = "S00000001";
			AssertEquals("Shortcut with identifier", "S00000001", shipment.HumanReadableShortcutName);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "Blah";
			AssertEquals("Shortcut with identifier and consignee", "S00000001 - Blah", shipment.HumanReadableShortcutName);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "Blah2";
			AssertEquals("Shortcut with identifier, consignee and consignor", "S00000001 - Blah2 - Blah", shipment.HumanReadableShortcutName);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addr = Factory.NewWithValidTestData<OrgAddress>();
			org.OH_FullName = "Blah Company";
			addr.OA_OH = org.PK;
			job.JH_OA_LocalChargesAddr = addr.PK;

			AssertEquals("Shortcut with identifier and local bill to", "S00000001 - " + org.OH_FullName, shipment.HumanReadableShortcutName);
		}

		#endregion

		#region TestCreateAndEditDatesSaved

		public void TestCreateAndEditDatesSaved()
		{
			var newShipment = Factory.New<CommonShipment>();
			newShipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.RL_Code;
			newShipment.JS_RL_NKDestination = "";

			Assert("Initially, no Create time set", newShipment.JS_SystemCreateTimeUtc.IsEmpty);
			Assert("Initially, no Edit time set", newShipment.JS_SystemLastEditTimeUtc.IsEmpty);

			ZDateTime beforeFirstSave = ZDateTime.UtcNow;
			Factory.Save();
			ZDateTime afterFirstSave = ZDateTime.UtcNow;

			var reloadedShipment = Factory.Load<CommonShipment>(newShipment.PK);
			Assert("Create time saved & reloaded", reloadedShipment.JS_SystemCreateTimeUtc >= beforeFirstSave && reloadedShipment.JS_SystemLastEditTimeUtc <= afterFirstSave);
			AssertEquals("Create and Edit time the same", reloadedShipment.JS_SystemCreateTimeUtc, reloadedShipment.JS_SystemLastEditTimeUtc);
		}

		#endregion

		#region TestCreateAndEditDatesSet

		public void TestCreateAndEditDatesSet()
		{
			var newShipment = Factory.New<CommonShipment>();
			newShipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.RL_Code;
			newShipment.JS_RL_NKDestination = "";

			Assert("Initially, no Create time set", newShipment.JS_SystemCreateTimeUtc.IsEmpty);
			Assert("Initially, no Edit time set", newShipment.JS_SystemLastEditTimeUtc.IsEmpty);

			ZDateTime beforeFirstSave = ZDateTime.UtcNow;
			Factory.Save();
			ZDateTime afterFirstSave = ZDateTime.UtcNow;

			Assert("Create time set", newShipment.JS_SystemCreateTimeUtc >= beforeFirstSave && newShipment.JS_SystemLastEditTimeUtc <= afterFirstSave);
			AssertEquals("Create and Edit time the same", newShipment.JS_SystemCreateTimeUtc, newShipment.JS_SystemLastEditTimeUtc);

			ZDateTime pretendCreatedTime = ZDateTime.Now.AddDays(-1);

			newShipment.JS_SystemCreateTimeUtc = pretendCreatedTime;
			newShipment.JS_SystemLastEditTimeUtc = pretendCreatedTime;
			newShipment.HasChanges = false;

			Factory.Save();
			AssertEquals("Not saved, so no time change", pretendCreatedTime, newShipment.JS_SystemCreateTimeUtc);
			AssertEquals("Not saved, so no time change", pretendCreatedTime, newShipment.JS_SystemLastEditTimeUtc);

			newShipment.JS_GoodsValue = 45;
			ZDateTime beforeSecondSave = ZDateTime.UtcNow;
			Factory.Save();
			ZDateTime afterSecondSave = ZDateTime.UtcNow;
			AssertEquals("Already created, so no time change", pretendCreatedTime, newShipment.JS_SystemCreateTimeUtc);
			Assert("Edit time updated", newShipment.JS_SystemLastEditTimeUtc >= beforeSecondSave && newShipment.JS_SystemLastEditTimeUtc <= afterSecondSave);

			newShipment.JS_GoodsValue = 245;
			ZDateTime beforeThirdSave = ZDateTime.UtcNow;
			Factory.Save();
			ZDateTime afterThirdSave = ZDateTime.UtcNow;
			AssertEquals("Already created, so no time change", pretendCreatedTime, newShipment.JS_SystemCreateTimeUtc);
			Assert("Edit time updated", newShipment.JS_SystemLastEditTimeUtc >= beforeThirdSave && newShipment.JS_SystemLastEditTimeUtc <= afterThirdSave);
		}

		#endregion

		#region TestHouseBillDefault

		public void TestHouseBillDefault()
		{
			var newShipment = Factory.New<CommonShipment>();
			newShipment.JS_RL_NKOrigin = HomePort;
			newShipment.JS_RL_NKDestination = OverseasPort;
			Factory.Save();
			AssertEquals("Export Shipment: Blank Housebill should default to CommonShipment number.", newShipment.JS_UniqueConsignRef, newShipment.JS_HouseBill);

			var importShipment = Factory.New<CommonShipment>();
			importShipment.JS_RL_NKOrigin = "";
			importShipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.RL_Code;
			Factory.Save();
			AssertEquals("Import Shipment: Blank Housebill should not default.", "", importShipment.JS_HouseBill);
		}

		public void TestHouseBillDefault_AddsLog()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			Factory.Save();

			AssertEquals("Expected House Bill to match JS_UniqueConsignRef for export shipment", shipment.JS_UniqueConsignRef, shipment.JS_HouseBill);

			var expectedReference = string.Format("House Bill number '{0}' has been generated.", shipment.JS_UniqueConsignRef);
			var houseBillLog = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, expectedReference));

			AssertNotNull(houseBillLog);
			Assert("Expected to find a log saying the house bill log has been generated", houseBillLog.Any());
		}

		public void TestHouseBillDefault_RollsBackChangesOnSaveFailed()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;

			Factory.Save();

			var initialHouseBill = shipment.JS_HouseBill;
			Assert("Expected to have generated a house bill", !initialHouseBill.IsEmpty);

			var expectedReference = string.Format("House Bill number '{0}' has been generated.", initialHouseBill);
			var houseBillLog = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, expectedReference));
			Assert("Expected to only find one log in this format", houseBillLog.Length == 1);

			try
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "abc";
				org.OH_ScreeningStatus = "abc";

				Factory.Save();

				Fail("Should not reach this point because org headers has invalid column. That org has no relevance to this test other than to ensure saving will fail.");
			}
			catch (ZSaveException) { }

			AssertEquals("Expected to have reverted back to the original consign ref", initialHouseBill, shipment.JS_HouseBill);

			houseBillLog = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, expectedReference));
			Assert("Expected to still find only one log in this format as any other generated log should have been deleted", houseBillLog.Length == 1);
		}

		public void TestHouseBillDefault_RollsBackChangesOnSaveFailed_InitialSaveFailure()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;

			var partialReference = "House Bill number ";
			var houseBillLog = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, partialReference));
			Assert("Expected not to find any log as we haven't saved the shipment", !houseBillLog.Any());

			try
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "abc";
				org.OH_ScreeningStatus = "abc";

				Factory.Save();

				Fail("Should not reach this point because org headers must have an OH_Code. This failing org has no use other than to ensure saving will fail.");
			}
			catch (ZSaveException) { }

			Assert("Expected to have set the house bill to blank as saving failed", shipment.JS_HouseBill.IsEmpty);

			houseBillLog = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, partialReference));
			Assert("Expected not to find any log as we were unable to generate a house bill.", !houseBillLog.Any());
		}

		#endregion

		#region TestDefaultJS_HouseBillOfLadingType

		public void TestDefaultJS_HouseBillOfLadingType()
		{
			var newShipment = Factory.New<CommonShipment>();
			newShipment.JS_TransportMode = Constants.TransportModes.Sea;
			Assert("Housebill of lading type list should not be empty.", newShipment.Lookups.JS_HouseBillOfLadingType_List.Count > 0);
			AssertEquals("Housebill of lading type list should be defaulted.", newShipment.Lookups.JS_HouseBillOfLadingType_List[0].Code, newShipment.JS_HouseBillOfLadingType);
		}

		public void TestDefaultJS_HouseBillOfLadingType_WithDefaultCode()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("TY1", "Type 1");
			list.AddPair("TY2", "Type 2");

			var typesWithDefault = new SystemDefinableCodeDescriptionBoolCollection(3, list, true);
			typesWithDefault.SetDefaultCode("TY2", false);

			using (FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typesWithDefault))
			{
				GlbDepartment.CurrentDepartment.GE_Sea = true;
				var newShipment = Factory.NewWithValidTestData<CommonShipment>();
				AssertEquals("Housebill of lading type should be defaulted.", "TY2", newShipment.JS_HouseBillOfLadingType);
			}
		}

		#endregion

		#region TestDefaultOriginalBillReqFromReleaseType

		public void TestDefaultOriginalBillReqFromReleaseType()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertEquals("Original bill required should be false by default", false, shipment.DocsAndCartage.RequiredDocuments.IsDocRequired(Constants.RefDocTypes.HouseBill));

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			AssertEquals("HBL required doc should be added by default when shipment release type is set to 'OBR'", true, shipment.DocsAndCartage.RequiredDocuments.IsDocRequired(Constants.RefDocTypes.HouseBill));

			var requiredHBLDocument = shipment.DocsAndCartage.RequiredDocuments.Cast<JobRequiredDocument>().Single(doc => doc.EQ_DocType == Constants.RefDocTypes.HouseBill);
			AssertEquals("'Original Required' flag should be set by default when shipment release type is set to 'OBR'", true, requiredHBLDocument.EQ_OriginalDocRequired);

			Factory.Save();

			shipment.JS_ReleaseType = "XXX";
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;

			AssertEquals("Setting 'Original Required' document for HBL again reuses the first document added",
				requiredHBLDocument,
				shipment.DocsAndCartage.RequiredDocuments.Cast<JobRequiredDocument>().Single(doc => doc.EQ_DocType == Constants.RefDocTypes.HouseBill));

			AssertEquals("'Original Required' flag should be set by default when shipment release type is set to 'OBR'", true, requiredHBLDocument.EQ_OriginalDocRequired);
		}

		#endregion

		#region TestDefaultElectronicBillOfLadingFields

		public void TestDefaultElectronicBillOfLadingFields()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(Constants.BillOfLadingBillType.Codes.Straight, shipment.JS_ElectronicBillOfLadingType);
			AssertEquals(Constants.BillOfLadingBillTerms.Codes.NonTransferable, shipment.JS_ElectronicBillOfLadingTerms);
			AssertEquals(ZString.Empty, shipment.JS_ElectronicBillOfLadingStatus);

			shipment.JS_ElectronicBillOfLadingStatus = "SUR";
			shipment.JS_TransportMode = Constants.TransportModes.Road;

			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(ZString.Empty, shipment.JS_ElectronicBillOfLadingType);
			AssertEquals(ZString.Empty, shipment.JS_ElectronicBillOfLadingTerms);
			AssertEquals(ZString.Empty, shipment.JS_ElectronicBillOfLadingStatus);
		}

		public void TestDefaultElectronicBillOfLadingFields_WhenCreatingNewShipment()
		{
			GlbDepartment.CurrentDepartment.GE_Sea = false;

			var shipment = Factory.New<CommonShipment>();
			AssertNoExceptionThrown(Factory.Save);

			AssertEquals(ZString.Empty, GlbDepartment.CurrentDepartment.TransportMode);
			AssertEquals(ZString.Empty, shipment.JS_TransportMode);
			AssertEquals(ZString.Empty, shipment.JS_ElectronicBillOfLadingType);
			AssertEquals(ZString.Empty, shipment.JS_ElectronicBillOfLadingTerms);
			AssertEquals(ZString.Empty, shipment.JS_ElectronicBillOfLadingStatus);

			GlbDepartment.CurrentDepartment.GE_Sea = true;

			var shipment2 = Factory.New<CommonShipment>();
			AssertNoExceptionThrown(Factory.Save);

			AssertEquals(Constants.TransportModes.Sea, GlbDepartment.CurrentDepartment.TransportMode);
			AssertEquals(Constants.TransportModes.Sea, shipment2.JS_TransportMode);
			AssertEquals(Constants.BillOfLadingBillType.Codes.Straight, shipment2.JS_ElectronicBillOfLadingType);
			AssertEquals(Constants.BillOfLadingBillTerms.Codes.NonTransferable, shipment2.JS_ElectronicBillOfLadingTerms);
			AssertEquals(ZString.Empty, shipment2.JS_ElectronicBillOfLadingStatus);
		}

		#endregion

		#region TestDefaultElectronicBillOfLadingHouseBill

		public void TestDefaultElectronicBillOfLadingHouseBill()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "S0000872";
			Factory.Save();

			AssertEquals(ZString.Empty, shipment.JS_ElectronicBillOfLadingHouseBill);
			AssertNotEquals(shipment.JS_HouseBill, shipment.JS_ElectronicBillOfLadingHouseBill);

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				shipment.JS_HouseBill = "S00008720";
				AssertNotEquals("JS_TransportMode must be Sea", shipment.JS_HouseBill, shipment.JS_ElectronicBillOfLadingHouseBill);
			}

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();

			boleroEBLConfiguration.EnableEBLIntegration = false;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				shipment.JS_HouseBill = "S00008720";
				AssertNotEquals("'Enable Bolero eHBL Integration' This registry must be enabled", shipment.JS_HouseBill, shipment.JS_ElectronicBillOfLadingHouseBill);
			}

			boleroEBLConfiguration.EnableEBLIntegration = true;
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				shipment.JS_HouseBill = "S00008720";
				AssertEquals(shipment.JS_HouseBill, shipment.JS_ElectronicBillOfLadingHouseBill);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				shipment.JS_ElectronicBillOfLadingHouseBill = "S00008721";
				shipment.JS_HouseBill = "S00008720";
				AssertNotEquals("JS_ElectronicBillOfLadingHouseBill must be empty", shipment.JS_HouseBill, shipment.JS_ElectronicBillOfLadingHouseBill);
			}
		}

		#endregion

		public void TestDefaultHBLDocsRemovedForNonObrReleaseTypeIfNotSaved()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.ExpressBofL;
			AssertNull("Precondition: Original bill required should be false", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			AssertNotNull("Precondition: HBL required doc should be added by default when shipment release type is set to 'OBR'", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.ExpressBofL;
			AssertNull("Original bill added by default should no longer be present as the shipment was not yet saved", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));

			Factory.Save();

			AssertNull("No Original Bill should added by default yet", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			AssertNotNull("Precondition: HBL required doc should be added by default when shipment release type is set to 'OBR'", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));

			Factory.Save();

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.ExpressBofL;
			AssertNotNull("Original bill added by default and was saved should still be present", shipment.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));
		}

		#region TestCoLoadsGridLabel

		public void TestCoLoadsGridLabel()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Assert("Co-Load", shipment.JS_Calc_CoLoadsGridLabel.Contains("Co-Load"));

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			Assert("Buyers Consol.", shipment.JS_Calc_CoLoadsGridLabel.Contains("Buyers Consol"));

			shipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			Assert("Shippers Consol.", shipment.JS_Calc_CoLoadsGridLabel.Contains("Shippers Consol"));

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			Assert("Assembly Master", shipment.JS_Calc_CoLoadsGridLabel.Contains("Assembly"));
		}

		#endregion

		#region TestJobDocAddressContentsChanged

		public void TestJobDocAddressContentsChanged()
		{
			WonkyShipmentForTesting shipment = Factory.New<WonkyShipmentForTesting>();
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "1 Cuckoo Squeaker Ln";
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "7 Gibbiceps Rd";
			Assert(shipment.ConsigneeAddressChanged);
			Assert(shipment.ConsignorAddressChanged);
		}

		public class WonkyShipmentForTesting : CommonShipment
		{
			public WonkyShipmentForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			public bool ConsignorAddressChanged;
			public bool ConsigneeAddressChanged;

			protected override void ConsignorDocumentaryAddressContentsChanged()
			{
				base.ConsignorDocumentaryAddressContentsChanged();
				ConsignorAddressChanged = true;
			}

			protected override void ConsigneeDocumentaryAddressContentsChanged()
			{
				base.ConsigneeDocumentaryAddressContentsChanged();
				ConsigneeAddressChanged = true;
			}

			protected override ConsolCollection GetNewConsolCollection()
			{
				return new ConsolCollection(this);
			}
		}

		#endregion

		#region TestCloneIncludesAddresses

		public void TestCloneIncludesAddresses()
		{
			OrgHeader randomOrg = Factory.NewWithValidTestData<OrgHeader>();

			CommonShipment shipment = GetShipment();
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "Blah";
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = randomOrg.PK;

			CommonShipment shipmentCopy = (CommonShipment)shipment.Clone();
			AssertEquals("Consignor Override", true, shipmentCopy.ConsignorDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignor Company Name", "Blah", shipmentCopy.ConsignorDocumentaryAddress.E2_CompanyName);
			AssertEquals("Consignor Doc Address Has Correct Parent", shipmentCopy.PK, shipmentCopy.ConsignorDocumentaryAddress.E2_ParentID);

			AssertEquals("Consignee Override", false, shipmentCopy.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignee PK", randomOrg.PK, shipmentCopy.ConsigneeDocumentaryAddress.OrganisationPK);
			AssertEquals("Consignee Doc Address Has Correct Parent", shipmentCopy.PK, shipmentCopy.ConsigneeDocumentaryAddress.E2_ParentID);

			AssertEquals("Old Consignor Parent Not Changed", shipment.PK, shipment.ConsignorDocumentaryAddress.E2_ParentID);
			AssertEquals("Old Consignee Parent Not Changed", shipment.PK, shipment.ConsigneeDocumentaryAddress.E2_ParentID);
		}

		#endregion

		#region TestDocAddressPropertiesShouldBeDeleteAware

		public void TestDocAddressPropertiesShouldBeDeleteAware()
		{
			CommonShipment shipment = GetShipment();

			AssertNoJobDocAddressProperties(shipment);
			shipment.DocAddresses.RemoveAndDeleteAll();
			AssertNoJobDocAddressProperties(shipment);
		}

		void AssertNoJobDocAddressProperties(CommonShipment shipment)
		{
			ZStringBuilder errors = new ZStringBuilder();

			foreach (PropertyInfo info in shipment.GetType().GetProperties())
			{
				if (typeof(JobDocAddress).IsAssignableFrom(info.PropertyType))
				{
					JobDocAddress address = (JobDocAddress)info.GetValue(shipment, null);
					if (address == null)
					{
						errors.Append(info.Name + " returned null" + System.Environment.NewLine);
					}
					else if (address.IsDeleted)
					{
						errors.Append(info.Name + " returned a deleted address" + System.Environment.NewLine);
					}
				}
			}

			AssertEquals("Expected no errors", "", errors.ToString());
		}

		#endregion

		#region TestBusinessObjectsWithRelatedEvents

		public void TestBusinessObjectsWithRelatedEvents()
		{
			CommonShipment masterShipmentA = CommonShipment.New(Factory);
			masterShipmentA.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment masterShipmentB = CommonShipment.New(Factory);
			masterShipmentB.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			masterShipmentB.JS_JS_ColoadMasterShipment = masterShipmentA.PK;

			CommonShipment masterShipmentC = CommonShipment.New(Factory);
			masterShipmentC.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			masterShipmentC.JS_JS_ColoadMasterShipment = masterShipmentB.PK;

			CommonShipment masterShipmentD = CommonShipment.New(Factory);
			masterShipmentD.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			masterShipmentD.JS_JS_ColoadMasterShipment = masterShipmentC.PK;

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_JS_ColoadMasterShipment = masterShipmentD.PK;
			CommonConsol consol = shipment.Consols.AddNew();
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();

			var invoices = new List<ZGuid>();
			var invoiceLines = new List<ZGuid>();

			var masterTransport = masterShipmentA.Transports.AddNew();
			var consolTransport = consol.Transports.AddNew();
			var transport1 = shipment.Transports.AddNew();
			var transport2 = shipment.Transports.AddNew();

			var voyage = Factory.New<JobVoyage>();

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "UAIEV";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			transport1.JW_JX = voyage.Sailings[0].PK;

			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.Containers.Add(container1);

			PackLine line2 = shipment.OuterPackLines.AddNew();
			line2.Containers.Add(container2);

			CommonPickupDeliveryConfirm picConfirm = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm dlvConfirm = shipment.DeliveryConfirms.AddNew();

			AssertEquals("Must see Consol log milestones and events", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(consol));
			AssertEquals("Must see JobDocsAndCartage logs", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(shipment.DocsAndCartage));
			AssertEquals("Must see containers logs", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(container2));
			AssertEquals("Must see picConfirm logs", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(picConfirm));
			AssertEquals("Must see dlvConfirm logs", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(dlvConfirm));
			AssertEquals("Must see master shipment", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(masterShipmentA));
			AssertEquals("Must see master shipment", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(masterShipmentB));
			AssertEquals("Must see master shipment", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(masterShipmentC));
			AssertEquals("Must see master shipment", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(masterShipmentD));
			AssertEquals("Must see transport", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(transport1));
			AssertEquals("Must see transport", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(transport2));
			AssertEquals("Must see consol's transport", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(consolTransport));
			AssertEquals("Must see master's transport", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(masterTransport));
			AssertEquals("Must see voyage", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(voyage));

			void AddDeclarationAndRelatedData(string countryCode)
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = company.PK;
				company.GC_RN_NKCountryCode = countryCode;
				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var declaration = Factory.New<IBaseJobDeclaration>();
					declaration.JE_JS = shipment.PK;

					((BusinessObject)declaration).FillWithValidTestData();

					var invoice = Factory.New<IBaseJobComInvoiceHeader>();
					invoice.JZ_JE = declaration.PK;

					((BusinessObject)invoice).FillWithValidTestData();

					var invoiceLine = invoice.AddNewInvoiceLine();

					((BusinessObject)invoiceLine).FillWithValidTestData();

					invoices.Add(invoice.PK);
					invoiceLines.Add(invoiceLine.PK);

					Factory.Save();
					shipment = NewFactory().Load<CommonShipment>(shipment.PK);
				}
			}

			AddDeclarationAndRelatedData(Core.Constants.CountryCodes.Australia);
			AddDeclarationAndRelatedData(Core.Constants.CountryCodes.UnitedStates);
			AddDeclarationAndRelatedData(Core.Constants.CountryCodes.Germany);

			var pks = shipment.BusinessObjectsWithRelatedEvents.Select(c => c.PK);
			AssertEquals("Must see Invoice Headers", true, invoices.All(c => pks.Contains(c)));
			AssertEquals("Must see Invoice Lines", true, invoiceLines.All(c => pks.Contains(c)));
		}

		#endregion

		#region TestUpdateActualChargeableWeightSuspendUpdateActualChargeableWeight

		public void TestUpdateActualChargeableWeightSuspendUpdateActualChargeableWeight()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ActualChargeable = 0.1m;
			using (shipment.SuspendSettingActualChargeable())
			{
				shipment.UpdateChargeableWeights();
				AssertEquals(0.1m, shipment.JS_ActualChargeable);
			}

			shipment.UpdateChargeableWeights();
			AssertNotEquals(0.1m, shipment.JS_ActualChargeable);
		}

		#endregion

		#region TestDeletedBusinessObjectLogs

		public void TestDeletedBusinessObjectLogs()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.Delete();
			AssertNotNull("Logs should not throw an exception when accessing from deleted business objects", shipment.BusinessObjectsWithRelatedEvents);
			AssertEquals("Logs collection should be empty for a deleted Shipment", 0, shipment.BusinessObjectsWithRelatedEvents.Length);
		}

		#endregion

		#region TestDeletingShipmentDeletesJobDocAddresses

		public void TestDeletingShipmentDeletesJobDocAddresses()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignee = false;
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_IsConsignor = false;

			var shipment = CommonShipment.New(Factory);
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			var docAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, shipment.PK));
			Assert(docAddresses.Length > 0);

			shipment.Delete();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertNull("Shipment should have been deleted", newFactory.LoadTop1<CommonShipment>(new ZQuery(JobShipmentSchema.PK, shipment.PK)));
			docAddresses = newFactory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, shipment.PK));
			AssertEquals("JobDocAddress should have been deleted", 0, docAddresses.Length);
		}

		#endregion

		#region Readonly packs

		public void TestPacksAreReadOnlyWhenNonMasterParentIs()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.SetReadOnlyIncludingChildren(true);
			shipment.SetReadOnlyIncludingChildren(true);
			AssertEquals(true, shipment.InnerPackLines.ReadOnly);
			AssertEquals(true, shipment.OuterPackLines.ReadOnly);
		}

		#endregion

		#region TestPopulateBillAndShipmentNumber

		[TestDate(2007, 02, 05)]
		[RunInExtraTransaction]
		public void TestPopulateBillAndShipmentNumberIfNeeded_Import()
		{
			BillOfLadingNumberCustomisationsByServiceLevel newCustomisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisation newCustomisation = newCustomisations.BillOfLadingNumberCustomisations.AddNew();
			newCustomisation.ServiceLevel = "STD";
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			FreightDataRegistry.Instance.HouseBillNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisations);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			AssertEquals("precondition: should be import", true, shipment.IsImport());

			shipment.OnSaving();
			AssertEquals("JS_UniqueConsignRef", "S00001000", shipment.JS_UniqueConsignRef);
			AssertEquals("JS_HouseBill", "", shipment.JS_HouseBill);

			shipment.OnSaving();
			AssertEquals("JS_UniqueConsignRef", "S00001000", shipment.JS_UniqueConsignRef);
			AssertEquals("JS_HouseBill", "", shipment.JS_HouseBill);

			FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);

			shipment.JS_UniqueConsignRef = "";
			shipment.OnSaving();
			AssertEquals("JS_UniqueConsignRef", "S7B001", shipment.JS_UniqueConsignRef);
			AssertEquals("JS_HouseBill", "", shipment.JS_HouseBill);

			shipment.OnSaving();
			AssertEquals("JS_UniqueConsignRef", "S7B001", shipment.JS_UniqueConsignRef);
			AssertEquals("JS_HouseBill", "", shipment.JS_HouseBill);
		}

		[TestDate(2007, 02, 05)]
		[RunInExtraTransaction]
		public void TestPopulateBillAndShipmentNumberIfNeeded_Export()
		{
			CommonShipment shipment;

			BillOfLadingNumberCustomisationsByServiceLevel newCustomisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisation newCustomisation = newCustomisations.BillOfLadingNumberCustomisations["ALL"];
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			FreightDataRegistry.Instance.HouseBillNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisations);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			AssertEquals("precondition: should be export", true, shipment.IsExport());

			Factory.Save();
			AssertEquals("Populate on first saving: JS_UniqueConsignRef", "S00001000", shipment.JS_UniqueConsignRef);
			AssertEquals("Populate on first saving: JS_HouseBill", "S7B001", shipment.JS_HouseBill);

			var houseBillLog = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "House Bill number 'S7B001' has been generated."));
			AssertNotNull(houseBillLog);
			Assert("Expected to find a log with the generated house bill", houseBillLog.Any());

			shipment.JS_HouseBill = "";
			Factory.Save();
			AssertEquals("Dont populate on subsiquent savings: JS_UniqueConsignRef", "S00001000", shipment.JS_UniqueConsignRef);
			AssertEquals("Dont populate on subsiquent savings: JS_HouseBill", "", shipment.JS_HouseBill);

			BillOfLadingNumberCustomisation newShipmentCustomisation = new BillOfLadingNumberCustomisation();
			SetElement(newShipmentCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1).Fountain = true;
			SetElement(newShipmentCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
			SetElement(newShipmentCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newShipmentCustomisation);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			AssertEquals("precondition: should be export", true, shipment.IsExport());

			Factory.Save();
			AssertEquals("Populate on first saving: JS_UniqueConsignRef", "S7B002", shipment.JS_UniqueConsignRef);
			AssertEquals("Populate on first saving: JS_HouseBill", "S7B002", shipment.JS_HouseBill);

			houseBillLog = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "House Bill number 'S7B002' has been generated."));
			AssertNotNull(houseBillLog);
			Assert("Expected to find a log with the new house bill number", houseBillLog.Any());
		}

		public void TestPopulateHouseBillForHLVShipmentType()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;

			Factory.Save();
			AssertEquals("JS_UniqueConsignRef is populated", false, shipment.JS_UniqueConsignRef.IsEmpty);
			AssertEquals("HouseBill is populated", false, shipment.JS_HouseBill.IsEmpty);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;

			Factory.Save();
			AssertEquals("JS_UniqueConsignRef is populated", false, shipment.JS_UniqueConsignRef.IsEmpty);
			AssertEquals("HouseBill is populated", false, shipment.JS_HouseBill.IsEmpty);
		}

		[TestDate(2007, 02, 05)]
		[RunInExtraTransaction]
		public void TestPopulateBillAndShipmentNumberIfNeeded_AdditionalPorts()
		{
			BillOfLadingNumberCustomisationsByServiceLevel newCustomisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisation newCustomisation = newCustomisations.BillOfLadingNumberCustomisations["ALL"];
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			FreightDataRegistry.Instance.HouseBillNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisations);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = OverseasPort2;
			Assert("precondition: should NOT be export", !shipment.IsExport());
			Assert("precondition: should NOT be export", !shipment.IsImport());

			Factory.Save();
			AssertEquals("Populate on first saving: JS_UniqueConsignRef", "S00001000", shipment.JS_UniqueConsignRef);
			AssertEquals("Should NOT Populate on first saving: JS_HouseBill", "", shipment.JS_HouseBill);

			GlbBranchExtraPorts extraPort = GlbBranch.CurrentBranch.ExtraPorts.AddNew();
			extraPort.GY_RL_NKAdditionalBranchRelatedPort = OverseasPort;

			shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = OverseasPort2;
			Assert("precondition: should NOT be export", !shipment.IsExport());
			Assert("precondition: should NOT be export", !shipment.IsImport());

			Factory.Save();
			AssertEquals("Populate on first saving: JS_UniqueConsignRef", "S00001001", shipment.JS_UniqueConsignRef);
			AssertEquals("Should NOW populate because Origin(OverseasPort) is considered LOCAL: JS_HouseBill", "S7B001", shipment.JS_HouseBill);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = OverseasPort2;
			shipment.JS_RL_NKDestination = OverseasPort;
			Assert("precondition: should NOT be export", !shipment.IsExport());
			Assert("precondition: should NOT be export", !shipment.IsImport());

			Factory.Save();
			AssertEquals("Populate on first saving: JS_UniqueConsignRef", "S00001002", shipment.JS_UniqueConsignRef);
			AssertEquals("Should NOT populate because Origin(OverseasPort2) is NOT considered LOCAL: JS_HouseBill", "", shipment.JS_HouseBill);
		}

		[TestDate(2007, 02, 05)]
		public void TestPopulateBillAndShipmentNumberIfNeeded_FixFountain()
		{
			Db.Connection.RollbackTransaction();
			using (var adminConnection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(adminConnection, Db.Connection.CloseConnection))
			{
				BillOfLadingNumberCustomisation newCustomisation = new BillOfLadingNumberCustomisation();
				newCustomisation.ServiceLevel = "STD";
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1).Fountain = true;
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
				FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);

				CommonShipment shipment = Factory.New<CommonShipment>();
				Factory.Save();
				AssertEquals("JS_UniqueConsignRef", "S7B001", shipment.JS_UniqueConsignRef);
				shipment.JS_UniqueConsignRef = "S7B002";
				Factory.Save();

				shipment = Factory.New<CommonShipment>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				AssertEquals("Should reset the CommonShipment HBL when failing to save if generated", "", shipment.JS_HouseBill);
				Factory.Save();
				AssertEquals("JS_UniqueConsignRef", "S7B003", shipment.JS_UniqueConsignRef);

				newCustomisation.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
				FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);
				shipment.JS_UniqueConsignRef = "S7B004S";
				Factory.Save();

				shipment = Factory.New<CommonShipment>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				AssertEquals("Should reset the CommonShipment HBL when failing to save if generated", "", shipment.JS_HouseBill);
				Factory.Save();
				AssertEquals("JS_UniqueConsignRef", "S7B0052", shipment.JS_UniqueConsignRef);
			}
			Db.Connection.BeginTransaction();
		}

		[TestDate(2007, 02, 05)]
		public void TestPopulateBillAndShipmentNumberIfNeeded_FixFountainSameLength()
		{
			Db.Connection.RollbackTransaction();
			using (var adminConnection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(adminConnection, Db.Connection.CloseConnection))
			{
				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "S9997";
				shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "S10007";
				Factory.Save();

				BillOfLadingNumberCustomisation newCustomisation = new BillOfLadingNumberCustomisation();
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 1, "3");
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 2).Fountain = true;
				FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);
				var fountain = Env.NumberFountains.GetForwardingGeneratorFountain("7");
				((IDbConnected)Factory).Connection.BeginTransaction();
				fountain.SetNext(Factory, 1000);
				((IDbConnected)Factory).Connection.CommitTransaction();
				shipment = Factory.New<CommonShipment>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();
				AssertEquals("JS_UniqueConsignRef", "S10017", shipment.JS_UniqueConsignRef);
			}
			Db.Connection.BeginTransaction();
		}

		[TestDate(2007, 02, 05)]
		public void TestPopulateBillAndShipmentNumberIfNeeded_FixFountainDifferentSuffixes()
		{
			Db.Connection.RollbackTransaction();
			using (var adminConnection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(adminConnection, Db.Connection.CloseConnection))
			{
				BillOfLadingNumberCustomisation newCustomisation = new BillOfLadingNumberCustomisation();
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 1, "3");
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 2).Fountain = true;
				FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);
				var fountain = Env.NumberFountains.GetForwardingGeneratorFountain("7");
				((IDbConnected)Factory).Connection.BeginTransaction();
				fountain.SetNext(Factory, 1002);
				((IDbConnected)Factory).Connection.CommitTransaction();
				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "S10027";
				shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "S10037";
				shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "S10048";
				Factory.Save();

				shipment = Factory.New<CommonShipment>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();
				AssertEquals("JS_UniqueConsignRef", "S10047", shipment.JS_UniqueConsignRef);
			}
			Db.Connection.BeginTransaction();
		}

		[TestDate(2007, 02, 05)]
		public void TestPopulateBillAndShipmentNumberIfNeeded_FixFountainNoPrefix()
		{
			Db.Connection.RollbackTransaction();
			using (var adminConnection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(adminConnection, Db.Connection.CloseConnection))
			{
				BillOfLadingNumberCustomisation newCustomisation = new BillOfLadingNumberCustomisation();
				newCustomisation.RemoveFountainPrefix = true;
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 1, "3");
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 2).Fountain = true;
				FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);
				var fountain = Env.NumberFountains.GetForwardingGeneratorFountain("7");
				((IDbConnected)Factory).Connection.BeginTransaction();
				fountain.SetNext(Factory, 1005);
				((IDbConnected)Factory).Connection.CommitTransaction();
				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "10057";
				shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "10067";
				Factory.Save();

				shipment = Factory.New<CommonShipment>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();
				AssertEquals("JS_UniqueConsignRef", "10077", shipment.JS_UniqueConsignRef);
			}
			Db.Connection.BeginTransaction();
		}

		[UseSnapshotProtection]
		public void TestPopulateBillAndShipmentNumberIfNeeded_FixFountainIfManualShipmentNumberEntryIsAllowed()
		{
			using (RunNonTransactioned())
			{
				bool oldValue = Env.Registry.AllowManualShipmentEntry;
				Env.Registry.AllowManualShipmentEntry = true;
				try
				{
					CommonShipment shipment = Factory.New<CommonShipment>();
					Factory.Save();
					AssertEquals("JS_UniqueConsignRef", "S00001000", shipment.JS_UniqueConsignRef);
					shipment.JS_UniqueConsignRef = "S00001001";
					Factory.Save();

					shipment = Factory.New<CommonShipment>();
					try
					{
						Factory.Save();
						Fail("Index exception should happen");
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
						ErrorReporter.Clear();
					}

					shipment.JS_UniqueConsignRef = "";
					Factory.Save();
					AssertEquals("JS_UniqueConsignRef", "S00001002", shipment.JS_UniqueConsignRef);

					shipment = Factory.New<CommonShipment>();
					shipment.JS_UniqueConsignRef = "S00001002";
					try
					{
						Factory.Save();
						Fail("Index exception should happen");
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
						ErrorReporter.Clear();
					}

					AssertEquals("JS_UniqueConsignRef", "S00001002", shipment.JS_UniqueConsignRef);
				}
				finally
				{
					Env.Registry.AllowManualShipmentEntry = false;
				}
			}
		}

		public void TestClearHousebillNumberifGeneratedAndSaveFailed()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_HouseBill = "BlahBlahBlah";
			shipment.OnSaved(false);
			AssertEquals("Should not reset the CommonShipment HBL when failing to save if entered manually", "BLAHBLAHBLAH", shipment.JS_HouseBill);

			shipment = CommonShipment.New(Factory);
			shipment.OnSaved(false);
			AssertEquals("Should reset the CommonShipment HBL when failing to save if generated", "", shipment.JS_HouseBill);
		}

		[TestDate(2007, 02, 05)]
		[RunInExtraTransaction]
		public void TestPopulateBillAndShipmentNumberIfNeeded_UsingMacro()
		{
			var newCustomisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			newCustomisations.AllowNonAlphanumericCharacters = true;
			newCustomisations.EnableMacroInsertion = true;
			var newCustomisation = newCustomisations.BillOfLadingNumberCustomisations["ALL"];
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1).Detail = "<JS_TransportMode>";
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			FreightDataRegistry.Instance.HouseBillNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisations);

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_RL_NKOrigin = HomePort;
			shipment1.JS_RL_NKDestination = OverseasPort;
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();
			AssertEquals("JS_HouseBill", "SSEAB001", shipment1.JS_HouseBill);

			FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_RL_NKOrigin = HomePort;
			shipment2.JS_RL_NKDestination = OverseasPort;
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();
			AssertEquals("JS_HouseBill", "SAIRB002", shipment2.JS_HouseBill);
		}

		#endregion

		#region JS_Calc_Container*

		public void TestJS_Calc_ContainerCount()
		{
			Action<CommonShipment, ZGuid, short> addContainer = (shipment1, refContainerPK, jC_ContainerCount) =>
			{
				CommonContainer container = shipment1.Consols[0].Containers.AddNew();
				container.JC_ContainerCount = jC_ContainerCount;
				container.JC_RC = refContainerPK;

				PackLine packLine = shipment1.OuterPackLines.AddNew();
				packLine.SetContainer(container.PK);

				AssertEquals("Precondition", true, shipment1.Containers.Contains(container));
			};

			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();

			addContainer(shipment, RC_20GP_PK, 1);
			addContainer(shipment, RC_20GP_PK, 2);

			AssertEquals(3m, shipment.JS_Calc_TEUCount);

			addContainer(shipment, RC_40GP_PK, 4);
			addContainer(shipment, RC_40GP_PK, 8);

			AssertEquals(27m, shipment.JS_Calc_TEUCount);

			addContainer(shipment, RC_20RE_PK, 16);
			addContainer(shipment, RC_20RE_PK, 32);

			addContainer(shipment, RC_40RE_PK, 20);
			addContainer(shipment, RC_40RE_PK, 30);

			RefContainer[] refContainers = Factory.Load<RefContainer>(new ZQuery());
			ZGuid otherContainerTypePK = refContainers.First(container => container.IsOtherContainerType).PK;

			addContainer(shipment, otherContainerTypePK, 13);
			addContainer(shipment, otherContainerTypePK, 23);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			shipment = anotherFactory.Load<CommonShipment>(shipment.PK);

			AssertEquals(3, shipment.JS_Calc_20GPCount);
			AssertEquals(12, shipment.JS_Calc_40GPCount);
			AssertEquals(48, shipment.JS_Calc_20RECount);
			AssertEquals(50, shipment.JS_Calc_40RECount);
			AssertEquals(36, shipment.JS_Calc_OtherContainerCount);
			AssertEquals(3 + 12 + 48 + 50 + 36, shipment.JS_Calc_ContainerCount);
		}

		#endregion

		#region TestJS_OrderReferences
		public void TestJS_OrderReferences()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.DocsAndCartage.JP_OrderItemsAsString = "Test stuff";
			AssertEquals("Order references is DocsAndCartage.JP_OrderItemsAsString", shipment.DocsAndCartage.JP_OrderItemsAsString, shipment.JS_OrderReferences);
		}
		#endregion

		#region Test Template Copy

		public void TestDateTimesClearedOnTemplateCopy()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_E_DEP = ZDateTime.Today;
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Today;

			CommonShipment shipmentCopy = (CommonShipment)shipment.TemplateCopy();
			AssertEquals("DateTimes Cleared on Copy", ZDateTime.Empty, shipmentCopy.JS_E_DEP);
			AssertEquals("DateTimes Cleared on Copy", ZDateTime.Empty, shipmentCopy.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public void TestUniqueDetailsClearedOnTemplateCopy()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "Magic";
			shipment.JS_HouseBill = "Happens";

			CommonShipment shipmentCopy = (CommonShipment)shipment.TemplateCopy();
			AssertEquals("Unique References Cleared on Copy", ZString.Empty, shipmentCopy.JS_UniqueConsignRef);
			AssertEquals("Unique References Cleared on Copy", ZString.Empty, shipmentCopy.JS_HouseBill);
		}

		public void TestHVLVDoesNotNeedConsignorPickupDeliveryAddress()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			var address = shipment.DocAddresses.AddNew(DocAddressType.ConsignorPickupDeliveryAddress);
			address.OrganisationPK = Factory.New<OrgHeader>().PK;
			address.E2_OA_Address = ZGuid.Empty;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			shipment.RunPreSaveValidation();
			AssertNotContains("HVLV data must have a Consignor Pickup/Delivery Address.", "A real Organization & Address is required for HVLV data.", shipment.GetErrors().ToMessageListString());
		}

		public void TestHVLVMustHaveConsignorDocumentaryAddress()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			var address = shipment.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			address.OrganisationPK = Factory.New<OrgHeader>().PK;
			address.E2_OA_Address = ZGuid.Empty;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			shipment.RunPreSaveValidation();
			AssertContains("HVLV data must have a Consignor Doc Address.", "A real Organization & Address is required for HVLV data.", shipment.GetErrors().ToMessageListString());
		}

		public void TestCanSaveAfterTemplateCopy()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignee = false;
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_IsConsignor = false;
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;
			consignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.DocsAndCartage.JP_OrderItemsAsString = "splaty";
			shipment.RunPreSaveValidation();
			AssertEquals("Cannot have errors for this test", string.Empty, shipment.GetErrors().ToMessageListString());

			CommonShipment copiedShipment = (CommonShipment)shipment.TemplateCopy();
			copiedShipment.DocsAndCartage.JP_OrderItemsAsString = "bah";
			copiedShipment.RunPreSaveValidation();
			AssertEquals("Copied CommonShipment shouldnt have errors either", string.Empty, copiedShipment.GetErrors().ToMessageListString());
		}

		public void TestTemplateCopyAsMaster()
		{
			CommonShipment master = GetShipment();
			master.JS_RL_NKOrigin = "AUSYD";
			CommonShipment kid1 = GetShipment();
			kid1.JS_RL_NKOrigin = "AUBNE";
			CommonShipment kid2 = GetShipment();
			kid2.JS_RL_NKOrigin = "AUMEL";
			CommonShipment freeShipment = GetShipment();
			freeShipment.JS_RL_NKOrigin = "AUPER";
			CommonShipment kid21 = GetShipment();
			kid21.JS_RL_NKOrigin = "AUADL";
			CommonShipment kid211 = GetShipment();
			kid211.JS_RL_NKOrigin = "AUOOL";
			CommonShipment kid212 = GetShipment();
			kid212.JS_RL_NKOrigin = "AUNTL";

			kid1.JS_JS_ColoadMasterShipment = master.PK;
			kid2.JS_JS_ColoadMasterShipment = master.PK;
			kid21.JS_JS_ColoadMasterShipment = kid2.PK;
			kid211.JS_JS_ColoadMasterShipment = kid21.PK;
			kid212.JS_JS_ColoadMasterShipment = kid21.PK;

			List<CommonShipment> clones = master.TemplateCopy(ZGuid.Empty).ToList();

			AssertEquals(6, clones.Count);
			ZGuid newParent = new ZGuid();
			ZGuid kid2Clone = new ZGuid();
			ZGuid kid21Clone = new ZGuid();
			ZGuid kid1cloneParent = ZGuid.Empty;
			ZGuid kid2cloneParent = ZGuid.Empty;
			ZGuid kid21cloneParent = ZGuid.Empty;
			ZGuid kid211cloneParent = ZGuid.Empty;
			ZGuid kid212cloneParent = ZGuid.Empty;

			foreach (CommonShipment clone in clones)
			{
				switch (clone.JS_RL_NKOrigin)
				{
					case "AUSYD":
						Assert(clone.JS_JS_ColoadMasterShipment.IsEmpty);
						newParent = clone.PK;
						break;

					case "AUBNE":
						kid1cloneParent = clone.JS_JS_ColoadMasterShipment;
						break;

					case "AUMEL":
						kid2cloneParent = clone.JS_JS_ColoadMasterShipment;
						kid2Clone = clone.PK;
						break;

					case "AUADL":
						kid21cloneParent = clone.JS_JS_ColoadMasterShipment;
						kid21Clone = clone.PK;
						break;

					case "AUOOL":
						kid211cloneParent = clone.JS_JS_ColoadMasterShipment;
						break;

					case "AUNTL":
						kid212cloneParent = clone.JS_JS_ColoadMasterShipment;
						break;

					default:
						Fail("Unexpected clone");
						break;
				}
			}

			AssertEquals(kid1cloneParent, newParent);
			AssertEquals(kid1cloneParent, kid2cloneParent);
			AssertEquals(kid21cloneParent, kid2Clone);
			AssertEquals(kid211cloneParent, kid21Clone);
			AssertEquals(kid212cloneParent, kid21Clone);

			Assert(!kid1cloneParent.IsEmpty);
			Assert(!kid2Clone.IsEmpty);
			Assert(!kid21Clone.IsEmpty);
		}

		public void TestTemplateCopyForSubshipmentsDoesNotAttachConsols()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment master = consol.Shipments.AddNew();
			master.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment subShipment = master.CoLoadShipments.AddNew();

			CommonShipment[] copies = master.TemplateCopy(ZGuid.Empty).ToArray();
			AssertEquals("Both shipments copied", 2, copies.Length);
			AssertEquals("Master-Sub relationship preserved", copies[0].PK, copies[1].JS_JS_ColoadMasterShipment);
			AssertEquals("Consol not attached", false, copies.Any(shipment => shipment.Consols.Count > 0));
		}

		public void TestColumnNameExcludedFromTemplateCopy()
		{
			var shipment = GetShipment();
			shipment.JS_TH_OneTimeQuote = ZGuid.NewZGuid();
			shipment.JS_JS_ColoadMasterShipment = ZGuid.NewZGuid();

			var shipmentCopy = (CommonShipment)shipment.TemplateCopy();
			AssertEquals("JS_TH_OneTimeQuote should not be copied.", shipmentCopy.JS_TH_OneTimeQuote, ZGuid.Empty);
			AssertEquals("JS_JS_ColoadMasterShipment should not be copied.", shipmentCopy.JS_JS_ColoadMasterShipment, ZGuid.Empty);
		}

		public void TestJobDocAddressInTheSameAlternativeFactoryOfShipmentTemplateCopy()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.DocAddresses.AddNew(DocAddressType.ControllingAgent);

			var alternativeFactory = new BusinessObjectFactory();
			var shipmentCopy = (CommonShipment)shipment.TemplateCopy(alternativeFactory);
			var shipmentCopyFactoryInstance = shipmentCopy.Factory?._Instance;

			AssertEquals("Alternative factory must be the same as the shipment template copy.", shipmentCopyFactoryInstance, alternativeFactory?._Instance);
			AssertEquals("JobDocAddress factory must be the same as the shipment template copy.", shipmentCopyFactoryInstance, shipmentCopy.DocAddresses.First().Factory?._Instance);
		}

		public void TestPackLinesInTheSameAlternativeFactoryOfShipmentTemplateCopy()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.InnerPackLines.AddNew();
			shipment.OuterPackLines.AddNew();

			var alternativeFactory = new BusinessObjectFactory();
			var shipmentCopy = (CommonShipment)shipment.TemplateCopy(alternativeFactory);
			var shipmentCopyFactoryInstance = shipmentCopy.Factory?._Instance;

			AssertEquals("Alternative factory must be the same as the shipment template copy.", shipmentCopyFactoryInstance, alternativeFactory?._Instance);
			AssertEquals("InnerPackLines factory must be the same as the shipment template copy.", shipmentCopyFactoryInstance, shipmentCopy.InnerPackLines.First().Factory?._Instance);
			AssertEquals("OuterPackLines factory must be the same as the shipment template copy.", shipmentCopyFactoryInstance, shipmentCopy.OuterPackLines.First().Factory?._Instance);
		}

		public void TestIRPEventCreatedWithShipmentTemplateCopy()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.AddAddressType(OrgAddressType.Office);
			org.OH_Code = "ORG1";
			org.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_OA_ExportReceivingDepot = org.MainAddress.PK;
			shipment.JS_InterimReceipt = "9999";
			shipment.JS_A_RCV = ZDateTime.Today;
			Factory.Save();

			var shipmentCopy = (CommonShipment)((ITemplateCopyable)shipment).TemplateCopy();
			AssertEquals("9999", shipmentCopy.JS_InterimReceipt);
			AssertEquals(ZDateTime.Empty, shipmentCopy.JS_A_RCV);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, shipmentCopy.JS_A_RCV.Kind);
			AssertNull("InterimReceiptProduced event should not be logged", shipmentCopy.Logs.MostRecentLogByEventTime(AutoEvents.InterimReceiptProduced));
		}

		#endregion

		#region Test Set

		public void TestWeightUnit_Set()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_ActualWeight = 0;
			shipment.JS_UnitOfWeight = "";

			shipment.JS_ActualWeight = 5;
			Assert(!shipment.JS_UnitOfWeight.IsEmpty);
		}

		public void TestVolumeUnit_Set()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_ActualVolume = 0;
			shipment.JS_UnitOfVolume = "";

			shipment.JS_ActualVolume = 5;
			Assert(!shipment.JS_UnitOfWeight.IsEmpty);
		}

		public void TestTotalPackageCountUnit_Set()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TotalPackageCount = 0;
			shipment.JS_F3_NKTotalCountPackType = "";

			shipment.JS_TotalPackageCount = 5;
			Assert(!shipment.JS_F3_NKTotalCountPackType.IsEmpty);
		}

		public void TestOuterPackageCountUnit_Set()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_OuterPacks = 0;
			shipment.JS_F3_NKPackType = "";

			shipment.JS_OuterPacks = 5;
			Assert(!shipment.JS_F3_NKPackType.IsEmpty);
		}

		public void TestSetJS_ReleaseType()
		{
			ReleaseTypes releaseTypes = FreightDataRegistry.Instance.ReleaseTypes.DefaultValue;
			releaseTypes.OriginalsNumber = 5;
			releaseTypes.CopiesNumber = 6;

			ReleaseType expressReleaseType = releaseTypes.Types.FindByCode(Core.Constants.ShipmentReleaseTypes.ExpressBofL);
			expressReleaseType.OriginalsNumber = 0;
			expressReleaseType.CopiesNumber = 4;

			FreightDataRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, releaseTypes);

			CommonShipment shipment = GetShipment();
			AssertEquals("Starting condition - Release Type should not be EBL", true, shipment.JS_ReleaseType != Core.Constants.ShipmentReleaseTypes.ExpressBofL);
			AssertEquals("Default from ReleaseTypes registry value.", (byte)5, shipment.JS_NoOriginalBills);
			AssertEquals("Default from ReleaseTypes registry value.", (byte)6, shipment.JS_NoCopyBills);

			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			AssertEquals("Originals should be 0", (byte)0, shipment.JS_NoOriginalBills);
			AssertEquals("Default Copies from Express Bills registry value.", (byte)4, shipment.JS_NoCopyBills);
		}

		#endregion

		#region Test Calculated Properties

		public void TestJS_PaymentTerm()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_INCO = "";
			AssertEquals("No INCO", "", shipment.JS_PaymentTerm);

			shipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			AssertEquals("Free On Board is Collect", Constants.PaymentType.Collect, shipment.JS_PaymentTerm);

			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			AssertEquals("Cost and Freight is Prepaid", Constants.PaymentType.Prepaid, shipment.JS_PaymentTerm);
		}

		public void TestJS_PaymentTermDisplay()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_INCO = "";
			AssertEquals("No INCO", "", shipment.JS_PaymentTermDisplay);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			foreach (string inco in Constants.IncoTerms.Incoterms2000.Union(Core.Constants.IncoTerms.Incoterms2010))
			{
				shipment.JS_INCO = inco;
				Assert("missing prepaid / collect description", !string.IsNullOrEmpty(shipment.JS_PaymentTermDisplay));
			}

			shipment.JS_RL_NKDestination = "AUBNE";

			foreach (string inco in domesticPaymentTerms)
			{
				shipment.JS_INCO = inco;
				Assert("missing prepaid / collect description", !string.IsNullOrEmpty(shipment.JS_PaymentTermDisplay));
			}

			shipment.JS_RL_NKDestination = "USLAX";

			foreach (string inco in prepaidIncos)
			{
				shipment.JS_INCO = inco;
				AssertEquals("Freight Prepaid", shipment.JS_PaymentTermDisplay);
			}

			foreach (string inco in collectIncos)
			{
				shipment.JS_INCO = inco;
				AssertEquals("Freight Collect", shipment.JS_PaymentTermDisplay);
			}
		}

		readonly string[] prepaidIncos = new string[]
					{
						Core.Constants.IncoTerms.CostAndFreight,
						Core.Constants.IncoTerms.CostInsuranceAndFreight,
						Core.Constants.IncoTerms.CarriagePaidTo,
						Core.Constants.IncoTerms.CarriageAndInsurancePaidTo,
						Core.Constants.IncoTerms.DeliveredAtFrontier,
						Core.Constants.IncoTerms.DeliveredExShip,
						Core.Constants.IncoTerms.DeliveredExQuay,
						Core.Constants.IncoTerms.DeliveredDutyUnpaid,
						Core.Constants.IncoTerms.DeliveredDutyPaid,
						Core.Constants.IncoTerms.DeliveredAtPlace,
						Core.Constants.IncoTerms.DeliveredAtTerminal
					};

		readonly string[] collectIncos = new string[]
					{
						Core.Constants.IncoTerms.FreeCarrier,
						Core.Constants.IncoTerms.FreeAlongsideShip,
						Core.Constants.IncoTerms.FreeOnBoard
					};

		readonly string[] domesticPaymentTerms = new string[]
					{
						Core.Constants.DomesticPaymentTerms.Collect,
						Core.Constants.DomesticPaymentTerms.CollectCOD,
						Core.Constants.DomesticPaymentTerms.CollectThirdParty,
						Core.Constants.DomesticPaymentTerms.Prepaid
					};

		#endregion

		#region Test Chargeable Weight

		public void TestJS_ActualChargeable_UsesRegistrySettings()
		{
			FreightDataRegistry.Instance.DomesticChargeableFactorAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6250m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(166m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6060m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(170m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.DomesticChargeableFactorCourier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(5882m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(175m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.DomesticChargeableFactorSea.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres),
				new ConversionFactor(100m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.DomesticChargeableFactorRail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(1010m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(110m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = AlternateHomePort;
			AssertEquals("Shipment should be domestic", true, shipment.IsDomesticFreight);

			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualWeight = 800;
			shipment.JS_ActualVolume = 5;

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Chargeable (Air)", 800M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
			AssertEquals("Chargeable Unit (Air)", "KG", shipment.JS_ChargeableUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Chargeable (Road)", 825.08M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
			AssertEquals("Chargeable Unit (Road)", "KG", shipment.JS_ChargeableUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Chargeable (Courier)", 850.05M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
			AssertEquals("Chargeable Unit (Courier)", "KG", shipment.JS_ChargeableUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Chargeable (Sea)", 5M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
			AssertEquals("Chargeable Unit (Sea)", "M3", shipment.JS_ChargeableUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Chargeable (Rail)", 5M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
			AssertEquals("Chargeable Unit (Rail)", "M3", shipment.JS_ChargeableUnit);

			shipment.JS_ActualWeight = Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, Constants.Weight.Pounds);
			shipment.JS_ActualVolume = Constants.Volume.Convert(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume, Constants.Volume.CubicFeet);

			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Chargeable (Air)", 1838.061M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 3));
			AssertEquals("Chargeable Unit (Air)", "LB", shipment.JS_ChargeableUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Chargeable (Road)", 1794.813M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 3));
			AssertEquals("Chargeable Unit (Road)", "LB", shipment.JS_ChargeableUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Chargeable (Courier)", 1763.698M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 3));
			AssertEquals("Chargeable Unit (Courier)", "LB", shipment.JS_ChargeableUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Chargeable (Sea)", 176.573M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 3));
			AssertEquals("Chargeable Unit (Sea)", "CF", shipment.JS_ChargeableUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Chargeable (Rail)", 176.573M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 3));
			AssertEquals("Chargeable Unit (Rail)", "CF", shipment.JS_ChargeableUnit);
		}

		public void TestChargeableWeightRoad()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			shipment.JS_ActualVolume = 10;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			AssertEquals("Actual Chargeable for Road.", 3330m, Utilities.Round(shipment.JS_ActualChargeable, 2));

			ZDecimal actualChargeable = shipment.JS_ActualChargeable;
			shipment.JS_ActualWeight = -20;
			AssertEquals("Don't update Chargeable if Weight is negative.", actualChargeable, shipment.JS_ActualChargeable);
		}

		public void TestChargeableWeightAir()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 800;
			shipment.JS_ActualVolume = 5;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			AssertEquals("Chargeable Weight (M3)", 833.33M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));

			shipment.JS_UnitOfWeight = "XX";
			AssertEquals("Don't recalculate if Unit code invalid.", 833.33M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));

			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 2;
			AssertEquals("Chargeable Weight (KG)", 800M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));

			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_DocumentedWeight = 800;
			shipment.JS_DocumentedVolume = 5;
			AssertEquals("Documented Chargeable Weight (M3)", 833.33M, ZArchitecture.Core.Utilities.Round(shipment.JS_DocumentedChargeable, 2));
		}

		public void TestChargeableWeightSea()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ActualWeight = 800;
			shipment.JS_ActualVolume = 2;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			AssertEquals("Chargeable Weight (M3)", 2M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));

			shipment.JS_ActualWeight = 0;
			shipment.JS_ActualVolume = 10;
			AssertEquals("Standard Volume unit (M3)", 10m, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet; //1 CF = 0.0283168466 M3
			AssertEquals("Convert to Cubic Feet", 0.28m, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
		}

		public void TestChargeableWeightRail()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualWeight = 800;
			shipment.JS_ActualVolume = 2;
			AssertEquals("Chargeable Weight (M3)", 2M, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));

			shipment.JS_ActualWeight = 0;
			shipment.JS_ActualVolume = 10;
			AssertEquals("Standard Volume unit (M3)", 10m, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet; //1 CF = 0.0283168466 M3
			AssertEquals("Convert to Cubic Feet", 0.28m, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
		}

		public void TestCalculateVolumeFromChargeable()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualChargeable = 0;
			shipment.JS_ActualVolume = 0;
			shipment.JS_ActualWeight = 100;

			shipment.JS_ActualChargeable = 90;
			AssertEquals("Weight > Chargeable. Can't calculate volume.", 0m, shipment.JS_ActualVolume);

			shipment.JS_ActualChargeable = 120;
			AssertEquals("Weight < Chargeable. Volume = Chargeable / 166.666", 0.72m, shipment.JS_ActualVolume);

			shipment.JS_ActualChargeable = 150;
			AssertEquals("Volume already entered. Don't recalculate.", 0.72m, shipment.JS_ActualVolume);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ActualVolume = 0;
			shipment.JS_ActualChargeable = 1.5m;
			AssertEquals("Sea Freight. Don't calculate volume.", 0m, shipment.JS_ActualVolume);
		}

		public void TestCalculateWeightFromChargeable()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ActualChargeable = 0;
			shipment.JS_ActualWeight = 0;
			shipment.JS_ActualVolume = 1.5m;

			shipment.JS_ActualChargeable = 1.2m;
			AssertEquals("Volume > Chargeable. Can't calculate weight.", 0m, shipment.JS_ActualWeight);

			shipment.JS_ActualChargeable = 1.7m;
			AssertEquals("Volume < Chargeable. Weight = Chargeable * 1000", 1700m, shipment.JS_ActualWeight);

			shipment.JS_ActualChargeable = 1.9m;
			AssertEquals("Weight already entered. Don't recalculate.", 1700m, shipment.JS_ActualWeight);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 0;
			shipment.JS_ActualChargeable = 1500m;
			AssertEquals("Air Freight. Don't calculate weight.", 0m, shipment.JS_ActualWeight);
		}

		public void TestNoCircularStackWhenEnteringChargeable()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualVolume = 0;
			shipment.JS_ActualWeight = 0;
			shipment.JS_ActualChargeable = 1.234m;
			AssertEquals("Volume calculated from chargeable.", 0.007m, shipment.JS_ActualVolume);
			AssertEquals("Chargeable should not be recalculated.", 1.234m, shipment.JS_ActualChargeable);
		}

		public void TestChargeableWeight_UnitedStatesOrUSOverseasTerritories()
		{
			AssertDomesticFactorIsUsed("MPTIQ", "USLAX");
			AssertDomesticFactorIsUsed("USLAX", "VICHA");
			AssertDomesticFactorIsUsed("MPTIQ", "GUGUM");
			AssertDomesticFactorIsUsed("ASPPG", "PRCBJ");

			void AssertDomesticFactorIsUsed(ZString originPort, ZString destinationPort)
			{
				var shipment = GetShipment();
				AssertDomesticFactorPreSetting_UnitedStatesOrUSOverseasTerritories(shipment);

				shipment.JS_RL_NKOrigin = originPort;
				shipment.JS_RL_NKDestination = destinationPort;

				shipment.JS_ActualWeight = 2;
				shipment.JS_ActualWeight = 1;
				shipment.JS_ActualVolume = 400;
				AssertEquals("Is used", 1m, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualChargeable, 2));
			}
		}

		public void TestDontUpdateActualsFromChargeable_WhenSettingActualWeight()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			shipment.JS_ActualChargeable = 0;
			shipment.JS_ActualWeight = 7;

			AssertGreaterThan("PRE: Chargeable is calculated when Actual Weight is set.", shipment.JS_ActualChargeable, 0);
			AssertEquals("Actual Volume is not calculated from Chargeable when Actual Weight is being set.", 0m, shipment.JS_ActualVolume);
		}

		public void TestDontUpdateActualsFromChargeable_WhenSettingActualVolume()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.JS_ActualChargeable = 0;
			shipment.JS_ActualVolume = 7;

			AssertGreaterThan("PRE: Chargeable is calculated when Actual Volume is set.", shipment.JS_ActualChargeable, 0);
			AssertEquals("Actual Weight is not calculated from Chargeable when Actual Volume is being set.", 0m, shipment.JS_ActualWeight);
		}

		public void TestDontUpdateActualsFromChargeable_WhenSettingLoadingMeters()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Truck;

			shipment.JS_ActualVolume = 7;
			shipment.JS_ActualChargeable = 0;
			shipment.JS_LoadingMeters = 7;

			AssertGreaterThan("PRE: Chargeable is calculated when Loading Meters is set.", shipment.JS_ActualChargeable, 0);
			AssertEquals("Actual Weight is not calculated from Chargeable when Loading Meters is being set.", 0m, shipment.JS_ActualWeight);
		}

		public void TestDontUpdateActualsFromChargeable_WhenSettingActualWeightUnit()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			shipment.JS_ActualChargeable = 0;
			shipment.JS_ActualWeight = 10;
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;

			AssertGreaterThan("PRE: Chargeable is calculated when Unit Of Weight is set.", shipment.JS_ActualChargeable, 0);
			AssertEquals("Actual Volume is not calculated from Chargeable when Unit Of Weight is being set.", 0m, shipment.JS_ActualVolume);
		}

		public void TestUpdateActualsFromChargeable_UnitedStatesOrUSOverseasTerritories()
		{
			AssertDomesticFactorIsUsed("MPTIQ", "USLAX");
			AssertDomesticFactorIsUsed("USLAX", "VICHA");
			AssertDomesticFactorIsUsed("MPTIQ", "GUGUM");
			AssertDomesticFactorIsUsed("ASPPG", "PRCBJ");

			void AssertDomesticFactorIsUsed(ZString originPort, ZString destinationPort)
			{
				var shipment = GetShipment();
				AssertDomesticFactorPreSetting_UnitedStatesOrUSOverseasTerritories(shipment);

				shipment.JS_RL_NKOrigin = originPort;
				shipment.JS_RL_NKDestination = destinationPort;

				shipment.JS_ActualWeight = 2;
				shipment.JS_ActualWeight = 1;
				shipment.JS_ActualChargeable = 100;
				AssertEquals("Is used", 40000m, ZArchitecture.Core.Utilities.Round(shipment.JS_ActualVolume, 2));
			}
		}

		public void TestJS_Calc_ActualVolumeWeight_UnitedStatesOrUSOverseasTerritories()
		{
			AssertDomesticFactorIsUsed("MPTIQ", "USLAX");
			AssertDomesticFactorIsUsed("USLAX", "VICHA");
			AssertDomesticFactorIsUsed("MPTIQ", "GUGUM");
			AssertDomesticFactorIsUsed("ASPPG", "PRCBJ");

			void AssertDomesticFactorIsUsed(ZString originPort, ZString destinationPort)
			{
				var shipment = GetShipment();
				AssertDomesticFactorPreSetting_UnitedStatesOrUSOverseasTerritories(shipment);

				shipment.JS_RL_NKOrigin = originPort;
				shipment.JS_RL_NKDestination = destinationPort;

				shipment.JS_ActualWeight = 2;
				shipment.JS_ActualWeight = 1;
				shipment.JS_ActualVolume = 400;
				AssertEquals("Is used", 1m, ZArchitecture.Core.Utilities.Round(shipment.JS_Calc_ActualVolumeWeight, 2));
			}
		}

		void AssertDomesticFactorPreSetting_UnitedStatesOrUSOverseasTerritories(CommonShipment shipment)
		{
			FreightDataRegistry.Instance.InternationalChargeableFactorAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6000m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(200m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.DomesticChargeableFactorAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6000m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(400m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicInches;
		}

		public void TestJS_Calc_ActualVolumeWeightAndUnit_InvalidConversionsReturnZero()
		{
			var rebelScumFactor = ConversionFactor.Standard.Metric.LoadingMeters;
			var imperialFactor = ConversionFactor.Standard.Imperial.Domestic;
			var factor = new ChargeableFactor(rebelScumFactor, imperialFactor);

			using (FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, factor))
			using (FreightDataRegistry.Instance.DomesticChargeableFactorRail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, factor))
			{
				Factory.Save();

				var shipment = GetShipment();
				shipment.JS_RL_NKOrigin = "AUMEL";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
				shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
				shipment.JS_ActualWeight = 100m;
				shipment.JS_ActualVolume = 1m;
				shipment.JS_TransportMode = Constants.TransportModes.Air;

				AssertEquals("Pre-condition: Air Chargeable Unit", Constants.Weight.Kilograms, shipment.JS_ChargeableUnit);
				AssertEquals("Pre-condition: Air Actual Unit", Constants.Weight.Kilograms, shipment.JS_Calc_ActualVolumeWeightUnit);
				AssertEquals("Expected actual volume converted to weight via registry default conversion factor", 167m, shipment.JS_Calc_ActualVolumeWeight, 1m);

				shipment.JS_TransportMode = Constants.TransportModes.Sea;

				AssertEquals("Pre-condition: Sea Chargeable Unit", Constants.Volume.CubicMetres, shipment.JS_ChargeableUnit);
				AssertEquals("Pre-condition: Sea Actual Unit", Constants.Volume.CubicMetres, shipment.JS_Calc_ActualVolumeWeightUnit);
				AssertEquals("Expected actual weight converted to volume via registry default conversion factor", 0.1m, shipment.JS_Calc_ActualVolumeWeight, 1m);

				shipment.JS_TransportMode = Constants.TransportModes.Road;

				AssertEquals("Pre-condition: Road Chargeable Unit", Constants.Weight.Kilograms, shipment.JS_ChargeableUnit);
				AssertEquals("Pre-condition: Road Actual Unit", Constants.Weight.Kilograms, shipment.JS_Calc_ActualVolumeWeightUnit);
				AssertEquals("No conversion path between M3 to KG/LM so 0 is expected", 0m, shipment.JS_Calc_ActualVolumeWeight);

				shipment.JS_TransportMode = Constants.TransportModes.Rail;

				AssertEquals("Pre-condition: Rail Chargeable Unit", Constants.Volume.CubicMetres, shipment.JS_ChargeableUnit);
				AssertEquals("Pre-condition: Rail Actual Unit", Constants.Volume.CubicMetres, shipment.JS_Calc_ActualVolumeWeightUnit);
				AssertEquals("Again, cannot convert due to missing conversion path.", 0m, shipment.JS_Calc_ActualVolumeWeight);

				shipment.JS_UnitOfWeight = "OH";
				shipment.JS_UnitOfVolume = "NO";
				shipment.JS_TransportMode = Constants.TransportModes.Air;

				AssertEquals("Pre-condition: no unit", "", shipment.JS_Calc_ActualVolumeWeightUnit);
				AssertEquals("Cannot convert with invalid unit", 0m, shipment.JS_Calc_ActualVolumeWeight);

				shipment.JS_TransportMode = Constants.TransportModes.Sea;

				AssertEquals("Pre-condition: no unit", "", shipment.JS_Calc_ActualVolumeWeightUnit);
				AssertEquals("Stil cannot convert with invalid unit", 0m, shipment.JS_Calc_ActualVolumeWeight);
			}
		}

		#endregion

		#region Test JS_Calc_ActualVolumeWeight

		public void TestJS_Calc_ActualVolumeWeight()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = AlternateHomePort;

			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 300m;
			shipment.JS_ActualVolume = 0.308m;

			AssertEquals("Weight Volume: convert actual volume to weight", 51.333333m, shipment.JS_Calc_ActualVolumeWeight);
			AssertEquals("Chargeable Unit", "KG", shipment.JS_Calc_ActualVolumeWeightUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ActualVolume = 15m;
			shipment.JS_ActualWeight = 5553m;

			AssertEquals("Volume Weight: convert actual weight to volume", 5.553m, shipment.JS_Calc_ActualVolumeWeight);
			AssertEquals("Chargeable Unit", "M3", shipment.JS_Calc_ActualVolumeWeightUnit);

			shipment.JS_UnitOfVolume = ZString.Empty;
			AssertEquals("When there's no unit we cannot calculate chargeable", 0m, shipment.JS_Calc_ActualVolumeWeight);
		}

		public void TestJS_Calc_ActualVolumeWeight_ChangesWithDependencies()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = AlternateHomePort;

			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 300m;
			shipment.JS_ActualVolume = 0.308m;

			var valueChangedCount = 0;
			var unitChangedCount = 0;

			shipment.JS_Calc_ActualVolumeWeightInfo.ValueChanged += (sender, e) => valueChangedCount++;
			shipment.JS_Calc_ActualVolumeWeightUnitInfo.ValueChanged += (sender, e) => unitChangedCount++;

			shipment.JS_ActualWeight = 500m;

			AssertEquals("JS_Calc_ActualVolumeWeight should have changed", 1, valueChangedCount);
			AssertEquals("JS_Calc_ActualVolumeWeightUnit should have changed", 1, unitChangedCount);

			shipment.JS_ActualVolume = 0.108m;

			AssertEquals("JS_Calc_ActualVolumeWeight should have changed", 2, valueChangedCount);
			AssertEquals("JS_Calc_ActualVolumeWeightUnit should have changed", 2, unitChangedCount);

			shipment.JS_UnitOfWeight = Constants.Weight.Grams;

			AssertEquals("JS_Calc_ActualVolumeWeight should have changed", 3, valueChangedCount);
			AssertEquals("JS_Calc_ActualVolumeWeightUnit should have changed", 3, unitChangedCount);

			shipment.JS_UnitOfVolume = Constants.Volume.CubicCentimeters;

			AssertEquals("JS_Calc_ActualVolumeWeight should have changed", 4, valueChangedCount);
			AssertEquals("JS_Calc_ActualVolumeWeightUnit should have changed", 4, unitChangedCount);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("JS_Calc_ActualVolumeWeight should have changed", 5, valueChangedCount);
			AssertEquals("JS_Calc_ActualVolumeWeightUnit should have changed", 5, unitChangedCount);

			shipment.JS_RL_NKOrigin = USPort;

			AssertEquals("JS_Calc_ActualVolumeWeight should have changed", 6, valueChangedCount);
			AssertEquals("JS_Calc_ActualVolumeWeightUnit should have changed", 6, unitChangedCount);

			shipment.JS_RL_NKDestination = USPortAlt;

			AssertEquals("JS_Calc_ActualVolumeWeight should have changed", 7, valueChangedCount);
			AssertEquals("JS_Calc_ActualVolumeWeightUnit should have changed", 7, unitChangedCount);
		}

		#endregion

		#region Test JS_Calc_ExcessActualVolumeWeight and JS_Calc_ExcessChargeableVolumeWeight

		public void TestJS_Calc_ExcessActual_and_ChargeableVolumeWeight()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 300m;
			shipment.JS_ActualVolume = 0.308m;

			AssertEquals("Excess actual weight", 248.666667m, shipment.JS_Calc_ExcessActualVolumeWeight);
			AssertEquals("Excess chargeable weight", 0m, shipment.JS_Calc_ExcessChargeableVolumeWeight);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualVolume = 6.912m;
			shipment.JS_ActualWeight = 1072m;

			AssertEquals("Volume Weight should be correct", 1152m, shipment.JS_Calc_ActualVolumeWeight);

			AssertEquals("Excess actual weight", 0m, shipment.JS_Calc_ExcessActualVolumeWeight);
			AssertEquals("Excess chargeable weight", 80m, shipment.JS_Calc_ExcessChargeableVolumeWeight);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ActualVolume = 5m;
			shipment.JS_ActualWeight = 5500m;

			AssertEquals("Excess actual volume", 0m, shipment.JS_Calc_ExcessActualVolumeWeight);
			AssertEquals("Excess chargeable volume", 0.5m, shipment.JS_Calc_ExcessChargeableVolumeWeight);

			shipment.JS_ActualVolume = 5.5m;
			shipment.JS_ActualWeight = 2100m;

			AssertEquals("Excess actual volume", 3.4m, shipment.JS_Calc_ExcessActualVolumeWeight);
			AssertEquals("Excess chargeable volume", 0m, shipment.JS_Calc_ExcessChargeableVolumeWeight);
		}

		public void TestJS_Calc_ExcessActualVolumeWeightUnit()
		{
			var shipment = Factory.New<CommonShipment>();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Excess Actual Volume Weight Unit", "KG", shipment.JS_Calc_ExcessActualVolumeWeightUnit);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Excess Actual Volume Weight Unit", "M3", shipment.JS_Calc_ExcessActualVolumeWeightUnit);
		}

		#endregion

		#region Documented/Manifested Weight/Volume/LoadingMeters/Chargeable

		public void TestDocumentedWeight()
		{
			AssertRelatedMeasure(JobShipmentSchema.JS_ActualWeight.Name, JobShipmentSchema.JS_DocumentedWeight.Name, JobShipmentSchema.JS_DocumentedChargeable.Name);
		}

		public void TestDocumentedVolume()
		{
			AssertRelatedMeasure(JobShipmentSchema.JS_ActualVolume.Name, JobShipmentSchema.JS_DocumentedVolume.Name, JobShipmentSchema.JS_DocumentedChargeable.Name);
		}

		public void TestDocumentedLoadingMeters()
		{
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition: loading meters enabled", true, FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value);

			AssertRelatedMeasure(JobShipmentSchema.JS_LoadingMeters.Name, JobShipmentSchema.JS_DocumentedLoadingMeters.Name, JobShipmentSchema.JS_DocumentedChargeable.Name);
		}

		public void TestManifestedWeight()
		{
			AssertRelatedMeasure(JobShipmentSchema.JS_ActualWeight.Name, JobShipmentSchema.JS_ManifestedWeight.Name, JobShipmentSchema.JS_ManifestedChargeable.Name);
		}

		public void TestManifestedVolume()
		{
			AssertRelatedMeasure(JobShipmentSchema.JS_ActualVolume.Name, JobShipmentSchema.JS_ManifestedVolume.Name, JobShipmentSchema.JS_ManifestedChargeable.Name);
		}

		public void TestManifestedLoadingMeters()
		{
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition: loading meters enabled", true, FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value);

			AssertRelatedMeasure(JobShipmentSchema.JS_LoadingMeters.Name, JobShipmentSchema.JS_ManifestedLoadingMeters.Name, JobShipmentSchema.JS_ManifestedChargeable.Name);
		}

		void AssertRelatedMeasure(string actualMeasureName, string relatedMeasureName, string relatedChargeableName)
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = relatedMeasureName.Contains("LoadingMeters") ? Constants.TransportModes.Road : Constants.TransportModes.Air;

			AssertEquals("Actual Value", 0m, shipment[actualMeasureName]);
			AssertEquals("Related Value", 0m, shipment[relatedMeasureName]);

			shipment[actualMeasureName] = 1m;
			AssertEquals("Actual Value", 1m, shipment[actualMeasureName]);
			AssertEquals("Related Value", 1m, shipment[relatedMeasureName]);

			shipment[actualMeasureName] = 2m;
			AssertEquals("Actual Value", 2m, shipment[actualMeasureName]);
			AssertEquals("Related Value", 2m, shipment[relatedMeasureName]);
			AssertEquals("Related chargeable should == Actual", shipment.JS_ActualChargeable, shipment[relatedChargeableName]);

			shipment[relatedMeasureName] = 3m;
			AssertEquals("Actual Value", 2m, shipment[actualMeasureName]);
			AssertEquals("Related Value", 3m, shipment[relatedMeasureName]);
			AssertNotEquals("Related chargeable should be updated", shipment.JS_ActualChargeable, shipment[relatedChargeableName]);

			ZDecimal previousRelatedChargeable = new ZDecimal(shipment[relatedChargeableName]);
			shipment[actualMeasureName] = 4m;
			AssertEquals("Actual Value", 4m, shipment[actualMeasureName]);
			AssertEquals("Related Value", 3m, shipment[relatedMeasureName]);
			AssertEquals("Related chargeable should not change", previousRelatedChargeable, shipment[relatedChargeableName]);

			shipment[relatedMeasureName] = 4m;
			AssertEquals("Actual Value", 4m, shipment[actualMeasureName]);
			AssertEquals("Related Value", 4m, shipment[relatedMeasureName]);
			AssertEquals("Related chargeable should == Actual", shipment.JS_ActualChargeable, shipment[relatedChargeableName]);

			shipment[actualMeasureName] = 5m;
			AssertEquals("Actual Value", 5m, shipment[actualMeasureName]);
			AssertEquals("Related Value", 5m, shipment[relatedMeasureName]);
			AssertEquals("Related chargeable should == Actual", shipment.JS_ActualChargeable, shipment[relatedChargeableName]);
		}

		public void TestDocumentedChargeable()
		{
			AssertRelatedChargeable(JobShipmentSchema.JS_DocumentedChargeable.Name);
		}

		public void TestManifestedChargeable()
		{
			AssertRelatedChargeable(JobShipmentSchema.JS_ManifestedChargeable.Name);
		}

		void AssertRelatedChargeable(string relatedChargeableName)
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Actual chargeable", 0m, shipment.JS_ActualChargeable);
			AssertEquals("Related chargeable", 0m, shipment[relatedChargeableName]);

			shipment.JS_ActualVolume = 1m;
			AssertEquals("Actual chargeable", 1m, shipment.JS_ActualChargeable);
			AssertEquals("Related chargeable", 1m, shipment[relatedChargeableName]);

			shipment.JS_ActualWeight = 2000m;
			AssertEquals("Actual chargeable", 2m, shipment.JS_ActualChargeable);
			AssertEquals("Related chargeable", 2m, shipment[relatedChargeableName]);

			shipment.JS_ActualChargeable = 1.8m;
			AssertEquals("Actual chargeable", 1.8m, shipment.JS_ActualChargeable);
			AssertEquals("Related chargeable", 1.8m, shipment[relatedChargeableName]);

			shipment[relatedChargeableName] = 1.9m;
			AssertEquals("Actual chargeable", 1.8m, shipment.JS_ActualChargeable);
			AssertEquals("Related chargeable", 1.9m, shipment[relatedChargeableName]);

			shipment.JS_ActualChargeable = 1.85m;
			AssertEquals("Actual chargeable", 1.85m, shipment.JS_ActualChargeable);
			AssertEquals("Related chargeable", 1.9m, shipment[relatedChargeableName]);

			shipment[relatedChargeableName] = 1.85m;
			shipment.JS_ActualChargeable = 1.95m;
			AssertEquals("Actual chargeable", 1.95m, shipment.JS_ActualChargeable);
			AssertEquals("Related chargeable", 1.95m, shipment[relatedChargeableName]);
		}

		#endregion

		#region Active Filter

		public void TestActiveFilter()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			shipment.JS_IsCancelled = true;
			CommonShipment[] retrievedShipments = (CommonShipment[])Factory.Load(typeof(CommonShipment), new ZQuery(JobShipmentSchema.PK, shipment.PK));
			AssertEquals("Should not be able to load inactive declaration", 0, retrievedShipments.Length);

			shipment.JS_IsCancelled = false;
			retrievedShipments = (CommonShipment[])Factory.Load(typeof(CommonShipment), new ZQuery(JobShipmentSchema.PK, shipment.PK));
			AssertEquals("Should be able to load active declaration", 1, retrievedShipments.Length);
		}

		#endregion

		#region IDocsAndCartageParent

		public void TestBlankJobDocsAndCartageIsCreatedForNewShipment()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			AssertEquals("Precondition - CommonShipment foreign key to JobDocsAndCartage is empty", false, shipment.IsDocsAndCartageSet);
			AssertNotNull("A new JobDocsAndCartage should have been created", shipment.DocsAndCartage);
		}

		public void TestJobDocsAndCartageIsReloaded()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			AssertNotNull("Touch JobDocsAndCartage so it's created", shipment.DocsAndCartage);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CommonShipment loadedShipment = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("The correct JobDocsAndCartage should have been reloaded", shipment.DocsAndCartage.PK, loadedShipment.DocsAndCartage.PK);
		}

		public void TestShipmentKeepsJobDocsAndCartageAfterSave()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			JobDocsAndCartage jDC = JobDocsAndCartage.Load(shipment);
			AssertNull("Precondition - foreign key to JobDocsAndCartage is empty", jDC);
			Factory.Save();
			AssertNotNull("Touch JobDocsAndCartage so it loads.", shipment.DocsAndCartage);
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CommonShipment loadedShipment = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("Foreign key to JobDocsAndCartage was not saved correctly.", shipment.DocsAndCartage.PK, loadedShipment.DocsAndCartage.PK);
		}

		#endregion

		#region IHaveInternalCartage Tests

		public void TestInternalCartageServiceLevel()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_RS_NKServiceLevel = "XXX";

			ZString serviceLevel = ((IHaveInternalCartage)shipment).ServiceLevel;
			AssertEquals("The correct Service Level should be available through interface.", "XXX", serviceLevel);
			AssertEquals("The correct Service Level should be available through interface.", shipment.JS_RS_NKServiceLevel, serviceLevel);
		}

		#endregion

		#region IManifestProvider

		public void TestIManifestProvider_CustomsManifestVisibilityChanged()
		{
			bool visibilityChangedFired = false;
			EventHandler handler = (sender, args) => visibilityChangedFired = true;

			CommonShipment shipment = GetShipment();
			IManifestProvider manifestProvider = shipment;
			manifestProvider.CustomsManifestVisibilityChanged += handler;

			AssertEquals("Visibility shouldn't have changed initially", false, visibilityChangedFired);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Visibility should change for transport mode", true, visibilityChangedFired);
			visibilityChangedFired = false;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("Visibility should change for coload", true, visibilityChangedFired);
			visibilityChangedFired = false;
			CommonConsol consol = shipment.Consols.AddNew();
			AssertEquals("Visibility should change when adding consol", true, visibilityChangedFired);
			visibilityChangedFired = false;
			shipment.JS_HouseBill = "POOPY";
			AssertEquals("Visibility should change when changing house number", true, visibilityChangedFired);
			visibilityChangedFired = false;

			manifestProvider.CustomsManifestVisibilityChanged -= handler;

			visibilityChangedFired = false;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_HouseBill = "GLOOPY";
			shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "YYYYY";
			AssertEquals("Visibility should NOT change for any reason after the event handler is unhooked", false, visibilityChangedFired);
		}

		public void TestIManifestProvider_CustomsManifestVisibilityChanged_ConsolsNotLoadedOnHook()
		{
			CommonShipment shipment = GetShipment();
			IManifestProvider manifestProvider = shipment;
			manifestProvider.CustomsManifestVisibilityChanged += (sender, args) => { };
			AssertNull("Consols not loaded just for the sake of hooking CustomsManifestVisibilityChanged", typeof(CommonShipment).InvokeMember("fConsols", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, shipment, null));
		}

		#endregion

		#region IImportExport

		public void TestImportExportFromImportDepartment()
		{
			SetDepartment(true, false, false);
			AssertShipmentDirection("", "", true, false, false, false);
			AssertShipmentDirection(AlternateHomePort, "", false, true, false, false);
			AssertShipmentDirection("", AlternateHomePort, true, false, false, false);
			AssertShipmentDirection(OverseasPort, "", true, false, false, false);
			AssertShipmentDirection("", OverseasPort, false, true, false, false);
			AssertShipmentDirection(AlternateHomePort, HomePort, false, false, true, false);
			AssertShipmentDirection(OverseasPort, OverseasPort2, false, false, false, true);
			AssertShipmentDirection(OverseasPort, AlternateHomePort, true, false, false, false);
			AssertShipmentDirection(AlternateHomePort, OverseasPort, false, true, false, false);
		}

		public void TestImportExportFromExportDepartment()
		{
			SetDepartment(false, true, false);
			AssertShipmentDirection("", "", false, true, false, false);
			AssertShipmentDirection(AlternateHomePort, "", false, true, false, false);
			AssertShipmentDirection("", AlternateHomePort, true, false, false, false);
			AssertShipmentDirection(OverseasPort, "", true, false, false, false);
			AssertShipmentDirection("", OverseasPort, false, true, false, false);
			AssertShipmentDirection(AlternateHomePort, HomePort, false, false, true, false);
			AssertShipmentDirection(OverseasPort, OverseasPort2, false, false, false, true);
			AssertShipmentDirection(OverseasPort, AlternateHomePort, true, false, false, false);
			AssertShipmentDirection(AlternateHomePort, OverseasPort, false, true, false, false);
		}

		public void TestImportExportFromDomesticDepartment()
		{
			SetDepartment(false, false, true);
			AssertShipmentDirection("", "", false, false, true, false);
			AssertShipmentDirection(AlternateHomePort, "", false, false, true, false);
			AssertShipmentDirection("", AlternateHomePort, false, false, true, false);
			AssertShipmentDirection(OverseasPort, "", true, false, false, false);
			AssertShipmentDirection("", OverseasPort, false, true, false, false);
			AssertShipmentDirection(AlternateHomePort, HomePort, false, false, true, false);
			AssertShipmentDirection(OverseasPort, OverseasPort2, false, false, false, true);
			AssertShipmentDirection(OverseasPort, AlternateHomePort, true, false, false, false);
			AssertShipmentDirection(AlternateHomePort, OverseasPort, false, true, false, false);
		}

		#endregion

		#region TestDeletingShipmentWithoutDeletingDocs

		[ExpectNoExceptions]
		public void TestDeletingShipmentWithoutDeletingDocs()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			JobDocsAndCartage docsAndCartage = shipment.DocsAndCartage;
			JobRequiredDocument doc = shipment.DocsAndCartage.RequiredDocuments.AddNew();
			doc.EQ_DocType = "MSC";
			consol.Shipments.Remove(shipment);
			Factory.Save();
		}

		#endregion

		#region TestIsAllowedToUpdateAdviseDates

		public void TestIsAllowedToUpdateAdviseDates()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			Assert(((IHaveInternalCartage)shipment).IsAllowedToUpdateAdviseDates);
		}

		#endregion

		#region TestGetCanOverrideCheckpoint

		public void TestGetCanOverrideCheckpoint()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			AssertEquals("Prequisite", true, shipment.IsImport());

			SecurityCheckpoint checkpoint = ((IDocAddresses)shipment).GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress);
			AssertEquals(Env.Security.None, checkpoint);

			checkpoint = ((IDocAddresses)shipment).GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress);
			AssertEquals(Env.Security.None, checkpoint);

			checkpoint = ((IDocAddresses)shipment).GetCanOverrideCheckpoint(shipment.ConsignorPickupAddress);
			AssertEquals(Env.Security.None, checkpoint);
		}

		#endregion

		#region OwnerRef

		public void TestOwnerRef()
		{
			var shipment = CommonShipment.New(Factory);
			AssertEquals(true, ((IHaveInternalCartage)shipment).OwnerRef.IsEmpty);
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OwnerRef = "Owner Ref";
			AssertEquals(true, ((IHaveInternalCartage)shipment).OwnerRef.IsEmpty);
			shipment.DocsAndCartage.JP_OrderItemsAsString = "Order Ref";
			AssertEquals("Order,Ref", ((IHaveInternalCartage)shipment).OwnerRef);
		}

		#endregion

		#region TestGetRegistryDefaultContainerMode

		public void TestGetRegistryDefaultContainerMode()
		{
			var testShipment = GetShipment();
			var defaultContainerMode = testShipment.GetRegistryDefaultContainerMode();
			AssertEquals("Default Container Mode should not exist", "", defaultContainerMode);

			var setupCollection = new DefaultContainerModesCollection();
			var registryItem1 = new DefaultContainerModes();
			registryItem1.TransportMode = Constants.TransportModes.Air;
			registryItem1.ContainerMode = Constants.ContainerModes.ULD;
			setupCollection.Add(registryItem1);

			var registryItem2 = new DefaultContainerModes();
			registryItem2.TransportMode = Constants.TransportModes.Road;
			registryItem2.ContainerMode = Constants.ContainerModes.FTL;
			setupCollection.Add(registryItem2);
			AssertEquals("Should now be 2 default Container modes", 2, setupCollection.Count);

			FreightConfigurationRegistry.Instance.DefaultContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, setupCollection);
			var value = FreightConfigurationRegistry.Instance.DefaultContainerModes.Value;
			AssertEquals("Value.Count", 2, value.Count);

			testShipment.JS_TransportMode = Constants.TransportModes.Road;
			defaultContainerMode = testShipment.GetRegistryDefaultContainerMode();
			AssertEquals("Default Container Mode of FTL should now be returned", Constants.ContainerModes.FTL, defaultContainerMode);

			testShipment.JS_TransportMode = Constants.TransportModes.Sea;
			defaultContainerMode = testShipment.GetRegistryDefaultContainerMode();
			AssertEquals("Expecting no registry default Container mode value for Sea", "", defaultContainerMode);

			testShipment.JS_TransportMode = Constants.TransportModes.Air;
			defaultContainerMode = testShipment.GetRegistryDefaultContainerMode();
			AssertEquals("Expecting default value of ULD for Air", Constants.ContainerModes.ULD, defaultContainerMode);
		}

		#endregion

		#region TestCannotDeleteExceptionOnDeletingShipment

		[ExpectNoExceptions]
		public void TestNoExceptionOnDeletingAnUnsavedForwardingShipment()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_IsCFSRegistered = false;
			shipment.JS_IsBooking = false;
			shipment.Delete();
		}

		[ExpectNoExceptions]
		public void TestNoExceptionOnDeletingAnUnsavedCFSShipment()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_IsBooking = false;
			shipment.Delete();
		}

		[ExpectNoExceptions]
		public void TestExceptionOnDeletingANonconsolidatedBookingShipment()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsBooking = true;
			Factory.Save();
			shipment.Delete();
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestExceptionOnDeletingAConsolidatedBookingShipment()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_IsBooking = true;
			Factory.Save();
			shipment.Delete();
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestExceptionOnDeletingAForwardingShipment()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_IsBooking = false;
			shipment.JS_IsCFSRegistered = false;
			Factory.Save();
			shipment.Delete();
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestExceptionOnDeletingACFSShipment()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsBooking = false;
			shipment.JS_IsCFSRegistered = true;
			Factory.Save();
			shipment.Delete();
		}

		#endregion

		#region Synchronising With Milestone Events

		public void TestOnETDChange_UpdateMilestone()
		{
			TestOnDepartureArrivalDateChange_UpdateMilestone(JobShipmentSchema.JS_E_DEP, Events.Departure);
		}

		public void TestOnETAChange_UpdateMilestone()
		{
			TestOnDepartureArrivalDateChange_UpdateMilestone(JobShipmentSchema.JS_E_ARV, Events.Arrival);
		}

		void TestOnDepartureArrivalDateChange_UpdateMilestone(SchemaDateTimeColumn departureOrArrivalProperty, Event arrivalOrDepartureEvent)
		{
			CommonShipment shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			IWorkflowProvider workflowProvider = (IWorkflowProvider)shipment;
			ProcessTask milestone = workflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = arrivalOrDepartureEvent.Code;

			shipment[departureOrArrivalProperty] = new ZDateTime(2005, 1, 2);
			AssertEquals("P9_ScheduledDate set", milestone.P9_ScheduledDate.ToZDateTime(), new ZDateTime(2005, 1, 2));
		}

		#endregion

		#region IBillGenerationSupport

		public void TestIBillGenerationSupport_Default()
		{
			CommonShipment shipment = GetShipment();
			IBillGenerationSupport support = shipment;
			AssertEquals("Transhipment Indicator is not used at this level.", "", support.TranshipmentIndicator);
			AssertNull("Default carrier principal", support.CarrierPrincipal);
			AssertNull("Default Destination", support.Destination);
			AssertNull("Default Origin", support.Origin);
			Assert("Default TransportMode", support.TransportMode.IsEmpty);
			AssertNull("Default Load", support.Load);
			AssertNull("Default Discharge", support.Discharge);
		}

		public void TestIBillGeneratinSupport_CarrierPrincipal()
		{
			CommonShipment shipment = GetShipment();
			IBillGenerationSupport support = shipment;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OA_BookedShippingLineAddress = org1.MainAddress.PK;
			AssertEquals("CarrierPrincipal for standalone Shipment", support.CarrierPrincipal, org1);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_OA_ShippingLineAddress = org2.MainAddress.PK;
			AssertEquals("CarrierPrincipal for consolidated Shipment", support.CarrierPrincipal, org2);
		}

		#endregion

		#region Cartage and ACI Zone Tests

		RefDomesticCartageZone CreateRefDomesticZone(ZString postCode, ZString loco, ZString zone, ZString city)
		{
			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, loco);
			var iata = unloco != null ? unloco.RL_IATA : ZString.Empty;

			var refDomesticCartageZone = Factory.New<RefDomesticCartageZone>();
			refDomesticCartageZone.F1_CityTownPostCode = postCode;
			refDomesticCartageZone.F1_RL_NKLoco = loco;
			refDomesticCartageZone.F1_Zone = zone;
			refDomesticCartageZone.F1_CityTown = city;
			refDomesticCartageZone.F1_PortCode = iata;

			return refDomesticCartageZone;
		}

		public void TestCartageAndACIZoneVisibility()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = TransportProvider1.MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = TransportProvider1.MainAddress.PK;
			shipment.IsDomesticFreight = false;

			shipment.ConsignorPickupAddress.E2_Postcode = "4300";
			shipment.ConsignorPickupAddress.E2_RN_NKCountryCode = "AU";

			shipment.ConsigneeDeliveryAddress.E2_Postcode = "4110";
			shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "AU";

			// ACI Zone information...
			CreateRefDomesticZone("4300", "AUBNE", "AFR", "");
			CreateRefDomesticZone("4110", "AUSYD", "EUO", "");
			CreateRefDomesticZone("4300", "AUSGO", "EUR", "");
			CreateRefDomesticZone("4110", "AUSHB", "IND", "");
			CreateRefDomesticZone("4200", "AUSGO", "EUO", "");
			CreateRefDomesticZone("4220", "AUSHB", "AFR", "");
			CreateRefDomesticZone("4110", "AUSHB", "CTY", "city1");
			Factory.Save();

			AssertEquals("Consignor PostCode", "4300", ((IDocAddress)shipment.ConsignorPickupAddress).E2_Postcode);
			AssertEquals("Consignor City", ZString.Empty, ((IDocAddress)shipment.ConsignorPickupAddress).E2_City);
			AssertEquals("Consignor PortCode", ZString.Empty, ((IDocAddress)shipment.ConsignorPickupAddress).E2_PortCode);

			AssertEquals("Consignee PostCode", "4110", ((IDocAddress)shipment.ConsigneeDeliveryAddress).E2_Postcode);
			AssertEquals("Consignee City", ZString.Empty, ((IDocAddress)shipment.ConsigneeDeliveryAddress).E2_City);
			AssertEquals("Consignee PortCode", ZString.Empty, ((IDocAddress)shipment.ConsigneeDeliveryAddress).E2_PortCode);

			AssertEquals("ACI Consignor Zone should be empty", ZString.Empty, shipment.JS_Calc_ACIConsignorOriginZone);
			AssertEquals("ACI Consignee Zone should be empty", ZString.Empty, shipment.JS_Calc_ACIConsigneeDestinationZone);
			AssertEquals("Servicing Postal Filter should not be added", false, shipment.Lookups.RefUNLOCO_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Servicing Postal Code:Property"));

			shipment.IsDomesticFreight = true;

			AssertEquals("ACI Consignor Zone should be empty", ZString.Empty, shipment.JS_Calc_ACIConsignorOriginZone);
			AssertEquals("ACI Consignee Zone should be empty", ZString.Empty, shipment.JS_Calc_ACIConsigneeDestinationZone);
			AssertEquals("Servicing Postal Filter should not be added", false, shipment.Lookups.RefUNLOCO_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Servicing Postal Code:Property"));

			CreateRateTransportZone();   // Cartage Zone information
			AssertEquals("Deliver Cartage Zone should be Region1", "Region1", shipment.JS_Calc_DeliveryCartageZone);
			AssertEquals("Pickup Cartage Zone should be Region2", "Region2", shipment.JS_Calc_PickupCartageZone);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);

			AssertEquals("ACI Consignor Zone should be AFR", "AFR", shipment.JS_Calc_ACIConsignorOriginZone);
			AssertEquals("ACI Consignee Zone should be EUO", "EUO", shipment.JS_Calc_ACIConsigneeDestinationZone);
			AssertEquals("Servicing Postal Filter should be added", true, shipment.Lookups.RefUNLOCO_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Servicing Postal Code:Property"));

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

			AssertEquals("ACI Consignor Zone should be AFR", "AFR", shipment.JS_Calc_ACIConsignorOriginZone);
			AssertEquals("ACI Consignee Zone should be EUO", "EUO", shipment.JS_Calc_ACIConsigneeDestinationZone);
			AssertEquals("Servicing Postal Filter should be added", true, shipment.Lookups.RefUNLOCO_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Servicing Postal Code:Property"));

			shipment.JS_RL_NKOrigin = "AUSGO";
			AssertEquals("ACI Consignor Zone should be EUR", "EUR", shipment.JS_Calc_ACIConsignorOriginZone);

			var deliveryAddress = Factory.New<OrgAddress>();
			deliveryAddress.OA_PostCode = "4110";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSHB";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			AssertEquals("ACI Consignee Zone should be IND", "IND", shipment.JS_Calc_ACIConsigneeDestinationZone);

			deliveryAddress.OA_City = "city1";
			AssertEquals("ACI Consignee Zone should be CTY", "CTY", shipment.JS_Calc_ACIConsigneeDestinationZone);

			shipment.ConsignorPickupAddress.E2_Postcode = "4200";
			AssertEquals("ACI Consignor Zone should be EUO", "EUO", shipment.JS_Calc_ACIConsignorOriginZone);

			deliveryAddress.OA_PostCode = "4220";
			AssertEquals("ACI Consignee Zone should be AFR", "AFR", shipment.JS_Calc_ACIConsigneeDestinationZone);

			shipment.IsDomesticFreight = false;

			AssertEquals("ACI Consignor Zone should be EUO", "EUO", shipment.JS_Calc_ACIConsignorOriginZone);
			AssertEquals("ACI Consignee Zone should be AFR", "AFR", shipment.JS_Calc_ACIConsigneeDestinationZone);
			AssertEquals("Servicing Postal Filter should be added", true, shipment.Lookups.RefUNLOCO_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Servicing Postal Code:Property"));
		}

		void CreateRateTransportZone()
		{
			ObjectFactory.Configure(TestConfiguration.ConfigurationLocation);
			IRateTransportZoneTestHelper zoneTestHelper = ObjectFactory.Get<IRateTransportZoneTestHelper>();
			zoneTestHelper.AddTestZone("Region1", "4110", "4200", "");
			zoneTestHelper.AddTestZone("Region1", "", "", "Neverland");
			zoneTestHelper.AddTestZone("Region2", "4300", "4399", "");
			zoneTestHelper.CreateRateTransportZones(Factory, TransportProvider1);
		}

		OrgHeader TransportProvider1
		{
			get
			{
				if (transportProvider1 == null)
				{
					transportProvider1 = OrgHeader.New(Factory);
					transportProvider1.OH_FullName = "Transport Provider 1";
					transportProvider1.OH_Code = "TRAN1";
					transportProvider1.MainAddress.OA_Address1 = "Some Address";
				}
				return transportProvider1;
			}
		}
		OrgHeader transportProvider1;

		#endregion

		#region Implementation

		void AssertShipmentDirection(ZString origin, ZString destination, bool isImport, bool isExport, bool isDomestic, bool isCrossTrade)
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			AssertEquals("IsImport", isImport, shipment.IsImport());
			AssertEquals("IsExport", isExport, shipment.IsExport());
			AssertEquals("IsDomestic", isDomestic, shipment.IsDomestic());
			AssertEquals("IsCrossTrade", isCrossTrade, shipment.IsCrossTrade());
		}

		void SetDepartment(bool isImport, bool isExport, bool isDomestic)
		{
			GlbDepartment.CurrentDepartment.GE_Export = isExport;
			GlbDepartment.CurrentDepartment.GE_Import = isImport;
			GlbDepartment.CurrentDepartment.GE_Domestic = isDomestic;
		}

		void SetDepartmentTransportMode(string transportMode)
		{
			GlbDepartment.CurrentDepartment.GE_Sea = transportMode == Core.Constants.TransportModes.Sea;
			GlbDepartment.CurrentDepartment.GE_Air = transportMode == Core.Constants.TransportModes.Air;
			GlbDepartment.CurrentDepartment.GE_Road = transportMode == Core.Constants.TransportModes.Road;
			GlbDepartment.CurrentDepartment.GE_Rail = transportMode == Core.Constants.TransportModes.Rail;
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			return element;
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order, string detail)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Detail = detail;
			return element;
		}

		#endregion

		public void TestScheduleValues()
		{
			ZDateTime now = ZDateTime.Now;

			CommonShipment shipment = Factory.New<CommonShipment>();

			AssertEquals("", shipment.JS_Calc_CurrentVessel);
			AssertEquals("", shipment.JS_Calc_CurrentVoyageFlight);
			AssertEquals(ZDateTime.Empty, shipment.JS_Calc_CurrentETD);
			AssertEquals(ZDateTime.Empty, shipment.JS_Calc_CurrentETA);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins[0].JA_E_DEP = now.AddDays(1);
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations[0].JB_E_ARV = now.AddDays(10);
			voyage.GenerateSailings();

			shipment.JS_JX = voyage.Sailings[0].PK;

			AssertEquals(TestVessel1.RV_FK, shipment.JS_Calc_CurrentVessel);
			AssertEquals("1234", shipment.JS_Calc_CurrentVoyageFlight);
			AssertEquals(now.AddDays(1), shipment.JS_Calc_CurrentETD);
			AssertEquals(now.AddDays(10), shipment.JS_Calc_CurrentETA);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUCNS";
			consol.JK_RL_NKDischargePort = "NLAMS";
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].JW_Vessel = TestVessel2.RV_FK;
			consol.Transports[0].JW_VoyageFlight = "41233";
			consol.Transports[0].JW_ETD = now.AddDays(20);
			consol.Transports[0].JW_ETA = now.AddDays(25);

			shipment.Consols.Add(consol);

			AssertEquals(TestVessel2.RV_FK, shipment.JS_Calc_CurrentVessel);
			AssertEquals("41233", shipment.JS_Calc_CurrentVoyageFlight);
			AssertEquals(now.AddDays(20), shipment.JS_Calc_CurrentETD);
			AssertEquals(now.AddDays(25), shipment.JS_Calc_CurrentETA);
		}

		public void TestDefaultPackLine()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals("Precondition", 0, shipment.JS_TotalPackageCount);
			AssertEquals("New packline not added", 0, shipment.InnerPackLines.Count);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_TotalPackageCount = 10;
			shipment.JS_F3_NKTotalCountPackType = "AA";

			shipment.JS_ActualWeight = 10m;
			shipment.JS_UnitOfWeight = "WW";

			shipment.JS_ActualVolume = 20m;
			shipment.JS_UnitOfVolume = "VV";

			shipment.JS_LoadingMeters = 30m;

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			AssertEquals("Precondition: no packlines in database", 0, anotherFactory.Load<PackLine>(new ZQuery()).Length);

			shipment = anotherFactory.Load<CommonShipment>(shipment.PK);
			AssertEquals("New packline added on first access", 1, shipment.InnerPackLines.Count);

			PackLine defaultLine = shipment.InnerPackLines[0];
			AssertEquals("Default line added, but it's HasChanges was reset to false", false, defaultLine.HasChanges);

			AssertEquals(10, defaultLine.JL_PackageCount);
			AssertEquals("AA", defaultLine.JL_F3_NKPackType);

			AssertEquals(10m, defaultLine.JL_ActualWeight);
			AssertEquals("WW", defaultLine.JL_ActualWeightUQ);

			AssertEquals(20m, defaultLine.JL_ActualVolume);
			AssertEquals("VV", defaultLine.JL_ActualVolumeUQ);

			AssertEquals(30m, defaultLine.JL_LoadingMeters);

			JobContainerPackPivot[] pivots = Factory.Load<JobContainerPackPivot>(new ZQuery(JobContainerPackPivotSchema.J6_JL, shipment.InnerPackLines[0].PK));
			AssertEquals("Container for InnerPackLine", 0, pivots.Length);
		}

		public void TestHasRelatedReceivingAgent()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			AssertEquals(true, shipment.HasRelatedReceivingAgent(consol));

			shipment.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);
			shipment.Consignor.SetRelatedParty(org2, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Pickup);
			shipment.Consignee.SetRelatedParty(org3, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery);

			AssertEquals(false, shipment.HasRelatedReceivingAgent(consol));

			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;
			AssertEquals(true, shipment.HasRelatedReceivingAgent(consol));

			shipment.Consignee.AllRelatedParties.RemoveRelatedParty(RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);
			shipment.Consignee.AllRelatedParties.RemoveRelatedParty(RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery);
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			AssertEquals(true, shipment.HasRelatedReceivingAgent(consol));
		}

		public void TestHasRelatedSendingAgent()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			AssertEquals(true, shipment.HasRelatedSendingAgent(consol));

			shipment.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery);
			shipment.Consignor.SetRelatedParty(org2, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup);
			shipment.Consignor.SetRelatedParty(org3, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery);
			AssertEquals(false, shipment.HasRelatedSendingAgent(consol));

			consol.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			AssertEquals(true, shipment.HasRelatedSendingAgent(consol));

			shipment.Consignor.AllRelatedParties.RemoveRelatedParty(RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup);
			shipment.Consignor.AllRelatedParties.RemoveRelatedParty(RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery);

			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			AssertEquals(true, shipment.HasRelatedSendingAgent(consol));
		}

		public void TestHasRelatedReceivingSendingAgentMatchesConsolMode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			shipment.Consignee.SetRelatedParty(org2, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			shipment.Consignor.SetRelatedParty(org3, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			AssertEquals("Org2 does not match the receiving agent for the 'FCL' consol mode", false, shipment.HasRelatedReceivingAgent(consol));
			AssertEquals("HasRelatedSendingAgent will return true as the consol mode 'FCL' is different to related party mode 'LCL'", true, shipment.HasRelatedSendingAgent(consol));

			shipment.Consignee.SetRelatedParty(org, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			shipment.Consignor.SetRelatedParty(org1, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);

			AssertEquals("Org match the receiving agent for 'FCL' consol mode", true, shipment.HasRelatedReceivingAgent(consol));
			AssertEquals("HasRelatedSendingAgent will return true as the consol mode 'FCL' is different to related party mode 'LCL'", true, shipment.HasRelatedSendingAgent(consol));

			shipment.Consignee.SetRelatedParty(org4, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			shipment.Consignor.SetRelatedParty(org3, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);

			AssertEquals("HasRelatedReceivingAgent will return true as the consol transport mode 'SEA' is different to related party mode 'AIR'", true, shipment.HasRelatedReceivingAgent(consol));
			AssertEquals("Org3 does not match the sending agent for the 'SEA' transport mode", false, shipment.HasRelatedSendingAgent(consol));
		}

		public void TestFindRelatedReceivingSendingForwarderAddressPK_BasedOnMode()
		{
			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty2 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty3 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty4 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty5 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty6 = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "ABC Co.";
			consignee.SetRelatedParty(relatedParty1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignee.SetRelatedParty(relatedParty2, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
			consignee.SetRelatedParty(relatedParty3, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "SDF Co.";
			consignor.SetRelatedParty(relatedParty4, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			consignor.SetRelatedParty(relatedParty5, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignor.SetRelatedParty(relatedParty6, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			var result = shipment.FindRelatedReceivingForwarderAddressPK();
			AssertEquals("Consignee related party 1", relatedParty1.MainAddress.PK, shipment.FindRelatedReceivingForwarderAddressPK());
			result = shipment.FindRelatedSendingForwarderAddressPK();
			AssertEquals("Consignor related party 4", relatedParty4.MainAddress.PK, shipment.FindRelatedSendingForwarderAddressPK());
		}

		public void TestShipmentMasterBillNumbersForRelatedConsols()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "consolOrder2";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_UniqueConsignRef = "consolOrder1";
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_UniqueConsignRef = "consolOrder3";
			consol3.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();
			var shipment = Factory.New<CommonShipment>();

			AssertEquals("MasterBillNumbers", ZString.Empty, shipment.JS_JK_MasterBillNumbers);

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			consol1.JK_MasterBillNum = "MASTERBILL1";
			consol2.JK_MasterBillNum = "MASTERBILL2";

			AssertEquals("MasterBillNumbers", "MASTERBILL2, MASTERBILL1, <none>", shipment.JS_JK_MasterBillNumbers);
		}

		public void TestFindRelatedSendingForwarderAddressPK()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			shipment.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			shipment.Consignor.SetRelatedParty(org2, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			shipment.Consignor.SetRelatedParty(org3, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			AssertEquals(org2.MainAddress.PK, shipment.FindRelatedSendingForwarderAddressPK());

			consignor.AllRelatedParties.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals(org1.MainAddress.PK, shipment.FindRelatedSendingForwarderAddressPK());
		}

		public void TestFindRelatedReceivingForwarderAddressPK()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			shipment.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			shipment.Consignor.SetRelatedParty(org2, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			shipment.Consignee.SetRelatedParty(org3, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			AssertEquals(org1.MainAddress.PK, shipment.FindRelatedReceivingForwarderAddressPK());

			consignee.AllRelatedParties.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals(org2.MainAddress.PK, shipment.FindRelatedReceivingForwarderAddressPK());
		}

		public void TestJobDocsAndCartageShoudNotBeNull()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertNotNull("A new JobDocsAndCartage should have been created.", shipment.DocsAndCartage);

			shipment.Delete();
			Assert("DocsAndCartage shouldn't be deleted after shipment be deleted.", !shipment.DocsAndCartage.IsDeleted);
			AssertNotNull("DocsAndCartage shouldn't be null after shipment be deleted.", shipment.DocsAndCartage);
		}

		public void TestBusinessObjectsWithRelatedEventsForDeclaration()
		{
			var shipment = GetShipment();

			var jobDec = Factory.New<IBaseJobDeclaration>();
			jobDec.JE_JS = shipment.PK;

			AssertEquals("Related Logs has JobDeclaration", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(jobDec));
		}

		public void TestBusinessObjectsWithRelatedEventsForCusEntryHeader()
		{
			var shipment = GetShipment();

			var jobDec = Factory.New<IBaseJobDeclaration>();
			jobDec.JE_JS = shipment.PK;

			var cusEntryHeader = (BusinessObject)Factory.New<ICusEntryHeader>();
			cusEntryHeader[CusEntryHeaderSchema.CH_JE.Name] = jobDec.PK;

			AssertEquals("Related Logs has JobDeclaration", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(cusEntryHeader));
		}

		public void TestBusinessObjectsWithRelatedEventsForNumbers()
		{
			CommonShipment shipment = GetShipment();
			CusEntryNumber number = shipment.Numbers.AddNew();

			AssertEquals("Related Logs has Number Record", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(number));
		}

		public void TestClone()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "XX";
			shipment.JS_ActualVolume = 0.125m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_TotalPackageCount = 5;

			CommonShipment cloneShipment = (CommonShipment)shipment.Clone();
			AssertEquals("Transport mode should be cloned.", shipment.JS_TransportMode, cloneShipment.JS_TransportMode);
			AssertEquals("Packing mode should be cloned.", shipment.JS_PackingMode, cloneShipment.JS_PackingMode);
			AssertEquals("Shipment type should be cloned.", shipment.JS_ShipmentType, cloneShipment.JS_ShipmentType);

			AssertEquals("Persistent values should be cloned.", shipment.JS_ActualWeight, cloneShipment.JS_ActualWeight);
			AssertEquals("Calculated values should be cloned.", shipment.JS_ActualChargeable, cloneShipment.JS_ActualChargeable);
			AssertEquals("Should only be one default inner Packline.", 1, cloneShipment.InnerPackLines.Count);
			AssertEquals("Outer packlines should be cloned.", 1, cloneShipment.OuterPackLines.Count);
		}

		public void TestClone_WithCartage()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			JobDocsAndCartage oldCartage = shipment.DocsAndCartage;
			CommonShipment clonedShipment = (CommonShipment)shipment.Clone();

			Assert("Should not have the same cartage object", oldCartage.PK != clonedShipment.DocsAndCartage.PK);
			AssertEquals("The old cartage object shouldn't get deleted", false, oldCartage.IsDeleted);
			AssertEquals("The old cartage object should still be attached to the original Shipment", shipment.PK, oldCartage.JP_ParentID);
			AssertEquals("JS_TransportMode should be road", shipment.JS_TransportMode, clonedShipment.JS_TransportMode);
			AssertEquals("JS_TransportMode should be road", shipment.JS_PackingMode, clonedShipment.JS_PackingMode);
		}

		public void TestClone_WithDeclaration()
		{
			var shipment = CommonShipment.New(Factory);
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var clonedShipment = (CommonShipment)shipment.Clone();
			if (clonedShipment.Declarations.Length > 0)
			{
				var declarationDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent((IShipmentWithDocsAndCartage)(clonedShipment.Declarations[0]));
				AssertEquals(
					 "Cloned CommonShipment cartage same as declaration cartage",
					 clonedShipment.DocsAndCartage.PK, declarationDocsAndCartage.PK);
			}
			else
			{
				Assert("If we're not going to clone the declaration, this test is irrelevant", true);
			}
		}

		public void TestCloneDetailedGoodsDescription()
		{
			var shipment = GetShipment();
			shipment.DetailedGoodsDescriptionNoteText = "Dmitry and Annie are good mentors";

			var cloneShipment = (CommonShipment)shipment.Clone();

			AssertEquals("Dmitry and Annie are good mentors", cloneShipment.DetailedGoodsDescriptionNoteText);
		}

		public void TestCloneBrokers()
		{
			var importer = Factory.New<OrgHeader>();
			var exporter = Factory.New<OrgHeader>();

			var shipment = GetShipment();

			shipment.JS_OH_ImportBroker = importer.PK;
			shipment.JS_OH_ExportBroker = exporter.PK;

			var cloneShipment = (CommonShipment)shipment.Clone();

			AssertEquals(shipment.ImportBroker, cloneShipment.ImportBroker);
			AssertEquals(shipment.ExportBroker, cloneShipment.ExportBroker);
		}

		public void TestCopyPersistentValuesFrom()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Now;
			CommonShipment clonedShipment = CommonShipment.New(Factory);

			ZGuid beforeCartagePK = clonedShipment.DocsAndCartage.PK;
			clonedShipment.CopyPersistentValuesFrom(shipment);
			AssertEquals("Should have the same JobDocsAndCartage", beforeCartagePK, clonedShipment.DocsAndCartage.PK);

			AssertEquals("Copied values correctly", shipment.DocsAndCartage.JP_DeliveryCartageAdvised, clonedShipment.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public void TestCopyOuterPacklines()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 11;
			shipment.JS_ActualVolume = 12;

			shipment.OuterPackLines.RemoveAndDeleteAll();
			PackLine packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 4;
			packline1.JL_ActualWeight = 5;
			packline1.JL_ActualVolume = 6;

			PackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 6;
			packline2.JL_ActualWeight = 6;
			packline2.JL_ActualVolume = 6;

			CommonShipment clonedShipment = (CommonShipment)shipment.Clone();

			AssertEquals(10, clonedShipment.JS_OuterPacks);
			AssertEquals(11m, clonedShipment.JS_ActualWeight);
			AssertEquals(12m, clonedShipment.JS_ActualVolume);
			AssertEquals(2, clonedShipment.OuterPackLines.Count);

			clonedShipment.OuterPackLines.Sort("JL_PackageCount", ListSortDirection.Ascending);
			AssertEquals(4, clonedShipment.OuterPackLines[0].JL_PackageCount);
			AssertEquals(5m, clonedShipment.OuterPackLines[0].JL_ActualWeight);
			AssertEquals(6m, clonedShipment.OuterPackLines[0].JL_ActualVolume);

			AssertEquals(6, clonedShipment.OuterPackLines[1].JL_PackageCount);
			AssertEquals(6m, clonedShipment.OuterPackLines[1].JL_ActualWeight);
			AssertEquals(6m, clonedShipment.OuterPackLines[1].JL_ActualVolume);
		}

		public void TestClone_ConvertedBooking_OutturnValuesAreSetToDefault_ForAllPackLines()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_IsBooking = true;

			PackLine packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_OutturnComment = "Test OutRunComment";
			packline1.JL_MarksAndNumbers = "TestMarksAndNumber";
			packline1.JL_Outturn = 4;
			packline1.JL_OutturnedWidth = 40;
			packline1.JL_OutturnedHeight = 41;
			packline1.JL_OutturnedLength = 42;
			packline1.JL_OutturnedWeight = 43;
			packline1.JL_OutturnedVolume = 44;
			packline1.JL_Damaged = 3;
			packline1.JL_Pillaged = 4;

			PackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_OutturnComment = "Test OutRunComment2";
			packline2.JL_MarksAndNumbers = "TestMarksAndNumber";
			packline2.JL_Outturn = 0;
			packline2.JL_OutturnedWidth = 40;
			packline2.JL_OutturnedHeight = 41;
			packline2.JL_OutturnedLength = 42;
			packline2.JL_OutturnedWeight = 43;
			packline2.JL_OutturnedVolume = 44;
			packline2.JL_Damaged = 3;
			packline2.JL_Pillaged = 4;

			CommonShipment clonedShipment = (CommonShipment)shipment.Clone();

			AssertEquals(ZString.Empty, clonedShipment.OuterPackLines[0].JL_OutturnComment);
			AssertEquals(ZString.Empty, clonedShipment.OuterPackLines[0].JL_MarksAndNumbers);
			AssertEquals(0, clonedShipment.OuterPackLines[0].JL_Outturn);
			AssertEquals(0, clonedShipment.OuterPackLines[0].JL_Damaged);
			AssertEquals(0, clonedShipment.OuterPackLines[0].JL_Pillaged);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedWidth);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedHeight);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedLength);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedWeight);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedVolume);

			AssertEquals(ZString.Empty, clonedShipment.OuterPackLines[1].JL_OutturnComment);
			AssertEquals(ZString.Empty, clonedShipment.OuterPackLines[1].JL_MarksAndNumbers);
			AssertEquals(0, clonedShipment.OuterPackLines[1].JL_Damaged);
			AssertEquals(0, clonedShipment.OuterPackLines[1].JL_Pillaged);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[1].JL_OutturnedWidth);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[1].JL_OutturnedHeight);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[1].JL_OutturnedLength);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[1].JL_OutturnedWeight);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[1].JL_OutturnedVolume);
		}

		public void TestClone_GetPropertiesToExcludeFromCloning()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			shipment.JS_ElectronicBillOfLadingVersion = 2;
			shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
			shipment.JS_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.Transferable;
			shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
			shipment.JS_ElectronicBillOfLadingReference = "WiseTech";
			shipment.JS_ElectronicBillOfLadingHouseBill = "S000010";

			var result = (CommonShipment)shipment.Clone();
			AssertEquals((ZShort)0, result.JS_ElectronicBillOfLadingVersion);
			AssertNullOrEmpty(result.JS_ElectronicBillOfLadingType);
			AssertNullOrEmpty(result.JS_ElectronicBillOfLadingTerms);
			AssertNullOrEmpty(result.JS_ElectronicBillOfLadingStatus);
			AssertNullOrEmpty(result.JS_ElectronicBillOfLadingReference);
			AssertNullOrEmpty(result.JS_ElectronicBillOfLadingHouseBill);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			result = (CommonShipment)shipment.Clone();
			AssertEquals((ZShort)0, result.JS_ElectronicBillOfLadingVersion);
			AssertEquals("Default Value", Constants.BillOfLadingBillType.Codes.Straight, result.JS_ElectronicBillOfLadingType);
			AssertEquals("Default Value", Constants.BillOfLadingBillTerms.Codes.NonTransferable, result.JS_ElectronicBillOfLadingTerms);
			AssertNullOrEmpty(result.JS_ElectronicBillOfLadingStatus);
			AssertNullOrEmpty(result.JS_ElectronicBillOfLadingReference);
		}

		public void TestImporterDeliverToAddressChanged_SetsDefaultLocalTransportProvider()
		{
			OrgHeader localTransport = Factory.New<OrgHeader>();

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_Address1 = "TEST";
			importer.AllRelatedParties.SetRelatedParty(importer.MainAddress.PK, localTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OH_ImportBroker = importer.PK;
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals(ZGuid.Empty, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals(ZGuid.Empty, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals(localTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);
		}

		public void TestImporterDeliverToAddressChanged_SetsDefaultLocalTransportProviderFallback()
		{
			OrgHeader localTransport = Factory.New<OrgHeader>();

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_Address1 = "TEST";
			importer.AllRelatedParties.SetRelatedParty(importer.MainAddress.PK, localTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_OH_ImportBroker = importer.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals("Related Party with mode of ALL is fallback for Transport Provider", localTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);
		}

		public void TestDestinationChanged_SetsDefaultLocalTransportProviderFallback()
		{
			OrgHeader auTransport = Factory.New<OrgHeader>();
			OrgHeader sydTransport = Factory.New<OrgHeader>();
			OrgHeader fallbackTransport = Factory.New<OrgHeader>();

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_Address1 = "TEST";
			importer.AllRelatedParties.SetRelatedParty(importer.MainAddress.PK, auTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty, "AU");
			importer.AllRelatedParties.SetRelatedParty(importer.MainAddress.PK, sydTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty, "AUSYD");
			importer.AllRelatedParties.SetRelatedParty(importer.MainAddress.PK, fallbackTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OH_ImportBroker = importer.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			shipment.JS_RL_NKDestination = "AUMEL";
			AssertEquals("AUMEL falls back to AU transport", auTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);

			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("AUSYD transport is specific", sydTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);

			shipment.JS_RL_NKDestination = "USCHI";
			AssertEquals("Fallback transport applies to USCHI", fallbackTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr);
		}

		public void TestShipperPickUpAddressChanged_SetsDefaultLocalTransportProvider()
		{
			OrgHeader localTransport = Factory.New<OrgHeader>();

			OrgHeader shipper = Factory.New<OrgHeader>();
			shipper.MainAddress.OA_Address1 = "TEST";
			shipper.AllRelatedParties.SetRelatedParty(shipper.MainAddress.PK, localTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OH_DeliveryAgent = shipper.PK;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.ConsignorPickupAddress.E2_OA_Address = shipper.MainAddress.PK;
			AssertEquals(ZGuid.Empty, shipment.JS_OA_ImportReleaseDepot);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsignorPickupAddress.E2_OA_Address = shipper.MainAddress.PK;
			AssertEquals(ZGuid.Empty, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsignorPickupAddress.E2_OA_Address = shipper.MainAddress.PK;
			AssertEquals(localTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr);
		}

		public void TestShipperPickUpAddressChanged_SetsDefaultLocalTransportProviderFallback()
		{
			OrgHeader localTransport = Factory.New<OrgHeader>();

			OrgHeader shipper = Factory.New<OrgHeader>();
			shipper.MainAddress.OA_Address1 = "TEST";
			shipper.AllRelatedParties.SetRelatedParty(shipper.MainAddress.PK, localTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.All, ZString.Empty);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OH_DeliveryAgent = shipper.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.ConsignorPickupAddress.E2_OA_Address = shipper.MainAddress.PK;
			AssertEquals("Related Party with mode of ALL is fallback for Transport Provider", localTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr);
		}

		public void TestOriginChanged_SetsDefaultLocalTransportProviderFallback()
		{
			OrgHeader auTransport = Factory.New<OrgHeader>();
			OrgHeader sydTransport = Factory.New<OrgHeader>();
			OrgHeader fallbackTransport = Factory.New<OrgHeader>();

			OrgHeader shipper = Factory.New<OrgHeader>();
			shipper.MainAddress.OA_Address1 = "TEST";
			shipper.AllRelatedParties.SetRelatedParty(shipper.MainAddress.PK, auTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.All, ZString.Empty, "AU");
			shipper.AllRelatedParties.SetRelatedParty(shipper.MainAddress.PK, sydTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.All, ZString.Empty, "AUSYD");
			shipper.AllRelatedParties.SetRelatedParty(shipper.MainAddress.PK, fallbackTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.All, ZString.Empty);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OH_DeliveryAgent = shipper.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = shipper.MainAddress.PK;
			shipment.JS_RL_NKOrigin = "AUMEL";
			AssertEquals("AUMEL falls back to AU transport", auTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr);

			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals("AUSYD transport is specific", sydTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr);

			shipment.JS_RL_NKOrigin = "USCHI";
			AssertEquals("Fallback transport applies to USCHI", fallbackTransport.Addresses[0].PK, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr);
		}

		public void TestDestinationChanged_SetDefaultExportBroker()
		{
			var relatedParty = Factory.New<OrgHeader>();
			var shipper = Factory.New<OrgHeader>();
			shipper.AllRelatedParties.SetRelatedParty(shipper.MainAddress.PK, relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.All, ZString.Empty, ZString.Empty);
			var shipment = Factory.New<CommonShipment>();

			shipment.ConsignorPK = shipper.PK;
			shipment.JS_RL_NKDestination = "USLAX";

			AssertEquals("Export Broker Default", relatedParty.PK, shipment.JS_OH_ExportBroker);
		}

		#region Related Business Objects

		#region TestLocalConsol

		public void TestLocalConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				CommonConsol exportConsol = GetConsol("AUSYD", "HKHKG");
				CommonConsol importConsol = GetConsol("HKHKG", "AUSYD");

				CommonConsol departureConsol = GetConsol("GBLON", "SGSIN");
				CommonConsol arrivalConsol = GetConsol("SGSIN", "AUBNE");

				CommonShipment shipment = exportConsol.Shipments.AddNew();
				shipment.Consols.Add(importConsol);
				shipment.Consols.Add(departureConsol);
				shipment.Consols.Add(arrivalConsol);

				ZQuery query = new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUBNE");
				query.AddToFilter(OrgHeaderSchema.OH_IsConsignee, true);

				OrgHeader consignee = Factory.LoadTop1<OrgHeader>(query);
				AssertNotNull("Precondition", consignee);

				shipment.ConsigneePK = consignee.PK;
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_RL_NKDestination = "AUBNE";

				Factory.Save();

				AssertNotNull("Local Consol should not be null", shipment.LocalConsol);
				AssertEquals("Local Consol", importConsol.PK, shipment.LocalConsol.PK);

				shipment.Consols.Remove(importConsol);
				AssertNotNull("Local Consol should not be null", shipment.LocalConsol);
				AssertEquals("Local Consol", arrivalConsol.PK, shipment.LocalConsol.PK);

				shipment.Consols.RemoveAndDeleteAll();
				departureConsol = GetConsol("AUBNE", "NZAKL");
				shipment.Consols.Add(departureConsol);

				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.JS_RL_NKOrigin = "AUBNE";

				AssertNotNull("Local Consol should not be null", shipment.LocalConsol);
				AssertEquals(departureConsol.PK, shipment.LocalConsol.PK);

				shipment.Consols.RemoveAndDeleteAll();
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_RL_NKDestination = "NZAKL";

				exportConsol = GetConsol("AUSYD", "HKHKG");
				importConsol = GetConsol("HKHKG", "AUSYD");
				departureConsol = GetConsol("GBLON", "SGSIN");
				arrivalConsol = GetConsol("SGSIN", "NZAKL");

				shipment.Consols.Add(arrivalConsol);
				shipment.Consols.Add(departureConsol);
				shipment.Consols.Add(exportConsol);
				shipment.Consols.Add(importConsol);

				AssertNotNull("Local Consol should not be null", shipment.LocalConsol);
				AssertEquals(exportConsol.PK, shipment.LocalConsol.PK);

				shipment.Consols.Remove(exportConsol);
				AssertNotNull("Local Consol should not be null", shipment.LocalConsol);
				AssertEquals(departureConsol.PK, shipment.LocalConsol.PK);
			}
		}

		public void TestConsolContainers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				CommonConsol departureConsol = GetConsol("GBLON", "SGSIN");
				CommonConsol arrivalConsol = GetConsol("SGSIN", "AUBNE");
				CommonShipment shipment = arrivalConsol.Shipments.AddNew();
				shipment.Consols.Add(departureConsol);
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUBNE");
				query.AddToFilter(OrgHeaderSchema.OH_IsConsignee, true);
				OrgHeader consignee = Factory.LoadTop1<OrgHeader>(query);
				AssertNotNull("Precondition", consignee);
				shipment.ConsigneePK = consignee.PK;
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_RL_NKDestination = "AUBNE";
				CommonContainer c1 = arrivalConsol.Containers.AddNew();
				c1.JC_ContainerNum = "C1";
				CommonContainer c2 = arrivalConsol.Containers.AddNew();
				c2.JC_ContainerNum = "C2";
				CommonContainer c3 = departureConsol.Containers.AddNew();
				c3.JC_ContainerNum = "C3";
				CommonContainer c4 = departureConsol.Containers.AddNew();
				c4.JC_ContainerNum = "C4";
				Factory.Save();

				AssertEquals("2 depart containers", 2, shipment.AllDepartureContainers.Count);
				Assert("depart containers containes C3", shipment.AllDepartureContainers.Contains(c3));
				Assert("depart containers containes C4", shipment.AllDepartureContainers.Contains(c4));
				AssertEquals("2 arrival containers", 2, shipment.AllArrivalContainers.Count);
				Assert("arrival containers containes C1", shipment.AllArrivalContainers.Contains(c1));
				Assert("arrival containers containes C2", shipment.AllArrivalContainers.Contains(c2));
			}
		}

		CommonConsol GetConsol(ZString loadPort, ZString dischargePort)
		{
			CommonConsol result = Factory.New<CommonConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			result.JK_MasterBillNum = "masterbill";
			result.JK_RL_NKLoadPort = loadPort;
			result.JK_RL_NKDischargePort = dischargePort;
			return result;
		}

		#endregion

		#region TestShippingLine

		public void TestShippingLine()
		{
			CommonConsol departureConsol = GetConsol("SGSIN", "AUSYD");
			OrgHeader shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shippingLine.OH_IsShippingLine = true;

			departureConsol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			CommonShipment shipment = departureConsol.Shipments.AddNew();

			OrgHeader shipmentShippingLine = ((ILocalShippingLineProvider)shipment).ShippingLine;
			AssertNotNull("Shipment's Shipping line", shipmentShippingLine);
			AssertEquals(shippingLine.PK, shipmentShippingLine.PK);
		}

		#endregion

		#region GLBPortdeliveryTime

		public void TestETAOnShipmentGetsUpdatedWithDefaultPortDeliveryTime()
		{
			ZDateTime dateOfArrival = new ZDateTime(2005, 5, 14);
			GlbPortDeliveryTime defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = Core.Constants.TransportModes.Sea;
			defaultDelay.G1_RL_NKDischargePort = "USLAX";
			defaultDelay.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay.G1_DaysDelayFromArrivalToDeliver = 3;

			GlbPortDeliveryTime defaultDelay2 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay2.G1_FreightMode = Core.Constants.ContainerModes.LCL;
			defaultDelay2.G1_RL_NKDischargePort = "USLAX";
			defaultDelay2.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay2.G1_DaysDelayFromArrivalToDeliver = 5;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport transport = consol.Transports[0];
			transport.JW_ETA = dateOfArrival;
			transport.JW_ETD = dateOfArrival.AddDays(-4);
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;

			CommonShipment shipment = consol.Shipments.AddNew();
			AssertEquals("Expecting CommonShipment Eta - to be Consol Eta", dateOfArrival, shipment.JS_E_ARV);

			shipment.JS_RL_NKDestination = "USAAA";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			AssertEquals("Expecting CommonShipment Eta to be 3 days after Consol Eta", dateOfArrival.AddDays(3), shipment.JS_E_ARV);

			shipment.JS_RL_NKDestination = "USAAI";
			AssertEquals("ETA on CommonShipment should be ETA on Consol (No delivery time for USAAI)", dateOfArrival, shipment.JS_E_ARV);

			shipment.JS_RL_NKDestination = "USAAA";
			shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertEquals("ETA on CommonShipment should now be 5 days after Consol ETA", dateOfArrival.AddDays(5), shipment.JS_E_ARV);
		}

		public void TestETAOnShipmentGetsUpdatedWithDefaultPortDeliveryTime_ConsolDischargeInDifferentCountry()
		{
			var dateOfArrival = new ZDateTime(2017, 5, 14);
			var defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = Constants.TransportModes.Sea;
			defaultDelay.G1_RL_NKDischargePort = "USSEA";
			defaultDelay.G1_RL_NKDestinationPort = "CAVAN";
			defaultDelay.G1_DaysDelayFromArrivalToDeliver = 3;

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USSEA";

			var transport = consol.Transports[0];
			transport.JW_ETA = dateOfArrival;
			transport.JW_ETD = dateOfArrival.AddDays(-4);
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;

			var shipment = consol.Shipments.AddNew();
			AssertEquals("Expecting CommonShipment Eta - to be Consol Eta", dateOfArrival, shipment.JS_E_ARV);

			shipment.JS_RL_NKDestination = "CAVAN";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			AssertEquals("Expecting CommonShipment Eta to be 3 days after Consol Eta", dateOfArrival.AddDays(3), shipment.JS_E_ARV);
		}

		public void TestConsigneeChangedUpdatesDefaultPortDeliveryTime()
		{
			OrgHeader client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUSYD"));

			ZDateTime dateOfArrival = new ZDateTime(2012, 5, 14);
			GlbPortDeliveryTime defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = Core.Constants.ContainerModes.FCL;
			defaultDelay.G1_RL_NKDischargePort = "AUSYD";
			defaultDelay.G1_RL_NKDestinationPort = "AUSYD";
			defaultDelay.G1_OH_ClientOverride = client.PK;
			defaultDelay.G1_DaysDelayFromArrivalToDeliver = 0;
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 4;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NLRTM";
			consol.JK_RL_NKDischargePort = "AUSYD";

			Transport transport = consol.Transports[0];
			transport.JW_ETA = dateOfArrival;
			transport.JW_ETD = dateOfArrival.AddDays(-4);
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			Factory.Save();

			AssertEquals("Shipment.DocsAndCartage.Estimated delivery should be empty", true, shipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);

			shipment.ConsigneePK = client.PK;

			AssertEquals("ETA on CommonShipment should now be 4 days after Consol ETA", dateOfArrival.AddDays(4), shipment.DocsAndCartage.JP_EstimatedDelivery);
		}

		public void TestETDeliveryOnShipmentGetsUpdatedWithDefaultPortDeliveryTime()
		{
			FreightDataRegistry.Instance.DefaultDeliveryWhenDelayIsNotSet.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ZDateTime dateOfArrival = new ZDateTime(2005, 5, 14);
			GlbPortDeliveryTime defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = Core.Constants.TransportModes.Sea;
			defaultDelay.G1_RL_NKDischargePort = "USLAX";
			defaultDelay.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay.G1_DaysDelayFromArrivalToDeliver = 2;
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 4;

			GlbPortDeliveryTime defaultDelay2 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay2.G1_FreightMode = Core.Constants.ContainerModes.LCL;
			defaultDelay2.G1_RL_NKDischargePort = "USLAX";
			defaultDelay2.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay2.G1_DaysDelayFromArrivalToDeliver = 3;
			defaultDelay2.G1_DaysFromDestinationArrivalToClientDelivery = 5;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport transport = consol.Transports[0];
			transport.JW_ETA = dateOfArrival;
			transport.JW_ETD = dateOfArrival.AddDays(-4);
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;

			CommonShipment shipment = consol.Shipments.AddNew();
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery should be empty", true, shipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);

			shipment.JS_RL_NKDestination = "USAAA";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			AssertEquals("Shipment.DocsAndCartage.Estimated delivery 6 days after Consol Eta", dateOfArrival.AddDays(6), shipment.DocsAndCartage.JP_EstimatedDelivery);

			shipment.JS_RL_NKDestination = "USAAI";
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery should still be 6 days after consol ETA", dateOfArrival.AddDays(6), shipment.DocsAndCartage.JP_EstimatedDelivery);

			shipment.JS_RL_NKDestination = "USAAA";
			shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertEquals("ETA on CommonShipment should now be 8 days after Consol ETA", dateOfArrival.AddDays(8), shipment.DocsAndCartage.JP_EstimatedDelivery);

			FreightDataRegistry.Instance.DefaultDeliveryWhenDelayIsNotSet.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CommonShipment shipment2 = consol.Shipments.AddNew();

			AssertEquals("Shipment2.DocsAndCartage.Estimated delivery should be empty", true, shipment2.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);

			shipment2.JS_RL_NKDestination = "USAAA";
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			AssertEquals("Shipment2.DocsAndCartage.Estimated delivery should be 6 days after Consol Eta", dateOfArrival.AddDays(6), shipment2.DocsAndCartage.JP_EstimatedDelivery);

			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 0;

			CommonShipment shipment3 = consol.Shipments.AddNew();
			AssertEquals("Shipment2.DocsAndCartage.Estimated delivery should be empty", true, shipment3.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);

			shipment3.JS_RL_NKDestination = "USAAA";
			shipment3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment3.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			AssertEquals("Shipment2.DocsAndCartage.Estimated delivery should be empty", ZDateTime.Empty, shipment3.DocsAndCartage.JP_EstimatedDelivery);
		}

		public void TestETDeliveryOnShipmentGetsUpdatedWithDefaultPortDeliveryTime_All()
		{
			FreightDataRegistry.Instance.DefaultDeliveryWhenDelayIsNotSet.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ZDateTime dateOfArrival = new ZDateTime(2010, 7, 13);

			GlbPortDeliveryTime defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = Core.Constants.TransportModes.Sea;
			defaultDelay.G1_RL_NKDischargePort = "USLAX";
			defaultDelay.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay.G1_DaysDelayFromArrivalToDeliver = 2;
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 1;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport transport = consol.Transports[0];
			transport.JW_ETD = dateOfArrival.AddDays(-4);
			transport.JW_ETA = dateOfArrival;
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;

			CommonShipment shipment = consol.Shipments.AddNew();
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery should be empty", true, shipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);

			shipment.JS_RL_NKDestination = "USAAA";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery 3 days after Consol Eta", dateOfArrival.AddDays(3), shipment.DocsAndCartage.JP_EstimatedDelivery);

			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 2;
			shipment.JS_RL_NKDestination = "USAAI";
			shipment.JS_RL_NKDestination = "USAAA";
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery 4 days after Consol ETA", dateOfArrival.AddDays(4), shipment.DocsAndCartage.JP_EstimatedDelivery);

			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 4;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery 6 days after Consol ETA", dateOfArrival.AddDays(6), shipment.DocsAndCartage.JP_EstimatedDelivery);

			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 5;
			consol.JK_RL_NKDischargePort = "USLAP";
			consol.JK_RL_NKDischargePort = "USLAX";
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery 7 days after Consol ETA", dateOfArrival.AddDays(7), shipment.DocsAndCartage.JP_EstimatedDelivery);

			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 6;
			shipment.JS_E_ARV = shipment.JS_E_ARV.AddDays(1);
			shipment.JS_E_ARV = shipment.JS_E_ARV.AddDays(-1);
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery 8 days after Consol ETA", dateOfArrival.AddDays(8), shipment.DocsAndCartage.JP_EstimatedDelivery);

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 7;
			shipment.Consols.Remove(consol);
			shipment.Consols.Add(consol);
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery 9 days after Consol ETA", dateOfArrival.AddDays(9), shipment.DocsAndCartage.JP_EstimatedDelivery);
		}

		#endregion

		#region Consol

		public void TestHasConsolsDischargingInCurrentCountry()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			CommonConsol foreignConsol = shipment.Consols.AddNew();
			foreignConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			foreignConsol.JK_RL_NKDischargePort = OverseasPort;
			AssertEquals("Shipment doesnt have consol with discharge port in the home country", false, shipment.HasConsolsDischargingInCurrentCountry);

			CommonConsol importConsol = shipment.Consols.AddNew();
			importConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			importConsol.JK_RL_NKDischargePort = HomePort;
			AssertEquals("Shipment does have consol with discharge port in the home country", true, shipment.HasConsolsDischargingInCurrentCountry);

			shipment.Consols.Remove(importConsol);
			AssertEquals("Shipment doesnt have consol with discharge port in the home country", false, shipment.HasConsolsDischargingInCurrentCountry);
		}

		public void TestHasConsolsLoadingInCurrentCountry()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			CommonConsol foreignConsol = shipment.Consols.AddNew();
			foreignConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			foreignConsol.JK_RL_NKDischargePort = OverseasPort;
			AssertEquals("Shipment doesnt have consol with discharge port in the home country", false, shipment.HasConsolsLoadingInCurrentCountry);

			CommonConsol importConsol = shipment.Consols.AddNew();
			importConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			importConsol.JK_RL_NKLoadPort = HomePort;
			AssertEquals("Shipment does have consol with discharge port in the home country", true, shipment.HasConsolsLoadingInCurrentCountry);

			shipment.Consols.Remove(importConsol);
			AssertEquals("Shipment doesnt have consol with discharge port in the home country", false, shipment.HasConsolsLoadingInCurrentCountry);
		}

		public void TestArrivalConsolForSingleConsolWithDifferentPorts()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = shipment.Consols.AddNew();

			shipment.JS_RL_NKOrigin = "GBLHR";
			shipment.JS_RL_NKDestination = "SGSIN";

			consol.JK_RL_NKLoadPort = "DEFRA";
			consol.JK_RL_NKDischargePort = "ITMIL";

			AssertEquals("Single consol is still the arrival consol even if ports differ", consol.PK, shipment.ArrivalConsol.PK);
		}

		#endregion

		#region CusEntryNum

		public void TestCusEntryNumsIncludedFromClearedCusHAWBForNZ()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			CommonShipment shipment = GetSetupShipmentForCusEntryNumsIncludedFromClearedCusHAWBTest();
			AssertEquals("CusEntryNumbers Collection", 1, shipment.CusEntryNumbers.Count);
			CusEntryNumber entryNumber = shipment.CusEntryNumbers[0];
			AssertEquals("12345677", entryNumber[CusEntryNumSchema.CE_EntryNum.Name]);
		}

		public void TestCusEntryNumsIncludedFromClearedCusSCAOceanBillForNZ()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			CommonShipment shipment = GetShipmentForClearedCusSCAOceanBillTest();
			AssertEquals("CusEntryNumbers Collection", 1, shipment.CusEntryNumbers.Count);
			CusEntryNumber entryNumber = shipment.CusEntryNumbers[0];
			AssertEquals("65925817", entryNumber[CusEntryNumSchema.CE_EntryNum.Name]);
		}

		public void TestCusEntryNumsIncludedFromClearedCusHAWBForAU()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CommonShipment shipment = GetSetupShipmentForCusEntryNumsIncludedFromClearedCusHAWBTest();
			AssertEquals("CusEntryNumbers Collection", 0, shipment.CusEntryNumbers.Count);
		}

		CommonShipment GetSetupShipmentForCusEntryNumsIncludedFromClearedCusHAWBTest()
		{
			CommonShipment shipment = GetShipment();
			shipment.FillWithValidTestData();
			Factory.Save();
			AssertEquals("CusEntryNumbers Collection", 0, shipment.CusEntryNumbers.Count);

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			BusinessObject nzCusMAWB = (BusinessObject)secondFactory.New<NZ.ICusMAWB>();
			nzCusMAWB["ECINumber"] = "12345677";
			BusinessObject nzCusHAWB1 = (BusinessObject)secondFactory.New<NZ.ICusHAWB>();
			nzCusHAWB1[CusHAWBSchema.CS_CM] = nzCusMAWB.PK;
			nzCusHAWB1[CusHAWBSchema.CS_JS] = shipment.PK;
			nzCusHAWB1[CusHAWBSchema.CS_CustomsStatus] = "WOF"; // Must match: LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff
			nzCusHAWB1[CusHAWBSchema.CS_IsSelfAssessedClearance] = true;
			BusinessObject nzCusHAWB2 = (BusinessObject)secondFactory.New<NZ.ICusHAWB>();
			nzCusHAWB2[CusHAWBSchema.CS_CM] = nzCusMAWB.PK;
			nzCusHAWB2[CusHAWBSchema.CS_JS] = shipment.PK;
			nzCusHAWB2[CusHAWBSchema.CS_CustomsStatus] = "WOF"; // Must match: LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff
			nzCusHAWB2[CusHAWBSchema.CS_IsSelfAssessedClearance] = true;
			secondFactory.Save();

			ReloadShipmentEntryNumbers(shipment);
			return shipment;
		}

		CommonShipment GetShipmentForClearedCusSCAOceanBillTest()
		{
			CommonShipment shipment = GetShipment();
			shipment.FillWithValidTestData();
			Factory.Save();
			AssertEquals("CusEntryNumbers Collection", 0, shipment.CusEntryNumbers.Count);

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			BusinessObject nzCusOceanBill = (BusinessObject)secondFactory.New<NZ.ICusSCAOceanBill>();
			nzCusOceanBill["EntryNumber"] = "65925817";
			BusinessObject nzHouseBill1 = (BusinessObject)secondFactory.New<NZ.ICusSCAHouse>();
			nzHouseBill1[CusSCAHouseSchema.CA_CB] = nzCusOceanBill.PK;
			nzHouseBill1[CusSCAHouseSchema.CA_JS] = shipment.PK;
			nzHouseBill1[CusSCAHouseSchema.CA_ShipmentStatus] = "WOF";
			nzHouseBill1[CusSCAHouseSchema.CA_IsSAC] = true;
			BusinessObject nzHouseBill2 = (BusinessObject)secondFactory.New<NZ.ICusSCAHouse>();
			nzHouseBill2[CusSCAHouseSchema.CA_CB] = nzCusOceanBill.PK;
			nzHouseBill2[CusSCAHouseSchema.CA_JS] = shipment.PK;
			nzHouseBill2[CusSCAHouseSchema.CA_ShipmentStatus] = "WOF";
			nzHouseBill2[CusSCAHouseSchema.CA_IsSAC] = true;
			secondFactory.Save();

			ReloadShipmentEntryNumbers(shipment);
			return shipment;
		}

		public void TestECNField()
		{
			CommonShipment shipment = GetShipment();
			AssertEquals("", shipment.ECNOrCAN);

			shipment.CustomsEntryNumberType = "EX1";//Exemption code
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			shipment.CustomsEntryNumber = "TEST123";
			AssertEquals("TEST123", shipment.ECNOrCAN);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			AssertEquals("", shipment.ECNOrCAN);

			shipment.CustomsEntryNumberType = CANType.CustomsAuthorityNumber.Code;
			AssertEquals("TEST123", shipment.ECNOrCAN);

			shipment.CustomsEntryNumberType = "EX1";
			AssertEquals("EX1", shipment.ECNOrCAN);

			shipment.CustomsEntryNumberType = Enterprise.Customs.Common.AU.CMR.CMRExportExemptionCodes.EXLV.Code;
			AssertEquals("EXLV", shipment.ECNOrCAN);
		}

		public void TestCustomsEntryNumberTypeIsAnExemptionCode()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "GBLON";

			shipment.CustomsEntryNumberType = "EX1";//Exemption code
			Assert("Should be exempt for AU Export", shipment.CustomsEntryNumberTypeIsAnExemptionCode);
			Assert("Customs Entry number readonly", shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.CustomsEntryNumberType = "XXX";
			Assert("Should NOT be exempt", !shipment.CustomsEntryNumberTypeIsAnExemptionCode);
			Assert("Customs Entry number not readonly", !shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.JS_RL_NKOrigin = "GBLON";
			shipment.JS_RL_NKDestination = "AUBNE";

			shipment.CustomsEntryNumberType = "EX1";
			Assert("Not exempt as not an export", !shipment.CustomsEntryNumberTypeIsAnExemptionCode);
			Assert("Customs Entry number not readonly", !shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.CustomsEntryNumberType = "XXX";
			Assert("Should NOT be exempt", !shipment.CustomsEntryNumberTypeIsAnExemptionCode);
			Assert("Customs Entry number not readonly", !shipment.CustomsEntryNumberInfo.ReadOnly);

			ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;

				shipment.CustomsEntryNumberType = "EX1";
				Assert("Not exempt as GB company export", !shipment.CustomsEntryNumberTypeIsAnExemptionCode);
				Assert("Customs Entry number not readonly", !shipment.CustomsEntryNumberInfo.ReadOnly);

				shipment.CustomsEntryNumberType = "XXX";
				Assert("Not exempt for GB company export either", !shipment.CustomsEntryNumberTypeIsAnExemptionCode);
				Assert("Customs Entry number not readonly", !shipment.CustomsEntryNumberInfo.ReadOnly);

				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "GBLON";

				shipment.CustomsEntryNumberType = "EX1";
				Assert("Not exempt as GB company import", !shipment.CustomsEntryNumberTypeIsAnExemptionCode);
				Assert("Customs Entry number not readonly", !shipment.CustomsEntryNumberInfo.ReadOnly);

				shipment.CustomsEntryNumberType = "XXX";
				Assert("Not exempt for GB company import either", !shipment.CustomsEntryNumberTypeIsAnExemptionCode);
				Assert("Customs Entry number not readonly", !shipment.CustomsEntryNumberInfo.ReadOnly);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountry;
			}
		}

		public void TestExemptionEntryNumberTypeStaysAfterSave()
		{
			var shipment = GetShipment();
			shipment.CustomsEntryNumberType = "EX1";//Exemption code
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			Factory.Save();

			shipment.CustomsEntryNumberType = "EX3";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentLoaded = newFactory.Load<CommonShipment>(shipment.PK);
			AssertEquals("Customs Entry number empty", ZString.Empty, shipmentLoaded.CustomsEntryNumber);
			Assert("Customs Entry number readonly", shipmentLoaded.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Customs EntryNumber Exemption type stays", "EX3", shipmentLoaded.CustomsEntryNumberType);
		}

		public void TestShipmentLoadsExistingEntryNumbers()
		{
			CommonShipment shipment = GetShipment();
			shipment.FillWithValidTestData();
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_RL_NKOrigin = "CATOR";
			Factory.Save();
			AssertEquals("CusEntryNumbers Collection", 0, shipment.CusEntryNumbers.Count);

			AddCusEntryNum(TestEntryNumber, CommonShipment.Schema.TableName, shipment.PK, true);
			AddCusEntryNum("CAENTRYNUM", CommonShipment.Schema.TableName, shipment.PK, true).CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			shipment.ResetCusEntryNumbers();
			AssertEquals("CusEntryNumbers Collection", 1, shipment.CusEntryNumbers.Count);

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			var otherAUCompany = secondFactory.New<GlbCompany>();
			otherAUCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			otherAUCompany.GC_Code = "!AU";
			var otherAUCompanyBranch = secondFactory.New<GlbBranch>();
			otherAUCompanyBranch.GB_Code = "!VU";
			otherAUCompanyBranch.GB_GC = otherAUCompany.PK;
			BusinessObject testDec = (BusinessObject)secondFactory.New<IBaseJobDeclaration>();
			testDec[JobDeclarationSchema.Constants.JE_GB] = otherAUCompanyBranch.PK;
			BusinessObject testHeader = (BusinessObject)secondFactory.New<ICusEntryHeader>();
			testHeader[CusEntryHeaderSchema.Constants.CH_JE] = testDec.PK;
			BusinessObject testHeader2 = (BusinessObject)secondFactory.New<ICusEntryHeader>();
			testHeader2[CusEntryHeaderSchema.Constants.CH_JE] = testDec.PK;
			testDec[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;

			var otherCACompany = secondFactory.New<GlbCompany>();
			otherCACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			otherCACompany.GC_Code = "!CA";
			var otherCACompanyBranch = secondFactory.New<GlbBranch>();
			otherCACompanyBranch.GB_Code = "!VA";
			otherCACompanyBranch.GB_GC = otherCACompany.PK;
			var caDeclaration = secondFactory.New<CA.IJobDeclaration>();
			caDeclaration.JE_GB = otherCACompanyBranch.PK;
			caDeclaration.JE_JS = shipment.PK;
			caDeclaration.JE_MessageType = "EXP";
			var number = secondFactory.New<CusEntryNumber>();
			number.CE_EntryNum = "RC1792201232000019";
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CTN;
			number.CE_ParentID = caDeclaration.PK;
			number.CE_ParentTable = caDeclaration.TableName;
			number.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			secondFactory.Save();

			AddCusEntryNum(TestEntryNumber2, testDec.TableName, testDec.PK, true);

			ReloadShipmentEntryNumbers(shipment);
			AssertEquals("CusEntryNumbers Collection", 2, shipment.CusEntryNumbers.Count);

			AddCusEntryNum(TestEntryNumber3, testHeader.TableName, testHeader.PK, true);
			AddCusEntryNum(TestEntryNumber4, testHeader2.TableName, testHeader2.PK, true);
			AddCusEntryNum(TestEntryNumber4, testHeader2.TableName, testHeader2.PK, true).CE_RN_NKCountryCode = Core.Constants.CountryCodes.Albania;

			ReloadShipmentEntryNumbers(shipment);
			AssertEquals("CusEntryNumbers Collection", 4, shipment.CusEntryNumbers.Count);
			AssertEquals("CusEntryNumbers Collection", 6, shipment.CusEntryNumbersForAllCountries.Count);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			ReloadShipmentEntryNumbers(shipment);
			AssertEquals("CusEntryNumbers Collection", 2, shipment.CusEntryNumbers.Count);
			AssertEquals("CusEntryNumbers Collection", 6, shipment.CusEntryNumbersForAllCountries.Count);
		}

		public void TestCusEntryNumbers_NCTS5()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var shipment = GetShipment();
				var nctsHeader = (BusinessObject)Factory.New<EU.NCTS.ICusInBondHeader>();
				nctsHeader[CusInBondHeaderSchema.BH_ParentID] = shipment.PK;
				nctsHeader[CusInBondHeaderSchema.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS5;
				var nctsDepartureMovement = (BusinessObject)Factory.New<EU.NCTS.IDepartureMovementHeader>();
				nctsDepartureMovement[CusInBondMoveHeaderSchema.BM_BH] = nctsHeader.PK;
				var mrn = Factory.New<CusEntryNumber>();
				mrn.CE_EntryNum = "RC1792201232000019";
				mrn.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				mrn.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
				mrn.CE_ParentID = nctsHeader.PK;

				AssertEquals("CusEntryNumber linked to a NCTS5 Movement should be contained into Shipment CusEntryNumbers.", true, shipment.CusEntryNumbers.Contains(mrn));
			}
		}

		public void TestCustomsEntryNumber()
		{
			CommonShipment shipment = GetShipment();

			AddCusEntryNum("ONE", CommonShipment.Schema.TableName, shipment.PK, true);
			AddCusEntryNum("TWO", CommonShipment.Schema.TableName, shipment.PK, true);
			AddCusEntryNum("", CommonShipment.Schema.TableName, shipment.PK, true);
			AddCusEntryNum("FOUR", CommonShipment.Schema.TableName, shipment.PK, true);
			AddCusEntryNum("", CommonShipment.Schema.TableName, shipment.PK, true);

			AssertEquals("ONE, TWO, FOUR", shipment.CustomsEntryNumber);
		}

		[ExpectNoExceptions]
		public void TestDeleteWithDeclarationAttached()
		{
			var shipment = GetShipment();
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			object x = shipment.DocsAndCartage;
			var cartageOnDeclaration = (JobDocsAndCartage)System.ComponentModel.TypeDescriptor.GetProperties(declaration)["DocsAndCartage"].GetValue(declaration);
			shipment.Delete();

			AssertEquals("Cartage not deleted", false, cartageOnDeclaration.IsDeleted);
		}

		public void TestShipmentCustomsEntryNumber()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals(typeof(CommonShipmentCustomsEntryNumber), shipment.ShipmentCustomsEntryNumber.GetType());

			shipment.CustomsEntryNumber = "11111";
			AssertEquals("11111", shipment.ShipmentCustomsEntryNumber.EntryNumber);

			shipment.ShipmentCustomsEntryNumber.EntryNumber = "22222";
			AssertEquals("22222", shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = "COC";
			AssertEquals("COC", shipment.ShipmentCustomsEntryNumber.EntryType);

			shipment.ShipmentCustomsEntryNumber.EntryType = "CAN";
			AssertEquals("CAN", shipment.CustomsEntryNumberType);

			shipment.CustomsEntryNumberIssueDate = new ZDateTime(2011, 1, 1);
			AssertEquals(new ZDateTime(2011, 1, 1), shipment.ShipmentCustomsEntryNumber.IssueDate);

			shipment.ShipmentCustomsEntryNumber.IssueDate = new ZDateTime(2012, 2, 2);
			AssertEquals(new ZDateTime(2012, 2, 2), shipment.CustomsEntryNumberIssueDate);

			shipment.CustomsEntryNumberExpiryDate = new ZDateTime(2013, 3, 3);
			AssertEquals(new ZDateTime(2013, 3, 3), shipment.ShipmentCustomsEntryNumber.ExpiryDate);

			shipment.ShipmentCustomsEntryNumber.ExpiryDate = new ZDateTime(2014, 4, 4);
			AssertEquals(new ZDateTime(2014, 4, 4), shipment.CustomsEntryNumberExpiryDate);
		}

		public void TestCustomDeclarationNumber()
		{
			CommonShipment shipment = GetShipment();
			shipment.FillWithValidTestData();

			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";

			Factory.Save();
			AssertEquals("Precondition CusEntryNumbers Count", 0, shipment.CusEntryNumbers.Count);
			AssertEquals("CusEntryNumberInfo ReadOnly", false, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("CusEntryNumberTypeInfo ReadOnly", false, shipment.CustomsEntryNumberTypeInfo.ReadOnly);

			shipment.CustomsEntryNumber = TestEntryNumber;
			AssertEquals("CusEntryNumbers Count", 1, shipment.CusEntryNumbers.Count);
			AssertEquals("Shipment CustomsEntryNumber", TestEntryNumber, shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumber = "";
			AssertEquals("CusEntryNumbers Count", 0, shipment.CusEntryNumbers.Count);

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			BusinessObject testDec = (BusinessObject)secondFactory.New<IBaseJobDeclaration>();
			BusinessObject testHeader = (BusinessObject)secondFactory.New<ICusEntryHeader>();
			testHeader[CusEntryHeaderSchema.Constants.CH_JE] = testDec.PK;
			BusinessObject testHeader2 = (BusinessObject)secondFactory.New<ICusEntryHeader>();
			testHeader2[CusEntryHeaderSchema.Constants.CH_JE] = testDec.PK;
			testDec[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			secondFactory.Save();

			AddCusEntryNum(TestEntryNumber2, testDec.TableName, testDec.PK, true);

			ReloadShipmentEntryNumbers(shipment);
			AssertEquals("Precondition CusEntryNumbers Count", 1, shipment.CusEntryNumbers.Count);
			AssertEquals("CusEntryNumberInfo ReadOnly", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("CusEntryNumberTypeInfo ReadOnly", true, shipment.CustomsEntryNumberTypeInfo.ReadOnly);

			AddCusEntryNum(TestEntryNumber3, testHeader.TableName, testHeader.PK, true);
			AddCusEntryNum(TestEntryNumber4, testHeader2.TableName, testHeader2.PK, true);

			ReloadShipmentEntryNumbers(shipment);
			AssertEquals("Precondition CusEntryNumbers Count", 3, shipment.CusEntryNumbers.Count);
			AssertEquals("CusEntryNumberInfo ReadOnly", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("CusEntryNumberTypeInfo ReadOnly", true, shipment.CustomsEntryNumberTypeInfo.ReadOnly);
		}

		public void TestAddDeclarationCusEntryNumbersQuery_DE_ExcludeCancelledMRNEntriesForExport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				CommonShipment shipment = GetShipment();
				shipment.FillWithValidTestData();

				shipment.JS_RL_NKOrigin = "DEWIB";
				shipment.JS_RL_NKDestination = "AUSYD";

				Factory.Save();
				AssertEquals("Precondition CusEntryNumbers Count", 0, shipment.CusEntryNumbers.Count);

				var secondFactory = new BusinessObjectFactory();
				var germanDeclaration = secondFactory.New<DE.IJobDeclaration>();
				germanDeclaration.JE_MessageType = DEJobMessageTypeList.Codes.Export;
				germanDeclaration.JE_JS = shipment.PK;
				var entryHeaderValid = secondFactory.New<ICusEntryHeader>();
				entryHeaderValid.CH_JE = germanDeclaration.PK;
				entryHeaderValid.CH_EntryStatus = "570";
				var entryHeaderInvalid1 = secondFactory.New<ICusEntryHeader>();
				entryHeaderInvalid1.CH_JE = germanDeclaration.PK;
				entryHeaderInvalid1.CH_EntryStatus = "191";
				var entryHeaderInvalid2 = secondFactory.New<ICusEntryHeader>();
				entryHeaderInvalid2.CH_JE = germanDeclaration.PK;
				entryHeaderInvalid2.CH_EntryStatus = "520";
				secondFactory.Save();

				var entryNumValid = AddCusEntryNum("MRNTest1", "CusEntryHeader", entryHeaderValid.PK, systemGenerated: true);
				entryNumValid.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				var entryNumInvalid1 = AddCusEntryNum("MRNTest2", "CusEntryHeader", entryHeaderInvalid1.PK, systemGenerated: true);
				entryNumInvalid1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				var entryNumInvalid2 = AddCusEntryNum("MRNTest3", "CusEntryHeader", entryHeaderInvalid2.PK, systemGenerated: true);
				entryNumInvalid2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				ReloadShipmentEntryNumbers(shipment);
				AssertEquals("Single valid Entry", entryNumValid, shipment.CusEntryNumbers.SingleOrDefault());
			}
		}

		public void TestCusEntryNumType()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			Assert("Pre-Condition Cus Entry Numbers 1 or less", shipment.CusEntryNumbers.Count <= 1);
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Issue Date Read Only", true, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", true, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX1;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Issue Date Read Only", true, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", true, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			shipment.CustomsEntryNumber = "12345";
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Issue Date Read Only", false, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", false, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);

			shipment.CustomsEntryNumber = "";
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Issue Date Read Only", true, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", true, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX1;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Blank", "", shipment.CustomsEntryNumber);
			AssertEquals("Entry Number Issue Date Read Only", true, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", true, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);
			AssertEquals("Entry Number Issue Date Blank", ZDateTime.Empty, shipment.CustomsEntryNumberIssueDate);
			AssertEquals("Entry Number Expiry Date Blank", ZDateTime.Empty, shipment.CustomsEntryNumberExpiryDate);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			shipment.CustomsEntryNumber = "12345";
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Issue Date Read Only", false, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", false, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);

			shipment.CustomsEntryNumber = "";
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Issue Date Read Only", true, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", true, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX2;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Blank", "", shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX3;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Blank", "", shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX5;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Blank", "", shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX7;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Blank", "", shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX9;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Blank", "", shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EXA;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Blank", "", shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EXB;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Blank", "", shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			AssertEquals("Entry Number Read Only", false, shipment.CustomsEntryNumberInfo.ReadOnly);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EXC;
			AssertEquals("Entry Number Read Only", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Entry Number Blank", "", shipment.CustomsEntryNumber);
		}

		public void TestCusEntryNum_EconomicGrouping()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			CusEntryNumber number = AddCusEntryNum("1234", JobShipmentSchema.Constants.TableName, shipment.PK, false);
			number.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			number.CE_EntryType = "XX";
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("UK in EU so should load Germany's number", "1234", shipment.CustomsEntryNumber);
			AssertEquals("UK in EU so should load Germany's number", "XX", shipment.CustomsEntryNumberType);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			shipment.ResetCusEntryNumbers();
			AssertNotEquals("Iceland not in EU", "1234", shipment.CustomsEntryNumber);
			AssertNotEquals("Iceland not in EU", "XX", shipment.CustomsEntryNumberType);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			shipment.ResetCusEntryNumbers();

			CusEntryNumber numberInUK = AddCusEntryNum("9876", JobShipmentSchema.Constants.TableName, shipment.PK, false);
			numberInUK.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			numberInUK.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			numberInUK.CE_EntryType = "ZZ";

			AssertEquals("UK has its own number so should load it", "9876", shipment.CustomsEntryNumber);
			AssertEquals("UK has its own number so should load it", "ZZ", shipment.CustomsEntryNumberType);
		}

		public void TestCusEntryNum_USEntriesAreIncludedForUSOverseasTerritories()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			var usCustomsEntryNumber = AddCusEntryNum("12345", JobShipmentSchema.Constants.TableName, shipment.PK, false);
			usCustomsEntryNumber.CE_RN_NKCountryCode = "US";
			usCustomsEntryNumber.CE_EntryType = "ITN";

			var auCustomsEntryNumber = AddCusEntryNum("56789", JobShipmentSchema.Constants.TableName, shipment.PK, false);
			auCustomsEntryNumber.CE_RN_NKCountryCode = "AU";
			auCustomsEntryNumber.CE_EntryType = "ECN";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				shipment.ResetCusEntryNumbers();
				AssertEquals("ECN", shipment.CustomsEntryNumberType);
				AssertEquals("56789", shipment.CustomsEntryNumber);
			}

			foreach (var countryCode in new[] { "US", "PR", "AS", "MP", "VI", "GU" })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					shipment.ResetCusEntryNumbers();
					AssertEquals("US entry type is included for " + countryCode, "ITN", shipment.CustomsEntryNumberType);
					AssertEquals("US entry number is included for " + countryCode, "12345", shipment.CustomsEntryNumber);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				shipment.ResetCusEntryNumbers();
				AssertEquals("Default entry type for DE", "PMT", shipment.CustomsEntryNumberType);
				AssertEquals("No entry is defined for DE", "", shipment.CustomsEntryNumber);
			}

			var prCustomsEntryNumber = AddCusEntryNum("88888", JobShipmentSchema.Constants.TableName, shipment.PK, false);
			prCustomsEntryNumber.CE_RN_NKCountryCode = "PR";
			prCustomsEntryNumber.CE_EntryType = "ITN";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("PR"))
			{
				shipment.ResetCusEntryNumbers();
				AssertEquals("ITN", shipment.CustomsEntryNumberType);
				AssertEquals("PR Number is chosen over US Number for Peru", "88888", shipment.CustomsEntryNumber);
			}

			foreach (var countryCode in new[] { "US", "AS", "MP", "VI", "GU" })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					shipment.ResetCusEntryNumbers();
					AssertEquals("ITN", shipment.CustomsEntryNumberType);
					AssertEquals("US entry number is chosen over PR Number for " + countryCode, "12345", shipment.CustomsEntryNumber);
				}
			}

			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "ASABC";
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AS"))
			{
				shipment.ResetCusEntryNumbers();
				AssertEquals("For AS/MP/GU in IMP context, do not display the EXP entry context. Show default IMP type and no number.", "CRN", shipment.CustomsEntryNumberType);
				AssertEquals("For AS/MP/GU in IMP context, do not display the EXP entry context. Show default IMP type and no number.", ZString.Empty, shipment.CustomsEntryNumber);
			}

			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "MPABC";
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("MP"))
			{
				shipment.ResetCusEntryNumbers();
				AssertEquals("For AS/MP/GU in IMP context, do not display the EXP entry context. Show default IMP type and no number.", "CRN", shipment.CustomsEntryNumberType);
				AssertEquals("For AS/MP/GU in IMP context, do not display the EXP entry context. Show default IMP type and no number.", ZString.Empty, shipment.CustomsEntryNumber);
			}

			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "GUABC";
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GU"))
			{
				shipment.ResetCusEntryNumbers();
				AssertEquals("For AS/MP/GU in IMP context, do not display the EXP entry context. Show default IMP type and no number.", "CRN", shipment.CustomsEntryNumberType);
				AssertEquals("For AS/MP/GU in IMP context, do not display the EXP entry context. Show default IMP type and no number.", ZString.Empty, shipment.CustomsEntryNumber);
			}
		}

		public void TestEntryNumberTypeInfo()
		{
			var shipment = CommonShipment.New(Factory);
			Factory.Save();
			AssertNotNull("You must access the number for it to be set read only", shipment.CustomsEntryNumber);
			AssertEquals("Readonly", false, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Readonly", false, shipment.CustomsEntryNumberTypeInfo.ReadOnly);
			AssertEquals("Readonly", true, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Readonly", true, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);
			var declaration = Factory.New<IBaseJobDeclaration>();

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNum.CE_ParentID = declaration.PK;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			var secondFactory = new BusinessObjectFactory();
			var loadedShipment = secondFactory.Load<CommonShipment>(shipment.PK);
			AssertNotNull("You must access the number for it to be set read only", loadedShipment.CustomsEntryNumber);
			AssertEquals("Readonly", true, loadedShipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Readonly", true, loadedShipment.CustomsEntryNumberTypeInfo.ReadOnly);
			AssertEquals("Readonly", true, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Readonly", true, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);
		}

		public void TestEntryNumberTypeInfo2()
		{
			var shipment = CommonShipment.New(Factory);
			Factory.Save();
			AssertNotNull("You must access the number for it to be set read only", shipment.CustomsEntryNumber);
			AssertEquals("Readonly", false, shipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Readonly", false, shipment.CustomsEntryNumberTypeInfo.ReadOnly);
			var declaration = Factory.New<IBaseJobDeclaration>();

			var entryHeader = (BusinessObject)Factory.New<ICusEntryHeader>();
			entryHeader[CusEntryHeaderSchema.Constants.CH_JE] = declaration.PK;

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			declaration.JE_JS = shipment.PK;
			Factory.Save();

			var secondFactory = new BusinessObjectFactory();
			var loadedShipment = secondFactory.Load<CommonShipment>(shipment.PK);
			AssertNotNull("You must access the number for it to be set read only", loadedShipment.CustomsEntryNumber);
			AssertEquals("Readonly", true, loadedShipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("Readonly", true, loadedShipment.CustomsEntryNumberTypeInfo.ReadOnly);
		}

		public void TestCustomsEntryNumberType()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.CustomsEntryNumberType = "EXPE";
			AssertEquals("EXPE", shipment.CustomsEntryNumberType);
			shipment.CustomsEntryNumberType = "EX1";
			AssertEquals("EX1", shipment.CustomsEntryNumberType);
		}

		public void TestCustomsEntryNumberTypeSavedWhenNoCustomsEntryNumber()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			CommonShipment shipment = Factory.New<CommonShipment>();

			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.CustomsEntryNumberType = "EXTI";
			Factory.Save();

			CommonShipment shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("AU Export: Exemption code is saved without a corresponding Customs Entry Number", "EXTI", shipment2.CustomsEntryNumberType);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.CustomsEntryNumberType = "CCN";
			shipment.CustomsEntryNumber = "12345";
			Factory.Save();

			shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("AU Export: Non-exemption code saved with a corresponding Customs Entry Number", "CCN", shipment2.CustomsEntryNumberType);
			AssertEquals("Number is also saved", "12345", shipment2.CustomsEntryNumber);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.CustomsEntryNumberType = "CCN";
			Factory.Save();

			shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("AU Export: Non-exemption code reset without a corresponding Customs Entry Number", shipment2.ShipmentCustomsEntryNumber.EntryType_List[0].Code, shipment2.CustomsEntryNumberType);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.CustomsEntryNumberType = "CCN";
			Factory.Save();

			shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("AU Import: Code is saved without a corresponding Customs Entry Number", "CCN", shipment2.CustomsEntryNumberType);

			ZString savedCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;

			try
			{
				shipment = Factory.New<CommonShipment>();
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.CustomsEntryNumberType = "CCN";
				Factory.Save();

				shipment2 = factory2.Load<CommonShipment>(shipment.PK);
				AssertEquals("GB Export: Code is saved without a corresponding Customs Entry Number", "CCN", shipment2.CustomsEntryNumberType);

				shipment = Factory.New<CommonShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "GBLON";
				shipment.CustomsEntryNumberType = "CCN";
				Factory.Save();

				shipment2 = factory2.Load<CommonShipment>(shipment.PK);
				AssertEquals("GB Import: Code is saved without a corresponding Customs Entry Number", "CCN", shipment2.CustomsEntryNumberType);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = savedCountryCode;
			}
		}

		public void TestBrokerageCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.FrenchGuyana))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("FR", shipment.BrokerageCountryCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("FR", shipment.BrokerageCountryCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("DE", shipment.BrokerageCountryCode);
			}
		}

		public void TestCusEntryNumbersIsManagedByDataRefreshBus()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			Assert("Should be managed by data refresh bus", shipment.CusEntryNumbers.IsManagedForDataRefresh);
		}

		public void TestCusEntryNumberDates()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "DEHAM";

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			AssertEquals("Entry Number Issue Date Read Only", true, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", true, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);

			shipment.CustomsEntryNumber = "asd";
			AssertEquals("Entry Number Issue Date Read Only", false, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", false, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);
			shipment.CustomsEntryNumberExpiryDate = ZDateTime.Today.AddDays(2);
			shipment.CustomsEntryNumberExpiryDate = ZDateTime.Today.AddDays(3);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX1;
			AssertEquals("Entry Number Issue Date Read Only", true, shipment.CustomsEntryNumberIssueDateInfo.ReadOnly);
			AssertEquals("Entry Number Expiry Date Read Only", true, shipment.CustomsEntryNumberExpiryDateInfo.ReadOnly);
			AssertEquals("Entry Number Issue Date Blank", ZDateTime.Empty, shipment.CustomsEntryNumberIssueDate);
			AssertEquals("Entry Number Expiry Date Blank", ZDateTime.Empty, shipment.CustomsEntryNumberExpiryDate);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			shipment.CustomsEntryNumber = "asd";
			shipment.CustomsEntryNumberIssueDate = ZDateTime.Today.AddDays(2);
			shipment.CustomsEntryNumberExpiryDate = ZDateTime.Today.AddDays(3);
			AssertEquals(false, shipment.CustomsEntryNumberIssueDateInfo.HasNotifications());
			AssertEquals(false, shipment.CustomsEntryNumberExpiryDateInfo.HasNotifications());

			shipment.CustomsEntryNumberIssueDate = ZDateTime.Today.AddDays(3);
			shipment.CustomsEntryNumberExpiryDate = ZDateTime.Today.AddDays(2);
			AssertEquals(true, shipment.CustomsEntryNumberIssueDateInfo.HasNotifications());
			AssertEquals(true, shipment.CustomsEntryNumberExpiryDateInfo.HasNotifications());
		}

		public void TestCustomsEntryNumberTypeCreatedFromDeclaration()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry("US");

				var shipment = Factory.New<CommonShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USMIA";

				Factory.Save();

				CusEntryNumber customsEntryNumber = Factory.New<CusEntryNumber>();
				customsEntryNumber.CE_EntryType = CusEntryNumberTypes.UnitedStates.EntrySummary;
				customsEntryNumber.CE_EntryNum = "000056789";
				customsEntryNumber.CE_ParentID = shipment.PK;
				customsEntryNumber.CE_ParentTable = JobShipmentSchema.Constants.TableName;
				customsEntryNumber.CE_EntryIsSystemGenerated = false;
				customsEntryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				Factory.Save();

				shipment = Factory.Load<CommonShipment>(shipment.PK);

				AssertEquals("Shipment should have one custom entry number", 1, shipment.CusEntryNumbers.Count);
				AssertEquals("Shipment should load custom entry number", customsEntryNumber.PK, shipment.CusEntryNumbers[0].PK);
				AssertEquals("Customs number should have been loaded", customsEntryNumber.CE_EntryNum, shipment.CustomsEntryNumber);
				AssertEquals("Customs number type should have been loaded", customsEntryNumber.CE_EntryType, shipment.CustomsEntryNumberType);

				shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryType();

				AssertEquals(false, shipment.CustomsEntryNumberTypeInfo.HasError("Enter a valid selection."));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestClearCustomsEntryNumberWithDefaultValue()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Japan);

				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.CustomsEntryNumberType = CusEntryNumberTypes.Standard.ClearancePermitNumber;
				shipment.CustomsEntryNumber = "7";
				AssertEquals(1, shipment.CusEntryNumbers.Count);

				shipment.CustomsEntryNumber = "";
				AssertEquals(0, shipment.CusEntryNumbers.Count);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestDeDuplicateCusEntryNumsFilters()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			var shipment = GetShipment();
			shipment.FillWithValidTestData();
			Factory.Save();
			AssertEquals("CusEntryNumbers Collection", 0, shipment.CusEntryNumbers.Count);

			var secondFactory = new BusinessObjectFactory();
			var nzCusMAWB = (BusinessObject)secondFactory.New<NZ.ICusMAWB>();
			nzCusMAWB["ECINumber"] = "12345677";
			var nzCusHAWB1 = (BusinessObject)secondFactory.New<NZ.ICusHAWB>();
			nzCusHAWB1[CusHAWBSchema.CS_CM] = nzCusMAWB.PK;
			nzCusHAWB1[CusHAWBSchema.CS_JS] = shipment.PK;
			nzCusHAWB1[CusHAWBSchema.CS_CustomsStatus] = "WOF"; // Must match: LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff
			nzCusHAWB1[CusHAWBSchema.CS_IsSelfAssessedClearance] = true;
			var nzCusHAWB2 = (BusinessObject)secondFactory.New<NZ.ICusHAWB>();
			nzCusHAWB2[CusHAWBSchema.CS_CM] = nzCusMAWB.PK;
			nzCusHAWB2[CusHAWBSchema.CS_JS] = shipment.PK;
			nzCusHAWB2[CusHAWBSchema.CS_CustomsStatus] = "WOF"; // Must match: LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff
			nzCusHAWB2[CusHAWBSchema.CS_IsSelfAssessedClearance] = true;
			secondFactory.Save();

			ReloadShipmentEntryNumbers(shipment);
			AssertEquals("CusEntryNumbers Collection", 1, shipment.CusEntryNumbers.Count);
			var query = ((ICusEntryNumFilterProvider)nzCusHAWB1).ValidCusEntryNumFilter;
			var parts = shipment.CusEntryNumbers.CompleteFilter.LiteralTextADO.Split(new[] { query.LiteralTextADO }, StringSplitOptions.None);
			AssertEquals("Should be one occurence", 2, parts.Length);
		}

		public void TestNumbersAsString()
		{
			var shipment = GetShipment();
			Factory.Save();

			var cusNumber1 = shipment.Numbers.AddNew();
			cusNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			cusNumber1.CE_RN_NKCountryCode = "";
			cusNumber1.CE_EntryNum = "XXXABILL1";
			AssertEquals("AMS: XXXABILL1", shipment.NumbersAsString);

			cusNumber1.CE_RN_NKCountryCode = "AU";
			AssertEquals("AMS: XXXABILL1/AU", shipment.NumbersAsString);

			var cusNumber2 = shipment.Numbers.AddNew();
			cusNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusNumber2.CE_EntryNum = "CV00001";
			cusNumber2.CE_RN_NKCountryCode = "CN";

			AssertEquals("AMS: XXXABILL1/AU, BKG: CV00001/CN", shipment.NumbersAsString);
		}

		#region eHub Interchange Reference

		public void TestEHubInterchangeReferenceNumberCannotBeModifiedOrDeleted()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			var entryNum = shipment.Numbers.AddNew();
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryType = CusEntryNumLookups.HIR;
			entryNum.CE_EntryNum = "12345";

			Factory.Save();

			var reloadedShipment = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			var reloadedEntryNum = reloadedShipment.Numbers.OfType<CusEntryNumber>().First(n => n.CE_EntryType == CusEntryNumLookups.HIR);
			Assert("HIR is read-only", reloadedEntryNum.ReadOnly);
			Assert("HIR cannot be deleted", !reloadedEntryNum.CanDelete);
			AssertEquals("The HIR is system generated and cannot be deleted.", reloadedEntryNum.ReasonForNotAbleToDelete);
		}

		#endregion

		#endregion

		#region JobDocsAndCartage

		public void TestDocsAndCartageAlwaysExists()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			AssertNotNull("DocsAndCartage always not null.", shipment.DocsAndCartage);
		}

		public void TestDocsAndCartageDeleting()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			JobDocsAndCartage cartage = shipment.DocsAndCartage;
			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;

			shipment.Delete();
			AssertEquals("Cartage object got deleted.", true, cartage.IsDeleted);
			AssertEquals("Create new DocsAndCartage after deleted.", ZDateTime.Empty, shipment.DocsAndCartage.JP_EstimatedDelivery);
		}

		#endregion

		#region NoteTypes

		public void TestNoteTypes()
		{
			CommonShipment shipment = GetShipment();
			Assert("PredefinedNoteTypes.Instance.CertificateOfOriginNote should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.CertificateOfOriginNote, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.ClientVisibleJobNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.ClientVisibleJobNotes, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.DeliveryInstructionsNote should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.DeliveryInstructionsNote, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.PickupInstructionsNote should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.PickupInstructionsNote, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.DetailedGoodsDescription should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.DetailedGoodsDescription, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.HandlingInstructions should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.HandlingInstructions, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.MarksAndNumbers should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.MarksAndNumbers, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.SpecialInstructions should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.SpecialInstructions, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.BookingNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.BookingNotes, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.CartageHistoryNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.CartageHistoryNotes, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.InternalWorkNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.InternalWorkNotes, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.PaymentHandlingInstructions should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.PaymentHandlingInstructions, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.AutoRatingAuditLog should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.AutoRatingAuditLog, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.FaxEmailTransmissionLog should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.OutturnNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.OutturnNotes, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.ManifestGoodsDescription should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.ManifestGoodsDescription, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.CustomsInstructionNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.CustomsInstructionNotes, shipment.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes, shipment.NoteTypes));
		}

		public void TestNoteTypes_DeclarationsShouldBeLoadedInShipmentFactory()
		{
			AssertDeclaratonExistInFactoryCache<CommonShipment>(
				"Shipment's factory should be used to load declarations while getting NoteTypes", true);
		}

		public void TestNoteTypes_ShipmentCantHaveDeclarations_DeclarationsShouldBeLoadedInReadonlyFactory()
		{
			AssertDeclaratonExistInFactoryCache<ShipmentForNoteTypesTesting>(
				"Shipment's readonly factory should be used to load declarations while getting NoteTypes", false);
		}

		void AssertDeclaratonExistInFactoryCache<T>(string message, bool shouldBeInFactoryCache)
			where T : CommonShipment
		{
			TestCaseHelper.ClearTable(JobDeclarationSchema.Constants.TableName);

			var creationFactory = new BusinessObjectFactory();
			creationFactory.RefreshEnabled = false;

			var shipment = creationFactory.New<CommonShipment>();

			var declaration = creationFactory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			creationFactory.Save();

			shipment = Factory.Load<T>(shipment.PK);
			object loaded = shipment.NoteTypes;

			var query = new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK);
			query.FetchOnlyFromLocalCache = true;

			var alreadyLoadedDeclarations = Factory.Load<IBaseJobDeclaration>(query);
			AssertEquals(message, shouldBeInFactoryCache, alreadyLoadedDeclarations.Length > 0);

			bool shouldBeInReadonlyFactoryCache = !shouldBeInFactoryCache;
			alreadyLoadedDeclarations = Factory.GetCachedReadOnlyFactory().Load<IBaseJobDeclaration>(query);
			AssertEquals(message, shouldBeInReadonlyFactoryCache, alreadyLoadedDeclarations.Length > 0);
		}

		class ShipmentForNoteTypesTesting : CommonShipment
		{
			public ShipmentForNoteTypesTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool CanHaveDeclarations
			{
				get { return false; }
			}
		}

		#endregion

		#region NoteContextsForRelatedNotes

		public void TestNoteContextsForRelatedNotes()
		{
			var shipment = Factory.New<TestShipmentExposer>();
			shipment.JS_TransportMode = "";
			Assert("Always have Forwarding module", (shipment.NoteContextsForRelatedNotes.Module & StmNoteContextModule.F) != 0);
			Assert("Always have 'Forwarding, Brokerage, CFS and Orders' module option", (shipment.NoteContextsForRelatedNotes.Module & StmNoteContextModule.I) != 0);
			Assert("Always have 'CommonShipment and Declaration' module option", (shipment.NoteContextsForRelatedNotes.Module & StmNoteContextModule.E) != 0);
			Assert("Not be attached to declaration yet", (shipment.NoteContextsForRelatedNotes.Module & StmNoteContextModule.D) == 0);
			Assert("Not be air yet", (shipment.NoteContextsForRelatedNotes.FreightMode & StmNoteContextFreightMode.I) == 0);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Assert("Should be air", (shipment.NoteContextsForRelatedNotes.FreightMode & StmNoteContextFreightMode.I) != 0);

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			Assert("Should be attached to declaration", (shipment.NoteContextsForRelatedNotes.Module & StmNoteContextModule.D) != 0);
		}

		public void TestConsigorAndConsigneeNotes()
		{
			TestShipmentExposer shipment = Factory.New<TestShipmentExposer>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			OrgHeader consignee = Factory.New<OrgHeader>();
			OrgHeader consignor = Factory.New<OrgHeader>();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignee Pickup Instructions").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignor Pickup Instructions").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);

			AssertEquals("Should only show 1 note", 1, shipment.Notes.VisibleNotes.Count);
			AssertEquals("The one note that is shown should be the Consignor Note", "Consignor Pickup Instructions", shipment.Notes.VisibleNotes[0].ST_NoteText);

			shipment = Factory.New<TestShipmentExposer>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			consignee = Factory.New<OrgHeader>();
			consignor = Factory.New<OrgHeader>();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignee Delivery Instructions").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignor Delivery Instructions").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			AssertEquals("Should only show 1 note", 1, shipment.Notes.VisibleNotes.Count);
			AssertEquals("The one note that is shown should be the Consignor Note", "Consignee Delivery Instructions", shipment.Notes.VisibleNotes[0].ST_NoteText);

			shipment = Factory.New<TestShipmentExposer>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			consignee = Factory.New<OrgHeader>();
			consignor = consignee;

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			StmNote consigneeNote = consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignee Delivery Instructions");
			consigneeNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			StmNote consignorNote = consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignor Delivery Instructions");
			consignorNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			AssertEquals("Should show 2 notes", 2, shipment.Notes.VisibleNotes.Count);
			AssertContainsExactElementsInAnyOrder("The show both notes cause Consignee is the same as Consignor", new StmNote[] { consigneeNote, consignorNote }, shipment.Notes.VisibleNotes);
		}

		public void TestCrossTradeNotes()
		{
			var shipment = Factory.New<TestShipmentExposer>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "USLAX";

			var cne = Factory.New<OrgHeader>();
			var cnr = Factory.New<OrgHeader>();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;

			cne.Notes.AddNew(false, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "Internal - Consignee Cross Trade").ST_NoteContextDirection = nameof(StmNoteContextDirection.X);
			cnr.Notes.AddNew(false, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "Internal - Consignor Cross Trade").ST_NoteContextDirection = nameof(StmNoteContextDirection.X);

			var expectedList = new List<ZString> { "Internal - Consignee Cross Trade", "Internal - Consignor Cross Trade" };

			AssertEquals("Should show 2 notes", 2, shipment.Notes.VisibleNotes.Count);
			AssertContainsExactElementsInAnyOrder(expectedList, shipment.Notes.VisibleNotes.Cast<StmNote>().Select(c => c.ST_NoteDataAsText));
		}

		#region Test Note Properties

		public void TestMarksAndNumber()
		{
			CommonShipment shipment = GetShipment();
			string marksAndNumbers = "THIS IS THE TEST MARKS AND NUMBER THAT CAN BE ENTERED\nFROM THE CONSOL GRID\nWHICH allows better data entry methods";
			shipment.JS_MarksAndNumbers = marksAndNumbers;

			StmNote generatedNote = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description)[0];
			AssertNotNull("Failed to create the Marks and Numbers Note from calculated field", generatedNote);
			AssertEquals("StmNote Text", marksAndNumbers, generatedNote.ST_NoteText);
		}

		public void TestMarksAndNumbersShort()
		{
			CommonShipment shipment = GetShipment();
			AssertEquals("No Note", "", shipment.JS_MarksAndNumbersShort);

			string noteText = "Short Marks and Numbers";
			shipment.JS_MarksAndNumbersShort = noteText;
			StmNote[] marksAndNumbersNotes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
			AssertEquals("Should create new Note.", 1, marksAndNumbersNotes.Length);
			AssertEquals("Set: Should update Note.", noteText, marksAndNumbersNotes[0].ST_NoteText);

			marksAndNumbersNotes[0].ST_NoteText = "First Line.\r\nSecond Line.";
			AssertEquals("Get: Multiline notes.", "First Line.", shipment.JS_MarksAndNumbersShort);

			shipment.JS_MarksAndNumbersShort = "Change First Line.";
			AssertEquals("Set: Should update first line only.", "Change First Line.\r\nSecond Line.", marksAndNumbersNotes[0].ST_NoteText);
		}

		public void TestMarksAndNumbersShortSetsHasChanges()
		{
			CommonShipment testShipment = GetShipment();
			AssertEquals("Precondition - HasChanges false", false, testShipment.HasChanges);

			testShipment.JS_MarksAndNumbersShort = "Set MarksAndNumbers short text";
			AssertEquals("Enter new note should set HasChanges to true", true, testShipment.HasChanges);
			Factory.Save();

			testShipment.JS_MarksAndNumbersShort = "Modify MarksAndNumbers short text";
			AssertEquals("Modify note should set HasChanges to true", true, testShipment.HasChanges);
		}

		public void TestMarksAndNumbersWorksInAllLanguages()
		{
			var shipment = GetShipment();
			shipment.JS_MarksAndNumbers = "Numbers and marks";
			shipment.Factory.Save();
			foreach (var language in DataFile.GetAvailableLanguages())
			{
				using (Res.TemporarilySwitchLanguage(language))
				{
					shipment = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
					AssertEquals(language, "Numbers and marks", shipment.JS_MarksAndNumbers);
				}
			}
		}

		#endregion

		#region TestShipmentExposer
		internal class TestShipmentExposer : CommonShipment
		{
			public TestShipmentExposer(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new StmNoteContexts NoteContextsForRelatedNotes
			{
				get { return base.NoteContextsForRelatedNotes; }
			}

			protected override ConsolCollection GetNewConsolCollection()
			{
				return new ConsolCollection(this);
			}
		}

		#endregion

		#endregion

		#region When Having a Declaration Attached

		public void TestEventsMergedWithDeclaration()
		{
			var shipment = CommonShipment.New(Factory);
			var shipmentLog = shipment.Logs.AddNew();

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var declarationLog = ((BusinessObject)declaration).GetLogs().AddNew();

			var shipmentLogs = new StmALogCollectionView(shipment);
			AssertEquals("Log on Shipment", true, shipmentLogs.Contains(shipmentLog.PK));
			AssertEquals("Log on declaration", true, shipmentLogs.Contains(declarationLog.PK));
		}

		#endregion

		#endregion

		#region JS_ShippedOnBoardDate

		public void TestJS_ShippedOnBoardDate_EventIsCreated_EventTypeDependsFromShippedOnBoardType()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Shipped;
			shipment.JS_ShippedOnBoardDate = ZDateTime.Now;

			var freightLoadedLog = shipment.Logs.MostRecentLogByEventTime(Events.FreightLoaded);
			AssertEquals(false, freightLoadedLog.SL_IsEstimate);

			var cargoReceivedLog = shipment.Logs.MostRecentLogByEventTime(Events.CargoReceivedAtDepot);
			AssertNull("CargoReceived log not created", cargoReceivedLog);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Received;
			shipment.JS_ShippedOnBoardDate = ZDateTime.Now;

			freightLoadedLog = shipment.Logs.MostRecentLogByEventTime(Events.FreightLoaded);
			AssertNull("FreightLoaded log not created", cargoReceivedLog);

			cargoReceivedLog = shipment.Logs.MostRecentLogByEventTime(Events.CargoReceivedAtDepot);
			AssertEquals(false, cargoReceivedLog.SL_IsEstimate);
		}

		public void TestJS_ShippedOnBoardDate_EventIsCreated_EventParameters_WithoutLOC()
		{
			var shipment = Factory.New<CommonShipment>();

			shipment.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Shipped;
			shipment.JS_ShippedOnBoardDate = ZDateTime.Now;

			var eventLog = shipment.Logs.MostRecentLogByEventTime(Events.FreightLoaded);
			AssertEquals(false, eventLog.SL_IsEstimate);
			AssertEquals("Facility", "|FAC=CTO", eventLog.SL_Reference);
		}

		public void TestJS_ShippedOnBoardDate_EventIsCreated_EventParameters_WithLOC()
		{
			var shipment = Factory.New<CommonShipment>();

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.Transports[0].JW_ETD = ZDateTime.Now;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "USLAX";
			consol2.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);

			AssertEquals("Precondition", consol1, shipment.Consols.GetEarliestConsol());

			shipment.JS_ShippedOnBoardDate = ZDateTime.Now.AddMinutes(10);

			var eventLog = shipment.Logs.MostRecentLogByEventTime(Events.FreightLoaded);
			AssertEquals(false, eventLog.SL_IsEstimate);
			AssertEquals("Location should be earliest consol's first load port", "|FAC=CTO|LOC=AUSYD", eventLog.SL_Reference);
		}

		public void TestJS_ShippedOnBoardDate_InboundEvent_CargoReceived()
		{
			var eventTime = ZDateTimeOffset.Now;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Shipped;

			shipment.Logs.AddNew(Events.CargoReceivedAtDepot, "", eventTime);
			AssertEquals("ShippedOnBoardType not matched - date not updated", ZDateTime.Empty, shipment.JS_ShippedOnBoardDate);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Received;

			shipment.Logs.AddNew(Events.CargoReceivedAtDepot, "", eventTime);
			AssertEquals("ShippedOnBoardType matched - date updated", eventTime.ToZDateTime(), shipment.JS_ShippedOnBoardDate);
		}

		public void TestJS_ShippedOnBoardDate_InboundEvent_FreightLoaded()
		{
			var eventTime = ZDateTimeOffset.Now;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Received;

			shipment.Logs.AddNew(Events.FreightLoaded, "", eventTime);
			AssertEquals("ShippedOnBoardType not matched, date not updated", ZDateTime.Empty, shipment.JS_ShippedOnBoardDate);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Shipped;

			shipment.Logs.AddNew(Events.FreightLoaded, "|LOC=USLAX", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, shipment.JS_ShippedOnBoardDate);

			shipment.Logs.AddNew(Events.FreightLoaded, "", eventTime);
			AssertEquals("Location is empty, date updated", eventTime.ToZDateTime(), shipment.JS_ShippedOnBoardDate);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Shipped;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.Transports[0].JW_ETD = ZDateTime.Now;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "USLAX";
			consol2.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);

			AssertEquals("Precondition", consol1, shipment.Consols.GetEarliestConsol());

			shipment.Logs.AddNew(Events.FreightLoaded, "|LOC=NZAKL", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, shipment.JS_ShippedOnBoardDate);

			shipment.Logs.AddNew(Events.FreightLoaded, "|LOC=AUSYD", eventTime);
			AssertEquals("Location matched, date updated", eventTime.ToZDateTime(), shipment.JS_ShippedOnBoardDate);

			var newEventTime = eventTime.AddMinutes(10);

			shipment.Logs.AddNew(Events.FreightLoaded, "|LOC=AUSYD", newEventTime);
			AssertEquals("Should not update non-empty date", eventTime.ToZDateTime(), shipment.JS_ShippedOnBoardDate);
		}

		#endregion

		#region ColoadShipments

		public void TestGetPksFromAllSubShipmentsAndDeclarationsWithoutChildren()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			var shipmentWithCLD = masterShipment.CoLoadShipments.AddNew();
			shipmentWithCLD.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			var clddeclaration = Factory.New<IBaseJobDeclaration>();
			clddeclaration.JE_JS = shipmentWithCLD.PK;

			var childShipment = shipmentWithCLD.CoLoadShipments.AddNew();
			childShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var childdeclaration = Factory.New<IBaseJobDeclaration>();
			childdeclaration.JE_JS = childShipment.PK;

			var expectedPks = new[]
			{
				childShipment.PK, childdeclaration.PK
			};

			var atcualPks = masterShipment.GetPksFromAllSubShipmentsAndDeclarationsWithoutChildren();

			AssertContainsExactElementsInAnyOrder("Shoudl only get all related child shipment pks without these master pks", expectedPks, atcualPks);
		}

		public void TestCoLoadMaster_ListFilterDefaults()
		{
			ColoadHelper.MasterShipment.Consols.AddNew();
			var collection = ColoadHelper.MasterShipment.Lookups.CoLoadMaster_List;
			AssertEquals("Could have a CFS CommonShipment filter appear so should have NO filters by defaut", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Co-Load Status" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Could have a CFS CommonShipment filter appear so should have NO filters by defaut", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FreightConstants.NumberFilterTypes.ConsolNo + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		public void TestColoadShipmentsAreCreatedWhenSimpleTotalsValuesChangeInSubShipmentAcrossFactories_Volume()
		{
			AssertColoadShipmentsAreCreatedWhenSimpleTotalsValuesChangeInSubShipmentAcrossFactories(JobShipmentSchema.JS_ActualVolume, false);
		}

		public void TestColoadShipmentsAreCreatedWhenSimpleTotalsValuesChangeInSubShipmentAcrossFactories_Weight()
		{
			AssertColoadShipmentsAreCreatedWhenSimpleTotalsValuesChangeInSubShipmentAcrossFactories(JobShipmentSchema.JS_ActualWeight, false);
		}

		public void TestColoadShipmentsAreCreatedWhenSimpleTotalsValuesChangeInSubShipmentAcrossFactories_Packs()
		{
			AssertColoadShipmentsAreCreatedWhenSimpleTotalsValuesChangeInSubShipmentAcrossFactories(JobShipmentSchema.JS_OuterPacks, true);
		}

		void AssertColoadShipmentsAreCreatedWhenSimpleTotalsValuesChangeInSubShipmentAcrossFactories(SchemaColumn field, bool convertToInt)
		{
			ColoadHelper.MasterShipmentOnX.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			ColoadHelper.MasterShipmentOnX.Factory.Save();

			ColoadHelper.SubShipmentOnY[field.Name] = ColoadHelper.ConvertToZIntOrZDecimal(130, convertToInt);
			ColoadHelper.SubShipmentOnY.JS_JS_ColoadMasterShipment = ColoadHelper.MasterShipmentOnX.PK;
			ColoadHelper.FactoryY.Save();

			ZGuid masterPK = ColoadHelper.MasterShipmentOnX.PK;
			ZGuid subShipmentPK = ColoadHelper.SubShipmentOnY.PK;

			ColoadHelper.ResetFactoryX();

			var loadedMaster = ColoadHelper.FactoryX.Load<CommonShipment>(masterPK);
			var loadedSub = ColoadHelper.FactoryX.Load<CommonShipment>(subShipmentPK);
			AssertEquals("Master volume is equal to sub after load", loadedMaster[field.Name], loadedMaster[field.Name]);

			loadedSub[field.Name] = ColoadHelper.ConvertToZIntOrZDecimal(150, convertToInt);
			ColoadHelper.FactoryX.Save();
			AssertEquals("Master " + field.Name + " is equal to sub which can only happpen of SubShipments ColoadShipments collection loads", loadedSub[field.Name], loadedMaster[field.Name]);
		}

		ColoadShipmentCollectionTest.ColoadShipmentTestHelper ColoadHelper
		{
			get
			{
				if (fColoadHelper == null)
				{
					fColoadHelper = new ColoadShipmentCollectionTest.ColoadShipmentTestHelper(Factory);
				}
				return fColoadHelper;
			}
		}
		ColoadShipmentCollectionTest.ColoadShipmentTestHelper fColoadHelper;

		public void TestCrossFactoryUpdatesRemoveShipmentsFromColoadCollectionWithoutException()
		{
			BusinessObjectFactory factoryX = new BusinessObjectFactory();
			BusinessObjectFactory factoryY = new BusinessObjectFactory();

			CommonConsol consol = factoryX.New<CommonConsol>();
			CommonShipment masterOnX = consol.Shipments.AddNew();
			CommonShipment subOnX = consol.Shipments.AddNew();
			masterOnX.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			subOnX.JS_JS_ColoadMasterShipment = masterOnX.PK;
			int readColoadShipmentsCollectionCountToLoadCollecton = masterOnX.CoLoadShipments.Count;
			AssertEquals("There should be one item on the collection", 1, readColoadShipmentsCollectionCountToLoadCollecton);
			AssertEquals("MasterOnX will be the coload master of SubOnX", masterOnX.PK, subOnX.JS_JS_ColoadMasterShipment);
			factoryX.Save();

			CommonShipment subOnY = factoryY.Load<CommonShipment>(subOnX.PK);
			AssertEquals("Sub on New factory should have same master", masterOnX.PK, subOnY.JS_JS_ColoadMasterShipment);
			subOnY.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			factoryY.Save();
			AssertEquals("Master on X coload shipments collection no longer contains sub", false, masterOnX.CoLoadShipments.Contains(subOnY.PK));
			subOnY.JS_JS_ColoadMasterShipment = masterOnX.PK;
			AssertEquals("Sub gets added back onto Master on Y coload shipments collection ", true, subOnY.CoLoadMasterShipment.CoLoadShipments.Contains(subOnY.PK));
			factoryY.Save();
			AssertEquals("Sub gets added back onto Master on X coload shipments collection ", true, masterOnX.CoLoadShipments.Contains(subOnY.PK));

			subOnX.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertEquals("Sub gets added back onto Master on X coload shipments collection because IsChangingColoadMaster flag is working properly", false, masterOnX.CoLoadShipments.Contains(subOnX.PK));
		}

		public void TestGetPksFromAllSubShipmentsWithoutChildren()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			var shipmentWithCLD = masterShipment.CoLoadShipments.AddNew();
			shipmentWithCLD.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			var childShipment1 = shipmentWithCLD.CoLoadShipments.AddNew();
			childShipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipmentWithASM = shipmentWithCLD.CoLoadShipments.AddNew();
			shipmentWithASM.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			var childShipment2 = shipmentWithASM.CoLoadShipments.AddNew();
			childShipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipmentWithCLB = shipmentWithASM.CoLoadShipments.AddNew();
			shipmentWithCLB.JS_ShipmentType = Core.Constants.ShipmentTypes.BlindCoLoadMaster;

			var childShipment3 = shipmentWithCLB.CoLoadShipments.AddNew();
			childShipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var expectedPks = new[]
			{
				childShipment1.PK, childShipment2.PK, childShipment3.PK
			};

			var atcualPks = masterShipment.GetPksFromAllSubShipmentsWithoutChildren();

			AssertContainsExactElementsInAnyOrder("Shoudl only get all related child shipment pks without these master pks", expectedPks, atcualPks);
		}

		#endregion

		#region Properties

		public void TestFreightPayableAt()
		{
			CommonShipment shipment = GetShipment();
			AssertNull(shipment.FreightPayableAt);

			shipment.JS_INCO = "FOB";
			AssertNull("IsCollect but Origin = null - therefore null", shipment.FreightPayableAt);

			shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals("IsCollect so result = Destination.Code", "USLAX", shipment.FreightPayableAt.RL_Code);

			shipment.JS_INCO = "CIF";
			AssertNull(shipment.FreightPayableAt);

			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals("IsCollect so result = Origin.Code", "AUSYD", shipment.FreightPayableAt.RL_Code);
		}

		#region Packlines Totals Units

		public void TestTotalsUnitDefaultFromCRegistry()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_UnitOfVolume = "";
			shipment.JS_UnitOfWeight = "";
			Env.Registry.FreightVolumeUnit = Enterprise.Core.Constants.Volume.CubicYards;
			AssertEquals("Volume Unit", Enterprise.Core.Constants.Volume.CubicYards, shipment.TotalPackLineVolumeUnit);
			Env.Registry.FreightWeightUnit = Enterprise.Core.Constants.Weight.OuncesTroy;
			AssertEquals("Weight Unit", Enterprise.Core.Constants.Weight.OuncesTroy, shipment.TotalPackLineWeightUnit);

			shipment.JS_UnitOfVolume = Enterprise.Core.Constants.Volume.CubicDecimetres;
			shipment.JS_UnitOfWeight = Enterprise.Core.Constants.Weight.Grams;
			AssertEquals("Volume Unit", Enterprise.Core.Constants.Volume.CubicDecimetres, shipment.TotalPackLineVolumeUnit);
			AssertEquals("Weight Unit", Enterprise.Core.Constants.Weight.Grams, shipment.TotalPackLineWeightUnit);
		}

		#endregion

		#region Test Buyers Consol Updating

		public void TestBuyersConsolMasterDoesNotUpdateBCNSubShipments()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			CommonShipment master = consol.Shipments.AddNew();
			CommonShipment sub1 = consol.Shipments.AddNew();
			CommonShipment sub2 = consol.Shipments.AddNew();
			CommonShipment sub3 = consol.Shipments.AddNew();
			CommonShipment sub4 = consol.Shipments.AddNew();

			master.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals("Sub house bills should NOT have their master JS_JS updated when Master is designated", ZGuid.Empty, sub1.JS_JS_ColoadMasterShipment);
			AssertEquals("Sub house bills should NOT have their master JS_JS updated when Master is designated", ZGuid.Empty, sub2.JS_JS_ColoadMasterShipment);
			AssertEquals("Sub house bills should NOT have their master JS_JS updated when Master is designated", ZGuid.Empty, sub3.JS_JS_ColoadMasterShipment);
			AssertEquals("Sub house bills should NOT have their master JS_JS updated when Master is designated", ZGuid.Empty, sub4.JS_JS_ColoadMasterShipment);

			master.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			AssertEquals("Sub house bills should have their master JS_JS cleared when Master is undesignated", ZGuid.Empty, sub1.JS_JS_ColoadMasterShipment);
			AssertEquals("Sub house bills should have their master JS_JS cleared when Master is undesignated", ZGuid.Empty, sub2.JS_JS_ColoadMasterShipment);
			AssertEquals("Sub house bills should have their master JS_JS cleared when Master is undesignated", ZGuid.Empty, sub3.JS_JS_ColoadMasterShipment);
			AssertEquals("Sub house bills should have their master JS_JS cleared when Master is undesignated", ZGuid.Empty, sub4.JS_JS_ColoadMasterShipment);
		}

		#endregion

		#region Chargeable Amount

		public void TestChargeableAmount()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_ActualVolume = 2m;
			shipment.JS_UnitFreightRate = 1.538m;
			ZDecimal amount = 2m * 1000000m / 6000m;
			amount = ZArchitecture.Core.Utilities.Round(amount, 3) * 1.538m;
			AssertEquals("Chargeable Amount is volume converted to weight * freightrate", ZArchitecture.Core.Utilities.Round(amount, 3), ZArchitecture.Core.Utilities.Round(shipment.ChargeableAmount, 3));

			shipment.JS_ActualWeight = 500m;
			amount = 500m * 1.538m;
			AssertEquals("Chargeable Amount is weight * freightrate", amount, shipment.ChargeableAmount);

			CommonShipment shipment2 = GetShipment();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_ActualWeight = 100m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.JS_UnitFreightRate = 1.538m;
			amount = 2m * 1.538m;
			AssertEquals("Chargeable Amount is volume converted to weight * freightrate", amount, shipment2.ChargeableAmount);

			shipment2.JS_ActualWeight = 5000m;
			amount = 5m * 1.538m;
			AssertEquals("Chargeable Amount is weight * freightrate", amount, shipment2.ChargeableAmount);
		}

		#endregion

		#region Location

		#region TestILocationConsumer

		public void TestILocationConsumer()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			var row = helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);
			Factory.Save();

			var whsLoc = (IWhsLocation)row.Locations[0];
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_WL = whsLoc.PK;
			var locationConsumer = (ILocationConsumer)shipment;

			AssertEquals(locationConsumer.LocationTypeForMessages, "");
			AssertEquals(locationConsumer.LocationPK, whsLoc.PK);

			locationConsumer.LocationTitle = "Test";
			AssertEquals(locationConsumer.LocationTitle, "Test");
		}

		#endregion

		public void TestLocationWhsGuidInfo()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			AssertNotNull(shipment.LocationWhsGuidInfo);
			AssertEquals("LocationWhsGuidInfo name", PackLocation.Schema.LocationWhsGuid, shipment.LocationWhsGuidInfo.Name);
		}

		public void TestJS_WL()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			var row = helper.CreateRowAndGenerateLocations(whs, "A", 4, 3, 2);
			var whsLoc = (IWhsLocation)row.Locations[23];
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_WL = whsLoc.PK;

			AssertEquals("Location1.LocationWhsGuid", whs.PK, shipment.LocationWhsGuid);
			AssertEquals("Location1.LocationString", "A-4-3-2", shipment.LocationString);
		}

		public void TestLocationString()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("1");
			var row = helper.CreateRowAndGenerateLocations(whs, "A", 4, 3, 2);
			var whsLoc = (IWhsLocation)row.Locations[23];
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_WL = whsLoc.PK;

			shipment.LocationWhsGuid = whs.PK;
			shipment.LocationString = "A-1-1-1";
			AssertEquals("LocationString", "A-1-1-1", shipment.LocationString);
		}

		public void TestLocationStringInfo()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();

			AssertNotNull(shipment.LocationStringInfo);
			AssertEquals("LocationStringInfo name", PackLocation.Schema.LocationString, shipment.LocationStringInfo.Name);
		}

		#endregion

		#endregion

		#region TransportMode

		public void TestJS_TransportMode()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Shipment packing mode should default to FCL", Enterprise.Core.Constants.ContainerModes.FCL, shipment.JS_PackingMode);
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Shipment packing mode should default to LSE", Enterprise.Core.Constants.ContainerModes.Loose, shipment.JS_PackingMode);
		}

		public void TestJS_TransporModeDefaultsFromSystemRegistry()
		{
			var setupCollection = new DefaultContainerModesCollection();
			var registryItem1 = new DefaultContainerModes();
			registryItem1.TransportMode = Constants.TransportModes.Air;
			registryItem1.ContainerMode = Constants.ContainerModes.ULD;
			setupCollection.Add(registryItem1);

			var registryItem2 = new DefaultContainerModes();
			registryItem2.TransportMode = Constants.TransportModes.Road;
			registryItem2.ContainerMode = Constants.ContainerModes.FTL;
			setupCollection.Add(registryItem2);
			AssertEquals("Should now be 2 default Container modes", 2, setupCollection.Count);

			FreightConfigurationRegistry.Instance.DefaultContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, setupCollection);
			var value = FreightConfigurationRegistry.Instance.DefaultContainerModes.Value;
			AssertEquals("Value.Count", 2, value.Count);

			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Shipment packing mode should default from Registry to FTL", Constants.ContainerModes.FTL, shipment.JS_PackingMode);
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Shipment packing mode should default from Registry ULD", Constants.ContainerModes.ULD, shipment.JS_PackingMode);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Shipment packing mode should default to FCL as normal", Constants.ContainerModes.FCL, shipment.JS_PackingMode);
		}

		#endregion

		#region JS_ShipmentType

		public void TestJS_ShipmentTypeDefault()
		{
			CommonShipment shipment = GetShipment();
			AssertEquals("Shipment type should default to STD", Constants.ShipmentTypes.StandardHouse, shipment.JS_ShipmentType);
		}

		public virtual void TestJS_ShipmentTypeBoolProperties()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertShipmentTypeBoolProperties(shipment, true, false, false, false, false, false, false, false, false, false);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertShipmentTypeBoolProperties(shipment, false, true, false, false, false, true, false, false, false, false);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			AssertShipmentTypeBoolProperties(shipment, false, false, true, false, false, true, false, false, false, false);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertShipmentTypeBoolProperties(shipment, false, false, false, true, false, true, false, false, false, false);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertShipmentTypeBoolProperties(shipment, false, false, false, false, true, true, false, false, false, false);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			AssertShipmentTypeBoolProperties(shipment, false, false, false, false, false, false, true, false, false, false);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertShipmentTypeBoolProperties(shipment, false, false, false, false, false, true, false, true, false, false);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertShipmentTypeBoolProperties(shipment, false, false, false, false, false, false, false, false, true, false);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueMaster;
			AssertShipmentTypeBoolProperties(shipment, false, false, false, false, false, true, false, false, false, true);
		}

		void AssertShipmentTypeBoolProperties(CommonShipment shipment, bool isStandard, bool isBCN, bool isSCN, bool isCoload, bool isAssembly, bool isLeadOrMaster, bool isHighVolumeLowValueLegacy, bool isBlindCoLoadMaster, bool isHighVolumeLowValue, bool isHighVolumeLowValueMaster)
		{
			AssertEquals("IsStandardHouse", isStandard, shipment.IsStandardHouse);
			AssertEquals("IsBuyersConsolLead", isBCN, shipment.IsBuyersConsolLead);
			AssertEquals("IsShippersConsolMaster", isSCN, shipment.IsShippersConsolLead);
			AssertEquals("IsCoLoadMaster", isCoload, shipment.IsCoLoadMaster);
			AssertEquals("IsAssemblyMaster", isAssembly, shipment.IsAssemblyMaster);
			AssertEquals("IsLeadOrMaster", isLeadOrMaster, shipment.IsLeadOrMaster);
			AssertEquals("IsHighVolumeLowValue", isHighVolumeLowValue, shipment.IsHighVolumeLowValue);
			AssertEquals("IsHighVolumeLowValueLegacy", isHighVolumeLowValueLegacy, shipment.IsHighVolumeLowValueLegacy);
			AssertEquals("IsHighVolumeLowValueMaster", isHighVolumeLowValueMaster, shipment.IsHighVolumeLowValueMaster);
			AssertEquals("IsBlindCoLoadMaster", isBlindCoLoadMaster, shipment.IsBlindCoLoadMaster);
		}

		public void TestSettingHighVolumeLowValueShipment()
		{
			foreach (var shipmentType in new[] { Constants.ShipmentTypes.HighVolumeLowValue, Constants.ShipmentTypes.HighVolumeLowValueLegacy })
			{
				var shipment = GetShipment();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var master = Factory.New<CommonShipment>();

				shipment.JS_JS_ColoadMasterShipment = master.PK;
				shipment.OuterPackLines.AddNew();
				shipment.InnerPackLines.AddNew();

				AssertEquals("prerequisite - shipment has outer packlines", true, shipment.OuterPackLines.Count > 0);
				AssertEquals("prerequisite - shipment has inner packlines", true, shipment.InnerPackLines.Count > 0);

				shipment.JS_ShipmentType = shipmentType;

				AssertEquals(string.Format("{0} shipment does support master / lead", shipmentType), master.PK, shipment.JS_JS_ColoadMasterShipment);
				AssertEquals(string.Format("{0} shipment does support outer packlines", shipmentType), 1, shipment.OuterPackLines.Count);
				AssertEquals(string.Format("{0} shipment does support inner packlines", shipmentType), 1, shipment.InnerPackLines.Count);

				AssertEquals(string.Format("{0} shipment does support master / lead", shipmentType), false, shipment.JS_JS_ColoadMasterShipmentInfo.ReadOnly);
				AssertEquals(string.Format("{0} shipment does support outer packlines", shipmentType), false, shipment.OuterPackLines.ReadOnly);
				AssertEquals(string.Format("{0} shipment does support inner packlines", shipmentType), false, shipment.InnerPackLines.ReadOnly);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
				shipment.CoLoadShipments.AddNew();

				AssertEquals("prerequisite - shipment has coloads", true, shipment.CoLoadShipments.Count > 0);

				shipment.JS_ShipmentType = shipmentType;

				AssertEquals(string.Format("{0} shipment does not support coload shipments", shipmentType), 0, shipment.CoLoadShipments.Count);
				AssertEquals(string.Format("{0} shipment does not support coload shipments", shipmentType), true, shipment.CoLoadShipments.ReadOnly);
			}
		}

		#endregion

		#region Org Fetcher Testing

		#region Pickup & Delivery Address

		public void TestJobDocsAndCartagePickupAddressPK()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			OrgAddress pickupAddress = Factory.New<OrgAddress>();
			pickupAddress.OA_Address1 = "Pickup Address 1";
			pickupAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));

			OrgHeader orgH = Factory.New<OrgHeader>();
			orgH.Addresses.Add(pickupAddress);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = orgH.PK;
			AssertEquals("Pickup Address should be set.", pickupAddress.PK, shipment.ConsignorPickupAddress.E2_OA_Address);
			shipment.ConsignorPK = ZGuid.Empty;
			AssertEquals("Clear Pickup Address when Consignor changed.", true, shipment.ConsignorPickupAddress.E2_OA_Address.IsEmpty);
		}

		public void TestJobDocsAndCartageDeliveryAddressPK()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			OrgAddress deliveryAddress = Factory.New<OrgAddress>();
			deliveryAddress.OA_Address1 = "Delivery Address 1";
			deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.DLV));

			OrgHeader orgH = Factory.New<OrgHeader>();
			orgH.Addresses.Add(deliveryAddress);

			shipment.ConsigneePK = orgH.PK;
			AssertEquals("Delivery Address should be set.", deliveryAddress.PK, shipment.ConsigneeDeliveryAddress.E2_OA_Address);
			shipment.ConsigneePK = ZGuid.Empty;
			AssertEquals("Clear Delivery Address when Consignee changed.", true, shipment.ConsigneeDeliveryAddress.E2_OA_Address.IsEmpty);
		}

		#endregion

		#region TestConsigneePK_Set Stuff

		public void TestConsigneePK_Set_ImporterBroker()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			// ensure no ImportBroker is set for a random Guid
			shipment.ConsigneePK = ZGuid.NewZGuid();
			Assert(shipment.JS_OH_ImportBroker.IsEmpty);

			FreightTestHelper.FreightImportPKs testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsigneePK = testPKs.OrgHeader;
			AssertEquals("Shipment should have AIR customs broker", testPKs.AirImportCustomsBroker, shipment.JS_OH_ImportBroker);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.ConsigneePK = testPKs.OrgHeader;
			AssertEquals("Shipment should have SEA customs broker", testPKs.SeaImportCustomsBroker, shipment.JS_OH_ImportBroker);
		}

		public void TestConsigneePK_Set_ImporterBroker_OnlyOnce()
		{
			var importPKs = FreightTestHelper.CreateImportOrgMiscServInDB();

			var notifyPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			notifyPartyOrg.OH_Code = "NOTIFYORG";

			var contact = notifyPartyOrg.Contacts.AddNew();
			contact.OC_ContactName = "TEST";

			var shipment = GetShipment();
			shipment.FillWithValidTestData();

			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsigneePK = importPKs.OrgHeader;
			shipment.NotifyPartyContactPK = contact.PK;

			Factory.Save();

			var newFactory = NewFactory();
			var newShipment = newFactory.Load<CommonShipment>(shipment.PK);

			var hitCount = 0;
			newShipment.OnExportOrImportBrokerUpdate += (sender, args) =>
			{
				hitCount++;
			};

			AssertEquals("Should default to zero", 0, hitCount);
			AssertNotNull("For initialise events, should not null", newShipment.ConsigneeDocumentaryAddress);

			newShipment.JS_OH_ImportBroker = ZGuid.Empty;
			newShipment.ConsigneePK = ZGuid.Empty;
			AssertEquals("Should not call as consignee is null", 0, hitCount);

			newShipment.ConsigneePK = importPKs.OrgHeader;
			AssertEquals("Should call only once", 1, hitCount);
		}

		public void TestConsigneePK_Set_ImporterBroker_Via_DocumentaryAddress()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			// ensure no ImportBroker is set for a random Guid
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.NewZGuid();
			Assert(shipment.JS_OH_ImportBroker.IsEmpty);

			FreightTestHelper.FreightImportPKs testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = testPKs.OrgHeader;
			AssertEquals("Shipment should have AIR customs broker", testPKs.AirImportCustomsBroker, shipment.JS_OH_ImportBroker);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = testPKs.OrgHeader;
			AssertEquals("Shipment should have SEA customs broker", testPKs.SeaImportCustomsBroker, shipment.JS_OH_ImportBroker);
		}

		public void TestConsigneePK_Set_ImporterBroker_Via_DocumentaryAddress_AfterFirstSaving()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.OnExportOrImportBrokerUpdate += (sender, e) => { e.ShouldUpdateBroker = true; };

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.NewZGuid();
			Assert(shipment.JS_OH_ImportBroker.IsEmpty);

			Factory.Save();

			var testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = testPKs.OrgHeader;
			AssertEquals("Shipment should have AIR customs broker", testPKs.AirImportCustomsBroker, shipment.JS_OH_ImportBroker);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = testPKs.OrgHeader;
			AssertEquals("Shipment should have SEA customs broker", testPKs.SeaImportCustomsBroker, shipment.JS_OH_ImportBroker);

			var anotherSeaBroker = CreateOrgHeader().PK;
			var anotherConsignee = Factory.New<OrgHeader>();
			anotherConsignee.OH_Code = "--CNOR--";
			anotherConsignee.OH_FullName = "Test Consignee";
			anotherConsignee.MainAddress.OA_Address1 = "Consignee Address";
			anotherConsignee.OH_IsConsignee = true;
			anotherConsignee.OH_RL_NKClosestPort = "GBLON";
			anotherConsignee.SetRelatedParty(anotherSeaBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = anotherConsignee.PK;
			AssertEquals("Shipment should have updated Importer", anotherConsignee.PK, shipment.ConsigneePK);
			AssertEquals("Shipment should have updated SEA customs broker", anotherSeaBroker, shipment.JS_OH_ImportBroker);
		}

		public void TestConsigneePK_Set_NotShowPromptUpdatingBroker_WhenBrokerNoNeedChange()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = OverseasPort2;

			var hitCount = 0;
			shipment.OnExportOrImportBrokerUpdate += (sender, e) => { hitCount++; };

			shipment.ConsigneePK = ZGuid.NewZGuid();
			Factory.Save();

			Assert(shipment.JS_OH_ImportBroker.IsEmpty);
			AssertEquals(0, hitCount);

			FreightTestHelper.FreightImportPKs testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsigneePK = testPKs.OrgHeader;
			Assert(shipment.JS_OH_ImportBroker.IsEmpty);
			AssertEquals("Not show prompt when broker is no need change", 0, hitCount);
		}

		public void TestConsigneePK_NotSet_ImporterBroker_Via_DocumentaryAddress_WhenConsignorPKChanged()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			shipment.OnExportOrImportBrokerUpdate += (sender, e) => { e.ShouldUpdateBroker = false; };

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.NewZGuid();
			Assert(shipment.JS_OH_ImportBroker.IsEmpty);

			Factory.Save();

			var testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = testPKs.OrgHeader;
			Assert("Shipment should not have import broker", shipment.JS_OH_ImportBroker.IsEmpty);

			var anotherSeaBroker = CreateOrgHeader().PK;
			var anotherConsignor = Factory.New<OrgHeader>();
			anotherConsignor.OH_Code = "--CNOR--";
			anotherConsignor.OH_FullName = "Test Consignor";
			anotherConsignor.MainAddress.OA_Address1 = "Consignor Address";
			anotherConsignor.OH_IsConsignor = true;
			anotherConsignor.OH_RL_NKClosestPort = "GBLON";
			anotherConsignor.SetRelatedParty(anotherSeaBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = anotherConsignor.PK;
			AssertEquals("Shipment should have updated Exporter", anotherConsignor.PK, shipment.ConsignorPK);

			Assert("Shipment should not have import broker", shipment.JS_OH_ImportBroker.IsEmpty);
		}

		public void TestConsigneePK_NotSet_ImporterBroker_Via_E2_OA_Address()
		{
			var shipment = GetShipment();

			shipment.ConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += (sender, e) =>
			{
				AssertEquals(true, shipment.IsChangingConsigneeAddress);
			};

			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.OnExportOrImportBrokerUpdate += (sender, e) => { e.ShouldUpdateBroker = true; };

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.NewZGuid();
			Assert(shipment.JS_OH_ImportBroker.IsEmpty);

			var testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var mainHeader = Factory.Load<OrgHeader>(testPKs.OrgHeader);

			AssertEquals(false, shipment.IsChangingConsigneeAddress);
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = mainHeader.PK;
			AssertEquals(false, shipment.IsChangingConsigneeAddress);
			Assert(shipment.JS_OH_ImportBroker.IsEmpty);
		}

		public void TestConsigneePK_Set_DeliveryEquipmentNeeded()
		{
			AssertConsigneePK_Set_DeliveryEquipmentNeeded(false);
		}

		public void TestConsigneePK_Set_DeliveryEquipmentNeeded_Via_DocumentaryAddress()
		{
			AssertConsigneePK_Set_DeliveryEquipmentNeeded(true);
		}

		void AssertConsigneePK_Set_DeliveryEquipmentNeeded(bool setViaDocumentaryAddress)
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = consignee.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_Address1 = "Neverland";
			address.OA_AIREquipmentNeeded = "AAA";
			address.OA_FCLEquipmentNeeded = "BBB";
			address.OA_LCLEquipmentNeeded = "CCC";

			Func<string, string, CommonShipment> createShipment = (transportMode, packingMode) =>
			{
				CommonShipment newShipment = Factory.New<CommonShipment>();
				newShipment.JS_TransportMode = transportMode;
				newShipment.JS_PackingMode = packingMode;
				return newShipment;
			};

			// AIR
			CommonShipment shipment = createShipment(Constants.TransportModes.Air, "");
			SetConsigneeAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignee, "AAA");

			// FCL
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			SetConsigneeAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignee, "BBB");

			// LCL
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			SetConsigneeAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignee, "CCC");

			//BCN
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.BuyersConsol);
			SetConsigneeAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignee, "BBB");

			// Booking
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			shipment.JS_IsBooking = true;
			SetConsigneeAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignee, "CCC");

			// Booking, with ASK mode => setting to blank as it not supported for bookings
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			shipment.JS_IsBooking = true;

			address.OA_LCLEquipmentNeeded = Constants.EquipmentNeeded.Ask;
			SetConsigneeAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignee, "");
		}

		void SetConsigneeAndAssertPickupEquipmentNeeded(CommonShipment shipment, bool setViaDocumentaryAddress, OrgHeader consignee, string expectedEquipment)
		{
			AssertEquals("Precondition", "", shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			if (setViaDocumentaryAddress)
			{
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			}
			else
			{
				shipment.ConsigneePK = consignee.PK;
			}
			AssertEquals("FCL delivery equipment should be defaulted", expectedEquipment, shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);

			if (setViaDocumentaryAddress)
			{
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			}
			else
			{
				shipment.ConsigneePK = ZGuid.Empty;
			}
			AssertEquals("FCL delivery equipment should be empty", "", shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
		}

		#endregion

		#region TestConsignorPK_Set Stuff

		public void TestConsignorPK_Set()
		{
			CommonShipment shipment = GetShipment();
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency.ToString());

			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "";

			// reset properties for testing
			shipment.JS_OH_ExportBroker = ZGuid.Empty;
			shipment.DocsAndCartage.PickupCartageCoPK = ZGuid.Empty;
			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			shipment.ConsignorPK = ZGuid.Empty; // important to do this last as it may change the above properties!
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);
			Assert(shipment.JS_RX_NKGoodsValueCurr.IsEmpty);
			Assert(shipment.ConsignorPK.IsEmpty);

			// ensure no broker / cartage is set for a random Guid
			shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);

			// ensure goods currency is set to default LocalCurrency for the random Guid
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, shipment.JS_RX_NKGoodsValueCurr);

			// ensure the broker / cartage / goods currency are set correctly
			FreightTestHelper.FreightExportPKs testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			shipment.ConsignorPK = testPKs.OrgHeader;
			AssertEquals("Shipment should have an Exporter", testPKs.OrgHeader, shipment.ConsignorPK);
			AssertEquals("Shipment should have SEA customs broker", testPKs.SeaExportCustomsBroker, shipment.JS_OH_ExportBroker);
			AssertEquals("Shipment should have LCL Export Cartage", testPKs.LCLExpCartage, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("Shipment should have Goods Currency Unit", testPKs.Currency, shipment.GoodsValueCurr.PK);
		}

		public void TestConsignorPK_Set_Via_DocumentaryAddress()
		{
			CommonShipment shipment = GetShipment();
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency.ToString());

			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "";

			// reset properties for testing
			shipment.JS_OH_ExportBroker = ZGuid.Empty;
			shipment.DocsAndCartage.PickupCartageCoPK = ZGuid.Empty;
			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty; // important to do this last as it may change the above properties!
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);
			Assert(shipment.JS_RX_NKGoodsValueCurr.IsEmpty);
			Assert(shipment.ConsignorPK.IsEmpty);

			// ensure no broker / cartage is set for a random Guid
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);

			// ensure goods currency is set to default LocalCurrency for the random Guid
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, shipment.JS_RX_NKGoodsValueCurr);

			// ensure the broker / cartage / goods currency are set correctly
			FreightTestHelper.FreightExportPKs testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = testPKs.OrgHeader;
			AssertEquals("Shipment should have an Exporter", testPKs.OrgHeader, shipment.ConsignorPK);
			AssertEquals("Shipment should have SEA customs broker", testPKs.SeaExportCustomsBroker, shipment.JS_OH_ExportBroker);
			AssertEquals("Shipment should have LCL Export Cartage", testPKs.LCLExpCartage, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("Shipment should have Goods Currency Unit", testPKs.Currency, shipment.GoodsValueCurr.PK);
		}

		public void TestConsignorPK_Set_Via_DocumentaryAddress_AfterFirstSaving()
		{
			var shipment = GetShipment();
			var filter = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency.ToString());

			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "";
			shipment.OnExportOrImportBrokerUpdate += (sender, e) => { e.ShouldUpdateBroker = true; };

			shipment.JS_OH_ExportBroker = ZGuid.Empty;
			shipment.DocsAndCartage.PickupCartageCoPK = ZGuid.Empty;
			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);
			Assert(shipment.JS_RX_NKGoodsValueCurr.IsEmpty);
			Assert(shipment.ConsignorPK.IsEmpty);

			Factory.Save();

			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, shipment.JS_RX_NKGoodsValueCurr);

			var testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = testPKs.OrgHeader;
			AssertEquals("Shipment should have an Exporter", testPKs.OrgHeader, shipment.ConsignorPK);
			AssertEquals("Shipment should have SEA customs broker", testPKs.SeaExportCustomsBroker, shipment.JS_OH_ExportBroker);
			AssertEquals("Shipment should have LCL Export Cartage", testPKs.LCLExpCartage, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("Shipment should have Goods Currency Unit", testPKs.Currency, shipment.GoodsValueCurr.PK);

			var anotherSeaBroker = CreateOrgHeader().PK;
			var anotherConsignor = Factory.New<OrgHeader>();
			anotherConsignor.OH_Code = "--CNOR--";
			anotherConsignor.OH_FullName = "Test Consignor";
			anotherConsignor.MainAddress.OA_Address1 = "Consignor Address";
			anotherConsignor.OH_IsConsignor = true;
			anotherConsignor.OH_RL_NKClosestPort = "GBLON";
			anotherConsignor.SetRelatedParty(anotherSeaBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = anotherConsignor.PK;
			AssertEquals("Shipment should have updated Exporter", anotherConsignor.PK, shipment.ConsignorPK);
			AssertEquals("Shipment should have updated SEA customs broker", anotherSeaBroker, shipment.JS_OH_ExportBroker);
		}

		public void TestConsignorPK_Set_NotShowPromptUpdatingBroker_WhenBrokerNoNeedChange()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = OverseasPort;

			var hitCount = 0;
			shipment.OnExportOrImportBrokerUpdate += (sender, e) => { hitCount++; };

			shipment.ConsignorPK = ZGuid.NewZGuid();
			Factory.Save();

			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			AssertEquals(0, hitCount);

			var testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsignorPK = testPKs.OrgHeader;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			AssertEquals("Not show prompt when broker is no need change", 0, hitCount);
		}

		public void TestConsignorPK_NotSet_Via_DocumentaryAddress_WhenConsigneePKChanged()
		{
			var shipment = GetShipment();
			var filter = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency.ToString());

			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "";
			shipment.OnExportOrImportBrokerUpdate += (sender, e) => { e.ShouldUpdateBroker = false; };

			shipment.JS_OH_ExportBroker = ZGuid.Empty;
			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.JS_RX_NKGoodsValueCurr.IsEmpty);
			Assert(shipment.ConsignorPK.IsEmpty);

			Factory.Save();

			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, shipment.JS_RX_NKGoodsValueCurr);

			var testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = testPKs.OrgHeader;
			AssertEquals("Shipment should have an Exporter", testPKs.OrgHeader, shipment.ConsignorPK);
			Assert("Shipment should not have export broker", shipment.JS_OH_ExportBroker.IsEmpty);
			AssertEquals("Shipment should have Goods Currency Unit", testPKs.Currency, shipment.GoodsValueCurr.PK);

			var anotherSeaBroker = CreateOrgHeader().PK;
			var anotherConsignee = Factory.New<OrgHeader>();
			anotherConsignee.OH_Code = "--CNOR--";
			anotherConsignee.OH_FullName = "Test Consignee";
			anotherConsignee.MainAddress.OA_Address1 = "Consignee Address";
			anotherConsignee.OH_IsConsignee = true;
			anotherConsignee.OH_RL_NKClosestPort = "GBLON";
			anotherConsignee.SetRelatedParty(anotherSeaBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = anotherConsignee.PK;
			AssertEquals("Shipment should have updated Importer", anotherConsignee.PK, shipment.ConsigneePK);

			AssertEquals("Shipment should have Exporter", testPKs.SeaExportCustomsBroker, shipment.JS_OH_ExportBroker);
		}

		public void TestConsignorPK_NotSet_ExportBroker_Via_E2_OA_Address()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "";
			shipment.OnExportOrImportBrokerUpdate += (sender, e) => { e.ShouldUpdateBroker = true; };

			shipment.JS_OH_ExportBroker = ZGuid.Empty;
			shipment.DocsAndCartage.PickupCartageCoPK = ZGuid.Empty;
			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);
			Assert(shipment.JS_RX_NKGoodsValueCurr.IsEmpty);
			Assert(shipment.ConsignorPK.IsEmpty);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);

			var testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			var mainHeader = Factory.Load<OrgHeader>(testPKs.OrgHeader);

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = mainHeader.PK;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
		}

		public void TestConsignorPK_Set_ExporterOrigin()
		{
			ZString location = "L1111";

			CommonShipment shipment = GetShipment();
			ZGuid exporterPK = FreightTestHelper.CreateOrgHeaderInDB(1, location);
			ZString countryCode = FreightTestHelper.CreateCountry(1);
			FreightTestHelper.CreateUNLocoInDB(location, countryCode, 1);

			Assert(shipment.ConsignorPK.IsEmpty);

			shipment.ConsignorPK = exporterPK;
			AssertEquals("Shipment should have Exporter", exporterPK, shipment.ConsignorPK);
			AssertEquals("Shipment should have Origin", location, shipment.JS_RL_NKOrigin);
		}

		public void TestConsignorPK_Set_ExporterOrigin_Via_DocumentaryAddress()
		{
			ZString location = "L1111";

			CommonShipment shipment = GetShipment();
			ZGuid exporterPK = FreightTestHelper.CreateOrgHeaderInDB(1, location);
			ZString countryCode = FreightTestHelper.CreateCountry(1);
			FreightTestHelper.CreateUNLocoInDB(location, countryCode, 1);

			Assert(shipment.ConsignorPK.IsEmpty);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = exporterPK;
			AssertEquals("Shipment should have Exporter", exporterPK, shipment.ConsignorPK);
			AssertEquals("Shipment should have Origin", location, shipment.JS_RL_NKOrigin);
		}

		public void TestConsignorPK_Set_PickupEquipmentNeeded()
		{
			AssertConsignorPK_Set_PickupEquipmentNeeded(false);
		}

		public void TestConsignorPK_Set_PickupEquipmentNeeded_Via_DocumentaryAddress()
		{
			AssertConsignorPK_Set_PickupEquipmentNeeded(true);
		}

		void AssertConsignorPK_Set_PickupEquipmentNeeded(bool setViaDocumentaryAddress)
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = consignor.Addresses.AddNew(OrgAddressType.Pickup, true);
			address.OA_Address1 = "Neverland";
			address.OA_AIREquipmentNeeded = "AAA";
			address.OA_FCLEquipmentNeeded = "BBB";
			address.OA_LCLEquipmentNeeded = "CCC";

			Func<string, string, CommonShipment> createShipment = (transportMode, packingMode) =>
			{
				CommonShipment newShipment = Factory.New<CommonShipment>();
				newShipment.JS_TransportMode = transportMode;
				newShipment.JS_PackingMode = packingMode;
				return newShipment;
			};

			// AIR
			CommonShipment shipment = createShipment(Constants.TransportModes.Air, "");
			SetConsignorAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignor, "AAA");

			// FCL
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			SetConsignorAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignor, "BBB");

			// LCL
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			SetConsignorAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignor, "CCC");

			//BCN
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.BuyersConsol);
			SetConsignorAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignor, "CCC");

			// Booking
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			shipment.JS_IsBooking = true;
			SetConsignorAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignor, "CCC");

			// Booking, with ASK mode => setting to blank as it not supported for bookings
			shipment = createShipment(Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			shipment.JS_IsBooking = true;

			address.OA_LCLEquipmentNeeded = Constants.EquipmentNeeded.Ask;
			SetConsignorAndAssertPickupEquipmentNeeded(shipment, setViaDocumentaryAddress, consignor, "");
		}

		void SetConsignorAndAssertPickupEquipmentNeeded(CommonShipment shipment, bool setViaDocumentaryAddress, OrgHeader consignor, string expectedEquipment)
		{
			AssertEquals("Precondition", "", shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			if (setViaDocumentaryAddress)
			{
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			}
			else
			{
				shipment.ConsignorPK = consignor.PK;
			}
			AssertEquals("FCL pickup equipment should be defaulted", expectedEquipment, shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			if (setViaDocumentaryAddress)
			{
				shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			}
			else
			{
				shipment.ConsignorPK = ZGuid.Empty;
			}
			AssertEquals("FCL pickup equipment should be empty", "", shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded);
		}

		#endregion

		#region TestJobDocsAndCartageRegistersChanges

		public void TestJobDocsAndCartageRegistersChanges()
		{
			CommonShipment s1 = GetExportShipment(typeof(CommonShipment));
			s1.JS_TransportMode = Constants.TransportModes.Sea;
			s1.JS_PackingMode = Constants.ContainerModes.LCL;
			s1.ConsignorPickupAddress.E2_OA_Address = s1.Consignor.MainAddress.PK;

			AssertEquals("S1.ConsignorPickupAddress.E2_OA_Address should be Consignor's main address.", s1.Consignor.MainAddress.PK, s1.ConsignorPickupAddress.E2_OA_Address);
		}

		#endregion

		#endregion

		#region ICancellable

		public void TestCanCancel()
		{
			var shipment = CommonShipment.New(Factory);
			AssertEquals("CanCancel", ZString.Empty, shipment.CanCancel());

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;
			Factory.Save();
			AssertEquals("CanCancel", ZString.Empty, shipment.CanCancel());

			var universalMessage = shipment.Messages.AddNew();
			universalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			AssertEquals("CanCancel", ZString.Empty, shipment.CanCancel());

			var otherMessage = shipment.Messages.AddNew();
			otherMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			AssertEquals("Cannot cancel: custom messages attached", "You cannot deactivate " + shipment.HumanReadableName + " since it has customs messages attached.", shipment.CanCancel());

			var universalMessage1 = shipment.Messages.AddNew();
			universalMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			AssertEquals("Cannot cancel: custom messages attached", "You cannot deactivate " + shipment.HumanReadableName + " since it has customs messages attached.", shipment.CanCancel());

			shipment.Messages.RemoveAndDeleteAll();
			Factory.Save();

			var cargoIMP2Message = shipment.Messages.AddNew();
			cargoIMP2Message.EM_ApplicationCode = ApplicationCodeList.Codes.CargoIMPPhase2;
			AssertEquals("CanCancel", ZString.Empty, shipment.CanCancel());

			var otherMessage1 = shipment.Messages.AddNew();
			otherMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			AssertEquals("Cannot cancel: custom messages attached", "You cannot deactivate " + shipment.HumanReadableName + " since it has customs messages attached.", shipment.CanCancel());

			var cargoIMP2Message1 = shipment.Messages.AddNew();
			cargoIMP2Message1.EM_ApplicationCode = ApplicationCodeList.Codes.CargoIMPPhase2;
			AssertEquals("Cannot cancel: custom messages attached", "You cannot deactivate " + shipment.HumanReadableName + " since it has customs messages attached.", shipment.CanCancel());

			shipment.Messages.RemoveAndDeleteAll();
			Factory.Save();

			var xmsMessage = shipment.Messages.AddNew();
			xmsMessage.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
			AssertEquals("CanCancel", ZString.Empty, shipment.CanCancel());

			var otherMessage2 = shipment.Messages.AddNew();
			otherMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			AssertEquals("Cannot cancel: custom messages attached", "You cannot deactivate " + shipment.HumanReadableName + " since it has customs messages attached.", shipment.CanCancel());

			var xmsMessage1 = shipment.Messages.AddNew();
			xmsMessage1.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
			AssertEquals("Cannot cancel: custom messages attached", "You cannot deactivate " + shipment.HumanReadableName + " since it has customs messages attached.", shipment.CanCancel());
		}

		public void TestCanCancel_MasterShipment()
		{
			var masterShipment = Factory.New<CommonShipment>();
			var subShipment = Factory.New<CommonShipment>();

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("Can cancel empty master", ZString.Empty, masterShipment.CanCancel());

			masterShipment.CoLoadShipments.Add(subShipment);
			Factory.Save();
			AssertEquals("Cannot cancel: Master / Lead shipment", "You cannot deactivate " + masterShipment.HumanReadableName + " since it is a Master / Lead shipment with sub-shipments attached.", masterShipment.CanCancel());
		}

		public void TestCanCancel_CurrentCompanyJob()
		{
			var shipment = CommonShipment.New(Factory);
			AssertEquals("CanCancel", ZString.Empty, shipment.CanCancel());

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;
			job.JH_JobNum = "JobNum";
			Factory.Save();
			AssertEquals("CanCancel", ZString.Empty, shipment.CanCancel());
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			charge.JR_OSSellAmt = 0m;
			Factory.Save();

			var expectedMessage = $@"{shipment.HumanReadableName.ToString()} cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header (JobNum) in the company EDI.";

			AssertEquals("Cannot cancel: job with non-zero job charge", expectedMessage, shipment.CanCancel());

			var accrual = Factory.Load<AccTransactionLines>(charge.JR_AL_APLine);
			AssertNotNull("Should have an Accrual", accrual);
			Assert("Accrual should not be reversed", accrual.AL_ReverseDate.IsEmpty);
			charge.JR_OSCostAmt = 0m;
			Factory.Save();

			Assert("Accrual should be reversed", !accrual.AL_ReverseDate.IsEmpty);
			AssertEquals("Cannot cancel: job with a zero job charge and reversed accrual", expectedMessage, shipment.CanCancel());
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCanCancel_ForeignCompanyJob()
		{
			var foreignCompany = Factory.New<GlbCompany>();
			foreignCompany.GC_Code = "BLA";
			Factory.Save();

			var shipment = CommonShipment.New(Factory);
			AssertEquals("CanCancel", ZString.Empty, shipment.CanCancel());

			var foreignJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			foreignJob.JH_ParentTableCode = "JS";
			foreignJob.JH_ParentID = shipment.PK;
			foreignJob.JH_GC = foreignCompany.PK;
			foreignJob.JH_JobNum = "JobNum";
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = foreignJob.PK;
			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			charge.JR_OSSellAmt = 0m;
			Factory.Save();

			var expectedMessage = $@"{shipment.HumanReadableName.ToString()} cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header (JobNum) in the company {foreignCompany.GC_Code}.";

			AssertEquals("Cannot cancel: foreign job with non-zero charge", expectedMessage, shipment.CanCancel());

			var accrual = Factory.Load<AccTransactionLines>(charge.JR_AL_APLine);
			AssertNotNull("Should have an Accrual", accrual);
			Assert("Accrual should not be reversed", accrual.AL_ReverseDate.IsEmpty);
			charge.JR_OSCostAmt = 0m;
			Factory.Save();

			Assert("Accrual should be reversed", !accrual.AL_ReverseDate.IsEmpty);
			AssertEquals("Cannot cancel: job with a zero job charge and reversed accrual", expectedMessage, shipment.CanCancel());
		}

		public void TestCanReactivate()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			AssertNull("Can always reactivate", shipment.CanReactivate());
		}

		public void TestReadOnlySetWhenCancelled()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			shipment.JS_IsCancelled = true;
			foreach (ZPropertyInfo property in shipment.ZPropertyInfoHash)
			{
				AssertEquals("All properties should be read-only, no exception", true, property.ReadOnly);
			}
			shipment.JS_IsCancelled = false;
			AssertEquals(false, shipment.JS_IsBookingInfo.ReadOnly);
		}

		public void TestCanCancelShipmentWithConsol()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			Assert("Expected no related consols", shipment.Consols.Count == 0);
			Assert("Expected not to cancel shipment without consol", string.IsNullOrEmpty(shipment.CanCancel()));

			CommonConsol consol = shipment.Consols.AddNew();

			Assert("Expected 1 related consol", shipment.Consols.Count == 1);
			Assert("Expected to cancel shipment with consol", !string.IsNullOrEmpty(shipment.CanCancel()));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			shipment.Consols.Remove(consol);

			Assert("Expected no related consols", shipment.Consols.Count == 0);
			Assert("Expected not to cancel shipment without consol", string.IsNullOrEmpty(shipment.CanCancel()));
		}

		#endregion

		#region Internal Cartage Properties

		public void TestExportDepotAddress()
		{
			CommonShipment shipment = GetExportShipment2(typeof(CommonShipment));
			AssertEquals("Expecting Shipment's ExportDepotAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartagePickupDepotAddress);
			CommonConsol consol = GetExportConsol(typeof(CommonConsol));
			consol.Shipments.Add(shipment);
			AssertEquals("Expecting Shipment's ExportDepotAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartagePickupDepotAddress);
			consol.JK_OA_PackDepotAddress = LocalDepot.MainAddress.PK;
			AssertEquals("Expecting Shipment's ExportDepotAddress to be local depot", LocalDepot.MainAddress.PK, ((IHaveInternalCartage)shipment).CartagePickupDepotAddress);
		}

		public void TestExportDepotAddressOverrideCFS()
		{
			CommonShipment shipment = GetExportShipment2(typeof(CommonShipment));
			CommonConsol consol = GetExportConsol(typeof(CommonConsol));
			consol.Shipments.Add(shipment);
			consol.JK_OA_PackDepotAddress = LocalDepot.MainAddress.PK;
			AssertEquals("Expecting Shipment's ExportDepotAddress to be local depot", LocalDepot.MainAddress.PK, ((IHaveInternalCartage)shipment).CartagePickupDepotAddress);

			shipment.JS_OA_ExportReceivingDepot = AlternateLocalDepot.MainAddress.PK;
			AssertEquals("Expecting Shipment's ExportDepotAddress to be local depot", AlternateLocalDepot.MainAddress.PK, ((IHaveInternalCartage)shipment).CartagePickupDepotAddress);

			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			AssertEquals("Expecting Shipment's ExportDepotAddress to be local depot", LocalDepot.MainAddress.PK, ((IHaveInternalCartage)shipment).CartagePickupDepotAddress);
		}

		public void TestImportDepotAddress()
		{
			CommonShipment shipment = GetImportShipment2(typeof(CommonShipment));
			AssertEquals("Expecting Shipment's ImportDepotAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartageDeliveryDepotAddress);
			CommonConsol consol = GetImportConsol(typeof(CommonConsol));
			consol.Shipments.Add(shipment);
			AssertEquals("Expecting Shipment's ImportDepotAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartageDeliveryDepotAddress);
			consol.JK_OA_UnpackDepotAddress = LocalDepot.MainAddress.PK;
			AssertEquals("Expecting Shipment's ImportDepotAddress to be local depot", LocalDepot.MainAddress.PK, ((IHaveInternalCartage)shipment).CartageDeliveryDepotAddress);
		}

		public void TestImportDepotAddressOverrideCFS()
		{
			CommonShipment shipment = GetImportShipment2(typeof(CommonShipment));
			CommonConsol consol = GetImportConsol(typeof(CommonConsol));
			consol.Shipments.Add(shipment);
			consol.JK_OA_UnpackDepotAddress = LocalDepot.MainAddress.PK;
			AssertEquals("Expecting Shipment's ImportDepotAddress to be local depot", LocalDepot.MainAddress.PK, ((IHaveInternalCartage)shipment).CartageDeliveryDepotAddress);

			shipment.JS_OA_ImportReleaseDepot = AlternateLocalDepot.MainAddress.PK;
			AssertEquals("Expecting Shipment's ExportDepotAddress to be local depot", AlternateLocalDepot.MainAddress.PK, ((IHaveInternalCartage)shipment).CartageDeliveryDepotAddress);

			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			AssertEquals("Expecting Shipment's ExportDepotAddress to be local depot", LocalDepot.MainAddress.PK, ((IHaveInternalCartage)shipment).CartageDeliveryDepotAddress);
		}

		public void TestExportCTOAddress()
		{
			CommonShipment shipment = GetExportShipment2(typeof(CommonShipment));
			AssertEquals("Expecting Shipment's ExportCTOAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartagePickupCTOAddress);
			CommonConsol consol = GetExportConsol(typeof(CommonConsol));
			consol.Shipments.Add(shipment);
			AssertEquals("Expecting Shipment's ExportCTOAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartagePickupCTOAddress);
			consol.JK_OA_DepartureCTOAddress = LocalCTO.MainAddress.PK;
			AssertEquals("Expecting Shipment's ExportCTOAddress to be local CTO", LocalCTO.MainAddress.PK, ((IHaveInternalCartage)shipment).CartagePickupCTOAddress);
		}

		public void TestImportCTOAddress()
		{
			CommonShipment shipment = GetImportShipment2(typeof(CommonShipment));
			AssertEquals("Expecting Shipment's ImportCTOAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartageDeliveryCTOAddress);
			CommonConsol consol = GetImportConsol(typeof(CommonConsol));
			consol.Shipments.Add(shipment);
			AssertEquals("Expecting Shipment's ImportCTOAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartageDeliveryCTOAddress);
			consol.JK_OA_ArrivalCTOAddress = LocalCTO.MainAddress.PK;
			AssertEquals("Expecting Shipment's ImportCTOAddress to be local CTO", LocalCTO.MainAddress.PK, ((IHaveInternalCartage)shipment).CartageDeliveryCTOAddress);
		}

		public void TestExportContainerYardAddress()
		{
			CommonShipment shipment = GetExportShipment2(typeof(CommonShipment));
			AssertEquals("Expecting Shipment's ExportContainerYardAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartagePickupContainerYardAddress);
			CommonConsol consol = GetExportConsol(typeof(CommonConsol));
			consol.Shipments.Add(shipment);
			AssertEquals("Expecting Shipment's ExportContainerYardAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartagePickupContainerYardAddress);
			consol.JK_OA_ContainerYardEmptyPickupAddress = LocalContainerYard.MainAddress.PK;
			AssertEquals("Expecting Shipment's ExportContainerYardAddress to be local ContainerYard", LocalContainerYard.MainAddress.PK, ((IHaveInternalCartage)shipment).CartagePickupContainerYardAddress);
		}

		public void TestImportContainerYardAddress()
		{
			CommonShipment shipment = GetImportShipment2(typeof(CommonShipment));
			AssertEquals("Expecting Shipment's ImportContainerYardAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartageDeliveryContainerYardAddress);
			CommonConsol consol = GetImportConsol(typeof(CommonConsol));
			consol.Shipments.Add(shipment);
			AssertEquals("Expecting Shipment's ImportContainerYardAddress to be empty", ZGuid.Empty, ((IHaveInternalCartage)shipment).CartageDeliveryContainerYardAddress);
			consol.JK_OA_ContainerYardEmptyReturnAddress = LocalContainerYard.MainAddress.PK;
			AssertEquals("Expecting Shipment's ImportContainerYardAddress to be local ContainerYard", LocalContainerYard.MainAddress.PK, ((IHaveInternalCartage)shipment).CartageDeliveryContainerYardAddress);
		}

		public void TestExporterAddress()
		{
			CommonShipment shipment = GetExportShipment2(typeof(CommonShipment));
			AssertEquals("Expecting Shipment's ExporterAddress to be Shipment's consignor address", shipment.Consignor.MainAddress.PK, ((IHaveInternalCartage)shipment).CartageExporterDocAddress.Address.PK);
		}

		public void TestImporterAddress()
		{
			CommonShipment shipment = GetImportShipment2(typeof(CommonShipment));
			AssertEquals("Expecting Shipment's ImporterAddress to be Shipment's consignee address", shipment.Consignee.MainAddress.PK, ((IHaveInternalCartage)shipment).CartageImporterDocAddress.Address.PK);
		}

		#endregion

		#region Entry Number Tests

		public void TestDeleteEmptyCANEntryNumberOnSaving()
		{
			int before = Factory.Load(typeof(CusEntryNumber), new ZQuery()).Length;

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.CustomsEntryNumberType = "ECN";
			shipment.CustomsEntryNumberType = "CAN";
			shipment.CustomsEntryNumber = "";
			Factory.Save();

			int after = Factory.Load(typeof(CusEntryNumber), new ZQuery()).Length;
			AssertEquals("NumberOfCusEntryNumbers", before, after);
		}

		public void TestCustomsEntryNumberTypeInfo()
		{
			EntryNumberTestHelperShipment shipment = Factory.New<EntryNumberTestHelperShipment>();
			shipment.entryNumberReadOnly = true;
			AssertEquals("CustomsEntryNumberInfo.ReadOnly", true, shipment.CustomsEntryNumberInfo.ReadOnly);
			shipment.entryNumberReadOnly = false;
			AssertEquals("CustomsEntryNumberInfo.ReadOnly", false, shipment.CustomsEntryNumberInfo.ReadOnly);
		}

		public void TestCustomsEntryNumberInfo()
		{
			EntryNumberTestHelperShipment shipment = Factory.New<EntryNumberTestHelperShipment>();
			shipment.entryNumberTypeReadOnly = true;
			AssertEquals("CustomsEntryNumberTypeInfo.ReadOnly", true, shipment.CustomsEntryNumberTypeInfo.ReadOnly);
			shipment.entryNumberTypeReadOnly = false;
			AssertEquals("CustomsEntryNumberTypeInfo.ReadOnly", false, shipment.CustomsEntryNumberTypeInfo.ReadOnly);
		}

		public void TestEntryNumberAndTypeReadOnly()
		{
			TestEntryNumberAndTypeReadOnly(false, 0);
			TestEntryNumberAndTypeReadOnly(false, 1);
			TestEntryNumberAndTypeReadOnly(false, 2);
			TestEntryNumberAndTypeReadOnly(true, 0);
			TestEntryNumberAndTypeReadOnly(true, 1);
			TestEntryNumberAndTypeReadOnly(true, 2);
		}

		void TestEntryNumberAndTypeReadOnly(bool isSystemGenerated, int numberOfEntryNumbers)
		{
			CommonShipment testShipment = CommonShipment.New(Factory);
			for (int i = 0; i < numberOfEntryNumbers; i++)
			{
				CusEntryNumber entryNum = CusEntryNumber.New(testShipment, "XXX", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				entryNum.CE_EntryIsSystemGenerated = isSystemGenerated;
			}
			AssertEquals(numberOfEntryNumbers > 1 || (numberOfEntryNumbers > 0 && isSystemGenerated), testShipment.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals(numberOfEntryNumbers > 1 || (numberOfEntryNumbers > 0 && isSystemGenerated), testShipment.CustomsEntryNumberTypeInfo.ReadOnly);
		}

		#region TestHelper

		class EntryNumberTestHelperShipment : CommonShipment
		{
			public EntryNumberTestHelperShipment(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool entryNumberReadOnly;
			protected bool CustomsEntryNumber_ReadOnly
			{
				get { return entryNumberReadOnly; }
			}

			public bool entryNumberTypeReadOnly;
			protected bool CustomsEntryNumberType_ReadOnly
			{
				get { return entryNumberTypeReadOnly; }
			}

			protected override ConsolCollection GetNewConsolCollection()
			{
				return new ConsolCollection(this);
			}
		}

		#endregion

		#endregion

		#region Implementation

		#region CusEntryNumbers Implementation

		const string TestEntryNumber = "ENTRYNUM";
		const string TestEntryNumber2 = "ENTRYNUM2";
		const string TestEntryNumber3 = "ENTRYNUM3";
		const string TestEntryNumber4 = "ENTRYNUM4";

		void ReloadShipmentEntryNumbers(CommonShipment shipment)
		{
			shipment.ResetCusEntryNumbers();
		}

		CusEntryNumber AddCusEntryNum(ZString entryNumber, ZString tableName, ZGuid parentID, ZBool systemGenerated)
		{
			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "IMP";
			newEntryNumber.CE_EntryNum = entryNumber;
			newEntryNumber.CE_ParentID = parentID;
			newEntryNumber.CE_ParentTable = tableName;
			newEntryNumber.CE_EntryIsSystemGenerated = systemGenerated;
			newEntryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return newEntryNumber;
		}

		#endregion

		#endregion

		public void TestUpdateHBLContainerPackModeOverride()
		{
			var collFCL = new HBLDeliveryModeCollection();
			collFCL.Add("DOOR/DOOR", (NoResString)"DOOR/DOOR");
			collFCL.Add("CY/CY", (NoResString)"CY/CY");
			var hblDeliveryModesFCL = new HBLDeliveryModes("FCL", collFCL);
			hblDeliveryModesFCL.DefaultHBLDeliveryMode = "DOOR/DOOR";
			FreightDataRegistry.Instance.HBLDeliveryMode_FCL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hblDeliveryModesFCL);

			var collLCL = new HBLDeliveryModeCollection();
			collLCL.Add("DOOR/DOOR", (NoResString)"DOOR/DOOR");
			collLCL.Add("CFS/CFS", (NoResString)"CFS/CFS");
			var hblDeliveryModesLCL = new HBLDeliveryModes("LCL", collLCL);
			hblDeliveryModesLCL.DefaultHBLDeliveryMode = "CFS/CFS";
			FreightDataRegistry.Instance.HBLDeliveryMode_LCL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hblDeliveryModesLCL);

			var shipment = GetShipment();
			shipment.JS_TransportMode = "SEA";

			var dataImportingInterface = shipment as ISupportDataImporting;
			AssertEquals(false, dataImportingInterface.IsImportingData);

			AssertEquals("FCL", shipment.JS_PackingMode);
			AssertEquals("Default HBL Delivery Code for FCL", "DOOR/DOOR", shipment.JS_HBLContainerPackModeOverride);

			shipment.JS_PackingMode = "LCL";
			AssertEquals("Default HBL Delivery Code for LCL", "CFS/CFS", shipment.JS_HBLContainerPackModeOverride);

			shipment.JS_PackingMode = "FCL";
			AssertEquals("Default HBL Delivery Code for FCL", "DOOR/DOOR", shipment.JS_HBLContainerPackModeOverride);

			dataImportingInterface.IsImportingData = true;
			AssertEquals(true, dataImportingInterface.IsImportingData);

			shipment.JS_PackingMode = "LCL";
			AssertEquals("Don't default HBL Delivery Code when importing", "DOOR/DOOR", shipment.JS_HBLContainerPackModeOverride);
		}

		public void TestCoLoadMasterShipment()
		{
			CommonShipment masterShipment = GetShipment();
			masterShipment.JS_UniqueConsignRef = "S12345678";
			masterShipment.JS_HouseBill = "MASTER HOUSE";
			CommonShipment coLoad = GetShipment();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			coLoad.JS_JS_ColoadMasterShipment = masterShipment.PK;

			AssertNotNull("Master Shipment.", coLoad.CoLoadMasterShipment);
			AssertEquals("Incorrect Master Shipment.", "MASTER HOUSE", coLoad.CoLoadMasterShipment.JS_HouseBill);
			AssertEquals("Coload CommonShipment same type as coloaded Shipment", coLoad.CoLoadMasterShipment.GetType().FullName, coLoad.GetType().FullName);
			AssertEquals("Incorrect Master Bill No.", "MASTER HOUSE", coLoad.JS_Calc_CoLoadMasterBillNo);
			AssertEquals("Incorrect Master ShipmentID.", "S12345678", coLoad.JS_Calc_CoLoadMasterShipmentID);

			coLoad.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertNull("No Master Shipment.", coLoad.CoLoadMasterShipment);
			AssertEquals("Should be no Master Bill No.", ZString.Empty, coLoad.JS_Calc_CoLoadMasterBillNo);
			AssertEquals("Should be no ShipmentID.", ZString.Empty, coLoad.JS_Calc_CoLoadMasterShipmentID);
		}

		public void TestCoLoadMasterCircularReference()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

			CommonShipment shipment1 = GetShipment();
			CommonShipment shipment2 = GetShipment();
			CommonShipment shipment3 = GetShipment();

			shipment1.Consols.Add(consol);
			shipment2.Consols.Add(consol);
			shipment3.Consols.Add(consol);

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment3.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			Factory.Save();

			shipment1.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment2.JS_JS_ColoadMasterShipment = shipment3.PK;
			shipment3.JS_JS_ColoadMasterShipment = shipment1.PK;

			shipment1.RunPreSaveValidation();

			AssertHasError("Circular reference should be detected", shipment3.JS_JS_ColoadMasterShipmentInfo, "Co-Load Master cannot be a Co-Load of this Shipment.");

			shipment3.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			AssertNoErrors("Circular reference has been removed", shipment3.JS_JS_ColoadMasterShipmentInfo);
		}

		public void TestCoLoadMasterHouseBill()
		{
			CommonShipment masterShipment = GetShipment();
			CommonShipment shipment = GetShipment();
			masterShipment.JS_HouseBill = "TestMasterHouseBill";

			AssertEquals("Master HouseBill with no co-load master", "", shipment.ColoadMasterShipmentHouseBill);
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertEquals("Master HouseBill after co-load master is set", masterShipment.JS_HouseBill, shipment.ColoadMasterShipmentHouseBill);
		}

		public void TestCoLoadShipments()
		{
			CommonShipment masterShipment = GetShipment();
			CommonShipment coLoad1 = GetShipment();
			CommonShipment coLoad2 = GetShipment();
			Factory.Save();

			coLoad1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			coLoad2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertEquals("Check CoLoad count on Master.", 2, masterShipment.CoLoadShipments.Count);
		}

		public void TestCoLoadShipmentPacklines()
		{
			CommonShipment shipment1 = GetShipment();
			shipment1.OuterPackLines.AddNew();
			CommonShipment shipment2 = GetShipment();
			shipment2.OuterPackLines.AddNew();
			Factory.Save();

			AssertEquals("Tender touch of the collection", 1, shipment1.OuterPackLines.Count);
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("No packlines on coload if no attached shipments", 0, shipment1.OuterPackLines.Count);

			shipment1.CoLoadShipments.Add(shipment2);
			AssertEquals("Co-Load master must have packlines from attached shipments", 1, shipment1.OuterPackLines.Count);
			AssertEquals("Co-Load master must have packlines from attached shipments", shipment2.OuterPackLines[0], shipment1.OuterPackLines[0]);
		}

		[ExpectNoExceptions]
		public void TestDeleteWillUnsubscribeUpdatedByDataRefreshIncludingChildrenEvent()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			ChildEditableService.SetState(factory1, ChildEditableServiceStates.Shipment);
			CommonConsol consol = factory1.New<CommonConsol>();

			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ChildEditableService.SetState(factory2, ChildEditableServiceStates.Shipment);
			CommonConsol consolReloaded = factory2.Load<CommonConsol>(consol.PK);
			AssertEquals("Same consol is loaded in Factory2", consol.PK, consolReloaded.PK);

			var shipment1 = consolReloaded.Shipments.AddNew();
			shipment1.JS_IsForwardRegistered = false;
			shipment1.Delete();

			consol.JK_RL_NKLoadPort = "AUSYD";
			factory1.Save();
		}

		public void TestMasterChanged()
		{
			var mAS_1 = FreightTestHelper.GetShipment<CommonShipment>("MAS_1", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var mAS_2 = FreightTestHelper.GetShipment<CommonShipment>("MAS_2", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var sUB = FreightTestHelper.GetShipment<CommonShipment>("SUB", Constants.ShipmentTypes.AssemblyMaster, Factory);

			int eventFireCount = 0;
			sUB.MasterChanged += new EventHandler<MasterChangedEventArgs>((sender, args) => eventFireCount++);

			sUB.JS_JS_ColoadMasterShipment = mAS_1.PK;
			AssertEquals(1, eventFireCount);

			sUB.JS_JS_ColoadMasterShipment = mAS_2.PK;
			AssertEquals(2, eventFireCount);

			sUB.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertEquals(3, eventFireCount);

			mAS_1.CoLoadShipments.Add(sUB);
			AssertEquals(4, eventFireCount);
		}

		public void TestJS_JK_ConsolID()
		{
			var consol1 = FreightTestHelper.GetConsol<CommonConsol>("consol1", Factory);
			var consol2 = FreightTestHelper.GetConsol<CommonConsol>("consol2", Factory);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("shipment", Constants.ShipmentTypes.StandardHouse, Factory);

			FreightTestHelper.AssertConsolCollection("Should be NO consols", shipment.Consols);
			AssertEquals("Should be NO consols", ZString.Empty, shipment.JS_JK_ConsolID);

			shipment.Consols.Add(consol2);
			FreightTestHelper.AssertConsolCollection("Should be consol2", shipment.Consols, consol2);
			AssertEquals("Should be consol2", "consol2", shipment.JS_JK_ConsolID);

			shipment.Consols.Add(consol1);
			FreightTestHelper.AssertConsolCollection("Should be consol1 and consol2", shipment.Consols, consol1, consol2);
			AssertEquals("Should be consol1 and consol2", "consol1, consol2", shipment.JS_JK_ConsolID);
		}

		public void TestAttachColoadShipmentMasterConsols()
		{
			var consol1 = FreightTestHelper.GetConsol<CommonConsol>("consol1", Factory);
			var consol2 = FreightTestHelper.GetConsol<CommonConsol>("consol2", Factory);
			var consol3 = FreightTestHelper.GetConsol<CommonConsol>("consol3", Factory);
			var consol4 = FreightTestHelper.GetConsol<CommonConsol>("consol4", Factory);

			var masterShipment = FreightTestHelper.GetShipment<CommonShipment>("master", Constants.ShipmentTypes.CoLoadMaster, Factory);
			masterShipment.Consols.Add(consol1);
			masterShipment.Consols.Add(consol2);

			var subShipment1 = FreightTestHelper.GetShipment<CommonShipment>("SUB1", Constants.ShipmentTypes.StandardHouse, Factory);
			subShipment1.Consols.Add(consol3);
			subShipment1.Consols.Add(consol4);

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertContainsExactElementsInAnyOrder("Master shipment should not be attached to sub-shipment's consols.", new[] { consol1, consol2 }, masterShipment.Consols);
			AssertContainsExactElementsInAnyOrder("Sub shipment should be attached to master shipment's consols in addition to its own.", new[] { consol1, consol2, consol3, consol4 }, subShipment1.Consols);

			var subShipment2 = FreightTestHelper.GetShipment<CommonShipment>("SUB2", Constants.ShipmentTypes.StandardHouse, Factory);
			subShipment2.Consols.Add(consol1);
			subShipment2.Consols.Add(consol4);

			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertContainsExactElementsInAnyOrder("Master shipment should not be attached to any additional consols.", new[] { consol1, consol2 }, masterShipment.Consols);
			AssertContainsExactElementsInAnyOrder("Sub shipment should be attached to consols from the master, as well as consols that it was already attached to", new[] { consol1, consol2, consol4 }, subShipment2.Consols);
		}

		public void TestAttachColoadMasterShipmentConsols_CheckForExistingRelationship()
		{
			var consol = FreightTestHelper.GetConsol<CommonConsol>("consol1", Factory);
			var masterShipment = FreightTestHelper.GetShipment<CommonShipment>("master", Constants.ShipmentTypes.AssemblyMaster, Factory);

			consol.Shipments.Add(masterShipment);

			var shipments = (IBindingList)consol.Shipments;
			var addedShipment = (CommonShipment)shipments.AddNew();

			Assert("Relationship should be set", consol.Shipments.GetRelationshipBusinessObject(addedShipment) != null);
			AssertEquals("Consol has not been added to collection yet", addedShipment.Consols.Contains(consol), false);

			addedShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			addedShipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			addedShipment.Delete();

			AssertNoExceptionThrown("No JobConShipLink with deleted shipment should be saved.", Factory.Save);
		}

		public void TestHasParent()
		{
			var aSM = Factory.NewWithValidTestData<CommonShipment>();
			aSM.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var cLD = aSM.CoLoadShipments.AddNew();
			cLD.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var sTD_1 = cLD.CoLoadShipments.AddNew();
			sTD_1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var sTD_2 = aSM.CoLoadShipments.AddNew();
			sTD_2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			AssertEquals(false, aSM.HasMaster(Constants.ShipmentTypes.AssemblyMaster));
			AssertEquals(false, aSM.HasMaster(Constants.ShipmentTypes.CoLoadMaster));

			AssertEquals(true, cLD.HasMaster(Constants.ShipmentTypes.AssemblyMaster));
			AssertEquals(false, cLD.HasMaster(Constants.ShipmentTypes.CoLoadMaster));

			AssertEquals(true, sTD_1.HasMaster(Constants.ShipmentTypes.AssemblyMaster));
			AssertEquals(true, sTD_1.HasMaster(Constants.ShipmentTypes.CoLoadMaster));

			AssertEquals(true, sTD_2.HasMaster(Constants.ShipmentTypes.AssemblyMaster));
			AssertEquals(false, sTD_2.HasMaster(Constants.ShipmentTypes.CoLoadMaster));
		}

		public void TestShipmentAndCoLoadCollectionSameType()
		{
			CommonShipment master = GetShipment();
			CommonShipment coLoad = master.CoLoadShipments.AddNew();
			AssertEquals("Shipment and Co Load Types", master.GetType().FullName, coLoad.GetType().FullName);
		}

		public void TestOuterPacksAndOuterPackLines()
		{
			CommonShipment shipment = GetShipment();
			AssertEquals("Outer pack lines Count", 0, shipment.OuterPackLines.Count);
			AssertEquals("Outer Pack Count", 0, shipment.JS_OuterPacks);

			shipment.JS_OuterPacks = 1;
			AssertEquals("Outer pack lines Count", 1, shipment.OuterPackLines.Count);
		}

		public void TestCanHaveOwnPackLines()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			AssertEquals("CanHaveOwnPackLines for STD", true, shipment.CanHaveOwnPackLines);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("CanHaveOwnPackLines for CLD with no coloads", true, shipment.CanHaveOwnPackLines);

			shipment.CoLoadShipments.AddNew();
			AssertEquals("CanHaveOwnPackLines for CLD with a coload", false, shipment.CanHaveOwnPackLines);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals("prerequisite - shipment has a coload", 1, shipment.CoLoadShipments.Count);
			AssertEquals("CanHaveOwnPackLines for ASM with a coload", false, shipment.CanHaveOwnPackLines);

			shipment.CoLoadShipments.RemoveAndDeleteAll();
			AssertEquals("CanHaveOwnPackLines for ASM with no coloads", true, shipment.CanHaveOwnPackLines);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertEquals("CanHaveOwnPackLines for HLV", true, shipment.CanHaveOwnPackLines);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			AssertEquals("CanHaveOwnPackLines for HLS", true, shipment.CanHaveOwnPackLines);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals("CanHaveOwnPackLines for BCN", true, shipment.CanHaveOwnPackLines);
		}

		public void TestInvoicePrintingLogsIncludedInRelatedLogs()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_HouseBill = "TESTFORINVOICEPRINT";
			shipment.ConsigneePK = LocalConsignee.PK;
			Factory.Save();
			InvoiceCreationTestHelper helper = new InvoiceCreationTestHelper(Factory);
			AccTransactionHeader header = helper.SetupTransaction(GlbBranch.CurrentBranch, "ANYNUMBER", LocalConsignee, shipment.JS_UniqueConsignRef, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			Assert("Unique Consign Ref Not Set", !shipment.JS_UniqueConsignRef.IsEmpty);
			StmALog printLog = header.Logs.AddNew(Events.DocumentSent, "FAKE Invoice Printed");

			StmALogCollectionView shipmentLogs = new StmALogCollectionView(shipment);
			Assert("Failed to get invoice printed log", shipmentLogs.Contains(printLog.PK));
		}

		public void TestEstimatedDeliveryDoesntUpdateWhenImporting()
		{
			CommonShipment testShipment = GetShipment();

			ZDateTime dateOfArrival = new ZDateTime(2005, 5, 14);
			GlbPortDeliveryTime defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = Core.Constants.TransportModes.Sea;
			defaultDelay.G1_RL_NKDischargePort = "USLAX";
			defaultDelay.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay.G1_DaysDelayFromArrivalToDeliver = 2;
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 4;

			((ISupportDataImporting)testShipment).IsImportingData = true;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETA = dateOfArrival;
			transport.JW_ETD = dateOfArrival.AddDays(-4);
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;

			testShipment.Consols.Add(consol);
			AssertEquals("Shipment.DocsAndCartage.Estimated delivery is empty", true, testShipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);

			testShipment.JS_RL_NKDestination = "USAAA";
			testShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals("Shipment.DocsAndCartage.Estimated delivery empty", true, testShipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);
		}

		public void TestJS_JS_ColoadMasterShipmentHasRelatedBizOAttribute()
		{
			CommonShipment masterShipment = GetShipment();
			CommonShipment subShipment = GetShipment();

			masterShipment.JS_UniqueConsignRef = "ABC";
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			AssertEquals("ABC", RelatedBusinessObjectAttribute.GetCodeForGuid(subShipment.JS_JS_ColoadMasterShipmentInfo));
		}

		public void TestConsolPassesThroughCountry()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol = shipment.Consols.AddNew();

			Transport originalTransport = consol.Transports[0];
			Transport transport1 = consol.Transports.AddNew();
			Transport transport2 = consol.Transports.AddNew();
			originalTransport.JW_RL_NKLoadPort = "AUSYD";
			originalTransport.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "CACAD";

			AssertEquals("Consol passes through AU", true, shipment.ShipmentPassesThroughCountry("AU"));
			AssertEquals("Consol passes through NZ", true, shipment.ShipmentPassesThroughCountry("NZ"));
			AssertEquals("Consol passes through US", true, shipment.ShipmentPassesThroughCountry("US"));
			AssertEquals("Consol passes through CA", true, shipment.ShipmentPassesThroughCountry("CA"));
			AssertEquals("Consol passes through ZA", false, shipment.ShipmentPassesThroughCountry("ZA"));

			CommonConsol consol2 = shipment.Consols.AddNew();
			Transport transport3 = consol2.Transports[0];
			transport3.JW_RL_NKLoadPort = "CACAD";
			transport3.JW_RL_NKDiscPort = "ZAAAM";

			AssertEquals("Consol passes through ZA", true, shipment.ShipmentPassesThroughCountry("ZA"));
			AssertEquals("Consol passes through AG", false, shipment.ShipmentPassesThroughCountry("AG"));

			shipment.JS_RL_NKDestination = "AGANU";
			AssertEquals("Consol passes through AG", true, shipment.ShipmentPassesThroughCountry("AG"));

			shipment.JS_RL_NKDestination = "ZAAAM";
			shipment.JS_RL_NKOrigin = "AGANU";
			AssertEquals("Consol passes through AG", true, shipment.ShipmentPassesThroughCountry("AG"));
		}

		public void TestIsLeadOrMasterTogglesCoLoadShipmentsReadOnly()
		{
			CommonShipment shipment = GetShipment();

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(false, shipment.CoLoadShipments.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			AssertEquals(false, shipment.CoLoadShipments.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(false, shipment.CoLoadShipments.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals(false, shipment.CoLoadShipments.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals(false, shipment.CoLoadShipments.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(true, shipment.CoLoadShipments.ReadOnly);
		}

		public void TestTotalOuterPacksPillaged()
		{
			CommonShipment shipment = GetShipment();
			PackLine packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_Pillaged = int.MaxValue;
			PackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_Pillaged = int.MaxValue;
			AssertEquals("The overflow exception is caught and the max returned", int.MaxValue, shipment.TotalOuterPacksPillaged);
		}

		public void TestTotalOuterPacksDamaged()
		{
			CommonShipment shipment = GetShipment();
			PackLine packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_Damaged = int.MaxValue;
			PackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_Damaged = int.MaxValue;
			AssertEquals("The overflow exception is caught and the max returned", int.MaxValue, shipment.TotalOuterPacksDamaged);
		}

		public void TestTotalOuterPacksWeight_Imperial()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			PackLine packline1 = shipment.OuterPackLines.AddNew();
			ZDecimal testresult = 0;

			AssertEquals(testresult, shipment.TotalOuterPacksWeight_Imperial);

			packline1.JL_ActualWeight = 100;
			var testresult1 = shipment.GetRoundedValue(null, shipment.TotalOuterPacksWeight_ImperialInfo, Constants.Weight.Convert(shipment.TotalOuterPacksWeight, shipment.TotalPackLineWeightUnit, Constants.Weight.Pounds, false));
			AssertEquals(testresult1, shipment.TotalOuterPacksWeight_Imperial);

			PackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 100;

			var testresult2 = shipment.GetRoundedValue(null, shipment.TotalOuterPacksWeight_ImperialInfo, Constants.Weight.Convert(shipment.TotalOuterPacksWeight, shipment.TotalPackLineWeightUnit, Constants.Weight.Pounds, false));
			AssertEquals(testresult2, shipment.TotalOuterPacksWeight_Imperial);
		}

		public void TestTotalOuterPacksVolume_Imperial()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			PackLine packline1 = shipment.OuterPackLines.AddNew();
			ZDecimal testresult = 0;

			AssertEquals(testresult, shipment.TotalOuterPacksVolume_Imperial);

			packline1.JL_ActualVolume = 100;
			var testresult1 = shipment.GetRoundedValue(null, shipment.TotalOuterPacksVolume_ImperialInfo, Constants.Volume.Convert(shipment.TotalOuterPacksVolume, shipment.TotalPackLineVolumeUnit, Constants.Volume.CubicFeet));
			AssertEquals(testresult1, shipment.TotalOuterPacksVolume_Imperial);

			PackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualVolume = 100;

			var testresult2 = shipment.GetRoundedValue(null, shipment.TotalOuterPacksVolume_ImperialInfo, Constants.Volume.Convert(shipment.TotalOuterPacksVolume, shipment.TotalPackLineVolumeUnit, Constants.Volume.CubicFeet));
			AssertEquals(testresult2, shipment.TotalOuterPacksVolume_Imperial);
			Assert(testresult1 < testresult2);
		}

		public void TestJS_ActualWeight_Imperial()
		{
			CommonShipment shipment = GetShipment();

			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_ActualWeight = 0;
			ZDecimal testresult = 0;

			AssertEquals(testresult, shipment.JS_ActualVolume_Imperial);

			shipment.JS_ActualWeight = 100;

			testresult = shipment.GetRoundedValue(null, shipment.JS_ActualWeight_ImperialInfo, Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.TotalPackLineWeightUnit, Constants.Weight.Pounds, false));
			AssertEquals(testresult, shipment.JS_ActualWeight_Imperial);
		}

		public void TestJS_ActualVolume_Imperial()
		{
			CommonShipment shipment = GetShipment();

			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment.JS_ActualVolume = 0;
			ZDecimal testresult = 0;

			AssertEquals(testresult, shipment.JS_ActualVolume_Imperial);

			shipment.JS_ActualVolume = 100;

			testresult = shipment.GetRoundedValue(null, shipment.JS_ActualVolume_ImperialInfo, Constants.Volume.Convert(shipment.JS_ActualVolume, shipment.TotalPackLineVolumeUnit, Constants.Volume.CubicFeet, false));
			AssertEquals(testresult, shipment.JS_ActualVolume_Imperial);
		}

		public void TestDefaultCartageCompany()
		{
			OrgHeader airCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader fclCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader lclCartage = Factory.NewWithValidTestData<OrgHeader>();

			FreightDataRegistry.Instance.FCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, fclCartage.PK.ToGuid());
			FreightDataRegistry.Instance.LCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, lclCartage.PK.ToGuid());
			FreightDataRegistry.Instance.AIRCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, airCartage.PK.ToGuid());

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			GlbBranch.CurrentBranch.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUBNE";
			GlbBranch.CurrentBranch.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";

			CommonShipment shipment = GetShipment();
			shipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.Consignor.OH_RL_NKClosestPort = "NZAKL";
			shipment.Consignee.OH_RL_NKClosestPort = "USLAX";
			Factory.Save();

			shipment.DocsAndCartage.PickupCartageCoPK = Guid.Empty;
			shipment.DocsAndCartage.DeliveryCartageCoPK = Guid.Empty;
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			AssertEquals("No defaulting for pickup cartage company from registry", ZGuid.Empty, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("No defaulting for delivery cartage company from registry", ZGuid.Empty, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.Consignor.OH_RL_NKClosestPort = "AUSYD";
			shipment.DocsAndCartage.PickupCartageCoPK = Guid.Empty;
			shipment.DocsAndCartage.DeliveryCartageCoPK = Guid.Empty;
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			AssertEquals("AIR Cartage Company, defaulting from registry, consignor unloco = branch home port", airCartage.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("No defaulting for delivery cartage company from registry", ZGuid.Empty, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.Consignee.OH_RL_NKClosestPort = "AUBNE";
			shipment.DocsAndCartage.PickupCartageCoPK = Guid.Empty;
			shipment.DocsAndCartage.DeliveryCartageCoPK = Guid.Empty;
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			AssertEquals("AIR Cartage Company, defaulting from registry, consignor unloco = branch home port", airCartage.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("AIR Cartage Company, defaulting from registry, consignee unloco = one of branch related ports", airCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.DocsAndCartage.PickupCartageCoPK = Guid.Empty;
			shipment.DocsAndCartage.DeliveryCartageCoPK = Guid.Empty;
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			AssertEquals("AIR Cartage Company", airCartage.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("AIR Cartage Company", airCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.DocsAndCartage.PickupCartageCoPK = Guid.Empty;
			shipment.DocsAndCartage.DeliveryCartageCoPK = Guid.Empty;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			AssertEquals("FCL Cartage Company", fclCartage.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("FCL Cartage Company", fclCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.DocsAndCartage.PickupCartageCoPK = Guid.Empty;
			shipment.DocsAndCartage.DeliveryCartageCoPK = Guid.Empty;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			AssertEquals("LCL Cartage Company", lclCartage.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("LCL Cartage Company", lclCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
		}

		#region ICDArchive
		public void TestCDJobNumber()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "ABC123";
			AssertEquals("CD Job number", "ABC123", shipment.CDArchiveInfo.JobNumber);

			shipment.JS_UniqueConsignRef = "ABC1234";
			AssertEquals("CD Job number", "ABC1234", shipment.CDArchiveInfo.JobNumber);
		}

		public void TestCDHouseBill()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_HouseBill = "1234567890";
			AssertEquals("Housebill number", "1234567890", shipment.CDArchiveInfo.HouseBill);

			shipment.JS_HouseBill = "111111";
			AssertEquals("Housebill number", "111111", shipment.CDArchiveInfo.HouseBill);
		}

		public void TestCDMasterBill()
		{
			CommonShipment shipment = GetShipment();

			CommonConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_MasterBillNum = "11111";

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_MasterBillNum = "22222";

			AssertEquals("Masterbill -takes the first one, ignores subsequent", "11111", shipment.CDArchiveInfo.MasterBill);
		}

		public void TestCDOrderNumbers()
		{
			CommonShipment shipment = GetShipment();
			AssertEquals("Order numbers not defined at this level", ZString.Empty, shipment.CDArchiveInfo.OrderNumbers);
		}

		public void TestCDInvoiceNumbers()
		{
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			CommonShipment shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "S00001001";
			shipment.ConsigneePK = importer.PK;

			InvoiceCreationTestHelper invoiceCreator = new InvoiceCreationTestHelper(Factory);
			AccTransactionHeader header1 = invoiceCreator.SetupTransaction(GlbBranch.CurrentBranch, "11111", importer, "11111", ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice);

			AccTransactionHeader header2 = invoiceCreator.SetupTransaction(GlbBranch.CurrentBranch, "22222", importer, "22222", ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice);

			AccTransactionHeader header3 = invoiceCreator.SetupTransaction(GlbBranch.CurrentBranch, "33333", importer, "33333", ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice);

			AssertEquals("No matching invoices", ZString.Empty, shipment.CDArchiveInfo.InvoiceNumbers);

			header1.AH_ConsolidatedInvoiceRef = "S00001001";
			AssertEquals("Matching invoice found", "S00001001", shipment.CDArchiveInfo.InvoiceNumbers);
		}

		public void TestCDContainerNumbers()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.AutomaticallyUpdatePackLineContainers = false;
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();

			PackLine line1 = shipment.OuterPackLines.AddNew();
			PackLine line2 = shipment.OuterPackLines.AddNew();

			container1.JC_ContainerCount = 1;
			container1.PackLines.Add(line1);

			container2.JC_ContainerCount = 1;
			container2.PackLines.Add(line2);

			container1.JC_ContainerNum = "123";
			container2.JC_ContainerNum = "456";

			AssertEquals("Container count should be > 0", true, shipment.Containers.Any());
			AssertEquals("Container numbers", true, shipment.CDArchiveInfo.ContainerNumbers.Contains("123"));
			AssertEquals("Container numbers", true, shipment.CDArchiveInfo.ContainerNumbers.Contains("456"));
		}

		public void TestCDETA()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_E_ARV = new ZDateTime(2004, 11, 30);
			AssertEquals("ETA date", new ZDateTime(2004, 11, 30), shipment.CDArchiveInfo.ETA);

			shipment.JS_E_ARV = new ZDateTime(2004, 12, 30);
			AssertEquals("ETA date", new ZDateTime(2004, 12, 30), shipment.CDArchiveInfo.ETA);
		}

		public void TestCDETD()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_E_DEP = new ZDateTime(2004, 11, 30);
			AssertEquals("ETD date", new ZDateTime(2004, 11, 30), shipment.CDArchiveInfo.ETD);

			shipment.JS_E_DEP = new ZDateTime(2004, 12, 30);
			AssertEquals("ETD date", new ZDateTime(2004, 12, 30), shipment.CDArchiveInfo.ETD);
		}

		public void TestCDVessel()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];

			transport.JW_Vessel = "ABC Vessel";
			AssertEquals("Vessel info", "ABC Vessel", shipment.CDArchiveInfo.Vessel);
		}

		public void TestCDVoyageFlight()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];

			transport.JW_Vessel = "ABC Vessel";
			transport2.JW_Vessel = "XYZ Vessel";
			AssertEquals("Vessel info - gets first consol vessel", "ABC Vessel", shipment.CDArchiveInfo.Vessel);

			transport.JW_Vessel = "DEF Vessel";
			AssertEquals("Vessel info", "DEF Vessel", shipment.CDArchiveInfo.Vessel);
		}

		public void TestCDEntryNumber()
		{
			var shipment = GetShipment();

			var declaration1 = Factory.New<IBaseJobDeclaration>();
			declaration1.JE_JS = shipment.PK;

			var declaration2 = Factory.New<IBaseJobDeclaration>();
			declaration2.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
			declaration2.JE_JS = shipment.PK;

			var declarationNumberProperty1 = declaration1.GetType().GetProperty("DeclarationNumber");
			declarationNumberProperty1.SetValue(declaration1, new ZString("en1"));

			var declarationNumberProperty2 = declaration2.GetType().GetProperty("DeclarationNumber");
			declarationNumberProperty2.SetValue(declaration2, new ZString("en2"));

			AssertEquals("EntryNumber", "en1, en2", shipment.CDArchiveInfo.EntryNumber);
		}

		public void TestCDOrigin()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";

			AssertEquals("Origin", "AUSYD", shipment.CDArchiveInfo.Origin);

			shipment.JS_RL_NKOrigin = "AUMEL";
			AssertEquals("Origin", "AUMEL", shipment.CDArchiveInfo.Origin);
		}

		public void TestCDDestination()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals("Destination", "USLAX", shipment.CDArchiveInfo.Destination);

			shipment.JS_RL_NKDestination = "USSFO";
			AssertEquals("Destination", "USSFO", shipment.CDArchiveInfo.Destination);
		}

		public void TestCDConsigneeCode()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			CommonShipment shipment = GetShipment();
			shipment.ConsigneePK = org.PK;

			AssertEquals("Consignee", org.OH_Code, shipment.CDArchiveInfo.ConsigneeCode);
		}

		public void TestCDConsignorCode()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			CommonShipment shipment = GetShipment();
			shipment.ConsignorPK = org.PK;

			AssertEquals("Consignor", org.OH_Code, shipment.CDArchiveInfo.ConsignorCode);
		}
		#endregion

		[ExpectNoExceptions]
		public void TestJobDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			var shipment = Factory.New<CommonShipment>();

			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_GC = GlbCompany.CurrentCompany.PK;

			var job = shipment.Job;
			header.Delete();
			var branchOnDeletedJob = shipment.Job != null ? shipment.Job.Branch : null;
		}

		public void TestDelete_ShouldRemoveShipmentJobHeaderTogether_WhenShipmentJobHeaderIsNotNull()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.CreateShipmentJobHeaderWithMutex();

			shipment.Delete();

			AssertNull(shipment.ShipmentJobHeader);
		}

		#region ICusNumbers Supporters

		CusEntryNumber GetCusEntryNumber(CommonShipment shipment, ZString numberType, RefCountry country, string category)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, numberType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country.Code);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, shipment.PK);
			query.AddToFilter(CusEntryNumSchema.CE_Category, category);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, shipment.TableName);
			CusEntryNumber[] numbers = Factory.Load<CusEntryNumber>(query);
			return (numbers != null && numbers.Length > 0) ? numbers[0] : null;
		}

		void CreateCusEntryNumber(CommonShipment shipment, ZString numberType, RefCountry country, ZString value, string category)
		{
			CusEntryNumber number = Factory.New<CusEntryNumber>();
			number.CE_Category = category;
			number.CE_EntryNum = value;
			number.CE_EntryType = numberType;
			number.CE_ParentID = shipment.PK;
			number.CE_ParentTable = shipment.TableName;
			number.CE_RN_NKCountryCode = country.Code;
		}

		public void TestAdditionalReferenceNumberSupporter()
		{
			CommonShipment shipment = GetShipment();
			RefCountry australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia);
			RefCountry iceland = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Iceland);
			CreateCusEntryNumber(shipment, "COC", iceland, "123", CusEntryNumber.Categories.AdditionalReferenceNumber);
			CreateCusEntryNumber(shipment, "AAA", australia, "jjj", CusEntryNumber.Categories.AdditionalReferenceNumber);
			(shipment as IAdditionalReferenceNumberSupporter).CreateOrUpdate("COC", australia.RN_Code, "123", new NotificationBuffer());
			CreateCusEntryNumber(shipment, "COC", iceland, "123", CusEntryNumber.Categories.AdditionalReferenceNumber);
			CusEntryNumber number = GetCusEntryNumber(shipment, "COC", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNotNull("COC reference number", number);
			AssertEquals("123", number.CE_EntryNum);
			number = GetCusEntryNumber(shipment, "AAA", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNotNull("Reference number", number);
			(shipment as IAdditionalReferenceNumberSupporter).CreateOrUpdate("COC", australia.RN_Code, "456", new NotificationBuffer());
			number = GetCusEntryNumber(shipment, "COC", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNotNull("COC reference number", number);
			AssertEquals("456", number.CE_EntryNum);
			NotificationBuffer notify = new NotificationBuffer();
			(shipment as IAdditionalReferenceNumberSupporter).CreateOrUpdate("ZZ1", australia.RN_Code, "aaa", notify);
			number = GetCusEntryNumber(shipment, "ZZ1", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNull("ZZ1 reference number", number);
			AssertContains("No support for additional reference number type 'ZZ1'", notify.AsString);
			AssertEquals("Default value should be false", false, (shipment as IAdditionalReferenceNumberSupporter).IncludeSpecialCustomsInstructionsItems);

			CreateCusEntryNumber(shipment, "CLC", australia, "ooo", CusEntryNumber.Categories.AdditionalReferenceNumber);
			(shipment as IAdditionalReferenceNumberSupporter).CreateOrUpdate("CLC", australia.RN_Code, "ooo", new NotificationBuffer());
			number = GetCusEntryNumber(shipment, "CLC", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNotNull("Client Contract Number", number);
			AssertEquals("ooo", number.CE_EntryNum);
			(shipment as IAdditionalReferenceNumberSupporter).CreateOrUpdate("CLC", australia.RN_Code, "ccc", new NotificationBuffer());
			number = GetCusEntryNumber(shipment, "CLC", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertEquals("Number is updated", "ccc", number.CE_EntryNum);

			CreateCusEntryNumber(shipment, "CON", australia, "xxx", CusEntryNumber.Categories.AdditionalReferenceNumber);
			CreateCusEntryNumber(shipment, "CON", australia, "yyy", CusEntryNumber.Categories.AdditionalReferenceNumber);

			AssertContainsExactElementsInAnyOrder(
				"Shipment should have Additional Reference Numbers",
				new[] { "123", "123", "456", "ccc", "jjj", "xxx", "yyy" },
				(shipment as IAdditionalReferenceNumberSupporter).AdditionalReferenceNumbers.OfType<CusEntryNumber>().Select(x => x.CE_EntryNum));
			Assert("Additional Reference Numbers is 'Numbers'", ReferenceEquals((shipment as IAdditionalReferenceNumberSupporter).AdditionalReferenceNumbers, shipment.Numbers));
		}

		public void TestIAdditionalReferenceNumberSupporter_CreateOrUpdate_SetsInterimReceipt()
		{
			CommonShipment ship = Factory.New<CommonShipment>();
			RefCountry iceland = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Iceland);
			CustomsReferenceNumberTypeCollection list = FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value;
			CustomsReferenceNumberTypeCollection newList = (CustomsReferenceNumberTypeCollection)list.Clone(null, null);
			newList.Add("IMR", (NoResString)"imr test");
			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newList);
			((IAdditionalReferenceNumberSupporter)ship).CreateOrUpdate("IMR", iceland.RN_Code, "456", null);
			AssertEquals("456", ship.JS_InterimReceipt);
			((IAdditionalReferenceNumberSupporter)ship).CreateOrUpdate("IMR", "", "537", null);
			AssertEquals("537", ship.JS_InterimReceipt);
		}

		public void TestIAdditionalReferenceNumberSupporter_CreateOrUpdate_NonUnique()
		{
			CustomsReferenceNumberTypeCollection customsReferenceNumberTypeCollection = new CustomsReferenceNumberTypeCollection();
			customsReferenceNumberTypeCollection.Add("AAA", (NoResString)"AAA Desc").IsUnique = true;
			customsReferenceNumberTypeCollection.Add("BBB", (NoResString)"BBB Desc").IsUnique = false;

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customsReferenceNumberTypeCollection);

			CommonShipment shipment = Factory.New<CommonShipment>();

			IAdditionalReferenceNumberSupporter additionalReferenceNumberSupporter = shipment;
			additionalReferenceNumberSupporter.CreateOrUpdate("AAA", Core.Constants.CountryCodes.Australia, "11111", null);
			additionalReferenceNumberSupporter.CreateOrUpdate("BBB", Core.Constants.CountryCodes.Australia, "22222", null);

			AssertContainsExactElementsInAnyOrder(new[] { "AAA|11111", "BBB|22222" },
				shipment.Numbers.Cast<CusEntryNumber>().Select((c) => string.Concat(c.CE_EntryType, "|", c.CE_EntryNum)).ToArray());

			shipment = Factory.New<CommonShipment>();

			CusEntryNumber cusEntryNumber1 = shipment.Numbers.AddNew();
			cusEntryNumber1.CE_EntryType = "AAA";
			cusEntryNumber1.CE_EntryNum = "00000";

			CusEntryNumber cusEntryNumber2 = shipment.Numbers.AddNew();
			cusEntryNumber2.CE_EntryType = "BBB";
			cusEntryNumber2.CE_EntryNum = "11111";

			CusEntryNumber cusEntryNumber3 = shipment.Numbers.AddNew();
			cusEntryNumber3.CE_EntryType = "BBB";
			cusEntryNumber3.CE_EntryNum = "22222";

			additionalReferenceNumberSupporter = shipment;
			additionalReferenceNumberSupporter.CreateOrUpdate("AAA", Core.Constants.CountryCodes.Australia, "11111", null);
			additionalReferenceNumberSupporter.CreateOrUpdate("BBB", Core.Constants.CountryCodes.Australia, "22222", null);

			AssertContainsExactElementsInAnyOrder(new[] { "AAA|11111", "BBB|11111", "BBB|22222" },
				shipment.Numbers.Cast<CusEntryNumber>().Select((c) => string.Concat(c.CE_EntryType, "|", c.CE_EntryNum)).ToArray());
		}

		public void TestCusEntryNumberSupporter()
		{
			CommonShipment shipment = GetShipment();
			RefCountry australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia);
			RefCountry iceland = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Iceland);
			CreateCusEntryNumber(shipment, "COC", iceland, "123", CusEntryNumber.Categories.CustomsPermitClearanceNumber);

			NotificationBuffer notify = new NotificationBuffer();
			(shipment as ICusEntryNumberSupporter).CreateOrUpdate("COC", australia.RN_Code, "123", notify);
			CusEntryNumber number = GetCusEntryNumber(shipment, "COC", australia, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			AssertNull("COC reference number", number);
			AssertContains("No support for customs entry number type 'COC'", notify.AsString);

			(shipment as ICusEntryNumberSupporter).CreateOrUpdate("CAN", australia.RN_Code, "123", new NotificationBuffer());
			number = GetCusEntryNumber(shipment, "CAN", australia, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			AssertNotNull("CAN reference number", number);
			AssertEquals("123", number.CE_EntryNum);
			(shipment as ICusEntryNumberSupporter).CreateOrUpdate("CAN", australia.RN_Code, "aaa", new NotificationBuffer());
			number = GetCusEntryNumber(shipment, "CAN", australia, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			AssertNotNull("Customs number", number);
			AssertEquals("aaa", number.CE_EntryNum);

			shipment = GetShipment();
			CreateCusEntryNumber(shipment, "CAN", iceland, "123", CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			CreateCusEntryNumber(shipment, "CAN", australia, "123", CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			(shipment as ICusEntryNumberSupporter).CreateOrUpdate("CCN", australia.RN_Code, "aaa", new NotificationBuffer());
			number = GetCusEntryNumber(shipment, "CCN", australia, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			AssertEquals("aaa", number.CE_EntryNum);
			number = GetCusEntryNumber(shipment, "CAN", australia, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			AssertNull("Customs number", number);
			CreateCusEntryNumber(shipment, "CAN", australia, "bbb", CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			(shipment as ICusEntryNumberSupporter).CreateOrUpdate("XLV", australia.RN_Code, "ccc", new NotificationBuffer());
			number = GetCusEntryNumber(shipment, "XLV", australia, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			AssertNotNull("Customs number", number);
			AssertEquals("ccc", number.CE_EntryNum);
			number = GetCusEntryNumber(shipment, "CCN", australia, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			AssertNotNull("Customs number", number);
			number = GetCusEntryNumber(shipment, "CAN", australia, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			AssertNotNull("Customs number", number);
		}

		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var shipment = Factory.New<CommonShipment>();
			var numberTypeProvider = shipment as IAdditionalReferenceNumberTypeProvider;

			Action<string> assertNumberTypeList = (countryCode) =>
			{
				var port = shipment.JS_RL_NKDestination = SetPort(countryCode);
				var actualList = numberTypeProvider.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, countryCode);

				foreach (ICodeDescription pair in CusEntryNumLookups.GetAdditionalReferenceNumberTypes(new AdditionalReferenceNumberTypesParameters(countryCode) { Parent = shipment, DischargeCountryCode = port.Left(2) }))
				{
					Assert($"Should contain {countryCode}/{pair.Code}", actualList.ContainsCode(pair.Code));
				}

				foreach (ICodeDescription pair in NonCustomsAdditionalReferences)
				{
					Assert($"Should contain {countryCode}/{pair.Code}", actualList.ContainsCode(pair.Code));
				}
			};

			assertNumberTypeList(Constants.CountryCodes.Australia);
			assertNumberTypeList(Constants.CountryCodes.China);
			assertNumberTypeList(Constants.CountryCodes.HongKong);
			assertNumberTypeList(Constants.CountryCodes.Taiwan);
			assertNumberTypeList(Constants.CountryCodes.UnitedStates);
			assertNumberTypeList(Constants.CountryCodes.Israel);

			var numberTypeList = numberTypeProvider.GetAdditionalReferenceNumberTypeList("XXX", Constants.CountryCodes.Australia);
			AssertEquals("Empty list when category is not AdditionalReferenceNumber", 0, numberTypeList.Count);
		}

		CodeDescriptionPairList NonCustomsAdditionalReferences => new ShipmentNonCustomsAdditionalReferenceCodesCodeList();

		ZString SetPort(ZString countryCode)
		{
			var result = ZString.Empty;
			switch (countryCode)
			{
				case Constants.CountryCodes.Israel:
					result = "ILTLV";
					break;
			}
			return result;
		}

		#endregion

		#region Confirmations

		public void TestSetPickupConfirmsComplete()
		{
			ZDateTime now = ZDateTime.Now;
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.AIR;
			shipment.JS_OuterPacks = 10;
			AssertEquals(0, shipment.PickupConfirms.Count);
			shipment.DocsAndCartage.JP_PickupCartageCompleted = now;
			shipment.DocsAndCartage.JP_PickupRequiredBy = now.AddHours(2);
			shipment.DocsAndCartage.JP_EstimatedPickup = now.AddHours(3);

			AssertEquals(1, shipment.PickupConfirms.Count);
			AssertEquals(10, shipment.PickupConfirms[0].TotalBookedPackages);
			AssertEquals(10, shipment.PickupConfirms[0].TotalDeliveredPackages);
			AssertEquals(now, shipment.PickupConfirms[0].EU_PickupDeliveryTime);
			AssertEquals(now.AddHours(2), shipment.PickupConfirms[0].EU_RequestedPickupDeliveryTime);
			AssertEquals(now.AddHours(3), shipment.PickupConfirms[0].EU_PlannedPickupDeliveryTime);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Empty;
			AssertEquals(1, shipment.PickupConfirms.Count);
			AssertEquals(10, shipment.PickupConfirms[0].TotalBookedPackages);
			AssertEquals(10, shipment.PickupConfirms[0].TotalDeliveredPackages);
			AssertEquals(ZDateTime.Empty, shipment.PickupConfirms[0].EU_PickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, shipment.PickupConfirms[0].EU_RequestedPickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, shipment.PickupConfirms[0].EU_PlannedPickupDeliveryTime);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = now.AddDays(1);
			shipment.DocsAndCartage.JP_PickupRequiredBy = now.AddDays(1).AddHours(2);
			shipment.DocsAndCartage.JP_EstimatedPickup = now.AddDays(1).AddHours(3);
			AssertEquals(1, shipment.PickupConfirms.Count);
			AssertEquals(10, shipment.PickupConfirms[0].TotalBookedPackages);
			AssertEquals(10, shipment.PickupConfirms[0].TotalDeliveredPackages);
			AssertEquals(now.AddDays(1), shipment.PickupConfirms[0].EU_PickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(2), shipment.PickupConfirms[0].EU_RequestedPickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(3), shipment.PickupConfirms[0].EU_PlannedPickupDeliveryTime);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			CommonContainer container = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			shipment = consol.Shipments.AddNew();
			PackLine packline = shipment.OuterPackLines.AddNew();
			PackLine packline2 = shipment.OuterPackLines.AddNew();
			packline.SetContainer(consol, container);
			packline2.SetContainer(consol, container2);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = now;
			shipment.DocsAndCartage.JP_PickupRequiredBy = now.AddHours(2);
			shipment.DocsAndCartage.JP_EstimatedPickup = now.AddHours(3);

			AssertEquals(now, container.OriginConfirm.EU_PickupDeliveryTime);
			AssertEquals(now, container2.OriginConfirm.EU_PickupDeliveryTime);
			AssertEquals(now.AddHours(2), container.OriginConfirm.EU_RequestedPickupDeliveryTime);
			AssertEquals(now.AddHours(2), container2.OriginConfirm.EU_RequestedPickupDeliveryTime);
			AssertEquals(now.AddHours(3), container.OriginConfirm.EU_PlannedPickupDeliveryTime);
			AssertEquals(now.AddHours(3), container2.OriginConfirm.EU_PlannedPickupDeliveryTime);

			AssertEquals(now, container.JC_DepartureCartageComplete);
			AssertEquals(now, container2.JC_DepartureCartageComplete);
			AssertEquals(now.AddHours(3), container.JC_DepartureEstimatedPickup);
			AssertEquals(now.AddHours(3), container2.JC_DepartureEstimatedPickup);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, container.OriginConfirm.EU_PickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, container2.OriginConfirm.EU_PickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, container.OriginConfirm.EU_RequestedPickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, container2.OriginConfirm.EU_RequestedPickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, container.OriginConfirm.EU_PlannedPickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, container2.OriginConfirm.EU_PlannedPickupDeliveryTime);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = now.AddDays(1);
			shipment.DocsAndCartage.JP_PickupRequiredBy = now.AddDays(1).AddHours(2);
			shipment.DocsAndCartage.JP_EstimatedPickup = now.AddDays(1).AddHours(3);
			AssertEquals(now.AddDays(1), container.OriginConfirm.EU_PickupDeliveryTime);
			AssertEquals(now.AddDays(1), container2.OriginConfirm.EU_PickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(2), container.OriginConfirm.EU_RequestedPickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(2), container2.OriginConfirm.EU_RequestedPickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(3), container.OriginConfirm.EU_PlannedPickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(3), container2.OriginConfirm.EU_PlannedPickupDeliveryTime);
		}

		public void TestSetDeliveryConfirmsComplete()
		{
			ZDateTime now = ZDateTime.Now;
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.AIR;
			shipment.JS_OuterPacks = 10;
			AssertEquals(0, shipment.DeliveryConfirms.Count);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = now;
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = now.AddHours(2);
			shipment.DocsAndCartage.JP_EstimatedDelivery = now.AddHours(3);

			AssertEquals(1, shipment.DeliveryConfirms.Count);
			AssertEquals(10, shipment.DeliveryConfirms[0].TotalBookedPackages);
			AssertEquals(10, shipment.DeliveryConfirms[0].TotalDeliveredPackages);

			AssertEquals(now, shipment.DeliveryConfirms[0].EU_PickupDeliveryTime);
			AssertEquals(now.AddHours(2), shipment.DeliveryConfirms[0].EU_RequestedPickupDeliveryTime);
			AssertEquals(now.AddHours(3), shipment.DeliveryConfirms[0].EU_PlannedPickupDeliveryTime);

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			AssertEquals(1, shipment.DeliveryConfirms.Count);
			AssertEquals(10, shipment.DeliveryConfirms[0].TotalBookedPackages);
			AssertEquals(10, shipment.DeliveryConfirms[0].TotalDeliveredPackages);
			AssertEquals(ZDateTime.Empty, shipment.DeliveryConfirms[0].EU_PickupDeliveryTime);

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = now.AddDays(1);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = now.AddDays(1).AddHours(2);
			shipment.DocsAndCartage.JP_EstimatedDelivery = now.AddDays(1).AddHours(3);

			AssertEquals(1, shipment.DeliveryConfirms.Count);
			AssertEquals(10, shipment.DeliveryConfirms[0].TotalBookedPackages);
			AssertEquals(10, shipment.DeliveryConfirms[0].TotalDeliveredPackages);
			AssertEquals(now.AddDays(1), shipment.DeliveryConfirms[0].EU_PickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(2), shipment.DeliveryConfirms[0].EU_RequestedPickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(3), shipment.DeliveryConfirms[0].EU_PlannedPickupDeliveryTime);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			CommonContainer container = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			shipment = consol.Shipments.AddNew();
			PackLine packline = shipment.OuterPackLines.AddNew();
			PackLine packline2 = shipment.OuterPackLines.AddNew();
			packline.SetContainer(consol, container);
			packline2.SetContainer(consol, container2);

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = now;
			AssertEquals(now, container.DestinationConfirm.EU_PickupDeliveryTime);
			AssertEquals(now, container2.DestinationConfirm.EU_PickupDeliveryTime);

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, container.DestinationConfirm.EU_PickupDeliveryTime);
			AssertEquals(ZDateTime.Empty, container2.DestinationConfirm.EU_PickupDeliveryTime);

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = now.AddDays(1);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = now.AddDays(1).AddHours(2);
			shipment.DocsAndCartage.JP_EstimatedDelivery = now.AddDays(1).AddHours(3);

			AssertEquals(now.AddDays(1), container.DestinationConfirm.EU_PickupDeliveryTime);
			AssertEquals(now.AddDays(1), container2.DestinationConfirm.EU_PickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(3), container.DestinationConfirm.EU_PlannedPickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(3), container2.DestinationConfirm.EU_PlannedPickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(2), container.DestinationConfirm.EU_RequestedPickupDeliveryTime);
			AssertEquals(now.AddDays(1).AddHours(2), container2.DestinationConfirm.EU_RequestedPickupDeliveryTime);
		}

		public void TestConfirmsCompletePerPackLine()
		{
			var now = ZDateTime.Now;
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.AIR;

			var packline = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();
			var packLine3 = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 5;
			packline2.JL_PackageCount = 4;
			packLine3.JL_PackageCount = 3;

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = now;

			shipment.DeliveryConfirms[0].GetDivot(packline).J8_PackagesDelivered = 6;
			shipment.DeliveryConfirms[0].GetDivot(packline2).J8_PackagesDelivered = 4;
			shipment.DeliveryConfirms[0].GetDivot(packLine3).J8_PackagesDelivered = 2;

			AssertEquals(false, shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual));
		}

		public void TestAutoCreateLooseConfirmations()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(true, shipment.AutoCreateLooseConfirmations);
		}

		#endregion

		public void TestCFSPickupAddressDefault()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.AddAddressType(OrgAddressType.Office);

			var exportAddress = org.Addresses.AddNew();
			exportAddress.AddAddressType(OrgAddressType.Pickup);

			var importAddress = org.Addresses.AddNew();
			importAddress.AddAddressType(OrgAddressType.Delivery);

			var shipment = Factory.New<CommonShipment>();

			shipment.JS_OA_ExportReceivingDepot_ZAddress.OrgPK = org.PK;
			AssertEquals("Should be Pickup Address", exportAddress.PK, shipment.JS_OA_ExportReceivingDepot);
		}

		public void TestCFSDeliveryAddressDefault()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.AddAddressType(OrgAddressType.Office);

			var exportAddress = org.Addresses.AddNew();
			exportAddress.AddAddressType(OrgAddressType.Pickup);

			var importAddress = org.Addresses.AddNew();
			importAddress.AddAddressType(OrgAddressType.Delivery);

			var shipment = Factory.New<CommonShipment>();

			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = org.PK;
			AssertEquals("Should be Delivery Address", importAddress.PK, shipment.JS_OA_ImportReleaseDepot);
		}

		public void TestJS_JS_ColoadMasterShipmentForBinding()
		{
			CommonShipment master1 = Factory.New<CommonShipment>();
			CommonShipment master2 = Factory.New<CommonShipment>();

			CommonShipment sub = Factory.New<CommonShipment>();
			sub.JS_JS_ColoadMasterShipmentForBinding = master1.PK;

			int valueNotSetHitCount = 0;
			sub.ValueNotSet += (s, e) => valueNotSetHitCount++;

			AssertEquals(master1.PK, sub.JS_JS_ColoadMasterShipmentForBinding);
			AssertEquals(sub.JS_JS_ColoadMasterShipmentForBinding, sub.JS_JS_ColoadMasterShipment);
			AssertEquals(0, valueNotSetHitCount);

			sub.JS_JS_ColoadMasterShipmentForBinding = master2.PK;
			AssertEquals(master2.PK, sub.JS_JS_ColoadMasterShipmentForBinding);
			AssertEquals(sub.JS_JS_ColoadMasterShipmentForBinding, sub.JS_JS_ColoadMasterShipment);
			AssertEquals(0, valueNotSetHitCount);

			sub.JS_JS_ColoadMasterShipmentForBinding = master1.PK;

			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = false;

			try
			{
				sub.JS_JS_ColoadMasterShipmentForBinding = ZGuid.Empty;

				AssertEquals(master1.PK, sub.JS_JS_ColoadMasterShipmentForBinding);
				AssertEquals(sub.JS_JS_ColoadMasterShipmentForBinding, sub.JS_JS_ColoadMasterShipment);
				AssertEquals(1, valueNotSetHitCount);

				sub.JS_JS_ColoadMasterShipmentForBinding = master2.PK;
				AssertEquals(master2.PK, sub.JS_JS_ColoadMasterShipmentForBinding);
				AssertEquals(sub.JS_JS_ColoadMasterShipmentForBinding, sub.JS_JS_ColoadMasterShipment);
				AssertEquals(1, valueNotSetHitCount);
			}
			finally
			{
				Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
			}
		}

		public void TestHasConsolPastCutOffDate()
		{
			CommonConsol consol1 = Factory.New<CommonConsol>();
			CommonConsol consol2 = Factory.New<CommonConsol>();
			CommonConsol consol3 = Factory.New<CommonConsol>();

			consol2.JK_ConsolCutOffDateLocal = ZDateTime.Now.AddDays(1);
			consol3.JK_ConsolCutOffDateLocal = ZDateTime.Now.AddDays(-1);

			CommonShipment shipment = Factory.New<CommonShipment>();

			Assert("No consols", !shipment.HasConsolPastCutOffDate);

			shipment.Consols.Add(consol1);

			Assert("Consol without a cutoff date", !shipment.HasConsolPastCutOffDate);

			shipment.Consols.Add(consol2);

			Assert("Consol with future cutoff date", !shipment.HasConsolPastCutOffDate);

			shipment.Consols.Add(consol3);

			Assert("Consol past cutoff date", shipment.HasConsolPastCutOffDate);
		}

		public void TestUpdateCustomsEntryIssueDate()
		{
			CommonShipment shipment = GetShipment();
			RefCountry japan = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Japan);
			CreateCusEntryNumber(shipment, "PMT", japan, "Japan Permit6", CusEntryNumber.Categories.CustomsPermitClearanceNumber);

			CusEntryNumber number = GetCusEntryNumber(shipment, "PMT", japan, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			ZDateTime currentDateTime = ZDateTime.Now;
			shipment.UpdateCustomsEntryIssueDate("PMT", japan.RN_Code, currentDateTime);
			AssertEquals(currentDateTime, number.CE_IssueDate);
		}

		public void TestUpdateETAETDFromArrivalAndDepartrureEvent_MatchingLocation()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";

			var eventTime = new ZDateTimeOffset(2019, 6, 12);
			shipment.Logs.AddNew(Events.Departure, "|LOC=USLAX", eventTime, true);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, shipment.JS_E_DEP);
			shipment.Logs.AddNew(Events.Arrival, "|LOC=USLAX", eventTime.AddDays(10), true);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, shipment.JS_E_ARV);

			shipment.Logs.AddNew(Events.Departure, string.Empty, eventTime, true);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, shipment.JS_E_DEP);
			shipment.Logs.AddNew(Events.Arrival, string.Empty, eventTime.AddDays(10), true);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, shipment.JS_E_ARV);

			shipment.Logs.AddNew(Events.Departure, "|LOC=AUSYD", eventTime, true);
			AssertEquals("Location matched, date updated", eventTime.ToZDateTime(), shipment.JS_E_DEP);
			shipment.Logs.AddNew(Events.Arrival, "|LOC=CNSHA", eventTime.AddDays(10), true);
			AssertEquals("Location matched, date updated", eventTime.AddDays(10).ToZDateTime(), shipment.JS_E_ARV);
		}

		public void TestETDETAAddDateEventWithLocationParameter()
		{
			var shipment = GetShipment();

			var eventTime = new ZDateTime(2019, 6, 12);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";

			shipment.JS_E_DEP = eventTime;
			shipment.JS_E_ARV = eventTime.AddDays(10);

			Assert("LOC parameter added", shipment.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.DepartureCode && x.SL_Reference == $"To: {eventTime.ToShortDateString()}|LOC=AUSYD"));
			Assert("LOC parameter added", shipment.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.ArrivalCode && x.SL_Reference == $"To: {eventTime.AddDays(10).ToShortDateString()}|LOC=CNSHA"));

			shipment.JS_RL_NKOrigin = string.Empty;
			shipment.JS_RL_NKDestination = string.Empty;

			shipment.JS_E_DEP = eventTime.AddDays(15);
			shipment.JS_E_ARV = eventTime.AddDays(20);

			Assert("LOC parameter not added for empty origin", shipment.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.DepartureCode && x.SL_Reference == $"To: {eventTime.AddDays(15).ToShortDateString()}"));
			Assert("LOC parameter not added for empty destination", shipment.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.ArrivalCode && x.SL_Reference == $"To: {eventTime.AddDays(20).ToShortDateString()}"));
		}

		public void TestShipmentInnerPacksUnit()
		{
			FreightPacksDataRegistry.Instance.InnerPackUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DOZ");
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals("DOZ", shipment.ShipmentInnerPacksUnit);
		}

		public void TestInnerPackLineCollection()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals("Precondition", 0, shipment.JS_TotalPackageCount);

			shipment.JS_F3_NKTotalCountPackType = "BAG";
			shipment.JS_ActualVolume = 2;
			shipment.JS_UnitOfVolume = "CF";
			shipment.JS_ActualWeight = 30;
			shipment.JS_UnitOfWeight = "LB";
			AssertEquals("Inner pack line is not created if the Total Package Count is 0", 0, shipment.InnerPackLines.Count);

			shipment.JS_TotalPackageCount = 100;
			AssertEquals("Inner pack line is created if the Total Package Count is > 0", 1, shipment.InnerPackLines.Count);

			PackLine inner = shipment.InnerPackLines[0];
			AssertEquals("Count", 100, inner.JL_PackageCount);
			AssertEquals("Volume", (ZDecimal)2, inner.JL_ActualVolume);
			AssertEquals("VolumeUQ", "CF", inner.JL_ActualVolumeUQ);
			AssertEquals("Weight", (ZDecimal)30, inner.JL_ActualWeight);
			AssertEquals("WeightUQ", "LB", inner.JL_ActualWeightUQ);
		}

		public void TestSetJS_TotalPackageCount()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TotalPackageCount = 100;
			AssertEquals("Inner pack line is created", 1, shipment.InnerPackLines.Count);
		}

		public void TestAssemblyMasterNotCreateOrphanInnerPackline()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.CoLoadShipments.AddNew();
			shipment.JS_TotalPackageCount = 2;
			AssertEquals(0, shipment.InnerPackLines.Count);
		}

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent commonShipment = Factory.New<CommonShipment>();
			Assert(commonShipment.AllowInvoiceDeletion);
		}

		#endregion

		#region Job Header Deactivation Tests

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var shipment = Factory.New<CommonShipment>();
			var jobLoader = new JobHeader.Loader(shipment);
			var job = jobLoader.TryCreate();
			Factory.Save();

			shipment.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("shipment {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deleted by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating shipment, IsCancelled flag should be set to true", shipment.IsCancelled);
			Assert("Deactivating shipment, IsCancelledInfo should have changes", shipment.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, shipment.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet cancelled", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		#endregion

		public void TestLogs()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(typeof(CommonShipmentLogs), shipment.Logs.GetType());
		}

		public void TestLogs_QuickBookingLogsDoNotGetLoaded()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			var logParent = (IStmALogParent)quotedBooking;
			logParent.Logs.AddNew(AutoEvents.Authorised);

			logParent = (IStmALogParent)quotedBooking.ForwardingShipment;
			logParent.Logs.AddNew(AutoEvents.MiscellaneousEvent);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var quotedBookingInAnotherFactory = factory.Load<IQuotedBooking>(quotedBooking.ForwardingShipment.PK);
			var forwardingShipment = (CommonShipment)quotedBookingInAnotherFactory.ForwardingShipment;
			forwardingShipment.JS_IsForwardRegistered = true;

			factory.Save();

			factory = new BusinessObjectFactory();

			var commonShipment = (IStmALogParent)factory.Load<CommonShipment>(quotedBooking.ForwardingShipment.PK);

			AssertContainsExactElementsInAnyOrder("booking logs should not be loaded",
				new[] { "MIS" },
				commonShipment.Logs.GetAllLogs().Cast<StmALog>()
					.Where(log => log.SL_SE_NKEvent != AutoEvents.AddedARecordToTheSystemCode && log.SL_SE_NKEvent != AutoEvents.EditedARecordCode)
					.Select(log => log.SL_SE_NKEvent.ToString()));
		}

		public void TestJS_InspectionTypeCode()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "DEHAM";

			AssertEquals("Inspection Type defaults to UNK", "UNK", shipment.JS_InspectionTypeCode);

			CusEntryNumber number = Factory.New<CusEntryNumber>();
			number.CE_ParentTable = "JobShipment";
			number.CE_ParentID = shipment.PK;
			number.CE_Category = "INS";
			number.CE_EntryType = "INS";
			number.CE_RN_NKCountryCode = ZString.Empty;
			number.CE_EntryNum = "XRY";

			CusEntryNumber auNumber = Factory.New<CusEntryNumber>();
			auNumber.CE_ParentTable = "JobShipment";
			auNumber.CE_ParentID = shipment.PK;
			auNumber.CE_Category = "INS";
			auNumber.CE_EntryType = "INS";
			auNumber.CE_RN_NKCountryCode = "AU";
			auNumber.CE_EntryNum = "EDS";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("Specific country code is used over blank country code", "EDS", shipment2.JS_InspectionTypeCode);

			shipment2.JS_InspectionTypeCode = "HAN";
			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var shipment3 = factory3.Load<CommonShipment>(shipment.PK);
			AssertEquals("Value is saved correctly", "HAN", shipment3.JS_InspectionTypeCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				AssertEquals("AU code is not used for US company", "XRY", shipment3.JS_InspectionTypeCode);
				shipment3.JS_InspectionTypeCode = "NUC";
			}

			shipment3.JS_InspectionTypeCode = "UNK";
			AssertEquals("AU code is set to UNK", "UNK", shipment3.JS_InspectionTypeCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				AssertEquals("US code is not affected", "NUC", shipment3.JS_InspectionTypeCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			{
				AssertEquals("Other countries still use the fallback code", "XRY", shipment3.JS_InspectionTypeCode);
			}
		}

		public void TestJS_InspectionTypeCode_EUCountrySecurityStatus()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "JPOSA";
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				reloadedShipment.JS_InspectionTypeCode = "XRY";
				newFactory.Save();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("European Union countries share a single Inspection Status", "XRY", reloadedShipment.JS_InspectionTypeCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Liechtenstein))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("Other countries that share the EU security status also share a single Inspection Status", "XRY", reloadedShipment.JS_InspectionTypeCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("Doesn't apply to non EU countries", "UNK", reloadedShipment.JS_InspectionTypeCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("Doesn't apply to non EU countries", "UNK", reloadedShipment.JS_InspectionTypeCode);
			}
		}

		public void TestJS_InspectionTypeCodeRefresh()
		{
			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUBNE";
			shipment1.JS_RL_NKDestination = "DEHAM";
			Factory.Save();

			AssertEquals("Inspection Type defaults to UNK", "UNK", shipment1.JS_InspectionTypeCode);

			var factory2 = NewFactory();
			var shipment2 = factory2.Load<CommonShipment>(shipment1.PK);
			shipment2.JS_InspectionTypeCode = "VPT";

			factory2.Save();

			AssertEquals("Inspection Type Code in Shipment 1 should be VPT.", "VPT", shipment1.JS_InspectionTypeCode);

			bool valueChanged = false;
			shipment2.JS_InspectionTypeCodeInfo.ValueChanged += (sender, args) => valueChanged = true;

			shipment1.JS_InspectionTypeCode = "XRY";
			Factory.Save();

			Assert("JS_InspectionTypeCodeInfo in Shipment 2 should be changed.", valueChanged);
			AssertEquals("Inspection Type Code in Shipment 2 should be XRY.", "XRY", shipment2.JS_InspectionTypeCode);

			valueChanged = false;
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, shipment1.PK);
			var number1 = Factory.Load<CusEntryNumber>(query).FirstOrDefault();
			number1.Delete();
			Factory.Save();

			Assert("JS_InspectionTypeCodeInfo in Shipment 2 should be changed.", valueChanged);
			AssertEquals("Inspection Type Code in shipment2 defaults to UNK.", "UNK", shipment1.JS_InspectionTypeCode);
		}

		#region JS_AdditionalInspectionTypeCode

		public void TestJS_AdditionalInspectionTypeCode_Default()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_IsHighRisk = true;

			AssertEquals("Additional Inspection Type defaults to 'UNK' for CommonShipment", "UNK", shipment.JS_AdditionalInspectionTypeCode);
		}

		public void TestJS_AdditionalInspectionTypeCode_EUCountrySecurityStatus()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "JPOSA";
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				reloadedShipment.JS_IsHighRisk = true;
				reloadedShipment.JS_AdditionalInspectionTypeCode = "XRY";
				newFactory.Save();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("European Union countries share a single Additional Inspection Status", true, reloadedShipment.JS_IsHighRisk);
				AssertEquals("European Union countries share a single Inspection Status", "XRY", reloadedShipment.JS_AdditionalInspectionTypeCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Liechtenstein))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("European Union countries share a single Additional Inspection Status", true, reloadedShipment.JS_IsHighRisk);
				AssertEquals("European Union countries share a single Inspection Status", "XRY", reloadedShipment.JS_AdditionalInspectionTypeCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("Doesn't apply to non EU countries", "UNK", reloadedShipment.JS_AdditionalInspectionTypeCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
				AssertEquals("Doesn't apply to non EU countries", "UNK", reloadedShipment.JS_AdditionalInspectionTypeCode);
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestShipmentShouldNotResetJS_IsBooing()
		{
			TestShipmentShouldNotReset((shipment1, val) => shipment1.JS_IsBooking = val, (shipment2) => shipment2.JS_IsBooking, "CommonShipmentShouldNotResetJS_IsBooking");
		}

		void TestShipmentShouldNotReset(Action<CommonShipment, ZBool> setMethod, Func<CommonShipment, ZBool> getMethod, string key)
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			using (Globals.TemporaryOverrideForIsTest(false))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				var shipment = GetShipment();
				setMethod(shipment, true);

				Factory.Save();

				setMethod(shipment, false);

				Assert(!getMethod(shipment));

				errorReporterMock.Verify(reporter => reporter.Report(key, Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()), Moq.Times.Once);
			}

			errorReporterMock = new Mock<IErrorReporter>();
			using (Globals.TemporaryOverrideForIsTest(false))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				var shipment = GetShipment();
				setMethod(shipment, true);

				Factory.Save();

				using (shipment.SuspendCheckReset())
				{
					setMethod(shipment, false);
				}

				Assert(!getMethod(shipment));

				errorReporterMock.Verify(reporter => reporter.Report(key, Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()), Moq.Times.Never);
			}
		}

		public void TestDocumentFieldExcludeFromMap_JS_IsBooing()
		{
			var isBookingInfo = typeof(CommonShipment).GetProperty("JS_IsBooking");

			Assert(Attribute.IsDefined(isBookingInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMapAttribute), false));
		}

		public void TestDocumentFieldExcludeFromMap_JS_IsForwardRegistered()
		{
			var isForwardRegisteredInfo = typeof(CommonShipment).GetProperty("JS_IsForwardRegistered");

			Assert(Attribute.IsDefined(isForwardRegisteredInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMapAttribute), false));
		}

		[ExpectNoExceptions]
		public void TestShipmentShouldNotResetJS_IsShipping()
		{
			TestShipmentShouldNotReset((shipment1, val) => shipment1.JS_IsShipping = val, (shipment2) => shipment2.JS_IsShipping, "CommonShipmentShouldNotResetJS_IsShipping");
		}

		[ExpectNoExceptions]
		public void TestShipmentShouldNotResetJS_IsCFSRegistered()
		{
			TestShipmentShouldNotReset((shipment1, val) => shipment1.JS_IsCFSRegistered = val, (shipment2) => false, "CommonShipmentShouldNotResetJS_IsCFSRegistered");
		}

		[ExpectNoExceptions]
		public void TestShipmentShouldNotResetJS_IsForwardRegistered()
		{
			TestShipmentShouldNotReset((shipment1, val) => shipment1.JS_IsForwardRegistered = val, (shipment2) => shipment2.JS_IsForwardRegistered, "CommonShipmentShouldNotResetJS_IsForwardRegistered");
		}

		public void TestDocumentFieldExcludeFromMap_JS_IsShipping()
		{
			var isShippingInfo = typeof(CommonShipment).GetProperty("JS_IsShipping");

			Assert(Attribute.IsDefined(isShippingInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMapAttribute), false));
		}

		public void TestDocumentFieldExcludeFromMap_JS_IsCFSRegistered()
		{
			var isCFSRegisteredInfo = typeof(CommonShipment).GetProperty("JS_IsCFSRegistered");

			Assert(Attribute.IsDefined(isCFSRegisteredInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMapAttribute), false));
		}

		public void TestDocumentFieldExcludeFromMap_JS_TH_OneTimeQuote()
		{
			var oneTimeQuoteInfo = typeof(CommonShipment).GetProperty("JS_TH_OneTimeQuote");

			Assert(Attribute.IsDefined(oneTimeQuoteInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMapAttribute), false));
		}

		public void TestWorkflowSetFieldReadonlyCheckBypassForShipment()
		{
			var isBookingInfo = typeof(CommonShipment).GetProperty("JS_IsBooking");
			Assert(Attribute.IsDefined(isBookingInfo, typeof(WorkflowSetFieldReadonly), false));

			var isForwardRegisteredInfo = typeof(CommonShipment).GetProperty("JS_IsForwardRegistered");
			Assert(Attribute.IsDefined(isForwardRegisteredInfo, typeof(WorkflowSetFieldReadonly), false));

			var isShippingInfo = typeof(CommonShipment).GetProperty("JS_IsShipping");
			Assert(Attribute.IsDefined(isShippingInfo, typeof(WorkflowSetFieldReadonly), false));

			var isCFSRegisteredInfo = typeof(CommonShipment).GetProperty("JS_IsCFSRegistered");
			Assert(Attribute.IsDefined(isCFSRegisteredInfo, typeof(WorkflowSetFieldReadonly), false));

			var oneTimeQuoteInfo = typeof(CommonShipment).GetProperty("JS_TH_OneTimeQuote");
			Assert(Attribute.IsDefined(oneTimeQuoteInfo, typeof(WorkflowSetFieldReadonly), false));
		}

		public void TestActionFieldTypeHiddenForShipment()
		{
			var isBookingInfo = typeof(CommonShipment).GetProperty("JS_IsBooking");
			AssertEquals(ActionFieldType.Hidden, ((ActionFieldAttribute)Attribute.GetCustomAttribute(isBookingInfo, typeof(ActionFieldAttribute))).FieldType);

			var isForwardRegisteredInfo = typeof(CommonShipment).GetProperty("JS_IsForwardRegistered");
			AssertEquals(ActionFieldType.Hidden, ((ActionFieldAttribute)Attribute.GetCustomAttribute(isForwardRegisteredInfo, typeof(ActionFieldAttribute))).FieldType);

			var isShippingInfo = typeof(CommonShipment).GetProperty("JS_IsShipping");
			AssertEquals(ActionFieldType.Hidden, ((ActionFieldAttribute)Attribute.GetCustomAttribute(isShippingInfo, typeof(ActionFieldAttribute))).FieldType);

			var isCFSRegisteredInfo = typeof(CommonShipment).GetProperty("JS_IsCFSRegistered");
			AssertEquals(ActionFieldType.Hidden, ((ActionFieldAttribute)Attribute.GetCustomAttribute(isCFSRegisteredInfo, typeof(ActionFieldAttribute))).FieldType);

			var oneTimeQuoteInfo = typeof(CommonShipment).GetProperty("JS_TH_OneTimeQuote");
			AssertEquals(ActionFieldType.Hidden, ((ActionFieldAttribute)Attribute.GetCustomAttribute(oneTimeQuoteInfo, typeof(ActionFieldAttribute))).FieldType);
		}

		public void TestHasDangerousGoodsSubstancesForbiddenOnPassengerAircraft()
		{
			var undgSubstanceNLM = Factory.New<UNDGSubstance>();
			undgSubstanceNLM.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstanceNLM.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode;
			var undgSubstanceFOB = Factory.New<UNDGSubstance>();
			undgSubstanceFOB.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstanceFOB.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;

			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals(false, shipment.HasDangerousGoodsSubstancesForbiddenOnPassengerAircraft());

			var packline = shipment.OuterPackLines.AddNew();
			var dangerousGood = packline.UNDGs.AddNew();
			dangerousGood.DI_IMOClass = "CLS1";
			dangerousGood.DI_DG = undgSubstanceNLM.PK;
			AssertEquals(false, shipment.HasDangerousGoodsSubstancesForbiddenOnPassengerAircraft());

			packline = shipment.OuterPackLines.AddNew();
			dangerousGood = packline.UNDGs.AddNew();
			dangerousGood.DI_IMOClass = "CLS1";
			dangerousGood.DI_DG = undgSubstanceFOB.PK;
			AssertEquals(true, shipment.HasDangerousGoodsSubstancesForbiddenOnPassengerAircraft());
		}

		public void TestWebCreatedInspectionTypeCodesAreConvertedOnSave()
		{
			var shipment1PK = CreateShipmentWithWebInspectionType(ZString.Empty);
			var shipment2PK = CreateShipmentWithWebInspectionType(Constants.CountryCodes.Australia);

			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, shipment1PK);
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, "");

			var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals("Fallback customs entry number is not affected", "WEB", cusEntryNumber.CE_EntryNum);

			query = new ZQuery(CusEntryNumSchema.CE_ParentID, shipment1PK);
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Australia);

			cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals("Country specific customs entry number is created", "UNK", cusEntryNumber.CE_EntryNum);

			query = new ZQuery(CusEntryNumSchema.CE_ParentID, shipment2PK);

			cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals("Country specific 'WEB' CusEntryNum is deleted on save", null, cusEntryNumber);
		}

		ZGuid CreateShipmentWithWebInspectionType(ZString countryCode)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "JPOSA";

			var inspectionType = Factory.New<CusEntryNumber>();
			inspectionType.CE_Category = "INS";
			inspectionType.CE_EntryType = "INS";
			inspectionType.CE_RN_NKCountryCode = countryCode;
			inspectionType.CE_ParentTable = JobShipmentSchema.Constants.TableName;
			inspectionType.CE_ParentID = shipment.PK;
			inspectionType.CE_EntryNum = "WEB";

			Factory.Save();

			return shipment.PK;
		}

		public void TestCommonShipmentPropertyJs_Calc_LastETA_ShouldNotThrowException()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Combination;
			consol.JK_RL_NKLoadPort = "USLAX";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment1 = factory2.New<CommonShipment>();
			var consolInAnotherFactory = factory2.Load<CommonConsol>(consol.PK);
			shipment1.Consols.Add(consolInAnotherFactory);

			consol.Delete();
			Factory.Save();

			AssertNoExceptionThrown(shipment1.UpdateETAWithPortDefaultDeliveryTime);
		}

		public void TestIsAirOrSeaAirAndHasFirstAirLegLoadingInCurrentCountry()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Assert("Air TransportMode", shipment.IsAirOrSeaAirAndHasFirstAirLegLoadingInCurrentCountry);

			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			Assert(!shipment.IsAirOrSeaAirAndHasFirstAirLegLoadingInCurrentCountry);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = HomePort;
			Assert(shipment.IsAirOrSeaAirAndHasFirstAirLegLoadingInCurrentCountry);
		}

		public void TestIsSeaAirAndHasFirstAirLegLoadingInCurrentCountry()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = HomePort;
			Assert(shipment.IsSeaAirAndHasFirstAirLegLoadingInCurrentCountry);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Assert("Air TransportMode", !shipment.IsSeaAirAndHasFirstAirLegLoadingInCurrentCountry);

			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			consol.JK_RL_NKLoadPort = OverseasPort;
			Assert("Overseas Load Port", !shipment.IsSeaAirAndHasFirstAirLegLoadingInCurrentCountry);
		}

		public void TestIsSeaAir()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Assert(!shipment.IsSeaAir);

			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			Assert(shipment.IsSeaAir);
		}

		#region Freight Spot Rates

		public void TestFreightSpotRatesDefaultValues()
		{
			var shipment = GetShipment();

			AssertEquals(0m, shipment.JS_UnitFreightRate);
			AssertEquals(ZString.Empty, shipment.JS_RX_NKFrtRateCurrency);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, shipment.JS_FreightSpotRateAutoratingMode);

			AssertEquals(0m, shipment.JS_FreightCostRate);
			AssertEquals(ZString.Empty, shipment.JS_RX_NKFreightCostRateCurrency);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, shipment.JS_FreightCostRateAutoratingMode);

			AssertEquals(0m, shipment.JS_GatewayFreightSellRate);
			AssertEquals(ZString.Empty, shipment.JS_RX_NKGatewayFreightSellRateCurrency);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, shipment.JS_FreightGatewaySellRateAutoratingMode);
		}

		public void TestStandardRateAutoratingModeDefaultsToZeroFreightRate()
		{
			var shipment = GetShipment();

			shipment.JS_UnitFreightRate = 120.2570m;
			shipment.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_FreightCostRate = 100.2600m;
			shipment.JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_GatewayFreightSellRate = 69.1500m;
			shipment.JS_FreightGatewaySellRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;

			AssertEquals(120.2570m, shipment.JS_UnitFreightRate);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.AllInRate, shipment.JS_FreightSpotRateAutoratingMode);
			AssertEquals(100.2600m, shipment.JS_FreightCostRate);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, shipment.JS_FreightCostRateAutoratingMode);
			AssertEquals(69.1500m, shipment.JS_GatewayFreightSellRate);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, shipment.JS_FreightGatewaySellRateAutoratingMode);

			shipment.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightGatewaySellRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;

			CombineAssertions("When a StandardRate autorating mode is entered, rate should default to zero", () =>
			{
				AssertEquals(ZDecimal.Zero, shipment.JS_UnitFreightRate);
				AssertEquals(ZDecimal.Zero, shipment.JS_FreightCostRate);
				AssertEquals(ZDecimal.Zero, shipment.JS_GatewayFreightSellRate);
			});
		}

		public void TestNonZeroFreightSpotRateDefaultsToFreightPlusRateAutoratingMode()
		{
			var shipment = GetShipment();

			CombineAssertions("Pre-condition: by default, freight spot rates are set to zero, autorating modes are set to StandardRate", () =>
			{
				AssertEquals(ZDecimal.Zero, shipment.JS_UnitFreightRate);
				AssertEquals(ZDecimal.Zero, shipment.JS_FreightCostRate);
				AssertEquals(ZDecimal.Zero, shipment.JS_GatewayFreightSellRate);

				AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, shipment.JS_FreightSpotRateAutoratingMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, shipment.JS_FreightCostRateAutoratingMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, shipment.JS_FreightGatewaySellRateAutoratingMode);
			});

			shipment.JS_UnitFreightRate = 120.2570m;
			shipment.JS_FreightCostRate = 100.2600m;
			shipment.JS_GatewayFreightSellRate = 69.1500m;

			CombineAssertions("When a non-zero rate is entered, autorating mode should default from StandardRate to FreightPlusRate", () =>
			{
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, shipment.JS_FreightSpotRateAutoratingMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, shipment.JS_FreightCostRateAutoratingMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, shipment.JS_FreightGatewaySellRateAutoratingMode);
			});

			shipment.JS_FreightSpotRateAutoratingMode = "";
			shipment.JS_FreightCostRateAutoratingMode = "";
			shipment.JS_FreightGatewaySellRateAutoratingMode = "";

			shipment.JS_UnitFreightRate = 152.2160m;
			shipment.JS_FreightCostRate = 150.2600m;
			shipment.JS_GatewayFreightSellRate = 65.1500m;

			CombineAssertions("When a non-zero rate is entered, autorating mode should default from an empty value to FreightPlusRate", () =>
			{
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, shipment.JS_FreightSpotRateAutoratingMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, shipment.JS_FreightCostRateAutoratingMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, shipment.JS_FreightGatewaySellRateAutoratingMode);
			});

			shipment.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_FreightGatewaySellRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.AllInRate;

			shipment.JS_UnitFreightRate = 100.2560m;
			shipment.JS_FreightCostRate = 75.2600m;
			shipment.JS_GatewayFreightSellRate = 55.1500m;

			CombineAssertions("Autorating mode should not re-default from non-empty, non-StardardRate", () =>
			{
				AssertEquals(Constants.FreightRateAutoratingModes.Code.AllInRate, shipment.JS_FreightSpotRateAutoratingMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.AllInRate, shipment.JS_FreightCostRateAutoratingMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.AllInRate, shipment.JS_FreightGatewaySellRateAutoratingMode);
			});
		}

		public void TestFreightSpotRatesCurrencyDefaultsBasedOnPackingMode()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = "CNBJS";
			shipment.JS_RL_NKDestination = "NZAKL";

			shipment.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_FreightGatewaySellRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;

			CombineAssertions("Freight rate currency should be blank by default", () =>
			{
				AssertEquals(ZString.Empty, shipment.JS_RX_NKFrtRateCurrency);
				AssertEquals(ZString.Empty, shipment.JS_RX_NKFreightCostRateCurrency);
				AssertEquals(ZString.Empty, shipment.JS_RX_NKGatewayFreightSellRateCurrency);
			});

			ResetCurrencyToZeroAndSetRates(shipment);

			CombineAssertions("Freight rate currency should be USD for LCL/FCL Sea shipments", () =>
			{
				AssertEquals(Constants.CurrencyCodes.UnitedStates, shipment.JS_RX_NKFrtRateCurrency);
				AssertEquals(Constants.CurrencyCodes.UnitedStates, shipment.JS_RX_NKFreightCostRateCurrency);
				AssertEquals(Constants.CurrencyCodes.UnitedStates, shipment.JS_RX_NKGatewayFreightSellRateCurrency);
			});

			shipment.JS_PackingMode = Constants.ContainerModes.Liquid;
			shipment.JS_RL_NKOrigin = "CNAAT";
			ResetCurrencyToZeroAndSetRates(shipment);

			CombineAssertions("Freight rate currency should be blank for NON-LCL/FCL Sea shipments", () =>
			{
				AssertEquals(ZString.Empty, shipment.JS_RX_NKFrtRateCurrency);
				AssertEquals(ZString.Empty, shipment.JS_RX_NKFreightCostRateCurrency);
				AssertEquals(ZString.Empty, shipment.JS_RX_NKGatewayFreightSellRateCurrency);
			});

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "CNBDG";
			ResetCurrencyToZeroAndSetRates(shipment);

			CombineAssertions("Freight rate currency should be local currency of Origin for Air shipments", () =>
			{
				AssertEquals(Constants.CurrencyCodes.China, shipment.JS_RX_NKFrtRateCurrency);
				AssertEquals(Constants.CurrencyCodes.China, shipment.JS_RX_NKFreightCostRateCurrency);
				AssertEquals(Constants.CurrencyCodes.China, shipment.JS_RX_NKGatewayFreightSellRateCurrency);
			});

			shipment.JS_RL_NKOrigin = "";
			ResetCurrencyToZeroAndSetRates(shipment);

			CombineAssertions("Freight rate currency should fallback to local currency of current company, Origin is blank for Air shipments", () =>
			{
				AssertEquals(Constants.CurrencyCodes.Australia, shipment.JS_RX_NKFrtRateCurrency);
				AssertEquals(Constants.CurrencyCodes.Australia, shipment.JS_RX_NKFreightCostRateCurrency);
				AssertEquals(Constants.CurrencyCodes.Australia, shipment.JS_RX_NKGatewayFreightSellRateCurrency);
			});

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			shipment.JS_PackingMode = Constants.ContainerModes.LTL;
			shipment.JS_RL_NKOrigin = "CNBDG";
			ResetCurrencyToZeroAndSetRates(shipment);

			CombineAssertions("Freight rate currency should be local currency of Destination for NON-Air, NON-Sea shipments", () =>
			{
				AssertEquals(Constants.CurrencyCodes.NewZealand, shipment.JS_RX_NKFrtRateCurrency);
				AssertEquals(Constants.CurrencyCodes.NewZealand, shipment.JS_RX_NKFreightCostRateCurrency);
				AssertEquals(Constants.CurrencyCodes.NewZealand, shipment.JS_RX_NKGatewayFreightSellRateCurrency);
			});

			shipment.JS_RL_NKDestination = "";
			ResetCurrencyToZeroAndSetRates(shipment);

			CombineAssertions("Freight rate currency should fallback to local currency of current company, if Destination is blank for NON-Air, NON-Sea shipments", () =>
			{
				AssertEquals(Constants.CurrencyCodes.Australia, shipment.JS_RX_NKFrtRateCurrency);
				AssertEquals(Constants.CurrencyCodes.Australia, shipment.JS_RX_NKFreightCostRateCurrency);
				AssertEquals(Constants.CurrencyCodes.Australia, shipment.JS_RX_NKGatewayFreightSellRateCurrency);
			});
		}

		void ResetCurrencyToZeroAndSetRates(CommonShipment shipment)
		{
			shipment.JS_RX_NKFrtRateCurrency = ZString.Empty;
			shipment.JS_RX_NKFreightCostRateCurrency = ZString.Empty;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = ZString.Empty;

			shipment.JS_UnitFreightRate += 10.2500m;
			shipment.JS_FreightCostRate += 25.2600m;
			shipment.JS_GatewayFreightSellRate += 23.1400m;
		}

		#endregion

		#region IsStandAloneShipmentFromBooking

		public void TestIsStandAloneShipmentFromBooking_NewConsol()
		{
			var shipment = GetShipment();
			shipment.JS_IsBooking = ZBool.True;
			Assert(shipment.IsStandAloneShipmentFromBooking);

			var consol = shipment.Consols.AddNew();
			Assert(!shipment.IsStandAloneShipmentFromBooking);

			Factory.Save();
			Assert(!shipment.IsStandAloneShipmentFromBooking);
		}

		public void TestIsStandAloneShipmentFromBooking_ExistedConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "ConsolRef";
			Factory.Save();

			var shipment = GetShipment();
			shipment.JS_IsBooking = ZBool.True;
			Assert(shipment.IsStandAloneShipmentFromBooking);

			shipment.Consols.Add(consol);
			Assert(!shipment.IsStandAloneShipmentFromBooking);

			Factory.Save();
			Assert(!shipment.IsStandAloneShipmentFromBooking);
		}

		public void TestIsNotStandAloneShipmentFromBooking_DetachFromConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "ConsolRef";

			var shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "MasterRef";
			shipment.JS_IsBooking = ZBool.True;
			Assert(shipment.IsStandAloneShipmentFromBooking);

			shipment.Consols.Add(consol);
			Assert(!shipment.IsStandAloneShipmentFromBooking);

			Factory.Save();

			Assert(!shipment.IsStandAloneShipmentFromBooking);
			AssertEquals("ConsolRef|TYP=Consol", shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single().SL_Reference);

			consol.Shipments.Remove(shipment);
			AssertEquals("ConsolRef|TYP=Consol", shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Detached.Code).Single().SL_Reference);
			Assert(!shipment.IsStandAloneShipmentFromBooking);

			Factory.Save();
			Assert(!shipment.IsStandAloneShipmentFromBooking);
		}

		#endregion

		#region Universal Copy

		public void TestUniversalCopyIgnoreElement()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var componentType = shipment.GetType();
			var ignoreElementAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true);
			var attribute = ignoreElementAttributes[0] as UniversalCopyIgnoreElementAttribute;
			AssertCollectionContains("JS_UniqueConsignRef", JobShipmentSchema.Constants.JS_UniqueConsignRef, attribute.ElementNames);
			AssertCollectionContains("JS_OverrideWaybillDefaults", JobShipmentSchema.Constants.JS_OverrideWaybillDefaults, attribute.ElementNames);
			AssertCollectionContains("JS_AdditionalInspectionTypeCode", CommonShipment.Schema.JS_AdditionalInspectionTypeCode, attribute.ElementNames);
			AssertCollectionContains("ShipmentJobHeader", "ShipmentJobHeader", attribute.ElementNames);
			AssertCollectionContains("Job", "Job", attribute.ElementNames);
			AssertCollectionContains("OriginPickupConfirms", "OriginPickupConfirms", attribute.ElementNames);
			AssertCollectionContains("DestinationDeliveryConfirms", "DestinationDeliveryConfirms", attribute.ElementNames);
			AssertCollectionContains("OriginCFSArrivalConfirms", "OriginCFSArrivalConfirms", attribute.ElementNames);
			AssertCollectionContains("OriginCFSDepartureConfirms", "OriginCFSDepartureConfirms", attribute.ElementNames);
			AssertCollectionContains("DestinationCFSArrivalConfirms", "DestinationCFSArrivalConfirms", attribute.ElementNames);
			AssertCollectionContains("DestinationCFSDepartureConfirms", "DestinationCFSDepartureConfirms", attribute.ElementNames);
			AssertCollectionContains("HVLVItemRunningTotals", "HVLVItemRunningTotals", attribute.ElementNames);
			AssertCollectionContains("JS_ElectronicBillOfLadingVersion", JobShipmentSchema.Constants.JS_ElectronicBillOfLadingVersion, attribute.ElementNames);
			AssertCollectionContains("JS_ElectronicBillOfLadingType", JobShipmentSchema.Constants.JS_ElectronicBillOfLadingType, attribute.ElementNames);
			AssertCollectionContains("JS_ElectronicBillOfLadingTerms", JobShipmentSchema.Constants.JS_ElectronicBillOfLadingTerms, attribute.ElementNames);
			AssertCollectionContains("JS_ElectronicBillOfLadingStatus", JobShipmentSchema.Constants.JS_ElectronicBillOfLadingStatus, attribute.ElementNames);
			AssertCollectionContains("JS_ElectronicBillOfLadingReference", JobShipmentSchema.Constants.JS_ElectronicBillOfLadingReference, attribute.ElementNames);
		}

		public void TestUniversalCopy_IsDomesticFreight()
		{
			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKOrigin, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKDestination, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "USCHI";

			Assert("Precondition", shipment.IsDomesticFreight);

			var copyShipment = (CommonShipment)new BusinessObjectCopyManager().Copy(shipment, copyTree).Object;
			Assert("IsDomesticFreight", copyShipment.IsDomesticFreight);

			shipment.JS_RL_NKDestination = "NZAKL";

			Assert("Precondition", !shipment.IsDomesticFreight);

			var copyShipment2 = (CommonShipment)new BusinessObjectCopyManager().Copy(shipment, copyTree).Object;
			Assert("!IsDomesticFreight", !copyShipment2.IsDomesticFreight);
		}

		public void TestUniversalCopy_CusEntryNumbers()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "MYKUL";

			shipment.CustomsEntryNumberType = "CAN";
			shipment.CustomsEntryNumber = "12345";

			Factory.Save();

			Assert("Precondition: shipment CusEntryNum is not system generated", !shipment.CusEntryNumbers[0].CE_EntryIsSystemGenerated);

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKOrigin, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKDestination, CopyMethod = CopyMethod.Copy });

			var cusEntryNumberNode = new EntityCopyTemplateNode { Name = "CusEntryNumber" };
			cusEntryNumberNode.Nodes.Add(new PropertyCopyTemplateNode { Name = CusEntryNumSchema.Constants.CE_EntryType, CopyMethod = CopyMethod.Copy });
			cusEntryNumberNode.Nodes.Add(new PropertyCopyTemplateNode { Name = CusEntryNumSchema.Constants.CE_EntryNum, CopyMethod = CopyMethod.Copy });

			var cusEntryNumbersNode = new CollectionCopyTemplateNode
			{
				Name = "CusEntryNumbers",
				ItemPropertyName = "CusEntryNumbers",
				ItemsTableName = CusEntryNumSchema.Constants.TableName,
				InnerNode = cusEntryNumberNode,
				CopyMethod = CollectionCopyMethod.All
			};

			entityNode.Nodes.Add(cusEntryNumbersNode);

			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var copyManager = new BusinessObjectCopyManager();

			var copiedShipment = copyManager.Copy(shipment, copyTree).Object as CommonShipment;
			AssertNotNull("New CommonShipment created by copy BusinessObjectCopyManager", copiedShipment);
			AssertEquals("AUSYD", copiedShipment.JS_RL_NKOrigin);
			AssertEquals("MYKUL", copiedShipment.JS_RL_NKDestination);

			var copiedCusEntryNumber = copiedShipment.CusEntryNumbers[0];
			AssertEquals("CAN", copiedCusEntryNumber.CE_EntryType);
			AssertEquals("12345", copiedCusEntryNumber.CE_EntryNum);
			Assert("Copied CusEntryNum is not system generated", !copiedCusEntryNumber.CE_EntryIsSystemGenerated);
		}

		public void TestUniversalCopy_AdditionalReferenceNumbers()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "MYKUL";

			var ccnNumber = shipment.Numbers.AddNew();
			ccnNumber.CE_EntryIsSystemGenerated = false;
			ccnNumber.CE_EntryType = "CCN";
			ccnNumber.CE_EntryNum = "12345";

			var hirNumber = shipment.Numbers.AddNew();
			hirNumber.CE_EntryType = "HIR";
			hirNumber.CE_EntryNum = "67890";
			hirNumber.CE_EntryIsSystemGenerated = true;

			Factory.Save();

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKOrigin, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKDestination, CopyMethod = CopyMethod.Copy });

			var cusEntryNumberNode = new EntityCopyTemplateNode { Name = "ReferenceNumber" };
			cusEntryNumberNode.Nodes.Add(new PropertyCopyTemplateNode { Name = CusEntryNumSchema.Constants.CE_Category, CopyMethod = CopyMethod.Copy });
			cusEntryNumberNode.Nodes.Add(new PropertyCopyTemplateNode { Name = CusEntryNumSchema.Constants.CE_RN_NKCountryCode, CopyMethod = CopyMethod.Copy });
			cusEntryNumberNode.Nodes.Add(new PropertyCopyTemplateNode { Name = CusEntryNumSchema.Constants.CE_EntryType, CopyMethod = CopyMethod.Copy });
			cusEntryNumberNode.Nodes.Add(new PropertyCopyTemplateNode { Name = CusEntryNumSchema.Constants.CE_EntryNum, CopyMethod = CopyMethod.Copy });

			var cusEntryNumbersNode = new CollectionCopyTemplateNode
			{
				Name = "ReferenceNumbers",
				ItemPropertyName = "CE_ParentID",
				ItemParentTablePropertyName = "CE_ParentTable",
				ItemsTableName = CusEntryNumSchema.Constants.TableName,
				InnerNode = cusEntryNumberNode,
				CopyMethod = CollectionCopyMethod.All
			};

			entityNode.Nodes.Add(cusEntryNumbersNode);

			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var copyManager = new BusinessObjectCopyManager();

			var copiedShipment = copyManager.Copy(shipment, copyTree).Object as CommonShipment;
			AssertNotNull("New CommonShipment created by copy BusinessObjectCopyManager", copiedShipment);
			AssertEquals("AUSYD", copiedShipment.JS_RL_NKOrigin);
			AssertEquals("MYKUL", copiedShipment.JS_RL_NKDestination);

			var copiedCusEntryNumber = copiedShipment.Numbers[0];
			AssertEquals("CCN", copiedCusEntryNumber.CE_EntryType);
			AssertEquals("12345", copiedCusEntryNumber.CE_EntryNum);
			Assert("Copied CusEntryNum is not system generated", !copiedCusEntryNumber.CE_EntryIsSystemGenerated);

			AssertEquals("HIR is a system generated number that should not be copied", 1, copiedShipment.Numbers.Count);
		}

		public void TestUniversalCopy_PackLine_HSCodes()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "MYKUL";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_HarmonisedCode = "123456789";

			var usHSCode = packline.HarmonisedCodes.AddNew();
			usHSCode.JLH_RN_NKCountry = "US";
			usHSCode.JLH_Code = "111111111";

			var nzHSCode = packline.HarmonisedCodes.AddNew();
			nzHSCode.JLH_RN_NKCountry = "NZ";
			nzHSCode.JLH_Code = "222222222";

			Factory.Save();

			var elementType = typeof(CommonShipment);
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(elementType, true);
			var copyTemplateTree = new CopyTemplateTree(interfaceType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);

			var packLinesNode = (CollectionCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.FirstOrDefault(n => n.Name == "JobPackLines");
			AssertNotNull("Pre-condition: packline collection node", packLinesNode);
			var packLineNode = ((TemplateCopyTemplateNode)packLinesNode.InnerNode).TemplateNode as EntityCopyTemplateNode;
			AssertNotNull("Pre-condition: packline template node", packLineNode);

			var harmonisedCodeNode = (PropertyCopyTemplateNode)(packLineNode.Nodes.FirstOrDefault(n => n.Name == "JL_HarmonisedCode"));
			AssertNotNull("PackLine.JL_HarmonisedCode", harmonisedCodeNode);

			var countryHarmonisedCodesNode = (CollectionCopyTemplateNode)(packLineNode.Nodes.FirstOrDefault(n => n.Name == "HarmonisedCodes"));
			AssertNotNull("PackLine.HarmonisedCodes", countryHarmonisedCodesNode);

			packLinesNode.CopyMethod = CollectionCopyMethod.All;
			packLinesNode.InnerNode = packLineNode;
			((PropertyCopyTemplateNode)(packLinesNode.InnerNode as EntityCopyTemplateNode).Nodes.FirstOrDefault(n => n.Name == "JL_FreightMode")).CopyMethod = CopyMethod.Copy;
			harmonisedCodeNode.CopyMethod = CopyMethod.Copy;
			countryHarmonisedCodesNode.CopyMethod = CollectionCopyMethod.All;
			var countryHarmonisedCodeTemplateNode = countryHarmonisedCodesNode.InnerNode as EntityCopyTemplateNode;
			((PropertyCopyTemplateNode)countryHarmonisedCodeTemplateNode.Nodes.FirstOrDefault(n => n.Name == "JLH_Code")).CopyMethod = CopyMethod.Copy;
			((PropertyCopyTemplateNode)countryHarmonisedCodeTemplateNode.Nodes.FirstOrDefault(n => n.Name == "JLH_RN_NKCountry")).CopyMethod = CopyMethod.Copy;

			var copyManager = new BusinessObjectCopyManager();
			var copiedShipment = copyManager.Copy(shipment, copyTemplateTree).Object as CommonShipment;
			CombineAssertions("PackLine and its HS codes are copied", () =>
			{
				AssertEquals(1, copiedShipment.OuterPackLines.Count);
				var copiedPackLine = copiedShipment.OuterPackLines[0];
				AssertEquals("123456789", copiedPackLine.JL_HarmonisedCode);
				AssertEquals(2, copiedPackLine.HarmonisedCodes.Count);
				AssertEquals("US", copiedPackLine.HarmonisedCodes[0].JLH_RN_NKCountry);
				AssertEquals("111111111", copiedPackLine.HarmonisedCodes[0].JLH_Code);
				AssertEquals("NZ", copiedPackLine.HarmonisedCodes[1].JLH_RN_NKCountry);
				AssertEquals("222222222", copiedPackLine.HarmonisedCodes[1].JLH_Code);
			});
		}

		public void TestUniversalCopy_DefaultElectronicBillOfLadingFields()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			shipment.JS_ElectronicBillOfLadingVersion = 2;
			shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
			shipment.JS_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.Transferable;
			shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
			shipment.JS_ElectronicBillOfLadingReference = "WiseTech";
			shipment.JS_ElectronicBillOfLadingHouseBill = "123456789";

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKOrigin, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_RL_NKDestination, CopyMethod = CopyMethod.Copy });

			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var copyManager = new BusinessObjectCopyManager();

			var copiedShipment = copyManager.Copy(shipment, copyTree).Object as CommonShipment;
			AssertNotNull("New ForwardingShipment created by copy BusinessObjectCopyManager", copiedShipment);
			AssertNullOrEmpty(copiedShipment.JS_TransportMode);

			AssertEquals((ZShort)0, copiedShipment.JS_ElectronicBillOfLadingVersion);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingType);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingTerms);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingStatus);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingReference);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingHouseBill);

			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobShipmentSchema.Constants.JS_TransportMode, CopyMethod = CopyMethod.Copy });

			copiedShipment = copyManager.Copy(shipment, copyTree).Object as CommonShipment;
			AssertNotNull("New ForwardingShipment created by copy BusinessObjectCopyManager", copiedShipment);
			AssertEquals(Core.Constants.TransportModes.Air, copiedShipment.JS_TransportMode);

			AssertEquals((ZShort)0, copiedShipment.JS_ElectronicBillOfLadingVersion);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingType);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingTerms);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingStatus);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingReference);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingHouseBill);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			copiedShipment = copyManager.Copy(shipment, copyTree).Object as CommonShipment;
			AssertNotNull("New ForwardingShipment created by copy BusinessObjectCopyManager", copiedShipment);
			AssertEquals(Core.Constants.TransportModes.Sea, copiedShipment.JS_TransportMode);

			AssertEquals((ZShort)0, copiedShipment.JS_ElectronicBillOfLadingVersion);
			AssertEquals("Default Value", Constants.BillOfLadingBillType.Codes.Straight, copiedShipment.JS_ElectronicBillOfLadingType);
			AssertEquals("Default Value", Constants.BillOfLadingBillTerms.Codes.NonTransferable, copiedShipment.JS_ElectronicBillOfLadingTerms);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingStatus);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingReference);
			AssertNullOrEmpty(copiedShipment.JS_ElectronicBillOfLadingHouseBill);
		}

		#endregion

		#region CarrierServiceLevel

		public void TestCarrierServiceLevelNoExceptionFromNonUniqueCode()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PL_NKCarrierServiceLevel = "ABC";

			var serviceLevel = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel.PL_Code = "ABC";

			var serviceLevel2 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel2.PL_Code = "ABC";

			AssertNoExceptionThrown("Accessing CarrierServiceLevel should not throw exception", () => { _ = shipment.CarrierServiceLevel; });
		}

		#endregion

		#region Shipment Penalties

		public void TestShipmentPenaltiesCollection()
		{
			var shipment1 = Factory.New<CommonShipment>();
			var shipment2 = Factory.New<CommonShipment>();
			var consol1 = Factory.New<CommonConsol>();
			var consol2 = Factory.New<CommonConsol>();

			consol1.Shipments.Add(shipment1);
			consol1.Shipments.Add(shipment2);
			consol2.Shipments.Add(shipment1);
			consol2.Shipments.Add(shipment2);

			var container1_1 = consol1.Containers.AddNew();
			var container1_2 = consol1.Containers.AddNew();
			var container2_1 = consol2.Containers.AddNew();
			var container2_2 = consol2.Containers.AddNew();

			var packLine1_1 = shipment1.OuterPackLines.AddNew();
			var packLine1_2 = shipment1.OuterPackLines.AddNew();
			var packLine2_1 = shipment2.OuterPackLines.AddNew();
			var packLine2_2 = shipment2.OuterPackLines.AddNew();

			//undo any packing that automatically allocated.
			UnpackAllContainers(shipment1);
			UnpackAllContainers(shipment2);

			container1_1.AddPackLine(packLine1_1);
			container1_1.AddPackLine(packLine2_1);
			container1_2.AddPackLine(packLine1_2);
			container2_1.AddPackLine(packLine1_1);
			container2_1.AddPackLine(packLine2_1);
			container2_2.AddPackLine(packLine2_2);

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { container1_1, container1_2, container2_1 }, shipment1.Containers);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { container1_1, container2_1, container2_2 }, shipment2.Containers);

			shipment1.DeliveryPenalties.DeleteAll();
			shipment1.PickupPenalties.DeleteAll();
			shipment2.DeliveryPenalties.DeleteAll();
			shipment2.PickupPenalties.DeleteAll();
			Factory.Save();

			var penalty1 = shipment1.DeliveryPenalties.AddNew();
			penalty1.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty1.CPY_PenaltyType = Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty1.CPY_RX_NKCurrency = "AUD";
			penalty1.CPY_JC_Container = container1_1.PK;

			var penalty2 = shipment2.DeliveryPenalties.AddNew();
			penalty2.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty2.CPY_PenaltyType = Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty2.CPY_RX_NKCurrency = "AUD";
			penalty2.CPY_JC_Container = container2_1.PK;

			var penalty3 = shipment2.DeliveryPenalties.AddNew();
			penalty3.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty3.CPY_PenaltyType = Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty3.CPY_RX_NKCurrency = "AUD";
			penalty3.CPY_JC_Container = container2_2.PK;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var otherShipment1 = factory2.Load<CommonShipment>(shipment1.PK);
			var otherShipment2 = factory2.Load<CommonShipment>(shipment2.PK);

			AssertEquals(1, otherShipment1.DeliveryPenalties.Count);
			AssertEquals(2, otherShipment2.DeliveryPenalties.Count);
			AssertNotNull(otherShipment2.DeliveryPenalties.FindByPK(penalty3.PK));
		}

		#endregion

		#region AddressAdditionalInfo

		public void TestAddressAdditionalInfo_ItemsAreBindedToDependentAdresses_ConsignorAndConsigneePickupDeliveryAddress()
		{
			//Arrange
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ORG3";

			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "ORG4";

			var shipment = Factory.New<CommonShipment>();
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org1.PK;

			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			shipment.ConsignorPickupAddress.E2_OA_Address = org2.MainAddress.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = org2.PK;
			shipment.PickupByTransportMode = "IWT";
			shipment.DeliveryByTransportMode = "RAI";

			Factory.Save();

			Assert(!shipment.PickupByTransportMode_ReadOnly);
			Assert(!shipment.DeliveryByTransportMode_ReadOnly);

			AssertEquals("IWT", shipment.PickupByTransportMode);
			AssertEquals("RAI", shipment.DeliveryByTransportMode);

			//Action
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;

			//Assert: TransportMode shouldn't change when address override is checked
			Assert(!shipment.PickupByTransportMode_ReadOnly);
			Assert(!shipment.DeliveryByTransportMode_ReadOnly);

			AssertEquals("IWT", shipment.PickupByTransportMode);
			AssertEquals("RAI", shipment.DeliveryByTransportMode);

			//Action
			shipment.ConsignorPickupAddress.E2_CompanyName = string.Empty;
			shipment.ConsigneeDeliveryAddress.E2_CompanyName = string.Empty;

			//Assert: TransportMode shouldn't change when Company name is set to empty
			Assert(!shipment.PickupByTransportMode_ReadOnly);
			Assert(!shipment.DeliveryByTransportMode_ReadOnly);

			AssertEquals("IWT", shipment.PickupByTransportMode);
			AssertEquals("RAI", shipment.DeliveryByTransportMode);

			//Action
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;

			//Assert: TransportMode shouldn't change when we uncheck the override address
			Assert(!shipment.PickupByTransportMode_ReadOnly);
			Assert(!shipment.DeliveryByTransportMode_ReadOnly);

			AssertEquals("IWT", shipment.PickupByTransportMode);
			AssertEquals("RAI", shipment.DeliveryByTransportMode);
		}

		public void TestAddressAdditionalInfo_ItemsAreBindedToDependentAdresses()
		{
			//Arrange
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ORG3";

			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "ORG4";

			var shipment = Factory.New<CommonShipment>();
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;

			//Action
			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = org1.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org2.PK;
			shipment.JS_OA_ExportReceivingDepot = org3.MainAddress.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = org3.PK;

			shipment.CFSArrivalByTransportMode = "ROA";
			shipment.CFSDepartureByTransportMode = "IWT";
			shipment.DeliveryByTransportMode = "IWT";
			shipment.PickupByTransportMode = "RAI";

			Factory.Save();

			//Assert: Transport modes should be mapped correctly
			Assert(!shipment.CFSArrivalByTransportMode_ReadOnly);
			Assert(!shipment.DeliveryByTransportMode_ReadOnly);
			Assert(!shipment.CFSDepartureByTransportMode_ReadOnly);
			Assert(!shipment.PickupByTransportMode_ReadOnly);

			AssertEquals("ROA", shipment.CFSArrivalByTransportMode);
			AssertEquals("IWT", shipment.DeliveryByTransportMode);
			AssertEquals("IWT", shipment.CFSDepartureByTransportMode);
			AssertEquals("RAI", shipment.PickupByTransportMode);
		}

		public void TestAddressAdditionalInfo_IsDeletedWhenDependentAdressIsCleared()
		{
			//Arrange
			var org = Factory.New<OrgHeader>();
			org.MainAddress.AddAddressType(OrgAddressType.Office);
			org.OH_Code = "ORG1";
			org.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = Guid.NewGuid();
			shipment.ConsigneeDeliveryAddress.OrganisationPK = Guid.NewGuid();
			shipment.JS_OA_ExportReceivingDepot = org.MainAddress.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = Guid.NewGuid();
			shipment.CFSArrivalByTransportMode = "ROA";
			shipment.CFSDepartureByTransportMode = "IWT";
			shipment.DeliveryByTransportMode = "IWT";
			shipment.PickupByTransportMode = "RAI";

			Factory.Save();

			var rowsInDB  = Factory.Load<JobAddressAdditionalInfo>(new ZQuery(JobAddressAdditionalInfoSchema.JAI_ParentID, shipment.PK));

			AssertEquals(4, rowsInDB.Length);

			AssertEquals("ROA", shipment.CFSArrivalByTransportMode);
			AssertEquals("IWT", shipment.DeliveryByTransportMode);
			AssertEquals("IWT", shipment.CFSDepartureByTransportMode);
			AssertEquals("RAI", shipment.PickupByTransportMode);

			//Action
			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = Guid.Empty;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = Guid.Empty;
			shipment.JS_OA_ExportReceivingDepot = Guid.Empty;
			shipment.ConsignorPickupAddress.OrganisationPK = Guid.Empty;

			Factory.Save();

			rowsInDB = Factory.Load<JobAddressAdditionalInfo>(new ZQuery(JobAddressAdditionalInfoSchema.JAI_ParentID, shipment.PK));

			//Assert: If dependant addresses are null then related transport modes should be readonly and empty
			AssertEquals(0, rowsInDB.Length);

			Assert(shipment.CFSArrivalByTransportMode_ReadOnly);
			Assert(shipment.DeliveryByTransportMode_ReadOnly);
			Assert(shipment.CFSDepartureByTransportMode_ReadOnly);
			Assert(shipment.PickupByTransportMode_ReadOnly);

			AssertEquals(string.Empty, shipment.CFSArrivalByTransportMode);
			AssertEquals(string.Empty, shipment.DeliveryByTransportMode);
			AssertEquals(string.Empty, shipment.CFSDepartureByTransportMode);
			AssertEquals(string.Empty, shipment.PickupByTransportMode);
		}

		public void TestAddressAdditionalInfo_HasDefaultValueWhenDependentAdressIsReset()
		{
			//Arrange
			var org1 = Factory.New<OrgHeader>();
			org1.MainAddress.AddAddressType(OrgAddressType.Office);
			org1.OH_Code = "ORG1";
			org1.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var org2 = Factory.New<OrgHeader>();
			org2.MainAddress.AddAddressType(OrgAddressType.Office);
			org2.OH_Code = "ORG2";
			org2.MainAddress.OA_RL_NKRelatedPortCode = "AUML";

			var shipment = Factory.New<CommonShipment>();
			shipment.CFSArrivalByTransportMode = "RAI";
			shipment.CFSDepartureByTransportMode = "RAI";
			shipment.DeliveryByTransportMode = "RAI";
			shipment.PickupByTransportMode = "RAI";

			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = org1.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org1.PK;
			shipment.JS_OA_ExportReceivingDepot = org1.MainAddress.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = org1.PK;

			Factory.Save();

			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = org2.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org2.PK;
			shipment.JS_OA_ExportReceivingDepot = org2.MainAddress.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = org2.PK;

			shipment.CFSArrivalByTransportMode = "IWT";
			shipment.CFSDepartureByTransportMode = "IWT";
			shipment.DeliveryByTransportMode = "IWT";
			shipment.PickupByTransportMode = "IWT";

			Factory.Save();

			//Action
			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = org1.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org1.PK;
			shipment.JS_OA_ExportReceivingDepot = org1.MainAddress.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = org1.PK;

			//Assert: If dependent addresses are changed then related transport modes should be readonly and empty
			AssertEquals(string.Empty, shipment.CFSArrivalByTransportMode);
			AssertEquals(string.Empty, shipment.DeliveryByTransportMode);
			AssertEquals(string.Empty, shipment.CFSDepartureByTransportMode);
			AssertEquals(string.Empty, shipment.PickupByTransportMode);

			//Action
			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = org2.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org2.PK;
			shipment.JS_OA_ExportReceivingDepot = org2.MainAddress.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = org2.PK;

			//Assert: If dependent addresses are changed and then updated to their initial value then related transport modes should be readonly and empty
			AssertEquals(string.Empty, shipment.CFSArrivalByTransportMode);
			AssertEquals(string.Empty, shipment.DeliveryByTransportMode);
			AssertEquals(string.Empty, shipment.CFSDepartureByTransportMode);
			AssertEquals(string.Empty, shipment.PickupByTransportMode);
		}

		#endregion

		#region Advance Cargo Reporting Self-Filer

		public void TestIsAdvanceCargoReportingSelfFiler()
		{
			var shipment = Factory.New<CommonShipmentForTest>();
			Assert(!shipment.IsAdvanceCargoReportingSelfFiler);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			var misc = consignee.MiscServ;
			misc.OM_IMAdvanceCargoReportingSelfFiler = true;
			Assert(shipment.IsAdvanceCargoReportingSelfFiler);

			misc.OM_IMAdvanceCargoReportingSelfFiler = false;
			Assert(!shipment.IsAdvanceCargoReportingSelfFiler);
		}

		#endregion
	}
}
