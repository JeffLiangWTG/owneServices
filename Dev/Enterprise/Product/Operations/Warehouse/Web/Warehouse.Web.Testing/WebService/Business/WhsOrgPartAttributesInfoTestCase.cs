using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsOrgPartAttributesInfo))]
	public class WhsOrgPartAttributesInfoTestCase : DataObjectInfoTestCase<WhsOrgPartAttributesInfo>
	{
		#region Test Cases

		public void TestAdditionalContructors()
		{
			WhsOrgPartAttributesInfo partAttributesInfo = new WhsOrgPartAttributesInfo();
			AssertEquals("", partAttributesInfo.Attribute1Caption);
			AssertEquals(false, partAttributesInfo.Attribute1IsMandatory);
			AssertEquals("", partAttributesInfo.Attribute2Caption);
			AssertEquals(false, partAttributesInfo.Attribute2IsMandatory);
			AssertEquals("", partAttributesInfo.Attribute3Caption);
			AssertEquals(false, partAttributesInfo.Attribute3IsMandatory);
			AssertEquals(false, partAttributesInfo.IsSerialNumberUsedByOrganisation);
			AssertEquals("", partAttributesInfo.SerialNumberCaption);
			AssertEquals(false, partAttributesInfo.ExpiryDateIsUsed);
			AssertEquals(false, partAttributesInfo.PackingDateIsUsed);

			TestAdditionalConstructorsCore();
		}

		protected virtual void TestAdditionalConstructorsCore()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			WhsTestHelperFunctions helper = new WhsTestHelperFunctions(factory);
			OrgHeader org = helper.CreateClient();
			org.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			org.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			org.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";
			org.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			org.MiscServ.OM_IMUseSerialNumber = true;
			org.MiscServ.OM_IMUseExpiryDate = true;
			org.MiscServ.OM_IMUsePackingDate = false;

			WhsOrgPartAttributesInfo partAttributesInfo = new WhsOrgPartAttributesInfo(org);
			AssertEquals("Attr1 Name", partAttributesInfo.Attribute1Caption);
			AssertEquals(true, partAttributesInfo.Attribute1IsMandatory);
			AssertEquals("Attr2 Name", partAttributesInfo.Attribute2Caption);
			AssertEquals(false, partAttributesInfo.Attribute2IsMandatory);
			AssertEquals("Attr3 Name", partAttributesInfo.Attribute3Caption);
			AssertEquals(true, partAttributesInfo.Attribute3IsMandatory);
			AssertEquals("Serial #", partAttributesInfo.SerialNumberCaption);
			AssertEquals(true, partAttributesInfo.ExpiryDateIsUsed);
			AssertEquals(false, partAttributesInfo.PackingDateIsUsed);

			org.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name2";
			org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			org.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name2";
			org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			org.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name2";
			org.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.BatchNumber;
			org.MiscServ.OM_IMUseExpiryDate = false;
			org.MiscServ.OM_IMUsePackingDate = true;

			partAttributesInfo = new WhsOrgPartAttributesInfo(org);
			AssertEquals("Attr1 Name2", partAttributesInfo.Attribute1Caption);
			AssertEquals(false, partAttributesInfo.Attribute1IsMandatory);
			AssertEquals("Attr2 Name2", partAttributesInfo.Attribute2Caption);
			AssertEquals(true, partAttributesInfo.Attribute2IsMandatory);
			AssertEquals("Attr3 Name2", partAttributesInfo.Attribute3Caption);
			AssertEquals(true, partAttributesInfo.Attribute3IsMandatory);
			AssertEquals(true, partAttributesInfo.IsSerialNumberUsedByOrganisation);
			AssertEquals("Serial #", partAttributesInfo.SerialNumberCaption);
			AssertEquals(false, partAttributesInfo.ExpiryDateIsUsed);
			AssertEquals(true, partAttributesInfo.PackingDateIsUsed);
		}

		public void TestAttribute1Caption()
		{
			AssertEquals("", Parent.Attribute1Caption);

			Parent.Attribute1Caption = "1234";
			AssertEquals("1234", Parent.Attribute1Caption);

			Parent.Attribute1Caption = "4321";
			AssertEquals("4321", Parent.Attribute1Caption);
		}

		public void TestAttribute1IsMandatory()
		{
			AssertEquals(false, Parent.Attribute1IsMandatory);

			Parent.Attribute1IsMandatory = true;
			AssertEquals(true, Parent.Attribute1IsMandatory);

			Parent.Attribute1IsMandatory = false;
			AssertEquals(false, Parent.Attribute1IsMandatory);
		}

		public void TestAttribute2Caption()
		{
			AssertEquals("", Parent.Attribute2Caption);

			Parent.Attribute2Caption = "1234";
			AssertEquals("1234", Parent.Attribute2Caption);

			Parent.Attribute2Caption = "4321";
			AssertEquals("4321", Parent.Attribute2Caption);
		}

		public void TestAttribute2IsMandatory()
		{
			AssertEquals(false, Parent.Attribute2IsMandatory);

			Parent.Attribute2IsMandatory = true;
			AssertEquals(true, Parent.Attribute2IsMandatory);

			Parent.Attribute2IsMandatory = false;
			AssertEquals(false, Parent.Attribute2IsMandatory);
		}

		public void TestAttribute3Caption()
		{
			AssertEquals("", Parent.Attribute3Caption);

			Parent.Attribute3Caption = "1234";
			AssertEquals("1234", Parent.Attribute3Caption);

			Parent.Attribute3Caption = "4321";
			AssertEquals("4321", Parent.Attribute3Caption);
		}

		public void TestAttribute3IsMandatory()
		{
			AssertEquals(false, Parent.Attribute3IsMandatory);

			Parent.Attribute3IsMandatory = true;
			AssertEquals(true, Parent.Attribute3IsMandatory);

			Parent.Attribute3IsMandatory = false;
			AssertEquals(false, Parent.Attribute3IsMandatory);
		}

		public void TestIsSerialNumberUsedByOrganisation()
		{
			AssertEquals(false, Parent.IsSerialNumberUsedByOrganisation);

			Parent.IsSerialNumberUsedByOrganisation = true;
			AssertEquals(true, Parent.IsSerialNumberUsedByOrganisation);

			Parent.IsSerialNumberUsedByOrganisation = false;
			AssertEquals(false, Parent.IsSerialNumberUsedByOrganisation);
		}

		public void TestSerialNumberCaption()
		{
			AssertEquals("", Parent.SerialNumberCaption);

			Parent.SerialNumberCaption = "Serial # 1 of 3";
			AssertEquals("Serial # 1 of 3", Parent.SerialNumberCaption);

			Parent.SerialNumberCaption = "Serial # 2 of 3";
			AssertEquals("Serial # 2 of 3", Parent.SerialNumberCaption);
		}

		public void TestExpiryDateIsUsed()
		{
			AssertEquals(false, Parent.ExpiryDateIsUsed);

			Parent.ExpiryDateIsUsed = true;
			AssertEquals(true, Parent.ExpiryDateIsUsed);

			Parent.ExpiryDateIsUsed = false;
			AssertEquals(false, Parent.ExpiryDateIsUsed);
		}

		public void TestPackingDateIsUsed()
		{
			AssertEquals(false, Parent.PackingDateIsUsed);

			Parent.PackingDateIsUsed = true;
			AssertEquals(true, Parent.PackingDateIsUsed);

			Parent.PackingDateIsUsed = false;
			AssertEquals(false, Parent.PackingDateIsUsed);
		}

		#endregion

		#region Implementation

		protected new WhsOrgPartAttributesInfo Parent
		{
			get
			{
				return (WhsOrgPartAttributesInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsOrgPartAttributesInfo();
		}

		#endregion
	}
}
