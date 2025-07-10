using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	public abstract class JobConfigurationSelectorValidation
	{
		public JobConfigurationSelectorValidation(IJobConfigurationSelector parent)
		{
			fParent = parent;
		}

		protected IJobConfigurationSelector Parent => fParent;
		readonly IJobConfigurationSelector fParent;

		#region JobType

		public virtual void ValidateJobType()
		{
			MandatoryValidation.CheckEntered(Parent.JobTypeInfo, (IMultilingualString)ResString.GetMultilingualString("7f02c24a-606e-4f05-90e8-af17b66d55d1", "Job Type"));
			ListValidation.ErrorIfInvalidCode(Parent.JobTypeInfo, Parent.JobTypeList);
			if (!Parent.JobTypeInfo.HasErrors())
			{
				CheckForDuplicate(Parent.JobTypeInfo);
			}

			if (!Parent.JobTypeInfo.HasErrors())
			{
				Parent.ValidateDirectionCode();
				Parent.ValidateMode();
			}
		}

		protected void CheckForDuplicate(ZPropertyInfo propertyInfo)
		{
			if (Parent.ParentCollectionForValidation.Any((item) => item != Parent && IsDuplicateJobParameter(item)))
			{
				propertyInfo.AddError(DuplicateJobParametersError);
			}
		}

		protected virtual bool IsDuplicateJobParameter(IJobConfigurationSelector item)
			=> IsDuplicateJobConfigurationSelector(Parent.JobType, Parent.DirectionCode, Parent.Mode, item.JobType, item.DirectionCode, item.Mode);

		public static bool IsDuplicateJobConfigurationSelector(string parentJobType, string parentDirectionCode, string parentMode, string itemJobType, string itemDirectionCode, string itemMode)
			 => (parentJobType == RevenueRecognitionLookups.JobTypeAdditionalCodes.All || itemJobType == RevenueRecognitionLookups.JobTypeAdditionalCodes.All || itemJobType == parentJobType)
				&& (string.IsNullOrEmpty(itemDirectionCode) || itemDirectionCode == Constants.FreightShipmentDirection.Code.All || parentDirectionCode == Constants.FreightShipmentDirection.Code.All || itemDirectionCode == parentDirectionCode)
				&& (string.IsNullOrEmpty(itemMode) || itemMode == RevenueRecognitionLookups.ModeAdditionalCodes.All || parentMode == RevenueRecognitionLookups.ModeAdditionalCodes.All || itemMode == parentMode);

		protected abstract string DuplicateJobParametersError { get; }

		#endregion

		#region Direction

		public virtual void ValidateDirectionCode()
		{
			if (!Parent.DirectionCodeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.DirectionCodeInfo, (IMultilingualString)ResString.GetMultilingualString("e7435d5a-a6c9-4faa-b815-08f8a8dd5fc4", "Direction"));
				ListValidation.ErrorIfInvalidCode(Parent.DirectionCodeInfo, Parent.DirectionList);
			}
		}

		#endregion

		#region Mode

		public virtual void ValidateMode()
		{
			if (!Parent.ModeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.ModeInfo, (IMultilingualString)ResString.GetMultilingualString("9767b001-e44b-40f3-b5a0-3432164fea92", "Mode"));
				ListValidation.ErrorIfInvalidCode(Parent.ModeInfo, Parent.ModeList);

				if (!Parent.ModeInfo.HasErrors())
				{
					if ((Parent.JobType == "CSH" || Parent.JobType == "CLL") &&
						(Parent.Mode != Core.Constants.TransportModes.Air &&
						Parent.Mode != Core.Constants.TransportModes.Sea &&
						Parent.Mode != Core.Constants.TransportModes.Road &&
						Parent.Mode != Core.Constants.TransportModes.Rail &&
						Parent.Mode != RevenueRecognitionLookups.ModeAdditionalCodes.All))
					{
						Parent.ModeInfo.AddError(Res.GetString("26a6da1f-24c0-4f9d-80ef-aefedf26dd84", "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type."));
					}
				}
			}
		}

		#endregion
	}
}
