using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsProductPartAttributesInfo))]
	public class WhsProductPartAttributesInfoTestCase : WhsOrgPartAttributesInfoTestCase
	{
		#region TestGetDateFormat

		public void TestGetDateFormat()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var client = helper.CreateClient("Client1");
			client.MiscServ.OM_IMUseExpiryDate = true;
			client.MiscServ.OM_IMUsePackingDate = true;
			var whs = helper.CreateWarehouse("WHS");
			Factory.Save();

			var part = helper.CreateProduct(client, "Part");
			var relation = helper.CreateProductClientRelationShip(client, part);
			relation.OU_UseExpiryDate = false;
			relation.OU_UsePackingDate = false;
			relation.OU_ExpiryDateFormatString = "";
			relation.OU_PackingDateFormatString = "";

			var partAttributesInfo = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals("", partAttributesInfo.ExpiryDateFormatString);
			AssertEquals("", partAttributesInfo.PackingDateFormatString);

			relation.OU_UseExpiryDate = true;
			relation.OU_UsePackingDate = true;
			relation.OU_ExpiryDateFormatString = "";
			relation.OU_PackingDateFormatString = "";

			partAttributesInfo = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals("ddMMyy", partAttributesInfo.ExpiryDateFormatString);
			AssertEquals("ddMMyy", partAttributesInfo.PackingDateFormatString);

			whs.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			partAttributesInfo = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals("yyMMdd", partAttributesInfo.ExpiryDateFormatString);
			AssertEquals("yyMMdd", partAttributesInfo.PackingDateFormatString);

			relation.OU_ExpiryDateFormatString = "YYMMDD";
			relation.OU_PackingDateFormatString = "DDMMYY";

			partAttributesInfo = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals("YYMMDD", partAttributesInfo.ExpiryDateFormatString);
			AssertEquals("DDMMYY", partAttributesInfo.PackingDateFormatString);
		}

		#endregion

		#region TestGetInfo

		public void TestGetInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var emptyInfo = WhsProductPartAttributesInfo.GetInfo(null, null, data.Whs1);
			AssertEquals(Guid.Empty, emptyInfo.ClientPK);
			AssertEquals(Guid.Empty, emptyInfo.ProductPK);
			AssertNotEquals("Null arguments should not cache result.", emptyInfo, WhsProductPartAttributesInfo.GetInfo(null, null, data.Whs1));

			var emptyProductInfo = WhsProductPartAttributesInfo.GetInfo(data.Org1, null, data.Whs1);
			AssertEquals(data.Org1.PK.ToGuid(), emptyProductInfo.ClientPK);
			AssertEquals(Guid.Empty, emptyProductInfo.ProductPK);
			AssertNotEquals("Null arguments should not cache result.", emptyProductInfo, WhsProductPartAttributesInfo.GetInfo(data.Org1, null, data.Whs1));

			var emptyClientInfo = WhsProductPartAttributesInfo.GetInfo(null, data.Part1, data.Whs1);
			AssertEquals(Guid.Empty, emptyClientInfo.ClientPK);
			AssertEquals(data.Part1.PK.ToGuid(), emptyClientInfo.ProductPK);
			AssertNotEquals("Null arguments should not cache result.", emptyClientInfo, WhsProductPartAttributesInfo.GetInfo(null, data.Part1, data.Whs1));

			var part1Info = WhsProductPartAttributesInfo.GetInfo(data.Org1, data.Part1, data.Whs1);
			AssertEquals(data.Org1.PK.ToGuid(), part1Info.ClientPK);
			AssertEquals(data.Part1.PK.ToGuid(), part1Info.ProductPK);
			AssertEquals("Non null arguments should cache result.", part1Info, WhsProductPartAttributesInfo.GetInfo(data.Org1, data.Part1, data.Whs1));

			var part2Info = WhsProductPartAttributesInfo.GetInfo(data.Org1, data.Part2, data.Whs1);
			AssertEquals(data.Org1.PK.ToGuid(), part2Info.ClientPK);
			AssertEquals(data.Part2.PK.ToGuid(), part2Info.ProductPK);
			AssertEquals("Non null arguments should cache result.", part2Info, WhsProductPartAttributesInfo.GetInfo(data.Org1, data.Part2, data.Whs1));
			AssertNotEquals("Caches should be per Client & Product.", part1Info, part2Info);

			var newOrg = Factory.New<OrgHeader>();
			var differentClientInfo = WhsProductPartAttributesInfo.GetInfo(newOrg, data.Part1, data.Whs1);
			AssertEquals(newOrg.PK.ToGuid(), differentClientInfo.ClientPK);
			AssertEquals(data.Part1.PK.ToGuid(), differentClientInfo.ProductPK);
			AssertEquals("Non null arguments should cache result.", differentClientInfo, WhsProductPartAttributesInfo.GetInfo(newOrg, data.Part1, data.Whs1));
			AssertNotEquals("Caches should be per Client & Product.", part1Info, differentClientInfo);
		}

		public void TestGetInfo_NullWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			AssertExceptionThrown("Warehouse must not be null.", typeof(ArgumentNullException), () => WhsProductPartAttributesInfo.GetInfo(null, null, null));
			AssertExceptionThrown("Warehouse must not be null.", typeof(ArgumentNullException), () => WhsProductPartAttributesInfo.GetInfo(data.Org1, null, null));
			AssertExceptionThrown("Warehouse must not be null.", typeof(ArgumentNullException), () => WhsProductPartAttributesInfo.GetInfo(null, data.Part1, null));
			AssertExceptionThrown("Warehouse must not be null.", typeof(ArgumentNullException), () => WhsProductPartAttributesInfo.GetInfo(data.Org1, data.Part1, null));
		}

		public void TestGetInfo_CacheByWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var whs1 = data.Whs1;
			whs1.WarehouseAddress.OA_RN_NKCountryCode = "AU";
			var whs2 = Helper.CreateWarehouse("WHS2");
			whs2.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var part1Info = WhsProductPartAttributesInfo.GetInfo(data.Org1, data.Part1, whs1);
			AssertEquals("ddMMyy", part1Info.ExpiryDateFormatString); // Date format for testing
			AssertEquals("ddMMyy", part1Info.PackingDateFormatString); // Date format for testing
			AssertEquals("Non null arguments should cache result.", part1Info, WhsProductPartAttributesInfo.GetInfo(data.Org1, data.Part1, whs1));

			var part2Info = WhsProductPartAttributesInfo.GetInfo(data.Org1, data.Part1, whs2);
			AssertEquals("yyMMdd", part2Info.ExpiryDateFormatString); // Date format for testing
			AssertEquals("yyMMdd", part2Info.PackingDateFormatString); // Date format for testing
			AssertEquals("Non null arguments should cache result.", part2Info, WhsProductPartAttributesInfo.GetInfo(data.Org1, data.Part1, whs2));
			AssertNotEquals("Caches should be per Client, Product & Warehouse.", part1Info, part2Info);
		}

		#endregion

		#region Constructors

		protected override void TestAdditionalConstructorsCore()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var client = helper.CreateClient();
			client.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			client.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";
			client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.JulianBatchNumber;
			client.MiscServ.OM_IMUseExpiryDate = true;
			client.MiscServ.OM_IMUsePackingDate = true;

			var whs = helper.CreateWarehouse("WHS");

			var part = helper.CreateProduct(client, "Part");
			var relation = helper.CreateProductClientRelationShip(client, part);
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = false;
			relation.OU_UsePartAttrib3 = true;
			relation.OU_UseExpiryDate = true;
			relation.OU_UsePackingDate = false;
			relation.OU_UseSerialNumber = true;
			relation.OU_ExpiryDateFormatString = "YYMMDD";
			relation.OU_PackingDateFormatString = "DDMMYY";
			relation.OU_CompletePalletPicking = false;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			relation.OU_Hi = 10;
			relation.OU_Ti = 15;

			var partAttributesInfo = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals("Attr1 Name", partAttributesInfo.Attribute1Caption);
			AssertEquals(true, partAttributesInfo.Attribute1IsMandatory);
			AssertEquals(true, partAttributesInfo.Attribute1IsUsed);
			AssertEquals("Attr2 Name", partAttributesInfo.Attribute2Caption);
			AssertEquals(false, partAttributesInfo.Attribute2IsMandatory);
			AssertEquals(false, partAttributesInfo.Attribute2IsUsed);
			AssertEquals("Attr3 Name", partAttributesInfo.Attribute3Caption);
			AssertEquals(true, partAttributesInfo.Attribute3IsMandatory);
			AssertEquals(true, partAttributesInfo.Attribute3IsUsed);
			AssertEquals(false, partAttributesInfo.IsSerialNumberUsedByProduct);
			AssertEquals(false, partAttributesInfo.IsSerialNumberReleaseCaptured);
			AssertEquals("YYMMDD", partAttributesInfo.ExpiryDateFormatString);
			AssertEquals("DDMMYY", partAttributesInfo.PackingDateFormatString);
			AssertEquals(false, partAttributesInfo.CompletePalletPickingUsed);
			AssertEquals(false, partAttributesInfo.IsAttributeNeutral);
			AssertEquals(3, partAttributesInfo.RFAttributeConfirm);
			AssertEquals((short)10, partAttributesInfo.Hi);
			AssertEquals((short)15, partAttributesInfo.Ti);

			client.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name2";
			client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			client.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name2";
			client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			client.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name2";
			client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.BatchNumber;
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = false;
			relation.OU_UseExpiryDate = false;
			relation.OU_UsePackingDate = true;
			relation.OU_PackingDateFormatString = "";
			relation.OU_ExpiryDateFormatString = "";
			relation.OU_CompletePalletPicking = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			relation.OU_Hi = 5;
			relation.OU_Ti = 20;

			partAttributesInfo = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals("Attr1 Name2", partAttributesInfo.Attribute1Caption);
			AssertEquals(false, partAttributesInfo.Attribute1IsMandatory);
			AssertEquals(true, partAttributesInfo.Attribute1IsUsed);
			AssertEquals("Attr2 Name2", partAttributesInfo.Attribute2Caption);
			AssertEquals(true, partAttributesInfo.Attribute2IsMandatory);
			AssertEquals(true, partAttributesInfo.Attribute2IsUsed);
			AssertEquals("Attr3 Name2", partAttributesInfo.Attribute3Caption);
			AssertEquals(true, partAttributesInfo.Attribute3IsMandatory);
			AssertEquals(false, partAttributesInfo.Attribute3IsUsed);
			AssertEquals(false, partAttributesInfo.IsSerialNumberUsedByProduct);
			AssertEquals(false, partAttributesInfo.IsSerialNumberReleaseCaptured);
			AssertEquals(string.Empty, partAttributesInfo.ExpiryDateFormatString);
			AssertEquals("ddMMyy", partAttributesInfo.PackingDateFormatString);
			AssertEquals(true, partAttributesInfo.CompletePalletPickingUsed);
			AssertEquals(true, partAttributesInfo.IsAttributeNeutral);
			AssertEquals(1, partAttributesInfo.RFAttributeConfirm);
			AssertEquals((short)5, partAttributesInfo.Hi);
			AssertEquals((short)20, partAttributesInfo.Ti);
		}

		public void TestConstructor_IsBatchNo()
		{
			var client = Helper.CreateClient();
			client.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			client.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.BatchNumber;
			client.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";
			client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.BatchNumber;

			var whs = Helper.CreateWarehouse("WHS");

			var part = Helper.CreateProduct(client, "Part");
			var relation = Helper.CreateProductClientRelationShip(client, part);
			var partAttributesInfo1 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo1.Attribute1IsBatchNoAndUsed);
			AssertEquals(false, partAttributesInfo1.Attribute2IsBatchNoAndUsed);
			AssertEquals(false, partAttributesInfo1.Attribute3IsBatchNoAndUsed);

			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = true;
			var partAttributesInfo2 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(true, partAttributesInfo2.Attribute1IsBatchNoAndUsed);
			AssertEquals(true, partAttributesInfo2.Attribute2IsBatchNoAndUsed);
			AssertEquals(true, partAttributesInfo2.Attribute3IsBatchNoAndUsed);

			client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			var partAttributesInfo3 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo3.Attribute1IsBatchNoAndUsed);
			AssertEquals(false, partAttributesInfo3.Attribute2IsBatchNoAndUsed);
			AssertEquals(false, partAttributesInfo3.Attribute3IsBatchNoAndUsed);
		}

		public void TestConstructor_NullWhs()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "Part");

			AssertExceptionThrown(typeof(ArgumentNullException), () => new WhsProductPartAttributesInfo(client, part, null));
		}

		#endregion

		#region Properties

		#region TestAttribute1IsUsed

		public void TestAttribute1IsUsed()
		{
			AssertEquals(false, Parent.Attribute1IsUsed);

			Parent.Attribute1IsUsed = true;
			AssertEquals(true, Parent.Attribute1IsUsed);

			Parent.Attribute1IsUsed = false;
			AssertEquals(false, Parent.Attribute1IsUsed);
		}

		#endregion

		#region TestAttribute1IsBatchNoAndUsed

		public void TestAttribute1IsBatchNoAndUsed()
		{
			AssertEquals(false, Parent.Attribute1IsBatchNoAndUsed);

			Parent.Attribute1IsBatchNoAndUsed = true;
			AssertEquals(true, Parent.Attribute1IsBatchNoAndUsed);

			Parent.Attribute1IsBatchNoAndUsed = false;
			AssertEquals(false, Parent.Attribute1IsBatchNoAndUsed);
		}

		#endregion

		#region TestAttribute1IsReleaseCaptured

		public void TestAttribute1IsReleaseCaptured()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct("Guitars", client);
			var whs = Helper.CreateWarehouse("WHS");
			var partAttributesInfo1 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo1.Attribute1IsReleaseCaptured);

			Helper.SetClientAttributeType(client, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber);
			var partAttributesInfo2 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo2.Attribute1IsReleaseCaptured);

			var relation = part.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
			relation.OU_UsePartAttrib1 = true;
			var partAttributesInfo3 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo3.Attribute1IsReleaseCaptured);

			relation.OU_IsPartAttrib1ReleaseCaptured = true;
			var partAttributesInfo4 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(true, partAttributesInfo4.Attribute1IsReleaseCaptured);
		}

		#endregion

		#region TestAttribute2IsUsed

		public void TestAttribute2IsUsed()
		{
			AssertEquals(false, Parent.Attribute2IsUsed);

			Parent.Attribute2IsUsed = true;
			AssertEquals(true, Parent.Attribute2IsUsed);

			Parent.Attribute2IsUsed = false;
			AssertEquals(false, Parent.Attribute2IsUsed);
		}

		#endregion

		#region TestAttribute2IsBatchNoAndUsed

		public void TestAttribute2IsBatchNoAndUsed()
		{
			AssertEquals(false, Parent.Attribute2IsBatchNoAndUsed);

			Parent.Attribute2IsBatchNoAndUsed = true;
			AssertEquals(true, Parent.Attribute2IsBatchNoAndUsed);

			Parent.Attribute2IsBatchNoAndUsed = false;
			AssertEquals(false, Parent.Attribute2IsBatchNoAndUsed);
		}

		#endregion

		#region TestAttribute2IsReleaseCaptured

		public void TestAttribute2IsReleaseCaptured()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct("Guitars", client);
			var whs = Helper.CreateWarehouse("WHS");
			var partAttributesInfo1 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo1.Attribute2IsReleaseCaptured);

			Helper.SetClientAttributeType(client, AttributeNumber.Two, PartAttributeTypeList.Codes.BatchNumber);
			var partAttributesInfo2 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo2.Attribute2IsReleaseCaptured);

			var relation = part.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
			relation.OU_UsePartAttrib2 = true;
			var partAttributesInfo3 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo3.Attribute2IsReleaseCaptured);

			relation.OU_IsPartAttrib2ReleaseCaptured = true;
			var partAttributesInfo4 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(true, partAttributesInfo4.Attribute2IsReleaseCaptured);
		}

		#endregion

		#region TestAttribute3IsUsed

		public void TestAttribute3IsUsed()
		{
			AssertEquals(false, Parent.Attribute3IsUsed);

			Parent.Attribute3IsUsed = true;
			AssertEquals(true, Parent.Attribute3IsUsed);

			Parent.Attribute3IsUsed = false;
			AssertEquals(false, Parent.Attribute3IsUsed);
		}

		#endregion

		#region TestAttribute3IsBatchNoAndUsed

		public void TestAttribute3IsBatchNoAndUsed()
		{
			AssertEquals(false, Parent.Attribute3IsBatchNoAndUsed);

			Parent.Attribute3IsBatchNoAndUsed = true;
			AssertEquals(true, Parent.Attribute3IsBatchNoAndUsed);

			Parent.Attribute3IsBatchNoAndUsed = false;
			AssertEquals(false, Parent.Attribute3IsBatchNoAndUsed);
		}

		#endregion

		#region TestAttribute3IsReleaseCaptured

		public void TestAttribute3IsReleaseCaptured()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct("Guitars", client);
			var whs = Helper.CreateWarehouse("WHS");
			var partAttributesInfo1 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo1.Attribute3IsReleaseCaptured);

			Helper.SetClientAttributeType(client, AttributeNumber.Three, PartAttributeTypeList.Codes.BatchNumber);
			var partAttributesInfo2 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo2.Attribute3IsReleaseCaptured);

			var relation = part.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
			relation.OU_UsePartAttrib3 = true;
			var partAttributesInfo3 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo3.Attribute3IsReleaseCaptured);

			relation.OU_IsPartAttrib3ReleaseCaptured = true;
			var partAttributesInfo4 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(true, partAttributesInfo4.Attribute3IsReleaseCaptured);
		}

		#endregion

		#region SerialNumber

		public void TestIsSerialNumberUsedByProduct()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct("Guitars", client);
			var whs = Helper.CreateWarehouse("WHS");
			var partAttributesInfo1 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo1.IsSerialNumberUsedByProduct);

			Helper.SetClientAttributeType(client, AttributeNumber.Serial, true);
			var partAttributesInfo2 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo2.IsSerialNumberUsedByProduct);

			var relation = part.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
			relation.OU_UseSerialNumber = true;
			var partAttributesInfo3 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(true, partAttributesInfo3.IsSerialNumberUsedByProduct);
		}

		public void TestIsSerialNumberReleaseCaptured()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct("Guitars", client);
			var whs = Helper.CreateWarehouse("WHS");
			Helper.SetClientAttributeType(client, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(client, part, AttributeNumber.Serial, true);
			var partAttributesInfo1 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(false, partAttributesInfo1.IsSerialNumberReleaseCaptured);

			Helper.SetProductAttributeUse(client, part, AttributeNumber.Serial, true, setReleaseCaptured: true);
			var partAttributesInfo2 = new WhsProductPartAttributesInfo(client, part, whs);
			AssertEquals(true, partAttributesInfo2.IsSerialNumberReleaseCaptured);
		}

		#endregion

		#region TestExpiryDateFormatString

		public void TestExpiryDateFormatString()
		{
			AssertEquals("", Parent.ExpiryDateFormatString);

			Parent.ExpiryDateFormatString = "DDMMYY";
			AssertEquals("DDMMYY", Parent.ExpiryDateFormatString);

			Parent.ExpiryDateFormatString = "";
			AssertEquals("", Parent.ExpiryDateFormatString);
		}

		#endregion

		#region TestPackingDateFormatString

		public void TestPackingDateFormatString()
		{
			AssertEquals("", Parent.PackingDateFormatString);

			Parent.PackingDateFormatString = "DDMMYY";
			AssertEquals("DDMMYY", Parent.PackingDateFormatString);

			Parent.PackingDateFormatString = "";
			AssertEquals("", Parent.PackingDateFormatString);
		}

		#endregion

		#region TestCompletePalletPickingUsed

		public void TestCompletePalletPickingUsed()
		{
			AssertEquals(false, Parent.CompletePalletPickingUsed);

			Parent.CompletePalletPickingUsed = true;
			AssertEquals(true, Parent.CompletePalletPickingUsed);

			Parent.CompletePalletPickingUsed = false;
			AssertEquals(false, Parent.CompletePalletPickingUsed);
		}

		#endregion

		#region TestIsAttributeNeutral

		public void TestIsAttributeNeutral()
		{
			AssertEquals(false, Parent.IsAttributeNeutral);

			Parent.IsAttributeNeutral = true;
			AssertEquals(true, Parent.IsAttributeNeutral);

			Parent.IsAttributeNeutral = false;
			AssertEquals(false, Parent.IsAttributeNeutral);
		}

		#endregion

		#region TestHasReleaseCapturedAttribute

		public void TestHasReleaseCapturedAttribute()
		{
			Parent.Attribute1IsReleaseCaptured = false;
			Parent.Attribute2IsReleaseCaptured = false;
			Parent.Attribute3IsReleaseCaptured = false;
			Parent.IsSerialNumberReleaseCaptured = false;
			AssertEquals(false, Parent.HasReleaseCapturedAttribute);

			Parent.Attribute1IsReleaseCaptured = true;
			AssertEquals(true, Parent.HasReleaseCapturedAttribute);

			Parent.Attribute1IsReleaseCaptured = false;
			Parent.Attribute2IsReleaseCaptured = true;
			AssertEquals(true, Parent.HasReleaseCapturedAttribute);

			Parent.Attribute2IsReleaseCaptured = false;
			Parent.Attribute3IsReleaseCaptured = true;
			AssertEquals(true, Parent.HasReleaseCapturedAttribute);

			Parent.Attribute3IsReleaseCaptured = false;
			Parent.IsSerialNumberReleaseCaptured = true;
			AssertEquals(true, Parent.HasReleaseCapturedAttribute);
		}

		#endregion

		#region TestHasSerialNumberAttribute

		public void TestHasSerialNumberAttribute()
		{
			Parent.IsSerialNumberUsedByProduct = false;
			AssertEquals(false, Parent.HasSerialNumberAttribute);

			Parent.IsSerialNumberUsedByProduct = true;
			AssertEquals(true, Parent.HasSerialNumberAttribute);
		}

		#endregion

		#region TestProductPK

		public void TestProductPK()
		{
			AssertEquals(Guid.Empty, Parent.ProductPK);

			var newGuid = new Guid();
			Parent.ProductPK = newGuid;
			AssertEquals(newGuid, Parent.ProductPK);
		}

		#endregion

		#region TestProductPK

		public void TestClientPK()
		{
			AssertEquals(Guid.Empty, Parent.ClientPK);

			var newGuid = new Guid();
			Parent.ClientPK = newGuid;
			AssertEquals(newGuid, Parent.ClientPK);
		}

		#endregion

		#region TestHi

		public void TestHi()
		{
			AssertEquals((short)0, Parent.Hi);

			Parent.Hi = 10;
			AssertEquals((short)10, Parent.Hi);

			Parent.Hi = 15;
			AssertEquals((short)15, Parent.Hi);
		}

		#endregion

		#region TestTi

		public void TestTi()
		{
			AssertEquals((short)0, Parent.Ti);

			Parent.Ti = 10;
			AssertEquals((short)10, Parent.Ti);

			Parent.Ti = 15;
			AssertEquals((short)15, Parent.Ti);
		}

		#endregion

		#region TestPartAttributeMaxLength

		public void TestPartAttributeMaxLength()
		{
			AssertEquals(WhsDocketLineSchema.WE_PartAttrib1.MaxLength, Parent.Attribute1MaxLength);
			AssertEquals(WhsDocketLineSchema.WE_PartAttrib2.MaxLength, Parent.Attribute2MaxLength);
			AssertEquals(WhsDocketLineSchema.WE_PartAttrib3.MaxLength, Parent.Attribute3MaxLength);
			AssertEquals(WhsPickLineSchema.WZ_ReleaseCapturedSerialNumber.MaxLength, Parent.SerialNumberMaxLength);
		}

		#endregion

		#endregion

		#region Implementation

		protected new WhsProductPartAttributesInfo Parent
		{
			get
			{
				return (WhsProductPartAttributesInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsProductPartAttributesInfo();
		}

		#endregion
	}
}
