using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eTail.ServiceTasks
{
	class NumberFountainInfo
	{
		public NumberFountainInfo(INumberFountainProxy fountain, string prefix, int formatDigits)
		{
			Fountain = Argument.NotNull(fountain, nameof(fountain));
			Prefix = prefix;
			FormatDigits = formatDigits;
		}

		public INumberFountainProxy Fountain { get; }
		public string Prefix { get; }
		public int FormatDigits { get; }
	}
}
