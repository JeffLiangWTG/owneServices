using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal sealed class EIDOShipmentMessagingDataTest : BaseAgencyTest
	{
		public void TestContainerHasChangesAffectingEIDO()
		{
			string[] properties = new string[]
			{
				JobContainerSchema.Constants.JC_ContainerNum,
				JobContainerSchema.Constants.JC_RC,
				JobContainerSchema.Constants.JC_JS_FCLBookingOnlyLink,
				JobContainerSchema.Constants.JC_OA_ArrivalContainerYardAddress,
				JobContainerSchema.Constants.JC_SealNum,
				JobContainerSchema.Constants.JC_AdditionalSealNum,
				JobContainerSchema.Constants.JC_Additional2SealNum,
			};

			AgencyShipmentContainer container = Factory.NewWithValidTestData<AgencyShipmentContainer>();
			AssertEquals("not in the database", true, EIDOShipmentMessagingData.HasChangesAffectingEIDO(container));

			Factory.Save();
			AssertEquals("freshly saved", false, EIDOShipmentMessagingData.HasChangesAffectingEIDO(container));

			for (int i = 0; i < properties.Length; i++)
			{
				ZPropertyInfo info = container.ZPropertyInfoHash[properties[i]];

				IZType oldValue = info.Value;

				ChangeValue(info);
				AssertEquals(info.Name + ": value changed", true, EIDOShipmentMessagingData.HasChangesAffectingEIDO(container));

				info.Value = oldValue;
				AssertEquals(info.Name + ": change undone", false, EIDOShipmentMessagingData.HasChangesAffectingEIDO(container));
			}
		}

		public void TestShipmentHasChangesAffectingEIDO()
		{
			string[] properties = new string[]
			{
				JobShipmentSchema.Constants.JS_HouseBill,
				JobShipmentSchema.Constants.JS_UniqueConsignRef,
				JobShipmentSchema.Constants.JS_OH_DeliveryAgent,
				JobShipmentSchema.Constants.JS_JX,
			};

			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			AssertEquals("not in the database", true, EIDOShipmentMessagingData.HasChangesAffectingEIDO(shipment));

			Factory.Save();
			AssertEquals("freshly saved", false, EIDOShipmentMessagingData.HasChangesAffectingEIDO(shipment));

			for (int i = 0; i < properties.Length; i++)
			{
				ZPropertyInfo info = shipment.ZPropertyInfoHash[properties[i]];

				IZType oldValue = info.Value;

				ChangeValue(info);
				AssertEquals(info.Name + ": value changed", true, EIDOShipmentMessagingData.HasChangesAffectingEIDO(shipment));

				info.Value = oldValue;
				AssertEquals(info.Name + ": change undone", false, EIDOShipmentMessagingData.HasChangesAffectingEIDO(shipment));
			}
		}

		public void TestGenerationNullShipment()
		{
			AssertExceptionThrown("Should have thrown an ArgumentNullException", typeof(ArgumentNullException), delegate
			{ EIDOShipmentMessagingData.NewOriginal(null); });
		}

		public void TestGenerationShouldNotHoldAReferenceToBusinessObjects()
		{
			AssertNoReferenceToBusinessObjects("FCL", delegate
				(out WeakReference reference)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();

				JobVoyage voyage = factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				voyage.GenerateSailings();

				AgencyShipment shipment = factory.New<AgencyShipment>();
				shipment.JS_JX = voyage.Sailings[0].PK;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				shipment.OuterPackLines.AddNew().JL_JC = shipment.RealContainers.AddNew().PK;
				shipment.OuterPackLines.AddNew().JL_JC = shipment.RealContainers.AddNew().PK;
				shipment.OuterPackLines.AddNew().JL_JC = shipment.RealContainers.AddNew().PK;

				AgencyShipmentContainer container = shipment.RealContainers.AddNew();
				container.JC_ContainerImportDORelease = "PIN";

				reference = new WeakReference(shipment);
				return EIDOShipmentMessagingData.NewOriginal(container);
			});
		}
		delegate object SetupWeakReference(out WeakReference reference);
		void AssertNoReferenceToBusinessObjects(string message, SetupWeakReference setup)
		{
			WeakReference reference;

			object data = setup(out reference);

			AssertNotNull(message + ": setup should set the reference", reference);
			AssertNotNull(message + ": setup should return some data", data);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			AssertEquals(message + ": should have collected the businessObject", false, reference.IsAlive);
		}

		public void TestMessageFunction()
		{
			IEIDOMessagingData data;

			data = EIDOShipmentMessagingData.NewOriginal(Factory.New<AgencyShipment>().RealContainers.AddNew());
			AssertEquals(EIDOMessageFunction.Original, data.MessageFunction);

			data = EIDOShipmentMessagingData.NewCancellation(Factory.New<AgencyShipment>().RealContainers.AddNew());
			AssertEquals(EIDOMessageFunction.Cancelation, data.MessageFunction);
		}

		[TestDate]
		public void TestMessagePrepared()
		{
			TestDateAttribute.Date = ZDateTime.Now.ToDateTime();

			IEIDOMessagingData data = EIDOShipmentMessagingData.NewOriginal(Factory.New<AgencyShipment>().RealContainers.AddNew());

			AssertEquals(TestDateAttribute.Date, data.MessagePrepared);
		}

		public void TestMessageRecipientNameAndAddress()
		{
			OrgHeader recipient = Factory.New<OrgHeader>();
			SetNameAndAddress(recipient.MainAddress, 'R');
			SetAcosCode(recipient, "MR-ACOS");

			JobVoyage voyage = Factory.New<JobVoyage>();
			JobSailing sailing = FindOrCreateSailing(voyage, OverseasPort, HomePort);
			AgencyShipment shipment = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();

			IEIDOOrganisation org;

			org = EIDOShipmentMessagingData.NewOriginal(container).MessageRecipient;
			AssertEquals(null, org);

			sailing.Destination.JB_OA_ArrivalCTOAddress = recipient.MainAddress.PK;
			org = EIDOShipmentMessagingData.NewOriginal(container).MessageRecipient;
			AssertNameAndAddress('R', org.NameAndAddress);
			AssertEquals("MR-ACOS", org.AcosCode);
		}

		public void TestMessageSenderNameAndAddress()
		{
			OrgHeader proxy = Factory.NewWithValidTestData<OrgHeader>();
			SetNameAndAddress(proxy.MainAddress, 'P');
			SetAcosCode(proxy, "MS-ACOS");
			Factory.Save();

			ZGuid oldProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			try
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = proxy.PK;

				AgencyShipment shipment = Factory.New<AgencyShipment>();
				AgencyShipmentContainer container = shipment.RealContainers.AddNew();

				IEIDOOrganisation org = EIDOShipmentMessagingData.NewOriginal(container).MessageSender;

				AssertNameAndAddress('P', org.NameAndAddress);
				AssertEquals("MS-ACOS", org.AcosCode);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = oldProxy;
			}
		}

		public void TestMessageIssuerNameAndAddress()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			SetNameAndAddress(principal.MainAddress, 'P');
			SetAcosCode(principal, "P-ACOS");

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = principal.PK;
			var container = shipment.RealContainers.AddNew();
			IEIDOOrganisation org = EIDOShipmentMessagingData.NewOriginal(container).Issuer;

			AssertNameAndAddress('P', org.NameAndAddress);
			AssertEquals("P-ACOS", org.AcosCode);
		}

		public void TestMessageCargoCollectionAddress()
		{
			OrgHeader cto = Factory.New<OrgHeader>();
			SetNameAndAddress(cto.MainAddress, 'C');
			SetAcosCode(cto, "CT-ACOS");

			JobVoyage voyage = Factory.New<JobVoyage>();
			JobSailing sailing = FindOrCreateSailing(voyage, OverseasPort, HomePort);
			AgencyShipment shipment = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();

			IEIDOOrganisation org;

			org = EIDOShipmentMessagingData.NewOriginal(container).CargoCollection;
			AssertEquals(null, org);

			sailing.Destination.JB_OA_ArrivalCTOAddress = cto.MainAddress.PK;
			org = EIDOShipmentMessagingData.NewOriginal(container).CargoCollection;
			AssertNameAndAddress('C', org.NameAndAddress);
			AssertEquals("CT-ACOS", org.AcosCode);
		}

		public void TestMessagePIN()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			AssertEquals("", EIDOShipmentMessagingData.NewOriginal(container).PIN);

			container.JC_ContainerImportDORelease = "pin";
			AssertEquals("pin", EIDOShipmentMessagingData.NewOriginal(container).PIN);
		}

		[EIDOMessagingConfiguration(Password = "RaNdOmPaSsWoRd")]
		public void TestMessagePassword()
		{
			OrgHeader principal = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "Principal");

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = principal.PK;

			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			AssertEquals("RaNdOmPaSsWoRd", EIDOShipmentMessagingData.NewOriginal(container).Password);
		}

		public void TestMessageReferenceNumber()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();

			shipment.JS_UniqueConsignRef = "S00000100";
			AssertEquals("S00000100", EIDOShipmentMessagingData.NewOriginal(container).ReferenceNumber);

			shipment.JS_UniqueConsignRef = "S00000101";
			AssertEquals("S00000101", EIDOShipmentMessagingData.NewOriginal(container).ReferenceNumber);
		}

		public void TestMessageBillOfLading()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();

			shipment.JS_HouseBill = "BLATICUS";
			AssertEquals("BLATICUS", EIDOShipmentMessagingData.NewOriginal(container).BillOfLading);

			shipment.JS_HouseBill = "BLAH";
			AssertEquals("BLAH", EIDOShipmentMessagingData.NewOriginal(container).BillOfLading);
		}

		public void TestMessageCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier Name";
			SetAcosCode(carrier, "CA-ACOS");

			IEIDOMessagingData data;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();

			data = EIDOShipmentMessagingData.NewOriginal(container);
			AssertEquals("", data.CarrierName);
			AssertEquals("", data.CarrierACOS);

			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			data = EIDOShipmentMessagingData.NewOriginal(container);
			AssertEquals("Carrier Name", data.CarrierName);
			AssertEquals("CA-ACOS", data.CarrierACOS);
		}

		public void TestMessageVesselName()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;

			JobSailing sailing = FindOrCreateSailing(voyage, OverseasPort, HomePort);

			AgencyShipment shipment1 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container1 = shipment1.RealContainers.AddNew();

			AgencyShipment shipment2 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container2 = shipment2.RealContainers.AddNew();

			Transport transport2a = shipment2.Transports.AddNew();
			transport2a.JW_Vessel = "BANOWATI";
			transport2a.JW_RL_NKLoadPort = HomePort;
			transport2a.JW_RL_NKDiscPort = AlternateHomePort;

			Transport transport2b = shipment2.Transports.AddNew();
			transport2b.JW_Vessel = "CONDOR";
			transport2b.JW_RL_NKLoadPort = OverseasPort2;
			transport2b.JW_RL_NKDiscPort = OverseasPort;

			AssertEquals("MAJAPAHIT", EIDOShipmentMessagingData.NewOriginal(container1).VesselName);
			AssertEquals("BANOWATI", EIDOShipmentMessagingData.NewOriginal(container2).VesselName);
		}

		public void TestMessageVesselLloyds()
		{
			var vessel1 = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).FirstOrDefault();
			var vessel2 = RefVessel.LookupVesselByName("BANOWATI", Factory).FirstOrDefault();
			var lloyds1 = vessel1?.RV_LloydsNumber ?? ZString.Empty;
			var lloyds2 = vessel2?.RV_LloydsNumber ?? ZString.Empty;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel1.RV_FK;

			JobSailing sailing = FindOrCreateSailing(voyage, OverseasPort, HomePort);

			AgencyShipment shipment1 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container1 = shipment1.RealContainers.AddNew();

			AgencyShipment shipment2 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container2 = shipment2.RealContainers.AddNew();

			Transport transport2a = shipment2.Transports.AddNew();
			transport2a.JW_Vessel = "BANOWATI";
			transport2a.JW_RL_NKLoadPort = HomePort;
			transport2a.JW_RL_NKDiscPort = AlternateHomePort;

			Transport transport2b = shipment2.Transports.AddNew();
			transport2b.JW_Vessel = "CONDOR";
			transport2b.JW_RL_NKLoadPort = OverseasPort2;
			transport2b.JW_RL_NKDiscPort = OverseasPort;

			AssertEquals(lloyds1, EIDOShipmentMessagingData.NewOriginal(container1).VesselLloyds);
			AssertEquals(lloyds2, EIDOShipmentMessagingData.NewOriginal(container2).VesselLloyds);
		}

		public void TestMessageVoyage()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "x42";

			JobSailing sailing = FindOrCreateSailing(voyage, OverseasPort, HomePort);

			AgencyShipment shipment1 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container1 = shipment1.RealContainers.AddNew();

			AgencyShipment shipment2 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container2 = shipment2.RealContainers.AddNew();

			Transport transport2a = shipment2.Transports.AddNew();
			transport2a.JW_VoyageFlight = "y42";
			transport2a.JW_RL_NKLoadPort = HomePort;
			transport2a.JW_RL_NKDiscPort = AlternateHomePort;

			Transport transport2b = shipment2.Transports.AddNew();
			transport2b.JW_VoyageFlight = "z42";
			transport2b.JW_RL_NKLoadPort = OverseasPort2;
			transport2b.JW_RL_NKDiscPort = OverseasPort;

			AssertEquals("x42", EIDOShipmentMessagingData.NewOriginal(container1).Voyage);
			AssertEquals("y42", EIDOShipmentMessagingData.NewOriginal(container2).Voyage);
		}

		public void TestMessageDischargePort()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();

			JobSailing sailing = FindOrCreateSailing(voyage, OverseasPort, HomePort);

			AgencyShipment shipment1 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container1 = shipment1.RealContainers.AddNew();

			AgencyShipment shipment2 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container2 = shipment2.RealContainers.AddNew();

			Transport transport2a = shipment2.Transports.AddNew();
			transport2a.JW_RL_NKLoadPort = HomePort;
			transport2a.JW_RL_NKDiscPort = AlternateHomePort;

			Transport transport2b = shipment2.Transports.AddNew();
			transport2b.JW_RL_NKLoadPort = OverseasPort2;
			transport2b.JW_RL_NKDiscPort = OverseasPort;

			AssertEquals(HomePort, EIDOShipmentMessagingData.NewOriginal(container1).DischargePort);
			AssertEquals(AlternateHomePort, EIDOShipmentMessagingData.NewOriginal(container2).DischargePort);
		}

		public void TestMessageEstimatedArrivalDate()
		{
			ZDateTime now = ZDateTime.Now;
			JobVoyage voyage = Factory.New<JobVoyage>();

			JobSailing sailing = FindOrCreateSailing(voyage, OverseasPort, HomePort);
			sailing.Origin.JA_E_DEP = now.AddDays(5);
			sailing.Destination.JB_E_ARV = now.AddDays(6);

			AgencyShipment shipment1 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container1 = shipment1.RealContainers.AddNew();

			AgencyShipment shipment2 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container2 = shipment2.RealContainers.AddNew();

			AgencyShipment shipment3 = NewShipment(sailing, null, false, true);
			AgencyShipmentContainer container3 = shipment3.RealContainers.AddNew();

			Transport transport2a = shipment2.Transports.AddNew();
			transport2a.JW_RL_NKLoadPort = HomePort;
			transport2a.JW_RL_NKDiscPort = AlternateHomePort;
			transport2a.JW_ETD = now.AddDays(7);
			transport2a.JW_ETA = now.AddDays(8);

			Transport transport2b = shipment2.Transports.AddNew();
			transport2b.JW_RL_NKLoadPort = OverseasPort2;
			transport2b.JW_RL_NKDiscPort = OverseasPort;
			transport2b.JW_ETD = now.AddDays(3);
			transport2b.JW_ETA = now.AddDays(4);

			Transport transport3 = shipment3.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = HomePort;
			transport3.JW_RL_NKDiscPort = AlternateHomePort2;

			AssertEquals(now.AddDays(6).ToDateTime(), EIDOShipmentMessagingData.NewOriginal(container1).EstimatedArrivalDate);
			AssertEquals(now.AddDays(8).ToDateTime(), EIDOShipmentMessagingData.NewOriginal(container2).EstimatedArrivalDate);
			AssertEquals(null, EIDOShipmentMessagingData.NewOriginal(container3).EstimatedArrivalDate);
		}

		public void TestEquipmentContainerISOCode()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = ContainerNum1;
			container1.JC_RC = RC_20GP_PK;

			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = ContainerNum2;
			container2.JC_RC = RC_40RE_PK;

			IDictionary<string, IEIDOEquiptmentData> equiptment;

			equiptment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container1));
			AssertEquals(container1.Container.RC_ISOType, equiptment[ContainerNum1].ContainerISOCode);

			equiptment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container2));
			AssertEquals(container2.Container.RC_ISOType, equiptment[ContainerNum2].ContainerISOCode);
		}

		public void TestEquipmentSealNumber()
		{
			IDictionary<string, IEIDOEquiptmentData> equiptment;

			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = ContainerNum1;

			container.JC_SealNum = "";
			container.JC_AdditionalSealNum = "";
			equiptment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container));
			AssertContainsExactElementsInAnyOrder(
				equiptment[ContainerNum1].SealNumbers,
				Array.Empty<string>());

			container.JC_SealNum = "Seal1";
			equiptment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container));
			AssertContainsExactElementsInAnyOrder(
				equiptment[ContainerNum1].SealNumbers,
				new string[] { "Seal1" });

			container.JC_AdditionalSealNum = "Seal2";
			equiptment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container));
			AssertContainsExactElementsInAnyOrder(
				equiptment[ContainerNum1].SealNumbers,
				new string[] { "Seal1", "Seal2" });

			container.JC_SealNum = "";
			equiptment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container));
			AssertContainsExactElementsInAnyOrder(
				equiptment[ContainerNum1].SealNumbers,
				new string[] { "Seal2" });
		}

		public void TestEquipmentGoodsDescription()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = ContainerNum1;

			IDictionary<string, IEIDOEquiptmentData> equiptment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container));
			AssertEquals("", equiptment[ContainerNum1].GoodsDescription);
		}

		public void TestEquipmentHandlingInstructions()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = ContainerNum1;

			IDictionary<string, IEIDOEquiptmentData> equipment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container));
			AssertEquals("", equipment[ContainerNum1].HandlingInstructions);
		}

		public void TestEquipmentIMDGClassAndCode()
		{
			UNDGSubstance substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			IDictionary<string, IEIDOEquiptmentData> equiptment;

			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = ContainerNum1;

			AgencyShipmentPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_JC = container.PK;

			equiptment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container));
			AssertEquals("", equiptment[ContainerNum1].IMDGClass);
			AssertEquals("", equiptment[ContainerNum1].IMDGClassCode);

			line.UNDGs.AddNew().DI_DG = substance.PK;
			equiptment = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container));
			AssertEquals(substance.DG_Class, equiptment[ContainerNum1].IMDGClass);
			AssertEquals(substance.DG_UNNO, equiptment[ContainerNum1].IMDGClassCode);
		}

		public void TestEquipmentEmptyReturnBy()
		{
			IEIDOOrganisation emptyReturn;

			var containerYard = Factory.New<OrgHeader>();
			SetNameAndAddress(containerYard.MainAddress, 'Y');
			SetAcosCode(containerYard, "CY-ACOS");

			var shipment = Factory.New<AgencyShipment>();

			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = ContainerNum1;

			emptyReturn = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container))[ContainerNum1].EmptyReturn;
			AssertNull(null, emptyReturn);

			container.JC_OA_ArrivalContainerYardAddress = containerYard.MainAddress.PK;
			emptyReturn = GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container))[ContainerNum1].EmptyReturn;
			AssertNameAndAddress('Y', emptyReturn.NameAndAddress);
			AssertEquals("CY-ACOS", emptyReturn.AcosCode);
		}

		public void TestEquipmentGrossKilograms()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = ContainerNum1;
			container1.JC_GrossWeight = 15000;
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = ContainerNum2;
			container2.JC_GrossWeightUQ = Core.Constants.Weight.Tonnes;
			container2.JC_GrossWeight = 16;

			AssertEquals(15000m, GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container1))[ContainerNum1].GrossKilograms);
			AssertEquals(16000m, GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container2))[ContainerNum2].GrossKilograms);
		}

		public void TestEquipmentIsEmpty()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = ContainerNum1;
			container.JC_IsEmptyContainer = true;
			AssertEquals(true, GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container))[ContainerNum1].IsEmpty);

			container.JC_IsEmptyContainer = false;
			AssertEquals(false, GetEquipmentHash(EIDOShipmentMessagingData.NewOriginal(container))[ContainerNum1].IsEmpty);
		}

		[TestDate(2016, 12, 25, 12, 59, 00)]
		public void TestGenerateEIDOMessage()
		{
			#region Setup OrgHeader/Voyage

			var cto = Factory.NewWithValidTestData<OrgHeader>();
			SetNameAndAddress(cto.MainAddress, "CTO");
			SetAcosCode(cto, "T-ACOS");

			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			SetNameAndAddress(principal.MainAddress, "PCP");
			SetAcosCode(principal, "P-ACOS");

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier Name";
			SetAcosCode(carrier, "C-ACOS");

			var emptyReturnContainerYard = Factory.NewWithValidTestData<OrgHeader>();
			SetNameAndAddress(emptyReturnContainerYard.MainAddress, "MTYARD");
			SetAcosCode(emptyReturnContainerYard, "MT-ACOS");

			var proxy = Factory.NewWithValidTestData<OrgHeader>();
			SetNameAndAddress(proxy.MainAddress, "PROXY");
			SetAcosCode(proxy, "X-ACOS");

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "Vyg";
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;

			Factory.Save();

			#endregion

			var eIDOHeader = new EIDOMessagingHeader();
			eIDOHeader.Email = "test@cargowise.com";

			var identity = eIDOHeader.Identities.AddNew();
			identity.PrincipalPK = principal.PK;
			identity.SenderID = "sender";
			identity.RecipientID = "recipient";
			identity.Password = "password";

			var sailing = FindOrCreateSailing(voyage, "SGSIN", "AUSYD");
			sailing.Origin.JA_E_DEP = new ZDateTime(2016, 12, 13);
			sailing.Destination.JB_E_ARV = new ZDateTime(2016, 12, 15);
			sailing.Destination.JB_OA_ArrivalCTOAddress = cto.MainAddress.PK;

			AgencyShipment shipment = NewShipment(sailing, principal, false, true);
			shipment.JS_UniqueConsignRef = "S00000100";
			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			shipment.JS_HouseBill = "HLB";

			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = ContainerNum1;
			container.JC_RC = RC_20GP_PK;
			container.JC_SealNum = "Seal1";
			container.JC_OA_ArrivalContainerYardAddress = emptyReturnContainerYard.MainAddress.PK;

			var currentProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			try
			{
				AgencyRegistry.Instance.EIDOMessagingDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eIDOHeader);
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = proxy.PK;

				var eIDOMessage = EIDOShipmentMessagingData.NewOriginal(container);
				var builderMessage = new EIDOEdifactMessageBuilder().GenerateMessageText(eIDOMessage);

				#region expected message

				var expectedMessage = "UNH+<<MSGNO PLACEHOLDER>>+IFCSUM:D:98B:UN:ANZ20'" +
									"BGM+640+<<MSGNO PLACEHOLDER>>+9+AB'" +
									"DTM+137:201612251259:203'" +
									"NAD+AV+PASSWORD'" +
									"NAD+MR+T-ACOS:160:184+COMPANY CTO:ADDRESS1 CTO:ADDRESS2 CTO:CITY CTO:ST CTO PC CTO'" +
									"NAD+MS+X-ACOS:160:184+COMPANY PROXY:ADDRESS1 PROXY:ADDRESS2 PROXY:CITY PROXY:ST PROXY PC PROXY'" +
									"TDT+20+VYG+1++C-ACOS::184:CARRIER NAME+++7920572:::MAJAPAHIT'" +
									"LOC+11+AUSYD'" +
									"DTM+132:20161215:102'" +
									"NAD+CA+P-ACOS:160:184+COMPANY PCP:ADDRESS1 PCP:ADDRESS2 PCP:CITY PCP:ST PCP PC PCP'" +
									"NAD+SF+T-ACOS:160:184+COMPANY CTO:ADDRESS1 CTO:ADDRESS2 CTO:CITY CTO:ST CTO PC CTO'" +
									"CNI+1+S00000100'" +
									"RFF+BM:HLB'" +
									"EQD+CN+FAKE4100011+22G0+++5'" +
									"MEA+AAE+G+KGM:2280'" +
									"SEL+SEAL1'" +
									"NAD+CR+MT-ACOS:160:184+COMPANY MTYARD:ADDRESS1 MTYARD:ADDRESS2 MTYARD:CITY MTYARD:ST MTYARD PC MTYARD'" +
									"UNT+18+<<MSGNO PLACEHOLDER>>'";

				#endregion

				AssertEquals(expectedMessage, builderMessage);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = currentProxy;
			}
		}

		#region Implementation

		void ChangeValue(ZPropertyInfo info)
		{
			if (info.PropertyType == typeof(ZString))
			{
				ZString old = (ZString)info.Value;
				if (old.IsEmpty)
				{
					info.Value = new ZString("X");
				}
				else if (old.EndsWith("X"))
				{
					info.Value = new ZString(old.Left(old.Length - 1) + "Y");
				}
				else
				{
					info.Value = new ZString(old.Left(old.Length - 1) + "X");
				}
			}
			else if (info.PropertyType == typeof(ZGuid))
			{
				info.Value = ZGuid.NewZGuid();
			}
			else
			{
				throw new InvalidOperationException("Dont know how to handle " + info.PropertyType.Name);
			}
		}

		void SetNameAndAddress(OrgAddress address, char c)
		{
			SetNameAndAddress(address, c.ToString());
		}

		void SetNameAndAddress(OrgAddress address, string c)
		{
			address.Header.OH_FullName = "Company " + c;
			address.OA_Address1 = "Address1 " + c;
			address.OA_Address2 = "Address2 " + c;
			address.OA_City = "City " + c;
			address.OA_State = "ST " + c;
			address.OA_PostCode = "PC " + c;
		}

		void AssertNameAndAddress(char c, string nameAndAddress)
		{
			AssertNameAndAddress("", c, nameAndAddress);
		}

		void AssertNameAndAddress(string message, char c, string nameAndAddress)
		{
			const string NameAndAddressFormat = "Company {0}\nAddress1 {0}\nAddress2 {0}\nCity {0}\nST {0} PC {0}";
			AssertMultilineASCIIEquals(message, string.Format(NameAndAddressFormat, c), nameAndAddress);
		}

		IDictionary<string, IEIDOEquiptmentData> GetEquipmentHash(IEIDOMessagingData data)
		{
			IDictionary<string, IEIDOEquiptmentData> result = new Dictionary<string, IEIDOEquiptmentData>();

			foreach (IEIDOEquiptmentData equipment in data.Equipment)
			{
				if (result.ContainsKey(equipment.ContainerNumber))
				{
					throw new ApplicationException("duplicate container found: " + equipment.ContainerNumber);
				}

				result.Add(equipment.ContainerNumber, equipment);
			}

			return result;
		}

		const string ContainerNum1 = "FAKE4100011";
		const string ContainerNum2 = "FAKE4100027";

		#endregion
	}
}
