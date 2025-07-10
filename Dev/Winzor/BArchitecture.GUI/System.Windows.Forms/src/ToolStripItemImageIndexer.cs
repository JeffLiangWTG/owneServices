using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
	public partial class ToolStripItem
	{
		internal class ToolStripItemImageIndexer : ImageList.Indexer
		{
			private readonly ToolStripItem item;

			public ToolStripItemImageIndexer(ToolStripItem item)
			{
				this.item = item;
			}

			public override ImageList ImageList
			{
				get
				{
					if ((item != null) && (item.Owner != null))
					{
						return item.Owner.ImageList;
					}
					return null;
				}
				set { Debug.Assert(false, "We should never set the image list"); }
			}
		}
	}
}
