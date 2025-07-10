using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ProfitShareShipmentsGridTest : TestCaseWithFactory
	{
		public void TestFindBoxColumnsDataHasListAttributes()
		{
			using (var grid = new ProfitShareShipmentsGrid())
			{
				var propertiesToCheck = new HashSet<string>();
				var columnStyleInfos = grid.ColumnStyles.OfType<ZCodeFindBoxColumnStyleInfo>();
				columnStyleInfos.ForEach(x => propertiesToCheck.Add(x.ColumnName));

				var type = typeof(ProfitShareForwardingShipmentWrapper);
				foreach (var propertyName in propertiesToCheck)
				{
					var propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
					var listAttribute = (ListAttribute)System.Attribute.GetCustomAttributes(propertyInfo, typeof(ListAttribute), false).FirstOrDefault();
					AssertNotNull($"{propertyName} should have a List attribute", listAttribute);
				}
			}
		}
	}
}
