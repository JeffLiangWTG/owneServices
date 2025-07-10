using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business.Rating
{
	public partial class RatingDocumentsChargeGroupingOrRollup : AutoRatingDocumentsChargeGroupingOrRollup, IRatingDocRollupOrSort
	{
		[CodeAlive("is going to used to store the Charge grouping and Roll up configuration for Organization")]
		public RatingDocumentsChargeGroupingOrRollup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		public bool IsRegistry => false;

		public ZString Module
		{
			get { return RCG_Module; }
			set { RCG_Module = value; }
		}

		ZPropertyInfo IRatingDocRollupOrSort.ModuleInfo => RCG_ModuleInfo;

		public ZString JobType
		{
			get { return RCG_JobType; }
			set { RCG_JobType = value; }
		}

		ZPropertyInfo IRatingDocRollupOrSort.JobTypeInfo => RCG_JobTypeInfo;

		public ZString TransportMode
		{
			get { return RCG_TransportMode; }
			set { RCG_TransportMode = value; }
		}

		ZPropertyInfo IRatingDocRollupOrSort.TransportModeInfo => RCG_TransportModeInfo;

		public ZString Display
		{
			get { return RCG_Display; }
			set { RCG_Display = value; }
		}

		ZPropertyInfo IRatingDocRollupOrSort.DisplayInfo => RCG_DisplayInfo;

		public ZString Style
		{
			get { return RCG_Style; }
			set { RCG_Style = value; }
		}

		ZPropertyInfo IRatingDocRollupOrSort.StyleInfo => RCG_StyleInfo;

		#endregion

		#region RCG_Module

		[List("Lookups.ModuleList")]
		public override ZString RCG_Module
		{
			get
			{
				return base.RCG_Module;
			}
			set
			{
				base.RCG_Module = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateRCG_JobType();
					Validation.ValidateRCG_TransportMode();
				}
			}
		}

		#endregion

		#region RCG_JobType

		[List("Lookups.JobTypeList")]
		public override ZString RCG_JobType
		{
			get
			{
				return base.RCG_JobType;
			}
			set
			{
				base.RCG_JobType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateRCG_Module();
					Validation.ValidateRCG_TransportMode();
				}
			}
		}

		#endregion

		#region RCG_TransportMode

		[List("Lookups.TransportModeList")]
		public override ZString RCG_TransportMode
		{
			get
			{
				return base.RCG_TransportMode;
			}
			set
			{
				base.RCG_TransportMode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateRCG_JobType();
					Validation.ValidateRCG_Module();
				}
			}
		}

		#endregion

		#region RCG_Display

		public const string DisplayDefaultCode = "DEF";

		[List("Lookups.DisplayList")]
		public override ZString RCG_Display
		{
			get
			{
				return base.RCG_Display;
			}
			set
			{
				base.RCG_Display = value;
			}
		}

		#endregion

		#region RCG_Style

		public const string StyleDefaultCode = "DEF";

		[List("Lookups.StyleList")]
		public override ZString RCG_Style
		{
			get
			{
				return base.RCG_Style;
			}
			set
			{
				base.RCG_Style = value;
			}
		}

		#endregion

		internal OrgRatingDocRollupOrGroupHelper Helper => helper ??= new OrgRatingDocRollupOrGroupHelper(this);
		OrgRatingDocRollupOrGroupHelper helper;

		public IBusinessObjectCollection ParentCollection => CompanyData != null
			? CompanyData.RatingDocRollupOrGroups
			: null;
	}
}
