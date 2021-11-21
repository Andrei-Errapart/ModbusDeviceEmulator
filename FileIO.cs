using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ModbusDeviceEmulator
{
    class FileIO
    {
        [DataContract()]
        private class Pin
        {
            [DataMember()]
            public string SignalName;
            [DataMember()]
            public int Value1bit;
            [DataMember()]
            public int Value16bit;
            [DataMember()]
            public int ChangePerMinute;
        }

        [DataContract()]
        private class Device
        {
            [DataMember()]
            public int Address;
            [DataMember()]
            public Pin[] Pins;
        }


        // ======================================================================
        /// Object to Json 
        private static string json<T>(T myObj)
        {
            using (var ms = new System.IO.MemoryStream() )
            {
                (new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T))).WriteObject(ms, myObj);
                return System.Text.Encoding.Default.GetString(ms.ToArray());
            }
        }

        // ======================================================================
        /// Object from Json 
        private static T unjson<T>(string jsonString)
        {
            using (var ms = new System.IO.MemoryStream(System.Text.ASCIIEncoding.Default.GetBytes(jsonString)))
            {
                var obj = (new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T))).ReadObject(ms);
                return (T)obj;
            }
        }

        // ======================================================================
        public static void Store(string Filename, IList<IOModuleControl> devices)
        {
            Device[] fdevices = new Device[devices.Count];
            for (int d_index=0; d_index<devices.Count; ++d_index)
            {
                var rd = devices[d_index];
                var fd = new Device() { Address=rd.Address, Pins = new Pin[rd.Pins.Count] };
                for (int p_index = 0; p_index < rd.Pins.Count; ++p_index)
                {
                    var rp = rd.Pins[p_index];
                    fd.Pins[p_index] = new Pin() { SignalName = rp.SignalName, Value1bit = rp.Value1bit, Value16bit = rp.Value16bit, ChangePerMinute=rp.ChangePerMinute };
                }
                fdevices[d_index] = fd;
            }
            System.IO.File.WriteAllText(Filename, json<Device[]>(fdevices));
        }

        // ======================================================================
        public static void Load(string Filename, IList<IOModuleControl> devices)
        {
            var json_string = System.IO.File.ReadAllText(Filename);
            var fdevices = unjson<Device[]>(json_string);
            foreach (var fd in fdevices)
            {
                foreach (var rd in devices)
                {
                    if (rd.Address == fd.Address)
                    {
                        foreach (var fp in fd.Pins)
                        {
                            var rp = rd.Pins.Where((IOPin p) => p.SignalName == fp.SignalName).SingleOrDefault();
                            if (rp != null)
                            {
                                rp.Value1bit = fp.Value1bit;
                                rp.Value16bit = fp.Value16bit;
                                rp.ChangePerMinute = fp.ChangePerMinute;
                            }
                        }
                    }
                }
            }
        }
    }
}
