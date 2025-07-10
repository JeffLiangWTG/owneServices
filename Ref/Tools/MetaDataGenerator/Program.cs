namespace CargoWise.RefDbRepo.MetaDataGenerator;

static class Program
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	static void Main()
	{
		try
		{
			GenerateMetaData();
		}
		catch (Exception ex)
		{
			LogErrorMessage(ex);
		}
	}

	static void GenerateMetaData()
	{
		var generator = new RemoteDbMetaDataGenerator(ApplicationConfig.ConnectionString);
		generator.Run();
	}

	static void LogErrorMessage(Exception ex)
	{
		Console.Error.WriteLine($"Error generating MetaData: {ex.Message}");
	}
}
