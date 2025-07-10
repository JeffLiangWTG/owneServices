using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal abstract class WhsDocketLine_OldTestCase : WhsBusinessObjectTestCase
	{
		#region General

		public void TestTypeDecider()
		{
			WhsDocketLine line = (WhsDocketLine)GetNewBusinessObject();
			AssertEquals("DocketType", GetExpectedDocketType(), line.DocketType);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#endregion

		#region Implementation

		protected void AssertLine(WhsDocketLine line, ZGuid productPK, ZString locationString, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3)
		{
			AssertEquals("Product incorrect", productPK, line.WE_OP);
			AssertEquals("Location incorrect", locationString, line.LocationString);
			AssertEquals("Expiry Date incorrect", expiryDate, line.WE_ExpiryDate);
			AssertEquals("Packing Date incorrect", packingDate, line.WE_PackingDate);
			AssertEquals("Part Attrib1 incorrect", partAttrib1, line.WE_PartAttrib1);
			AssertEquals("Part Attrib2 incorrect", partAttrib2, line.WE_PartAttrib2);
			AssertEquals("Part Attrib3 incorrect", partAttrib3, line.WE_PartAttrib3);
		}

		protected void ClearLine(WhsDocketLine line)
		{
			line.WE_OP = ZGuid.Empty;
			line.WE_WL = ZGuid.Empty;
			line.LocationString = "";
			line.WE_ExpiryDate = ZDate.Empty;
			line.WE_PackingDate = ZDate.Empty;
			line.WE_PartAttrib1 = "";
			line.WE_PartAttrib2 = "";
			line.WE_PartAttrib3 = "";
		}

		protected abstract Type GetExpectedDocketType();

		#endregion
	}
}
