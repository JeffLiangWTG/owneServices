using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationSection : AutoNZCClassificationSection, IFamilyMember, ITraversibleNode
	{
		public new class Schema : AutoNZCClassificationSection.Schema
		{
			public const string SectionPadded = "SectionPadded";
		}

		public NZCClassificationSection(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public NZCClassificationChapterCollection Chapters
		{
			get
			{
				if (fChapters == null)
				{
					fChapters = new NZCClassificationChapterCollection(this);
					fChapters.Load();
				}
				return fChapters;
			}
		}

		#region Implementation

		protected NZCClassificationChapterCollection fChapters;

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		public ZString SectionPadded
		{
			get { return Q1_Section.PadLeft(2, '0'); }
		}

		#endregion

		#region IFamilyMember Members

		public ZString WrappedLongDescription
		{
			get { return Q1_Description; }
		}

		public ZPropertyInfo WrappedLongDescriptionInfo
		{
			get { return Q1_DescriptionInfo; }
		}

		public ZString StatUnit
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo StatUnitInfo
		{
			get { return GetZPropertyInfo(nameof(StatUnit)); }
		}

		public ZString SuppUnit
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo SuppUnitInfo
		{
			get { return GetZPropertyInfo(nameof(SuppUnit)); }
		}

		#endregion

		#region IFamilyMember Members

		public bool HasChildren
		{
			get { return true; }
		}

		public CargoWise.EntityFramework.IFamilyMember[] Children
		{
			get { return (CargoWise.EntityFramework.IFamilyMember[])Chapters.ToArray(typeof(CargoWise.EntityFramework.IFamilyMember)); }
		}

		public ZPropertyInfo LongDescriptionInfo
		{
			get { return WrappedLongDescriptionInfo; }
		}

		public string ShortDescription
		{
			get { return Q1_Section + " " + Q1_Description; }
		}

		public ZString LongDescription
		{
			get { return WrappedLongDescription; }
		}

		#endregion

		#region ITraversibleNode Members

		public CargoWise.EntityFramework.IFamilyMember[] GetHierarchy()
		{
			return new IFamilyMember[] { this };
		}

		#endregion
	}
}
