using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	class PG26sData : IDataSerialiser
	{
		internal PG26sData(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		public IEnumerable<AEPAPG26> PG26s
		{
			get { return pg26s; }
		}

		public bool HasData
		{
			get { return pg26s != null && pg26s.Count > 0; }
		}

		public void Clear()
		{
			if (pg26s != null)
			{
				pg26s.Clear();
			}
		}

		List<AEPAPG26> pg26s;

		public void Add(AEPAPG26 pg26)
		{
			if (pg26 != null)
			{
				(pg26s = pg26s ?? new List<AEPAPG26>()).Add(pg26);
			}
		}

		public IEnumerable<ZString> Serialise()
		{
			if (HasData)
			{
				var packagingDetail = new ZStringBuilder();
				foreach (var pg26 in PG26s)
				{
					var package = new ZStringBuilder();
					package.AppendIfNotEmpty("ID: ", pg26.PackageIdentifier);
					package.AppendIfNotEmpty("Method:", Serialiser.AppendDescription(pg26.PackagingMethod, ShippingOrPackingingUnitList));
					package.AppendIfNotEmpty("Material: ", pg26.PackageMaterial);
					package.AppendIfNotEmpty("Fille: ", pg26.PackageFiller);
					packagingDetail.Append("Qty" + pg26.PackagingQualifier + ": " + (pg26.Quantity.ToString() + " " + pg26.UnitOfMeasurePackagingLevel).Trim() + (package.IsEmpty ? "" : " (" + package.ToStringWithDelimiterBetweenAppends(" ") + ")"));
				}
				yield return Serialiser.CreateLine(false, Serialiser.CreateValue("Packaging: ", packagingDetail.ToStringWithDelimiterBetweenAppends(",  ")));
			}
		}

		ShippingOrPackingingUnitList ShippingOrPackingingUnitList
		{
			get { return factory.GetCachedValue<ShippingOrPackingingUnitList>(); }
		}

		readonly BusinessObjectFactory factory;
	}
}
