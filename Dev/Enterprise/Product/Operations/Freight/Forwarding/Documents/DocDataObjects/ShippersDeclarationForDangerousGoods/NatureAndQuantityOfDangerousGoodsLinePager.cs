using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class NatureAndQuantityOfDangerousGoodsLinePager
	{
		public NatureAndQuantityOfDangerousGoodsLinePager(int natureAndQuantityLinesPerPage)
		{
			this.natureAndQuantityLinesPerPage = natureAndQuantityLinesPerPage > 0
				? natureAndQuantityLinesPerPage
				: throw new ArgumentOutOfRangeException(nameof(natureAndQuantityLinesPerPage));

			this.lines = new List<NatureAndQuantityOfDangerousGoodsLine>();
		}

		readonly int natureAndQuantityLinesPerPage = 10;
		readonly List<NatureAndQuantityOfDangerousGoodsLine> lines;

		public void Add(NatureAndQuantityOfDangerousGoodsLine line)
		{
			if (line != null)
			{
				lines.Add(line);
			}
		}

		public NatureAndQuantityOfDangerousGoodsLine[][] GetLinesPerPage()
		{
			var res = new List<NatureAndQuantityOfDangerousGoodsLine[]>();
			var page = new List<NatureAndQuantityOfDangerousGoodsLine>();

			foreach (var line in lines)
			{
				page.Add(line);

				if (page.Count == natureAndQuantityLinesPerPage)
				{
					res.Add(page.ToArray());
					page.Clear();
				}
			}

			for (var i = page.Count; i < natureAndQuantityLinesPerPage; i++)
			{
				page.Add(new NatureAndQuantityOfDangerousGoodsLine(NatureAndQuantityOfDangerousGoodsLineType.Dummy));
			}

			if (page.Count > 0)
			{
				res.Add(page.ToArray());
			}

			return res.ToArray();
		}
	}
}
