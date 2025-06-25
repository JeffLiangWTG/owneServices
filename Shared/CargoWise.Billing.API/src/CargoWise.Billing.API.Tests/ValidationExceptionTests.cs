using System;
using System.IO;
#if NETFRAMEWORK
using System.Runtime.Serialization.Formatters.Binary;
#else
using Newtonsoft.Json;
#endif
using NUnit.Framework;

namespace CargoWise.Billing.API.Tests
{
	[TestFixture]
	public class ValidationExceptionTest
	{
		[Test]
		public void TestValidationExceptionSerialization()
		{
			var exception = new API.ValidationException("Message.", new[] { "Error 1", "Error 2" }, new Exception("Inner exception."));
#if NETFRAMEWORK
			var formatter = new BinaryFormatter();
			using (var stream = new MemoryStream())
			{
				formatter.Serialize(stream, exception);
				stream.Seek(0, 0);
				var deserializedException = (API.ValidationException)formatter.Deserialize(stream);

				Assert.That(deserializedException.Message, Is.EqualTo(exception.Message));
				Assert.That(deserializedException.Errors, Is.EqualTo(exception.Errors));
				Assert.That(deserializedException.InnerException.Message, Is.EqualTo(exception.InnerException.Message));
				Assert.That(deserializedException.ToString(), Is.EqualTo(exception.ToString()));
			}
#else
			var jsonString = JsonConvert.SerializeObject(exception);
			var deserializedException = JsonConvert.DeserializeObject<API.ValidationException>(jsonString);
			Assert.That(deserializedException.Message, Is.EqualTo(exception.Message));
			Assert.That(deserializedException.Errors, Is.EqualTo(exception.Errors));
			Assert.That(deserializedException.InnerException.Message, Is.EqualTo(exception.InnerException.Message));
			Assert.That(deserializedException.ToString(), Is.EqualTo(exception.ToString()));
#endif
		}
	}
}
