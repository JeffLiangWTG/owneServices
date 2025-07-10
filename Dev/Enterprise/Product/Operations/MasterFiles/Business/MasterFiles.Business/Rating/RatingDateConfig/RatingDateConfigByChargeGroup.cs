using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Rating
{
	public class RatingDateConfigByChargeGroup : NonPersistentBusinessObject
	{
		public RatingDateConfigByChargeGroup(RatingDateConfigCollection ratingDateConfigCollection)
			: base(ratingDateConfigCollection.Factory)
		{
			this.ratingDateConfigCollection = ratingDateConfigCollection;
		}

		readonly RatingDateConfigCollection ratingDateConfigCollection;

		#region Charge Group

		[MaxLength(3)]
		public ZString ChargeGroup
		{
			get => chargeGroup;
			set
			{
				SetNonPersistentPropertyValue(ChargeGroupInfo, ref chargeGroup, value);
				ChargeGroupSettings.RefreshBinding();
			}
		}
		ZString chargeGroup;

		public ZPropertyInfo ChargeGroupInfo => GetZPropertyInfo(nameof(ChargeGroup));

		#endregion

		#region Charge Group Description

		MultilingualString chargeGroupDescription;

		[MaxLength(256)]
		public MultilingualString ChargeGroupDescription
		{
			get => chargeGroupDescription ?? (NoResString)"";
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				SetNonPersistentPropertyValue(ChargeGroupDescriptionInfo, ref chargeGroupDescription, value, false);
			}
		}

		public ZPropertyInfo ChargeGroupDescriptionInfo => GetZPropertyInfo(nameof(ChargeGroupDescription));

		#endregion

		#region ChargeGroupSettings

		public RatingDateConfigCollectionViewModel ChargeGroupSettings
		{
			get
			{
				if (chargeGroupSettings == null)
				{
					chargeGroupSettings = new RatingDateConfigCollectionViewModel(ratingDateConfigCollection, ChargeGroup);
					foreach (var ratingDateConfig in ratingDateConfigCollection.Find(x => x.RDT_ChargeGroup == ChargeGroup))
					{
						chargeGroupSettings.Add(new RatingDateConfigViewModel(ratingDateConfig));
					}
					RegisterEditableChildObject(chargeGroupSettings);
				}
				return chargeGroupSettings;
			}
		}
		RatingDateConfigCollectionViewModel chargeGroupSettings;

		#endregion
	}
}
