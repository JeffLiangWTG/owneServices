using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsLightDocketLineInfo))]
	public class WhsLightDocketLineInfoTestCase : DataObjectInfoTestCase<WhsLightDocketLineInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var lightDocketLine = new WhsLightDocketLineInfo();
			AssertConstructor(lightDocketLine);
		}

		protected virtual void AssertConstructor(WhsLightDocketLineInfo lightDocketLine)
		{
			AssertNotNull(lightDocketLine);
			AssertEquals(Guid.Empty, lightDocketLine.PK);
			AssertEquals("", lightDocketLine.ProductCode);
			AssertEquals("", lightDocketLine.Location);
			AssertEquals("", lightDocketLine.PalletID);
			AssertEquals(Guid.Empty, lightDocketLine.DockDoorLocationPK);
			AssertEquals("", lightDocketLine.InventoryHeldCode);
			AssertEquals(0m, lightDocketLine.Packs);
			AssertEquals("", lightDocketLine.PackUQ);
			AssertEquals("", lightDocketLine.Attribute1);
			AssertEquals("", lightDocketLine.Attribute2);
			AssertEquals("", lightDocketLine.Attribute3);
			AssertEquals("", lightDocketLine.SerialNumber);
			AssertEquals(new DateTime(), lightDocketLine.ExpiryDate);
			AssertEquals(new DateTime(), lightDocketLine.PackingDate);
		}

		#endregion

		#region Properties

		public void TestPK()
		{
			AssertEquals(Guid.Empty, Parent.PK);

			var newPK = Guid.NewGuid();
			Parent.PK = newPK;
			AssertEquals(newPK, Parent.PK);
		}

		public void TestLocation()
		{
			AssertEquals("", Parent.Location);

			Parent.Location = "1234";
			AssertEquals("1234", Parent.Location);

			Parent.Location = "4321";
			AssertEquals("4321", Parent.Location);
		}

		public void TestLocation_UserFriendly()
		{
			AssertEquals("", Parent.Location_UserFriendly);

			Parent.Location_UserFriendly = "1234";
			AssertEquals("1234", Parent.Location_UserFriendly);

			Parent.Location_UserFriendly = "4321";
			AssertEquals("4321", Parent.Location_UserFriendly);
		}

		public void TestPalletID()
		{
			AssertEquals("", Parent.PalletID);

			Parent.PalletID = "1234";
			AssertEquals("1234", Parent.PalletID);

			Parent.PalletID = "4321";
			AssertEquals("4321", Parent.PalletID);
		}

		public void TestPacks()
		{
			AssertEquals(0m, Parent.Packs);

			Parent.Packs = 10m;
			AssertEquals(10m, Parent.Packs);

			Parent.Packs = 20.20m;
			AssertEquals(20.20m, Parent.Packs);
		}

		public void TestPackUQ()
		{
			AssertEquals("", Parent.PackUQ);

			Parent.PackUQ = "1234";
			AssertEquals("1234", Parent.PackUQ);

			Parent.PackUQ = "4321";
			AssertEquals("4321", Parent.PackUQ);
		}

		public void TestAttribute1()
		{
			AssertEquals("", Parent.Attribute1);

			Parent.Attribute1 = "1234";
			AssertEquals("1234", Parent.Attribute1);

			Parent.Attribute1 = "4321";
			AssertEquals("4321", Parent.Attribute1);
		}

		public void TestAttribute2()
		{
			AssertEquals("", Parent.Attribute2);

			Parent.Attribute2 = "1234";
			AssertEquals("1234", Parent.Attribute2);

			Parent.Attribute2 = "4321";
			AssertEquals("4321", Parent.Attribute2);
		}

		public void TestAttribute3()
		{
			AssertEquals("", Parent.Attribute3);

			Parent.Attribute3 = "1234";
			AssertEquals("1234", Parent.Attribute3);

			Parent.Attribute3 = "4321";
			AssertEquals("4321", Parent.Attribute3);
		}

		public void TestSerialNumber()
		{
			AssertEquals("", Parent.SerialNumber);

			Parent.SerialNumber = "1234";
			AssertEquals("1234", Parent.SerialNumber);

			Parent.SerialNumber = "4321";
			AssertEquals("4321", Parent.SerialNumber);
		}

		public void TestHeldCode()
		{
			AssertEquals("", Parent.InventoryHeldCode);

			Parent.InventoryHeldCode = "HEL";
			AssertEquals("HEL", Parent.InventoryHeldCode);

			Parent.InventoryHeldCode = "DAM";
			AssertEquals("DAM", Parent.InventoryHeldCode);
		}

		public void TestExpiryDate()
		{
			AssertEquals(new DateTime(), Parent.ExpiryDate);

			Parent.ExpiryDate = new DateTime(2008, 03, 12);
			AssertEquals(new DateTime(2008, 03, 12), Parent.ExpiryDate);

			Parent.ExpiryDate = new DateTime(2008, 03, 13);
			AssertEquals(new DateTime(2008, 03, 13), Parent.ExpiryDate);
		}

		public void TestPackingDate()
		{
			AssertEquals(new DateTime(), Parent.PackingDate);

			Parent.PackingDate = new DateTime(2008, 03, 12);
			AssertEquals(new DateTime(2008, 03, 12), Parent.PackingDate);

			Parent.PackingDate = new DateTime(2008, 03, 13);
			AssertEquals(new DateTime(2008, 03, 13), Parent.PackingDate);
		}

		#endregion

		#region Implementation

		protected new WhsLightDocketLineInfo Parent => (WhsLightDocketLineInfo)base.Parent;

		protected override DataObjectInfo GetNewObjectInfo() => new WhsLightDocketLineInfo();

		#endregion
	}
}
