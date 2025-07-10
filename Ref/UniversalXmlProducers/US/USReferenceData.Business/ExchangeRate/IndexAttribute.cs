using System;

namespace CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate
{
	public sealed class IndexAttribute : Attribute
	{
		public IndexAttribute(string header, int columnIndex)
		{
			Header = header;
			ColumnIndex = columnIndex;
		}

		public string Header { get; }
		public int ColumnIndex { get; }
	}
}
