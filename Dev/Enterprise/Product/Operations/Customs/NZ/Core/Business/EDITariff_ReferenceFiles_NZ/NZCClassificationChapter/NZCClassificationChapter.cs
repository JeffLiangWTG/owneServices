using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationChapter : AutoNZCClassificationChapter, IFamilyMember, ITraversibleNode
	{
		public NZCClassificationChapter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		public NZCClassificationCollection Tariffs
		{
			get
			{
				if (fTariffs == null)
				{
					fTariffs = new NZCClassificationCollection(this);
					fTariffs.Load();
				}
				return fTariffs;
			}
		}

		public NZCClassificationSection Section
		{
			get { return (NZCClassificationSection)Factory.Load(typeof(NZCClassificationSection), Q2_Q1_Section); }
		}

		#region Implementation

		protected NZCClassificationCollection fTariffs;
		#endregion

		#region IFamilyMember Members

		public ZString WrappedLongDescription
		{
			get { return Q2_Description; }
		}

		public ZPropertyInfo WrappedLongDescriptionInfo
		{
			get { return Q2_DescriptionInfo; }
		}

		public ZString StatUnit
		{
			get { return new ZString(); }
		}

		public ZPropertyInfo StatUnitInfo
		{
			get { return GetZPropertyInfo(nameof(StatUnit)); }
		}

		public ZString SuppUnit
		{
			get { return new ZString(); }
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
			get { return (CargoWise.EntityFramework.IFamilyMember[])Tariffs.ToArray(typeof(CargoWise.EntityFramework.IFamilyMember)); }
		}

		public ZPropertyInfo LongDescriptionInfo
		{
			get { return Q2_DescriptionInfo; }
		}

		public string ShortDescription
		{
			get { return Q2_Chapter + " " + Q2_Description; }
		}

		public ZString LongDescription
		{
			get { return Q2_Description; }
		}

		#endregion

		#region ITraversibleNode Members

		public CargoWise.EntityFramework.IFamilyMember[] GetHierarchy()
		{
			ArrayList list = new ArrayList();
			list.Add(this);
			list.AddRange(Section.GetHierarchy());
			return (IFamilyMember[])list.ToArray(typeof(IFamilyMember));
		}

		#endregion
	}
}
