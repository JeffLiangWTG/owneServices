using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyBizOWithIncoTerm : DummyBusinessObject
	{
		public DummyBizOWithIncoTerm(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString Z0_Incoterm
		{
			get { return incoterm; }
			set
			{
				if (SetNonPersistentPropertyValue(Z0_IncotermInfo, ref incoterm, value) && !IsValidationSuspended)
				{
					Validation.ValidateZ0_Incoterm();
				}
			}
		}

		ZString incoterm;

		public ZPropertyInfo Z0_IncotermInfo
		{
			get { return GetZPropertyInfo(nameof(Z0_Incoterm)); }
		}

		public new DummyBizOWithIncoTermValidation Validation
		{
			get { return (DummyBizOWithIncoTermValidation)base.Validation; }
		}

		protected override DummyBizoValidation GetNewValidation()
		{
			return new DummyBizOWithIncoTermValidation(this);
		}
	}
}
