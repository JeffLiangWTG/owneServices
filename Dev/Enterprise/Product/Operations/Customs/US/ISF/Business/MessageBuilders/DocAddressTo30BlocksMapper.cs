using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	class DocAddressTo30BlocksMapper : I30Blocks
	{
		public static DocAddressTo30BlocksMapper New(IISFDocAddress docAddress)
		{
			return docAddress == null ? null : new DocAddressTo30BlocksMapper(docAddress);
		}

		DocAddressTo30BlocksMapper(IISFDocAddress docAddress)
		{
			this.docAddress = docAddress;
		}
		readonly IISFDocAddress docAddress;

		ZString entityCode
		{
			get { return CommercialEntityTypeList.GetCodeFromDocAddressType(docAddress.E2_AddressType); }
		}

		#region I30Blocks Members

		ZString I30Blocks.EntityCode
		{
			get { return entityCode; }
		}

		ZString I30Blocks.EntityName
		{
			get { return docAddress.E2_CompanyNameTruncated; }
		}

		ZString I30Blocks.EntityIdentifierQualifier
		{
			get
			{
				return entityCode == CommercialEntityTypeList.Codes.Consignee ||
					(entityCode == CommercialEntityTypeList.Codes.ShipToParty && (docAddress.E2_GovRegNumType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem || docAddress.E2_GovRegNumType == OrgCusCode.USACodeTypes.FIRMSCode)) ||
					docAddress.E2_GovRegNumType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem ?
					EntityIdentifierQualifierList.GetCodeFromCusCodeType(docAddress.E2_GovRegNumType) : ZString.Empty;
			}
		}

		ZString I30Blocks.EntityIdentifier
		{
			get
			{
				switch (docAddress.E2_GovRegNumType)
				{
					case OrgCusCode.CodeTypes.PassportID:
						return docAddress.E2_PassportID;
					case OrgCusCode.USACodeTypes.SocialSecurityNumber:
						return docAddress.E2_SocialSecurityNumber;
					default:
						return docAddress.E2_GovRegNum;
				}
			}
		}

		ZString I30Blocks.PassportCountryOfIssue
		{
			get { return docAddress.E2_PassportCountryOfIssue; }
		}

		ZDateTime I30Blocks.PassportDateOfBirth
		{
			get { return docAddress.E2_PassportDateOfBirth; }
		}

		ZString I30Blocks.LegalName
		{
			get { return docAddress.E2_Contact; }
		}

		ZDateTime I30Blocks.SocialSecurityNumberDateOfBirth
		{
			get { return docAddress.E2_SocialSecurityNumberDateOfBirth; }
		}

		ZString I30Blocks.SecondaryEntityCode
		{
			get { return ZString.Empty; }
		}

		ZString I30Blocks.SecondaryEntityName
		{
			get { return ZString.Empty; }
		}

		IEnumerable<IAddressingInformation> I30Blocks.AddressingInformation
		{
			get
			{
				AddressingInformation result = null;
				foreach (ZString addressBlock in AddressInformation.Split(35))
				{
					if (result == null)
					{
						result = new AddressingInformation();
					}
					if (result.AddressInformation1.IsEmpty)
					{
						result.AddressInformation1 = addressBlock;
					}
					else
					{
						result.AddressInformation2 = addressBlock;
						yield return result;
						result = null;
					}
				}
				if (result != null)
				{
					yield return result;
				}
			}
		}

		ZString I30Blocks.City
		{
			get { return docAddress.E2_City.Left(35); } // check ISFSF36.City.Length
		}

		//TODO: Possibly needs to be remapped to ISO SubDivision Code (http://www.unece.org/cefact/locode/service/sublocat.htm)
		ZString I30Blocks.CountrySubEntityCode
		{
			get { return docAddress.E2_State.Left(3); } // check ISFSF36.CountrySubEntityCode.Length
		}

		ZString I30Blocks.PostalCode
		{
			get { return docAddress.E2_Postcode.Left(15); } // check ISFSF36.PostalCode.Length
		}

		ZString I30Blocks.Country
		{
			get { return docAddress.CountryCode; }
		}

		#endregion

		ZString AddressInformation
		{
			get
			{
				if (!addressInformation.HasValue)
				{
					addressInformation = docAddress.E2_Address1.Trim() + docAddress.E2_Address2.Trim();
					addressInformation = addressInformation.Value.Trim();
				}
				return addressInformation.Value;
			}
		}
		ZString? addressInformation;

		class AddressingInformation : IAddressingInformation
		{
			#region IAddressingInformation Members
			ZString IAddressingInformation.AddressComponentQualifier1
			{
				get { return AddressComponetQualifierList.Codes.UnstructuredStreetAddress; }
			}

			public ZString AddressInformation1;
			ZString IAddressingInformation.AddressInformation1
			{
				get { return AddressInformation1; }
			}

			ZString IAddressingInformation.AddressComponentQualifier2
			{
				get { return AddressComponetQualifierList.Codes.UnstructuredStreetAddress; }
			}

			public ZString AddressInformation2;
			ZString IAddressingInformation.AddressInformation2
			{
				get { return AddressInformation2; }
			}

			#endregion
		}
	}
}
