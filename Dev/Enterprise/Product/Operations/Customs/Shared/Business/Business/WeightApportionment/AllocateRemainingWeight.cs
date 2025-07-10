using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public enum AllocateRemainingWeightWay
	{
		ByPrice,
		ByQuantity
	}

	public class AllocateRemainingWeightWrapper : IWeightHolder
	{
		public AllocateRemainingWeightWrapper(ICommonInvoiceDataProvider jobDeclaration)
		{
			declaration = jobDeclaration;
		}
		readonly ICommonInvoiceDataProvider declaration;

		AllocateRemainingWeightWay ByWay { get; set; }

		ZWeight IWeightHolder.TotalWeight => ZWeight.Empty;

		ZWeight IWeightHolder.TotalNetWeight => ZWeight.Empty;

		IWeightApportionee[] IWeightHolder.AllApportionees => Array.Empty<IWeightApportionee>();

		IWeightHolder[] IWeightHolder.WeightHolders
		{
			get
			{
				var result = new List<IWeightHolder>();
				declaration.Invoices.ForEach(x =>
				{
					result.Add(new AllocateRemainingWeightInvoiceHolder(x, this, false));
					result.Add(new AllocateRemainingWeightInvoiceHolder(x, this, true));
				});
				return result.ToArray();
			}
		}

		public void ApportionRemaining(IMessageNotificationCollector notification, AllocateRemainingWeightWay way)
		{
			ByWay = way;
			if (!declaration.Invoices.Any())
			{
				notification?.Notifications?.AddWarning(Res.GetString("E98A14E5-F919-4C67-833F-A73221D34769", "Unable to allocate remaining weight. Job must have at least one invoice header."));
			}
			else
			{
				Allocate(notification);
			}

			if (notification != null && (notification.Notifications?.ContainsWarning() ?? false))
			{
				notification.ShowWarning(notification.Notifications.WarningNotificationsAsString(), Res.GetString("787A57BF-5AF4-4984-AE52-8EDE7B81376E", "Allocate Remaining Weight Warning"));
				notification.Notifications.Clear();
			}
		}

		void Allocate(IMessageNotificationCollector notification)
		{
			if (PreAllocate(notification))
			{
				declaration.WeightApportionManager.ApportionAll(this, null);
			}
		}

		bool PreAllocate(IMessageNotificationCollector notification)
		{
			var result = false;
			IWeightHolder holder = this;
			foreach (var childWeightHolder in holder.WeightHolders)
			{
				var invoiceHolder = (AllocateRemainingWeightInvoiceHolder)childWeightHolder;
				var invoiceNumber = invoiceHolder.InvoiceHeader.JZ_InvoiceNumber;
				if (!childWeightHolder.AllApportionees.Any())
				{
					if (invoiceHolder.IsApportionNetWeight)
					{
						notification?.Notifications?.AddWarning(Res.GetString("0C1EF2D9-AC94-4F9E-9AE3-273E02A830AC", "The remaining net weight cannot be allocated because all invoice lines under the invoice (Invoice Number: {0}) have net weight.", invoiceNumber));
					}
					else
					{
						notification?.Notifications?.AddWarning(Res.GetString("3644EE26-4902-4C85-90C3-CE5F6CCEF24E", "The remaining gross weight cannot be allocated because all invoice lines under the invoice (Invoice Number: {0}) have gross weight.", invoiceNumber));
					}
				}
				else
				{
					if (childWeightHolder.TotalNetWeight.Amount <= 0 && invoiceHolder.IsApportionNetWeight)
					{
						notification?.Notifications?.AddWarning(Res.GetString("5A43501A-1CE5-4ABA-A2E9-2940A4491A2C", "The remaining net weight cannot be allocated because the sum of the entered invoice line net weight is larger than or equal to the entered invoice header (Invoice Number: {0}) net weight.", invoiceNumber));
					}
					else if (childWeightHolder.TotalWeight.Amount <= 0 && !invoiceHolder.IsApportionNetWeight)
					{
						notification?.Notifications?.AddWarning(Res.GetString("0696F658-009C-419E-8276-DBD0B5118537", "The remaining gross weight cannot be allocated because the sum of the entered invoice line gross weight is larger than or equal to the entered invoice header (Invoice Number: {0}) gross weight.", invoiceNumber));
					}
					else
					{
						result = true;
					}
				}
			}
			return result;
		}

		class AllocateRemainingWeightInvoiceHolder : IWeightHolder
		{
			public AllocateRemainingWeightInvoiceHolder(BaseJobComInvoiceHeader invoiceHeader, AllocateRemainingWeightWrapper parent, bool isApportionNetWeight)
			{
				invoiceHeaderWeightHolder = invoiceHeader;
				InvoiceHeader = invoiceHeader;
				this.parent = parent;
				IsApportionNetWeight = isApportionNetWeight;
			}

			readonly IWeightHolder invoiceHeaderWeightHolder;
			readonly AllocateRemainingWeightWrapper parent;
			public bool IsApportionNetWeight { get; }
			public BaseJobComInvoiceHeader InvoiceHeader { get; }

			ZWeight IWeightHolder.TotalWeight
			{
				get
				{
					var result = ZWeight.Empty;
					if (!IsApportionNetWeight)
					{
						var totalWeight = invoiceHeaderWeightHolder.TotalWeight;
						result = totalWeight;
						Apportionees.Where(x => !x.Weight.IsEmpty).ForEach(apportionee =>
						{
							result -= new ZWeight(apportionee.Weight, apportionee.WeightUQ.IsEmpty ? totalWeight.Unit : apportionee.WeightUQ);
						});
					}
					return result;
				}
			}

			ZWeight IWeightHolder.TotalNetWeight
			{
				get
				{
					var result = ZWeight.Empty;
					if (IsApportionNetWeight)
					{
						var totalNetWeight = invoiceHeaderWeightHolder.TotalNetWeight;
						result = totalNetWeight;
						Apportionees.Where(x => !x.NetWeight.IsEmpty).ForEach(apportionee =>
						{
							result -= new ZWeight(apportionee.NetWeight, apportionee.NetWeightUQ.IsEmpty ? totalNetWeight.Unit : apportionee.NetWeightUQ);
						});
					}
					return result;
				}
			}

			IWeightApportionee[] IWeightHolder.AllApportionees => Apportionees.Where(x => IsApportionNetWeight ? x.NetWeight.IsEmpty : x.Weight.IsEmpty).ToArray();

			IWeightApportionee[] Apportionees => InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Select(x => GetWeightApportionee(x)).ToArray();

			IWeightApportionee GetWeightApportionee(BaseJobComInvoiceLine invoiceLine) => new AllocateRemainingWeightApportionee(invoiceLine, parent, IsApportionNetWeight);

			IWeightHolder[] IWeightHolder.WeightHolders => null;
		}

		class AllocateRemainingWeightApportionee : IWeightApportionee
		{
			public AllocateRemainingWeightApportionee(BaseJobComInvoiceLine invoiceLine, AllocateRemainingWeightWrapper parent, bool needToApportionNetWeight)
			{
				weightApportionee = invoiceLine;
				this.invoiceLine = invoiceLine;
				this.parent = parent;
				this.needToApportionNetWeight = needToApportionNetWeight;
			}

			readonly BaseJobComInvoiceLine invoiceLine;
			readonly AllocateRemainingWeightWrapper parent;
			protected readonly IWeightApportionee weightApportionee;
			readonly bool needToApportionNetWeight;

			public ZDecimal Amount
			{
				get
				{
					var result = ZDecimal.Zero;
					switch (parent.ByWay)
					{
						case AllocateRemainingWeightWay.ByPrice:
							result = invoiceLine.LinePriceForWeightApportionCalculation;
							break;
						case AllocateRemainingWeightWay.ByQuantity:
							result = invoiceLine.JI_InvoiceQuantity;
							break;
					}
					return result;
				}
			}

			public virtual ZDecimal Weight { get => weightApportionee.Weight; set => weightApportionee.Weight = value; }
			public virtual ZString WeightUQ { get => weightApportionee.WeightUQ; set => weightApportionee.WeightUQ = value; }
			public virtual ZDecimal NetWeight { get => weightApportionee.NetWeight; set => weightApportionee.NetWeight = value; }
			public virtual ZString NetWeightUQ { get => weightApportionee.NetWeightUQ; set => weightApportionee.NetWeightUQ = value; }

			bool IWeightApportionee.NeedToApportionNetWeight => needToApportionNetWeight;

			ZDecimal IWeightApportionee.MinimumReapportionedLineWeight => weightApportionee.MinimumReapportionedLineWeight;
		}
	}
}
