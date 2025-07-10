using System.Web.Services.Protocols;

namespace Enterprise.Warehouse.Web.WebService
{
	/// <summary>
	/// Header for SOAP Messages that includes authentication information
	/// </summary>
	public class SecuritySOAPHeader : SoapHeader
	{
		#region Constructors

		public SecuritySOAPHeader()
		{
			UserName = "";
			Password = "";
			BranchCode = "";
			DepartmentCode = "";
			WarehouseCode = "";
			SecurityKey = "";
			DeviceID = "";
			ProcessID = 0;
			DeviceVersion = "";
			IsAndroidDevice = false;
		}

		public SecuritySOAPHeader(SecuritySOAPHeader securitySOAPHeader)
		{
			Actor = securitySOAPHeader.Actor;
			DidUnderstand = securitySOAPHeader.DidUnderstand;
			EncodedMustUnderstand = securitySOAPHeader.EncodedMustUnderstand;
			EncodedMustUnderstand12 = securitySOAPHeader.EncodedMustUnderstand12;
			EncodedRelay = securitySOAPHeader.EncodedRelay;
			MustUnderstand = securitySOAPHeader.MustUnderstand;
			Relay = securitySOAPHeader.Relay;
			Role = securitySOAPHeader.Role;

			UserName = securitySOAPHeader.UserName;
			Password = securitySOAPHeader.Password;
			BranchCode = securitySOAPHeader.BranchCode;
			DepartmentCode = securitySOAPHeader.DepartmentCode;
			WarehouseCode = securitySOAPHeader.WarehouseCode;
			SecurityKey = securitySOAPHeader.SecurityKey;
			DeviceID = securitySOAPHeader.DeviceID;
			DeviceModelDetails = securitySOAPHeader.DeviceModelDetails;
			ProcessID = securitySOAPHeader.ProcessID;
			DeviceVersion = securitySOAPHeader.DeviceVersion;
			IsAndroidDevice = securitySOAPHeader.IsAndroidDevice;
		}

		#endregion

		#region Properties

		public string UserName
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}

		public string BranchCode
		{
			get;
			set;
		}

		public string DepartmentCode
		{
			get;
			set;
		}

		public string WarehouseCode
		{
			get;
			set;
		}

		public string SecurityKey
		{
			get;
			set;
		}

		public int ProcessID
		{
			get;
			set;
		}

		public string DeviceID
		{
			get;
			set;
		}

		public string DeviceModelDetails
		{
			get;
			set;
		}

		public string DeviceVersion
		{
			get;
			set;
		}

		public bool IsAndroidDevice
		{
			get;
			set;
		}

		#endregion
	}
}
