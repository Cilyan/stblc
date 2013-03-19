using System;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;

namespace stblc
{
    class MainClass
    {
        public static int Main (string[] args)
        {
            /* Little boilerplate to parse command line */
            if (args.Length == 1) {
                if ((args[0] == "-h") || (args[0] == "--help")) {
                    printUsage ();
                    return -1;
                } else {
                    string filein = args [0];
                    string fileout = filein.Substring (0, filein.LastIndexOf ('.')) + ".stbl";
                    return convertFile (filein, fileout);
                }
            } else if (args.Length == 2) {
                string filein = args [0];
                string fileout = args [1];
                return convertFile (filein, fileout);
            } else {
                printUsage ();
                return -1;
            }
        }

        /**
         * This is where the real things are handled.
         */
        public static int convertFile(string filein, string fileout) {
            try {
                XDocument doc = XDocument.Load(filein);
                STBLWriter stblWriter = new STBLWriter(fileout);
                int result;
                if (doc.Root.Name == "TEXT") {
                    result = parseTwallanFormat (doc.Root, stblWriter);
                } else {
                    result = parseXSTBLFormat (doc.Root, stblWriter);
                }
                stblWriter.write ();
                stblWriter.close ();
                return result;
            } catch (System.IO.FileNotFoundException e) {
                Console.WriteLine (e.Message);
                return -1;
            }
        }

        /**
         * This function parses an XML file formatted like
         * what Twallan's STBL.exe awaits
         */
        private static int parseTwallanFormat(XElement Root, STBLWriter stblWriter) {
            bool awaits_key = true;
            bool warnings = false;
            string key = "";
            foreach(XElement elem in Root.Descendants()) {
                // KEY tag
                if (awaits_key) {
                    if (elem.Name == "KEY") {
                        key = preprocessKey(elem.Value);
                        awaits_key = false;
                    } else {
                        Console.WriteLine ("WARN: <KEY> awaited at {0}. Skipping.", ((IXmlLineInfo)elem).LineNumber);
                        warnings = true;
                    }
                // STR tag
                } else {
                    if (elem.Name == "STR") {
                        if (stblWriter.add(key, elem.Value) != 0) {
                            Console.WriteLine ("WARN: element at {0} is duplicate.", ((IXmlLineInfo)elem).LineNumber);
                        }
                        awaits_key = true;
                    } else {
                        Console.WriteLine ("WARN: <STR> awaited at {0}. Skipping.", ((IXmlLineInfo)elem).LineNumber);
                        awaits_key = true;
                        warnings = true;
                    }
                }
            }
            if (warnings) {
                return -2;
            }
            return 0;
        }

        /**
         * Little helper function to make writing keys more simple by allowing
         * to use the FNV function in a KEY like
         * <KEY>Gameplay/Excel/buffs/BuffOrigins:FNV(MyPackage.Origin.MyOrigin)</KEY>
         */
        private static string preprocessKey(string key) {
            int start, end;
            string before, skey, after;
            ulong ukey;

            /* search FNV( function call */
            start = key.IndexOf ("FNV(");
            if (start == -1) {
                /* No function, return key unchanged */
                return key;
            }
            /* Find end of function call */
            end = key.IndexOf (")", start+4);
            /* Be nice to those who forget to close the call... */
            if (end == -1) {
                Console.WriteLine("WARN: Closing of FNV() call not found");
                end = key.Length;
            }
            /* Hash the content of FNV(), also allowing nested calls */
            /* (is it useful? :) ) */
            ukey = STBLWriter.FNV64 (preprocessKey (key.Substring (start+4, end-start-4)));
            /* Get the pieces */
            before = key.Substring (0, start);
            skey = ukey.ToString ("X16");
            after = "";
            /* Possible use of another FNV() in the rest of the string */
            if (end < key.Length) {
                after = preprocessKey(key.Substring (end+1));
            }
            /* Put the pieces back together */
            return before + skey + after;
        }

        /**
         * This function parses an XML file in the new format
         * allowed by stblc
         */
        private static int parseXSTBLFormat(XElement Root, STBLWriter stblWriter) {
            Console.WriteLine("Unimplemented at the moment");
            return 0;
        }

        /**
         * This function prints a little help to the user
         */
        public static void printUsage() {
            Console.WriteLine("Usage: " + Process.GetCurrentProcess().ProcessName + " input.xml [output.stbl]");
        }
    }
}
