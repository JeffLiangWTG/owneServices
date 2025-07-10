using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	class ACECountryProcessor : CountryProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[TopLevel(typeof(ERFF102), typeof(ERFF202))]
	abstract class CountryProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIWithDatabaseLockProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public override void Process()
		{
			USCCountry country = null;

			foreach (MessageBlock block in messageBlocks)
			{
				ERFF102 f102 = block as ERFF102;
				if (f102 != null)
				{
					country = null;
					if (!f102.Country.IsEmpty)
					{
						country = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, f102.Country);
						if (country == null)
						{
							country = Factory.New<USCCountry>();
							country.UC_Code = f102.Country;
						}
						country.UC_RateColumnIndicator = f102.RateColumnIndicator.ToString();
						country.UC_RestrictionIndicator = f102.RestrictionIndicator != ZInt.Zero;
						country.UC_GSPIndicator = f102.GeneralizedSystemOfPreferencesGSPIndicator != "0";
						country.UC_GSPBeginDate = f102.GeneralizedSystemOfPreferencesGSPBeginDate;
						country.UC_GSPEndDate = f102.GeneralizedSystemOfPreferencesGSPEndDate;
						country.UC_SPICode = f102.SpecialProgramsIndicatorSPICode;
						country.UC_Name = f102.CountryName;
						country.UC_CurrencyName = f102.CurrencyName;
						country.UC_CurrencyCode = f102.ISOCurrencyCode;
						country.UC_DrawbackEligibility = f102.DrawbackEligibility == "1";
						country.UC_SpecialTradeProgramsIndicator = f102.AGOACBTPAATPDEASPIIndicator;
					}
				}
				else if (country != null)
				{
					ERFF202 f202 = block as ERFF202;

					if (f202 != null)
					{
						country.UC_RateColumnBeginDate = f202.RateColumnBeginDate;
						country.UC_RateColumnEndDate = f202.RateColumnEndDate;
						country.UC_RestrictionIndicatorBeginDate = f202.RestrictionIndicatorBeginDate;
						country.UC_RestrictionIndicatorEndDate = f202.RestrictionIndicatorEndDate;
						country.UC_SPIBeginDate = f202.SpecialProgramsIndicatorBeginDate;
						country.UC_SPIEndDate = f202.SpecialProgramsIndicatorEndDate;
						country.UC_SheduleCCountryCode = f202.Country;
						country.UC_SpecialTradeProgramsBeginDate = f202.AGOACBTPAATPDEASpecialProgramsIndicatorBeginDate;
						country.UC_SpecialTradeProgramsEndDate = f202.AGOACBTPAATPDEASpecialProgramsIndicatorEndDate;
						country.UC_LesserDevelopedCountry = f202.AGOALDDIndicatorAGOALesserDevelopedBeneficiaryCountry == "1";
						country.UC_MiscellaneousSPIIndicator = f202.MiscellaneousSPIIndicator;
						country.UC_MiscellaneousSPIBeginDate = f202.MiscellaneousSPIBeginDate;
						country.UC_MiscellaneousSPIEndDate = f202.MiscellaneousSPIEndDate;
					}
				}
			}

			if (!IsReferenceRequestedByServiceTask)
			{
				var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
				GenerateHtmlEmailAndSendToOriginalOrGroup("Country Codes", "Country Codes", "Country Codes", "Your query for Country Codes was successful. Please check your reference files for updated information.", false, branch, null);
			}
		}

		protected override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.Country; }
		}
	}
}
