using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class FamilyMemberCollectionForBinding : NonPersistentBusinessObjectCollection<NonPersistentBusinessObject>
	{
		public FamilyMemberCollectionForBinding()
			: base(new BusinessObjectFactory())
		{
		}

		public new IFamilyMember this[int index]
		{
			get { return Elements[index] as IFamilyMember; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FamilyMemberDummy();
		}
	}

	public class FamilyMemberDummy : NonPersistentBusinessObject, IFamilyMember, IObsoleteValidation
	{
		public FamilyMemberDummy()
		{
		}

		public ZString WrappedLongDescription
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo WrappedLongDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(WrappedLongDescription)); }
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

		public ZDecimal DutyRate
		{
			get { return 0m; }
		}

		public ZPropertyInfo DutyRateInfo
		{
			get { return GetZPropertyInfo(nameof(DutyRate)); }
		}

		#region IFamilyMember Members

		public bool HasChildren
		{
			get { return false; }
		}

		public CargoWise.EntityFramework.IFamilyMember[] Children
		{
			get { return System.Array.Empty<CargoWise.EntityFramework.IFamilyMember>(); }
		}

		public ZPropertyInfo LongDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(WrappedLongDescription)); }
		}

		public string ShortDescription
		{
			get { return string.Empty; }
		}

		public ZString LongDescription
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
