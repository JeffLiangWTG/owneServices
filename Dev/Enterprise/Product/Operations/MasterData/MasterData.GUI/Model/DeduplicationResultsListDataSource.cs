using System.Windows.Forms;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;

namespace Enterprise.MasterData.GUI
{
	public class DeduplicationResultsListDataSource
	{
		public ZGuid PK { get; set; }
		public string FullNameCaption { get; set; }
		public string FullName { get; set; }
		public string InfoCaption { get; set; }
		public string Info { get; set; }
		public bool IsActive { get; set; }
		public string ToolTip { get; set; }
		public ConfidenceRating Confidence { get; set; }
		public double Score { get; set; }
		public string PersonType { get; set; }
		public Padding PersonTypePadding { get; set; }
		public string MainInfo { get; set; }
		public string Email { get; set; }
		public bool EmailPanelVisible { get; set; }
		public string Phone { get; set; }
		public bool PhonePanelVisible { get; set; }
		public string Associations { get; set; }
		public bool AssociationsPanelVisible => !string.IsNullOrEmpty(Associations);
	}
}
