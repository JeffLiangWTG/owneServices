using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusContainerValidation
//
//    This class should be used for overriding validation in AutoCusContainerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusContainerValidation : AutoCusContainerValidation
	{
		public CusContainerValidation(AutoCusContainer parent)
			: base(parent)
		{
		}

		#region Locally Typed CusContainer Reference (Parent)
		public new BaseCusContainer Parent
		{
			get { return (BaseCusContainer)base.Parent; }
		}
		#endregion

		#region CheckCO_ContainerNumber Override and associated virtual methods
		public static string CannotHaveContainersOnAirJob
		{
			get { return Res.GetString("b7de4e02-7660-43fc-9c64-238ae01866e8", "Cannot Have Containers On Air Customs Jobs - Please remove all containers or change the transport mode to sea freight."); }
		}

		protected override void CheckCO_ContainerNumber()
		{
			base.CheckCO_ContainerNumber();
			if (Parent.Declaration != null && !Parent.CO_ContainerNumber.IsEmpty)
			{
				if (!Parent.Declaration.IsAir)
				{
					CheckContainerNoHasValidCharactersOnly();
					if (!Parent.CO_ContainerNumberInfo.HasMessageErrors())
					{
						CheckContainerNoHasValidCheckDigit();

						if (Parent.Declaration.IsContainerPackingRequired)
						{
							CheckContainerNoHasPackages();
						}
					}

					CheckForDuplicateContainerNumber();

					if (Parent.Declaration.IsContainerInvoiceLinkRelevant)
					{
						if (!Parent.Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().Any((invoiceLine => invoiceLine?.ContainersPivot?.Contains(Parent) ?? false)))
						{
							Parent.CO_ContainerNumberInfo.AddNotification(Parent.Declaration.ContainerNotLinkedSeverity, MessageForContainerShouldLinkToOneInvoiceLine);
						}
					}
				}
				else
				{
					if (!Parent.Declaration.ContainersAlwaysRequired && !Parent.Declaration.ContainersRequired)
					{
						Parent.CO_ContainerNumberInfo.AddMessageError(CannotHaveContainersOnAirJob);
					}
				}
			}

			Parent.JobContainer.Validation.ValidateJC_ContainerNum();
			Parent.CO_ContainerNumberInfo.AddAllNotificationsFrom(Parent.JobContainer.JC_ContainerNumInfo);
		}

		protected virtual string MessageForContainerShouldLinkToOneInvoiceLine
		{
			get { return ContainerShouldLinkToOneInvoiceLine; }
		}

		public static string ContainerShouldLinkToOneInvoiceLine
		{
			get { return Res.GetString("1ae88e06-3c25-4aca-9cfc-45889ab95aab", "A container should be linked to at least one invoice line. Please go to the invoice line tab -> containers sub tab and select appropriate container(s)"); }
		}

		protected override void CheckCO_FCL_LCL_AIR()
		{
			base.CheckCO_FCL_LCL_AIR();
			Parent.JobContainer.Validation.ValidateJC_ContainerMode();
			Parent.CO_FCL_LCL_AIRInfo.AddAllNotificationsFrom(Parent.JobContainer.JC_ContainerModeInfo);
		}

		protected override void CheckCO_RC()
		{
			base.CheckCO_RC();
			Parent.JobContainer.Validation.ValidateJC_RC();
			Parent.CO_RCInfo.AddAllNotificationsFrom(Parent.JobContainer.JC_RCInfo);
		}

		protected override void CheckCO_Seal()
		{
			base.CheckCO_Seal();
			Parent.JobContainer.Validation.ValidateJC_SealNum();
			Parent.CO_SealInfo.AddAllNotificationsFrom(Parent.JobContainer.JC_SealNumInfo);
		}

		public static string ContainersRequirePackages
		{
			get { return Res.GetString("1851e4d0-b7c9-48c9-a941-356284bf5e95", "Container Has No Packing Lines - You must enter some packing lines for this container."); }
		}

		protected virtual void CheckContainerNoHasPackages()
		{
			bool foundPackageRecord = false;

			foreach (BasePackingGroup packingGroup in Parent.PackingGroups)
			{
				if (packingGroup.Packages.Count != 0)
				{
					foundPackageRecord = true;
					break;
				}
			}

			if (!foundPackageRecord)
			{
				ContainerPackageNotFoundMessage();
			}
		}

		public virtual void ContainerPackageNotFoundMessage()
		{
			Parent.CO_ContainerNumberInfo.AddMessageError(ContainersRequirePackages);
		}

		public static string MustOnlyContainAlphaNumerics
		{
			get { return Res.GetString("1215af40-6612-477b-b63d-57d5e3e303c2", "Invalid Characters In Container Number - Container number must only contain alphanumeric characters."); }
		}
		protected virtual void CheckContainerNoHasValidCharactersOnly()
		{
			if (Parent.CO_ContainerNumber.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") != Parent.CO_ContainerNumber)
			{
				Parent.CO_ContainerNumberInfo.AddMessageError(MustOnlyContainAlphaNumerics);
			}
		}

		protected virtual void CheckContainerNoHasValidCheckDigit()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.CO_ContainerNumberInfo);
			ContainerNumberValidation.WarnIfInvalid(Parent.CO_ContainerNumberInfo);
		}

		public static string DuplicateContainerNumber
		{
			get { return Res.GetString("173fb518-8aca-4a9a-8fc3-19e6ff59c6a3", "Duplicate Container Number - Please remove duplicate container."); }
		}
		void CheckForDuplicateContainerNumber()
		{
			BaseJobDeclaration declaration = Parent.Declaration;

			ZQuery matchingContainerFilter = new ZQuery(CusContainerSchema.CO_ContainerNumber, Parent.CO_ContainerNumber);
			matchingContainerFilter.AddToFilter(CusContainerSchema.CO_JE, Parent.CO_JE);
			matchingContainerFilter.AddToFilter(CusContainerSchema.CO_ClusterKey, Parent.CO_ClusterKey);
			matchingContainerFilter.AddToFilter(CusContainerSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			matchingContainerFilter.FetchOnlyFromLocalCache = declaration == null || !declaration.IsInDatabase;
			var duplicateContainer = Parent.Factory.LoadTop1<BaseCusContainer>(matchingContainerFilter);
			if (duplicateContainer != null)
			{
				Parent.CO_ContainerNumberInfo.AddError(DuplicateContainerNumber);
			}
			else
			{
				ZDateTime twoMonthsAgo = ZDateTime.Today.AddMonths(-2);

				if (declaration.JE_DateOfArrival > twoMonthsAgo)
				{
					ZDBOnlyQuery declarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
					declarationQuery.AddToFilter(JobDeclarationFilter.ForCountry(false, declaration.CountryCode, Parent.Factory), JoinCondition.And);
					declarationQuery.AddToFilter(JoinCondition.And, JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration.PK);
					declarationQuery.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.GreaterThan, twoMonthsAgo);

					ZDBOnlySubQuery duplicateContainerQuery = new ZDBOnlySubQuery(typeof(BaseCusContainer), CusContainerSchema.CO_JE);
					duplicateContainerQuery.AddToFilter(JoinCondition.And, CusContainerSchema.CO_ContainerNumber, SQLComparisonOperator.Equal, Parent.CO_ContainerNumber);

					declarationQuery.AddSubQuery(duplicateContainerQuery, JoinCondition.And);

					BaseJobDeclaration otherDec = Parent.Factory.LoadTop1<BaseJobDeclaration>(declarationQuery);
					if (otherDec != null)
					{
						var warningText = Res.GetString("C3741300-2395-40F9-A296-59A81EC37FA3", "Another Declaration already contains the same container as this declaration (Declaration: '{0}', Company: '{1}', Branch: '{2}').",
							otherDec.JE_DeclarationReference, otherDec.Company.GC_Name, (otherDec.Branch != null ? otherDec.Branch.GB_BranchName : ZString.Empty));
						Parent.CO_ContainerNumberInfo.AddWarning(warningText);
					}
				}
			}
		}

		protected override void CheckCO_RN_NKOwnerCountry()
		{
			base.CheckCO_RN_NKOwnerCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.CO_RN_NKOwnerCountryInfo);
		}

		#endregion

		#region CustomLabelPropertyValidation Overrides
		protected override void CheckCO_CustomDecimal1()
		{
			base.CheckCO_CustomDecimal1();
			CustomLabelPropertyValidation.Validate(new BaseCusContainer.CustomLabelsProvider(Parent.Declaration), Parent.CO_CustomDecimal1Info);
		}

		protected override void CheckCO_CustomDate1()
		{
			base.CheckCO_CustomDate1();
			CustomLabelPropertyValidation.Validate(new BaseCusContainer.CustomLabelsProvider(Parent.Declaration), Parent.CO_CustomDate1Info);
		}

		protected override void CheckCO_CustomAttrib1()
		{
			base.CheckCO_CustomAttrib1();
			CustomLabelPropertyValidation.Validate(new BaseCusContainer.CustomLabelsProvider(Parent.Declaration), Parent.CO_CustomAttrib1Info);
		}

		protected CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}
		CustomLabelPropertyValidation customLabelPropertyValidation;

		#endregion
	}
}
