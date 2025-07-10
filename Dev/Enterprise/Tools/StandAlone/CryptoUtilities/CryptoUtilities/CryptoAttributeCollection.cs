using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Enterprise.CryptoUtilities
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Previously located in ThirdParty. Include until it is known that it is not required.")]
	public class CryptoAttributeCollection : IDisposable
	{
		public CryptoAttributeCollection(params CryptoAttribute[] Attributes)
		{
			int AttributeStructSize = Marshal.SizeOf(typeof(CRYPT_ATTRIBUTE));
			Interlocked.Increment(ref Functions.CryptoAttributeCollectionsInMemory);
			this.Attributes = Attributes;

			//Marshal the Attributes and the array of Attribute pointers
			AttributePointerArrayCount = Attributes.Length;
			AttributePointerArray = Marshal.AllocHGlobal(AttributeStructSize * AttributePointerArrayCount);
			for (int i = 0; i < AttributePointerArrayCount; i++)
			{
				unchecked
				{
					Attributes[i].CopyToUnmanagedMemory(new IntPtr(AttributePointerArray.ToInt64() + i * AttributeStructSize));
				}
			}
		}

		public void PopulateAttributes(CRYPT_SIGN_MESSAGE_PARA Result)
		{
			Result.cAuthAttr = (uint)AttributePointerArrayCount;                                                    //number of authenticated attributes to include
			Result.rgAuthAttr = AttributePointerArray;
		}

		public void Dispose()
		{
			for (int i = 0; i < Attributes.Length; i++)
			{
				if (Attributes[i] != null)
				{
					Attributes[i].Dispose();
					Attributes[i] = null;
				}
			}
			if (AttributePointerArray != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(AttributePointerArray);
				AttributePointerArray = IntPtr.Zero;
				AttributePointerArrayCount = 0;
			}
			Interlocked.Decrement(ref Functions.CryptoAttributeCollectionsInMemory);
		}

		private readonly CryptoAttribute[] Attributes;
		internal IntPtr AttributePointerArray = IntPtr.Zero;
		internal int AttributePointerArrayCount;
	}
}
