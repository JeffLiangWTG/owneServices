using System;
using System.Reflection;
using CargoWise.Common.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleChangeEmailTextFormatTemplateSwitcher : IDisposable
	{
		public ScheduleChangeEmailTextFormatTemplateSwitcher()
		{
			EmailTemplateHtmlField.SetValue(this, EmailTemplateAsText);
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		#region IDisposable

		public void Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
			EmailTemplateHtmlField.SetValue(this, null);
		}

		#endregion

		#region Implementation

		FieldInfo EmailTemplateHtmlField
		{
			get { return typeof(ScheduleChangeEmailSender).GetField("emailTemplateHtml", BindingFlags.NonPublic | BindingFlags.Static); }
		}

		string EmailTemplateAsText
		{
			get
			{
				return
@"(*TransportModeDescription*) Schedule changes (*ChangesFrom*) to (*ChangesTo*)
(*AllDelayAlertsDeliveryStatus*)
Load     Discharge  Field              Old Value       Updated Value
====================================================================
<!--StartSection ChangeDetails-->
<!--StartSection UserVesselVoyageCarrier-->
Changed By: (*ChangedBy*)
<!--StartSection VesselName-->
(*VesselNameLabel*): (*VesselName*)
<!--EndSection VesselName-->
<!--StartSection Voyage-->
(*VoyageLabel*): (*Voyage*)
<!--EndSection Voyage-->
<!--StartSection Carrier-->
(*CarrierLabel*): (*Carrier*)
<!--EndSection Carrier-->
<!--EndSection UserVesselVoyageCarrier-->
---
<!--StartSection DataProviderHeading-->
(*DataProviderHeading*)
<!--EndSection DataProviderHeading-->
<!--StartSection DateChangeDetails-->
(*LoadPort*)    (*DischargePort*)      (*FieldName*) (*OldDate*) (*UpdatedDate*)
<!--StartSection DataProviderDetails-->
(*DataProvider*)
<!--EndSection DataProviderDetails-->
<!--EndSection DateChangeDetails-->
<!--StartSection JobsAffected-->

The following (*AffectedJobType*) jobs are affected:
<!--StartSection JobAffected-->
(*JobNumberIndent*)(*JobNumber*) ((*JobUrl*)) (*DelayAlertDeliveryStatus*)
<!--EndSection JobAffected-->
<!--EndSection JobsAffected-->

<!--EndSection ChangeDetails-->
Schedule change notifications";
			}
		}

		#endregion
	}
}
