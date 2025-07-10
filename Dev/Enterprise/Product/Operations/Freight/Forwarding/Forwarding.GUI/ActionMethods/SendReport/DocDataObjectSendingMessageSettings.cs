using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class DocDataObjectSendingMessageSettings : OperationalActionMethodSettings
	{
		#region Schema

		public static class Schema
		{
			public const string ErrorAction = "ErrorAction";
			public const int ErrorActionMaxLength = 3;
		}

		#endregion

		#region ErrorAction

		[List("ErrorAction_List")]
		[MaxLength(Schema.ErrorActionMaxLength)]
		public ZString ErrorAction
		{
			get { return errorAction; }
			set
			{
				CheckMaximumLength(ErrorActionInfo, value);
				SetNonPersistentPropertyValue(ErrorActionInfo, ref errorAction, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateErrorAction();
				}
			}
		}
		ZString errorAction;

		public ZPropertyInfo ErrorActionInfo
		{
			get { return GetZPropertyInfo(Schema.ErrorAction); }
		}

		#region ErrorAction_List

		public static class Codes
		{
			public const string Abort = "ABT";
			public const string Skip = "SKP";
		}

		public static class Descriptions
		{
			public static MultilingualString Abort { get { return ResString.GetMultilingualString("DocDataObjectSendingMessageSettings|Abort", "Abort if any of the consolidations have errors"); } }
			public static MultilingualString Skip { get { return ResString.GetMultilingualString("DocDataObjectSendingMessageSettings|Skip", "Skip consolidations which have errors"); } }
		}

		public CodeDescriptionPairList ErrorAction_List
		{
			get
			{
				if (errorAction_List == null)
				{
					errorAction_List = new CodeDescriptionPairList();
					errorAction_List.AddPair(Codes.Abort, Descriptions.Abort);
					errorAction_List.AddPair(Codes.Skip, Descriptions.Skip);
				}
				return errorAction_List;
			}
		}
		CodeDescriptionPairList errorAction_List;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public DocDataObjectSendingMessageSettingsValidation Validation
		{
			get { return GetNewValidation(); }
		}

		DocDataObjectSendingMessageSettingsValidation GetNewValidation()
		{
			return new DocDataObjectSendingMessageSettingsValidation(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.ErrorAction);
			writer.WriteValue(ErrorAction);
			writer.WriteEndElement();
		}

		protected override void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();
			ErrorAction = reader.ReadElementString(Schema.ErrorAction);
			reader.ReadEndElement();
		}

		#endregion
	}
}
