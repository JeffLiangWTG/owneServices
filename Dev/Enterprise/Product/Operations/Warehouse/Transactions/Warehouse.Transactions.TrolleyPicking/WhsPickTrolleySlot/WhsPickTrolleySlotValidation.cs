//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPickTrolleySlotValidation
//
//    This class should be used for overriding validation in AutoWhsPickTrolleySlotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	using CargoWise.EntityFramework;
	using ZArchitecture.Schema;

	public class WhsPickTrolleySlotValidation : AutoWhsPickTrolleySlotValidation
	{
		public WhsPickTrolleySlotValidation(AutoWhsPickTrolleySlot parent) : base(parent)
		{
		}

		protected override void CheckWTS_KP_Package()
		{
			base.CheckWTS_KP_Package();

			if (!IsPackageUnique)
			{
				Parent.WTS_KP_PackageInfo.AddError(Res.GetString("6ad93321-ab36-46b9-85dc-7194bbd0d392", "Package should be unique"));
			}
		}

		bool IsPackageUnique
		{
			get
			{
				var query = new ZQuery(WhsPickTrolleySlotSchema.WTS_KP_Package, Parent.WTS_KP_Package);
				query.AddToFilter(WhsPickTrolleySlotSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				return Parent.Factory.LoadTop1<WhsPickTrolleySlot>(query) == null;
			}
		}
	}
}

// Add tests to TrolleyPicking.Testing project.
