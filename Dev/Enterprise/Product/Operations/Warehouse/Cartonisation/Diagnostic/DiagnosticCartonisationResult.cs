using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	public class DiagnosticCartonisationResult
	{
		public DiagnosticCartonisationResult(CartonsCollection cartons, ItemsToPackCollection products, IEnumerable<ICartonWithItems> result, TimeSpan executionDuration)
		{
			Cartons = cartons;
			Products = products;
			Result = result;
			ExecutionDuration = executionDuration;
		}

		readonly CartonsCollection Cartons;
		readonly ItemsToPackCollection Products;
		readonly IEnumerable<ICartonWithItems> Result;
		readonly TimeSpan ExecutionDuration;

		#region ToString

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Algorithm took {0} to run", ExecutionDuration));
			foreach (var item in Result)
			{
				var carton = Cartons.Cast<DummyCartonDefinition>().SingleOrDefault(c => c.PK == item.CartonPK);
				if (carton == null)
				{
					sb.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Produced CartonPK='{0}' was not in original list.", item.CartonPK));
				}
				else
				{
					sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} :", carton));
				}
				foreach (var contentResult in item.Items)
				{
					var product = Products.Cast<DummyCartonisableItem>().SingleOrDefault(p => p.PK == contentResult.CartonisableItemPK);
					if (product == null)
					{
						sb.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Produced CartonisableItemPK='{0}' was not in original list.", contentResult.CartonisableItemPK));
					}
					else
					{
						sb.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"-->  {0} x {1} from {2}", contentResult.Quantity, product.ItemDefinition, product.Location));
					}
				}
			}
			if (!Result.Any())
			{
				sb.AppendLine((NoResString)"No results were returned");
			}
			return sb.ToString();
		}

		#endregion
	}
}

