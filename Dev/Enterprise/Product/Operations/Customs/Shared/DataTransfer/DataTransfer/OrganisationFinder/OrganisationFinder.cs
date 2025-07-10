using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer
{
	public class OrganisationFinder
	{
		public OrganisationFinder(UnknownOrganisationCodeEventArgs organisationInfo, BusinessObjectFactory factory)
		{
			Factory = factory;
			OrganisationInfo = organisationInfo;
		}

		public void FindSimilarOrganisations()
		{
			UnknownOrg.SimilarOrgFinder.FindSimilarOrganisations(new ZQuery(OrgPatternMatchSchema.OS_UNLOCO, SQLComparisonOperator.StartsWith, UnknownOrg.OH_RL_NKClosestPort), false);
		}

		#region Properties

		public OrgHeaderForFinding UnknownOrg
		{
			get
			{
				if (fUnknownOrg == null)
				{
					fUnknownOrg = Factory.New<OrgHeaderForFinding>();
					fUnknownOrg.OH_FullName = OrganisationInfo.Name.Left(OrgHeader.Schema.OH_FullNameTruncatedLength);
					fUnknownOrg.PrimaryRegistrationNumber.Number = OrganisationInfo.RegistrationNo.Left(fUnknownOrg.PrimaryRegistrationNumber.NumberInfo.MaxLength);
					fUnknownOrg.MainAddress.OA_Address1 = OrganisationInfo.Street.Left(fUnknownOrg.MainAddress.OA_Address1Info.MaxLength);
					fUnknownOrg.MainAddress.OA_Address2 = OrganisationInfo.Street2.Left(fUnknownOrg.MainAddress.OA_Address2Info.MaxLength);
					fUnknownOrg.MainAddress.OA_City = OrganisationInfo.City.Left(fUnknownOrg.MainAddress.OA_CityInfo.MaxLength);
					fUnknownOrg.MainAddress.OA_PostCode = OrganisationInfo.PostCode.Left(fUnknownOrg.MainAddress.OA_PostCodeInfo.MaxLength);
					fUnknownOrg.OH_Code = OrganisationInfo.Code.Left(fUnknownOrg.OH_CodeInfo.MaxLength);
					if (OrganisationInfo.Country.IsEmpty)
					{
						fUnknownOrg.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Australia;
					}
					else
					{
						fUnknownOrg.OH_RL_NKClosestPort = OrganisationInfo.Country.Left(fUnknownOrg.OH_RL_NKClosestPortInfo.MaxLength);
					}
					fUnknownOrg.HasChanges = false;
					fUnknownOrg.Addresses.HasChanges = false;
				}
				return fUnknownOrg;
			}
		}

		#endregion

		#region Implementation

		protected OrgHeaderForFinding fUnknownOrg;
		protected readonly BusinessObjectFactory Factory;
		protected readonly UnknownOrganisationCodeEventArgs OrganisationInfo;

		#endregion
	}
}
