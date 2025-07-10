using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class ExportCustomsManifestLinesCollection : DependentBusinessObjectCollection<ExportCustomsManifestLines, ExportCustomsManifestHeader>
	{
		public ExportCustomsManifestLinesCollection(ExportCustomsManifestHeader parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ExportCustomsManifestLines);
		}

		public override void Load()
		{
			base.Load();
			this.Sort(ExportCustomsManifestLines.Schema.EL_LineNo, System.ComponentModel.ListSortDirection.Ascending);
		}

		public int TotalContainers
		{
			get
			{
				int result = 0;
				foreach (ExportCustomsManifestLines line in this)
				{
					result += line.EL_NumberOfContainers;
				}
				return result;
			}
		}

		public int TotalPackages
		{
			get
			{
				int result = 0;
				foreach (ExportCustomsManifestLines line in this)
				{
					result += line.EL_NumberOfPackages;
				}
				return result;
			}
		}

		protected void EL_TypeOfCANInfo_ValueChanged(object sender, EventArgs e)
		{
			if (TypeOfCANChanged != null)
			{
				TypeOfCANChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler TypeOfCANChanged;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((ExportCustomsManifestLines)child).EL_LineNo = (short)(MaximumLineNo + 1);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			ExportCustomsManifestLines line = bizOAdded as ExportCustomsManifestLines;
			line.EL_TypeOfCANInfo.ValueChanged += new EventHandler(EL_TypeOfCANInfo_ValueChanged);
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			ExportCustomsManifestLines line = bizO as ExportCustomsManifestLines;
			line.EL_TypeOfCANInfo.ValueChanged -= new EventHandler(EL_TypeOfCANInfo_ValueChanged);
		}

		protected int MaximumLineNo
		{
			get
			{
				int result = 0;
				foreach (ExportCustomsManifestLines line in this)
				{
					if (line.EL_LineNo > result)
					{
						result = line.EL_LineNo;
					}
				}
				return result;
			}
		}
	}
}
