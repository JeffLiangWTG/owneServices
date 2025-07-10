using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class TWJobRequiredDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider
	{
		public TWJobRequiredDocumentWrapper(OrgHeader orgHeader, JobRequiredDocument requiredDocument)
		{
			Org = Argument.NotNull(orgHeader, nameof(orgHeader));
			DefaultOrg = OrgHeader.DefaultOrg;
			RequiredDocument = requiredDocument;
			orgAddressData = new AddressData(Org, Core.SharedConstants.Languages.ChineseTraditional);
			defaultOrgAddressData = new AddressData(DefaultOrg, Core.SharedConstants.Languages.ChineseTraditional);
		}

		readonly AddressData orgAddressData;
		readonly AddressData defaultOrgAddressData;

		public OrgHeader DefaultOrg { get; }

		public OrgHeader Org { get; }

		JobRequiredDocument RequiredDocument { get; }

		#region IBODocDataProvider
		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;

		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => Org;

		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;

		IZType IBODocDataProvider.GetCustomField(string fieldName, string fieldType) => BasicBODocDataProvider.GetCustomField(fieldName, fieldType);

		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string fieldType) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, fieldType);

		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);

		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);

		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);

		string IBODocDataProvider.ToString() => Org.HumanReadableName;

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));
		IBODocDataProvider basicBODocDataProvider;
		#endregion

		#region Fields
		public ZString BoxNumber => RequiredDocument?.BoxNumberDocAttrib?.D0_AttribDisplayValue ?? ZString.Empty;

		public ZString CustomsDistrictDescription
		{
			get
			{
				var result = ZString.Empty;
				var customsDistrictDocAttrib = RequiredDocument?.CustomsDistrictDocAttrib;
				if (customsDistrictDocAttrib != null)
				{
					result = customsDistrictDocAttrib.Lookups.AttributeValueList.GetDescriptionFromCode(customsDistrictDocAttrib.D0_AttribDisplayValue);
					if (result.EndsWith((NoResString)"關", StringComparison.CurrentCulture) || result.EndsWith((NoResString)"关", StringComparison.CurrentCulture))
					{
						result = result.Length > 1 ? result.Left(result.Length - 1) : ZString.Empty;
					}
				}
				if (result.IsEmpty)
				{
					result = "　　";
				}
				return result;
			}
		}

		public ZString OrgCompanyName => orgAddressData.CompanyName;

		public ZString DefaultOrgCompanyName => defaultOrgAddressData.CompanyName;

		public ZString OrgAddressFormat => orgAddressData.ChineseTraditionalAddressFormat;

		public ZString DefaultOrgAddressFormat => defaultOrgAddressData.ChineseTraditionalAddressFormat;

		public ZString ControlledPremisesID => Org.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID);

		public ZString UniformNumber => OrgHeaderHelper.GetVatOrPasOrPid(Org);

		public ZString OrgPhonePrefix => GetPhonePrefix(Org.MainAddress.OA_Phone.SubstringSafe(4, 1));

		public ZString OrgPhone => Org.MainAddress.OA_Phone.SubstringSafe(5);

		public ZString DefaultOrgPhonePrefix => GetPhonePrefix(DefaultOrg.MainAddress.OA_Phone.SubstringSafe(4, 1));

		public ZString DefaultOrgPhone => DefaultOrg.MainAddress.OA_Phone.SubstringSafe(5);

		ZString GetPhonePrefix(ZString value) => value.IsEmpty ? ZString.Empty : ZString.Format("0{0}", value);

		#endregion
	}
}
