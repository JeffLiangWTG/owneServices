using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class InternalCartageBehaviorStrategy : CommonCartageBehaviorStrategy
	{
		public InternalCartageBehaviorStrategy()
		{
		}

		public override CodeDescriptionPairList GetJobTypeList(CommonCartage cartage)
		{
			var cartageJobTypes = new CodeDescriptionPairList();
			var cartageTypes = new CommonCartageTypeCollection(cartage.Factory, GetJobTypeListQuery(cartage));
			foreach (CommonCartageType type in cartageTypes)
			{
				cartageJobTypes.AddPair(type.E3_JobType, type.E3_DescriptionMultilingual);
			}
			cartageJobTypes.SortByDescription();

			return cartageJobTypes;
		}

		protected override void SetupAddress(CommonCartage cartage, JobDocAddress docAddress, ZString orgType)
		{
			if (cartage.CartageInternalType != null)
			{
				JobDocAddress parentAddress = cartage.CartageInternalType.GetCartageAddress(orgType);
				if (parentAddress != null)
				{
					docAddress.E2_AddressOverride = parentAddress.E2_AddressOverride;
					if (!docAddress.E2_AddressOverride)
					{
						docAddress.E2_OA_Address = parentAddress.E2_OA_Address;
						docAddress.E2_Contact = parentAddress.E2_Contact;
					}
					else
					{
						docAddress.E2_CompanyName = parentAddress.E2_CompanyNameTruncated;
						docAddress.E2_Address1 = parentAddress.E2_Address1;
						docAddress.E2_Address2 = parentAddress.E2_Address2;
						docAddress.E2_City = parentAddress.E2_City;
						docAddress.E2_State = parentAddress.E2_State;
						docAddress.E2_Postcode = parentAddress.E2_Postcode;

						docAddress.E2_RN_NKCountryCode = parentAddress.E2_RN_NKCountryCode;

						docAddress.E2_Contact = parentAddress.E2_Contact;
						docAddress.E2_Phone = parentAddress.E2_Phone;
						docAddress.E2_Fax = parentAddress.E2_Fax;
						docAddress.E2_Email = parentAddress.E2_Email;
						docAddress.E2_Mobile = parentAddress.E2_Mobile;
					}
				}
			}
		}

		public override ZString GetPortOfLoading(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.PortOfLoading ?? ZString.Empty;
		}

		public override ZString GetPortOfDischarge(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.PortOfDischarge ?? ZString.Empty;
		}

		public override ZString GetVessel(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.Vessel ?? ZString.Empty;
		}

		public override ZString GetVoyageFlight(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.VoyageFlight ?? ZString.Empty;
		}

		public override ZDateTime GetE_DEP(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.E_DEP ?? ZDateTime.Empty;
		}

		public override ZDateTime GetE_ARV(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.E_ARV ?? ZDateTime.Empty;
		}

		public override ZDateTime GetA_DEP(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.A_DEP ?? ZDateTime.Empty;
		}

		public override ZDateTime GetA_ARV(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.A_ARV ?? ZDateTime.Empty;
		}

		public override ZDateTime GetFCLReceivalCommences(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.FCLReceivalCommences ?? ZDateTime.Empty;
		}

		public override ZDateTime GetLCLReceivalCommences(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.LCLReceivalCommences ?? ZDateTime.Empty;
		}

		public override ZDateTime GetFCLCutOff(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.FCLCutOff ?? ZDateTime.Empty;
		}

		public override ZDateTime GetLCLCutOff(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.LCLCutOff ?? ZDateTime.Empty;
		}

		public override ZDateTime GetFCLAvailabilityDate(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.FCLAvailabilityDate ?? ZDateTime.Empty;
		}

		public override ZDateTime GetLCLAvailabilityDate(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.LCLAvailabilityDate ?? ZDateTime.Empty;
		}

		public override ZDateTime GetFCLStorageDate(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.FCLStorageDate ?? ZDateTime.Empty;
		}

		public override ZDateTime GetLCLStorageDate(CommonCartage cartage)
		{
			return cartage?.CartageInternalType?.LCLStorageDate ?? ZDateTime.Empty;
		}
	}
}
