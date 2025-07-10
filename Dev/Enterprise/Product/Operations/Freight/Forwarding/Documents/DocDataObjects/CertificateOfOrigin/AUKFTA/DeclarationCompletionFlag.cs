using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	public class DeclarationCompletionFlag : DocDataObject
	{
		#region IsExporter

		public ZBool IsExporter
		{
			get => isExporter;
			set
			{
				if (SetNonPersistentPropertyValue(IsExporterInfo, ref isExporter, value) && isExporter)
				{
					IsProducer = false;
					IsAuthorizedOnBehalfExporter = false;
					IsAuthorizedOnBehalfProducer = false;
				}
				Value = isExporter ? nameof(IsExporter) : ZString.Empty;
				ValidateAll();
			}
		}

		ZBool isExporter;

		public ZPropertyInfo IsExporterInfo => GetZPropertyInfo(nameof(IsExporter));

		#endregion

		#region IsProducer

		public ZBool IsProducer
		{
			get => isProducer;
			set
			{
				if (SetNonPersistentPropertyValue(IsProducerInfo, ref isProducer, value) && isProducer)
				{
					IsExporter = false;
					IsAuthorizedOnBehalfExporter = false;
					IsAuthorizedOnBehalfProducer = false;
				}
				Value = isProducer ? nameof(IsProducer) : ZString.Empty;
				ValidateAll();
			}
		}

		ZBool isProducer;

		public ZPropertyInfo IsProducerInfo => GetZPropertyInfo(nameof(IsProducer));

		#endregion

		#region IsAuthorizedOnBehalfExporter

		public ZBool IsAuthorizedOnBehalfExporter
		{
			get => isAuthorizedOnBehalfExporter;
			set
			{
				if (SetNonPersistentPropertyValue(IsAuthorizedOnBehalfExporterInfo, ref isAuthorizedOnBehalfExporter, value) && isAuthorizedOnBehalfExporter)
				{
					IsExporter = false;
					IsProducer = false;
					IsAuthorizedOnBehalfProducer = false;
				}
				Value = isAuthorizedOnBehalfExporter ? nameof(IsAuthorizedOnBehalfExporter) : ZString.Empty;
				ValidateAll();
			}
		}

		ZBool isAuthorizedOnBehalfExporter;

		public ZPropertyInfo IsAuthorizedOnBehalfExporterInfo => GetZPropertyInfo(nameof(IsAuthorizedOnBehalfExporter));

		#endregion

		#region IsAuthorizedOnBehalfProducer

		public ZBool IsAuthorizedOnBehalfProducer
		{
			get => isAuthorizedOnBehalfProducer;
			set
			{
				if (SetNonPersistentPropertyValue(IsAuthorizedOnBehalfProducerInfo, ref isAuthorizedOnBehalfProducer, value) && isAuthorizedOnBehalfProducer)
				{
					IsExporter = false;
					IsProducer = false;
					IsAuthorizedOnBehalfExporter = false;
				}
				Value = isAuthorizedOnBehalfProducer ? nameof(IsAuthorizedOnBehalfProducer) : ZString.Empty;
				ValidateAll();
			}
		}

		ZBool isAuthorizedOnBehalfProducer;

		public ZPropertyInfo IsAuthorizedOnBehalfProducerInfo => GetZPropertyInfo(nameof(IsAuthorizedOnBehalfProducer));

		#endregion

		#region Value

		public ZString Value
		{
			get => _value;
			private set
			{
				if (SetNonPersistentPropertyValue(ValueInfo, ref _value, value))
				{
					Validate(ValueInfo);
				}
			}
		}

		ZString _value;

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));

		#endregion
	}
}
