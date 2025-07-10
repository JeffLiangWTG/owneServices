using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class LoadListDepotAutoPackingTestCase : BaseFreightTest
	{
		public void TestPack()
		{
			LoadList.AutomaticallyUpdatePackLineContainers = true;
			LoadList.JK_TransportMode = Constants.TransportModes.Sea;
			LoadList.Transports[0].JW_JX = Helper.SydLaxSailing.PK;
			CFSContainer container = LoadList.Containers.AddNew();
			CFSShipment shipment = LoadList.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;
			CFSPackLine pack = shipment.OuterPackLines.AddNew();
			Factory.Save();

			AssertEquals("1 unpacked, 0 packed", 1, LoadList.UnAllocatedPackLines.Count);
			AssertEquals("1 unpacked, 0 packed", 0, container.PackLines.Count);
			AssertEquals("no changes", false, LoadList.HasChanges);

			//Container.PackLines.Add(Pack);
			container.AddPackLine(pack);

			AssertEquals("0 unpacked, 1 packed", 0, LoadList.UnAllocatedPackLines.Count);
			AssertEquals("0 unpacked, 1 packed", 1, container.PackLines.Count);
			AssertEquals("changed by pack", true, LoadList.HasChanges);
			AssertEquals("should be CFS", ZBool.True, LoadList.JK_IsCFS);
		}

		public void TestUnpack()
		{
			LoadList.AutomaticallyUpdatePackLineContainers = true;
			LoadList.JK_TransportMode = Constants.TransportModes.Sea;
			LoadList.Transports[0].JW_JX = Helper.SydLaxSailing.PK;
			CFSContainer container = LoadList.Containers.AddNew();
			container.JC_JK = LoadList.PK;
			CFSShipment shipment = LoadList.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;
			CFSPackLine pack = shipment.OuterPackLines.AddNew();
			Factory.Save();

			AssertEquals("1 unpacked, 0 packed", 0, container.PackLines.Count);
			AssertEquals("1 unpacked, 0 packed", 1, LoadList.UnAllocatedPackLines.Count);
			AssertEquals("no changes", false, LoadList.HasChanges);

			//Container.PackLines.Add(Pack);
			container.AddPackLine(pack);

			AssertEquals("0 unpacked, 1 packed", 0, LoadList.UnAllocatedPackLines.Count);
			AssertEquals("0 unpacked, 1 packed", 1, container.PackLines.Count);
			AssertEquals("changed by pack", true, container.PackLines.HasChanges);

			Factory.Save();
			AssertEquals("just saved", false, LoadList.HasChanges);
			//Container.PackLines.Remove(Pack);
			container.RemovePackLine(pack);

			AssertEquals("1 unpacked, 0 packed", 1, LoadList.UnAllocatedPackLines.Count);
			AssertEquals("1 unpacked, 0 packed", 0, container.PackLines.Count);
			AssertEquals("changed by unpack", true, container.PackLines.HasChanges);
			AssertEquals("should be CFS", ZBool.True, LoadList.JK_IsCFS);
		}

		public void TestLoadListSailingAndClientAreDefaultedToAttachedContainers()
		{
			LoadList.AutomaticallyUpdatePackLineContainers = true;
			AssertEquals("This test is only valid when containers are automatically packed", true, LoadList.AutomaticallyUpdatePackLineContainers);

			LoadList.JK_TransportMode = Constants.TransportModes.Sea;
			LoadList.Transports[0].JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;
			LoadList.JK_OH_Forwarder = LocalForwarder.PK;
			CFSContainer container = LoadList.Containers.AddNew();
			AssertEquals("JC_JX should be the same as the load list", LoadList.Schedule.PK, container.JC_JX);
			AssertEquals("JC_OH_CFSClient should be the same as the load list", LoadList.JK_OH_Forwarder, container.JC_OH_CFSClient);

			LoadList.Transports[0].JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;
			LoadList.JK_OH_Forwarder = LocalDepot.PK;
			AssertEquals("JC_JX should update to be the same as the LoadList", LoadList.Schedule.PK, container.JC_JX);
			AssertEquals("JC_OH_CFSClient should update to be the same as the load list", LoadList.JK_OH_Forwarder, container.JC_OH_CFSClient);
		}

		public void TestAllocatingPacklinesGetRemovedFromUnAllocatedPackLineView()
		{
			LoadList.AutomaticallyUpdatePackLineContainers = true;
			LoadList.JK_TransportMode = Constants.TransportModes.Sea;
			LoadList.Transports[0].JW_JX = Helper.SydLaxSailing.PK;
			CFSContainer container = LoadList.Containers.AddNew();
			container.JC_RC = RC_40GP_PK;
			CFSShipment shipment = LoadList.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;
			CFSPackLine pack = shipment.OuterPackLines.AddNew();
			pack.JL_PackageCount = 5;
			Factory.Save();

			AssertEquals("1 unpacked, 0 packed", 1, LoadList.UnAllocatedPackLines.Count);
			AssertEquals("1 unpacked, 0 packed", 0, container.PackLines.Count);
			AssertEquals("no changes", false, LoadList.HasChanges);
			LoadList.AllocateShipment(shipment);
			AssertEquals("0 unpacked, 1 packed", 0, LoadList.UnAllocatedPackLines.Count);
			AssertEquals("0 unpacked, 1 packed", 1, container.PackLines.Count);
		}

		public void TestGetDocBusinessObjectForCommonConsol()
		{
			var consol = Factory.New<CFSLoadListConsol>();

			var queryProvider = new Mock<ICommonConsolDocumentSupporterQueryProvider>();

			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.Setup(m => m.GetConsolToPrint(It.Is<DocumentCommonConsol>(p => p.Consol == consol)))
				.Returns((DocumentCommonConsol)null);
			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LoadListDocument, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertEquals("LoadListConsol wrapper should be null", null, wrapper);
			queryProvider.Verify(m => m.GetConsolToPrint(It.Is<DocumentCommonConsol>(p => p.Consol == consol)), Times.Once());
			queryProvider
				.Setup(m => m.GetConsolToPrint(It.Is<DocumentCommonConsol>(p => p.Consol == consol)))
				.Returns(new DocumentCommonConsol(consol, Core.Constants.DataContext.LoadListDocument));
			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LoadListDocument, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertEquals("LoadListConsol wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocLoadListConsol", "DocLoadListConsol", wrapper[0].GetType().Name);
			queryProvider.Verify(m => m.GetConsolToPrint(It.Is<DocumentCommonConsol>(p => p.Consol == consol)),
				Times.Exactly(2));
		}

		public void TestOnFactorySaveDoesNotLoadTheWholeConsolTree()
		{
			CFSLoadListConsol consol1 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol1.JK_UniqueConsignRef = "C001";

			CFSShipment shipment1_1 = consol1.Shipments.AddNew();
			shipment1_1.JS_UniqueConsignRef = "S001-1";
			shipment1_1.JS_ActualWeight = 1;
			shipment1_1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment1_1.OuterPackLines.AddNew();

			CFSShipment shipment1_2 = consol1.Shipments.AddNew();
			shipment1_2.JS_UniqueConsignRef = "S001-2";
			shipment1_2.JS_ActualWeight = 2;
			shipment1_2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment1_2.OuterPackLines.AddNew();

			CFSLoadListConsol consol2 = shipment1_2.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C002";
			CFSShipment shipment2_1 = consol2.Shipments.AddNew();
			shipment2_1.JS_UniqueConsignRef = "S002-1";
			shipment2_1.JS_ActualWeight = 4;
			shipment2_1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment2_1.OuterPackLines.AddNew();

			CFSLoadListConsol consol3 = shipment2_1.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C003";
			CFSShipment shipment3_1 = consol3.Shipments.AddNew();
			shipment3_1.JS_UniqueConsignRef = "S003-1";
			shipment3_1.JS_ActualWeight = 8;
			shipment3_1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment3_1.OuterPackLines.AddNew();

			CFSLoadListConsol consol4 = shipment3_1.Consols.AddNew();
			consol4.JK_UniqueConsignRef = "C004";
			CFSShipment shipment4_1 = consol3.Shipments.AddNew();
			shipment4_1.JS_UniqueConsignRef = "S004-1";
			shipment4_1.JS_ActualWeight = 16;
			shipment4_1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment4_1.OuterPackLines.AddNew();

			CFSLoadListConsol consol5 = shipment4_1.Consols.AddNew();
			consol5.JK_UniqueConsignRef = "C005";
			CFSShipment shipment5_1 = consol3.Shipments.AddNew();
			shipment5_1.JS_UniqueConsignRef = "S005-1";
			shipment5_1.JS_ActualWeight = 32;
			shipment5_1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment5_1.OuterPackLines.AddNew();

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			CFSLoadListConsol consol1OnNewFactory = newFactory.Load<CFSLoadListConsol>(consol1.PK);

			newFactory.Save();

			var loadedShipmentsAndConsols = from bizO in ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects
											where bizO is CFSLoadListConsol || bizO is CFSShipment
											select bizO is CFSLoadListConsol ? ((CFSLoadListConsol)bizO).JK_UniqueConsignRef : ((CFSShipment)bizO).JS_UniqueConsignRef;

			AssertContainsExactElementsInAnyOrder(new ZString[] { "C001" }, loadedShipmentsAndConsols);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			new ConstantsAndReusables(Factory).EnsureCurrentCompanyMatchesCurrentBranch();
			Helper = new SailingsForTestClasses(Factory);
			LoadList = Factory.New<AutoPackedLoadListForTest>();
		}

		protected OrgHeader CreateForwarder(string manifestProviderID)
		{
			return CreateForwarder(manifestProviderID, Factory);
		}

		protected OrgHeader CreateForwarder(string manifestProviderID, BusinessObjectFactory orgFactory)
		{
			OrgHeader result = orgFactory.New<OrgHeader>();
			result.OH_FullName = "Forwarder " + manifestProviderID;
			result.MainAddress.OA_Address1 = "Address 1 " + manifestProviderID;
			result.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			OrgCusCode code = result.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.ManifestProviderID;
			code.OK_RN_NKCodeCountry = "AU";
			return result;
		}

		public class AutoPackedLoadListForTest : CFSLoadListConsol
		{
			public AutoPackedLoadListForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				AutomaticallyUpdatePackLineContainers = true;
			}
		}

		#endregion

		protected AutoPackedLoadListForTest LoadList;
		protected SailingsForTestClasses Helper;
	}
}
