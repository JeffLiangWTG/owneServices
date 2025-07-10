
namespace Enterprise.Freight.Agency.Business
{
	using System.Collections.Generic;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.ZArchitecture.Core;

	public class PortMessageTargetPortList : ReadOnlyCodeDescriptionPairList
	{
		public PortMessageTargetPortList(JobVoyage voyage)
		{
			Voyage = Argument.NotNull(voyage, "voyage");
		}

		public void Load()
		{
			LoadCore();
		}

		public new PortMessageTargetPort this[int index]
		{
			get { return (PortMessageTargetPort)base[index]; }
		}

		public PortMessageTargetPort this[string code]
		{
			get
			{
				int index = IndexOfCode(code);
				return index < 0 ? null : this[index];
			}
		}

		protected void LoadCore()
		{
			Elements.Clear();

			var lookup = new SortedDictionary<string, PortDirections>();

			foreach (VoyageOrigin origin in Voyage.Origins)
			{
				if (ShouldIncludePort(origin.JA_RL_NKPortOfLoading))
				{
					lookup[origin.JA_RL_NKPortOfLoading] = PortDirections.Load;
				}
			}

			foreach (VoyageDestination destination in Voyage.Destinations)
			{
				if (ShouldIncludePort(destination.JB_RL_NKPortOfDischarge))
				{
					lookup.TryGetValue(destination.JB_RL_NKPortOfDischarge, out var existing);
					lookup[destination.JB_RL_NKPortOfDischarge] = existing | PortDirections.Discharge;
				}
			}

			foreach (var pair in lookup)
			{
				Elements.Add(new PortMessageTargetPort(pair.Key, pair.Value));
			}
		}

		protected virtual bool ShouldIncludePort(ZString port)
		{
			return true;
		}

		protected JobVoyage Voyage
		{
			get;
			private set;
		}
	}
}
