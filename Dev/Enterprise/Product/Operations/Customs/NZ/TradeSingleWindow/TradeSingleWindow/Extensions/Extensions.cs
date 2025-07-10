using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow
{
	public static class Extensions
	{
		public static ZString GetSenderReferenceNumber(this ZString reference)
		{
			var messageReference = reference.KeepAlphanumericCharacters();
			return messageReference.IsEmpty ? (ZString)TSWConstants.SendersReferencePlaceHolder : messageReference.Left(14);
		}

		public static ZString GetSupplierCode(this OrgHeader organisation)
		{
			return organisation.GetFormattedCusCode(OrgCusCode.CodeTypes.SupplierCode, 9);
		}

		public static ZString GetCustomsClientCode(this OrgHeader organisation)
		{
			return organisation.GetFormattedCusCode(OrgCusCode.CodeTypes.CustomsClientCode, 9);
		}

		public static ZString GetCCPOrATFCode(this JobDocAddress docAddress)
		{
			return GetCCPOrATFCode(docAddress?.Address, docAddress?.Organisation);
		}

		public static ZString GetCCPOrATFCode(OrgAddress orgAddress, OrgHeader orgHeader)
		{
			var code = orgAddress?.GetCCPOrATFCode() ?? ZString.Empty;
			if (code.IsEmpty)
			{
				return orgHeader?.GetCCPOrATFCode() ?? ZString.Empty;
			}
			return code;
		}

		static ZString GetCCPOrATFCode(this OrgAddress address)
		{
			var code = address.GetFormattedCusCode(OrgCusCode.CodeTypes.ControlledPremisesID);
			if (code.IsEmpty)
			{
				code = address.GetFormattedCusCode(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility);
			}
			return code;
		}

		static ZString GetCCPOrATFCode(this OrgHeader organisation)
		{
			var code = organisation.GetFormattedCusCode(OrgCusCode.CodeTypes.ControlledPremisesID);
			if (code.IsEmpty)
			{
				code = organisation.GetFormattedCusCode(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility);
			}
			return code;
		}

		static ZString GetFormattedCusCode(this OrgAddress address, ZString cusCode, int length = 0)
		{
			var result = address?.CustomsCodes.GetCustomsRegNo(cusCode, Core.Constants.CountryCodes.NewZealand) ?? ZString.Empty;
			if (!result.IsEmpty && length > 0)
			{
				result = result.Left(length).PadLeft(length, '0');
			}
			return result;
		}

		static ZString GetFormattedCusCode(this OrgHeader organisation, ZString cusCode, int length = 0)
		{
			var result = organisation?.CustomsCodes.GetCustomsRegNo(cusCode, Core.Constants.CountryCodes.NewZealand, ZGuid.Empty) ?? ZString.Empty;
			if (!result.IsEmpty && length > 0)
			{
				result = result.Left(length).PadLeft(length, '0');
			}
			return result;
		}

		public static IContact GetAllocatedNZCustomsContact(this OrgHeader organisation)
		{
			return OrgContactWrapper.New(organisation?.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.NZCustoms));
		}

		public static IEnumerable<ICommunication> GetCommunications(this OrgHeader organisation)
		{
			var communications = organisation?.GetAllocatedNZCustomsContact()?.Communications;
			if (communications?.Any() ?? false)
			{
				foreach (var contactComms in communications)
				{
					if (!contactComms.ContactDetail.IsEmpty)
					{
						yield return contactComms;
					}
				}
			}
			else if (organisation?.MainAddress != null)
			{
				var mainAddress = organisation.MainAddress;
				if (!mainAddress.OA_Email.IsEmpty)
				{
					yield return new Communication(mainAddress.OA_Email, CommunicationTypeList.Codes.EM);
				}

				if (!mainAddress.OA_Phone.IsEmpty)
				{
					var phoneNo = new Communication(mainAddress.OA_Phone, CommunicationTypeList.Codes.TE);
					if (!phoneNo.ContactDetail.IsEmpty)
					{
						yield return phoneNo;
					}
				}

				if (!mainAddress.OA_Mobile.IsEmpty)
				{
					var mobileNo = new Communication(mainAddress.OA_Mobile, CommunicationTypeList.Codes.AL);
					if (!mobileNo.ContactDetail.IsEmpty)
					{
						yield return mobileNo;
					}
				}

				if (!mainAddress.OA_Fax.IsEmpty)
				{
					var faxNo = new Communication(mainAddress.OA_Fax, CommunicationTypeList.Codes.FX);
					if (!faxNo.ContactDetail.IsEmpty)
					{
						yield return faxNo;
					}
				}
			}
		}

		public static string GetXmlEnumAttributeFromValue<T>(this T itemValue) where T : struct, Enum
		{
			var member = typeof(T).GetMember(itemValue.ToString()).FirstOrDefault();
			return member?.GetCustomAttributes(false).OfType<XmlEnumAttribute>().FirstOrDefault()?.Name;
		}

		public static T GetXmlEnumValueFromAttribute<T>(ZString enumAttributeName) where T : struct, IConvertible
		{
			foreach (var field in typeof(T).GetFields())
			{
				if (Attribute.GetCustomAttribute(field, typeof(XmlEnumAttribute)) is XmlEnumAttribute xmlEnumAttribute && xmlEnumAttribute.Name == enumAttributeName)
				{
					return (T)field.GetValue(null);
				}
			}

			return default;
		}
	}
}
