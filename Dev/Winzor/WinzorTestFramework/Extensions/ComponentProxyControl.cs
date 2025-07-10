#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Diffing.Extensions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace WinzorTestFramework;

public class ComponentProxyControl<TComponent> : Control, IInAppComponent<TComponent>
	where TComponent : IComponent
{
	readonly ComponentParameterCollection parameters;

	public TComponent? Instance { get; private set; }

	public ComponentProxyControl()
	{
		parameters = new ComponentParameterCollection();
	}

	public ComponentProxyControl(Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null) : this()
	{
		if (parameterBuilder is not null)
		{
			TryUpdateParameters(parameterBuilder);
		}
	}

	public async Task UpdateParametersAsync(Action<ComponentParameterCollectionBuilder<TComponent>> action)
	{
		if (TryUpdateParameters(action))
		{
			await InvokeStateHasChangedAsync();
		}
	}

	protected internal override bool ShouldRender => Visible;

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		var cascadingValues = new Queue<ComponentParameter>(parameters.Where(x => x.IsCascadingValue));
		void RenderWithCascadingValues(RenderTreeBuilder builder)
		{
			if (cascadingValues.Count > 0)
			{
				RenderCascadingValue(builder, cascadingValues.Dequeue(), RenderWithCascadingValues);
			}
			else
			{
				RenderInternalComponent(builder);
			}
		}
		RenderWithCascadingValues(builder);
	}

	bool TryUpdateParameters(Action<ComponentParameterCollectionBuilder<TComponent>> action)
	{
		var builder = new ComponentParameterCollectionBuilder<TComponent>(action);
		var newParams = builder.Build();

		if (newParams.Count > 0)
		{
			CheckParameters(newParams);
			foreach (var param in newParams)
			{
				var toRemove = parameters.Where(x =>
					param.IsCascadingValue == x.IsCascadingValue
					&& (param.Value is null || (param.IsCascadingValue && param.Value.GetType() == x.Value?.GetType()))
					&& Equals(param.Name, x.Name)
				);

				foreach (var oldParameter in toRemove)
				{
					parameters.Remove(oldParameter);
				}
			}
			parameters.AddRange(newParams);
			return true;
		}
		return false;
	}

	void RenderCascadingValue(RenderTreeBuilder builder, ComponentParameter cascadingValue, RenderFragment content)
	{
		builder.OpenComponent(0, typeof(CascadingValue<>).MakeGenericType(cascadingValue.Value!.GetType()));

		if (cascadingValue.Name is not null)
		{
			builder.AddAttribute(1, nameof(CascadingValue<object>.Name), cascadingValue.Name);
		}

		builder.AddAttribute(2, nameof(CascadingValue<object>.Value), cascadingValue.Value);
		builder.AddAttribute(3, nameof(CascadingValue<object>.IsFixed), value: true);
		builder.AddAttribute(4, nameof(CascadingValue<object>.ChildContent), content);

		builder.CloseComponent();
	}

	void RenderInternalComponent(RenderTreeBuilder builder)
	{
		builder.OpenComponent<TComponent>(0);

#pragma warning disable ASP0006
		int sequence = 100;

		foreach (var parameter in parameters.Where(x => !x.IsCascadingValue))
		{
			builder.AddAttribute(sequence++, parameter.Name!, parameter.Value);
		}

		builder.AddComponentReferenceCapture(sequence, o => Instance = (TComponent)o);
#pragma warning restore ASP0006
		builder.CloseComponent();
	}

	void CheckParameters(ComponentParameterCollection parametersToCheck)
	{
		var unnamedNonCascading = parametersToCheck
			.Where(p => p.Name is null && !p.IsCascadingValue);
		if (unnamedNonCascading.Any())
		{
			throw new InvalidOperationException("All non-cascading parameters must have a name");
		}

		var ordinaryDuplicates = parametersToCheck
			.Where(p => p.Name is not null && !p.IsCascadingValue)
			.GroupBy(p => p.Name)
			.Where(g => g.Count() > 1);
		foreach (var paramGroup in ordinaryDuplicates)
		{
			throw new InvalidOperationException($"Duplicate named non-cascading parameter \"{paramGroup.Key}\"");
		}

		var namedCascadingDuplicate = parametersToCheck
			.Where(p => p.Name is not null && p.IsCascadingValue)
			.GroupBy(p => (p.Name, Type: p.Value?.GetType()))
			.Where(g => g.Count() > 1);
		foreach (var paramGroup in namedCascadingDuplicate)
		{
			throw new InvalidOperationException($"Duplicate named cascading parameter with name \"{paramGroup.Key.Name}\" and type \"{paramGroup.Key.Type}\"");
		}

		var unnamedDuplicates = parameters
			.Where(p => p.Name is null && p.IsCascadingValue)
			.GroupBy(
				p => p.Value?.GetType())
			.Where(g => g.Count() > 1);

		foreach (var paramGroup in unnamedDuplicates)
		{
			throw new InvalidOperationException($"Duplicate unnamed cascading parameter with type \"{paramGroup.Key}\"");
		}
	}
}
