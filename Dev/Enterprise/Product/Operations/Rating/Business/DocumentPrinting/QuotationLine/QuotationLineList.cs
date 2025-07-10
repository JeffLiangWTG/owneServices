namespace Enterprise.Rating.Business
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Enterprise.ResourceStrings.Business;

	[CodeStringFinderSupportedReturnType]
	public class QuotationLineList : IList<QuotationLine>, IEquatable<QuotationLineList>
	{
		readonly List<QuotationLine> lines = new List<QuotationLine>();

		public int IndexOf(QuotationLine line) => lines.IndexOf(line);

		public void Insert(int index, QuotationLine line)
		{
			lines.Insert(index, line);
		}

		public void RemoveAt(int index)
		{
			lines.RemoveAt(index);
		}

		public QuotationLine this[int index]
		{
			get { return lines[index]; }
			set { lines[index] = value; }
		}

		/// <summary>
		/// Adding a NULL line will be ignored.
		/// </summary>
		/// <param name="line"></param>
		public void Add(QuotationLine line)
		{
			if (line != null)
			{
				lines.Add(line);
			}
		}

		public void AddRange(IEnumerable<QuotationLine> collection)
		{
			lines.AddRange(collection);
		}

		public void Clear()
		{
			lines.Clear();
		}

		public bool Contains(QuotationLine line) => lines.Contains(line);

		public void CopyTo(QuotationLine[] array, int arrayIndex)
		{
			lines.CopyTo(array, arrayIndex);
		}

		public int Count => lines.Count;

		public bool IsReadOnly => false;

		public bool Remove(QuotationLine line) => lines.Remove(line);

		public IEnumerator<QuotationLine> GetEnumerator() => lines.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => lines.GetEnumerator();

		public override bool Equals(object obj) => this.Equals((QuotationLineList)obj);

		public bool Equals(QuotationLineList other)
		{
			if (other == null || Count != other.Count)
			{
				return false;
			}

			for (var index = 0; index < Count; index++)
			{
				if (!this[index].Equals(other[index]))
				{
					return false;
				}
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
		public override int GetHashCode() => base.GetHashCode();
	}
}
