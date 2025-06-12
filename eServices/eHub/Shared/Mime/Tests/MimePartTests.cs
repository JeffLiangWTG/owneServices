using System;
using System.IO;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Shared.Mime.Tests
{
	[TestClass]
	public class MimePartTests
	{
		[TestMethod]
		public void MimePart_Parse()
		{
			var message = TestFileHelpers.GetResourceStream("TestFiles.MessageAsAttachmentWithBodyIn.txt");

			var expected = new MimePart.Multipart("multipart/mixed", "_E53422A7-A603-4963-A3C4-106EE0138016_")
			{
				Headers =
				{
					{ "MIME-Version", "1.0" },
					{ "Content-Type", "multipart/mixed;\tboundary=\"_E53422A7-A603-4963-A3C4-106EE0138016_\"" }
				},
				Parts =
				{
					new MimePart.Content("text/plain")
					{
						Contents = new MemoryStream(Encoding.Default.GetBytes(@"This message has attachments.")),
						Headers =
						{
							{ "MIME-Version", "1.0" },
							{ "Content-Type", "text/plain" }
						},
						Raw = new MemoryStream(Encoding.Default.GetBytes(
@"MIME-Version: 1.0
Content-Type: text/plain

This message has attachments."))
					},
					new MimePart.Content("text/plain")
					{
						Contents = new MemoryStream(Encoding.Default.GetBytes(
@"Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor.
Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.
Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate.")),
						Headers =
						{
							{ "MIME-Version", "1.0" },
							{ "Content-Transfer-Encoding", "8bit" },
							{ "Content-ID", "{45D3D31C-9308-4F55-9EB1-B0335067759A}" },
							{ "Content-Description", "body" },
							{ "Content-Disposition", "inline; filename=\"Attachment0\"" },
						},
						Raw = new MemoryStream(Encoding.Default.GetBytes(
@"MIME-Version: 1.0
Content-Transfer-Encoding: 8bit
Content-ID: {45D3D31C-9308-4F55-9EB1-B0335067759A}
Content-Description: body
Content-Disposition: inline; filename=""=?utf-8?B?QXR0YWNobWVudDB=?=""

Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor.
Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.
Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate."))
					}
				},
				Raw = TestFileHelpers.GetResourceStream("TestFiles.MessageAsAttachmentWithBodyIn.txt")
			};

			var compareConfig = new ComparisonConfig();
			compareConfig.CustomComparers.Add(new MimePartTests.StreamComparer(RootComparerFactory.GetRootComparer()));
			var compareLogic = new CompareLogic(compareConfig);

			using (var mimeParts = MimePart.Parse(message))
			{
				var compareResult = compareLogic.Compare(expected, mimeParts);
				if (!compareResult.AreEqual)
					throw new Exception(compareResult.DifferencesString);
			}
		}

		[TestMethod]
		public void MimePart_Format()
		{
			var message = new MimePart.Multipart("multipart/mixed", "_E53422A7-A603-4963-A3C4-106EE0138016_")
			{
				Headers =
				{
					{ "MIME-Version", "1.0" },
				},
				Parts =
				{
					new MimePart.Content("text/plain")
					{
						Contents = new MemoryStream(Encoding.Default.GetBytes(@"This message has attachments.")),
						Headers =
						{
							{ "MIME-Version", "1.0" },
						},
					},
					new MimePart.Content("text/plain")
					{
						Contents = new MemoryStream(Encoding.Default.GetBytes(
@"Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor.
Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.
Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate.")),
						Headers =
						{
							{ "MIME-Version", "1.0" },
							{ "Content-Transfer-Encoding", "8bit" },
							{ "Content-ID", "{45D3D31C-9308-4F55-9EB1-B0335067759A}" },
							{ "Content-Description", "body" },
							{ "Content-Disposition", "inline; filename=\"Attachment0\"" },
						},
					}
				},
			};

			var actual = message.Format();

			Assert.AreEqual(TestFileHelpers.GetResourceText("TestFiles.MessageAsAttachmentWithBodyOut.txt"), new StreamReader(actual).ReadToEnd());
		}

		[TestMethod]
		public void MimePart_FormatAsSmime()
		{
			var message = new MimePart.Content("text/plain")
			{
				Contents = new MemoryStream(Encoding.Default.GetBytes(
@"Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor.
Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.
Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate.")),
			};
			message.Headers["Content-Transfer-Encoding"] = "8bit";
			message.Headers["Content-Description"] = "body";

			var cert = TestFileHelpers.GetResourceData("TestFiles.00505655TST.pfx");

			MimePart.GetNewGuid = () => new Guid("C2B7ECF9-2E1E-488C-83C7-F3864F7E0C25");

			var actual = message.FormatAsSMime(cert, "miragef7");

			Assert.AreEqual(TestFileHelpers.GetResourceText("TestFiles.MessageAsBodySigned.txt"), new StreamReader(actual).ReadToEnd());
		}

		[TestMethod]
		public void MimePart_HeaderSequence()
		{
			var startId = "1234567890-ABCDEFGHIJ";
			var contentType = string.Format("multipart/related;\tstart-info=\"text/xml\";\ttype=\"application/xop+xml\";\tstart=\"<{0}>\"", startId);

			var message = new MimePart.Multipart(contentType, "_QWERTYUIOP-ASDFGHJKL-ZXCVBNM_")
			{
				Headers =
				{
					{ "MIME-Version", "1.0" },
					{ "Content-Type", contentType },
					{ "Test-Content", "Test Header 3" },
					{ "Additional-Info", "Test Header 4" }
				},
				Parts =
				{
					new MimePart.Content("text/xml")
					{
						Headers =
						{
							{ "Content-Transfer-Encoding", "8bit" },
							{ "Seq-Test", "Non-Alphabetical Order" },
							{ "Content-ID", string.Format("<{0}>", startId) },
							{ "Content-Type", "application/xop+xml; type=\"text/xml\"; charset=UTF-8" }
						},
						Contents = new MemoryStream(Encoding.UTF8.GetBytes("<Details><Message>The quick brown fox jumps over the lazy dog</Message></Details>"))
					},
					new MimePart.Content("text/xml; charset=UTF-8")
					{
						Headers =
						{
							{ "Content-ID", string.Format("<{0}>", startId) },
							{ "Content-Transfer-Encoding", "8bit" },
							{ "Test-Part", "Part 2" }
						},
						Contents = new MemoryStream(Encoding.UTF8.GetBytes("This section should have a Content-Type first with spaces not tabs"))
					}
				}
			};

			var actual = message.Format();

			Assert.AreEqual(TestFileHelpers.GetResourceText("TestFiles.MessageSequence.txt"), new StreamReader(actual).ReadToEnd());

		}

		public class StreamComparer : BaseTypeComparer
		{
			public StreamComparer(RootComparer rootComparer) : base(rootComparer) { }

			public override bool IsTypeMatch(Type type1, Type type2)
			{
				return type1.IsSubclassOf(typeof(Stream)) && type2.IsSubclassOf(typeof(Stream));
			}

			public override void CompareType(CompareParms parms)
			{
				var object1 = (Stream)parms.Object1;
				var object2 = (Stream)parms.Object2;

				string streamContents1 = new StreamReader(object1).ReadToEnd();
				string streamContents2 = new StreamReader(object2).ReadToEnd();

				if (streamContents1 != streamContents2)
				{
					Difference difference = new Difference
					{
						PropertyName = parms.BreadCrumb,
						Object1Value = streamContents1,
						Object2Value = streamContents2
					};
					parms.Result.Differences.Add(difference);
				}
			}
		}
	}
}
