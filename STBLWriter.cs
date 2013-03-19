using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace stblc
{
    public class STBLWriter
    {
        FileStream outStream;
        BinaryWriter outWriter;
        Dictionary <ulong, string> strDict;

        public STBLWriter (string fileout)
        {
            outStream = new FileStream (fileout, FileMode.Create, FileAccess.Write);
            outWriter = new BinaryWriter (outStream, Encoding.Unicode);
            strDict = new Dictionary <ulong, string> ();
        }

        public int add(string key, string str) {
            ulong ukey = FNV64 (key);
            if (strDict.ContainsKey (ukey)) {
                strDict [ukey] = str;
                return 1;
            }
            strDict.Add (ukey, str);
            return 0;
        }

        public void write() {
            /* header 'STBL'*/
            outWriter.Write (new byte[] {0x53, 0x54, 0x42, 0x4C});
            /* version */
            outWriter.Write (new byte[] {2});
            /* blank */
            outWriter.Write (new byte[2]);
            /* count */
            outWriter.Write (strDict.Count);
            /* blank */
            outWriter.Write (new byte[6]);
            /* items */
            foreach (KeyValuePair<ulong, string> item in strDict) {
                /* Key */
                outWriter.Write (item.Key);
                /* Size */
                outWriter.Write (item.Value.Length);
                /* Message */
                outWriter.Write (item.Value.ToCharArray ());
            }
        }


        /**
         * Encode a string into an pseudo 64 bits Fowler–Noll–Vo hash
         * suitable for The Sims 3
         */
        public static ulong FNV64 (string data)
        {
            string lower_data = data.ToLower ();
            ulong hash = 0xcbf29ce484222325uL;
            for (int i = 0; i < lower_data.Length; i++)
            {
                byte b = (byte)lower_data[i];
                hash *= 1099511628211uL;
                hash ^= (ulong)b;
            }
            return hash;
        }

        public void close() {
            outWriter.Close ();
            outStream.Close ();
        }
    }
}

