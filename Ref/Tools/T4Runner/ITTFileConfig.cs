namespace CargoWise.RefDbRepo.T4Runner
{
	public interface ITTFileConfig
	{
		string DirectoryPath { get; set; }
		string NamespaceName { get; set; }
		string FileName { get; set; }

		void CallProcessor(string namespaceName, string ttFile);
	}
}
