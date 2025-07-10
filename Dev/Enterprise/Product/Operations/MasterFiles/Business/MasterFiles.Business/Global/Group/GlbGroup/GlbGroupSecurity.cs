using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class SecurityStringSplitter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SecurityStringSplitter()
			: base()
		{ }

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Class is inherited and cannot be static")]
		public class Schema
		{
			public const string SecurityRight = "SecurityRight";
			public const string SecurityRightPart1 = "SecurityRightPart1";
			public const string SecurityRightPart2 = "SecurityRightPart2";
			public const string SecurityRightPart3 = "SecurityRightPart3";
			public const string SecurityRightPart4 = "SecurityRightPart4";
			public const string Summary = "Summary";
		}

		#endregion

		public const int LastSecurityRightIndex = 4;

		public ZString SecurityRight { get; set; }

		public ZString SecurityRightPart1
		{
			get
			{
				return GetPart(1);
			}
		}

		public ZString SecurityRightPart2
		{
			get
			{
				return GetPart(2);
			}
		}

		public ZString SecurityRightPart3
		{
			get
			{
				return GetPart(3);
			}
		}

		public ZString SecurityRightPart4
		{
			get
			{
				return GetPart(4);
			}
		}

		ZString GetPart(int partnum)
		{
			if (SecurityRight.IsEmpty || partnum < 1 || partnum > LastSecurityRightIndex)
			{
				return ZString.Empty;
			}

			string[] parts = SecurityRight.ToString().Split(new string[] { "->" }, StringSplitOptions.None);
			if (parts.Length >= partnum)
			{
				if (partnum < LastSecurityRightIndex)
				{
					return parts[partnum - 1].Trim();
				}
				else
				{
					return string.Join("->", parts, LastSecurityRightIndex - 1, parts.Length - (LastSecurityRightIndex - 1)).Trim();
				}
			}
			return ZString.Empty;
		}

		public ZString Summary { get; set; }
	}

	public class GlbGroupSecurity : SecurityStringSplitter
	{
		public GlbGroupSecurity()
			: base()
		{ }
	}
}
