using System;
using System.Data;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.NewService
{
	public class SourceDataContentWriterStream : Stream
	{
		readonly IDbConnection _dbConnection;
		readonly Guid _sourceDataPk;
		readonly IDbTransaction _dbTransaction;

#pragma warning disable CA2213 // Disposable fields should be disposed
		IDbCommand _cmdAppendChunk;
#pragma warning restore CA2213 // Disposable fields should be disposed
		SqlParameter _paramChunk;

		long offset;

		string _fileType;
		string _source;
		string _contentType;
		string _fileName;
		string _contacts;

		public SourceDataContentWriterStream(IDbConnection connection, IDbTransaction transaction, string fileName, string contentType, string fileType, string source, string contacts)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNullOrEmpty(fileName, nameof(fileName));
			Argument.NotNullOrEmpty(contentType, nameof(contentType));
			Argument.NotNullOrEmpty(fileType, nameof(fileType));
			Argument.NotNullOrEmpty(source, nameof(source));
			Argument.NotNull(contacts, nameof(contacts));

			_dbTransaction = transaction;
			_dbConnection = connection;
			_sourceDataPk = Guid.NewGuid();

			SetFileData(fileName, contentType, fileType, source, contacts);
			ConfigureAppendChunkCommand();
		}

		void ConfigureAppendChunkCommand()
		{
			_cmdAppendChunk = _dbConnection.CreateCommand();
			_cmdAppendChunk.CommandText = @"
UPDATE [dbo].[SourceData]
	SET [SDA_Content].WRITE(@chunk, NULL, NULL)
	WHERE [SDA_PK] = @key";
			_cmdAppendChunk.Transaction = _dbTransaction;
			var parameterKey = _cmdAppendChunk.CreateParameter();
			parameterKey.ParameterName = "@key";
			parameterKey.Value = _sourceDataPk;
			_cmdAppendChunk.Parameters?.Add(parameterKey);

			_paramChunk = new SqlParameter("@chunk", SqlDbType.VarBinary, -1);
			_cmdAppendChunk.Parameters?.Add(_paramChunk);
		}

		void SetFileData(string fileName, string contentType, string fileType, string source, string contacts)
		{
			_fileName = fileName;
			_fileType = fileType;
			_source = source;
			_contentType = contentType;
			_contacts = contacts;
		}

		void PreSaveSourceData(byte[] firstChunk)
		{
			var cmdInsert = _dbConnection.CreateCommand();
			cmdInsert.Transaction = _dbTransaction;
			cmdInsert.CommandText = @"INSERT INTO dbo.SourceData (SDA_PK, SDA_Source, SDA_FileName, SDA_FileType, SDA_ContentType, SDA_Content, SDA_Contacts, SDA_Status)
VALUES (@pk, @source, @fileName, @fileType, @contentType, @firstChunk, @contacts, 'QUE')";
			var fileNameParam = cmdInsert.CreateParameter();
			fileNameParam.ParameterName = "@fileName";
			fileNameParam.Value = _fileName;
			var contentTypeParam = cmdInsert.CreateParameter();
			contentTypeParam.ParameterName = "@contentType";
			contentTypeParam.Value = _contentType;
			var fileTypeParam = cmdInsert.CreateParameter();
			fileTypeParam.ParameterName = "@fileType";
			fileTypeParam.Value = _fileType;
			var sourceParam = cmdInsert.CreateParameter();
			sourceParam.ParameterName = "@source";
			sourceParam.Value = _source;
			var firstChunkParam = cmdInsert.CreateParameter();
			firstChunkParam.ParameterName = "@firstChunk";
			firstChunkParam.Value = firstChunk;
			var contactsParam = cmdInsert.CreateParameter();
			contactsParam.ParameterName = "@contacts";
			contactsParam.Value = _contacts;
			var pkParam = cmdInsert.CreateParameter();
			pkParam.ParameterName = "@pk";
			pkParam.Value = _sourceDataPk;

			cmdInsert.Parameters?.Add(fileNameParam);
			cmdInsert.Parameters?.Add(contentTypeParam);
			cmdInsert.Parameters?.Add(fileTypeParam);
			cmdInsert.Parameters?.Add(sourceParam);
			cmdInsert.Parameters?.Add(firstChunkParam);
			cmdInsert.Parameters?.Add(pkParam);
			cmdInsert.Parameters?.Add(contactsParam);

			cmdInsert.ExecuteNonQuery();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			byte[] bytesToWrite = buffer;
			if (offset != 0 || count != buffer.Length)
			{
				bytesToWrite = new MemoryStream(buffer, offset, count).ToArray();
			}
			if (this.offset == 0)
			{
				PreSaveSourceData(bytesToWrite);
				this.offset = count;
			}
			else
			{
				_paramChunk.Value = bytesToWrite;
				_cmdAppendChunk.ExecuteNonQuery();
				this.offset += count;
			}
		}

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length
		{
			get
			{
				throw new NotSupportedException("Cannot get the length of this Stream.");
			}
		}

		public override long Position
		{
			get { throw new NotSupportedException("Cannot get the position of this Stream."); }
			set { throw new NotSupportedException("Cannot set the position of this Stream."); }
		}

		public override void Flush()
		{
			_dbTransaction.Commit();
			_dbConnection.Close();
			_cmdAppendChunk.Dispose();
			Dispose();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}
	}
}

