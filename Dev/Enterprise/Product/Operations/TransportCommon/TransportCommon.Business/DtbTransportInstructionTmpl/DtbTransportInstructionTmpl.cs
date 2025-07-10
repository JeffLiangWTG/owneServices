using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionTmpl : AutoDtbBookingInstructionTmpl
	{
		protected DtbTransportInstructionTmpl(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region Template

		public DtbTransportTmpl Template
		{
			get { return (DtbTransportTmpl)Factory.Load(TemplateType, K2_KT_BookingTmpl); }
		}

		protected abstract Type TemplateType { get; }

		#endregion

		#endregion

		#region Lookups

		public new DtbTransportInstructionTmplLookups Lookups
		{
			get { return (DtbTransportInstructionTmplLookups)base.Lookups; }
		}

		protected sealed override DtbBookingInstructionTmplLookups GetNewLookups()
		{
			return GetNewLookupsCore();
		}

		protected abstract DtbTransportInstructionTmplLookups GetNewLookupsCore();

		#endregion

		#region Validation

		public new DtbTransportInstructionTmplValidation Validation
		{
			get { return (DtbTransportInstructionTmplValidation)base.Validation; }
		}

		protected sealed override DtbBookingInstructionTmplValidation GetNewValidation()
		{
			return GetNewValidationCore();
		}

		protected abstract DtbTransportInstructionTmplValidation GetNewValidationCore();

		#endregion
	}
}
