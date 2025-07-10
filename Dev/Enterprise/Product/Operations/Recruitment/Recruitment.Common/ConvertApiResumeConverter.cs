using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using ConvertApiDotNet;
using ConvertApiDotNet.Exceptions;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruitment.Common
{
	public class ConvertApiResumeConverter : IResumeConverter
	{
		public readonly IConvertApi ConvertApi;
		readonly ILogger logger;
		readonly string cvDocType;

		public ConvertApiResumeConverter(ILogger logger)
		{
			this.logger = logger;

			var key = RecruitmentDataRegistry.Instance.ConvertApiSecretKey.Value;
			if (string.IsNullOrEmpty(key))
			{
				logger?.Debug((NoResString)"ConvertApi Key is not set. Please set " + RecruitmentDataRegistry.Instance.ConvertApiSecretKey.HumanReadableRegistryPath() + (NoResString)" to convert resumes to supported types");
			}
			else
			{
				ConvertApi = new ConvertApiImpl(key);
			}

			var factory = new BusinessObjectFactory();
			cvDocType = RecruiterDataRegistry.Instance.GetDocTypeCV(factory)?.RT_DocType;
		}

		public ConvertApiResumeConverter(ILogger logger, IConvertApi convertApi)
		{
			this.logger = logger;
			this.ConvertApi = Argument.NotNull(convertApi, nameof(convertApi));

			var factory = new BusinessObjectFactory();
			cvDocType = RecruiterDataRegistry.Instance.GetDocTypeCV(factory)?.RT_DocType;
		}

		public static readonly ImmutableArray<string> fileExtensions = ImmutableArray.Create("DOC", "DOCX", "MSG", "EML");

		public bool CanConvert(string ext)
			=> fileExtensions.Any(allowedExt => ext.Trim('.').Equals(allowedExt, StringComparison.OrdinalIgnoreCase));

		public IeDoc GetResumeToConvert(HRJobApplication host)
		{
			var resumes = host.DocManagerInfo.AllEDocs
				.Cast<IeDoc>()
				.Where(doc => doc.ImageData.IsValid && cvDocType.Equals(doc.DocType, StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (resumes.Any(doc => PreviewableDocumentHelper.IsSupported(doc.DataType)))
			{
				logger?.Information((NoResString)"A document in a supported filetype was already present. No conversion will occur.");
				return null;
			}

			var toConvert = GetResumeToConvertInPreference(resumes);
			if (toConvert == null)
			{
				logger?.Information((NoResString)"No convertible files could be found");
				return null;
			}

			return toConvert;
		}

		static IeDoc GetResumeToConvertInPreference(IEnumerable<IeDoc> resumes)
		{
			IeDoc toConvert = null;
			foreach (var ext in fileExtensions)
			{
				toConvert = resumes.FirstOrDefault(x => x.DataType.ToString().Trim('.').Equals(ext, StringComparison.OrdinalIgnoreCase));

				if (toConvert != null)
				{
					break;
				}
			}

			return toConvert;
		}

		public void AttachEdocCore(HRJobApplication jobApplication, string filename, Stream data, string docType)
		{
			_ = jobApplication.DocManagerInfo.AddFileOrDocument(data.ReadFully(), Path.GetFileName(filename), docType, false);
			jobApplication.DocManagerInfo.Save();
		}

		public string ConvertedFileExtension => "PDF";

		public async Task<Stream> ConvertToPdfAsync(string inputFileExtension, Stream input)
			=> await ConvertApi.ConvertAsync(inputFileExtension, ConvertedFileExtension, input).ConfigureAwait(false);

		public void ConvertAvailableResumes(HRJobApplication host)
		{
			if (ConvertApi is null)
			{
				return;
			}

			var toConvert = GetResumeToConvert(host);
			if (toConvert == null)
			{
				return;
			}

			try
			{
				var imageDataStream = toConvert.GetImageDataReader();
				var inputFileExtension = Path.GetExtension(toConvert.FileName).ToUpperInvariant().TrimStart('.');
				var runner = Task.Run(() => ConvertToPdfAsync(inputFileExtension, imageDataStream));
				AttachEdocCore(host, Path.ChangeExtension(toConvert.FileName, ConvertedFileExtension), runner.Result, toConvert.DocType.ToString());
			}
			catch (ConvertApiException ex)
			{
				logger?.Warning(FormattableString.Invariant($"Document could not be converted: {ex.Message}"));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = ex.Message;
				if (ex is AggregateException aggregate)
				{
					message = string.Join(System.Environment.NewLine, aggregate.InnerExceptions.Select(inner => inner.Message));
				}

				logger?.Warning(FormattableString.Invariant($"Document could not be converted: {message}"));
			}
		}
	}

	public class ConvertApiImpl : ConvertApi, IConvertApi
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public ConvertApiImpl(string secret, int requestTimeoutInSeconds = 180) : base(secret)
		{
			_ = Argument.NotNullOrEmpty(secret, nameof(secret));
			Key = secret;
		}

		public readonly string Key;

		public async Task<Stream> ConvertAsync(string fromFormat, string toFormat, Stream data)
		{
			CheckValidExtension(fromFormat, nameof(fromFormat));
			CheckValidExtension(toFormat, nameof(toFormat));
			Argument.NotNull(data, nameof(data));

			using (data)
			{
				var response = await ConvertAsync(fromFormat, toFormat, new ConvertApiFileParam(data, "data." + fromFormat)).ConfigureAwait(false);
				return await response.Files.Single().FileStreamAsync().ConfigureAwait(false);
			}
		}

		void CheckValidExtension(string format, string paramName)
		{
			Argument.NotNullOrEmpty(format, paramName);

			if (Path.HasExtension(format))
			{
				throw new ArgumentException("Extension expected, filename received: " + format, paramName);
			}
		}
	}
}
