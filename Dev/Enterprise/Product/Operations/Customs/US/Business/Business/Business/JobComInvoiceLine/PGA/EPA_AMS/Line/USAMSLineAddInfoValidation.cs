//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAMSLineAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAMSLineAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USAMSLineAddInfoValidation : AutoUSAMSLineAddInfoValidation
	{
		public USAMSLineAddInfoValidation(AutoUSAMSLineAddInfo parent) : base(parent)
		{
		}

		public AMSLine AMSLineDetail
		{
			get { return (AMSLine)Parent.Parent; }
		}

		public bool IsPGAValidation
		{
			get
			{
				var header = AMSLineDetail.Parent;
				var declaration = header?.InvoiceLine?.Declaration;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		internal const string EnterNumberGreaterThanZero = "Please enter a number greater than 0.";
		internal const string ConfirmSubmittedALLDocument = "Please make sure inspection document has been submitted.";

		protected override void CheckUS_InspecDateTime()
		{
			base.CheckUS_InspecDateTime();
			if (ShouldCheckForInspection)
			{
				if (IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_InspecDateTimeInfo);
				}
				if (!Parent.US_InspecDateTime.IsEmpty)
				{
					var invoiceLine = AMSLineDetail.Parent.InvoiceLine;
					if (invoiceLine != null)
					{
						var declaration = invoiceLine.Declaration;
						if (declaration != null)
						{
							var entryDate = declaration.US_EntryDate;
							if (entryDate.IsValid && Parent.US_InspecDateTime.IsValid && entryDate > Parent.US_InspecDateTime)
							{
								Parent.US_InspecDateTimeInfo.AddMessageError(InspecDateTimeShouldAfterEntryDate);
							}
						}
					}
					if (Parent.US_InspecDateTime.IsValid && (Parent.US_InspecDateTime.Hour == 24 || Parent.US_InspecDateTime.Hour == 0 && Parent.US_InspecDateTime.Minute == 0))
					{
						Parent.US_InspecDateTimeInfo.AddMessageError(TimeOfInspectionFormat);
					}
				}
			}
		}
		internal const string TimeOfInspectionFormat = "Time of Inspection may not be 00:00 nor 24:00";
		internal const string InspecDateTimeShouldAfterEntryDate = "Date of Inspection cannot be before the Entry Arrival Date.";

		protected override void CheckUS_OA_Applicant()
		{
			base.CheckUS_OA_Applicant();
			if (ShouldCheckForInspection)
			{
				var amsLine = AMSLineDetail;
				if (IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_ApplicantInfo);

					var applicantAddress = amsLine != null ? amsLine.ApplicantAddress : null;
					if (applicantAddress != null)
					{
						OrganisationValidation.ValidatePGAContact(Parent.US_OA_ApplicantInfo, OrgHeaderWrapper.New(applicantAddress));
						OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ApplicantInfo, applicantAddress);
						OrganisationValidation.ValidateCharactorsForAddressDescription(amsLine.US_OA_ApplicantInfo, applicantAddress);
					}
				}
			}
		}

		protected override void CheckUS_OA_GoodsLocation()
		{
			base.CheckUS_OA_GoodsLocation();
			if (ShouldCheckForInspection)
			{
				var amsLine = AMSLineDetail;
				if (IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_GoodsLocationInfo);

					var goodsLocationAddress = amsLine != null ? amsLine.GoodsLocationAddress : null;
					if (goodsLocationAddress != null)
					{
						OrganisationValidation.ValidatePGAContact(Parent.US_OA_GoodsLocationInfo, OrgHeaderWrapper.New(goodsLocationAddress));
						OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_GoodsLocationInfo, goodsLocationAddress);
						OrganisationValidation.ValidateCharactorsForAddressDescription(amsLine.US_OA_GoodsLocationInfo, goodsLocationAddress);
					}
				}
			}
		}

		bool ShouldCheckForInspection
		{
			get
			{
				var ams = AMSLineDetail.Parent;
				return ams != null && (ams.US_Program == AMSProgramList.Codes.EG1 || ams.US_Program == AMSProgramList.Codes.MO1 || ams.US_Program == AMSProgramList.Codes.MO5 || ams.US_Program == AMSProgramList.Codes.PN1);
			}
		}

		protected override void CheckUS_IsDocSubmitted()
		{
			base.CheckUS_IsDocSubmitted();
			var ams = AMSLineDetail.Parent;
			if (IsPGAValidation)
			{
				if (!Parent.US_IsDocSubmitted && ams != null && (ams.US_Program == AMSProgramList.Codes.EG1 || ams.US_Program == AMSProgramList.Codes.EG2 || ams.US_Program == AMSProgramList.Codes.MO2))
				{
					Parent.US_IsDocSubmittedInfo.AddMessageError(ConfirmSubmittedALLDocument);
				}
			}
		}

		protected override void CheckUS_ProductNumber()
		{
			base.CheckUS_ProductNumber();
			var ams = AMSLineDetail.Parent;
			if (ams != null && ShouldCheckForProductNumber)
			{
				var productNumber = Parent.US_ProductNumber;
				if (productNumber.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductNumberInfo);
				}
				else
				{
					CheckProductNumber(Parent.US_ProductNumberInfo);
				}
			}
		}

		protected override void CheckUS_NetWeightUQ()
		{
			base.CheckUS_NetWeightUQ();
			if (ShouldCheckForNetWeight)
			{
				if (IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightUQInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.US_NetWeightUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			}
		}

		protected override void CheckUS_NetWeight()
		{
			base.CheckUS_NetWeight();
			if (ShouldCheckForNetWeight)
			{
				if (IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightInfo);
					if (Parent.US_NetWeight < 0m)
					{
						Parent.US_NetWeightInfo.AddMessageError(EnterNumberGreaterThanZero);
					}
				}
			}
		}

		bool ShouldCheckForNetWeight
		{
			get
			{
				var ams = AMSLineDetail.Parent;
				return ams != null && (ams.US_Program == AMSProgramList.Codes.MO1 || ams.US_Program == AMSProgramList.Codes.MO4 || ams.US_Program == AMSProgramList.Codes.MO5 || ams.US_Program == AMSProgramList.Codes.MO6 || ams.US_Program == AMSProgramList.Codes.PN1);
			}
		}

		bool ShouldCheckForProductNumber
		{
			get
			{
				var ams = AMSLineDetail.Parent;
				return ams != null && (ams.US_Program == AMSProgramList.Codes.EG1 || ams.US_Program == AMSProgramList.Codes.EG2 || ams.US_Program == AMSProgramList.Codes.MO1 || ams.US_Program == AMSProgramList.Codes.MO4 || ams.US_Program == AMSProgramList.Codes.MO5 || ams.US_Program == AMSProgramList.Codes.MO6 || ams.US_Program == AMSProgramList.Codes.PN1);
			}
		}

		protected override void CheckUS_PackagesUQ()
		{
			base.CheckUS_PackagesUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PackagesUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (!Parent.US_Packages.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PackagesUQInfo);
			}
		}

		protected override void CheckUS_Packages()
		{
			base.CheckUS_Packages();
			if (ShouldCheckPackages)
			{
				if (IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PackagesInfo);

					if (Parent.US_Packages < 0m)
					{
						Parent.US_PackagesInfo.AddMessageError(EnterNumberGreaterThanZero);
					}
				}
			}
		}

		bool ShouldCheckPackages
		{
			get
			{
				var ams = AMSLineDetail.Parent;
				return ams != null && (ams.US_Program == AMSProgramList.Codes.MO1);
			}
		}

		protected override void CheckUS_PackageWeightUQ()
		{
			base.CheckUS_PackagesUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PackageWeightUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (!Parent.US_PackageWeight.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PackageWeightUQInfo);
			}
		}

		protected override void CheckUS_QtyPerPackageUQ()
		{
			base.CheckUS_QtyPerPackageUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_QtyPerPackageUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (!Parent.US_QtyPerPackage.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_QtyPerPackageUQInfo);
			}
		}

		protected override void CheckUS_LotEntity()
		{
			base.CheckUS_LotEntity();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_LotEntityInfo, AMSLineDetail.AddInfoLookups.LotEntityList);
		}

		protected override void CheckUS_OA_FinalHandler()
		{
			base.CheckUS_OA_FinalHandler();
			CheckOrganisationAMSCodeWhenOR1(((USAMSLineAddInfo)Parent).FinalHandlerAddress, Parent.US_OA_FinalHandlerInfo, 10, false);
		}

		protected override void CheckUS_OA_CerFinalHandler()
		{
			base.CheckUS_OA_CerFinalHandler();
			CheckOrganisationAMSCodeWhenOR1(((USAMSLineAddInfo)Parent).CerFinalHandlerAddress, Parent.US_OA_CerFinalHandlerInfo, 3, true);
		}

		void CheckOrganisationAMSCodeWhenOR1(OrgAddress address, ZPropertyInfo addressInfo, int codeLength, bool checkPGAContact)
		{
			if (IsPGAValidation && AMSLineDetail.Parent.IsOR1Program)
			{
				USAMSAddInfoValidation.CheckPGAContact(address, addressInfo, codeLength, checkPGAContact);
			}
		}

		void CheckProductNumber(ZPropertyInfo propertyInfo)
		{
			var productNumber = (ZString)propertyInfo.Value;
			if (!productNumber.IsEmpty)
			{
				var productBO = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Parent.Factory, productNumber, Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsStandardProductAndServiceCodes, ZDateTime.Today);
				if (productBO == null)
				{
					propertyInfo.AddMessageError(Res.GetString("de34c5b2-ab3e-4033-843e-50b5123feca4", "The product number entered is invalid."));
				}
				else if (AMSLineDetail.Parent is AMS header)
				{
					var productType = header.ProductType;
					if (!productType.IsEmpty && productType != USAMSLineProductNumberCollection.All
						&& (!productBO.Attributes.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, productType)))
					{
						var program = header.US_Program;
						propertyInfo.AddMessageError(Res.GetString("c5d6c213-e985-4988-b197-90960bfb2ad6", "The product number is not supported when the AMS program is '{0}'.", program));
					}
				}
			}
		}
	}
}
