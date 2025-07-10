using System.Drawing;

namespace System.Windows.Forms;

public partial class LinkLabel
{
	public class Link
	{
		public object? LinkData { get; set; }

		public bool Enabled = true;

		internal LinkLabel? Owner;

		/// <summary>
		///  Description for accessibility
		/// </summary>
		public string? Description { get; set; }

		string? name;

		/// <summary>
		///  The name for the link - useful for indexing by key.
		/// </summary>
		public string Name
		{
			get => name ?? string.Empty;
			set => name = value;
		}

		int start;

		internal int length;

		public int Start
		{
			get => start;
			set
			{
				start = value;
				Owner?.NotifyRenderRequired();
			}
		}

		public int Length
		{
			get => length;
			set
			{
				length = value;
				Owner?.NotifyRenderRequired();
			}
		}

		public object? Tag { get; set; }

		public bool Visited { get; set; }

		public Link()
		{
		}

		public Link(int start, int length)
		{
			this.start = start;
			this.length = length;
		}

		public Link(int start, int length, object linkData)
		{
			this.start = start;
			this.length = length;
			LinkData = linkData;
		}

		internal Link(LinkLabel owner)
		{
			Owner = owner;
		}

		internal Region? VisualRegion { get; set; }
	}
}
