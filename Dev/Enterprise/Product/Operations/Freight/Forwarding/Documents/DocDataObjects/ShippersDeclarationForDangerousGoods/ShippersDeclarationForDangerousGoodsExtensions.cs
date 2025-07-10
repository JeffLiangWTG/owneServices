using System.Collections.Generic;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class ShippersDeclarationForDangerousGoodsExtensions
	{
		public static IEnumerable<NatureAndQuantityOfDangerousGoodsLine> AsEnumerable(this ShippersDeclarationForDangerousGoodsDetail decl)
		{
			if (decl == null)
			{
				yield break;
			}

			yield return decl.NatureAndQuantity1;
			yield return decl.NatureAndQuantity2;
			yield return decl.NatureAndQuantity3;
			yield return decl.NatureAndQuantity4;
			yield return decl.NatureAndQuantity5;
			yield return decl.NatureAndQuantity6;
			yield return decl.NatureAndQuantity7;
			yield return decl.NatureAndQuantity8;
			yield return decl.NatureAndQuantity9;
			yield return decl.NatureAndQuantity10;
		}
	}
}
