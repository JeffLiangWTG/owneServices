using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	[AllowPublicConstructor, AllowNoStaticNew]
	public class DocDA74Container : DocumentWrapper, IObsoleteValidation
	{
		public DocDA74Container(ZString containerNumber1, ZString containerNumber2, BusinessObjectFactory factory)
			: base(null, factory)
		{
			fFirstContainerNumber = containerNumber1;
			fSecondContainerNumber = containerNumber2;
			containerNumberTable1 = new Hashtable();
			containerNumberTable2 = new Hashtable();
			SetContainerNumbers(containerNumber1, containerNumberTable1);
			SetContainerNumbers(containerNumber2, containerNumberTable2);
		}

		public override string ToString()
		{
			return GetType().ToString();
		}

		#region FirstContainer
		public ZString FirstContainerNumber
		{
			get { return fFirstContainerNumber; }
		}

		public ZString FirstContainerPrefix1
		{
			get { return containerNumberTable1[0].ToString(); }
		}

		public ZString FirstContainerPrefix2
		{
			get { return containerNumberTable1[1].ToString(); }
		}

		public ZString FirstContainerPrefix3
		{
			get { return containerNumberTable1[2].ToString(); }
		}

		public ZString FirstContainerPrefix4
		{
			get { return containerNumberTable1[3].ToString(); }
		}

		public ZString FirstContainerNumber1
		{
			get { return containerNumberTable1[4].ToString(); }
		}

		public ZString FirstContainerNumber2
		{
			get { return containerNumberTable1[5].ToString(); }
		}

		public ZString FirstContainerNumber3
		{
			get { return containerNumberTable1[6].ToString(); }
		}

		public ZString FirstContainerNumber4
		{
			get { return containerNumberTable1[7].ToString(); }
		}

		public ZString FirstContainerNumber5
		{
			get { return containerNumberTable1[8].ToString(); }
		}

		public ZString FirstContainerNumber6
		{
			get { return containerNumberTable1[9].ToString(); }
		}

		public ZString FirstContainerNumber7
		{
			get { return containerNumberTable1[10].ToString(); }
		}

		#endregion

		#region SecondContainer
		public ZString SecondContainerNumber
		{
			get { return fSecondContainerNumber; }
		}

		public ZString SecondContainerPrefix1
		{
			get { return containerNumberTable2[0].ToString(); }
		}

		public ZString SecondContainerPrefix2
		{
			get { return containerNumberTable2[1].ToString(); }
		}

		public ZString SecondContainerPrefix3
		{
			get { return containerNumberTable2[2].ToString(); }
		}

		public ZString SecondContainerPrefix4
		{
			get { return containerNumberTable2[3].ToString(); }
		}

		public ZString SecondContainerNumber1
		{
			get { return containerNumberTable2[4].ToString(); }
		}

		public ZString SecondContainerNumber2
		{
			get { return containerNumberTable2[5].ToString(); }
		}

		public ZString SecondContainerNumber3
		{
			get { return containerNumberTable2[6].ToString(); }
		}

		public ZString SecondContainerNumber4
		{
			get { return containerNumberTable2[7].ToString(); }
		}

		public ZString SecondContainerNumber5
		{
			get { return containerNumberTable2[8].ToString(); }
		}

		public ZString SecondContainerNumber6
		{
			get { return containerNumberTable2[9].ToString(); }
		}

		public ZString SecondContainerNumber7
		{
			get { return containerNumberTable2[10].ToString(); }
		}
		#endregion

		#region Implementation
		protected void SetContainerNumbers(ZString containerNumber, Hashtable containerNumberTable)
		{
			int i = 0;
			for (i = 0; i < containerNumber.Length; i++)
			{
				containerNumberTable[i] = containerNumber[i].ToString();
			}

			if (containerNumberTable.Count < 11)
			{
				for (; i < 11; i++)
				{
					containerNumberTable[i] = "";
				}
			}
		}

		readonly Hashtable containerNumberTable1;
		readonly Hashtable containerNumberTable2;
		readonly ZString fFirstContainerNumber;
		readonly ZString fSecondContainerNumber;

		#endregion
	}
}
