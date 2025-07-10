using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class UnitOfMeasure : IUnitOfMeasure
	{
		protected UnitOfMeasure(ZPropertyInfo totalQuantityInfo, ZString totalUQ)
			: this(Argument.NotNull(totalQuantityInfo, nameof(totalQuantityInfo)).HumanReadableName, totalUQ)
		{
		}

		protected UnitOfMeasure(ZString name, ZString totalUQ)
		{
			if (name.IsEmpty)
			{
				throw new ArgumentException("Unit of Measure Name is mandatory.", nameof(name));
			}

			Name = name;
			TotalUQ = totalUQ;
		}

		public ZString Name { get; }
		public ZString TotalUQ { get; }

		public abstract int DecimalPlacesForRounding { get; }

		ZDecimal IUnitOfMeasure.GetQuantityFromProduct(OrgSupplierPart part) => GetQuantityFromProduct(Argument.NotNull(part, nameof(part)));
		protected abstract ZDecimal GetQuantityFromProduct(OrgSupplierPart part);

		ZString IUnitOfMeasure.GetUQFromProduct(OrgSupplierPart part) => GetUQFromProduct(Argument.NotNull(part, nameof(part)));
		protected abstract ZString GetUQFromProduct(OrgSupplierPart part);
	}
}
