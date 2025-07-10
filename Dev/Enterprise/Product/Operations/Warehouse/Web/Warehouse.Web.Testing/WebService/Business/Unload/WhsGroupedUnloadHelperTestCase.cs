using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsGroupedUnloadHelperTestCase : TestCaseWithFactory
	{
		#region TestGetValidReceive_InvalidReceivePK

		public void TestGetValidReceive_InvalidReceivePK()
		{
			var response = new WebServiceResponse();
			WhsGroupedUnloadHelper.LoadValidNotFinalizedReceive(response, Factory, Guid.Empty);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Receive record could not be found.", response.ErrorMessage);
			AssertEquals(false, response.NoError());
		}

		#endregion

		#region TestGetValidReceive_FinalizedReceive

		public void TestGetValidReceive_FinalizedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			var response = new WebServiceResponse();
			WhsGroupedUnloadHelper.LoadValidNotFinalizedReceive(response, Factory, receive.PK.ToGuid());
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Receive record has been finalized.", response.ErrorMessage);
			AssertEquals(false, response.NoError());
		}

		#endregion

		#region TestCheckASNLinesPalletInfo

		#region TestCheckASNLinesPalletInfo_InvalidPalletID

		public void TestCheckASNLinesPalletInfo_InvalidPalletID()
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			// create receive line but without PalletID
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();

			PopulateASNLines(receive);

			// ASN Line without PalletID
			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pallet Id 'PLT1' not expected for this receive. Use Product Unload to receive it.", response.ErrorMessage);
		}

		#endregion

		#region TestCheckASNLinesPalletInfo_InvalidPalletID_NoASNLines

		public void TestCheckASNLinesPalletInfo_InvalidPalletID_NoASNLines()
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Factory.Save();

			// no ASN Lines in receive
			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pallet Id 'PLT1' not expected for this receive. Use Product Unload to receive it.", response.ErrorMessage);
		}

		#endregion

		#region TestCheckASNLinesPalletInfo_SerialNumberUsedForReleaseCaptured

		public void TestCheckASNLinesPalletInfo_SerialNumberUsedForReleaseCaptured()
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Factory.Save();

			PopulateASNLines(receive);

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", false);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
		}

		#endregion

		#region TestCheckASNLinesPalletInfo_SerialNumberUsedButNotEntered

		public void TestCheckASNLinesPalletInfo_SerialNumberUsedButNotEntered()
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			// create receive line but without SerialNumber
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Factory.Save();

			PopulateASNLines(receive);

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", false);
			AssertEquals(ErrorTypes.PalletAsnLineMissingMandatoryAttributes, response.Error);
			AssertEquals("Product on this Pallet Id 'PLT1' contains serial numbers which must be entered. Use Product Unload mode to unload it.", response.ErrorMessage);
		}

		#endregion

		#region TestCheckASNLinesPalletInfo_SerialNumberEnteredButQuantityMoreThanOne

		public void TestCheckASNLinesPalletInfo_SerialNumberEnteredButQuantityMoreThanOne()
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");

			// set Serial Number used, and create receive line with serial number:SN01, and Quantity:10m
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			line.WI_SerialNumber = "SN01";

			Factory.Save();
			PopulateASNLines(receive);

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", false);
			AssertEquals(ErrorTypes.PalletAsnLineMissingMandatoryAttributes, response.Error);
			AssertEquals("Product on this Pallet Id 'PLT1' contains serial numbers which must be entered. Use Product Unload mode to unload it.", response.ErrorMessage);
		}

		#endregion

		#region TestCheckASNLinesPalletInfo_SerialNumberUsedInClientButNotUsedInProduct

		public void TestCheckASNLinesPalletInfo_SerialNumberUsedInClientButNotUsedInProduct()
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Factory.Save();

			PopulateASNLines(receive);

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", false);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
		}

		#endregion

		#region TestCheckASNLinesPalletInfo_MandatoryAttributeUsedButNotEntered

		public void TestCheckASNLinesPalletInfo_MandatoryAttributeUsedButNotEntered_Attribute1()
		{
			AssertMandatoryAttributeUsedButNotEntered(AttributeNumber.One);
		}

		public void TestCheckASNLinesPalletInfo_MandatoryAttributeUsedButNotEntered_Attribute2()
		{
			AssertMandatoryAttributeUsedButNotEntered(AttributeNumber.Two);
		}

		public void TestCheckASNLinesPalletInfo_MandatoryAttributeUsedButNotEntered_Attribute3()
		{
			AssertMandatoryAttributeUsedButNotEntered(AttributeNumber.Three);
		}

		public void TestCheckASNLinesPalletInfo_MandatoryAttributeUsedButNotEntered_ExpiryDate()
		{
			AssertMandatoryAttributeUsedButNotEntered(AttributeNumber.ExpiryDate);
		}

		public void TestCheckASNLinesPalletInfo_MandatoryAttributeUsedButNotEntered_PackingDate()
		{
			AssertMandatoryAttributeUsedButNotEntered(AttributeNumber.PackingDate);
		}

		[TestDate(2016, 12, 19)]
		void AssertMandatoryAttributeUsedButNotEntered(AttributeNumber mandatoryAttr)
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);

			// set mandatory attribute used, and create receive line but mandatory value is empty.
			Helper.SetClientAttributeType(data.Org1, mandatoryAttr, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, mandatoryAttr, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");

			Factory.Save();
			PopulateASNLines(receive);

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", false);
			AssertEquals(ErrorTypes.PalletAsnLineMissingMandatoryAttributes, response.Error);
			AssertEquals("Product on this Pallet Id 'PLT1' contains mandatory attribute which must be entered. Use Product Unload mode to unload it.", response.ErrorMessage);
		}

		#endregion

		#region TestCheckASNLinesPalletInfo_MandatoryAttributeUsedForReleaseCaptured

		public void TestCheckASNLinesPalletInfo_MandatoryAttributeUsedForReleaseCaptured_Attribute1()
		{
			CheckASNLinesPalletInfo_MandatoryAttributeUsedForReleaseCaptured(AttributeNumber.One);
		}

		public void TestCheckASNLinesPalletInfo_MandatoryAttributeUsedForReleaseCaptured_Attribute2()
		{
			CheckASNLinesPalletInfo_MandatoryAttributeUsedForReleaseCaptured(AttributeNumber.Two);
		}

		public void TestCheckASNLinesPalletInfo_MandatoryAttributeUsedForReleaseCaptured_Attribute3()
		{
			CheckASNLinesPalletInfo_MandatoryAttributeUsedForReleaseCaptured(AttributeNumber.Three);
		}

		void CheckASNLinesPalletInfo_MandatoryAttributeUsedForReleaseCaptured(AttributeNumber mandatoryAttr)
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);

			// set mandatory attribute used and is Release Captured, and create receive line but mandatory value is empty.
			Helper.SetClientAttributeType(data.Org1, mandatoryAttr, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, mandatoryAttr, true, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");

			Factory.Save();
			PopulateASNLines(receive);

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", false);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
		}

		#endregion

		#region TestCheckASNLinesPalletInfo_PreventReceiveOfPartsWithoutWeightOrDims

		public void TestCheckASNLinesPalletInfo_PreventReceiveOfPartsWithoutWeightOrDims()
		{
			var response1 = new WebServiceResponse();
			var response2 = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsCheckPartWeightOrDimsOnReceive = "ALL";

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var part5 = Helper.CreateProduct(data.Org1, "P5");

			part3.OP_StockKeepingUnit = "BAG";
			part3.OP_Weight = 2m;

			var productUnit3 = part3.PartUnits[0];
			productUnit3.OF_Depth = 1m;
			productUnit3.OF_Width = 1m;
			productUnit3.OF_Height = 1m;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "TEST1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, part3, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, part4, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, part5, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, part3, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, part4, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, part5, 10m, data.Whs1.DefaultLocation, "PLT1");
			Factory.Save();

			PopulateASNLines(receive1);

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response1, receive1, "PLT1", false);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("The following product(s) on this Pallet Id 'PLT1' cannot be received as the Product Master is missing weight or dimensions: \r\nP1\r\nP2\r\nP4\r\nP5", response1.ErrorMessage);

			var product1Unit = data.Part1.PartUnits[0];
			product1Unit.OF_Weight = 1m;
			product1Unit.OF_Cubic = 1m;

			var product2Unit = data.Part2.PartUnits[0];
			product2Unit.OF_Weight = 1m;
			product2Unit.OF_Cubic = 1m;

			part4.OP_StockKeepingUnit = "UNT";
			part4.OP_Weight = 3m;

			part5.OP_StockKeepingUnit = "CNT";
			part5.OP_Weight = 4m;
			Factory.Save();

			var productUnit4 = part4.PartUnits[0];
			productUnit4.OF_Depth = 2m;
			productUnit4.OF_Width = 2m;
			productUnit4.OF_Height = 2m;

			var productUnit5 = part5.PartUnits[0];
			productUnit5.OF_Depth = 3m;
			productUnit5.OF_Width = 3m;
			productUnit5.OF_Height = 3m;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "TEST2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive2, part3, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive2, part4, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive2, part5, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive2, part3, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive2, part4, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive2, part5, 10m, data.Whs1.DefaultLocation, "PLT2");
			Factory.Save();

			PopulateASNLines(receive2);

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response2, receive2, "PLT2", false);
			AssertEquals(ErrorTypes.None, response2.Error);
			AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
		}

		#endregion

		#endregion

		#region NoMandatoryAttributesUsedInProduct

		public void TestCheckASNLinesPalletInfo_MandatoryAttributeUsedInClientButNotUsedInProduct()
		{
			NoMandatoryAttributesUsedInProduct(true);
		}

		public void TestASNLinesForPalletID_NoMandatoryAttributesUsedInClientAndProduct()
		{
			NoMandatoryAttributesUsedInProduct(false);
		}

		void NoMandatoryAttributesUsedInProduct(bool usedInClient)
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);

			if (usedInClient)
			{
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Factory.Save();

			PopulateASNLines(receive);

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", false);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
		}

		#endregion

		#region TestCheckASNLinesPalletInfo_PalletHasUnloaded

		public void TestCheckASNLinesPalletInfo_PalletHasUnloaded_IsPerformingUnload()
		{
			TestCheckASNLinesPalletInfo_PalletHasUnloaded(isPerformingUnload: true);
		}

		public void TestCheckASNLinesPalletInfo_PalletHasUnloaded_IsGettingASNLineInfo()
		{
			TestCheckASNLinesPalletInfo_PalletHasUnloaded(isPerformingUnload: false);
		}

		void TestCheckASNLinesPalletInfo_PalletHasUnloaded(bool isPerformingUnload)
		{
			var response = new WebServiceResponse();
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Factory.Save();

			PopulateASNLines(receive);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");

			WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, "PLT1", isPerformingUnload);
			if (isPerformingUnload)
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Pallet Id 'PLT1' was already unloaded.", response.ErrorMessage);
			}
			else
			{
				AssertEquals(ErrorTypes.YesNoEnquiry, response.Error);
				AssertEquals("Pallet Id 'PLT1' was already unloaded. Do you want to check it?", response.ErrorMessage);
			}
		}

		#endregion

		#region PopulateASNLines

		void PopulateASNLines(WhsReceive receive, bool hasUnloaded = false)
		{
			var expectAsnCount = receive.Lines.Count;
			// create ASN Lines for receive
			receive.PopulateASNLines();
			if (!hasUnloaded)
			{
				receive.Inventory.RemoveAndDeleteAll();
			}

			AssertEquals("Precondition:", expectAsnCount, receive.AsnLines.Count);
		}

		#endregion

		#region Implementation

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
