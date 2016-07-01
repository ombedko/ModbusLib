//  Ole M. Brastein
//  Date: 2014.02.05
//  Rev 1.0 - Initial release
//
//
//  Description:
//  CMBMsg implemenst a class to hold one modbus msg with all its parameters and data. The class also has functions for converting the msg to 
//  array used for sending. This class handles convertions(bytewapping) of the header, but the msg handlers has to deal with byteswapping of the PDU data, since msg patterns are unique to each function code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModBus {
	public enum MBFunctionType : byte { enReadCoils = 0x01, enReadDiscreteInputs = 0x02, enReadHoldingRegisters = 0x03, enReadInputRegisters = 0x04, enWriteSingleCoil = 0x05, enWriteSingleRegister = 0x06, enWriteMultipleCoils = 0x0F, enWriteMultipleRegisters = 0x10, enReadSlaveID = 0x11 };
	public enum MBExceptionCode : byte { enOK = 0x00, enIllegalFunction = 0x01, enIllegalDataAddress = 0x02, enIllegalDataValue = 0x03,enUnknown = 0xFF,enWrongTransactionID = 0xFE }

	public class MBMsg {
		#region Properties
		public byte UnitID { get; protected set; }
		public MBFunctionType FuncID { get; protected set; }
		public byte[] Data { get; protected set; }
		public MBExceptionCode ExceptionCode { get; protected set; }
		#endregion

		#region constructor	
		protected MBMsg(){
			UnitID = 0;
			FuncID = MBFunctionType.enReadSlaveID;
			Data = null;			
		}

		protected MBMsg(byte nUnitID, MBFunctionType nFuncID, byte[] pMsgData) {
			UnitID = nUnitID;
			FuncID = nFuncID;
			Data = new byte[pMsgData.Length];
			Array.Copy(pMsgData, Data, pMsgData.Length);			
		}
		#endregion

		#region virtual methods
		public virtual byte[] ToArray() {
			return null;
		}
		public virtual bool FromArray(byte[] pMsgAsBytes) {
			return false;
		}
		#endregion

		#region helper functions
		//  Helper functions to do byte swapping and get/put shorts from byte arrays
		public static void PutShortToByteSwappedBuffer(byte[] pDstBuf, int nOffset, ushort nSrc) {
			byte[] pSrc = BitConverter.GetBytes(nSrc);

			if(pDstBuf.Length > nOffset + 1) {
				pDstBuf[nOffset + 1] = pSrc[0];
				pDstBuf[nOffset + 0] = pSrc[1];
			}
			else
				return;
				//throw new Exception("Not enough data in destination array");
		}
		public static short GetShortFromByteSwappedBuffer(byte[] pSrcBuf, int nOffset) {
			short nDst = 0;
			byte[] pDst = new byte[2];

			if(pSrcBuf.Length > nOffset + 1) {
				pDst[0] = pSrcBuf[nOffset + 1];
				pDst[1] = pSrcBuf[nOffset + 0];
				nDst = BitConverter.ToInt16(pDst, 0);
				return nDst;
			}
			else
				return 0;
				//throw new Exception("Not enough data in source array");
			
			
		}
		#endregion
	}
}
