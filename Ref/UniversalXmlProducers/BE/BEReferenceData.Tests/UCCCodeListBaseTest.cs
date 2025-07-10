using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class UCCCodeListBaseTest : UCCodeListAbstractTest
	{
		protected override string codeType => "UCCCodeListBase";

		protected override string dataSource => "BE Additional Info";
	}
}
