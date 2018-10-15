using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using Modbus.Serial;
using Windows.Devices.SerialCommunication;
using System.Net;
using System.Net.Sockets;


//ModbusTcpMasterReadInputs();

//ModbusSerialRtuMasterWriteRegisters();

//ModbusSerialAsciiMasterReadRegisters();

//ModbusTcpMasterReadInputs();

//ModbusTcpMasterReadInputsFromModbusSlave();

//StartModbusTcpSlave();

//StartModbusUdpSlave();

namespace UWPModbusSampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(InputTextBox.Text))
            {
                ModbusTcpWriteSingleRegister();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ModbusTcpMasterReadInputs();
        }

        /// <summary>
        ///     Simple Modbus TCP master write holding registers example.
        /// </summary>
        public async void ModbusTcpWriteSingleRegister()
        {
            ushort inputValue = ushort.Parse(InputTextBox.Text);

            using (TcpClient client = new TcpClient())
            {
                
                client.ReceiveTimeout = 3000;
                client.SendTimeout = 3000;
                await client.ConnectAsync(IpAddressInputBox.Text, int.Parse(TcpPortInputBox.Text));
                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

                ushort address = ushort.Parse(writeRegisterInputBox.Text);

                master.WriteSingleRegister(address, inputValue);

            }
        }

        /// <summary>
        ///     Simple Modbus serial RTU master write holding registers example.
        /// </summary>
        public async void ModbusSerialRtuMasterWriteRegisters()
        {

            using (SerialDevice port = await SerialDevice.FromIdAsync("COM1"))

            {

                // configure serial port

                port.BaudRate = 9600;

                port.DataBits = 8;

                port.Parity = SerialParity.None;

                port.StopBits = SerialStopBitCount.One;

                var adapter = new SerialPortAdapter(port);

                // create modbus master

                IModbusSerialMaster master = ModbusSerialMaster.CreateRtu(adapter);



                byte slaveId = 1;

                ushort startAddress = 100;

                ushort[] registers = new ushort[] { 1, 2, 3 };



                // write three registers

                master.WriteMultipleRegisters(slaveId, startAddress, registers);

            }

        }

        /// <summary>
        ///     Simple Modbus serial ASCII master read holding registers example.
        /// </summary>
        public async void ModbusSerialAsciiMasterReadRegisters()

        {

            using (SerialDevice port = await SerialDevice.FromIdAsync("COM1"))

            {

                // configure serial port

                port.BaudRate = 9600;

                port.DataBits = 8;

                port.Parity = SerialParity.None;

                port.StopBits = SerialStopBitCount.One;



                var adapter = new SerialPortAdapter(port);

                // create modbus master

                IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(adapter);



                byte slaveId = 1;

                ushort startAddress = 1;

                ushort numRegisters = 5;



                // read five registers		

                ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);



                for (int i = 0; i < numRegisters; i++)

                {

                    //Console.WriteLine($"Register {startAddress + i}={registers[i]}");

                }

            }



            // output: 

            // Register 1=0

            // Register 2=0

            // Register 3=0

            // Register 4=0

            // Register 5=0

        }

        /// <summary>
        ///     Simple Modbus TCP master read inputs example.
        /// </summary>
        public async void ModbusTcpMasterReadInputs()
        {
            using (TcpClient client = new TcpClient())

            {
                client.ReceiveTimeout = 3000;
                client.SendTimeout = 3000;
                await client.ConnectAsync(IpAddressInputBox.Text, int.Parse(TcpPortInputBox.Text));
                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

                // read five input values

                ushort startAddress = ushort.Parse(readRegisterInputBox.Text);

                ushort numInputs = ushort.Parse(readRegisterNumberOfRegistersInputBox.Text);

                bool[] inputs = master.ReadInputs(startAddress, numInputs);

                for (int i = 0; i < numInputs; i++)

                {

                    ResultsTextBox.Text += ($"Input {(startAddress + i)}={(inputs[i] ? 1 : 0)}") + "\r\n";

                }

            }

        }

        /// <summary>
        ///     Simple Modbus UDP master write coils example.
        /// </summary>
        public void ModbusUdpMasterWriteCoils()
        {

            using (UdpClient client = new UdpClient())

            {

                IPEndPoint endPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 502);

                client.Client.Connect(endPoint);

                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);



                ushort startAddress = 1;



                // write three coils

                master.WriteMultipleCoils(startAddress, new bool[] { true, false, true });

            }

        }

        /// <summary>
        ///     Simple Modbus serial RTU slave example.
        /// </summary>
        public async void StartModbusSerialRtuSlave()

        {

            using (SerialDevice slavePort = await SerialDevice.FromIdAsync("COM2"))

            {

                // configure serial port

                slavePort.BaudRate = 9600;

                slavePort.DataBits = 8;

                slavePort.Parity = SerialParity.None;

                slavePort.StopBits = SerialStopBitCount.One;



                byte unitId = 1;



                var adapter = new SerialPortAdapter(slavePort);

                // create modbus slave

                ModbusSlave slave = ModbusSerialSlave.CreateRtu(unitId, adapter);

                slave.DataStore = DataStoreFactory.CreateDefaultDataStore();



                slave.ListenAsync().GetAwaiter().GetResult();

            }

        }

        /// <summary>
        ///     Simple Modbus serial USB ASCII slave example.
        /// </summary>
        public void StartModbusSerialUsbAsciiSlave()
        {

            // TODO

        }

        /// <summary>
        ///     Simple Modbus serial USB RTU slave example.
        /// </summary>
        public void StartModbusSerialUsbRtuSlave()
        {

            // TODO

        }

        /// <summary>
        ///     Simple Modbus TCP slave example.
        /// </summary>
        public void StartModbusTcpSlave()
        {

            byte slaveId = 1;

            int port = 502;

            IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });

            // create and start the TCP slave

            TcpListener slaveTcpListener = new TcpListener(address, port);

            slaveTcpListener.Start();



            ModbusSlave slave = ModbusTcpSlave.CreateTcp(slaveId, slaveTcpListener);

            slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

            slave.ListenAsync().GetAwaiter().GetResult();

        }

        /// <summary>
        ///     Simple Modbus UDP slave example.
        /// </summary>
        public void StartModbusUdpSlave()
        {
            using (UdpClient client = new UdpClient(502))

            {

                ModbusUdpSlave slave = ModbusUdpSlave.CreateUdp(client);

                slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

                slave.ListenAsync().GetAwaiter().GetResult();

            }
        }

        /// <summary>
        ///     Modbus TCP master and slave example.
        /// </summary>
        public async void ModbusTcpMasterReadInputsFromModbusSlave()
        {
            byte slaveId = 1;

            int port = 502;

            IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });

            // create and start the TCP slave

            TcpListener slaveTcpListener = new TcpListener(address, port);

            slaveTcpListener.Start();

            ModbusSlave slave = ModbusTcpSlave.CreateTcp(slaveId, slaveTcpListener);

            var listenTask = slave.ListenAsync();

            // create the master

            TcpClient masterTcpClient = new TcpClient();

            masterTcpClient.ReceiveTimeout = 3000;
            masterTcpClient.SendTimeout = 3000;
            await masterTcpClient.ConnectAsync(IpAddressInputBox.Text, int.Parse(TcpPortInputBox.Text));
            ModbusIpMaster master = ModbusIpMaster.CreateIp(masterTcpClient);

            ushort numInputs = 5;

            ushort startAddress = 100;

            // read five register values

            ushort[] inputs = master.ReadInputRegisters(startAddress, numInputs);

            for (int i = 0; i < numInputs; i++)
            {

                ResultsTextBox.Text += ($"Register {(startAddress + i)}={(inputs[i])}") + "\r\n";

            }



            // clean up

            masterTcpClient.Dispose();

            slaveTcpListener.Stop();

        }

        /// <summary>
        ///     Write a 32 bit value.
        /// </summary>
        public async void ReadWriteSerial32BitValue()
        {

            using (SerialDevice device = await SerialDevice.FromIdAsync("device_id"))

            {

                // configure serial port

                device.BaudRate = 9600;

                device.DataBits = 8;

                device.Parity = SerialParity.Even;

                device.StopBits = SerialStopBitCount.One;



                var adapter = new SerialPortAdapter(device);

                // create modbus master

                ModbusSerialMaster master = ModbusSerialMaster.CreateRtu(adapter);



                byte slaveId = 1;

                ushort startAddress = 1008;

                uint largeValue = UInt16.MaxValue + 5;



                ushort lowOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes(largeValue), 0);

                ushort highOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes(largeValue), 2);



                // write large value in two 16 bit chunks

                master.WriteMultipleRegisters(slaveId, startAddress, new ushort[] { lowOrderValue, highOrderValue });



                // read large value in two 16 bit chunks and perform conversion

                ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress, 2);

                uint value = ModbusUtility.GetUInt32(registers[1], registers[0]);

            }

        }

    }
}
