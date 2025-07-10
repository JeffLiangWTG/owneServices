using System;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PrinterInfo : DataObjectInfo
	{
		public PrinterInfo()
		{
			Name = "";
		}

		public PrinterInfo(IStmPrintQueue printer)
		{
			Name = printer.SQ_DisplayName;
			PK = printer.PK.ToGuid();
		}

		public string Name
		{
			get;
			set;
		}

		public Guid PK
		{
			get;
			set;
		}
	}
}
