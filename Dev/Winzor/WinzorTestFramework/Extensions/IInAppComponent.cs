#nullable enable
using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace WinzorTestFramework;

/**
 * Something based on IRenderedComponent for use in our playwright tests.
 */
public interface IInAppComponent<TComponent> where TComponent : IComponent
{
	TComponent? Instance { get; }
	public Task UpdateParametersAsync(Action<ComponentParameterCollectionBuilder<TComponent>> action);
}
