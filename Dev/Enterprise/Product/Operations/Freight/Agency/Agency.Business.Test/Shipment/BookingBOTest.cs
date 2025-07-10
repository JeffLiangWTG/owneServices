using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBooking))]
	public class BookingBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.AgencyShipment);
			}
		}

		#endregion

		public void TestDoNotLoadBillOfLadingWhenBookingIsAlreadyLoaded()
		{
			var factory1 = new BusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = "factory to create new Booking with container and customs entry number"
			};

			var newBooking = factory1.New<AgencyBooking>();
			var newBookedContainer = newBooking.BookedContainers.AddNew();
			var newBookedContainerRefNumbers = newBookedContainer.AdditionalReferenceNumbers;
			var newCusEntryNumber = newBookedContainerRefNumbers.AddNew();

			factory1.Save();

			var factory2 = new BusinessObjectFactory
			{
				NameForDebugging = "factory which loads saved Booking and then is refreshed by DataRefreshBuss"
			};

			var booking = factory2.Load<AgencyBooking>(newBooking.PK);
			AgencyBooking.SetCurrentBookingInfo(booking.Factory, booking.PK, booking.JS_ShipmentStatus);

			var bookedContainer = (AgencyBookingContainer)booking.BookedContainers.Single();

			// we cannot allow CusEntryNumber to load Parent before DataRefreshBuss starts refreshing
			var cusEntryNumber = factory2.Load<CusEntryNumber>(newCusEntryNumber.PK);
			bookedContainer.RegisterEditableChildObject(cusEntryNumber);

			void LoadBookedContainerAsAnotherType()
			{
				var bookedContainerAsOtherType = factory2.Load<BillOfLadingContainer>(bookedContainer.PK);

				var containersInFactoryCache = factory2.GetBizOsForPK(bookedContainer.PK.ToGuid());
				AssertContainsExactElementsInAnyOrder("prereq; we have 2 containers of different types loaded in factory",
					new[]
					{
						bookedContainer,
						(BusinessObject)bookedContainerAsOtherType
					},
					containersInFactoryCache);
			}

			booking.BeforeUpdatedByDataRefresh += (obj, args) => LoadBookedContainerAsAnotherType();

			var bookingWasUpdatedByDataRefreshBuss = false;
			booking.UpdatedByDataRefresh += (obj, args) =>
			{
				bookingWasUpdatedByDataRefreshBuss = true;
			};

			var factory3 = new BusinessObjectFactory
			{
				NameForDebugging = "factory which is used to confirm Booking (i.e. convert Booking to Bill Of Lading)"
			};

			var bill = factory3.Load<BillOfLading>(newBooking.PK);
			bill.Confirm();

			factory3.Save();

			Assert("Booking was updated by DataRefreshBuss", bookingWasUpdatedByDataRefreshBuss);

			Assert("Booking was made readonly", booking.ReadOnly);
			Assert("Booked Container was made readonly", bookedContainer.ReadOnly);
			Assert("Customs Entry Number was made readonly", cusEntryNumber.ReadOnly);

			AssertEquals("No error thrown because booking was refreshing by DataRefreshBus", string.Empty, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSaveBOLWhenOldBookingIsRefreshingByDataRefreshBus_NoError()
		{
			var factory1 = new BusinessObjectFactory
			{
				RefreshEnabled = true,
				NameForDebugging = "factory to create new Booking with container and customs entry number"
			};

			var newBooking = factory1.New<AgencyBooking>();
			newBooking.Confirm();
			factory1.Save();

			var factory2 = new BusinessObjectFactory
			{
				RefreshEnabled = true,
				NameForDebugging = "factory which loads saved Booking and then is refreshed by DataRefreshBus"
			};
			
			var booking = factory2.Load<AgencyBooking>(newBooking.PK);
			AgencyBooking.SetCurrentBookingInfo(booking.Factory, booking.PK, booking.JS_ShipmentStatus);
			var cusEntryNumberAdditionalReference = booking.Numbers;

			AssertEquals(0, cusEntryNumberAdditionalReference.Count);
			
			var newCusEntryNumber = factory1.New<CusEntryNumber>();
			newCusEntryNumber.CE_Category = "OTH";
			newBooking.Numbers.Add(newCusEntryNumber);

			factory1.Save();

			AssertEquals("No error thrown because booking was refreshing by DataRefreshBus", string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDtbBookingParentControllerID()
		{
			var booking = Factory.New<AgencyBooking>();
			AssertEquals(ControllerIDs.AgencyBooking, ((IDtbBookingParent)booking).ControllerID);
		}

		public void TestGetCustomBusinessObject()
		{
			var workflowTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode);
			AddCustomField(workflowTemplate, "stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(workflowTemplate, "intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(workflowTemplate, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(workflowTemplate, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var agencyBooking = Factory.New<AgencyBooking>();
			var agencyBookingProvider = (ICustomFieldProvider)agencyBooking;
			var agencyBookingProviderCustomBizo = agencyBookingProvider.GetCustomBusinessObject();
			var agencyBookingDynamicBizo = (IDynamicBusinessObject)agencyBookingProviderCustomBizo;
			AssertNotNull(agencyBookingDynamicBizo.GetProperty("__STRINGFIELD__prop__ZString"));
			AssertNotNull(agencyBookingDynamicBizo.GetProperty("__INTFIELD__prop__ZInt"));
			AssertNotNull(agencyBookingDynamicBizo.GetProperty("__DATETIMEFIELD__prop__ZDateTime"));
			AssertNotNull(agencyBookingDynamicBizo.GetProperty("__BOOLFIELD__prop__ZBool"));
		}

		public GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		public void TestDefaultBookingWeightUnit()
		{
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms);
			AgencyBooking shipment1 = Factory.New<AgencyBooking>();
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			AgencyBooking shipment2 = Factory.New<AgencyBooking>();
			AssertEquals(Constants.Weight.Kilograms, shipment1.JS_UnitOfWeight);
			AssertEquals(Constants.Weight.Tonnes, shipment2.JS_UnitOfWeight);
		}

		public void TestDefaultBookingVolumeUnit()
		{
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);
			AgencyBooking shipment1 = Factory.New<AgencyBooking>();
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.MegaLitre);
			AgencyBooking shipment2 = Factory.New<AgencyBooking>();
			AssertEquals(Constants.Volume.CubicMetres, shipment1.JS_UnitOfVolume);
			AssertEquals(Constants.Volume.MegaLitre, shipment2.JS_UnitOfVolume);
		}

		public void TestContainerReadonlyness()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			AssertEquals("BookedContainers should not be readonly", false, booking.BookedContainers.ReadOnly);
			AssertEquals("RealContainers should not be readonly", false, booking.RealContainers.ReadOnly);
		}

		public void TestEmailSubject()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			booking.JS_UniqueConsignRef = "V00001";
			String emailSubject = ((ISendEmailSource)booking).EmailSubject;
			AssertEquals("Agency Booking - V00001", emailSubject);
		}

		public void TestDocumentSupporterReturnsCorrectBusinessContext()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			AssertEquals("DocumentSupporter should be of the correct type", typeof(AgencyShipmentDocumentSupporter), shipment.DocumentSupporter.GetType());
			AssertEquals("DocumentSupporter BusinessContext", BusinessContext.AgencyBooking, shipment.DocumentSupporter.BusinessContext);
		}

		public void TestGetTheSameBOsWhenReferingBackToSelfFromThePackLine()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			AgencyShipmentPackLine packLine = shipment.OuterPackLines.AddNew();
			AssertEquals("Should at the very least have the same PK", shipment.PK, packLine.Shipment.PK);
			AssertSame("need the same Shipment instance", shipment, packLine.Shipment);
			AssertSame("need the same collection instance", shipment.BookedContainers, packLine.Shipment.BookedContainers);
		}

		public void TestSetDefaultValues()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			AssertEquals("JS_IsShipping must be set.", true, shipment.JS_IsShipping);
			AssertEquals("JS_IsBooking must not be set.", false, shipment.JS_IsBooking);
			AssertEquals("JS_IsForwardRegistered must not be set.", false, shipment.JS_IsForwardRegistered);
			AssertEquals("JS_IsCFSRegistered must not be set.", false, shipment.JS_IsCFSRegistered);
			AssertEquals("JS_ShipmentStatus must be Booked.", ShipmentStatusList.Codes.Booked, shipment.JS_ShipmentStatus);
		}

		public void TestLoadingOrSaveingAConfirmedBookingSetsItToReadOnly()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.Confirm();
			Factory.Save();
			AssertEquals("Saved Shipment should be readonly", true, shipment.ReadOnly);
			AgencyBooking boookingReloaded = new BusinessObjectFactory().Load<AgencyBooking>(shipment.PK);
			AssertEquals("Loaded Shipment should be readonly", true, boookingReloaded.ReadOnly);
		}

		public void TestLoadingOrSaveingACancelledBookingSetsItToReadOnly()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_IsCancelled = true;
			Factory.Save();
			AssertEquals("Saved Shipment should be readonly", true, shipment.ReadOnly);
			AgencyBooking boookingReloaded = new BusinessObjectFactory().Load<AgencyBooking>(shipment.PK);
			AssertEquals("Loaded Shipment should be readonly", true, boookingReloaded.ReadOnly);
		}

		public void TestLoadingOrSaveingANonConfirmedBookingDontSetItToReadOnly()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			Factory.Save();
			AssertEquals("Saved Shipment should NOT be readonly", false, shipment.ReadOnly);
			AgencyBooking boookingReloaded = new BusinessObjectFactory().Load<AgencyBooking>(shipment.PK);
			AssertEquals("Loaded Shipment should not be readonly", false, boookingReloaded.ReadOnly);
		}

		public void TestConfirmMethod_WouldSetBookingToReadonlyAndChangeShipmentStatusList_ElectronicBookingAndShippingInstructions_true()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AgencyBooking shipment = Factory.New<AgencyBooking>();
				AssertEquals("Precondition: shipment status list contains booking codes", "BKD, WTL, WEB", shipment.Lookups.JS_ShipmentStatus_List.CodesAsString);
				shipment.Confirm();
				AssertEquals("The Shipment should now be readonly", true, shipment.ReadOnly);
				AssertEquals("The Shipment should now be Confirmed", ShipmentStatusList.Codes.Confirmed, shipment.JS_ShipmentStatus);
				AssertEquals("The Shipment status list should now only contain CNF and WFI", "ESI, CNF, SIJ, WFI", shipment.Lookups.JS_ShipmentStatus_List.CodesAsString);
			}
		}

		public void TestConfirmMethod_WouldSetBookingToReadonlyAndChangeShipmentStatusList_ElectronicBookingAndShippingInstructions_false()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<AgencyBooking>();
				AssertEquals("Precondition: shipment status list contains booking codes", "EBK, BKD, WEB, WTL", shipment.Lookups.JS_ShipmentStatus_List.CodesAsString);
				shipment.Confirm();
				AssertEquals("The Shipment should now be readonly", true, shipment.ReadOnly);
				AssertEquals("The Shipment should now be Confirmed", ShipmentStatusList.Codes.Confirmed, shipment.JS_ShipmentStatus);
				AssertEquals("The Shipment status list should now only contain CNF and WFI", "ESI, CNF, WFI", shipment.Lookups.JS_ShipmentStatus_List.CodesAsString);
			}
		}

		public void TestStatusValidation()
		{
			AgencyShipment shipment = Factory.New<AgencyBooking>();
			shipment.JS_ShipmentStatus = "XYZ";
			AssertHasErrors("XYZ is an invalid code", shipment.JS_ShipmentStatusInfo);
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertNoNotifications(ShipmentStatusList.Codes.Booked + " is a valid code", shipment.JS_ShipmentStatusInfo);
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertHasErrors(ShipmentStatusList.Codes.Confirmed + " is only valid if the shipment has been confirmed", shipment.JS_ShipmentStatusInfo);
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			AssertNoNotifications(ShipmentStatusList.Codes.WaitListed + " is a valid code", shipment.JS_ShipmentStatusInfo);

			shipment.Confirm();
			shipment = Factory.Load<BillOfLading>(shipment.PK);
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertNoNotifications(ShipmentStatusList.Codes.Confirmed + " is now valid as the shipment has been confirmed", shipment.JS_ShipmentStatusInfo);
		}

		public void TestBookingCustomisation()
		{
			ZString homePort = !GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty ? GlbBranch.CurrentBranch.GB_RL_NKHomePort : new ZString("AUSYD");
			ZString overseasPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort == "HKHKG" ? "AUSYD" : "HKHKG";
			BillOfLadingNumberCustomisation shipmentNumberCustomisation = new BillOfLadingNumberCustomisation();
			SetElement(shipmentNumberCustomisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "SHP", true);
			BillOfLadingNumberCustomisation bookingCustomisation = new BillOfLadingNumberCustomisation();
			SetElement(bookingCustomisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "BOK", true);
			BillOfLadingNumberCustomisation billOfLadingCustomisation = new BillOfLadingNumberCustomisation();
			SetElement(billOfLadingCustomisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "BOL", true);
			AgencyRegistry.Instance.OceanBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shipmentNumberCustomisation);
			AgencyRegistry.Instance.BookingNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bookingCustomisation);
			AgencyRegistry.Instance.OceanBillNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingCustomisation);
			AgencyBooking shipment;
			shipment = Factory.New<AgencyBooking>();
			shipment.JS_RL_NKOrigin = homePort;
			shipment.JS_RL_NKDestination = overseasPort;
			Factory.Save();
			CombineAssertions(delegate
			{
				AssertEquals("Export: Should get the Shipment Number from the 'OceanBillShipmentNumberCustomisation' registry item", "VSHP00000001", shipment.JS_UniqueConsignRef);
				AssertEquals("Export: Should get the Booking Number object from the 'BookingNumberCustomisation' registry item", "VBOK00000001", shipment.JS_CFSReference);
				AssertEquals("Export: Should get the Bill of Lading object from the 'OceanBillNumberCustomisation' registry item", "VBOL00000001", shipment.JS_HouseBill);
			});
			shipment = Factory.New<AgencyBooking>();
			shipment.JS_RL_NKOrigin = overseasPort;
			shipment.JS_RL_NKDestination = homePort;
			Factory.Save();
			CombineAssertions(delegate
			{
				AssertEquals("Import: Should get the Shipment Number from the 'ShipmentNumberCustomisation' registry item", "VSHP00000002", shipment.JS_UniqueConsignRef);
				AssertEquals("Import: Should NOT set the Booking Number", "", shipment.JS_CFSReference);
				AssertEquals("Import: Should NOT set the Bill of Lading", "", shipment.JS_HouseBill);
			});
			shipment = Factory.New<AgencyBooking>();
			shipment.JS_RL_NKOrigin = overseasPort;
			shipment.JS_RL_NKDestination = homePort;
			AgencyRegistry.Instance.AlwaysGenerateBookingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AgencyRegistry.Instance.AlwaysGenerateBillOfLadingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
			CombineAssertions(delegate
			{
				AssertEquals("Import: Should get the Shipment Number from the 'ShipmentNumberCustomisation' registry item", "VSHP00000003", shipment.JS_UniqueConsignRef);
				AssertEquals("Import: Should NOT set the Booking Number", "VBOK00000002", shipment.JS_CFSReference);
				AssertEquals("Import: Should NOT set the Bill of Lading", "VBOL00000002", shipment.JS_HouseBill);
			});
			shipment = Factory.New<AgencyBooking>();
			shipment.JS_RL_NKOrigin = homePort;
			shipment.JS_RL_NKDestination = overseasPort;
			shipment.JS_CFSReference = "BookingRef";
			shipment.JS_HouseBill = "BOL";
			Factory.Save();
			CombineAssertions(delegate
			{
				AssertEquals("Export-Preset: Should get the Shipment Number from the 'OceanBillShipmentNumberCustomisation' registry item", "VSHP00000004", shipment.JS_UniqueConsignRef);
				AssertEquals("Export-Preset: Should NOT overwrite the Booking Number", "BookingRef", shipment.JS_CFSReference);
				AssertEquals("Export-Preset: Should NOT overwrite the Bill of Lading", "BOL", shipment.JS_HouseBill);
			});
		}

		public void TestBookingWithMileStone_ShouldNotDeleteTransport()
		{
			ZDateTime oldDate = ZDateTime.Today.AddDays(-5);
			ZDateTime newDate = ZDateTime.Today.AddDays(+5);
			var sailing = CreateSailing("AUBNE", "SGSIN", oldDate);
			Factory.Save();
			sailing.Origin.JA_E_DEP = newDate;
			AgencyShipment shipment = Factory.New<AgencyBooking>();
			var workflowProvider = (IWorkflowProvider)shipment;
			var milestone = workflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			shipment.JS_NKLoadPort = "AUBNE";
			shipment.JS_NKDischargePort = "SGSIN";
			shipment.JS_E_DEP = oldDate;
			AssertEquals("Pre-condition: Booking's Sailing should linked", sailing.PK, shipment.JS_JX);
			AssertEquals("Pre-condition: Departure Date should default as Sailing ETD Date", newDate, shipment.JS_E_DEP);
			Factory.Save();
			AssertEquals("Milestones should be added to shipment", 1, shipment.WorkflowItems.Milestones.Count);
			AssertEquals("Booking's Sailing should not changed.", sailing.PK, shipment.JS_JX);
			AssertEquals("Departure Date should not changed.", newDate, shipment.JS_E_DEP);
		}

		public void TestVisualizableDocumentsSupportableAttribute()
		{
			var agencyBooking = Factory.New<AgencyBooking>();
			var attribute = agencyBooking.GetType().GetCustomAttribute<VisualizableDocumentsSupportableAttribute>();
			AssertNotNull("Has VisualizableDocumentsSupportable attribute", attribute);
			AssertEquals("BookingVisualizableDocumentSupporter", attribute.SupporterType.Name);
		}

		#region LightValidationTester

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new BookingBOLightValidationTester(bizObjToTest);
		}

		class BookingBOLightValidationTester : LightValidationTester
		{
			public BookingBOLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;

				return propertyName != StmALog.Schema.SL_SE_NKEvent
						&& propertyName != StmALog.Schema.SL_Table
						&& propertyName != StmALog.Schema.SL_Parent;
			}
		}

		#endregion

		#region ShipmentStatusChangedLog

		public void TestShipmentStatusChangedLog()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();
				var stuLogCount = 0;

				AssertEquals(++stuLogCount, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));
				Assert(booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == ShipmentStatusList.Codes.Booked && x.Parameters["RES"] == "Booking Confirmed"));

				foreach (ICodeDescription status in new AgencyShipmentStatusList(false))
				{
					if (status.Code == ShipmentStatusList.Codes.Booked)
					{
						continue;
					}

					booking.JS_ShipmentStatus = status.Code;
					Factory.Save();

					AssertEquals(++stuLogCount, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));
					Assert(booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == status.Code));
					AssertEquals(status.Code, booking.JS_ShipmentStatus);

					if (status.Code == ShipmentStatusList.Codes.BookingRejected)
					{
						Assert(booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == status.Code && x.Parameters["RES"] == "Booking Rejected"));
					}
				}

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();

				stuLogCount = booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status");

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

				AssertEquals(stuLogCount + 1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));
				Assert(booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == ShipmentStatusList.Codes.Booked && x.Parameters["OLD"] == ShipmentStatusList.Codes.Booked));
			}
		}

		public void TestShipmentStatusChangedLog_Registry()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));

				foreach (ICodeDescription status in new AgencyShipmentStatusList(false))
				{
					if (status.Code == ShipmentStatusList.Codes.Booked)
					{
						continue;
					}

					booking.JS_ShipmentStatus = status.Code;

					AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));
					Assert(!booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == status.Code));
				}
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));

				foreach (ICodeDescription status in new AgencyShipmentStatusList(false))
				{
					if (status.Code == ShipmentStatusList.Codes.Booked)
					{
						continue;
					}

					booking.JS_ShipmentStatus = status.Code;

					AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));
					Assert(booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == status.Code));
				}
			}
		}

		public void TestShipmentStatusChangedLog_SaveOneTime()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));
				Assert(booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == ShipmentStatusList.Codes.Booked && x.Parameters["RES"] == "Booking Confirmed" && x.Parameters["OLD"] == null));

				foreach (ICodeDescription status in new AgencyShipmentStatusList(false))
				{
					if (status.Code == ShipmentStatusList.Codes.Booked)
					{
						continue;
					}

					booking.JS_ShipmentStatus = status.Code;

					AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));
					Assert(booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == status.Code));

					if (status.Code == ShipmentStatusList.Codes.BookingRejected)
					{
						Assert(booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == status.Code && x.Parameters["RES"] == "Booking Rejected"));
					}
				}

				Factory.Save();
				AssertEquals(2, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));
			}
		}

		public void TestSuspendAutomaticCreationOfStatusChangedLog()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();

				AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));

				using (booking.SuspendAutomaticCreationOfStatusChangedLog())
				{
					foreach (ICodeDescription status in new AgencyShipmentStatusList(false))
					{
						if (status.Code == ShipmentStatusList.Codes.Booked)
						{
							continue;
						}

						booking.JS_ShipmentStatus = status.Code;
						Factory.Save();

						AssertEquals(1, booking.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status"));
						Assert(!booking.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters["TYP"] == "Shipment Status" && x.Parameters["NEW"] == status.Code));
					}
				}
			}
		}

		#endregion

		#region JS_ShipmentStatusReadOnly

		public void TestJS_ShipmentStatusReadOnly()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = Factory.New<AgencyBooking>();

				var readonlyStatusList = new CodeDescriptionPairList();
				readonlyStatusList.AddPair(ShipmentStatusList.Codes.EBookingCancellationRequest, ShipmentStatusList.Descriptions.EBookingCancellationRequest);
				readonlyStatusList.AddPair(ShipmentStatusList.Codes.BookingCancelled, ShipmentStatusList.Descriptions.BookingCancelled);

				foreach (ICodeDescription status in new AgencyShipmentStatusList(false))
				{
					booking.JS_ShipmentStatus = status.Code;

					AssertEquals(booking.JS_ShipmentStatus_ReadOnly, readonlyStatusList.ContainsCode(status.Code));
				}

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

				Assert(!booking.IsReceivedElectronicBooking());

				booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();

				Assert(booking.IsReceivedElectronicBooking());

				readonlyStatusList.AddPair(ShipmentStatusList.Codes.Booked, ShipmentStatusList.Descriptions.Booked);
				readonlyStatusList.AddPair(ShipmentStatusList.Codes.BookingRejected, ShipmentStatusList.Descriptions.BookingRejected);

				foreach (ICodeDescription status in new AgencyShipmentStatusList(false))
				{
					booking.JS_ShipmentStatus = status.Code;

					AssertEquals(booking.JS_ShipmentStatus_ReadOnly, readonlyStatusList.ContainsCode(status.Code));
				}
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var booking = Factory.New<AgencyBooking>();

				foreach (ICodeDescription status in new AgencyShipmentStatusList(false))
				{
					booking.JS_ShipmentStatus = status.Code;

					Assert(!booking.JS_ShipmentStatus_ReadOnly);
				}
			}
		}

		#endregion

		#region Custom Fields Test

		[TestedType(typeof(AgencyBooking))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#endregion

		#region AgencyBookingLoggingStrategy

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_WithoutHIRNumber()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_CFSReference = "123";
				Factory.Save();

				AssertEquals(0, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_HasNewBKDSTULogNotInDB()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;
				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.JS_CFSReference = "123";

				AssertEquals(1, agencyBooking.Logs.LogsNotInDB.Cast<StmALog>()
					.Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode
						&& x.Parameters.TryGetValue(Params.Type, out var type) && type == Core.Constants.EventReferenceMessageTypes.ShipmentStatus
						&& x.Parameters.TryGetValue(Params.New, out var newValue) && newValue == ShipmentStatusList.Codes.Booked));

				Factory.Save();

				AssertEquals(0, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_NotInDatabase()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;
				agencyBooking.JS_CFSReference = "123";
				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_ShipmentStatusIsNotBooked()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;
				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_CFSReference = "123";
				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_RegistryIsDisabled()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;
				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_CFSReference = "123";
				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
				billOfLading.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				billOfLading.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

				billOfLading.JS_CFSReference = "123";
				Factory.Save();

				Assert(!billOfLading.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;
				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_CFSReference = "123";
				Factory.Save();

				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_OA_BookedShippingLineAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				Factory.Save();

				AssertEquals(2, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgAddress>().Header.PK;
				Factory.Save();

				AssertEquals(3, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
				voyage.GenerateSailings();

				agencyBooking.JS_JX = voyage.Sailings[0].PK;
				Factory.Save();

				AssertEquals(4, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_BookingReference = "234";
				Factory.Save();

				AssertEquals(4, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_Port()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Transports.RemoveAll();
				agencyBooking.JS_JX = ZGuid.Empty;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				var newFactory = new BusinessObjectFactory();
				agencyBooking = newFactory.Load<AgencyBooking>(agencyBooking.PK);

				AssertNull(agencyBooking.MostInterestingTransport);

				AssertNullOrEmpty(agencyBooking.JS_Calc_CurrentLoadPort);
				AssertNullOrEmpty(agencyBooking.JS_Calc_CurrentDischargePort);

				var transport = agencyBooking.Transports.AddNew();
				transport.JW_RL_NKLoadPort = "TWKEL";
				transport.JW_RL_NKDiscPort = "USLAX";

				AssertNotNull(agencyBooking.MostInterestingTransport);

				AssertEquals("TWKEL", agencyBooking.MostInterestingTransport.JW_RL_NKLoadPort);
				AssertEquals("USLAX", agencyBooking.MostInterestingTransport.JW_RL_NKDiscPort);

				AssertEquals("TWKEL", agencyBooking.JS_Calc_CurrentLoadPort);
				AssertEquals("USLAX", agencyBooking.JS_Calc_CurrentDischargePort);

				newFactory.Save();

				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				AssertNotNull(agencyBooking.MostInterestingTransport);

				AssertEquals("TWKEL", agencyBooking.JS_Calc_CurrentLoadPort);
				AssertEquals("USLAX", agencyBooking.JS_Calc_CurrentDischargePort);

				transport.Delete();

				AssertNull(agencyBooking.MostInterestingTransport);

				AssertNullOrEmpty(agencyBooking.JS_Calc_CurrentLoadPort);
				AssertNullOrEmpty(agencyBooking.JS_Calc_CurrentDischargePort);

				newFactory.Save();

				AssertEquals(2, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				AssertNull(agencyBooking.MostInterestingTransport);

				AssertNullOrEmpty(agencyBooking.JS_Calc_CurrentLoadPort);
				AssertNullOrEmpty(agencyBooking.JS_Calc_CurrentDischargePort);
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_TransportsIncludingRelatedOriginalCount()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Transports.RemoveAll();
				agencyBooking.JS_JX = ZGuid.Empty;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				var newFactory = new BusinessObjectFactory();
				agencyBooking = newFactory.Load<AgencyBooking>(agencyBooking.PK);

				Assert(!agencyBooking.TransportsIncludingRelatedOriginalCount.HasValue);

				agencyBooking.JS_GoodsDescription = "GoodsDescription233";
				newFactory.Save();

				AssertEquals(0, agencyBooking.TransportsIncludingRelatedOriginalCount);

				var transport = agencyBooking.Transports.AddNew();
				transport.JW_RL_NKLoadPort = "TWKEL";
				transport.JW_RL_NKDiscPort = "USLAX";

				AssertEquals(0, agencyBooking.TransportsIncludingRelatedOriginalCount);
				AssertEquals(1, agencyBooking.TransportsIncludingRelated.Count);

				newFactory.Save();

				AssertEquals(1, agencyBooking.TransportsIncludingRelatedOriginalCount);
				AssertEquals(1, agencyBooking.TransportsIncludingRelated.Count);
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport.Delete();

				AssertEquals(1, agencyBooking.TransportsIncludingRelatedOriginalCount);
				AssertEquals(0, agencyBooking.TransportsIncludingRelated.Count);

				newFactory.Save();

				AssertEquals(0, agencyBooking.TransportsIncludingRelatedOriginalCount);
				AssertEquals(0, agencyBooking.TransportsIncludingRelated.Count);
				AssertEquals(2, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				var transport1 = agencyBooking.Transports.AddNew();
				transport1.JW_RL_NKLoadPort = "TWKEL";
				transport1.JW_RL_NKDiscPort = "USLAX";

				var transport2 = agencyBooking.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "USLAX";
				transport2.JW_RL_NKDiscPort = "CNSHG";

				agencyBooking.Transports.RemoveAndDeleteAll();

				AssertEquals(0, agencyBooking.TransportsIncludingRelatedOriginalCount);
				AssertEquals(0, agencyBooking.TransportsIncludingRelated.Count);

				newFactory.Save();

				AssertEquals(0, agencyBooking.TransportsIncludingRelatedOriginalCount);
				AssertEquals(0, agencyBooking.TransportsIncludingRelated.Count);
				AssertEquals(2, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_TransportsIncludingRelated()
		{
			var principals = new AllowSendingBookingConfirmationCollection();
			var principal = principals.AddNew();
			principal.PrincipalPK = ZGuid.Empty;
			principal.Enabled = true;

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, principals))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Transports.RemoveAll();
				agencyBooking.JS_JX = ZGuid.Empty;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

				var transport1 = agencyBooking.Transports.AddNew();
				transport1.JW_RL_NKLoadPort = "TWKEL";
				transport1.JW_RL_NKDiscPort = "USLAX";
				transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				transport1.JW_Vessel = "COSCO NEBULA";
				transport1.JW_VoyageFlight = "85475";
				transport1.JW_ETD = new ZDateTime(2021, 01, 24, 8, 45, 00);
				transport1.JW_ATD = new ZDateTime(2021, 01, 24, 8, 45, 00);
				transport1.JW_ETA = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport1.JW_TerminalReceivalCommences = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport1.JW_DepotReceivalCommences = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport1.JW_TerminalCutOff = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport1.JW_DepotCutOff = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport1.JW_DocumentaryCutOff = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport1.JW_VGMCutOff = new ZDateTime(2021, 01, 26, 12, 15, 00);

				var transport2 = agencyBooking.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "USLAX";
				transport2.JW_RL_NKDiscPort = "CNSHG";
				transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				transport2.JW_Vessel = "COSCO NEBULA2";
				transport2.JW_VoyageFlight = "85476";
				transport2.JW_ETD = new ZDateTime(2021, 01, 24, 8, 45, 00);
				transport2.JW_ATD = new ZDateTime(2021, 01, 24, 8, 45, 00);
				transport2.JW_ETA = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport2.JW_TerminalReceivalCommences = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport2.JW_DepotReceivalCommences = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport2.JW_TerminalCutOff = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport2.JW_DepotCutOff = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport2.JW_DocumentaryCutOff = new ZDateTime(2021, 01, 26, 12, 15, 00);
				transport2.JW_VGMCutOff = new ZDateTime(2021, 01, 26, 12, 15, 00);

				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_RL_NKLoadPort = "AUSYD";
				Factory.Save();

				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_RL_NKDiscPort = "AUMEL";
				Factory.Save();

				AssertEquals(2, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
				Factory.Save();

				AssertEquals(3, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				Factory.Save();

				AssertEquals(3, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_Vessel = "COSCO NEBULA23333";
				Factory.Save();

				AssertEquals(4, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_VoyageFlight = "23333";
				Factory.Save();

				AssertEquals(5, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_ETD = new ZDateTime(2021, 01, 25, 8, 45, 00);
				Factory.Save();

				AssertEquals(6, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_ETA = new ZDateTime(2021, 01, 27, 12, 15, 00);
				Factory.Save();

				AssertEquals(7, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
				Factory.Save();

				AssertEquals(8, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				Factory.Save();

				AssertEquals(8, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				Factory.Save();

				AssertEquals(8, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;

				transport1.JW_TerminalReceivalCommences = new ZDateTime(2021, 01, 26, 12, 16, 15);

				Factory.Save();

				AssertEquals(9, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;

				transport2.JW_DepotReceivalCommences = new ZDateTime(2021, 01, 26, 12, 16, 15);

				Factory.Save();

				AssertEquals(10, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;

				transport1.JW_TerminalCutOff = new ZDateTime(2021, 01, 26, 12, 16, 15);

				Factory.Save();

				AssertEquals(11, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;

				transport2.JW_DocumentaryCutOff = new ZDateTime(2021, 01, 26, 12, 16, 15);

				Factory.Save();

				AssertEquals(12, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_VGMCutOff = new ZDateTime(2021, 01, 26, 12, 16, 15);

				Factory.Save();

				AssertEquals(13, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_VGMCutOff = new ZDateTime(2021, 01, 26, 12, 16, 15);

				Factory.Save();

				AssertEquals(13, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;

				transport2.JW_VGMCutOff = new ZDateTime(2021, 01, 26, 12, 25, 15);

				Factory.Save();

				AssertEquals(13, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_OA_CarrierAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

				Factory.Save();

				AssertEquals(14, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_CarrierBookingReference = "2333333";

				Factory.Save();

				AssertEquals(14, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_DataRefresh()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
				voyage.GenerateSailings();

				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Transports.RemoveAll();
				agencyBooking.JS_JX = voyage.Sailings[0].PK;
				agencyBooking.JS_BookingReference = ZString.Empty;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_BookingReference = "T123456";
				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				var newFactory = new BusinessObjectFactory();
				var voyageInNewFactory = newFactory.Load<JobVoyage>(voyage.PK);

				voyageInNewFactory.JV_OH_Line = orgHeader.PK;
				newFactory.Save();

				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_BookingReference = ZString.Empty;
				Factory.Save();

				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestAgencyBookingLoggingStrategyAddEDTEventLogs_MainSeaRoutingLegATD()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking.Transports.RemoveAll();
				agencyBooking.JS_JX = ZGuid.Empty;
				agencyBooking.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

				var transport1 = agencyBooking.Transports.AddNew();
				transport1.JW_RL_NKLoadPort = "TWKEL";
				transport1.JW_RL_NKDiscPort = "USLAX";
				transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				transport1.JW_ETD = new ZDateTime(2021, 01, 24, 8, 45, 00);
				transport1.JW_ETA = new ZDateTime(2021, 01, 26, 12, 15, 00);

				Factory.Save();

				Assert(!agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_CFSReference = "123";
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_ATD = new ZDateTime(2021, 01, 24, 8, 45, 00);
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				#region Actual Departure Date (ATD) of MAI SEA Leg has been set

				agencyBooking.JS_CFSReference = "345";
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "TWKEL";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHG";
				voyage.GenerateSailings();

				agencyBooking.JS_JX = voyage.Sailings[0].PK;
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_OA_BookedShippingLineAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgAddress>().Header.PK;
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				#region New transport leg
				var transport2 = agencyBooking.Transports.AddNew();
				transport2.JW_TransportMode = Core.Constants.TransportModes.Other;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;

				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_Vessel = "COSCO NEBULA2";
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_VoyageFlight = "61341";
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_RL_NKLoadPort = "USLAX";
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_RL_NKDiscPort = "CNSHG";
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_ETD = new ZDateTime(2024, 01, 27, 8, 45, 00);
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_ETA = new ZDateTime(2024, 01, 28, 12, 15, 00);
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport2.JW_OA_CarrierAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				#endregion

				#region Main Sea Leg Fields
				transport1.JW_TerminalReceivalCommences = new ZDateTime(2024, 01, 26, 12, 15, 00);
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_TerminalCutOff = new ZDateTime(2024, 01, 26, 12, 15, 00);
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_DocumentaryCutOff = new ZDateTime(2024, 01, 26, 12, 15, 00);
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_DepotReceivalCommences = new ZDateTime(2024, 01, 26, 12, 15, 00);
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_DepotCutOff = new ZDateTime(2024, 01, 26, 12, 15, 00);
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				transport1.JW_VGMCutOff = new ZDateTime(2024, 01, 26, 12, 15, 00);
				Factory.Save();
				AssertEquals(1, agencyBooking.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				#endregion

				#endregion
			}
		}

		#endregion

		#region Implementation

		JobSailing CreateSailing(ZString load, ZString discharge, ZDateTime etd)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;
			return sailing;
		}

		void SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order, string detail, bool fountain)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Detail = detail;
			element.Fountain = fountain;
		}

		JobVoyage Voyage;
		VoyageOrigin Origin1;
		VoyageOrigin Origin2;
		VoyageOrigin Origin3;
		JobSailing Sailing1;
		JobSailing Sailing2;
		JobSailing Sailing3;
		JobSailing Sailing4;
		public void SetupSailing(bool countryAllocation, bool originAllocation, bool sailingAllocation)
		{
			Voyage = Factory.New<JobVoyage>();
			Origin1 = Voyage.Origins.AddNew();
			Origin1.JA_RL_NKPortOfLoading = "AUBNE";
			Origin1.JA_E_DEP = ZDateTime.Now.AddDays(10);
			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUDRW";
			Origin2.JA_E_DEP = ZDateTime.Now.AddDays(16);
			Origin3 = Voyage.Origins.AddNew();
			Origin3.JA_RL_NKPortOfLoading = "SGSIN";
			Origin3.JA_E_DEP = ZDateTime.Now.AddDays(22);
			VoyageDestination destination1 = Voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUDRW";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(14);
			VoyageDestination destination2 = Voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";
			destination2.JB_E_ARV = ZDateTime.Now.AddDays(20);
			VoyageDestination destination3 = Voyage.Destinations.AddNew();
			destination3.JB_RL_NKPortOfDischarge = "MYBAG";
			destination3.JB_E_ARV = ZDateTime.Now.AddDays(26);
			Sailing1 = Voyage.Sailings.AddNew();
			Sailing1.JX_JA = Origin1.PK;
			Sailing1.JX_JB = destination2.PK;
			Sailing2 = Voyage.Sailings.AddNew();
			Sailing2.JX_JA = Origin1.PK;
			Sailing2.JX_JB = destination1.PK;
			Sailing3 = Voyage.Sailings.AddNew();
			Sailing3.JX_JA = Origin2.PK;
			Sailing3.JX_JB = destination2.PK;
			Sailing4 = Voyage.Sailings.AddNew();
			Sailing4.JX_JA = Origin3.PK;
			Sailing4.JX_JB = destination3.PK;
			string allocationMethod = AllocationMethodList.Codes.NotSet;
			if (countryAllocation)
			{
				allocationMethod = AllocationMethodList.Codes.Country;
				foreach (VoyageCountry country in new VoyageCountry[] { Origin1.VoyageCountry, Origin3.VoyageCountry })
				{
					SlotAllocation allocation = country.SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 80);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 240);
				}
			}

			if (originAllocation)
			{
				allocationMethod = AllocationMethodList.Codes.Origin;
				foreach (VoyageOrigin origin in new VoyageOrigin[] { Origin1, Origin2, Origin3 })
				{
					SlotAllocation allocation = origin.SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 80);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 240);
				}
			}

			if (sailingAllocation)
			{
				allocationMethod = AllocationMethodList.Codes.Sailing;
				foreach (JobSailing sailing in new JobSailing[] { Sailing1, Sailing2, Sailing3, Sailing4 })
				{
					SlotAllocation allocation = sailing.SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 40);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 160);
				}
			}

			foreach (VoyageCountry country in new VoyageCountry[] { Origin1.VoyageCountry, Origin3.VoyageCountry })
			{
				country.J0_AllocationMethod = allocationMethod;
			}
		}

		#endregion
	}
}
