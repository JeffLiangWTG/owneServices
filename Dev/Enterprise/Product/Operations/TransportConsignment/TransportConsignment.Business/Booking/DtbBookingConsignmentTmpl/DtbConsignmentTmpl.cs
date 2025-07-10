using System.Data;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentTmpl : DtbTransportTmpl
	{
		public DtbConsignmentTmpl(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region Instructions

		protected override DtbTransportInstructionTmplCollection GetNewTemplateInstructions()
		{
			return new DtbConsignmentInstructionTmplCollection(this);
		}

		#endregion

		#endregion

		#region Lookups

		public new DtbConsignmentTmplLookups Lookups
		{
			get { return (DtbConsignmentTmplLookups)base.Lookups; }
		}

		protected override DtbTransportTmplLookups GetNewLookupsCore()
		{
			return new DtbConsignmentTmplLookups(this);
		}

		#endregion

		#region Validation

		public new DtbConsignmentTmplValidation Validation
		{
			get { return (DtbConsignmentTmplValidation)base.Validation; }
		}

		protected override DtbTransportTmplValidation GetNewValidationCore()
		{
			return new DtbConsignmentTmplValidation(this);
		}

		#endregion
	}
}
