using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace WinzorFramework.JSInterop;
public abstract class JsInteropBaseWithSpecificExceptionIgnore : JSInteropBase
{
	protected JsInteropBaseWithSpecificExceptionIgnore(IJSRuntime jsRuntime, string moduleSrc, IFileVersionHash fileVersionHash) : base(jsRuntime, moduleSrc, fileVersionHash)
	{
	}

	protected override async Task InvokeJsAsync(string identifier, params object?[] args)
	{
		await ExceptionHandlerExtension.HandleJSExceptionAsync(() => base.InvokeJsAsync(identifier, args));
	}
}
