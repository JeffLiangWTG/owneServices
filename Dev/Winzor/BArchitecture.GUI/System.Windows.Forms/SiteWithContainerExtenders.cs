using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Windows.Forms;

// In .NET 5 and 6 ComponentModel code differs from .NET Framework such that IExtenderProviders from the site container are no longer used
// https://github.com/dotnet/runtime/blob/release/5.0/src/libraries/System.ComponentModel.TypeConverter/src/System/ComponentModel/ReflectTypeDescriptionProvider.cs#L598-L607
// This causes the KBindingSource+Test.TestExtenderProviderImpl test to fail and likely some functionality of KBindingSource to not work
// As a workaround we can expose the container IExtenderProvider via the IExtenderListService
// The ComponentModel code has since been fixed: https://github.com/dotnet/runtime/commit/ae20c2e14906920d19cf9618a42ad0f7e2df03ba#diff-e7462847968c4e5d2d36cebb657208f5cf2f6ce4e3c59a9ba936a6ef343c92d0
// Once we have ugpraded to a .NET version with the fixed code we can remove this class
class SiteWithContainerExtenders : ISite, IExtenderListService
{
	public SiteWithContainerExtenders(ISite site)
	{
		this.inner = site;
	}

	public IComponent Component => inner.Component;

	public IContainer? Container => inner.Container;

	public bool DesignMode => inner.DesignMode;

	public string? Name { get => inner.Name; set => inner.Name = value; }

	public object? GetService(Type serviceType)
	{
		if (serviceType == typeof(IExtenderListService))
		{
			return this;
		}
		return inner.GetService(serviceType);
	}

	public IExtenderProvider[] GetExtenderProviders()
	{
		return Container?.Components.Cast<object>().Where(o => o is IExtenderProvider).Cast<IExtenderProvider>().ToArray() ?? Array.Empty<IExtenderProvider>();
	}

	readonly ISite inner;
}
