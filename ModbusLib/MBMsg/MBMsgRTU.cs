using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModBus {
	public class MBMsgRTU:MBMsg {
		#region properties
		public ushort CRC{get;private set;}
		#endregion

		#region constructor
		public MBMsgRTU()
			: base() {
		}

		public MBMsgRTU(byte nUnitID, MBFunctionType nFuncID, byte[] pMsgData)
			: base(nUnitID, nFuncID, pMsgData) {				
		}
		#endregion

		#region virtual methods
		public override bool FromArray(byte[] pBuf) {			
			if(pBuf.Length >= 2) {										//buffer must be at least 2 bytes for header

				//extract header
				UnitID = pBuf[0];
				FuncID = (MBFunctionType)pBuf[1];			
			
				//extract data
				if((byte)FuncID < 0x80) {
					Data = new byte[pBuf.Length-2];						//alocate space
					Array.Copy(pBuf, 2, Data, 0, pBuf.Length-2);		//copy data
					ExceptionCode = MBExceptionCode.enOK;				//Set excepetion code
					return true;					
				}
				else {
					if(pBuf.Length >= 3) {								//buffer must be 3 (2 header + 1 exception code) bytes for exception code
						Data = new byte[1];								//only one byte data
						Data[0] = pBuf[2];								//copy data
						ExceptionCode = (MBExceptionCode)pBuf[2];		//Set excepetion code
						return true;
					}
				}				
			}
			return false;
		}

		public override byte[] ToArray() {
			byte[] pBuf = new byte[2 + Data.Length + 2];

			//put the msg header to array			
			pBuf[0] = UnitID;											//set unit ID
			pBuf[1] = (byte)FuncID;										//set func ID
			Array.Copy(Data, 0, pBuf, 2, Data.Length);					//copy data

			CRC = ModRTU_CRC(pBuf, 2 + Data.Length);				//calculate crc
			pBuf[pBuf.Length - 2] = BitConverter.GetBytes(CRC)[0];
			pBuf[pBuf.Length - 1] = BitConverter.GetBytes(CRC)[1];
			return pBuf;
		}
		#endregion

		#region helpers
		// Compute the MODBUS RTU CRC
		private static ushort ModRTU_CRC(byte[] buf, int len, ushort crc_init = 0xFFFF){
		  ushort crc = crc_init; //0xFFFF;
 
		  for(ushort pos = 0; pos < len; pos++) {
			crc ^= (ushort)buf[pos];          // XOR byte into least sig. byte of crc
 
			for(byte i = 8; i != 0; i--) {    // Loop over each bit
			  if ((crc & 0x0001) != 0) {      // If the LSB is set
				crc >>= 1;                    // Shift right and XOR 0xA001
				crc ^= 0xA001;
			  }
			  else                            // Else LSB is not set
				crc >>= 1;                    // Just shift right
			}
		  }
		  // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)
		  return crc;  
		}
		#endregion
	}	
}
