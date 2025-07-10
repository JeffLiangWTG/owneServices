using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInstructionTmpl : Common.AutoDtbBookingInstructionTmpl
	{
		public DtbBookingInstructionTmpl(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DtbBookingTmpl Template
		{
			get { return (DtbBookingTmpl)Factory.Load(TemplateType, K2_KT_BookingTmpl); }
		}

		Type TemplateType
		{
			get { return typeof(DtbBookingTmpl); }
		}

		// persistent

		[RelatedBusinessObject("Template")]
		public override ZGuid K2_KT_BookingTmpl
		{
			get { return base.K2_KT_BookingTmpl; }
			set { base.K2_KT_BookingTmpl = value; }
		}

		[ReadOnlyMember(nameof(IsSystem))]
		[List("Lookups.OrganisationTypes")]
		public override ZString K2_OrgType
		{
			get { return base.K2_OrgType; }
			set { base.K2_OrgType = value; }
		}

		[List("Lookups.DropModes")]
		public override ZString K2_DropMode
		{
			get { return base.K2_DropMode; }
			set { base.K2_DropMode = value; }
		}

		[ReadOnlyMember(nameof(IsSystem))]
		[List("Lookups.InstructionTypes")]
		public override ZString K2_InstructionType
		{
			get { return base.K2_InstructionType; }
			set { base.K2_InstructionType = value; }
		}

		[ReadOnlyMember(nameof(IsSystem))]
		public override ZInt K2_Sequence
		{
			get { return base.K2_Sequence; }
			set { base.K2_Sequence = value; }
		}

		[ReadOnlyMember(nameof(IsSystem))]
		[List("Lookups.DefaultPackageTypes")]
		public override ZString K2_PackageType
		{
			get { return base.K2_PackageType; }
			set { base.K2_PackageType = value; }
		}

		[ReadOnlyMember(nameof(IsSystem))]
		public override ZBool K2_IsContainerRateable
		{
			get { return base.K2_IsContainerRateable; }
			set
			{
				base.K2_IsContainerRateable = value;

				var template = Template;
				if (template != null)
				{
					template.ValidatedInstructionsK2_IsContainerRateable(this);
				}
			}
		}

		bool K2_IsContainerRateable_ReadOnly
		{
			get { return Template != null && !Template.IsContainerRateable; }
		}

		[ReadOnlyMember(nameof(IsSystem))]
		public override ZBool K2_IsLooseRateable
		{
			get { return base.K2_IsLooseRateable; }
			set
			{
				base.K2_IsLooseRateable = value;

				var template = Template;
				if (template != null)
				{
					template.ValidatedInstructionsK2_IsLooseRateable(this);
				}
			}
		}

		bool K2_IsLooseRateable_ReadOnly
		{
			get { return Template != null && !Template.IsLooseRateable; }
		}

		// calculated propeties

		public new DtbBookingInstructionTmplLookups Lookups
		{
			get { return (DtbBookingInstructionTmplLookups)base.Lookups; }
		}

		protected override Common.DtbBookingInstructionTmplLookups GetNewLookups()
		{
			return new DtbBookingInstructionTmplLookups(this);
		}

		public new DtbBookingInstructionTmplValidation Validation
		{
			get { return (DtbBookingInstructionTmplValidation)base.Validation; }
		}

		protected override Common.DtbBookingInstructionTmplValidation GetNewValidation()
		{
			return new DtbBookingInstructionTmplValidation(this);
		}

		public bool IsSystem
		{
			get { return Template != null && Template.KT_IsSystem; }
		}

		public override bool CanDelete
		{
			get { return !IsSystem; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("498884a6-84ec-480e-883b-d187b9882e06", "This Instruction cannot be deleted because the Transport Booking Template is System Defined."); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbBookingInstructionTmplFetchStrategy(this);
		}
	}
}
