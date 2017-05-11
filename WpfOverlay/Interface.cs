using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
 
namespace WpfOverlay
{
     public interface WPFInterface
       {
           object GetObject(string ObjectName);
       }
       
    public class InterfaceBridge : MainWindow, WPFInterface
    {
        MainWindow _mainwindow = new MainWindow(); 
        public object GetObject(string ObjectName)
        {
            try
            {
                FieldInfo fld = typeof(MainWindow).GetField(ObjectName);
                Console.WriteLine("Value of interface call is : " + fld.GetValue(null));
                Type TheType = fld.GetValue(null).GetType();
                Console.WriteLine("Type has been found as " + TheType.ToString());
                object bam = Convert.ChangeType(fld.GetValue(null), TheType);
                Console.WriteLine("Bam bool value is: " + bam);
                return bam;
            }
            catch (Exception E)
            {
                MessageBox.Show(E.ToString(), "GetObject()");
                return new object();
            }
        }
    }
}
