using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.Business;
	using Enterprise.MasterFiles.Business;

	public class BillNumberWrapperTest : TestCaseWithFactory
	{
		public void TestBillNumberWrapper()
		{
			CreateImportSeaJob();
			Declaration.Bill masterBill = (Declaration.Bill)JobDeclaration.PrimaryMasterBill;
			var wrappedBillNumber = new BillNumberWrapper(masterBill);
			AssertEquals("Master - BillNumber", "OB293042-24902", wrappedBillNumber.BillNumber);
			AssertEquals("Master Sea - BillType", "MB", wrappedBillNumber.BillType);

			Declaration.Bill houseBill = (Declaration.Bill)JobDeclaration.PrimaryHouseBill;
			wrappedBillNumber = new BillNumberWrapper(houseBill);
			AssertEquals("House - BillNumber", "HB92027", wrappedBillNumber.BillNumber);
			AssertEquals("House Sea - BillType", "BM", wrappedBillNumber.BillType);
		}

		public void TestParcelPost()
		{
			CreateImportPostJob();
			Declaration.Bill houseBill = (Declaration.Bill)JobDeclaration.PrimaryHouseBill;
			var wrappedBillNumber = new BillNumberWrapper(houseBill);
			AssertEquals("Parcel Number", "P243902Y", wrappedBillNumber.BillNumber);
			AssertEquals("Parcel - BillType", "ABU", wrappedBillNumber.BillType);
		}

		public void TestRelatedPackages()
		{
			CreateImportSeaJob();
			var houseBill = (Declaration.Bill)JobDeclaration.PrimaryHouseBill;
			var packingGroup = houseBill.PackingGroups[0];

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";
			Factory.Save();

			var wrappedBillNumber = new BillNumberWrapper(houseBill);
			AssertEquals("RelatedPackages", true, wrappedBillNumber.RelatedPackages.IsCountEqualTo(1));
			foreach (ZGuid packagePK in wrappedBillNumber.RelatedPackages)
			{
				AssertEquals("Related Package PK", packLine1.PK, packagePK);
			}
		}

		public void TestRelatedEquipment()
		{
			CreateImportSeaJob();
			var container = JobDeclaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "HLMU0344741";
			container.CO_ContainerSize = "40";
			container.CO_FCL_LCL_AIR = "FCL";
			container.CO_Weight = 1500m;
			container.CO_WeightUQ = "KG";

			var houseBill = (Declaration.Bill)JobDeclaration.PrimaryHouseBill;
			var packingGroup = houseBill.PackingGroups[0];
			packingGroup.CR_CO_Container = container.PK;

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";
			Factory.Save();

			var wrappedBillNumber = new BillNumberWrapper(houseBill);
			AssertEquals("RelatedEquipment", true, wrappedBillNumber.RelatedEquipment.IsCountEqualTo(1));
			foreach (ZGuid packingGroupPK in wrappedBillNumber.RelatedEquipment)
			{
				AssertEquals("Related Equipment PK", packingGroup.PK, packingGroupPK);
			}
		}

		#region Implementation

		JobDeclaration JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				}

				return jobDeclaration;
			}
		}
		JobDeclaration jobDeclaration;

		void CreateImportSeaJob()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_DeclarationReference = "BSIS00002309";
			JobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-14);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "SGSIN";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			Factory.Save();
		}

		void CreateImportPostJob()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_TransportMode = JobTransportModeList.Codes.Post;
			JobDeclaration.JE_TotalWeight = .35m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-14);
			JobDeclaration.JE_GoodsDescription = "PACKAGE";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "P243902Y";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PK";
			Factory.Save();
		}

		#endregion
	}
}
