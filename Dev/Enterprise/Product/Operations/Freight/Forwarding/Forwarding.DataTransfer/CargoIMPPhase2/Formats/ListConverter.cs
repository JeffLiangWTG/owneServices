using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	[Immutable]
	abstract class ListConverter<TSource, TResult>
	{
		public abstract TResult Convert(TSource data, FormattingResult formattingResult);
	}
}
