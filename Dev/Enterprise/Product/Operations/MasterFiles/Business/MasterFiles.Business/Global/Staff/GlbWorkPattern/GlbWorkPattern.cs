using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbWorkPattern : AutoGlbWorkPattern
	{
		public GlbWorkPattern(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ChildEditable]
		public GlbWorkTimeCollection WorkTimes
		{
			get
			{
				if (workTimes == null)
				{
					var query = new ZQuery(GlbWorkTimeSchema.GW_ParentTableCode, GlbWorkPatternSchema.Constants.Prefix);
					query.AddToFilter(GlbWorkTimeSchema.GW_ParentID, PK);
					workTimes = new GlbWorkTimeCollection(Factory, query);

					RegisterEditableChildObject(workTimes);
				}

				return workTimes;
			}
		}
		GlbWorkTimeCollection workTimes;

		[ZDateTimeDurationValue]
		public override ZDateTime GWP_StandardDuration
		{
			get => base.GWP_StandardDuration;
			set => base.GWP_StandardDuration = value.ConvertToDurationBasedDate(GWP_StandardDurationInfo);
		}
	}
}
