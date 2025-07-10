using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business.Testing
{
	abstract class ContractPenaltyMatcherTest : TestCaseWithFactory
	{
		public void TestAllFilterPropertiesAffectMatching_Detention()
		{
			SetupForTest(Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertAllFilterPropertiesAffectMatching(true);
		}

		public void TestAllFilterPropertiesAffectMatching_Storage()
		{
			SetupForTest(Constants.ContainerPenaltyPenaltyType.Codes.Storage);
			AssertAllFilterPropertiesAffectMatching(false);
		}

		public void TestCreditorTypeDoesntAffectMatchDetention()
		{
			SetupForTest(Constants.ContainerPenaltyPenaltyType.Codes.Detention);

			var matcher = GetNewMatcher();

			Filter.CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			AssertNotNull("Creditor Type should be irrelevant to Detention matching.", matcher.MatchDetention(Filter));

			Filter.CreditorType = "XXX";
			AssertNotNull("Creditor Type should be irrelevant to Detention matching.", matcher.MatchDetention(Filter));

			Filter.CreditorType = string.Empty;
			AssertNotNull("Creditor Type should be irrelevant to Detention matching.", matcher.MatchDetention(Filter));

			Filter.CreditorType = Constants.ContainerPenaltyCreditorType.Codes.CTO;
			AssertNotNull("Creditor Type should be irrelevant to Detention matching.", matcher.MatchDetention(Filter));
		}

		public void TestCreditorTypeAffectsMatchStorage()
		{
			SetupForTest(Constants.ContainerPenaltyPenaltyType.Codes.Storage);

			var matcher = GetNewMatcher();

			Filter.CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			AssertNotNull("CAR Creditor Type should be allowed for Storage matching.", matcher.MatchStorage(Filter));

			Filter.CreditorType = "XXX";
			AssertNull("Non-CAR Creditor Type should not be allowed for Storage matching.", matcher.MatchStorage(Filter));

			Filter.CreditorType = string.Empty;
			AssertNull("Non-CAR Creditor Type should not be allowed for Storage matching.", matcher.MatchStorage(Filter));

			Filter.CreditorType = Constants.ContainerPenaltyCreditorType.Codes.CTO;
			AssertNull("Non-CAR Creditor Type should not be allowed for Storage matching.", matcher.MatchStorage(Filter));
		}

		public void TestCreditorTypeDoesntAffectMatchMDD()
		{
			SetupForTest(Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);

			var matcher = GetNewMatcher();

			Filter.CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			AssertNotNull("Creditor Type should be irrelevant to Detention matching.", matcher.MatchMDD(Filter));

			Filter.CreditorType = "XXX";
			AssertNotNull("Creditor Type should be irrelevant to Detention matching.", matcher.MatchMDD(Filter));

			Filter.CreditorType = string.Empty;
			AssertNotNull("Creditor Type should be irrelevant to Detention matching.", matcher.MatchMDD(Filter));

			Filter.CreditorType = Constants.ContainerPenaltyCreditorType.Codes.CTO;
			AssertNotNull("Creditor Type should be irrelevant to Detention matching.", matcher.MatchMDD(Filter));
		}

		public void TestMatchDetentionReturnsNullWhenNoContainerProvided()
		{
			SetupForTest(Constants.ContainerPenaltyPenaltyType.Codes.Detention);

			var matcher = GetNewMatcher();
			var result = matcher.MatchDetention(Filter);
			AssertNotNull("All filters matching, so matcher should return a penalty.", result);

			Filter.Container = null;
			result = matcher.MatchDetention(Filter);
			AssertNull("Filter does not have a container specified and so matcher should return null.", result);
		}

		public void TestMatchStorageReturnsNullWhenNoContainerProvided()
		{
			SetupForTest(Constants.ContainerPenaltyPenaltyType.Codes.Storage);

			var matcher = GetNewMatcher();
			var result = matcher.MatchStorage(Filter);
			AssertNotNull("All filters matching, so matcher should return a penalty.", result);

			Filter.Container = null;
			result = matcher.MatchStorage(Filter);
			AssertNull("Filter does not have a container specified and so matcher should return null.", result);
		}

		public void TestMatchMDDReturnsNullWhenNoContainerProvided()
		{
			SetupForTest(Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);

			var matcher = GetNewMatcher();
			var result = matcher.MatchMDD(Filter);
			AssertNotNull("All filters matching, so matcher should return a penalty.", result);

			Filter.Container = null;
			result = matcher.MatchMDD(Filter);
			AssertNull("Filter does not have a container specified and so matcher should return null.", result);
		}

		#region Implementation

		protected abstract ContractPenaltyMatcher GetNewMatcher();

		protected abstract string[] ValidProcessTypes { get; }
		protected abstract string ExpectedContractType { get; }

		#region Test Setup

		protected IRatingContract Contract => contract ?? (contract = Factory.New<IRatingContract>());
		IRatingContract contract;

		protected IRatingContractContainerDetention ContractDetention => contractDetention ?? (contractDetention = Factory.New<IRatingContractContainerDetention>());
		IRatingContractContainerDetention contractDetention;

		protected OrgHeader ContractOrg => contractOrg ?? (contractOrg = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader contractOrg;

		protected ContainerPenaltyMatchFilter Filter => filter ?? (filter = new ContainerPenaltyMatchFilter());
		ContainerPenaltyMatchFilter filter;

		protected void SetupForTest(string penaltyType)
		{
			ResetForTest();

			SetContractProperties();
			SetDetentionProperties(penaltyType);
			SetMatchingFilterProperties();
		}

		protected void ResetForTest()
		{
			contract = null;
			contractDetention = null;
			contractOrg = null;
			filter = null;
		}

		protected virtual void SetContractProperties()
		{
			Contract.RCT_OH = ContractOrg.PK;
			Contract.RCT_StartDate = ZDate.Today.AddDays(-15);
			Contract.RCT_ContractNumber = "CN0001";
			Contract.RCT_EndDate = ZDate.Today.AddDays(15);
			Contract.RCT_ContractType = ExpectedContractType;
			Contract.RCT_IsActive = true;
		}

		protected virtual void SetDetentionProperties(string penaltyType)
		{
			ContractDetention.RCD_RCT = Contract.PK;
			ContractDetention.RCD_Direction = Constants.ContainerDetentionDirection.Import;
			ContractDetention.RCD_PenaltyType = penaltyType;
			ContractDetention.RCD_FreeDays = 5;
			ContractDetention.RCD_StartDateOverride = ZDate.Today.AddDays(-5);
			ContractDetention.RCD_EndDateOverride = ZDate.Today.AddDays(5);
			ContractDetention.RCD_ContainerType = "40G";
			ContractDetention.RCD_OriginPortOrCountry = "AUSYD";
			ContractDetention.RCD_DetentionPortOrCountry = "NZAKL";
		}

		protected virtual void SetMatchingFilterProperties()
		{
			SetupContainerAndRelatedBizos();

			Filter.OriginPort = "AUSYD";
			Filter.DetentionPort = "NZAKL";
			Filter.ContainerClass = "40G";
			Filter.Direction = Constants.ContainerDetentionDirection.Import;
			Filter.CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			Filter.ProcessType = ValidProcessTypes[0];
			Filter.GetClientsFunction = GetClientsForTest;

			SetFilterContractOrg(ContractOrg);
			SetFilterContractNumber("CN0001");
			SetFilterDepartureDateTime(ZDate.Today);
		}

		IReadOnlyCollection<IOrgHeader> GetClientsForTest(ICommonShipment shipment, ZString direction)
		{
			var commonShipment = shipment as CommonShipment;
			var clients = new List<IOrgHeader>();

			if (commonShipment == null)
			{
				return clients;
			}

			if (commonShipment.ShipmentJobHeader?.LocalCharges != null)
			{
				clients.Add(commonShipment.ShipmentJobHeader?.LocalCharges);
			}

			if (commonShipment.Consignor != null)
			{
				clients.Add(commonShipment.Consignor);
			}

			if (commonShipment.Consignee != null)
			{
				clients.Add(commonShipment.Consignee);
			}

			return clients;
		}

		protected abstract void SetupContainerAndRelatedBizos();

		protected abstract void SetFilterContractOrg(OrgHeader value);

		protected abstract void SetFilterContractNumber(ZString value);

		protected abstract void SetFilterDepartureDateTime(ZDateTime value);

		#endregion

		#region Assertions

		protected virtual void AssertAllFilterPropertiesAffectMatching(bool isDetention)
		{
			AssertPropertyAffectsMatching(isDetention, value => Filter.DetentionPort = value, (string)Filter.DetentionPort, "CNSHA");
			AssertPropertyAffectsMatching(isDetention, value => Filter.ContainerClass = value, (string)Filter.ContainerClass, "CNSHA");
			AssertPropertyAffectsMatching(isDetention, value => Filter.Direction = value, (string)Filter.Direction, Constants.ContainerDetentionDirection.Export);
			AssertPropertyAffectsMatching(isDetention, value => Filter.OriginPort = value, (string)Filter.OriginPort, "CNSHA");

			if (!isDetention)
			{
				ContractDetention.RCD_Direction = Constants.ContainerDetentionDirection.Export;
				Filter.Direction = Constants.ContainerDetentionDirection.Export;
				AssertFilterPropertyDoesntAffectMatching(isDetention, value => Filter.OriginPort = value, (string)Filter.OriginPort, "CNSHA");

				AssertPropertyAffectsMatching(isDetention, value => Filter.CreditorType = value, (string)Filter.CreditorType, Constants.ContainerPenaltyCreditorType.Codes.CTO);
			}
			else
			{
				AssertFilterPropertyDoesntAffectMatching(isDetention, value => Filter.CreditorType = value, (string)Filter.CreditorType, Constants.ContainerPenaltyCreditorType.Codes.CTO);
			}

			AssertPropertyAffectsMatching(isDetention, value => SetFilterContractNumber(value), "CN0001", "AU0001");
			AssertPropertyAffectsMatching(isDetention, SetFilterContractOrg, ContractOrg, Factory.NewWithValidTestData<OrgHeader>());

			var detentionDateLowerBound = ContractDetention.RCD_StartDateOverride;
			AssertPropertyAffectsMatching(isDetention, value => SetFilterDepartureDateTime(value), detentionDateLowerBound, detentionDateLowerBound.AddDays(-1));

			ContractDetention.RCD_StartDateOverride = ZDate.Empty;
			AssertPropertyAffectsMatching(isDetention, value => SetFilterDepartureDateTime(value), detentionDateLowerBound.AddDays(-1), Contract.RCT_StartDate.AddDays(-1));

			var detentionDateUpperBound = ContractDetention.RCD_EndDateOverride;
			AssertPropertyAffectsMatching(isDetention, value => SetFilterDepartureDateTime(value), detentionDateUpperBound, detentionDateUpperBound.AddDays(1));

			var oldContractUpperBound = Contract.RCT_EndDate;
			ContractDetention.RCD_EndDateOverride = ZDate.Empty;
			AssertPropertyAffectsMatching(isDetention, value => SetFilterDepartureDateTime(value), detentionDateUpperBound.AddDays(1), oldContractUpperBound.AddDays(1));

			Contract.RCT_EndDate = ZDate.Empty;
			AssertPropertyAffectsMatching(isDetention, value => SetFilterDepartureDateTime(value), oldContractUpperBound.AddDays(1), Contract.RCT_StartDate.AddDays(-1));

			foreach (var processType in ValidProcessTypes)
			{
				AssertPropertyAffectsMatching(isDetention, value => Filter.ProcessType = value, (string)Filter.ProcessType, "XXX");
			}

			AssertPropertyAffectsMatching(isDetention, value => Contract.RCT_ContractType = value, ExpectedContractType, "XXX");
		}

		protected void AssertPropertyAffectsMatching<T>(bool isDetention, Action<T> setValue, T matchingValue, T nonMatchingValue)
		{
			var matcher = GetNewMatcher();

			setValue(nonMatchingValue);
			var result = isDetention ? matcher.MatchDetention(Filter) : matcher.MatchStorage(Filter);
			AssertNull("Property did not affect matching.", result);

			setValue(matchingValue);
			result = isDetention ? matcher.MatchDetention(Filter) : matcher.MatchStorage(Filter);
			AssertNotNull("All filters matching, so matcher should return a penalty.", result);
		}

		protected void AssertFilterPropertyDoesntAffectMatching<T>(bool isDetention, Action<T> setFilterValue, T matchingValue, T nonMatchingValue)
		{
			var matcher = GetNewMatcher();

			setFilterValue(nonMatchingValue);
			var result = isDetention ? matcher.MatchDetention(Filter) : matcher.MatchStorage(Filter);
			AssertNotNull("Property should not affect matching.", result);

			setFilterValue(matchingValue);
			result = isDetention ? matcher.MatchDetention(Filter) : matcher.MatchStorage(Filter);
			AssertNotNull("Property should not affect matching.", result);
		}

		#endregion

		#endregion
	}
}
