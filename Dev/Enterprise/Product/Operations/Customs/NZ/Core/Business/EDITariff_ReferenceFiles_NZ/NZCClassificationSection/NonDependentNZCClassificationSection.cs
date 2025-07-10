using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NonDependentNZCClassificationSectionCollection : BusinessObjectCollection<NZCClassificationSection>, IFamilyMemberCollection
	{
		public NonDependentNZCClassificationSectionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override void Load()
		{
			base.Load();
			Sort(NZCClassificationSection.Schema.SectionPadded, ListSortDirection.Ascending);
		}

		public ITraversibleNode GetNearestNodeForCode(ZString code)
		{
			code = code.Trim();
			ITraversibleNode result = null;
			while (code.Length > 0 && result == null)
			{
				if (code.Length <= 2)
				{
					result = Factory.LoadTop1<NZCClassificationChapter>(new ZQuery(NZCClassificationChapterSchema.Q2_Chapter, code));
					code = "";
				}
				else
				{
					result = NZCClassification.GetClassForPartialCode(Factory, code, ZDateTime.Today);
					int chopPosition = code.LastIndexOf('.');
					code = code.Substring(0, code.Length - 1).TrimEnd('.').TrimEnd(' ').Trim('.');  // Need both dot removals
				}
			}
			return result;
		}

		public CargoWise.EntityFramework.IFamilyMember[] FamilyMembers
		{
			get { return this.Cast<CargoWise.EntityFramework.IFamilyMember>().ToArray(); }
		}
	}
}
