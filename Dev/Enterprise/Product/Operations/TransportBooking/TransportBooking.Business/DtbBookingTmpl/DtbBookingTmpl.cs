using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	[CodeProperty(DtbBookingTmplSchema.Constants.KT_Code)]
	[DescriptionProperty(DtbBookingTmplSchema.Constants.KT_Description)]
	public sealed class DtbBookingTmpl : Common.AutoDtbBookingTmpl
	{
		public DtbBookingTmpl(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		public DtbBookingInstructionTmplCollection Instructions
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

		DtbBookingInstructionTmplCollection instructions;

		DtbBookingInstructionTmplCollection GetNewTemplateInstructions()
		{
			return new DtbBookingInstructionTmplCollection(this);
		}

		// persistent

		bool KT_IsSystem_ReadOnly
		{
			get { return true; }
		}

		[ReadOnlyMember(nameof(KT_IsSystem))]
		public override ZString KT_Code
		{
			get { return base.KT_Code; }
			set { base.KT_Code = value; }
		}

		[ReadOnlyMember(nameof(KT_IsSystem))]
		[TranslatableDataField(Schema.TableName, Schema.KT_Description, @"Database\Odyssey\Data\Public\TransportBookingTemplate\TransportBookingTemplate.xml", MaxLength = Schema.KT_DescriptionMaxLength, Type = typeof(DtbBookingTmpl), Asmid = ResString.AssemblyId)]
		public override ZString KT_Description
		{
			get { return base.KT_Description; }
			set { base.KT_Description = value; }
		}

		public MultilingualString KT_DescriptionMultilingual
		{
			get { return GetMultilingual(KT_DescriptionInfo); }
		}

		[ReadOnlyMember(nameof(KT_IsSystem))]
		[List("Lookups.Directions")]
		public override ZString KT_Direction
		{
			get { return base.KT_Direction; }
			set { base.KT_Direction = value; }
		}

		[ReadOnlyMember(nameof(KT_IsSystem))]
		[List("Lookups.RatingFreightModes")]
		public override ZString KT_RatingFreightMode
		{
			get { return base.KT_RatingFreightMode; }
			set
			{
				base.KT_RatingFreightMode = value;
				ResetInstructionRateablesIfRequired();
			}
		}

		void ResetInstructionRateablesIfRequired()
		{
			foreach (DtbBookingInstructionTmpl instruction in Instructions)
			{
				try
				{
					using (instruction.GetValidationSuspender())
					{
						if (!IsContainerRateable && instruction.K2_IsContainerRateable)
						{
							instruction.K2_IsContainerRateable = false;
						}

						if (!IsLooseRateable && instruction.K2_IsLooseRateable)
						{
							instruction.K2_IsLooseRateable = false;
						}
					}
				}
				finally
				{
					ValidatedInstructionsK2_IsContainerRateable();
					ValidatedInstructionsK2_IsLooseRateable();
				}
			}
		}

		internal void ValidatedInstructionsK2_IsContainerRateable(DtbBookingInstructionTmpl except = null)
		{
			Array.ForEach(Instructions.Where(i => i != except).ToArray(), i => i.Validation.ValidateK2_IsContainerRateable());
		}

		internal void ValidatedInstructionsK2_IsLooseRateable(DtbBookingInstructionTmpl except = null)
		{
			Array.ForEach(Instructions.Where(i => i != except).ToArray(), i => i.Validation.ValidateK2_IsLooseRateable());
		}

		// calculated propeties

		public ZBool IsContainerRateable
		{
			get { return KT_RatingFreightMode == RatingFreightModes.Codes.Containerised || KT_RatingFreightMode == RatingFreightModes.Codes.Both; }
		}

		public ZBool IsLooseRateable
		{
			get { return KT_RatingFreightMode == RatingFreightModes.Codes.Loose || KT_RatingFreightMode == RatingFreightModes.Codes.Both; }
		}

		public new DtbBookingTmplLookups Lookups
		{
			get { return (DtbBookingTmplLookups)base.Lookups; }
		}

		protected override Common.DtbBookingTmplLookups GetNewLookups()
		{
			return new DtbBookingTmplLookups(this);
		}

		public new DtbBookingTmplValidation Validation
		{
			get { return (DtbBookingTmplValidation)base.Validation; }
		}

		protected override Common.DtbBookingTmplValidation GetNewValidation()
		{
			return GetNewValidationCore();
		}

		DtbBookingTmplValidation GetNewValidationCore()
		{
			return new DtbBookingTmplValidation(this);
		}

		public bool IsUsedInRegistry
		{
			get
			{
				var registryTmplDefaults = TransportRegistry.Instance.JobTemplateDefault.Value.Cast<JobTemplateDefault>();
				return registryTmplDefaults.Any(t => t.BookingTemplate == KT_Code);
			}
		}

		public override string CanCancel()
		{
			if (IsUsedInRegistry)
			{
				return Res.GetString("F203CC1F-1D75-4BE3-ADBF-A1403FF764AE", "This Template is set as a default template in Registry setting 'Job Template Defaults' and therefore cannot be made Inactive.");
			}
			else
			{
				return base.CanCancel();
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbBookingTmplFetchStrategy(this);
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return KT_Code.IsEmpty ?
					Res.GetString("61323a2b-afca-4032-809d-0b88faa6969c", "Transport Booking Template") :
					Res.GetString("22ea928c-26d4-4d78-a34e-e2578b500593", "Transport Booking Template {0}", KT_Code);
			}
		}

		public override void Delete()
		{
			Instructions.DeleteAll(); // tested by SaveAndDeleteBusinessObject
			base.Delete();
		}
	}
}
