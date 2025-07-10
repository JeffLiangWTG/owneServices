using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	class PG07AndPG08sData : IDataSerialiser
	{
		internal PG07AndPG08sData(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		public AEPAPG07 PG07;
		public IEnumerable<AEPAPG08> PG08s
		{
			get { return pg08s; }
		}

		public bool HasDataPG08s
		{
			get { return pg08s != null && pg08s.Count > 0; }
		}

		public void Clear()
		{
			PG07 = null;
			if (pg08s != null)
			{
				pg08s.Clear();
			}
		}

		List<AEPAPG08> pg08s;

		public void Add(AEPAPG08 pg08)
		{
			if (pg08 != null)
			{
				(pg08s = pg08s ?? new List<AEPAPG08>()).Add(pg08);
			}
		}

		public IEnumerable<ZString> Serialise()
		{
			var pg07 = PG07;
			if (pg07 != null)
			{
				var ids = new ZStringBuilder();
				ids.AppendIfNotEmpty(pg07.ItemIdentityNumber);
				if (HasDataPG08s)
				{
					PG08s.ForEach(x =>
					{
						ids.AppendIfNotEmpty(x.ItemIdentityNumber);
						ids.AppendIfNotEmpty(x.ItemIdentityNumber1);
						ids.AppendIfNotEmpty(x.ItemIdentityNumber2);
						ids.AppendIfNotEmpty(x.ItemIdentityNumber3);
					});
				}
				var itemIdentityNumberQualifier = Serialiser.PrependDescription(pg07.ItemIdentityNumberQualifier, ItemIdentityNumberQualifierList);
				if (itemIdentityNumberQualifier == pg07.ItemIdentityNumberQualifier)
				{
					itemIdentityNumberQualifier = "ID Type '" + itemIdentityNumberQualifier + "'";
				}
				yield return Serialiser.CreateLine(false, Serialiser.CreateValue("Brand Name: ", pg07.TradeNameBrandName), Serialiser.CreateValue("Model: ", pg07.Model), Serialiser.CreateValue("Manufacture Date: ", pg07.ManufactureMonthAndYear), Serialiser.CreateValue(itemIdentityNumberQualifier + ": ", ids.ToStringWithDelimiterBetweenAppends(", ")));
			}
		}

		ItemIdentityNumberQualifierList ItemIdentityNumberQualifierList
		{
			get { return factory.GetCachedValue<ItemIdentityNumberQualifierList>(); }
		}
		readonly BusinessObjectFactory factory;
	}
}
