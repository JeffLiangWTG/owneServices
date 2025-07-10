using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	class DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter : DummyBusinessObject, IAddInfoWithConcurrencyResolverSupporter, INAddInfoSupporter
	{
		public DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public TestAddInfo AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					addInfo = (TestAddInfo)((IAddInfoWithConcurrencyResolverSupporter)this).NewAddInfoBizObj(Z0_VarCharMaxInfo);
					addInfo.EnableConcurrencyResolver = true;
				}
				return addInfo;
			}
		}
		TestAddInfo addInfo;

		#region IAddInfoWithConcurrencyResolverSupporter Members

		BaseAddInfo IAddInfoWithConcurrencyResolverSupporter.NewAddInfoBizObj(ZPropertyInfo addInfoProperty)
		{
			return new TestAddInfo(addInfoProperty);
		}

		ZPropertyInfo IAddInfoWithConcurrencyResolverSupporter.NotificationAddInfo
		{
			get { return Z0_CodeInfo; }
		}

		#endregion

		#region IAddInfoManager Members

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		public override ZString Z0_VarCharMax
		{
			get
			{
				return AddInfoParser.ConcatAddInfoStrings(base.Z0_VarCharMax, base.Z0_NVarCharMax);
			}
			set
			{
				var addInfoStrings = AddInfo.SplitAddInfoString(value);

				base.Z0_VarCharMax = addInfoStrings.Item1;
				base.Z0_NVarCharMax = addInfoStrings.Item2;
			}
		}

		ZPropertyInfoString INAddInfoSupporter.NAddInfoProperty => base.Z0_NVarCharMaxInfo as ZPropertyInfoString;
	}
}
