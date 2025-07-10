using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class FieldSynchroniserTest : TestCaseWithFactory
	{
		public void TestDestinationInfo()
		{
			var shpt = Factory.NewWithValidTestData<ForwardingShipment>();
			BaseJobDeclaration dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var fieldSync = new FieldSynchroniser(dec.JE_CustomsProfileInfo, shpt.JS_PackingModeInfo);
			AssertEquals("DestinationInfo", dec.JE_CustomsProfileInfo, fieldSync.Destination);
		}

		public void TestSynchWithDelegatedDestiation()
		{
			var sourceShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			BaseJobDeclaration destinationDeclaration = null;
			var fieldSynchroniser = new FieldSynchroniser(delegate
			{ return destinationDeclaration?.JE_AgentsReferenceInfo; }, delegate
			{ return sourceShipment?.JS_BookingReference; }, delegate
			{ return new[] { sourceShipment?.JS_BookingReferenceInfo }; }, delegate
			{ return false; });
			fieldSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			// We have not exploded even though destinationDeclaration is null
			destinationDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			sourceShipment.JS_BookingReference = "BR";
			AssertEquals("BR", destinationDeclaration.JE_AgentsReference);
		}

		public void TestJobDocAddressPersistingSyncronizedValueIsSaved()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.MainAddress.OA_Address1 = "My Address";

			var sourceDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			sourceDeclaration.ContainerTerminalOperatorDocAddress.OrganisationPK = organization.PK;
			AssertEquals("Pre-condition: sourceDeclaration.ContainerTerminalOperatorDocAddress.AddressAsASingleLine", "MY ADDRESS", sourceDeclaration.ContainerTerminalOperatorDocAddress.AddressAsASingleLine);

			var destinationDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var fieldSynchroniser = new FieldSynchroniser(destinationDeclaration.ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo, sourceDeclaration.ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo);
			fieldSynchroniser.JobDocAddressPersistingSyncronizedValue = destinationDeclaration.ContainerTerminalOperatorDocAddress;
			fieldSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			destinationDeclaration.ContainerTerminalOperatorDocAddress.HasChanges = false;
			AssertEquals("Pre-condition: destinationDeclaration.ContainerTerminalOperatorDocAddress.AddressAsASingleLine", "MY ADDRESS", destinationDeclaration.ContainerTerminalOperatorDocAddress.AddressAsASingleLine);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobDocAddress = newFactory.Load<JobDocAddress>(destinationDeclaration.ContainerTerminalOperatorDocAddress.PK);
			AssertNotNull("destinationDeclaration.ContainerTerminalOperatorDocAddress", jobDocAddress);
			AssertEquals("destinationDeclaration.ContainerTerminalOperatorDocAddress", "MY ADDRESS", jobDocAddress.AddressAsASingleLine);
		}

		public void TestJobDocAddressPersistingSyncronizedValueWithSourceNotValidNotSaved()
		{
			var sourceDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			sourceDeclaration.ContainerTerminalOperatorDocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("Pre-condition: sourceDeclaration.ContainerTerminalOperatorDocAddress.AddressAsASingleLine", ZString.Empty, sourceDeclaration.ContainerTerminalOperatorDocAddress.AddressAsASingleLine);

			var destinationDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var fieldSynchroniser = new FieldSynchroniser(destinationDeclaration.ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo, sourceDeclaration.ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo);
			fieldSynchroniser.JobDocAddressPersistingSyncronizedValue = destinationDeclaration.ContainerTerminalOperatorDocAddress;
			fieldSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			destinationDeclaration.ContainerTerminalOperatorDocAddress.HasChanges = false;
			AssertEquals("Pre-condition: destinationDeclaration.ContainerTerminalOperatorDocAddress.AddressAsASingleLine", ZString.Empty, destinationDeclaration.ContainerTerminalOperatorDocAddress.AddressAsASingleLine);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobDocAddress = newFactory.Load<JobDocAddress>(destinationDeclaration.ContainerTerminalOperatorDocAddress.PK);
			AssertNull("destinationDeclaration.ContainerTerminalOperatorDocAddress", jobDocAddress);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_WithNullDestinationArgument()
		{
			new FieldSynchroniser(null, DummyBusinessObject.New(Factory).Z0_AnotherDateInfo);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_WithNullSourceValueArgument()
		{
			var obj = DummyBusinessObject.New(Factory);
			new FieldSynchroniser(obj.Z0_AnotherDateInfo, null, () => new[] { obj.Z0_AnotherDecimalInfo });
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_WithNullInfosToHookValueChangedEventArgument()
		{
			var obj = DummyBusinessObject.New(Factory);
			new FieldSynchroniser(obj.Z0_AnotherDateInfo, () => ZString.Empty, null);
		}

		public void TestFieldSynchroniserIsEnabledByDefault()
		{
			var obj = DummyBusinessObject.New(Factory);
			var synchroniser = new FieldSynchroniser(obj.Z0_AnotherDateInfo, () => ZString.Empty, () => new[] { obj.Z0_AnotherNumberInfo });
			AssertEquals("Default Value of Synchroniser.Enabled", true, synchroniser.IsEnabled);
		}

		public void TestDelegateSynchroniser()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_BookingReference = "BK123";
			consol.JK_MasterBillNum = "MB123";
			shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB123";
			declaration = Factory.New<BaseJobDeclaration>();
			new FieldSynchroniser(declaration.JE_HouseBillInfo,
								  delegate
								  {
									  if (declaration.IsAir)
									  {
										  return shipment.JS_HouseBill;
									  }

									  return declaration.IsSea ? consol.JK_BookingReference : consol.JK_MasterBillNum;
								  }, () => new[] { declaration.JE_TransportModeInfo, consol.JK_MasterBillNumInfo, consol.JK_BookingReferenceInfo, shipment.JS_HouseBillInfo });

			declaration.JE_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Destination Field", "MB123", declaration.JE_HouseBill);
			consol.JK_MasterBillNum = "MB321";
			AssertEquals("Destination Field", "MB321", declaration.JE_HouseBill);

			declaration.JE_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Destination Field", "HB123", declaration.JE_HouseBill);
			shipment.JS_HouseBill = "HB321";
			AssertEquals("Destination Field", "HB321", declaration.JE_HouseBill);

			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Destination Field", "BK123", declaration.JE_HouseBill);
			consol.JK_BookingReference = "BK321";
			AssertEquals("Destination Field", "BK321", declaration.JE_HouseBill);
		}

		public void TestFieldSyncroniserDoesntSetReadOnlyWhenReadonlyDisabledParameterPassedIntoConstructor()
		{
			var source = DummyBusinessObject.New(Factory);
			var destination = DummyBusinessObject.New(Factory);
			var synchroniser = new FieldSynchroniser(destination.Z0_CodeInfo, source.Z0_CodeInfo, true);

			AssertEquals("Source.Z0_CodeInfo.ReadOnly", false, source.Z0_CodeInfo.ReadOnly);
			AssertEquals("Destination.Z0_CodeInfo.ReadOnly", false, destination.Z0_CodeInfo.ReadOnly);

			synchroniser.SetEnabled(false, false);
			AssertEquals("Source.Z0_CodeInfo.ReadOnly", false, source.Z0_CodeInfo.ReadOnly);
			AssertEquals("Destination.Z0_CodeInfo.ReadOnly", false, destination.Z0_CodeInfo.ReadOnly);

			synchroniser.SetEnabled(true, false);
			AssertEquals("Source.Z0_CodeInfo.ReadOnly", false, source.Z0_CodeInfo.ReadOnly);
			AssertEquals("Destination.Z0_CodeInfo.ReadOnly", false, destination.Z0_CodeInfo.ReadOnly);
		}

		public void TestLastValueNotChangedDoesNotResync()
		{
			var source = DummyBusinessObject.New(Factory);
			var destination = DummyBusinessObject.New(Factory);

			var synchroniser = new FieldSynchroniser(destination.Z0_CodeInfo, source.Z0_CodeInfo, true);

			source.Z0_Code = "SYNC";
			AssertEquals("SYNC", destination.Z0_Code);

			destination.Z0_Code = "NOSNC";
			source.Z0_Code = "SYNC";
			AssertEquals("NOSNC", destination.Z0_Code);

			source.Z0_Code = "SYNC2";
			AssertEquals("SYNC2", destination.Z0_Code);
		}

		public void TestJobDocAddressReadOnly()
		{
			var source = DummyBusinessObject.New(Factory);
			var destination = Factory.New<BaseJobDeclaration>();
			var synchroniser = new FieldSynchroniser(destination.DepotDocAddress.E2_OA_AddressInfo, source.Z0_CodeInfo);
			synchroniser.SetEnabled(false, false);
			AssertEquals("Readonly", false, destination.DepotDocAddress.ReadOnly);
			synchroniser.SetEnabled(true, false);
			AssertEquals("Readonly", true, destination.DepotDocAddress.ReadOnly);
		}

		public void TestFieldSyncroniserSetsAndResetsReadOnlyWhenEnabledIsChanged()
		{
			var source = DummyBusinessObject.New(Factory);
			var destination = DummyBusinessObject.New(Factory);

			AssertEquals("Precondition: Source.Z0_CodeInfo.ReadOnly", false, source.Z0_CodeInfo.ReadOnly);
			AssertEquals("Precondition: Destination.Z0_CodeInfo.ReadOnly", false, destination.Z0_CodeInfo.ReadOnly);

			var synchroniser = new FieldSynchroniser(destination.Z0_CodeInfo, source.Z0_CodeInfo);

			AssertEquals("Source.Z0_CodeInfo.ReadOnly", false, source.Z0_CodeInfo.ReadOnly);
			AssertEquals("Destination.Z0_CodeInfo.ReadOnly", true, destination.Z0_CodeInfo.ReadOnly);

			synchroniser.SetEnabled(false, false);
			AssertEquals("Source.Z0_CodeInfo.ReadOnly", false, source.Z0_CodeInfo.ReadOnly);
			AssertEquals("Destination.Z0_CodeInfo.ReadOnly", false, destination.Z0_CodeInfo.ReadOnly);

			synchroniser.SetEnabled(true, false);
			AssertEquals("Source.Z0_CodeInfo.ReadOnly", false, source.Z0_CodeInfo.ReadOnly);
			AssertEquals("Destination.Z0_CodeInfo.ReadOnly", true, destination.Z0_CodeInfo.ReadOnly);
		}

		public void TestFieldSynchroniserSynchronise()
		{
			shipment.JS_HouseBill = TestHousebillNum1;
			declaration.JE_HouseBill = TestHousebillNum2;

			var testSync = new FieldSynchroniser(declaration.JE_HouseBillInfo, shipment.JS_HouseBillInfo);

			AssertEquals("PreCondition:Shipment's House bill", TestHousebillNum1, shipment.JS_HouseBill);
			AssertEquals("PreCondition:Declaration's House bill", TestHousebillNum2, declaration.JE_HouseBill);

			testSync.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Shipment's House bill", TestHousebillNum1, shipment.JS_HouseBill);
			AssertEquals("Declaration's House bill", TestHousebillNum1, declaration.JE_HouseBill);

			shipment.JS_HouseBill = TestHousebillNum2;
			AssertEquals("Shipment's House bill", TestHousebillNum2, shipment.JS_HouseBill);
			AssertEquals("Declaration's House bill", TestHousebillNum2, declaration.JE_HouseBill);

			testSync.SetEnabled(false, false);
			shipment.JS_HouseBill = TestHousebillNum1;
			AssertEquals("Shipment's House bill", TestHousebillNum1, shipment.JS_HouseBill);
			AssertEquals("Declaration's House bill", TestHousebillNum2, declaration.JE_HouseBill);
		}

		public void TestFieldSynchroniserOnDeletedDestination()
		{
			shipment.JS_HouseBill = TestHousebillNum1;
			declaration.JE_HouseBill = TestHousebillNum2;

			var testSync = new FieldSynchroniser(declaration.JE_HouseBillInfo, shipment.JS_HouseBillInfo);
			testSync.SetEnabled(true, false);

			AssertEquals("PreCondition:Shipment's House bill", TestHousebillNum1, shipment.JS_HouseBill);
			AssertEquals("PreCondition:Declaration's House bill", TestHousebillNum2, declaration.JE_HouseBill);
			declaration.Delete();
			shipment.JS_HouseBill = TestHousebillNum3;
		}

		public void TestSynchronisationBasedOnWrappedPropertyWorks()
		{
			var source = new WrappedTestObject(Factory);
			var destination = DummyBusinessObject.New(Factory);

			var synchroniser = new FieldSynchroniser(destination.Z0_VarCharMaxInfo, () => source.DummyString, () => new[] { source.DummyStringInfo });
			source.wrappedObject.Z0_VarCharMax = "Testing Dummy";
			AssertEquals("Testing Dummy", destination.Z0_VarCharMax);

			source.wrappedObject.Z0_VarCharMax = "Dummy Blah";
			AssertEquals("Dummy Blah", destination.Z0_VarCharMax);

			source.DummyString = "Testing Dummy";
			AssertEquals("Testing Dummy", destination.Z0_VarCharMax);

			source.DummyString = "Dummy Blah";
			AssertEquals("Dummy Blah", destination.Z0_VarCharMax);
		}

		public void TestDeduplicateInfos()
		{
			shipment.JS_BookingReference = "BOOK123";
			var fieldSynchroniser = new FieldSynchroniser(declaration.JE_OwnerRefInfo, () => shipment.JS_BookingReference,
				delegate
				{
					var list = new List<ZPropertyInfo>(2);
					list.Add(shipment.JS_BookingReferenceInfo);
					list.Add(shipment.JS_BookingReferenceInfo); // this one should not cause an exception
					return list.ToArray();
				});
			fieldSynchroniser.Synchronise(true);
			AssertEquals("BOOK123", declaration.JE_OwnerRef);
		}

		public void TestUpdateInfoEventsAndReSynchronise()
		{
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL1";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL2";

			var fieldSynchroniser = new FieldSynchroniser(declaration.JE_OwnerRefInfo,
				delegate
				{
					shipment.Transports.Sort(Transport.Schema.JW_LegOrder);
					return shipment.Transports[0].JW_Vessel;
				},
				delegate
				{
					var list = new List<ZPropertyInfo>();
					shipment.Transports.Sort(Transport.Schema.JW_LegOrder);
					foreach (Transport transport in shipment.Transports)
					{
						list.Add(transport.JW_VesselInfo);
						list.Add(transport.JW_LegOrderInfo);
					}
					return list.ToArray();
				});
			fieldSynchroniser.Synchronise(true);
			AssertEquals("VESSEL1", declaration.JE_OwnerRef);

			transport2.JW_Vessel = "VESSEL2B";
			AssertEquals("VESSEL1", declaration.JE_OwnerRef);

			transport1.JW_Vessel = "VESSEL1A";
			AssertEquals("VESSEL1A", declaration.JE_OwnerRef);

			transport1.JW_LegOrder = 3;
			AssertEquals("VESSEL2B", declaration.JE_OwnerRef);

			transport1.JW_Vessel = "VESSEL1";
			AssertEquals("VESSEL2B", declaration.JE_OwnerRef);

			transport2.JW_Vessel = "VESSEL2";
			AssertEquals("VESSEL2", declaration.JE_OwnerRef);

			var transport3 = shipment.Transports.AddNew();
			transport3.JW_Vessel = "VESSEL3";
			transport3.JW_LegOrder = 1;
			AssertEquals("VESSEL2", declaration.JE_OwnerRef);

			fieldSynchroniser.UpdateInfoEventsAndReSynchronise();
			AssertEquals("VESSEL3", declaration.JE_OwnerRef);

			transport3.JW_Vessel = "VESSEL3A";
			AssertEquals("VESSEL3A", declaration.JE_OwnerRef);
		}

		public void TestDestinationEnabledDelegate()
		{
			var source = DummyBusinessObject.New(Factory);
			var destination = DummyBusinessObject.New(Factory);

			var synchroniser = new FieldSynchroniser(destination.Z0_CodeInfo, delegate
			{ return source.Z0_Code; }, delegate
			{ return new[] { source.Z0_CodeInfo }; }, true, delegate
			{ return destination.Z0_Code != "OFF"; });

			source.Z0_Code = "ON";
			AssertEquals("Destination should change", "ON", destination.Z0_Code);

			source.Z0_Code = "OFF";
			AssertEquals("Destination should change", "OFF", destination.Z0_Code);

			source.Z0_Code = "ON2";
			AssertEquals("Destination should not change", "OFF", destination.Z0_Code);

			destination.Z0_Code = "X";
			source.Z0_Code = "ON3";
			AssertEquals("Destination should change again", "ON3", destination.Z0_Code);
		}

		#region WrappedTestObject

		class WrappedTestObject : NonPersistentBusinessObject
		{
			DummyBusinessObject fwrappedObject;

			public WrappedTestObject(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZString DummyString
			{
				get { return wrappedObject.Z0_VarCharMax; }
				set { wrappedObject.Z0_VarCharMax = value; }
			}

			public ZPropertyInfo DummyStringInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(DummyString), x => wrappedObject.Z0_VarCharMaxInfo); }
			}

			public DummyBusinessObject wrappedObject
			{
				get { return fwrappedObject ?? (fwrappedObject = DummyBusinessObject.New(Factory)); }
			}
		}

		#endregion

		#region Implementation

		const string TestHousebillNum1 = "J2002290013";
		const string TestHousebillNum2 = "8473492";
		const string TestHousebillNum3 = "83330222011";
		BaseJobDeclaration declaration;
		ForwardingShipment shipment;

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<ForwardingShipment>();
			declaration = Factory.New<BaseJobDeclaration>();
		}

		#endregion
	}
}
