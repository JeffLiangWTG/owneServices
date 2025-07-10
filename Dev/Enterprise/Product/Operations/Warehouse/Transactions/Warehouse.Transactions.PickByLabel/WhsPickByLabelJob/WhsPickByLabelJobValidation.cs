//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPickByLabelJobValidation
//
//    This class should be used for overriding validation in AutoWhsPickByLabelJobValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public class WhsPickByLabelJobValidation : AutoWhsPickByLabelJobValidation
	{
		public WhsPickByLabelJobValidation(AutoWhsPickByLabelJob parent) : base(parent)
		{
		}

		string GetLabelsWithDifferentDDL()
		{
			var parent = Parent as WhsPickByLabelJob;
			return string.Join(",", parent.Labels.Cast<WhsPickByLabelLabel>().Where(l => !l.DockDoorLocationPK.Equals(parent.WTK_WL_DockDoor)).Select(l => l.Package.KP_PackageID));
		}

		void ValidateLabelsDockDoorLocations()
		{
			var labelsWithDifferentDDL = GetLabelsWithDifferentDDL();
			if (!string.IsNullOrEmpty(labelsWithDifferentDDL))
			{
				Parent.AddRowError(Res.GetString("0c23634f-0ba3-47f5-8716-ad25d7b24471", "Label or labels [ '{0}' ] are not assigned to dock door location '{1}', please contact Support.", labelsWithDifferentDDL, (Parent as WhsPickByLabelJob).DockDoorLocation.WLV_LocationString));
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLabelsDockDoorLocations();
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(
				WhsPickByLabelJobSchema.Constants.WTK_P9_Task);
	}
}
// Add tests to PickByLabel.Testing project.
