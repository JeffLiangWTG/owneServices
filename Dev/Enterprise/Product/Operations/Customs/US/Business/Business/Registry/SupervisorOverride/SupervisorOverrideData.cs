using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[DebuggerDisplay("NominatedErrors count = {NominatedMessageErrors.Count}")]
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]

	public class SupervisorOverrideData : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string NominatedErrorsCount = "NominatedErrorsCount";
			public const string TargetNominatedErrorFieldFormat = "TargetNominatedErrorField_{0}";
			public const string TargetNominatedErrorMessageFormat = "TargetNominatedErrorMessage_{0}";
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			SupervisorOverrideData supervisorOverrideData = GetNewSupervisorOverrideData();
			using (supervisorOverrideData.GetValidationSuspender())
			{
				supervisorOverrideData.NominatedMessageErrors.AddRange(this.NominatedMessageErrors);
			}

			return supervisorOverrideData;
		}

		protected virtual SupervisorOverrideData GetNewSupervisorOverrideData()
		{
			return new SupervisorOverrideData();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateNominatedMessageErrors();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.NominatedErrorsCount, NominatedMessageErrors.Count.ToString());
			for (int y = 0; y < NominatedMessageErrors.Count; y++)
			{
				writer.WriteElementString(string.Format(Schema.TargetNominatedErrorFieldFormat, y), NominatedMessageErrors[y].FieldName);
				writer.WriteElementString(string.Format(Schema.TargetNominatedErrorMessageFormat, y), NominatedMessageErrors[y].MessageErrorText);
			}
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			int targetNominatedErrorsCount = reader.ReadElementStringAsZInt(Schema.NominatedErrorsCount);
			for (int y = 0; y < targetNominatedErrorsCount; y++)
			{
				var nominatedMessageError = new NominatedMessageError();
				nominatedMessageError.FieldName = reader.ReadElementString(string.Format(Schema.TargetNominatedErrorFieldFormat, y));
				nominatedMessageError.MessageErrorText = reader.ReadElementString(string.Format(Schema.TargetNominatedErrorMessageFormat, y));
				NominatedMessageErrors.Add(nominatedMessageError);
			}
		}
		#endregion

		#region NominatedMessageErrorCollection

		public NominatedMessageErrorCollection NominatedMessageErrors
		{
			get
			{
				if (nominatedMessageErrors == null)
				{
					nominatedMessageErrors = new NominatedMessageErrorCollection(CurrentFactory);
					RegisterEditableChildObject(nominatedMessageErrors);
				}
				return nominatedMessageErrors;
			}
		}
		NominatedMessageErrorCollection nominatedMessageErrors;

		public void ValidateNominatedMessageErrors()
		{
			NominatedMessageErrors.RunPreSaveValidation();
		}

		#endregion

		#region For Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var field = this.NominatedMessageErrors.AddNew();
			field.FieldName = "Test";
		}
#endif
		#endregion
	}
}
