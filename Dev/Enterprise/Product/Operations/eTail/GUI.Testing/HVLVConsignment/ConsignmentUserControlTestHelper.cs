using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.eTail.Business;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	public class ConsignmentUserControlTestHelper : Assertion
	{
		public static void AssertColumnStyleIsUpperCase(IEnumerable<ZGridColumnInfo> columnInfos, string columnName)
		{
			var columnInfo = columnInfos.Single(columnStyle => columnStyle.ColumnName == columnName);
			AssertEquals($"{columnName} column should always be Upper Case", CharacterCasing.Upper, columnInfo.CharacterCasing);
		}

		public static HVLVConsignment BuildConsignment(HVLVConsignmentHeader header)
		{
			var consignment = header.Consignments.AddNew();
			consignment.HVC_IsActive = true;
			consignment.HVC_Status = "BKD";
			consignment.HVC_ConsigneeName = "name";
			consignment.HVC_ConsigneeAddress1 = "address";
			consignment.HVC_ConsigneeCity = "city";
			consignment.HVC_ConsigneePostcode = "3053";
			consignment.HVC_ShipperName = "name";
			consignment.HVC_ShipperAddress1 = "address";
			consignment.HVC_ShipperCity = "city";
			consignment.HVC_RN_NKConsigneeCountryCode = "CN";
			return consignment;
		}
	}
}
