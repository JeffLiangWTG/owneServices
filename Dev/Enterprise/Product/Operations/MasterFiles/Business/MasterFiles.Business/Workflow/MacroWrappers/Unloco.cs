using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class Unloco : IUnloco
	{
		public Unloco(RefUNLOCO unloco)
		{
			this.unloco = unloco;
		}

		readonly RefUNLOCO unloco;

		public ZString Code
		{
			get
			{
				if (!code.HasValue)
				{
					code = unloco?.RL_Code ?? ZString.Empty;
				}
				return code.Value;
			}
			set => code = value;
		}
		ZString? code;

		public ZString Name
		{
			get
			{
				if (!name.HasValue)
				{
					name = unloco?.RL_PortName ?? ZString.Empty;
				}
				return name.Value;
			}
			set => name = value;
		}

		ZString? name;

		public ZString IATACode
		{
			get
			{
				if (!iataCode.HasValue)
				{
					iataCode = unloco?.RL_IATA ?? ZString.Empty;
				}
				return iataCode.Value;
			}
			set => iataCode = value;
		}
		ZString? iataCode;

		public ICountry Country => country ?? (country = new Country(unloco?.Country));
		Country country;

		[MacroIgnore]
		public IRefUNLOCOCollection Unlocos => unlocos ?? (unlocos = new RefUNLOCOCollection(unloco?.Factory));
		RefUNLOCOCollection unlocos;

		public override string ToString() => Code;
	}
}
