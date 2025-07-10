using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonShipment))]
	public class ShipmentBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonShipment);
			}
		}

		#endregion

		#region Master Shipments - Assembly, Co-Load, Blind Co-Load, Buyers Consoladations

		#region ColoadMaster Tests

		public void TestIsCoLoadMaster()
		{
			CommonShipment ship = Factory.New<CommonShipment>();
			AssertEquals(false, ship.IsCoLoadMaster);
			ship.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(true, ship.IsCoLoadMaster);
		}

		public void TestSettingCoLoadMasterFiresEvent()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			bool eventWasFired = false;
			shipment.ShipmentTypeChanging += (s, e) => eventWasFired = true;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("event not fired because the shipment already fulfils all criteria of an CLD shipment", false, eventWasFired);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.OuterPackLines.AddNew();

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("event fired because the shipment CLD shipment does not allow packlines", true, eventWasFired);
		}

		public void TestUncheckingMasterFiresEvent()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.CoLoadShipments.AddNew();

			bool eventWasFired = false;
			shipment.ShipmentTypeChanging += (s, e) => eventWasFired = true;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("event fired because the STD shipment does not allow coloads", true, eventWasFired);
		}

		public void TestUncheckingLeadFiresEvent()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			shipment.CoLoadShipments.AddNew();

			bool eventWasFired = false;
			shipment.ShipmentTypeChanging += (s, e) => eventWasFired = true;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("event fired because the STD shipment does not allow coloads", true, eventWasFired);
		}

		public void TestSettingCoLoadMasterUsesEventCancel()
		{
			var shup = Factory.New<CommonShipment>();
			shup.OuterPackLines.AddNew();

			bool handlerCancelsEvent = true;

			shup.ShipmentTypeChanging += (sender, e) => e.Cancel = handlerCancelsEvent;

			shup.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(false, shup.IsCoLoadMaster);

			handlerCancelsEvent = false;
			shup.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(true, shup.IsCoLoadMaster);
		}

		public void TestSettingColoadDoesntResetValues()
		{
			CommonShipment ship = Factory.New<CommonShipment>();
			ship.JS_RX_NKInsuranceCurrency = ship.JS_RX_NKGoodsValueCurr;
			ship.JS_InsuranceValue = 1000;
			ship.JS_GoodsValue = 2000;
			ship.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(1000m, ship.JS_InsuranceValue);
			AssertEquals(2000m, ship.JS_GoodsValue);

			CommonShipment newShipment = Factory.New<CommonShipment>();
			newShipment.JS_RX_NKInsuranceCurrency = ship.JS_RX_NKGoodsValueCurr;

			ship.CoLoadShipments.Add(newShipment);
			AssertEquals(1000m, ship.JS_InsuranceValue);
			AssertEquals(2000m, ship.JS_GoodsValue);
		}

		public void TestUn_SettingCoLoadMasterUsesEventCancel()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.OuterPackLines.AddNew();

			bool handlerCancelsEvent = true;

			shipment.ShipmentTypeChanging += (sender, e) => e.Cancel = handlerCancelsEvent;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(false, shipment.IsCoLoadMaster);

			shipment.InnerPackLines.AddNew();
			shipment.InnerPackLines.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.CoLoadShipments.AddNew();
			shipment.CoLoadShipments.AddNew();

			AssertEquals(2, shipment.OuterPackLines.Count);
			AssertEquals(2, shipment.InnerPackLines.Count);

			handlerCancelsEvent = false;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(true, shipment.IsCoLoadMaster);
			AssertEquals(0, shipment.OuterPackLines.Count);
			AssertEquals(0, shipment.InnerPackLines.Count);
			AssertEquals(2, shipment.CoLoadShipments.Count);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(false, shipment.IsCoLoadMaster);
			AssertEquals(0, shipment.OuterPackLines.Count);
			AssertEquals(0, shipment.InnerPackLines.Count);
			AssertEquals(0, shipment.CoLoadShipments.Count);
		}

		public void TestUn_SettingLeadUsesEventCancel()
		{
			CommonShipment ship = Factory.New<CommonShipment>();
			ship.InnerPackLines.AddNew();
			ship.InnerPackLines.AddNew();
			ship.OuterPackLines.AddNew();
			ship.OuterPackLines.AddNew();
			ship.CoLoadShipments.AddNew();
			ship.CoLoadShipments.AddNew();

			ship.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			bool handlerCancelsEvent = true;

			ship.ShipmentTypeChanging += (sender, e) => e.Cancel = handlerCancelsEvent;

			ship.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(true, ship.IsBuyersConsolLead);
			AssertEquals(2, ship.OuterPackLines.Count);
			AssertEquals(2, ship.InnerPackLines.Count);

			handlerCancelsEvent = false;
			ship.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(false, ship.IsBuyersConsolLead);
			AssertEquals(2, ship.OuterPackLines.Count);
			AssertEquals(2, ship.InnerPackLines.Count);
			AssertEquals(0, ship.CoLoadShipments.Count);
		}

		public void TestResetValueSFromSubs()
		{
			CommonShipment leadShip = Factory.New<CommonShipment>();
			leadShip.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CommonShipment sub1 = leadShip.CoLoadShipments.AddNew();
			CommonShipment sub2 = leadShip.CoLoadShipments.AddNew();

			leadShip.JS_UnitOfWeight = Constants.Weight.Decitons;
			leadShip.JS_ActualWeight = 1m;
			leadShip.JS_UnitOfVolume = Constants.Volume.CubicDecimetres;
			leadShip.JS_ActualVolume = 1m;
			leadShip.JS_OuterPacks = 1;
			leadShip.JS_TotalPackageCount = 1;
			leadShip.JS_GoodsValue = 1;
			leadShip.JS_InsuranceValue = 1;

			sub1.JS_UnitOfWeight = Constants.Weight.Kilograms;
			sub1.JS_ActualWeight = 2m;
			sub1.JS_UnitOfVolume = Constants.Volume.Litre;
			sub1.JS_ActualVolume = 2m;
			sub1.JS_LoadingMeters = 2.2m;
			sub1.JS_OuterPacks = 2;
			sub1.JS_F3_NKPackType = Constants.PkgUnit.Bottle;
			sub1.JS_TotalPackageCount = 2;
			sub1.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Carton;
			sub1.JS_GoodsValue = 2;
			sub1.JS_InsuranceValue = 2;

			sub2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			sub2.JS_ActualWeight = 3m;
			sub2.JS_UnitOfVolume = Constants.Volume.Litre;
			sub2.JS_ActualVolume = 3m;
			sub2.JS_LoadingMeters = 3.3m;
			sub2.JS_OuterPacks = 3;
			sub2.JS_F3_NKPackType = Constants.PkgUnit.Bag;
			sub2.JS_TotalPackageCount = 3;
			sub1.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Box;
			sub2.JS_GoodsValue = 3;
			sub2.JS_InsuranceValue = 3;

			leadShip.JS_RX_NKInsuranceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			sub1.JS_RX_NKInsuranceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			sub2.JS_RX_NKInsuranceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			leadShip.ResetAllValuesFromSubs();

			AssertEquals(Constants.Weight.Kilograms, leadShip.JS_UnitOfWeight);
			AssertEquals(5m, leadShip.JS_ActualWeight);
			AssertEquals(Constants.Volume.Litre, leadShip.JS_UnitOfVolume);
			AssertEquals(5m, leadShip.JS_ActualVolume);
			AssertEquals(5.5m, leadShip.JS_LoadingMeters);
			AssertEquals(5, leadShip.JS_OuterPacks);
			AssertEquals(Constants.PkgUnit.Package, leadShip.JS_F3_NKPackType);
			AssertEquals(5, leadShip.JS_TotalPackageCount);
			AssertEquals(Constants.PkgUnit.Package, leadShip.JS_F3_NKTotalCountPackType);
			AssertEquals(5m, leadShip.JS_GoodsValue);
			AssertEquals(5m, leadShip.JS_InsuranceValue);
		}

		public void TestCoLoadMasterReadOnlyFields()
		{
			var ship = Factory.New<CommonShipment>();
			AssertReadonlyProperties(ship, false);

			ship.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertReadonlyProperties(ship, false);

			ship.CoLoadShipments.AddNew();
			AssertReadonlyProperties(ship, false);
		}

		public void TestBCNMasterBehaviour()
		{
			CommonShipment ship = Factory.New<CommonShipment>();
			AssertReadonlyProperties(ship, false);

			ship.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertReadonlyProperties(ship, false);

			CommonShipment childShipment = ship.CoLoadShipments.AddNew();
			childShipment.JS_ActualWeight = 333m;
			AssertReadonlyProperties(ship, false);
			AssertEquals(0m, ship.JS_ActualWeight);
		}

		public void TestAssemblyMasterBehaviour()
		{
			var ship = Factory.New<CommonShipment>();
			ship = Factory.New<CommonShipment>();
			AssertReadonlyProperties(ship, false);

			ship.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertReadonlyProperties(ship, false);

			var childShipment = ship.CoLoadShipments.AddNew();
			childShipment.JS_ActualWeight = 333m;
			AssertReadonlyProperties(ship, false);
			AssertEquals(333m, ship.JS_ActualWeight);
		}

		void AssertReadonlyProperties(CommonShipment ship, bool expectedReadOnly)
		{
			AssertEquals(expectedReadOnly, ship.InnerPackLines.ReadOnly);
			AssertEquals(expectedReadOnly, ship.OuterPackLines.ReadOnly);
		}

		public void TestLeadShipmentInnerOuterPacksRelationship()
		{
			CommonShipment leadShipment = Factory.New<CommonShipment>();

			leadShipment.InnerPackLines.AddNew();
			leadShipment.InnerPackLines.AddNew();
			leadShipment.InnerPackLines.AddNew();

			leadShipment.OuterPackLines.AddNew();
			leadShipment.OuterPackLines.AddNew();
			leadShipment.OuterPackLines.AddNew();

			AssertEquals(3, leadShipment.InnerPackLines.Count);
			AssertEquals(3, leadShipment.OuterPackLines.Count);

			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			AssertEquals(3, leadShipment.InnerPackLines.Count);
			AssertEquals(3, leadShipment.OuterPackLines.Count);

			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			AssertEquals(0, leadShipment.InnerPackLines.Count);
			AssertEquals(0, leadShipment.OuterPackLines.Count);
		}

		public void TestCoLoadMasterInnerOuterPacksRelationship()
		{
			var ship = Factory.New<CommonShipment>();

			ship.InnerPackLines.AddNew();
			ship.InnerPackLines.AddNew();
			ship.InnerPackLines.AddNew();

			ship.OuterPackLines.AddNew();
			ship.OuterPackLines.AddNew();
			ship.OuterPackLines.AddNew();

			AssertEquals(false, ship.InnerPackLines.ReadOnly);
			AssertEquals(false, ship.OuterPackLines.ReadOnly);
			AssertEquals(true, ship.CoLoadShipments.ReadOnly);

			ship.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertEquals(0, ship.InnerPackLines.Count);
			AssertEquals(0, ship.OuterPackLines.Count);

			AssertEquals(false, ship.InnerPackLines.ReadOnly);
			AssertEquals(false, ship.OuterPackLines.ReadOnly);

			ship.CoLoadShipments.AddNew();
			AssertEquals(false, ship.InnerPackLines.ReadOnly);
			AssertEquals(false, ship.OuterPackLines.ReadOnly);
			AssertEquals(false, ship.CoLoadShipments.ReadOnly);
		}

		public void TestUnmasteringShipmentRemovesChildrenPacklines()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.InnerPackLines.AddNew();
			shipment.InnerPackLines.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.CoLoadShipments.Add(shipment);
			AssertEquals(2, masterShipment.InnerPackLines.Count);
			AssertEquals(2, masterShipment.OuterPackLines.Count);
			AssertEquals(2, shipment.InnerPackLines.Count);
			AssertEquals(2, shipment.OuterPackLines.Count);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(0, masterShipment.InnerPackLines.Count);
			AssertEquals(0, masterShipment.OuterPackLines.Count);
			AssertEquals(2, shipment.InnerPackLines.Count);
			AssertEquals(2, shipment.OuterPackLines.Count);
		}

		[ExpectNoExceptions]
		public void TestUpdatedByDataRefreshIncludingChildren_NoExceptionWhenDeleted()
		{
			CommonShipmentForTest masterShipment1 = Factory.New<CommonShipmentForTest>();
			CommonShipmentForTest subShipment1 = Factory.New<CommonShipmentForTest>();

			masterShipment1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			subShipment1.JS_JS_ColoadMasterShipment = masterShipment1.PK;

			var shipmentType = typeof(CommonShipment);
			var eventMethod = shipmentType.GetMethod("Shipment_UpdatedByDataRefreshIncludingChildren", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

			subShipment1.Delete();
			eventMethod.Invoke(subShipment1, new object[] { subShipment1, null });
		}

		#endregion

		#region Blind Co-load Master Tests

		public void TestIsBlindCoLoadMaster()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals(false, shipment.IsCoLoadMaster);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals(true, shipment.IsBlindCoLoadMaster);
			AssertEquals(true, shipment.IsLeadOrMaster);
		}

		#region Shipment Type Changing Event

		public void TestSettingBlindCoLoadMasterFiresEvent()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			bool eventWasFired = false;
			shipment.ShipmentTypeChanging += (s, e) => eventWasFired = true;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals("event not fired because the shipment already fulfils all criteria of an CLB shipment", false, eventWasFired);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.OuterPackLines.AddNew();

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals("event fired because the shipment CLB shipment does not allow packlines", true, eventWasFired);
		}

		public void TestTypeChangeFromBlindCoLoadMasterFiresEvent()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			shipment.CoLoadShipments.AddNew();

			bool eventWasFired = false;
			shipment.ShipmentTypeChanging += (s, e) => eventWasFired = true;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("event fired because the STD shipment does not allow coloads", true, eventWasFired);
		}

		public void TestSettingBlindCoLoadMasterUsesEventCancel()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();

			bool handlerCancelsEvent = true;

			shipment.ShipmentTypeChanging += (sender, e) => e.Cancel = handlerCancelsEvent;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals(false, shipment.IsBlindCoLoadMaster);

			handlerCancelsEvent = false;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals(true, shipment.IsBlindCoLoadMaster);
		}

		public void TestUnSettingBlindCoLoadMasterUsesEventCancel()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.OuterPackLines.AddNew();

			bool handlerCancelsEvent = true;

			shipment.ShipmentTypeChanging += (sender, e) => e.Cancel = handlerCancelsEvent;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			AssertEquals(false, shipment.IsBlindCoLoadMaster);

			shipment.InnerPackLines.AddNew();
			shipment.InnerPackLines.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.CoLoadShipments.AddNew();
			shipment.CoLoadShipments.AddNew();

			AssertEquals(2, shipment.OuterPackLines.Count);
			AssertEquals(2, shipment.InnerPackLines.Count);

			handlerCancelsEvent = false;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			AssertEquals(true, shipment.IsBlindCoLoadMaster);
			AssertEquals(0, shipment.OuterPackLines.Count);
			AssertEquals(0, shipment.InnerPackLines.Count);
			AssertEquals(2, shipment.CoLoadShipments.Count);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			AssertEquals(false, shipment.IsBlindCoLoadMaster);
			AssertEquals(0, shipment.OuterPackLines.Count);
			AssertEquals(0, shipment.InnerPackLines.Count);
			AssertEquals(0, shipment.CoLoadShipments.Count);
		}

		#endregion

		#region Resetting Blind Co-Load Master Values

		public void TestUnmasteringBlindCoLoadMasterShipment_DoesNotResetBasicRegistrationFields()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_RX_NKInsuranceCurrency = masterShipment.JS_RX_NKGoodsValueCurr;
			masterShipment.JS_InsuranceValue = 1000;
			masterShipment.JS_GoodsValue = 2000;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals(1000m, masterShipment.JS_InsuranceValue);
			AssertEquals(2000m, masterShipment.JS_GoodsValue);

			var subShipment = Factory.New<CommonShipment>();
			subShipment.JS_RX_NKInsuranceCurrency = masterShipment.JS_RX_NKGoodsValueCurr;
			masterShipment.CoLoadShipments.Add(subShipment);

			AssertEquals(1000m, masterShipment.JS_InsuranceValue);
			AssertEquals(2000m, masterShipment.JS_GoodsValue);
		}

		public void TestUnmasteringBlindCoLoadMasterShipment_RemovesChildrenPacklines()
		{
			var masterShipment = Factory.New<CommonShipment>();
			var subShipment = Factory.New<CommonShipment>();
			subShipment.InnerPackLines.AddNew();
			subShipment.InnerPackLines.AddNew();
			subShipment.OuterPackLines.AddNew();
			subShipment.OuterPackLines.AddNew();

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			masterShipment.CoLoadShipments.Add(subShipment);
			AssertEquals(2, masterShipment.InnerPackLines.Count);
			AssertEquals(2, masterShipment.OuterPackLines.Count);
			AssertEquals(2, subShipment.InnerPackLines.Count);
			AssertEquals(2, subShipment.OuterPackLines.Count);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(0, masterShipment.InnerPackLines.Count);
			AssertEquals(0, masterShipment.OuterPackLines.Count);
			AssertEquals(2, subShipment.InnerPackLines.Count);
			AssertEquals(2, subShipment.OuterPackLines.Count);
		}

		public void TestResetValuesFromSubs_BlindCoLoadMaster()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			masterShipment.JS_UnitOfWeight = Constants.Weight.Decitons;
			masterShipment.JS_ActualWeight = 1m;
			masterShipment.JS_UnitOfVolume = Constants.Volume.CubicDecimetres;
			masterShipment.JS_ActualVolume = 1m;
			masterShipment.JS_OuterPacks = 1;
			masterShipment.JS_TotalPackageCount = 1;
			masterShipment.JS_GoodsValue = 1;
			masterShipment.JS_InsuranceValue = 1;

			var subShipment1 = masterShipment.CoLoadShipments.AddNew();
			subShipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
			subShipment1.JS_ActualWeight = 2m;
			subShipment1.JS_UnitOfVolume = Constants.Volume.Litre;
			subShipment1.JS_ActualVolume = 2m;
			subShipment1.JS_LoadingMeters = 2.2m;
			subShipment1.JS_OuterPacks = 2;
			subShipment1.JS_F3_NKPackType = Constants.PkgUnit.Bottle;
			subShipment1.JS_TotalPackageCount = 2;
			subShipment1.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Carton;
			subShipment1.JS_GoodsValue = 2;
			subShipment1.JS_InsuranceValue = 2;

			var subShipment2 = masterShipment.CoLoadShipments.AddNew();
			subShipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			subShipment2.JS_ActualWeight = 3m;
			subShipment2.JS_UnitOfVolume = Constants.Volume.Litre;
			subShipment2.JS_ActualVolume = 3m;
			subShipment2.JS_LoadingMeters = 3.3m;
			subShipment2.JS_OuterPacks = 3;
			subShipment1.JS_F3_NKPackType = Constants.PkgUnit.Bag;
			subShipment2.JS_TotalPackageCount = 3;
			subShipment1.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Box;
			subShipment2.JS_GoodsValue = 3;
			subShipment2.JS_InsuranceValue = 3;

			masterShipment.JS_RX_NKInsuranceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			subShipment1.JS_RX_NKInsuranceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			subShipment2.JS_RX_NKInsuranceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			masterShipment.ResetAllValuesFromSubs();

			AssertEquals(Constants.Weight.Kilograms, masterShipment.JS_UnitOfWeight);
			AssertEquals(5m, masterShipment.JS_ActualWeight);
			AssertEquals(Constants.Volume.Litre, masterShipment.JS_UnitOfVolume);
			AssertEquals(5m, masterShipment.JS_ActualVolume);
			AssertEquals(5.5m, masterShipment.JS_LoadingMeters);
			AssertEquals(5, masterShipment.JS_OuterPacks);
			AssertEquals(Constants.PkgUnit.Package, masterShipment.JS_F3_NKPackType);
			AssertEquals(5, masterShipment.JS_TotalPackageCount);
			AssertEquals(Constants.PkgUnit.Package, masterShipment.JS_F3_NKTotalCountPackType);
			AssertEquals(5m, masterShipment.JS_GoodsValue);
			AssertEquals(5m, masterShipment.JS_InsuranceValue);
		}

		#endregion

		#region Blind Co-Load Master Read Only Fields

		public void TestBlindCoLoadMasterReadOnlyFields()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertReadonlyProperties(shipment, false);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertReadonlyProperties(shipment, false);

			shipment.CoLoadShipments.AddNew();
			AssertReadonlyProperties(shipment, false);
		}

		public void TestCoLoadMasterInnerOuterPacksRelationship_BlindCoLoad()
		{
			var shipment = Factory.New<CommonShipment>();

			shipment.InnerPackLines.AddNew();
			shipment.InnerPackLines.AddNew();
			shipment.InnerPackLines.AddNew();

			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();

			AssertEquals(3, shipment.InnerPackLines.Count);
			AssertEquals(3, shipment.OuterPackLines.Count);

			AssertEquals(false, shipment.InnerPackLines.ReadOnly);
			AssertEquals(false, shipment.OuterPackLines.ReadOnly);
			AssertEquals(true, shipment.CoLoadShipments.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			AssertEquals(0, shipment.InnerPackLines.Count);
			AssertEquals(0, shipment.OuterPackLines.Count);

			AssertEquals(false, shipment.InnerPackLines.ReadOnly);
			AssertEquals(false, shipment.OuterPackLines.ReadOnly);
			AssertEquals(false, shipment.CoLoadShipments.ReadOnly);

			shipment.CoLoadShipments.AddNew();
			AssertEquals(false, shipment.InnerPackLines.ReadOnly);
			AssertEquals(false, shipment.OuterPackLines.ReadOnly);
		}

		#endregion

		#endregion

		#endregion

		#region Test Calculated Properties

		public void TestTotalPillagedAndDamaged()
		{
			PackLine outerPack1 = Shipment.OuterPackLines.AddNew();
			outerPack1.JL_PackageCount = 10;
			outerPack1.JL_Outturn = 7;
			outerPack1.JL_Pillaged = 1;
			outerPack1.JL_Damaged = 2;

			PackLine outerPack2 = Shipment.OuterPackLines.AddNew();
			outerPack2.JL_PackageCount = 20;
			outerPack2.JL_Outturn = 13;
			outerPack2.JL_Pillaged = 3;
			outerPack2.JL_Damaged = 4;

			AssertEquals("Total Outer Packs Pillaged.", 4, Shipment.TotalOuterPacksPillaged);
			AssertEquals("Total Outer Packs Damaged.", 6, Shipment.TotalOuterPacksDamaged);
		}

		public void TestDocumentedWeightConvertedToStandardUnit()
		{
			Shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			Shipment.JS_DocumentedWeight = 100;
			AssertEquals("Weight Unit is KG. No conversion.", 100m, Shipment.JS_Calc_DocumentedWeight_Converted);

			Shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			AssertEquals("Weight Unit is LB. Should convert.", 45.359m, Math.Round(Shipment.JS_Calc_DocumentedWeight_Converted, 3));
		}

		public void TestDocumentedVolumeConvertedToStandardUnit()
		{
			Shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			Shipment.JS_DocumentedVolume = 10;
			AssertEquals("Volume Unit is M3. No conversion.", 10m, Shipment.JS_Calc_DocumentedVolume_Converted);

			Shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;
			AssertEquals("Volume Unit is CF. Should convert.", 0.283m, Math.Round(Shipment.JS_Calc_DocumentedVolume_Converted, 3));
		}

		#endregion

		#region Test Findbox Lists

		public void TestCoLoadMasterFindboxList()
		{
			var aSM = FreightTestHelper.GetShipment<CommonShipment>("ASM", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var cLD = FreightTestHelper.GetShipment<CommonShipment>("CLD", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var bCN = FreightTestHelper.GetShipment<CommonShipment>("BCN", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			var sTD = FreightTestHelper.GetShipment<CommonShipment>("STD", Constants.ShipmentTypes.StandardHouse, Factory);

			var shipment = FreightTestHelper.GetShipment<CommonShipment>("shipment", Constants.ShipmentTypes.AssemblyMaster, Factory);
			FreightTestHelper.AssertShipmentCollection(shipment.Lookups.CoLoadMaster_List.Cast<BusinessObject>(), aSM, bCN);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			FreightTestHelper.AssertShipmentCollection(shipment.Lookups.CoLoadMaster_List.Cast<BusinessObject>(), aSM, bCN);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			FreightTestHelper.AssertShipmentCollection(shipment.Lookups.CoLoadMaster_List.Cast<BusinessObject>());

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			FreightTestHelper.AssertShipmentCollection(shipment.Lookups.CoLoadMaster_List.Cast<BusinessObject>(), aSM, cLD, bCN);
		}

		#endregion

		public override void TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed()
		{
			// TODO: This should be undo when when Enterprise.Integration.Customs.AU.ICustomsManifestLineSequence is changed to make JobConsol it's parent; there is a requirement that a CustomsManifestLineSequence record still exists after CommonShipment is deleted and this record is can be linked back JobConsol
			Assert(true);
		}

		public void TestGetDeclarationFor()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "!1#";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "#1@";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "!2#";
			company2.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "#2@";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var declaration1 = Factory.New<IBaseJobDeclaration>();
			declaration1.JE_GB = branch1.PK;
			declaration1.JE_JS = Shipment.PK;
			declaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			var declaration2 = Factory.New<IBaseJobDeclaration>();
			declaration2.JE_GB = branch2.PK;
			declaration2.JE_JS = Shipment.PK;
			declaration2.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<CommonShipment>(Shipment.PK);
			AssertNull("No dec with same company for this shipment", shipmentInNewFactory.GetDeclarationFor(GlbCompany.CurrentCompany.PK));
			var declaration = shipmentInNewFactory.GetDeclarationFor(company2.PK);
			AssertEquals("Should have matched to declaration2 as it was created earlier", declaration2.PK, declaration.PK);

			var declaration3 = Factory.New<IBaseJobDeclaration>();
			declaration3.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration3.JE_JS = Shipment.PK;
			declaration3.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);
			Factory.Save();
			declaration = shipmentInNewFactory.GetDeclarationFor(GlbCompany.CurrentCompany.PK);
			AssertEquals("Should have matched to declaration3 as it was created earlier", declaration3.PK, declaration.PK);
		}

		public void TestDonotAllowToAttachConsolToShipment_WithSameLoadOrDischargePort()
		{
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "AUSYD";
			consol1.JK_UniqueConsignRef = "C0000001";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol1);

			Factory.Save();

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_RL_NKLoadPort = "AUBNE";
			consol2.JK_RL_NKDischargePort = "AUSYD";
			consol2.JK_UniqueConsignRef = "C0000002";

			var consol3 = Factory.NewWithValidTestData<CommonConsol>();
			consol3.JK_RL_NKLoadPort = "AUSYD";
			consol3.JK_RL_NKDischargePort = "HKHKG";
			consol3.JK_UniqueConsignRef = "C0000003";

			var masterShipment = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			masterShipment.JS_UniqueConsignRef = "S000002";
			masterShipment.Consols.Add(consol2);
			masterShipment.Consols.Add(consol3);

			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			Assert("Sub-shipment's consol should not be attached to master shipment.", !masterShipment.Consols.Contains(consol1));
			Assert("Sub-shipment already has a consol with same load/discharge.", !shipment.Consols.Contains(consol2));
			Assert("Master shipment's consol should be attached to sub-shipment.", shipment.Consols.Contains(consol3));
		}

		#region Implementation

		protected CommonShipment Shipment;
		CommonConsol Consol;
		ZString CompanyCountry;
		ZString HomePort;

		protected override void SetUp()
		{
			CompanyCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			HomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			Shipment = CommonShipment.New(Factory);
			Consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(Consol);
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(CompanyCountry);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = HomePort;
			base.TearDown();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CommonShipment>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommonShipment shipment = factory.New<CommonShipment>();
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsCFSRegistered = false;

			OrgHeader consignor = factory.New<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;
			shipment.ConsignorPK = consignor.PK;

			return shipment;
		}

		#endregion
	}
}
