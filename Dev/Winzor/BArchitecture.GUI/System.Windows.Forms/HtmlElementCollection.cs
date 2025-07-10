#nullable disable

using System.Collections;

namespace System.Windows.Forms;

public class HtmlElementCollection : ICollection, IEnumerable
{
	public int Count { get; }

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	public HtmlElement this[int index]
	{
		get { return null; }
	}

	public void CopyTo(Array array, int index)
	{
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			array.SetValue(this[i], index++);
		}
	}

	public IEnumerator GetEnumerator()
	{
		HtmlElement[] array = new HtmlElement[Count];
		((ICollection)this).CopyTo(array, 0);
		return array.GetEnumerator();
	}
}
