using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class AWBPrintSettings : OperationalActionMethodSettings
	{
		#region Schema

		public static class Schema
		{
			public const string OnErrorMessageError = "OnErrorMessageError";
			public const int OnErrorMessageErrorMaxLength = 3;
		}

		#endregion

		#region OnErrorMessageError

		[List("OnErrorMessageError_List")]
		[MaxLength(Schema.OnErrorMessageErrorMaxLength)]
		public ZString OnErrorMessageError
		{
			get { return onErrorMessageError; }
			set
			{
				CheckMaximumLength(OnErrorMessageErrorInfo, value);
				SetNonPersistentPropertyValue(OnErrorMessageErrorInfo, ref onErrorMessageError, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOnErrorMessageError();
				}
			}
		}
		ZString onErrorMessageError;

		public virtual ZPropertyInfo OnErrorMessageErrorInfo
		{
			get { return GetZPropertyInfo(Schema.OnErrorMessageError); }
		}

		#region OnErrorMessageError_List

		public static class Codes
		{
			public const string Abort = "ABT";
			public const string Skip = "SKP";
		}

		public static class Descriptions
		{
			public static MultilingualString Abort { get { return ResString.GetMultilingualString("AWBPrintSettings|Abort", "Abort if any of the consolidations have errors"); } }
			public static MultilingualString Skip { get { return ResString.GetMultilingualString("AWBPrintSettings|Skip", "Skip consolidations which have errors"); } }
		}

		public CodeDescriptionPairList OnErrorMessageError_List
		{
			get
			{
				if (onErrorMessageError_List == null)
				{
					onErrorMessageError_List = new CodeDescriptionPairList();
					onErrorMessageError_List.AddPair(Codes.Abort, Descriptions.Abort);
					onErrorMessageError_List.AddPair(Codes.Skip, Descriptions.Skip);
				}
				return onErrorMessageError_List;
			}
		}
		CodeDescriptionPairList onErrorMessageError_List;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public AWBPrintSettingsValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual AWBPrintSettingsValidation GetNewValidation()
		{
			return new AWBPrintSettingsValidation(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.OnErrorMessageError);
			writer.WriteValue(OnErrorMessageError);
			writer.WriteEndElement();
		}

		protected override void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();
			OnErrorMessageError = reader.ReadElementString(Schema.OnErrorMessageError);
			reader.ReadEndElement();
		}

		#endregion
	}
}
