using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageRatingAdapterTest : TestCaseWithFactory
	{
		public void TestJobDatesProvider()
		{
			var cartageRating = new CartageMoveRatingAdapter(Factory.New<CommonBookedCtgMove>(), null, null);
			AssertType<CartageJobDatesProvider>(((IAutoRating)cartageRating).JobDatesProvider);
		}

		public void TestImportBroker()
		{
			var cartageRating = new CartageMoveRatingAdapter(Factory.New<CommonBookedCtgMove>(), null, null);
			AssertNull(((IAutoRating)cartageRating).ImportBroker);
		}

		public void TestExportBroker()
		{
			var cartageRating = new CartageMoveRatingAdapter(Factory.New<CommonBookedCtgMove>(), null, null);
			AssertNull(((IAutoRating)cartageRating).ExportBroker);
		}

		public void TestCartageLeg()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.DeleteAll();
			CartageRatingAdapter rating = new CartageMoveRatingAdapter(looseMove, null, null);
			Assert(!((IAutoRating)rating).StatusInformation.CanExecute);
			CommonCartageLeg leg1 = looseMove.CartageLegs.AddNew();
			CommonCartageLeg leg2 = looseMove.CartageLegs.AddNew();
			CommonCartageLeg leg3 = looseMove.CartageLegs.AddNew();
			rating = new CartageMoveRatingAdapter(looseMove, null, null);
			Assert(((IAutoRating)rating).StatusInformation.CanExecute);
			AssertEquals(leg1, rating.FirstCartageLeg);
			AssertEquals(leg3, rating.LastCartageLeg);
		}

		public void TestTime()
		{
			var year = ZDateTime.Now.Year;
			var zero = new ZDateTime(year, 1, 1);
			var totalChargeCode = Factory.New<AccChargeCode>();
			totalChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;
			totalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			var cartage = Factory.New<CommonCartage>();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.DeleteAll();
			var leg1 = looseMove.CartageLegs.AddNew();
			leg1.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(1);
			leg1.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(1).AddMinutes(1);
			leg1.JU_CartageDeliveryDemurrage = zero.AddDays(1).AddHours(1).AddMinutes(1);
			var leg2 = looseMove.CartageLegs.AddNew();
			leg2.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(2);
			leg2.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(2).AddMinutes(2);
			leg2.JU_CartageDeliveryDemurrage = zero.AddDays(2).AddHours(2).AddMinutes(2);
			var leg3 = looseMove.CartageLegs.AddNew();
			leg3.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(3);
			leg3.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(3).AddMinutes(3);
			leg3.JU_CartageDeliveryDemurrage = zero.AddDays(3).AddHours(3).AddMinutes(3);
			var rating = new CartageMoveRatingAdapter(looseMove, null, null, true);
			AssertEquals(6, rating.JobServices.Time(totalChargeCode).Span.Days);
			AssertEquals(12, rating.JobServices.Time(totalChargeCode).Span.Hours);
			AssertEquals(18, rating.JobServices.Time(totalChargeCode).Span.Minutes);
		}

		public void TestJobServiceTime()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			chargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var container = Factory.New<CommonContainer>();
			move.EW_JC_Container = container.PK;
			var service = container.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDateTime.Today;
			service.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);
			var leg = move.CartageLegs.AddNew();
			var cartageRating = new CartageMoveRatingAdapter(move, leg.PickupFromDocAddress, leg.DeliverToDocAddress, true);
			AssertEquals(2d, cartageRating.JobServices.Time(chargeCode).Span.TotalHours);
		}

		public void TestTimeFreeTime()
		{
			var year = ZDateTime.Now.Year;
			var zero = new ZDateTime(year, 1, 1);
			var totalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			totalChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;
			totalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			var cartage = Factory.New<CommonCartage>();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.DeleteAll();
			var cto = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			var cfs = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			var cnr = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter);
			var cne = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter);
			var cyd = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard);
			var other = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageMSC);
			var freeWaitingTimeCollection = new FreeWaitingTimeCollection();
			var freeWaitingTime = freeWaitingTimeCollection.AddNew();
			freeWaitingTime.DropMode = Constants.EquipmentNeeded.Any;
			freeWaitingTime.CTO = zero.AddDays(0).AddHours(0).AddMinutes(30);
			freeWaitingTime.CFS = zero.AddDays(0).AddHours(1).AddMinutes(0);
			freeWaitingTime.CNR = zero.AddDays(0).AddHours(1).AddMinutes(30);
			freeWaitingTime.CNE = zero.AddDays(0).AddHours(1).AddMinutes(30);
			freeWaitingTime.CYD = zero.AddDays(0).AddHours(2).AddMinutes(0);
			freeWaitingTime.Other = zero.AddDays(0).AddHours(2).AddMinutes(30);
			var leg1 = looseMove.CartageLegs.AddNew();
			leg1.JU_E2PickupAddressID = cto.PK;
			leg1.JU_E2WaitPointAddressID = cfs.PK;
			leg1.JU_E2DeliveryAddressID = cnr.PK;
			leg1.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(1); // 0:30
			leg1.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(1).AddMinutes(1); // 1:00
			leg1.JU_CartageDeliveryDemurrage = zero.AddDays(1).AddHours(1).AddMinutes(1); // 1:30
			var leg2 = looseMove.CartageLegs.AddNew();
			leg1.JU_E2PickupAddressID = cne.PK;
			leg1.JU_E2WaitPointAddressID = cyd.PK;
			leg1.JU_E2DeliveryAddressID = other.PK;
			leg2.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(2); // 1:30
			leg2.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(2).AddMinutes(2); // 2:00
			leg2.JU_CartageDeliveryDemurrage = zero.AddDays(2).AddHours(2).AddMinutes(2); // 2:30
			var leg3 = looseMove.CartageLegs.AddNew();
			leg1.JU_E2PickupAddressID = cto.PK;
			leg1.JU_E2WaitPointAddressID = cfs.PK;
			leg1.JU_E2DeliveryAddressID = cnr.PK;
			leg3.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(3); // 1:30
			leg3.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(3).AddMinutes(3); // 2:00
			leg3.JU_CartageDeliveryDemurrage = zero.AddDays(3).AddHours(3).AddMinutes(3); // 2:30
			var rating = new CartageMoveRatingAdapter(looseMove, null, null, true);
			AssertEquals(6, rating.JobServices.Time(totalChargeCode).Span.Days);
			AssertEquals(12, rating.JobServices.Time(totalChargeCode).Span.Hours);
			AssertEquals(18, rating.JobServices.Time(totalChargeCode).Span.Minutes);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var address = org.MainAddress;
			cartage.Job.JH_OA_LocalChargesAddr = address.PK;
			cartage.LocalClientPK = org.PK;
			address.OA_UseCumulativeFreeWaitingTime = true;
			var freewaiting = address.FreeWaitingCollection.AddNew();
			freewaiting.OY_DropMode = Constants.EquipmentNeeded.Any;
			freewaiting.OY_OtherFreeWaitingTime = zero.AddHours(5).AddMinutes(0);
			Factory.Save();
			rating = new CartageMoveRatingAdapter(looseMove, null, null, true);
			AssertEquals(5, rating.JobServices.Time(totalChargeCode).Span.Days);
			AssertEquals(6, rating.JobServices.Time(totalChargeCode).Span.Hours);
			AssertEquals(18, rating.JobServices.Time(totalChargeCode).Span.Minutes);
			freewaiting.Delete();
			TransportRegistry.Instance.AmountOfFreeWaitingTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, freeWaitingTimeCollection);
			rating = new CartageMoveRatingAdapter(looseMove, null, null, true);
			AssertEquals(6, rating.JobServices.Time(totalChargeCode).Span.Days);
			AssertEquals(0, rating.JobServices.Time(totalChargeCode).Span.Hours);
			AssertEquals(10, rating.JobServices.Time(totalChargeCode).Span.Minutes);
			TransportRegistry.Instance.UseCumulativeFreeWaitingTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			rating = new CartageMoveRatingAdapter(looseMove, null, null, true);
			AssertEquals(5, rating.JobServices.Time(totalChargeCode).Span.Days);
			AssertEquals(18, rating.JobServices.Time(totalChargeCode).Span.Hours);
			AssertEquals(18, rating.JobServices.Time(totalChargeCode).Span.Minutes);
		}

		public void TestAutoRatingFreightMode()
		{
			foreach (FieldInfo field in typeof(Constants.CartageJobType).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				string cartageType = (string)field.GetValue(null);
				if (cartageType.Length == 4)
				{
					CommonCartage cartage = Factory.New<CommonCartage>();
					cartage.JJ_E3_NKJobType = cartageType;
					if (cartage.CartageType.IsContainerised)
					{
						cartage.ContainerBookedMoves.AddNew();
					}
					else
					{
						cartage.JJ_OuterPacks = 10;
					}

					AssertNotNull("There should be some Local Transport rating objects for " + cartageType, cartage.GetFirstAdapter());
					foreach (CartageRatingAdapter cartageRating in cartage.GetRatingAdapters())
					{
						AssertNotEquals(FreightMode.UKN, ((IAutoRatingFreightInfo)cartageRating).FreightMode);
					}
				}
			}
		}

		public void TestIAutoRating_JobServices_Demurrage()
		{
			var year = ZDateTime.Now.Year;
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "###";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			CartageLegRatingAdapter cartageRatingAdapter = new CartageLegRatingAdapter(leg, forWorkSheet: false, shouldAutorateServices: true);
			//CartageDemurrage Total
			Assert(!cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			AssertEquals(0, (int)cartageRatingAdapter.JobServices.Time(chargeCode).Span.TotalMinutes);
			leg.JU_CartagePickupDemurrage = new ZDateTime(year, 1, 1, 1, 30, 0);
			Assert(cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			AssertEquals(90, (int)cartageRatingAdapter.JobServices.Time(chargeCode).Span.TotalMinutes);
			leg.JU_CartageDeliveryDemurrage = new ZDateTime(year, 1, 1, 0, 45, 0);
			Assert(cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			AssertEquals(135, (int)cartageRatingAdapter.JobServices.Time(chargeCode).Span.TotalMinutes);
			leg.JU_CartagePickupDemurrage = ZDateTime.Empty;
			Assert(cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			AssertEquals(45, (int)cartageRatingAdapter.JobServices.Time(chargeCode).Span.TotalMinutes);
			leg.JU_CartageDeliveryDemurrage = ZDateTime.Empty;
			Assert(!cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			AssertEquals(0, (int)cartageRatingAdapter.JobServices.Time(chargeCode).Span.TotalMinutes);
			leg.JU_CartagePickupDemurrage = new ZDateTime(year, 1, 1, 1, 30, 0);
			Assert(cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			AssertEquals(90, (int)cartageRatingAdapter.JobServices.Time(chargeCode).Span.TotalMinutes);
		}

		public void TestCartageRatingJobServices_DemurrageService_EnabledByLegCartagePickupDemurrage()
		{
			var year = ZDateTime.Now.Year;
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			CartageLegRatingAdapter cartageRatingAdapter = new CartageLegRatingAdapter(leg, forWorkSheet: false, shouldAutorateServices: true);
			//CartageDemurrage Total
			Assert(!cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			var serviceInfos = cartageRatingAdapter.JobServices.FindServices(chargeCode);
			var serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(0d, serviceInfo.ServiceDuration.TotalMinutes);
			leg.JU_CartagePickupDemurrage = new ZDateTime(year, 1, 1, 1, 30, 0);
			Assert(cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			serviceInfos = cartageRatingAdapter.JobServices.FindServices(chargeCode);
			serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(90d, serviceInfo.ServiceDuration.TotalMinutes);
			leg.JU_CartageDeliveryDemurrage = new ZDateTime(year, 1, 1, 0, 45, 0);
			Assert(cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			serviceInfos = cartageRatingAdapter.JobServices.FindServices(chargeCode);
			serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(135d, serviceInfo.ServiceDuration.TotalMinutes);
			leg.JU_CartagePickupDemurrage = ZDateTime.Empty;
			Assert(cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			serviceInfos = cartageRatingAdapter.JobServices.FindServices(chargeCode);
			serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(45d, serviceInfo.ServiceDuration.TotalMinutes);
			leg.JU_CartageDeliveryDemurrage = ZDateTime.Empty;
			Assert(!cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			serviceInfos = cartageRatingAdapter.JobServices.FindServices(chargeCode);
			serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(0d, serviceInfo.ServiceDuration.TotalMinutes);
			leg.JU_CartagePickupDemurrage = new ZDateTime(year, 1, 1, 1, 30, 0);
			Assert(cartageRatingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Transport, ChargeCodeSubGroupList.CartageDemurrageTotal));
			serviceInfos = cartageRatingAdapter.JobServices.FindServices(chargeCode);
			serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(90d, serviceInfo.ServiceDuration.TotalMinutes);
		}

		public void TestCartageRatingJobServices_LooseCartageLegDemurrageDatesSet_EnablesDemurrageService()
		{
			var year = ZDateTime.Now.Year;
			var zero = new ZDateTime(year, 1, 1);
			var totalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			totalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			totalChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;
			var cartage = Factory.New<CommonCartage>();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.DeleteAll();
			var leg1 = looseMove.CartageLegs.AddNew();
			leg1.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(1);
			leg1.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(1).AddMinutes(1);
			leg1.JU_CartageDeliveryDemurrage = zero.AddDays(1).AddHours(1).AddMinutes(1);
			var leg2 = looseMove.CartageLegs.AddNew();
			leg2.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(2);
			leg2.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(2).AddMinutes(2);
			leg2.JU_CartageDeliveryDemurrage = zero.AddDays(2).AddHours(2).AddMinutes(2);
			var leg3 = looseMove.CartageLegs.AddNew();
			leg3.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(3);
			leg3.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(3).AddMinutes(3);
			leg3.JU_CartageDeliveryDemurrage = zero.AddDays(3).AddHours(3).AddMinutes(3);
			var rating = (IAutoRating)new CartageMoveRatingAdapter(looseMove, null, null, true);
			var serviceInfo = rating.JobServices.FindServices(totalChargeCode).FirstOrDefault();
			AssertEquals(6, serviceInfo.ServiceDuration.Days);
			AssertEquals(12, serviceInfo.ServiceDuration.Hours);
			AssertEquals(18, serviceInfo.ServiceDuration.Minutes);
		}

		public void TestCartageRatingJobServices_StandardService_EnabledByServiceCompletedDate()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			chargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var container = Factory.New<CommonContainer>();
			move.EW_JC_Container = container.PK;
			var service = container.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDateTime.Today;
			service.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);
			var leg = move.CartageLegs.AddNew();
			var cartageRating = new CartageMoveRatingAdapter(move, leg.PickupFromDocAddress, leg.DeliverToDocAddress, true);
			AssertEquals(2d, cartageRating.JobServices.FindServices(chargeCode).FirstOrDefault().ServiceDuration.TotalHours);
		}

		public void TestCartageRatingJobServices_CartageDemurrageServiceDurationIsOffSetByFreeTime()
		{
			var year = ZDateTime.Now.Year;
			var zero = new ZDateTime(year, 1, 1);
			var cartageDemurrageCharge = Factory.New<AccChargeCode>();
			cartageDemurrageCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			cartageDemurrageCharge.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;
			var cartage = Factory.New<CommonCartage>();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.DeleteAll();
			var cto = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			var cfs = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			var cnr = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter);
			var cne = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter);
			var cyd = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard);
			var other = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageMSC);
			var freeWaitingTimeCollection = new FreeWaitingTimeCollection();
			var freeWaitingTime = freeWaitingTimeCollection.AddNew();
			freeWaitingTime.DropMode = Constants.EquipmentNeeded.Any;
			freeWaitingTime.CTO = zero.AddDays(0).AddHours(0).AddMinutes(30);
			freeWaitingTime.CFS = zero.AddDays(0).AddHours(1).AddMinutes(0);
			freeWaitingTime.CNR = zero.AddDays(0).AddHours(1).AddMinutes(30);
			freeWaitingTime.CNE = zero.AddDays(0).AddHours(1).AddMinutes(30);
			freeWaitingTime.CYD = zero.AddDays(0).AddHours(2).AddMinutes(0);
			freeWaitingTime.Other = zero.AddDays(0).AddHours(2).AddMinutes(30);
			var leg1 = looseMove.CartageLegs.AddNew();
			leg1.JU_E2PickupAddressID = cto.PK;
			leg1.JU_E2WaitPointAddressID = cfs.PK;
			leg1.JU_E2DeliveryAddressID = cnr.PK;
			leg1.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(1); // 0:30
			leg1.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(1).AddMinutes(1); // 1:00
			leg1.JU_CartageDeliveryDemurrage = zero.AddDays(1).AddHours(1).AddMinutes(1); // 1:30
			var leg2 = looseMove.CartageLegs.AddNew();
			leg1.JU_E2PickupAddressID = cne.PK;
			leg1.JU_E2WaitPointAddressID = cyd.PK;
			leg1.JU_E2DeliveryAddressID = other.PK;
			leg2.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(2); // 1:30
			leg2.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(2).AddMinutes(2); // 2:00
			leg2.JU_CartageDeliveryDemurrage = zero.AddDays(2).AddHours(2).AddMinutes(2); // 2:30
			var leg3 = looseMove.CartageLegs.AddNew();
			leg1.JU_E2PickupAddressID = cto.PK;
			leg1.JU_E2WaitPointAddressID = cfs.PK;
			leg1.JU_E2DeliveryAddressID = cnr.PK;
			leg3.JU_CartagePickupDemurrage = zero.AddDays(0).AddHours(0).AddMinutes(3); // 1:30
			leg3.JU_CartageWaitPointDemurrage = zero.AddDays(0).AddHours(3).AddMinutes(3); // 2:00
			leg3.JU_CartageDeliveryDemurrage = zero.AddDays(3).AddHours(3).AddMinutes(3); // 2:30
			var rating = new CartageMoveRatingAdapter(looseMove, null, null, true);
			var serviceInfo = rating.JobServices.FindServices(cartageDemurrageCharge).FirstOrDefault();
			var duration = serviceInfo.ServiceDuration;
			AssertEquals(6, duration.Days);
			AssertEquals(12, duration.Hours);
			AssertEquals(18, duration.Minutes);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var address = org.MainAddress;
			cartage.Job.JH_OA_LocalChargesAddr = address.PK;
			cartage.LocalClientPK = org.PK;
			address.OA_UseCumulativeFreeWaitingTime = true;
			var freewaiting = address.FreeWaitingCollection.AddNew();
			freewaiting.OY_DropMode = Constants.EquipmentNeeded.Any;
			freewaiting.OY_OtherFreeWaitingTime = zero.AddHours(5).AddMinutes(0);
			Factory.Save();
			rating = new CartageMoveRatingAdapter(looseMove, null, null, true);
			serviceInfo = rating.JobServices.FindServices(cartageDemurrageCharge).FirstOrDefault();
			duration = serviceInfo.ServiceDuration;
			AssertEquals(5, duration.Days);
			AssertEquals(6, duration.Hours);
			AssertEquals(18, duration.Minutes);
			freewaiting.Delete();
			TransportRegistry.Instance.AmountOfFreeWaitingTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, freeWaitingTimeCollection);
			rating = new CartageMoveRatingAdapter(looseMove, null, null, true);
			serviceInfo = rating.JobServices.FindServices(cartageDemurrageCharge).FirstOrDefault();
			duration = serviceInfo.ServiceDuration;
			AssertEquals(6, duration.Days);
			AssertEquals(0, duration.Hours);
			AssertEquals(10, duration.Minutes);
			TransportRegistry.Instance.UseCumulativeFreeWaitingTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			rating = new CartageMoveRatingAdapter(looseMove, null, null, true);
			serviceInfo = rating.JobServices.FindServices(cartageDemurrageCharge).FirstOrDefault();
			duration = serviceInfo.ServiceDuration;
			AssertEquals(5, duration.Days);
			AssertEquals(18, duration.Hours);
			AssertEquals(18, duration.Minutes);
		}

		public void TestCartageRatingJobServices_NoAdditionalJobServices()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var ratingResult = new CartageMoveRatingAdapter(move, move.PickupFromDocAddress, move.DeliverToDocAddress, false);
			Assert(ratingResult.JobServices.All(i => !i.IsEnabled));
		}

		public void TestCartageRatingJobServices_SingleAdditionalJobServices()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var service = move.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service.ES_Completed = ZDateTime.Today;
			service.ES_ServiceCount = 1;
			var ratingResult = new CartageMoveRatingAdapter(move, move.PickupFromDocAddress, move.DeliverToDocAddress, false);
			AssertEquals("CLN", ratingResult.JobServices.Single(i => i.IsEnabled).ServiceCode);
		}

		public void TestCartageRatingJobServices_MultipleAdditionalJobServices()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var enabledService = move.Services.AddNew();
			enabledService.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			enabledService.ES_Completed = ZDateTime.Today;
			enabledService.ES_ServiceCount = 1;
			var disabledService = move.Services.AddNew();
			disabledService.ES_ServiceCode = Constants.FreightServiceType.Codes.Washing;
			disabledService.ES_ServiceCount = 1;
			var legAdapter = new CartageLegRatingAdapter(leg, forWorkSheet: false, shouldAutorateServices: true);
			var legAdapterService = legAdapter.JobServices.Single();
			AssertEquals("DME", legAdapterService.ServiceCode);
			Assert(!legAdapterService.IsEnabled);
			var moveAdapter = new CartageMoveRatingAdapter(move, move.PickupFromDocAddress, move.DeliverToDocAddress, false);
			Assert(moveAdapter.JobServices.First(i => i.ServiceCode == Constants.FreightServiceType.Codes.Cleaning).IsEnabled);
			AssertEquals("Service has not completed date so is not enabled", false, moveAdapter.JobServices.First(i => i.ServiceCode == Constants.FreightServiceType.Codes.Washing).IsEnabled);
			AssertEquals("Not included in the list of services so is considered disabled", false, moveAdapter.JobServices.First(i => i.ServiceCode == Constants.FreightServiceType.Codes.Fumigation).IsEnabled);
		}

		public void TestAutoRatingMeasures()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_GrossWeight = 2000m;
			cartage.ContainerBookedMoves[0].EW_BookedPackCount = 5;

			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			container1.JC_GrossWeight = 1000m;
			cartage.ContainerBookedMoves[1].EW_BookedPackCount = 11;

			var adapters = cartage.GetRatingAdapters();
			AssertEquals("Should have two rating adapter", 2, adapters.Count);

			var autoRating = adapters.First();
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertQuantity(rateableMeasures.GetQuantity(MeasureType.Weight), 2000m, Constants.Weight.Kilograms);
			AssertQuantity(rateableMeasures.GetQuantity(MeasureType.Volume), 0m, Constants.Volume.CubicMetres);
			AssertQuantity(rateableMeasures.GetQuantity(MeasureType.Package), 5m, Constants.PkgUnit.Pallet);
			var actualContainerInfo = rateableMeasures.GetContainerGroups().Single().Containers.Single();
			AssertEquals(2000m, actualContainerInfo.ContainerWeightInKG);
			AssertEquals(1m, actualContainerInfo.TEU);

			var lastAutoRating = adapters.Last();
			rateableMeasures = (RateableMeasureSet)lastAutoRating.RateableMeasures;
			AssertQuantity(rateableMeasures.GetQuantity(MeasureType.Weight), 1000m, Constants.Weight.Kilograms);
			AssertQuantity(rateableMeasures.GetQuantity(MeasureType.Volume), 0m, Constants.Volume.CubicMetres);
			AssertQuantity(rateableMeasures.GetQuantity(MeasureType.Package), 11m, Constants.PkgUnit.Pallet);

			cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.JJ_Weight = 1m;
			cartage.JJ_WeightUQ = Constants.Weight.Tonnes;
			cartage.JJ_Volume = 100m;
			cartage.JJ_VolumeUQ = Constants.Volume.Litre;
			cartage.JJ_OuterPacks = 15;
			cartage.JJ_F3_NKPackType = Constants.PkgUnit.Bag;
			autoRating = cartage.GetFirstAdapter();
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertQuantity(rateableMeasures.GetQuantity(MeasureType.Weight), 1m, Constants.Weight.Tonnes);
			AssertQuantity(rateableMeasures.GetQuantity(MeasureType.Volume), 100m, Constants.Volume.Litre);
			AssertQuantity(rateableMeasures.GetQuantity(MeasureType.Package), 15m, Constants.PkgUnit.Bag);
		}

		public void TestAutoRatingMeasures_Units()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.JJ_OuterPacks = 15;
			cartage.JJ_F3_NKPackType = Constants.PkgUnit.Bag;
			var adapter = cartage.GetFirstAdapter();
			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals(15m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals(ZString.Empty, rateableMeasures.GetUnit(MeasureType.Unit));
			var move = cartage.LooseBookedMoves[0];
			move.EW_BookedPackCount = 5;
			move.EW_F3_NKPackType = Constants.PkgUnit.Pallet;
			adapter = cartage.GetFirstAdapter();
			rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals(ZString.Empty, rateableMeasures.GetUnit(MeasureType.Unit));
		}

		void AssertQuantity(ZArchitecture.Quantity quantity, ZDecimal value, ZString unit)
		{
			AssertEquals(value, quantity.Amount);
			AssertEquals(unit, quantity.Unit);
		}

		public void TestAutoRatingContainers()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			IAutoRating autoRating = cartage.GetFirstAdapter();
			AssertEquals(ZGuid.Empty, ((RateableMeasureSet)autoRating.RateableMeasures).GetContainerTypePKs().Single());
			RefContainer gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.JC_RC = gP20.PK;
			AssertEquals(gP20.PK, ((RateableMeasureSet)autoRating.RateableMeasures).GetContainerTypePKs().Single());
			cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			autoRating = cartage.GetFirstAdapter();
			AssertEquals(false, ((RateableMeasureSet)autoRating.RateableMeasures).HasMeasureType(MeasureType.ContainerCount));
		}

		public void TestAutoRatingContainerNumber()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var autoRating = cartage.GetFirstAdapter();
			AssertEquals(ZGuid.Empty, ((RateableMeasureSet)autoRating.RateableMeasures).GetContainerTypePKs().Single());
			container.JC_ContainerNum = "MWH1234567";
			AssertEquals("MWH1234567", ((RateableMeasureSet)autoRating.RateableMeasures).GetContainerGroups().Single().ContainerNumber);
		}

		public void TestAutoRatingEquipmentAndServiceLevel()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			AssertNotNull(cartage.GetFirstAdapter());
			IAutoRating autoRating = cartage.GetFirstAdapter();
			AssertEquals("", autoRating.PickupCartageEquipment);
			AssertEquals("", autoRating.DeliveryCartageEquipment);
			AssertEquals("STD", autoRating.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			cartage.JJ_DropMode = "SDL";
			AssertEquals("SDL", autoRating.PickupCartageEquipment);
			AssertEquals("SDL", autoRating.DeliveryCartageEquipment);
			cartage.JJ_RS_NKServiceLevel = "ABC";
			AssertEquals("ABC", autoRating.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			move.EW_DropMode = "XXX";
			AssertEquals("XXX", autoRating.PickupCartageEquipment);
			AssertEquals("XXX", autoRating.DeliveryCartageEquipment);
		}

		public void TestAutoRatingStatusInformation()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = "C1010";
			AssertNotNull(cartage.GetFirstAdapter());
			IAutoRating autoRating = cartage.GetFirstAdapter();
			AssertEquals("AutoRating can be executed", true, autoRating.StatusInformation.CanExecute);
			cartage.GetBookedMoves(container)[0].CartageLegs[0].JU_AdditionalService = "SVC";
			cartage.GetBookedMoves(container)[0].CartageLegs[1].JU_AdditionalService = "SVC";
			var provider = ((IRatingSupporter)cartage).AdaptersProvider;
			var uiInteractor = new RatingAdaptersProviderTest.TestUIInteractor();
			using (provider.NewRatingSession())
			{
				Assert("AutoRating can NOT be executed", !provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
				AssertEquals("AutoRating error regarding cartage legs", "Booking (C1010) doesn't contain any included Port Transport Legs. Cannot continue. Ensure all Legs are not marked additional.", uiInteractor.errors.Last());
			}

			cartage.GetBookedMoves(container)[0].CartageLegs.DeleteAll();
			using (provider.NewRatingSession())
			{
				Assert("AutoRating can NOT be executed", !provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
				AssertEquals("AutoRating error regarding cartage legs", "Booking (C1010) doesn't contain any included Port Transport Legs. Cannot continue. Ensure all Legs are not marked additional.", uiInteractor.errors.Last());
			}
		}

		public void TestAutoRatingStatusInformation_InvalidUnit()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = "C1010";

			var autoRating = cartage.GetFirstAdapter();
			AssertNotNull("Pre-condition", autoRating);
			AssertEquals("AutoRating can be executed", true, autoRating.StatusInformation.CanExecute);

			var provider = ((IRatingSupporter)cartage).AdaptersProvider;
			var uiInteractor = new RatingAdaptersProviderTest.TestUIInteractor();

			var move = cartage.GetBookedMoves(container)[0];
			CombineAssertions(() =>
			{
				AssertInvalidUnit((unit) => container.JC_GrossWeightUQ = unit, "M3", "KG", "weight");

				AssertInvalidUnit((unit) => move.EW_DistanceUnit = unit, "W", "KM", "distance");
				AssertInvalidUnit((unit) => move.EW_WeightUQ = unit, "M3", "KG", "weight");
				AssertInvalidUnit((unit) => move.EW_VolumeUQ = unit, "KG", "M3", "volume");
			});

			var leg = move.CartageLegs[0];
			leg.JU_AdditionalService = "SVC";
			AssertInvalidUnit((unit) => leg.JU_DistanceUnit = unit, "W", "KM", "distance");

			void AssertInvalidUnit(Action<string> unitAction, string invalidUnit, string validUnit, string unitType)
			{
				uiInteractor.errors.Clear();
				unitAction(invalidUnit);
				using (provider.NewRatingSession())
				{
					Assert($"{unitType}: AutoRating can NOT be executed", !provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
					AssertEquals("Error Count", 1, uiInteractor.errors.Count);
					if (uiInteractor.errors.Count > 0)
					{
						AssertEquals($"Invalid unit of {unitType}: '{invalidUnit}'.", uiInteractor.errors[0]);
					}
				}

				uiInteractor.errors.Clear();
				unitAction(validUnit);
				using (provider.NewRatingSession())
				{
					Assert($"{unitType}: AutoRating can be executed", provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
					AssertEquals("Error Count", 0, uiInteractor.errors.Count);
				}

				uiInteractor.errors.Clear();
				unitAction("");
				using (provider.NewRatingSession())
				{
					Assert($"{unitType}: AutoRating can NOT be executed", !provider.CanExecuteAutoRating(uiInteractor, AutoRateOptions.AutorateRevenue));
					AssertEquals("Error Count", 1, uiInteractor.errors.Count);
					if (uiInteractor.errors.Count > 0)
					{
						AssertEquals($"Invalid unit of {unitType}: ''.", uiInteractor.errors[0]);
					}
				}

				unitAction(validUnit);
			}
		}

		public void TestIJobNumber()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "hello";
			CommonBookedCtgMove bookedMove = cartage.LooseBookedMoves.AddNew();
			CartageRatingAdapter cartageRatingAdapter = new CartageMoveRatingAdapter(bookedMove, null, null);
			AssertEquals("CartageRatingAdapter has the jobnumber from the cartage", "hello", ((IJobNumber)cartageRatingAdapter).JobNumber);
		}

		[ExpectNoExceptions]
		public void TestIAutoRatingLocations_Origin()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USLAX";
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirExport, 1);
			cartage.JJ_GB = branch.PK;
			IAutoRatingLocations autoRatingLocation = new CartageMoveRatingAdapter(cartage.BookedMovesCollection[0], null, null);
			AssertEquals("AUSYD", autoRatingLocation.Origin.Code);
			cartage.JJ_GB = ZGuid.Empty;
			AssertEquals("USLAX", autoRatingLocation.Origin.Code);
		}

		public void TestConditionsSupporter()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var adapter = new CartageMoveRatingAdapter(move, Factory.New<JobDocAddress>(), Factory.New<JobDocAddress>());
			AssertType<CartageRateLineConditionsSupporter>(((IAutoRatingFreightConditionsSupportable)adapter).ConditionsSupporter);
		}

		public void TestRatingAdapterTypeAndID()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			cartage.ContainerBookedMoves.AddNew();
			var adapter = cartage.GetFirstAdapter();
			AssertEquals(AdapterType.PortTransport, adapter.AdapterType);
			AssertEquals(cartage.JJ_ConsignmentID, adapter.OperationalJobCode);
			AssertEquals(cartage.JJ_ConsignmentID, adapter.JobID);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
