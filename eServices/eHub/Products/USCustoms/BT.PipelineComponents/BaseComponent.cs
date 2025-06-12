using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.BizTalk.Component.Interop;

namespace CargoWise.eHub.Products.USCustoms.BT.PipelineComponents
{
	public abstract class BaseComponent : IBaseComponent, IPersistPropertyBag, IComponentUI
	{
		protected abstract string DisplayName { get; }
		protected abstract Guid ClassID { get; }

		[Browsable(false)]
		public string Name { get { return DisplayName; } }
		[Browsable(false)]
		public string Description { get { return DisplayName; } }
		[Browsable(false)]
		public virtual string Version { get { return "1.0"; } }
		[Browsable(false)]
		public IntPtr Icon { get { return IntPtr.Zero; } }

		public void InitNew() { }

		public virtual IEnumerator Validate(object obj)
		{
			return null;
		}

		public void GetClassID(out Guid classid)
		{
			classid = ClassID;
		}

		public void Load(IPropertyBag pb, int errlog)
		{
			foreach (var prop in GetComponentProperties())
			{
				try
				{
					object val = null;
					pb.Read(prop.Name, out val, 0);
					if (val != null)
						prop.SetValue(this, val, null);
				}
				catch (System.ArgumentException) { }
			}
		}

		public void Save(IPropertyBag pb, bool fClearDirty, bool fSaveAllProperties)
		{
			foreach (var prop in GetComponentProperties())
			{
				object val = prop.GetValue(this, null);
				pb.Write(prop.Name, ref val);
			}
		}

		PropertyInfo[] GetComponentProperties()
		{
			return propertyInfos.GetOrAdd(this.GetType(), this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => { var b = Attribute.GetCustomAttribute(p, typeof(BrowsableAttribute)) as BrowsableAttribute; return b == null ? true : b.Browsable; }).ToArray());
		}

		static ConcurrentDictionary<Type, PropertyInfo[]> propertyInfos = new ConcurrentDictionary<Type, PropertyInfo[]>();
	}
}