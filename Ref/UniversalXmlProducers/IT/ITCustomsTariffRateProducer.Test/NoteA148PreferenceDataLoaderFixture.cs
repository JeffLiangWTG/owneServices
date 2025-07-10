using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	sealed class NoteA148PreferenceDataLoaderFixture
	{
		[Test]
		public void OnSuccessfulConnection_LoadPreferenceCodes()
		{
			var preferenceDataList = new[]
			{
				new PreferenceData { Code = "100", DataGrouping ="EUN"},
				new PreferenceData { Code = "110", DataGrouping ="EUN"},
			};

			var preferenceDataWrapper = new PreferenceDataWrapper
			{
				Value = preferenceDataList
			};

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(preferenceDataWrapper))
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var preferenceDataLookup = new NoteA148PreferenceDataLoader(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = preferenceDataLookup.Load(DummyUrl);
				Assert.IsTrue(loadSuccessful, "Load Successful?");

				var loadedPreferences = ((IPreferenceDataLookup)preferenceDataLookup).Preferences;
				CollectionAssert.AreEqual(preferenceDataList, loadedPreferences, new PreferenceDataComparer(), "Loaded Preferences");
			}
		}

		[Test]
		public void OnUnsuccessfulConnection_LogsErrorMessage()
		{
			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent(string.Empty)
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var preferenceDataLookup = new NoteA148PreferenceDataLoader(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = preferenceDataLookup.Load(DummyUrl);
				Assert.IsFalse(loadSuccessful, "Load Successful?");

				_loggerMock.Verify(x => x.Log("Unable to get Preference Code response from database"));
			}
		}

		[Test]
		public void OnSuccessfulConnectionButEmptyResponse_LogsErrorMessage()
		{
			var preferenceDataWrapper = new PreferenceDataWrapper
			{
				Value = new PreferenceData[0]
			};

			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(preferenceDataWrapper))
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var preferenceDataLookup = new NoteA148PreferenceDataLoader(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = preferenceDataLookup.Load(DummyUrl);
				Assert.IsFalse(loadSuccessful, "Load Successful?");

				_loggerMock.Verify(x => x.Log("Unable to load Preference Code from database"));
			}
		}

		[Test]
		public void OnSuccessfulConnectionButInvalidJsonResponse_LogsErrorMessage()
		{
			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{ text: \"I am a JSON response but I am not valid.\"}")
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var preferenceDataLookup = new NoteA148PreferenceDataLoader(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = preferenceDataLookup.Load(DummyUrl);
				Assert.IsFalse(loadSuccessful, "Load Successful?");

				_loggerMock.Verify(x => x.Log("Unable to load Preference Code from database"));
			}
		}

		[Test]
		public void OnSuccessfulConnectionButInvalidResponse_LogsErrorMessage()
		{
			var responseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("I am not a JSON response"),
			};

			using (responseMessage)
			{
				_httpHandlerMock
					.Setup(x => x.Get(It.IsAny<string>()))
					.Returns(responseMessage);

				var preferenceDataLookup = new NoteA148PreferenceDataLoader(_loggerMock.Object, _httpHandlerMock.Object);
				var loadSuccessful = preferenceDataLookup.Load(DummyUrl);
				Assert.IsFalse(loadSuccessful, "Load Successful?");
				_loggerMock.Verify(x => x.Log(It.IsAny<Exception>()));
			}
		}

		[SetUp]
		public void Setup()
		{
			_httpHandlerMock = new Mock<IHttpHandler>();
			_loggerMock = new Mock<ILogger>();
		}

		Mock<IHttpHandler> _httpHandlerMock;
		Mock<ILogger> _loggerMock;
		const string DummyUrl = "http://localhost/dummyurl";

		sealed class PreferenceDataComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				var preferenceX = (PreferenceData)x;
				var preferenceY = (PreferenceData)y;

				var result = preferenceX.Code.CompareTo(preferenceY.Code);
				return result != 0 ? result : preferenceX.DataGrouping.CompareTo(preferenceY.DataGrouping);
			}
		}
	}
}
