using System;
using System.Xml.Serialization;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.Business
{
	[Serializable]
	public class DeduplicationPresenterModel
	{
		public ConfidenceRating Confidence { get; set; }

		[XmlIgnore]
		public Type GlowTargetType { get; set; }

		[XmlIgnore]
		public Type MasterType { get; set; }
		public object MasterValue { get; set; }
		public object TargetValue { get; set; }

		public string GroupNameForType { get; set; }
		public Guid TargetID { get; set; }
		public string Master { get; set; }
		public string Target { get; set; }
		public double MainScore { get; set; }

		public Guid ChildTargetID { get; set; }
		public Guid ChildMasterID { get; set; }
		public ConfidenceRating ChildScoringResultGroupRating { get; set; }

		public string ChildGroupNameForType { get; set; }
		public DeduplicationDisplayMode ChildDisplayModeForType { get; set; }
		public ConfidenceRating ChildConfidenceRating { get; set; }
		public double ChildScore { get; set; }
		public string ChildMasterValue { get; set; }
		public string ChildTargetValue { get; set; }
		public double ChildComparisonResultScore { get; set; }
		public string ChildDisplayNameForMasterColumns { get; set; }
		public string ChildDisplayNameForTargetColumns { get; set; }
	}
}
