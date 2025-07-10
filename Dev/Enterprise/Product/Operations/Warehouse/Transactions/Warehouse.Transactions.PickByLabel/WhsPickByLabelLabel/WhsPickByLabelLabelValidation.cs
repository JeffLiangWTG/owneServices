//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPickByLabelLabelValidation
//
//    This class should be used for overriding validation in AutoWhsPickByLabelLabelValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public class WhsPickByLabelLabelValidation : AutoWhsPickByLabelLabelValidation
	{
		public WhsPickByLabelLabelValidation(AutoWhsPickByLabelLabel parent) : base(parent)
		{
		}

		protected override void CheckWTL_KP_Package()
		{
			var pickByLabelJob = Parent.PickByLabelJob;
			if (!Parent.DockDoorLocationPK?.Equals(pickByLabelJob?.DockDoorLocation.PK) ?? true)
			{
				Parent.WTL_KP_PackageInfo.AddError(Res.GetString("0225c2fe-658d-4dd4-a4ab-84bea05e863c", "Package '{0}' is assigned to a different dock door location than previous labels.", Parent.Package?.KP_PackageID ?? string.Empty));
			}
			else if (Parent.Pick?.WP_WL_PackingStation != pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().FirstOrDefault()?.Pick?.WP_WL_PackingStation)
			{
				// we have ensured that packing station is set on new pick in GetPickByLabelPackage.cs
				Parent.WTL_KP_PackageInfo.AddError(Res.GetString("eb107948-3ccd-4942-98a1-c33838ccef37", "Package '{0}' is assigned to a different packing station location than previous labels.", Parent.Package?.KP_PackageID ?? string.Empty));
			}
			base.CheckWTL_KP_Package();
		}

		#region Implementations

		public new WhsPickByLabelLabel Parent
		{
			get { return (WhsPickByLabelLabel)base.Parent; }
		}

		#endregion
	}
}

// Add tests to PickByLabel.Testing project.
