using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(WhsOrderLine))]
	sealed class TrackingWhsOrderLineTest : WhsOrderLineTest
	{
		public void TestLineNo()
		{
			TrackingOrderLine.WhsOrderLine.WE_LineNo = 2;
			AssertEquals((short)2, TrackingOrderLine.WE_LineNo);

			TrackingOrderLine.WhsOrderLine.WE_LineNo = 4;
			AssertEquals((short)4, TrackingOrderLine.WE_LineNo);
		}

		#region TestIsDocketFinalising

		public override void TestIsDocketFinalising()
		{
			Assert("Strange failure", true);
		}

		#endregion

		#region TestProductCodeAndDescription

		public void TestProductCodeAndDescription()
		{
			AssertNull("Precondition - No Product selected", TrackingOrderLine.WhsOrderLine.SupplierPart);
			AssertEquals("ProductDescription should be empty", ZString.Empty, TrackingOrderLine.ProductDescription);

			OrgSupplierPart part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PROD1";
			part1.OP_Desc = "First Product";
			TrackingOrderLine.WhsOrderLine.WE_OP = part1.PK;
			AssertEquals("SupplierPart should be Product1", part1.PK, TrackingOrderLine.WhsOrderLine.SupplierPart.PK);
			AssertEquals("Should be First Product", "PROD1", TrackingOrderLine.WhsOrderLine.ProductCode);
			AssertEquals("Should be First Product", "First Product", TrackingOrderLine.ProductDescription);

			OrgSupplierPart part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PROD2";
			part2.OP_Desc = "Second Product";
			TrackingOrderLine.WhsOrderLine.WE_OP = part2.PK;
			AssertEquals("SupplierPart should be Product1", part2.PK, TrackingOrderLine.WhsOrderLine.SupplierPart.PK);
			AssertEquals("Should be Second Product", "PROD2", TrackingOrderLine.WhsOrderLine.ProductCode);
			AssertEquals("Should be Second Product", "Second Product", TrackingOrderLine.ProductDescription);
		}

		#endregion

		#region TestProductDescriptionInfo

		public void TestProductDescriptionInfo()
		{
			ZPropertyInfoString info = TrackingOrderLine.ProductDescriptionInfo as ZPropertyInfoString;
			AssertNotNull("Should be of type ZPropertyInfoString", info);
			AssertEquals("MaxLength", OrgSupplierPartSchema.OP_Desc.MaxLength, info.MaxLength);
			Assert("ReadOnly", info.ReadOnly);
		}

		#endregion

		#region TestShortfallsAreCalculated

		public void TestShortfallsAreCalculated()
		{
			Docket.FillWithValidTestData();
			OrgSupplierPart part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PROD1";
			part1.OP_Desc = "First Product";
			TrackingOrderLine.WhsOrderLine.WE_OP = part1.PK;

			AssertEquals("Precondition: SupplierPart should be Product1", part1.PK, TrackingOrderLine.WhsOrderLine.SupplierPart.PK);
			AssertEquals("Precondition: Should be First Product", "PROD1", TrackingOrderLine.WhsOrderLine.ProductCode);
			AssertEquals("Precondition: Should be First Product", "First Product", TrackingOrderLine.ProductDescription);
			AssertEquals("Precondition: Order should be Order1", Docket.PK, TrackingOrderLine.Order.WhsOrder.PK);

			ZDecimal originalUnitsValue = TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity;
			ZDecimal originalShortfallValue = TrackingOrderLine.WhsOrderLine.WE_ShortfallQuantityCached;
			ZDecimal originalPacksValue = TrackingOrderLine.WhsOrderLine.WE_PackQuantity;

			TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity += 50;

			AssertNotEquals("Units quantity should have been updated", originalUnitsValue, TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity);
			AssertNotEquals("Shortfall Quantity should have been updated", originalShortfallValue, TrackingOrderLine.WhsOrderLine.WE_ShortfallQuantityCached);

			TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity = originalUnitsValue;
			AssertEquals("Units quantity should have been reset", originalUnitsValue, TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity);
			AssertEquals("Shortfall Quantity should have been reset", originalShortfallValue, TrackingOrderLine.WhsOrderLine.WE_ShortfallQuantityCached);

			TrackingOrderLine.WhsOrderLine.WE_PackQuantity += 50;

			AssertNotEquals("Packs quantity should have been updated", originalPacksValue, TrackingOrderLine.WhsOrderLine.WE_PackQuantity);
			AssertNotEquals("Shortfall Quantity should have been updated", originalShortfallValue, TrackingOrderLine.WhsOrderLine.WE_ShortfallQuantityCached);

			TrackingOrderLine.WhsOrderLine.WE_PackQuantity = originalPacksValue;
			AssertEquals("Packs quantity should have been reset", originalPacksValue, TrackingOrderLine.WhsOrderLine.WE_PackQuantity);
			AssertEquals("Shortfall Quantity should have been reset", originalShortfallValue, TrackingOrderLine.WhsOrderLine.WE_ShortfallQuantityCached);
		}

		protected override bool ShortfallQuantityIsCached
		{
			get { return false; }
		}

		#endregion

		#region TestReleaseDetailsNoAttributesMet

		public void TestReleaseDetailsNoAttributesMet()
		{
			SetupReleaseDetailsData();

			var originalUnitsMet = TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity;
			try
			{
				var today = ZDate.Today;
				Docket.Client.MiscServ.OM_IMUseSerialNumber = true;
				Docket.Client.MiscServ.OM_IMUseExpiryDate = true;
				Docket.Client.MiscServ.OM_IMUsePackingDate = true;
				TrackingOrderLine.WhsOrderLine.SupplierPart.OP_StockKeepingUnit = "CAS";
				TrackingOrderLine.WhsOrderLine.WE_ExpiryDate = today.AddDays(5);
				TrackingOrderLine.WhsOrderLine.WE_PackingDate = today.AddDays(-5);
				TrackingOrderLine.WhsOrderLine.WE_PartAttrib1 = "C-789";
				TrackingOrderLine.WhsOrderLine.WE_PartAttrib2 = "Medium";
				TrackingOrderLine.WhsOrderLine.WE_PartAttrib3 = "Purple";
				TrackingOrderLine.WhsOrderLine.WE_SerialNumber = "SN1";

				AssertEquals("ReleaseDetails - No Attributes Met. 1 record will be created", 1, TrackingOrderLine.ReleaseDetails.Count);
				AssertEquals("Release Details should have the expiry date set.", today.AddDays(5).ToShortDateString(), TrackingOrderLine.ReleaseDetails[0].W1_ExpiryDate);
				AssertEquals("Release Details should have the packing date set.", today.AddDays(-5).ToShortDateString(), TrackingOrderLine.ReleaseDetails[0].W1_PackingDate);
				AssertEquals("Release Details should have the serial number set.", "SN1", TrackingOrderLine.ReleaseDetails[0].W1_SerialNumber);
			}
			finally
			{
				TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity = originalUnitsMet;
			}
		}

		#endregion

		#region TestReleaseDetailsOneAttributesMet

		public void TestReleaseDetailsOneAttributesMet()
		{
			SetupReleaseDetailsData();
			var releaseLine1 = new WhsReleaseLine(TrackingOrderLine.WhsOrderLine);

			try
			{
				TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity = 20m;
				TrackingOrderLine.WhsOrderLine.SupplierPart.OP_StockKeepingUnit = "BSK";

				using (releaseLine1.GetValidationSuspender())
				{
					releaseLine1.PartAttribute1 = "B-456";
					releaseLine1.PartAttribute2 = "Small";
					releaseLine1.PartAttribute3 = "Blue";
				}

				var releaseLine2 = TrackingOrderLine.WhsOrderLine.ReleaseLines.AddNew();

				using (releaseLine2.GetValidationSuspender())
				{
					releaseLine2.PartAttribute1 = "B-456";
					releaseLine2.PartAttribute2 = "Small";
					releaseLine2.PartAttribute3 = "Blue";
					ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(TrackingOrderLine.WhsOrderLine, WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine2, TrackingOrderLine.WhsOrderLine));
					releaseLine2.Quantity = 15m;
				}

				AssertEquals("ReleaseDetails - One Release Line.", 1, TrackingOrderLine.ReleaseDetails.Count);
			}
			finally
			{
				TrackingOrderLine.WhsOrderLine.ReleaseLines.ClearCollection();
			}
		}

		public void TestReleaseDetailsOneAttributesMet_WithSerialNumber()
		{
			SetupReleaseDetailsData();

			var releaseLine1 = new WhsReleaseLine(TrackingOrderLine.WhsOrderLine);

			try
			{
				Docket.Client.MiscServ.OM_IMUseSerialNumber = true;
				TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity = 2m;
				TrackingOrderLine.WhsOrderLine.WE_F3_NKPackType = "BSK";
				TrackingOrderLine.WhsOrderLine.SupplierPart.OP_StockKeepingUnit = "BSK";

				using (releaseLine1.GetValidationSuspender())
				{
					releaseLine1.SetupReleaseLine("B-456", "Small", "Blue", ZDate.Empty, ZDate.Empty, "SN1");
				}

				var releaseLine2 = TrackingOrderLine.WhsOrderLine.ReleaseLines.AddNew();

				using (releaseLine2.GetValidationSuspender())
				{
					releaseLine2.SetupReleaseLine("B-456", "Small", "Blue", ZDate.Empty, ZDate.Empty, "SN1");
					ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(TrackingOrderLine.WhsOrderLine, WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine2, TrackingOrderLine.WhsOrderLine));
					releaseLine2.Quantity = 1m;
				}

				AssertEquals("ReleaseDetails - 1 Release Line.", 1, TrackingOrderLine.ReleaseDetails.Count);
				var trackingReleaseLine = TrackingOrderLine.ReleaseDetails[0];
				AssertEquals("PROD1", trackingReleaseLine.ProductCode);
				AssertEquals("First Product", trackingReleaseLine.ProductDescription);
				AssertEquals(2m, trackingReleaseLine.Packs);
				AssertEquals("Basket", trackingReleaseLine.PacksUQ);
				AssertEquals("Basket", trackingReleaseLine.UnitsQName);
				AssertEquals("1", trackingReleaseLine.QtyOrdered);
				AssertEquals("B-456", trackingReleaseLine.W1_PartAttrib1);
				AssertEquals("Small", trackingReleaseLine.W1_PartAttrib2);
				AssertEquals("Blue", trackingReleaseLine.W1_PartAttrib3);
				AssertEquals("SN1", trackingReleaseLine.W1_SerialNumber);
			}
			finally
			{
				TrackingOrderLine.WhsOrderLine.ReleaseLines.ClearCollection();
			}
		}

		public void TestReleaseDetails_ExistingSKUWithReleaseLine()
		{
			SetupReleaseDetailsData();
			var releaseLine = TrackingOrderLine.WhsOrderLine.ReleaseLines.AddNew();

			try
			{
				TrackingOrderLine.WhsOrderLine.SupplierPart.OP_StockKeepingUnit = Constants.PkgUnit.Basket;
				TrackingOrderLine.WhsOrderLine.WE_F3_NKPackType = "ABC";

				var trackingReleaseLine = TrackingOrderLine.ReleaseDetails[0];

				AssertEquals("Basket", trackingReleaseLine.UnitsQName);
				AssertEquals("ABC", trackingReleaseLine.PacksUQ);
			}
			finally
			{
				TrackingOrderLine.WhsOrderLine.ReleaseLines.ClearCollection();
			}
		}

		public void TestReleaseDetails_ExistingSKUWithoutReleaseLine()
		{
			SetupReleaseDetailsData();

			try
			{
				TrackingOrderLine.WhsOrderLine.SupplierPart.OP_StockKeepingUnit = Constants.PkgUnit.Basket;
				TrackingOrderLine.WhsOrderLine.WE_F3_NKPackType = "ABC";

				var trackingReleaseLine = TrackingOrderLine.ReleaseDetails[0];

				AssertEquals("Basket", trackingReleaseLine.UnitsQName);
				AssertEquals("ABC", trackingReleaseLine.PacksUQ);
			}
			finally
			{
				TrackingOrderLine.WhsOrderLine.ReleaseLines.ClearCollection();
			}
		}

		public void TestReleaseDetails_NonExistingSKUWithReleaseLine()
		{
			SetupReleaseDetailsData();
			var releaseLine = TrackingOrderLine.WhsOrderLine.ReleaseLines.AddNew();

			try
			{
				TrackingOrderLine.WhsOrderLine.SupplierPart.OP_StockKeepingUnit = "TVA";
				TrackingOrderLine.WhsOrderLine.WE_F3_NKPackType = "ABC";

				var trackingReleaseLine = TrackingOrderLine.ReleaseDetails[0];

				AssertEquals("TVA", trackingReleaseLine.UnitsQName);
				AssertEquals("ABC", trackingReleaseLine.PacksUQ);
			}
			finally
			{
				TrackingOrderLine.WhsOrderLine.ReleaseLines.ClearCollection();
			}
		}

		public void TestReleaseDetails_NonExistingSKUWithoutReleaseLine()
		{
			SetupReleaseDetailsData();

			try
			{
				TrackingOrderLine.WhsOrderLine.SupplierPart.OP_StockKeepingUnit = "TVA";
				TrackingOrderLine.WhsOrderLine.WE_F3_NKPackType = "ABC";

				var trackingReleaseLine = TrackingOrderLine.ReleaseDetails[0];

				AssertEquals("TVA", trackingReleaseLine.UnitsQName);
				AssertEquals("ABC", trackingReleaseLine.PacksUQ);
			}
			finally
			{
				TrackingOrderLine.WhsOrderLine.ReleaseLines.ClearCollection();
			}
		}

		#endregion

		#region TestReleaseDetailsTwoAttributesMet

		public void TestReleaseDetailsTwoAttributesMet()
		{
			SetupReleaseDetailsData();
			var releaseLine1 = new WhsReleaseLine(TrackingOrderLine.WhsOrderLine);
			var releaseLine2 = new WhsReleaseLine(TrackingOrderLine.WhsOrderLine);

			try
			{
				TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity = 40m;
				TrackingOrderLine.WhsOrderLine.SupplierPart.OP_StockKeepingUnit = "BOX";

				using (releaseLine1.GetValidationSuspender())
				{
					releaseLine1.PartAttribute1 = "A-123";
					releaseLine1.PartAttribute2 = "Small";
					releaseLine1.PartAttribute3 = "Green";
				}

				var copy1 = TrackingOrderLine.WhsOrderLine.ReleaseLines.AddNew();

				using (copy1.GetValidationSuspender())
				{
					copy1.PartAttribute1 = "A-123";
					copy1.PartAttribute2 = "Small";
					copy1.PartAttribute3 = "Green";
					ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(TrackingOrderLine.WhsOrderLine, WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(copy1, TrackingOrderLine.WhsOrderLine));
					copy1.Quantity = 30m;
				}

				using (releaseLine2.GetValidationSuspender())
				{
					releaseLine2.PartAttribute1 = "B-456";
					releaseLine2.PartAttribute2 = "Large";
					releaseLine2.PartAttribute3 = "Blue";
				}

				var copy2 = TrackingOrderLine.WhsOrderLine.ReleaseLines.AddNew();

				using (copy2.GetValidationSuspender())
				{
					copy2.PartAttribute1 = "B-456";
					copy2.PartAttribute2 = "Large";
					copy2.PartAttribute3 = "Blue";
					copy2.Quantity = 10m;
				}

				AssertEquals("ReleaseDetails - Two Release Lines.", 2, TrackingOrderLine.ReleaseDetails.Count);
			}
			finally
			{
				TrackingOrderLine.WhsOrderLine.ReleaseLines.ClearCollection();
			}
		}

		#endregion

		#region TestReleaseDetailsWithPackingExpiryDates

		public void TestReleaseDetailsWithPackingExpiryDates()
		{
			SetupReleaseDetailsData();
			var releaseLine1 = new WhsReleaseLine(TrackingOrderLine.WhsOrderLine);

			TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity = 40m;
			TrackingOrderLine.WhsOrderLine.SupplierPart.OP_StockKeepingUnit = "PCE";
			releaseLine1.SetupReleaseLine("A-123", "", "Green", new ZDate(2008, 07, 11), new ZDate(2008, 01, 29), "Serial");

			var releaseLine2 = TrackingOrderLine.WhsOrderLine.ReleaseLines.AddNew("A-123", "", "Green", "Serial", new ZDate(2008, 07, 11), new ZDate(2008, 01, 29));
			ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(TrackingOrderLine.WhsOrderLine, WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine2, TrackingOrderLine.WhsOrderLine));
			releaseLine2.Quantity = 30m;

			try
			{
				AssertEquals("Release Details - 1 Release Line, not using expiry date", "", TrackingOrderLine.ReleaseDetails[0].W1_ExpiryDate);
				AssertEquals("Release Details - 1 Release Line, not using packing date", "", TrackingOrderLine.ReleaseDetails[0].W1_PackingDate);

				Docket.Client.MiscServ.OM_IMUseExpiryDate = true;
				Docket.Client.MiscServ.OM_IMUsePackingDate = false;
				AssertNotEquals("Release Details - 1 Release Line, using expiry date", "", TrackingOrderLine.ReleaseDetails[0].W1_ExpiryDate);
				AssertEquals("Release Details - 1 Release Line, not using packing date", "", TrackingOrderLine.ReleaseDetails[0].W1_PackingDate);

				Docket.Client.MiscServ.OM_IMUseExpiryDate = false;
				Docket.Client.MiscServ.OM_IMUsePackingDate = true;
				AssertEquals("Release Details - 1 Release Line, not using expiry date", "", TrackingOrderLine.ReleaseDetails[0].W1_ExpiryDate);
				AssertNotEquals("Release Details - 1 Release Line, using packing date", "", TrackingOrderLine.ReleaseDetails[0].W1_PackingDate);

				Docket.Client.MiscServ.OM_IMUseExpiryDate = true;
				Docket.Client.MiscServ.OM_IMUsePackingDate = true;
				AssertNotEquals("Release Details - 1 Release Line, using expiry date", "", TrackingOrderLine.ReleaseDetails[0].W1_ExpiryDate);
				AssertNotEquals("Release Details - 1 Release Line, using packing date", "", TrackingOrderLine.ReleaseDetails[0].W1_PackingDate);
			}
			finally
			{
				Docket.Client.MiscServ.OM_IMUseExpiryDate = false;
				Docket.Client.MiscServ.OM_IMUsePackingDate = false;
			}
		}

		#endregion

		#region Cloning

		[ExpectNoExceptions]
		public void TestSupportsClone()
		{
			AssertNotNull("SupportsClone", TrackingOrderLine.Clone());
		}

		protected override void TestCloneWE_WD(WhsDocketLine orderLine)
		{
			base.TestCloneWE_WD(orderLine);

			var trackingLine = TrackingHelper.Get((WhsOrderLine)orderLine);
			trackingLine.WhsOrderLine.WE_WD = ZGuid.NewZGuid();

			var clone = (TrackingWhsOrderLine)trackingLine.Clone();

			AssertNotNull("Clone", clone);
			AssertNotEquals("Different Copy", trackingLine.PK, clone.PK);
			AssertNotEquals("Different Inner Copy", trackingLine.WhsOrderLine.PK, clone.WhsOrderLine.PK);
			AssertEquals("WE_WD", trackingLine.WhsOrderLine.WE_WD, clone.WhsOrderLine.WE_WD);
		}

		protected override void TestCloneLineNoAndSubLineNo(WhsDocketLine orderLine)
		{
			base.TestCloneWE_WD(orderLine);

			var trackingLine = TrackingHelper.Get((WhsOrderLine)orderLine);
			trackingLine.WhsOrderLine.WE_LineNo = 5;
			trackingLine.WhsOrderLine.WE_SubLineNo = 6;

			var clone = (TrackingWhsOrderLine)trackingLine.Clone();

			AssertEquals((ZShort)5, clone.WhsOrderLine.WE_LineNo);
			AssertEquals((ZShort)7, clone.WhsOrderLine.WE_SubLineNo);
		}

		#endregion

		#region Allocated & Unallocated Quantity

		[ExpectNoExceptions]
		public void TestAllocatedUnallocatedQuantity()
		{
			TrackingOrderLine.WhsOrderLine.WE_WD = Factory.New<WhsOrder>().PK;
			var testPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart.OP_PartNum = "PROD1";
			testPart.OP_Desc = "First Product";
			testPart.OP_CountDecimalPlaces = 2;

			Assert("Precondition - No Quantity was entered", TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity.IsEmpty);
			AssertEquals("Should be no CrossDocks", 0, TrackingOrderLine.WhsOrderLine.ReservedPickLines.Count);
			AssertEquals("AllocatedQuantity should be 0", ZDecimal.Zero, TrackingOrderLine.WhsOrderLine.WE_CrossDockQuantity);
			AssertEquals("UnallocatedQuantity should be 0", ZDecimal.Zero, TrackingOrderLine.UnallocatedQuantity);

			AssertEquals("AllocatedQuantityAsString should be empty", ZString.Empty, TrackingOrderLine.AllocatedQuantityAsString);
			TrackingOrderLine.WhsOrderLine.WE_OP = testPart.PK;
			AssertEquals("AllocatedQuantityAsString should be 0.00", "0.00", TrackingOrderLine.AllocatedQuantityAsString);

			TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity = 10;
			AssertEquals("AllocatedQuantity should be 0", ZDecimal.Zero, TrackingOrderLine.WhsOrderLine.WE_CrossDockQuantity);
			AssertEquals("UnallocatedQuantity should be 10", 10m, TrackingOrderLine.UnallocatedQuantity);
			AssertEquals("AllocatedQuantityAsString should be 0.00", "0.00", TrackingOrderLine.AllocatedQuantityAsString);

			var pickLine1 = Helper.CreateReservePickLine(TrackingOrderLine.WhsOrderLine, Factory.New<WhsInventoryView>(), 5m);
			AssertEquals("AllocatedQuantity should be 5", 5m, TrackingOrderLine.WhsOrderLine.WE_CrossDockQuantity);
			AssertEquals("UnallocatedQuantity should be 5", 5m, TrackingOrderLine.UnallocatedQuantity);
			AssertEquals("AllocatedQuantityAsString should be 5.00", "5.00", TrackingOrderLine.AllocatedQuantityAsString);

			var pickLine2 = Helper.CreateReservePickLine(TrackingOrderLine.WhsOrderLine, Factory.New<WhsInventoryView>(), 2m);
			AssertEquals("AllocatedQuantity should be 7", 7m, TrackingOrderLine.WhsOrderLine.WE_CrossDockQuantity);
			AssertEquals("UnallocatedQuantity should be 3", 3m, TrackingOrderLine.UnallocatedQuantity);
			AssertEquals("AllocatedQuantityAsString should be 7.00", "7.00", TrackingOrderLine.AllocatedQuantityAsString);

			pickLine2.ReservedQuantity = 5m;
			AssertEquals("AllocatedQuantity should be 10", 10m, TrackingOrderLine.WhsOrderLine.WE_CrossDockQuantity);
			AssertEquals("UnallocatedQuantity should be 0", ZDecimal.Zero, TrackingOrderLine.UnallocatedQuantity);
			AssertEquals("AllocatedQuantityAsString should be 10.00", "10.00", TrackingOrderLine.AllocatedQuantityAsString);
		}

		#endregion

		public void TestGetWrappedBizO()
		{
			var wrappedBizO = TrackingOrderLine.GetWrappedBizO();

			AssertEquals(TrackingOrderLine.WhsOrderLine, wrappedBizO);
		}

		public void TestGetWrappedBindTo()
		{
			var wrappedBindTo = TrackingOrderLine.GetWrappedBindTo("Lookups");

			AssertEquals("WhsOrderLine+Lookups", wrappedBindTo);
		}

		#region Implementation

		#region Test Setup

		bool oldIsWeb;

		protected override void SetUp()
		{
			base.SetUp();
			oldIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			Globals.IsWeb = oldIsWeb;
			base.TearDown();
		}

		#endregion

		new TrackingWhsOrder GetNewWhsDocket()
		{
			return TrackingHelper.Get(Docket);
		}

		TrackingWhsOrderLine trackingOrderLine;
		TrackingWhsOrderLine TrackingOrderLine
		{
			get { return trackingOrderLine ?? (trackingOrderLine = TrackingHelper.Get(DocketLine)); }
		}

		void SetupReleaseDetailsData()
		{
			testPart1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart1.OP_PartNum = "PROD1";
			testPart1.OP_Desc = "First Product";
			Helper.CreateProductUnit(testPart1, Constants.PkgUnit.Basket, Constants.PkgUnit.Pallet, 100);

			TrackingOrderLine.WhsOrderLine.WE_OP = testPart1.PK;
			TrackingOrderLine.WhsOrderLine.WE_TransactionQuantity = 10m;

			Docket.FillWithValidTestData();
			Docket.Client.MiscServ.OM_IMPartAttrib1Name = "Batch";
			Docket.Client.MiscServ.OM_IMPartAttrib2Name = "Size";
			Docket.Client.MiscServ.OM_IMPartAttrib3Name = "Colour";
			Docket.Client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			Docket.Client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			Docket.Client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			Docket.Client.PartAttributeManager.SetProductToUseAttribute(testPart1, 1, true);
			Docket.Client.PartAttributeManager.SetProductToUseAttribute(testPart1, 2, true);
			Docket.Client.PartAttributeManager.SetProductToUseAttribute(testPart1, 3, true);
			Docket.Client.MiscServ.OM_IMUseExpiryDate = false;
			Docket.Client.MiscServ.OM_IMUsePackingDate = false;

			Factory.Save();
		}

		OrgSupplierPart testPart1;

		#endregion
	}
}
