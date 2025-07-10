using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CommonShipmentDataObjectWriterTest : UniversalShipmentDataObjectWriterTest
	{
		#region Implementation

		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager manager)
		{
			return new CommonShipmentDataObjectWriterForTest(manager);
		}

		protected override BusinessObject GetShipmentBusinessObject()
		{
			var shipment = Factory.New<ShipmentBizObjForTest>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USMIA";
			shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredAtPlace;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			shipment.JS_GoodsDescription = "frozen ducks";
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.BankSightDraft;
			shipment.JS_HBLAWBChargesDisplay = "SHW";
			shipment.JS_ShippedOnBoard = "LDN";

			shipment.JS_NoCopyBills = 2;
			shipment.JS_NoOriginalBills = 3;

			shipment.JS_ActualVolume = 20.30m;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicInches;
			shipment.JS_ActualWeight = 10.23m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			shipment.JS_InterimReceipt = "Interim_Receipt";

			shipment.JS_BookingReference = "BKGREF001";
			shipment.JS_CFSReference = "CFSREF001";

			shipment.JS_OuterPacks = 6;
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Basket;

			shipment.JS_RS_NKServiceLevel = "STD";

			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_Address1 = "20 Send Ln";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var consignor = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_Address1 = "20 Receive Ave";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			shipment.JS_E_DEP = new ZDateTime(2012, 5, 1);
			shipment.JS_E_ARV = new ZDateTime(2012, 5, 5);
			shipment.JS_A_BKD = new ZDateTime(2012, 5, 7);

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "snap frozen ducks for your convenience");

			var entryNumber = shipment.CusEntryNumbers.AddNew();
			entryNumber.CE_EntryType = "CAN";
			entryNumber.CE_EntryNum = "11111";

			var additionalRefNumber = shipment.Numbers.AddNew();
			additionalRefNumber.CE_EntryType = "XXX";
			additionalRefNumber.CE_EntryNum = "22222";

			var consol = shipment.Consols.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "XXXX001";
			container.JC_SealNum = "SEAL00";
			container.JC_SealParty = "CAR";
			container.JC_AdditionalSealNum = "SEAL01";
			container.JC_AdditionalSealParty = "CRD";
			container.JC_Additional2SealNum = "SEAL02";
			container.JC_Additional2SealParty = "CTO";
			container.JC_SetPointTempUnit = "C";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_Description = "Camel toes";
			packLine.SetContainer(container.PK);

			var transportLeg = shipment.Transports.AddNew();
			transportLeg.JW_RL_NKLoadPort = "USMIA";
			transportLeg.JW_RL_NKLoadPort = "SGSIN";

			return shipment;
		}

		protected override string GetExpectedDataObjectXml()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				return resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.TestFiles.CommonShipment_UniversalShipment.xml");
			}
		}

		#endregion

		#region SetUp/TearDown

		protected override void SetUp()
		{
			base.SetUp();

			var dataContextManager = new Mock<IShipmentDataContextManager>();
			var contextManagers = new Hashtable
			{
				{ nameof(DataContextType.DummyBusinessObject), new TestObjectHandle(dataContextManager.Object) }
			};

			universalDataContextManagersSubstitution = ObjectFactory.Substitute("UniversalDataContextManagers", contextManagers);
		}

		protected override void TearDown()
		{
			base.TearDown();

			universalDataContextManagersSubstitution.Dispose();
		}

		IDisposable universalDataContextManagersSubstitution;

		[UniversalDataContext(DataContextType.DummyBusinessObject)]
		class ShipmentBizObjForTest : CommonShipment
		{
			public ShipmentBizObjForTest(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}
		}

		class CommonShipmentDataObjectWriterForTest : CommonShipmentDataObjectWriter<CommonShipment>
		{
			public CommonShipmentDataObjectWriterForTest(IDataWritingManager manager)
				: base(manager)
			{
			}

			protected override DataContextType GetTopLevelDataContextType()
			{
				return DataContextType.DummyBusinessObject;
			}
		}

		#endregion
	}
}
