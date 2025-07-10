using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public abstract class CodeInfo : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected CodeInfo(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string TablePrefix = "ZO_";
			public const string ZO_Code = "ZO_Code";
			public const string ZO_Data = "ZO_Data";
			public const string ZO_Description = "ZO_Description";
		}

		#endregion

		#region Serialisation/Deserialisation

		public override string ToString()
		{
			string result = ZO_Code;
			if (!ZO_Data.IsEmpty)
			{
				result += BaseAddInfo.CodeValueSeparator + ZO_Data;
			}
			return result;
		}

		public void LoadFromString(ZString addInfoString)
		{
			if (!addInfoString.IsEmpty)
			{
				ZString[] codeDataPair = addInfoString.Split(BaseAddInfo.CodeValueSeparator);

				fZO_Code = codeDataPair[0];
				if (codeDataPair.Length == 2)
				{
					fZO_Data = codeDataPair[1];
				}
			}
		}

		public CodeInfoCollection Collection;

		#endregion

		#region ZO_Code

		public virtual ZString ZO_Code
		{
			get { return fZO_Code; }
			set
			{
				value = RemoveReservedCharacters(value);
				SetNonPersistentPropertyValue(ZO_CodeInfo, ref fZO_Code, value);
				if (!IsValidationSuspended)
				{
					ValidateZO_Code();
				}
			}
		}
		ZString fZO_Code;

		protected ZString RemoveReservedCharacters(ZString value)
		{
			value = value.Replace(BaseAddInfo.Separator, ' ').Replace(BaseAddInfo.CodeInfoSeparator, ' ').Replace(BaseAddInfo.CodeValueSeparator, ' ');
			return value;
		}

		public virtual ZPropertyInfo ZO_CodeInfo
		{
			get { return GetZPropertyInfo(Schema.ZO_Code); }
		}

		protected abstract int ZO_Code_MaxLength { get; }

		public virtual void ValidateZO_Code()
		{
			ZO_CodeInfo.ClearAllNotifications();
			ListValidation.MessageErrorIfInvalidCode(ZO_CodeInfo, ZO_CodeList);
			ValidateDuplicateCode();
		}

		protected virtual void ValidateDuplicateCode()
		{
			if (!IsDuplicateCodeAllowed && !ZO_Code.IsEmpty && Collection != null)
			{
				foreach (CodeInfo codeInfo in Collection)
				{
					if (codeInfo != this && codeInfo.ZO_Code == ZO_Code)
					{
						ZO_CodeInfo.AddError(Res.GetString("08f4907f-1a38-48b7-9ab4-cce694e649d3", "{0} already exists.", ZO_Code));
					}
				}
			}
		}

		protected abstract internal bool IsDuplicateCodeAllowed { get; }

		public abstract CodeDescriptionPairList ZO_CodeList { get; }

		#endregion

		#region ZO_Description

		public ZString ZO_Description
		{
			get { return ZO_CodeList.GetDescriptionFromCode(ZO_Code); }
		}

		public ZPropertyInfo ZO_DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ZO_Description); }
		}

		#endregion

		#region ZO_Data

		public virtual ZString ZO_Data
		{
			get { return fZO_Data; }
			set
			{
				value = RemoveReservedCharacters(value);
				SetNonPersistentPropertyValue(ZO_DataInfo, ref fZO_Data, value);
				if (!IsValidationSuspended)
				{
					ValidateZO_Data();
				}
			}
		}
		ZString fZO_Data;

		public virtual ZPropertyInfo ZO_DataInfo
		{
			get { return GetZPropertyInfo(Schema.ZO_Data); }
		}

		protected abstract int ZO_Data_MaxLength { get; }

		public virtual void ValidateZO_Data()
		{
			ZO_DataInfo.ClearAllNotifications();
			if (ZO_Code.IsEmpty)
			{
				if (!ZO_Data.IsEmpty)
				{
					ZO_DataInfo.AddError(Res.GetString("e076960b-426f-466f-8ca8-941b12ba6344", "You have entered {0} without {1}.", HumanFriendlyDataColumnName, HumanFriendlyCodeColumnName));
				}
			}
			else
			{
				DataRequirements requirement = DataRequirement;
				if (requirement == DataRequirements.DataRequiredForCode && ZO_Data.IsEmpty)
				{
					ZO_DataInfo.AddMessageError(Res.GetString("8992ea05-0047-449b-b762-687b80de4ee1", "Please enter {0} corresponding to the {1}", HumanFriendlyDataColumnName, HumanFriendlyCodeColumnName));
				}
				else if (requirement == DataRequirements.DataNotRequiredForCode && !ZO_Data.IsEmpty)
				{
					ZO_DataInfo.AddMessageError(Res.GetString("78cadace-6cba-40db-84cb-16bdde752218", "{0} does not require {1} to be entered", ZO_Code, HumanFriendlyDataColumnName));
				}
			}
		}

		protected abstract internal string HumanFriendlyCodeColumnName { get; }
		protected abstract internal string HumanFriendlyDataColumnName { get; }

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateZO_Code();
			ValidateZO_Data();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			CodeInfo result = (CodeInfo)Activator.CreateInstance(GetType(), new object[] { Factory });
			using (result.GetValidationSuspender())
			{
				result.ZO_Code = ZO_Code;
				result.ZO_Data = ZO_Data;
			}
			return result;
		}

		#endregion

		#region Implementation

		protected internal enum DataRequirements { DataIsOptional, DataRequiredForCode, DataNotRequiredForCode }
		protected abstract internal DataRequirements DataRequirement { get; }

		#endregion
	}
}
