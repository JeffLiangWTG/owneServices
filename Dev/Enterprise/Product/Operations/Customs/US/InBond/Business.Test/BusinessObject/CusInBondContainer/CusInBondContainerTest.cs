using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondContainer))]
	sealed class CusInBondContainerTest : Customs.Business.Testing.CusInBondContainerTest<CusInBondContainer>
	{
		public void TestUniversalCopy()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])typeof(CusInBondContainer).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false);
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertEquals(4, ignoreElementAttributes[0].ElementNames.Count);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.UNDGDataItems, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInBondVehicleCtrls, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInBondCargoDescs, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondContainer.Schema.BC_MessageStatus, ignoreElementAttributes[0].ElementNames);

			var commodities = typeof(CusInBondContainer).GetProperty(nameof(CusInBondContainer.Commodities), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(commodities, typeof(UniversalCopyCollectionEntityAttribute)));

			var uNDGs = typeof(CusInBondContainer).GetProperty(nameof(CusInBondContainer.UNDGs), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(uNDGs, typeof(UniversalCopyCollectionEntityAttribute)));
		}

		public void TestIResetToOriginal()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew("APLU", "654987");
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			moveHeader.InBondNumber = "Inbond1";
			container.BC_MessageStatus = "AAV";
			bill.B0_MasterBillNumber = "Bill1";
			container.BC_ContainerNum = "Container1";
			var iResetToOriginal = container as IResetToOriginal;
			AssertEquals("Inbond1", iResetToOriginal.InBondNumber);
			AssertEquals("Inbond1", iResetToOriginal.MovementDescription);
			AssertEquals("AAV", iResetToOriginal.CustomsStatus);
			Assert(container.IsResetableToOriginal);
			AssertEquals("Bill1", iResetToOriginal.BillNumber);
			AssertEquals("Container1", iResetToOriginal.ContainerNumber);
			AssertEquals("Containers", iResetToOriginal.Level);
			iResetToOriginal.ResetStatus("");
			AssertEquals(ZString.Empty, container.BC_MessageStatus);
		}

		public void TestIControllerIDProviderMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var container = bill.MovementDetail.Containers.AddNew();
			IControllerIDProvider provider = container;
			AssertEquals("ControllerID", ControllerIDs.Customs.US.InBond, provider.ControllerID);
			AssertEquals("BusinessObjectPK", header.PK.ToGuid(), provider.BusinessObjectPK);
			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			AssertEquals("ControllerID", ControllerIDs.JobShipment, provider.ControllerID);
			AssertEquals("BusinessObjectPK", shipment.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestIInBondLineDetailsHeaderMembers()
		{
			var (_, container) = GetHeaderAndContainer();
			IInBondLineDetailsHeader lineHeaderDetails = container;
			AssertEquals(container, lineHeaderDetails.Container);
			var hazardousLines = new List<IHazardousMaterial>(lineHeaderDetails.HazardousLines);
			AssertEquals(0, hazardousLines.Count);
			CusInBondCargoDesc commodity1 = container.Commodities.AddNew();
			commodity1.BY_HarmonisedTariff = "2010102010";
			commodity1.BY_MarksAndNumbers = "HELLO BOB";
			CusInBondCargoDesc commodity2 = container.Commodities.AddNew();
			commodity2.BY_HarmonisedTariff = "1010102010";
			commodity2.BY_MarksAndNumbers = "HELLO JACK";
			CusInBondCargoDesc commodity3 = container.Commodities.AddNew();
			commodity3.BY_HarmonisedTariff = "1010103010";
			commodity3.BY_MarksAndNumbers = "HELLO WENDY";
			List<IInBondTariffLineDetails> tariffLines = new List<IInBondTariffLineDetails>(lineHeaderDetails.TariffLines);
			AssertEquals(3, tariffLines.Count);
			AssertEquals(commodity2, tariffLines[0]);
			AssertEquals(commodity3, tariffLines[1]);
			AssertEquals(commodity1, tariffLines[2]);
		}

		public void TestIInBondContainerMembers()
		{
			var (_, container) = GetHeaderAndContainer();
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			IInBondContainer inBondContainer = container;
			AssertEquals("ContainerDescriptionCode", "", inBondContainer.ContainerDescriptionCode);
			container.BC_RC = containerType.PK;
			AssertEquals("ContainerDescriptionCode", "40", inBondContainer.ContainerDescriptionCode);
			container.BC_ContainerNum = "TURE1235985";
			AssertEquals("ContainerNumber", "TURE1235985", inBondContainer.ContainerNumber);
			container.BC_Seal1 = "SEAL2342";
			AssertEquals("SealNumber1", "SEAL2342", inBondContainer.SealNumber1);
			container.BC_Seal2 = "SEAL6954";
			AssertEquals("SealNumber2", "SEAL6954", inBondContainer.SealNumber2);
		}

		public void TestLookups()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertEquals(typeof(CusInBondContainerLookups), container.Lookups.GetType());
		}

		public void TestValidation()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertEquals(typeof(CusInBondContainerValidation), container.Validation.GetType());
		}

		public void TestContainerDetailsDefaulting()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container.BC_ContainerNum = "TURE2122123";
			container.BC_RC = containerType.PK;
			container.BC_Seal1 = "SL123";
			container.BC_Seal2 = "SL456";
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			var container2 = moveDetail2.Containers.AddNew();
			container2.BC_ContainerNum = "TURE2122123";
			AssertEquals(containerType.PK, container2.BC_RC);
			AssertEquals("SL123", container2.BC_Seal1);
			AssertEquals("SL456", container2.BC_Seal2);
			container2.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertEquals(ZGuid.Empty, container2.BC_RC);
			AssertEquals(ZString.Empty, container2.BC_Seal1);
			AssertEquals(ZString.Empty, container2.BC_Seal2);
		}

		public void TestReadOnlyFields()
		{
			var (_, container) = GetHeaderAndContainer();
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container.BC_ContainerNum = ZString.Empty;
			container.BC_RC = containerType.PK;
			container.BC_Seal1 = "SL123";
			container.BC_Seal2 = "SL456";
			AssertEquals(false, container.BC_RCInfo.ReadOnly);
			AssertEquals(false, container.BC_Seal1Info.ReadOnly);
			AssertEquals(false, container.BC_Seal2Info.ReadOnly);
			container.BC_ContainerNum = "TURE2342322";
			AssertEquals(false, container.BC_RCInfo.ReadOnly);
			AssertEquals(false, container.BC_Seal1Info.ReadOnly);
			AssertEquals(false, container.BC_Seal2Info.ReadOnly);
			AssertEquals(containerType.PK, container.BC_RC);
			AssertEquals("SL123", container.BC_Seal1);
			AssertEquals("SL456", container.BC_Seal2);
			container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertEquals(true, container.BC_RCInfo.ReadOnly);
			AssertEquals(true, container.BC_Seal1Info.ReadOnly);
			AssertEquals(true, container.BC_Seal2Info.ReadOnly);
			AssertEquals(ZGuid.Empty, container.BC_RC);
			AssertEquals("", container.BC_Seal1);
			AssertEquals("", container.BC_Seal2);
		}

		public void TestPieceCountReadOnly()
		{
			var (header, container) = GetHeaderAndContainer();
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = false;
			CusInBondCargoDesc commodity1 = container.Commodities.AddNew();
			commodity1.BY_HarmonisedTariff = "2010102010";
			commodity1.BY_PieceCount = 50;
			commodity1.BY_MarksAndNumbers = "HELLO BOB";
			CusInBondCargoDesc commodity2 = container.Commodities.AddNew();
			commodity2.BY_HarmonisedTariff = "1010102010";
			commodity2.BY_MarksAndNumbers = "HELLO JACK";
			commodity2.BY_PieceCount = 70;
			CusInBondCargoDesc commodity3 = container.Commodities.AddNew();
			commodity3.BY_HarmonisedTariff = "1010103010";
			commodity3.BY_MarksAndNumbers = "HELLO WENDY";
			commodity3.BY_PieceCount = 80;
			AssertEquals(3, container.Commodities.Count);
			container.BC_PieceCount = 1234;
			AssertEquals(1234, container.BC_PieceCount);
			AssertEquals(50, container.Commodities[0].BY_PieceCount);
			AssertEquals(70, container.Commodities[1].BY_PieceCount);
			AssertEquals(80, container.Commodities[2].BY_PieceCount);
			header.BH_FTZMove = true;
			header.BH_OA_Importer = importer.MainAddress.PK;
			var flag = header.SupportsBondedWarehousing;
			AssertEquals(true, container.BC_PieceCountInfo.ReadOnly);
			AssertEquals(0, container.BC_PieceCount);
			commodity1.BY_PieceCount = 50;
			commodity2.BY_PieceCount = 70;
			commodity3.BY_PieceCount = 80;
			header.BH_FTZMove = false;
			AssertEquals(false, container.BC_PieceCountInfo.ReadOnly);
			AssertEquals(50, container.Commodities[0].BY_PieceCount);
			AssertEquals(70, container.Commodities[1].BY_PieceCount);
			AssertEquals(80, container.Commodities[2].BY_PieceCount);
		}

		public void TestDefaultChildLinesSuspender()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertEquals(false, container.IsDefaultChildLinesSuspended);
			using (container.SuspendDefaultingChildLines())
			{
				AssertEquals(true, container.IsDefaultChildLinesSuspended);
				using (container.SuspendDefaultingChildLines())
				{
					AssertEquals(true, container.IsDefaultChildLinesSuspended);
				}

				AssertEquals(true, container.IsDefaultChildLinesSuspended);
			}

			AssertEquals(false, container.IsDefaultChildLinesSuspended);
		}

		public void TestContainerDispositions()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew("APLU", "654987");
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "MRKU7934651";
			var iDispositions = container as IDispositionCodeDateParent;
			AssertEquals("Dispositions list", typeof(DispositionList), iDispositions.DispositionCodeDescriptionList.GetType());
			var container2 = moveDetail.Containers.AddNew();
			container2.BC_ContainerNum = "MRKU8209900";
			moveHeader.InBondNumber = "388172422";
			var message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			message.EM_MessageNum = "31250";
			message.EM_MessageText = "B018888XJ5NS                                               31250                " +
				"1062388172422   250597102                                                       " +
				"3013SAFM558221828                                   0000000023 1301311456PCWH   " +
				"4062388172422      2505    MRKU7934651                                          " +
				"50EQUIPMENT MRKU7934651 ARRIVED                                                 " +
				"50ON 130131 AT 2505                                                             " +
				"601MRKU7934651   E174913                                                        " +
				"60 MRKU8209900   E174914                                                        " +
				"Y  8888XJ5NS00007";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			AssertEquals(1, container.DispositionCodes.Count);
			AssertEquals(DispositionList.Codes._13, container.DispositionCodes[0].US_Code);
			AssertEquals(0, container2.DispositionCodes.Count);
		}

		public void TestDeleteContainers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container0 = declaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "CONT123456";
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT112233";
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MBTEST1";
			var package0 = declaration.Packages.AddNew();
			package0.CW_HouseBill = masterBill.CU_BillUniqueCode;
			package0.CW_ContainerNoOrEquipmentNo = "CONT123456";
			var package1 = declaration.Packages.AddNew();
			package1.CW_HouseBill = masterBill.CU_BillUniqueCode;
			package1.CW_ContainerNoOrEquipmentNo = "CONT112233";
			var header0 = Factory.New<CusInBondHeader>();
			header0.BH_ParentID = declaration.PK;
			header0.BH_ParentTableCode = declaration.TablePrefix;
			var synchronizer = (CusInBondHeaderDeclarationSynchronizer)header0.Synchroniser;
			synchronizer.Synchronise(true);
			AssertEquals(1, header0.Bills.Count);
			AssertEquals(1, header0.Bills[0].MoveDetails.Count);
			var containers = header0.Bills[0].MoveDetails[0].Containers;
			AssertEquals(2, containers.Count);
			Assert("Container can't be deleted.", !containers[0].CanDelete);
			AssertEquals(string.Format("This Container cannot be deleted {0}", ValidationConstants.Synchronize.SynchronizedFromParent(JobDeclarationSchema.Constants.TableName)), containers[0].ReasonForNotAbleToDelete);
			Assert("Container can't be deleted.", !containers[1].CanDelete);
			AssertEquals(string.Format("This Container cannot be deleted {0}", ValidationConstants.Synchronize.SynchronizedFromParent(JobDeclarationSchema.Constants.TableName)), containers[1].ReasonForNotAbleToDelete);
			header0.BH_OverrideFreightDefaults = true;
			Assert("Container can be deleted.", containers[0].CanDelete);
			Assert("Container can be deleted.", containers[1].CanDelete);
			var header1 = Factory.New<CusInBondHeader>();
			var bill = header1.Bills.AddNew();
			var moveDetail = bill.MoveDetails.AddNew();
			var container2 = moveDetail.Containers.AddNew();
			var container3 = moveDetail.Containers.AddNew();
			Assert("Container can be deleted.", container2.CanDelete);
			Assert("Container can be deleted.", container3.CanDelete);
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew("APLU", "654987");
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			return moveDetail.Containers.AddNew();
		}

		(CusInBondHeader, CusInBondContainer) GetHeaderAndContainer()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew("APLU", "654987");
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			return (header, container);
		}
	}
}
