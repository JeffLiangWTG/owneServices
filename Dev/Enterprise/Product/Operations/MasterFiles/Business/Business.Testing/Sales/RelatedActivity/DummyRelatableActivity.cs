using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DummyRelatableActivity : DummyBusinessObject, IRelatableActivity
	{
		public DummyRelatableActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			DummyBaseBusinessObject.TypeDecider.AddTypeForLoadOverride((r, f) => (Guid)r[DummyBizoSchema.Constants.PK] == PK, GetType());
		}

		public void OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
			RelatedActivitySaved = relatedActivity;
		}

		public IRelatableActivity RelatedActivitySaved { get; private set; }

		public ZString ActivityType
		{
			get;
			set;
		}

		public IOrgHeader Client
		{
			get;
			set;
		}

		public ZBool ClientHasChanges
		{
			get;
			set;
		}

		public IOrgContact Contact
		{
			get;
			set;
		}

		public ZBool ContactHasChanges
		{
			get;
			set;
		}

		public ZString Summary
		{
			get;
			set;
		}

		public ZDateTime SystemCreateTimeUtc
		{
			get;
			set;
		}

		public ZString SystemCreateUser
		{
			get;
			set;
		}

		public ZDateTime SystemLastEditTimeUtc
		{
			get;
			set;
		}

		public ZString SystemLastEditUser
		{
			get;
			set;
		}

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityCollection == null)
				{
					relatedChildActivityCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityCollection;
			}
			set { relatedChildActivityCollection = value; }
		}
		IRelatedChildActivityPivotCollection relatedChildActivityCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityCollection == null)
				{
					relatedParentActivityCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityCollection;
			}
			set { relatedParentActivityCollection = value; }
		}
		IRelatedParentActivityPivotCollection relatedParentActivityCollection;

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;
	}
}
