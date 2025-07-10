using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class AWBHeaderManager<T> : ActiveBusinessObjectCollection<ExportAWBHeader>
		where T : BusinessObject, IAWBParent
	{
		public AWBHeaderManager(T parent)
			: base(parent.Factory, new ZQuery(ExportAWBHeaderSchema.EH_ParentID, parent.PK))
		{
			this.parent = parent;
			this.CountChanged += AWBHeaderManager_CountChanged;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { parent };
		}

		void AWBHeaderManager_CountChanged(object sender, EventArgs e)
		{
			RemoveExtraHeadersIfNeeded();
		}

		readonly T parent;

		public ExportAWBHeader AWBHeader
		{
			get
			{
				if (!parent.IsAWBHeaderAccessible)
				{
					return null;
				}

				RemoveExtraHeadersIfNeeded();

				return (Count == 1)
					? this[0]
					: LoadHeader();
			}
		}

		public ExportAWBHeader LoadExistingAWBHeader()
		{
			return Count > 0 ? AWBHeader : null;
		}

		void RemoveExtraHeadersIfNeeded()
		{
			if (!removingExtraHeaders && Count > 1)
			{
				removingExtraHeaders = true;
				try
				{
					var headersToDelete = this.Where(awb => !awb.IsInDatabase).ToArray();
					foreach (var awbHeader in headersToDelete)
					{
						awbHeader.Delete();
					}
				}
				finally
				{
					removingExtraHeaders = false;
				}
			}
		}
		bool removingExtraHeaders;

		ExportAWBHeader LoadHeader()
		{
			using (parent.SuspendSettingHasChanges())
			{
				var header = parent.LoadOrCreateAWB();

				if (!header.IsInDatabase)
				{
					header.HasChanges = false;
				}

				return header;
			}
		}

		public void Refresh()
		{
			IActiveBusinessObjectCollection collection = this;
			collection.Refresh();
			collection.RefreshBindingIncludingChildren();
		}
	}
}
