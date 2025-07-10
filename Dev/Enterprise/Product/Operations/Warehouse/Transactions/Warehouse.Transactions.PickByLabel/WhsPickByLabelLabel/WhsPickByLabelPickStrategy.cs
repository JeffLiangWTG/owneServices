using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public class WhsPickByLabelPickStrategy : IWhsPickByLabelPickStrategy
	{
		public void OnFinalised(BusinessObjectFactory factory, IWhsPick pick)
		{
			if (pick.WP_PickStatus == PickStatus.Codes.Finalised)
			{
				var pickBO = pick as WhsPick;
				var query = @"
	SELECT
		WTK_PK
	FROM
		dbo.WhsDocket
		JOIN dbo.WhsDocketLine ON WD_PK = WE_WD
		JOIN dbo.WhsPickLine ON WE_PK = WZ_WE_TransactionLine
		JOIN dbo.PkgPackageItemDivot ON WZ_PK = KI_ParentID
		JOIN dbo.WhsPickByLabelLabel ON KI_KP_Package = WTL_KP_Package
		JOIN dbo.WhsPickByLabelJob ON WTL_WTK_PickByLabelJob = WTK_PK
	WHERE
		WTK_FinalisedDate IS NULL AND
		WD_WP = @PickPK
";
				var parameter = new ZSqlParameterCollection();
				parameter.Add("@PickPK", pickBO.PK, WhsDocketSchema.WD_WP);

				var pickByLabeljobQuery = new ZDBOnlyQuery(typeof(WhsPickByLabelJob));
				pickByLabeljobQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsPickByLabelJobSchema.PK.Name, query), parameter);

				var pickByLabelJobs = factory.Load<WhsPickByLabelJob>(pickByLabeljobQuery);
				foreach (var pickByLabelJob in pickByLabelJobs)
				{
					WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(pickByLabelJob);
				}
			}
		}

		public bool IsOrderActionAllowed(IWhsOrder order, PickOrderAction action, out ErrorNotification reasonNotAllowed)
		{
			var isOrderAssociatedWithPickByLabelJob = IsOrderAssociatedWithPickByLabelJob(order);
			switch (action)
			{
				case PickOrderAction.DetachOrder when isOrderAssociatedWithPickByLabelJob:
				case PickOrderAction.AttachOrder when isOrderAssociatedWithPickByLabelJob:
					reasonNotAllowed = new ErrorNotification(OrderErrorTypes.CannotPerformThisOperationBecauseOrderHasPackagesAssignedToPickByLabelJob);
					return false;
				default:
					reasonNotAllowed = null;
					return true;
			}
		}

		public bool IsOrderAssociatedWithPickByLabelJob(IWhsOrder order)
		{
			return ((WhsOrder)order)?.HasAnyPackageAssignedToAPickByLabelJob() ?? false;
		}
	}
}
