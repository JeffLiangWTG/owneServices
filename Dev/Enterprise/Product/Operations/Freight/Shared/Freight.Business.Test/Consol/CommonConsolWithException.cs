using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolWithException : CommonConsol
	{
		public CommonConsolWithException(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public NumberFountainUniqueIndexFailureHandler NewConsolNumberFountainUniqueIndexFailureHandlerWithException()
		{
			return new ConsolNumberFountainUniqueIndexFailureHandlerWithException(this);
		}

		class ConsolNumberFountainUniqueIndexFailureHandlerWithException : ConsolNumberFountainUniqueIndexFailureHandler
		{
			public ConsolNumberFountainUniqueIndexFailureHandlerWithException(CommonConsolWithException consol)
				: base(consol)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return new BrokenNumberFountain(); }
			}
		}
	}
}
