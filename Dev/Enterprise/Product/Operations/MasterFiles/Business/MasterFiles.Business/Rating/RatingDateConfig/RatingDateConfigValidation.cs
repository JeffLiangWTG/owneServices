//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRatingDateConfigValidation
//
//    This class should be used for overriding validation in AutoRatingDateConfigValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business.Rating
{
	using System.Collections.Generic;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWiseOne.ResourceStrings;
	using Enterprise.ZArchitecture.Schema;

	public class RatingDateConfigValidation : AutoRatingDateConfigValidation
	{
		public RatingDateConfigValidation(AutoRatingDateConfig parent) : base(parent)
		{
		}

		protected override void CheckRDT_JobType()
		{
			MandatoryValidation.CheckEntered(Parent.RDT_JobTypeInfo, (IMultilingualString)ResString.GetMultilingualString("EB6999F7-5FE8-4A21-951D-B838B324316F", "Job Type"));
			ListValidation.ErrorIfInvalidCode(Parent.RDT_JobTypeInfo, Parent.Lookups.JobTypeList);
			if (!Parent.RDT_JobTypeInfo.HasErrors())
			{
				CheckForDuplicate(Parent.RDT_JobTypeInfo);
			}

			if (!Parent.RDT_JobTypeInfo.HasErrors())
			{
				ValidateRDT_Direction();
				ValidateRDT_ContainerMode();
			}
		}

		void CheckForDuplicate(ZPropertyInfo propertyInfo)
		{
			foreach (var item in ParentCollectionForValidation)
			{
				if (item != Parent)
				{
					if (IsDuplicateJobParameter(item))
					{
						propertyInfo.AddError(Business.Res.GetString("A56F9E62-12C1-4D09-8F23-ECCEB1012B7B", "At least one more record already sets Auto Rate behavior for the same Job parameters."));
						break;
					}
				}
			}
		}

		bool IsDuplicateJobParameter(RatingDateConfig item)
		{
			var result = JobConfigurationSelectorValidation.IsDuplicateJobConfigurationSelector(Parent.RDT_JobType, Parent.RDT_Direction, Parent.RDT_TransportMode, item.RDT_JobType, item.RDT_Direction, item.RDT_TransportMode);
			if (!result)
			{
				return false;
			}

			return AutoRateDateValidation.IsDuplicateAutoRateDate((IAutoRateDate)Parent, item);
		}

		IEnumerable<RatingDateConfig> ParentCollectionForValidation
		{
			get
			{
				var query = new ZQuery(RatingDateConfigSchema.RDT_ParentTableCode, OrgHeaderSchema.Constants.Prefix);
				query.AddToFilter(RatingDateConfigSchema.RDT_ParentID, Parent.RDT_ParentID);
				query.AddToFilter(RatingDateConfigSchema.RDT_ChargeGroup, Parent.RDT_ChargeGroup);
				return Parent.Factory.Load<RatingDateConfig>(query);
			}
		}

		protected override void CheckRDT_Direction()
		{
			if (!Parent.RDT_DirectionInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.RDT_DirectionInfo, (IMultilingualString)ResString.GetMultilingualString("8092D0B5-AD40-4AD0-B9E3-DBB428CA7326", "Direction"));
				ListValidation.ErrorIfInvalidCode(Parent.RDT_DirectionInfo, Parent.Lookups.DirectionList);
			}
		}

		protected override void CheckRDT_TransportMode()
		{
			if (!Parent.RDT_TransportModeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.RDT_TransportModeInfo, (IMultilingualString)ResString.GetMultilingualString("383EEEB7-8FFE-4CE9-9CFB-054C0206A6D2", "Mode"));
				ListValidation.ErrorIfInvalidCode(Parent.RDT_TransportModeInfo, Parent.Lookups.TransportModeList);
			}
		}

		protected override void CheckRDT_AutoratingDate()
		{
			MandatoryValidation.CheckEntered(Parent.RDT_AutoratingDateInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RDT_AutoratingDateInfo, Parent.Lookups.DateTypeList);
		}

		protected override void CheckRDT_RateType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.RDT_RateTypeInfo, Parent.Lookups.RateTypeList);
		}

		protected override void CheckRDT_ContainerMode()
		{
			if (!Parent.RDT_ContainerModeInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.RDT_ContainerModeInfo, Parent.Lookups.ContainerModeList);
			}
		}

		protected override void CheckRDT_Location()
		{
			ListValidation.ErrorIfInvalidCode(Parent.RDT_LocationInfo, Parent.Lookups.AutoRatingLocationCollection);
		}
	}
}
