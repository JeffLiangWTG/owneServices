using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSPackLineTest : BaseFreightTest
	{
		public void TestValidateJL_PackageCount()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			packLine.JL_PackageCount = 10;
			packLine.JL_Outturn = 10;
			CommonPickupDeliveryConfirm containerLeg = shipment.DestinationCFSDepartures.AddNew();
			CommonConfirmDivot divot = containerLeg.GetDivot(packLine);
			divot.J8_PackagesDelivered = 10;
			AssertEquals("PackLine should be fully delivered", true, packLine.IsFullyDelivered);

			packLine.JL_PackageCount = 9;
			AssertHasWarning(packLine.JL_PackageCountInfo, "10 packages have already been delivered.");

			packLine.JL_PackageCount = 11;
			AssertNoWarnings("Packline should have no warnings", packLine.JL_PackageCountInfo);
		}
		public void TestOutturnDimensionsOnlyEditableWhenOutturnsExist()
		{
			CFSPackLine newLine = Factory.NewWithValidTestData<CFSPackLine>();
			newLine.JL_PackageCount = 3;
			Factory.Save();

			// we need to test the read-only state of an object loaded direct from the DB
			// so that SetDefaultValues() isn't playing with the ReadOnly, thus the new factory
			BusinessObjectFactory freshFactory = new BusinessObjectFactory();
			CFSPackLine savedLine = freshFactory.Load<CFSPackLine>(newLine.PK);

			AssertEquals("Saved Pack Line Outturn length readonly when no outturns", true, savedLine.JL_OutturnedLengthInfo.ReadOnly);
			AssertEquals("Saved Pack Line Outturn height readonly when no outturns", true, savedLine.JL_OutturnedHeightInfo.ReadOnly);
			AssertEquals("Saved Pack Line Outturn width readonly when no outturns", true, savedLine.JL_OutturnedWidthInfo.ReadOnly);
			AssertEquals("Saved Pack Line Outturn width readonly when no outturns", true, savedLine.JL_OutturnedVolumeInfo.ReadOnly);
			AssertEquals("Saved Pack Line Outturn width readonly when no outturns", true, savedLine.JL_OutturnedWeightInfo.ReadOnly);

			savedLine.JL_Outturn = 2;
			AssertEquals("Pack Line Outturn length editable when outturns exist", false, savedLine.JL_OutturnedLengthInfo.ReadOnly);
			AssertEquals("Pack Line Outturn height editable when outturns exist", false, savedLine.JL_OutturnedHeightInfo.ReadOnly);
			AssertEquals("Pack Line Outturn width editable when outturns exist", false, savedLine.JL_OutturnedWidthInfo.ReadOnly);
			AssertEquals("Pack Line Outturn width editable when outturns exist", false, savedLine.JL_OutturnedVolumeInfo.ReadOnly);
			AssertEquals("Pack Line Outturn width editable when outturns exist", false, savedLine.JL_OutturnedWeightInfo.ReadOnly);

			savedLine.JL_Outturn = 0;
			AssertEquals("Pack Line Outturn length editable when no outturns", true, savedLine.JL_OutturnedLengthInfo.ReadOnly);
			AssertEquals("Pack Line Outturn height editable when no outturns", true, savedLine.JL_OutturnedHeightInfo.ReadOnly);
			AssertEquals("Pack Line Outturn width editable when no outturns", true, savedLine.JL_OutturnedWidthInfo.ReadOnly);
			AssertEquals("Pack Line Outturn width editable when outturns exist", true, savedLine.JL_OutturnedVolumeInfo.ReadOnly);
			AssertEquals("Pack Line Outturn width editable when outturns exist", true, savedLine.JL_OutturnedWeightInfo.ReadOnly);
		}

		public void TestIsFullyDeliveredWithLooseImportShipment()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm leg = shipment.DestinationCFSDepartures.AddNew();
			CommonConfirmDivot divot = leg.GetDivot(packLine);

			AssertUsingManifest(packLine, " the shipment is import", divot);
		}

		public void TestIsFullyDeliveredWithContainerisedImportShipment()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)GetImportConsol(typeof(CFSLoadListConsol));
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			CFSContainer container = consol.Containers.AddNew();
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();
			container.AddPackLine(shipment.OuterPackLines[0]);

			CommonPickupDeliveryConfirm leg = shipment.DestinationCFSDepartures.AddNew();
			CommonConfirmDivot divot = leg.GetDivot(packLine);

			container.JC_LCLUnpack = ZDateTime.Today;
			AssertUsingOutturn(packLine, " the shipment is a containersied Import ", divot);
		}

		public void TestIsFullyDeliveredWithExportShipmentWithNoOutturn()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm leg = shipment.DestinationCFSDepartures.AddNew();
			CommonConfirmDivot divot = leg.GetDivot(packLine);
			//Leg.JU_Leg = Constants.CartageLegType.ReturnToCNR;

			AssertUsingManifest(packLine, " the shipment is export and it doesn't have outturn", false, divot);
		}

		public void TestIsFullyDeliveredWithExportShipmentWithOutturn()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm leg = shipment.DestinationCFSDepartures.AddNew();
			CommonConfirmDivot divot = leg.GetDivot(packLine);
			//Leg.JU_Leg = Constants.CartageLegType.ReturnToCNR;

			AssertUsingOutturn(packLine, " the shipment is export and it's got outturn values", divot);
		}

		void AssertUsingManifest(CFSPackLine packLine, string reasonForUsingManifest, CommonConfirmDivot divot)
		{
			AssertUsingManifest(packLine, reasonForUsingManifest, true, divot);
		}

		void AssertUsingManifest(CFSPackLine packLine, string reasonForUsingManifest, bool dotheOutturn, CommonConfirmDivot divot)
		{
			packLine.JL_PackageCount = 10;
			packLine.JL_Outturn = dotheOutturn ? 11 : 0; //unusual put possible

			divot.J8_PackagesDelivered = 11;
			AssertEquals("This packline should not be fully delivered as it's using using manifest because " + reasonForUsingManifest, false, packLine.IsFullyDelivered);

			divot.J8_PackagesDelivered = 10;
			AssertEquals("This packline should be fully delivered as it's using using manifest because " + reasonForUsingManifest, true, packLine.IsFullyDelivered);
		}

		void AssertUsingOutturn(CFSPackLine packLine, string reasonForUsingOutturn, CommonConfirmDivot divot)
		{
			packLine.JL_PackageCount = 10;
			packLine.JL_Outturn = 11; //unusual put possible

			divot.J8_PackagesDelivered = 10;
			AssertEquals("This packline should not be fully delivered as it's using using outturn because " + reasonForUsingOutturn, false, packLine.IsFullyDelivered);

			divot.J8_PackagesDelivered = 11;
			AssertEquals("This packline should be fully delivered as it's using using outturn because " + reasonForUsingOutturn, true, packLine.IsFullyDelivered);
		}

		public void TestGetCustomsStatusDescriptionType()
		{
			var systemCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
				IPackLineStatusProvider packLineStatusProvider = (IPackLineStatusProvider)Activator.CreateInstance(ObjectFactory.GetType<IPackLineStatusProvider>());
				IPackLineStatus packLineStatus = packLineStatusProvider.GetPackLineStatus(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Enterprise.Customs.AU.Declaration.Business.PackLineStatus", packLineStatus.GetType().ToString());

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.NewZealand);
				packLineStatusProvider = (IPackLineStatusProvider)Activator.CreateInstance(ObjectFactory.GetType<IPackLineStatusProvider>());
				packLineStatus = packLineStatusProvider.GetPackLineStatus(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Enterprise.Customs.NZ.Business.Declaration.PackLineStatus", packLineStatus.GetType().ToString());

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.NewZealand);
				BusinessObject jobDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				jobDeclaration[JobDeclarationSchema.JE_MessageType] = "IMP";
				jobDeclaration[JobDeclarationSchema.JE_MessageSubType] = "NOR";
				jobDeclaration[JobDeclarationSchema.JE_TransportMode] = "SEA";
				jobDeclaration[JobDeclarationSchema.JE_MasterBill] = "OBL";
				jobDeclaration[JobDeclarationSchema.JE_HouseBill] = "HBL";
				jobDeclaration[JobDeclarationSchema.JE_EntryStatus] = "DOR";
				BusinessObject container = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
				container[CusContainerSchema.CO_JE] = jobDeclaration.PK;
				container[CusContainerSchema.CO_ContainerNumber] = "C1";
				Factory.Save();

				CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "OBL";
				CFSContainer testContainer = consol.Containers.AddNew();
				testContainer.JC_ContainerNum = "C1";
				CFSShipment shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HBL";
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				CFSPackLine packLine = shipment.OuterPackLines.AddNew();
				testContainer.AddPackLine(shipment.OuterPackLines[0]);
				AssertEquals("Delivery Order Received", packLine.CustomsStatusDescription);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(systemCountry);
			}
		}

		const string NoValueContainerPackingOrderError = "This value must be unique on the container.";
		const string DuplicateValueContainerPackingOrderError = " is entered on the shipment(s) ";

		public void TestJL_ContainerPackingOrderIsUnique()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			var consol = Factory.New<CFSLoadListConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			consol.Shipments.Add(shipment);

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.Containers.RemoveAll();
			packLine1.JL_JC = container1.PK;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.Containers.RemoveAll();
			packLine2.JL_JC = container1.PK;
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.Containers.RemoveAll();
			packLine3.JL_JC = container2.PK;
			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.Containers.RemoveAll();

			AssertNoError(packLine1.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoWarning(packLine1.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoError(packLine2.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoWarning(packLine2.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoError(packLine3.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoWarning(packLine3.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoError(packLine4.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoWarning(packLine4.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);

			packLine2.JL_ContainerPackingOrder = 1;
			packLine3.JL_ContainerPackingOrder = 1;
			packLine4.JL_ContainerPackingOrder = 1;
			AssertNoError(packLine1.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoWarning(packLine1.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertHasError(packLine2.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError + " Packing Order '1'" + DuplicateValueContainerPackingOrderError + "S00001001.");
			AssertNoWarning(packLine2.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError + " Packing Order '1'" + DuplicateValueContainerPackingOrderError + "S00001001.");

			AssertNoError(packLine3.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError + " Packing Order '1'" + DuplicateValueContainerPackingOrderError + "S00001001.");
			AssertNoWarning(packLine3.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError + " Packing Order '1'" + DuplicateValueContainerPackingOrderError + "S00001001.");
			AssertNoError(packLine4.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoWarning(packLine4.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);

			packLine1.Validation.ValidateJL_ContainerPackingOrder();
			AssertHasError(packLine1.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError + " Packing Order '1'" + DuplicateValueContainerPackingOrderError + "S00001001.");
			AssertHasError(packLine2.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError + " Packing Order '1'" + DuplicateValueContainerPackingOrderError + "S00001001.");

			AssertNoError(packLine3.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError + " Packing Order '1'" + DuplicateValueContainerPackingOrderError + "S00001001.");
			AssertNoWarning(packLine3.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError + " Packing Order '1'" + DuplicateValueContainerPackingOrderError + "S00001001.");
			AssertNoError(packLine4.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertNoWarning(packLine4.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);

			var packLine5 = shipment.OuterPackLines.AddNew();
			packLine5.Containers.RemoveAndDeleteAll();
			packLine5.JL_JC = container1.PK;

			packLine1.JL_ContainerPackingOrder = 0;
			packLine5.JL_ContainerPackingOrder = 0;
			AssertNoError(packLine5.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
			AssertHasWarning(packLine5.JL_ContainerPackingOrderInfo, NoValueContainerPackingOrderError);
		}

		public void TestArrivalAtCFSDate()
		{
			ZDateTime now = ZDateTime.Now;
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			CFSPackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			CFSPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 20;

			CommonPickupDeliveryConfirm leg1 = shipment.OriginCFSArrivals.AddNew();
			CommonConfirmDivot divot1 = leg1.GetDivot(packLine1);
			divot1.J8_PackagesDelivered = 10;
			CommonConfirmDivot divot2 = leg1.GetDivot(packLine2);
			divot2.J8_PackagesDelivered = 0;
			leg1.EU_PickupDeliveryTime = now;

			CommonPickupDeliveryConfirm leg2 = shipment.OriginCFSArrivals.AddNew();
			CommonConfirmDivot divot3 = leg2.GetDivot(packLine1);
			divot3.J8_PackagesDelivered = 0;
			CommonConfirmDivot divot4 = leg2.GetDivot(packLine2);
			divot4.J8_PackagesDelivered = 20;
			leg2.EU_PickupDeliveryTime = now.AddDays(1);

			AssertEquals("Arrival Date for Pack Line 1 should be today", now, packLine1.ArrivalAtCFS);
			AssertEquals("Arrival Date for Pack Line 2 should be tomorrow", now.AddDays(1), packLine2.ArrivalAtCFS);
		}
	}
}
