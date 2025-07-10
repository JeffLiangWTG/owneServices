using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USDeliveryOrderContainerAddInfo))]
	sealed class USDeliveryOrderContainerAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateContainerDetails()
		{
			Declaration.JE_MasterBill = "MB1";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TotalNoOfPacks = 10;
			Declaration.JE_TotalNoOfPacksPackType = ShippingOrPackingingUnitList.Codes.Board;
			CusContainer cusContainer = Declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "TURE1232122";
			cusContainer.CO_Seal = "SL23";
			cusContainer.CO_FCL_LCL_AIR = "FCL";
			cusContainer.CO_WeightUQ = Core.Constants.Weight.Kilotonnes;
			cusContainer.CO_Weight = 1.423m;
			cusContainer.JobContainer.JC_ArrivalSlotReference = "SL2334";
			cusContainer.JobContainer.JC_LCLStorageCommences = new ZDateTime(2010, 3, 12, 1, 1, 1);
			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.SetCountrySpecificContainerCode("FR", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			refContainer.RC_Code = "40FR";
			cusContainer.CO_RC = refContainer.PK;
			AssertEquals(1, Declaration.Packages.Count);
			var package = Declaration.Packages[0];
			AssertCollectionContains(package, cusContainer.Packages);
			Header.DeliveryOrderLines.RemoveAndDeleteAll();
			DeliveryOrderContainer container = Header.DeliveryOrderContainers.AddNew();
			container.US_ContainerNumber = "TURE1232122";
			AssertEquals("SL23", container.US_ContainerSeal);
			AssertEquals("FCL", container.US_ContainerMode);
			AssertEquals("40FR", container.US_ContainerType);
			AssertEquals("SL2334", container.US_SlotReference);
			AssertEquals(1.423m, container.US_Weight);
			AssertEquals(Core.Constants.Weight.Kilotonnes, container.US_WeightUQ);
			AssertEquals(new ZDateTime(2010, 3, 11), container.US_LastFreeDay);
			AssertEquals(0, Header.DeliveryOrderLines.Count);
			cusContainer.JobContainer.JC_ArrivalCTOStorageStartDate = new ZDateTime(2010, 3, 15, 1, 1, 1);
			container.Delete();
			container = Header.DeliveryOrderContainers.AddNew();
			container.US_ContainerNumber = "TURE1232122";
			AssertEquals("SL23", container.US_ContainerSeal);
			AssertEquals("FCL", container.US_ContainerMode);
			AssertEquals("40FR", container.US_ContainerType);
			AssertEquals("SL2334", container.US_SlotReference);
			AssertEquals(1.423m, container.US_Weight);
			AssertEquals(Core.Constants.Weight.Kilotonnes, container.US_WeightUQ);
			AssertEquals(new ZDateTime(2010, 3, 14), container.US_LastFreeDay);
			AssertEquals(0, Header.DeliveryOrderLines.Count);
		}

		protected override BusinessObject GetNewBusinessObject() => new USDeliveryOrderContainerAddInfo(Header.DeliveryOrderContainers.AddNew().B7_AddInfoDataInfo);

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				}

				return declaration;
			}
		}

		DeliveryOrderHeader header;
		DeliveryOrderHeader Header => header ?? (header = Declaration.DeliveryOrderHeaders.AddNew());
	}
}
