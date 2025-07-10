using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefUNLOCOLookups : AutoRefUNLOCOLookups
	{
		public RefUNLOCOLookups(AutoRefUNLOCO parent) : base(parent)
		{
			Argument.NotNull(parent, "parent");
			Parent = parent;
		}

		public new AutoRefUNLOCO Parent
		{
			get;
			private set;
		}

		#region UNLOCOs

		public RefUNLOCOCollection UNLOCOs
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public override RefCountryStatesCollection CountryStates
		{
			get { return new RefCountryStatesCollection(Factory, new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, Parent.Country != null ? Parent.Country.RN_Code : CargoWise.Types.ZString.Empty)); }
		}

		#endregion
	}
}
