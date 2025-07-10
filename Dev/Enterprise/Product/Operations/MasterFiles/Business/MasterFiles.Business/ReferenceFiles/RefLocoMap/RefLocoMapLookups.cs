using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefLocoMapLookups : AutoRefLocoMapLookups
	{
		public RefLocoMapLookups(AutoRefLocoMap parent) : base(parent)
		{
		}

		new RefLocoMap Parent
		{
			get { return (RefLocoMap)base.Parent; }
		}

		#region Local Codes

		public virtual CodeDescriptionPairList RY_SystemUsage_List
		{
			get
			{
				CodeDescriptionPairList result = null;

				if (Parent.Country != null)
				{
					switch (Parent.Country.Code)
					{
						case Core.Constants.CountryCodes.Australia:
							result = new AirSeaMailSystemUsageList();
							break;

						case Core.Constants.CountryCodes.UnitedKingdom:
							result = new GBLocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.UnitedStates:
							result = new USLocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.Canada:
							result = new CALocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.Iceland:
							result = new ISLocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.China:
							result = new CNLocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.Singapore:
							result = new SGLocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.Turkey:
							result = new TRLocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.Mexico:
							result = new MXLocoMapSystemUsageList();
							break;

						case Core.Constants.CountryCodes.Argentina:
							result = new ARLocoMapSystemUsageList();
							break;

						default:
							result = new CodeDescriptionPairList();
							break;
					}

					if (Parent.Country.Code.Equals(Parent.LocoPort?.Country?.Code) && Core.Constants.CountryCodes.IsFranceOrTerritory(Parent.Country.Code))
					{
						result = new FRLocoMapSystemUsageList();
					}
				}
				else
				{
					result = new CodeDescriptionPairList();
				}

				return result;
			}
		}

		public virtual CodeDescriptionPairList RY_LocalPortCode_List
		{
			get
			{
				return new FrenchPortSystemCodeList();
			}
		}

		#endregion

		#region Countries

		public RefCountryCollection CountryCollection
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion
	}
}
