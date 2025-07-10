using System.IO;
using CargoWise.Macros;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IeDocBase
	{
		ZBlob ImageData { get; set; }

		ZBool IsDeleted { get; set; }
		ZBool IsPublished { get; set; }
		ZBool IsSystemGenerated { get; }

		ZDateTime DateAdded { get; set; }
		ZDateTime LastEdited { get; }
		ZString LastEditedUser { get; }

		ZGuid UniqueKey { get; }

		ZString Description { get; set; }

		[MacroIgnore]
		ZString DocType { get; set; }
		ZString DocSource { get; set; }
		ZString DocSourceDescription { get; }
		ZString FileName { get; }
		ZString DataType { get; }
		ZString FileNameOnly { get; }

		void NotifyReadByUser();

		Stream GetImageDataReader();
		void SetImageDataStream(Stream stream);

		void Delete();
	}
}
