using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusContainerValidation : Customs.Business.CusContainerValidation
	{
		public CusContainerValidation(CusContainer parent)
			: base(parent)
		{
		}

		public new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
		}
		protected override void CheckCO_Seal()
		{
			base.CheckCO_Seal();
			if (string.Equals(Parent.Declaration.JE_MessageType, JobMessageTypeList.Codes.Export))
			{
				if (Parent.Declaration.CusContainers.OfType<CusContainer>().Any(row => row.PK != Parent.PK && !string.IsNullOrEmpty(row.CO_Seal) && row.CO_Seal == Parent.CO_Seal))
				{
					Parent.CO_SealInfo.AddMessageError(DuplicateSealNumberErrorMessage);
				}
			}
		}

		internal const string DuplicateSealNumberErrorMessage = "The same seal number is not allowed on multiple containers.";

		protected override void CheckCO_FCL_LCL_AIR()
		{
			base.CheckCO_FCL_LCL_AIR();
			ListValidation.MessageErrorIfInvalidCode(Parent.CO_FCL_LCL_AIRInfo, Parent.Lookups.CO_FCL_LCL_NCT_List, (NoResString)FCL_LCL_AIRShouldBeInList);
		}

		internal const string FCL_LCL_AIRShouldBeInList = "Please enter a valid Container Mode code. The code you have selected is not in the Container Mode codes List.";

		protected override void CheckCO_WeightUQ()
		{
			base.CheckCO_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.CO_WeightUQInfo, Parent.Lookups.WeightUnits, (NoResString)WeightUQShouldBeInList);
		}

		internal const string WeightUQShouldBeInList = "Please enter a valid Weight UQ code. The code you have selected is not in the Weight UQ codes List.";

		protected override void CheckCO_ContainerNumber()
		{
			bool isRailCarNumber = false;
			string uSContainerType = "";
			if (Parent.Container != null)
			{
				uSContainerType = Parent.Container.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
				isRailCarNumber = Parent.IsRailCar(uSContainerType);
			}

			if (isRailCarNumber)
			{
				if (Parent.CO_ContainerNumber.Length > 14)
				{
					Parent.CO_ContainerNumberInfo.AddMessageError(string.Format(InvalidRailCarNumber, uSContainerType, CusContainer.RailCarCodes.GetDescriptionFromCode(uSContainerType)));
				}
				else
				{
					if (Parent.Declaration.IsRail)
					{
						Parent.CO_ContainerNumberInfo.AddWarning(string.Format(TreatedAsRailCarNumber, uSContainerType, CusContainer.RailCarCodes.GetDescriptionFromCode(uSContainerType)));
					}
					else
					{
						Parent.CO_ContainerNumberInfo.AddMessageError(string.Format(InvalidUseOfRailCarNumber, uSContainerType, CusContainer.RailCarCodes.GetDescriptionFromCode(uSContainerType)));
					}
				}
			}
			else
			{
				base.CheckCO_ContainerNumber();
			}
		}
		internal const string InvalidRailCarNumber = "Rail Car numbers cannot be greater than 14 characters. (This number appears to be a Rail Car number, as the US Container Code, ({0} - {1}), entered for this Container Type indicates as such.)";
		internal const string TreatedAsRailCarNumber = "This number will be treated as a Rail Car number, as the US Container Code, ({0} - {1}), entered for this Container Type indicates as such.";
		internal const string InvalidUseOfRailCarNumber = "Transport mode must be 20 and 21 (RAI) if using rail car numbers. (This number appears to be a Rail Car number, as the US Container Code, ({0} - {1}), entered for this Container Type indicates as such.)";

		protected override void CheckContainerNoHasPackages()
		{
		}

		protected override void CheckCO_RC()
		{
			base.CheckCO_RC();

			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsACECargoReleaseValidationMode && Parent.InvoiceLinePivotCollection.Count > 0)
			{
				if (declaration.PGAFlags.HasInvoiceLinesWithAPHIS)
				{
					EnsureContainerTypeAndLengthAreSet("APHIS");
				}

				if (Parent
					.InvoiceLinePivotCollection.OfType<Customs.Business.CusContainerInvoiceLinePivot>()
					.Select(p => p.InvoiceLine).OfType<JobComInvoiceLine>()
					.Any(l => l.HasAMSDetails)
				)
				{
					EnsureContainerTypeAndLengthAreSet("AMS");
				}
			}

			ValidateCO_ContainerNumber();
		}

		void EnsureContainerTypeAndLengthAreSet(string requirementSource)
		{
			if (Parent.CO_RC.IsEmpty)
			{
				Parent.CO_RCInfo.AddMessageError(string.Format(CultureInfo.InvariantCulture, ContainerTypeRequiredFor, requirementSource));
			}
			else
			{
				var refContainer = Parent.Container;
				if (refContainer == null || refContainer.RC_Length == ZDecimal.Zero)
				{
					Parent.CO_RCInfo.AddMessageError(string.Format(CultureInfo.InvariantCulture, ContainerLengthRequiredFor, requirementSource));
				}
			}
		}

		internal const string ContainerTypeRequiredFor = "Container Type is required for {0}.";
		internal const string ContainerLengthRequiredFor = "Container type must include a length for {0}.";

		internal bool IsEntrySummaryValidationMode
		{
			get { return Parent.Declaration != null && Parent.Declaration.IsEntrySummaryValidationMode; }
		}
	}
}
