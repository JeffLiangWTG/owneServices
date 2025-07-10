using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageWorkSheetDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestRunSheetDocSupporterReturnsNullWithInvalidDataContext()
		{
			AssertEquals(null, iWorkSheet.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, null));
		}

		public void TestGetDocumentWrappers_GenericFreightJob()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var iWorkSheet = (IDocumentSupportable)workSheet;
			DocumentWrapper[] docs = iWorkSheet.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			var docSupporter = new CartageWorkSheetDocumentSupporter(workSheet);
			AssertEquals("Count of documents to be printed", 1, docs.Length);
			AssertEquals("Enterprise.DocumentWrappers.GenericWrappers.FreightWrapperFromRunSheet", docs[0].GetType().ToString());
		}

		public void TestGetDocumentWrappers_DocBuilder()
		{
			DocumentZQuery menuFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Leg Cartage Advice");
			menuFilter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.JobCartageRunSheet);
			DocumentCommand cartageMenu = Factory.LoadTop1<DocumentCommand>(menuFilter);
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move1 = cartage.ContainerBookedMoves.AddNew();
			var container1 = move1.Container;
			var leg1 = move1.CartageLegs.AddNew();
			var leg3 = move1.CartageLegs.AddNew();
			var move2 = cartage.ContainerBookedMoves.AddNew();
			var container2 = move2.Container;
			var leg2 = move2.CartageLegs.AddNew();
			var leg4 = move2.CartageLegs.AddNew();
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg1);
			workSheet.CartageLegs.Add(leg2);
			workSheet.CartageLegs.Add(leg3);
			workSheet.CartageLegs.Add(leg4);
			Constants.DataContext overridenDataContext = (Constants.DataContext)Enum.Parse(typeof(Constants.DataContext), cartageMenu.Documents[0].DocConfigs[0].S3_OverrideDataContext);
			AssertEquals("Should be overriden", Constants.DataContext.GenericLocalTransportLeg, overridenDataContext);
			CartageWorkSheetDocumentSupporter docSupporter = new CartageWorkSheetDocumentSupporter(workSheet);
			DocumentWrapper[] docs = docSupporter.GetDocumentWrappers(overridenDataContext, cartageMenu);
			AssertEquals("Count of documents to be printed", 4, docs.Length);
			workSheet.OnGetCartageLegsToPrint += new EventHandler<DocumentCartageLegEventArgs>(cartage_OnGetCartageLegsToPrint);
			docs = docSupporter.GetDocumentWrappers(overridenDataContext, cartageMenu);
			AssertEquals("Count of documents to be printed", 2, docs.Length);
			workSheet.OnGetCartageLegsToPrint -= new EventHandler<DocumentCartageLegEventArgs>(cartage_OnGetCartageLegsToPrint);
		}

		public void TestGetDocumentWrappers()
		{
			DocumentZQuery menuFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Leg Cartage Advice");
			var cartageMenu = Factory.LoadTop1<StmMenuItem>(menuFilter);
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move1 = cartage.ContainerBookedMoves.AddNew();
			var container1 = move1.Container;
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg3 = move1.CartageLegs.AddNew();
			var move2 = cartage.ContainerBookedMoves.AddNew();
			var container2 = move2.Container;
			CommonCartageLeg leg2 = move2.CartageLegs.AddNew();
			CommonCartageLeg leg4 = move2.CartageLegs.AddNew();
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg1);
			workSheet.CartageLegs.Add(leg2);
			workSheet.CartageLegs.Add(leg3);
			workSheet.CartageLegs.Add(leg4);
			CartageWorkSheetDocumentSupporter docSupporter = new CartageWorkSheetDocumentSupporter(workSheet);
			DocumentWrapper[] docs = docSupporter.GetDocumentWrappers(Constants.DataContext.CartageAdvice, cartageMenu);
			AssertEquals("Count of documents to be printed", 4, docs.Length);
			workSheet.OnGetCartageLegsToPrint += new EventHandler<DocumentCartageLegEventArgs>(cartage_OnGetCartageLegsToPrint);
			docs = docSupporter.GetDocumentWrappers(Constants.DataContext.CartageAdvice, cartageMenu);
			AssertEquals("Count of documents to be printed", 2, docs.Length);
			workSheet.OnGetCartageLegsToPrint -= new EventHandler<DocumentCartageLegEventArgs>(cartage_OnGetCartageLegsToPrint);
		}

		public void TestGetDocumentWrappers_NoLegWordInMenuName()
		{
			DocumentZQuery menuFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Cartage Advice");
			var cartageMenu = Factory.LoadTop1<StmMenuItem>(menuFilter);
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move1 = cartage.ContainerBookedMoves.AddNew();
			var container1 = move1.Container;
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg3 = move1.CartageLegs.AddNew();
			var move2 = cartage.ContainerBookedMoves.AddNew();
			var container2 = move2.Container;
			CommonCartageLeg leg2 = move2.CartageLegs.AddNew();
			CommonCartageLeg leg4 = move2.CartageLegs.AddNew();
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg1);
			workSheet.CartageLegs.Add(leg2);
			workSheet.CartageLegs.Add(leg3);
			workSheet.CartageLegs.Add(leg4);
			CartageWorkSheetDocumentSupporter docSupporter = new CartageWorkSheetDocumentSupporter(workSheet);
			DocumentWrapper[] docs = docSupporter.GetDocumentWrappers(Constants.DataContext.CartageAdvice, cartageMenu);
			AssertEquals("Count of documents to be printed", 4, docs.Length);
			workSheet.OnGetCartageLegsToPrint += new EventHandler<DocumentCartageLegEventArgs>(cartage_OnGetCartageLegsToPrint);
			docs = docSupporter.GetDocumentWrappers(Constants.DataContext.CartageAdvice, cartageMenu);
			AssertEquals("Count of documents to be printed", 2, docs.Length);
			workSheet.OnGetCartageLegsToPrint -= new EventHandler<DocumentCartageLegEventArgs>(cartage_OnGetCartageLegsToPrint);
		}

		void cartage_OnGetCartageLegsToPrint(object sender, DocumentCartageLegEventArgs e)
		{
			e.ContinueToPrint = true;
			for (int i = 0; i < e.DocumentCartageLegOptions.CartageLegs.Count; i++)
			{
				if (i < 2)
				{
					e.DocumentCartageLegOptions.CartageLegs[i].PrintCartageLeg = false;
				}
				else
				{
					e.DocumentCartageLegOptions.CartageLegs[i].PrintCartageLeg = true;
				}
			}
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("Constants.DataContext.ContainerLeg is Supported", true, iWorkSheet.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CommonWorkSheet)));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.TransportCustomiseDocuments, iWorkSheet.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.JobCartageRunSheet, iWorkSheet.DocumentSupporter.BusinessContext);
		}

		public void GetContactOrganisation()
		{
			AssertEquals(null, iWorkSheet.DocumentSupporter.GetContactOrganisation(null, ContactType.LocalTransport, DocumentDirection.ANY));
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			workSheet.EY_OH_TransportCo = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals(workSheet.EY_OH_TransportCo, iWorkSheet.DocumentSupporter.GetContactOrganisation(null, ContactType.LocalTransport, DocumentDirection.ANY).OrgHeader.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			workSheet = Factory.New<CommonWorkSheet>();
			iWorkSheet = workSheet;
		}

		CommonWorkSheet workSheet;
		IDocumentSupportable iWorkSheet;
	}
}
