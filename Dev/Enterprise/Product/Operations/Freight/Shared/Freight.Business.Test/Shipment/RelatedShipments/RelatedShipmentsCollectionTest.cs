using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class RelatedShipmentsCollectionTest<T> : ActiveBusinessObjectCollectionTestCase<RelatedShipmentsCollection<T>>
			where T : CommonShipment
	{
		public void TestRelatedShipmentCollectionCanContainHLShipments()
		{
			var cLD = FreightTestHelper.GetShipment<T>("CLD", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var cLB = FreightTestHelper.GetShipment<T>("CLB", Constants.ShipmentTypes.BlindCoLoadMaster, Factory);
			var hLS = FreightTestHelper.GetShipment<T>("HLS", Constants.ShipmentTypes.HighVolumeLowValueLegacy, Factory);
			var hLV = FreightTestHelper.GetShipment<T>("HLV", Constants.ShipmentTypes.HighVolumeLowValue, Factory);
			var sTD = FreightTestHelper.GetShipment<T>("STD", Constants.ShipmentTypes.StandardHouse, Factory);

			Factory.Save();

			var referenceShipment = FreightTestHelper.GetShipment<T>("STD", Constants.ShipmentTypes.StandardHouse, Factory);
			var relatedShipmentsCollection = GetRelatedShipmentsCollection(referenceShipment, true);

			AssertContainsExactElementsInAnyOrder("shipment is master", new[] { cLD, cLB }, relatedShipmentsCollection);

			referenceShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			relatedShipmentsCollection = GetRelatedShipmentsCollection(referenceShipment, false);

			AssertContainsExactElementsInAnyOrder("shimpent is sub shipment", new[] { sTD, hLV, hLS }, relatedShipmentsCollection);
		}

		public void TestAttachShipmentNotifications()
		{
			var cLD = FreightTestHelper.GetShipment<T>("CLD", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var cLB = FreightTestHelper.GetShipment<T>("CLB", Constants.ShipmentTypes.BlindCoLoadMaster, Factory);
			var bCN = FreightTestHelper.GetShipment<T>("BCN", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			var aSM = FreightTestHelper.GetShipment<T>("ASM", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var sTD = FreightTestHelper.GetShipment<T>("STD", Constants.ShipmentTypes.StandardHouse, Factory);
			var hLV = FreightTestHelper.GetShipment<T>("HLV", Constants.ShipmentTypes.HighVolumeLowValue, Factory);
			var hLS = FreightTestHelper.GetShipment<T>("HLS", Constants.ShipmentTypes.HighVolumeLowValueLegacy, Factory);

			var shipment = FreightTestHelper.GetShipment<T>("shipment", Constants.ShipmentTypes.CoLoadMaster, Factory);
			AssertCollectionHasErrors("CLD can't have sub CLD", shipment, cLD, false, true);
			AssertCollectionHasErrors("CLD can't have sub CLB", shipment, cLB, false, true);
			AssertCollectionHasErrors("CLD can't have sub BCN", shipment, bCN, false, true);
			AssertCollectionHasErrors("CLD can't have sub ASM", shipment, aSM, false, true);
			AssertCollectionHasErrors("CLD can have sub STD", shipment, sTD, false, false);
			AssertCollectionHasErrors("CLD can have sub HLV", shipment, hLV, false, false);
			AssertCollectionHasErrors("CLD can have sub HLS", shipment, hLS, false, false);

			AssertCollectionHasErrors("CLD can't have master CLD", shipment, cLD, true, true);
			AssertCollectionHasErrors("CLD can't have master CLB", shipment, cLB, true, true);
			AssertCollectionHasErrors("CLD can have master BCN", shipment, bCN, true, false);
			AssertCollectionHasErrors("CLD can have master ASM", shipment, aSM, true, false);
			AssertCollectionHasErrors("CLD can't have master STD", shipment, sTD, true, true);
			AssertCollectionHasErrors("CLD can't have master HLV", shipment, hLV, true, true);
			AssertCollectionHasErrors("CLD can't have master HLS", shipment, hLS, true, true);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertCollectionHasErrors("CLB can't have sub CLD", shipment, cLD, false, true);
			AssertCollectionHasErrors("CLB can't have sub CLB", shipment, cLB, false, true);
			AssertCollectionHasErrors("CLB can't have sub BCN", shipment, bCN, false, true);
			AssertCollectionHasErrors("CLB can't have sub ASM", shipment, aSM, false, true);
			AssertCollectionHasErrors("CLB can have sub STD", shipment, sTD, false, false);
			AssertCollectionHasErrors("CLB can have sub HLV", shipment, hLV, false, false);
			AssertCollectionHasErrors("CLB can have sub HLS", shipment, hLS, false, false);

			AssertCollectionHasErrors("CLB can't have master CLD", shipment, cLD, true, true);
			AssertCollectionHasErrors("CLB can't have master CLB", shipment, cLB, true, true);
			AssertCollectionHasErrors("CLB can have master BCN", shipment, bCN, true, false);
			AssertCollectionHasErrors("CLB can have master ASM", shipment, aSM, true, false);
			AssertCollectionHasErrors("CLB can't have master STD", shipment, sTD, true, true);
			AssertCollectionHasErrors("CLB can't have master HLV", shipment, hLV, true, true);
			AssertCollectionHasErrors("CLB can't have master HLS", shipment, hLS, true, true);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertCollectionHasErrors("BCN can have sub CLD", shipment, cLD, false, false);
			AssertCollectionHasErrors("BCN can have sub CLB", shipment, cLB, false, false);
			AssertCollectionHasErrors("BCN can't have sub BCN", shipment, bCN, false, true);
			AssertCollectionHasErrors("BCN can have sub ASM", shipment, aSM, false, false);
			AssertCollectionHasErrors("BCN can have sub STD", shipment, sTD, false, false);
			AssertCollectionHasErrors("BCN can have sub HLV", shipment, hLV, false, false);
			AssertCollectionHasErrors("BCN can have sub HLS", shipment, hLS, false, false);

			AssertCollectionHasErrors("BCN can't have master CLD", shipment, cLD, true, true);
			AssertCollectionHasErrors("BCN can't have master CLB", shipment, cLB, true, true);
			AssertCollectionHasErrors("BCN can't have master BCN", shipment, bCN, true, true);
			AssertCollectionHasErrors("BCN can't have master ASM", shipment, aSM, true, true);
			AssertCollectionHasErrors("BCN can't have master STD", shipment, sTD, true, true);
			AssertCollectionHasErrors("BCN can't have master HLV", shipment, hLV, true, true);
			AssertCollectionHasErrors("BCN can't have master HLS", shipment, hLS, true, true);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertCollectionHasErrors("ASM can have sub CLD", shipment, cLD, false, false);
			AssertCollectionHasErrors("ASM can have sub CLB", shipment, cLB, false, false);
			AssertCollectionHasErrors("ASM can't have sub BCN", shipment, bCN, false, true);
			AssertCollectionHasErrors("ASM can have sub ASM", shipment, aSM, false, false);
			AssertCollectionHasErrors("ASM can have sub STD", shipment, sTD, false, false);
			AssertCollectionHasErrors("ASM can have sub HLV", shipment, hLV, false, false);
			AssertCollectionHasErrors("ASM can have sub HLS", shipment, hLS, false, false);

			AssertCollectionHasErrors("ASM can't have master CLD", shipment, cLD, true, true);
			AssertCollectionHasErrors("ASM can't have master CLB", shipment, cLB, true, true);
			AssertCollectionHasErrors("ASM can have master BCN", shipment, bCN, true, false);
			AssertCollectionHasErrors("ASM can have master ASM", shipment, aSM, true, false);
			AssertCollectionHasErrors("ASM can't have master STD", shipment, sTD, true, true);
			AssertCollectionHasErrors("ASM can't have master HLV", shipment, hLV, true, true);
			AssertCollectionHasErrors("ASM can't have master HLS", shipment, hLS, true, true);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertCollectionHasErrors("STD can't have sub CLD", shipment, cLD, false, true);
			AssertCollectionHasErrors("STD can't have sub CLB", shipment, cLB, false, true);
			AssertCollectionHasErrors("STD can't have sub BCN", shipment, bCN, false, true);
			AssertCollectionHasErrors("STD can't have sub ASM", shipment, aSM, false, true);
			AssertCollectionHasErrors("STD can't have sub STD", shipment, sTD, false, true);
			AssertCollectionHasErrors("STD can't have sub HLV", shipment, hLV, false, true);
			AssertCollectionHasErrors("STD can't have sub HLS", shipment, hLS, false, true);

			AssertCollectionHasErrors("STD can have master CLD", shipment, cLD, true, false);
			AssertCollectionHasErrors("STD can have master CLB", shipment, cLB, true, false);
			AssertCollectionHasErrors("STD can have master BCN", shipment, bCN, true, false);
			AssertCollectionHasErrors("STD can have master ASM", shipment, aSM, true, false);
			AssertCollectionHasErrors("STD can't have master STD", shipment, sTD, true, true);
			AssertCollectionHasErrors("STD can't have master HLV", shipment, hLV, true, true);
			AssertCollectionHasErrors("STD can't have master HLS", shipment, hLS, true, true);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			AssertCollectionHasErrors("HLS can't have sub CLD", shipment, cLD, false, true);
			AssertCollectionHasErrors("HLS can't have sub CLB", shipment, cLB, false, true);
			AssertCollectionHasErrors("HLS can't have sub BCN", shipment, bCN, false, true);
			AssertCollectionHasErrors("HLS can't have sub ASM", shipment, aSM, false, true);
			AssertCollectionHasErrors("HLS can't have sub STD", shipment, sTD, false, true);
			AssertCollectionHasErrors("HLS can't have sub HLV", shipment, hLV, false, true);
			AssertCollectionHasErrors("HLS can't have sub HLS", shipment, hLS, false, true);

			AssertCollectionHasErrors("HLS can have master CLD", shipment, cLD, true, false);
			AssertCollectionHasErrors("HLS can have master CLB", shipment, cLB, true, false);
			AssertCollectionHasErrors("HLS can have master BCN", shipment, bCN, true, false);
			AssertCollectionHasErrors("HLS can have master ASM", shipment, aSM, true, false);
			AssertCollectionHasErrors("HLS can't have master STD", shipment, sTD, true, true);
			AssertCollectionHasErrors("HLS can't have master HLV", shipment, hLV, true, true);
			AssertCollectionHasErrors("HLS can't have master HLS", shipment, hLS, true, true);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertCollectionHasErrors("HLV can't have sub CLD", shipment, cLD, false, true);
			AssertCollectionHasErrors("HLV can't have sub CLB", shipment, cLB, false, true);
			AssertCollectionHasErrors("HLV can't have sub BCN", shipment, bCN, false, true);
			AssertCollectionHasErrors("HLV can't have sub ASM", shipment, aSM, false, true);
			AssertCollectionHasErrors("HLV can't have sub STD", shipment, sTD, false, true);
			AssertCollectionHasErrors("HLV can't have sub HLV", shipment, hLV, false, true);
			AssertCollectionHasErrors("HLV can't have sub HLS", shipment, hLS, false, true);

			AssertCollectionHasErrors("HLV can have master CLD", shipment, cLD, true, false);
			AssertCollectionHasErrors("HLV can have master CLB", shipment, cLB, true, false);
			AssertCollectionHasErrors("HLV can have master BCN", shipment, bCN, true, false);
			AssertCollectionHasErrors("HLV can have master ASM", shipment, aSM, true, false);
			AssertCollectionHasErrors("HLV can't have master STD", shipment, sTD, true, true);
			AssertCollectionHasErrors("HLV can't have master HLV", shipment, hLV, true, true);
			AssertCollectionHasErrors("HLV can't have master HLS", shipment, hLS, true, true);
		}

		public void TestPassengerFlightNotifications()
		{
			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
			((ShipmentInspectionType)inspectionTypes.Types.FindByCode("NUC")).AllowedOnPassengerFlights = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "MYKUL";

				var flight1 = consol.Transports[0];
				flight1.JW_IsCargoOnly = false;

				var masterShipment = consol.Shipments.AddNew();
				masterShipment.JS_ShipmentType = "CLM";
				masterShipment.JS_InspectionTypeCode = "APP";

				var subShipment = Factory.New<T>();
				subShipment.JS_ShipmentType = "STD";
				subShipment.JS_TransportMode = "AIR";
				subShipment.JS_RL_NKOrigin = "GBLHR";
				subShipment.JS_RL_NKDestination = "MYKUL";
				subShipment.JS_InspectionTypeCode = "NUC";

				var expectedError = "For a voyage that is not Cargo Only, all Shipments must be Aviation Security Approved or Exempt";
				var notificationProvider = masterShipment.Lookups.CoLoadShipment_List as IFilterModuleExtraNotificationProvider;

				Assert("Can't add sub-shipment with Inspection Type NUC to a passenger flight", notificationProvider.GetExtraNotification(subShipment).Message.Contains(expectedError));

				subShipment.JS_InspectionTypeCode = "PHS";
				AssertNull("Can add sub-shipment with Inspection Type PHS to a passenger flight", notificationProvider.GetExtraNotification(subShipment));

				subShipment.JS_InspectionTypeCode = "APP";
				AssertNull("Can add sub-shipment with Inspection Type APP to a passenger flight", notificationProvider.GetExtraNotification(subShipment));

				subShipment.JS_InspectionTypeCode = "UNK";
				AssertNull("Can add sub-shipment with Inspection Type UNK to a passenger flight. (UNK means 'not inspected yet')", notificationProvider.GetExtraNotification(subShipment));

				flight1.JW_IsCargoOnly = true;
				subShipment.JS_InspectionTypeCode = "NUC";
				AssertNull("OK to add sub-shipment with Inspection Type NUC to a cargo only flight", notificationProvider.GetExtraNotification(subShipment));
			}
		}

		void AssertCollectionHasErrors(ZString message, T parentShipment, T selectedShipment, bool isMasterCollection, bool expectedErrors)
		{
			var errors = new StringCollectionX();

			var collection = new RelatedShipmentsCollectionForTest(parentShipment, isMasterCollection);
			collection.AddNotificationForTest(errors, selectedShipment);

			if (expectedErrors)
			{
				Assert(message, errors.Count > 0);
			}
			else
			{
				Assert(message, errors.Count == 0);
			}
		}

		protected override RelatedShipmentsCollection<T> GetCollectionToTest()
		{
			return new RelatedShipmentsCollectionForTest(Factory);
		}

		protected abstract RelatedShipmentsCollection<T> GetRelatedShipmentsCollection();
		protected abstract RelatedShipmentsCollection<T> GetRelatedShipmentsCollection(T referenceShipment, bool isMasterShipmentCollection);

		#region Helper class

		class RelatedShipmentsCollectionForTest : RelatedShipmentsCollection<T>
		{
			public RelatedShipmentsCollectionForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RelatedShipmentsCollectionForTest(T referenceShipment, bool isMasterShipmentCollection)
				: base(referenceShipment, isMasterShipmentCollection)
			{
			}

			public void AddNotificationForTest(StringCollectionX errors, BusinessObject selectedBusinessObject)
			{
				base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			}
		}

		#endregion
	}
}
