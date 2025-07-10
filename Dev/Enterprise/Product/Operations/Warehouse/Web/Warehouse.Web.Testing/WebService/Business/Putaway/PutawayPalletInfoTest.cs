using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PutawayPalletInfo))]
	class PutawayPalletInfoTest : DataObjectInfoTestCase<PutawayPalletInfo>
	{
		#region TestConstructor

		public void TestConstructor_Default()
		{
			var palletInfo = new PutawayPalletInfo();
			AssertEquals("Pallet ID correct", "", palletInfo.PalletID);
			AssertEquals("Location correct", "", palletInfo.Location);
			AssertEquals("Location correct", "", palletInfo.Location_UserFriendly);
			AssertEquals("LocationFormattedCheckDigit correct", "", palletInfo.LocationFormattedCheckDigit);
			AssertEquals("ClientCode correct", "", palletInfo.ClientCode);
			AssertEquals("DocketPK correct", Guid.Empty, palletInfo.DocketPK);
		}

		public void TestConstructor_WithDocketPK()
		{
			var testPK = Guid.NewGuid();
			var palletInfo = new PutawayPalletInfo("PLT4", "A0102", "A-01-02", "11", "Client", testPK);
			AssertEquals("Pallet ID correct", "PLT4", palletInfo.PalletID);
			AssertEquals("Location correct", "A0102", palletInfo.Location);
			AssertEquals("Location correct", "A-01-02", palletInfo.Location_UserFriendly);
			AssertEquals("LocationFormattedCheckDigit correct", "11", palletInfo.LocationFormattedCheckDigit);
			AssertEquals("ClientCode correct", "Client", palletInfo.ClientCode);
			AssertEquals("DocketPK correct", testPK, palletInfo.DocketPK);
		}

		public void TestConstructor_WithLocationPutawaySequence()
		{
			var locationPK = Guid.NewGuid();
			var docketPK = Guid.NewGuid();
			var palletInfo = new PutawayPalletInfo("PLT4", "A0102", "A-01-02", "11", locationPK, "Client", docketPK, "10", "PLT1");
			AssertEquals("Pallet ID correct", "PLT4", palletInfo.PalletID);
			AssertEquals("Location correct", "A0102", palletInfo.Location);
			AssertEquals("Location correct", "A-01-02", palletInfo.Location_UserFriendly);
			AssertEquals("LocationFormattedCheckDigit correct", "11", palletInfo.LocationFormattedCheckDigit);
			AssertEquals("ClientCode correct", "Client", palletInfo.ClientCode);
			AssertEquals("DocketPK correct", docketPK, palletInfo.DocketPK);
			AssertEquals("LocationPutawaySequence correct", "10", palletInfo.LocationPutawaySequence);
			AssertEquals("AllocatedPalletID correct", "PLT1", palletInfo.AllocatedPalletID);
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			var palletInfo = new PutawayPalletInfo();
			AssertEquals("Pallet ID correct", "", palletInfo.PalletID);

			palletInfo.PalletID = "ABC";
			AssertEquals("Pallet ID correct", "ABC", palletInfo.PalletID);
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			var palletInfo = new PutawayPalletInfo();
			AssertEquals("Location correct", "", palletInfo.Location);

			palletInfo.Location = "GG-1";
			AssertEquals("Location correct", "GG-1", palletInfo.Location);
		}

		#endregion

		#region TestLocationFormattedCheckDigit

		public void TestLocationFormattedCheckDigit()
		{
			var palletInfo = new PutawayPalletInfo();
			AssertEquals("LocationFormattedCheckDigit correct", "", palletInfo.LocationFormattedCheckDigit);

			palletInfo.LocationFormattedCheckDigit = "11";
			AssertEquals("LocationFormattedCheckDigit correct", "11", palletInfo.LocationFormattedCheckDigit);
		}

		#endregion

		#region TestClientCode

		public void TestClientCode()
		{
			var palletInfo = new PutawayPalletInfo();
			AssertEquals("ClientCode correct", "", palletInfo.ClientCode);

			palletInfo.ClientCode = "Client";
			AssertEquals("ClientCode correct", "Client", palletInfo.ClientCode);
		}

		#endregion

		#region TestDocketPK

		public void TestDocketPK()
		{
			var palletInfo = new PutawayPalletInfo();
			AssertEquals("DocketPK correct", Guid.Empty, palletInfo.DocketPK);

			var testPK = Guid.NewGuid();
			palletInfo.DocketPK = testPK;
			AssertEquals("DocketPK correct", testPK, palletInfo.DocketPK);
		}

		#endregion

		#region TestPopulateInfoFromProducts

		public void TestPopulateInfoFromProducts_ConsistentItems()
		{
			var palletInfo = new PutawayPalletInfo();
			palletInfo.ConsolidateProductInfos(
				new[] { "Product1", "Product1" },
				"PartAttribute1",
				new[] { "Attrib1", "Attrib1" },
				"PartAttribute2",
				new[] { "Attrib2", "Attrib2" },
				"PartAttribute3",
				new[] { "Attrib3", "Attrib3" },
				new[] { "SN1", "SN1" },
				new[] { new DateTime(2023, 1, 1), new DateTime(2023, 1, 1) },
				new[] { new DateTime(2023, 1, 1), new DateTime(2023, 1, 1) });
			AssertEquals("Product1", palletInfo.ProductCode);
			AssertEquals("PartAttribute1", palletInfo.PartAttrib1Name);
			AssertEquals("Attrib1", palletInfo.PartAttrib1);
			AssertEquals("PartAttribute2", palletInfo.PartAttrib2Name);
			AssertEquals("Attrib2", palletInfo.PartAttrib2);
			AssertEquals("PartAttribute3", palletInfo.PartAttrib3Name);
			AssertEquals("Attrib3", palletInfo.PartAttrib3);
			AssertEquals("SN1", palletInfo.SerialNumber);
			AssertEquals(new DateTime(2023, 1, 1).ToShortDateString(), palletInfo.ExpiryDate);
			AssertEquals(new DateTime(2023, 1, 1).ToShortDateString(), palletInfo.PackingDate);
		}

		public void TestPopulateInfoFromProducts_MismatchedItems()
		{
			var palletInfo = new PutawayPalletInfo();
			palletInfo.ConsolidateProductInfos(
				new[] { "Product1", "Product2" },
				"PartAttribute1",
				new[] { "Attrib1", "Attrib1_other" },
				"PartAttribute2",
				new[] { "Attrib2", "Attrib2_other" },
				"PartAttribute3",
				new[] { "Attrib3", "Attrib3_other" },
				new[] { "SN1", "SN2" },
				new[] { new DateTime(2023, 1, 1), new DateTime(2022, 1, 1) },
				new[] { new DateTime(2023, 1, 1), new DateTime(2022, 1, 1) });
			AssertEquals("<Many>", palletInfo.ProductCode);
			AssertEquals("PartAttribute1", palletInfo.PartAttrib1Name);
			AssertEquals("<Many>", palletInfo.PartAttrib1);
			AssertEquals("PartAttribute2", palletInfo.PartAttrib2Name);
			AssertEquals("<Many>", palletInfo.PartAttrib2);
			AssertEquals("PartAttribute3", palletInfo.PartAttrib3Name);
			AssertEquals("<Many>", palletInfo.PartAttrib3);
			AssertEquals("<Many>", palletInfo.SerialNumber);
			AssertEquals("<Many>", palletInfo.ExpiryDate);
			AssertEquals("<Many>", palletInfo.PackingDate);
		}

		public void TestPopulateInfoFromProducts_BlankAttributesOnItems()
		{
			var palletInfo = new PutawayPalletInfo();
			palletInfo.ConsolidateProductInfos(
				new[] { "" },
				"PartAttribute1",
				new[] { "" },
				"PartAttribute2",
				new[] { "" },
				"PartAttribute3",
				new[] { "" },
				new[] { "" },
				new[] { DateTime.MinValue },
				new[] { DateTime.MinValue });
			AssertEquals("", palletInfo.ProductCode);
			AssertEquals("PartAttribute1", palletInfo.PartAttrib1Name);
			AssertEquals("", palletInfo.PartAttrib1);
			AssertEquals("PartAttribute2", palletInfo.PartAttrib2Name);
			AssertEquals("", palletInfo.PartAttrib2);
			AssertEquals("PartAttribute3", palletInfo.PartAttrib3Name);
			AssertEquals("", palletInfo.PartAttrib3);
			AssertEquals("", palletInfo.SerialNumber);
			AssertEquals("", palletInfo.ExpiryDate);
			AssertEquals("", palletInfo.PackingDate);
		}

		#endregion

		#region Implementation

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new PutawayPalletInfo();
		}

		#endregion
	}
}
