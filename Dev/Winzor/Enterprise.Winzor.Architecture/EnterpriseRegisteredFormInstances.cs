using System;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Common;
using CargoWiseNext.Infrastructure.Installations;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;

namespace Enterprise.Winzor.Architecture;

public class EnterpriseRegisteredFormInstances : RegisteredFormInstances
{
	public EnterpriseRegisteredFormInstances()
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
	public override Uri Add(Uri serverBaseUrl, Form form, out bool isNewActiveForm, out bool isNewForm)
	{
		var uri = base.Add(serverBaseUrl, form, out isNewActiveForm, out isNewForm);

		if (isNewActiveForm && form is ZForm zf)
		{
			zf.Shown += (s, e) => SetCrashRecoveryUrl(serverBaseUrl);
			zf.DisplayModeChanged += (s, e) => SetCrashRecoveryUrl(serverBaseUrl);
		}
		return uri;
	}

	protected override Uri GenerateFormURL(Uri serverBaseUrl, Form form)
	{
		string query = null;
		if (form is ZForm zForm)
		{
			var shortcutUrl = ZFormUtilities.BusinessEntityShortcutUrl(zForm, useWebHyperlinks: false);
			if (!string.IsNullOrEmpty(shortcutUrl))
			{
				query = UrlHandler.GetQueryStringTextFromUrl(shortcutUrl);
			}
		}

		if (string.IsNullOrEmpty(query))
		{
			var qs = new QueryString
			{
				{ (NoResString)"Command", ShowTransientFormUrlCommand },
				{ "ID", Guid.NewGuid().ToString("N") }
			};
			query = qs.ToString();
		}

		var uri = new UriBuilder(serverBaseUrl) { Query = query }.Uri;
		return uri;
	}

	protected override bool Deregister(Uri serverBaseUrl, Form form, Uri formUri)
	{
		var deregistered = base.Deregister(serverBaseUrl, form, formUri);
		if (deregistered && form is ZForm)
		{
			SetCrashRecoveryUrl(serverBaseUrl);
		}
		return deregistered;
	}

	public const string ShowTransientFormUrlCommand = "ShowTransientForm";

	void SetCrashRecoveryUrl(Uri serverBaseUrl)
	{
		var uriBuilder = new UriBuilder(serverBaseUrl);
		var persist = WindowPersister.GetOpenFormUrls();

		if (!string.IsNullOrEmpty(persist))
		{
			var query = new QueryString
			{
				{ (NoResString)"persist", persist }
			};
			uriBuilder.Query = query.ToString();
		}
		var recoveryUrl = uriBuilder.Uri.ToString();

		Initialization.MainFormInstance?.Invoke(async () =>
		{
			var jsRuntime = Initialization.MainFormInstance.CargoWiseClientServices?.JSRuntime;
			if (null != jsRuntime)
			{
				await ExceptionHandlerExtension.HandleJSExceptionAsync(async () =>
				{
					await jsRuntime.InvokeVoidAsync("localStorage.setItem", LocalStorageItemKeys.CrashRecoveryUrl, recoveryUrl);
				});
			}
		});
	}
}
