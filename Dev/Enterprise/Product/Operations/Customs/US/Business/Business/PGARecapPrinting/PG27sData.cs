using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	class PG27sData : IDataSerialiser
	{
		internal PG27sData()
		{
		}

		public IEnumerable<AEPAPG27> PG27s
		{
			get { return pg27s; }
		}

		public bool HasData
		{
			get { return pg27s != null && pg27s.Count > 0; }
		}

		public void Clear()
		{
			if (pg27s != null)
			{
				pg27s.Clear();
			}
		}

		List<AEPAPG27> pg27s;

		public void Add(AEPAPG27 pg27)
		{
			if (pg27 != null)
			{
				(pg27s = pg27s ?? new List<AEPAPG27>()).Add(pg27);
			}
		}

		public IEnumerable<ZString> Serialise()
		{
			if (HasData)
			{
				var containerDetail = new ZStringBuilder();
				foreach (var pg27 in PG27s)
				{
					containerDetail.AppendIfNotEmpty(AddContainerDetail(pg27.ContainerNumberEquipmentID, pg27.ContainerLength, pg27.TypeOfContainer));
					containerDetail.AppendIfNotEmpty(AddContainerDetail(pg27.ContainerNumberEquipmentID1, pg27.ContainerLength1, pg27.TypeOfContainer1));
					containerDetail.AppendIfNotEmpty(AddContainerDetail(pg27.ContainerNumberEquipmentID2, pg27.ContainerLength2, pg27.TypeOfContainer2));
				}
				yield return Serialiser.CreateLine(false, Serialiser.CreateValue("Container Numbers: ", containerDetail.ToStringWithDelimiterBetweenAppends(",  ")));
			}
		}

		ZString AddContainerDetail(ZString containerNumber, ZInt containerLength, ZString typeOfContainer)
		{
			var result = ZString.Empty;
			if (!containerNumber.IsEmpty)
			{
				var detail = new ZStringBuilder();
				detail.AppendIfNotEmpty(containerLength.IsEmpty ? "" : containerLength.ToString());
				string type;
				switch (typeOfContainer)
				{
					case "1":
						type = "Refrigerated";
						break;
					case "2":
						type = "Not refrigerated";
						break;
					default:
						type = typeOfContainer;
						break;
				}
				detail.AppendIfNotEmpty(type);
				result = detail.IsEmpty ? containerNumber : Serialiser.AddDelimited(containerNumber, detail.ToStringWithDelimiterBetweenAppends(" "), Serialiser.DelimitedType.Parenthesis);
			}
			return result;
		}
	}
}
