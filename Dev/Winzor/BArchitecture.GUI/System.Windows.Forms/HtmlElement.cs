#nullable disable

namespace System.Windows.Forms;

public class HtmlElement
{
	public string GetAttribute(string attributeName)
	{
		object attribute = NativeHtmlElement.GetAttribute(attributeName, 0);
		if (attribute != null)
		{
			return attribute.ToString();
		}

		return "";
	}

	UnsafeNativeMethods.IHTMLElement NativeHtmlElement => htmlElement;

	readonly UnsafeNativeMethods.IHTMLElement htmlElement;

	public void AttachEventHandler(string eventName, EventHandler eventHandler)
	{
	}

	public string Name { get; set; }

	public object InvokeMember(string methodName)
	{
		return InvokeMember(methodName, null);
	}

	public object InvokeMember(string methodName, params object[] parameter)
	{
		return null;
	}
}

internal interface UnsafeNativeMethods
{
	public interface IHTMLElement
	{
		object GetAttribute(string attributeName, int index);
	}
}
