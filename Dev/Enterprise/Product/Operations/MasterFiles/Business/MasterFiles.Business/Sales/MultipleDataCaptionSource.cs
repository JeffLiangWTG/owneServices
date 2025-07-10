using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MultipleDataCaptionSource : ICustomizableDataCaptionSource
	{
		readonly ZPropertyInfo[] infos;
		public MultipleDataCaptionSource(ZPropertyInfo[] infos, ZString description)
		{
			this.infos = infos;
			Description = description.SubstringSafe(0, 20);
		}

		protected IEnumerable<ZPropertyInfoString> StringCaptionProperties { get; }

		public string GetKey(object context, string caption)
		{
			foreach (var info in infos)
			{
				if (info.Value.Equals(caption))
				{
					return info.CustomizableDataResourceStrings.Source.GetKey(context, caption);
				}
			}

			return infos[0].CustomizableDataResourceStrings.Source.GetKey(context, caption);
		}

		public IEnumerable<IResString> GetCompileTimeSystemCaptions()
		{
			return null;
		}

		public IEnumerable<IResString> GetRuntimeCaptions(IResString userCaption = null, object context = null)
		{
			return new HashSet<IResString>(infos.SelectMany(info => info.CustomizableDataResourceStrings.Source.GetRuntimeCaptions(userCaption, info.BizObj)), new ResourceString.ResourceKeyEqualityComparer());
		}

		public string Description { get; }

		public int MaxLength
		{
			get
			{
				var max = infos.OrderByDescending(i => i.MaxLength).FirstOrDefault();
				if (max != null)
				{
					return max.MaxLength;
				}

				return -1;
			}
		}

		public int GetMaxLength(ZString caption)
		{
			int result = 0;

			foreach (var property in infos)
			{
				if (property.Value.Equals(caption))
				{
					result = property.MaxLength;
					break;
				}
			}

			if (result == 0)
			{
				result = MaxLength;
			}

			return result;
		}

		public ushort Asmid
		{
			get { return infos[0].CustomizableDataResourceStrings.Source.Asmid; }
			set { }
		}
	}
}
