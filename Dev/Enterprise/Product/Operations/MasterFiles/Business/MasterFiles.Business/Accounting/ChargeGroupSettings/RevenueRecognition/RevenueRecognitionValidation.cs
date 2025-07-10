using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class RevenueRecognitionValidation : JobConfigurationSelectorValidation
	{
		public RevenueRecognitionValidation(IRevenueRecognition parent) : base(parent)
		{
		}

		protected new IRevenueRecognition Parent
		{
			get { return (IRevenueRecognition)base.Parent; }
		}

		#region JobType

		public override void ValidateJobType()
		{
			base.ValidateJobType();

			if (!Parent.JobTypeInfo.HasErrors())
			{
				Parent.ValidateBrokerCode();
				Parent.ValidateRecognitionDateOptionCode();
			}
		}

		protected override bool IsDuplicateJobParameter(IJobConfigurationSelector item)
		{
			var revenueItem = (IRevenueRecognition)item;

			return base.IsDuplicateJobParameter(item)
					&&
					(revenueItem.BrokerCode == "" || revenueItem.BrokerCode == RevenueRecognitionLookups.BrokerCodes.All || Parent.BrokerCode == RevenueRecognitionLookups.BrokerCodes.All || revenueItem.BrokerCode == Parent.BrokerCode);
		}

		protected override string DuplicateJobParametersError
		{
			get
			{
				return Res.GetString("7e33d7f5-6189-43ad-9499-9fb03896e958", "At least one more record already sets revenue recognition behavior for the same Job parameters.");
			}
		}

		#endregion

		#region Mode

		public override void ValidateMode()
		{
			base.ValidateMode();

			if (!Parent.ModeInfo.ReadOnly && !Parent.ModeInfo.HasErrors())
			{
				if (Parent.JobType == "SHP" && Parent.Mode != "AIR" && Parent.RecognitionDateOptionCode == RevenueRecognitionLookups.RecognitionDateOptionCodes.AWBIssueDate)
				{
					Parent.ModeInfo.AddError(Res.GetString("2e02e9ad-5f5b-44a7-a76f-404f4c688d30", "You can only use the 'AWB Issue Date' Recognition Option for Shipments with a transport mode of 'AIR'."));
				}
			}
		}

		#endregion

		public override void ValidateDirectionCode()
		{
			base.ValidateDirectionCode();

			if (!Parent.DirectionCodeInfo.ReadOnly && !Parent.DirectionCodeInfo.HasErrors())
			{
				Parent.ValidateBrokerCode();
			}
		}

		#region RecognitionDateOption

		public void ValidateRecognitionDateOptionCode()
		{
			MandatoryValidation.CheckEntered(Parent.RecognitionDateOptionCodeInfo, (IMultilingualString)ResString.GetMultilingualString("7ed971ca-fe50-476f-a715-4a0e8dac3f06", "Recognition Date Option"));
			ListValidation.ErrorIfInvalidCode(Parent.RecognitionDateOptionCodeInfo, Parent.RecognitionDateOptionList);

			if (!Parent.RecognitionDateOptionCodeInfo.HasErrors())
			{
				Parent.ValidateOffsetType();
				Parent.ValidateOffset();
			}
		}

		#endregion

		#region Offset

		public void ValidateOffset()
		{
			if (!Parent.OffsetInfo.ReadOnly)
			{
				string msg = JobConfigurationSelectorHelper.ValidateOffset(Parent.OffsetType, Parent.Offset);
				if (!string.IsNullOrEmpty(msg))
				{
					Parent.OffsetInfo.AddError(msg);
				}
			}
		}

		#endregion

		#region Offset Type

		public void ValidateOffsetType()
		{
			if (!Parent.OffsetTypeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.OffsetTypeInfo, JobConfigurationSelectorHelper.OffSetTypeText);
				ListValidation.ErrorIfInvalidCode(Parent.OffsetTypeInfo, Parent.OffsetTypeList);

				if (!Parent.OffsetTypeInfo.HasErrors())
				{
					Parent.ValidateOffset();
				}
			}
		}

		#endregion

		#region Broker

		public void ValidateBrokerCode()
		{
			if (!Parent.BrokerCodeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.BrokerCodeInfo, Res.GetString("fae1347a-91e5-4933-9ad7-1d9640fb5502", "Broker"));
				ListValidation.ErrorIfInvalidCode(Parent.BrokerCodeInfo, Parent.BrokerList);
			}
		}

		#endregion
	}
}
