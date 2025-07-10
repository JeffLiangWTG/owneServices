using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class OrganisationDetails : IOrganisationDetails, ISimplifiedEntryOrganisationDetails
	{
		public static OrganisationDetails New(ZPropertyInfo guidInfo)
		{
			var parent = guidInfo.BizObj;
			var objectPk = (ZGuid)guidInfo.Value;
			var organisationDetails = (BusinessObject)parent.Factory.Load<OrgHeader>(objectPk) ?? parent.Factory.Load<OrgAddress>(objectPk);
			return organisationDetails == null ? null : new OrganisationDetails(organisationDetails, parent, guidInfo.HumanReadableName);
		}

		public static OrganisationDetails New(ZPropertyInfo guidInfo, OrgMatchedCustomsRegNoType regoNoType, bool loadFromOrgHeader = false)
		{
			var parent = guidInfo.BizObj;
			var objectPk = (ZGuid)guidInfo.Value;
			BusinessObject organisationDetails = null;

			if (loadFromOrgHeader)
			{
				organisationDetails = parent.Factory.Load<OrgHeader>(objectPk);
			}
			else
			{
				organisationDetails = parent.Factory.Load<OrgAddress>(objectPk);
			}

			return organisationDetails == null ? null : new OrganisationDetails(organisationDetails, parent, guidInfo.HumanReadableName, regoNoType);
		}

		protected OrganisationDetails(BusinessObject details, BusinessObject parent, ZString organisationType)
		{
			this.addressDetails = details as IAddressDetails;
			this.parent = parent;
			this.organisationType = organisationType;

			var detailsAsOrgHeader = details as OrgHeader;
			var detailsAsOrgAddress = details as OrgAddress;

			this.orgHeader = detailsAsOrgHeader ?? (detailsAsOrgAddress != null ? detailsAsOrgAddress.Header : null);
		}

		protected OrganisationDetails(BusinessObject details, BusinessObject parent, ZString organisationType, OrgMatchedCustomsRegNoType regoNoType)
			: this(details, parent, organisationType)
		{
			matchedCustomsRegNo = details is OrgHeader ? OrgHeaderWrapper.GetCustomsRelatedCode(details as OrgHeader, regoNoType) : OrgHeaderWrapper.GetAddressCustomsRelatedCode(details as OrgAddress, regoNoType);
		}

		readonly IAddressDetails addressDetails;
		readonly BusinessObject parent;
		readonly ZString organisationType;
		readonly ZString matchedCustomsRegNo;
		readonly OrgHeader orgHeader;

		#region IOrganisationDetails Members

		public ZString MatchedCustomsRegoNumber
		{
			get { return matchedCustomsRegNo; }
		}

		public ZString UserFriendlyPath
		{
			get
			{
				var result = ZString.Empty;
				if (addressDetails is OrgHeader)
				{
					result = parent.HumanReadableName + " > " + organisationType;
				}
				else if (addressDetails is OrgAddress)
				{
					var address = addressDetails as OrgAddress;
					var stringBuilder = new ZStringBuilder();
					stringBuilder.Append(address.EffectiveCompanyNameTruncated + " > ");
					stringBuilder.Append(address.OA_Code);
					result = stringBuilder.ToString();
				}
				return result;
			}
		}

		#endregion

		#region IAddressDetails Members

		public ZString AddressLine1
		{
			get { return addressDetails.AddressLine1; }
		}

		public ZString AddressLine2
		{
			get { return addressDetails.AddressLine2; }
		}

		public ZString City
		{
			get { return addressDetails.City; }
		}

		public ZString CompanyName
		{
			get { return addressDetails.CompanyName; }
		}

		public ZString ContactName
		{
			get { return addressDetails.ContactName; }
		}

		public ZString Country
		{
			get { return addressDetails.Country; }
		}

		public ZString Email
		{
			get { return addressDetails.Email; }
		}

		public ZString Fax
		{
			get { return addressDetails.Fax; }
		}

		public ZString Phone
		{
			get { return addressDetails.Phone; }
		}

		public ZString PostCode
		{
			get { return addressDetails.PostCode; }
		}

		public ZString State
		{
			get { return addressDetails.State; }
		}

		#endregion

		#region ISimplifiedEntryOrganisationDetails Members

		ZString ISimplifiedEntryOrganisationDetails.EntityCode
		{
			get;
			set;
		}

		ZString ISimplifiedEntryOrganisationDetails.EntityIdentifierQualifier
		{
			get { return ACECargoReleaseData.GetEntityCustomsNumberType(((ISimplifiedEntryOrganisationDetails)this).EntityIdentifier); }
		}

		ZString ISimplifiedEntryOrganisationDetails.EntityIdentifier
		{
			get
			{
				return orgHeader == null ? ZString.Empty :
					orgHeader.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates,
							OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
							OrgCusCode.USACodeTypes.CBPAssignedNumber,
							OrgCusCode.USACodeTypes.SocialSecurityNumber);
			}
		}

		IEnumerable<(ZString IdentifierType, ZString Identifier)> ISimplifiedEntryOrganisationDetails.GlobalBusinessIdentifiers
		{
			get
			{
				(ZString, ZString) GetCustomsRegNoFromAddress(OrgAddress orgAddress, ZString codeType)
				{
					var customsRegNo = orgAddress.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.UnitedStates);
					if (customsRegNo.IsEmpty && codeType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem)
					{
						var orgCusCodes = orgAddress?.Header?.CustomsCodes.GetOrgCusCodesForCodeAndCountry(codeType, Core.Constants.CountryCodes.UnitedStates);
						customsRegNo = orgCusCodes?.FirstOrDefault(x => x.OK_OA_PremisesAddress.IsEmpty)?.OK_CustomsRegNo ?? ZString.Empty;
					}

					if (codeType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem)
					{
						codeType = DataUniversalNumberingSystemCodeInGlobalBusinessIdentifier;
					}

					return (codeType, customsRegNo);
				}

				var entityCode = ((ISimplifiedEntryOrganisationDetails)this).EntityCode;
				var entitiesAllowedGBIPilot = new List<ZString>()
				{
					EntityCodeList.Codes.ManufacturerSupplier,
					EntityCodeList.Codes.SellingParty,
					EntityCodeList.Codes.Shipper,
					EntityCodeList.Codes.Exporter,
					EntityCodeList.Codes.Distributor,
					EntityCodeList.Codes.Packager
				};

				if (addressDetails is OrgAddress address && entitiesAllowedGBIPilot.Contains(entityCode))
				{
					yield return GetCustomsRegNoFromAddress(address, OrgCusCode.USACodeTypes.LegalEntityIdentifier);
					yield return GetCustomsRegNoFromAddress(address, OrgCusCode.USACodeTypes.GlobalLocationNumber);
					yield return GetCustomsRegNoFromAddress(address, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
				}
			}
		}

		public const string DataUniversalNumberingSystemCodeInGlobalBusinessIdentifier = "DUNS";

		#endregion
	}
}
