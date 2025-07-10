using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Core;
using NLog.Layouts;

namespace Enterprise.Recruiter.Business
{
	public class DaxtraResumeParser : IApplicantResumeParser, IDisposable
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Json attributes")]
		public DaxtraResumeParser()
		{
			var jsonLayout = new JsonLayout
			{
				IncludeEventProperties = true
			};

			jsonLayout.Attributes.Add(new JsonAttribute("message", "${message}"));
			jsonLayout.Attributes.Add(new JsonAttribute("resume", "${event-properties:item=resume}"));
			jsonLayout.Attributes.Add(new JsonAttribute("fields", "${event-properties:item=fields}"));
			jsonLayout.Attributes.Add(new JsonAttribute("userCode", "${event-properties:item=userCode}"));

			loggerProvider = new HRKafkaLoggerProvider(GetType().Name, jsonLayout);
		}

		public DaxtraResumeParser(IHRLoggerProvider loggerProvider)
		{
			this.loggerProvider = loggerProvider;
		}

		public IApplicantResumeParseResult Parse(string resumeFileName, byte[] resumeFileContent)
		{
			var result = new ApplicantResumeParseResult();

			var serviceUrl = RecruiterDataRegistry.Instance.DaxtraServiceUrl.Value;
			var account = RecruiterDataRegistry.Instance.DaxtraAccountName.Value;
			var service = GetServiceClient(serviceUrl, account);

			if (service.IsReadyToUse)
			{
				service.TimeoutMilliseconds = RecruiterDataRegistry.Instance.DaxtraTimeout.Value * 1000;
				var parsedResumeXml = service.ProcessCV(resumeFileName, resumeFileContent);
				if (string.IsNullOrEmpty(parsedResumeXml))
				{
					result.Status = ApplicantResumeParseStatus.Fail;
					result.Message = service.ErrorMessage;
				}
				else
				{
					var numErrorsPrev = numErrors;
					result.ParsedResume = ReadFromXml(parsedResumeXml);
					result.Status = numErrorsPrev == numErrors
						? ApplicantResumeParseStatus.Success
						: ApplicantResumeParseStatus.Fail;

					Log(resumeFileContent, result.ParsedResume, result.Status == ApplicantResumeParseStatus.Fail);
				}
			}
			else
			{
				result.Status = ApplicantResumeParseStatus.Fail;
				result.Message = NotReadyMessage;
			}

			return result;
		}

		public IApplicantResume ReadFromXml(string resumeXml)
		{
			#region SuppressResourceStringsCheckRegion

			resumeXml = resumeXml.Replace("encoding='utf-8'", "").Replace("encoding=\"utf-8\"", "");
			var result = new ApplicantResume()
			{
				ResumeXml = resumeXml
			};

			try
			{
				string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
				if (resumeXml.StartsWith(byteOrderMarkUtf8, StringComparison.Ordinal))
				{
					resumeXml = resumeXml.Remove(0, byteOrderMarkUtf8.Length);
				}

				var doc = new XmlDocument();
				doc.PreserveWhitespace = true;
				doc.LoadXml(resumeXml);

				const string contactInfoPath = "Resume/StructuredXMLResume/ContactInfo";
				result.Name = doc.SelectSingleNode($"{contactInfoPath}/PersonName/FormattedName")?.InnerText ?? ZString.Empty;

				result.EmailAddress = doc.SelectSingleNode($"{contactInfoPath}/ContactMethod/InternetEmailAddress")?.InnerText ?? ZString.Empty;
				if (string.IsNullOrEmpty(result.EmailAddress))
				{
					var email = doc.SelectNodes("Resume/NonXMLResume/TextResume/span[@class='email']");
					if (email.Count > 0)
					{
						result.EmailAddress = email[0]?.InnerText;
					}
				}

				result.Gender = doc.SelectSingleNode($"{contactInfoPath}/PersonName/sex")?.InnerText ?? ZString.Empty;
				result.Mobile = doc.SelectSingleNode($"{contactInfoPath}/ContactMethod/Mobile/FormattedNumber")?.InnerText ?? ZString.Empty;
				result.HomePhone = doc.SelectSingleNode($"{contactInfoPath}/ContactMethod/Telephone/FormattedNumber")?.InnerText ?? ZString.Empty;
				result.Address1 = doc.SelectSingleNode($"{contactInfoPath}/ContactMethod/PostalAddress/DeliveryAddress/AddressLine")?.InnerText ?? ZString.Empty;
				result.City = doc.SelectSingleNode($"{contactInfoPath}/ContactMethod/PostalAddress/Municipality")?.InnerText ?? ZString.Empty;
				result.State = doc.SelectSingleNode($"{contactInfoPath}/ContactMethod/PostalAddress/Region")?.InnerText ?? ZString.Empty;
				result.Postcode = doc.SelectSingleNode($"{contactInfoPath}/ContactMethod/PostalAddress/PostalCode")?.InnerText ?? ZString.Empty;
				result.Country = doc.SelectSingleNode($"{contactInfoPath}/ContactMethod/PostalAddress/CountryCode")?.InnerText ?? ZString.Empty;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("", ex);
				numErrors++;
			}

			#endregion

			return result;
		}

		public IApplicantResumeBatchSendResult SendBatch(byte[] contents)
		{
			var result = new ApplicantResumeBatchSendResult();

			var serviceUrl = RecruiterDataRegistry.Instance.DaxtraServiceUrl.Value;
			var account = RecruiterDataRegistry.Instance.DaxtraAccountName.Value;
			var service = GetServiceClient(serviceUrl, account);

			if (service.IsReadyToUse)
			{
				service.TimeoutMilliseconds = RecruiterDataRegistry.Instance.DaxtraTimeout.Value * 1000;
				var token = service.ProcessBatch(contents);
				if (string.IsNullOrEmpty(token))
				{
					result.Status = ApplicantResumeParseStatus.Fail;
					result.Message = service.ErrorMessage;
				}
				else
				{
					result.Token = token;
					result.Status = ApplicantResumeParseStatus.Success;
				}

				if (!string.IsNullOrEmpty(service.ErrorMessage))
				{
					result.Message = service.ErrorMessage;
				}
			}
			else
			{
				result.Status = ApplicantResumeParseStatus.Fail;
				result.Message = NotReadyMessage;
			}

			return result;
		}

		public IApplicantResumeBatchGetDataResult GetData(string token)
		{
			var result = new ApplicantResumeBatchGetDataResult();

			var serviceUrl = RecruiterDataRegistry.Instance.DaxtraServiceUrl.Value;
			var account = RecruiterDataRegistry.Instance.DaxtraAccountName.Value;
			var service = GetServiceClient(serviceUrl, account);

			if (service.IsReadyToUse)
			{
				service.TimeoutMilliseconds = RecruiterDataRegistry.Instance.DaxtraTimeout.Value * 1000;
				var data = service.GetData(token);

				if (data != null)
				{
					var strData = Encoding.UTF8.GetString(data);
					if (strData == token)
					{
						result.Status = ApplicantBatchGetDataStatus.Processing;
					}
					else if (string.IsNullOrEmpty(strData) || strData == ProcessFinished)
					{
						result.Status = ApplicantBatchGetDataStatus.Finished;
					}
					else if (strData.Contains(CsError))
					{
						result.Status = ApplicantBatchGetDataStatus.Error;
						result.Error = strData;
					}
					else
					{
						result.Status = ApplicantBatchGetDataStatus.Zip;
						result.ZipContent = data;
					}
				}

				if (!string.IsNullOrEmpty(service.ErrorMessage))
				{
					result.Status = ApplicantBatchGetDataStatus.Error;
					result.Error = service.ErrorMessage;
				}
			}
			else
			{
				result.Status = ApplicantBatchGetDataStatus.Error;
				result.Error = NotReadyMessage;
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		protected virtual CVXtractorServiceClient GetServiceClient(string serviceUrl, string account)
		{
			return new CVXtractorServiceClient(serviceUrl, account, useJsonOutput: false);
		}

		const string ProcessFinished = "PROCESS_NOT_FOUND";
		const string CsError = "CSERROR";
		static string NotReadyMessage => Res.GetString("72790DDD-3111-4525-BD5A-A6014D7395C8", "The parsing service is not ready.");

		#region Logging

		readonly IHRLoggerProvider loggerProvider;

		int numErrors;

		internal void Log(byte[] resume, IApplicantResume parsedResume, bool hasError)
		{
			using (var sha256 = SHA256.Create())
			{
				var hash = BitConverter.ToString(sha256.ComputeHash(resume)).Replace("-", "");
				var fieldsParsed = typeof(IApplicantResume)
					.GetProperties()
					.Where(p =>
						p.PropertyType == typeof(ZString)
						&& p.Name != nameof(IApplicantResume.ResumeXml)
						&& (ZString)p.GetValue(parsedResume) != ZString.Empty)
					.Select(p => p.Name);

				var log = (NoResString)"User: {userCode}; fields retrieved: {fields}; resume parsed: {resume}";
				var logger = loggerProvider.GetLogger();
				logger.Info(
					log,
					GlbStaff.CurrentUser.GS_Code.ToString(),
					string.Join(", ", fieldsParsed),
					hash);

				if (hasError)
				{
					var error = (NoResString)"Error parsing resume {resume}";
					logger.Error(error, hash);
				}
			}
		}

		#endregion

		#region IDisposable

		bool isDisposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					loggerProvider?.Dispose();
				}
				isDisposed = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		#endregion

	}
}
