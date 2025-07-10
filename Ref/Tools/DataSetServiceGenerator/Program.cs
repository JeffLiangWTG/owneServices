using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Tools.Common;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.DataSetServiceGenerator
{
	static class Program
	{
		static void Main(string[] args)
		{
			var safeConnectionString = ApplicationConfig.SafeConnectionString;
			var servicePath = ApplicationConfig.DataSetServicePath;

			using (var connection = new SqlConnection(safeConnectionString))
			{
				connection.Open();
				var dataSetHelper = new DataSetHelper(connection);
				var dataSetsList = DataSetStructureProvider.StructuredDataSets;
				var serviceGenerator = new DataSetServiceGenerator(dataSetsList, dataSetHelper);
				serviceGenerator.CreateDataSetServices(servicePath);
			}
		}
	}
}
