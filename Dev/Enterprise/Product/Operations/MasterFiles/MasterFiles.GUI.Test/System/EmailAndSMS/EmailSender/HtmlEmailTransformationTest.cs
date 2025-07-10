using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	sealed class HtmlEmailTransformationTest : TestCaseWithFactory
	{
		public void TestInvalidEmailSubject()
		{
			EmailDef email = new EmailDef();
			email.Subject = "My name is invalid Subject Because I have :\\>@#<$";
			var result = HtmlEmailTransformation.ConvertFileWithExtensionPath(email.Subject);
			AssertNoExceptionThrown("Expect not throw expcetion when Path.GetExtension", () => FindIllegalPathChars(result));
			AssertNoExceptionThrown("Expect not throw expcetion when Path.GetExtension", () => Path.GetExtension(result));
		}

		public void TestCorrectExtension()
		{
			EmailDef email = new EmailDef();
			email.Subject = "My name is invalid Subject Because I have :\\>@#<$";
			AssertEquals("Expected right ExtensionPath", ".html", Path.GetExtension(HtmlEmailTransformation.ConvertFileWithExtensionPath(email.Subject)));
		}

		void FindIllegalPathChars(string path)
		{
			int index = -1;
			do
			{
				index = path.IndexOfAny(Path.GetInvalidPathChars(), index + 1);
				if (index > -1)
				{
					throw new ArgumentNullException(path, String.Format("Invalid char \"{0}\" at position {1}", path[index], index));
				}
			} while (index > -1);
		}
	}
}
