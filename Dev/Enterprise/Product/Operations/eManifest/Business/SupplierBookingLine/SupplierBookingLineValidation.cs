//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSupplierBookingLineValidation
//
//    This class should be used for overriding validation in AutoSupplierBookingLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eManifest.Business
{
	public class SupplierBookingLineValidation : AutoSupplierBookingLineValidation
	{
		public SupplierBookingLineValidation(AutoSupplierBookingLine parent)
			: base(parent)
		{
		}

		readonly AddressValidation AddressValidationHelper = new AddressValidation();

		new SupplierBookingLine Parent
		{
			get { return (SupplierBookingLine)base.Parent; }
		}

		#region Consignment Reference

		protected override void CheckDL_ConsigneeReference()
		{
			base.CheckDL_ConsigneeReference();

			if (HasDuplicateReferencesOnHeader)
			{
				Parent.DL_ConsigneeReferenceInfo.AddError(Res.GetString("f6e52cac-552d-4ee7-a317-c681a433c0ba", "A consignment reference should be unique for the eManifest."));
			}
			else if (Parent.DL_ConsigneeReference.IsEmpty && !Parent.HasValidGS1Prefix)
			{
				Parent.DL_ConsigneeReferenceInfo.AddError(Res.GetString("e8ab2d0c-12e6-43b4-9513-515764ae1083",
					"A GS1 Prefix is needed to generate a SSCC number for booking lines without a consignment reference. Please add this line's unique consignment reference or add a GS1 Prefix to the eManifest's Consignor under Organization > Config > Registration Numbers / Codes."));
			}
		}

		#region Has Duplicates

		bool HasDuplicateReferencesOnHeader
		{
			get
			{
				var parent = Parent;
				var consigneeReference = parent.DL_ConsigneeReference;
				var bookingHeader = parent.BookingHeader;

				if (!consigneeReference.IsEmpty && bookingHeader != null && bookingHeader.BookingLines.Count > 1)
				{
					var validationCache = parent.Factory.ValidationCache;
					if (validationCache != null)
					{
						object allReferencesAsObject;
						Dictionary<ZString, List<ZGuid>> allReferences;
						if (!validationCache.CachedData.TryGetValue(bookingHeader.BookingLines, out allReferencesAsObject) ||
							(allReferences = allReferencesAsObject as Dictionary<ZString, List<ZGuid>>) == null)
						{
							allReferences = new Dictionary<ZString, List<ZGuid>>();
							foreach (SupplierBookingLine line in bookingHeader.BookingLines)
							{
								List<ZGuid> recordsWithCurrentReference;
								if (!allReferences.TryGetValue(line.DL_ConsigneeReference, out recordsWithCurrentReference))
								{
									recordsWithCurrentReference = new List<ZGuid>();
									allReferences.Add(line.DL_ConsigneeReference, recordsWithCurrentReference);
								}
								recordsWithCurrentReference.Add(line.PK);
							}

							validationCache.CachedData[bookingHeader.BookingLines] = allReferences;
						}

						List<ZGuid> recordsWithReference;
						if (allReferences.TryGetValue(consigneeReference, out recordsWithReference) && recordsWithReference.Count > 1)
						{
							return true;
						}
					}
					else
					{
						return bookingHeader.BookingLines
							.Cast<SupplierBookingLine>()
							.Any(line => ((object)line) != parent && line.DL_ConsigneeReference == consigneeReference);
					}
				}

				return false;
			}
		}

		#endregion

		#endregion

		protected override void CheckDL_CubicUQ()
		{
			base.CheckDL_CubicUQ();

			ListValidation.ErrorIfInvalidCode(Parent.DL_CubicUQInfo, Parent.Lookups.DL_CubicUQ_List);
		}

		protected override void CheckDL_GrossWeight()
		{
			base.CheckDL_GrossWeight();

			MandatoryValidation.CheckNotNegative(Parent.DL_GrossWeightInfo);
		}

		protected override void CheckDL_GrossWeightUQ()
		{
			base.CheckDL_GrossWeightUQ();

			ListValidation.ErrorIfInvalidCode(Parent.DL_GrossWeightUQInfo, Parent.Lookups.DL_GrossWeightUQ_List);
		}

		protected override void CheckDL_RS_NKServiceLevel()
		{
			base.CheckDL_RS_NKServiceLevel();

			ListValidation.ErrorIfInvalidCode(Parent.DL_RS_NKServiceLevelInfo, Parent.Lookups.DL_RS_NKServiceLevel_List);
		}

		protected override void CheckDL_F3_NKPackType()
		{
			base.CheckDL_F3_NKPackType();

			ListValidation.ErrorIfInvalidCode(Parent.DL_F3_NKPackTypeInfo, Parent.Lookups.PackTypes);
		}

		#region Consignee

		protected override void CheckDL_ConsigneeName()
		{
			MandatoryValidation.CheckEntered(Parent.DL_ConsigneeNameInfo);
		}

		protected override void CheckDL_ConsigneeAddress1()
		{
			MandatoryValidation.CheckEntered(Parent.DL_ConsigneeAddress1Info);
		}

		protected override void CheckDL_ConsigneeCity()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_CityMandatory.Value)
			{
				MandatoryValidation.CheckEntered(Parent.DL_ConsigneeCityInfo);
			}
			AddressValidationHelper.CheckPostcodeViaCity(Parent.DL_ConsigneeCityInfo, Parent.DL_ConsigneePostCodeInfo, Parent.ConsigneeCountryCode);
		}

		protected override void CheckDL_ConsigneeState()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_UseStateRules.Value)
			{
				AddressValidationHelper.CheckState(Parent.DL_ConsigneeStateInfo, Parent.ConsigneeCountryCode, Parent.ValidationSection);
			}
		}

		protected override void CheckDL_ConsigneePostCode()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_UsePostcodeRules.Value)
			{
				AddressValidationHelper.CheckPostCode(Parent.DL_ConsigneePostCodeInfo, Parent.ConsigneeCountryCode, Parent.ValidationSection);
			}
			AddressValidationHelper.CheckCityViaPostcode(Parent.DL_ConsigneePostCodeInfo, Parent.DL_ConsigneeCityInfo, Parent.ConsigneeCountryCode);
		}

		protected override void CheckDL_RN_NKConsigneeCountryCode()
		{
			if (!Parent.DL_RN_NKConsigneeCountryCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.DL_RN_NKConsigneeCountryCodeInfo);
			}

			if (RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.Value)
			{
				MandatoryValidation.CheckEntered(Parent.DL_RN_NKConsigneeCountryCodeInfo);
			}
		}

		#endregion

		#region Consignor

		protected override void CheckDL_ConsignorName()
		{
			MandatoryValidation.CheckEntered(Parent.DL_ConsignorNameInfo);
		}

		protected override void CheckDL_ConsignorAddress1()
		{
			MandatoryValidation.CheckEntered(Parent.DL_ConsignorAddress1Info);
		}

		protected override void CheckDL_ConsignorCity()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_CityMandatory.Value)
			{
				MandatoryValidation.CheckEntered(Parent.DL_ConsignorCityInfo);
			}

			AddressValidationHelper.CheckPostcodeViaCity(Parent.DL_ConsignorCityInfo, Parent.DL_ConsignorPostCodeInfo, Parent.ConsignorCountryCode);
		}

		protected override void CheckDL_ConsignorState()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_UseStateRules.Value)
			{
				AddressValidationHelper.CheckState(Parent.DL_ConsignorStateInfo, Parent.ConsignorCountryCode, Parent.ValidationSection);
			}
		}

		protected override void CheckDL_ConsignorPostCode()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_UsePostcodeRules.Value)
			{
				AddressValidationHelper.CheckPostCode(Parent.DL_ConsignorPostCodeInfo, Parent.ConsignorCountryCode, Parent.ValidationSection);
			}

			AddressValidationHelper.CheckCityViaPostcode(Parent.DL_ConsignorPostCodeInfo, Parent.DL_ConsignorCityInfo, Parent.ConsignorCountryCode);
		}

		protected override void CheckDL_RN_NKConsignorCountryCode()
		{
			if (!Parent.DL_RN_NKConsignorCountryCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.DL_RN_NKConsignorCountryCodeInfo);
			}

			if (RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.Value)
			{
				MandatoryValidation.CheckEntered(Parent.DL_RN_NKConsignorCountryCodeInfo);
			}
		}

		protected override void CheckDL_ConsignorEmail()
		{
			AddressValidationHelper.CheckEmail(Parent.DL_ConsignorEmailInfo);
		}

		#endregion
	}
}

