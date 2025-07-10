using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.Warehouse.Yard.DataTransfer.Universal.Test;
using Enterprise.Warehouse.Yard.Integration;

namespace Enterprise.Warehouse.Yard.DataTransfer.Test
{
	class YardDropoffValidationProviderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestGet_WithValidData_ShouldReturnValidResponse()
		{
			var now = ZDateTime.UtcNow;
			var yardTimeZone = ((IGlbBranch)GlbBranch.CurrentBranch).HomeTimeZone.GetUtcOffsetBasedOnUtc(now.ToDateTime());

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: true, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: false, isCreateGateOut: false)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "CNT0001",
				IsLaden = false
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Accept, result.Result);
			AssertEquals("CNT0001", result.ContainerNumber);
			AssertEquals("20GP", result.ContainerCode);
			AssertEquals("22G0", result.IsoCode);
			AssertEquals("Twenty foot general purpose", result.ContainerDescription);
			AssertEquals("COMPANY2", result.Owner);
			AssertEquals("ACT0001", result.ReferenceNumber);
			AssertEquals(true, result.IsStoringOrderAvailable);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(-2).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, result.AvailableDateUtc);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(2).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, result.ExpiryDateUtc);
		}

		public void TestGet_WithCommunityCodes_ShouldReturnValidResponse()
		{
			var now = ZDateTime.UtcNow;
			var yardTimeZone = ((IGlbBranch)GlbBranch.CurrentBranch).HomeTimeZone.GetUtcOffsetBasedOnUtc(now.ToDateTime());

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", "CC_123"),
				client: ("COMPANY2", "ADDRESS2", "CC_234"),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: true, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: null
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = "CC_123",
				OrgCode = string.Empty,
				AddressCode = string.Empty,
				ReferenceNumber = "CNT0001",
				IsLaden = false
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Accept, result.Result);
			AssertEquals("CNT0001", result.ContainerNumber);
			AssertEquals("20GP", result.ContainerCode);
			AssertEquals("22G0", result.IsoCode);
			AssertEquals("Twenty foot general purpose", result.ContainerDescription);
			AssertEquals("CC_234", result.Owner);
			AssertEquals("ACT0001", result.ReferenceNumber);
			AssertEquals(true, result.IsStoringOrderAvailable);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(-2).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, result.AvailableDateUtc);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(2).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, result.ExpiryDateUtc);
		}

		public void TestGet_MultipleUnitsFound_ShouldReturnBasedOnEarliestOffHireDate()
		{
			var now = ZDateTime.UtcNow;
			var yardTimeZone = ((IGlbBranch)GlbBranch.CurrentBranch).HomeTimeZone.GetUtcOffsetBasedOnUtc(now.ToDateTime());

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(1).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false),
							(number: "CNT0002", type: "20FR", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					),
					(
						acceptanceNumber: "ACT0002",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20FR", unitType: "GEN", empty: false, offHire: now.AddDays(1).Date, createTime: now, isReject: false),
							(number: "CNT0002", type: "20GP", unitType: "GEN", empty: false, offHire: now.AddDays(1).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: null
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "CNT0001",
				IsLaden = true
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Accept, result.Result);
			AssertEquals("CNT0001", result.ContainerNumber);
			AssertEquals("ACT0002", result.ReferenceNumber);
			AssertEquals("22P1", result.IsoCode);
			AssertEquals("COMPANY2", result.Owner);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(2).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, result.ExpiryDateUtc);
		}

		public void TestGet_MultipleUnitsFoundWithSameOffHireDate_ShouldReturnBasedOnEarliestToDate()
		{
			var now = ZDateTime.UtcNow;
			var yardTimeZone = ((IGlbBranch)GlbBranch.CurrentBranch).HomeTimeZone.GetUtcOffsetBasedOnUtc(now.ToDateTime());

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(1).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false),
							(number: "CNT0002", type: "20FR", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					),
					(
						acceptanceNumber: "ACT0002",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20FR", unitType: "GEN", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false),
							(number: "CNT0002", type: "20GP", unitType: "GEN", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: null
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "CNT0001",
				IsLaden = true
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Accept, result.Result);
			AssertEquals("CNT0001", result.ContainerNumber);
			AssertEquals("ACT0001", result.ReferenceNumber);
			AssertEquals("22G0", result.IsoCode);
			AssertEquals("COMPANY2", result.Owner);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(1).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, result.ExpiryDateUtc);
		}

		public void TestGet_MultipleUnitsFoundWithSameOffHireDateAndToDate_ShouldReturnBasedOnEarliestCreateDate()
		{
			var now = ZDateTime.UtcNow;
			var yardTimeZone = ((IGlbBranch)GlbBranch.CurrentBranch).HomeTimeZone.GetUtcOffsetBasedOnUtc(now.ToDateTime());

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now.AddHours(1), isReject: false),
							(number: "CNT0002", type: "20FR", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					),
					(
						acceptanceNumber: "ACT0002",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20FR", unitType: "GEN", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false),
							(number: "CNT0002", type: "20GP", unitType: "GEN", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: null
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "CNT0001",
				IsLaden = true
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Accept, result.Result);
			AssertEquals("CNT0001", result.ContainerNumber);
			AssertEquals("ACT0002", result.ReferenceNumber);
			AssertEquals("22P1", result.IsoCode);
			AssertEquals("COMPANY2", result.Owner);
			AssertEquals(new System.DateTimeOffset(now.Date.AddDays(2).ToDateTime(), yardTimeZone).ToUniversalTime().DateTime, result.ExpiryDateUtc);
		}

		public void TestGet_CannotFindContainerDueToOrgCode_ShouldReturnRejectWithCNF()
		{
			TestCaseCannotFindFacility("INVALID", "ADDRESS1", "RELEASE01", false);
		}

		public void TestGet_CannotFindContainerDueToAddressCode_ShouldReturnRejectWithCNF()
		{
			TestCaseCannotFindFacility("COMPANY1", "INVALID", "RELEASE01", false);
		}

		public void TestGet_CannotFindContainerDueToContainerNumber_ShouldReturnUnavailableWithCNF()
		{
			TestCaseCannotFindContainer("COMPANY1", "ADDRESS1", "INVALID", false);
		}

		public void TestGet_CannotFindContainerDueToIncorrectStatus_ShouldReturnUnavailableWithCNF()
		{
			TestCaseCannotFindContainer("COMPANY1", "ADDRESS1", "INVALID", false);
		}

		public void TestGet_YardUnitHasBeenRejected_ShouldReturnUnavailableWithCNF()
		{
			var now = ZDateTime.UtcNow;
			var yardTimeZone = ((IGlbBranch)GlbBranch.CurrentBranch).HomeTimeZone.GetUtcOffsetBasedOnUtc(now.ToDateTime());

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: true, offHire: now.AddDays(2).Date, createTime: now, isReject: true)
						}
					)
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: true, isCreateGateOut: false)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "CNT0001",
				IsLaden = false
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Unavailable, result?.Result);
			AssertNullOrEmpty(result.IsoCode);
			AssertNullOrEmpty(result.ReferenceNumber);
			AssertNullOrEmpty(result.MessageCode.ToString());
			AssertEquals("CNT0001", result.ContainerNumber);
		}

		public void TestGet_CannotFindContainer_ShouldReturnUnavailableResult()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("YARD", "ADDRESS1", string.Empty),
				client: ("CLIENT", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACCEPT001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "TEST1234567", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: null
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "YARD",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "TEST7654321",
				IsLaden = true
			};

			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Unavailable, result?.Result);
			AssertNotNullOrEmpty(result.ContainerNumber);
			AssertNullOrEmpty(result.IsoCode);
			AssertNullOrEmpty(result.ReferenceNumber);
			AssertNullOrEmpty(result.MessageCode.ToString());
			AssertEquals("TEST7654321", result.ContainerNumber);
		}

		public void TestGet_ContainerHasActiveReceiveOrderWithTPU_ShouldReturnAcceptResult()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("YARD", "ADDRESS1", string.Empty),
				client: ("CLIENT", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACCEPT001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "TEST1234567", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: false, isCreateGateOut: false)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "YARD",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "TEST1234567",
				IsLaden = true
			};

			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Accept, result?.Result);
			AssertNullOrEmpty(result.MessageCode.ToString());
			AssertNotNullOrEmpty(result.ContainerNumber);
			AssertNotNullOrEmpty(result.IsoCode);
			AssertNotNullOrEmpty(result.ReferenceNumber);
			AssertEquals("TEST1234567", result.ContainerNumber);
			AssertEquals("22G0", result.IsoCode);
			AssertEquals("ACCEPT001", result.ReferenceNumber);
		}

		public void TestGet_ContainerHasActiveReceiveOrderWithoutTPU_ShouldReturnAcceptResult()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("YARD", "ADDRESS1", string.Empty),
				client: ("CLIENT", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACCEPT001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "TEST1234567", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: null
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "YARD",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "TEST1234567",
				IsLaden = true
			};

			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Accept, result?.Result);
			AssertNullOrEmpty(result.MessageCode.ToString());
			AssertNotNullOrEmpty(result.ContainerNumber);
			AssertNotNullOrEmpty(result.IsoCode);
			AssertNotNullOrEmpty(result.ReferenceNumber);
			AssertEquals("TEST1234567", result.ContainerNumber);
			AssertEquals("22G0", result.IsoCode);
			AssertEquals("ACCEPT001", result.ReferenceNumber);
		}

		public void TestGet_ContainerHasInactiveReceiveOrder_ShouldReturnUnavailableResult()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("YARD", "ADDRESS1", string.Empty),
				client: ("CLIENT", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACCEPT001",
						from: now.AddDays(-5).Date,
						to: now.AddDays(-3).Date,
						containers: new[]
						{
							(number: "TEST1234567", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: false, isCreateGateOut: false)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "YARD",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "TEST1234567",
				IsLaden = true
			};

			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Unavailable, result?.Result);
			AssertNotNullOrEmpty(result.ContainerNumber);
			AssertNullOrEmpty(result.IsoCode);
			AssertNullOrEmpty(result.ReferenceNumber);
			AssertNullOrEmpty(result.MessageCode.ToString());
			AssertEquals("TEST1234567", result.ContainerNumber);
		}

		public void TestGet_ContainerAlreadyGatedIn_WithInactivePRA_ShouldReturnRejectWithCGI()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("YARD", "ADDRESS1", string.Empty),
				client: ("CLIENT", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACCEPT001",
						from: now.AddDays(-3).Date,
						to: now.AddDays(-2).Date,
						containers: new[]
						{
							(number: "TEST1234567", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(-2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: true, isCreateGateOut: false)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "YARD",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "TEST1234567",
				IsLaden = true
			};

			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Reject, result?.Result);
			AssertEquals(YardDropoffData.ResponseCode.CGI, result?.MessageCode);
			AssertEquals(YardDropoffData.ErrorMessages[YardDropoffData.ResponseCode.CGI], result.Message);
		}

		public void TestGet_ContainerAlreadyGatedIn_WithActivePRA_ShouldReturnRejectWithCGI()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("YARD", "ADDRESS1", string.Empty),
				client: ("CLIENT", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACCEPT001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "TEST1234567", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: true, isCreateGateOut: false)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "YARD",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "TEST1234567",
				IsLaden = true
			};

			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Reject, result?.Result);
			AssertEquals(YardDropoffData.ResponseCode.CGI, result?.MessageCode);
			AssertEquals(YardDropoffData.ErrorMessages[YardDropoffData.ResponseCode.CGI], result.Message);
		}

		public void TestGet_MultipleActiveAndInactivePRAs_ThenReturnEarliestActivePRA()
		{
			var now = ZDateTime.UtcNow;
			var latest = now.AddDays(1).Date.ToZDateTime();

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: true, offHire: now.AddDays(2).Date, createTime: latest, isReject: false)
						}
					),
					(
						acceptanceNumber: "ACT0002",
						from: now.AddDays(3).Date,
						to: now.AddDays(3).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: true, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					),
					(
						acceptanceNumber: "ACT0003",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "GEN", empty: true, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					),
					(
						acceptanceNumber: "ACT0004",
						from: now.AddDays(3).Date,
						to: now.AddDays(3).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "GEN", empty: true, offHire: now.AddDays(2).Date, createTime: latest, isReject: false)
						}
					)
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: false, isCreateGateOut: false)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "CNT0001",
				IsLaden = false
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Accept, result.Result);
			AssertEquals("ACT0003", result.ReferenceNumber);
		}

		public void TestGet_NoActivePRAButHasMultipleInactivePRAs_ShouldReturnUnavailableResult()
		{
			var now = ZDateTime.UtcNow;
			var latest = now.AddDays(1).Date.ToZDateTime();
			var oldest = now.AddDays(-5).Date.ToZDateTime();

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(3).Date,
						to: now.AddDays(3).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: true, offHire: now.AddDays(3).Date, createTime: latest, isReject: false)
						}
					),
					(
						acceptanceNumber: "ACT0002",
						from: now.AddDays(4).Date,
						to: now.AddDays(4).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: true, offHire: now.AddDays(4).Date, createTime: now, isReject: false)
						}
					),
					(
						acceptanceNumber: "ACT0003",
						from: now.AddDays(3).Date,
						to: now.AddDays(3).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "GEN", empty: true, offHire: now.AddDays(3).Date, createTime: now, isReject: false)
						}
					),
					(
						acceptanceNumber: "ACT0004",
						from: now.AddDays(-5).Date,
						to: now.AddDays(-5).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: true, offHire: now.AddDays(-5).Date, createTime: oldest, isReject: false)
						}
					)
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: false, isCreateGateOut: false)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "CNT0001",
				IsLaden = false
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Unavailable, result?.Result);
			AssertNotNullOrEmpty(result.ContainerNumber);
			AssertNullOrEmpty(result.IsoCode);
			AssertNullOrEmpty(result.ReferenceNumber);
			AssertNullOrEmpty(result.MessageCode.ToString());
		}

		public void TestGet_ContainerAlreadyGatedOut_NoActivePRA_ShouldReturnUnavailableResult()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("YARD", "ADDRESS1", string.Empty),
				client: ("CLIENT", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACCEPT001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "TEST1234567", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: true, isCreateGateOut: true)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "YARD",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "TEST1234567",
				IsLaden = true
			};

			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Unavailable, result?.Result);
			AssertNotNullOrEmpty(result.ContainerNumber);
			AssertNullOrEmpty(result.IsoCode);
			AssertNullOrEmpty(result.ReferenceNumber);
			AssertNullOrEmpty(result.MessageCode.ToString());
		}

		public void TestGet_ContainerAlreadyGatedOut_HasActivePRA_ShouldReturnAcceptResult()
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("YARD", "ADDRESS1", string.Empty),
				client: ("CLIENT", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACCEPT001",
						from: now.AddDays(-3).Date,
						to: now.AddDays(-1).Date,
						containers: new[]
						{
							(number: "TEST1234567", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(-1).Date, createTime: now, isReject: false)
						}
					),
				},
				transportationUnitDetails: (transportationReference: "REF001", isCreateGateIn: true, isCreateGateOut: true)
			);

			SetupTestData(
				yard: ("YARD", "ADDRESS1", string.Empty),
				client: ("CLIENT", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACCEPT002",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "TEST1234567", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					),
				},
				transportationUnitDetails: (transportationReference: "REF002", isCreateGateIn: false, isCreateGateOut: false)
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = "YARD",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "TEST1234567",
				IsLaden = true
			};

			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Accept, result.Result);
			AssertEquals("TEST1234567", result.ContainerNumber);
			AssertEquals("ACCEPT002", result.ReferenceNumber);
			AssertEquals("20GP", result.ContainerCode);
			AssertEquals("22G0", result.IsoCode);
			AssertEquals("Twenty foot general purpose", result.ContainerDescription);
			AssertEquals("CLIENT", result.Owner);
			AssertEquals(true, result.IsStoringOrderAvailable);
		}

		void TestCaseCannotFindContainer(string org, string address, string containerNumber, bool empty)
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP", unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: null
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = org,
				AddressCode = address,
				ReferenceNumber = containerNumber,
				IsLaden = !empty
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Unavailable, result?.Result);
			AssertNullOrEmpty(result?.MessageCode.ToString());
		}

		void TestCaseCannotFindFacility(string org, string address, string containerNumber, bool empty)
		{
			var now = ZDateTime.UtcNow;

			SetupTestData(
				yard: ("COMPANY1", "ADDRESS1", string.Empty),
				client: ("COMPANY2", "ADDRESS2", string.Empty),
				receiveAdviceDetails: new[]
				{
					(
						acceptanceNumber: "ACT0001",
						from: now.AddDays(-2).Date,
						to: now.AddDays(2).Date,
						containers: new[]
						{
							(number: "CNT0001", type: "20GP",unitType: "CNT", empty: false, offHire: now.AddDays(2).Date, createTime: now, isReject: false)
						}
					)
				},
				transportationUnitDetails: null
			);

			Factory.SaveForTesting();

			IYardValidationProvider dataProvider = new YardDropoffValidationProvider();
			var request = new YardDropoffRequest()
			{
				FacilityCode = null,
				OrgCode = org,
				AddressCode = address,
				ReferenceNumber = containerNumber,
				IsLaden = !empty
			};
			var result = (YardDropoffData)dataProvider.Get(request);

			AssertEquals(YardDropoffData.ResultEnum.Reject, result?.Result);
			AssertEquals(YardDropoffData.ResponseCode.FNF, result?.MessageCode);
			AssertEquals(YardDropoffData.ErrorMessages[YardDropoffData.ResponseCode.FNF], result.Message);
		}

		void SetupTestData(
			(string name, string address, string communityCode) yard,
			(string name, string address, string communityCode) client,
			(string acceptanceNumber, ZDate from, ZDate to, (string number, string type, string unitType, bool empty, ZDate offHire, ZDateTime createTime, bool isReject)[] containers)[] receiveAdviceDetails,
			(string transportationReference, bool isCreateGateIn, bool isCreateGateOut)? transportationUnitDetails
			)
		{
			var helper = new CYDYardTestHelper(Factory.BOFactory);
			var warehouseAddress = UniversalTestHelper.GetOrganization(Factory, yard.name, yard.address) ?? UniversalTestHelper.CreateOrganization(Factory, yard.name, yard.address);
			if (!warehouseAddress.CustomsCodes.GetOrgCusCodeTypeCollectionForRegNoAndCountry(yard.communityCode, string.Empty).Any())
			{
				warehouseAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, yard.communityCode, string.Empty);
			}
			var containerYard = UniversalTestHelper.GetWarehouse(warehouseAddress, Factory) ?? UniversalTestHelper.CreateWarehouse(warehouseAddress, Factory);
			var clientAddress = UniversalTestHelper.GetOrganization(Factory, client.name, client.address) ?? UniversalTestHelper.CreateOrganization(Factory, client.name, client.address);
			clientAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, client.communityCode, string.Empty);

			CYDTransportationUnit transportationUnitForDelivery = null;
			CYDTransportationUnit transportationUnitForPickup = null;
			if (transportationUnitDetails is not null)
			{
				transportationUnitForDelivery = UniversalTestHelper.CreateTransportationUnit(Factory, containerYard, transportationUnitDetails.Value.transportationReference);
				if (transportationUnitDetails.Value.isCreateGateIn)
				{
					var row = helper.CreateRowAndGenerateLocations(containerYard, "Dock1", 2, 2);
					var location = row.Locations[0];
					helper.GateInTransportationUnit(transportationUnitForDelivery, new ZDateTimeOffset(2024, 9, 3, 10, 30, 0), location);
				}
				if (transportationUnitDetails.Value.isCreateGateOut)
				{
					transportationUnitForPickup = UniversalTestHelper.CreateTransportationUnit(Factory, containerYard, transportationUnitDetails.Value.transportationReference);
					var row = helper.CreateRowAndGenerateLocations(containerYard, "Dock2", 2, 2);
					var location = row.Locations[0];
					helper.GateInTransportationUnit(transportationUnitForPickup, new ZDateTimeOffset(2024, 9, 5, 10, 30, 0), location);
					helper.GateOutTransportationUnit(transportationUnitForPickup, new ZDateTimeOffset(2024, 9, 6, 10, 30, 0));
				}
			}

			foreach (var (acceptanceNumber, from, to, containers) in receiveAdviceDetails)
			{
				// release advice
				var receiveAdvice = UniversalTestHelper.CreateReceiveAdvice(Factory, containerYard, acceptanceNumber, from, to);
				_ = UniversalTestHelper.CreateJobDocAddress(Factory, clientAddress, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, receiveAdvice);

				foreach (var (number, type, unitType, empty, offHire, createTime, isReject) in containers)
				{
					var receiveAdviceLine = UniversalTestHelper.CreateReceiveAdviceLine(Factory, receiveAdvice, type);
					receiveAdviceLine.UnitLineItem.YLI_IsEmpty = empty;
					receiveAdviceLine.UnitLineItem.YLI_Type = unitType;
					receiveAdviceLine.YRL_OffHireDate = offHire;
					var yardUnit = UniversalTestHelper.CreateYardUnitState(Factory, containerYard, receiveAdviceLine, number);
					yardUnit.YUS_SystemCreateTimeUtc = createTime;

					if (transportationUnitForDelivery is not null)
					{
						yardUnit.YUS_YTU_ReceiveTransportationUnit = transportationUnitForDelivery.PK;

						var delivery = UniversalTestHelper.CreateDelivery(Factory, receiveAdviceLine, transportationUnitForDelivery, yardUnit, type);
						delivery.YDL_IsReject = isReject;
					}

					if (transportationUnitForPickup is not null)
					{
						yardUnit.YUS_YTU_DispatchTransportationUnit = transportationUnitForPickup.PK;
					}
				}
			}
		}
	}
}
