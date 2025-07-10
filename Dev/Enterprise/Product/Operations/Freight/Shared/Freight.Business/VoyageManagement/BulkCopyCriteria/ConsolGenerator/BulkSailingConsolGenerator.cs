using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Given a list of Sailings, generates a Consol for each and attaches the Sailing to the Consol.
	/// Currently generates AIR Consols only.
	/// </summary>
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public abstract class BulkSailingConsolGenerator : AutoBulkSailingConsolGenerator, IDefaultNumberOfDecimalsSupporter
	{
		protected BulkSailingConsolGenerator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		public override ZBool CreateConsol
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.CreateConsol; }
			set
			{
				base.CreateConsol = value;
				AllocateNeutralMasterInfo.RefreshBinding();
				WeightInfo.RefreshBinding();
				WeightUnitInfo.RefreshBinding();
				VolumeInfo.RefreshBinding();
				VolumeUnitInfo.RefreshBinding();
			}
		}

		protected bool DoNotCreateConsol
		{
			get { return !CreateConsol; }
		}

		public bool CreateConsol_ReadOnly { get; set; }

		[ReadOnlyMember(nameof(DoNotCreateConsol))]
		public override ZBool AllocateNeutralMaster
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.AllocateNeutralMaster; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.AllocateNeutralMaster = value; }
		}

		[ReadOnlyMember(nameof(DoNotCreateConsol))]
		[MeasureUnit(Schema.WeightUnit, MeasureUnitType.Weight)]
		public override ZDecimal Weight
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Weight; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Weight = this.GetRoundedValue(WeightInfo, value); }
		}

		[ReadOnlyMember(nameof(DoNotCreateConsol))]
		[List("Lookups.WeightUnitList")]
		public override ZString WeightUnit
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.WeightUnit; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.WeightUnit = value;
				this.SetRoundedValue(WeightInfo);
			}
		}

		[ReadOnlyMember(nameof(DoNotCreateConsol))]
		[MeasureUnit(Schema.VolumeUnit, MeasureUnitType.Volume)]
		public override ZDecimal Volume
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Volume; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Volume = this.GetRoundedValue(VolumeInfo, value); }
		}

		[ReadOnlyMember(nameof(DoNotCreateConsol))]
		[List("Lookups.VolumeUnitList")]
		public override ZString VolumeUnit
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.VolumeUnit; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.VolumeUnit = value;
				this.SetRoundedValue(VolumeInfo);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Accessors have attributes")]
		public override ZBool CopyShipments
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.CopyShipments; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.CopyShipments = value; }
		}

		public bool CopyShipments_ReadOnly
		{
			get { return CanNotCreateConsol; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Accessors have attributes")]
		public override ZBool CopyRoutings
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.CopyRoutings; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.CopyRoutings = value; }
		}

		public bool CopyRoutings_ReadOnly
		{
			get { return CanNotCreateConsol; }
		}

		bool CanNotCreateConsol
		{
			get { return DoNotCreateConsol || TemplateConsol == null; }
		}

		#endregion

		#region Lookups

		public BulkSailingConsolGeneratorLookups Lookups
		{
			get { return lookups ?? (lookups = new BulkSailingConsolGeneratorLookups(this)); }
		}
		BulkSailingConsolGeneratorLookups lookups;

		#endregion

		public ZGuid TemplateConsolPK
		{
			get { return templateConsolPK; }
			set
			{
				if (templateConsolPK != value)
				{
					templateConsolPK = value;
					templateConsol = null;
					CopyShipments = !value.IsEmpty;
				}
			}
		}
		ZGuid templateConsolPK;

		protected CommonConsol TemplateConsol
		{
			get { return templateConsol ?? (templateConsol = GetTemplateConsol()); }
		}
		CommonConsol templateConsol;

		public void GenerateConsols(BaseJobSailingCollection sailings)
		{
			foreach (BaseJobSailing sailing in sailings)
			{
				CommonConsol consol = GetNewConsol();

				using (consol.GetValidationSuspender())
				{
					if (TemplateConsolPK.IsEmpty)
					{
						consol.JK_TransportMode = Constants.TransportModes.Air;
						consol.JK_RL_NKLoadPort = sailing.JX_JA_RL_NKPortOfLoading;
						consol.JK_RL_NKDischargePort = sailing.JX_JB_RL_NKPortOfDischarge;
					}

					using (consol.Transports.MostInterestingTransport.GetValidationSuspender())
					{
						consol.Transports.MostInterestingTransport.JW_JX = sailing.PK;
					}

					if (!TemplateConsolPK.IsEmpty && CopyRoutings)
					{
						FillTransports(consol);
					}

					if (Weight > 0m)
					{
						if (consol.JK_TransportMode == Constants.TransportModes.Air)
						{
							consol.JK_TotalShipmentChargeableUnit = WeightUnit;
						}
						else
						{
							consol.JK_TotalShipmentActOtherUnit = WeightUnit;
						}
						consol.JK_TotalShipmentActWeightCheck = Weight;
					}

					if (Volume > 0m)
					{
						if (consol.JK_TransportMode == Constants.TransportModes.Air)
						{
							consol.JK_TotalShipmentActOtherUnit = VolumeUnit;
						}
						else
						{
							consol.JK_TotalShipmentChargeableUnit = VolumeUnit;
						}
						consol.JK_TotalShipmentActVolumeCheck = Volume;
					}

					consol.JK_IsNeutralMaster = AllocateNeutralMaster;
				}

				if (!IsValidationSuspended && TemplateConsolPK.IsEmpty)
				{
					consol.Validation.ValidateJK_MasterBillNum();
				}
			}
		}

		public MainFormConsolCollection CreatedConsols
		{
			get
			{
				if (createdConsols == null)
				{
					createdConsols = new MainFormConsolCollection(Factory);
					createdConsols.SetReadOnlyIncludingChildren(true);
				}
				return createdConsols;
			}
		}
		MainFormConsolCollection createdConsols;

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return Constants.TransportModes.Air; }
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.Weight:
					unitOfMeasure = WeightUnit;
					break;

				case Schema.Volume:
					unitOfMeasure = VolumeUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			// not required, transport mode is always AIR
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CopyRoutings = true;
			WeightUnit = Env.Registry.FreightWeightUnit;
			VolumeUnit = Env.Registry.FreightVolumeUnit;
		}

		void FillTransports(CommonConsol consol)
		{
			consol.Transports.SuspendValidation();
			try
			{
				foreach (Transport origTransport in TemplateConsol.Transports)
				{
					if (origTransport != TemplateConsol.Transports.MostInterestingTransport)
					{
						Transport transport = origTransport.TemplateCopy();
						using (transport.GetValidationSuspender())
						{
							consol.Transports.Add(transport);
							FillTransport(consol, transport, origTransport);
						}
					}
				}
			}
			finally
			{
				consol.Transports.ResumeValidation();
			}
		}

		void FillTransport(CommonConsol consol, Transport transport, Transport origTransport)
		{
			TimeSpan diff = CalculateTransportsTimeSpan(consol.Transports.MostInterestingTransport,
				TemplateConsol.Transports.MostInterestingTransport);
			if (diff != TimeSpan.Zero)
			{
				if (origTransport.Sailing != null)
				{
					BaseJobSailing sailing = CopySailing(origTransport, diff);

					if (sailing != null)
					{
						transport.JW_JX = sailing.PK;
					}
				}
				else
				{
					if (!origTransport.JW_ETD.IsEmpty)
					{
						transport.JW_ETD = origTransport.JW_ETD.Add(diff);
					}

					if (!origTransport.JW_ETA.IsEmpty)
					{
						transport.JW_ETA = origTransport.JW_ETA.Add(diff);
					}
				}
			}
		}

		BaseJobSailing CopySailing(Transport transport, TimeSpan diff)
		{
			BuildSingleJobSailingHelper helper = new BuildSingleJobSailingHelper(transport.Sailing);
			helper.FilteredOrigin = transport.JW_RL_NKLoadPort;
			helper.FilteredDestination = transport.JW_RL_NKDiscPort;
			return helper.CopySchedule(diff);
		}

		TimeSpan CalculateTransportsTimeSpan(Transport copiedTransport, Transport originalTransport)
		{
			TimeSpan diff = TimeSpan.Zero;

			if (!copiedTransport.JW_ETD.IsEmpty && !originalTransport.JW_ETD.IsEmpty)
			{
				diff = copiedTransport.JW_ETD - originalTransport.JW_ETD;
			}
			else if (!copiedTransport.JW_ETA.IsEmpty && !originalTransport.JW_ETA.IsEmpty)
			{
				diff = copiedTransport.JW_ETA - originalTransport.JW_ETA;
			}

			return diff;
		}

		protected CommonConsol GetNewConsol()
		{
			CommonConsol result = CreateConsolFromTemplate(TemplateConsol) ?? CreateNewConsol();

			CreatedConsols.Add(result);

			return result;
		}

		protected abstract CommonConsol GetTemplateConsol();
		protected abstract CommonConsol CreateConsolFromTemplate(CommonConsol templateConsol);
		protected abstract CommonConsol CreateNewConsol();

		#endregion
	}
}
