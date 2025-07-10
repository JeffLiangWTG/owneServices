using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Module
{
	public class PRASettings : OperationalActionMethodSettings
	{
		public PRASettings(BusinessObjectFactory factory)
			: base(factory) { }

		#region Schema

		public static class Schema
		{
			public const string ErrorBehaviour = "ErrorBehaviour";
			public const int ErrorBehaviourMaxLength = 3;
		}

		#endregion

		#region ErrorBehaviour

		[List("ErrorBehaviour_List")]
		[MaxLength(Schema.ErrorBehaviourMaxLength)]
		public ZString ErrorBehaviour
		{
			get { return errorBehaviour; }
			set
			{
				CheckMaximumLength(ErrorBehaviourInfo, value);
				if (SetNonPersistentPropertyValue(ErrorBehaviourInfo, ref errorBehaviour, value)
					&& !IsValidationSuspended)
				{
					Validation.ValidateErrorBehaviour();
				}
			}
		}
		ZString errorBehaviour;

		public virtual ZPropertyInfo ErrorBehaviourInfo
		{
			get { return GetZPropertyInfo(Schema.ErrorBehaviour); }
		}
		#endregion

		#region ErrorBehaviour_List

		public static class Codes
		{
			public const string Abort = "ABT";
			public const string Skip = "SKP";
		}

		public static class Descriptions
		{
			public static MultilingualString Abort { get { return ResString.GetMultilingualString("PRASettings|Abort", "Abort if any of the consolidations/containers have errors"); } }
			public static MultilingualString Skip { get { return ResString.GetMultilingualString("PRASettings|Skip", "Skip consolidations/containers which have errors"); } }
		}

		public CodeDescriptionPairList ErrorBehaviour_List
		{
			get
			{
				if (errorBehaviour_List == null)
				{
					errorBehaviour_List = new CodeDescriptionPairList();
					errorBehaviour_List.AddPair(Codes.Abort, Descriptions.Abort);
					errorBehaviour_List.AddPair(Codes.Skip, Descriptions.Skip);
				}
				return errorBehaviour_List;
			}
		}
		CodeDescriptionPairList errorBehaviour_List;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public PRASettingsValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual PRASettingsValidation GetNewValidation()
		{
			return new PRASettingsValidation(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.ErrorBehaviour);
			writer.WriteValue(errorBehaviour);
			writer.WriteEndElement();
		}

		protected override void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();
			errorBehaviour = reader.ReadElementString(Schema.ErrorBehaviour);
			reader.ReadEndElement();
		}

		#endregion
	}
}
