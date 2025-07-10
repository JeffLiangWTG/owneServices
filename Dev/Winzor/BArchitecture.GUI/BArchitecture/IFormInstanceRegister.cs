using System.Windows.Forms;

namespace WinzorFramework;

public interface IFormInstanceRegister
{
	Uri Add(Uri serverBaseUrl, Form form);

	Uri Add(Uri serverBaseUrl, Form form, out bool isNewActiveForm, out bool isNewForm);

	Form? Lookup(Uri uri);

	Form? ClaimFormInstance(Uri uri);

	void Remove(Form form);
}
