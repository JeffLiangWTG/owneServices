using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class ActualEvent : AutoActualEvent
	{
		#region ArrivedAndUnloadedPieces

		public ZString ArrivedAndUnloadedPieces => UnloadedPieces > 0
			? ZString.Format("{0}/{1}", ArrivedPieces, UnloadedPieces)
			: (ZString)ArrivedPieces.ToString();

		public ZPropertyInfo ArrivedAndUnloadedPiecesInfo => GetZPropertyInfo(nameof(ArrivedAndUnloadedPieces));

		#endregion
	}
}
