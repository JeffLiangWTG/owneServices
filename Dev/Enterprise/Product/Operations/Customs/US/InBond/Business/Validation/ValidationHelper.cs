using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class ValidationHelper
	{
		public void ValidateFlightNumerFormat(ZPropertyInfo propertyInfo, bool isAirInBondInitiationAndDeletionMode)
		{
			if (propertyInfo != null)
			{
				var propertyValue = propertyInfo.Value.ToString();
				if (!string.IsNullOrEmpty(propertyValue))
				{
					if (isAirInBondInitiationAndDeletionMode && !Regex.IsMatch(propertyValue, @"^[0-9]{3}([0-9a-zA-Z]?|([0-9][a-zA-Z]?))$"))
					{
						propertyInfo.AddMessageError(ValidationConstants.Header.InvalidFlightNumber);
					}

					if (propertyValue.Length > 5)
					{
						propertyInfo.AddMessageError(ValidationConstants.Header.VoyageTripNumberLengthExceeded);
					}
				}
			}
		}

		public void ValidateCarrierSCACLength(CusInBondHeader header, ZPropertyInfo propertyInfo)
		{
			if ((header.IsAirInBondInitiationAndDeletionMode || header.IsAirInBondLevelMode) && propertyInfo.Value.ToString().Length > 3)
			{
				propertyInfo.AddMessageError(ValidationConstants.Header.InvalidAirCarrierCodeLength);
			}
		}

		public static void ValidationInBondCarrierID(ZPropertyInfo propertyInfo, ZString inBondCarrierID, ZGuid inBondCarrier, CusInBondHeader header)
		{
			MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			if (!inBondCarrierID.IsEmpty)
			{
				if (!(EmployerIdentificationNumberValidator.IsValidEIN(inBondCarrierID, true) || CBPAssignedNumberValidator.IsValidCBPAssignedNumber(inBondCarrierID) || SocialSecurityNumberValidator.IsValidSSN(inBondCarrierID)))
				{
					propertyInfo.AddMessageError(ValidationConstants.MoveHeader.InBondCarrierIDValid);
				}

				if (inBondCarrier.IsEmpty)
				{
					var inbondHeader = header;
					if (inbondHeader != null)
					{
						var poa = inbondHeader.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.CountryCodes.UnitedStates, (doc) => { return IsPOAForInBondCarrier(doc, inBondCarrierID); });
						if (poa == null)
						{
							poa = inbondHeader.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.CountryCodes.UnitedStates, (doc) => { return IsPOAForInBondCarrier(doc, inBondCarrierID); });
							if (poa == null)
							{
								poa = inbondHeader.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.CountryCodes.UnitedStates, IsPOAForAnyInBondCarrier);
								if (poa == null)
								{
									poa = inbondHeader.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.CountryCodes.UnitedStates, IsPOAForAnyInBondCarrier);
								}
							}
						}

						PowerOfAttorneyValidator validator = new PowerOfAttorneyValidator();
						if (poa == null)
						{
							var notificationType = CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType();
							propertyInfo.AddNotification(notificationType, ValidationConstants.MoveHeader.GetNoPOADocumentForString(validator.CountrySpecificNameForPOA, inBondCarrierID));
						}
						else
						{
							validator.ValidatePowerOfAttorneyDocumentDates(propertyInfo, poa, "In-Bond");
							if (inbondHeader.MovementHeaders.UniqeInBondCarrierIDList.Count > 1 && poa.EQ_OH_DocumentOwner.IsEmpty)
							{
								propertyInfo.AddWarning(ValidationConstants.MoveHeader.GetMultipleCarrierIDsPOAWarning(validator.CountrySpecificNameForPOA));
							}
						}
					}
				}
			}
		}

		static bool IsPOAForAnyInBondCarrier(JobRequiredDocument doc)
		{
			return doc.EQ_OH_DocumentOwner.IsEmpty;
		}

		static bool IsPOAForInBondCarrier(JobRequiredDocument doc, ZString carrierID)
		{
			var documentOwner = doc.DocumentOwner;
			var filter = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, carrierID);
			var codeTypeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
			codeTypeFilter.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.CBPAssignedNumber);
			codeTypeFilter.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.SocialSecurityNumber);
			filter.AddToFilter(codeTypeFilter);
			return documentOwner != null && documentOwner.CustomsCodes.Find(filter).Length > 0;
		}
	}
}
