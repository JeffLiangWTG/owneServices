//
// Summary:
//     Defines a message filter interface.

namespace System.Windows.Forms
{
	public interface IMessageFilter
	{
		//
		// Summary:
		//     Filters out a message before it is dispatched.
		//
		// Parameters:
		//   m:
		//     The message to be dispatched. You cannot modify this message.
		//
		// Returns:
		//     true to filter the message and stop it from being dispatched; false to allow
		//     the message to continue to the next filter or control.
		bool PreFilterMessage(ref Message m);
	}
}
