//  Ole M. Brastein
//  Date: 2014.02.11
//  Rev 1.0 - Initial release
//
//
//  Description:
//  Implements building and interpretation of WriteMultipleRegisters commands

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModBus {
	public class MBCommandWriteMultipleRegisters : MBCommand {
		public MBCommandWriteMultipleRegisters(string sName) : base(sName) { }

		// build the command
		public void Write(short[] data) {
			if(data.GetLength(0) != _nQuantity)
				return;

			//create data part of this msg
			byte[] pMsgData = new byte[_nQuantity * 2 + 5];
			pMsgData[0] = BitConverter.GetBytes(_nStartAdr)[1];
			pMsgData[1] = BitConverter.GetBytes(_nStartAdr)[0];
			pMsgData[2] = BitConverter.GetBytes(_nQuantity)[1];
			pMsgData[3] = BitConverter.GetBytes(_nQuantity)[0];
			pMsgData[4] = (byte)(_nQuantity * 2);

			for(int i = 0; i < _nQuantity; i++) {
				pMsgData[5 + 2 * i] = BitConverter.GetBytes(data[i])[1];
				pMsgData[6 + 2 * i] = BitConverter.GetBytes(data[i])[0];
			}

			//build command msg	and add to send que
			_QueMsg(0, MBFunctionType.enWriteMultipleRegisters, pMsgData);
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
