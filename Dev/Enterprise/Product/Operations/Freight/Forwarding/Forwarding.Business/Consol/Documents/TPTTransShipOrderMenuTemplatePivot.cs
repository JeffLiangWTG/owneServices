using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ExcelTemplates.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class TPTTransShipOrderMenuTemplatePivot : IStmMenuTemplatePivot
	{
		public TPTTransShipOrderMenuTemplatePivot(BusinessObjectFactory factory, TPTTransShipOrderMenuItem menuItem, string documentName)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(menuItem, nameof(menuItem));

			SI_DocumentTitle = documentName;
			MenuItem = menuItem;
			SI_SU = menuItem.PK;
			SI_SO = new ZGuid("56bafcc0-9c3e-4bc7-92cb-89ef43f9f6f6");
			SI_DataStoreName = documentName;
			SI_IsSystemDefined = true;
			SI_PrintCopyType = Convert.ToString(PrintCopyType.ALL, CultureInfo.InvariantCulture);
		}

		readonly BusinessObjectFactory factory;

		public ZGuid PK { get; } = ZGuid.NewZGuid();
		public ZString SI_DocumentTitle { get; set; }
		public ZByte SI_Index { get; set; }
		public ZBool SI_IsClientSpecific { get; set; }
		public ZBool SI_IsSystemDefined { get; set; } = true;
		public ZString SI_MenuTemplateFilter { get; set; }
		public ZBool SI_PrintByDefault { get; set; }
		public ZString SI_PrintCopyType { get; set; }
		public ZGuid SI_RT_DocType { get; set; }
		public ZGuid SI_SO { get; set; }
		public ZGuid SI_SU { get; set; }
		public ZShort SI_TrailingLines { get; set; }
		public ZString SI_DataStoreName { get; set; }

		public IRefDocType DocType { get; }
		public IStmMenuItem MenuItem { get; }

		public IStmTemplate Template => template ?? (template = factory.Load<StmTemplate>(SI_SO));
		IStmTemplate template;
	}
}
