using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.eTail.Integration;

namespace Enterprise.eTail.Business
{
	public class HVLVItemRoutingLabelProvider : IHVLVRoutingLabelProvider
	{
		public HVLVItemRoutingLabelProvider(Guid itemPK)
		{
			factory = new BusinessObjectFactory();
			item = factory.Load<HVLVItem>(itemPK);
		}

		readonly HVLVItem item;
		readonly BusinessObjectFactory factory;

		public byte[] RoutingLabel => GetRoutingLabel();

		byte[] GetRoutingLabel()
		{
			if (item != null)
			{
				var docCommand = DocumentCommand.GetDocumentCommand(factory, item.Consignment, HVLVConsignmentDocumentSupporter.HVLVRoutingLabelMenuItemName);
				var note = DocumentNote.LoadNote(item);
				using (var pack = new DocumentPack(docCommand, item, note.GetCompleteFieldList(), null))
				{
					var report = pack.FirstOrDefault() as Report;
					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						return DocumentConverter.ConvertFromExcel(outputStream.ToArray(), OutputFormatType.PDF, ZArchitecture.Environment.ColourDepth.Colour256);
					}
				}
			}

			return null;
		}
	}
}
