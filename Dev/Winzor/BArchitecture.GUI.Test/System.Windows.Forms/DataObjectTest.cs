using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace System.Windows.Forms;

class DataObjectTest
{
	[Test]
	public void GetHtmlData()
	{
		var dataObject = new DataObject();
		dataObject.SetData(DataFormats.Html, "Version:0.9\r\nStartHTML:000000149\r\nEndHTML:000000517\r\nStartFragment:000000183\r\nEndFragment:000000485\r\nStartSelection:000000183\r\nEndSelection:000000485\r\n<html><body><!--StartFragment--><a href=\"edient:Command=ShowStorageDoc&BusinessEntityPK=ea934030-6dcb-475e-a1a3-aa191d02de81&StorageDocPK=6d56a87c-28e4-4560-823e-b99da285261d&Hash=%2bZQp3E3OwMZWoRHwcZfoX70OYhZww74RC\">\r\nE 21-Feb-25 17:47:  File 屏幕截图 2025-02-20 180834[0].tif (BOM Assemble Instruction) added to eDocs tab.\r\n</a><!--EndFragment--></body></html>");
		var html = (string)dataObject.GetData(DataFormats.Html);
		Assert.That(html, Is.EqualTo("<a href=\"edient:Command=ShowStorageDoc&BusinessEntityPK=ea934030-6dcb-475e-a1a3-aa191d02de81&StorageDocPK=6d56a87c-28e4-4560-823e-b99da285261d&Hash=%2bZQp3E3OwMZWoRHwcZfoX70OYhZww74RC\">\r\nE 21-Feb-25 17:47:  File 屏幕截图 2025-02-20 180834[0].tif (BOM Assemble Instruction) added to eDocs tab.\r\n</a>"));
	}
}
