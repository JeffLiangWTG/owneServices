using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IHugeSequenceNumberLine : ISequenceNumberLine<ZInt>
	{
	}

	public interface IShortSequenceNumberLine : ISequenceNumberLine<ZShort>
	{
	}

	public interface ISequenceNumberLine<T> : ISequenceNumberLine where T : INumericZType
	{
		T SequenceNumber { get; set; }
	}

	public interface ISequenceNumberLine
	{
		ZGuid FKToHeader { get; }
	}

	public interface ISequenceNumberHeader
	{
		IEnumerable<ISequenceNumberLine> Lines { get; }
	}

	public class HugeSequenceNumberGenerator : BaseSequenceNumberGenerator<ZInt, IHugeSequenceNumberLine>
	{
		public HugeSequenceNumberGenerator(ISequenceNumberHeader header)
			: base(header)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1006")]
		public HugeSequenceNumberGenerator(Func<IEnumerable<IHugeSequenceNumberLine>> getLines)
			: base(getLines)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1006")]
		public HugeSequenceNumberGenerator(ISequenceNumberHeader header, Func<IHugeSequenceNumberLine, ZBool> isLineBelongToThisDelegate)
			: base(header, isLineBelongToThisDelegate)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1006")]
		public HugeSequenceNumberGenerator(Func<IEnumerable<IHugeSequenceNumberLine>> getLines, Func<IHugeSequenceNumberLine, ZBool> isLineBelongToThisDelegate)
			: base(getLines, isLineBelongToThisDelegate)
		{
		}

		protected override int SequenceMaxNumberCore => int.MaxValue;

		protected override ZInt ConvertValue(int value)
		{
			return EnsureValidSequenceNumber(value);
		}

		public static int EnsureValidSequenceNumber(long numberToAssign)
		{
			return numberToAssign > int.MaxValue || numberToAssign < 0 ? 0 : (int)numberToAssign;
		}
	}

	public class ShortSequenceNumberGenerator : BaseSequenceNumberGenerator<ZShort, IShortSequenceNumberLine>
	{
		public ShortSequenceNumberGenerator(ISequenceNumberHeader header)
			: base(header)
		{
		}

		public ShortSequenceNumberGenerator(ISequenceNumberHeader header, Func<int> getSequenceStartingNumber, Func<int> getSequenceMaxNumber)
			: base(header, getSequenceStartingNumber, getSequenceMaxNumber)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1006")]
		public ShortSequenceNumberGenerator(Func<IEnumerable<IShortSequenceNumberLine>> getLines)
			: base(getLines)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1006")]
		public ShortSequenceNumberGenerator(ISequenceNumberHeader header, Func<IShortSequenceNumberLine, ZBool> isLineBelongToThisDelegate)
			: base(header, isLineBelongToThisDelegate)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1006")]
		public ShortSequenceNumberGenerator(Func<IEnumerable<IShortSequenceNumberLine>> getLines, Func<IShortSequenceNumberLine, ZBool> isLineBelongToThisDelegate)
			: base(getLines, isLineBelongToThisDelegate)
		{
		}

		protected override int SequenceMaxNumberCore => short.MaxValue;

		protected override ZShort ConvertValue(int value)
		{
			return EnsureValidSequenceNumber(value, SequenceMaxNumber);
		}

		public static short EnsureValidSequenceNumber(int numberToAssign, int maxValue = short.MaxValue)
		{
			return numberToAssign > maxValue || numberToAssign < 0 ? (short)0 : (short)numberToAssign;
		}
	}

	public abstract class BaseSequenceNumberGenerator<T, U>
		where T : INumericZType
		where U : ISequenceNumberLine<T>
	{
		protected BaseSequenceNumberGenerator(ISequenceNumberHeader header)
			: this(header, (x) => { return true; })
		{
		}

		protected BaseSequenceNumberGenerator(ISequenceNumberHeader header, Func<int> getSequenceStartingNumber, Func<int> getSequenceMaxNumber)
			: this(header)
		{
			this.getSequenceStartingNumber = getSequenceStartingNumber;
			this.getSequenceMaxNumber = getSequenceMaxNumber;
		}
		readonly Func<int> getSequenceStartingNumber;
		readonly Func<int> getSequenceMaxNumber;

		[SuppressMessage("Microsoft.Design", "CA1006")]
		protected BaseSequenceNumberGenerator(ISequenceNumberHeader header, Func<U, ZBool> isLineBelongToThisDelegate)
			: this(() => header.Lines.Cast<U>(), isLineBelongToThisDelegate)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1006")]
		protected BaseSequenceNumberGenerator(Func<IEnumerable<U>> getLines)
			: this(getLines, (x) => { return true; })
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1006")]
		protected BaseSequenceNumberGenerator(Func<IEnumerable<U>> getLines, Func<U, ZBool> isLineBelongToThisDelegate)
		{
			this.getLines = getLines;
			this.isLineBelongToThisDelegate = isLineBelongToThisDelegate;
		}

		readonly Func<IEnumerable<U>> getLines;
		readonly Func<U, ZBool> isLineBelongToThisDelegate;
		int index;

		protected IEnumerable<U> Lines
		{
			get { return getLines().Where(x => isLineBelongToThisDelegate(x)); }
		}

		public void RecalculateWhenAboutToBeDetachedOrDeleted(U lineToBeDeleted)
		{
			if (!IsSuspended)
			{
				RecalculateWhenAboutToBeDetachedOrDeletedCore(lineToBeDeleted);
			}
		}

		protected virtual void RecalculateWhenAboutToBeDetachedOrDeletedCore(U lineToBeDeleted)
		{
			using (new SequenceNumberSuspender(this))
			{
				var reusableSequenceNumber = lineToBeDeleted.SequenceNumber.ToZInt();

				if (reusableSequenceNumber >= SequenceStartingNumber)
				{
					foreach (var line in Lines)
					{
						var safeValue = line.SequenceNumber.ToZInt();

						if (safeValue > reusableSequenceNumber)
						{
							line.SequenceNumber = ConvertValue(safeValue - 1);
						}
					}
				}
			}
		}

		public void ReCalculateAll()
		{
			if (!IsSuspended)
			{
				ReCalculateAllCore();
			}
		}

		protected virtual IEnumerable<U> OrderedLinesForReCalculateAll => Lines.OrderBy(x => x.SequenceNumber);

		protected virtual void ReCalculateAllCore()
		{
			using (new SequenceNumberSuspender(this))
			{
				var sequence = SequenceStartingNumber;
				var maximized = false;
				foreach (var line in OrderedLinesForReCalculateAll)
				{
					if (maximized)
					{
						line.SequenceNumber = ConvertValue(0);
					}
					else
					{
						line.SequenceNumber = ConvertValue(sequence);
						if (sequence < SequenceMaxNumber)
						{
							sequence++;
						}
						else
						{
							maximized = true;
						}
					}
				}
			}
		}

		public int SequenceStartingNumber => getSequenceStartingNumber?.Invoke() ?? SequenceStartingNumberCore;

		protected virtual int SequenceStartingNumberCore => 1;

		public void RecalculateWhenAdded(U lineToAdd)
		{
			if (!IsSuspended)
			{
				RecalculateWhenAddedCore(lineToAdd);
			}
		}

		protected virtual void RecalculateWhenAddedCore(U lineToAdd)
		{
			using (new SequenceNumberSuspender(this))
			{
				var linesCount = Lines.Count();
				var count = Lines.Contains(lineToAdd) ? linesCount : linesCount + 1;

				lineToAdd.SequenceNumber = ConvertValue(SequenceStartingNumber + count - 1);
			}
		}

		public void RecalculateWhenRenumbered(U lineBeingRenumbered, T oldValue)
		{
			if (!IsSuspended)
			{
				RecalculateWhenRenumberedCore(lineBeingRenumbered, oldValue);
			}
		}

		protected virtual void RecalculateWhenRenumberedCore(U lineBeingRenumbered, T oldValue)
		{
			using (new SequenceNumberSuspender(this))
			{
				var lines = new List<U>(Lines);
				var totalLines = lines.Contains(lineBeingRenumbered) ? lines.Count : lines.Count + 1;
				var maxLineSequenceNumber = totalLines + SequenceStartingNumber - 1;
				var safeValue = lineBeingRenumbered.SequenceNumber.ToZInt();

				//if users assigned 5 to the last fourth line, it should assign 4 instead when SequenceStartingNumber is 1.
				var newNumber = safeValue > maxLineSequenceNumber ? ConvertValue(maxLineSequenceNumber) : ConvertValue(safeValue);
				var safeNewNumber = newNumber.ToZInt();

				if (safeValue != safeNewNumber)
				{
					lineBeingRenumbered.SequenceNumber = newNumber;
				}

				if (IsDuplicateLineNumber(lineBeingRenumbered, newNumber) && safeNewNumber > 0)
				{
					//work the line number range that should be fixed
					var safeOldValue = oldValue.ToZInt();
					var startPosition = Math.Min(safeOldValue, safeNewNumber);
					var endPosition = Math.Max(safeOldValue, safeNewNumber);
					var offset = CalculateOffset(safeOldValue, safeNewNumber);

					foreach (var line in lines)
					{
						var safeSequenceNumber = line.SequenceNumber.ToZInt();

						if (!line.Equals(lineBeingRenumbered) && safeSequenceNumber >= startPosition && safeSequenceNumber <= endPosition)
						{
							line.SequenceNumber = ConvertValue(safeSequenceNumber + offset);
						}
					}
				}
			}
		}

		public IDisposable GetLineNumberSuspender()
		{
			return new SequenceNumberSuspender(this);
		}

		#region Implementation

		internal bool IsDuplicateLineNumber(U lineBeingRenumbered, T newNumber)
		{
			bool result = false;

			foreach (U line in Lines)
			{
				if (!(line?.Equals(lineBeingRenumbered) ?? false) && line.SequenceNumber.Equals(newNumber) && line.FKToHeader == lineBeingRenumbered.FKToHeader)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		protected short CalculateOffset(int oldLineNumber, int newLineNumber)
		{
			return oldLineNumber < newLineNumber ? (short)-1 : (short)1;
		}

		protected abstract T ConvertValue(int value);

		public int SequenceMaxNumber => getSequenceMaxNumber?.Invoke() ?? SequenceMaxNumberCore;

		protected abstract int SequenceMaxNumberCore { get; }

		protected bool IsSuspended
		{
			get { return index > 0; }
		}

		protected sealed class SequenceNumberSuspender : IDisposable
		{
			public SequenceNumberSuspender(BaseSequenceNumberGenerator<T, U> generator)
			{
				this.generator = generator;
				this.generator.index++;
			}

			readonly BaseSequenceNumberGenerator<T, U> generator;

			#region IDisposable Members

			public void Dispose()
			{
				generator.index--;
			}

			#endregion
		}

		#endregion
	}
}
