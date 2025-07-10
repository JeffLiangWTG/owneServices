using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusStatus : NonPersistentBusinessObject, ICodeDescription, IObsoleteValidation
	{
		public CusStatus(ZPropertyInfo wrappedStatusInfo) : this(wrappedStatusInfo, new CodeDescriptionPairList(), ZString.Empty)
		{
		}

		public CusStatus(ZPropertyInfo wrappedStatusInfo, CodeDescriptionPairList statusList) : this(wrappedStatusInfo, statusList, ZString.Empty)
		{
		}

		public CusStatus(ZPropertyInfo wrappedStatusInfo, CodeDescriptionPairList statusList, ZString defaultValue)
		{
			this.WrappedStatusInfo = wrappedStatusInfo;
			this.fStatusList = statusList;
			this.DefaultValue = defaultValue;
		}

		#region Schema

		public abstract class Schema
		{
			public const string Description = "Description";
			public const string Code = "Code";
			public const string UserFriendlyStatuses = "UserFriendlyStatuses";
		}

		#endregion

		#region Properties

		#region Description

		string ICodeDescription.Description
		{
			get { return Description; }
		}

		public ZString Description
		{
			get { return StatusList.GetDescriptionFromCode(Code); }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region Code

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Code
		{
			get { return Code; }
		}

		[BusinessObjectTestExclude]
		[ReadOnly(true)]
		public virtual ZString Code
		{
			set
			{
				WrappedStatusInfo.Value = value;
			}
			get
			{
				if (WrappedStatusInfo.Value.IsEmpty && !DefaultValue.IsEmpty)
				{
					return DefaultValue;
				}
				return (ZString)WrappedStatusInfo.Value;
			}
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Code, x => WrappedStatusInfo); }
		}

		#endregion

		#region StatusList

		public CodeDescriptionPairList StatusList
		{
			get { return fStatusList; }
		}

		readonly CodeDescriptionPairList fStatusList;

		#endregion

		#region UserFriendlyStatuses

		public virtual ZString UserFriendlyStatuses
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo UserFriendlyStatusesInfo
		{
			get { return GetZPropertyInfo(Schema.UserFriendlyStatuses); }
		}

		#endregion

		#endregion

		#region Implementation

		protected readonly ZPropertyInfo WrappedStatusInfo;
		protected readonly ZString DefaultValue;

		#endregion
	}
}
