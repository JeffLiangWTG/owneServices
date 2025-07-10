using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.Common;

namespace Enterprise.Freight.Forwarding.AWB.TNT
{
	public class RowReader<T> : Disposable where T : RowData, new()
	{
		public RowReader(Stream rowSource)
		{
			this.reader = new StreamReader(rowSource, Encoding.Unicode, true, 8192);
		}
		readonly StreamReader reader;

		public IEnumerable<T> Rows
		{
			get
			{
				do
				{
					string oneLine = reader.ReadLine();
					if (!string.IsNullOrEmpty(oneLine))
					{
						var result = new T();
						result.ReadRow(oneLine);
						yield return result;
					}
				}
				while (!reader.EndOfStream);

				yield break;
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing && reader != null)
			{
				reader.Dispose();
			}
		}
	}
}
