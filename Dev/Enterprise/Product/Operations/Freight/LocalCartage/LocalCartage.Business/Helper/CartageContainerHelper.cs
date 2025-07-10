using System;
using System.Linq;

using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageContainerHelper : NonPersistentBusinessObject, Integration.ICartageContainerHelper
	{
		public CartageContainerHelper(BusinessObjectFactory factory, CommonContainer parentContainer)
			: base(factory)
		{
			this.container = parentContainer;
		}

		readonly CommonContainer container;

		public CommonCartage FirstCartage
		{
			get
			{
				var move = Array.Find(GetAllBookedMovesForContainer, p => p.Cartage != null);
				return move != null ? move.Cartage : null;
			}
		}

		public CommonCartage DestinationCartage
		{
			get
			{
				var move = Array.Find(GetAllBookedMovesForContainer, p => p.Cartage != null && p.Cartage.IsImportOrDestination);
				return move != null ? move.Cartage : null;
			}
		}

		CommonBookedCtgMove[] GetAllBookedMovesForContainer
		{
			get { return Factory.Load<CommonBookedCtgMove>(new ZQuery(JobBookedCtgMoveSchema.EW_JC_Container, container.PK)); }
		}

		public void DeleteAllBookedMoves()
		{
			var allMoves = GetAllMoves();
			allMoves.DeleteAll();
		}

		public void DeleteAllBookedMovesForDataRefresh()
		{
			var allMoves = GetAllMoves();
			using (((IBusinessObjectCollection)allMoves).SuspendListChanged())
			{
				foreach (var move in allMoves.Where(move => !move.IsInDatabase).ToArray())
				{
					((IBusiness)move).DeleteForDataRefresh();
				}
			}
		}

		CommonBookedCtgMoveCollection GetAllMoves()
		{
			return new CommonBookedCtgMoveCollection(Factory, new DependentRelationship(container, typeof(CommonBookedCtgMove), null, JobBookedCtgMoveSchema.EW_JC_Container));
		}

		void Integration.ICartageContainerHelper.DeleteAllBookedMoves(bool forDataRefresh)
		{
			if (forDataRefresh)
			{
				DeleteAllBookedMovesForDataRefresh();
			}
			else
			{
				DeleteAllBookedMoves();
			}
		}

		Integration.ICommonCartage Integration.ICartageContainerHelper.DestinationCartage
		{
			get { return DestinationCartage; }
		}
	}
}
