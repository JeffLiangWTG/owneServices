using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSContainerRatingAdapterTest : TestCaseWithFactory
	{
		public void TestJobDatesProvider()
		{
			var container = Factory.New<CFSContainer>();
			AssertType<CFSContainerStorageJobDateProvider>(GetIAutoRating(container).JobDatesProvider);
		}

		public void TestImportBroker()
		{
			var container = Factory.New<CFSContainer>();
			AssertNull(GetIAutoRating(container).ImportBroker);
		}

		public void TestExportBroker()
		{
			var container = Factory.New<CFSContainer>();
			AssertNull(GetIAutoRating(container).ExportBroker);
		}

		public void TestAdapterTypeAndID()
		{
			var container = Factory.New<CFSContainer>();
			var adapter = GetIAutoRating(container);
			AssertEquals(AdapterType.CFSContainer, adapter.AdapterType);
			AssertEquals(container.JC_ContainerNum, adapter.OperationalJobCode);
		}

		#region Autorating

		public void TestAutoratingFreightMode()
		{
			var container = Factory.New<CFSContainer>();
			AssertEquals(FreightMode.FCL, GetIAutoRating(container).FreightMode);
		}

		public void TestAutoRatingContainers()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);

			var measures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(ZGuid.Empty, measures.GetContainerTypePKs().Single());
			AssertEquals(1, measures.GetAllContainers().Count());

			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.JC_RC = gP20.PK;
			measures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(gP20.PK, measures.GetContainerTypePKs().Single());
			AssertEquals(1, measures.GetAllContainers().Count());
		}

		public void TestAutoRatingBondedStorage()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage;

			AssertEquals(0m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(-15);
			AssertEquals(0m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));

			container.JC_FCLStorageArrivedUnderbond = true;
			AssertEquals(16m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));

			container.JC_FCLStorageUnderbondCleared = container.JC_ArrivalCTOStorageStartDate.AddDays(5);
			AssertEquals(6m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));
		}

		public void TestAutoRatingBondedStorage_UsesDateOnly()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);
			container.JC_FCLStorageArrivedUnderbond = true;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage;

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 16, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 22, 10, 0, 0);
			AssertEquals(13m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 0, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 21, 23, 0, 0);
			AssertEquals(12m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 0, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 22, 0, 0, 0);
			AssertEquals(13m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
		}

		public void TestAutoRatingFreeStorage()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLContainerStorage;

			AssertEquals(0m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLContainerStorage));

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(-15);
			AssertEquals(16m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLContainerStorage));

			container.JC_FCLStorageArrivedUnderbond = true;
			AssertEquals(0m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLContainerStorage));

			container.JC_FCLStorageUnderbondCleared = container.JC_ArrivalCTOStorageStartDate.AddDays(5);
			AssertEquals(10m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLContainerStorage));
		}

		public void TestAutoRatingFreeStorage_UsesDateOnly()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLContainerStorage;

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 16, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 22, 10, 0, 0);
			AssertEquals(13m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 0, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 21, 23, 0, 0);
			AssertEquals(12m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 0, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 22, 0, 0, 0);
			AssertEquals(13m, (decimal)autoRating.JobServices.Time(chargeCode).Span.TotalDays);
		}

		public void TestAutoRatingFCLAndBondedStorage()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);

			var bondChargeCode = Factory.New<AccChargeCode>();
			bondChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			bondChargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage;

			var fclChargeCode = Factory.New<AccChargeCode>();
			fclChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			fclChargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLContainerStorage;

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(-10);
			container.JC_FCLStorageArrivedUnderbond = false;
			container.JC_DepartureTime = ZDateTime.Today;
			AssertEquals("Pre-condition - default Bond Storage is 0 day", 0m, (decimal)autoRating.JobServices.Time(bondChargeCode).Span.TotalDays);
			AssertEquals("Pre-condition - start and end date inclusive", 11m, (decimal)autoRating.JobServices.Time(fclChargeCode).Span.TotalDays);

			container.JC_FCLStorageArrivedUnderbond = true;
			container.JC_FCLStorageUnderbondCleared = container.JC_ArrivalCTOStorageStartDate.AddDays(5);

			AssertEquals("Container BOND Storage should be equal to 6 days.", 6m, (decimal)autoRating.JobServices.Time(bondChargeCode).Span.TotalDays);
			AssertEquals("Container FCL Storage should be equal to 5 days.", 5m, (decimal)autoRating.JobServices.Time(fclChargeCode).Span.TotalDays);

			container.JC_DepartureTime = ZDateTime.Today.AddDays(-1);
			AssertEquals("Container FCL Storage should be equal to 4 days.", 4m, (decimal)autoRating.JobServices.Time(fclChargeCode).Span.TotalDays);
		}

		public void TestContainerJobServices_HasUnderbondStorageCommencesDateSet_EnablesUnderbondStorageStorageService()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage;

			var serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(0d, serviceInfo.ServiceDuration.TotalDays);
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(-15);
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(0d, serviceInfo.ServiceDuration.TotalDays);
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));

			container.JC_FCLStorageArrivedUnderbond = true;
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(16d, serviceInfo.ServiceDuration.TotalDays);
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));

			container.JC_FCLStorageUnderbondCleared = container.JC_ArrivalCTOStorageStartDate.AddDays(5);
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(6d, serviceInfo.ServiceDuration.TotalDays);
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));
		}

		public void TestContainerJobServices_HasUnderbondStorageCommencesDateSet_UsesOnlyDatePart()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);
			container.JC_FCLStorageArrivedUnderbond = true;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage;

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 16, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 22, 10, 0, 0);
			var serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(13d, serviceInfo.ServiceDuration.TotalDays);

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 0, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 21, 23, 0, 0);
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(12d, serviceInfo.ServiceDuration.TotalDays);

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 0, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 22, 0, 0, 0);
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(13d, serviceInfo.ServiceDuration.TotalDays);
		}

		public void TestContainerJobServices_HasContainerStorageCommencesDateSet_EnablesContainerStorageStorageService()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLContainerStorage;

			var serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(0d, serviceInfo.ServiceDuration.TotalDays);
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLContainerStorage));

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(-15);
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(16d, serviceInfo.ServiceDuration.TotalDays);
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLContainerStorage));

			container.JC_FCLStorageArrivedUnderbond = true;
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(0d, serviceInfo.ServiceDuration.TotalDays);
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLContainerStorage));

			container.JC_FCLStorageUnderbondCleared = container.JC_ArrivalCTOStorageStartDate.AddDays(5);
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(10d, serviceInfo.ServiceDuration.TotalDays);
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.ContainerStorage, Core.Constants.FreightServiceType.Codes.FCLContainerStorage));
		}

		public void TestContainerJobServices_HasContainerStorageCommencesDateSet_UsesOnlyDatePart()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLContainerStorage;

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 16, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 22, 10, 0, 0);
			var serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(13d, serviceInfo.ServiceDuration.TotalDays);

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 0, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 21, 23, 0, 0);
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(12d, serviceInfo.ServiceDuration.TotalDays);

			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2008, 1, 10, 0, 0, 0);
			container.JC_DepartureTime = new ZDateTime(2008, 1, 22, 0, 0, 0);
			serviceInfo = autoRating.JobServices.FindServices(chargeCode).FirstOrDefault();
			AssertEquals(13d, serviceInfo.ServiceDuration.TotalDays);
		}

		public void TestContainerJobServices_HasContainerAndUnderbondStorageCommencesDate()
		{
			var container = Factory.New<CFSContainer>();
			var autoRating = GetIAutoRating(container);

			var bondChargeCode = Factory.New<AccChargeCode>();
			bondChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			bondChargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage;

			var fclChargeCode = Factory.New<AccChargeCode>();
			fclChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			fclChargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.FCLContainerStorage;

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(-10);
			container.JC_FCLStorageArrivedUnderbond = false;
			container.JC_DepartureTime = ZDateTime.Today;

			var bondServiceInfo = autoRating.JobServices.FindServices(bondChargeCode).FirstOrDefault();
			var fclServiceInfo = autoRating.JobServices.FindServices(fclChargeCode).FirstOrDefault();

			AssertEquals("Pre-condition - default Bond Storage is 0 day", 0d, bondServiceInfo.ServiceDuration.TotalDays);
			AssertEquals("Pre-condition - start and end date inclusive", 11d, fclServiceInfo.ServiceDuration.TotalDays);

			container.JC_FCLStorageArrivedUnderbond = true;
			container.JC_FCLStorageUnderbondCleared = container.JC_ArrivalCTOStorageStartDate.AddDays(5);

			bondServiceInfo = autoRating.JobServices.FindServices(bondChargeCode).FirstOrDefault();
			AssertEquals("Container BOND Storage should be equal to 6 days.", 6d, bondServiceInfo.ServiceDuration.TotalDays);

			fclServiceInfo = autoRating.JobServices.FindServices(fclChargeCode).FirstOrDefault();
			AssertEquals("Container FCL Storage should be equal to 5 days.", 5d, fclServiceInfo.ServiceDuration.TotalDays);

			container.JC_DepartureTime = ZDateTime.Today.AddDays(-1);
			fclServiceInfo = autoRating.JobServices.FindServices(fclChargeCode).FirstOrDefault();
			AssertEquals("Container FCL Storage should be equal to 4 days.", 4d, fclServiceInfo.ServiceDuration.TotalDays);
		}

		#endregion

		#region GetIAutoRating

		static IAutoRating GetIAutoRating(CFSContainer container)
		{
			return new CFSContainerRatingAdapter(container);
		}

		#endregion
	}
}
