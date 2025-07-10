using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	class PG24sData : IDataSerialiser
	{
		internal PG24sData(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		public IEnumerable<AEPAPG24> PG24s
		{
			get { return pg24s; }
		}

		public bool HasData
		{
			get { return pg24s != null && pg24s.Count > 0; }
		}

		public void Clear()
		{
			if (pg24s != null)
			{
				pg24s.Clear();
			}
		}

		List<AEPAPG24> pg24s;

		public void Add(AEPAPG24 pg24)
		{
			if (pg24 != null)
			{
				(pg24s = pg24s ?? new List<AEPAPG24>()).Add(pg24);
			}
		}

		public IEnumerable<ZString> Serialise()
		{
			if (HasData)
			{
				foreach (var groupedPG24s in PG24s.GroupBy(x => x.RemarksTypeCode))
				{
					var pg24Data = new ZStringBuilder();
					var remarksType = "Remarks";
					if (!groupedPG24s.Key.IsEmpty)
					{
						var remarksTypeDesc = RemarksTypeCodeList.GetDescriptionFromCode(groupedPG24s.Key);
						if (!string.IsNullOrEmpty(remarksType))
						{
							remarksType = remarksTypeDesc;
						}
						remarksType += " (" + groupedPG24s.Key + ")";
					}
					foreach (var pg24 in groupedPG24s)
					{
						pg24Data.AppendIfNotEmpty((pg24.RemarksCode + " " + pg24.RemarksText).Trim());
					}
					yield return Serialiser.CreateLine(false, Serialiser.CreateValue(remarksType + ": ", pg24Data.ToStringWithDelimiterBetweenAppends(", ")));
				}
			}
		}

		RemarksTypeCodeList RemarksTypeCodeList
		{
			get { return factory.GetCachedValue<RemarksTypeCodeList>(); }
		}

		readonly BusinessObjectFactory factory;
	}
}
