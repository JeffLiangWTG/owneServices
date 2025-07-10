using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class OneStopContainerEventRequest
	{
		#region Constructor

		protected OneStopContainerEventRequest(CommonContainer container)
		{
			this.Container = container;
		}

		public static OneStopContainerEventRequest New(CommonContainer container)
		{
			OneStopContainerEventRequest result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(container);
			}
			else
			{
				result = new OneStopContainerEventRequest(container);
			}
			return result;
		}

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		protected delegate OneStopContainerEventRequest NewDelegate(CommonContainer container);

		#endregion

		public OCsvLine ToCsvLine(string eventType, string requestCountry)
		{
			return NewCsvLine(GetFieldValues(eventType, requestCountry));
		}

		protected virtual string[] GetFieldValues(string eventType, string requestCountry)
		{
			return new string[]
			{
				OneStopConstants.EDIABN,
				OneStopConstants.UserID,
				OneStopConstants.PIN,
				Env.Registry.MailboxEmailAddress,
				"CONTAINER",
				eventType,
				"ANY",
				"N",
				"",
				"",
				Container.JC_ContainerNum,
				GetPKString(Container),
				requestCountry
			};
		}

		public static string GetPKString(BusinessObject bizO)
		{
			return bizO.PK.ToString().Replace("-", "");
		}

		public bool IsEventRequestRequired()
		{
			return IsEventRequestRequiredCore();
		}

		protected virtual bool IsEventRequestRequiredCore()
		{
			bool result = false;
			if (!Container.IsDeleted && !Container.JC_ContainerNum.IsEmpty && Container.IsSeaContainer)
			{
				result = result || !Container.IsInDatabase;
				result = result || (Container.IsInDatabase && Container.JC_ContainerNumInfo.HasChanges);
			}
			return result;
		}

		#region Implementation

		BusinessObjectFactory Factory
		{
			get { return Container.Factory; }
		}

		public ZString PortOfLoading
		{
			get
			{
				ZString result = ZString.Empty;
				if (Container.Consol != null)
				{
					result = Container.JC_JA_NKPortOfLoading;
				}
				else if (Declaration != null)
				{
					result = (ZString)Declaration[JobDeclarationSchema.JE_RL_NKOrigin];
				}
				return result;
			}
		}

		public ZString PortOfDischarge
		{
			get
			{
				ZString result = ZString.Empty;
				if (Container.Consol != null)
				{
					result = Container.JC_JB_NKPortOfDischarge;
				}
				else if (Declaration != null)
				{
					result = (ZString)Declaration[JobDeclarationSchema.JE_RL_NKFinalDestination];
				}
				return result;
			}
		}

		protected BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					ZQuery customsContainerQuery = new ZQuery(CusContainerSchema.CO_JC, Container.PK);
					BusinessObject customsContainer = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(customsContainerQuery);
					declaration = (customsContainer == null) ? null : (BusinessObject)customsContainer[nameof(Declaration)];
				}
				return declaration;
			}
		}
		BusinessObject declaration;

		OCsvLine NewCsvLine(params string[] fieldValues)
		{
			bool[] includeQuotes = new bool[fieldValues.Length];
			for (int i = 0; i < includeQuotes.Length; i++)
			{
				includeQuotes[i] = true;
			}
			OCsvLine result = new OCsvLine(fieldValues, includeQuotes);
			return result;
		}

		public readonly CommonContainer Container;

		#endregion
	}
}
