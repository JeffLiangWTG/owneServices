//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeGovtChargeCodeOverrideValidation
//
//    This class should be used for overriding validation in AutoAccChargeGovtChargeCodeOverrideValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;

	public class AccChargeGovtChargeCodeOverrideValidation : AutoAccChargeGovtChargeCodeOverrideValidation
	{
		public AccChargeGovtChargeCodeOverrideValidation(AutoAccChargeGovtChargeCodeOverride parent) : base(parent)
		{
		}
		protected new AccChargeGovtChargeCodeOverride Parent => (AccChargeGovtChargeCodeOverride)base.Parent;

		protected override void CheckACG_CostSellAll()
		{
			base.CheckACG_CostSellAll();
			MandatoryValidation.CheckEntered(Parent.ACG_CostSellAllInfo, "Cost/Sell");
			ListValidation.ErrorIfInvalidCode(Parent.ACG_CostSellAllInfo, (NoResString)"Cost/Sell");
			CheckItIsNotDuplicate();
		}

		protected override void CheckACG_JobType()
		{
			base.CheckACG_JobType();
			ListValidation.ErrorIfInvalidCode(Parent.ACG_JobTypeInfo);
			MandatoryValidation.CheckEntered(Parent.ACG_JobTypeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckACG_TransportMode()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckACG_TransportMode();
				MandatoryValidation.CheckEntered(Parent.ACG_TransportModeInfo);

				if (!Parent.ACG_TransportMode.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.ACG_TransportModeInfo);
				}
			}
			else if (!Parent.ACG_TransportMode.IsEmpty)
			{
				Parent.ACG_TransportModeInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
			CheckItIsNotDuplicate();
		}

		protected override void CheckACG_Direction()
		{
			base.CheckACG_Direction();
			ListValidation.ErrorIfInvalidCode(Parent.ACG_DirectionInfo);
			MandatoryValidation.CheckEntered(Parent.ACG_DirectionInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckACG_GovtChargeCode()
		{
			base.CheckACG_GovtChargeCode();
			MandatoryValidation.CheckEntered(Parent.ACG_GovtChargeCodeInfo);
		}

		void CheckItIsNotDuplicate()
		{
			Parent.RemoveRowError(IsDuplicateErrorString);

			var parentCollection = (AccChargeGovtChargeCodeOverrideCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(pc => pc is AccChargeGovtChargeCodeOverrideCollection);

			if (parentCollection == null)
			{
				return;
			}

			if (parentCollection.Cast<AccChargeGovtChargeCodeOverride>().Any(c => c != Parent && Parent.IsDuplicateOf(c)))
			{
				Parent.AddRowError(IsDuplicateErrorString);
			}
		}

		bool IsParentChargeTypeComment
		{
			get { return Parent.ChargeCode != null && Parent.ChargeCode.AC_ChargeType == Core.Constants.ChargeType.Comment; }
		}

		internal static string IsDuplicateErrorString => Res.GetString("25DF04BF-2D67-45BB-9E0D-D1780B884D02", "At least one more record already sets Govt Charge Code for the same Job parameters.");
	}
}
