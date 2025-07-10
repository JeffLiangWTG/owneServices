
namespace Enterprise.Freight.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;

	public class TransportEventDataModel : BusinessObjectEventDataModel
	{
		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Just a property name")]
		public static class Properties
		{
			public const string Origin = "Origin";
			public const string Destination = "Destination";
		}

		#endregion

		public TransportEventDataModel(Transport transport)
			: base(transport)
		{
		}

		protected TransportEventDataModel(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected Transport Transport
		{
			get
			{
				return (Transport)base.Parent;
			}
		}

		public virtual string Origin
		{
			get
			{
				return Transport.JW_RL_NKLoadPort;
			}
		}

		public virtual string Destination
		{
			get
			{
				return Transport.JW_RL_NKDiscPort;
			}
		}
	}
}
