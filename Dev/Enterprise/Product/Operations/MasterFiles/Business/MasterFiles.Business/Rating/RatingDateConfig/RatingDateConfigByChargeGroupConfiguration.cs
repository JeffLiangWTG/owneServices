using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Rating
{
	public class RatingDateConfigByChargeGroupConfiguration : NonPersistentBusinessObject
	{
		public RatingDateConfigByChargeGroupConfiguration(RatingDateConfigCollection ratingDateConfigs)
		{
			this.ratingDateConfigs = ratingDateConfigs;
		}

		readonly RatingDateConfigCollection ratingDateConfigs;

		public RatingDateConfigByChargeGroupCollection RatingDateConfigByChargeGroups
		{
			get
			{
				if (ratingDateConfigByChargeGroups == null)
				{
					ratingDateConfigByChargeGroups = new RatingDateConfigByChargeGroupCollection(ratingDateConfigs);

					var chargeCodeGroups = new ChargeCodeGroupList();
					foreach (var chargeCodeGroup in chargeCodeGroups.Cast<CodeDescriptionPair>().Where(x => !NonApplicableChargeGroups.Contains(x.Code)))
					{
						var autoRateDateByChargeGroup = ratingDateConfigByChargeGroups.AddNew();
						autoRateDateByChargeGroup.ChargeGroup = chargeCodeGroup.Code;
						autoRateDateByChargeGroup.ChargeGroupDescription = chargeCodeGroup.MultilingualDescription;
					}

					RegisterEditableChildObject(ratingDateConfigByChargeGroups);
				}
				return ratingDateConfigByChargeGroups;
			}
		}
		RatingDateConfigByChargeGroupCollection ratingDateConfigByChargeGroups;

		HashSet<string> NonApplicableChargeGroups
		{
			get
			{
				if (nonApplicableChargeGroups == null)
				{
					nonApplicableChargeGroups = new HashSet<string>();
					nonApplicableChargeGroups.Add(ChargeCodeGroupList.Codes.CustomsDuty);
					nonApplicableChargeGroups.Add(ChargeCodeGroupList.Codes.NonJobRelated);
					nonApplicableChargeGroups.Add(ChargeCodeGroupList.Codes.NotGrouped);
				}

				return nonApplicableChargeGroups;
			}
		}
		HashSet<string> nonApplicableChargeGroups;
	}
}
