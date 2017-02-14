using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading;


namespace WpfOverlay
{
    class Ccompiler
    {

        public static void Compile(String lcCode)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //  Command.Remove(Delete);
                //  string lcCode = Delete.CommandContent;
               // Console.WriteLine(lcCode);


                // string lcCode = this.txtCode.Text;

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
                // *** Must create a fully functional assembly as a string


                // *** Load the resulting assembly into memory
                loParameters.GenerateInMemory = false;

                // *** Now compile the whole thing

                CompilerResults loCompiled =
                        loCompiler.CompileAssemblyFromSource(loParameters, lcCode);


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
                object loObject = loAssembly.CreateInstance("MyNamespace.MyClass");
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

    }
}
