using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class PersonMergeBusinessObject : NonPersistentBusinessObject
	{
		public PersonMergeBusinessObject()
			: base()
		{
		}

		public PersonMergeBusinessObject(GlbPerson person) : base()
		{
			Person = person;
			activeAssociationsCache = Person.TypeInfo;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			activeAssociation = ZString.Empty;
			mergeErrorMessage = ZString.Empty;
		}

		public GlbPerson Person { get; }

		public ZString FullName => Person.PER_FullName;

		protected ZString activeAssociationsCache;

		ZString activeAssociation;

		[MaxLength(100)]
		public ZString ActiveAssociations
		{
			get
			{
				if (Person != null)
				{
					if (Person.IsDissolving)
					{
						activeAssociation = activeAssociationsCache;
					}
					else
					{
						activeAssociation = Person.TypeInfo;
					}
				}

				return activeAssociation;
			}
			set
			{
				SetNonPersistentPropertyValue(ActiveAssociationsInfo, ref activeAssociation, value);
			}
		}

		public ZPropertyInfo ActiveAssociationsInfo => GetZPropertyInfo(nameof(ActiveAssociations));

		ZString status;

		ZString mergeErrorMessage;

		public ZString MergeStatus
		{
			get => status;
			set
			{
				SetNonPersistentPropertyValue(MergeStatusInfo, ref status, value);
			}
		}

		public ZString MergeErrorMessage
		{
			get => mergeErrorMessage;
			set
			{
				SetNonPersistentPropertyValue(MergeErrorMessageInfo, ref mergeErrorMessage, value);
			}
		}

		public ZPropertyInfo MergeStatusInfo => GetZPropertyInfo(nameof(MergeStatus));

		public ZPropertyInfo MergeErrorMessageInfo => GetZPropertyInfo(nameof(MergeErrorMessage));
	}
}
