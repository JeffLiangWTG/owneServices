using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using IBaseJobComInvoiceHeader = Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class BaseShipmentTest : BaseFreightTest
	{
		#region Test CommonShipment Classes

		#region Do Not Load Job Docs And Cartage

		public virtual void TestDoNotLoadJobDocsAndCartage()
		{
			CommonShipment shipment = GetShipment();
			AssertEquals("Job Docs And Cartage should not be referenced in Creating the Shipment", false, shipment.IsDocsAndCartageSet);
		}

		#endregion

		#region Unpack a CommonShipment ensure it no longer has references to the Container through the pivot

		public void TestContainerDetailsAreCleared()
		{
			var shipment = GetShipment();
			var consol1 = shipment.Consols.AddNew();
			var container1 = consol1.Containers.AddNew();

			shipment.JS_OuterPacks = 5;
			var packLine = (shipment.OuterPackLines.Count == 0) ? shipment.OuterPackLines.AddNew() : shipment.OuterPackLines[0];

			packLine.SetContainer(container1.PK);
			AssertEquals("Packline should be packed into the Container", container1, packLine.GetContainer(consol1));
			Factory.Save();
			consol1.Shipments.Remove(shipment);

			var consol2 = shipment.Consols.AddNew();
			var container2 = consol2.Containers.AddNew();
			packLine.SetContainer(container2.PK);

			var pivotFilter = new ZQuery(JobContainerPackPivotSchema.J6_JL, packLine.PK);
			var matchingPivots = Factory.Load<JobContainerPackPivot>(pivotFilter);
			AssertEquals("There should only be one pivot returned for the pack line after it has been moved between consols", 1, matchingPivots.Length);
		}

		#endregion

		#region Related Business Objects

		public void TestBusinessObjectsWithRelatedNotes()
		{
			var shipment = GetShipment();

			var consignee = Factory.New<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;

			var consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;

			AssertContainsExactElementsInAnyOrder(new[] { consignor, consignee }, shipment.BusinessObjectsWithRelatedNotes);

			var container1 = shipment.Consols.AddNew().Containers.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.JS_OuterPacks = 10;
			shipment.OuterPackLines[0].SetContainer(shipment.Consols[0], container1);

			var container2 = shipment.Consols[0].Containers.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.JS_OuterPacks = 10;
			shipment.OuterPackLines[1].SetContainer(shipment.Consols[0], container2);

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { consignor, consignee, container1, container2 }, shipment.BusinessObjectsWithRelatedNotes);

			var declaration1 = Factory.New<IBaseJobDeclaration>();
			declaration1.JE_JS = shipment.PK;

			if (shipment.Declarations.Length > 0)
			{
				var branchOfAnotherCompany = Factory.New<GlbCompany>().Branches.AddNew();
				branchOfAnotherCompany.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				var declaration2 = Factory.New<IBaseJobDeclaration>();
				declaration2.JE_GB = branchOfAnotherCompany.PK;
				declaration2.JE_JS = shipment.PK;

				var invoice1ForDeclaration2 = Factory.New<IBaseJobComInvoiceHeader>();
				invoice1ForDeclaration2.JZ_JE = declaration2.PK;

				var invoice2ForDeclaration2 = Factory.New<IBaseJobComInvoiceHeader>();
				invoice2ForDeclaration2.JZ_JE = declaration2.PK;

				AssertContainsExactElementsInAnyOrder(
					new BusinessObject[]
					{
						consignor,
						consignee,
						container1,
						container2,
						(BusinessObject)declaration1,
						(BusinessObject)declaration2,
						(BusinessObject)invoice1ForDeclaration2,
						(BusinessObject)invoice2ForDeclaration2,
					},
					shipment.BusinessObjectsWithRelatedNotes);
			}
		}

		public void TestPacklinesInShipmentAndConsolSame()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol = shipment.Consols.AddNew();
			consol.AutomaticallyUpdatePackLineContainers = true;
			consol.Containers.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();
			AssertEquals("Shipment Outerpack count", 2, shipment.OuterPackLines.Count);
			AssertEquals("Consols Related Pack Lines", 2, consol.Containers[0].PackLines.Count);

			foreach (PackLine packLine in shipment.OuterPackLines)
			{
				PackLine containerPackLine = null;

				foreach (PackLine packLineInContainer in consol.Containers[0].PackLines)
				{
					if (packLine.PK == packLineInContainer.PK)
					{
						containerPackLine = packLineInContainer;
					}
				}

				AssertEquals("PackLine same on CommonShipment and Container", packLine, containerPackLine);
			}
		}

		public void TestShipmentPackLinesInContainersPackLines()
		{
			CommonShipment master = GetShipment();
			master.Consols.AddNew();
			master.Consols[0].Containers.AddNew();
			master.OuterPackLines.AddNew();
			master.JS_OuterPacks = 10;
			master.OuterPackLines[0].SetContainer(master.Consols[0], master.Consols[0].Containers[0]);
			AssertEquals("Shipment should have 1 Container", 1, master.Containers.Count());
			AssertEquals("Shipment PackLines - CommonShipment Container PackL Line", master.OuterPackLines[0], master.Containers.First().PackLines[0]);
		}

		public void TestShipmentContainersAreSameAsConsolsContainers()
		{
			CommonShipment master = GetShipment();
			master.Consols.AddNew();
			master.Consols[0].Containers.AddNew();
			master.OuterPackLines.AddNew();
			master.JS_OuterPacks = 10;
			master.OuterPackLines[0].SetContainer(master.Consols[0], master.Consols[0].Containers[0]);
			AssertEquals("Shipments Container Count", 1, master.Containers.Count());
			AssertEquals("Shipments Container - CommonShipment Consol Container", master.Containers.First(), master.Consols[0].Containers[0]);
		}

		#endregion

		#endregion

		#region ContainersChildEditableRegistration

		public void TestContainersChildEditableRegistration()
		{
			CommonShipment shipment = GetShipment();
			var containers = shipment.ContainersForBinding;
			Assert(!shipment.IsRegisteredEditableChildObject(containers));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			shipment = GetShipment();
			containers = shipment.ContainersForBinding;
			AssertEquals(ContainersNeedToBeEditableRegistered, shipment.IsRegisteredEditableChildObject(containers));
		}

		protected virtual bool ContainersNeedToBeEditableRegistered
		{
			get { return false; }
		}

		#endregion

		#region ConsolsChildEditableRegistration

		public void TestConsolsChildEditableRegistration()
		{
			AssertEquals(ChildEditableServiceStates.Consol, ChildEditableService.GetState(Factory));
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", Core.Constants.ShipmentTypes.StandardHouse, Factory);
			var consols = shipment.Consols;
			AssertEquals(false, shipment.IsRegisteredEditableChildObject(consols));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", Core.Constants.ShipmentTypes.StandardHouse, Factory);
			consols = shipment.Consols;
			AssertEquals(true, shipment.IsRegisteredEditableChildObject(consols));
		}

		public void TestConsolsChildEditableRegistrationForOrderState()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Order);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", Core.Constants.ShipmentTypes.StandardHouse, Factory);
			var consols = shipment.Consols;
			AssertEquals(true, shipment.IsRegisteredEditableChildObject(consols));
		}

		#endregion //ConsolsChildEditableRegistration

		#region Implementation

		protected abstract CommonShipment GetShipment();

		protected virtual CommonShipment GetImportShipment()
		{
			CommonShipment result = GetShipment();
			result.JS_RL_NKDestination = HomePort;
			result.JS_RL_NKOrigin = OverseasPort;

			result.ConsigneeDocumentaryAddress.E2_OA_Address = CreateTestLocalConsignee().MainAddress.PK;
			result.ConsignorDocumentaryAddress.E2_OA_Address = CreateTestOverseasConsignor().MainAddress.PK;

			return result;
		}

		protected virtual CommonShipment GetExportShipment()
		{
			CommonShipment result = GetShipment();
			result.JS_RL_NKDestination = OverseasPort;
			result.JS_RL_NKOrigin = HomePort;

			result.ConsigneeDocumentaryAddress.E2_OA_Address = CreateTestOverseasConsignee().MainAddress.PK;
			result.ConsignorDocumentaryAddress.E2_OA_Address = CreateTestLocalConsignor().MainAddress.PK;

			return result;
		}

		protected virtual CommonShipment GetTranshipment()
		{
			CommonShipment result = GetShipment();
			result.JS_RL_NKDestination = OverseasPort;
			result.JS_RL_NKOrigin = OverseasPort2;

			result.ConsigneeDocumentaryAddress.E2_OA_Address = CreateTestOverseasConsignee().MainAddress.PK;
			result.ConsignorDocumentaryAddress.E2_OA_Address = CreateTestOverseasConsignor().MainAddress.PK;

			return result;
		}

		protected virtual CommonShipment GetDomestic()
		{
			CommonShipment result = GetShipment();
			result.JS_RL_NKDestination = HomePort;
			result.JS_RL_NKOrigin = AlternateHomePort;

			result.ConsigneeDocumentaryAddress.E2_OA_Address = CreateTestOverseasConsignee().MainAddress.PK;
			result.ConsignorDocumentaryAddress.E2_OA_Address = CreateTestLocalConsignor().MainAddress.PK;

			return result;
		}

		protected override string TestingCountry
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != HomePort.SubstringSafe(0, 2))
				{
					return HomePort.SubstringSafe(0, 2);
				}

				return null;
			}
		}

		public CommonShipment GetExportShipment2(Type shipmentType)
		{
			CommonShipment result = (CommonShipment)Factory.New(shipmentType);
			result.JS_RL_NKDestination = OverseasPort;
			result.JS_RL_NKOrigin = HomePort;
			result.ConsigneePK = OverseasConsignee.PK;
			result.ConsignorPK = LocalConsignor.PK;
			return result;
		}

		public CommonShipment GetImportShipment2(Type shipmentType)
		{
			CommonShipment result = (CommonShipment)Factory.New(shipmentType);
			result.JS_RL_NKDestination = HomePort;
			result.JS_RL_NKOrigin = OverseasPort;
			result.ConsigneePK = LocalConsignee.PK;
			result.ConsignorPK = OverseasConsignor.PK;
			return result;
		}

		#endregion
	}
}
