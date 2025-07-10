//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobDocAddressLookups
//
//    This class should be used for overriding collections in AutoJobDocAddressLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressLookups : AutoJobDocAddressLookups
	{
		public JobDocAddressLookups(AutoJobDocAddress parent)
			: base(parent)
		{
		}

		public new JobDocAddress Parent
		{
			get { return (JobDocAddress)base.Parent; }
		}

		public const string GovRegNumTypeDefaultValue = "DEF";

		#region State List

		public CodeDescriptionPairList State_List
		{
			get
			{
				return Factory.GetStateList(Parent.E2_RN_NKCountryCode, Parent.E2_RN_NKCountryCodeInfo.HasErrors());
			}
		}

		#endregion

		#region OrgHeader_List

		public OrgHeaderCollection OrgHeader_List
		{
			get
			{
				if (orgHeader_List == null || Parent.DocAddressType != orgHeader_List_CurrentDocAddressType)
				{
					if (Parent.Parent != null)
					{
						orgHeader_List = Parent.Parent.GetOrgHeaderList(Parent.DocAddressType);
					}

					orgHeader_List_CurrentDocAddressType = Parent.DocAddressType;

					if (orgHeader_List == null)
					{
						orgHeader_List = new OrgHeaderCollection(Factory);
					}
				}
				return orgHeader_List;
			}
		}

		OrgHeaderCollection orgHeader_List;

		DocAddressType orgHeader_List_CurrentDocAddressType;

		#endregion

		public CodeDescriptionPairList SelectableDocAddressType_List
		{
			get
			{
				CodeDescriptionPairList result;
				if (Parent != null && Parent.DocAddressManager.ManagedCollection != null)
				{
					result = Parent.DocAddressManager.GetApplicableCodeList();
					foreach (JobDocAddressRequirement requirement in Parent.DocAddressManager.Requirements)
					{
						if (Parent.E2_AddressType == DocAddressTypes.GetCode(Factory, requirement.DefaultDocAddressType))
						{
							result = RemoveSupportedCodesFromListWhenSelectingFromDefaultCode(result);
						}
					}
				}
				else
				{
					result = Factory.GetCachedValue<DocAddressTypes>();
				}
				return result;
			}
		}

		public CodeDescriptionPairList ResidentialCommercialAddressType_List
		{
			get { return new ResidentialCommercialAddressTypeList(); }
		}

		CodeDescriptionPairList RemoveSupportedCodesFromListWhenSelectingFromDefaultCode(CodeDescriptionPairList listSoFar)
		{
			CodeDescriptionPairList result = listSoFar;
			foreach (JobDocAddressRequirement requirement in Parent.DocAddressManager.Requirements)
			{
				foreach (DocAddressType docAddressType in requirement.SupportedDocAddressTypes)
				{
					ZString code = DocAddressTypes.GetCode(Factory, docAddressType);
					if (result.ContainsCode(code))
					{
						result.RemoveCode(code);
					}
				}
			}
			return result;
		}

		public virtual CodeDescriptionPairList GovRegNumTypes
		{
			get
			{
				if (Parent.Requirement != null && Parent.Requirement.LookupsGovRegNumTypes != null)
				{
					return Parent.Requirement.LookupsGovRegNumTypes(this);
				}
				else
				{
					CodeDescriptionPairList result = new CodeDescriptionPairList();
					result.AddPair(GovRegNumTypeDefaultValue, Res.GetString("bb7a8ace-e982-4562-a2be-f44c161a42f4", "Default"));

					if (Parent.Country != null)
					{
						OrgRegistrationNumberTypeList list = new OrgRegistrationNumberTypeList(Parent.Country);
						foreach (CodeDescriptionPair pair in list)
						{
							ZString code = list.GetActualCode(pair.Code);
							if (!result.ContainsCode(code))
							{
								result.AddPair(code, pair.Description);
							}
						}
					}

					return result;
				}
			}
		}

		public IBusinessObjectCollection SelectedOrganisationAddresses
		{
			get
			{
				OrgHeader org = Parent.Organisation;
				if (org != null)
				{
					return org.Addresses;
				}
				else
				{
					return Addresses;
				}
			}
		}

		#region ScreeningStatusesList

		public CodeDescriptionPairList ScreeningStatusesList => Factory.GetCachedValue<ScreeningStatusesList>();

		#endregion

		#region Address List

		public ZAddressList Address_List
		{
			get
			{
				ZAddressList result;

				if (Parent.E2_AddressOverride)
				{
					// A grid column bound to E2_OA_Address and overriden would display "No Address Specified" without the below code.
					result = new ZAddressList();
					result.AddAddress(Parent.E2_OA_Address, Parent.E2_Address1, Parent.AddressAsASingleLine);
				}
				else
				{
					OrgHeader organisation = Parent.Organisation;
					result = organisation != null ? organisation.Address_List : new ZAddressList();
				}

				return result;
			}
		}

		#endregion
	}
}
