using System;
using System.Globalization;
using System.IO;
using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AttachmentTextAppenderTest : TestCase
	{
		public void TestAppend()
		{
			// no password
			byte[] data = CreateZip("data.xml", "<h>clear</h>", null);
			StringBuilder sb = new StringBuilder();
			new AttachmentTextAppender().Append("data.xml", data, sb);
			AssertEquals("<h>clear</h>", sb.ToString());

			// with password
			data = CreateZip("data.xml", "<h>protected</h>", "xyzzy");
			sb.Length = 0;
			new AttachmentTextAppender().Append("data.xml", data, sb, "xyzzy");
			AssertEquals("<h>protected</h>", sb.ToString());

			// encrypted
			TwoWayEncoder cryptor = TwoWayEncoder.NewWithStandardInitialisationVector();
			cryptor.Encrypt("<h>stuff</h>");
			data = CreateZip("data.rgl", cryptor.Encrypt("<h>encrypted</h>"), null);
			sb.Length = 0;
			new AttachmentTextAppender().Append("data.rgl", data, sb, null);
			AssertEquals("<h>encrypted</h>", sb.ToString());

			// Exception
			foreach (AttachmentTextAppenderForTest.ExceptionType exceptionType in System.Enum.GetValues(typeof(AttachmentTextAppenderForTest.ExceptionType)))
			{
				data = CreateZip("data.xml", "<h>clear</h>", null);
				sb.Length = 0;
				AttachmentTextAppenderForTest attachmentTextAppender = new AttachmentTextAppenderForTest();
				if (exceptionType == AttachmentTextAppenderForTest.ExceptionType.None)
				{
					continue;
				}

				attachmentTextAppender.TestExceptionType = exceptionType;
				attachmentTextAppender.Append("data.xml", data, sb);
				AssertEquals(string.Format(CultureInfo.CurrentCulture, "Exception {0} must be caught", exceptionType.ToString()), "", sb.ToString());
			}
		}

		byte[] CreateZip(string fileName, string fileContents, string password)
		{
			byte[] data = Encoding.UTF8.GetBytes(fileContents);
			using (MemoryStream contentStream = new MemoryStream(data))
			using (MemoryStream zipStream = new MemoryStream())
			{
				ZipCreator creator = new ZipCreator(password);
				creator.ZipStream(new ZipStream[] { new ZipStream(fileName, contentStream) }, zipStream);
				return zipStream.ToArray();
			}
		}
	}

	class AttachmentTextAppenderForTest : AttachmentTextAppender
	{
		public enum ExceptionType
		{
			None,
			ArithmeticException,
			ArrayTypeMismatchException,
			DivideByZeroException,
			IndexOutOfRangeException,
			InvalidCastException,
			NullReferenceException,
			OutOfMemoryException,
			OverflowException,
			StackOverflowException
		}
		public ExceptionType TestExceptionType { get; set; }

		protected override StringBuilder Append(StringBuilder sb, string value)
		{
			switch (TestExceptionType)
			{
				case ExceptionType.ArithmeticException:
					throw new ArithmeticException();
				case ExceptionType.ArrayTypeMismatchException:
					throw new ArrayTypeMismatchException();
				case ExceptionType.DivideByZeroException:
					throw new DivideByZeroException();
				case ExceptionType.IndexOutOfRangeException:
					throw new IndexOutOfRangeException();
				case ExceptionType.InvalidCastException:
					throw new InvalidCastException();
				case ExceptionType.NullReferenceException:
					throw new NullReferenceException();
				case ExceptionType.OutOfMemoryException:
					throw new OutOfMemoryException();
				case ExceptionType.OverflowException:
					throw new OverflowException();
				case ExceptionType.StackOverflowException:
					throw new StackOverflowException();
			}
			return null;
		}
	}
}
