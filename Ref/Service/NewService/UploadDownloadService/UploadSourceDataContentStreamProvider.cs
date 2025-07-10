using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.NewService
{
	public class UploadSourceDataContentStreamProvider : MultipartStreamProvider
	{
		readonly IDbConnection _sqlConn;
		readonly IDbTransaction _sqlTran;
		readonly string _fileType;
		readonly string _source;
		readonly string _contentType;
		readonly string _contacts;
		const int _BufferSize10MB = 10000000;

		public UploadSourceDataContentStreamProvider(IDbConnection dbConnection, IDbTransaction dbTransaction, string fileType, string source, string contentType, string contacts)
		{
			Argument.NotNull(dbConnection, nameof(dbConnection));
			Argument.NotNull(dbTransaction, nameof(dbTransaction));
			Argument.NotNullOrEmpty(contentType, nameof(contentType));
			Argument.NotNullOrEmpty(fileType, nameof(fileType));
			Argument.NotNullOrEmpty(source, nameof(source));
			Argument.NotNull(contacts, nameof(contacts));

			_sqlConn = dbConnection;
			_sqlTran = dbTransaction;

			_fileType = fileType;
			_source = source;
			_contentType = contentType;
			_contacts = contacts;
		}

		public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
		{
			var fileName = !string.IsNullOrWhiteSpace(headers.ContentDisposition.FileName) ? headers.ContentDisposition.FileName : "NoName";
			fileName = fileName.Replace("\"", string.Empty);

			return new BufferedStream(new SourceDataContentWriterStream(_sqlConn, _sqlTran, fileName, _contentType, _fileType, _source, _contacts), _BufferSize10MB);
		}
	}
}
