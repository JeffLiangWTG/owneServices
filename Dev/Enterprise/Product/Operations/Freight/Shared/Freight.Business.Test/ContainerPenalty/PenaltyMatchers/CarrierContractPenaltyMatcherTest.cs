using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CarrierContractPenaltyMatcherTest : ContractPenaltyMatcherTest
	{
		#region Testing Match Order

		public void TestMatchStorageForImport()
		{
			var direction = Constants.ContainerDetentionDirection.Import;
			var processType = Constants.ContainerPenaltyProcessType.Import;
			TestMatchStorage(direction, processType);
		}

		public void TestMatchStorageForExport()
		{
			var direction = Constants.ContainerDetentionDirection.Export;
			var processType = Constants.ContainerPenaltyProcessType.Export;
			TestMatchStorage(direction, processType);
		}

		public void TestMatchStorageForDelivery()
		{
			var direction = Constants.ContainerDetentionDirection.Import;
			var processType = Constants.ContainerPenaltyProcessType.Delivery;
			TestMatchStorage(direction, processType);
		}

		public void TestMatchStorageForPickup()
		{
			var direction = Constants.ContainerDetentionDirection.Export;
			var processType = Constants.ContainerPenaltyProcessType.Pickup;
			TestMatchStorage(direction, processType);
		}

		void TestMatchStorage(string direction, string processType)
		{
			var ratingContract = Factory.New<IRatingContract>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			ratingContract.RCT_OH = carrier.PK;
			ratingContract.RCT_StartDate = ZDate.Today.AddDays(-15);
			ratingContract.RCT_ContractNumber = "CN0001";
			ratingContract.RCT_EndDate = ZDate.Today.AddDays(15);
			ratingContract.RCT_ContractType = "PRO";
			ratingContract.RCT_IsActive = true;
			ratingContract.RCT_TransportMode = "SEA";
			ratingContract.RCT_GS_NKContractOwner = "USR";

			BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, null, direction, 1, ZDate.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, client, direction, 2, ZDate.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, null, direction, 3, ZDate.Empty, ZDate.Empty, "40G", ZString.Empty, ZString.Empty);
			BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, null, direction, 4, ZDate.Empty, ZDate.Empty, ZString.Empty, "AUSYD", ZString.Empty);
			BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, null, direction, 5, ZDate.Empty, ZDate.Empty, ZString.Empty, "AU", ZString.Empty);
			if (direction == Constants.ContainerDetentionDirection.Import)
			{
				BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, null, direction, 6, ZDate.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, "CNSZX");
				BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, null, direction, 7, ZDate.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, "CN");
			}

			BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, null, direction, 8, ZDate.Today.AddDays(-3), ZDate.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			if (direction == Constants.ContainerDetentionDirection.Import)
			{
				BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, client, direction, 9, ZDate.Today.AddDays(-3), ZDate.Today.AddDays(3), "40G", "AUSYD", "CNSZX");
				BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, client, direction, 10, ZDate.Today.AddDays(3), ZDate.Today.AddDays(6), "40G", "AUSYD", "CNSZX");
			}
			else
			{
				BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, client, direction, 9, ZDate.Today.AddDays(-3), ZDate.Today.AddDays(3), "40G", "AUSYD", ZString.Empty);
				BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Storage, client, direction, 10, ZDate.Today.AddDays(3), ZDate.Today.AddDays(6), "40G", "AUSYD", ZString.Empty);
			}

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_CarrierContractNumber = "CN0001";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Today;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsigneePK = client.PK;
			var container = consol.Containers.AddNew();

			Factory.Save();

			var result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
			{
				Container = container,
				CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
				Direction = direction,
				ProcessType = processType,
				Client = client,
				ContainerClass = "40G",
				DetentionPort = "AUSYD",
				OriginPort = "CNSZX"
			});
			AssertEquals((byte)9, result.FreeDays);

			result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
			{
				Container = container,
				CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
				Direction = direction,
				ProcessType = processType,
				Client = client,
				ContainerClass = "60G",
				DetentionPort = "AUSYD",
				OriginPort = "CNSZX"
			});
			AssertEquals((byte)2, result.FreeDays);

			var otherClient = Factory.NewWithValidTestData<OrgHeader>();

			if (processType == Constants.ContainerPenaltyProcessType.Delivery || processType == Constants.ContainerPenaltyProcessType.Pickup)
			{
				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "40G",
					DetentionPort = "AUSYD",
					OriginPort = "CNSZX"
				});
				AssertNull(result);
			}
			else
			{
				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "40G",
					DetentionPort = "AUSYD",
					OriginPort = "CNSZX"
				});
				AssertEquals((byte)3, result.FreeDays);

				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "AUSYD",
					OriginPort = "CNSZX"
				});
				AssertEquals((byte)4, result.FreeDays);

				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "AUMBW",
					OriginPort = "CNSZX"
				});
				AssertEquals((byte)5, result.FreeDays);

				if (direction == Constants.ContainerDetentionDirection.Import)
				{
					result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
					{
						Container = container,
						CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
						Direction = direction,
						ProcessType = processType,
						Client = otherClient,
						ContainerClass = "60G",
						DetentionPort = "NZAKL",
						OriginPort = "CNSZX"
					});
					AssertEquals((byte)6, result.FreeDays);

					result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
					{
						Container = container,
						CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
						Direction = direction,
						ProcessType = processType,
						Client = otherClient,
						ContainerClass = "60G",
						DetentionPort = "NZAKL",
						OriginPort = "CNSHA"
					});
					AssertEquals((byte)7, result.FreeDays);
				}

				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertEquals((byte)8, result.FreeDays);

				consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Today.AddDays(-6);
				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertEquals((byte)1, result.FreeDays);

				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = Factory.NewWithValidTestData<CommonContainer>(),
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Transport,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.CTO,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = Constants.ContainerPenaltyProcessType.Pickup,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = Constants.ContainerPenaltyProcessType.Delivery,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Empty;
				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.Transports.MostInterestingTransport.JW_ATD = ratingContract.RCT_StartDate.AddDays(-2);
				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.Transports.MostInterestingTransport.JW_ATD = ratingContract.RCT_EndDate.AddDays(2);
				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Today.AddDays(-6);
				consol.JK_TransportMode = "AIR";
				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.JK_TransportMode = "SEA";
				result = new CarrierContractPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNotNull(result);
			}
		}

		public void TestMatchDetentionForImport()
		{
			var direction = Constants.ContainerDetentionDirection.Import;
			var processType = Constants.ContainerPenaltyProcessType.Import;
			AssertMatchPenalty(direction, processType, Constants.ContainerPenaltyPenaltyType.Codes.Detention);
		}

		public void TestMatchDetentionForExport()
		{
			var direction = Constants.ContainerDetentionDirection.Export;
			var processType = Constants.ContainerPenaltyProcessType.Export;
			AssertMatchPenalty(direction, processType, Constants.ContainerPenaltyPenaltyType.Codes.Detention);
		}

		public void TestMatchDetentionForDelivery()
		{
			var direction = Constants.ContainerDetentionDirection.Import;
			var processType = Constants.ContainerPenaltyProcessType.Delivery;
			AssertMatchPenalty(direction, processType, Constants.ContainerPenaltyPenaltyType.Codes.Detention);
		}

		public void TestMatchDetentionForPickup()
		{
			var direction = Constants.ContainerDetentionDirection.Export;
			var processType = Constants.ContainerPenaltyProcessType.Pickup;
			AssertMatchPenalty(direction, processType, Constants.ContainerPenaltyPenaltyType.Codes.Detention);
		}

		public void TestMatchDetentionReturnsNullAndDoesNotThrowExceptionWhenDepartureDateIsInvalidSqlDateTime()
		{
			var direction = Constants.ContainerDetentionDirection.Export;
			var processType = Constants.ContainerPenaltyProcessType.Export;
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var ratingContract = CreateRatingContract(carrier);

			BuildContainerDetention(ratingContract, Constants.ContainerPenaltyPenaltyType.Codes.Detention, client, direction, 9, ZDate.Today.AddDays(-3), ZDate.Today.AddDays(3), "40G", "AUSYD", "CNSZX");

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_CarrierContractNumber = "CN0001";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsigneePK = client.PK;
			var container = consol.Containers.AddNew();

			Factory.Save();

			var matchFilter = new ContainerPenaltyMatchFilter
			{
				Container = container,
				CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
				Direction = direction,
				ProcessType = processType,
				Client = client,
				ContainerClass = "40G",
				DetentionPort = "AUSYD",
				OriginPort = "CNSZX"
			};

			consol.Transports.MostInterestingTransport.JW_ATD = SqlDateTime.MinValue.Value.AddMilliseconds(-1);

			IContainerPenaltyMatchResult result = null;

			AssertNoExceptionThrown(() => { result = new CarrierContractPenaltyMatcher().MatchDetention(matchFilter); });
			AssertNull(result);
		}

		public void TestMatchMDDForImport()
		{
			var direction = Constants.ContainerDetentionDirection.Import;
			var processType = Constants.ContainerPenaltyProcessType.Import;
			AssertMatchPenalty(direction, processType, Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
		}

		public void TestMatchMDDForExport()
		{
			var direction = Constants.ContainerDetentionDirection.Export;
			var processType = Constants.ContainerPenaltyProcessType.Export;
			AssertMatchPenalty(direction, processType, Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
		}

		void AssertMatchPenalty(string direction, string processType, string penaltyType)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var ratingContract = CreateRatingContract(carrier);

			BuildContainerDetention(ratingContract, penaltyType, null, direction, 1, ZDate.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			BuildContainerDetention(ratingContract, penaltyType, client, direction, 2, ZDate.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			BuildContainerDetention(ratingContract, penaltyType, null, direction, 3, ZDate.Empty, ZDate.Empty, "40G", ZString.Empty, ZString.Empty);
			BuildContainerDetention(ratingContract, penaltyType, null, direction, 4, ZDate.Empty, ZDate.Empty, ZString.Empty, "AUSYD", ZString.Empty);
			BuildContainerDetention(ratingContract, penaltyType, null, direction, 5, ZDate.Empty, ZDate.Empty, ZString.Empty, "AU", ZString.Empty);
			BuildContainerDetention(ratingContract, penaltyType, null, direction, 6, ZDate.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, "CNSZX");
			BuildContainerDetention(ratingContract, penaltyType, null, direction, 7, ZDate.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, "CN");
			BuildContainerDetention(ratingContract, penaltyType, null, direction, 8, ZDate.Today.AddDays(-3), ZDate.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			BuildContainerDetention(ratingContract, penaltyType, client, direction, 9, ZDate.Today.AddDays(-3), ZDate.Today.AddDays(3), "40G", "AUSYD", "CNSZX");
			BuildContainerDetention(ratingContract, penaltyType, client, direction, 10, ZDate.Today.AddDays(3), ZDate.Today.AddDays(6), "40G", "AUSYD", "CNSZX");

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_CarrierContractNumber = "CN0001";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsigneePK = client.PK;
			var container = consol.Containers.AddNew();

			Factory.Save();

			Func<ContainerPenaltyMatchFilter, IContainerPenaltyMatchResult> matchDetention = filter => new CarrierContractPenaltyMatcher().MatchDetention(filter);
			Func<ContainerPenaltyMatchFilter, IContainerPenaltyMatchResult> matchMDD = filter => new CarrierContractPenaltyMatcher().MatchMDD(filter);
			var matchPenalty = penaltyType == Constants.ContainerPenaltyPenaltyType.Codes.Detention
				? matchDetention : matchMDD;

			var result = matchPenalty(new ContainerPenaltyMatchFilter
			{
				Container = container,
				CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
				Direction = direction,
				ProcessType = processType,
				Client = client,
				ContainerClass = "40G",
				DetentionPort = "AUSYD",
				OriginPort = "CNSZX"
			});
			AssertEquals((byte)9, result.FreeDays);

			result = matchPenalty(new ContainerPenaltyMatchFilter
			{
				Container = container,
				CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
				Direction = direction,
				ProcessType = processType,
				Client = client,
				ContainerClass = "60G",
				DetentionPort = "AUSYD",
				OriginPort = "CNSZX"
			});
			AssertEquals((byte)2, result.FreeDays);

			if (!(processType == ContainerPenaltyProcessType.Delivery || processType == ContainerPenaltyProcessType.Pickup))
			{
				var otherClient = Factory.NewWithValidTestData<OrgHeader>();
				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "40G",
					DetentionPort = "AUSYD",
					OriginPort = "CNSZX"
				});
				AssertEquals((byte)3, result.FreeDays);

				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "AUSYD",
					OriginPort = "CNSZX"
				});
				AssertEquals((byte)4, result.FreeDays);

				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "AUMBW",
					OriginPort = "CNSZX"
				});
				AssertEquals((byte)5, result.FreeDays);

				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "CNSZX"
				});
				AssertEquals((byte)6, result.FreeDays);

				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "CNSHA"
				});
				AssertEquals((byte)7, result.FreeDays);

				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertEquals((byte)8, result.FreeDays);

				consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Today.AddDays(-6);
				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertEquals((byte)1, result.FreeDays);

				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = Factory.NewWithValidTestData<CommonContainer>(),
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = ZString.Empty,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNotNull("Detention only support CAR, CreditorType is meaningless", result);

				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = Constants.ContainerPenaltyProcessType.Pickup,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = Constants.ContainerPenaltyProcessType.Delivery,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Empty;
				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.Transports.MostInterestingTransport.JW_ATD = ratingContract.RCT_StartDate.AddDays(-2);
				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.Transports.MostInterestingTransport.JW_ATD = ratingContract.RCT_EndDate.AddDays(2);
				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Today.AddDays(-6);
				consol.JK_TransportMode = "AIR";
				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNull(result);

				consol.JK_TransportMode = "SEA";
				result = matchPenalty(new ContainerPenaltyMatchFilter
				{
					Container = container,
					CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Direction = direction,
					ProcessType = processType,
					Client = otherClient,
					ContainerClass = "60G",
					DetentionPort = "NZAKL",
					OriginPort = "SGSIN"
				});
				AssertNotNull(result);
			}
		}

		#endregion

		#region Contract Penalty Free Days Defaulting with Multiple Shipments

		public void TestMatchImportPenalty_WithMultipleShipments()
		{
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.DET, ContainerDetentionDirection.Import);
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.STO, ContainerDetentionDirection.Import);
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.MDD, ContainerDetentionDirection.Import);
		}

		public void TestMatchExportPenalty_WithMultipleShipments()
		{
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.DET, ContainerDetentionDirection.Export);
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.STO, ContainerDetentionDirection.Export);
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.MDD, ContainerDetentionDirection.Export);
		}

		void AssertMatchPenalty_WithMultipleShipments(ZString penaltyType, ZString direction)
		{
			var consol = CreateConsolWithMultipleShipments(3);
			var useRegistry = penaltyType == ContainerDetentionPenaltyType.MDD || penaltyType == ContainerDetentionPenaltyType.STO;

			try
			{
				var shipment1 = consol.Shipments[0];
				var shipment2 = consol.Shipments[1];
				var shipment3 = consol.Shipments[2];
				var shipmentClient2 = direction == ContainerDetentionDirection.Export ? shipment2.Consignor : shipment2.Consignee;
				var shipmentClient3 = direction == ContainerDetentionDirection.Export ? shipment3.Consignor : shipment3.Consignee;

				using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
				using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
				using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
				using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
				{
					var orgDetention = consol.ShippingLine.CarrierContainerPenalties.AddNew();
					orgDetention.PD_FreeDays = 5;
					orgDetention.PD_PenaltyType = penaltyType;
					orgDetention.PD_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					orgDetention.PD_Direction = direction;
					orgDetention.PD_OH_Client = shipmentClient2.PK;

					var consolPenalty = direction == ContainerDetentionDirection.Export ? consol.Containers[0].ExportPenalties.AddNew() : consol.Containers[0].ImportPenalties.AddNew();
					consolPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					consolPenalty.CPY_JC_Container = consol.Containers[0].PK;
					consolPenalty.CPY_PenaltyType = penaltyType;
					AssertEquals("Precondition: Consol Penalty Free Days should default to the Carrier Organization Detention", (ZByte)5, consolPenalty.FreeTimeAsDays);

					var ratingContract = CreateRatingContract(consol.ShippingLine);
					BuildContainerDetention(ratingContract, penaltyType, shipmentClient3, direction, 2, ZDate.Empty, ZDate.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
					consol.JK_CarrierContractNumber = "CN0001";

					consolPenalty.CalculateDefaultFreeTimeAsDays();
					AssertEquals("Consol Penalty Free Days should default to the Carrier Contract Detention", (ZByte)2, consolPenalty.FreeTimeAsDays);

					var shipmentPenalty1 = direction == ContainerDetentionDirection.Export ? shipment1.PickupPenalties.AddNew() : shipment1.DeliveryPenalties.AddNew();
					shipmentPenalty1.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					shipmentPenalty1.CPY_JC_Container = consol.Containers[0].PK;
					shipmentPenalty1.CPY_PenaltyType = penaltyType;
					AssertEquals("Shipment Penalty Free Days should default to the Registry", useRegistry ? ZByte.Zero : (ZByte)10, shipmentPenalty1.FreeTimeAsDays);

					var shipmentPenalty2 = direction == ContainerDetentionDirection.Export ? shipment2.PickupPenalties.AddNew() : shipment2.DeliveryPenalties.AddNew();
					shipmentPenalty2.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					shipmentPenalty2.CPY_JC_Container = consol.Containers[0].PK;
					shipmentPenalty2.CPY_PenaltyType = penaltyType;
					AssertEquals("Shipment Penalty Free Days should default to the Carrier Organization Detention", (ZByte)5, shipmentPenalty2.FreeTimeAsDays);

					var shipmentPenalty3 = direction == ContainerDetentionDirection.Export ? shipment3.PickupPenalties.AddNew() : shipment3.DeliveryPenalties.AddNew();
					shipmentPenalty3.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					shipmentPenalty3.CPY_JC_Container = consol.Containers[0].PK;
					shipmentPenalty3.CPY_PenaltyType = penaltyType;
					AssertEquals("Shipment Penalty Free Days should default to the Carrier Contract Detention", (ZByte)2, shipmentPenalty3.FreeTimeAsDays);
				}
			}
			finally
			{
				foreach (CommonShipment shipment in consol.Shipments)
				{
					if (shipment.Job != null)
					{
						shipment.Job.Dispose();
					}
				}
			}
		}

		#endregion

		#region Implementation

		IRatingContractContainerDetention BuildContainerDetention(IRatingContract ratingContract, ZString penaltyType, OrgHeader client, ZString direction, ZByte freeDays, ZDate startDateOverride, ZDate endDateOverride, ZString containerType, ZString detentionPortOrCountry, ZString originPortOrCountry)
		{
			var detention = Factory.New<IRatingContractContainerDetention>();
			detention.RCD_RCT = ratingContract.PK;
			detention.RCD_Direction = direction;
			detention.RCD_PenaltyType = penaltyType;
			detention.RCD_FreeDayType = Constants.ContainerDetentionFreeDayType.CTOAvailable;
			detention.RCD_FreeDays = freeDays;
			detention.RCD_StartDateOverride = startDateOverride;
			detention.RCD_EndDateOverride = endDateOverride;
			detention.RCD_OH_Client = client?.PK ?? ZGuid.Empty;
			detention.RCD_ContainerType = containerType;
			detention.RCD_OriginPortOrCountry = originPortOrCountry;
			detention.RCD_DetentionPortOrCountry = detentionPortOrCountry;
			return detention;
		}

		IRatingContract CreateRatingContract(OrgHeader carrier)
		{
			var ratingContract = Factory.New<IRatingContract>();
			ratingContract.RCT_OH = carrier.PK;
			ratingContract.RCT_StartDate = ZDate.Today.AddDays(-15);
			ratingContract.RCT_ContractNumber = "CN0001";
			ratingContract.RCT_EndDate = ZDate.Today.AddDays(15);
			ratingContract.RCT_ContractType = "PRO";
			ratingContract.RCT_IsActive = true;
			ratingContract.RCT_TransportMode = "SEA";
			ratingContract.RCT_GS_NKContractOwner = "USR";
			return ratingContract;
		}

		protected override ContractPenaltyMatcher GetNewMatcher() => new CarrierContractPenaltyMatcher();

		protected override string ExpectedContractType => Constants.RatingContractTypes.Provider;

		protected override string[] ValidProcessTypes => new string[]
		{
			Constants.ContainerPenaltyProcessType.Import,
			Constants.ContainerPenaltyProcessType.Export
		};

		protected override void SetContractProperties()
		{
			base.SetContractProperties();

			Contract.RCT_TransportMode = Constants.TransportModes.Sea;
		}

		protected override void SetDetentionProperties(string penaltyType)
		{
			base.SetDetentionProperties(penaltyType);

			ContractDetention.RCD_OH_Client = ClientForMatching.PK;
		}

		protected override void SetMatchingFilterProperties()
		{
			base.SetMatchingFilterProperties();

			Filter.Client = ClientForMatching;
			GetConsol(Filter).JK_TransportMode = Constants.TransportModes.Sea;
		}

		OrgHeader ClientForMatching
		{
			get
			{
				if (clientForMatching == null)
				{
					clientForMatching = Factory.NewWithValidTestData<OrgHeader>();
				}

				return clientForMatching;
			}
		}
		OrgHeader clientForMatching;

		protected override void SetupContainerAndRelatedBizos()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<CommonConsol>();
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			var container = consol.Containers.AddNew();
			Filter.Container = container;
		}

		protected override void SetFilterDepartureDateTime(ZDateTime value) => GetConsol(Filter).Transports.MostInterestingTransport.JW_ATD = value;

		protected override void SetFilterContractOrg(OrgHeader value) => GetConsol(Filter).JK_OA_ShippingLineAddress = value.MainAddress.PK;

		protected override void SetFilterContractNumber(ZString value) => GetConsol(Filter).JK_CarrierContractNumber = value;

		protected override void AssertAllFilterPropertiesAffectMatching(bool isDetention)
		{
			base.AssertAllFilterPropertiesAffectMatching(isDetention);

			AssertPropertyAffectsMatching(isDetention, value => Filter.Client = value, ClientForMatching, Factory.NewWithValidTestData<OrgHeader>());
			AssertPropertyAffectsMatching(isDetention, value => GetConsol(Filter).JK_TransportMode = value, Constants.TransportModes.Sea, Constants.TransportModes.Air);
		}

		CommonConsol GetConsol(ContainerPenaltyMatchFilter filter) => ((CommonContainer)filter.Container)?.Consol;

		CommonConsol CreateConsolWithMultipleShipments(int shipmentCount)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			(consol.Transports_Get(0) as Transport).JW_ATD = ZDateTime.Today;

			var container = consol.Containers.AddNew();

			for (int i = 0; i < shipmentCount; i++)
			{
				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_FullName = $"CONSIGNEE {i}";
				consignee.MainAddress.OA_Address1 = $"Consignee Address {i}";
				consignee.OH_IsConsignee = true;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_FullName = $"CONSIGNOR {i}";
				consignor.MainAddress.OA_Address1 = $"Consignor Address {i}";
				consignor.OH_IsConsignor = true;

				var shipment = Factory.New<IForwardingShipment>();
				consol.AddShipment(shipment);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

				(shipment as CommonShipment).CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				(shipment as CommonShipment).ShipmentJobHeader.LocalChargesPK = localClient.PK;

				var packLine = (shipment as CommonShipment).OuterPackLines.AddNew();
				packLine.SetContainer(consol as CommonConsol, container as CommonContainer);
			}

			return consol as CommonConsol;
		}

		#endregion
	}
}
