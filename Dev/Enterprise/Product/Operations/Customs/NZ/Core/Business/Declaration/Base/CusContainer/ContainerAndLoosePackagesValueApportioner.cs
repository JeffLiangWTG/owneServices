using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class ContainerAndLoosePackagesValueApportioner
	{
		public ContainerAndLoosePackagesValueApportioner(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration), "Must have a valid JobDeclaration Passed into the Constructor");
		}
		readonly JobDeclaration declaration;

		public ZDecimal GetValueForLoosePackages()
		{
			return GetContainerValue(ZGuid.Empty);
		}

		public ZDecimal GetValueForContainer(CusContainer container)
		{
			return GetContainerValue(container.PK);
		}

		#region Implementation
		ZDecimal GetContainerValue(ZGuid containerPK)
		{
			return ContainerValues.TryGetValue(containerPK, out var value) ? value : ZDecimal.Zero;
		}

		Dictionary<ZGuid, ZDecimal> ContainerValues => declaration.Factory.GetValue(ref cachedContainerValues, GetContainerValues);

		CachedProperty<Dictionary<ZGuid, ZDecimal>> cachedContainerValues;

		Dictionary<ZGuid, ZDecimal> GetContainerValues()
		{
			Dictionary<ZGuid, ZDecimal> result = new Dictionary<ZGuid, ZDecimal>();
			ZDecimal totalGoodsWeight = declaration.JE_TotalWeight;
			ZDecimal totalGoodsWeightFromContainers = declaration.CusContainers.TotalGoodsWeight;
			if (totalGoodsWeight >= totalGoodsWeightFromContainers)
			{
				SortedList<ZString, ZGuid> sortedByValue = new SortedList<ZString, ZGuid>(new HighestToLowestComparer());
				ZDecimal totalAmount = declaration.JE_ECI_InvoiceAmount;
				ZDecimal totalAmountApportioned = 0.00m;

				foreach (CusContainer container in declaration.CusContainers)
				{
					ZDecimal rawApportionedValue = (totalGoodsWeight.IsEmpty ? 0 : container.CO_Weight / totalGoodsWeight) * totalAmount;
					ZDecimal apportionedValue = rawApportionedValue.Round(2);
					totalAmountApportioned += apportionedValue;
					result.Add(container.PK, apportionedValue);
					sortedByValue.Add(apportionedValue.ToString().PadLeft(20) + container.PK.ToString(), container.PK);
				}

				if (declaration.Bills.Count > 0)
				{
					Bill houseBill = declaration.Bills[0];
					if (houseBill.LoosePackageCount > 0 && totalAmount > totalAmountApportioned)
					{
						result.Add(ZGuid.Empty, totalAmount - totalAmountApportioned);
					}
				}

				if (totalGoodsWeight != 0
					&& totalGoodsWeight == totalGoodsWeightFromContainers
					&& totalAmount != totalAmountApportioned)
				{
					int differenceInCents = Convert.ToInt32(((totalAmount - totalAmountApportioned) * 100));
					bool differenceIsPositive = differenceInCents > 0;
					int absoluteDifference = Math.Abs(differenceInCents);
					for (int index = 0; index < absoluteDifference; index++)
					{
						ZGuid containerPK = sortedByValue.Values[index];
						result[containerPK] += (differenceIsPositive ? 0.01m : -0.01m);
					}
				}
			}
			return result;
		}

		class HighestToLowestComparer : IComparer<ZString>
		{
			public int Compare(ZString x, ZString y)
			{
				int result = decimal.Compare(Convert.ToDecimal(x.Left(20)), Convert.ToDecimal(y.Left(20))) * -1;
				if (result == 0)
				{
					result = string.Compare(x.Substring(20), y.Substring(20));
				}
				return result;
			}
		}
		#endregion
	}
}
