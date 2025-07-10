using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	partial class AutoAPHISProduct
	{
		public APHISHeader Header
		{
			get { return (APHISHeader)Parent; }
		}

		public ZString ProgramType
		{
			get
			{
				if (programTypeCached == null)
				{
					programTypeCached = new CachedProperty<ZString>(Factory, () =>
					{
						var header = Header;
						return header == null ? ZString.Empty : header.US_ProgramType;
					});
				}
				return programTypeCached.Value;
			}
		}
		CachedProperty<ZString> programTypeCached;

		public ZString CategoryType
		{
			get
			{
				if (categoryTypeCached == null)
				{
					categoryTypeCached = new CachedProperty<ZString>(Factory, () =>
					{
						var header = Header;
						return header == null ? ZString.Empty : header.US_CategoryType;
					});
				}
				return categoryTypeCached.Value;
			}
		}
		CachedProperty<ZString> categoryTypeCached;

		public ZString CategoryCode
		{
			get
			{
				if (categoryCodeCached == null)
				{
					categoryCodeCached = new CachedProperty<ZString>(Factory, () =>
					{
						var header = Header;
						return header == null ? ZString.Empty : header.US_CategoryCode;
					});
				}
				return categoryCodeCached.Value;
			}
		}
		CachedProperty<ZString> categoryCodeCached;

		protected bool IsNotLiveAnimals
		{
			get { return CategoryType != APHISCategoryTypeCodeList.Codes.LiveAnimals; }
		}
	}
}
