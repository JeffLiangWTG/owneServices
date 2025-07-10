using System;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

static class ZStringExtensions
{
	public static ZString SubStringSafeWithStartAndEndIndex(this ZString source, int startCharIndex, int endCharIndex)
	{
		if (startCharIndex < 0 || endCharIndex < 0)
		{
			throw new ArgumentException("StartCharIndex and EndCharIndex cannot be negative");
		}

		if (startCharIndex > endCharIndex)
		{
			throw new ArgumentException("StartCharIndex cannot be greater than EndCharIndex.");
		}

		return source.SubstringSafe(startCharIndex, (endCharIndex + 1) - startCharIndex);
	}
}
