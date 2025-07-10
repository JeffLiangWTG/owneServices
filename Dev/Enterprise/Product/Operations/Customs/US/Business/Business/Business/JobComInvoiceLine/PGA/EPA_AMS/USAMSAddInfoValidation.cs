//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAMSAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAMSAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USAMSAddInfoValidation : AutoUSAMSAddInfoValidation
	{
		public USAMSAddInfoValidation(AutoUSAMSAddInfo parent) : base(parent)
		{
		}

		AMS AMSLine
		{
			get { return (AMS)Parent.Parent; }
		}

		public override void ValidateAll()
		{
			AMSLine.ClearRowNotifications();
			base.ValidateAll();

			EnsureThatAtLeastOneContainerSelectedForAMS();
		}

		bool IsPGAValidation
		{
			get
			{
				var declaration = AMSLine?.InvoiceLine?.Declaration;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		protected override void CheckUS_Program()
		{
			base.CheckUS_Program();

			var amsLine = AMSLine;
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.US_ProgramInfo, amsLine.AddInfoLookups.ProgramList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.US_ProgramInfo);
			}

			if (!parent.US_Program.IsEmpty)
			{
				var invoiceLine = amsLine.InvoiceLine;
				if (invoiceLine != null)
				{
					if (invoiceLine.AMSLines.Cast<AMS>().Select(x => x.US_Program).Distinct().Count() != invoiceLine.AMSLines.Count)
					{
						parent.US_ProgramInfo.AddMessageError(ProgramsRepeat);
					}
					else
					{
						CheckTariffCodeAndDetailsLine(invoiceLine);
					}
				}

				if (parent.US_Program == AMSProgramList.Codes.OR2 && amsLine.AMSLines.Count == 0)
				{
					parent.US_ProgramInfo.AddMessageError(AtLeastOneCertificateLineRequiredForOR2);
				}
			}
		}

		internal const string AtLeastOneCertificateLineRequiredForOR2 = "You should enter at least one certificate line.";

		protected override void CheckUS_NetWeight()
		{
			base.CheckUS_NetWeight();
			if (IsPGAValidation && !AMSLine.US_NetWeightInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightInfo);
			}
		}

		protected override void CheckUS_NetWeightUQ()
		{
			base.CheckUS_NetWeightUQ();
			if (IsPGAValidation && !AMSLine.US_NetWeightUQInfo.ReadOnly)
			{
				if (AMSLine.IsMO8Program)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightUQInfo);
				}
				if (!Parent.US_NetWeightUQ.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_NetWeightUQInfo, AMSLine.AddInfoLookups.UnitOfMeasureList);
				}
			}
		}

		internal const string ProgramsRepeat = "Same program cannot be reported more than once for entry line.";

		protected override void CheckUS_IntendedUseCode()
		{
			base.CheckUS_IntendedUseCode();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_IntendedUseCodeInfo, AMSLine.AddInfoLookups.IntendedUseCodeList);
			if (IsPGAValidation && !AMSLine.US_IntendedUseCode_ReadOnly && !AMSLine.IsMO7Program && !AMSLine.IsOR1Program)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseCodeInfo);
			}

			ValidateUS_IntendedUseDescription();
		}

		protected override void CheckUS_IntendedUseDescription()
		{
			base.CheckUS_IntendedUseDescription();

			if (IsPGAValidation && !AMSLine.US_IntendedUseDescription_ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseDescriptionInfo);
			}
		}

		protected override void CheckUS_CommercialDescription()
		{
			base.CheckUS_CommercialDescription();
			if (IsPGAValidation && !AMSLine.US_CommercialDescription_ReadOnly && !AMSLine.IsMO7Program)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CommercialDescriptionInfo);
			}
		}

		protected override void CheckUS_OA_CertifyingBody()
		{
			base.CheckUS_OA_CertifyingBody();

			if (!Parent.US_OA_CertifyingBody.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.US_OA_CertifyingBodyInfo, AMSLine.CertifyingBodyAddress);
			}
			CheckOrganisationAMSCodeWhenOR1(((AMSAddInfo)Parent).CertifyingBodyAddress, Parent.US_OA_CertifyingBodyInfo, 3);
		}

		protected override void CheckUS_OA_Recipient()
		{
			base.CheckUS_OA_Recipient();

			if (!Parent.US_OA_Recipient.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.US_OA_RecipientInfo, AMSLine.RecipientAddress);
			}
			CheckOrganisationAMSCodeWhenOR1(((AMSAddInfo)Parent).RecipientAddress, Parent.US_OA_RecipientInfo, 10);
		}

		void CheckOrganisationAMSCodeWhenOR1(OrgAddress address, ZPropertyInfo addressInfo, int codeLength)
		{
			if (IsPGAValidation && AMSLine.IsOR1Program)
			{
				CheckPGAContact(address, addressInfo, codeLength, true);
			}
		}

		internal static void CheckPGAContact(OrgAddress address, ZPropertyInfo addressInfo, int codeLength, ZBool checkPGAContact)
		{
			var orgHeader = address?.Header;
			if (address == null || orgHeader == null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(addressInfo);
			}
			else
			{
				var amsCode = orgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.AMSRegistrationNumber, Core.Constants.CountryCodes.UnitedStates);
				if (amsCode.IsEmpty)
				{
					addressInfo.AddMessageError(Res.GetString("0A4E442E-93AC-49F8-BE88-046AC07882C6", "Organization must have Registration Code of type AMS on file."));
				}
				else if (!(amsCode.IsNumbersOnlyOrEmpty && amsCode.Length == codeLength))
				{
					addressInfo.AddMessageError(Res.GetString("7BB0AD27-B2A3-4F19-AB4B-5E1741FFAD1C", "AMS code must be {0} digits.", codeLength));
				}

				if (checkPGAContact)
				{
					var wrapper = OrgHeaderWrapper.New(orgHeader) as IPGAContactDetails;
					if (wrapper.Name.IsEmpty)
					{
						addressInfo.AddMessageError(Res.GetString("2E9E27FE-2334-4CCA-B44C-17492AD42B3F", "Must have valid PGA contact info on file."));
					}
					else if (wrapper.PhoneNumber.IsEmpty || wrapper.EmailAddress.IsEmpty)
					{
						addressInfo.AddMessageError(Res.GetString("82FB40FA-310E-42AC-823E-334FDA8C1308", "PGA contact is missing phone number or email address."));
					}
				}
			}
		}

		protected override void CheckUS_IsElecImageSubmitted()
		{
			base.CheckUS_OA_Recipient();
			var parent = Parent;
			if (IsPGAValidation && parent.US_Program == AMSProgramList.Codes.OR1 && !parent.US_IsElecImageSubmitted)
			{
				parent.US_IsElecImageSubmittedInfo.AddMessageError(ElecImageSubmittedShouldBeTicked);
			}
		}

		internal const string ElecImageSubmittedShouldBeTicked = "Elec. Image Submitted should be ticked.";

		void CheckTariffCodeAndDetailsLine(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null)
			{
				var tariffs = new USCTariff[] { invoiceLine.ImportTariffForPGA, invoiceLine.ImportSupTariff };
				var programCode = AMSLine.US_Program;
				var dateForValidation = invoiceLine.EffectiveDateForDutyRate;
				var tariffEligbleForAMS = tariffs.FirstOrDefault(x => IsTariffEligbleForAMS(x, programCode, dateForValidation));
				if (tariffEligbleForAMS == null)
				{
					Parent.US_ProgramInfo.AddMessageError(TariffCodeNotForProgram);
				}
				else
				{
					var regulatedPeriod = tariffEligbleForAMS.RegulatedPeriodForTariff(programCode, dateForValidation);
					if (!regulatedPeriod.IsEmpty)
					{
						Parent.US_ProgramInfo.AddMessageError(Res.GetString("0F42A584-1A9F-4D1A-BC6D-E56F8B06A6C6", "Program {0} may not be used for tariff {1} with entry date {2}. This program may only be used {3} the regulated period of {4}.", programCode, tariffEligbleForAMS.UE_Tariff, dateForValidation.ToShortDateString(), AMSLine.IsMO7Program ? "outside" : "during", regulatedPeriod));
					}

					var details = AMSLine.AMSLines;
					if (!programCode.IsEmpty && programCode != AMSProgramList.Codes.MO7 && (details == null || details.Count == 0))
					{
						Parent.US_ProgramInfo.AddMessageError(Res.GetString("FEE9EBE1-A08B-4FDF-98F0-DD0663A7C2B5", "You should enter at least one details line."));
					}
				}
			}
		}
		internal const string TariffCodeNotForProgram = "Tariff is not eligible for this AMS Program. Please refer to the AMS Implementation Guide for a complete list.";

		internal static ZBool IsTariffEligbleForAMS(USCTariff tariff, ZString programCode, ZDateTime dateForDutyCalculation)
		{
			if (tariff != null && !programCode.IsEmpty)
			{
				return tariff.Factory.GetCachedValue<ZBool>("IsTariffEligbleForAMS" + tariff.UE_Tariff + programCode + dateForDutyCalculation.ToLongTimeString(), () => IsTariffEligbleForAMSCore(tariff, programCode, dateForDutyCalculation));
			}
			return false;
		}

		static bool IsTariffEligbleForAMSCore(USCTariff tariff, ZString programCode, ZDateTime dateForDutyCalculation)
		{
			var result = programCode == AMSProgramList.Codes.OR1 || programCode == AMSProgramList.Codes.OR2;
			if (!result)
			{
				result = tariff.Applies(programCode, dateForDutyCalculation);
			}

			return result;
		}

		void EnsureThatAtLeastOneContainerSelectedForAMS()
		{
			var amsLine = AMSLine;
			var invoiceLine = amsLine?.InvoiceLine;
			if (amsLine != null &&
				AMS.ProgramsRequiringContainerInfo.Contains(amsLine.US_Program) &&
				invoiceLine != null &&
				invoiceLine.Declaration.IsContainerised && invoiceLine.ContainersPivot.Count == 0)
			{
				amsLine.AddRowMessageError(AtLeastOneContainerRequiredForAMS);
			}
		}
		internal const string AtLeastOneContainerRequiredForAMS = "At least one container is mandatory for AMS reporting.";
	}
}
