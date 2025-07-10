using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class SalesTeamLookups : GlbGroupLookups
	{
		public SalesTeamLookups(SalesTeam parent)
			: base(parent)
		{
		}

		#region Complete Sales Rep List

		public GlbStaffCollection CompleteSalesRepList
		{
			get
			{
				if (completeSalesRepList == null)
				{
					completeSalesRepList = new GlbStaffCollection(Factory);
				}
				return completeSalesRepList;
			}
		}

		GlbStaffCollection completeSalesRepList;

		#endregion

		#region Countries

		public RefCountryCollection Countries
		{
			get
			{
				if (countries == null)
				{
					countries = new RefCountryCollection(Factory);
				}
				return countries;
			}
		}

		RefCountryCollection countries;

		#endregion

		#region Unlocos

		public RefUNLOCOCollection Unlocos
		{
			get
			{
				if (unlocos == null)
				{
					unlocos = new RefUNLOCOCollection(Factory);
				}
				return unlocos;
			}
		}

		RefUNLOCOCollection unlocos;

		#endregion

		#region Types

		protected override ICodeDescriptionPairList GetTypes()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(GlbGroupTypeList.Codes.Staff, GlbGroupTypeList.Descriptions.Staff);
			return list;
		}

		#endregion

		#region ParentTeams

		public virtual SalesTeamCollection ParentTeams
		{
			get { return new SalesTeamCollection(Factory); }
		}

		#endregion

	}
}
