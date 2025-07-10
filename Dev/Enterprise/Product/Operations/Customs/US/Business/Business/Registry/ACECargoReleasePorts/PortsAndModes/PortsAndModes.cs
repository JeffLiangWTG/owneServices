using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public class PortsAndModes : NonPersistentBusinessObject
	{
		public PortsAndModes(PortsAndModesCollection parentCollection)
			: base()
		{
			this.ParentCollection = parentCollection;
		}
		internal readonly PortsAndModesCollection ParentCollection;

		protected PortsAndModes(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static class Schema
		{
			public const string CertificationMethod = "CertificationMethod";
			public const int CertificationMethodMaxLength = 3;

			public const string Port = "Port";
			public const int PortMaxLength = 4;

			public const string TransportMode = "TransportMode";
			public const int TransportModeMaxLength = 3;
		}

		[MaxLength(Schema.CertificationMethodMaxLength)]
		[List(nameof(Lookups) + "." + nameof(PortsAndModesLookups.CertificationMethodsList))]
		public ZString CertificationMethod
		{
			get { return certificationMethod; }
			set
			{
				CheckMaximumLength(CertificationMethodInfo, value);
				SetNonPersistentPropertyValue(CertificationMethodInfo, ref certificationMethod, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					Validation.ValidateCertificationMethod();
				}
			}
		}
		ZString certificationMethod;

		public ZPropertyInfo CertificationMethodInfo
		{
			get { return this.GetZPropertyInfo(Schema.CertificationMethod); }
		}

		[MaxLength(Schema.PortMaxLength)]
		[List(nameof(Lookups) + "." + nameof(PortsAndModesLookups.PortList))]
		public ZString Port
		{
			get { return port; }
			set
			{
				CheckMaximumLength(PortInfo, value);
				SetNonPersistentPropertyValue(PortInfo, ref port, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					Validation.ValidatePort();
				}
			}
		}
		ZString port;

		public ZPropertyInfo PortInfo
		{
			get { return this.GetZPropertyInfo(Schema.Port); }
		}

		[MaxLength(Schema.TransportModeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(PortsAndModesLookups.TransportModeList))]
		public ZString TransportMode
		{
			get { return transportMode; }
			set
			{
				CheckMaximumLength(TransportModeInfo, value);
				SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					Validation.ValidateTransportMode();
				}
			}
		}
		ZString transportMode;

		public ZPropertyInfo TransportModeInfo
		{
			get { return this.GetZPropertyInfo(Schema.TransportMode); }
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public PortsAndModesValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected PortsAndModesValidation GetNewValidation()
		{
			return new PortsAndModesValidation(this);
		}

		#endregion

		public PortsAndModesLookups Lookups
		{
			get { return lookups ?? (lookups = new PortsAndModesLookups(this)); }
		}
		PortsAndModesLookups lookups;
	}
}
