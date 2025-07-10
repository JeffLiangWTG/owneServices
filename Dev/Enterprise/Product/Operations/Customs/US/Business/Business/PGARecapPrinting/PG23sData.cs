using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	class PG23sData : IDataSerialiser
	{
		internal PG23sData(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		public IEnumerable<AEPAPG23> PG23s
		{
			get { return pg23s; }
		}

		public bool HasData
		{
			get { return pg23s != null && pg23s.Count > 0; }
		}

		public void Clear()
		{
			if (pg23s != null)
			{
				pg23s.Clear();
			}
		}

		List<AEPAPG23> pg23s;

		public void Add(AEPAPG23 pg23)
		{
			if (pg23 != null)
			{
				(pg23s = pg23s ?? new List<AEPAPG23>()).Add(pg23);
			}
		}

		public IEnumerable<ZString> Serialise()
		{
			if (HasData)
			{
				bool isFirst = true;
				foreach (var pg23 in PG23s)
				{
					yield return Serialiser.CreateLine(false, Serialiser.CreateValue(isFirst ? "Affirmation of Compliance Code:  " : AffirmationOfCompliancePadding, Serialiser.AppendDescription(pg23.AffirmationOfComplianceCode, ACE_AffirmationOfComplianceList, Serialiser.DelimitedType.Parenthesis) + (pg23.AffirmationOfComplianceDescription.IsEmpty ? "" : " - " + pg23.AffirmationOfComplianceDescription)));
					isFirst = false;
				}
			}
		}
		const string AffirmationOfCompliancePadding = "                                 ";

		ACE_AffirmationOfComplianceList ACE_AffirmationOfComplianceList
		{
			get { return factory.GetCachedValue<ACE_AffirmationOfComplianceList>(); }
		}

		readonly BusinessObjectFactory factory;
	}
}
