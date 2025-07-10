using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerValidationTest : BaseFreightTest
	{
		public void TestIsNonOperativeReefer()
		{
			var consol = Factory.New<CommonConsol>();
			var container = Factory.New<CommonContainer>();
			container.JC_JK = consol.PK;
			consol.JK_RequiresTemperatureControl = true;
			container.JC_IsNonOperativeReefer = true;
			AssertHasError(container.JC_IsNonOperativeReeferInfo, "The Consolidation is Pre-Allocated as Temperature Controlled; container cannot be Non-Operating Reefer.");

			consol.JK_RequiresTemperatureControl = false;
			container.JC_IsNonOperativeReefer = true;
			AssertNoError(container.JC_IsNonOperativeReeferInfo, "The Consolidation is Pre-Allocated as Temperature Controlled; container cannot be Non-Operating Reefer.");

			consol.JK_RequiresTemperatureControl = false;
			container.JC_IsNonOperativeReefer = true;
			AssertNoError(container.JC_IsNonOperativeReeferInfo, "The Consolidation is Pre-Allocated as Temperature Controlled; container cannot be Non-Operating Reefer.");

			consol.JK_RequiresTemperatureControl = false;
			container.JC_IsNonOperativeReefer = false;
			AssertNoError(container.JC_IsNonOperativeReeferInfo, "The Consolidation is Pre-Allocated as Temperature Controlled; container cannot be Non-Operating Reefer.");
		}

		public void TestSealNumbers()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_SealParty = string.Empty;
			container.JC_AdditionalSealParty = string.Empty;
			container.JC_Additional2SealParty = string.Empty;

			AssertNoErrors(container.JC_SealNumInfo);
			AssertNoErrors(container.JC_AdditionalSealNumInfo);
			AssertNoErrors(container.JC_Additional2SealNumInfo);

			container.JC_SealParty = ContainerSealParties.Codes.CarrierShippingLine;
			container.JC_AdditionalSealParty = ContainerSealParties.Codes.ConsignorShipper;
			container.JC_Additional2SealParty = ContainerSealParties.Codes.Customs;

			AssertHasError(container.JC_SealNumInfo, "Please enter a Seal Number.");
			AssertHasError(container.JC_AdditionalSealNumInfo, "Please enter a Second Seal Number.");
			AssertHasError(container.JC_Additional2SealNumInfo, "Please enter a Third Seal Number.");

			container.JC_SealNum = "S000";
			container.JC_AdditionalSealNum = "S001";
			container.JC_Additional2SealNum = "S002";

			AssertNoErrors(container.JC_SealNumInfo);
			AssertNoErrors(container.JC_AdditionalSealNumInfo);
			AssertNoErrors(container.JC_Additional2SealNumInfo);
		}

		public void TestSealParties()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_SealParty = string.Empty;
			container.JC_AdditionalSealParty = string.Empty;
			container.JC_Additional2SealParty = string.Empty;

			AssertNoErrors(container.JC_SealPartyInfo);
			AssertNoErrors(container.JC_AdditionalSealPartyInfo);
			AssertNoErrors(container.JC_Additional2SealPartyInfo);

			container.JC_SealParty = ContainerSealParties.Codes.CarrierShippingLine;
			container.JC_AdditionalSealParty = ContainerSealParties.Codes.ConsignorShipper;
			container.JC_Additional2SealParty = ContainerSealParties.Codes.Customs;

			AssertNoErrors(container.JC_SealPartyInfo);
			AssertNoErrors(container.JC_AdditionalSealPartyInfo);
			AssertNoErrors(container.JC_Additional2SealPartyInfo);

			container.JC_SealParty = "XXX";
			container.JC_AdditionalSealParty = "XXX";
			container.JC_Additional2SealParty = "XXX";

			AssertHasError(container.JC_SealPartyInfo, "Enter a valid Sealed By.");
			AssertHasError(container.JC_AdditionalSealPartyInfo, "Enter a valid Second Sealed By.");
			AssertHasError(container.JC_Additional2SealPartyInfo, "Enter a valid Third Sealed By.");
		}

		public void TestJC_EmptyReturnedByVsDefault()
		{
			var now = ZDateTime.Now;

			var strategy = new Mock<IContainerDefaultingStrategy>(MockBehavior.Strict);
			var provider = new Mock<Converter<CommonContainer, IContainerDefaultingStrategy>>(MockBehavior.Strict);

			var container = Factory.New<DummyCommonContainer>();
			container.JC_EmptyReturnedBy = now;
			container.DefaultingStrategyProvider = provider.Object;

			provider
				.SetupSequence(m => m(container))
				.Returns((IContainerDefaultingStrategy)null)
				.Returns(strategy.Object)
				.Returns(strategy.Object)
				.Returns(strategy.Object);

			container.Validation.ValidateJC_EmptyReturnedBy();
			AssertNoNotifications(container.JC_EmptyReturnedByInfo);

			strategy
				.SetupSequence(m => m.CalculateRequiredBy())
				.Returns((ZDateTime.Empty, ZDateTime.Empty, ZString.Empty))
				.Returns((now.AddDays(1), now, ContainerDetentionFreeDayType.CTOAvailable))
				.Returns((now.Date.ToDateTime(), now.Date, ContainerDetentionFreeDayType.CTOAvailable));

			container.Validation.ValidateJC_EmptyReturnedBy();
			AssertNoNotifications(container.JC_EmptyReturnedByInfo);
			container.Validation.ValidateJC_EmptyReturnedBy();
			AssertHasWarning(container.JC_EmptyReturnedByInfo,
				$"The availability date and applicable detention free days indicate that this should be '{now.AddDays(1).ToShortDateString()}'.");
			container.Validation.ValidateJC_EmptyReturnedBy();
			AssertNoNotifications(container.JC_EmptyReturnedByInfo);

			provider.VerifyAll();
			strategy.VerifyAll();
		}

		public void TestValidateJC_OA_ArrivalContainerYardAddressIsValidZGuid()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_OA_ArrivalContainerYardAddress = ZGuid.Invalid;
			AssertHasError(container.JC_OA_ArrivalContainerYardAddressInfo, "Enter a valid " + container.JC_OA_ArrivalContainerYardAddressInfo.HumanReadableName + ".");
		}

		public void TestValidateJC_OA_DepartureContainerYardAddressIsValidZGuid()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_OA_DepartureContainerYardAddress = ZGuid.Invalid;
			AssertHasError(container.JC_OA_DepartureContainerYardAddressInfo, "Enter a valid " + container.JC_OA_DepartureContainerYardAddressInfo.HumanReadableName + ".");
		}

		public void TestCheckJC_RC()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_RC = ZGuid.Empty;
			AssertHasErrorContaining(container.JC_RCInfo, MandatoryValidation.MustBeEntered);

			RefContainer containerRef1 = Factory.New<RefContainer>();
			containerRef1.RC_Code = "20GM";
			containerRef1.RC_CubicCapacity = 16m;

			container.JC_RC = containerRef1.PK;
			AssertNoErrorContaining(container.JC_RCInfo, MandatoryValidation.MustBeEntered);

			container.StandAloneCustomsContainer = true;
			container.CreatedFromCusContainer = true;
			container.JC_RC = ZGuid.Empty;
			AssertHasWarningContaining(container.JC_RCInfo, MandatoryValidation.YouHaveNotEntered);

			container.JC_RC = containerRef1.PK;
			AssertNoWarningContaining(container.JC_RCInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJC_ChecksContainerTypeForAirConsolidations()
		{
			// Prerequisites
			var seaContainerType = Factory.New<RefContainer>();
			seaContainerType.RC_Code = "RCSEA1";
			seaContainerType.RC_Description = "Sea Container";
			seaContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;
			seaContainerType.RC_ContainerType = "DRY";

			var roadContainerType = Factory.New<RefContainer>();
			roadContainerType.RC_Code = "RCROA1";
			roadContainerType.RC_Description = "Road Container";
			roadContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Road;
			roadContainerType.RC_ContainerType = "DRY";

			var airContainerType = Factory.New<RefContainer>();
			airContainerType.RC_Code = "RCAIR1";
			airContainerType.RC_Description = "Air Container";
			airContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Air;
			airContainerType.RC_ContainerType = "DRY";

			var testCases = new[]
			{
				new
				{
					ContainerMode = ContainerModes.ULD,
					ContainerType = airContainerType,
					ExpectedError = (string)null
				},
				new
				{
					ContainerMode = ContainerModes.ULD,
					ContainerType = roadContainerType,
					ExpectedError = "Your container mode is Air, so you must choose an Air ULD container."
				},
				new
				{
					ContainerMode = ContainerModes.ULD,
					ContainerType = seaContainerType,
					ExpectedError = "Your container mode is Air, so you must choose an Air ULD container."
				},
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					// Arrange
					CommonContainer container = Factory.New<CommonConsol>()
						.WithTransport(TransportModes.Air)
						.WithContainerMode(testCase.ContainerMode)
						.AddContainer(containerMode: testCase.ContainerMode, containerType: testCase.ContainerType.RC_Code);

					// Act
					var containerTypeInfo = container.JC_RCInfo;

					// Assert
					if (testCase.ExpectedError == null)
					{
						var message =
							$"Container with Transport Mode {testCase.ContainerType.RC_ShippingMode} must not trigger validation error for Container Mode {testCase.ContainerMode}";
						AssertNoErrors(message, containerTypeInfo);
					}
					else
					{
						var message =
							$"Container with Transport Mode {testCase.ContainerType.RC_ShippingMode} must trigger validation error for Container Mode {testCase.ContainerMode}";
						AssertHasError(message, containerTypeInfo, testCase.ExpectedError);
					}
				}
			});
		}

		public void TestCheckJC_ChecksContainerTypeForSeaAndRoadConsolidations()
		{
			// Prerequisites
			var seaContainerType = Factory.New<RefContainer>();
			seaContainerType.RC_Code = "RCSEA1";
			seaContainerType.RC_Description = "Sea Container";
			seaContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;
			seaContainerType.RC_ContainerType = "DRY";

			var roadContainerType = Factory.New<RefContainer>();
			roadContainerType.RC_Code = "RCROA1";
			roadContainerType.RC_Description = "Road Container";
			roadContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Road;
			roadContainerType.RC_ContainerType = "DRY";

			var airContainerType = Factory.New<RefContainer>();
			airContainerType.RC_Code = "RCAIR1";
			airContainerType.RC_Description = "Air Container";
			airContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Air;
			airContainerType.RC_ContainerType = "DRY";

			var seaContainerModes = new[] { ContainerModes.FCL, ContainerModes.LCL, ContainerModes.Groupage, ContainerModes.BuyersConsol };
			var roadContainerModes = new[] { ContainerModes.FTL, ContainerModes.LTL };
			var seaAndRoadContainerModes = seaContainerModes.Concat(roadContainerModes);

			var testCases = seaAndRoadContainerModes
				.SelectMany(containerMode => new[] { seaContainerType, roadContainerType },
					(containerMode, containerType) => new
					{
						ContainerMode = containerMode,
						ContainerType = containerType,
						ExpectedError = (string)null
					})
				.Concat(roadContainerModes.Select(containerMode =>
					new
					{
						ContainerMode = containerMode,
						ContainerType = airContainerType,
						ExpectedError = (string)null
					}
				))
				.Concat(seaContainerModes.Select(containerMode => new
				{
					ContainerMode = containerMode,
					ContainerType = airContainerType,
					ExpectedError = "Your container mode is Sea, so you must choose a Sea FCL container."
				}));

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					// Arrange
					CommonContainer container = Factory.New<CommonConsol>()
						.WithTransport(TransportModes.Road)
						.WithContainerMode(testCase.ContainerMode)
						.AddContainer(containerMode: testCase.ContainerMode, containerType: testCase.ContainerType.RC_Code);

					// Act
					var containerTypeInfo = container.JC_RCInfo;

					// Assert
					if (testCase.ExpectedError == null)
					{
						var message =
							$"Container with Transport Mode {testCase.ContainerType.RC_ShippingMode} must not trigger validation error for Container Mode {testCase.ContainerMode}";
						AssertNoErrors(message, containerTypeInfo);
					}
					else
					{
						var message =
							$"Container with Transport Mode {testCase.ContainerType.RC_ShippingMode} must trigger validation error for Container Mode {testCase.ContainerMode}";
						AssertHasError(message, containerTypeInfo, testCase.ExpectedError);
					}
				}
			});
		}

		public void TestCheckJC_GrossWeightUQ()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_GrossWeightUQ = "1";
			AssertHasErrors("Invalid weight unit", container.JC_GrossWeightUQInfo);
			container.JC_GrossWeightUQ = Weight.Kilograms;
			AssertNoErrors("Valid weight unit", container.JC_GrossWeightUQInfo);
		}

		public void TestValidateJC_RC()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			container.JC_ContainerMode = ContainerModes.Bulk;
			container.JC_RC = ZGuid.Empty;
			AssertNoErrors("ContainerMode is bulk, not expecting errors", container.JC_RCInfo);

			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_RC = ZGuid.Empty;
			AssertHasErrors("JC_RC is empty, expecting errors.", container.JC_RCInfo);

			container.JC_RC = RC_20GP_PK;
			AssertNoNotifications("JC_RC is not empty, not expecting errors.", container.JC_RCInfo);
		}

		public void TestValidateJC_RC_WithSupplierBookingAttachedAndRegistryEnabled()
		{
			AssertErrorIfNonEditableSupplierBookingAttachedAndRegistryEnabled(
				(container) => container.JC_RCInfo,
				(container) => { container.JC_RC = RC_20GP_PK; },
				(container) => { container.JC_RC = RC_40GP_PK; },
				(container) => container.Validation.ContainerAlreadyAllocatedErrorMessage
			);
		}

		public void TestValidateJC_RC_WithSupplierBookingAttachedAndRegistryDisabled()
		{
			AssertNoErrorIfSupplierBookingAttachedAndRegistryDisabled(
				(container) => container.JC_RCInfo,
				(container) => { container.JC_RC = RC_20GP_PK; },
				(container) => { container.JC_RC = RC_40GP_PK; }
			);
		}

		public void TestValidateJC_ContainerNum()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "1234567890123";
			AssertHasErrors("Container Number cant have more than 12 characters", container.JC_ContainerNumInfo);

			container.JC_ContainerNum = "123456123456";
			AssertNoErrors("Container number does not have more than 12 characters, no errors expected", container.JC_ContainerNumInfo);

			container.JC_ContainerNum = ZString.Empty;
			AssertNoErrors("Container number can be empty, not expecting errors", container.JC_ContainerNumInfo);

			container.JC_ContainerMode = ContainerModes.ULD;
			container.JC_ContainerNum = "BLAHHH";
			AssertHasWarning(container.JC_ContainerNumInfo, "This is not a valid ULD number.");

			container.JC_ContainerNum = "AAA1234500";
			AssertHasWarning(container.JC_ContainerNumInfo, "This is not a valid ULD number.");

			container.JC_ContainerNum = "ABCD1234XX";
			AssertNoWarning(container.JC_ContainerNumInfo, "This is not a valid ULD number.");

			container.JC_ContainerNum = "A123123X1";
			AssertNoWarning(container.JC_ContainerNumInfo, "This is not a valid ULD number.");
		}

		public void TestValidateJC_ContainerNum_ShouldAllowEmptyContainerNum_WhenLoadListLinesAreAttached_AndOriginalValueIsEmpty_CY()
		{
			AssertValidateJC_ContainerNum_EmptyContainerNum_LoadListLinesAttached(string.Empty, ContainerLoadListHeaderLoadMode.ContainerYard);
		}

		public void TestValidateJC_ContainerNum_ShouldNotAllowEmptyContainerNum_WhenLoadListLinesAreAttached_AndOriginalValueIsNotEmpty_CY()
		{
			AssertValidateJC_ContainerNum_EmptyContainerNum_LoadListLinesAttached("123456123456", ContainerLoadListHeaderLoadMode.ContainerYard);
		}

		public void TestValidateJC_ContainerNum_ShouldAllowEmptyContainerNum_WhenLoadListLinesAreAttached_AndOriginalValueIsEmpty_CFS()
		{
			AssertValidateJC_ContainerNum_EmptyContainerNum_LoadListLinesAttached(string.Empty, ContainerLoadListHeaderLoadMode.ContainerFreightStation);
		}

		public void TestValidateJC_ContainerNum_ShouldAllowEmptyContainerNum_WhenLoadListLinesAreAttached_AndOriginalValueIsNotEmpty_CFS()
		{
			AssertValidateJC_ContainerNum_EmptyContainerNum_LoadListLinesAttached("123456123456", ContainerLoadListHeaderLoadMode.ContainerFreightStation);
		}

		void AssertValidateJC_ContainerNum_EmptyContainerNum_LoadListLinesAttached(string originalValue, string loadMode)
		{
			var booking = CreateSupplierBooking("JSB001", SupplierBookingStatus.Planned) as BusinessObject;

			var containerLoadList = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonContainerLoadList)));
			containerLoadList["CLH_LoadListId"] = "CLL001";
			containerLoadList["CLH_LoadMode"] = loadMode;

			var container = Factory.New<DummyContainerWithAttachedContainerLoadList>();
			container.JC_ContainerNum = originalValue;
			if (loadMode == ContainerLoadListHeaderLoadMode.ContainerYard)
			{
				container.JC_JSB_SupplierBooking = booking.PK;
			}
			else
			{
				container.JC_CLH_LoadListPlan = containerLoadList.PK;
			}

			Factory.Save();

			container.JC_ContainerNum = ZString.Empty;

			if (!originalValue.IsNullOrEmpty() && loadMode == ContainerLoadListHeaderLoadMode.ContainerYard)
			{
				AssertHasErrorContaining(container.JC_ContainerNumInfo, "The container number cannot be set to empty if a container number exists and the container is associated with a Supplier Booking and the container has 'Container Load List Lines'.");
			}
			else
			{
				AssertNoErrors("Container number can be empty when the original value is empty or when the load mode is CFS", container.JC_ContainerNumInfo);
			}
		}

		public void TestValidateJC_ContainerNum_ShouldAllowEmptyContainerNum_WhenNoLoadListLinesAttached_AndOriginalValueIsEmpty_CY()
		{
			AssertValidateJC_ContainerNum_EmptyContainerNum_NoLoadListLinesAttached(string.Empty, ContainerLoadListHeaderLoadMode.ContainerYard);
		}

		public void TestValidateJC_ContainerNum_ShouldAllowEmptyContainerNum_WhenNoLoadListLinesAttached_AndOriginalValueIsNotEmpty_CY()
		{
			AssertValidateJC_ContainerNum_EmptyContainerNum_NoLoadListLinesAttached("123456123456", ContainerLoadListHeaderLoadMode.ContainerYard);
		}

		public void TestValidateJC_ContainerNum_ShouldAllowEmptyContainerNum_WhenNoLoadListLinesAttached_AndOriginalValueIsEmpty_CFS()
		{
			AssertValidateJC_ContainerNum_EmptyContainerNum_NoLoadListLinesAttached(string.Empty, ContainerLoadListHeaderLoadMode.ContainerFreightStation);
		}

		public void TestValidateJC_ContainerNum_ShouldAllowEmptyContainerNum_WhenNoLoadListLinesAttached_AndOriginalValueIsNotEmpty_CFS()
		{
			AssertValidateJC_ContainerNum_EmptyContainerNum_NoLoadListLinesAttached("123456123456", ContainerLoadListHeaderLoadMode.ContainerFreightStation);
		}

		void AssertValidateJC_ContainerNum_EmptyContainerNum_NoLoadListLinesAttached(string originalValue, string loadMode)
		{
			var booking = CreateSupplierBooking("JSB001", SupplierBookingStatus.Planned);

			var containerLoadList = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonContainerLoadList)));
			containerLoadList["CLH_LoadListId"] = "CLL001";
			containerLoadList["CLH_LoadMode"] = loadMode;

			var container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = originalValue;
			if (loadMode == ContainerLoadListHeaderLoadMode.ContainerYard)
			{
				container.JC_JSB_SupplierBooking = (booking as BusinessObject).PK;
			}
			else
			{
				container.JC_CLH_LoadListPlan = containerLoadList.PK;
			}

			Factory.Save();

			container.JC_ContainerNum = ZString.Empty;

			AssertNoErrors("Container number can be empty when there are no load list lines attached", container.JC_ContainerNumInfo);
		}

		public void TestValidateJC_ContainerCount()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerCount = 0;
			AssertHasErrors("Expecting Container Count to be 0 and have errors", container.JC_ContainerCountInfo);

			container.JC_ContainerCount = -17;
			AssertHasErrors("Expecting Container Count to be negative and have errors", container.JC_ContainerCountInfo);

			container.JC_ContainerCount = 3;
			AssertNoErrors("Expecting Container Count to be valid, not expecting errors", container.JC_ContainerCountInfo);

			container.JC_ContainerNum = "Container";
			container.JC_ContainerCount = -6;

			AssertHasErrors("Container count must be 1 if the container number isn't empty, errors expected", container.JC_ContainerCountInfo);

			container.JC_ContainerNum = ZString.Empty;
			container.JC_ContainerCount = -6;

			AssertHasErrors("If the container number is empty, container count must be greater than 0, errors expected", container.JC_ContainerCountInfo);

			container.JC_ContainerNum = "Container";
			container.JC_ContainerCount = 6;

			AssertHasErrors("If the container number isn't empty, container count must be 1, errors expected", container.JC_ContainerCountInfo);

			container.JC_ContainerNum = "Container";
			container.JC_ContainerCount = 0;
			AssertHasErrors("If the container number isn't empty, container count must be 1, errors expected", container.JC_ContainerCountInfo);

			container.JC_ContainerNum = "Container";
			container.JC_ContainerCount = 1;

			AssertNoErrors("If the container number isn't empty, container count must be 1, no errors expected", container.JC_ContainerCountInfo);
		}

		public void TestValidateJC_ContainerCount_WithSupplierBookingAttachedAndRegistryEnabled()
		{
			AssertErrorIfNonEditableSupplierBookingAttachedAndRegistryEnabled(
				(container) => container.JC_ContainerCountInfo,
				(container) => { container.JC_ContainerCount = 5; },
				(container) => { container.JC_ContainerCount = 1; },
				(container) => container.Validation.ContainerAlreadyAllocatedErrorMessage
			);
		}

		public void TestValidateJC_ContainerCount_WithSupplierBookingAttachedAndRegistryDisabled()
		{
			AssertNoErrorIfSupplierBookingAttachedAndRegistryDisabled(
				(container) => container.JC_ContainerCountInfo,
				(container) => { container.JC_ContainerCount = 5; },
				(container) => { container.JC_ContainerCount = 1; }
			);
		}

		public void TestValidateJC_ArrivalSlotDate()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = ImportSailing1.PK;

			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ArrivalSlotDateTime = ZDateTime.Empty;
			AssertEquals("Slot Date can be empty.", false, container.JC_ArrivalSlotDateTimeInfo.HasWarnings());

			container.JC_ArrivalSlotDateTime = ZDateTime.Now.AddDays(-1);
			Assert("Slot Date is before now, should be warning", container.JC_ArrivalSlotDateTimeInfo.HasWarnings());

			container.JC_ArrivalSlotDateTime = ZDateTime.Now.AddDays(1);
			Assert("Slot Date is after current time, shouldn't be warning", !container.JC_ArrivalSlotDateTimeInfo.HasWarnings());
		}

		public void TestValidateJC_ArrivalSlotReference()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = ImportSailing1.PK;

			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ArrivalSlotReference = ZString.Empty;
			AssertEquals("Slot Reference no warning as no slot date exists", false, container.JC_ArrivalSlotReferenceInfo.HasNotifications());
			container.JC_ArrivalSlotDateTime = ZDateTime.Now.AddDays(1);
			Assert("Slot Reference is not empty, not expecting warning.", !container.JC_ArrivalSlotReferenceInfo.HasWarnings());
			container.JC_ArrivalSlotReference = "1901890";
			AssertEquals("Slot Reference is not empty and Slot Date has value, no warnings expected.", false, container.JC_ArrivalSlotReferenceInfo.HasWarnings());
			container.JC_ArrivalSlotReference = ZString.Empty;
			Assert("Slot Reference is empty and Slot Date has value, expecting warning.", container.JC_ArrivalSlotReferenceInfo.HasWarnings());
		}

		public void TestValidateJC_DepartureSlotDate()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_JX = ImportSailing1.PK;

			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			CommonContainer container = consol.Containers.AddNew();
			container.JC_DepartureSlotDateTime = ZDateTime.Empty;
			AssertEquals("Slot Date can be empty.", false, container.JC_DepartureSlotDateTimeInfo.HasWarnings());

			container.JC_DepartureSlotDateTime = ZDateTime.Now.AddDays(1);
			AssertEquals("Slot Date is after current time, shouldn't be warning", true, container.JC_DepartureSlotDateTimeInfo.HasWarnings());

			container.JC_DepartureSlotDateTime = container.Sailing.JX_JA_E_DEP.AddDays(-1);
			AssertEquals("Slot Date is before now, no warning expected", false, container.JC_DepartureSlotDateTimeInfo.HasWarnings());
		}

		public void TestValidateJC_DepartureSlotReference()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = ExportSailing1.PK;

			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			CommonContainer container = consol.Containers.AddNew();
			container.JC_DepartureSlotReference = ZString.Empty;
			AssertEquals("Slot Reference no warning as no slot date exists", false, container.JC_ArrivalSlotReferenceInfo.HasNotifications());
			container.JC_DepartureSlotDateTime = ZDateTime.Now.AddDays(-1);
			AssertEquals("Slot Reference is empty, expecting warning.", false, container.JC_DepartureSlotReferenceInfo.HasWarnings());
			container.JC_DepartureSlotReference = "12311451";
			Assert("Slot Reference is not empty, not expecting warning.", !container.JC_DepartureSlotReferenceInfo.HasWarnings());

			container.JC_DepartureSlotReference = ZString.Empty;
			Assert("Slot Reference is not empty, not expecting warning.", container.JC_DepartureSlotReferenceInfo.HasWarnings());
		}

		public void TestGrossWeightUQIsMandatory()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_GrossWeightUQ = Weight.Kilograms;
			AssertNoErrors(container.JC_GrossWeightUQInfo);
			container.JC_GrossWeightUQ = ZString.Empty;
			AssertHasErrors(container.JC_GrossWeightUQInfo);
		}

		public void TestValidateJC_AirVentFlow()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20RE_PK;
			container.JC_AirVentFlow = 1m;
			AssertNoErrors(container.JC_AirVentFlowInfo);

			container.JC_RC = RC_20GP_PK;
			container.JC_AirVentFlow = 1m;
			AssertHasErrors(container.JC_AirVentFlowInfo);

			container.JC_AirVentFlow = 0m;
			AssertNoErrors(container.JC_AirVentFlowInfo);

			container.JC_RC = RC_40RE_PK;
			AssertNoErrors(container.JC_AirVentFlowInfo);
		}

		public void TestValidateJC_AirVentFlowRateUnit()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20RE_PK;

			AssertNoErrors(container.JC_AirVentFlowRateUnitInfo);

			container.JC_AirVentFlow = 1m;
			AssertHasError(container.JC_AirVentFlowRateUnitInfo, "Please enter a Flow Unit.");

			container.JC_AirVentFlowRateUnit = AirFlowRateUnits.Codes.CubicMetersPerHour;
			AssertNoErrors(container.JC_AirVentFlowRateUnitInfo);

			container.JC_AirVentFlowRateUnit = "XXX";
			AssertHasError(container.JC_AirVentFlowRateUnitInfo, "Enter a valid Flow Unit.");
		}

		public void TestValidateJC_HumidityPercent()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20RE_PK;
			container.JC_HumidityPercent = 50;
			AssertNoErrors(container.JC_HumidityPercentInfo);

			container.JC_RC = RC_20GP_PK;
			container.JC_HumidityPercent = 50;
			AssertHasErrors(container.JC_HumidityPercentInfo);

			container.JC_HumidityPercent = 0;
			AssertNoErrors(container.JC_HumidityPercentInfo);

			container.JC_RC = RC_40RE_PK;
			AssertNoErrors(container.JC_HumidityPercentInfo);
		}

		public void TestValidateJC_TempRecorderSerialNo()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20RE_PK;
			container.JC_TempRecorderSerialNo = "123";
			AssertNoErrors(container.JC_TempRecorderSerialNoInfo);

			container.JC_RC = RC_20GP_PK;
			container.JC_TempRecorderSerialNo = "123";
			AssertHasErrors(container.JC_TempRecorderSerialNoInfo);

			container.JC_TempRecorderSerialNo = ZString.Empty;
			AssertNoErrors(container.JC_TempRecorderSerialNoInfo);

			container.JC_RC = RC_40RE_PK;
			AssertNoErrors(container.JC_TempRecorderSerialNoInfo);
		}

		public void TestValidateJC_SetPointTemp()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20RE_PK;
			container.JC_SetPointTemp = 1m;
			AssertNoErrors(container.JC_SetPointTempInfo);

			container.JC_RC = RC_20GP_PK;
			container.JC_SetPointTemp = 1m;
			AssertHasErrors(container.JC_SetPointTempInfo);

			container.JC_SetPointTemp = 0m;
			AssertNoErrors(container.JC_SetPointTempInfo);

			container.JC_RC = RC_40RE_PK;
			AssertNoErrors(container.JC_SetPointTempInfo);
		}

		public void TestValidateJC_SetPointTemp_WithinConsolRange()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = Temperature.Centigrade;
			consol.JK_RequiredTemperatureMinimum = 5;
			consol.JK_RequiredTemperatureMaximum = 15;

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ContainerType = ContainerTypes.Refrigerated;

			var container = consol.Containers.AddNew();
			container.JC_IsNonOperativeReefer = false;
			container.JC_SetPointTempUnit = Temperature.Centigrade;
			container.JC_RC = refContainer.PK;
			container.JC_IsControlledAtmosphere = true;

			var errorMessage = "This container is not within the Consolidation Pre-Allocated temperature range of 5°C to 15°C";

			container.JC_SetPointTemp = 4;
			AssertHasError(container.JC_SetPointTempInfo, errorMessage);

			container.JC_IsNonOperativeReefer = true;
			container.JC_SetPointTemp = 4;
			AssertNoError(container.JC_SetPointTempInfo, errorMessage);

			container.JC_IsNonOperativeReefer = false;
			container.JC_SetPointTemp = 4;
			AssertHasError(container.JC_SetPointTempInfo, errorMessage);

			container.JC_SetPointTemp = 5;
			AssertNoError(container.JC_SetPointTempInfo, errorMessage);

			container.JC_SetPointTemp = 15;
			AssertNoError(container.JC_SetPointTempInfo, errorMessage);

			container.JC_SetPointTemp = 16;
			AssertHasError(container.JC_SetPointTempInfo, errorMessage);
		}

		public void TestValidateJC_SetPointTemp_WithinPacklineRange()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = Temperature.Centigrade;
			consol.JK_RequiredTemperatureMinimum = 5;
			consol.JK_RequiredTemperatureMaximum = 15;

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ContainerType = ContainerTypes.Refrigerated;

			var container = consol.Containers.AddNew();
			container.JC_IsNonOperativeReefer = false;
			container.JC_SetPointTempUnit = Temperature.Centigrade;
			container.JC_RC = refContainer.PK;
			container.JC_IsControlledAtmosphere = true;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.Consols.Add(consol);

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			packline1.JL_RequiresTemperatureControl = true;
			packline1.JL_RequiredTemperatureUnit = Temperature.Centigrade;
			packline1.JL_RequiredTemperatureMinimum = 5;
			packline1.JL_RequiredTemperatureMaximum = 15;

			packline2.JL_RequiresTemperatureControl = true;
			packline2.JL_RequiredTemperatureUnit = Temperature.Centigrade;
			packline2.JL_RequiredTemperatureMinimum = 10;
			packline2.JL_RequiredTemperatureMaximum = 20;

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);

			var warning = "This container is not within the range of all Allocated Packlines temperature range";

			container.JC_SetPointTemp = 5;
			AssertHasWarning(container.JC_SetPointTempInfo, warning);

			container.JC_SetPointTemp = 9;
			AssertHasWarning(container.JC_SetPointTempInfo, warning);

			container.JC_SetPointTemp = 10;
			AssertNoWarning(container.JC_SetPointTempInfo, warning);

			container.JC_SetPointTemp = 15;
			AssertNoWarning(container.JC_SetPointTempInfo, warning);

			container.JC_SetPointTemp = 16;
			AssertHasWarning(container.JC_SetPointTempInfo, warning);

			container.JC_SetPointTemp = 20;
			AssertHasWarning(container.JC_SetPointTempInfo, warning);

			container.JC_IsNonOperativeReefer = true;
			container.JC_SetPointTemp = 5;
			AssertNoWarning(container.JC_SetPointTempInfo, warning);
		}

		public void TestValidateJC_SetPointTempUnit()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20RE_PK;
			container.Container.RC_ContainerType = ContainerTypes.Refrigerated;

			container.JC_SetPointTemp = 0m;
			container.JC_IsNonOperativeReefer = false;
			container.Validation.ValidateJC_SetPointTempUnit();
			AssertHasWarning(container.JC_SetPointTempUnitInfo, "The default temperature has not yet been verified by the user. Please check the temperature and the unit.");

			container.JC_IsNonOperativeReefer = true;
			container.Validation.ValidateJC_SetPointTempUnit();
			AssertNoWarning(container.JC_SetPointTempUnitInfo, "The default temperature has not yet been verified by the user. Please check the temperature and the unit.");

			container.JC_IsNonOperativeReefer = false;
			container.Container.RC_ContainerType = ContainerTypes.Other;
			container.Validation.ValidateJC_SetPointTempUnit();

			AssertNoWarning(container.JC_SetPointTempUnitInfo, "The default temperature has not yet been verified by the user. Please check the temperature and the unit.");

			container.JC_SetPointTemp = 1m;
			container.JC_SetPointTempUnit = "Z";
			AssertHasError(container.JC_SetPointTempUnitInfo, "Enter a valid " + container.JC_SetPointTempUnitInfo.Description + ".");

			container.JC_SetPointTempUnit = "C";
			AssertNoErrors(container.JC_SetPointTempUnitInfo);

			container.JC_SetPointTempUnit = ZString.Empty;
			AssertHasError(container.JC_SetPointTempUnitInfo, "Unit must be entered when temperature set point is entered.");

			container.JC_SetPointTempUnit = "F";
			AssertNoError(container.JC_SetPointTempUnitInfo, "Unit must be entered when temperature set point is entered.");
		}

		public void TestValidateJC_SetPointTempUnit_NoExceptionThrown()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RequiresTemperatureControl = true;

			var container = Factory.New<CommonContainer>();
			container.JC_RC = RC_40RE_PK;
			container.JC_SetPointTempUnit = "C";
			container.JC_JK = consol.PK;

			consol.JK_RequiredTemperatureUnit = "";
			AssertNoExceptionThrown(container.Validation.ValidateJC_SetPointTemp);

			consol.JK_RequiredTemperatureUnit = null;
			AssertNoExceptionThrown(container.Validation.ValidateJC_SetPointTemp);

			consol.JK_RequiredTemperatureUnit = "D";
			AssertNoExceptionThrown(container.Validation.ValidateJC_SetPointTemp);

			consol.JK_RequiredTemperatureUnit = "3";
			AssertNoExceptionThrown(container.Validation.ValidateJC_SetPointTemp);

			consol.JK_RequiredTemperatureUnit = "C";
			AssertNoExceptionThrown(container.Validation.ValidateJC_SetPointTemp);

			consol.JK_RequiredTemperatureUnit = "F";
			AssertNoExceptionThrown(container.Validation.ValidateJC_SetPointTemp);
		}

		public void TestValidateJC_IsControlledAtmosphere()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20RE_PK;
			container.JC_IsControlledAtmosphere = true;
			AssertNoErrors(container.JC_IsControlledAtmosphereInfo);

			container.JC_RC = RC_20GP_PK;
			container.JC_IsControlledAtmosphere = true;
			AssertHasErrors(container.JC_IsControlledAtmosphereInfo);

			container.JC_IsControlledAtmosphere = false;
			AssertNoErrors(container.JC_IsControlledAtmosphereInfo);

			container.JC_RC = RC_40RE_PK;
			AssertNoErrors(container.JC_IsControlledAtmosphereInfo);
		}

		public void TestValidateJC_IsControlledAtmosphere_WhenConsolRequiresTempControl()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();

			var errorMessage = "Container must have a Controlled Atmosphere as Consolidation is Pre-Allocated as Temperature Controlled";

			consol.JK_RequiresTemperatureControl = true;
			container.JC_IsControlledAtmosphere = false;
			AssertHasWarning(container.JC_IsControlledAtmosphereInfo, errorMessage);

			consol.JK_RequiresTemperatureControl = true;
			container.JC_IsControlledAtmosphere = true;
			AssertNoWarning(container.JC_IsControlledAtmosphereInfo, errorMessage);

			consol.JK_RequiresTemperatureControl = false;
			container.JC_IsControlledAtmosphere = true;
			AssertNoWarning(container.JC_IsControlledAtmosphereInfo, errorMessage);

			consol.JK_RequiresTemperatureControl = false;
			container.JC_IsControlledAtmosphere = false;
			AssertNoWarning(container.JC_IsControlledAtmosphereInfo, errorMessage);
		}

		public void TestValidateJC_ContainerQuality()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_ContainerQuality = "";
			AssertNoErrors("No container quality, expect no error", container.JC_ContainerQualityInfo);

			container.JC_ContainerQuality = "RIC";
			AssertNoErrors("Valid container quality, expect no error", container.JC_ContainerQualityInfo);

			container.JC_ContainerQuality = "MAD";
			AssertHasErrors("Invalid container quality, expect error", container.JC_ContainerQualityInfo);
		}

		public void TestValidateJC_ContainerStatus()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_ContainerStatus = "";
			AssertNoErrors("No container status, expect no error", container.JC_ContainerStatusInfo);

			container.JC_ContainerStatus = "AVL";
			AssertNoErrors("Valid container status, expect no error", container.JC_ContainerStatusInfo);

			container.JC_ContainerStatus = "BIG";
			AssertHasErrors("Invalid container status, expect error", container.JC_ContainerStatusInfo);
		}

		public void TestValidateIsChiller()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.StandAloneCustomsContainer = false;
			container.JC_RC = RC_20RE_PK;
			container.IsChiller = true;
			AssertNoErrors(container.IsChillerInfo);

			container.JC_RC = RC_20GP_PK;
			container.IsChiller = true;
			AssertHasErrors(container.IsChillerInfo);

			container.IsChiller = false;
			AssertNoErrors(container.IsChillerInfo);

			container.JC_RC = RC_40RE_PK;
			AssertNoErrors(container.IsChillerInfo);
		}

		public void TestValidateIsFreezer()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20RE_PK;
			container.IsFreezer = true;
			AssertNoErrors(container.IsFreezerInfo);

			container.JC_RC = RC_20GP_PK;
			container.IsFreezer = true;
			AssertHasErrors(container.IsFreezerInfo);

			container.IsFreezer = false;
			AssertNoErrors(container.IsFreezerInfo);

			container.JC_RC = RC_40RE_PK;
			AssertNoErrors(container.IsFreezerInfo);
		}

		public void TestValidateJC_RC2()
		{
			RefContainer uLD = RefContainer.New(Factory);
			uLD.RC_ShippingMode = "AIR";
			uLD.RC_IATARateClass = "ZZ1";

			RefContainer seaContainer = RefContainer.New(Factory);
			seaContainer.RC_ShippingMode = "SEA";

			CommonContainer testContainer = Factory.New<CommonContainer>();
			testContainer.JC_RC = seaContainer.PK;
			AssertNoErrors("No mode specified", testContainer.JC_RCInfo);
			testContainer.JC_RC = uLD.PK;
			AssertNoErrors("No mode specified", testContainer.JC_RCInfo);

			testContainer.JC_ContainerMode = "FCL";
			testContainer.JC_RC = uLD.PK;
			AssertHasErrors(testContainer.JC_RCInfo);

			testContainer.JC_RC = seaContainer.PK;
			AssertNoErrors(testContainer.JC_RCInfo);

			testContainer.JC_ContainerMode = "ULD";
			testContainer.JC_RC = seaContainer.PK;
			AssertHasErrors(testContainer.JC_RCInfo);

			testContainer.JC_RC = uLD.PK;
			AssertNoErrors(testContainer.JC_RCInfo);
		}

		public void TestValidateJC_ContainerNumWithPRAMessage()
		{
			ZDateTime now = ZDateTime.Now;

			GlbCompany.CurrentCompany.SetCountry("AU");

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRXU1234568";
			Factory.Save();

			EDIMessage unrelatedMessage = container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message = container.PRAMessages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message.EM_SystemCreateTimeUtc = now.AddMinutes(1);
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageSubType = "SSM";

			Assert("Precondition - no errors", !container.JC_ContainerNumInfo.HasErrors());
			container.JC_ContainerNum = "0000000000";
			Assert("Should have an error: " + CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo, container.JC_ContainerNumInfo.HasError(CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo));
		}

		public void TestValidateJC_ContainerNumWithPRARejectMessage()
		{
			ZDateTime now = ZDateTime.Now;

			GlbCompany.CurrentCompany.SetCountry("AU");

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRXU1234568";
			Factory.Save();

			EDIMessage unrelatedMessage = container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message = container.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageSubType = "SSM";

			EDIMessage message2 = container.Messages.AddNew(); // Rejection
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = "REJ";
			Assert("Precondition - no errors", !container.JC_ContainerNumInfo.HasErrors());
			container.JC_ContainerNum = "0000000000";
			Assert("Should have no errors", !container.JC_ContainerNumInfo.HasErrors());
		}

		public void TestValidateJC_ContainerNumWithManyPRAMessages()
		{
			ZDateTime now = ZDateTime.Now;

			GlbCompany.CurrentCompany.SetCountry("AU");

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRXU1234568";
			Factory.Save();

			EDIMessage unrelatedMessage = container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message1 = container.PRAMessages.AddNew(); // Original
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message1.EM_SystemCreateTimeUtc = now.AddMinutes(1);
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SSM";

			EDIMessage message2 = container.PRAMessages.AddNew(); // Rejection
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message2.EM_SystemCreateTimeUtc = now.AddMinutes(2);
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = "REJ";

			EDIMessage message3 = container.PRAMessages.AddNew(); // Original
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message3.EM_SystemCreateTimeUtc = now.AddMinutes(3);
			message3.EM_ReceiveTransmit = "TRX";
			message3.EM_MessageSubType = "SSM";

			Assert("Precondition - no errors", !container.JC_ContainerNumInfo.HasErrors());
			container.JC_ContainerNum = "0000000000";
			Assert("Should have an error: " + CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo, container.JC_ContainerNumInfo.HasError(CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo));
		}

		public void TestValidateJC_ContainerNumAfterCancelAndAcceptPRA()
		{
			ZDateTime now = ZDateTime.Now;

			GlbCompany.CurrentCompany.SetCountry("AU");

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRXU1234568";
			Factory.Save();

			EDIMessage unrelatedMessage = container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message1 = container.Messages.AddNew(); // Original
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SSM";

			EDIMessage message2 = container.Messages.AddNew(); // Acceptance
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = "ACK";

			EDIMessage message3 = container.Messages.AddNew(); // Cancellation
			message3.EM_ReceiveTransmit = "TRX";
			message3.EM_MessageSubType = "SCN";

			EDIMessage message4 = container.Messages.AddNew(); // Acceptance
			message4.EM_ReceiveTransmit = "RCV";
			message4.EM_MessageSubType = "ACK";

			Assert("Precondition - no errors", !container.JC_ContainerNumInfo.HasErrors());
			container.JC_ContainerNum = "0000000000";
			Assert("Should have no errors", !container.JC_ContainerNumInfo.HasErrors());
		}

		public void TestValidateJC_ContainerNumWithCancelPRA()
		{
			ZDateTime now = ZDateTime.Now;

			GlbCompany.CurrentCompany.SetCountry("AU");

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRXU1234568";
			Factory.Save();

			EDIMessage unrelatedMessage = container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message1 = container.PRAMessages.AddNew(); // Original
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message1.EM_SystemCreateTimeUtc = now.AddMinutes(1);
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SSM";

			EDIMessage message2 = container.PRAMessages.AddNew(); // Acceptance
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message2.EM_SystemCreateTimeUtc = now.AddMinutes(2);
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = "ACK";

			EDIMessage message3 = container.PRAMessages.AddNew(); // Cancellation
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message3.EM_SystemCreateTimeUtc = now.AddMinutes(3);
			message3.EM_ReceiveTransmit = "TRX";
			message3.EM_MessageSubType = "SCN";

			Assert("Precondition - no errors", !container.JC_ContainerNumInfo.HasErrors());
			container.JC_ContainerNum = "0000000000";
			Assert("Should have an error: " + CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyToCancellation, container.JC_ContainerNumInfo.HasError(CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyToCancellation));
		}

		public void TestValidateJC_ContainerNumAfterOriginalAndAcceptPRA()
		{
			ZDateTime now = ZDateTime.Now;

			GlbCompany.CurrentCompany.SetCountry("AU");

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRXU1234568";
			Factory.Save();

			EDIMessage unrelatedMessage = container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message1 = container.PRAMessages.AddNew(); // Original
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message1.EM_SystemCreateTimeUtc = now.AddMinutes(1);
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SSM";

			EDIMessage message2 = container.PRAMessages.AddNew(); // Acceptance
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message2.EM_SystemCreateTimeUtc = now.AddMinutes(2);
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = "ACK";

			Assert("Precondition - no errors", !container.JC_ContainerNumInfo.HasErrors());
			container.JC_ContainerNum = "0000000000";
			Assert("Should have an error: " + CommonContainerValidation.CannotChangeContainerWithoutCancellingPRAFirst, container.JC_ContainerNumInfo.HasError(CommonContainerValidation.CannotChangeContainerWithoutCancellingPRAFirst));
		}

		public void TestDoesNotValidateJC_ContainerNumWhenNotAU()
		{
			ZDateTime now = ZDateTime.Now;

			GlbCompany.CurrentCompany.SetCountry("NZ");

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRXU1234568";
			Factory.Save();

			EDIMessage unrelatedMessage = container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message = container.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageSubType = "SSM";
			container.JC_ContainerNum = "0000000000";
			Assert("Should not have an error: " + CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo, !container.JC_ContainerNumInfo.HasError(CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo));
		}

		public void TestCheckJC_GrossWeightVerificationDateTime_FutureDate()
		{
			var doNotEnterVGMDateMessage = "Please do not enter a VGM Verified Date.";
			var cannotBeFutureDateMessage = "The Verified Date cannot be a future date.";

			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeight = 123.456;
			var vgmVerifiedBy = Factory.NewWithValidTestData<OrgHeader>();
			vgmVerifiedBy.OH_RL_NKClosestPort = "AUSYD";
			container.GrossWeightVerifiedByAddress.OrganisationPK = vgmVerifiedBy.PK;
			Factory.Save();

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Empty;
			AssertNoErrors(container.JC_GrossWeightVerificationDateTimeInfo);

			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, doNotEnterVGMDateMessage);
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(1);
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, doNotEnterVGMDateMessage);
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(-1);
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, doNotEnterVGMDateMessage);
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.NotRequired;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(1);
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(-1);
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);

			container.GrossWeightVerifiedByAddress.OrganisationPK = vgmVerifiedBy.PK;
			Factory.Save();

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(1);
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(-1);
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(1);
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(-1);
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(1);
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now.AddDays(-1);
			AssertNoError(container.JC_GrossWeightVerificationDateTimeInfo, cannotBeFutureDateMessage);
		}

		public void TestCheckJC_GrossWeightVerificationDateTime()
		{
			var container = Factory.New<CommonContainer>();

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Empty;
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, "Please enter a VGM Verified Date.");

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Empty;
			AssertNoErrors(container.JC_GrossWeightVerificationDateTimeInfo);

			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, "Please do not enter a VGM Verified Date.");

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Empty;
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, "Please enter a VGM Verified Date.");

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.NotRequired;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Empty;
			AssertHasError(container.JC_GrossWeightVerificationDateTimeInfo, "Please enter a VGM Verified Date.");
		}

		public void TestValidateJC_GrossWeightVerificationType_EmptyContainer()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_IsEmptyContainer = true;
			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			AssertHasError(container.JC_GrossWeightVerificationTypeInfo, "Method 2 is not allowed for empty containers.");

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertNoErrors(container.JC_GrossWeightVerificationTypeInfo);
		}

		public void TestValidateJC_GrossWeightVerificationType_LoadPortChange()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "ONE";
			transport.JW_TransportMode = TransportModes.Sea;
			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			container.JC_GrossWeight = 30;
			AssertNoErrors(container.JC_GrossWeightVerificationTypeInfo);

			transport.JW_RL_NKLoadPort = "TWO";
			AssertHasError(container.JC_GrossWeightVerificationTypeInfo, "The Load Port has been changed. Please reselect the Verified Method.");

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			AssertNoError(container.JC_GrossWeightVerificationTypeInfo, "The Load Port has been changed. Please reselect the Verified Method.");
		}

		public void TestValidateJC_GrossWeightVerificationType_LoadPortRemoved()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "ONE";
			transport.JW_TransportMode = TransportModes.Sea;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "TWO";
			transport2.JW_TransportMode = TransportModes.Sea;

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			container.JC_GrossWeight = 30;

			AssertNoErrors(container.JC_GrossWeightVerificationTypeInfo);

			consol.Transports.Remove(transport);

			AssertHasError(container.JC_GrossWeightVerificationTypeInfo, "The Load Port has been changed. Please reselect the Verified Method.");
		}

		public void TestValidateJC_GrossWeightVerificationType_SeaPortChange()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var container = consol.Containers.AddNew();
			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			container.JC_GrossWeight = 30;

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AAA";
			transport1.JW_TransportMode = TransportModes.Air;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "BBB";
			transport2.JW_TransportMode = TransportModes.Air;

			var transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "CCC";
			transport3.JW_TransportMode = TransportModes.Sea;

			transport1.JW_RL_NKLoadPort = "DDD";
			AssertNoErrors(container.JC_GrossWeightVerificationTypeInfo);

			transport2.JW_RL_NKLoadPort = "EEE";
			AssertNoErrors(container.JC_GrossWeightVerificationTypeInfo);

			transport3.JW_RL_NKLoadPort = "FFF";
			AssertHasError(container.JC_GrossWeightVerificationTypeInfo, "The Load Port has been changed. Please reselect the Verified Method.");
		}

		public void TestValidateJC_GrossWeightVerificationType_InvalidCode()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = "";
			AssertHasError(container.JC_GrossWeightVerificationTypeInfo, "Please enter a VGM Verified Method.");

			container.JC_GrossWeightVerificationType = "ABC";
			AssertHasError(container.JC_GrossWeightVerificationTypeInfo, "Enter a valid VGM Verified Method.");
		}

		public void TestCheckJC_GrossWeight()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeight = 0;
			AssertHasError(container.JC_GrossWeightInfo, "The Verified Gross Container Weight has to be greater than 0.");

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.JC_GrossWeight = 0;
			AssertNoErrors(container.JC_GrossWeightInfo);

			container.JC_GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.JC_GrossWeight = 0;
			AssertHasError(container.JC_GrossWeightInfo, "The Verified Gross Container Weight has to be greater than 0.");
		}

		public void TestValidateJC_SellSpotRateMode()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			container.JC_SellSpotRateMode = "XXX";
			AssertHasErrors(container.JC_SellSpotRateModeInfo);

			container.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoErrors(container.JC_SellSpotRateModeInfo);

			container.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			AssertHasWarnings(container.JC_SellSpotRateModeInfo);

			container.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoWarnings(container.JC_SellSpotRateModeInfo);

			var containerStandard = consol.Containers.AddNew();

			container.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			var warning = "The One Off Freight Rate will be ignored as there are other Containers with different One Off Freight Rate Mode on this Consolidation";
			AssertHasWarning("Should have warning", container.JC_SellSpotRateModeInfo, warning);

			container.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoWarning(container.JC_SellSpotRateModeInfo, warning);

			var containerAIN = consol.Containers.AddNew();
			containerAIN.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.AllInRate;
			AssertHasWarning("Should have warning", containerAIN.JC_SellSpotRateModeInfo, warning);

			containerAIN.JC_SellSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoWarning(containerAIN.JC_SellSpotRateModeInfo, warning);
		}

		public void TestValidateJC_CostSpotRateMode()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			container.JC_CostSpotRateMode = "XXX";
			AssertHasErrors(container.JC_CostSpotRateModeInfo);

			container.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoErrors(container.JC_CostSpotRateModeInfo);

			container.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			AssertHasWarnings(container.JC_CostSpotRateModeInfo);

			container.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoWarnings(container.JC_CostSpotRateModeInfo);

			var containerStandard = consol.Containers.AddNew();

			container.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			var warning = "The Negotiated Cost will be ignored as there are other Containers with different Negotiated Cost Mode on this Consolidation";
			AssertHasWarning("Should have warning", container.JC_CostSpotRateModeInfo, warning);

			container.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoWarning(container.JC_CostSpotRateModeInfo, warning);

			var containerAIN = consol.Containers.AddNew();
			containerAIN.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.AllInRate;
			AssertHasWarning("Should have warning", containerAIN.JC_CostSpotRateModeInfo, warning);

			containerAIN.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoWarning(containerAIN.JC_CostSpotRateModeInfo, warning);
		}

		public void TestValidateJC_GatewaySellSpotRateMode()
		{
			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";

			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			container.JC_GatewaySellSpotRateMode = "XXX";
			AssertHasErrors(container.JC_GatewaySellSpotRateModeInfo);

			container.JC_GatewaySellSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoErrors(container.JC_GatewaySellSpotRateModeInfo);

			container.JC_GatewaySellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			AssertHasWarnings(container.JC_GatewaySellSpotRateModeInfo);

			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			container.JC_GatewaySellSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoWarnings(container.JC_GatewaySellSpotRateModeInfo);

			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUBNE";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			container.JC_GatewaySellSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			AssertNoWarnings(container.JC_GatewaySellSpotRateModeInfo);

			var containerStandard = consol.Containers.AddNew();

			container.JC_GatewaySellSpotRateMode = FreightRateAutoratingModes.Code.AllInRate;
			var warning = "The Gateway Sell Rate will be ignored as there are other Containers with different Gateway Sell Mode on this Consolidation";
			AssertHasWarning("Should have warning", container.JC_GatewaySellSpotRateModeInfo, warning);

			container.JC_GatewaySellSpotRateMode = FreightRateAutoratingModes.Code.StandardRate;
			AssertNoWarning(container.JC_GatewaySellSpotRateModeInfo, warning);
		}

		public void TestValidateSpotRateCurrencies()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();

			container.JC_RX_NKCostSpotRateCurrency = "XXX";
			container.JC_RX_NKGatewaySellSpotRateCurrency = "YYY";
			container.JC_RX_NKSellSpotRateCurrency = "ZZZ";

			AssertHasErrors(container.JC_RX_NKCostSpotRateCurrencyInfo);
			AssertHasErrors(container.JC_RX_NKSellSpotRateCurrencyInfo);
			AssertHasErrors(container.JC_RX_NKGatewaySellSpotRateCurrencyInfo);

			container.JC_RX_NKCostSpotRateCurrency = "USD";
			container.JC_RX_NKGatewaySellSpotRateCurrency = "AUD";
			container.JC_RX_NKSellSpotRateCurrency = "USD";

			AssertNoErrors(container.JC_RX_NKCostSpotRateCurrencyInfo);
			AssertNoErrors(container.JC_RX_NKSellSpotRateCurrencyInfo);
			AssertNoErrors(container.JC_RX_NKGatewaySellSpotRateCurrencyInfo);
		}

		public void TestValidateJC_JSB_SupplierBooking_NoExistingSupplierBookingShouldNotError()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var supplierBooking = CreateSupplierBooking("SOME-OTHER-SBK", SupplierBookingStatus.Approved);
				var container = Factory.New<CommonContainer>();
				container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;

				AssertNoErrors("Should be able to set if no supplier booking attached", container.JC_JSB_SupplierBookingInfo);
			});
		}

		public void TestValidateJC_JSB_SupplierBooking_WithSupplierBookingAttachedAndRegistryEnabled()
		{
			var supplierBooking = CreateSupplierBooking("SOME-OTHER-SBK", SupplierBookingStatus.Approved);
			Factory.Save();

			AssertErrorIfNonEditableSupplierBookingAttachedAndRegistryEnabled(
				(container) => container.JC_JSB_SupplierBookingInfo,
				(container) => { },
				(container) => { container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK; },
				(container) => container.Validation.ContainerHasLoadListLineMessage,
				false
			);

			AssertErrorIfNonEditableSupplierBookingAttachedAndRegistryEnabled(
				(container) => container.JC_JSB_SupplierBookingInfo,
				(container) => { },
				(container) => { container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK; },
				(container) => container.Validation.ContainerHasLoadListLineMessage,
				true
			);
		}

		public void TestValidateJC_JSB_SupplierBooking_WithSupplierBookingAttachedAndRegistryDisabled()
		{
			var supplierBooking = CreateSupplierBooking("SOME-OTHER-SBK", SupplierBookingStatus.Approved);
			Factory.Save();

			AssertNoErrorIfSupplierBookingAttachedAndRegistryDisabled(
				(container) => container.JC_JSB_SupplierBookingInfo,
				(container) => { },
				(container) => { container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK; }
			);
		}

		public void TestValidateJC_JSB_SupplierBooking_ShowsErrorIfLinkedSupplierBookingIsNotApproved()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				foreach (var status in typeof(SupplierBookingStatus).GetFields().Select(x => x.GetValue(null)).OfType<string>())
				{
					var supplierBooking = CreateSupplierBooking($"SBK-{status}", status);

					Factory.Save();

					var container = Factory.New<CommonContainer>();
					container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;

					if (status == SupplierBookingStatus.Approved || status == SupplierBookingStatus.Planned)
					{
						AssertNoErrors("Attached supplier booking is Approved", container.JC_JSB_SupplierBookingInfo);
					}
					else
					{
						AssertHasError(container.JC_JSB_SupplierBookingInfo, "Only approved or planned Supplier Bookings can be linked to containers.");
					}
				}
			});
		}

		public void TestValidateJC_JSB_SupplierBooking_ShowsNoErrorIfRegistryDisabled()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				foreach (var status in typeof(SupplierBookingStatus).GetFields().Select(x => x.GetValue(null)).OfType<string>())
				{
					var supplierBooking = CreateSupplierBooking($"SBK-{status}", status);

					Factory.Save();

					var container = Factory.New<CommonContainer>();
					container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;

					AssertNoErrors("Registry not enabled", container.JC_JSB_SupplierBookingInfo);
				}
			});
		}

		#region Implementation

		class DummyCommonContainer : CommonContainer
		{
			public DummyCommonContainer(BusinessObjectFactory factory, DataRow row)
				: base(factory, row) { }

			public Converter<CommonContainer, IContainerDefaultingStrategy> DefaultingStrategyProvider { get; set; }

			protected override IContainerDefaultingStrategy NewContainerDefaultingStrategyCore()
			{
				if (DefaultingStrategyProvider == null)
				{
					return base.NewContainerDefaultingStrategyCore();
				}
				else
				{
					return DefaultingStrategyProvider(this);
				}
			}
		}

		class DummyContainerWithAttachedContainerLoadList : CommonContainer
		{
			public DummyContainerWithAttachedContainerLoadList(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool IsAttachedToContainerLoadList() => true;
		}

		void AssertErrorIfNonEditableSupplierBookingAttachedAndRegistryEnabled(
			Func<CommonContainer, ZPropertyInfo> getPropertyInfo,
			Action<CommonContainer> initialSetter,
			Action<CommonContainer> changeState,
			Func<CommonContainer, MultilingualString> getMessage,
			bool hasAttachedToContainerLoadList = false)
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				foreach (var status in typeof(SupplierBookingStatus).GetFields().Select(x => x.GetValue(null)).OfType<string>())
				{
					var supplierBooking = CreateSupplierBooking($"SBK-{status}{hasAttachedToContainerLoadList}", status);

					var container = hasAttachedToContainerLoadList ? Factory.New<DummyContainerWithAttachedContainerLoadList>() : Factory.New<CommonContainer>();
					container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;
					initialSetter(container);

					Factory.Save();

					AssertNoError(
						"Pre-Condition",
						getPropertyInfo(container),
						getMessage(container)
					);

					var nonEditableStatuses = new[] { SupplierBookingStatus.Planned, SupplierBookingStatus.Converted };

					var expectedCanEdit = !nonEditableStatuses.Contains(status);
					if (getPropertyInfo(container).Name == nameof(CommonContainer.JC_JSB_SupplierBooking))
					{
						expectedCanEdit = expectedCanEdit || (status == SupplierBookingStatus.Converted && !hasAttachedToContainerLoadList);
					}

					changeState(container);

					if (expectedCanEdit)
					{
						AssertNoError($"supplier booking({status}) ${(hasAttachedToContainerLoadList ? "attached" : "not attached")} should have no error", getPropertyInfo(container), getMessage(container));
					}
					else
					{
						AssertHasError($"supplier booking({status}) ${(hasAttachedToContainerLoadList ? "attached" : "not attached")} should have error", getPropertyInfo(container), getMessage(container));
					}
				}
			});
		}

		void AssertNoErrorIfSupplierBookingAttachedAndRegistryDisabled(
			Func<CommonContainer, ZPropertyInfo> getPropertyInfo,
			Action<CommonContainer> initialSetter,
			Action<CommonContainer> changeState)
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				foreach (var status in typeof(SupplierBookingStatus).GetFields().Select(x => x.GetValue(null)).OfType<string>())
				{
					var supplierBooking = CreateSupplierBooking($"SBK-{status}", status);

					var container = Factory.New<CommonContainer>();
					container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;
					initialSetter(container);

					Factory.Save();

					changeState(container);
					AssertNoErrors("Should not error regardless of status if registry disabled", getPropertyInfo(container));
				}
			});
		}

		IJobSupplierBooking CreateSupplierBooking(string bookingID, string status)
		{
			var supplierBooking = Factory.New<IJobSupplierBooking>();
			supplierBooking.JSB_BookingId = bookingID;
			supplierBooking.JSB_Status = status;
			supplierBooking.JSB_TransportMode = TransportModes.Sea;
			supplierBooking.JSB_LoadMode = SupplierBookingLoadMode.ContainerYard;
			supplierBooking.JSB_OH_BookingParty = Factory.NewWithValidTestData<OrgHeader>().PK;

			return supplierBooking;
		}

		#endregion

	}
}
