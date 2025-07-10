using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using Moq;

namespace CargoWise.RefDbRepo.IEReferenceData.Tests
{
	internal static class TestHelper
	{
		internal static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			var assembly = Assembly.GetExecutingAssembly();
			var names = assembly.GetManifestResourceNames();
			var resourceName = names.FirstOrDefault(name => name == resourceDetails) ?? names.First(name => name.EndsWith(resourceDetails));

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}

			return result;
		}

		public static Mock<IApplicationConfig> GetBaseMock()
		{
			var mock = new Mock<IApplicationConfig>();

			foreach (var propertyInfo in typeof(IApplicationConfig).GetProperties())
			{
				if (propertyInfo.PropertyType == typeof(bool))
				{
					mock.SetupGet(GetPropertyExpression<IApplicationConfig, bool>(propertyInfo.Name))
						.Returns((bool)typeof(ApplicationConfig).GetProperty(propertyInfo.Name).GetValue(ApplicationConfig.Instance));
				}
				else
				{
					mock.SetupGet(GetPropertyExpression<IApplicationConfig, object>(propertyInfo.Name))
						.Returns(typeof(ApplicationConfig).GetProperty(propertyInfo.Name).GetValue(ApplicationConfig.Instance));
				}
			}

			return mock;
		}

		static Expression<Func<T, TResult>> GetPropertyExpression<T, TResult>(string propertyName)
		{
			var parameter = Expression.Parameter(typeof(T), "x");
			var property = Expression.Property(parameter, propertyName);
			var lambda = Expression.Lambda<Func<T, TResult>>(property, parameter);

			return lambda;
		}

		public static Mock<IApplicationConfig> SetupTariffTestInputPaths(this Mock<IApplicationConfig> mock)
		{
			var directory = GetRunningDirectory();
			mock.Setup(config => config.ExciseDuty_Url_Mineral_Oil).Returns(Path.Combine(directory, ExciseDutyTariffTestInputPath, "Mineral Oil Tax.html"));
			mock.Setup(config => config.ExciseDuty_Url_Alcohol_Products).Returns(Path.Combine(directory, ExciseDutyTariffTestInputPath, "Alcohol Products Tax.html"));
			mock.Setup(config => config.ExciseDuty_Url_Tobacco_Products).Returns(Path.Combine(directory, ExciseDutyTariffTestInputPath, "Tobacco Products Tax.html"));
			return mock;
		}
		public const string ExciseDutyTariffTestInputPath = "Tariffs\\TestFiles\\Input";

		public static string GetRunningDirectory() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public static string RuntimeDataPath { get; } = GetRuntimeDataPath();

		static string GetRuntimeDataPath()
		{
			var executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
			var directoryPath = Path.GetDirectoryName(executingAssemblyLocation);
			const string logFileName = "CargoWise.RefDbRepo.IEReferenceData.Services.RuntimeData.json";
			return Path.Combine(directoryPath, logFileName);
		}

		public static void ClearRuntimeData()
		{
			if (File.Exists(RuntimeDataPath))
			{
				File.Delete(RuntimeDataPath);
			}
		}
	}
}
