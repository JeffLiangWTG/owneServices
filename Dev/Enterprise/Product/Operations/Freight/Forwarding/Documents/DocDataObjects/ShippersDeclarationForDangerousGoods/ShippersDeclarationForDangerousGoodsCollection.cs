using System;
using System.Collections;
using System.Collections.Generic;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ShippersDeclarationForDangerousGoodsCollection : IReadOnlyCollection<ShippersDeclarationForDangerousGoodsDetail>
	{
		public ShippersDeclarationForDangerousGoodsCollection(ShippersDeclarationForDangerousGoodsDetail[] shippersDeclarationForDangerousGoods)
		{
			this.shippersDeclarationForDangerousGoods = shippersDeclarationForDangerousGoods ?? throw new ArgumentNullException(nameof(shippersDeclarationForDangerousGoods));
		}

		readonly ShippersDeclarationForDangerousGoodsDetail[] shippersDeclarationForDangerousGoods;

		public IEnumerator<ShippersDeclarationForDangerousGoodsDetail> GetEnumerator() => ((IEnumerable<ShippersDeclarationForDangerousGoodsDetail>)shippersDeclarationForDangerousGoods).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count => shippersDeclarationForDangerousGoods.Length;
	}
}
