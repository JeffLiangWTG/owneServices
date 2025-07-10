using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageLookups : JobCartageLookups
	{
		public CommonCartageLookups(CommonCartage parent)
			: base(parent)
		{
			if (parent is CommonCartage)
			{
				Cartage = parent;
			}
		}

		public OrgHeaderCollection FirstAddressList
		{
			get { return GetOrgHeaderList(Cartage.FirstDocAddress); }
		}

		public OrgHeaderCollection SecondAddressList
		{
			get { return GetOrgHeaderList(Cartage.SecondDocAddress); }
		}

		public OrgHeaderCollection ThirdAddressList
		{
			get { return GetOrgHeaderList(Cartage.ThirdDocAddress); }
		}

		public OrgHeaderCollection FourthAddressList
		{
			get { return GetOrgHeaderList(Cartage.FourthDocAddress); }
		}

		OrgHeaderCollection GetOrgHeaderList(JobDocAddress docAddress)
		{
			OrgHeaderCollection result = null;

			var orgTypeCode = docAddress != null ? CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(docAddress.DocAddressType) : ZString.Empty;
			switch (orgTypeCode)
			{
				case LocalCartageJobOrgTypeList.Codes.CFS:
					result = DepotList;
					break;
				case LocalCartageJobOrgTypeList.Codes.CTO:
					result = CTOList;
					break;
				case LocalCartageJobOrgTypeList.Codes.CYD:
					result = ContainerYardList;
					break;
				case LocalCartageJobOrgTypeList.Codes.CNE:
					result = ConsigneeList;
					break;
				case LocalCartageJobOrgTypeList.Codes.CNR:
					result = ConsignorList;
					break;
				default:
					result = OrgHeaderList;
					break;
			}

			// now append the UNLOCO and branch default to all cartage org lookups
			var homePort = Cartage.Branch != null ? Cartage.Branch.GB_RL_NKHomePort : GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", homePort));

			return result;
		}

		public CodeDescriptionPairList DropModes
		{
			get
			{
				if (Cartage.IsContainerised && Cartage.IsLoose)
				{
					return BindToLists.DropModes();
				}
				else
				{
					return BindToLists.DropModes(Cartage.IsContainerised);
				}
			}
		}

		CartageBindToLists BindToLists
		{
			get { return Factory.GetCachedValue("Enterprise.Freight.LocalCartage.Business.CommonCartageLookups.CartageBindToLists", () => new CartageBindToLists(Factory)); }
		}

		readonly CommonCartage Cartage;
	}
}
