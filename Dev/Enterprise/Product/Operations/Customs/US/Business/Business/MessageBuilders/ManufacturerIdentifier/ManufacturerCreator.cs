using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public static class OrganisationCreator
	{
		public static OrgAddress CreateManufacturerAndSendNameAddressQueryMessage(BusinessObjectFactory factory, ZString mID)
		{
			OrgAddress result = null;

			if (mID.IsLettersAndNumbersOnlyOrEmpty)
			{
				var organization = CreateOrganisation(factory, AutocreatefromMID.FullName, AutocreatefromMID.Address1, "ADDRESS DETAILS. ON SUCCESSFUL RESPONSE ADDRESS", "DETAILS WILL BE FILLED IN", ZString.Empty, ZString.Empty, mID.Left(2), true, false);
				organization.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, mID, factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates));
				ManufacturerIdentifierQueryBuilder.GenerateMessageAndAttachToOrganisation(organization, mID);
				result = organization.MainAddress;
			}

			return result;
		}

		public static OrgHeader CreateUltimateConsignee(BusinessObjectFactory factory, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString unloco, ZString einNumber)
		{
			var result = CreateOrganisation(factory, name, address1, address2, city, state, postCode, unloco, false, true);
			var codeType = BIRDOrganisationMatching.GetCodeTypeForNumber(OrgMatchedCustomsRegNoType.EIN, einNumber);
			result.CustomsCodes.AddNew(codeType, einNumber, factory.Load<RefCountry>(Core.CountryGuids.Instance.UnitedStates));
			return result;
		}

		public static OrgHeader CreateOrganisation(BusinessObjectFactory factory, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString unloco, ZBool isConsignor, ZBool isConsignee)
		{
			var result = factory.New<OrgHeader>();
			result.OH_RL_NKClosestPort = unloco;
			result.OH_IsConsignor = isConsignor;
			result.OH_IsConsignee = isConsignee;
			result.OH_Language = Constants.Languages.English;
			result.OH_FullName = name.Left(OrgHeader.Schema.OH_FullNameTruncatedLength);

			var mainAddress = result.MainAddress;
			mainAddress.OA_Address1 = address1.Left(mainAddress.OA_Address1Info.MaxLength);
			mainAddress.OA_Address2 = address2.Left(mainAddress.OA_Address2Info.MaxLength);
			mainAddress.OA_City = city.Left(mainAddress.OA_CityInfo.MaxLength);
			mainAddress.OA_State = state.Left(mainAddress.OA_StateInfo.MaxLength);
			mainAddress.OA_PostCode = postCode.Left(mainAddress.OA_PostCodeInfo.MaxLength);

			return result;
		}

		public const string ManufacturerCreated = "There is no organization matching this MID, {0}. System has created an organization named {0} and a manufacturer query to Customs has been queued. On response, system will populate its name and address.";

		public const string NoManufacturerCreatedAsMIDInvalid = "There is no organizaiton created because there are invalid characters in MID {0}, only alphanumeric characters are allowed.";

		public const string OrganisationCreated = "There is no organization matching this number, {0}. System has created an organization named {1}.";
	}
}
