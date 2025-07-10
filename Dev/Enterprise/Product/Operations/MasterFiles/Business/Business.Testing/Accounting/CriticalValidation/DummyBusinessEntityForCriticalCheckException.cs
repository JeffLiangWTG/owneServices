using System;
using System.Data;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyBusinessEntityForCriticalCheckException : DummyBusinessObject, ISupportCriticalValidation, IHaveConstructorStackTrace
	{
		public DummyBusinessEntityForCriticalCheckException(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return null; }
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			throw new NotImplementedException();
		}

		StackTrace IHaveConstructorStackTrace.ConstructorStackTrace { get; set; }
	}
}
