using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.DataTransfer.Universal.Test;
using Enterprise.Warehouse.Yard.Integration;

namespace Enterprise.Warehouse.Yard.DataTransfer.Test
{
	class YardPickupValidationProviderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestGet_WithValidData_ShouldReturnValidResponse()
		{
			var now = ZDateTime.UtcNow;
			var yardTimeZone = ((IGlbBranch)GlbBranch.CurrentBranch).HomeTimeZone.GetUtcOffsetBasedOnUtc(now.ToDateTime());

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				releaseAdviceDetails: new[]
				{
					(
						releaseNumber: "RELEASE01",
						from: now.AddDays(-2).Date,
						to: now.AddDays(3).Date,
						containers: new[]
						{
							(type: "20GP", quantities: new[] { (1, 0, now.AddDays(1).Date), (2, 0, now.AddDays(2).Date) }),
							(type: "20NOR", quantities: new[] { (3, 1, now.AddDays(2).Date), (4, 2, now.AddDays(2).Date) }),
							(type: "20RE", quantities: new[] { (3, 1, now.AddDays(2).Date), (4, 2, now.AddDays(2).Date) })
						}
					)
				});

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardPickupValidationProvider();
			var request = new YardPickupRequest
			{
				FacilityCode = string.Empty,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "RELEASE01"
			};
			var result = (YardPickupData)dataProvider.Get(request);

			AssertEquals(YardPickupData.ResultEnum.Accept, result.Result);
			AssertEquals(1, result.ReleaseDetails.Count);

			var releaseDetails = result.ReleaseDetails[0];
			AssertEquals("COMPANY2", releaseDetails.ClientCode);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(-2).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, releaseDetails.AvailableDateUtc);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(3).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, releaseDetails.ExpiryDateUtc);
			AssertEquals(3, releaseDetails.Containers.Count);

			var container0 = releaseDetails.Containers[0];
			AssertEquals("22G0", container0.IsoCode);
			AssertEquals(3, container0.TotalQuantity);
			AssertEquals(3, container0.AvailableQuantity);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(1).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, container0.ReadyDateUtc);
			AssertEquals("20GP", container0.Code);
			AssertNotNullOrEmpty("Code should be added to a valid response.", container0.Code);
			AssertNotNullOrEmpty("Description should be added to a valid response.", container0.Description);

			var container1 = releaseDetails.Containers[1];
			AssertEquals("22R0", container1.IsoCode);
			AssertEquals(7, container1.TotalQuantity);
			AssertEquals(4, container1.AvailableQuantity);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(1).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, container0.ReadyDateUtc);
			AssertEquals("20NOR", container1.Code);
			AssertNotNullOrEmpty("Code should be added to a valid response.", container1.Code);
			AssertNotNullOrEmpty("Description should be added to a valid response.", container1.Description);

			var container2 = releaseDetails.Containers[2];
			AssertEquals("22R0", container2.IsoCode);
			AssertEquals(7, container2.TotalQuantity);
			AssertEquals(4, container2.AvailableQuantity);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(1).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, container0.ReadyDateUtc);
			AssertEquals("20RE", container2.Code);
			AssertNotNullOrEmpty("Code should be added to a valid response.", container2.Code);
			AssertNotNullOrEmpty("Description should be added to a valid response.", container2.Description);
		}

		public void TestGet_WithCommunityCodes_ShouldReturnValidResponse()
		{
			var now = ZDateTime.UtcNow;
			var yardTimeZone = ((IGlbBranch)GlbBranch.CurrentBranch).HomeTimeZone.GetUtcOffsetBasedOnUtc(now.ToDateTime());

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", "CC_123"),
				client: ("COMPANY2", "ADDRESS2", "CC_234"),
				releaseAdviceDetails: new[]
				{
					(
						releaseNumber: "RELEASE01",
						from: now.AddDays(-2).Date,
						to: now.AddDays(3).Date,
						containers: new[]
						{
							(type: "20GP", quantities: new[] { (1, 0, now.AddDays(1).Date), (2, 0, now.AddDays(2).Date) }),
							(type: "20FR", quantities: new[] { (3, 1, now.AddDays(2).Date), (4, 2, now.AddDays(2).Date) })
						}
					)
				});

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardPickupValidationProvider();
			var request = new YardPickupRequest
			{
				FacilityCode = "CC_123",
				OrgCode = string.Empty,
				AddressCode = string.Empty,
				ReferenceNumber = "RELEASE01"
			};
			var result = (YardPickupData)dataProvider.Get(request);

			Factory.SaveForTesting();

			AssertEquals(YardPickupData.ResultEnum.Accept, result.Result);
			AssertEquals(1, result.ReleaseDetails.Count);

			var releaseDetails = result.ReleaseDetails[0];
			AssertEquals("CC_234", releaseDetails.ClientCode);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(-2).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, releaseDetails.AvailableDateUtc);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(3).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, releaseDetails.ExpiryDateUtc);
			AssertEquals(2, releaseDetails.Containers.Count);

			var container0 = releaseDetails.Containers[0];
			AssertEquals("22G0", container0.IsoCode);
			AssertEquals(3, container0.TotalQuantity);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(1).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, container0.ReadyDateUtc);

			var container1 = releaseDetails.Containers[1];
			AssertEquals("22P1", container1.IsoCode);
			AssertEquals(7, container1.TotalQuantity);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(1).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, container0.ReadyDateUtc);
		}

		public void TestGet_CannotFindReleaseAdvicesDueToOrgCode_ShouldReturnRejectWithRNF()
		{
			TestCaseCannotFindReleaseAdvices("INVALID", "ADDRESS1", "RELEASE01");
		}

		public void TestGet_CannotFindReleaseAdvicesDueToAddressCode_ShouldReturnRejectWithRNF()
		{
			TestCaseCannotFindReleaseAdvices("COMPANY1", "INVALID", "RELEASE01");
		}

		public void TestGet_CannotFindReleaseAdvicesDueToReleaseNumber_ShouldReturnRejectWithRNF()
		{
			TestCaseCannotFindReleaseAdvices("COMPANY1", "ADDRESS1", "INVALID");
		}

		void TestCaseCannotFindReleaseAdvices(string org, string address, string releaseNumber)
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				releaseAdviceDetails: new[]
				{
					(
						releaseNumber: "RELEASE01",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(type: "20GP", quantities: new[] { (1, 0, now.AddDays(1).Date) }),
						}
					)
				});

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardPickupValidationProvider();
			var request = new YardPickupRequest
			{
				FacilityCode = string.Empty,
				OrgCode = org,
				AddressCode = address,
				ReferenceNumber = releaseNumber
			};
			var result = (YardPickupData)dataProvider.Get(request);

			AssertEquals(YardPickupData.ResultEnum.Reject, result?.Result);
			AssertEquals(YardPickupData.ResponseCode.RNF, result?.MessageCode);
		}

		public void TestGet_WithExpiredReleaseAdvice_ShouldReturnRejectWithRNE()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				releaseAdviceDetails: new[]
				{
					(
						releaseNumber: "RELEASE01",
						from: now.AddDays(-5).Date,
						to: now.AddDays(-2).Date,
						containers: new[]
						{
							(type: "20GP", quantities: new[] { (1, 0, now.AddDays(-3).Date) }),
						}
					)
				});

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardPickupValidationProvider();
			var request = new YardPickupRequest
			{
				FacilityCode = string.Empty,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "RELEASE01"
			};
			var result = (YardPickupData)dataProvider.Get(request);

			AssertEquals(YardPickupData.ResultEnum.Reject, result?.Result);
			AssertEquals(YardPickupData.ResponseCode.RNE, result?.MessageCode);
			AssertNull("ReleaseDetails should be null in an invalid response", result?.ReleaseDetails);
		}

		public void TestGet_NothingIsAvailableToBePickedUp_ShouldReturnRejectWithRNU()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				releaseAdviceDetails: new[]
				{
					(
						releaseNumber: "RELEASE01",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(type: "20GP", quantities: new[] { (1, 1, now.AddDays(1).Date) }),
						}
					)
				});

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardPickupValidationProvider();
			var request = new YardPickupRequest
			{
				FacilityCode = string.Empty,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "RELEASE01"
			};
			var result = (YardPickupData)dataProvider.Get(request);

			AssertEquals(YardPickupData.ResultEnum.Reject, result?.Result);
			AssertEquals(YardPickupData.ResponseCode.RNU, result?.MessageCode);
			AssertNull("ReleaseDetails should be null in an invalid response", result?.ReleaseDetails);
		}

		void SetupTestData(
			 (string name, string address, string communityCode) yard,
			 (string name, string address, string communityCode) client,
			 (string releaseNumber, ZDate from, ZDate to, (string type, (int total, int pickup, ZDate readyDate)[] quantities)[] containers)[] releaseAdviceDetails)
		{
			var warehouseAddress = UniversalTestHelper.CreateOrganization(Factory, yard.name, yard.address);
			warehouseAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, yard.communityCode, string.Empty);
			var containerYard = UniversalTestHelper.CreateWarehouse(warehouseAddress, Factory);
			var clientAddress = UniversalTestHelper.CreateOrganization(Factory, client.name, client.address);
			clientAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, client.communityCode, string.Empty);

			foreach (var (releaseNumber, from, to, containers) in releaseAdviceDetails)
			{
				var releaseAdvice = UniversalTestHelper.CreateReleaseAdvice(Factory, containerYard, releaseNumber, from, to);
				_ = UniversalTestHelper.CreateJobDocAddress(Factory, clientAddress, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, releaseAdvice);

				foreach (var (type, quantities) in containers)
				{
					foreach (var (totalQuantity, pickupQuantity, readyDate) in quantities)
					{
						var line = UniversalTestHelper.CreateReleaseAdviceLine(Factory, releaseAdvice, type, (short)totalQuantity);
						line.YEL_ReadyDate = readyDate;
						if (pickupQuantity > 0)
						{
							var pickup = Factory.NewWithValidTestData<CYDPickup>();
							pickup.YPL_YEL_ReleaseAdviceLine = line.PK;
							pickup.UnitLineItem.YLI_Quantity = (short)pickupQuantity;
						}
					}
				}
			}
		}
	}
}
