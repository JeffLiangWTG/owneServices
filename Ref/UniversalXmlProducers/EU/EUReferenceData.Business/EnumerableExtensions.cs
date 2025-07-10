using System.Collections.Generic;

namespace System.Linq
{
	// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Linq/src/System/Linq/Chunk.cs
	public static class EnumerableExtensions
	{
		public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
		{
			if (source == null)
			{
				throw new ArgumentException("source must not be null.");
			}

			if (size < 1)
			{
				throw new ArgumentException("chunkSize must be greater than 0.");
			}

			return ChunkIterator(source, size);
		}

		static IEnumerable<TSource[]> ChunkIterator<TSource>(IEnumerable<TSource> source, int size)
		{
			using (IEnumerator<TSource> e = source.GetEnumerator())
			{
				while (e.MoveNext())
				{
					TSource[] chunk = new TSource[size];
					chunk[0] = e.Current;

					int i = 1;
					for (; i < chunk.Length && e.MoveNext(); i++)
					{
						chunk[i] = e.Current;
					}

					if (i == chunk.Length)
					{
						yield return chunk;
					}
					else
					{
						Array.Resize(ref chunk, i);
						yield return chunk;
						yield break;
					}
				}
			}
		}
	}
}
