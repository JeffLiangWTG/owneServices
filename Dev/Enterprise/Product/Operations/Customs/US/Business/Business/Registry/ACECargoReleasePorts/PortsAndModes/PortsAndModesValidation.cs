using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public class PortsAndModesValidation : ZValidation
	{
		public PortsAndModesValidation(PortsAndModes parent)
			: base(parent)
		{
			this.parent = parent;
			this.zValidationInternals = this;
			this.parentListInternals = parent;
		}
		readonly PortsAndModes parent;
		readonly IValidationInternals zValidationInternals;
		readonly ISingleElementListInternal parentListInternals;

		#region Implementation

		public void Add(PortsAndModesValidation validation)
		{
			zValidationInternals.Add(validation);
		}

		public void Remove(PortsAndModesValidation validation)
		{
			zValidationInternals.Remove(validation);
		}

		public override void ValidateAll()
		{
			IDisposable suspender = parentListInternals.SuspendListChanged();
			try
			{
				ValidateAllCore();
			}
			finally
			{
				suspender.Dispose();
			}
		}

		protected void ValidateAllCore()
		{
			ValidateCertificationMethod();
			ValidatePort();
			ValidateTransportMode();
		}

		public override Type AutoValidationType
		{
			get { return typeof(PortsAndModesValidation); }
		}

		#endregion

		#region Certification Method

		public void ValidateCertificationMethod()
		{
			zValidationInternals.Validate(Parent.CertificationMethodInfo, new RunValidationInvoker(this.CertificationMethodValidationInvoker));
		}

		void CertificationMethodValidationInvoker()
		{
			CheckCertificationMethodIsWesternEuropean();
			CheckCertificationMethod();
		}

		protected void CheckCertificationMethodIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.CertificationMethodInfo);
		}

		protected void CheckCertificationMethod()
		{
			MandatoryValidation.CheckEntered(Parent.CertificationMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CertificationMethodInfo, Parent.Lookups.CertificationMethodsList);
		}

		#endregion

		#region Port

		public void ValidatePort()
		{
			zValidationInternals.Validate(Parent.PortInfo, new RunValidationInvoker(this.PortValidationInvoker));
		}

		void PortValidationInvoker()
		{
			CheckPortIsWesternEuropean();
			CheckPort();
		}

		protected void CheckPortIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.PortInfo);
		}

		protected void CheckPort()
		{
			MandatoryValidation.CheckEntered(Parent.PortInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PortInfo, Parent.Lookups.PortList);
		}

		#endregion

		#region TransportMode

		public void ValidateTransportMode()
		{
			zValidationInternals.Validate(Parent.TransportModeInfo, new RunValidationInvoker(this.TransportModeValidationInvoker));
		}

		void TransportModeValidationInvoker()
		{
			CheckTransportModeIsWesternEuropean();
			CheckTransportMode();
		}

		protected void CheckTransportModeIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.TransportModeInfo);
		}

		protected void CheckTransportMode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TransportModeInfo, Parent.Lookups.TransportModeList);
			ValidateDuplicates();
		}

		#endregion

		void ValidateDuplicates()
		{
			Parent.ClearRowNotifications();

			if (parent.ParentCollection.OfType<PortsAndModes>().Count(x => x.CertificationMethod == Parent.CertificationMethod &&
							x.Port == Parent.Port &&
							(x.TransportMode == Parent.TransportMode || x.TransportMode.IsEmpty)) > 1)
			{
				Parent.AddRowError(DuplicateFound);
			}
		}
		internal const string DuplicateFound = "Duplicate entry found. This combination of Certification Method, Port and Transport Mode already exists.";

		public PortsAndModes Parent
		{
			get { return parent; }
		}
	}
}
