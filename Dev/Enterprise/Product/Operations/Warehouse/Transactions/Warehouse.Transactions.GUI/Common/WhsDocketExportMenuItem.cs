using System;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public abstract class WhsDocketExportMenuItem<TDocket, TValueObject> : ZMenuItem
		where TDocket : WhsDocket
		where TValueObject : IValueObject
	{
		protected WhsDocketExportMenuItem(TDocket docket, WhsValueObjectDataAdapter<TDocket, TValueObject> adapter)
		{
			this.Docket = docket;
			this.Adapter = adapter;
		}

		protected WhsDocketExportMenuItem(TDocket docket)
			: this(docket, null)
		{
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			ExportDirector.RunExport(Docket);
		}

		#region Director

		public WhsXmlExportDirector<TDocket, TValueObject> ExportDirector
		{
			get { return director ?? (director = GetNewDirector()); }
		}

		protected abstract WhsXmlExportDirector<TDocket, TValueObject> GetNewDirector();
		WhsXmlExportDirector<TDocket, TValueObject> director;

#if DEBUG
		public void ResetDirectorForTest()
		{
			director = null;
		}
#endif

		#endregion

		protected readonly WhsValueObjectDataAdapter<TDocket, TValueObject> Adapter;
		protected readonly TDocket Docket;
	}
}
