using System;

namespace Enterprise.Customs.TW.Business
{
	public static class FuncExtensions
	{
		public static Func<T, bool> And<T>(this Func<T, bool> left, Func<T, bool> right) => a => left(a) && right(a);
	}
}
