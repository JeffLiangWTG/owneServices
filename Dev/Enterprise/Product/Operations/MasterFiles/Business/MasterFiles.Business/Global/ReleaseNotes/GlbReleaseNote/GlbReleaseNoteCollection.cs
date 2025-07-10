using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbReleaseNoteCollection : ActiveBusinessObjectCollection<GlbReleaseNote>
	{
		public GlbReleaseNoteCollection(BusinessObjectFactory factory, params string[] additionalSections)
			: base(factory, new ZQuery(GlbReleaseNoteSchema.GF_Section, MergeSections(additionalSections)))
		{
		}

		public GlbReleaseNoteCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		static string[] MergeSections(string[] additionalSections)
		{
			var result = new string[] { NewsSectionTypeList.Codes.ProductUpdates };
			if (additionalSections != null)
			{
				result = result.Union(additionalSections).ToArray();
			}

			return result;
		}

		public bool CategoryIsVisible(GlbReleaseNote element)
		{
			if (DataRegistry.Instance.ProductivityWiseModeEnabled && !element.GF_Category.IsEmpty && !VisibleCategories.Contains(element.GF_Category))
			{ return false; }

			return true;
		}

		protected override bool MatchesFilterCore(GlbReleaseNote element, bool fetchOnlyFromLocalCache)
		{
			if (!CategoryIsVisible(element))
			{ return false; }

			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache);
		}

		HashSet<string> VisibleCategories
		{
			get
			{
				if (visibleCategories == null)
				{
					visibleCategories = new GlbReleaseNoteLookups(null).VisibleCategories;
				}
				return visibleCategories;
			}
		}
#if DEBUG
		[SuppressCollectionStateTest]
#endif
		HashSet<string> visibleCategories;

		protected override void SetDefaultsForNewElementCore(GlbReleaseNote newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
		}
	}
}
