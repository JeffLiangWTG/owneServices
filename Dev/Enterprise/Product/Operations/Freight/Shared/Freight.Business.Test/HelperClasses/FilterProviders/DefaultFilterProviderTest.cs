using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestsSubclassesOf(typeof(DefaultFilterProvider))]
	public abstract class DefaultFilterProviderTest<ProviderT> : BaseFreightTest
			where ProviderT : DefaultFilterProvider
	{
		public void TestDefaultValuesShouldNotAddDefaults()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults("should not have added any filters", Collection);
		}

		[ExpectNoExceptions]
		public void TestDoNotThrowExceptionWhenApplyingAllFilters()
		{
			Populate(Provider);
			Provider.SetDefaultFilters(Collection);
			Filter.SetExternalDefaults(Collection.FilterBusinessObjectDefaults);
		}

		[ExpectNoExceptions]
		public void TestFilterBusinessObjectDefaultsAreResourceStringFree()
		{
			Populate(Provider);

			var markerResource = new ResourceStringData("", "", "", "xxx", "adults only");

			using (var cache = Res.UseMockData())
			{
				cache.SetResourceGetter(key => ExcludedResourcesForResourceStringFreeTest.Contains(key) ? cache.GetDefaultResource(key) : markerResource);

				Provider.SetDefaultFilters(Collection);
				Filter.SetExternalDefaults(Collection.FilterBusinessObjectDefaults);
				var errorMessage = "{0} is an identifier and must not be a translatable resource string (i.e. must be English constant). Breaching filter: {1}.";
				foreach (FilterBusinessObjectDefault fboDefault in Collection.FilterBusinessObjectDefaults)
				{
					AssertNotEquals(string.Format(errorMessage, "Filter name", fboDefault.FilterName), "xxx", fboDefault.FilterName);
					AssertNotEquals(string.Format(errorMessage, "Property name", fboDefault.FilterName), "xxx", fboDefault.PropertyName);
					AssertNotEquals(string.Format(errorMessage, "Value", fboDefault.FilterName), "xxx", fboDefault.Value);
				}
			}
		}

		protected virtual List<string> ExcludedResourcesForResourceStringFreeTest
		{
			get { return new List<string>(); }
		}

		#region Implementation

		protected FilterBusinessObject Filter
		{
			get { return filter ?? (filter = GetModuleProperty<FilterBusinessObject>("FilterBusinessObject")); }
		}
		FilterBusinessObject filter;

		protected IFilterBusinessObjectDefaultsProvider Collection
		{
			get { return collection ?? (collection = GetModuleProperty<IFilterBusinessObjectDefaultsProvider>("GridCollection")); }
		}
		IFilterBusinessObjectDefaultsProvider collection;

		protected IZModule Module
		{
			get { return module ?? (module = ObjectFactory.Get<IModuleFactory>().Create(ModuleID)); }
		}
		IZModule module;

		protected ProviderT Provider
		{
			get { return provider ?? (provider = CreateProviderInstance()); }
		}
		ProviderT provider;

		protected override void TearDown()
		{
			using (module)
			{
				base.TearDown();
			}
		}

		ReturnT GetModuleProperty<ReturnT>(string name)
		{
			PropertyInfo info = Module.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			return (ReturnT)info.GetValue(Module, Array.Empty<object>());
		}

		protected virtual ProviderT CreateProviderInstance()
		{
			return Activator.CreateInstance<ProviderT>();
		}

		protected virtual void Populate(ProviderT provider)
		{
			foreach (PropertyInfo info in typeof(ProviderT).GetProperties())
			{
				Populate(provider, info);
			}
		}
		protected virtual void Populate(ProviderT provider, PropertyInfo info)
		{
			if (info.PropertyType == typeof(ZString))
			{
				Populate(provider, info, "A");
			}
			else if (info.PropertyType == typeof(ZDateTime))
			{
				Populate(provider, info, ZDateTime.Now);
			}
			else if (info.PropertyType == typeof(ZGuid))
			{
				Populate(provider, info, ZGuid.NewZGuid());
			}
			else if (info.PropertyType == typeof(ZBool) || info.PropertyType == typeof(ZBool?))
			{
				Populate(provider, info, true);
			}
			else
			{
				throw new InvalidOperationException("dont know how to default " + info.PropertyType.Name);
			}
		}
		protected void Populate(ProviderT provider, PropertyInfo info, object value)
		{
			if (value != null && value.GetType() == info.PropertyType)
			{
				info.SetValue(provider, value, Array.Empty<object>());
			}
			else
			{
				TypeConverter converter = TypeDescriptor.GetConverter(info.PropertyType);
				info.SetValue(provider, converter.ConvertFrom(value), Array.Empty<object>());
			}
		}

		#endregion

		protected abstract ModuleIdentifier ModuleID { get; }
	}
}
