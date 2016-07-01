using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModBus {
	public class MBMsgTCP : MBMsg {
		#region properties
		public ushort TransactionID { get; set; }
		public ushort ProtocolID { get; protected set; }
		public ushort Length { get; protected set; }
		#endregion

		#region constructor
		public MBMsgTCP()
			: base() {
		}

		public MBMsgTCP(byte nUnitID, MBFunctionType nFuncID, byte[] pMsgData)
			: base(nUnitID, nFuncID, pMsgData) {
				ProtocolID = 0;
				Length = (ushort)(pMsgData.Length + 2);
		}
		#endregion

		#region virtual methods
		public override bool FromArray(byte[] pBuf) {			
			if(pBuf.Length >= 8) {										//buffer must be at least 8 bytes for header

				//extract header
				TransactionID = (ushort)GetShortFromByteSwappedBuffer(pBuf, 0);
				ProtocolID = (ushort)GetShortFromByteSwappedBuffer(pBuf, 2);
				Length = (ushort)GetShortFromByteSwappedBuffer(pBuf, 4);
				UnitID = pBuf[6];
				FuncID = (MBFunctionType)pBuf[7];			
			
				//extract data
				if((byte)FuncID < 0x80) {

					if(Length >= 2 && pBuf.Length - 8 >= Length - 2) {	//Length must be at least 2 and buffer length -8 must be equal to length -2 or longer
						Data = new byte[Length - 2];					//alocate space
						Array.Copy(pBuf, 8, Data, 0, Length - 2);		//copy data
						ExceptionCode = MBExceptionCode.enOK;			//Set excepetion code
						return true;
					}
				}
				else {
					if(pBuf.Length >= 9) {								//buffer must be 9 (8 header + 1 exception code) bytes for exception code
						Data = new byte[1];								//only one byte data
						Data[0] = pBuf[8];								//copy data
						ExceptionCode = (MBExceptionCode)pBuf[8];		//Set excepetion code
						return true;
					}
				}				
			}
			return false;
		}

		public override byte[] ToArray() {
			byte[] pBuf = new byte[6 + Length];

			//put the msg header to array
			PutShortToByteSwappedBuffer(pBuf, 0, TransactionID);
			PutShortToByteSwappedBuffer(pBuf, 2, ProtocolID);
			PutShortToByteSwappedBuffer(pBuf, 4, Length);
			pBuf[6] = UnitID;
			pBuf[7] = (byte)FuncID;			
			Array.Copy(Data, 0, pBuf, 8, Length - 2);				//copy data
			return pBuf;
		}
		#endregion
	}
}
