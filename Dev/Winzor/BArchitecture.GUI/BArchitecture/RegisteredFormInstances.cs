using System.Collections.Concurrent;
using System.Windows.Forms;
using Microsoft.AspNetCore.WebUtilities;

namespace WinzorFramework;

public class RegisteredFormInstances : IFormInstanceRegister
{
	public Uri Add(Uri serverBaseUrl, Form form)
	{
		var uri = Add(serverBaseUrl, form, out var _, out var _);
		return uri;
	}

	protected virtual bool Deregister(Uri serverBaseUrl, Form form, Uri formUrl)
	{
		return RemoveActiveForm(formUrl);
	}

	bool RemoveActiveForm(Uri url)
	{
		return activeForms.TryRemove(url, out var _);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
	public void Remove(Form form) => forms.Where(kvp => ReferenceEquals(form, kvp.Value)).ToList().ForEach(kvp => forms.TryRemove(kvp.Key, out var _));

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
	public virtual Uri Add(Uri serverBaseUrl, Form form, out bool isNewActiveForm, out bool isNewForm)
	{
		var formUrl = GenerateFormURL(serverBaseUrl, form);
		form.Disposed += (object? sender, EventArgs e) =>
		{
			Deregister(serverBaseUrl, form, formUrl);
		};
		isNewForm = forms.TryAdd(formUrl, form);
		isNewActiveForm = activeForms.TryAdd(formUrl, form);
		return formUrl;
	}

	protected virtual Uri GenerateFormURL(Uri serverBaseUrl, Form form)
	{
		var query = new Dictionary<string, string?>
		{
			{ "ID", Guid.NewGuid().ToString("N") }
		};

		var uri = new Uri(QueryHelpers.AddQueryString(serverBaseUrl.OriginalString, query));
		return uri;
	}

	public Form? Lookup(Uri uri)
	{
		activeForms.TryGetValue(uri, out var form);
		return form;
	}

	public Form? ClaimFormInstance(Uri uri)
	{
		forms.TryRemove(uri, out var form);
		return form;
	}

	readonly ConcurrentDictionary<Uri, Form> forms = new ConcurrentDictionary<Uri, Form>();
	readonly ConcurrentDictionary<Uri, Form> activeForms = new ConcurrentDictionary<Uri, Form>();
}
