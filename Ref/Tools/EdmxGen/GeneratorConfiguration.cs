using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public class GeneratorConfiguration : IGeneratorConfiguration
	{
		public GeneratorConfiguration(string tempFilePath, XDocument currentEdmx, string namespaceName, string entityContainer, string connectionString)
		{
			Argument.NotNullOrEmpty(tempFilePath, nameof(tempFilePath));
			Argument.NotNull(currentEdmx, nameof(currentEdmx));
			Argument.NotNullOrEmpty(namespaceName, nameof(namespaceName));
			Argument.NotNullOrEmpty(entityContainer, nameof(entityContainer));
			Argument.NotNullOrEmpty(connectionString, nameof(connectionString));

			TempFilePath = tempFilePath;
			CurrentEDMX = currentEdmx;
			Namespace = namespaceName;
			EntityContainer = entityContainer;
			ConnectionString = connectionString;
		}

		public GeneratorConfiguration(string tempFilePath, XDocument currentEdmx, string namespaceName, string entityContainer, string connectionString, string[] parameters)
			: this(tempFilePath, currentEdmx, namespaceName, entityContainer, connectionString)
		{
			Argument.NotNullOrEmpty(tempFilePath, nameof(tempFilePath));
			Argument.NotNull(currentEdmx, nameof(currentEdmx));
			Argument.NotNullOrEmpty(namespaceName, nameof(namespaceName));
			Argument.NotNullOrEmpty(entityContainer, nameof(entityContainer));
			Argument.NotNullOrEmpty(connectionString, nameof(connectionString));
			Argument.NotNull(parameters, nameof(parameters));

			ExtraParameters = parameters;
		}

		public string TempFilePath { get; }

		public XDocument CurrentEDMX { get; }

		public string Namespace { get; }

		public string EntityContainer { get; }

		public string ConnectionString { get; }

		public string[] ExtraParameters
		{
			get
			{
				if (extraParameters == null)
				{
					extraParameters = new string[] { };
				}

				return extraParameters;
			}
			private set
			{
				extraParameters = value;
			}
		}
		string[] extraParameters;
	}
}
