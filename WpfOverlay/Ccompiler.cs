using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.CodeDom;
using System.Diagnostics;

namespace WpfOverlay
{
    class Ccompiler
    {

        public static void Compile(String lcCode, List<string> IntName = null, List<string> IntnameVal = null, List<string> StringName = null, List<string> StringNameVal = null)
        {
            var Timer4 = Stopwatch.StartNew();
            try { 

            if (VoiceRecognition.player != null)
            {
                VoiceRecognition.player.Play();
            }
            new Thread(() =>
            {
                string CodeTcompile;
                if (IntName != null || StringName != null)
                {
                    CodeTcompile = FinalParse(lcCode, IntName, IntnameVal, StringName, StringNameVal);
                    //  Debug.WriteLine("Finished Parsing The info ,  value are " + CodeTcompile);
                }
                else
                {
                    CodeTcompile = lcCode;
                }
                Thread.CurrentThread.IsBackground = true;

                CodeDomProvider loCompiler = CodeDomProvider.CreateProvider("CSharp");

                CompilerParameters loParameters = new CompilerParameters();
                // *** Start by adding any referenced assemblies
                loParameters.ReferencedAssemblies.Add("System.dll");
                loParameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                loParameters.ReferencedAssemblies.Add("System.Speech.dll");
                loParameters.ReferencedAssemblies.Add("System.Xml.dll");
                loParameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
                loParameters.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");
                loParameters.ReferencedAssemblies.Add("NetworkCommsDotNet.dll");
                loParameters.ReferencedAssemblies.Add("protobuf-net.dll");
                loParameters.ReferencedAssemblies.Add("Interface.dll");
                loParameters.ReferencedAssemblies.Add("System.Core.dll");

                // *** Must create a fully functional assembly as a string **Fully Functional assembly string is included in the making of the command


                // *** Load the resulting assembly into memory
                loParameters.GenerateInMemory = false;

                // *** Now compile the whole thing

                CompilerResults loCompiled =
                        loCompiler.CompileAssemblyFromSource(loParameters, CodeTcompile);

                if (loCompiled.Errors.HasErrors)
                {
                    string lcErrorMsg = "";

                    lcErrorMsg = loCompiled.Errors.Count.ToString() + " Errors:";
                    for (int x = 0; x < loCompiled.Errors.Count; x++)
                        lcErrorMsg = lcErrorMsg + "\r\nLine: " +
                                     loCompiled.Errors[x].Line.ToString() + " - " +
                                     loCompiled.Errors[x].ErrorText;

                    MessageBox.Show(lcErrorMsg + "\r\n\r\n" + lcCode,
                                    "Compiler Demo");
                    return;
                }


                Assembly loAssembly = loCompiled.CompiledAssembly;

                // *** Retrieve an obj ref – generic type only
                object loObject = loAssembly.CreateInstance("WpfOverlay.MyClass");
                if (loObject == null)
                {
                    MessageBox.Show("Couldn't load class.");
                    return;
                }

                object[] loCodeParms = new object[1];
                loCodeParms[0] = "West Wind Technologies";

                try
                {
                    object loResult = loObject.GetType().InvokeMember(
                                     "DynamicCode", BindingFlags.InvokeMethod,
                                     null, loObject, loCodeParms);

                    //  DateTime ltNow = (DateTime)loResult;
                    // MessageBox.Show("Method Call Result:\r\n\r\n" +
                    //      loResult.ToString(), "Compiler Demo");
                }
                catch (Exception loError)
                {
                    MessageBox.Show(loError.Message, "Compiler Demo");
                }
            }).Start();
        }
            finally
            {
                Timer4.Stop();
                Debug.WriteLine("Compiler Took " + Timer4.ElapsedMilliseconds + " ms");
            }
        }


     public static string  FinalParse(String lcCode, List<string> IntNames = null, List<string> IntVals = null, List<string> StringName = null, List<string> StringNameVal = null)//The FINAL parse that inserts the variable values to the code string
        {
            try
            {
                if (IntVals != null)
                {
                    if (IntNames.Count() < IntVals.Count())
                    {
                        MessageBox.Show(IntNames.Count() + " IntNames And " + IntVals.Count() + " Values", "Compiler String Injector"); //safety check
                        return null;
                    }


                    int Counter = 0;
                    foreach (string INTVAL in IntVals)
                    {
                        int index = lcCode.IndexOf("int " + IntNames[Counter]) + IntNames[Counter].Length + 4;
                        if (index != -1)
                        {
                            lcCode = lcCode.Insert(index, "=" + INTVAL);
                            Counter += 1;
                        }
                    }
                }
                if(StringNameVal != null)
                {
                    if (StringName.Count() < StringNameVal.Count())
                    {
                        MessageBox.Show(IntNames.Count() + " IntNames And " + IntVals.Count() + " Values", "Compiler String Injector"); //safety check
                        return null;
                    }


                    int Counter = 0;
                    foreach (string STRINGVAL in StringNameVal)
                    {
                        Debug.WriteLine("StringNamE :" + StringName[Counter]);
                        int index = lcCode.IndexOf("string " + StringName[Counter]) + StringName[Counter].Length + 7;
                        Debug.WriteLine("Final Index Is : " + index);
                        if (index != -1)
                        {
                            lcCode = lcCode.Insert(index, "=" + '"' + STRINGVAL + '"');
                            Counter += 1;
                        }
                    }
                }
            }
            catch (Exception E)
            {
                MessageBox.Show(E.ToString(), "Compiler String/int Injector");
                return null;
            }
            return lcCode;

        }

    }
}
