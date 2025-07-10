using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Integration.Reference;

namespace Enterprise.Freight.Forwarding.Business
{
	public class NonSecurityJobConsolAWBSpecialHandlingValidation : JobConsolAWBSpecialHandlingValidation
	{
		public NonSecurityJobConsolAWBSpecialHandlingValidation(NonSecurityJobConsolAWBSpecialHandling parent)
			: base(parent)
		{
		}

		protected new NonSecurityJobConsolAWBSpecialHandling Parent => (NonSecurityJobConsolAWBSpecialHandling)base.Parent;

		protected override void CheckJKH_Code()
		{
			base.CheckJKH_Code();

			MandatoryValidation.CheckEntered(Parent.JKH_CodeInfo);
			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("5d39729d-1dd6-4a0b-af03-b11e028cf29e", "The Special Handling Code is invalid."), Parent.JKH_CodeInfo);
			CheckNoDuplicateSpecialHandlingCodes();
			CheckConsolHasOverriddenMAWB();
			CheckJKHCodes();
			CheckJKHCodeExistsInBothIATAAndAirLine();
			CheckJKHCodeIsFromAirlineSpecific();
			CheckJKHCodeIsFromAirlineSpecificAndIsNotAddedToIATA();
		}

		void CheckNoDuplicateSpecialHandlingCodes()
		{
			var consol = Parent.Consol;
			if (consol != null)
			{
				if (consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().Any(x => x.JKH_Code == Parent.JKH_Code && x.PK != Parent.PK))
				{
					Parent.JKH_CodeInfo.AddError(Res.GetString("cb628924-0ea9-4cec-859c-1c1b49991adf", "Special Handling codes must be unique."));
				}
			}
		}

		void CheckConsolHasOverriddenMAWB()
		{
			var consol = Parent.Consol;
			if (consol != null && consol.JK_OverrideWaybillDefaults)
			{
				Parent.JKH_CodeInfo.AddWarning(Res.GetString("fa942329-3685-1b8a-48a1-aba92b5fc132", "The AWB tab is overridden so any changes made to this grid will not take effect there."));
			}
		}

		void CheckJKHCodes()
		{
			var consol = Parent.Consol;
			if (consol == null)
			{
				return;
			}

			if (Parent.JKH_Code == AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill)
			{
				var error = Res.GetString("55075ceb-24f1-78a9-4baf-44f3d3650b17", "ECC cannot be chosen here. It is to be used only by the airline.");

				if (!Parent.IsInDatabase || Parent.JKH_CodeInfo.HasChanges)
				{
					Parent.JKH_CodeInfo.AddError(error);
				}
				else
				{
					Parent.JKH_CodeInfo.AddMessageError(error);
				}
			}

			if (AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(Parent.JKH_Code) && consol.AWBHeader != null)
			{
				var consolExportAWBHeader = consol.AWBHeader as ConsolExportAWBHeader;
				if (consolExportAWBHeader != null && consol != null)
				{
					var consolSecurityStatusValidationHelper = new ConsolSecurityStatusValidationHelper(consol);
					consolSecurityStatusValidationHelper.CheckSecurityStatusCode(Parent.JKH_Code, Parent.JKH_CodeInfo);
				}
			}

			if (consol.JK_TransportMode == Core.Constants.TransportModes.Air && !consol.GetShipmentSpecialHandlingCodes().Contains(Parent.JKH_Code) && Parent.JKH_Code != consol.GetEFreightStatus())
			{
				Parent.JKH_CodeInfo.AddWarning(Res.GetString("c60c51e6-733f-46f7-b894-646ef6d90222", "Check if this Special Handling Code is still relevant as it does not exist on linked Shipment's Dangerous Goods Substances."));
			}
		}

		void CheckJKHCodeIsFromAirlineSpecific()
		{
			var consol = Parent.Consol;
			if (consol == null)
			{
				return;
			}

			if (Parent is NonSecurityJobConsolAWBSpecialHandling parent && parent?.Consol != null)
			{
				var hasAirlineSpecificSpecialHandlingCode = Parent.Lookups.SpecialHandlingCodeDescriptionListFromAirline.ContainsCode(Parent.JKH_Code);
				if (hasAirlineSpecificSpecialHandlingCode)
				{
					Parent.JKH_CodeInfo.AddWarning(Res.GetString("E887EF7D-78EB-42C8-8541-4AC875EABB7C", "Code is from airline specific special handling codes."));
				}
			}
		}

		void CheckJKHCodeIsFromAirlineSpecificAndIsNotAddedToIATA()
		{
			var consol = Parent.Consol;
			if (consol == null)
			{
				return;
			}

			if (Parent is NonSecurityJobConsolAWBSpecialHandling parent && parent?.Consol != null)
			{
				var hasAirlineSpecificSpecialHandlingCode = Parent.Lookups.SpecialHandlingCodeDescriptionListFromAirline.ContainsCode(Parent.JKH_Code);
				var hasIATASpecificSpecialHandlingCode = ObjectFactory.Get<IIATASpecialHandlingCodesProvider>().GetCodeDescriptionPairList().ContainsCode(Parent.JKH_Code);
				if (hasAirlineSpecificSpecialHandlingCode && !hasIATASpecificSpecialHandlingCode)
				{
					Parent.JKH_CodeInfo.AddWarning(Res.GetString("9ED69E2A-0071-42ED-A489-13BF5BEF1AC9", "A non-IATA approved Special Handling Code has been selected which may not be supported or accepted by other industry recipients."));
				}
			}
		}

		void CheckJKHCodeExistsInBothIATAAndAirLine()
		{
			var consol = Parent.Consol;
			if (consol == null)
			{
				return;
			}

			var hasAirlineSpecificSpecialHandlingCode = Parent.Lookups.SpecialHandlingCodeDescriptionListFromAirline.ContainsCode(Parent.JKH_Code);
			if (hasAirlineSpecificSpecialHandlingCode)
			{
				var specialHandlingCode = ObjectFactory.Get<IIATASpecialHandlingCodesProvider>();
				if (specialHandlingCode.GetCodeDescriptionPairList().ContainsCode(Parent.JKH_Code))
				{
					Parent.JKH_CodeInfo.AddWarning(Res.GetString("27D4BF7A-CB25-4107-96AC-45E20300DB3D", "This code currently exists as a defined Special Handling Code in the Reference File for this Airline. Please remove this from the Airline Reference File."));
				}
			}
		}
	}
}
