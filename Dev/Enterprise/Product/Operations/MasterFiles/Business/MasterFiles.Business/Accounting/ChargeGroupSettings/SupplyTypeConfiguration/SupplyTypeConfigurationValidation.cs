using CargoWise.EntityFramework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public class SupplyTypeConfigurationValidation : JobConfigurationSelectorValidation
	{
		public SupplyTypeConfigurationValidation(ISupplyTypeSelector parent) : base(parent)
		{
		}

		protected new ISupplyTypeSelector Parent
		{
			get { return (ISupplyTypeSelector)base.Parent; }
		}

		#region JobType

		protected override string DuplicateJobParametersError
		{
			get
			{
				return Res.GetString("2CC094B3-5040-4D52-9DA8-EEF60E13F886", "At least one more record already sets a behavior for the same Job parameters.");
			}
		}

		protected override bool IsDuplicateJobParameter(IJobConfigurationSelector item)
		{
			var supplyTypeConfigurationItem = (ISupplyTypeSelector)item;

			return base.IsDuplicateJobParameter(item)
				&& (supplyTypeConfigurationItem.Incoterm == INCOTermCodes.All || Parent.Incoterm == INCOTermCodes.All || supplyTypeConfigurationItem.Incoterm == Parent.Incoterm)
				&& (supplyTypeConfigurationItem.LineDepartmentPK.IsEmpty || Parent.LineDepartmentPK.IsEmpty || supplyTypeConfigurationItem.LineDepartmentPK == Parent.LineDepartmentPK);
		}

		#endregion

		public void ValidateIncoterm()
		{
			MandatoryValidation.CheckEntered(Parent.IncotermInfo);
			if (!Parent.Incoterm.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.IncotermInfo);
			}
		}

		public void ValidateLineDepartmentPK()
		{
			TypeValidation.CheckValidGuid(Parent.LineDepartmentPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.LineDepartmentPKInfo);
			ListValidation.ErrorIfCancelledAndEditable(Parent.LineDepartmentPKInfo);
		}

		public void ValidateSupplyType()
		{
			MandatoryValidation.CheckEntered(Parent.SupplyTypeInfo);

			if (!Parent.SupplyType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.SupplyTypeInfo);
			}
		}
	}
}
