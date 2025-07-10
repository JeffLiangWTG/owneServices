using System.Data;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	public interface IXmlProcessIntegrationTest
	{
		string[] FileNames { get; }
		XmlProcessIntegrationTestAssertResultHandler[] AssertResults { get; }
		void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand);
		string TestDescription { get; }
	}

	public delegate void XmlProcessIntegrationTestAssertResultHandler(IDbCommand stagingCommand, IDbCommand safeCommand);
}
