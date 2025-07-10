using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(PackUnpackShipment))]
	class PackUnpackShipmentTest : CFSBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonShipment);
			}
		}

		#endregion

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
		}

		[ExpectNoExceptions]
		public void TestUnSavedShipmentCanBeDeleted()
		{
			BusinessObject shipment = GetNewBusinessObject();
			shipment.Delete();
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestSavedShipmentCannotBeDeleted()
		{
			BusinessObject shipment = GetNewBusinessObject();
			Factory.Save();
			shipment.Delete();
		}

		#region GetNewValidation

		public virtual void TestGetNewValidation()
		{
			PackUnpackShipment shipment = Factory.NewWithValidTestData<PackUnpackShipment>();
			AssertEquals("Type of Validation", typeof(PackUnpackShipmentValidation), shipment.Validation.GetType());
		}

		#endregion

		public void TestDefaultJS_TranshipToOtherCFS()
		{
			AssertEquals("should be true by default", true, Shipment.JS_TranshipToOtherCFS);
		}

		//		public void TestCollectionReturnsSameCollectionTwice()
		//		{
		//			AssertEquals("shouldn't be constantly reloaded", Shipment.OuterPackLinesViewedByParentContainer,
		//				Shipment.OuterPackLinesViewedByParentContainer);
		//		}
		//
		public void TestAllowSurplusPacks()
		{
			AssertEquals("false by default", false, Shipment.AllowSurplusPacks);
		}

		public void TestValidateTotalsAgainstPackLines()
		{
			AssertEquals("true by default", true, Shipment.ValidateTotalsAgainstPackLines);
		}

		public void TestOuterPackLinesForParentContainer()
		{
			// tests a single shipment packed into multiple containers
			// from the perspective of one of these containers only its packlines should be editable
			TallyContainer newContainer = Factory.New<TallyContainer>();
			Shipment.ParentContainerRegistration = newContainer;
			Shipment.DefaultPackLine.SetContainer(newContainer.PK);

			AssertEquals("related to parent container, therefore editable", false, Shipment.DefaultPackLine.ReadOnly);

			TallyPackLine newPack = Factory.New<TallyPackLine>();
			newPack.JL_FreightMode = FreightConstants.OuterPackType;
			Shipment.OuterPackLines.Add(newPack);
			newPack.SetContainer((Factory.New<TallyContainer>()).PK);

			//AssertEquals("not related to parent container, therefore read-only", true, Shipment.OuterPackLinesViewedByParentContainer[1].ReadOnly);
		}

		public void TestJS_Calc_InStock()
		{
			AssertEquals("should be empty at start", 0, Shipment.OuterPackLines.Count);
			PackLine pack1 = Shipment.OuterPackLines.AddNew();
			PackLine pack2 = Shipment.OuterPackLines.AddNew();

			pack1.JL_Outturn = pack1.JL_PackageCount = 10;
			pack2.JL_Outturn = pack2.JL_PackageCount = 10;

			AssertEquals("20 - 20 = 0", 0, Shipment.JS_Calc_InStock);
		}

		public void TestCoLoadMasterOuterPackLinesReadOnly()
		{
			Shipment.ParentContainerRegistration = Factory.New<TallyContainer>();
			var pack1 = Shipment.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 10;
			pack1.SetContainer(Shipment.ParentContainerRegistration.PK);
			var aggPackLine = Shipment.OuterPackLines[0];
			Assert("Read Only", !pack1.ReadOnly);
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var newCoLoad = Shipment.CoLoadShipments.AddNew();
			Assert(aggPackLine.IsDeleted);
		}

		public void TestBlankJobDocsAndCartageIsCreatedForNewShipment()
		{
			PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
			AssertNotNull("A new JobDocsAndCartage should have been created when JS_RL_NKOrigin was set in SetDefaultValues when the shipment was created", shipment.DocsAndCartage);
			AssertEquals("The JobDocsAndCartage should be of type PackUnpackDocsAndCartage.", shipment.DocsAndCartage.GetType(), typeof(PackUnpackDocsAndCartage));
		}

		public void TestJobDocsAndCartageIsReloaded()
		{
			PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
			AssertNotNull("Touch JobDocsAndCartage so it's created", shipment.DocsAndCartage);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			PackUnpackShipment loadedShipment = factory2.Load<PackUnpackShipment>(shipment.PK);
			AssertEquals("The correct JobDocsAndCartage should have been reloaded", shipment.DocsAndCartage.PK, loadedShipment.DocsAndCartage.PK);
		}

		#region Imlementation

		protected PackUnpackShipment Shipment;

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<PackUnpackShipment>();
		}

		#endregion
	}
}
