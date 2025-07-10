using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	internal static class TestHelper
	{
		internal static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}

		internal static void SimulateDownload(string fileName, string resourceDetails)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var writer = new FileStream(fileName, FileMode.Create))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(writer);

				writer.Flush();
			}
		}

		internal static IDateTimeProvider MockDateTimeProvider(DateTime now)
		{
			var mock = new Mock<IDateTimeProvider>();
			mock.Setup(x => x.GetNow()).Returns(now);
			return mock.Object;
		}

		internal static bool AreEquivalent(this JToken jToken, object obj)
		{
			if (jToken == null != (obj == null))
			{
				return false;
			}
			else
			{
				switch (jToken.Type)
				{
					case JTokenType.Object:
						var jsonObject = (JObject)jToken;
						return jsonObject.Children().Cast<JProperty>().All(childProperty =>
							GetPropertyValue(obj, childProperty.Name, out var childPropertyValue) && AreEquivalent(childProperty, childPropertyValue)
						);
					case JTokenType.Property:
						var jsonProperty = (JProperty)jToken;
						return AreEquivalent(jsonProperty.Value, obj);
					case JTokenType.Array:
						if (obj is IEnumerable objEnumerable)
						{
							var objs = objEnumerable.Cast<object>().ToList();
							var arrayProperty = (JArray)jToken;
							return !arrayProperty.FindNonMatchingElements(objs, AreEquivalent).Any();
						}
						else
						{
							return false;
						}
					default:
						return jToken.Value<JValue>().Value.Equals(obj);
				}
			}
		}

		internal static bool GetPropertyValue(object obj, string propertyName, out object propertyValue)
		{
			return obj is IDictionary<string, object> dictionary && dictionary.TryGetValue(propertyName, out propertyValue)
				|| (propertyValue = obj.GetType().GetProperty(propertyName)?.GetValue(obj)) != null;
		}

		public static T1[] FindNonMatchingElements<T1, T2>(this IEnumerable<T1> enumerable1, IEnumerable<T2> enumerable2, Func<T1, T2, bool> matchFunc) =>
			enumerable1.Where(a => enumerable2.All(b => !matchFunc(a, b))).ToArray();

		static readonly Regex LineBreakingsAndIndention = new Regex(@"[\r\n]+\s*", RegexOptions.Compiled);
		public static string ToTestCaseDescription(this object testCase) => LineBreakingsAndIndention.Replace(testCase.ToString(), string.Empty).Substring(0, 100) + "...";

		public static string TempPath
		{
			get
			{
				const string appendedDirectory = "WiseTechGlobal";
				var path = Path.Combine(Path.GetTempPath(), appendedDirectory, Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
				return path;
			}
		}

		public static void AssertKeySets(string entity, IEnumerable<KeySet> keySets, params string[] properties)
		{
			var keys = keySets.Select(x => x.PropertySchema.Name).OrderBy(x => x).ToList();
			var fields = properties.OrderBy(x => x).ToList();

			Assert.That(string.Join(", ", fields), Is.EqualTo(string.Join(", ", keys)), $"Keys for {entity}");
		}

		public static IDisposable TemporarilyUseApplicationConfig(string key, string value)
		{
			var config = ApplicationConfig.Configuration;
			var oldValue = config.GetValue<string>(key);

			config[key] = value;

			return new DisposableAction(() => config[key] = oldValue);
		}
	}

	sealed class DisposableAction(Action dispose) : IDisposable
	{
		public DisposableAction(Action initialise, Action dispose)
			: this(dispose)
		{
			initialise?.Invoke();
		}

		public void Dispose()
		{
			dispose();
		}
	}
}
