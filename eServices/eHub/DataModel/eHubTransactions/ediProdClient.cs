using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public class ediProdClient
	{
		#region Fields

		[Key]
		public Nullable<System.Guid> CC_PK
		{
			get;
			set;
		}

		public string LE_EnterpriseCode
		{
			get;
			set;
		}

		public string LD_ServerCode
		{
			get;
			set;
		}

		public string EnterpriseServerCode
		{
			get;
			set;
		}

		public string LD_LicenceType
		{
			get;
			set;
		}

		public string OH_Code
		{
			get;
			set;
		}

		public string OH_FullName
		{
			get;
			set;
		}

		public Nullable<System.Guid> OH_PK
		{
			get;
			set;
		}

		public Nullable<int> HL_MajorVersion
		{
			get;
			set;
		}

		public Nullable<int> HL_MinorVersion
		{
			get;
			set;
		}

		public Nullable<int> HL_Release
		{
			get;
			set;
		}

		public Nullable<int> HL_Patch
		{
			get;
			set;
		}


		public string CC_ID
		{
			get;
			set;
		}
		#endregion

	}
}
