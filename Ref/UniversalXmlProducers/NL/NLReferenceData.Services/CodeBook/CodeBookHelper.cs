using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public static class CodeBookHelper
	{
		public static CodeBookModel ReadCodeBook(List<string> fileList)
		{
			var codeBook = new CodeBookModel();
			if (fileList.Count != 1)
			{
				throw new ProcessingException("Multiple files are downloaded, unable to process all files.");
			}

			using (var xmlStream = new FileStream(fileList[0], FileMode.Open, FileAccess.Read))
			{
				xmlStream.Seek(0, SeekOrigin.Begin);
				using (var reader = XmlReader.Create(xmlStream))
				{
					reader.ReadToFollowing("cbk");
					if (!reader.EOF)
					{
						codeBook = (CodeBookModel)new XmlSerializer(typeof(CodeBookModel)).Deserialize(reader.ReadSubtree());
					}
				}
			}
			return codeBook;
		}
	}
}
