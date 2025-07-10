using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RatingDateConfigViewModel : NonPersistentBusinessObject
	{
		public RatingDateConfigViewModel(RatingDateConfig ratingDateConfig)
			: base(ratingDateConfig.Factory)
		{
			this.ratingDateConfig = ratingDateConfig;
		}

		public readonly RatingDateConfig ratingDateConfig;

#if DEBUG
		public RatingDateConfig RatingDateConfigForTest => ratingDateConfig;
#endif

		#region Properties

		#region JobType

		[MaxLength(3)]
		[List("JobTypeList")]
		public ZString JobType
		{
			get => ratingDateConfig.RDT_JobType;
			set => ratingDateConfig.RDT_JobType = value;
		}

		public CodeDescriptionPairList JobTypeList => ratingDateConfig.Lookups.JobTypeList;

		public ZPropertyInfo JobTypeInfo => ratingDateConfig.RDT_JobTypeInfo;

		#endregion

		#region Direction

		[MaxLength(3)]
		[List("DirectionList")]
		public ZString DirectionCode
		{
			get => ratingDateConfig.RDT_Direction;
			set => ratingDateConfig.RDT_Direction = value;
		}

		public CodeDescriptionPairList DirectionList => ratingDateConfig.Lookups.DirectionList;

		public ZPropertyInfo DirectionCodeInfo => ratingDateConfig.RDT_DirectionInfo;

		public bool DirectionCode_ReadOnly => ratingDateConfig.RDT_Direction_ReadOnly;

		#endregion

		#region Mode

		[MaxLength(3)]
		[List("TransportModeList")]
		public ZString Mode
		{
			get => ratingDateConfig.RDT_TransportMode;
			set => ratingDateConfig.RDT_TransportMode = value;
		}

		public CodeDescriptionPairList TransportModeList => ratingDateConfig.Lookups.TransportModeList;

		public ZPropertyInfo ModeInfo => ratingDateConfig.RDT_TransportModeInfo;

		public bool Mode_ReadOnly => ratingDateConfig.RDT_TransportMode_ReadOnly;

		#endregion

		#endregion

		public RatingDateConfigLookups Lookups => ratingDateConfig.Lookups;

		#region RateType

		[MaxLength(3)]
		[List("RateTypeList")]
		public ZString RateType
		{
			get => ratingDateConfig.RDT_RateType;
			set => ratingDateConfig.RDT_RateType = value;
		}

		public CodeDescriptionPairList RateTypeList => ratingDateConfig.Lookups.RateTypeList;

		public ZPropertyInfo RateTypeInfo => ratingDateConfig.RDT_RateTypeInfo;

		#endregion

		#region ContainerMode

		[MaxLength(3)]
		[List("ContainerModeList")]
		public ZString ContainerMode
		{
			get => ratingDateConfig.RDT_ContainerMode;
			set => ratingDateConfig.RDT_ContainerMode = value;
		}

		public CodeDescriptionPairList ContainerModeList => ratingDateConfig.Lookups.ContainerModeList;

		public ZPropertyInfo ContainerModeInfo => ratingDateConfig.RDT_ContainerModeInfo;

		public bool ContainerMode_ReadOnly => ratingDateConfig.RDT_ContainerMode_ReadOnly;

		#endregion

		#region DateType

		[MaxLength(3)]
		[List("DateTypeList")]
		public ZString DateType
		{
			get => ratingDateConfig.RDT_AutoratingDate;
			set => ratingDateConfig.RDT_AutoratingDate = value;
		}

		public CodeDescriptionPairList DateTypeList => ratingDateConfig.Lookups.DateTypeList;

		public ZPropertyInfo DateTypeInfo => ratingDateConfig.RDT_AutoratingDateInfo;

		#endregion

		#region Location

		[List("AutoRatingLocationCollection")]
		public ZString Location
		{
			get => ratingDateConfig.RDT_Location;
			set => ratingDateConfig.RDT_Location = value;
		}

		public LocationCollection AutoRatingLocationCollection => ratingDateConfig.Lookups.AutoRatingLocationCollection;

		public ZPropertyInfo LocationInfo => ratingDateConfig.RDT_LocationInfo;

		public bool Location_ReadOnly => ratingDateConfig.RDT_Location_ReadOnly;

		#endregion

		#region IsFallbackDisabled

		public ZBool IsFallbackDisabled
		{
			get => ratingDateConfig.RDT_NoFallback;
			set => ratingDateConfig.RDT_NoFallback = value;
		}

		public ZPropertyInfo IsFallbackDisabledInfo => ratingDateConfig.RDT_NoFallbackInfo;

		protected bool IsFallbackDisabled_ReadOnly => DateType != JobDateTypes.Codes.HouseBillIssueDate;

		#endregion

		public override void Delete()
		{
			ratingDateConfig.Delete();
			base.Delete();
		}
	}
}
