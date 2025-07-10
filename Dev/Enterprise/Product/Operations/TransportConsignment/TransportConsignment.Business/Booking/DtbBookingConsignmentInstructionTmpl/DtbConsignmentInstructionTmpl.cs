using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentInstructionTmpl : DtbTransportInstructionTmpl
	{
		public DtbConsignmentInstructionTmpl(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region Template

		protected override Type TemplateType
		{
			get { return typeof(DtbConsignmentTmpl); }
		}

		#endregion

		#endregion

		#region Lookups

		public new DtbConsignmentInstructionTmplLookups Lookups
		{
			get { return (DtbConsignmentInstructionTmplLookups)base.Lookups; }
		}

		protected override DtbTransportInstructionTmplLookups GetNewLookupsCore()
		{
			return new DtbConsignmentInstructionTmplLookups(this);
		}

		#endregion

		#region Validation

		public new DtbConsignmentInstructionTmplValidation Validation
		{
			get { return (DtbConsignmentInstructionTmplValidation)base.Validation; }
		}

		protected override DtbTransportInstructionTmplValidation GetNewValidationCore()
		{
			return new DtbConsignmentInstructionTmplValidation(this);
		}

		#endregion
	}
}
