using System.Data;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	[CodeProperty(DtbBookingTmplSchema.Constants.KT_Code)]
	[DescriptionProperty(DtbBookingTmplSchema.Constants.KT_Description)]
	public abstract class DtbTransportTmpl : AutoDtbBookingTmpl
	{
		protected DtbTransportTmpl(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region Instructions

		[ChildEditable]
		public DtbTransportInstructionTmplCollection Instructions
		{
			get
			{
				if (instructions == null)
				{
					instructions = GetNewTemplateInstructions();
					RegisterEditableChildObject(instructions);
				}

				return instructions;
			}
		}

		protected abstract DtbTransportInstructionTmplCollection GetNewTemplateInstructions();

		DtbTransportInstructionTmplCollection instructions;

		#endregion

		#endregion

		#region Lookups

		public new DtbTransportTmplLookups Lookups
		{
			get { return (DtbTransportTmplLookups)base.Lookups; }
		}

		protected sealed override DtbBookingTmplLookups GetNewLookups()
		{
			return GetNewLookupsCore();
		}

		protected abstract DtbTransportTmplLookups GetNewLookupsCore();

		#endregion

		#region Validation

		public new DtbTransportTmplValidation Validation
		{
			get { return (DtbTransportTmplValidation)base.Validation; }
		}

		protected sealed override DtbBookingTmplValidation GetNewValidation()
		{
			return GetNewValidationCore();
		}

		protected abstract DtbTransportTmplValidation GetNewValidationCore();

		#endregion

		#region Delete

		public override void Delete()
		{
			Instructions.DeleteAll(); // tested by SaveAndDeleteBusinessObject
			base.Delete();
		}

		#endregion
	}
}
