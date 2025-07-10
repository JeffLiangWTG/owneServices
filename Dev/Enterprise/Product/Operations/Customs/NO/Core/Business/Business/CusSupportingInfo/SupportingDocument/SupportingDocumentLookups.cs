using System.Collections;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public abstract class SupportingDocumentLookups : Customs.Business.CusSupportingInfoLookups
	{
		protected SupportingDocumentLookups(SupportingDocument parent)
			: base(parent)
		{
		}
		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public sealed override ICollection CodeList
		{
			get
			{
				var declaration = Parent.Declaration;
				if (declaration == null)
				{
					return new CodeDescriptionPairList();
				}

				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Norway, ListType, declaration.DateOfValuation);
			}
		}

		protected abstract string ListType { get; }
	}
}
