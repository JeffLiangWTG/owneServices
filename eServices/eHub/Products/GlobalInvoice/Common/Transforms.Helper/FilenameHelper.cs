using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Products.GlobalInvoice.Common.Transforms.Helper
{
	public class FilenameHelper
	{
		public virtual string Base36Encoding(int input, int padLeft)
		{
			if (input < 0) throw new ArgumentOutOfRangeException("input cannot be negative: " + input);

			var result = new Stack<char>();
			while (input != 0)
			{
				result.Push(Base36CharList[input % Base36CharList.Length]);
				input /= Base36CharList.Length;
			}
			return new string(result.ToArray()).PadLeft(padLeft, '0');
		}

		private static readonly char[] Base36CharList = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
	}
}
