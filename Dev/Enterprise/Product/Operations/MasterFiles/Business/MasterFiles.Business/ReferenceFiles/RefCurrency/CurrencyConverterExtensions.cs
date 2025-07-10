namespace Enterprise.MasterFiles.Business
{
	using System;

	public static class CurrencyConverterExtensions
	{
		public static IJobExRateCurrencyConverter ToExRateCurrencyConverter(this CurrencyConverter currencyConverter)
		{
			var exRateCurrencyConverter = currencyConverter as IJobExRateCurrencyConverter;

			if (currencyConverter != null && exRateCurrencyConverter == null)
			{
				throw new NotSupportedException($"{currencyConverter.GetType().Name} does not support IJobExRateCurrencyConverter.");
			}

			return exRateCurrencyConverter;
		}
	}
}
