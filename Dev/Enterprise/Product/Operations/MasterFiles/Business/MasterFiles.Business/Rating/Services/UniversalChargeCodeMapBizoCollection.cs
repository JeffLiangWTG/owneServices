using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Rating.Services
{
	public class UniversalChargeCodeMapBizoCollection : NonPersistentBusinessObjectCollection<UniversalChargeCodeMapBizo>
	{
		public UniversalChargeCodeMapBizoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UniversalChargeCodeMapBizo(Factory);
		}

		public UniversalChargeCodeMapBizoCollection CopyCodeAndDescriptionToAnotherFactory(BusinessObjectFactory anotherFactory)
		{
			var copy = new UniversalChargeCodeMapBizoCollection(anotherFactory);
			foreach (UniversalChargeCodeMapBizo biz in this)
			{
				copy.AddNew(biz.Code, biz.Description);
			}
			return copy;
		}

		public SecurityCheckpoint[] RequiredSecurityRights(SecurityCore security)
		{
			bool needGlobal = false;
			bool needLinked = false;
			bool needBasic = false;
			var rights = new List<SecurityCheckpoint>();

			foreach (UniversalChargeCodeMapBizo biz in this)
			{
				if (!needGlobal && !biz.GlobalChargeCodePk.IsEmpty)
				{
					needGlobal = true;
					rights.Add(security.GlobalChargeCodesModify);
				}

				if ((!needLinked || !needBasic) && !biz.LocalChargeCodePk.IsEmpty)
				{
					var local = biz.LocalChargeCode;
					if (local.IsLinkedToGlobalChargeCode)
					{
						if (!needLinked)
						{
							needLinked = true;
							rights.Add(security.ChargeCodesLTGModify);
						}
					}
					else if (!needBasic)
					{
						needBasic = true;
						rights.Add(security.ChargeCodesModify);
					}
				}
			}

			return rights.ToArray();
		}

		public void ApplyUniversalCodeToChargeCodeForSaving()
		{
			foreach (UniversalChargeCodeMapBizo biz in this)
			{
				biz.ApplyUniversalCodeToChargeCodeForSaving();
			}
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
		protected override ZDataTable Table => null;

		public UniversalChargeCodeMapBizo AddNew(string universalCode, string description)
		{
			var biz = new UniversalChargeCodeMapBizo(this, universalCode, description);
			Add(biz);
			return biz;
		}
	}
}
