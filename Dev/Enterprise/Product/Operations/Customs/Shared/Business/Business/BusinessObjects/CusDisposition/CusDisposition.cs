using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusDisposition : AutoCusDisposition
	{
		public new class Schema : AutoCusDisposition.Schema
		{
			public const string StatusDescription = "StatusDescription";
		}

		public CusDisposition(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.CusDispositionFetchStrategy(this);
		}

		#endregion

		public BusinessObject Parent
		{
			get
			{
				if (fParent == null)
				{
					SetParentFromParentTableCodeAndParentIDIfPossible();
				}
				return fParent;
			}
			set { SetParent(value); }
		}
		BusinessObject fParent;

		void SetParentFromParentTableCodeAndParentIDIfPossible()
		{
			if (!CDI_ParentTableCode.IsEmpty && !CDI_ParentID.IsEmpty)
			{
				fParent = Factory.Load(CDI_ParentTableCode, CDI_ParentID);
			}
		}

		protected void SetParent(BusinessObject parent)
		{
			fParent = parent;
			if (fParent != null && (CDI_ParentID != fParent.PK || CDI_ParentTableCode != fParent.TablePrefix))
			{
				CDI_ParentID = fParent.PK;
				CDI_ParentTableCode = fParent.TablePrefix;
			}
		}

		public ZString StatusDescription
		{
			get
			{
				var result = ZString.Empty;
				var parent = Parent as ICusDispositionParent;
				if (parent != null)
				{
					result = parent.GetStatusDescription(CDI_Status);
				}
				return result;
			}
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(CusDisposition.Schema.StatusDescription); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CDI_Notes = "";
		}
	}
}
