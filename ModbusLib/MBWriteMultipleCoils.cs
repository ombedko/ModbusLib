using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModBus {
	public class MBCommandWriteMultipleCoils : MBCommand {
		public MBCommandWriteMultipleCoils(string sName) : base(sName) { }
		// build the command
		public void Write(bool[] data) {
			if(data.GetLength(0) != _nQuantity)
				return;
			byte nBytes = (byte)(_nQuantity / 8);
			if(_nQuantity % 8 != 0)
				nBytes++;

			//create data part of this msg
			byte[] pMsgData = new byte[nBytes + 5];
			pMsgData[0] = BitConverter.GetBytes(_nStartAdr)[1];
			pMsgData[1] = BitConverter.GetBytes(_nStartAdr)[0];
			pMsgData[2] = BitConverter.GetBytes(_nQuantity)[1];
			pMsgData[3] = BitConverter.GetBytes(_nQuantity)[0];
			
			pMsgData[4] = nBytes;
						
			byte nByteAdr = 0;
			byte nBitAdr = 0;
			for(byte i = 0; i < _nQuantity; i++) {
				nBitAdr = (byte)(i % 8);
				nByteAdr = (byte)(i / 8);

				if(data[i])	//true
					pMsgData[5 + nByteAdr] |= (byte)(0x01 << nBitAdr);
				else        //false
					pMsgData[5 + nByteAdr] &= (byte)(~(0x01 << nBitAdr));

			}
			

			//build command msg	and add to send que
			_QueMsg(0, MBFunctionType.enWriteMultipleCoils, pMsgData);
		}


		//  Handle reply from server, set control register buffers if needed
		public override void HandleMsg(MBMsg msg) {
			short nStartAdr;
			short nCount;

			//add some checking here, verify lengths ok and match the expected
			if(msg.Data.Length >= 4) {    //recieved correct number of bytes?            
				if((byte)msg.FuncID < 0x80) { //recieved an exception?                
					nStartAdr = MBMsg.GetShortFromByteSwappedBuffer(msg.Data, 0);
					nCount = MBMsg.GetShortFromByteSwappedBuffer(msg.Data, 2);
					if(nStartAdr != _nStartAdr && nCount != _nQuantity) {
						//recieved incorect ACK from server
					}
				}
			}
		}
	}
}
