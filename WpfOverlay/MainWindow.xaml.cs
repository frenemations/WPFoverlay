using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Xml.Serialization;
using System.Speech.Synthesis;
using System.Diagnostics;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;
using System.Windows.Input;

namespace WpfOverlay
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //lable for External accessing?
        public Label MainWindowLabel;

        public bool EditFinal = false;
        public RunAtStartup DeleteStart;
        CompletionWindow completionWindow;
        public RunAtStartup SelectedStart;
        public bool StartupType = false;
        private bool Relocate = false;
        private int indexer;
        public Command selected;
        public bool IsEdit = false;
        public bool IsStateButtonName = true;
        public string CurrentButtonName;
        public string CurrentCommandText;
        public string Voicetrigger;
        private bool auth = false;
        private bool hide = true;
        private bool Trigger = false;
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;
      public static SpeechSynthesizer synth = new SpeechSynthesizer();
        public static ObservableCollection<Command> Command;
        ObservableCollection<RunAtStartup> RunAtS;
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);

    

        public MainWindow()
        {

          

            foreach (var voice in synth.GetInstalledVoices())
                Console.WriteLine(voice.VoiceInfo.Name);

            RunAtS = new ObservableCollection<RunAtStartup>();

            RunAtStartup Boop = new RunAtStartup
            {
                DisplayName = "TEST",
                CommandContent2 = "BOOP"
            };
            RunAtS.Add(Boop);
          
            XmlSerializer xsx = new XmlSerializer(typeof(ObservableCollection<RunAtStartup>));
            using (StreamReader rd = new StreamReader("StartupCommands.xml"))
            {
                try {
                    RunAtS = xsx.Deserialize(rd) as ObservableCollection<RunAtStartup>;
                }
                catch(Exception E)
                {
                    Console.WriteLine(E);
                }
            }



            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Command>));
            using (StreamReader rd = new StreamReader("Commands.xml"))
            {
                Command = xs.Deserialize(rd) as ObservableCollection<Command>;
                //Console.WriteLine(Command.Last());

            }

            
            InitializeComponent();
            CompileAtStartup();
            Test.initializeVrec();
             COMI.Intialize();
            Client.Main();
            //initianize AvanonEdit Code Completion Handlers
            textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;

            // Set Propperties Of speech synthesizer
            synth.SelectVoice("Microsoft Zira Desktop");
            synth.Rate = 3;

            // Configure the audio output. 
            synth.SetOutputToDefaultAudioDevice();





          



            //timer For Clock
            GetSysTime(null, new EventArgs());
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(GetSysTime);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 01);
            dispatcherTimer.Start();

            StartListeningForWindowChanges();

            CommandBox.DataContext = Command;
            RunAtSTartup.DataContext = RunAtS;
        }

        //AvalonEdit Code Completion handlers
        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                // Open code completion after the user has pressed dot:
                completionWindow = new CompletionWindow(textEditor.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("Item1"));
                data.Add(new MyCompletionData("Item2"));
                data.Add(new MyCompletionData("Item3"));
                completionWindow.Show();
                completionWindow.Closed += delegate {
                    completionWindow = null;
                };
            }
        }


        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }




        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        public static int Handle
        {
            get
            {
                return FindWindow("Shell_TrayWnd", "");
            }
        }
        public static int StartHandle
        {
            get
            {
                return FindWindow("Button", "Start");
            }
        }

        public static void Show()
        {
            ShowWindow(Handle, SW_SHOW);
            ShowWindow(StartHandle, SW_SHOW);
        }

        public static void Hide()
        {
            ShowWindow(Handle, SW_HIDE);
            ShowWindow(StartHandle, SW_HIDE);
        }
        private void OnApplicationExit(object sender, EventArgs e)
        {
            Console.WriteLine("application is about to exit");
            SaveCommands();
            COMI.ShutDownCOM();
        }

        //Function to save ObservableCollection/button commands
        private void SaveCommands()
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Command>));
            using (StreamWriter wr = new StreamWriter("Commands.xml"))
            {
                xs.Serialize(wr, Command);
            }

            XmlSerializer xsx = new XmlSerializer(typeof(ObservableCollection<RunAtStartup>));
            using (StreamWriter wrw = new StreamWriter("StartupCommands.xml"))
            {
                xsx.Serialize(wrw, RunAtS);
            }
        }

        //Handles the coding input textbox
        private void UpdateTextNow(object sender, EventArgs e)
        {

            if (IsStateButtonName == true)
            {

                CurrentButtonName = textEditor.Text;
               // Console.WriteLine(CurrentCommandText);
            }
            else if (IsStateButtonName == false && IsEdit == false)
            {
                CurrentCommandText = textEditor.Text;
               // Console.WriteLine(CurrentButtonName);


            }
        }






        //Dynamic Button Handler/COMPILER

        private void button1_Click(object sender, System.EventArgs e)
        {
            Button CommandB = (Button)sender;


            Command Delete = (Command)CommandB.DataContext;
            //  Command.Remove(Delete);
            
            string lcCode = Delete.CommandContent;
           // Console.WriteLine(lcCode);

            Ccompiler.Compile(lcCode);
        }


        private void CompileAtStartup()
        {
            foreach(RunAtStartup R in RunAtS)
            {
                string lcCode = R.CommandContent2;
                Ccompiler.Compile(lcCode);
            }


            
        }



        //ListBox Command Handler
        private void ListBoxCommand(object sender, EventArgs e)
        {
            Button CommandB = (Button)sender;
            if (CommandB.DataContext is Command)
            {
                Command Delete = (Command)CommandB.DataContext;
                //  Command.Remove(Delete);
                string code = Delete.CommandContent;
                Console.Write(code);

            }

        }

        //add Item To Command Listbox Function
        public static void AddCommand(string ButtonName, string Command)
        {
            ListBox listbox = new ListBox();
            var button = new Button()
            {
                Content = ButtonName
            };

            listbox.Items.Add(button);

        }



        //set cursor position
        private double Resx = SystemParameters.PrimaryScreenWidth;
        private double Resy = SystemParameters.PrimaryScreenHeight;
        private void SetPosition(int a, int b)
        {



        }

        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        //handles taskbar hiding/showing
        public void TaskBar(object sender, EventArgs e)
        {


            if (hide == false)
            {
                hide = true;
                Show();

            }
            else if (hide == true)
            {
                hide = false;
                Hide();

            }
        }
        private void CursorHelper(int posX, int posY)
        {
            

        }

        //handles the sliding panel 
        private void SlidingPanel(object sender, EventArgs e)
        {
            if (Slider2.Value == 1)
            {
                VisualStateManager.GoToElementState(grid, "Invisible", false);
            }
            else
            {
                VisualStateManager.GoToElementState(grid, "Here", false);
            }
        }


        private void SlideChanged(object sender, EventArgs e)
        {
            //Console.WriteLine("SLIDERCHANGED RAN");

            Program.SetApplicationVolume((int)slider.Value);
        }



        private void ClickExpand(object sender, RoutedEventArgs e)
        {








        }
        //GLOBAL HOTKEY
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
    [In] IntPtr hWnd,
    [In] int id,
    [In] uint fsModifiers,
    [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        

        protected override void OnClosed(EventArgs e)
        {
            
            StopListeningForWindowChanges();//KEEP THIS IT UNHOOKS THE LISTENER FOR WINDOW FOCUS CHANGES 
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
            synth.Dispose();
            Test.rec.Dispose();
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            const uint VK_F10 = 0x09;
            const uint MOD_CTRL = 0x0002;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, VK_F10))
            {
                // handle error
                MessageBox.Show("Could Not Register Hotkey", "ERROR");
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {



           // System.Console.WriteLine("HotKey");

            if (Trigger == false)
            {

                grid.Visibility = Visibility.Collapsed;
                VisualStateManager.GoToElementState(grid, "Retracted", false);
                Trigger = true;
               // System.Console.WriteLine("It Excecuted it...");
            }
            else if (Trigger == true)
            {
                grid.Visibility = Visibility.Visible;
                this.Focus();
                SetCursorPos(20, 25);
                VisualStateManager.GoToElementState(grid, "Extended", false);
                Trigger = false;
            }
        }
        //Global hotkey 

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, uint dwflags);
        [DllImport("user32.dll")]
        internal static extern int UnhookWinEvent(IntPtr hWinEventHook);
        internal delegate void WinEventProc(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);

        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;
        private IntPtr winHook;
        private WinEventProc listener;

        public void StartListeningForWindowChanges()
        {
            listener = new WinEventProc(EventCallback);
            //setting the window hook
            winHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, listener, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        public void StopListeningForWindowChanges()
        {
            UnhookWinEvent(winHook);
        }

        private static void EventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {

            // handle active window changed!
           // System.Console.WriteLine("WINDOW CHANGED?");
            Program.GetActiveProcessFileName();//Update The Pid For the Slider Volume
        }
        //confirm Button name/command This Function handles ButtonName State
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (StartupType == true)
            {
                if (Relocate == true)
                {
                    Console.WriteLine("Relocate Launched for Startup types");
                    int SelectIndex;
                    textEditor.Text = "";
                    if (!int.TryParse(textEditor.Text, out SelectIndex))
                    {

                        return;
                    }
                    SelectIndex -= 1;
                    RunAtS.Remove(SelectedStart);
                    RunAtS.Insert(SelectIndex, SelectedStart);
                    Relocate = false;
                    AddButtonStatus.Text = "Button Name";
                }

                else if (IsStateButtonName == true)
                {
                    
                    IsStateButtonName = false;
                    //  textEditor.Text = "";
                    AddButtonStatus.Text = "Command Code";
                    if (IsEdit == true)
                    {

                        textEditor.Text = CurrentCommandText;
                        EditFinal = true;
                    }
                    else
                    {

                        //just to get things going and give the user a predefined idea of whats going on
                        textEditor.Text = @"using System;
using System.IO;
using System.Windows.Forms;

namespace MyNamespace
    {
        public class MyClass
        {
            public object DynamicCode(params object[] Parameters)
            {
              //code goes here
           

               return null;
             } 
            }
           }";
                    }

                }
               // else if
                else
                {
                    if (IsEdit == true)
                    {
                        CurrentCommandText = textEditor.Text;
                        IsEdit = false;
                       // Console.WriteLine(CurrentButtonName);
                    //    Console.WriteLine(CurrentCommandText);
                     //   Console.WriteLine("CurrentCommand Is");
                   //     Console.WriteLine(CurrentCommandText);
                        RunAtStartup TheSelectedOne2 = (RunAtStartup)RunAtSTartup.SelectedItem;
                        
                        SelectedStart = new RunAtStartup() { DisplayName = CurrentButtonName, CommandContent2 = CurrentCommandText };
                        RunAtS.RemoveAt(indexer);
                        RunAtS.Insert(indexer, SelectedStart);

                        textEditor.Text = "";
                        SelectedStart = null;
                        IsStateButtonName = true;
                        AddButtonStatus.Text = "Name Of Button";

                    }

                }

            }

            else
            {
                if (Relocate == true)
                {
                    Console.WriteLine("Relocate Launched");
                    int SelectIndex;
                    textEditor.Text = "";
                    if (!int.TryParse(textEditor.Text, out SelectIndex))
                    {

                        return;
                    }
                    SelectIndex -= 1;
                    Command.Remove(selected);
                    Command.Insert(SelectIndex, selected);
                    Relocate = false;
                    AddButtonStatus.Text = "Button Name";
                }

                else if (IsStateButtonName == true)
                {

                    IsStateButtonName = false;
                    AddButtonStatus.Text = "Command Code";
                    if (IsEdit == true)
                    {

                        textEditor.Text = CurrentCommandText;
                    }
                    else
                    {

                        //just to get things going and give the user a predefined idea of whats going on
                        textEditor.Text = @"using System;
using System.IO;
using System.Windows.Forms;

namespace MyNamespace
    {
        public class MyClass
        {
            public object DynamicCode(params object[] Parameters)
            {
              //code goes here
           

               return null;
             } 
            }
           }";
                    }

                }
                else
                {
                    if (IsEdit == true)
                    {
                        CurrentCommandText = textEditor.Text;
                        IsEdit = false;
                      
                        Command TheSelectedOne = (Command)CommandBox.SelectedItem;

                        selected = new Command() { ButtonName = CurrentButtonName, CommandContent = CurrentCommandText, VoiceTrigger = Voicetrigger, RequireAuth = auth };
                        Command.RemoveAt(indexer);
                        Command.Insert(indexer, selected);

                        textEditor.Text = "";
                        selected = null;
                        IsStateButtonName = true;
                        AddButtonStatus.Text = "Name Of Button";

                    }

                    // AddButtonStatus.Text = "Name Of Button";
                    //  IsStateButtonName = true;
                }

            }
        }
        private void CreateButton(object sender, RoutedEventArgs e)
        {
            if (IsStateButtonName == false)//if The command Propperty is assumed to be set
            {
                Command Comm = new Command() { ButtonName = CurrentButtonName, CommandContent = CurrentCommandText };
                Command.Add(Comm);
                CommandBox.DataContext = Command;
                CurrentButtonName = ""; CurrentCommandText = "";
                IsStateButtonName = true;
                AddButtonStatus.Text = "Name Of Button";
                textEditor.Text = "";

            }
            else
            {
                MessageBox.Show("Please Click Confirm To Enter Command Text Before Trying To Create A Button");
            }
        }
        //handles editing of commands
        private void EditCommand(object sender, RoutedEventArgs e)
        {
            IsStateButtonName = true;
            selected = (Command)CommandBox.SelectedItem;
            indexer = (int)CommandBox.SelectedIndex;
            IsEdit = true;
            CurrentButtonName = selected.ButtonName;
            CurrentCommandText = selected.CommandContent;
            Voicetrigger = selected.VoiceTrigger;
            if(RequireAuth.IsChecked != null)
            auth = (bool)RequireAuth.IsChecked;
            //   Console.WriteLine(CurrentButtonName);
          


            Console.WriteLine(selected.ButtonName);
            textEditor.Text = CurrentButtonName;





        }
        //handles deletion of commands
        private void DeleteCommand(object sender, RoutedEventArgs e)
        {
            Command Delete = (Command)CommandBox.SelectedItem;
            if (Delete != null)
            {
                textEditor.Text = "";
                IsStateButtonName = true;
                IsEdit = false;
                Command.Remove(Delete);
            }
            Delete = null;
    }

       

        private void InitiateRelocate(object sender, RoutedEventArgs e)
        {
            selected = (Command)CommandBox.SelectedItem;
            indexer = CommandBox.SelectedIndex;
            Relocate = true;
            AddButtonStatus.Text = "Enter Number Of Location";

        }

        private void GetSysTime(object sender, EventArgs e)
        {
          
               // SysTime = System.DateTime.Today.ToString();
                //Console.WriteLine("Systime Is " + SysTime);
                textBox.Text = DateTime.Now.ToString();
        }

        private void CommandBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Command Temp =  (Command)CommandBox.SelectedItem;
            if(Temp != null)
            RequireAuth.IsChecked = Temp.RequireAuth;
        }

        private void CreateStart(object sender, EventArgs e)
        {
            StartupType = true;
            if (IsStateButtonName == false)//if The command Propperty is assumed to be set
            {
                RunAtStartup Comm = new RunAtStartup() { DisplayName = CurrentButtonName, CommandContent2 = CurrentCommandText };
                RunAtS.Add(Comm);
                CommandBox.DataContext = Command;
                CurrentButtonName = ""; CurrentCommandText = "";
                IsStateButtonName = true;
                AddButtonStatus.Text = "Name Of Button";
                textEditor.Text = "";

            }
            else
            {
                MessageBox.Show("Please Click Confirm To Enter Command Text Before Trying To Create A Button");
            }
        }
        private void EditStart(object sender, EventArgs e)
        {
            StartupType = true;
            IsEdit = true;
            

            IsStateButtonName = true;
            SelectedStart = (RunAtStartup)RunAtSTartup.SelectedItem;
            indexer = (int)RunAtSTartup.SelectedIndex;
            IsEdit = true;
            CurrentButtonName = SelectedStart.DisplayName;
            CurrentCommandText = SelectedStart.CommandContent2;
            //   Console.WriteLine(CurrentButtonName);
          //  Console.WriteLine(CurrentCommandText);
            textEditor.Text = CurrentButtonName;
        }
        private void DeleteStartF(object sender, EventArgs e)
        {
            RunAtStartup Delete = (RunAtStartup)RunAtSTartup.SelectedItem;
            if (Delete != null)
            {
                textEditor.Text = "";
                IsStateButtonName = true;
                IsEdit = false;
                RunAtS.Remove(Delete);

            }
            Delete = null;
        }

        private void VoiceCommand(object sender, EventArgs e)
        {
            Command ChangeVoiceCommand = (Command)CommandBox.SelectedItem;
          string ChangeTrigger =  Microsoft.VisualBasic.Interaction.InputBox("Input Voice Command Trigger For This Button", "Input", "", 100,80);
            if (ChangeTrigger != null)
            {
                ChangeVoiceCommand.VoiceTrigger = ChangeTrigger;
            }
        }

        private void Authentication(object sender, EventArgs e)
        {
            Command temp = (Command)CommandBox.SelectedItem;
            try
            {
                temp.RequireAuth = (bool)RequireAuth.IsChecked;
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
            }

        }

        
    }
    }

   



