using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.US.InBond.Business
{
	public class USInBondMoveHeaderDocumentSupporter : CusInBondHeaderDocumentSupporter
	{
		public USInBondMoveHeaderDocumentSupporter(USInBondMoveHeader moveHeader)
			: base(moveHeader.Header)
		{
			this.moveHeader = moveHeader;
		}
		readonly USInBondMoveHeader moveHeader;

		protected override IBODocDataProvider[] GetBODocDataFor7512Print()
		{
			var result = new List<IBODocDataProvider>();

			if (moveHeader.MoveHeader is CusInBondMoveHeader movement)
			{
				result.Add(BODocDataProvider.Get(new CBP7512Document(movement)));
			}

			return result.ToArray();
		}

		protected override CusInBondMoveHeader GetMovementForShippingLine()
		{
			return moveHeader?.MoveHeader;
		}
	}
}
