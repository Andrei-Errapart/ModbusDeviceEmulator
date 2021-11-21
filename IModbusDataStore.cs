using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModbusDeviceEmulator
{
    public interface IModbusDataStore
    {
        bool IsConnected { get; }
        byte ReadCoils(byte[] dst, int dst_offset, int CoilAddress, int Count);
        byte ReadDiscreteInputs(byte[] dst, int dst_offset, int InputAddress, int Count);
        byte ReadHoldingRegisters(byte[] dst, int dst_offset, int RegisterAddress, int Count);
        byte ReadInputRegisters(byte[] dst, int dst_offset, int RegisterAddress, int Count);
        byte WriteSingleCoil(int CoilAddress, bool Value);
        byte WriteMultipleCoils(int CoilAddress, int Values, int Count);
        byte WriteSingleHoldingRegister(int RegisterAddress, int Value);
    };
}
